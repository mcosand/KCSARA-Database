IF NOT EXISTS (SELECT 1 FROM aspnet_Roles WHERE ApplicationId='F4266656-79F7-4723-9580-0A1AF8B13F0D' AND roleName LIKE 'cdb.trainingeditors')
  INSERT INTO aspnet_Roles (ApplicationId,RoleId,RoleName,LoweredRoleName) VALUES('F4266656-79F7-4723-9580-0A1AF8B13F0D',NEWID(),'cdb.trainingeditors','cdb.trainingeditors')

IF NOT EXISTS (SELECT 1 FROM RolesInRoles rr JOIN aspnet_Roles c ON c.RoleId=rr.ChildRoleId JOIN aspnet_Roles p ON p.RoleId=rr.ParentRoleId WHERE c.RoleName LIKE 'cdb.admins' AND p.RoleName like 'cdb.trainingeditors')
  INSERT INTO RolesInRoles (ChildRoleId, ParentRoleId) VALUES((SELECT RoleId FROM aspnet_Roles WHERE ApplicationId='f4266656-79f7-4723-9580-0a1af8b13f0d' AND RoleName='cdb.admins'),(SELECT RoleId FROM aspnet_Roles WHERE ApplicationId='f4266656-79f7-4723-9580-0a1af8b13f0d' AND RoleName='cdb.trainingeditors'))
