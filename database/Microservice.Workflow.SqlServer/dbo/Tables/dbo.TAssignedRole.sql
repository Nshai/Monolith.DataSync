CREATE TABLE [dbo].[TAssignedRole]
(
[AssignedRoleId] [int] NOT NULL IDENTITY(1, 1),
[TenantId] [int] NOT NULL,
[TemplateVersionId] [int] NULL,
[RunOnDemand] [bit] NULL,
[RoleId] [int] NULL,
[ConcurrencyId] [int] NOT NULL CONSTRAINT [DF_TAssignedRole_ConcurrencyId] DEFAULT ((1))
)
GO
ALTER TABLE [dbo].[TAssignedRole] ADD CONSTRAINT [PK_TAssignedRole] PRIMARY KEY NONCLUSTERED  ([AssignedRoleId]) WITH (FILLFACTOR=80)
GO
