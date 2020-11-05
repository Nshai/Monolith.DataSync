CREATE TABLE [dbo].[TTemplateTriggerSet]
(
[TemplateTriggerSetId] [int] NOT NULL IDENTITY(1,1),
[TriggerType] [varchar] (250) NULL,
[TemplateVersionId] int NOT NULL,
[TenantId] [int] NOT NULL,
[ConcurrencyId] [int] NOT NULL CONSTRAINT [DF_TTemplateTriggerSet_ConcurrencyId] DEFAULT ((1)),
[EventSubscriptionId] [int] NULL,
[TriggerList] [xml] NULL
)
GO
ALTER TABLE [dbo].[TTemplateTriggerSet] ADD CONSTRAINT [PK_TTemplateTriggerSet] PRIMARY KEY CLUSTERED  ([TemplateTriggerSetId]) WITH (FILLFACTOR=80)
GO
CREATE NONCLUSTERED INDEX [IDX_TTemplateTriggerSet_TenantId] ON [dbo].[TTemplateTriggerSet] ([TenantId])
GO