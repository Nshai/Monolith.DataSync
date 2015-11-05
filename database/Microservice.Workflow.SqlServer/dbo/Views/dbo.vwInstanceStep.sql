CREATE view [dbo].[vwInstanceStep]
  as
  SELECT
	  hist.[StepId],
      hist.[InstanceId],
	  instance.TenantId,
	  max(hist.Id) as StepIndex,
	  max(hist.[Step]) as Step,      
	  MAX(CONVERT(int,hist.IsComplete)) as IsComplete,
	  '[' + STUFF((SELECT ', ' + [Data] FROM [WF_TInstanceHistory] WHERE (StepId = hist.StepId and InstanceId = hist.InstanceId) FOR XML PATH(''),TYPE).value('(./text())[1]','VARCHAR(MAX)'), 1, 2, '') + ']' AS Data,
      max(hist.[TimestampUtc]) as TimestampUtc
  FROM [WF_TInstanceHistory] hist
  INNER JOIN [WF_TInstance] instance on hist.InstanceId = instance.Id  
  group by hist.InstanceId, hist.StepId, instance.TenantId
GO
