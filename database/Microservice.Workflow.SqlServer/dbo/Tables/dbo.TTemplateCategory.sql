CREATE TABLE [dbo].[TTemplateCategory]
(
[TemplateCategoryId] [int] NOT NULL IDENTITY(1, 1),
[TenantId] [int] NOT NULL,
[Name] [varchar] (255) NOT NULL,
[IsArchived] [bit] NOT NULL CONSTRAINT [DF_TTemplate_IsArchived] DEFAULT ((0)),
[ConcurrencyId] [int] NOT NULL CONSTRAINT [DF_TTemplateCategory_ConcurrencyId] DEFAULT ((1))
)
GO
ALTER TABLE [dbo].[TTemplateCategory] ADD CONSTRAINT [PK_TTemplateCategory] PRIMARY KEY CLUSTERED  ([TemplateCategoryId]) WITH (FILLFACTOR=80)
GO
CREATE NONCLUSTERED INDEX [IDX_TTemplateCategory_TenantId] ON [dbo].[TTemplateCategory] ([TenantId])
GO
