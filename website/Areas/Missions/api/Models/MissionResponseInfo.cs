/*
 * Copyright 2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.api.Models
{
  using System;
  using System.Linq;
using M = Kcsar.Database.Model;

  public class MissionResponseInfo : MissionResponseStatus
  {
    public bool HasBriefing { get; set; }
    public MissionResponse MyResponse { get; set; }

    public MissionResponseInfo LoadData(M.Mission m, Guid? userId)
    {
      this.LoadData(m);
      this.HasBriefing = false;
      if (userId != null && userId != Guid.Empty)
      {
        var responder = m.Responders.SingleOrDefault(f => f.MemberId == userId && f.Hours != null);
        this.MyResponse = MissionResponse.FromDatabase(responder, false, true, false);
      }
      
      return this;
    }

    public new static MissionResponseInfo FromData(M.Mission m)
    {
      return new MissionResponseInfo().LoadData(m, Guid.Empty);
    }

    public new static MissionResponseInfo FromData(M.Mission m, Guid? userId)
    {
      return new MissionResponseInfo().LoadData(m, userId);
    }
  }
}