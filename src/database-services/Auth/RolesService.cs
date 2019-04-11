using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Caching;
using IdentityModel.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sar.Database.Services.Auth
{
  public class RolesService : IRolesService
  {
    private readonly string authority;
    private readonly string clientId;
    private readonly string clientSecret;
    private readonly string url;
    private readonly string scope;

    private MemoryCache cache = new MemoryCache("rolesService");
    public RolesService(IHost host)
    {
      authority = host.GetConfig("auth:authority").Trim('/');
      clientId = host.GetConfig("api:service_client");
      clientSecret = host.GetConfig("api:service_secret");
      url = host.GetConfig("api:roles:url");
      scope = host.GetConfig("api:roles:scope");
    }

    public List<string> ListAllRolesForAccount(Guid accountId)
    {
      lock(cache)
      {
        var cached = (List<string>)cache.Get(accountId.ToString());
        if (cached != null) return cached;
      }

      TokenClient client = new TokenClient(authority + "/connect/token", clientId, clientSecret, AuthenticationStyle.PostValues);
      var token = client.RequestClientCredentialsAsync(scope).Result;

      var http = new HttpClient();
      http.SetBearerToken(token.AccessToken);
      var json = http.GetStringAsync(authority + $"/Account/{accountId}/Groups").Result;
      var payload = JsonConvert.DeserializeObject<JObject>(json);

      var list = payload["data"].ToObject<List<string>>();
      lock(cache)
      {
        cache.Set(accountId.ToString(), list, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5) });
      }
      return list;
    }
  }
}
