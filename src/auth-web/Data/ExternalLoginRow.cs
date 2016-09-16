using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sar.Auth.Data
{
  [Table("ExternalLogins")]
  public class ExternalLoginRow
  {
    [Key]
    [Column(Order = 1)]
    [MaxLength(50)]
    public string Provider { get; set; }

    [Key]
    [Column(Order = 2)]
    [MaxLength(255)]
    public string ProviderId { get; set; }

    public Guid AccountId { get; set; }

    [ForeignKey("AccountId")]
    public virtual AccountRow Account { get; set; }

    public DateTime? Created { get; set; }
    public DateTime? LastLogin { get; set; }
  }
}