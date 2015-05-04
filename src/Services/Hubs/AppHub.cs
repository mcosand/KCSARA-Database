/*
 * Copyright 2015 Matthew Cosand
 */

namespace Kcsara.Database.Services.Hubs
{
  using System;
  using Microsoft.AspNet.SignalR;

  public class AppHub : Hub
  {
    // right now the services can call dynamic methods
    // ex: GlobalHost.ConnectionManager.GetHubContext<AppHub>().Clients.All.eventUpdated(row.Id);
  }
}
