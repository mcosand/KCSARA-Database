/*
 * Copyright 2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.api.Models
{
  using System;
  using System.ComponentModel.DataAnnotations;

  public class CreateMission
  {
    [Required]
    [Display(Name = "Mission Title")]
    public string Title { get; set; }

    [Required]
    [Display(Name = "Staging Location")]
    public string Location { get; set; }

    public DateTime Started { get; set; }
  }
}