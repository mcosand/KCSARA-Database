/*
 * Copyright 2009-2014 Matthew Cosand
 */

namespace Kcsar.Membership
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Web.Security;
    using System.Text.RegularExpressions;

    public class RoleProvider : SqlRoleProvider, INestedRoleProvider
    {
        public static ExtendedRole GetRole(string roleName)
        {
            return (Roles.Provider is RoleProvider)
                ? ((RoleProvider)Roles.Provider).ExtendedGetRole(roleName)
                : new ExtendedRole { Name = roleName };
        }

        public static ExtendedRole[] GetRoles()
        {
            return (Roles.Provider is RoleProvider)
                ? ((RoleProvider)Roles.Provider).ExtendedGetAllRoles()
                : Roles.Provider.GetAllRoles().Select(x => new ExtendedRole { Name = x }).ToArray();
        }
        
        protected string connectionString = null;

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            SqlClientPermission pSql = new SqlClientPermission(System.Security.Permissions.PermissionState.Unrestricted);
            pSql.Assert();

            string connectionStringName = config["connectionStringName"];
            if (string.IsNullOrEmpty(connectionStringName))
            {
                throw new ProviderException("Connection string not specified");
            }

            this.connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;

            base.Initialize(name, config);
        }

        public ExtendedRole ExtendedGetRole(string roleName)
        {
            ExtendedRole[] roles = this.ExtendedGetAllRoles(roleName);

            return (roles.Length == 0) ? null : roles[0];
        }

        public ExtendedRole[] ExtendedGetAllRoles()
        {
            return this.ExtendedGetAllRoles(null);
        }

        public override void CreateRole(string roleName)
        {
            CreateRole(roleName, null);
        }

        public void CreateRole(string roleName, string fromManager)
        {
            Utils.CheckSimpleName(roleName, true);

            if (this.RoleExists(roleName))
            {
                throw new InvalidOperationException("Role " + roleName + " already exists");
            }

            base.CreateRole(roleName);
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            Utils.CheckSimpleName(roleName, true);

            bool success = base.DeleteRole(roleName, throwOnPopulatedRole);

            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(string.Format("DELETE FROM RolesInRoles WHERE ParentRoleId IN (SELECT RoleId FROM aspnet_Roles WHERE RoleName='{0}') OR ChildRoleId IN (SELECT RoleId FROM aspnet_Roles WHERE RoleName='{0}')", roleName), connection);

                cmd.ExecuteNonQuery();
            }

            return success;
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            base.AddUsersToRoles(usernames, roleNames);
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            base.RemoveUsersFromRoles(usernames, roleNames);
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            return this.FindUsersInRole(roleName, usernameToMatch, false);
        }

        public string[] FindUsersInRole(string roleName, string usernameToMatch, bool recurse)
        {
            List<string> names = new List<string>(base.FindUsersInRole(roleName, usernameToMatch));

            if (recurse)
            {
                string[] roles = GetRolesInRole(roleName, true);

                foreach (string role in roles)
                {
                    foreach (string user in base.FindUsersInRole(role, usernameToMatch))
                    {
                        if (!names.Contains(user))
                        {
                            names.Add(user);
                        }
                    }
                }
            }

            return names.ToArray();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            return this.IsUserInRole(username, roleName, false);
        }

        public bool IsUserInRole(string username, string roleName, bool recurse)
        {
            if (base.IsUserInRole(username, roleName))
            {
                return true;
            }

            if (recurse)
            {
                foreach (string role in GetRolesInRole(roleName, true))
                {
                    if (base.IsUserInRole(username, role))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override string[] GetRolesForUser(string username)
        {
            return this.GetRolesForUser(username, true);
        }

        public string[] GetRolesForUser(string username, bool recurse)
        {
            List<string> roles = new List<string>(base.GetRolesForUser(username));

            if (recurse)
            {
                int count = roles.Count;
                for (int i = 0; i < count; i++)
                {
                    foreach (string subRole in GetRolesInRole(roles[i], false, true))
                    {
                        if (!roles.Contains(subRole))
                        {
                            roles.Add(subRole);
                        }
                    }
                }
            }
            return roles.ToArray();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            return this.GetUsersInRole(roleName, false);
        }

        public string[] GetUsersInRole(string roleName, bool recurse)
        {
            List<string> users = new List<string>(base.GetUsersInRole(roleName));

            if (recurse)
            {
                foreach (string role in GetRolesInRole(roleName, true))
                {
                    foreach (string user in base.GetUsersInRole(roleName))
                    {
                        if (!users.Contains(user))
                        {
                            users.Add(user);
                        }
                    }
                }
            }

            return users.ToArray();
        }

        public bool IsRoleDirectlyInRole(string child, string parent)
        {
            Utils.CheckSimpleName(child, true);
            Utils.CheckSimpleName(parent, true);

            List<string> children = GetRoleChildren(parent);

            foreach (string c in children)
            {
                if (c.Equals(child, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public string[] GetRolesInRole(string roleName, bool recurse)
        {
            return GetRolesInRole(roleName, true, recurse);
        }

        public string[] GetRolesInRole(string roleName, bool findChildren, bool recurse)
        {
            Utils.CheckSimpleName(roleName, true);

            if (recurse)
            {
                Dictionary<string, List<string>> asc;
                Dictionary<string, List<string>> desc;
                GetRoleMappings(out desc, out asc);

                List<string> answer = new List<string>();
                GetRolesInRole(roleName, "", findChildren  ? desc : asc, answer);
                return answer.OrderBy(x => x).ToArray();
            }
            else if (findChildren)
            {
                return GetRoleChildren(roleName).OrderBy(x => x).ToArray();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void AddRoleToRole(string child, string parent)
        {
            Utils.CheckSimpleName(child, true);
            Utils.CheckSimpleName(parent, true);

            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                
                SqlCommand cmd = new SqlCommand(string.Format("INSERT INTO RolesInRoles SELECT c.RoleId as ChildRoleId,p.RoleId AS ParentRoleId FROM aspnet_Roles AS p,aspnet_Roles AS c WHERE p.RoleName = '{0}' AND c.RoleName='{1}'", parent, child), connection);
                connection.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public void ServiceAddRoleToRole(string child, string parent)
        {

        }

        public void RemoveRoleFromRole(string child, string parent)
        {
            Utils.CheckSimpleName(child, true);
            Utils.CheckSimpleName(parent, true);

            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(string.Format("DELETE FROM RolesInRoles WHERE ParentRoleId IN (SELECT RoleId FROM aspnet_Roles WHERE RoleName='{0}') AND ChildRoleId IN (SELECT RoleId FROM aspnet_Roles WHERE RoleName='{1}')", parent, child), connection);

                cmd.ExecuteNonQuery();
            }
        }

        private List<string> GetRoleChildren(string parent)
        {
            List<string> children = new List<string>();

            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {

                SqlCommand cmd = new SqlCommand("SELECT c.rolename FROM RolesInRoles as j JOIN aspnet_Roles as p ON j.ParentRoleId=p.RoleId JOIN aspnet_Roles as c ON j.ChildRoleId=c.RoleId WHERE p.RoleName='" + parent + "'", connection);
                connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        children.Add(reader.GetString(0));
                    }
                }
            }

            return children;
        }

        private void GetRoleMappings(out Dictionary<string, List<string>> descending, out Dictionary<string, List<string>> ascending)
        {
            descending = new Dictionary<string, List<string>>();
            ascending = new Dictionary<string, List<string>>();

            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {

                SqlCommand cmd = new SqlCommand("SELECT p.rolename,c.rolename FROM RolesInRoles as j JOIN aspnet_Roles as p ON j.ParentRoleId=p.RoleId JOIN aspnet_Roles as c ON j.ChildRoleId=c.RoleId", connection);
                connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
                {
                    while (reader.Read())
                    {
                        string parent = reader.GetString(0);
                        string child = reader.GetString(1);

                        if (!descending.ContainsKey(parent))
                        {
                            descending.Add(parent, new List<string>());
                        }
                        if (!ascending.ContainsKey(child))
                        {
                            ascending.Add(child, new List<string>());
                        }

                        if (!descending[parent].Contains(child))
                        {
                            descending[parent].Add(child);
                        }
                        if (!ascending[child].Contains(parent))
                        {
                            ascending[child].Add(parent);
                        }
                    }
                }
            }

        }

        private void GetRolesInRole(string roleName, string visited, Dictionary<string, List<string>> tree, List<string> answer)
        {

            if (tree.ContainsKey(roleName) && tree[roleName].Count > 0)
            {
                List<string> walker = tree[roleName];
                // Flatten the list, with loop detection

                string v = string.Format("{0}:", visited);
                foreach (string child in walker)
                {
                    if (v.Contains(string.Format(":{0}:", child)))
                    {
                        throw new ApplicationException(string.Format("Found loop in groups: {0}:{1}", visited, child));
                    }
                    
                    GetRolesInRole(child, v + child, tree, answer);
                    
                    if (!answer.Contains(child))
                    {
                        answer.Add(child);
                    }
                }
            }
        }

        private ExtendedRole[] ExtendedGetAllRoles(string roleName)
        {
            List<ExtendedRole> list = new List<ExtendedRole>();
            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                string query = "SELECT RoleName,ExternalDest,Owners,EmailAddress FROM aspnet_Roles";
                string joiner = " WHERE";

                if (!string.IsNullOrEmpty(roleName))
                {
                    Utils.CheckSimpleName(roleName, true);
                    query += joiner + " LoweredRoleName = '" + roleName.ToLowerInvariant() + "'";
                    joiner = " AND";
                }

                connection.Open();
                SqlCommand cmd = new SqlCommand(query, connection);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ExtendedRole(reader.IsDBNull(2) ? null : reader.GetString(2), reader.IsDBNull(1) ? null : reader.GetString(1))
                        {
                            Name = reader.GetString(0),
                            EmailAddress = reader.IsDBNull(3) ? null : reader.GetString(3)
                        });
                    }
                }
            }

            return list.ToArray();
        }


        #region INestedRoleProvider Members

        public void UpdateRole(ExtendedRole role, string originalRole)
        {
            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                SqlCommand cmd = new SqlCommand("UPDATE aspnet_Roles SET RoleName=@newname, LoweredRoleName=@newnamelower, owners=@owners, emailaddress=@email, externaldest=@destinations WHERE RoleName = @name", connection);
                cmd.Parameters.AddWithValue("newname", role.Name);
                cmd.Parameters.AddWithValue("newnamelower", role.Name.ToLowerInvariant());
                cmd.Parameters.AddWithValue("name", originalRole ?? role.Name);
                cmd.Parameters.AddWithValue("owners", string.Join(",", role.Owners.Select(f => f.ToString()).ToArray()));
                cmd.Parameters.AddWithValue("destinations", string.Join(",", role.Destinations.ToArray()));
                cmd.Parameters.AddWithValue("email", (object)role.EmailAddress ?? DBNull.Value);
                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        #endregion
    }

    public class ExtendedRole
    {
        public ExtendedRole()
            : this(null, null)
        {
        }

        public ExtendedRole(string ownerList, string destinationList)
        {
            this.Owners = new List<Guid>();
            if (!string.IsNullOrEmpty(ownerList))
            {
                this.Owners.AddRange(ownerList.Split(',').Select(f => new Guid(f)));
            }

            this.Destinations = new List<string>();
            if (!string.IsNullOrEmpty(destinationList))
            {
                this.Destinations.AddRange(destinationList.Split(','));
            }
        }

        public string Name { get; set; }
        public List<Guid> Owners { get; private set;  }
        public List<string> Destinations { get; private set; }
        public string EmailAddress { get; set; }

    }
}
