CREATE PROCEDURE [dbo].[SpNGetDelayedTemplates]	
@Version int = 1
AS

select 
Id,
TenantId,
Name,
[Definition],
DateUtc,
[Version]
from
WF_TTemplateDefinition 
where Id in (select distinct t.Id
from vwInstanceStep s with (nolock)
inner join WF_TInstance i on s.InstanceId = i.Id
inner join WF_TTemplateDefinition t on i.TemplateId = t.Id
where s.IsComplete = 0 and s.Step = 'Delay' and t.Version >= @Version)

GO