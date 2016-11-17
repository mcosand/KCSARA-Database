/*
 * Copyright 2013-2014 Matthew Cosand
 */
namespace Kcsara.Database
{
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using System.Linq;
  using System.Web;

  public class AppSettings : IAppSettings
  {
    public string GroupName
    {
      get { return ConfigurationManager.AppSettings["site:groupAcronym"] ?? "KCSARA"; }
    }

    public string GroupFullName
    {
      get { return ConfigurationManager.AppSettings["site:groupName"] ?? "King County Search and Rescue"; }
    }
  }
}
