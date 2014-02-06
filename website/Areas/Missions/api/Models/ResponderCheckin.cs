/*
 * Copyright 2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.api.Models
{
  using M = Kcsar.Database.Model;

  public class ResponderCheckin
  {
    public RespondingUnit Unit { get; set; }
    public M.ResponderStatus Status { get; set; }
    public M.MissionRole Role { get; set; }
    public Location Location { get; set; }
    public Responder Responder { get; set; }
  }
}