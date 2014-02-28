/*
 * Copyright 2008-2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.Controllers
{
  using Kcsar.Database.Model;
  using Kcsar.Membership;
  using Kcsara.Database.Web.Model;
  using Kcsara.Database.Web.Services;
  using MvcContrib.UI;
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using System.Drawing;
  using System.Globalization;
  using System.IO;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Net.Mail;
  using System.Runtime.Serialization;
  using System.Text.RegularExpressions;
  using System.Web;
  using System.Web.Mvc;
  using System.Web.Security;
  using Kcsara.Database.Services;

  public class BaseController : Controller
  {
    public Func<string, bool> UserInRole;
    public Func<string, object> GetSessionValue;
    public Action<string, object> SetSessionValue;
    public IAuthService Permissions = null;
    protected readonly IAppSettings settings;
    protected readonly IKcsarContext db;
    
    public BaseController(IKcsarContext db)
      : this(db, Ninject.ResolutionExtensions.Get<IAppSettings>(MvcApplication.myKernel))
    {
    }

    public BaseController(IKcsarContext db, IAppSettings settings)
      : base()
    {
      this.db = db;
      this.settings = settings;

      UserInRole = (f => User.IsInRole(f));
      GetSessionValue = (f => Session[f]);
      SetSessionValue = ((f, v) => Session[f] = v);
    }

    protected string GetDateFormat()
    {
      return "{0:yyyy-MM-dd}";
    }

    protected override void Initialize(System.Web.Routing.RequestContext requestContext)
    {
      base.Initialize(requestContext);
      Permissions = new AuthService(User, this.db);
      Document.StorageRoot = requestContext.HttpContext.Request.MapPath("~/Content/auth/documents/");
    }

    private UserSettings _settings;
    protected UserSettings UserSettings
    {
      get
      {
        // Lazily initialized because the session object isn't available during the constructor.

        if (this._settings == null)
        {
          this._settings = Kcsara.Database.Web.SettingsProvider.LoadSettings(this.GetSessionValue);
        }
        return this._settings;
      }
    }

    protected Expression<Func<T, bool>> GetSelectorPredicate<T>(IEnumerable<Guid> ids) where T : IModelObject
    {
      var o = Expression.Parameter(typeof(T), "o");
      var body = ids
        // t.Id == id
          .Select(id => Expression.Equal(Expression.Property(o, "Id"), Expression.Constant(id)))
        // t.Id == id1 OR t.ID == id2 Or ...
          .Aggregate((accum, clause) => Expression.Or(accum, clause));
      return Expression.Lambda<Func<T, bool>>(body, o);
    }

    protected DataActionResult GetLoginError()
    {
      Response.StatusCode = 403;
      return new DataActionResult("login");
    }

    protected DataActionResult Data(object model)
    {
      Type t = model.GetType();
      if (t.IsArray) t = t.GetElementType();

      bool isDataContract = t.GetCustomAttributes(typeof(DataContractAttribute), true).Length > 0;

      if (Request.AcceptTypes != null && Request.AcceptTypes.Contains("application/json") || string.Equals(Request.QueryString["format"], "json", StringComparison.OrdinalIgnoreCase))
      {
        return isDataContract ? (DataActionResult)(new JsonDataContractResult(model)) : new JsonGenericDataResult(model);
      }

      return isDataContract ? (DataActionResult)(new XmlDataContractResult(model)) : new XmlDataResult(model);
    }

    protected string AbsoluteUrl(string relative)
    {
      return Request.Url.GetLeftPart(UriPartial.Authority) + relative;
    }


    [Authorize]
    [HttpGet]
    public ActionResult SupportingDoc(Guid id, int? notify)
    {
      DocumentView view = (from d in this.db.Documents where d.Id == id select new DocumentView { Id = d.Id, Reference = d.ReferenceId, Title = d.FileName, Type = d.Type, Size = d.Size }).First();

      if (notify.HasValue)
      {
        ViewData["notify"] = notify.Value;
      }

      return View("SupportingDoc", view);
    }

    [Authorize]
    [HttpGet]
    public ActionResult UploadSupportingDoc(string type, Guid? target, int refId)
    {

      // too many overloads with string arguments, so we'll pack it into an array
      return View(new string[] { refId.ToString(), Request.Url.AbsoluteUri, string.Format("{0}", target) });
    }

    [Authorize]
    [HttpPost]
    public ActionResult UploadSupportingDoc(string type, int refId, FormCollection fields)
    {
      //string type = Request.QueryString["type"];
      //Guid? target = string.IsNullOrWhiteSpace(Request.QueryString["target"]) ? (Guid?)null : new Guid(Request.QueryString["target"]);
      //string refId = Request.QueryString["refId"];

      Guid target = new Guid(fields["target"]);
      Guid newId = Guid.Empty;

      foreach (string file in Request.Files)
      {
        HttpPostedFileBase hpf = Request.Files[file] as HttpPostedFileBase;

        byte[] contents = new byte[hpf.ContentLength];
        hpf.InputStream.Read(contents, 0, hpf.ContentLength);

        switch (type)
        {
          case "award":
            Document doc = new Document
            {
              Size = hpf.ContentLength,
              FileName = System.IO.Path.GetFileName(hpf.FileName),
              Contents = contents,
              ReferenceId = target,
              Type = type
            };
            newId = doc.Id;
            this.db.Documents.Add(doc);
            break;
        }

      }
      this.db.SaveChanges();
      // too many overloads with string arguments, so we'll pack it into an array
      return SupportingDoc(newId, refId);
    }

    [HttpGet]
    [Authorize]
    public ActionResult DownloadDoc(Guid id)
    {
      byte[] buffer;
      string name;
      string mime;
      var doc = (from d in this.db.Documents where d.Id == id select d).FirstOrDefault(); //new { p = d.Contents, n = d.FileName.Replace(" ","_"), t = d.MimeType }).FirstOrDefault();

      if (doc == null)
      {
        Response.StatusCode = 400;
        return new ContentResult { Content = "Document not found" };
      }
      buffer = doc.Contents;
      name = doc.FileName.Replace(" ", "_");
      mime = string.IsNullOrWhiteSpace(doc.MimeType) ? Kcsara.Database.Web.Documents.GuessMime(name) : doc.MimeType;

      Response.AddHeader("Content-Disposition", "inline; filename=" + name);
      return File(buffer, mime);
      //return new FileContentResult(buffer, mime);// { FileDownloadName = name };
    }

    [Authorize]
    public FileContentResult DocumentThumb(Guid id)
    {
      var thumb = (from d in this.db.Documents where d.Id == id select new { store = d.StorePath, type = d.MimeType }).First();

      byte[] result = new byte[0];
      try
      {
        using (Image img = Image.FromFile(Document.StorageRoot + thumb.store))
        {
          int h = 100;
          int w = (int)((double)img.Width / (double)img.Height * 100);
          if (w > h)
          {
            h = (int)(100.0 / w * 100.0);
            w = 100;
          }

          string tempFile = System.IO.Path.GetTempFileName();
          var thumbnail = img.GetThumbnailImage(w, h, null, new IntPtr());
          thumbnail.Save(tempFile, System.Drawing.Imaging.ImageFormat.Jpeg);
          result = System.IO.File.ReadAllBytes(tempFile);
          System.IO.File.Delete(tempFile);
        }
      }
      finally
      {
        // do nothing. We already have the empty data in result.
      }


      return new FileContentResult(result, thumb.type ?? "image/jpeg");
    }

    protected void SetPDFOutput(string baseUrl)
    {
      string princePath = ConfigurationManager.AppSettings["PrincePath"];
      if (string.IsNullOrEmpty(princePath))
      {
        this.HttpContext.Response.Write("<div style=\"color:red\">PrincePath not set in web.config</div>");
        return;
      }

      Prince prince = new Prince(princePath);

      prince.SetBaseURL("file:///" + baseUrl);

      this.HttpContext.Response.Filter = new PrinceFilter(prince, this.HttpContext.Response.Filter);
      this.HttpContext.Response.ContentType = "application/pdf";
      this.HttpContext.Response.AddHeader("Content-Disposition", "attachment; filename=sar-cards.pdf");
    }

    public ActionResult ClosePopup()
    {
      return View("ClosePopup");
    }
    public ActionResult Close()
    {
      return View("Close");
    }

    protected void MailView(ViewResult view, string toCsv, string subject)
    {
      MailMessage msg = new MailMessage();
      msg.To.Add(toCsv);
      msg.Subject = subject;

      string htmlSrc = RenderToString(view, this.ControllerContext);

      List<LinkedResource> resources = new List<LinkedResource>();

      Dictionary<string, Guid> images = new Dictionary<string, Guid>();
      foreach (Match m in Regex.Matches(htmlSrc, @"\<img[^\>]+src=""([^""]+)""", RegexOptions.IgnoreCase | RegexOptions.Multiline))
      {
        string imgUrl = m.Groups[1].Value;

        string imgFile = Server.MapPath(imgUrl);

        if (!images.ContainsKey(imgFile))
        {
          Guid id = Guid.NewGuid();
          resources.Add(new LinkedResource(imgFile) { ContentId = id.ToString() });
          images.Add(imgFile, id);
        }

        htmlSrc = Regex.Replace(htmlSrc, "src=\"" + imgUrl + "\"", string.Format(@"src=""cid:{0}""", images[imgFile]), RegexOptions.Multiline | RegexOptions.IgnoreCase);
      }

      AlternateView html = AlternateView.CreateAlternateViewFromString(htmlSrc, null, "text/html");
      foreach (LinkedResource resource in resources)
      {
        html.LinkedResources.Add(resource);
      }
      msg.AlternateViews.Add(html);

      SendMail(msg);
    }

    private static string RenderToString(ViewResult result, ControllerContext controllerContext)
    {
      StringWriter writer = new StringWriter();
      ViewContext viewContext = new ViewContext(controllerContext, new WebFormView(controllerContext, "omg"), result.ViewData, result.TempData, writer);
      var blockRenderer = new BlockRenderer(controllerContext.HttpContext);

      result.ExecuteResult(controllerContext);

      //HtmlHelper h = new HtmlHelper(new ViewContext(controllerContext, new WebFormView("omg"), result.ViewData, result.TempData, ), new ViewPage());
      //var blockRenderer = new BlockRenderer(controllerthis.db.HttpContext);

      //result.ExecuteResult(controllerContext);
      string s = blockRenderer.Capture(
          () => result.ExecuteResult(controllerContext)
      );

      return s;
    }

    protected void SendMail(string toAddresses, string subject, string body)
    {
      EmailService.SendMail(toAddresses, subject, body);
    }

    protected void SendMail(MailMessage msg)
    {
      EmailService.SendMail(msg);
    }

    public ActionResult CreateLoginRedirect()
    {
      //use the current url for the redirect
      string redirectOnSuccess = this.Request.Url.AbsolutePath;

      //send them off to the login page
      string redirectUrl = string.Format("?ReturnUrl={0}", redirectOnSuccess);
      string loginUrl = FormsAuthentication.LoginUrl + redirectUrl;
      return new RedirectResult(loginUrl);
    }
  }
}
