CREATE ROLE [aspnet_Profile_ReportingAccess]
    AUTHORIZATION [dbo];


GO
EXECUTE sp_addrolemember @rolename = N'aspnet_Profile_ReportingAccess', @membername = N'aspnet_Profile_FullAccess';

