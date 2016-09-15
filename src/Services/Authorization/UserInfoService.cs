using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Kcsara.Database.Web.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sar;
using Sar.WebApi;

namespace Kcsara.Database.Web.Services
{
  public interface IUserInfoService
  {
    Task<UserInfo> GetCurrentUserInfo();
  }

  public class UserInfoService : IUserInfoService
  {
    private readonly IWebApiHost _host;
    private string _userInfoEndpoint = null;
    private object _discoveryLock = new object();

    private readonly ObjectCache _cache = new MemoryCache("userinfo");
    private readonly CacheItemPolicy _cachePolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(10) };

    public UserInfoService(IWebApiHost host)
    {
      _host = host;
    }

    public async Task<UserInfo> GetCurrentUserInfo()
    {
      var subject = _host.User.FindFirst("sub").Value;
      var info = _cache[subject] as UserInfo;
      if (info == null)
      {
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _host.RequestToken);

        string json = await client.GetStringAsync(await GetUserInfoEndpoint());

        info = new UserInfo();
        var parsed = JsonConvert.DeserializeObject<JObject>(json);

        foreach (var item in parsed.Properties())
        {
          if (item.Path == "memberId") info.MemberId = new Guid(item.Values<string>().First());
          else if (item.Path == "role")
            info.Roles.AddRange(
              item.Value.Type == JTokenType.Array
                ? ((JArray)item.Value).Select(f => f.Value<string>())
                : new[] { item.Values<string>().First() }
              );
          else if (item.Path == "units")
            info.Units.AddRange(
              item.Value.Type == JTokenType.Array
                ? ((JArray)item.Value).Select(f => JsonConvert.DeserializeObject<NameIdPair>(f.Value<string>()))
                : new[] { JsonConvert.DeserializeObject<NameIdPair>(item.Values<string>().First()) }
                );
        }
        _cache.Add(subject, info, _cachePolicy);
      }

      return info;
    }

    private async Task<string> GetUserInfoEndpoint()
    {
      if (_userInfoEndpoint == null)
      {
        HttpClient client = new HttpClient();
        string info = await client.GetStringAsync(_host.GetConfig("auth:authority").TrimEnd('/') + "/.well-known/openid-configuration");
        var parsed = JsonConvert.DeserializeObject<JObject>(info);

        lock (_discoveryLock)
        {
          if (_userInfoEndpoint == null)
          {
            _userInfoEndpoint = parsed["userinfo_endpoint"].Value<string>();
          }
        }
      }
      return _userInfoEndpoint;
    }
  }
}
