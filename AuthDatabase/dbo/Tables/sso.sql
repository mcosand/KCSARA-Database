CREATE TABLE [dbo].[sso] (
    [ticketId] UNIQUEIDENTIFIER NOT NULL,
    [siteId]   INT              NOT NULL,
    [username] VARCHAR (256)    NOT NULL,
    [expires]  DATETIME         NOT NULL
);

