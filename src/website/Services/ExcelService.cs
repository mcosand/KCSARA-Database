/*
 * Copyright 2012-2014 Matthew Cosand
 */

namespace Kcsara.Database.Web
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using EPPlus = OfficeOpenXml;
  using Npoi = NPOI.HSSF.UserModel;
  using System.Linq;

  /// <summary>
  /// Excel file format/extensions understood by this service.
  /// </summary>
  public enum ExcelFileType
  {
    /// <summary>Legacy Excel file</summary>
    XLS,

    /// <summary>Excel 2007+ file</summary>
    XLSX
  }

  /// <summary>Provides a service that can operate on Excel spreadsheets.</summary>
  public class ExcelService
  {
    /// <summary>Create a new Workbook.</summary>
    /// <param name="type">The version of Excel to target.</param>
    /// <returns>A facade over the specific Excel libraries.</returns>
    public static ExcelFile Create(ExcelFileType type)
    {
      ExcelFile newFile;
      switch (type)
      {
        case ExcelFileType.XLS:
          newFile = new NpoiExcelFile();
          break;
        case ExcelFileType.XLSX:
          newFile = new EPPlusExcelFile();
          break;
        default:
          throw new InvalidOperationException();
      }
      return newFile;
    }

    /// <summary>Read an Excel file into memory.</summary>
    /// <param name="inputStream">The workbook to read.</param>
    /// <param name="type">The version of file being read.</param>
    /// <returns>A facade over the specific Excel libraries.</returns>
    public static ExcelFile Read(System.IO.Stream inputStream, ExcelFileType type)
    {
      ExcelFile file;
      switch (type)
      {
        case ExcelFileType.XLS:
          file = new NpoiExcelFile(inputStream);
          break;
        case ExcelFileType.XLSX:
          file = new EPPlusExcelFile(inputStream);
          break;
        default:
          throw new InvalidOperationException();
      }
      return file;
    }
  }

  #region ExcelFile
  /// <summary>A facade over the specific Excel libraries.</summary>
  public abstract class ExcelFile : IDisposable
  {
    /// <summary>Save the workbook to the specified stream.</summary>
    /// <param name="stream">The stream that will receive the file contents.</param>
    public abstract void Save(System.IO.Stream stream);

    /// <summary>Append a new sheet to the workbook.</summary>
    /// <param name="name">The name of the new sheet.</param>
    /// <returns>A facade representing the new sheet.</returns>
    public abstract ExcelSheet CreateSheet(string name);

    /// <summary>Append a new sheet to the workbook.</summary>
    /// <param name="from">The name of the original sheet.</param>
    /// <param name="to">The name of the new sheet.</param>
    /// <returns>A facade representing the new sheet.</returns>
    public abstract ExcelSheet CopySheet(string from, string to);

    /// <summary>Get the sheet at the specific index.</summary>
    /// <param name="index">0-based index of the sheet.</param>
    /// <returns>A facade representing the sheet.</returns>
    public abstract ExcelSheet GetSheet(int index);

    /// <summary>Get a worksheet by name.</summary>
    /// <param name="name">Name of the desired sheet.</param>
    /// <returns>A facade representing the sheet.</returns>
    public abstract ExcelSheet GetSheet(string name);

    /// <summary>Gets the MIME type of this file.</summary>
    public abstract string Mime { get; }

    /// <summary>Adds the appropriate file extension to the filename.</summary>
    /// <param name="filename">The root of the filename.</param>
    /// <returns>The filename with the appropriate extension.</returns>
    public abstract string AddExtension(string filename);

    /// <summary>Standard Dispose method</summary>
    public abstract void Dispose();
  }

  /// <summary>Facade over XLSX workbook</summary>
  public class EPPlusExcelFile : ExcelFile
  {
    EPPlus.ExcelPackage _package;
    Dictionary<EPPlus.ExcelWorksheet, ExcelSheet> _sheets = new Dictionary<EPPlus.ExcelWorksheet, ExcelSheet>();

    /// <summary>Default Constructor</summary>
    public EPPlusExcelFile()
    {
      _package = new EPPlus.ExcelPackage();
    }

    /// <summary>Constructor for existing file.</summary>
    /// <param name="stream">The file to read.</param>
    public EPPlusExcelFile(System.IO.Stream stream)
    {
      _package = new EPPlus.ExcelPackage(stream);
    }

    /// <summary>Append a new sheet to the workbook.</summary>
    /// <param name="name">The name of the new sheet.</param>
    /// <returns>A facade representing the new sheet.</returns>
    public override ExcelSheet CreateSheet(string name)
    {
      var native = this._package.Workbook.Worksheets.Add(name);
      return ResolveSheet(native);
    }

    /// <summary>Get the sheet at the specific index.</summary>
    /// <param name="index">0-based index of the sheet.</param>
    /// <returns>A facade representing the sheet.</returns>
    public override ExcelSheet GetSheet(int index)
    {
      EPPlus.ExcelWorksheet native = this._package.Workbook.Worksheets[index + 1];
      return ResolveSheet(native);
    }

    /// <summary>Get a worksheet by name.</summary>
    /// <param name="name">Name of the desired sheet.</param>
    /// <returns>A facade representing the sheet.</returns>
    public override ExcelSheet GetSheet(string name)
    {
      EPPlus.ExcelWorksheet native = this._package.Workbook.Worksheets[name];
      return ResolveSheet(native);
    }

    /// <summary>Append a new sheet to the workbook.</summary>
    /// <param name="from">The name of the original sheet.</param>
    /// <param name="to">The name of the new sheet.</param>
    /// <returns>A facade representing the new sheet.</returns>
    public override ExcelSheet CopySheet(string from, string to)
    {
      var native = this._package.Workbook.Worksheets.Add(to, this._package.Workbook.Worksheets[from]);
      return ResolveSheet(native);
    }

    private ExcelSheet ResolveSheet(EPPlus.ExcelWorksheet native)
    {
      if (!_sheets.ContainsKey(native))
      {
        _sheets.Add(native, new EPPlusExcelSheet(native));
      }
      return _sheets[native];
    }

    /// <summary>Save the workbook to the specified stream.</summary>
    /// <param name="stream">The stream that will receive the file contents.</param>
    public override void Save(System.IO.Stream stream)
    {
      _package.SaveAs(stream);
    }

    /// <summary>Gets the MIME type of this file.</summary>
    public override string Mime
    {
      get { return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; }
    }

    /// <summary>Adds the appropriate file extension to the filename.</summary>
    /// <param name="filename">The root of the filename.</param>
    /// <returns>The filename with the appropriate extension.</returns>
    public override string AddExtension(string filename)
    {
      return filename + ".xlsx";
    }

    /// <summary>Standard Dispose method</summary>
    public override void Dispose()
    {
      _package.Dispose();
    }
  }

  /// <summary>Facade over XLS File</summary>
  public class NpoiExcelFile : ExcelFile
  {
    Npoi.HSSFWorkbook _workbook;
    Dictionary<Npoi.HSSFSheet, ExcelSheet> _sheets = new Dictionary<Npoi.HSSFSheet, ExcelSheet>();

    /// <summary>Default constructor</summary>
    public NpoiExcelFile()
    {
      _workbook = new Npoi.HSSFWorkbook();
    }

    /// <summary>Constructor for existing workbook</summary>
    /// <param name="input">The file to read</param>
    public NpoiExcelFile(System.IO.Stream input)
    {
      _workbook = new Npoi.HSSFWorkbook(input);
    }

    /// <summary>Append a new sheet to the workbook.</summary>
    /// <param name="name">The name of the new sheet.</param>
    /// <returns>A facade representing the new sheet.</returns>
    public override ExcelSheet CreateSheet(string name)
    {
      var native = (Npoi.HSSFSheet)this._workbook.CreateSheet(name);
      return ResolveSheet(native);
    }

    /// <summary>Get the sheet at the specific index.</summary>
    /// <param name="index">0-based index of the sheet.</param>
    /// <returns>A facade representing the sheet.</returns>
    public override ExcelSheet GetSheet(int index)
    {
      Npoi.HSSFSheet native = (Npoi.HSSFSheet)this._workbook.GetSheetAt(index);
      return ResolveSheet(native);
    }

    /// <summary>Append a new sheet to the workbook.</summary>
    /// <param name="from">The name of the original sheet.</param>
    /// <param name="to">The name of the new sheet.</param>
    /// <returns>A facade representing the new sheet.</returns>
    public override ExcelSheet CopySheet(string from, string to)
    {
      int srcIdx = this._workbook.GetSheetIndex(this._workbook.GetSheet(from));
      var native = (Npoi.HSSFSheet)this._workbook.CloneSheet(srcIdx);

      this._workbook.SetSheetName(this._workbook.GetSheetIndex(native), to);
      return ResolveSheet(native);
    }

    /// <summary>Get a worksheet by name.</summary>
    /// <param name="name">Name of the desired sheet.</param>
    /// <returns>A facade representing the sheet.</returns>
    public override ExcelSheet GetSheet(string name)
    {
      Npoi.HSSFSheet native = (Npoi.HSSFSheet)this._workbook.GetSheet(name);
      return ResolveSheet(native);
    }

    private ExcelSheet ResolveSheet(Npoi.HSSFSheet native)
    {
      if (!_sheets.ContainsKey(native))
      {
        _sheets.Add(native, new NpoiExcelSheet(native));
      }
      return _sheets[native];
    }

    /// <summary>Save the workbook to the specified stream.</summary>
    /// <param name="stream">The stream that will receive the file contents.</param>
    public override void Save(System.IO.Stream stream)
    {
      this._workbook.Write(stream);
    }

    /// <summary>Gets the MIME type of this file.</summary>
    public override string Mime
    {
      get { return "application/vnd.ms-excel"; }
    }

    /// <summary>Adds the appropriate file extension to the filename.</summary>
    /// <param name="filename">The root of the filename.</param>
    /// <returns>The filename with the appropriate extension.</returns>
    public override string AddExtension(string filename)
    {
      return filename + ".xls";
    }

    /// <summary>Standard Dispose method</summary>
    public override void Dispose()
    {
    }
  }
  #endregion

  #region ExcelSheet
  /// <summary>Facade around an Excel worksheet.</summary>
  public abstract class ExcelSheet
  {
    /// <summary>Gets the cell located at a specific address.</summary>
    /// <param name="row">The 0-based row.</param>
    /// <param name="col">The 0-based column.</param>
    /// <returns>The cell at the specified location.</returns>
    public abstract ExcelCell CellAt(int row, int col);

    /// <summary>Gets the name of the sheet.</summary>
    public abstract string Name { get; }

    /// <summary>Gets or sets the sheet header text.</summary>
    public abstract string Header { get; set; }

    /// <summary>Gets or sets the sheet footer text.</summary>
    public abstract string Footer { get; set; }

    /// <summary>Gets the number of rows in the sheet.</summary>
    public abstract int NumRows { get; }

    /// <summary>
    /// Runs auto fit on all columns. Right most column is the right most cell touched by this class instance.
    /// </summary>
    public abstract void AutoFitAll();
  }

  /// <summary>
  /// Facade around an XLS worksheet
  /// </summary>
  public class EPPlusExcelSheet : ExcelSheet
  {
    private EPPlus.ExcelWorksheet _sheet;
    Dictionary<EPPlus.ExcelRange, ExcelCell> _cells = new Dictionary<EPPlus.ExcelRange, ExcelCell>();

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="sheet">The XLS sheet to wrap</param>
    public EPPlusExcelSheet(EPPlus.ExcelWorksheet sheet)
    {
      this._sheet = sheet;
    }

    /// <summary>Gets the cell located at a specific address.</summary>
    /// <param name="row">The 0-based row.</param>
    /// <param name="col">The 0-based column.</param>
    /// <returns>The cell at the specified location.</returns>
    public override ExcelCell CellAt(int row, int col)
    {
      return ResolveCell(this._sheet.Cells[row + 1, col + 1]);
    }

    private ExcelCell ResolveCell(EPPlus.ExcelRange native)
    {
      if (!_cells.ContainsKey(native))
      {
        _cells.Add(native, new EPPlusExcelCell(native));
      }
      return _cells[native];
    }

    /// <summary>Gets the name of the sheet.</summary>
    public override string Name
    {
      get { return this._sheet.Name; }
    }

    /// <summary>Gets the number of rows in the sheet.</summary>
    public override int NumRows
    {
      get
      {
        return this._sheet.Dimension.End.Row;
      }
    }

    /// <summary>Gets or sets the sheet header text.</summary>
    public override string Header
    {
      get
      {
        return this._sheet.HeaderFooter.FirstHeader.CenteredText;
      }
      set
      {
        this._sheet.HeaderFooter.FirstHeader.CenteredText = value;
      }
    }

    /// <summary>Gets or sets the sheet footer text.</summary>
    public override string Footer
    {
      get
      {
        return this._sheet.HeaderFooter.FirstFooter.CenteredText;
      }
      set
      {
        this._sheet.HeaderFooter.FirstFooter.CenteredText = value;
      }
    }

    /// <summary>
    /// Runs auto fit on all columns. Right most column is the right most cell touched by this class instance.
    /// </summary>
    public override void AutoFitAll()
    {
      if (this._cells.Count == 0) return;

      for (int i = 1; i <= this._cells.Values.Max(f => f.ColumnIndex); i++)
      {
        this._sheet.Column(i).AutoFit();
      }
    }
  }

  /// <summary>
  /// Wrapper for an XLSX Excel file
  /// </summary>
  public class NpoiExcelSheet : ExcelSheet
  {
    private Npoi.HSSFSheet _sheet;
    Dictionary<Npoi.HSSFCell, ExcelCell> _cells = new Dictionary<Npoi.HSSFCell, ExcelCell>();

    /// <summary>Default Constructor</summary>
    /// <param name="sheet">The native XLSX sheet</param>
    public NpoiExcelSheet(Npoi.HSSFSheet sheet)
    {
      this._sheet = sheet;
    }

    /// <summary>Gets the cell located at a specific address.</summary>
    /// <param name="row">The 0-based row.</param>
    /// <param name="col">The 0-based column.</param>
    /// <returns>The cell at the specified location.</returns>
    public override ExcelCell CellAt(int row, int col)
    {
      var dataRow = this._sheet.GetRow(row);
      if (dataRow == null)
      {
        dataRow = this._sheet.CreateRow(row);
      }

      if (col > dataRow.LastCellNum)
      {
        dataRow.CreateCell(col);
      }

      Npoi.HSSFCell cell = dataRow.GetCell(col) as Npoi.HSSFCell;
      if (cell == null)
      {
        cell = (Npoi.HSSFCell)dataRow.CreateCell(col);
      }

      return ResolveCell(cell);
    }

    private ExcelCell ResolveCell(Npoi.HSSFCell native)
    {
      if (!_cells.ContainsKey(native))
      {
        _cells.Add(native, new NpoiExcelCell(native));
      }
      return _cells[native];
    }

    /// <summary>Gets the name of the sheet.</summary>
    public override string Name
    {
      get { return this._sheet.SheetName; }
    }

    /// <summary>Gets the number of rows in the current sheet.</summary>
    public override int NumRows
    {
      get { return this._sheet.LastRowNum; }
    }

    /// <summary>Gets or sets the sheet header text.</summary>
    public override string Header
    {
      get
      {
        return this._sheet.Header.Center;
      }
      set
      {
        this._sheet.Header.Center = value;
      }
    }

    /// <summary>Gets or sets the sheet footer text.</summary>
    public override string Footer
    {
      get
      {
        return this._sheet.Footer.Center;
      }
      set
      {
        this._sheet.Footer.Center = value;
      }
    }

    /// <summary>
    /// Runs auto fit on all columns. Right most column is the right most cell touched by this class instance.
    /// </summary>
    public override void AutoFitAll()
    {
      if (this._cells.Count == 0) return;

      for (int i = 0; i <= this._cells.Values.Max(f => f.ColumnIndex); i++)
      {
        this._sheet.AutoSizeColumn(i);
      }
    }
  }
  #endregion

  /// <summary>Wrapper around an Excel cell</summary>
  public abstract class ExcelCell
  {
    /// <summary>Sets the value of the cell to a number.</summary>
    /// <param name="value">The new numeric value.</param>
    public abstract void SetValue(double value);

    /// <summary>Sets the value of the cell to a string.</summary>
    /// <param name="value">The new string value.</param>
    public abstract void SetValue(string value);

    public abstract void SetValue(DateTime? value);

    /// <summary>Gets the numeric value of the cell.</summary>
    public abstract double? NumericValue { get; }

    /// <summary>Gets the string value of the cell.</summary>
    public abstract string StringValue { get; }

    /// <summary>Gets the date stored in the cell.</summary>
    public abstract DateTime? DateValue { get; }

    /// <summary>Sets the background color of the cell.</summary>
    /// <param name="color">The desired color of the cell background. null will set the cell to 'NoFill'</param>
    public abstract void SetFillColor(Color? color);

    /// <summary>Sets the border color of the cell.</summary>
    /// <param name="color">The desired color of the border. null will remove the border.</param>
    public abstract void SetBorderColor(Color? color);

    /// <summary>Sets the foreground color of the cell.</summary>
    /// <param name="color">The desired foreground color.</param>
    public abstract void SetFontColor(Color? color);

    /// <summary>Sets whether or not the cell has a bold font.</summary>
    /// <param name="bold">true iff the cell should be bold.</param>
    public abstract void SetBold(bool bold);

    /// <summary>Sets whether or not the cell wraps its text.</summary>
    /// <param name="wrap">true iff the text should wrap.</param>
    public abstract void SetTextWrap(bool wrap);

    /// <summary>Gets the index of this cell's column.</summary>
    public abstract int ColumnIndex { get; }
  }

  /// <summary>Facade implementation for an XLSX Excel cell</summary>
  public class EPPlusExcelCell : ExcelCell
  {
    private EPPlus.ExcelRange _cell;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="cell">The native representation of this cell.</param>
    public EPPlusExcelCell(EPPlus.ExcelRange cell)
    {
      _cell = cell;
    }

    public override void SetValue(DateTime? value)
    {
      if (value.HasValue) { this._cell.Value = value.Value;  }
      else { this._cell.Value = ""; }
      _cell.Style.Numberformat.Format = "yyyy-mm-dd";
    }

    /// <summary>Sets the value of the cell to a number.</summary>
    /// <param name="value">The new numeric value.</param>
    public override void SetValue(double value)
    {
      this._cell.Value = value;
    }

    /// <summary>Sets the value of the cell to a string.</summary>
    /// <param name="value">The new string value.</param>
    public override void SetValue(string value)
    {
      this._cell.Value = value;
    }

    /// <summary>Gets the numeric value of the cell.</summary>
    public override double? NumericValue
    {
      get { double d; if (!double.TryParse(string.Format("{0}", this._cell.Value), out d)) return null; return d; }
    }

    /// <summary>Gets the date stored in the cell.</summary>
    public override DateTime? DateValue
    {
      get { return typeof(DateTime).IsAssignableFrom(this._cell.Value.GetType()) ? (DateTime?)this._cell.Value : null; }
    }

    /// <summary>Gets the string value of the cell.</summary>
    public override string StringValue
    {
      get { return string.Format("{0}", this._cell.Value); }
    }

    /// <summary>Sets the background color of the cell.</summary>
    /// <param name="color">The desired color of the cell background. null will set the cell to 'NoFill'</param>
    public override void SetFillColor(Color? color)
    {
      this._cell.Style.Fill.BackgroundColor.SetColor(color ?? Color.White);
    }

    /// <summary>Sets the border color of the cell.</summary>
    /// <param name="color">The desired color of the border. null will remove the border.</param>
    public override void SetBorderColor(Color? color)
    {
      var b = this._cell.Style.Border;
      foreach (var border in new[] { b.Bottom, b.Left, b.Top, b.Right })
      {
        if (color.HasValue)
        {
          border.Style = EPPlus.Style.ExcelBorderStyle.Medium;
          border.Color.SetColor(color.Value);
        }
        else
        {
          border.Style = EPPlus.Style.ExcelBorderStyle.None;
        }
      }
    }

    /// <summary>Sets the foreground color of the cell.</summary>
    /// <param name="color">The desired foreground color.</param>
    public override void SetFontColor(Color? color)
    {
      this._cell.Style.Font.Color.SetColor(color ?? Color.Black);
    }

    /// <summary>Sets whether or not the cell has a bold font.</summary>
    /// <param name="bold">true iff the cell should be bold.</param>
    public override void SetBold(bool bold)
    {
      this._cell.Style.Font.Bold = bold;
    }

    /// <summary>Sets whether or not the cell wraps its text.</summary>
    /// <param name="wrap">true iff the text should wrap.</param>
    public override void SetTextWrap(bool wrap)
    {
      this._cell.Style.WrapText = wrap;
    }

    /// <summary>Gets the index of this cell's column.</summary>
    public override int ColumnIndex
    {
      get { return this._cell.End.Column; }
    }
  }

  /// <summary>Facade implementation for an XLSX Excel cell</summary>
  public class NpoiExcelCell : ExcelCell
  {
    private Npoi.HSSFCell _cell;

    /// <summary>Default constructor</summary>
    /// <param name="cell">NPOI native encapsulation of the cell.</param>
    public NpoiExcelCell(Npoi.HSSFCell cell)
    {
      _cell = cell;
    }

    public override void SetValue(DateTime? value)
    {
      throw new NotImplementedException();
    }

    /// <summary>Sets the value of the cell to a number.</summary>
    /// <param name="value">The new numeric value.</param>
    public override void SetValue(double value)
    {
      this._cell.SetCellValue(value);
    }

    /// <summary>Sets the value of the cell to a string.</summary>
    /// <param name="value">The new string value.</param>
    public override void SetValue(string value)
    {
      this._cell.SetCellValue(value);
    }

    /// <summary>Gets the numeric value of the cell.</summary>
    public override double? NumericValue
    {
      get { return (this._cell.CellType == NPOI.SS.UserModel.CellType.NUMERIC) ? this._cell.NumericCellValue : (double?)null; }
    }

    /// <summary>Gets the string value of the cell.</summary>
    public override string StringValue
    {
      get
      {
        object o;
        switch (this._cell.CellType)
        {
          case NPOI.SS.UserModel.CellType.NUMERIC:
            o = this._cell.NumericCellValue;
            break;
          case NPOI.SS.UserModel.CellType.STRING:
            o = this._cell.StringCellValue;
            break;
          case NPOI.SS.UserModel.CellType.BLANK:
            return null;
          default:
            throw new NotImplementedException(this._cell.CellType.ToString());
        }
        return string.Format("{0}", o);
      }
    }

    /// <summary>Gets the date stored in the cell.</summary>
    public override DateTime? DateValue
    {
      get { return this._cell.DateCellValue; }
    }

    static Dictionary<Color, short> colorLookup = new Dictionary<Color, short> {
            { Color.Pink, NPOI.HSSF.Util.HSSFColor.ROSE.index },
            { Color.Green, NPOI.HSSF.Util.HSSFColor.GREEN.index },
            { Color.White, NPOI.HSSF.Util.HSSFColor.WHITE.index },
            { Color.Orange, NPOI.HSSF.Util.HSSFColor.LIGHT_ORANGE.index },
            { Color.Red, NPOI.HSSF.Util.HSSFColor.RED.index }

        };

    /// <summary>Sets the border color of the cell.</summary>
    /// <param name="color">The desired color of the border. null will remove the border.</param>
    public override void SetBorderColor(Color? color)
    {
      _cell.CellStyle = CloneStyle();

      if (color.HasValue)
      {
        this._cell.CellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.MEDIUM;
        this._cell.CellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.MEDIUM;
        this._cell.CellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.MEDIUM;
        this._cell.CellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.MEDIUM;
        this._cell.CellStyle.BottomBorderColor = colorLookup[color.Value];
        this._cell.CellStyle.LeftBorderColor = colorLookup[color.Value];
        this._cell.CellStyle.RightBorderColor = colorLookup[color.Value];
        this._cell.CellStyle.TopBorderColor = colorLookup[color.Value];
      }
      else
      {
        this._cell.CellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.NONE;
        this._cell.CellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.NONE;
        this._cell.CellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.NONE;
        this._cell.CellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.NONE;
      }
    }

    /// <summary>Sets the background color of the cell.</summary>
    /// <param name="color">The desired color of the cell background. null will set the cell to 'NoFill'</param>
    public override void SetFillColor(Color? color)
    {
      _cell.CellStyle = CloneStyle();
      if (color.HasValue)
      {
        this._cell.CellStyle.FillForegroundColor = colorLookup[color.Value];
        this._cell.CellStyle.FillPattern = NPOI.SS.UserModel.FillPatternType.SOLID_FOREGROUND;
      }
      else
      {
        this._cell.CellStyle.FillPattern = NPOI.SS.UserModel.FillPatternType.NO_FILL;
      }
    }

    /// <summary>Sets the foreground color of the cell.</summary>
    /// <param name="color">The desired foreground color.</param>
    public override void SetFontColor(Color? color)
    {
      _cell.CellStyle = CloneStyle();
      var f = _cell.CellStyle.GetFont(this._cell.Sheet.Workbook);

      _cell.CellStyle.SetFont(FindOrCreateFont(f.Boldweight, colorLookup[color ?? Color.Black], f.FontHeight, f.FontName, f.IsItalic, f.IsStrikeout, f.TypeOffset, f.Underline));
    }

    /// <summary>Sets whether or not the cell has a bold font.</summary>
    /// <param name="bold">true iff the cell should be bold.</param>
    public override void SetBold(bool bold)
    {
      _cell.CellStyle = CloneStyle();

      var f = _cell.CellStyle.GetFont(this._cell.Sheet.Workbook);
      short b = (short)(bold ? NPOI.SS.UserModel.FontBoldWeight.BOLD : NPOI.SS.UserModel.FontBoldWeight.NORMAL);

      _cell.CellStyle.SetFont(FindOrCreateFont(b, f.Color, f.FontHeight, f.FontName, f.IsItalic, f.IsStrikeout, f.TypeOffset, f.Underline));
    }

    private NPOI.SS.UserModel.IFont FindOrCreateFont(short bold, short color, short height, string name, bool italic, bool strikeout, short offset, byte underline)
    {
      var font = _cell.Sheet.Workbook.FindFont(bold, color, height, name, italic, strikeout, offset, underline);
      if (font == null)
      {
        font = _cell.Sheet.Workbook.CreateFont();
        font.Boldweight = bold;
        font.Color = color;
        font.FontHeight = height;
        font.FontName = name;
        font.IsItalic = italic;
        font.IsStrikeout = strikeout;
        font.TypeOffset = offset;
        font.Underline = underline;
      }
      return font;
    }

    /// <summary>Sets whether or not the cell wraps its text.</summary>
    /// <param name="wrap">true iff the text should wrap.</param>
    public override void SetTextWrap(bool wrap)
    {
      _cell.CellStyle = CloneStyle();
      _cell.CellStyle.WrapText = wrap;
    }

    /// <summary>Gets the index of this cell's column.</summary>
    public override int ColumnIndex
    {
      get { return this._cell.ColumnIndex; }
    }

    private NPOI.SS.UserModel.ICellStyle myStyle = null;

    private NPOI.SS.UserModel.ICellStyle CloneStyle()
    {
      if (myStyle == null)
      {
        myStyle = this._cell.Sheet.Workbook.CreateCellStyle();
        myStyle.BorderBottom = this._cell.CellStyle.BorderBottom;
        myStyle.BorderLeft = this._cell.CellStyle.BorderLeft;
        myStyle.BorderRight = this._cell.CellStyle.BorderRight;
        myStyle.BorderTop = this._cell.CellStyle.BorderTop;
        myStyle.FillForegroundColor = this._cell.CellStyle.FillForegroundColor;
        myStyle.FillPattern = this._cell.CellStyle.FillPattern;
      }
      return myStyle;
    }
  }
}
