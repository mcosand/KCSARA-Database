IF OBJECT_ID('aspnet_Applications', 'U') IS NOT NULL
BEGIN

BEGIN TRANSACTION MigrateUsers

  /* Migrate users */
  INSERT INTO AspNetUsers (Id,UserName,PasswordHash,SecurityStamp,EmailConfirmed,
  PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount,
  Email, NormalizedEmail, NormalizedUsername)
  SELECT
    aspnet_Users.UserId, 
	aspnet_Users.UserName,
	(aspnet_Membership.Password+'|'+aspnet_Membership.PasswordSalt),
    NewID(),
	1,
	NULL,
	0,
	0,
	CASE WHEN aspnet_Membership.IsLockedOut = 1 THEN DATEADD(YEAR, 1000, SYSUTCDATETIME()) ELSE NULL END,
	1,
	0,
	aspnet_Membership.Email,
	UPPER(aspnet_Membership.Email),
	UPPER(aspnet_Users.UserName)
  FROM aspnet_Users
  LEFT OUTER JOIN aspnet_Membership ON aspnet_Membership.ApplicationId = aspnet_Users.ApplicationId 
  AND aspnet_Users.UserId = aspnet_Membership.UserId
  LEFT OUTER JOIN AspNetUsers ON aspnet_Membership.UserId = AspNetUsers.Id
  WHERE AspNetUsers.Id IS NULL

  /* Migrate user question/answer */
  --INSERT INTO AspNetUsersExt (UserId, SecurityQuestion, SecurityAnswer, SecurityAnswerSalt)
  --SELECT aspnet_Users.UserId, aspnet_Membership.PasswordQuestion, PasswordAnswer, PasswordSalt
  --FROM aspnet_Users
  --LEFT OUTER JOIN aspnet_Membership ON aspnet_Membership.ApplicationId = aspnet_Users.ApplicationId 
  --AND aspnet_Users.UserId = aspnet_Membership.UserId
  --LEFT OUTER JOIN AspNetUsersExt ON aspnet_Membership.UserId = AspNetUsersExt.UserId
  --LEFT OUTER JOIN AspNetUsers ON aspnet_Membership.UserId = AspNetUsers.Id
  --WHERE AspNetUsers.Id IS NOT NULL AND AspNetUsersExt.UserId IS NULL

IF @@ERROR <> 0 
  BEGIN 
    ROLLBACK TRANSACTION MigrateUsers
    RETURN
  END

COMMIT TRANSACTION MigrateUsers

END