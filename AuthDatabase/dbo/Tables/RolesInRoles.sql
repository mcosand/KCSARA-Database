CREATE TABLE [dbo].[RolesInRoles] (
    [ChildRoleId]  UNIQUEIDENTIFIER NOT NULL,
    [ParentRoleId] UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY CLUSTERED ([ChildRoleId] ASC, [ParentRoleId] ASC),
    FOREIGN KEY ([ChildRoleId]) REFERENCES [dbo].[aspnet_Roles] ([RoleId]),
    FOREIGN KEY ([ParentRoleId]) REFERENCES [dbo].[aspnet_Roles] ([RoleId])
);

