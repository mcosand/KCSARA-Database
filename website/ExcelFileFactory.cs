namespace Kcsar.Database
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    //using GemBox.ExcelLite;
    using GemBox.Spreadsheet;
    using System.Reflection;

    public static class ExcelFileFactory
    {
        public static ExcelFile CreateExcelFile()
        {
            ExcelFile ex = new ExcelFile();
            ex.LimitNear += delegate(object sender_x, LimitEventArgs e_x) { e_x.WriteWarningWorksheet = false; };

            return ex;
        }

        public static ExcelFile ReadExcelStream(System.IO.Stream stream, string filename)
        {
            ExcelFile xl = CreateExcelFile();

            if (filename.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                xl.LoadXlsx(stream, XlsxOptions.None);
            }
            else
            {
                xl.LoadXls(stream, XlsOptions.None);
            }

            return xl;
        }
    }
}
