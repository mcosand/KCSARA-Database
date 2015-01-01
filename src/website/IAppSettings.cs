/*
 * Copyright 2013-2014 Matthew Cosand
 */
namespace Kcsara.Database
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;

  public interface IAppSettings
  {
    string GroupName { get; }
    string GroupFullName { get; }
  }
}
