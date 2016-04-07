/*
 * Copyright 2013-2016 Matthew Cosand
 */
using System;
using System.Web;
using Kcsara.Database.Services;

namespace Internal.Website
{
  public class AlwaysYesAuth : IAuthService
  {

    public Guid UserId
    {
      get { throw new NotImplementedException(); }
    }

    public bool IsSelf(Guid id)
    {
      return true;
    }

    public bool IsAdmin
    {
      get { return true; }
    }

    public bool IsAuthenticated
    {
      get { return true; }
    }

    public bool IsUser
    {
      get { return true; }
    }

    public bool IsInRole(params string[] group)
    {
      return true;
    }

    public bool IsMembershipForPerson(Guid id)
    {
      return true;
    }

    public bool IsMembershipForUnit(Guid id)
    {
      return true;
    }

    public bool IsUserOrLocal(HttpRequestBase request)
    {
      return true;
    }

    public bool IsRoleForPerson(string role, Guid personId)
    {
      return true;
    }

    public bool IsRoleForUnit(string role, Guid unitId)
    {
      return true;
    }
  }
}
