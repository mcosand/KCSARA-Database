/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.Services
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.IO;
  using System.Linq;
  using System.Net;
  using System.Security.Cryptography;
  using System.Text.RegularExpressions;
  using System.Threading.Tasks;
  using Kcsar.Database.Model;
  using log4net;
  using Microsoft.AspNet.Hosting;
  using Microsoft.Net.Http.Headers;
  using Models;

  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="ModelType"></typeparam>
  public interface IDocumentsService
  {
    IEnumerable<DocumentInfo> List(Guid referenceId);
    DocumentContent Get(Guid id);
    DocumentContent GetThumbnail(Guid id);
    Task<DocumentInfo> Save(Guid referenceId, DocumentUpload document);
    void Delete(Guid id);
  }

  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="RowType"></typeparam>
  /// <typeparam name="ModelType"></typeparam>
  public class DocumentsService : IDocumentsService
  {
    private readonly Func<IKcsarContext> dbFactory;
    private readonly ILog log;
    private readonly IHostingEnvironment hostingEnvironment;

    public DocumentsService(Func<IKcsarContext> dbFactory, IHostingEnvironment hosting, ILog log)
    {
      this.dbFactory = dbFactory;
      this.log = log;
      hostingEnvironment = hosting;
    }

    public DocumentContent Get(Guid id)
    {
      using (var db = dbFactory())
      {
        var info = (from d in db.Documents
                    where d.Id == id
                    select new { store = d.StorePath, type = d.MimeType, name = d.FileName })
                     .First();

        byte[] result = new byte[0];
        if (!File.Exists(GetFilePath(info.store)))
        {
          throw new NotFoundException();
        }

        return new DocumentContent { Data = File.ReadAllBytes(GetFilePath(info.store)), Mime = info.type, Filename = info.name };
      }
    }

    public DocumentContent GetThumbnail(Guid id)
    {
      using (var db = dbFactory())
      {
        var thumb = (from d in db.Documents
                     where d.Id == id
                     select new { store = d.StorePath, type = d.MimeType })
                     .First();

        byte[] result = new byte[0];
        if (!File.Exists(GetFilePath(thumb.store)))
        {
          throw new NotFoundException();
        }

        using (Image img = Image.FromFile(GetFilePath(thumb.store)))
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
        return new DocumentContent { Data = result, Mime = thumb.type ?? "image/jpeg" };
      }
    }

    public IEnumerable<DocumentInfo> List(Guid referenceId)
    {
      using (var db = dbFactory())
      {
        var a = db.Documents.Where(f => f.ReferenceId == referenceId).OrderBy(f => f.FileName)
          .Select(ToDocumentInfo<DocumentInfo>)
          .ToList();
        return a;
      }
    }

    public async Task<DocumentInfo> Save(Guid referenceId, DocumentUpload document)
    {
      if (referenceId == Guid.Empty) throw new StatusCodeException(HttpStatusCode.BadRequest, "referenceId is required");

      var file = document.File;
      if (file.Length > int.MaxValue)
      {
        throw new StatusCodeException(HttpStatusCode.BadRequest, "File too big");
      }

      var disposition = ContentDispositionHeaderValue.Parse(file.ContentDisposition);

      var row = new DocumentRow
      {
        ReferenceId = referenceId,
        MimeType = document.File.ContentType,
        Size = (int)document.File.Length,
        Type = document.Type,
        Description = document.Description,
        FileName = disposition.FileName.Trim('"')
      };

      var extension = Path.GetExtension(row.FileName);

      if (string.Equals(row.MimeType, "application/octet-stream", StringComparison.OrdinalIgnoreCase))
      {
        switch (extension.ToUpperInvariant())
        {
          case ".GPX":
            row.MimeType = "application/gpx+xml";
            break;
        }
      }

      using (var ms = new MemoryStream((int)file.Length))
      {
        await file.OpenReadStream().CopyToAsync(ms);
        ms.Position = 0;
        var md5 = MD5.Create();
        var hash = Regex.Replace(Convert.ToBase64String(md5.ComputeHash(ms)).Replace('/', '@'), "([a-z])", "$1-");
        row.StorePath = string.Format("{0}/{1}/{2}{3}", hash[0], (hash[1] == '-') ? hash[2] : hash[1], hash, extension);
        var discFile = GetFilePath(row.StorePath);
        var folder = Path.GetDirectoryName(discFile);

        if (!Directory.Exists(folder))
        {
          Directory.CreateDirectory(folder);
        }

        if (!File.Exists(discFile))
        {
          using (var filestream = new FileStream(discFile, FileMode.CreateNew, FileAccess.Write))
          {
            ms.Position = 0;
            await ms.CopyToAsync(filestream);
          }
        }
      }

      using (var db = dbFactory())
      {
        if (db.Documents.Any(f => f.ReferenceId == row.ReferenceId && f.StorePath == row.StorePath))
        {
          throw new StatusCodeException(HttpStatusCode.BadRequest, "File already exists");
        }

        db.Documents.Add(row);
        db.SaveChanges();
      }

      return ToDocumentInfo<DocumentInfo>(row);
    }

    private string GetFilePath(string relative)
    {
      return Path.Combine(hostingEnvironment.MapPath("documents/"), relative);
    }

    private T ToDocumentInfo<T>(DocumentRow row) where T : DocumentInfo, new()
    {
      var info = new T
      {
        Id = row.Id,
        Filename = row.FileName,
        Size = row.Size,
        Description = row.Description,
        Type = row.Type,
        Mime = row.MimeType,
        Changed = row.LastChanged,
        ChangedBy = row.ChangedBy
      };
      return info;
    }

    public void Delete(Guid id)
    {
      using (var db = dbFactory())
      {
        var document = db.Documents.SingleOrDefault(f => f.Id == id);
        if (document == null)
        {
          throw new NotFoundException();
        }

        db.Documents.Remove(document);

        var storeFile = document.StorePath;
        if (!db.Documents.Any(f => f.Id != id && f.StorePath == storeFile))
        {
          if (!Directory.Exists(GetFilePath("deleted")))
          {
            Directory.CreateDirectory(GetFilePath("deleted"));
          }
          File.Move(GetFilePath(storeFile), GetFilePath("deleted/" + storeFile.Substring(4)));
        }

        db.SaveChanges();
      }
    }
  }
}
