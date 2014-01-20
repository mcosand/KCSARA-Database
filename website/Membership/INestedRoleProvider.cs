/*
 * Copyright 2009-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kcsar.Membership
{
    public interface INestedRoleProvider
    {
        ExtendedRole ExtendedGetRole(string roleName);
        void UpdateRole(ExtendedRole role, string originalName);

        void AddRoleToRole(string child, string parent);
        void RemoveRoleFromRole(string child, string parent);

        string[] GetRolesInRole(string roleName, bool recurse);

        string[] GetUsersInRole(string roleName, bool recurse);
        string[] GetRolesForUser(string username, bool recurse);
        bool IsUserInRole(string username, string roleName, bool recurse);
        string[] FindUsersInRole(string roleName, string usernameToMatch, bool recurse);
    }
}
