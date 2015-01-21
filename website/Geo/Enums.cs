/*
 * Copyright 2010-2014 Matthew Cosand
 */
namespace Kcsara.Database.Geo
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;

  public enum LookupResult
  {
    Error,
    NotFound,
    Range,
    Success
  }

  public enum GeocodeQuality
  {
    Unknown = 0,
    NotFound = 1,
    Poor = 2,
    Medium = 8,
    High = 32
  }
}
