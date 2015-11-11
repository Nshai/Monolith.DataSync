CREATE TABLE [dbo].[WF_TInstance]
(
[Id] [uniqueidentifier] NOT NULL,
[TemplateId] [uniqueidentifier] NOT NULL,
[CorrelationId] [uniqueidentifier] NOT NULL DEFAULT (newid()),
[UserId] [int] NOT NULL,
[TenantId] [int] NOT NULL,
[EntityId] [int] NOT NULL,
[EntityType] [varchar] (50) COLLATE Latin1_General_CI_AS NOT NULL,
[RelatedEntityId] [int] NULL,
[ParentEntityType] [varchar] (50) COLLATE Latin1_General_CI_AS NULL,
[ParentEntityId] [int] NULL,
[CreatedUtc] [datetime] NOT NULL,
[Status] [varchar] (50) COLLATE Latin1_General_CI_AS NOT NULL,
[UniqueId] [uniqueidentifier] NOT NULL CONSTRAINT [DF_TInstance_UniqueId] DEFAULT (newid()),
[Version] [tinyint] NOT NULL default(0)
)
GO
ALTER TABLE [dbo].[WF_TInstance] ADD CONSTRAINT [PK_WF_TInstance] PRIMARY KEY CLUSTERED  ([Id])
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_WF_TInstance] ON [dbo].[WF_TInstance] ([TemplateId], [EntityId], [RelatedEntityId], [UniqueId])
GO
CREATE NONCLUSTERED INDEX IX_WF_TInstance_Id_TemplateId on WF_TInstance (Id,TemplateId)
GO