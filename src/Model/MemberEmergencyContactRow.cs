/*
 * Copyright 2013-2016 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kcsar.Database.Model
{
  [Table("MemberEmergencyContacts")]
  public class MemberEmergencyContactRow : ModelObject
  {
    public Guid MemberId { get; set; }

    [ForeignKey("MemberId")]
    public virtual MemberRow Member { get; set; }
    public string EncryptedData { get; set; }

    public EncryptionType EncryptionType { get; set; }

    public override string GetReportHtml()
    {
      return "Emergency Contact information for <b>" + this.Member.FullName + "</b>";
    }
  }
}
