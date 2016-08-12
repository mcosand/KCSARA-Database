/*
 * Copyright Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Sar.Auth.Data;

namespace Sar.Auth.Services
{
  public class SarClientStore : IClientStore
  {
    private readonly Func<IAuthDbContext> _dbFactory;

    public SarClientStore(Func<IAuthDbContext> dbFactory)
    {
      _dbFactory = dbFactory;
    }

    public async Task<Client> FindClientByIdAsync(string clientId)
    {
      using (var db = _dbFactory())
      {
        var row = await db.Clients.Where(f => f.ClientId == clientId).SingleOrDefaultAsync();
        if (row == null) return null;

        var client = new Client
        {
          ClientId = row.ClientId,
          ClientName = row.DisplayName,
          ClientSecrets = string.IsNullOrWhiteSpace(row.Secret) ? new List<Secret>() : new List<Secret> { new Secret(row.Secret.Sha256()) },
          Enabled = row.Enabled,
          IdentityTokenLifetime = 60 * 30, // 30 minutes
          Flow = row.Flow,
          RequireConsent = false,
          AllowRememberConsent = false,
          AccessTokenType = AccessTokenType.Jwt
        };

        switch (row.Flow)
        {
          case Flows.ClientCredentials:
            client.AllowedScopes = (row.AddedScopes ?? "").Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            client.Claims = row.Roles.Select(f => new Claim(Scopes.RolesClaim, f.Id)).ToList();
            client.PrefixClientClaims = false;
            return client;
          case Flows.Implicit:
          case Flows.Hybrid:
            client.RedirectUris = row.RedirectUris.Select(g => g.Uri).ToList();
            client.PostLogoutRedirectUris = new List<string>();
            client.AllowedScopes = new List<string> {
                Constants.StandardScopes.OpenId,
                Constants.StandardScopes.Profile,
                Constants.StandardScopes.Email,
                "kcsara-profile"
            }.Concat((row.AddedScopes ?? "").Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)).ToList();
            return client;
          case Flows.AuthorizationCode:
            client.RedirectUris = row.RedirectUris.Select(g => g.Uri).ToList();
            client.AllowedScopes = new List<string> {
                Constants.StandardScopes.OpenId,
                Constants.StandardScopes.Profile,
                Constants.StandardScopes.Email,
                "kcsara-profile"
            }.Concat((row.AddedScopes ?? "").Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)).ToList();
            return client;
          default:
            throw new NotSupportedException(row.Flow.ToString());
        }
      }
    }
  }
}