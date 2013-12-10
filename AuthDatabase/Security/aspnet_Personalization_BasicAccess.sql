CREATE ROLE [aspnet_Personalization_BasicAccess]
    AUTHORIZATION [dbo];


GO
EXECUTE sp_addrolemember @rolename = N'aspnet_Personalization_BasicAccess', @membername = N'aspnet_Personalization_FullAccess';

