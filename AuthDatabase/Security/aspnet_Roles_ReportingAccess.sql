CREATE ROLE [aspnet_Roles_ReportingAccess]
    AUTHORIZATION [dbo];


GO
EXECUTE sp_addrolemember @rolename = N'aspnet_Roles_ReportingAccess', @membername = N'aspnet_Roles_FullAccess';

