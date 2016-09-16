using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sar.Auth.Data
{
  [Table("LoginLog")]
  public class LoginLogRow
  {
    public long Id { get; set; }

    public DateTime Time { get; set; }

    public string ProviderId { get; set; }

    [ForeignKey("AccountId")]
    public virtual AccountRow Account { get; set; }
    public Guid AccountId { get; set; }
  }
}