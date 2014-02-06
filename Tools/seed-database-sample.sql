DECLARE @mtnrsq UNIQUEIDENTIFIER = NEWID()
IF EXISTS (SELECT 1 FROM SarUnits WHERE DisplayName='MtnRsq')
  SET @mtnrsq = (SELECT id FROM SarUnits WHERE DisplayName='MtnRsq')
ELSE
  INSERT INTO SarUnits (Id,DisplayName,LongName, HasOvertime, LastChanged) VALUES (@mtnrsq, 'MtnRsq', 'Mountain Rescue', 0, GETDATE())

IF NOT EXISTS (SELECT 1 FROM UnitStatus WHERE Unit_Id=@mtnrsq)
BEGIN
  INSERT INTO UnitStatus (Id,StatusName,IsActive,InternalWacLevel,WacLevel,GetsAccount,Unit_Id, LastChanged) VALUES (newid(), 'Active', 1, 4, 4, 0, @mtnrsq, GETDATE())
  INSERT INTO UnitStatus (Id,StatusName,IsActive,InternalWacLevel,WacLevel,GetsAccount,Unit_Id, LastChanged) VALUES (newid(), 'Novice', 1, 1, 1, 0, @mtnrsq, GETDATE())
  INSERT INTO UnitStatus (Id,StatusName,IsActive,InternalWacLevel,WacLevel,GetsAccount,Unit_Id, LastChanged) VALUES (newid(), 'Alumni', 0, 0, 0, 0, @mtnrsq, GETDATE())
END

DECLARE @dogs UNIQUEIDENTIFIER = NEWID()
IF EXISTS (SELECT 1 FROM SarUnits WHERE DisplayName='Dogs')
  SET @dogs = (SELECT id FROM SarUnits WHERE DisplayName='Dogs')
ELSE
  INSERT INTO SarUnits (Id,DisplayName,LongName, HasOvertime, LastChanged) VALUES (@dogs, 'Dogs', 'Search Dogs', 0, GETDATE())

IF NOT EXISTS (SELECT 1 FROM UnitStatus WHERE Unit_Id=@dogs)
BEGIN
  INSERT INTO UnitStatus (Id,StatusName,IsActive,InternalWacLevel,WacLevel,GetsAccount,Unit_Id, LastChanged) VALUES (newid(), 'Active', 1, 4, 4, 0, @dogs, GETDATE())
  INSERT INTO UnitStatus (Id,StatusName,IsActive,InternalWacLevel,WacLevel,GetsAccount,Unit_Id, LastChanged) VALUES (newid(), 'Trainee', 1, 1, 1, 0, @dogs, GETDATE())
  INSERT INTO UnitStatus (Id,StatusName,IsActive,InternalWacLevel,WacLevel,GetsAccount,Unit_Id, LastChanged) VALUES (newid(), 'OnLeave', 0, 0, 0, 0, @dogs, GETDATE())
  INSERT INTO UnitStatus (Id,StatusName,IsActive,InternalWacLevel,WacLevel,GetsAccount,Unit_Id, LastChanged) VALUES (newid(), 'Alumni', 0, 0, 0, 0, @dogs, GETDATE())
END

-- ========================== TRAINING ==================================

DECLARE @firstaid UNIQUEIDENTIFIER = NEWID()
IF EXISTS (SELECT 1 FROM TrainingCourses WHERE DisplayName='First Aid')
  SET @firstaid = (SELECT id FROM TrainingCourses WHERE DisplayName='First Aid')
ELSE
  INSERT INTO TrainingCourses (Id, DisplayName, FullName, Categories, ValidMonths, WacRequired, ShowOnCard, LastChanged)
       VALUES (@firstaid, 'First Aid', 'First Aid', 'medical', 24, 224, 0, GETDATE())

DECLARE @emt UNIQUEIDENTIFIER = NEWID()
IF EXISTS (SELECT 1 FROM TrainingCourses WHERE DisplayName='EMT')
  SET @emt = (SELECT id FROM TrainingCourses WHERE DisplayName='EMT')
ELSE
  INSERT INTO TrainingCourses (Id, DisplayName, FullName, Categories, ValidMonths, WacRequired, ShowOnCard, LastChanged)
       VALUES (@emt, 'EMT', 'Emergency Medical Technician', 'medical', 24, 0, 0, GETDATE())

IF NOT EXISTS (SELECT 1 FROM TrainingRules WHERE RuleText=CAST(@emt AS CHAR(36))+'>' + CAST(@firstaid AS CHAR(36)))
  INSERT INTO TrainingRules (Id, RuleText, LastChanged) VALUES (NEWID(), CAST(@emt AS CHAR(36))+'>' + CAST(@firstaid AS CHAR(36)), GETDATE())

DECLARE @helo UNIQUEIDENTIFIER = NEWID()
IF EXISTS (SELECT 1 FROM TrainingCourses WHERE DisplayName='Helo')
  SET @helo = (SELECT id FROM TrainingCourses WHERE DisplayName='Helo')
ELSE
  INSERT INTO TrainingCourses (Id, DisplayName, FullName, Categories, ValidMonths, WacRequired, ShowOnCard, LastChanged)
       VALUES (@helo, 'Helo', 'Helicopter Safety', 'helo', 24, 224, 0, GETDATE())


-- ==========================  PEOPLE ===================================

DECLARE @marc UNIQUEIDENTIFIER = NEWID()
IF EXISTS (SELECT 1 FROM Members WHERE FirstName='Marc')
  SET @marc = (SELECT id FROM Members WHERE FirstName='Marc')
ELSE
  INSERT INTO Members (Id,DEM,LastName,FirstName,InternalBirthDate,InternalGender,InternalWacLevel,WacLevelDate,BackgroundDate,LastChanged,[Status])
               VALUES (@marc, '1534', 'Rickenbacker', 'Marc', '1973-10-25', 'm', 4, '1995-09-21', '2012-10-04', GETDATE(), 48)

IF NOT EXISTS (SELECT 1 FROM UnitMemberships WHERE Unit_Id=@mtnrsq AND Person_Id=@marc)
  INSERT INTO UnitMemberships (Id, Activated, LastChanged, Person_Id, Unit_Id, Status_Id)
    VALUES(NEWID(), '1995-07-14', GETDATE(), @marc, @mtnrsq, (SELECT id FROM UnitStatus WHERE Unit_Id=@mtnrsq AND StatusName='Active'))

IF NOT EXISTS (SELECT 1 FROM UnitMemberships WHERE Unit_Id=@dogs AND Person_Id=@marc)
  INSERT INTO UnitMemberships (Id, Activated, LastChanged, Person_Id, Unit_Id, Status_Id)
    VALUES(NEWID(), '1997-09-21', GETDATE(), @marc, @dogs, (SELECT id FROM UnitStatus WHERE Unit_Id=@dogs AND StatusName='Active'))

IF NOT EXISTS (SELECT 1 FROM TrainingAwards WHERE Member_Id=@marc AND Course_Id=@emt)
  INSERT INTO TrainingAwards (Id, Completed, Expiry, metadata, LastChanged, Member_Id, Course_Id)
    VALUES (NEWID(), '2013-06-23', '2015-06-23', 'license #1234566', GETDATE(), @marc, @emt)



-- ================  VOLUNTEER ACCOUNTS / MEMBERSHIPS  ===============
DECLARE @userid UNIQUEIDENTIFIER
DECLARE @RC bit

DECLARE @usersnow DATETIME = GETUTCDATE()

IF NOT EXISTS (SELECT 1 FROM aspnet_Users WHERE username='marc' AND ApplicationId='F4266656-79F7-4723-9580-0A1AF8B13F0D')
BEGIN
  -- Seed admin account with password 'password'
	SET @userid = NEWID()
	execute [dbo].[aspnet_Membership_CreateUser] 'Kcsar','marc','+taapl3jPAcYr8P/gF4hGPz/7XQ=','obW1Z+SoySqNDfoTqRItCA==','marc@test.kcesar.org',null,null,1,@usersnow,@usersnow,1,1,@userid OUTPUT
END
ELSE
  SET @userid = (SELECT UserId FROM aspnet_Users WHERE username='marc' AND ApplicationId='F4266656-79F7-4723-9580-0A1AF8B13F0D')

EXECUTE @RC = [dbo].[aspnet_UsersInRoles_IsUserInRole] 'Kcsar','marc','cdb.admins'
IF (1 <> @RC)
  EXECUTE [dbo].[aspnet_UsersInRoles_AddUsersToRoles] 'Kcsar','marc','cdb.admins',@usersnow

IF NOT EXISTS (SELECT 1 FROM aspnet_Users WHERE username='bernard' AND ApplicationId='F4266656-79F7-4723-9580-0A1AF8B13F0D')
BEGIN
  -- Seed admin account with password 'password'
	SET @userid = NEWID()
	execute [dbo].[aspnet_Membership_CreateUser] 'Kcsar','bernard','+taapl3jPAcYr8P/gF4hGPz/7XQ=','obW1Z+SoySqNDfoTqRItCA==','bernard@test.kcesar.org',null,null,1,@usersnow,@usersnow,1,1,@userid OUTPUT
END
ELSE
  SET @userid = (SELECT UserId FROM aspnet_Users WHERE username='bernard' AND ApplicationId='F4266656-79F7-4723-9580-0A1AF8B13F0D')

EXECUTE @RC = [dbo].[aspnet_UsersInRoles_IsUserInRole] 'Kcsar','bernard','cdb.users'
IF (1 <> @RC)
  EXECUTE [dbo].[aspnet_UsersInRoles_AddUsersToRoles] 'Kcsar','bernard','cdb.users',@usersnow

IF NOT EXISTS (SELECT 1 FROM aspnet_Users WHERE username='virgil' AND ApplicationId='F4266656-79F7-4723-9580-0A1AF8B13F0D')
BEGIN
  -- Seed admin account with password 'password'
	SET @userid = NEWID()
	execute [dbo].[aspnet_Membership_CreateUser] 'Kcsar','virgil','+taapl3jPAcYr8P/gF4hGPz/7XQ=','obW1Z+SoySqNDfoTqRItCA==','virgil@test.kcesar.org',null,null,1,@usersnow,@usersnow,1,1,@userid OUTPUT
END
ELSE
  SET @userid = (SELECT UserId FROM aspnet_Users WHERE username='virgil' AND ApplicationId='F4266656-79F7-4723-9580-0A1AF8B13F0D')

EXECUTE @RC = [dbo].[aspnet_UsersInRoles_IsUserInRole] 'Kcsar','virgil','cdb.users'
IF (1 <> @RC)
  EXECUTE [dbo].[aspnet_UsersInRoles_AddUsersToRoles] 'Kcsar','virgil','cdb.users',@usersnow


-- ===================== TRAINING ROSTERS  ===============================
DECLARE @helo_class1 UNIQUEIDENTIFIER = NEWID()
IF EXISTS (SELECT 1 FROM Trainings WHERE Title='Helicopter' AND StartTime='2013-10-19 19:00')
  SET @helo_class1 = (SELECT id FROM Trainings WHERE Title='Helicopter' AND StartTime='2013-10-19 19:00')
ELSE
  INSERT INTO Trainings (Id,Title,County,StartTime,StopTime,LastChanged,Location)
    VALUES (@helo_class1, 'Helicopter', 'king', '2013-10-19 19:00', '2013-10-19 21:00', GETDATE(), 'Training Building')

IF NOT EXISTS (SELECT 1 FROM TrainingTrainingCourses WHERE Training_Id = @helo_class1 AND TrainingCourse_Id=@helo)
  INSERT INTO TrainingTrainingCourses (Training_Id, TrainingCourse_Id) VALUES (@helo_class1, @helo)

IF NOT EXISTS (SELECT 1 FROM TrainingRosters WHERE Person_Id=@marc AND Training_Id=@helo_class1)
  INSERT INTO TrainingRosters (Id, TimeIn, [TimeOut], Miles, LastChanged, Person_Id, Training_Id)
    VALUES (NEWID(), '2013-10-19 18:00', '2013-10-19 22:00', 60, GETDATE(), @marc, @helo_class1)

IF NOT EXISTS (SELECT 1 FROM TrainingAwards WHERE Member_Id=@marc AND Course_Id=@helo)
  INSERT INTO TrainingAwards (Id, Completed, Expiry, LastChanged, Member_Id, Course_Id, Roster_Id)
    VALUES (NEWID(), '2013-10-19 22:00', '2015-10-19 22:00', GETDATE(), @marc, @helo, (SELECT id FROM TrainingRosters WHERE Person_Id=@marc AND Training_Id=@helo_class1))

-- ========================  MISSIONS  ====================================
DECLARE @mSnowLake UNIQUEIDENTIFIER = NEWID()
IF EXISTS (SELECT 1 FROM Missions WHERE StartTime='2013-04-05 23:00')
  SET @mSnowLake = (SELECT id FROM Missions WHERE StartTime='2013-04-05 23:00')
ELSE
  INSERT INTO Missions (Id,Title,County,StateNumber,MissionType,StartTime,StopTime,Location,ReportCompleted,LastChanged)
    VALUES (@mSnowLake, 'Snow Lake Lost Snowshoer', 'king', '13-0450', 'search,rescue', '2013-04-05 23:00', '2013-04-06 09:00', 'Alpental', 1, GETDATE())

IF NOT EXISTS (SELECT 1 FROM MissionRosters WHERE Mission_Id=@mSnowLake AND Person_Id=@marc)
  INSERT INTO MissionRosters (Id,InternalRole,TimeIn,[TimeOut],Miles,LastChanged,Mission_Id,Person_Id,Unit_Id)
	  VALUES (NEWID(), 'Field', '2013-04-05 23:00', '2013-04-06 10:00', 60, GETDATE(), @mSnowLake, @marc, @mtnrsq)

DELETE FROM Missions WHERE Title LIKE '%/[recent/]' ESCAPE '/'

DECLARE @recent1 UNIQUEIDENTIFIER = NEWID()
INSERT INTO Missions (Id,Title,County,StateNumber,MissionType,StartTime,StopTime,Location,ReportCompleted,LastChanged)
  VALUES (@recent1, 'Snow Lake Hiker [recent]', 'king', '14-0050', 'rescue', DATEADD(day, -3, GETDATE()), null, 'Alpental', 1, GETDATE())

DECLARE @recent2 UNIQUEIDENTIFIER = NEWID()
INSERT INTO Missions (Id,Title,County,StateNumber,MissionType,StartTime,StopTime,Location,ReportCompleted,LastChanged)
  VALUES (@recent2, 'Annette Lake Hasty [recent]', 'king', '14-0083', 'rescue', DATEADD(minute, -94, GETDATE()), null, 'Annette Lake Trailhead', 0, GETDATE())
INSERT INTO MissionResponseStatus (Id, CallForPeriod, LastChanged)
  VALUES (@recent2, DATEADD(minute, -90, GETDATE()), GETDATE())

DECLARE @recent3 UNIQUEIDENTIFIER = NEWID()
INSERT INTO Missions (Id,Title,County,StateNumber,MissionType,StartTime,StopTime,Location,ReportCompleted,LastChanged)
  VALUES (@recent3, 'Evidence Search [recent]', 'king', '14-ES-017', 'evidence', DATEADD(minute, 3600, GETDATE()), null, 'Some urban place', 0, GETDATE())
INSERT INTO MissionResponseStatus (Id, CallForPeriod, LastChanged)
  VALUES (@recent3, DATEADD(minute, 3600, GETDATE()), GETDATE())


-- RANKS
DECLARE @mtnMember UNIQUEIDENTIFIER = NEWID()
IF EXISTS (SELECT 1 FROM TrainingCourses WHERE DisplayName='MtnRsq Basic')
  SET @mtnMember = (SELECT id FROM TrainingCourses WHERE DisplayName='MtnRsq Basic')
ELSE
  INSERT INTO TrainingCourses (Id, DisplayName, FullName, Categories, ValidMonths, WacRequired, ShowOnCard, LastChanged)
    VALUES (@mtnMember, 'MtnRsq Basic', 'MtnRsq Team Member', 'leadership', null, 0, 0, GETDATE())
UPDATE TrainingCourses SET Unit_ID=@mtnRsq WHERE ID=@mtnMember

DECLARE @mtnTL UNIQUEIDENTIFIER = NEWID()
IF EXISTS (SELECT 1 FROM TrainingCourses WHERE DisplayName='MtnRsq TL')
  SET @mtnTL = (SELECT id FROM TrainingCourses WHERE DisplayName='MtnRsq TL')
ELSE
  INSERT INTO TrainingCourses (Id, DisplayName, FullName, Categories, ValidMonths, WacRequired, ShowOnCard, LastChanged)
    VALUES (@mtnTL, 'MtnRsq TL', 'MtnRsq Team Leader', 'leadership', null, 0, 0, GETDATE())
UPDATE TrainingCourses SET Unit_ID=@mtnRsq WHERE Id=@mtnTL

DECLARE @mtnFL UNIQUEIDENTIFIER = NEWID()
IF EXISTS (SELECT 1 FROM TrainingCourses WHERE DisplayName='MtnRsq FL')
  SET @mtnFL = (SELECT id FROM TrainingCourses WHERE DisplayName='MtnRsq FL')
ELSE
  INSERT INTO TrainingCourses (Id, DisplayName, FullName, Categories, ValidMonths, WacRequired, ShowOnCard, LastChanged)
    VALUES (@mtnFL, 'MtnRsq FL', 'MtnRsq Field Member', 'leadership', null, 0, 0, GETDATE())
UPDATE TrainingCourses SET Unit_Id=@mtnRsq WHERE Id=@mtnFL

IF NOT EXISTS (SELECT 1 FROM TrainingAwards WHERE Member_Id=@marc AND Course_Id=@mtnMember)
  INSERT INTO TrainingAwards (Id, Completed, Expiry, LastChanged, Member_Id, Course_Id)
    VALUES (NEWID(), '1995-10-19', null, GETDATE(), @marc, @mtnMember)

IF NOT EXISTS (SELECT 1 FROM TrainingAwards WHERE Member_Id=@marc AND Course_Id=@mtnTL)
  INSERT INTO TrainingAwards (Id, Completed, Expiry, LastChanged, Member_Id, Course_Id)
    VALUES (NEWID(), '1999-10-19', null, GETDATE(), @marc, @mtnTL)
