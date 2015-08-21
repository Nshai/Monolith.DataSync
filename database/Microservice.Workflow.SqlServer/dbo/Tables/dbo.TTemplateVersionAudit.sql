CREATE TABLE [dbo].[TTemplateVersionAudit]
(
[AuditId] [int] NOT NULL IDENTITY(1, 1),
[TemplateVersionId] [int] NOT NULL,
[TemplateId] [int] NOT NULL,
[TenantId] [int] NOT NULL,
[Guid] [uniqueidentifier] NOT NULL,
[Status] [varchar] (100) COLLATE Latin1_General_CI_AS NOT NULL,
[CreatedDate] [datetime] NULL,
[OwnerUserId] [int] NULL,
[ApplicableToGroupId] [int] NULL,
[IncludeSubGroups] [bit] NULL,
[Definition] [varchar] (max) COLLATE Latin1_General_CI_AS NOT NULL,
[ConcurrencyId] [int] NOT NULL CONSTRAINT [DF_TTemplateVersionAudit_ConcurrencyId] DEFAULT ((1)),
[StampAction] [char] (1) COLLATE Latin1_General_CI_AS NOT NULL,
[StampDateTime] [datetime] NULL CONSTRAINT [DF_TTemplateVersionAudit_StampDateTime] DEFAULT (getdate()),
[StampUser] [varchar] (255) COLLATE Latin1_General_CI_AS NULL,
[Notes] [varchar] (max) COLLATE Latin1_General_CI_AS NULL,
[TriggerDefinition] [varchar] (max) COLLATE Latin1_General_CI_AS NULL
)
GO
ALTER TABLE [dbo].[TTemplateVersionAudit] ADD CONSTRAINT [PK_TTemplateVersionAudit] PRIMARY KEY NONCLUSTERED  ([AuditId]) WITH (FILLFACTOR=80)
GO
