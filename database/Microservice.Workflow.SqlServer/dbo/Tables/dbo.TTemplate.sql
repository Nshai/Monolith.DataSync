CREATE TABLE [dbo].[TTemplate]
(
[TemplateId] [int] NOT NULL IDENTITY(1, 1),
[TenantId] [int] NOT NULL,
[Name] [varchar] (255) NOT NULL,
[RelatedTo] [varchar] (100) NOT NULL,
[TemplateCategoryId] [int] NULL,
[CreatedDate] [datetime] NULL CONSTRAINT [DF_TTemplate_CreatedDate] DEFAULT (getdate()),
[ConcurrencyId] [int] NOT NULL CONSTRAINT [DF_TTemplate_ConcurrencyId] DEFAULT ((1))
)
GO
ALTER TABLE [dbo].[TTemplate] ADD CONSTRAINT [PK_TTemplate] PRIMARY KEY CLUSTERED  ([TemplateId]) WITH (FILLFACTOR=80)
GO
CREATE NONCLUSTERED INDEX [IDX_TTemplate_TenantId] ON [dbo].[TTemplate] ([TenantId])
GO
