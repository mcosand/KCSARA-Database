using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Kcsara.Database.Model;
using Kcsara.Database.Model.Members;
using Kcsara.Database.Model.Units;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sar.Services;

namespace Sar.Auth.Services
{
  public class ApiMemberInfoService : IMemberInfoService
  {
    private readonly IHost _host;

    public ApiMemberInfoService(IHost host)
    {
      _host = host;
    }

    public async Task<Dictionary<string, bool>> GetStatusToAccountMap()
    {
      string token = await GetApiToken();

      HttpClient client = new HttpClient();
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetApiToken());
      string typesJson = await client.GetStringAsync(_host.GetConfig("db-api:root") + "/units/statustypes");

      var types = JsonConvert.DeserializeObject<UnitStatusType[]>(typesJson);
      return types.ToDictionary(f => (f.Unit.Id.ToString() + f.Name).ToLowerInvariant(), f => f.GetsAccount);
    }

    public async Task<IList<MemberInfo>> FindMembersByEmail(string email)
    {
      string token = await GetApiToken();

      HttpClient client = new HttpClient();
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetApiToken());
      string membersJson = await client.GetStringAsync(_host.GetConfig("db-api:root") + "/members/byemail/" + HttpUtility.UrlEncode(email));

      var members = JsonConvert.DeserializeObject<MemberSummary[]>(membersJson);

      var infoTasks = members.Select(f => client.GetStringAsync(_host.GetConfig("db-api:root") + "/members/" + f.Id));
      var infoJsons = await Task.WhenAll(infoTasks);
      
      return infoJsons.Select(f => JsonConvert.DeserializeObject<MemberInfo>(f)).ToList();
    }

    public async Task<Member> GetMember(Guid memberId)
    {
      HttpClient client = new HttpClient();
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetApiToken());
      string memberJson = await client.GetStringAsync(_host.GetConfig("db-api:root") + "/members/" + memberId);

      
      var info = JsonConvert.DeserializeObject<MemberInfo>(memberJson);

      var unitsJson = await client.GetStringAsync(_host.GetConfig("db-api:root") + "/units/");
      var units = JsonConvert.DeserializeObject<Unit[]>(unitsJson).ToDictionary(f => f.Id, f => new Organization
      {
        Id = f.Id,
        Name = f.Name,
        LongName = f.FullName
      });

      var contactsJson = await client.GetStringAsync(_host.GetConfig("db-api:root") + "/members/" + memberId + "/contacts");
      var email = JsonConvert.DeserializeObject<PersonContact[]>(contactsJson).Where(f => f.Type == "email").FirstOrDefault();

      return new Member
      {
        Units = info.Units.Select(f => new OrganizationMembership { Org = units[f.Unit.Id], Status = f.Status }),
        Id = info.Id,
        FirstName = info.First,
        LastName = info.Last,
        Email = email?.Value,
        PhotoUrl = info.Photo,
        ProfileUrl = string.Format(_host.GetConfig("memberProfileTemplate"), info.Id)
      };
    }

    private async Task<string> GetApiToken()
    {
      HttpClient client = new HttpClient();

      HttpResponseMessage tokenResponse = await client.PostAsync(_host.GetConfig("auth:authority") + "/connect/token", new FormUrlEncodedContent(new List<KeyValuePair<string, string>> {
        new KeyValuePair<string, string>("grant_type", "client_credentials"),
        new KeyValuePair<string, string>("scope", "db-r-members"),
        new KeyValuePair<string, string>("client_id", _host.GetConfig("auth:clientId")),
        new KeyValuePair<string, string>("client_secret", _host.GetConfig("auth:secret"))
      }));
      string tokenBody = await tokenResponse.Content.ReadAsStringAsync();
      var parsed = JsonConvert.DeserializeObject<JObject>(tokenBody);
      var token = parsed["access_token"].Value<string>();
      return token;
    }
  }
}