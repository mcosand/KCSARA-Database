namespace Sar.Auth.Migrations
{
  using System;
  using System.Collections.Generic;
  using System.Data.Entity.Migrations;
  using System.Linq;
  using Data;
  using Database.Web.Auth.Services;
  using IdentityServer3.Core.Models;

  internal sealed class Configuration : DbMigrationsConfiguration<Sar.Auth.Data.AuthDbContext>
  {
    public Configuration()
    {
      AutomaticMigrationsEnabled = false;
    }

    protected override void Seed(Sar.Auth.Data.AuthDbContext context)
    {
      if (context.Clients.Count() == 0)
      {
        context.Clients.Add(new ClientRow
        {
          DisplayName = "KCSARA Database",
          ClientId = "databaseweb",
          Secret = "get_your_own",
          Enabled = true,
          Flow = Flows.Hybrid,
          AddedScopes = "database-api database",
          RedirectUris = new List<ClientUriRow>
          {
            new ClientUriRow { Uri = "http://localhost:4944/" }
          },
          LogoutUris = new List<ClientLogoutUriRow>
          {
            new ClientLogoutUriRow { Uri = "http://localhost:4944/" }
          }
        });

        context.Clients.Add(new ClientRow
        {
          DisplayName = "KCSARA Database",
          ClientId = "spaweb",
          Enabled = true,
          Flow = Flows.Implicit,
          AddedScopes = "database-api database",
          RedirectUris = new List<ClientUriRow>
          {
            new ClientUriRow { Uri = "http://localhost:4944/loggedIn" },
            new ClientUriRow { Uri = "http://localhost:4944/loginSilent" }
          },
          LogoutUris = new List<ClientLogoutUriRow>
          {
            new ClientLogoutUriRow { Uri = "http://localhost:4944/" }
          }
        });

        context.Clients.Add(new ClientRow
        {
          DisplayName = "Authentication Site",
          ClientId = "authweb",
          Secret = "get_your_own",
          Enabled = true,
          Flow = Flows.ClientCredentials,
          AddedScopes = "db-r-members"
        });

        var admin = new AccountRow
        {
          Email = "test@example.local",
          FirstName = "Admin",
          LastName = "User",
          Created = DateTime.UtcNow,
          PasswordDate = DateTime.UtcNow,
          PasswordHash = SarUserService.GetSaltedPassword("password"),
          Username = "admin"
        };
        context.Accounts.Add(admin);

        context.Roles.Add(new RoleRow { Id = "cdb.admins", Name = "cdb.admins", Accounts = new List<AccountRow> { admin } });
        context.Roles.Add(new RoleRow { Id = "cdb.users", Name = "cdb.users", Accounts = new List<AccountRow> { admin } });

        context.SaveChanges();

      }
    }
  }
}
