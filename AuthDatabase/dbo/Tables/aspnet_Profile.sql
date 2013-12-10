CREATE TABLE [dbo].[aspnet_Profile] (
    [UserId]               UNIQUEIDENTIFIER NOT NULL,
    [PropertyNames]        NTEXT            NOT NULL,
    [PropertyValuesString] NTEXT            NOT NULL,
    [PropertyValuesBinary] IMAGE            NOT NULL,
    [LastUpdatedDate]      DATETIME         NOT NULL,
    PRIMARY KEY CLUSTERED ([UserId] ASC),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[aspnet_Users] ([UserId])
);


GO
EXECUTE sp_tableoption @TableNamePattern = N'[dbo].[aspnet_Profile]', @OptionName = N'text in row', @OptionValue = N'6000';

