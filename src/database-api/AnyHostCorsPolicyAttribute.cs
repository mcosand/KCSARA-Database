using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http.Cors;

namespace Kcsara.Database.Api
{
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
  public class AnyHostCorsPolicyAttribute : Attribute, ICorsPolicyProvider
  {
    public Task<CorsPolicy> GetCorsPolicyAsync(HttpRequestMessage request, CancellationToken cancel)
    {
      var policy = new CorsPolicy
      {
        AllowAnyMethod = true,
        AllowAnyHeader = true,
        SupportsCredentials = true,
        PreflightMaxAge = 60 * 60 * 24 // 1 day
      };

      var origin = request.Headers.GetValues("Origin").FirstOrDefault();
      if (origin != null)
      {
        policy.Origins.Add(origin);
      }

      return Task.FromResult(policy);
    }
  }
}