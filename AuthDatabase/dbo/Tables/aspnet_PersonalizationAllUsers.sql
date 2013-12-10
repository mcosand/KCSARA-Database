CREATE TABLE [dbo].[aspnet_PersonalizationAllUsers] (
    [PathId]          UNIQUEIDENTIFIER NOT NULL,
    [PageSettings]    IMAGE            NOT NULL,
    [LastUpdatedDate] DATETIME         NOT NULL,
    PRIMARY KEY CLUSTERED ([PathId] ASC),
    FOREIGN KEY ([PathId]) REFERENCES [dbo].[aspnet_Paths] ([PathId])
);


GO
EXECUTE sp_tableoption @TableNamePattern = N'[dbo].[aspnet_PersonalizationAllUsers]', @OptionName = N'text in row', @OptionValue = N'6000';

