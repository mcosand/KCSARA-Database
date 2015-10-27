/*
 * Copyright 2013-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public bool IsSelf(string username)
    {
      return true;
    }

    public IEnumerable<string> GetGroupsForUser(string username)
    {
      return new string[0];
    }

    public IEnumerable<string> GetGroupsFoGroup(string group)
    {
      return new string[0];
    }

    public bool ValidateUser(string username, string password)
    {
      return true;
    }
  }
}
