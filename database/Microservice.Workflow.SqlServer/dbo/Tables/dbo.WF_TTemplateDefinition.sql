CREATE TABLE [dbo].[WF_TTemplateDefinition]
(
[Id] [uniqueidentifier] NOT NULL,
[Name] [varchar] (255) NOT NULL,
[TenantId] [int] NOT NULL,
[Definition] [xml] NOT NULL,
[DateUtc] [datetime] NOT NULL,
[Version] [tinyint] NOT NULL default(0)
)
GO
ALTER TABLE [dbo].[WF_TTemplateDefinition] ADD CONSTRAINT [PK_TWorkflowTemplate] PRIMARY KEY CLUSTERED  ([Id])
GO
CREATE NONCLUSTERED INDEX IX_WF_TTemplateDefinition_TenantId
ON [dbo].[WF_TTemplateDefinition] ([TenantId])
GO