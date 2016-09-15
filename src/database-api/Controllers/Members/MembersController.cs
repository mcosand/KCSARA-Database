using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sar;
using Sar.Database;
using Sar.Database.Model.Members;
using Sar.Database.Services;

namespace Kcsara.Database.Api.Controllers
{
  public class MembersController : ApiController
  {
    private readonly IMembersService _members;
    private readonly IHost _host;
    private readonly IAuthorizationService _authz;

    public MembersController(IMembersService members, IAuthorizationService authz, IHost host)
    {
      _members = members;
      _authz = authz;
      _host = host;
    }

    [HttpGet]
    [Route("members/{id}")]
    [AnyHostCorsPolicy]
    public async Task<MemberInfo> Get(Guid id)
    {
      if (!await _authz.AuthorizeAsync(User as ClaimsPrincipal, id, "Read:Member")) throw new AuthorizationException();

      return await _members.GetMember(id);
    }

    [HttpGet]
    [Route("members/{memberId}/emergencycontacts/count")]
    [AnyHostCorsPolicy]
    public async Task<object> EmergencyContactStatus(Guid memberId)
    {
      if (!await _authz.AuthorizeAsync(User as ClaimsPrincipal, memberId, "Read:Member")) throw new AuthorizationException();

      return new { Count = await _members.GetEmergencyContactCountAsync(memberId) };
    }

    [HttpPost]
    [Route("members")]
    public async Task<MemberInfo> Provision(ProvisionMemberInfo body)
    {
      if (!((ClaimsPrincipal)User).Claims.Any(f => f.Type == "scope" && f.Value == "db-w-members"))
      {
        throw new HttpResponseException(HttpStatusCode.Forbidden);
      }

      // Create the member
      var member = await _members.CreateMember(body);

      return await Get(member.Id);
    }

    private async Task<string> AuthenticatedPost(string endpoint, string scope, object postBody)
    {
      // Create an account for the member
      var client = new HttpClient();
      var httpArgs = new Dictionary<string, string>
        {
          { "grant_type", "client_credentials" },
          { "client_id", _host.GetConfig("auth:clientId") },
          { "client_secret", _host.GetConfig("auth:secret") },
          { "scope", scope }
        };
      var httpBody = new FormUrlEncodedContent(httpArgs);
      var response = await client.PostAsync(_host.GetConfig("auth:authority").TrimEnd('/') + "/connect/token", httpBody);
      var responseJson = await response.Content.ReadAsStringAsync();
      if (!response.IsSuccessStatusCode)
      {
        throw new HttpResponseException(HttpStatusCode.InternalServerError);
      }
      var tokenSet = JsonConvert.DeserializeObject<JObject>(responseJson);
      var token = tokenSet["access_token"].Value<string>();

      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
      response = await client.PostAsJsonAsync(endpoint, postBody);
      responseJson = await response.Content.ReadAsStringAsync();
      if (!response.IsSuccessStatusCode)
      {
        throw new HttpResponseException(HttpStatusCode.InternalServerError);
      }

      return responseJson;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("Members/ByWorkerNumber/{id}")]
    public async Task<IEnumerable<MemberSummary>> ByWorkerNumber(string id)
    {
      if (!await _authz.AuthorizeAsync(User as ClaimsPrincipal, null, "Read:Member")) throw new AuthorizationException();

      return await _members.ByWorkerNumber(id);
    }

    [HttpGet]
    [Route("Members/ByPhoneNumber/{id}")]
    public async Task<IEnumerable<MemberSummary>> ByPhoneNumber(string id)
    {
      if (!await _authz.AuthorizeAsync(User as ClaimsPrincipal, null, "Read:Member")) throw new AuthorizationException();

      return await _members.ByPhoneNumber(id);
    }

    [HttpGet]
    [Route("Members/ByEmail/{id}")]
    public async Task<IEnumerable<MemberSummary>> ByEmail(string id)
    {
      if (!await _authz.AuthorizeAsync(User as ClaimsPrincipal, null, "Read:Member")) throw new AuthorizationException();

      return await _members.ByEmail(id);
    }
  }
}