IF NOT EXISTS (SELECT 1 FROM aspnet_SchemaVersions)
BEGIN
  INSERT INTO aspnet_SchemaVersions VALUES('common', 1, 1)
  INSERT INTO aspnet_SchemaVersions VALUES('health monitoring', 1, 1)
  INSERT INTO aspnet_SchemaVersions VALUES('membership', 1, 1)
  INSERT INTO aspnet_SchemaVersions VALUES('personalization', 1, 1)
  INSERT INTO aspnet_SchemaVersions VALUES('profile', 1, 1)
  INSERT INTO aspnet_SchemaVersions VALUES('role manager', 1, 1)
END