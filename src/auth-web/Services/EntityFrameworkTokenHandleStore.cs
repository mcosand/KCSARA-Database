using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.EntityFramework.Serialization;
using Newtonsoft.Json;
using Sar.Auth.Data;

namespace Sar.Database.Web.Auth.Services
{
  internal class EntityFrameworkTokenHandleStore : ITokenHandleStore
  {
    private readonly Func<IAuthDbContext> _dbFactory;
    private readonly IClientStore _clients;
    private readonly IScopeStore _scopes;

    private MemoryCache _cache = new MemoryCache("tokenCache");

    public EntityFrameworkTokenHandleStore(Func<IAuthDbContext> dbFactory, IClientStore clients, IScopeStore scopes)
    {
      _dbFactory = dbFactory;
      _clients = clients;
      _scopes = scopes;
    }

    public Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject)
    {
      throw new NotImplementedException();
    }

    public async Task<Token> GetAsync(string key)
    {
      var token = _cache.Get(key) as Token;
      if (token == null)
      {
        using (var db = _dbFactory())
        {
          var json = await db.Tokens.Where(f => f.Key == key).Select(f => f.JsonCode).SingleOrDefaultAsync();
          if (json != null)
          {
            token = ConvertFromJson(json);
          }
        }
      }
      return token;
    }

    public async Task RemoveAsync(string key)
    {
      _cache.Remove(key);
      using (var db = _dbFactory())
      {
        var now = DateTimeOffset.UtcNow;
        foreach (var token in await db.Tokens.Where(f => f.Key == key).ToListAsync())
        {
          db.Tokens.Remove(token);
        }
        await db.SaveChangesAsync();
      }
    }

    public Task RevokeAsync(string subject, string client)
    {
      throw new NotImplementedException();
    }

    public async Task StoreAsync(string key, Token value)
    {
      _cache.Add(key, value, new CacheItemPolicy { AbsoluteExpiration = value.CreationTime.AddSeconds(value.Lifetime) });
      var efToken = new TokenRow
      {
        Key = key,
        SubjectId = value.SubjectId,
        ClientId = value.ClientId,
        JsonCode = ConvertToJson(value),
        Expiry = DateTimeOffset.UtcNow.AddSeconds(value.Lifetime),
        TokenType = TokenType.TokenHandle
      };
      using (var db = _dbFactory())
      {
        db.Tokens.Add(efToken);
        await db.SaveChangesAsync();
        (db as DbContext)?.Database?.ExecuteSqlCommand("DELETE FROM auth.Tokens WHERE Expiry < @expiry", new SqlParameter("@expiry", DateTimeOffset.UtcNow));
      }
    }

    JsonSerializerSettings GetJsonSerializerSettings()
    {
      var settings = new JsonSerializerSettings();
      settings.Converters.Add(new ClaimConverter());
      settings.Converters.Add(new ClaimsPrincipalConverter());
      settings.Converters.Add(new ClientConverter(_clients));
      settings.Converters.Add(new ScopeConverter(_scopes));
      return settings;
    }

    protected string ConvertToJson(Token value)
    {
      return JsonConvert.SerializeObject(value, GetJsonSerializerSettings());
    }

    protected Token ConvertFromJson(string json)
    {
      return JsonConvert.DeserializeObject<Token>(json, GetJsonSerializerSettings());
    }

  }
}