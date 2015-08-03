CREATE TABLE [dbo].[TTemplateAudit]
(
[AuditId] [int] NOT NULL IDENTITY(1, 1),
[TemplateId] [int] NOT NULL,
[TenantId] [int] NOT NULL,
[Name] [varchar] (255) COLLATE Latin1_General_CI_AS NOT NULL,
[RelatedTo] [varchar] (100) COLLATE Latin1_General_CI_AS NOT NULL,
[TemplateCategoryId] [int] NULL,
[CreatedDate] [datetime] NULL,
[ConcurrencyId] [int] NOT NULL CONSTRAINT [DF_TTemplateAduit_ConcurrencyId] DEFAULT ((1)),
[StampAction] [char] (1) COLLATE Latin1_General_CI_AS NOT NULL,
[StampDateTime] [datetime] NULL CONSTRAINT [DF_TTemplateAduit_StampDateTime] DEFAULT (getdate()),
[StampUser] [varchar] (255) COLLATE Latin1_General_CI_AS NULL
)
GO
ALTER TABLE [dbo].[TTemplateAudit] ADD CONSTRAINT [PK_TTemplateAudit] PRIMARY KEY NONCLUSTERED  ([AuditId]) WITH (FILLFACTOR=80)
GO
