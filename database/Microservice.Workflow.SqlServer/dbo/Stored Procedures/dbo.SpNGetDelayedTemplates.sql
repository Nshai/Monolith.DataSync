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
where s.IsComplete = 0 and s.Step = 'Delay' and t.Version >= @Version
and cast(SUBSTRING(Data, CHARINDEX('DelayUntil":"', Data, 0) + 13, 19) as datetime) < dateadd(day, 7, getdate())
and cast(SUBSTRING(Data, CHARINDEX('DelayUntil":"', Data, 0) + 13, 19) as datetime) >= getdate())

GO