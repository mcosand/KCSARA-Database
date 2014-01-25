KCSARA-Database
===============

Online database supporting search and rescue teams in King County, Washington USA


Getting Started
---------------
### Requirements
- Visual Studio 2012 (or later)
- SQL Database 2008 R2 or later (SQL Express, localdb, etc)

This project can be deployed into a Windows Azure web site instance using SQL Azure.

### First Run
1. Load kcsara-database.sln in Visual Studio
2. Review UpdateDatabaseKey app setting in web.config
3. Review ConnectionStrings, make sure they are compatible with your installation. If the connecting users has permissions to create a database on the server it does not need to be created before hand.
4. Run the project.
5. Point your web browser to `http://localhost:4944/admin/updatedatabase?updatekey=[appSettings Updatekey]`
6. The database is now running, and you can log in using username `admin`, with password `password`

### Add test data
`Tools\seed-database-sample.sql` contains sample data that can be loaded into the database either directly, or by using the administrative page `http://localhost:4944/admin/sql`.