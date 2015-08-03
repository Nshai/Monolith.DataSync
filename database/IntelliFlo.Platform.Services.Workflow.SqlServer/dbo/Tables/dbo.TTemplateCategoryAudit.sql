CREATE TABLE [dbo].[TTemplateCategoryAudit]
(
[AuditId] [int] NOT NULL IDENTITY(1, 1),
[TenantId] [int] NOT NULL,
[Name] [varchar] (255) COLLATE Latin1_General_CI_AS NOT NULL,
[IsArchived] [bit] NOT NULL,
[ConcurrencyId] [int] NOT NULL,
[TemplateCategoryId] [int] NOT NULL,
[StampAction] [char] (1) COLLATE Latin1_General_CI_AS NOT NULL,
[StampDateTime] [datetime] NULL CONSTRAINT [DF_TTemplateCategoryAudit_StampDateTime] DEFAULT (getdate()),
[StampUser] [varchar] (255) COLLATE Latin1_General_CI_AS NULL
)
GO
