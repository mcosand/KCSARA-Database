namespace Kcsara.Database.Web.Missions.Hubs
{
  using System;
  using Microsoft.AspNet.SignalR;
  using Microsoft.AspNet.SignalR.Hubs;

  public class ResponseHub : Hub
  {
  }

  public class ResponseHubClient
  {
    private IHubConnectionContext clients = GlobalHost.ConnectionManager.GetHubContext<ResponseHub>().Clients;
    
    public void BroadcastRespondersUpdate(Guid missionId)
    {
      this.clients.All.notifyRespondersUpdate(missionId);
    }
  }
}