/*
 * Copyright 2009-2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Data.Entity;
  using System.Data.Entity.Validation;
  using System.Linq;
  using System.Web;
  using System.Web.Mvc;
  using System.Web.Security;
  using Kcsar.Database.Model;
  using Kcsara.Database.Geo;
  using log4net;

  public partial class AdminController : BaseController
  {
    public AdminController(IKcsarContext db, IAppSettings settings) : base(db, settings) { }

    [AuthorizeWithLog]
    public ActionResult Index()
    {
      return View();
    }

    [AuthorizeWithLog(Roles = "cdb.admins")]
    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult DisconnectedPhotos()
    {
      string storePath = Server.MapPath(MembersController.PhotosStoreRelativePath);
      var photoFiles = (from m in this.db.Members where m.PhotoFile != "" && m.PhotoFile != null select m.PhotoFile.ToLower()).ToList();

      List<string> existingFiles = System.IO.Directory.GetFiles(storePath).Select(f => f.Substring(storePath.Length)).ToList();
      for (int i = 0; i < existingFiles.Count; i++)
      {
        int dbIdx = photoFiles.IndexOf(existingFiles[i].ToLower());
        if (dbIdx >= 0 || existingFiles[i] == MembersController.StandInPhotoFile.ToLower())
        {
          if (dbIdx >= 0)
          {
            photoFiles.RemoveAt(dbIdx);
          }
          existingFiles.RemoveAt(i);
          i--;
        }
      }

      return View(existingFiles);
    }

    [AcceptVerbs(HttpVerbs.Get)]
    [AuthorizeWithLog(Roles = "cdb.admins")]
    public ActionResult FixUnitMemberships()
    {
      Kcsar.Database.Model.UnitMembership lastUm = null;

      foreach (Kcsar.Database.Model.UnitMembership um in (from u in this.db.UnitMemberships.Include("Person").Include("Unit") select u).OrderBy(f => f.Person.Id).ThenBy(f => f.Unit.Id).ThenBy(f => f.Activated))
      {
        if (lastUm != null && um.Person.Id == lastUm.Person.Id && um.Unit.Id == lastUm.Unit.Id)
        {
          lastUm.EndTime = um.Activated;
        }
        lastUm = um;
      }
      this.db.SaveChanges();

      return new ContentResult { Content = "OK" };
    }

    public ActionResult FixAddresses(int? id)
    {
      id = id ?? 0;

      string data = "<table>";
      int pageSize = 40;
      foreach (var addr in (from a in this.db.PersonAddress.Include("Person") where a.Quality == (int)GeocodeQuality.Unknown select a).OrderBy(f => f.Person.LastName).ThenBy(f => f.Person.FirstName).Skip(id.Value * pageSize).Take(pageSize))
      {
        string oldAddr = addr.Street + "<br/>" + addr.City + " " + addr.State + " " + addr.Zip;

        GeographyServices.RefineAddressWithGeography(addr);

        data += string.Format("<tr><td><b>{0}</b></td><td style=\"white-space:nowrap\">{1}</td><td>{2}</td></tr>",
                            addr.Person.ReverseName,
                            oldAddr,
                            string.Format("Quality: {0}<br/>{2}",
                                addr.Quality,
                                HttpUtility.HtmlEncode(string.Format("[{0}][{1}][{2}][{3}]", addr.Street, addr.City, addr.State, addr.Zip)))
                            );
      }
      try
      {
        this.db.SaveChanges();
      }
      catch (DbEntityValidationException ex)
      {
        LogManager.GetLogger("AdminController").ErrorFormat("Validation error: {0}",
          string.Join("\n", ex.EntityValidationErrors.SelectMany(f => f.ValidationErrors.Select(g => g.PropertyName + ": " + g.ErrorMessage))));
        data += "<tr><td colspan=23>DIDN'T SAVE ANY CHANGES BECAUSE OF VALIDATION EXCEPTIONS. CHECK LOGS.</td></tr>";
      }
      return new ContentResult { Content = data + "</table>", ContentType = "text/html" };
    }

    private static string UnitNameAsGroupName(string groupName)
    {
      return groupName.ToLower().Replace(" ", "");
    }

    #region TestMethods
    private static int counter = 0;
    [AuthorizeWithLog(Roles = "cdb.admins")]
    public DataActionResult TestExceptionHandling()
    {
      throw new InvalidCastException("Test exception " + (counter++).ToString());
    }

    [AuthorizeWithLog]
    public ContentResult TestMail()
    {
      string email = Membership.GetUser().Email;
      base.SendMail(email, "Test mail from KCSARA database.", "This test mail was sent from the database at " + DateTime.Now.ToString());
      return base.Content("Mail sent to " + email);
    }
    #endregion
  }
}
