CREATE TABLE [dbo].[WF_TInstanceHistory]
(
[Id] [int] NOT NULL IDENTITY(1, 1),
[InstanceId] [uniqueidentifier] NOT NULL,
[StepId] [uniqueidentifier] NOT NULL,
[Step] [varchar] (50) COLLATE Latin1_General_CI_AS NOT NULL,
[Data] [varchar] (max) COLLATE Latin1_General_CI_AS NULL,
[IsComplete] [bit] NOT NULL,
[TimestampUtc] [datetime] NOT NULL
)
GO
ALTER TABLE [dbo].[WF_TInstanceHistory] ADD CONSTRAINT [PK_TWorkflowState_1] PRIMARY KEY CLUSTERED  ([Id])
GO
