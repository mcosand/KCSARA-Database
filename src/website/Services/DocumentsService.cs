/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.Services
{
  using System;
  using System.Collections.Generic;
  using System.Data.Entity.SqlServer;
  using System.Drawing;
  using System.IO;
  using System.Linq;
  using Kcsar.Database.Model;
  using log4net;
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
  }

  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="RowType"></typeparam>
  /// <typeparam name="ModelType"></typeparam>
  public class DocumentsService : IDocumentsService
  {
    public string StoreRoot { get; set; }

    private readonly Func<IKcsarContext> dbFactory;
    private readonly ILog log;

    public DocumentsService(Func<IKcsarContext> dbFactory, ILog log)
    {
      this.dbFactory = dbFactory;
      this.log = log;
      StoreRoot = string.Empty;
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
        if (!File.Exists(StoreRoot + info.store))
        {
          throw new NotFoundException();
        }

        return new DocumentContent { Data = File.ReadAllBytes(StoreRoot + info.store), Mime = info.type, Filename = info.name };
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
        if (!File.Exists(StoreRoot + thumb.store))
        {
          throw new NotFoundException();
        }

        using (Image img = Image.FromFile(StoreRoot + thumb.store))
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
          .Select(f => new DocumentInfo
          {
            Id = f.Id,
            Filename = f.FileName,
            Size = f.Size,
            Type = f.Type,
            Mime = f.MimeType
          })
          .ToList();
        return a;
      }
    }
  }
}
