using System.Configuration;
using System.Data.Entity;
using IdentityServer3.EntityFramework;
using Sar.Database.Data;

namespace Sar.Auth.Data
{
  public interface IAuthDbContext : IDbContext
  {
    IDbSet<AccountRow> Accounts { get; set; }
    IDbSet<ExternalLoginRow> ExternalLogins { get; set; }
    IDbSet<VerificationRow> Verifications { get; set; }

    IDbSet<ClientRow> Clients { get; set; }

    IDbSet<RoleRow> Roles { get; set; }

    IDbSet<LoginLogRow> Logins { get; set; }
  }

  public class AuthDbContext : OperationalDbContext, IAuthDbContext
  {
    public static void SetInitializer()
    {
      System.Data.Entity.Database.SetInitializer(new MigrateDatabaseToLatestVersion<AuthDbContext, Migrations.Configuration>());
    }

    public AuthDbContext() : base(ConfigurationManager.AppSettings["authStore"]) { }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      modelBuilder.HasDefaultSchema("authold");
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