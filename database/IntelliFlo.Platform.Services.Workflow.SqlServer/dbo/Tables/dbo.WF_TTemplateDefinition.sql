CREATE TABLE [dbo].[WF_TTemplateDefinition]
(
[Id] [uniqueidentifier] NOT NULL,
[Name] [varchar] (255) COLLATE Latin1_General_CI_AS NOT NULL,
[TenantId] [int] NOT NULL,
[Definition] [xml] NOT NULL,
[DateUtc] [datetime] NOT NULL,
[Version] [tinyint] NOT NULL default(0)
)
GO
ALTER TABLE [dbo].[WF_TTemplateDefinition] ADD CONSTRAINT [PK_TWorkflowTemplate] PRIMARY KEY CLUSTERED  ([Id])
GO
