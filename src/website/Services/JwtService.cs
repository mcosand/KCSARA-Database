using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kcsara.Database.Web.Services
{
  public static class JwtService
  {
    static Dictionary<string, X509Certificate2> Certificates = null;
    static object certLock = new object();

    static async Task<Dictionary<string, X509Certificate2>> GetCertificates()
    {
      if (Certificates == null)
      {
        var tempList = await FetchCertificates();
        lock (certLock)
        {
          if (Certificates == null)
          {
            Certificates = tempList;
          }
        }
      }
      return Certificates;
    }

    static async Task<Dictionary<string, X509Certificate2>> FetchCertificates()
    {
      using (var http = new HttpClient())
      {
        var openIdConnectConfig = await http.GetStringAsync(ConfigurationManager.AppSettings["auth:authority"] + "/.well-known/openid-configuration");
        var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(openIdConnectConfig);

        var json = await http.GetStringAsync(Convert.ToString(dictionary["jwks_uri"]));

        dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        JArray jsonKeys = (JArray)dictionary["keys"];

        var result = jsonKeys.ToDictionary(
          f => f["kid"].ToString(),
          f =>
          {
            var n = f["x5c"][0].ToString();
            var x = new X509Certificate2(Base64UrlEncoder.DecodeBytes(n));
            return x;
          });

        return result;
      }
    }

    public static async Task<JwtSecurityToken> ValidateToken(string idToken)
    {
      var token = new JwtSecurityToken(idToken);
      var jwtHandler = new JwtSecurityTokenHandler();

      var certificates = await GetCertificates();
      // Set up token validation
      var tokenValidationParameters = new TokenValidationParameters
      {
        ValidateAudience = false,
        ValidIssuer = ConfigurationManager.AppSettings["auth:authority"],
        IssuerSigningTokens = certificates.Values.Select(x => new X509SecurityToken(x)),
        IssuerSigningKeys = certificates.Values.Select(x => new X509SecurityKey(x)),
        IssuerSigningKeyResolver = (s, securityToken, identifier, parameters) =>
        {
          return identifier.OfType<NamedKeySecurityKeyIdentifierClause>().Select(x =>
          {
            if (!certificates.ContainsKey(x.Id))
              return null;

            return new X509SecurityKey(certificates[x.Id]);
          }).First(x => x != null);
        }
      };

      SecurityToken jwt;
      var claimsPrincipal = jwtHandler.ValidateToken(idToken, tokenValidationParameters, out jwt);
      return (JwtSecurityToken)jwt;
    }
  }
}