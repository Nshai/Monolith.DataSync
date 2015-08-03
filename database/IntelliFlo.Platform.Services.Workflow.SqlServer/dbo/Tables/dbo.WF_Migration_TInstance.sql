CREATE TABLE [dbo].[WF_Migration_TInstance]
(
[Id] [uniqueidentifier] NOT NULL,
[TemplateId] [uniqueidentifier] NOT NULL,
[CorrelationId] [uniqueidentifier] NOT NULL,
[UserId] [int] NOT NULL,
[TenantId] [int] NOT NULL,
[EntityId] [int] NOT NULL,
[EntityType] [varchar] (50) COLLATE Latin1_General_CI_AS NOT NULL,
[RelatedEntityId] [int] NULL,
[ParentEntityType] [varchar] (50) COLLATE Latin1_General_CI_AS NULL,
[ParentEntityId] [int] NULL,
[CreatedUtc] [datetime] NOT NULL,
[Status] [varchar] (50) COLLATE Latin1_General_CI_AS NOT NULL,
[UniqueId] [uniqueidentifier] NOT NULL,
[NewInstanceId] [uniqueidentifier] NOT NULL
)
GO
