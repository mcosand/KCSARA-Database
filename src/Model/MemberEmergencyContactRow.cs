/*
 * Copyright 2013-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;

  [Table("MemberEmergencyContacts")]
  public class MemberEmergencyContactRow : ModelObjectRow
  {
    [ForeignKey("MemberId")]
    public virtual MemberRow Member { get; set; }
    public Guid MemberId { get; set; }

    public string EncryptedData { get; set; }

    public override string GetReportHtml()
    {
      return "Emergency Contact information for <b>" + this.Member.FullName + "</b>";
    }
  }
}
