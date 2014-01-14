using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.api.Models
{
  public class CreateMission
  {
    [Required]
    [Display(Name="Mission Title")]
    public string Title { get; set; }

    [Required]
    [Display(Name="Staging Location")]
    public string Location { get; set; }

    public DateTime Started { get; set; }
  }
}