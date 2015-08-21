CREATE TABLE [dbo].[WF_Migration_TTemplateDefinition]
(
[Id] [uniqueidentifier] NOT NULL,
[Name] [varchar] (255) COLLATE Latin1_General_CI_AS NOT NULL,
[TenantId] [int] NOT NULL,
[Definition] [xml] NOT NULL,
[DateUtc] [datetime] NOT NULL
)
GO
