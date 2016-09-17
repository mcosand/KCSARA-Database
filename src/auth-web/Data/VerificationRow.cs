using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sar.Auth.Data
{
  [Table("Verifications")]
  public class VerificationRow
  {
    [Key]
    [Column(Order = 1)]
    [MaxLength(50)]
    public string Provider { get; set; }

    [Key]
    [Column(Order = 2)]
    [MaxLength(255)]
    public string ProviderId { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; }

    [Required]
    [MaxLength(50)]
    public string Code { get; set; }
    
    public DateTime Created { get; set; }
  }
}