using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sar.Auth.Data
{
  [Table("Tokens")]
  public class TokenRow
  {
    [Key, Column(Order = 0)]
    public virtual string Key { get; set; }

    [Key, Column(Order = 1)]
    public virtual TokenType TokenType { get; set; }

    [StringLength(200)]
    public virtual string SubjectId { get; set; }

    [Required]
    [StringLength(200)]
    public virtual string ClientId { get; set; }

    [Required]
    [DataType(DataType.Text)]
    public virtual string JsonCode { get; set; }

    [Required]
    public virtual DateTimeOffset Expiry { get; set; }
  }

  public enum TokenType : short
  {
    AuthorizationCode = 1,
    TokenHandle = 2,
    RefreshToken = 3
  }
}