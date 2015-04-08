/*
 * Copyright 2013-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data
{
  using System.ComponentModel.DataAnnotations.Schema;

  [Table("MemberMedicals")]
  public class MemberMedicalRow : ModelObjectRow
  {
    // This is an optional extension to a member. This row will share the key with the MemberRow
    public virtual MemberRow Member { get; set; }

    public string EncryptedAllergies { get; set; }
    public string EncryptedMedications { get; set; }
    public string EncryptedDisclosures { get; set; }

    public override string GetReportHtml()
    {
      return "Medical information for <b>" + this.Member.FullName + "</b>";
    }

  }
}
