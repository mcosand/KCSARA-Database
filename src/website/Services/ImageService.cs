using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.Services
{
  public class ImageService
  {
    public Bitmap GetResized(Stream imageStream, int targetW, int targetH)
    {
      Image imgPhoto = Image.FromStream(imageStream);

      // Resize and crop the photo to fit the prescribed format.
      // The image is cropped (from the center) only enough to fit the aspect ratio.
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
      return bmPhoto;
    }
  }
}