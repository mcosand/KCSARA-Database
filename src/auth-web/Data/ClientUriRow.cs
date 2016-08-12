/*
 * Copyright Matthew Cosand
 */
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sar.Auth.Data
{
  [Table("ClientUris")]
  public class ClientUriRow
  {
    public int Id { get; set; }

    [ForeignKey("ClientId")]
    public virtual ClientRow Client { get; set; }
    public int ClientId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Uri { get; set; }
  }
}