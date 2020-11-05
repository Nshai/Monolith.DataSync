CREATE TABLE [dbo].[TTemplateVersionAudit]
(
[AuditId] [int] NOT NULL IDENTITY(1, 1),
[TemplateVersionId] [int] NOT NULL,
[TemplateId] [int] NOT NULL,
[TenantId] [int] NOT NULL,
[Guid] [uniqueidentifier] NOT NULL,
[Status] [varchar] (100) NOT NULL,
[CreatedDate] [datetime] NULL,
[OwnerUserId] [int] NULL,
[ApplicableToGroupId] [int] NULL,
[IncludeSubGroups] [bit] NULL,
[Definition] [varchar] (max) NOT NULL,
[ConcurrencyId] [int] NOT NULL CONSTRAINT [DF_TTemplateVersionAudit_ConcurrencyId] DEFAULT ((1)),
[StampAction] [char] (1) NOT NULL,
[StampDateTime] [datetime] NULL CONSTRAINT [DF_TTemplateVersionAudit_StampDateTime] DEFAULT (getdate()),
[StampUser] [varchar] (255) NULL,
[Notes] [varchar] (max) NULL,
[TriggerDefinition] [varchar] (max) NULL
)
GO
ALTER TABLE [dbo].[TTemplateVersionAudit] ADD CONSTRAINT [PK_TTemplateVersionAudit] PRIMARY KEY NONCLUSTERED  ([AuditId]) WITH (FILLFACTOR=80)
GO
