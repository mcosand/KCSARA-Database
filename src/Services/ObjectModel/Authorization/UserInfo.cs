/*
 * Copyright 2016 Matthew Cosand
 */
using System;
using System.Collections.Generic;

namespace Kcsara.Database.Model
{
  public class UserInfo
  {
    public UserInfo()
    {
      Roles = new List<string>();
      Units = new List<NameIdPair>();
    }
    public List<string> Roles { get; set; }

    public Guid? MemberId { get; set; }

    public List<NameIdPair> Units { get; set; }
  }
}
