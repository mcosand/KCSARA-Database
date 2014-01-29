/*
 * Copyright 2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.api.Models
{
  using M = Kcsar.Database.Model;

  public class Responder : Person
  {
    public string WorkerNumber { get; set; }
    public bool IsVisitor { get; set; }

    public static Responder FromDatabase(M.MissionResponder m)
    {
      return new Responder
      {
        Id = m.MemberId,
        FirstName = m.FirstName ?? m.Member.FirstName,
        LastName = m.LastName ?? m.Member.LastName,
        WorkerNumber = m.WorkerNumber ?? m.Member.DEM,
        IsVisitor = !m.MemberId.HasValue
      };
    }
  }
}