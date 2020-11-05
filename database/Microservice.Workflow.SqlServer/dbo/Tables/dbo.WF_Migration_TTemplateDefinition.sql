CREATE TABLE [dbo].[WF_Migration_TTemplateDefinition]
(
[Id] [uniqueidentifier] NOT NULL,
[Name] [varchar] (255) NOT NULL,
[TenantId] [int] NOT NULL,
[Definition] [xml] NOT NULL,
[DateUtc] [datetime] NOT NULL
)
GO
