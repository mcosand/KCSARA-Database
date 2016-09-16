using System;
using System.Configuration;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Sar.Auth.Data
{
  public interface IAuthDbContext : IDisposable
  {
    IDbSet<AccountRow> Accounts { get; set; }
    IDbSet<ExternalLoginRow> ExternalLogins { get; set; }
    IDbSet<VerificationRow> Verifications { get; set; }

    IDbSet<ClientRow> Clients { get; set; }

    IDbSet<RoleRow> Roles { get; set; }

    IDbSet<LoginLogRow> Logins { get; set; }
    Task<int> SaveChangesAsync();
  }

  public class AuthDbContext : DbContext, IAuthDbContext
  {
    public AuthDbContext() : base(ConfigurationManager.AppSettings["authStore"]) { }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      modelBuilder.Entity<RoleRow>().HasMany(f => f.Clients).WithMany(f => f.Roles).Map(f => f.ToTable("ClientRoles"));
      modelBuilder.Entity<RoleRow>().HasMany(f => f.Accounts).WithMany(f => f.Roles).Map(f => f.ToTable("AccountRoles"));
      modelBuilder.Entity<RoleRow>().HasMany(f => f.Owners).WithMany().Map(f => f.ToTable("RoleOwners"));
      modelBuilder.Entity<RoleRow>().HasMany(f => f.Children).WithMany(f => f.Parents).Map(f => f.ToTable("RoleRoles"));
      base.OnModelCreating(modelBuilder);
    }

    public IDbSet<AccountRow> Accounts { get; set; }
    public IDbSet<ExternalLoginRow> ExternalLogins { get; set; }
    public IDbSet<VerificationRow> Verifications { get; set; }
    public IDbSet<ClientRow> Clients { get; set; }
    public IDbSet<LoginLogRow> Logins { get; set; }

    public IDbSet<RoleRow> Roles { get; set; }
  }
}