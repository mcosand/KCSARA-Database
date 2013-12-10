CREATE ROLE [aspnet_Personalization_ReportingAccess]
    AUTHORIZATION [dbo];


GO
EXECUTE sp_addrolemember @rolename = N'aspnet_Personalization_ReportingAccess', @membername = N'aspnet_Personalization_FullAccess';

