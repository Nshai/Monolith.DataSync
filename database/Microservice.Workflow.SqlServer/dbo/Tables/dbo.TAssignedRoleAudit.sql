CREATE TABLE [dbo].[TAssignedRoleAudit]
(
[AuditId] [int] NOT NULL IDENTITY(1, 1),
[TenantId] [int] NOT NULL,
[TemplateVersionId] [int] NULL,
[RunOnDemand] [bit] NULL,
[RoleId] [int] NULL,
[ConcurrencyId] [int] NOT NULL CONSTRAINT [DF_TAssignedRoleAudit_ConcurrencyId] DEFAULT ((1)),
[AssignedRoleId] [int] NOT NULL,
[StampAction] [char] (1) NOT NULL,
[StampDateTime] [datetime] NULL CONSTRAINT [DF_TAssignedRoleAudit_StampDateTime] DEFAULT (getdate()),
[StampUser] [varchar] (255) NULL
)
GO
ALTER TABLE [dbo].[TAssignedRoleAudit] ADD CONSTRAINT [PK_TAssignedRoleAudit] PRIMARY KEY NONCLUSTERED  ([AuditId]) WITH (FILLFACTOR=80)
GO



