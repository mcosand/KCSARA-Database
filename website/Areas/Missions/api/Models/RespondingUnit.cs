/*
 * Copyright 2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.api.Models
{
  using System;
  using M = Kcsar.Database.Model;

  public class RespondingUnit
  {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string LongName { get; set; }
    public Guid? UnitId { get; set; }
    public bool AmMember { get; set; }


    public static RespondingUnit FromDatabase(M.MissionRespondingUnit u)
    {
      return new RespondingUnit
        {
          Id = u.Id,
          Name = u.Name ?? u.Unit.DisplayName,
          LongName = u.LongName ?? u.Unit.LongName,
          UnitId = u.UnitId,
          AmMember = false
        };
    }
  }
}