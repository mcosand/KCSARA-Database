/*
 * Copyright 2011-2015 Matthew Cosand
 */

namespace Kcsara.Database.Web
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.Drawing.Imaging;
  using System.IO;
  using System.Web;
  using System.Windows.Media.Imaging;
  using Kcsar.Database.Data;

  public static class Documents
  {
    public readonly static Guid SubmittedDocument = new Guid("61D3E9FE-E03C-42F8-B73A-FAB603E68B24");

    public static byte[] RotateImage(byte[] source, bool? clockwise)
    {
      string tempfile = System.IO.Path.GetTempFileName();
      string targetFile = System.IO.Path.GetTempFileName();

      System.IO.File.WriteAllBytes(tempfile, source);

      using (Bitmap b = (Bitmap)Image.FromFile(tempfile))
      {
        b.RotateFlip((clockwise ?? true) ? RotateFlipType.Rotate90FlipNone : RotateFlipType.Rotate270FlipNone);
        b.Save(targetFile);
      }
      byte[] val = System.IO.File.ReadAllBytes(targetFile);
      System.IO.File.Delete(tempfile);
      System.IO.File.Delete(targetFile);
      return val;
    }

    public static DocumentRow ProcessImage(System.IO.Stream source, IKcsarContext ctx, string filename, Guid trainingId)
    {
      Bitmap bitmap = (Bitmap)Image.FromStream(source);

      string tempFile = System.IO.Path.GetTempFileName();
      bitmap.Save(tempFile, ImageFormat.Jpeg);
      byte[] contents = System.IO.File.ReadAllBytes(tempFile);

      System.IO.File.Delete(tempFile);
      bitmap.Dispose();

      DocumentRow doc = new DocumentRow
      {
        Size = contents.Length,
        FileName = System.IO.Path.GetFileName(filename),
        MimeType = "image/jpeg",
        Contents = contents,
        ReferenceId = trainingId,
        Type = "unknown"
      };
      ctx.Documents.Add(doc);
      return doc;
    }

    public static void ReceiveDocuments(HttpFileCollectionBase files, IKcsarContext ctx, Guid reference, string type)
    {
      foreach (string file in files)
      {
        HttpPostedFileBase hpf = files[file] as HttpPostedFileBase;
        ReceiveDocument(hpf.InputStream, hpf.FileName, hpf.ContentLength, ctx, reference, type);
      }
    }

    public static DocumentRow[] ReceiveDocument(Stream contentStream, string filename, int length, IKcsarContext ctx, Guid reference, string type)
    {
      List<DocumentRow> docs = new List<DocumentRow>();
      if (filename.ToLowerInvariant().EndsWith(".tif", StringComparison.OrdinalIgnoreCase))
      {
        TiffBitmapDecoder decode = new TiffBitmapDecoder(contentStream, BitmapCreateOptions.None, BitmapCacheOption.None);

        int frameCount = decode.Frames.Count;
        for (int i = 0; i < frameCount; i++)
        {
          System.IO.MemoryStream ms = new System.IO.MemoryStream();

          JpegBitmapEncoder encode = new JpegBitmapEncoder();
          encode.Frames.Add(BitmapFrame.Create(decode.Frames[i]));
          encode.Save(ms);
          ms.Seek(0, System.IO.SeekOrigin.Begin);
          docs.Add(Kcsara.Database.Web.Documents.ProcessImage(ms, ctx, filename.Replace(".tif", string.Format("-{0}.jpg", i)), reference));
          ms.Dispose();
        }
      }
      else if (filename.ToLowerInvariant().EndsWith(".jpg"))
      {
        docs.Add(Kcsara.Database.Web.Documents.ProcessImage(contentStream, ctx, filename, reference));
      }
      else
      {
        byte[] contents = new byte[length];
        contentStream.Read(contents, 0, length);

        DocumentRow doc = new DocumentRow
        {
          Size = length,
          FileName = System.IO.Path.GetFileName(filename),
          Contents = contents,
          ReferenceId = reference,
          MimeType = GuessMime(filename),
          Type = type
        };
        ctx.Documents.Add(doc);
        docs.Add(doc);
      }
      return docs.ToArray();
    }

    public static string GuessMime(string filename)
    {
      List<string> types = new List<string>(new[] {
                ".jpg", "image/jpeg",
                ".png", "image/png",
                ".pdf", "application/pdf",
                ".gpx", "application/gpx-xml",
                ".xls", "application/vnd.ms-excel",
                ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            });
      string extension = System.IO.Path.GetExtension(filename).ToLowerInvariant();

      int i = types.IndexOf(extension);
      if (i >= 0) return types[i + 1];

      return "application/octet-stream";
    }
  }
}
