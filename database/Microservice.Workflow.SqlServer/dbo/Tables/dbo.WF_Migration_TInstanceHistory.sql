CREATE TABLE [dbo].[WF_Migration_TInstanceHistory]
(
[Id] [int] NOT NULL,
[InstanceId] [uniqueidentifier] NOT NULL,
[StepId] [uniqueidentifier] NOT NULL,
[Step] [varchar] (50) NOT NULL,
[Data] [varchar] (max) NULL,
[IsComplete] [bit] NOT NULL,
[TimestampUtc] [datetime] NOT NULL
)
GO
