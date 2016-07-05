using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web;
using Kcsara.Database.Services;
using Microsoft.Owin;

namespace Kcsara.Database.Api
{
  public class OwinHost : IHost
  {
    public OwinHost()
    {
    }

    public string AccessToken
    {
      get
      {
        string header = HttpContext.Current.GetOwinContext().Request.Headers["Authorization"];
        if (string.IsNullOrWhiteSpace(header)) return null;
        string[] parts = header.Split(' ');
        if (parts.Length != 2 || parts[0] != "Bearer") return null;
        return parts[1];
      }
    }

    public ClaimsPrincipal User
    {
      get { return (ClaimsPrincipal)HttpContext.Current.GetOwinContext().Request.User; }
    }

    public string GetConfig(string key)
    {
      return ConfigurationManager.AppSettings[key];
    }
  }
}