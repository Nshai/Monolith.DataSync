CREATE view [dbo].[vwInstanceStep]
  as
  SELECT
	  [StepId],
      [InstanceId],
	  max(Id) as StepIndex,
	  max([Step]) as Step,      
	  MAX(CONVERT(int,IsComplete)) as IsComplete,
	  '[' + STUFF((SELECT ', ' + [Data] FROM [WF_TInstanceHistory] WHERE (StepId = hist.StepId and InstanceId = hist.InstanceId) FOR XML PATH(''),TYPE).value('(./text())[1]','VARCHAR(MAX)'), 1, 2, '') + ']' AS Data,
      max([TimestampUtc]) as TimestampUtc
  FROM [WF_TInstanceHistory] hist
  group by InstanceId, StepId
GO
