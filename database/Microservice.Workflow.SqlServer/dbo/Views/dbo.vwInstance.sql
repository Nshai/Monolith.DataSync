create view [dbo].[vwInstance]
  as
  SELECT
	  i.Id, 
	  i.TemplateId, 
	  i.CorrelationId, 
	  i.UserId, 
	  i.TenantId, 
	  t.TenantId 'TemplateTenantId',
	  i.EntityId, 
	  i.EntityType, 
	  i.RelatedEntityId, 
	  i.ParentEntityType,
	  i.ParentEntityId, 
	  i.CreatedUtc, 
	  i.[Status], 
	  i.UniqueId, 
	  i.[Version]
  FROM [WF_TInstance] i
  INNER JOIN [WF_TTemplateDefinition] t on i.TemplateId = t.Id  
GO