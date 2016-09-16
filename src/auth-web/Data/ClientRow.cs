using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IdentityServer3.Core.Models;

namespace Sar.Auth.Data
{
  [Table("Clients")]
  public class ClientRow
  {
    public int Id { get; set; }

    [Required]
    [Index(IsUnique = true)]
    [MaxLength(50)]
    public string ClientId { get; set; }

    [Required]
    [MaxLength(80)]
    public string DisplayName { get; set; }

    public bool Enabled { get; set; }

    public virtual ICollection<ClientUriRow> RedirectUris { get; set; }

    public virtual ICollection<ClientLogoutUriRow> LogoutUris { get; set; }

    [MaxLength(255)]
    public string AddedScopes { get; set; }

    public string Secret { get; set; }

    public virtual ICollection<RoleRow> Roles { get; set; }

    public Flows Flow { get; set; }
  }
}