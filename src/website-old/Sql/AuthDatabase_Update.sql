CREATE TABLE #checks
    (
    K VARCHAR(20) PRIMARY KEY,
    V VARCHAR(255)
    )
GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'authKey' AND [object_id] = OBJECT_ID(N'partners'))
        INSERT INTO #checks (K) VALUES('partners_authKey')

IF EXISTS (SELECT 1 FROM #checks WHERE K='partners_authKey') BEGIN
  BEGIN TRANSACTION
  SET QUOTED_IDENTIFIER ON
  SET ARITHABORT ON
  SET NUMERIC_ROUNDABORT OFF
  SET CONCAT_NULL_YIELDS_NULL ON
  SET ANSI_NULLS ON
  SET ANSI_PADDING ON
  SET ANSI_WARNINGS ON
  COMMIT TRANSACTION
END
GO
IF EXISTS (SELECT 1 FROM #checks WHERE K='partners_authKey') BEGIN
  BEGIN TRANSACTION
  CREATE TABLE dbo.Tmp_partners
	  (
	  siteId int NOT NULL,
	  ticketPage varchar(500) NULL,
	  referrerPattern varchar(100) NULL,
	  authKey varchar(100) NULL,
	  canDelegate bit NOT NULL,
	  unitId uniqueidentifier NULL,
	  defaultUser varchar(50) NULL
	  )  ON [PRIMARY]
END
GO
IF EXISTS (SELECT 1 FROM #checks WHERE K='partners_authKey') ALTER TABLE dbo.Tmp_partners SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS (SELECT 1 FROM #checks WHERE K='partners_authKey') BEGIN
  IF EXISTS (SELECT 1 FROM sys.objects WHERE type_desc LIKE '%CONSTRAINT' AND OBJECT_NAME(OBJECT_ID)='DF_partners_canDelegate')
    ALTER TABLE dbo.partners DROP CONSTRAINT DF_partners_canDelegate
  ALTER TABLE dbo.Tmp_partners ADD CONSTRAINT DF_partners_canDelegate DEFAULT 0 FOR canDelegate
END
GO
IF EXISTS (SELECT 1 FROM #checks WHERE K='partners_authKey' AND EXISTS(SELECT * FROM dbo.partners))
	   EXEC('INSERT INTO dbo.Tmp_partners (siteId, ticketPage, referrerPattern)
		  SELECT siteId, ticketPage, referrerPattern FROM dbo.partners WITH (HOLDLOCK TABLOCKX)')
GO
IF EXISTS (SELECT 1 FROM #checks WHERE K='partners_authKey') DROP TABLE dbo.partners
GO
IF EXISTS (SELECT 1 FROM #checks WHERE K='partners_authKey') EXECUTE sp_rename N'dbo.Tmp_partners', N'partners', 'OBJECT' 
GO
IF EXISTS (SELECT 1 FROM #checks WHERE K='partners_authKey') COMMIT

IF NOT EXISTS (SELECT 1 FROM aspnet_Roles WHERE ApplicationId='F4266656-79F7-4723-9580-0A1AF8B13F0D' AND roleName LIKE 'cdb.trainingeditors')
  INSERT INTO aspnet_Roles (ApplicationId,RoleId,RoleName,LoweredRoleName) VALUES('F4266656-79F7-4723-9580-0A1AF8B13F0D',NEWID(),'cdb.trainingeditors','cdb.trainingeditors')

IF NOT EXISTS (SELECT 1 FROM RolesInRoles rr JOIN aspnet_Roles c ON c.RoleId=rr.ChildRoleId JOIN aspnet_Roles p ON p.RoleId=rr.ParentRoleId WHERE c.RoleName LIKE 'cdb.admins' AND p.RoleName like 'cdb.trainingeditors')
  INSERT INTO RolesInRoles (ChildRoleId, ParentRoleId) VALUES((SELECT RoleId FROM aspnet_Roles WHERE ApplicationId='f4266656-79f7-4723-9580-0a1af8b13f0d' AND RoleName='cdb.admins'),(SELECT RoleId FROM aspnet_Roles WHERE ApplicationId='f4266656-79f7-4723-9580-0a1af8b13f0d' AND RoleName='cdb.trainingeditors'))

IF NOT EXISTS (SELECT 1 FROM aspnet_Roles WHERE ApplicationId='F4266656-79F7-4723-9580-0A1AF8B13F0D' AND roleName LIKE 'cdb.missioneditors')
  INSERT INTO aspnet_Roles (ApplicationId,RoleId,RoleName,LoweredRoleName) VALUES('F4266656-79F7-4723-9580-0A1AF8B13F0D',NEWID(),'cdb.missioneditors','cdb.missioneditors')

IF NOT EXISTS (SELECT 1 FROM RolesInRoles rr JOIN aspnet_Roles c ON c.RoleId=rr.ChildRoleId JOIN aspnet_Roles p ON p.RoleId=rr.ParentRoleId WHERE c.RoleName LIKE 'cdb.admins' AND p.RoleName like 'cdb.missioneditors')
  INSERT INTO RolesInRoles (ChildRoleId, ParentRoleId) VALUES((SELECT RoleId FROM aspnet_Roles WHERE ApplicationId='f4266656-79f7-4723-9580-0a1af8b13f0d' AND RoleName='cdb.admins'),(SELECT RoleId FROM aspnet_Roles WHERE ApplicationId='f4266656-79f7-4723-9580-0a1af8b13f0d' AND RoleName='cdb.missioneditors'))


DROP TABLE #checks