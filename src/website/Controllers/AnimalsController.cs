namespace Kcsara.Database.Web.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.Drawing.Drawing2D;
  using System.Drawing.Imaging;
  using System.Linq;
  using System.Web;
  using System.Web.Mvc;
  using Kcsar.Database.Model;
  using IO = System.IO;

  public class AnimalsController : BaseController
  {
    public AnimalsController(IKcsarContext db, IAppSettings settings) : base(db, settings) { }

    /// <summary>Vdir-relative directory to the meber's photo store. Includes trailing-slash.</summary>
    public const string PhotosStoreRelativePath = "~/Content/auth/animals/";
    public const string StandInPhotoFile = "none.jpg";

    #region Photos
    [AuthorizeWithLog(Roles = "cdb.admins")]
    public ActionResult PhotoUpload(string id)
    {
      string[] split = id.Split(',');
      List<Guid> ids = new List<Guid>();
      foreach (string s in split)
      {
        try
        {
          ids.Add(new Guid(s));
        }
        finally { }
      }

      var x = this.db.Animals.Where(GetSelectorPredicate<Animal>(ids)).OrderBy(f => f.Name);

      return View(x);
    }

    [AuthorizeWithLog(Roles = "cdb.admins")]
    [AcceptVerbs(HttpVerbs.Post)]
    [Route("animals/photopreview/go")]
    public ActionResult PhotoPreview()
    {
      Dictionary<Guid, Bitmap> images = new Dictionary<Guid, Bitmap>();

      ClearPreviewCache();
      foreach (string file in Request.Files)
      {
        HttpPostedFileBase hpf = Request.Files[file] as HttpPostedFileBase;
        if (hpf.ContentLength == 0)
          continue;

        Guid id = new Guid(file.Substring(1));

        Image imgPhoto = Image.FromStream(hpf.InputStream);

        // Resize and crop the photo to fit the prescribed format.
        // The image is cropped (from the center) only enough to fit the aspect ratio.
        int targetW = 375;
        int targetH = 500;
        float targetRatio = (float)targetW / (float)targetH;

        // If we were simply cropping the image to the aspect ratio, what would the new dimensions be?
        int aspectW = Math.Min(imgPhoto.Width, (int)(targetRatio * imgPhoto.Height));
        int aspectH = (int)(aspectW / targetRatio);

        Bitmap bmPhoto = new Bitmap(targetW, targetH, PixelFormat.Format24bppRgb);
        bmPhoto.SetResolution(72, 72);
        Graphics grPhoto = Graphics.FromImage(bmPhoto);
        grPhoto.SmoothingMode = SmoothingMode.AntiAlias;
        grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
        grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;
        grPhoto.DrawImage(imgPhoto, new Rectangle(0, 0, targetW, targetH), (imgPhoto.Width - aspectW) / 2, (imgPhoto.Height - aspectH) / 2, aspectW, aspectH, GraphicsUnit.Pixel);

        imgPhoto.Dispose();
        grPhoto.Dispose();
        images.Add(id, bmPhoto);

      }

      if (images.Count > 0)
      {
        Session["photoPreview"] = images;
      }

      var m = this.db.Animals.Where(GetSelectorPredicate<Animal>(images.Keys)).OrderBy(f => f.Name);

      return View(m);
    }

    [AcceptVerbs(HttpVerbs.Post)]
    [AuthorizeWithLog(Roles = "cdb.admins")]
    [Route("animals/photocommit/go")]
    public ActionResult PhotoCommit(FormCollection fields)
    {
      List<string> errors = new List<string>();
      string redirect = "/animals";
      if (Session["photoPreview"] == null)
      {
        errors.Add("Temporary data not found.");
      }
      else
      {
        Dictionary<Guid, Bitmap> images = (Dictionary<Guid, Bitmap>)Session["photoPreview"];

        var Animals = this.db.Animals.Where(GetSelectorPredicate<Animal>(images.Keys)).OrderBy(f => f.Name);

        string keepImages = (fields["keep"] ?? "").ToLowerInvariant();

        char[] badChars = IO.Path.GetInvalidFileNameChars();

        foreach (Animal m in Animals)
        {
          string storePath = Server.MapPath(AnimalsController.PhotosStoreRelativePath);

          if (keepImages.Contains(m.Id.ToString().ToLowerInvariant()))
          {
            string newFileName = string.Format("{0}{1}.jpg", string.Join("", m.Name.Split(badChars)), Guid.NewGuid());
            images[m.Id].Save(IO.Path.Combine(storePath, newFileName), ImageFormat.Jpeg);
            m.PhotoFile = newFileName;
          }
        }
        if (Animals.Count() == 1)
        {
          redirect = "/animals/" + Animals.First().Id.ToString();
        }

        this.db.SaveChanges();
      }


      if (errors.Count > 0)
      {
        ViewData["error"] = string.Join("<br/>", errors.ToArray());
        return View("Error");
      }

      return Redirect(redirect);
    }

    [AcceptVerbs(HttpVerbs.Get)]
    public StreamResult PhotoData(Guid id)
    {
      if (Session["photoPreview"] == null)
      {
        Response.StatusCode = 400;
        Response.End();
      }

      Dictionary<Guid, Bitmap> photos = (Dictionary<Guid, Bitmap>)Session["photoPreview"];

      StreamResult result = new StreamResult { ContentType = "image/jpeg" };
      photos[id].Save(result.Stream, ImageFormat.Jpeg);
      result.Stream.Position = 0;

      return result;
    }

    private void ClearPreviewCache()
    {
      if (Session["photoPreview"] == null)
      {
        return;
      }

      Dictionary<Guid, Bitmap> photos = (Dictionary<Guid, Bitmap>)Session["photoPreview"];
      foreach (Bitmap b in photos.Values)
      {
        b.Dispose();
      }
      Session["photoPreview"] = null;
    }
    #endregion

    public static string GetPhotoOrFillInPath(string photoFile)
    {
      return AnimalsController.PhotosStoreRelativePath + (photoFile ?? AnimalsController.StandInPhotoFile);
    }

  }
}
