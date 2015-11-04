CREATE TABLE [dbo].[_EventLog]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [ResourceName] VARCHAR(255) NOT NULL, 
	[ResourceVersion] int NOT NULL, 
    [EventType] VARCHAR(255) NOT NULL, 
    [Request] NVARCHAR(MAX) NULL, 
    [Response] NVARCHAR(MAX) NULL, 
    [EventDate] DATETIME NOT NULL, 
    [UserSubject] UNIQUEIDENTIFIER NOT NULL
)
