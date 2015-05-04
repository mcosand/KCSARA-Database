/*
 * Copyright 2015 Matthew Cosand
 */

[assembly: Microsoft.Owin.OwinStartup(typeof(Kcsara.Database.Web.Startup))]

namespace Kcsara.Database.Web
{
  using Owin;

  public class Startup
  {
    public void Configuration(IAppBuilder app)
    {
      app.MapSignalR();
    }
  }
}
