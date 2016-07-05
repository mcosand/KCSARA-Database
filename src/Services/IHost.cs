using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Kcsara.Database.Services
{
  public interface IHost
  {
    ClaimsPrincipal User { get; }
    string AccessToken { get; }
    string GetConfig(string key);
  }
}
