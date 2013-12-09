
namespace Kcsar.Database
{
    using Kcsara.Database.Web;
    using System;
    using System.Drawing;

    public class SheetAutoFitWrapper : IDisposable
    {
        public ExcelSheet Sheet { get; private set; }
        private float[] maxWidths = new float[50];
        private Graphics g;
        private Bitmap bmp;
        private float zeroWidth;
        private int maxColumn = 0;

        public SheetAutoFitWrapper(ExcelFile file, ExcelSheet sheet)
        {
            this.Sheet = sheet;
            bmp = new Bitmap(1, 1);
            g = Graphics.FromImage(bmp);
//            zeroWidth = Measure("0", new Font(file.DefaultFontName, file.DefaultFontSize / 20, GraphicsUnit.Point));
        }

        public void SetCellValue(string v, int row, int col)
        {
            if (col > maxColumn)
            {
                maxColumn = col;
            }

            ExcelCell c = this.Sheet.CellAt(row, col);
            c.SetValue(v);
//            c.Style.VerticalAlignment = VerticalAlignmentStyle.Top;

            //ExcelFont f = c.Style.Font;
            //float width = Measure(v, new Font(f.Name, f.Size / 20, GraphicsUnit.Point));
            float width = 0;
            if (width > maxWidths[col])
            {
                maxWidths[col] = width;
            }
        }

        public void AutoFit()
        {
            //int colCount = this.maxColumn;
            //while (colCount > 0)
            //{
            //    colCount--;
            //    this.Sheet.Columns[colCount].Width = Math.Max(750, (int)((maxWidths[colCount] / zeroWidth) * 256.0 * 1.2));
            //}
        }

        private float Measure(string str, Font font)
        {
            return g.MeasureString(str, font, new PointF(0, 0),
                    new StringFormat(StringFormat.GenericTypographic)
                    {
                        Trimming = StringTrimming.None,
                        Alignment = StringAlignment.Near,
                        FormatFlags = StringFormatFlags.MeasureTrailingSpaces
                    }
                    ).Width;
        }

        #region IDisposable Members

        public void Dispose()
        {
            g.Dispose();
            bmp.Dispose();
        }

        #endregion
    }
}
