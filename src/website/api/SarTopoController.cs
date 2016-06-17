namespace Kcsara.Database.Web.api
{
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using System.IdentityModel.Tokens;
  using System.Linq;
  using System.Net.Http;
  using System.Net.Http.Headers;
  using System.Text;
  using System.Text.RegularExpressions;
  using System.Threading.Tasks;
  using System.Web.Http;
  using IdentityModel.Client;
  using Kcsar.Database.Model;
  using log4net;
  using Models.SarTopo;
  using Newtonsoft.Json;
  using Services;

  [ModelValidationFilter]
  [CamelCaseControllerConfig]
  public class SarTopoController : BaseApiController
  {
    private readonly string sarTopoUrl;
    private readonly string sarTopoProvider;

    private static string tokenForSarTopo = null;
    private static DateTime tokenExpiry = DateTime.MinValue;
    private static object tokenLock = new object();

    public SarTopoController(IKcsarContext db, ILog log)
      : base(db, log)
    {
      sarTopoUrl = (ConfigurationManager.AppSettings["sartopo:root"] ?? string.Empty).TrimEnd('/');
      sarTopoProvider = ConfigurationManager.AppSettings["sartopo:provider"];

      if (string.IsNullOrWhiteSpace(sarTopoUrl) || string.IsNullOrWhiteSpace(sarTopoProvider))
      {
        throw new InvalidOperationException(string.Format("Invalid configuration. sartopo:root={0}, sartopo:provider={1}", sarTopoUrl, sarTopoProvider));
      }
    }

    /// <summary>Callback so that SARTopo can a) get a username for the API connection and b) check to see if the user has permissions for API access.</summary>
    /// <returns></returns>
    [HttpGet]
    [Route("api/sartopo/checkAccess")]
    [AllowAnonymous]
    public async Task<ApiAccessInfo> GetAccessInfo()
    {
      if (Request.Headers.Authorization == null || Request.Headers.Authorization.Scheme != "Bearer")
      {
        return new ApiAccessInfo { Allowed = false };
      }

      string id = null;

      try
      {
        var token = Request.Headers.Authorization.Parameter;
        JwtSecurityToken parsed = await JwtService.ValidateToken(token);

        // Right now we'll only access the API using a machine-to-machine access token. These tokens don't have 'sub'
        // claims. We'll use the client_id as the SARTopo account name.
        id = parsed.Claims.Where(f => f.Type == "client_id").Select(f => f.Value).Single();
      }
      catch (Exception ex)
      {
        log.Warn("Unhandled exception checking access for SarTopo API", ex);
      }

      return new ApiAccessInfo
      {
        Id = id,
        Allowed = id != null
      };
    }

    [HttpGet]
    [Authorize(Roles = "cdb.missioneditors")]
    [Route("api/sartopo/createmap/{name}")]
    public async Task<MapSummary> CreateMap(string name)
    {
      HttpClient client = new HttpClient();
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetSarTopoToken());

      var content = new FormUrlEncodedContent(new[]
      {
          new KeyValuePair<string, string>("name", name),
          new KeyValuePair<string, string>("comments", string.Empty),
          new KeyValuePair<string, string>("lat", Convert.ToString(47.5)),
          new KeyValuePair<string, string>("lng", Convert.ToString(-122.1)),
          new KeyValuePair<string, string>("permissions", "{\"sharing\":\"PRIVATE\",\"password\":\"\"}")
      });


      var response = await client.PostAsync(sarTopoUrl + "/rest/map/", content);
      var bytes = await response.Content.ReadAsByteArrayAsync();
      string body = Encoding.UTF8.GetString(bytes);
      var map = JsonConvert.DeserializeObject<MapSummary>(body);

      return map;
    }

    [HttpGet]
    [Authorize(Roles = "cdb.missioneditors")]
    [Route("api/sartopo/map/{name}/{timestamp:long=1}")]
    public async Task<string> GetMapData(string name, long timestamp)
    {
      if (Regex.IsMatch(name, "^[a-zA-Z0-9]{1,8}$") == false) throw new UnauthorizedAccessException("Invalid map name");

      HttpClient client = new HttpClient();
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetSarTopoToken());

      var setTenant = await client.GetStringAsync(sarTopoUrl + "/rest/map/" + name);
      var response = await client.GetStringAsync(sarTopoUrl + "/rest/since/" + Convert.ToString(timestamp));

      return setTenant + "\n\n" + response;
    }

    private async Task<string> GetSarTopoToken()
    {
      //if (DateTime.Now > tokenExpiry)
      //{
      var tokenClient = new TokenClient(
        ConfigurationManager.AppSettings["auth:authority"] + "/connect/token",
        ConfigurationManager.AppSettings["sartopo:clientId"],
        ConfigurationManager.AppSettings["sartopo:secret"]);
      var tokenResponse = await tokenClient.RequestClientCredentialsAsync("sartopo-api");

      if (!string.IsNullOrWhiteSpace(tokenResponse.Error))
      {
        throw new InvalidOperationException(tokenResponse.Error);
      }

      // Store the token issued by our local authority
      var token = tokenResponse.AccessToken;

      // Use the locally issued token to get a SarTopo issued token.
      HttpClient client = new HttpClient();
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
      var response = await client.GetStringAsync(sarTopoUrl + "/app/openidtradetoken/" + sarTopoProvider);
      var json = JsonConvert.DeserializeObject<TradeTokenResponse>(response);

      return json.Token;
      //lock (tokenLock)
      //{
      //  if (DateTime.Now > tokenExpiry)
      //  {
      //    tokenForSarTopo = json.Token;
      //    tokenExpiry = DateTime.Now.AddMinutes(5);
      //  }
      //}
      //  }
      //  return tokenForSarTopo;
    }
  }
}
