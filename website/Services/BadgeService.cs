using iTextSharp.text;
using iTextSharp.text.pdf;
using Kcsar.Database.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.Services
{
    public static class BadgeService
    {
        private const int CARD_WIDTH = 153;
        private const int CARD_HEIGHT = 243;
        private const int UPPER_BAR_HEIGHT = 54;
        private const int LOWER_BAR_HEIGHT = 44;
        private const int PHOTO_BAR_HEIGHT = 96;
        private const int PHOTO_WIDTH = 72;

        private static BaseFont baseFont = BaseFont.CreateFont(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Windows), "fonts", "arial.ttf"), BaseFont.CP1252, true);


        /// <summary>
        /// Creates a MemoryStream but does not dispose it.
        /// </summary>
        /// <param name="members"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public static MemoryStream CreateCards(IEnumerable<Member> members, DateTime expiration)
        {
            iTextSharp.text.Document doc = new iTextSharp.text.Document(new iTextSharp.text.Rectangle(CARD_WIDTH, CARD_HEIGHT));
            MemoryStream memStream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(doc, memStream);
            writer.CloseStream = false;
            doc.Open();


            foreach (var member in members)
            {
                foreach (var passport in new[] { false, true})
                {
                    Render_FrontBase(writer.DirectContentUnder, expiration);
                    Render_FrontContent(writer.DirectContent, member);
                    doc.NewPage();
                    Render_BackBase(writer.DirectContentUnder);
                    Render_BackContent(writer.DirectContent, member, passport);
                    doc.NewPage();
                }
            }

            doc.Close();
            byte[] buf = new byte[memStream.Position];
            memStream.Position = 0;

            return memStream;
        }

        private static void Render_FrontContent(PdfContentByte canvas, Member member)
        {
            Font f = new Font(baseFont, 12, Font.BOLD) { Color = BaseColor.WHITE };
            int bottom = CARD_HEIGHT - (UPPER_BAR_HEIGHT + PHOTO_BAR_HEIGHT) - 12 - 2;
            Phrase p = new Phrase(member.FullName, f);
            ColumnText.ShowTextAligned(canvas, Element.ALIGN_CENTER, p, CARD_WIDTH / 2, bottom, 0);
            
            bottom -= 14;
            f = new Font(baseFont, 12) { Color = BaseColor.WHITE };
            p = new Phrase(member.WacLevel.ToString(), f);
            ColumnText.ShowTextAligned(canvas, Element.ALIGN_CENTER, p, CARD_WIDTH / 2, bottom, 0);
            
            bottom -= 12;
            f = new Font(baseFont, 10, Font.BOLD) { Color = BaseColor.WHITE };
            p = new Phrase("KCSO DEM# " + member.DEM, f);
            ColumnText.ShowTextAligned(canvas, Element.ALIGN_CENTER, p, CARD_WIDTH / 2, bottom, 0);

            BarcodePDF417 barcode = new BarcodePDF417();
            barcode.SetText(string.Format("Unknown Text. To be added. King County Search and Rescue{0}{1}{2}", member.LastName, member.FirstName, member.DEM));
            barcode.CodeRows = 20;
            barcode.CodeColumns = 7;
            barcode.Options = BarcodePDF417.PDF417_FIXED_RECTANGLE;
            var img = barcode.GetImage();
            img.ScaleAbsolute(CARD_WIDTH - 30, 28);
            img.SetAbsolutePosition((CARD_WIDTH - img.ScaledWidth) / 2, CARD_HEIGHT - UPPER_BAR_HEIGHT - PHOTO_BAR_HEIGHT - LOWER_BAR_HEIGHT - img.ScaledHeight - 8);
            canvas.AddImage(img);
            
            if (!string.IsNullOrWhiteSpace(member.PhotoFile))
            {
                var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "auth", "members", member.PhotoFile);
                if (File.Exists(file))
                {
                    img = Image.GetInstance(file);
                    img.ScaleToFit(PHOTO_WIDTH, PHOTO_BAR_HEIGHT);
                    img.SetAbsolutePosition(CARD_WIDTH - img.ScaledWidth, CARD_HEIGHT - (UPPER_BAR_HEIGHT + img.ScaledHeight));
                    canvas.AddImage(img);
                }
            }
        }

        private static void Render_FrontBase(PdfContentByte under, DateTime expiration)
        {
            float BORDER_WIDTH = 1.5f;

            // Draw background stripes and bars
            under.SaveState();
            under.SetRGBColorFill(0xff, 0x45, 0x00);
            under.Rectangle(0, CARD_HEIGHT - (UPPER_BAR_HEIGHT + PHOTO_BAR_HEIGHT + LOWER_BAR_HEIGHT), CARD_WIDTH, (UPPER_BAR_HEIGHT + PHOTO_BAR_HEIGHT + LOWER_BAR_HEIGHT));
            under.Fill();
            under.SetRGBColorFill(0x0, 0x0, 0x0);
            under.Rectangle(0, CARD_HEIGHT - (UPPER_BAR_HEIGHT + PHOTO_BAR_HEIGHT) - BORDER_WIDTH, CARD_WIDTH, PHOTO_BAR_HEIGHT + 2 * BORDER_WIDTH);
            under.Fill();
            under.SetRGBColorFill(0xff, 0xff, 0xff);
            under.Rectangle(0, CARD_HEIGHT - (UPPER_BAR_HEIGHT + PHOTO_BAR_HEIGHT), CARD_WIDTH, PHOTO_BAR_HEIGHT);
            under.Fill();
            under.RestoreState();

            var img = Image.GetInstance(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "images", "sheriff.png"));
            img.ScaleToFit(CARD_WIDTH - PHOTO_WIDTH - 8, PHOTO_BAR_HEIGHT);
            img.SetAbsolutePosition(
                ((CARD_WIDTH - PHOTO_WIDTH) - img.ScaledWidth) / 2,
                CARD_HEIGHT - (UPPER_BAR_HEIGHT + img.ScaledHeight) - 4);

            under.AddImage(img);
            
            Font f = new Font(baseFont, 8) { Color = BaseColor.WHITE };

            Phrase p = new Phrase("Expiration:", f);
            ColumnText.ShowTextAligned(under, Element.ALIGN_LEFT, p, 4, CARD_HEIGHT - 10, 0);
            p = new Phrase(expiration.ToString("MM/dd/yyyy"), f);
            ColumnText.ShowTextAligned(under, Element.ALIGN_RIGHT, p, CARD_WIDTH - 4, CARD_HEIGHT - 10, 0);

            f = new Font(baseFont, 11) { Color = BaseColor.WHITE };
            p = new Phrase("King County", f);
            ColumnText.ShowTextAligned(under, Element.ALIGN_CENTER, p, CARD_WIDTH / 2, CARD_HEIGHT - 34, 0);

            f = new Font(baseFont, 14, Font.BOLD) { Color = BaseColor.WHITE };
            
            p = new Phrase("Search & Rescue", f);
            ColumnText.ShowTextAligned(under, Element.ALIGN_CENTER, p, CARD_WIDTH / 2, CARD_HEIGHT - 49, 0);
        }

        private static void Render_BackContent(PdfContentByte canvas, Member member, bool passport)
        {
            Font f = new Font(baseFont, 12, Font.BOLD);
            int bottom = CARD_HEIGHT - 32;
            Phrase p = new Phrase(member.FullName, f);
            ColumnText.ShowTextAligned(canvas, Element.ALIGN_CENTER, p, CARD_WIDTH / 2, bottom, 0);

            if (passport)
            {
                bottom = CARD_HEIGHT - 170;
                f = new Font(baseFont, 16f, Font.BOLD);
                p = new Phrase("PASSPORT", f);
                ColumnText.ShowTextAligned(canvas, Element.ALIGN_CENTER, p, CARD_WIDTH / 2, bottom, 0);
            }
            else
            {
                bottom = CARD_HEIGHT - 159;
                f = new Font(baseFont, 11f);
                p = new Phrase(member.WacLevel.ToString(), f);
                ColumnText.ShowTextAligned(canvas, Element.ALIGN_CENTER, p, CARD_WIDTH / 2, bottom, 0);

                bottom -= 12;
                p = new Phrase("KCSO DEM# " + member.DEM, f);
                ColumnText.ShowTextAligned(canvas, Element.ALIGN_CENTER, p, CARD_WIDTH / 2, bottom, 0);
            }

            BarcodePDF417 barcode = new BarcodePDF417();
            barcode.SetText(string.Format("Unknown Text. To be added. KCSAR{0}{1}{2}", member.LastName, member.FirstName, member.DEM));
            barcode.CodeRows = 9;
            barcode.CodeColumns = 7;
            barcode.Options = BarcodePDF417.PDF417_FIXED_RECTANGLE;
            var img = barcode.GetImage();
            img.ScaleAbsolute(CARD_WIDTH - 30, 14);
            img.SetAbsolutePosition((CARD_WIDTH - img.ScaledWidth) / 2, 50);
            canvas.AddImage(img);

        }

        private static void Render_BackBase(PdfContentByte under)
        {
            // Y relative to card top, line width, <repeat>
            float[] measures = new[] { 37, 3f, 144, 1.5f };
            
            under.SaveState();
            for (int i = 0; i < measures.Length / 2; i++)
            {
                under.SetLineWidth(measures[i * 2 + 1]);
                under.MoveTo(0, CARD_HEIGHT - measures[i * 2]);
                under.LineTo(CARD_WIDTH, CARD_HEIGHT - measures[i * 2]);
                under.Stroke();
            }

            under.SetFontAndSize(baseFont, 7);
            under.ShowTextAlignedKerned(Element.ALIGN_CENTER, "If found, please call KCSAR at (206) 205-8226", CARD_WIDTH / 2, 7, 0);
            
            under.RestoreState();
        }
    }
}