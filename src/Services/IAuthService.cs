/*
 * Copyright 2013-2015 Matthew Cosand
 */
namespace Kcsara.Database.Services
{
  using System;
  using System.Collections.Generic;
  using System.Web;

  public interface IAuthService
  {
    Guid UserId { get; }
    bool IsSelf(Guid id);
    bool IsSelf(string username);
    bool IsAdmin { get; }
    bool IsAuthenticated { get; }
    bool IsUser { get; }
    bool IsInRole(params string[] group);
    bool IsMembershipForPerson(Guid id);
    bool IsMembershipForUnit(Guid id);
    bool IsUserOrLocal(HttpRequestBase request);
    bool IsRoleForPerson(string role, Guid personId);
    bool IsRoleForUnit(string role, Guid unitId);

    bool ValidateUser(string username, string password);

    IEnumerable<string> GetGroupsForUser(string username);
    IEnumerable<string> GetGroupsFoGroup(string group);
  }
}
