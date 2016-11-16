using System;
using System.Configuration;
using System.IO;
using System.Security.Claims;
using System.Web;
using Sar.WebApi;

namespace Sar.Database.Web
{
  public class SystemWebHost : IWebApiHost
  {
    public string RequestToken
    {
      get
      {
        string header = HttpContext.Current.Request.Headers["Authorization"];
        if (string.IsNullOrWhiteSpace(header)) return null;
        string[] parts = header.Split(' ');
        if (parts.Length != 2 || parts[0] != "Bearer") return null;
        return parts[1];
      }
    }

    public ClaimsPrincipal User
    {
      get { return (ClaimsPrincipal)HttpContext.Current.User; }
    }

    public bool FileExists(string relativePath)
    {
      var basePath = AppDomain.CurrentDomain.BaseDirectory;
      var path = Path.Combine(basePath, relativePath);
      if (!path.StartsWith(basePath, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("can't navigate to parent directory");
      return File.Exists(path);
    }

    public string GetConfig(string key)
    {
      return ConfigurationManager.AppSettings[key];
    }

    public Stream OpenFile(string relativePath)
    {
      var basePath = AppDomain.CurrentDomain.BaseDirectory;
      var path = Path.Combine(basePath, relativePath);
      if (!path.StartsWith(basePath, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("can't navigate to parent directory");

      return File.OpenRead(path);
    }
  }
}
