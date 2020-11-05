CREATE TABLE [dbo].[TTemplateTriggerSetAudit]
(
[AuditId] [int] NOT NULL IDENTITY(1, 1) NOT FOR REPLICATION,
[TemplateTriggerSetId] [int] NOT NULL,
[TriggerType] [varchar] (250) NULL,
[TemplateVersionId] int NOT NULL,
[TenantId] [int] NOT NULL,
[ConcurrencyId] [int] NOT NULL CONSTRAINT [DF_TTemplateTriggerSetAudit_ConcurrencyId] DEFAULT ((1)),
[StampAction] [char] (1) NOT NULL,
[StampDateTime] [datetime] NULL CONSTRAINT [DF_TTemplateTriggerSetAudit_StampDateTime] DEFAULT (getdate()),
[StampUser] [varchar] (255) NULL,
[EventSubscriptionId] [int] NULL,
[TriggerList] [xml] NULL
)
GO
ALTER TABLE [dbo].[TTemplateTriggerSetAudit] ADD CONSTRAINT [PK_TTemplateTriggerSetAudit] PRIMARY KEY NONCLUSTERED  ([AuditId])
GO