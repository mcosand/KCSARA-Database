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