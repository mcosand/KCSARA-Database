using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sar.Auth.Data
{
  [Table("Roles")]
  public class RoleRow
  {
    [MaxLength(25)]
    public string Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; }

    public string Description { get; set; }

    public virtual ICollection<ClientRow> Clients { get; set; }
    public virtual ICollection<AccountRow> Accounts { get; set; }
    public virtual ICollection<RoleRow> Children { get; set; }
    public virtual ICollection<RoleRow> Parents { get; set; }

    public virtual ICollection<AccountRow> Owners { get; set; }
  }
}