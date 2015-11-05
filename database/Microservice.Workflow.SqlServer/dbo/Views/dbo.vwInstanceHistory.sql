create view [dbo].[vwInstanceHistory]
  as
  SELECT
	  hist.Id,
	  hist.[StepId],
      hist.[InstanceId],
	  instance.TenantId,
	  hist.Step,
	  hist.Data,
	  hist.IsComplete,
	  hist.TimestampUtc	  
  FROM [WF_TInstanceHistory] hist
  INNER JOIN [WF_TInstance] instance on hist.InstanceId = instance.Id  
GO