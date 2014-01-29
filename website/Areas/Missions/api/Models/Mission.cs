/*
 * Copyright 2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.api.Models
{
  using System;
  using M = Kcsar.Database.Model;

  public class Mission
  {
    public string StateNumber { get; set; }
    public string Title { get; set; }
    public DateTime StartTime { get; set; }
    public Guid Id { get; set; }

    public static Mission FromDatabase(M.Mission m)
    {
      return new Mission
      {
        Id = m.Id,
        Title = m.Title,
        StateNumber = m.StateNumber,
        StartTime = m.StartTime
      };
    }
  }
}