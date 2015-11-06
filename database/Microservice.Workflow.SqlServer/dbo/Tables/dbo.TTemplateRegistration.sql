CREATE TABLE [dbo].[TTemplateRegistration]
(
[Id] [int] NOT NULL IDENTITY(1, 1),
[TenantId] [int] NOT NULL,
[Identifier] [varchar] (50) COLLATE Latin1_General_CI_AS NOT NULL,
[TemplateId] [uniqueidentifier] NOT NULL,
[ConcurrencyId] [int] NOT NULL CONSTRAINT [DF_TTemplateRegistration_ConcurrencyId] DEFAULT ((1))
)
GO
ALTER TABLE [dbo].[TTemplateRegistration] ADD CONSTRAINT [PK_TTemplateRegistration] PRIMARY KEY CLUSTERED  ([Id]) WITH (FILLFACTOR=80)
GO
CREATE NONCLUSTERED INDEX [IDX_TTemplateRegistration_TenantIdIdentifier] ON [dbo].[TTemplateRegistration] ([TenantId], [Identifier])
GO