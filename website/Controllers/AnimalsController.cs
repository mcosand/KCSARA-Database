/*
 * Copyright 2009-2014 Matthew Cosand
 */

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
  using Kcsara.Database.Web.Model;
  using IO = System.IO;

  public class AnimalsController : BaseController
  {
    public AnimalsController(IKcsarContext db) : base(db) { }

    /// <summary>Vdir-relative directory to the meber's photo store. Includes trailing-slash.</summary>
    public const string PhotosStoreRelativePath = "~/Content/auth/animals/";
    public const string StandInPhotoFile = "none.jpg";

    [Authorize]
    public ActionResult Index()
    {
      var animals = (from a in this.db.Animals.Include("Owners").Include("Owners.Owner")
                     orderby a.Name
                     select
                       new AnimalListRow { Animal = a }).ToList();

      for (int i = 0; i < animals.Count; i++)
      {
        animals[i].PrimaryOwner = animals[i].Animal.GetPrimaryOwner();
        animals[i].PrimaryOwnerName = (animals[i].PrimaryOwner == null) ? "" : animals[i].PrimaryOwner.ReverseName;
        animals[i].ActiveUntil = (animals[i].Animal.Owners.Count(f => !f.Ending.HasValue) > 0) ? null : animals[i].Animal.Owners.Max(f => f.Ending);
      }

      return View(animals);
    }

    [Authorize]
    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult Detail(Guid id)
    {
      Animal animal = (from a in this.db.Animals.Include("Owners").Include("Owners.Owner").Include("MissionRosters").Include("MissionRosters.MissionRoster").Include("MissionRosters.MissionRoster.Mission") where a.Id == id select a).First();

      ViewData["PageTitle"] = "Animal Detail: " + animal.Name;

      return View(animal);
    }

    #region Create/Edit/Delete
    [AcceptVerbs(HttpVerbs.Get)]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult Create()
    {
      ViewData["PageTitle"] = "New Animal";

      Animal a = new Animal();

      Session.Add("NewAnimalGuid", a.Id);
      ViewData["NewAnimalGuid"] = Session["NewAnimalGuid"];

      return InternalEdit(a);
    }

    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult Create(FormCollection fields)
    {
      if (Session["NewAnimalGuid"] != null && Session["NewAnimalGuid"].ToString() != fields["NewAnimalGuid"])
      {
        throw new InvalidOperationException("Invalid operation. Are you trying to re-create an animal?");
      }
      Session.Remove("NewAnimalGuid");

      ViewData["PageTitle"] = "New Animal";

      Animal a = new Animal();
      this.db.Animals.Add(a);
      return InternalSave(a, fields);
    }


    [AcceptVerbs("GET")]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult Edit(Guid id)
    {
      Animal animal = (from a in this.db.Animals where a.Id == id select a).First();
      return InternalEdit(animal);
    }

    [AcceptVerbs("POST")]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult Edit(Guid id, FormCollection fields)
    {
      Animal animal = (from a in this.db.Animals where a.Id == id select a).First();
      return InternalSave(animal, fields);
    }

    private ActionResult InternalEdit(Animal a)
    {
      ViewData["TypeList"] = new SelectList(Animal.AllowedTypes, a.Type);

      return View("Edit", a);
    }

    private ActionResult InternalSave(Animal a, FormCollection fields)
    {
      //try
      //{
        TryUpdateModel(a, new string[] { "Name", "DemSuffix", "Comments", "Type" });
        if (ModelState.IsValid)
        {
          this.db.SaveChanges();
          TempData["message"] = "Saved";
          return RedirectToAction("ClosePopup");
        }
        return InternalEdit(a);
      //}
      //catch (DbValidationsException ex)
      //{
      //  this.CollectRuleViolations(ex, fields);
      //}
      //return InternalEdit(a);
    }

    [Authorize]
    [AcceptVerbs(HttpVerbs.Get)]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult Delete(Guid id)
    {
      return View((from a in this.db.Animals where a.Id == id select a).First());
    }

    [Authorize]
    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult Delete(Guid id, FormCollection fields)
    {
      Animal animal = (from a in this.db.Animals where a.Id == id select a).First();
      this.db.Animals.Remove(animal);
      this.db.SaveChanges();

      return RedirectToAction("ClosePopup");
    }
    #endregion

    #region Owners
    [AcceptVerbs("GET")]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult CreateOwner(Guid id)
    {
      ViewData["PageTitle"] = "New Owner";

      AnimalOwner o = new AnimalOwner();
      o.Animal = (from a in this.db.Animals where a.Id == id select a).First();
      o.Starting = DateTime.Today;
      //s.Person = (from p in this.db.Members where p.Id == personId select p).First();
      //s.Activated = DateTime.Today;

      Session.Add("NewOwnerGuid", o.Id);
      ViewData["NewOwnerGuid"] = Session["NewOwnerGuid"];

      return InternalEditOwner(o);
    }

    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult CreateOwner(Guid id, FormCollection fields)
    {
      if (Session["NewOwnerGuid"] != null && Session["NewOwnerGuid"].ToString() != fields["NewOwnerGuid"])
      {
        throw new InvalidOperationException("Invalid operation. Are you trying to re-create an ownership?");
      }
      Session.Remove("NewOwnerGuid");

      ViewData["PageTitle"] = "New Owner";

      AnimalOwner o = new AnimalOwner();
      o.Animal = (from a in this.db.Animals where a.Id == id select a).First();
      //    um.Person = (from p in this.db.Members where p.Id == personId select p).First();
      this.db.AnimalOwners.Add(o);
      return InternalSaveOwner(o, fields);
    }


    [AcceptVerbs("GET")]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult EditOwner(Guid id)
    {
      AnimalOwner o = (from ao in this.db.AnimalOwners.Include("Animal").Include("Owner") where ao.Id == id select ao).First();
      return InternalEditOwner(o);
    }

    [AcceptVerbs("POST")]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult EditOwner(Guid id, FormCollection fields)
    {
      AnimalOwner o = (from ao in this.db.AnimalOwners.Include("Animal").Include("Owner") where ao.Id == id select ao).First();
      return InternalSaveOwner(o, fields);
    }

    private ActionResult InternalEditOwner(AnimalOwner o)
    {
      return View("EditOwner", o);
    }

    private ActionResult InternalSaveOwner(AnimalOwner o, FormCollection fields)
    {
      //try
      //{
        TryUpdateModel(o, new string[] { "IsPrimary", "Starting", "Ending" });

        if (string.IsNullOrEmpty(fields["pid_a"]))
        {
          ModelState.AddModelError("Owner", "Required. Please pick from list.");

        }
        else
        {
          Guid personId = new Guid(fields["pid_a"]);
          Member member = (from m in this.db.Members where m.Id == personId select m).First();
          o.Owner = member;
        }

        if (ModelState.IsValid)
        {
          this.db.SaveChanges();
          TempData["message"] = "Saved";
          return RedirectToAction("ClosePopup");
        }

      //}
      //catch (RuleViolationsException ex)
      //{
      //  this.CollectRuleViolations(ex, fields);
      //}
      return InternalEditOwner(o);
    }

    [Authorize]
    [AcceptVerbs(HttpVerbs.Get)]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult DeleteOwner(Guid id)
    {
      return View((from ao in this.db.AnimalOwners.Include("Owner").Include("Animal") where ao.Id == id select ao).First());
    }

    [Authorize]
    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult DeleteOwner(Guid id, FormCollection fields)
    {
      AnimalOwner o = (from ao in this.db.AnimalOwners where ao.Id == id select ao).First();
      this.db.AnimalOwners.Remove(o);
      this.db.SaveChanges();

      return RedirectToAction("ClosePopup");
    }
    #endregion


    #region Photos
    [Authorize(Roles = "cdb.admins")]
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

    [Authorize(Roles = "cdb.admins")]
    [AcceptVerbs(HttpVerbs.Post)]
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
    [Authorize(Roles = "cdb.admins")]
    public ActionResult PhotoCommit(FormCollection fields)
    {
      List<string> errors = new List<string>();

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

        this.db.SaveChanges();
      }


      if (errors.Count > 0)
      {
        ViewData["error"] = string.Join("<br/>", errors.ToArray());
        return View("Error");
      }

      return RedirectToAction("ClosePopup");
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
