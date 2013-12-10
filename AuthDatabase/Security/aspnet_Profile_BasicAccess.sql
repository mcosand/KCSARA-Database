CREATE ROLE [aspnet_Profile_BasicAccess]
    AUTHORIZATION [dbo];


GO
EXECUTE sp_addrolemember @rolename = N'aspnet_Profile_BasicAccess', @membername = N'aspnet_Profile_FullAccess';

