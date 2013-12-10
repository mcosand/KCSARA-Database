CREATE ROLE [aspnet_Membership_ReportingAccess]
    AUTHORIZATION [dbo];


GO
EXECUTE sp_addrolemember @rolename = N'aspnet_Membership_ReportingAccess', @membername = N'aspnet_Membership_FullAccess';

