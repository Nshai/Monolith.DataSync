CREATE VIEW [dbo].[vwTemplate]
	AS 
	SELECT 
	t.TemplateId,
	t.TenantId,
	t.Name,
	t.RelatedTo,
	t.TemplateCategoryId,
	t.CreatedDate,
	t.ConcurrencyId,
	v.[Guid] as [Guid],
	v.[Status] as [Status]
	FROM TTemplate t
	INNER JOIN TTemplateVersion v on t.TemplateId = v.TemplateId
