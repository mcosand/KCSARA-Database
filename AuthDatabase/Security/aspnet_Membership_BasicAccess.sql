CREATE ROLE [aspnet_Membership_BasicAccess]
    AUTHORIZATION [dbo];


GO
EXECUTE sp_addrolemember @rolename = N'aspnet_Membership_BasicAccess', @membername = N'aspnet_Membership_FullAccess';

