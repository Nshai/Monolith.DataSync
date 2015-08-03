CREATE TABLE [dbo].[WF_Migration_TInstanceHistory]
(
[Id] [int] NOT NULL,
[InstanceId] [uniqueidentifier] NOT NULL,
[StepId] [uniqueidentifier] NOT NULL,
[Step] [varchar] (50) COLLATE Latin1_General_CI_AS NOT NULL,
[Data] [varchar] (max) COLLATE Latin1_General_CI_AS NULL,
[IsComplete] [bit] NOT NULL,
[TimestampUtc] [datetime] NOT NULL
)
GO
