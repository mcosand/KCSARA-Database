using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Kcsar.Database.Model;
using Microsoft.AspNet.Authentication;

namespace Kcsara.Database.Web
{
  public class MemberIdClaimsTransformer : IClaimsTransformer
  {
    private readonly Func<IKcsarContext> dbFactory;

    public static readonly string Type = "DatabaseId";

    public MemberIdClaimsTransformer(Func<IKcsarContext> dbFactory)
    {
      this.dbFactory = dbFactory;
    }

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
      if (principal.Identity.IsAuthenticated)
      {
        using (var db = dbFactory())
        {
          var member = db.Members.FirstOrDefault(f => f.Username == principal.Identity.Name);
          if (member != null)
          {
            (principal.Identity as ClaimsIdentity).AddClaim(new Claim(Type, member.Id.ToString()));
          }
        }
      }
      return Task.FromResult(principal);
    }
  }
}
