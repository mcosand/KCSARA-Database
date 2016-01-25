/*
 * Copyright 2013-2016 Matthew Cosand
 */
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kcsar.Database.Model
{
  [Table("MemberMedicals")]
  public class MemberMedicalRow : ModelObject
  {
    [ForeignKey("Id")]
    public virtual MemberRow Member { get; set; }
    public string EncryptedAllergies { get; set; }
    public string EncryptedMedications { get; set; }
    public string EncryptedDisclosures { get; set; }

    public EncryptionType EncryptionType { get; set; }

    public override string GetReportHtml()
    {
      return "Medical information for <b>" + this.Member.FullName + "</b>";
    }

  }

  public enum EncryptionType
  {
    Unknown = 0,
    MachineKey = 1,
    ManagedAES = 2,
  }
}
