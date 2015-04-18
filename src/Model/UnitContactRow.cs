/*
 * Copyright 2013-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using System.Linq;
  using System.Text.RegularExpressions;

  [Table("UnitContacts")]
  public class UnitContactRow : ModelObjectRow
  {
    public string Type { get; set; }
    public string Value { get; set; }
    [ForeignKey("UnitId")]
    public virtual UnitRow Unit { get; set; }
    public Guid UnitId { get; set; }

    public string Purpose { get; set; }

    public static readonly string[] AllowedTypes = new string[] { "email", "phone" };

    public static bool TryParse(string type, string input, bool strict, out string result)
    {
      result = input;

      if (string.IsNullOrEmpty(input))
      {
        return false;
      }

      if (type.ToLower() == "phone")
      {
        // Allow custom (international) numbers if they start with '+'
        if (input[0] == '+')
        {
          return true;
        }

        string pattern = strict ? @"^\d{3}\-\d{3}-\d{4}( x\d+)?$" : @"^\(?(\d{3})\)?[ \.\-]*(\d{3})[ \.\-]*(\d{4})(\s*e?xt?\s*(\d+))?$";
        Match m = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
        if (!m.Success)
        {
          return false;
        }

        if (!strict)
        {
          result = string.Format("{0}-{1}-{2}{3}", m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value, "");
        }

        return true;
      }
      else if (type.ToLower() == "email")
      {
        // Pattern from http://www.regular-expressions.info/email.html
        string pattern = @"^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$";

        return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
      }

      return true;
    }

    public UnitContactRow()
      : base()
    {
      this.Type = UnitContactRow.AllowedTypes[0];
    }

    public override string ToString()
    {
      return this.GetReportHtml();
    }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0}</b> {1} contact: {2}/{3}", this.Unit.DisplayName, this.Purpose, this.Type, this.Value);
    }

    //public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    //{
    //  if (string.IsNullOrEmpty(this.Type))
    //  {
    //    yield return new ValidationResult("Required", new[] { "Type" });
    //  }
    //  else if (!UnitContactRow.AllowedTypes.Contains(this.Type.ToLower()))
    //  {
    //    yield return new ValidationResult("Must be one of: " + string.Join(", ", UnitContactRow.AllowedTypes), new[] { "Type" });
    //  }

    //  if (string.IsNullOrEmpty(this.Value))
    //  {
    //    yield return new ValidationResult("Required", new[] { "Value" });
    //  }

    //  string dummy;
    //  if (!UnitContactRow.TryParse(this.Type, this.Value, true, out dummy))
    //  {
    //    yield return new ValidationResult(string.Format("'{0}' is not in valid form", this.Value), new[] { "Value" });
    //  }
    //}
  }
}
