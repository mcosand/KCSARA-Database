using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sar.Auth.Data
{
  [Table("AccountResetTokens")]
  public class AccountResetTokenRow
  {
    public AccountResetTokenRow()
    {
      CreatedUtc = DateTime.UtcNow;
    }

    public DateTime CreatedUtc { get; set; }

    [Key]
    [Column(Order = 0)]
    public Guid AccountId { get; set; }

    [ForeignKey("AccountId")]
    public AccountRow Account { get; set; }

    [Key]
    [Column(Order = 1)]
    [Required]
    [MaxLength(256)]
    public string Token { get; set; }
  }
}