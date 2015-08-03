CREATE TABLE [dbo].[TTemplateVersion]
(
[TemplateVersionId] [int] NOT NULL IDENTITY(1, 1),
[TenantId] [int] NOT NULL,
[TemplateId] [int] NOT NULL,
[Guid] [uniqueidentifier] NOT NULL CONSTRAINT [DF_TTemplateVersion_Guid] DEFAULT (newid()),
[Status] [varchar] (100) COLLATE Latin1_General_CI_AS NOT NULL,
[CreatedDate] [datetime] NULL CONSTRAINT [DF_TTemplateVersion_CreatedDate] DEFAULT (getdate()),
[OwnerUserId] [int] NULL,
[ApplicableToGroupId] [int] NULL,
[IncludeSubGroups] [bit] NULL,
[Definition] [varchar] (max) COLLATE Latin1_General_CI_AS NOT NULL,
[ConcurrencyId] [int] NOT NULL CONSTRAINT [DF_TTemplateVersion_ConcurrencyId] DEFAULT ((1)),
[Notes] [varchar] (max) COLLATE Latin1_General_CI_AS NULL,
[TriggerDefinition] [varchar] (max) COLLATE Latin1_General_CI_AS NULL
)
GO
ALTER TABLE [dbo].[TTemplateVersion] ADD CONSTRAINT [PK_TTemplateVersion] PRIMARY KEY CLUSTERED  ([TemplateVersionId]) WITH (FILLFACTOR=80)
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_TTemplateVersion_Guid] ON [dbo].[TTemplateVersion] ([Guid])
GO
CREATE NONCLUSTERED INDEX [IDX_TTemplateVersion_TenantId] ON [dbo].[TTemplateVersion] ([TenantId])
GO
create index IX_TTemplateVersion_TemplateId on TTemplateVersion (TemplateId)
GO
CREATE NONCLUSTERED INDEX IX_TTemplateVersion_Status ON [dbo].[TTemplateVersion] ([Status]) INCLUDE ([TemplateId]) 
go