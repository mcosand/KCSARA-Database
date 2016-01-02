/*
 * Copyright 2015-2016 Matthew Cosand
 */
namespace Kcsara.Database.Web.Models
{
  using System;

  public class EventSummary
  {
    public Guid Id { get; set; }

    public string Name { get; set; }
    public string StateNumber { get; set; }
    public string Location { get; set; }


    public DateTime Start { get; set; }
    public DateTime? Stop { get; set; }
  }
}