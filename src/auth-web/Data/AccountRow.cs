using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sar.Auth.Data
{
  [Table("Accounts")]
  public class AccountRow
  {
    public AccountRow()
    {
      Id = Guid.NewGuid();
      Logins = new List<LoginLogRow>();
    }

    public Guid Id { get; set; }

    [MaxLength(100)]
    public string Username { get; set; }
    [MaxLength(500)]
    public string PasswordHash { get; set; }
    public DateTime? PasswordDate { get; set; }

    [MaxLength(100)]
    public string FirstName { get; set; }
    [MaxLength(100)]
    public string LastName { get; set; }

    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; }

    public Guid? MemberId { get; set; }

    [MaxLength(255)]
    public string LockReason { get; set; }
    public DateTime? Locked { get; set; }

    public virtual ICollection<RoleRow> Roles { get; set; }

    public virtual ICollection<LoginLogRow> Logins { get; set; }

    public DateTime? Created { get; set; }
    public DateTime? LastLogin { get; set; }
  }
}