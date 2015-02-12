/****** Object:  Table [b_Scheduler]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_Scheduler', N'U') IS NULL
CREATE TABLE [b_Scheduler]
(
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](128) NOT NULL CONSTRAINT [DF__b_Scheduler__name]  DEFAULT (''),
	[classname] [nvarchar](128) NOT NULL CONSTRAINT [DF__b_Scheduler__classname]  DEFAULT (''),
	[assembly] [nvarchar](128) NOT NULL CONSTRAINT [DF__b_Scheduler__assembly]  DEFAULT (''),
	[parameters] [nvarchar](max) NOT NULL CONSTRAINT [DF_b_Scheduler_parameters]  DEFAULT (''),
	[starttime] [datetime] NOT NULL CONSTRAINT [DF__b_Scheduler__starttime]  DEFAULT (dateadd(minute,(15),getdate())),
	[periodic] [bit] NOT NULL CONSTRAINT [DF__b_Scheduler__periodic]  DEFAULT ((0)),
	[period] [int] NOT NULL CONSTRAINT [DF__b_Scheduler__period]  DEFAULT ((0)),
	[active] [bit] NOT NULL CONSTRAINT [DF__b_Scheduler__active]  DEFAULT ((0)),
	[last_updated] [datetime] NOT NULL CONSTRAINT [DF_b_Scheduler_last_updated]  DEFAULT (getdate()),
	[lock_release_utc] [datetime] NOT NULL CONSTRAINT DF_b_Scheduler_lock_release_utc DEFAULT (getutcdate()),
	[last_locked_by] [nvarchar](128) NULL,
	CONSTRAINT [PK__b_scheduler__id] PRIMARY KEY CLUSTERED ([id] ASC)
)
GO
IF OBJECT_ID(N'b_Scheduler', N'U') IS NOT NULL
AND NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_scheduler_active')
CREATE NONCLUSTERED INDEX [IX_b_scheduler_active] ON [b_Scheduler] 
(
	[active] ASC
)
GO
IF OBJECT_ID(N'b_Scheduler', N'U') IS NOT NULL
AND NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_scheduler_name')
CREATE UNIQUE NONCLUSTERED INDEX [IX_b_scheduler_name] ON [b_Scheduler] 
(
	[name] ASC
)
GO
IF OBJECT_ID(N'b_Scheduler', N'U') IS NOT NULL
AND NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_scheduler_starttime')
CREATE NONCLUSTERED INDEX [IX_b_scheduler_starttime] ON [b_Scheduler] 
(
	[starttime] ASC
)
GO


/****** Object:  Table [b_Log]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_Log', N'U') IS NULL
CREATE TABLE [b_Log]
(
	[id] [int] IDENTITY(1,1) NOT NULL CONSTRAINT [PK_b_Log] PRIMARY KEY CLUSTERED,
	[type] [tinyint] NOT NULL CONSTRAINT [DF_b_Log_type]  DEFAULT ((0)),
	[code] [int] NOT NULL CONSTRAINT [DF_b_Log_code]  DEFAULT ((0)),
	[source] [nvarchar](256) NOT NULL CONSTRAINT [DF_b_Log_source_1]  DEFAULT (''),
	[title] [nvarchar](512) NOT NULL CONSTRAINT [DF_b_Log_source]  DEFAULT ('Unknown'),
	[message] [nvarchar](max) NOT NULL CONSTRAINT [DF_b_Log_message]  DEFAULT (''),
	[occured] [datetime] NOT NULL CONSTRAINT [DF_b_Log_occured]  DEFAULT (getdate()),
	[text_type] [char](4) NOT NULL CONSTRAINT [DF_b_Log_body_type]  DEFAULT ('text')
)
GO


/****** Object:  Table [b_Modules]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_Modules', N'U') IS NULL
CREATE TABLE [b_Modules]
(
	[ID] [varchar](128) NOT NULL,
	[Type] [nvarchar](256) NULL,
	[DateAdd] [datetime] NOT NULL,
	CONSTRAINT [PK_b_Modules] PRIMARY KEY CLUSTERED ([ID] ASC)
)
GO


/****** Object:  Table [b_Language]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_Language', N'U') IS NULL
CREATE TABLE [b_Language]
(
	[ID] [char](2) NOT NULL,
	[Sort] [int] NOT NULL CONSTRAINT [DF_b_Language_Sort]  DEFAULT ((100)),
	[Default] [bit] NOT NULL CONSTRAINT [DF_b_Language_Default]  DEFAULT ((0)),
	[Active] [bit] NOT NULL CONSTRAINT [DF_b_Language_Active]  DEFAULT ((1)),
	[Name] [nvarchar](50) NOT NULL,
	[Culture] [varchar](50) NOT NULL CONSTRAINT [DF_b_Language_Culture]  DEFAULT ('en-US'),
	CONSTRAINT [PK_b_Language] PRIMARY KEY CLUSTERED ([ID] ASC)
)
GO


/****** Object:  Table [b_Site]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_Site', N'U') IS NULL
CREATE TABLE [b_Site]
(
	[ID] [varchar](50) NOT NULL,
	[Sort] [int] NOT NULL,
	[Default] [bit] NOT NULL,
	[Active] [bit] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Directory] [nvarchar](50) NOT NULL,
	[LanguageId] [char](2) NOT NULL,
	[DocRoot] [nvarchar](255) NULL,
	[DomainLimited] [bit] NOT NULL,
	[ServerName] [varchar](255) NULL,
	[SiteName] [nvarchar](255) NULL,
	[Email] [varchar](255) NULL,
	[Culture] [varchar](50) NOT NULL CONSTRAINT [DF_b_Site_Culture]  DEFAULT ('en-US'),
	[RemapDirectory] NVARCHAR(255) CONSTRAINT DF_b_Site_RemapDirectory NULL,
	CONSTRAINT [PK_b_Site] PRIMARY KEY CLUSTERED ([ID] ASC),
	CONSTRAINT [FK_b_Site_b_Language] FOREIGN KEY([LanguageId]) REFERENCES [b_Language] ([ID])
)
GO


/****** Object:  Table [b_Site_Domain]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_Site_Domain', N'U') IS NULL
CREATE TABLE [b_Site_Domain]
(
	[ID] [varchar](50) NOT NULL,
	[Domain] [varchar](255) NOT NULL
)
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_Site_Domain_ID')
	CREATE CLUSTERED INDEX IX_b_Site_Domain_ID ON b_Site_Domain(ID ASC);
GO

/****** Object:  Table [b_Template_Condition]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_Template_Condition', N'U') IS NULL
CREATE TABLE [b_Template_Condition]
(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SiteId] [varchar](50) NOT NULL,
	[Condition] [nvarchar](255) NOT NULL,
	[ConditionType] [int] NOT NULL,
	[Sort] [int] NOT NULL,
	[Template] [nvarchar](255) NOT NULL,
	CONSTRAINT [PK_b_Template_Condition] PRIMARY KEY CLUSTERED ([ID] ASC)
)
GO


/****** Object:  Table [b_MailerTemplates]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_MailerTemplates', N'U') IS NULL
CREATE TABLE [b_MailerTemplates]
(
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](128) NOT NULL,
	[last_updated] [datetime] NOT NULL CONSTRAINT [DF_b_MailerTemplates_lastupdated]  DEFAULT (getdate()),
	[active] [bit] NOT NULL CONSTRAINT [DF_b_MailerTemplates_active]  DEFAULT ((1)),
	[email_from] [nvarchar](256) NOT NULL CONSTRAINT [DF_b_MailerTemplates_email_from]  DEFAULT (''),
	[email_to] [nvarchar](256) NOT NULL CONSTRAINT [DF_b_MailerTemplates_email_to]  DEFAULT (''),
	[subject] [nvarchar](256) NOT NULL CONSTRAINT [DF_b_MailerTemplates_subject]  DEFAULT (''),
	[message] [nvarchar](MAX) NOT NULL CONSTRAINT [DF_b_MailerTemplates_message]  DEFAULT (''),
	[body_type] [char](4) NOT NULL CONSTRAINT [DF_b_MailerTemplates_body_type]  DEFAULT ('text'),
	[bcc] [nvarchar](MAX) NOT NULL CONSTRAINT [DF_b_MailerTemplates_bcc]  DEFAULT (''),
	CONSTRAINT [PK_b_MailerTemplates] PRIMARY KEY CLUSTERED ([id] ASC)
)
GO
IF OBJECT_ID(N'b_MailerTemplates', N'U') IS NOT NULL
AND NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_MailerTemplates_Name')
CREATE NONCLUSTERED INDEX [IX_b_MailerTemplates_Name] ON [b_MailerTemplates] 
(
	[name] ASC
)
GO


/****** Object:  Table [b_MailerTemplates_Sites]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_MailerTemplates_Sites', N'U') IS NULL
CREATE TABLE [b_MailerTemplates_Sites]
(
	[templateId] [int] NOT NULL,
	[siteId] [varchar](50) NOT NULL CONSTRAINT [DF_b_MailerTemplates_Sites_siteId]  DEFAULT (''),
	CONSTRAINT [PK_b_MailerTemplates_Sites] PRIMARY KEY CLUSTERED ([templateId] ASC, [siteId] ASC)
)
GO
IF OBJECT_ID(N'b_MailerTemplates_Sites', N'U') IS NOT NULL
AND NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_MailerTemplates_Sites_site')
CREATE NONCLUSTERED INDEX [IX_b_MailerTemplates_Sites_site] ON [b_MailerTemplates_Sites] 
(
	[siteId] ASC
)
GO
IF OBJECT_ID(N'b_MailerTemplates_Sites', N'U') IS NOT NULL
AND NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_MailerTemplates_Sites_template')
CREATE NONCLUSTERED INDEX [IX_b_MailerTemplates_Sites_template] ON [b_MailerTemplates_Sites] 
(
	[templateId] ASC
)
GO


/****** Object:  Table [b_MailerEvents]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_MailerEvents', N'U') IS NULL
CREATE TABLE [b_MailerEvents]
(
	[id] [int] IDENTITY(1,1) NOT NULL,
	[template] [nvarchar](128) NOT NULL,
	[site] [nvarchar](128) NOT NULL,
	[parameters] [nvarchar](max) NOT NULL,
	[duplicate] [char](1) NOT NULL CONSTRAINT [DF_b_MailerEvents_duplicates]  DEFAULT ('Y'),
	[status] [char](1) NOT NULL CONSTRAINT [DF_b_MailerEvents_status]  DEFAULT ('N'),
	[last_updated] [datetime] NOT NULL CONSTRAINT [DF_b_MailerEvents_last_updated]  DEFAULT (getdate()),
	[template_id] [int] NOT NULL CONSTRAINT [DF_b_MailerEvents_template_id]  DEFAULT ((0)),
	[lock_release_utc] datetime NOT NULL CONSTRAINT DF_b_MailerEvents_lock_release_utc DEFAULT (''),
	[last_locked_by] nvarchar(128) NULL,
	CONSTRAINT [PK_b_MailerEvents] PRIMARY KEY CLUSTERED ([id] ASC)
)
GO


/****** Object:  Table [b_Users]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_Users', N'U') IS NULL
CREATE TABLE [b_Users]
(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](256) NOT NULL,
	[LoweredUserName] [nvarchar](256) NOT NULL,
	[Password] [nvarchar](128) NOT NULL,
	[PasswordFormat] [int] NOT NULL CONSTRAINT [DF_b_users_PASSWORD_FORMAT]  DEFAULT ((0)),
	[PasswordSalt] [nvarchar](128) NOT NULL,
	[ProviderName] [nvarchar](128) NOT NULL,
	[LoweredProviderName] [nvarchar](128) NOT NULL,
	[LastActivityDate] [datetime] NOT NULL,
	[Email] [nvarchar](256) NULL,
	[LoweredEmail] [nvarchar](256) NULL,
	[PasswordQuestion] [nvarchar](256) NULL,
	[PasswordAnswer] [nvarchar](128) NULL,
	[CheckWord] [nvarchar](50) NULL,
	[CheckWordWindowStart] [datetime] NULL,
	[IsApproved] [bit] NOT NULL,
	[IsLockedOut] [bit] NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[LastLoginDate] [datetime] NOT NULL,
	[LastPasswordChangedDate] [datetime] NOT NULL,
	[LastLockoutDate] [datetime] NOT NULL,
	[FailedPasswordAttemptCount] [int] NOT NULL,
	[FailedPasswordAttemptWindowStart] [datetime] NOT NULL,
	[FailedPasswordAnswerAttemptCount] [int] NOT NULL,
	[FailedPasswordAnswerAttemptWindowStart] [datetime] NOT NULL,
	[Comment] [ntext] NULL,
	[FirstName] [nvarchar](50) NULL,
	[SecondName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
	[SiteID] [varchar](50) NULL,
	[BirthdayDate] [datetime] NULL,
	[DisplayName] [nvarchar](256) NULL CONSTRAINT [DF_b_Users_DisplayName]  DEFAULT (''),
	[ImageId] [int] NULL,
	[Gender] [char](1) NULL,
	[ActivationToken] [nvarchar](15) NULL,
	CONSTRAINT [PK_b_users] PRIMARY KEY NONCLUSTERED ([ID] ASC),
	CONSTRAINT [FK_b_Users_b_Site] FOREIGN KEY([SiteID]) REFERENCES [b_Site] ([ID])
)
GO
IF OBJECT_ID(N'b_Users', N'U') IS NOT NULL
AND NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_Users')
CREATE UNIQUE CLUSTERED INDEX [IX_b_Users] ON [b_Users] 
(
	[LoweredProviderName] ASC,
	[LoweredUserName] ASC
)
GO


/****** Object:  Table [b_Roles]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_Roles', N'U') IS NULL
CREATE TABLE [b_Roles]
(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RoleName] [nvarchar](128) NOT NULL,
	[LoweredRoleName] [nvarchar](128) NOT NULL,
	[Active] [bit] NOT NULL CONSTRAINT [DF_b_roles_Active]  DEFAULT ((1)),
	[Comment] [ntext] NULL,
	[Policy] [xml] NULL,
	[EffectivePolicy] [xml] NULL,
	[Title] [nvarchar](255) NULL,
	CONSTRAINT [PK_b_roles] PRIMARY KEY NONCLUSTERED ([ID] ASC)
)
GO
IF OBJECT_ID(N'b_Roles', N'U') IS NOT NULL
AND NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_Roles')
CREATE UNIQUE CLUSTERED INDEX [IX_b_Roles] ON [b_Roles] 
(
	[LoweredRoleName] ASC
)
GO


/****** Object:  Table [b_RolesTasks]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_RolesTasks', N'U') IS NULL
CREATE TABLE [b_RolesTasks]
(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TaskName] [nvarchar](128) NOT NULL,
	[LoweredTaskName] [nvarchar](128) NOT NULL,
	[Comment] [ntext] NULL,
	[Title] [nvarchar](255) NULL,
	CONSTRAINT [PK_b_RolesTasks] PRIMARY KEY NONCLUSTERED ([ID] ASC)
)
GO
IF OBJECT_ID(N'b_RolesTasks', N'U') IS NOT NULL
AND NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_RolesTasks')
CREATE UNIQUE CLUSTERED INDEX [IX_b_RolesTasks] ON [b_RolesTasks] 
(
	[LoweredTaskName] ASC
)
GO


/****** Object:  Table [b_RolesOperations]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_RolesOperations', N'U') IS NULL
CREATE TABLE [b_RolesOperations]
(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[OperationName] [nvarchar](128) NOT NULL,
	[LoweredOperationName] [nvarchar](128) NOT NULL,
	[OperationType] [nvarchar](128) NULL,
	[ModuleID] [varchar](50) NOT NULL CONSTRAINT [DF_b_RolesOperations_ModuleID]  DEFAULT (''),
	[Comment] [ntext] NULL,
	CONSTRAINT [PK_b_RolesOperations] PRIMARY KEY NONCLUSTERED ([ID] ASC)
)
GO
IF OBJECT_ID(N'b_RolesOperations', N'U') IS NOT NULL
AND NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_RolesOperations')
CREATE UNIQUE CLUSTERED INDEX [IX_b_RolesOperations] ON [b_RolesOperations] 
(
	[LoweredOperationName] ASC
)
GO


/****** Object:  Table [b_RolesTasksInTasks]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_RolesTasksInTasks', N'U') IS NULL
CREATE TABLE [b_RolesTasksInTasks]
(
	[TaskID] [int] NOT NULL,
	[SubTaskID] [int] NOT NULL,
	CONSTRAINT [PK_b_RolesTasksInTasks] PRIMARY KEY CLUSTERED ([TaskID] ASC, [SubTaskID] ASC),
	CONSTRAINT [FK_b_RolesTasksInTasks_b_RolesTasks] FOREIGN KEY([SubTaskID]) REFERENCES [b_RolesTasks] ([ID]),
	CONSTRAINT [FK_b_RolesTasksInTasks_b_RolesTasksInTasks] FOREIGN KEY([TaskID]) REFERENCES [b_RolesTasks] ([ID])
)
GO


/****** Object:  Table [b_RolesTasksInOperations]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_RolesTasksInOperations', N'U') IS NULL
CREATE TABLE [b_RolesTasksInOperations]
(
	[TaskID] [int] NOT NULL,
	[OperationID] [int] NOT NULL,
	CONSTRAINT [PK_b_RolesTasksInOperations] PRIMARY KEY CLUSTERED ([TaskID] ASC, [OperationID] ASC),
	CONSTRAINT [FK_b_RolesTasksInOperations_b_RolesOperations] FOREIGN KEY([OperationID]) REFERENCES [b_RolesOperations] ([ID]),
	CONSTRAINT [FK_b_RolesTasksInOperations_b_RolesTasks] FOREIGN KEY([TaskID]) REFERENCES [b_RolesTasks] ([ID])
)
GO


/****** Object:  Table [b_RolesInTasks]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_RolesInTasks', N'U') IS NULL
CREATE TABLE [b_RolesInTasks]
(
	[RoleID] [int] NOT NULL,
	[TaskID] [int] NOT NULL,
	[ModuleID] [varchar](50) NOT NULL CONSTRAINT [DF_b_RolesInTasks_ModuleID]  DEFAULT (''),
	[ExternalID] [varchar](50) NOT NULL CONSTRAINT [DF_b_RolesInTasks_ExternalID]  DEFAULT ((0)),
	CONSTRAINT [PK_b_RolesInTasks] PRIMARY KEY CLUSTERED ([RoleID] ASC, [TaskID] ASC, [ExternalID] ASC,	[ModuleID] ASC),
	CONSTRAINT [FK_b_RolesInTasks_b_roles] FOREIGN KEY([RoleID]) REFERENCES [b_Roles] ([ID]),
	CONSTRAINT [FK_b_RolesInTasks_b_RolesTasks] FOREIGN KEY([TaskID]) REFERENCES [b_RolesTasks] ([ID])
)
GO


/****** Object:  Table [b_RolesInOperations]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_RolesInOperations', N'U') IS NULL
CREATE TABLE [b_RolesInOperations]
(
	[RoleID] [int] NOT NULL,
	[OperationID] [int] NOT NULL,
	[ModuleID] [varchar](50) NOT NULL  CONSTRAINT [DF_b_RolesInOperations_ModuleID]  DEFAULT (''),
	[ExternalID] [varchar](50) NOT NULL CONSTRAINT [DF_b_RolesInOperations_ExternalID]  DEFAULT ((0)),
	CONSTRAINT [PK_b_RolesInOperations] PRIMARY KEY CLUSTERED ([RoleID] ASC, [OperationID] ASC,	[ExternalID] ASC, [ModuleID] ASC),
	CONSTRAINT [FK_b_RolesInOperations_b_roles] FOREIGN KEY([RoleID]) REFERENCES [b_Roles] ([ID]),
	CONSTRAINT [FK_b_RolesInOperations_b_RolesOperations] FOREIGN KEY([OperationID]) REFERENCES [b_RolesOperations] ([ID])
)
GO


/****** Object:  Table [b_RolesInRoles]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_RolesInRoles', N'U') IS NULL
CREATE TABLE [b_RolesInRoles]
(
	[RoleID] [int] NOT NULL,
	[SubRoleID] [int] NOT NULL,
	CONSTRAINT [PK_b_RolesInRoles] PRIMARY KEY CLUSTERED ([RoleID] ASC, [SubRoleID] ASC),
	CONSTRAINT [FK_b_RolesInRoles_b_roles] FOREIGN KEY([RoleID]) REFERENCES [b_Roles] ([ID]),
	CONSTRAINT [FK_b_RolesInRoles_b_roles1] FOREIGN KEY([SubRoleID]) REFERENCES [b_Roles] ([ID])
)
GO


/****** Object:  Table [b_UsersInRoles]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_UsersInRoles', N'U') IS NULL
CREATE TABLE [b_UsersInRoles]
(
	[UserID] [int] NOT NULL,
	[RoleID] [int] NOT NULL,
	[ActiveFrom] [datetime] NULL,
	[ActiveTo] [datetime] NULL,
	CONSTRAINT [PK_b_UsersInRoles] PRIMARY KEY CLUSTERED ([UserID] ASC,	[RoleID] ASC),
	CONSTRAINT [FK_b_UsersInRoles_b_roles] FOREIGN KEY([RoleID]) REFERENCES [b_Roles] ([ID]),
	CONSTRAINT [FK_b_UsersInRoles_b_users] FOREIGN KEY([UserID]) REFERENCES [b_Users] ([ID])
)
GO

/****** Object:  Table [b_RolesCache]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_RolesCache', N'U') IS NULL
CREATE TABLE [b_RolesCache]
(
	[roleId] [int] NOT NULL,
	[subRoleId] [int] NOT NULL,
	CONSTRAINT [PK_b_RolesCache] PRIMARY KEY CLUSTERED ([roleId] ASC, [subRoleId] ASC)
)
GO


/****** Object:  Table [b_RolesOperationsCache]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_RolesOperationsCache', N'U') IS NULL
CREATE TABLE [b_RolesOperationsCache]
(
	[RoleID] [int] NOT NULL,
	[OperationID] [int] NOT NULL,
	[ModuleID] [varchar](50) NOT NULL CONSTRAINT [DF_b_RolesOperationsCache_ModuleID]  DEFAULT (''),
	[ExternalID] [varchar](50) NOT NULL,
	CONSTRAINT [PK_b_RolesOperationsCache] PRIMARY KEY CLUSTERED ([RoleID] ASC, [OperationID] ASC, [ExternalID] ASC, [ModuleID] ASC)
)
GO


/****** Object:  Table [b_ComponentEvents]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_ComponentEvents', N'U') IS NULL
CREATE TABLE [b_ComponentEvents]
(
	[message] [nvarchar](128) NOT NULL,
	[siteId] [varchar](50) NOT NULL,
	[path] [nvarchar](256) NOT NULL,
	[componentName] [varchar](128) NOT NULL,
	[parameters] [nvarchar](max) NULL
)
GO
IF OBJECT_ID(N'b_ComponentEvents', N'U') IS NOT NULL
AND NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_ComponentEvents')
CREATE CLUSTERED INDEX [IX_b_ComponentEvents] ON [b_ComponentEvents] 
(
	[message] ASC
)
GO

/****** Object:  Table [b_File]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_File', N'U') IS NULL
CREATE TABLE [b_File]
(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UpdateDate] [datetime] NOT NULL CONSTRAINT [DF_b_File_UpdateDate]  DEFAULT (getdate()),
	[ModuleId] [varchar](128) NOT NULL,
	[Height] [int] NULL,
	[Width] [int] NULL,
	[FileSize] [int] NOT NULL,
	[ContentType] [varchar](255) NOT NULL CONSTRAINT [DF_b_File_ContentType]  DEFAULT ('IMAGE'),
	[Folder] [varchar](255) NULL,
	[FileName] [nvarchar](255) NOT NULL,
	[FileNameOriginal] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[StorageId] INT CONSTRAINT DF_b_File_StorageId DEFAULT (0),
	[TempGuid] NVARCHAR(64) NOT NULL CONSTRAINT DF_b_File_TempGuid DEFAULT N'',
	CONSTRAINT [PK_b_File] PRIMARY KEY CLUSTERED ([ID] ASC)
)
GO

IF OBJECT_ID(N'b_File', N'U') IS NOT NULL AND NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_File_TempGuid')
CREATE NONCLUSTERED INDEX IX_b_File_TempGuid ON b_File
(
	TempGuid ASC
)
GO

/****** Object:  Table [b_FileAction]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_FileAction', N'U') IS NULL
CREATE TABLE [b_FileAction]
(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FileName] [nvarchar](255) NOT NULL,
	[Folder] [nvarchar](255) NULL,
	[Action] [char](1) NOT NULL CONSTRAINT [DF_b_FileAction_Action]  DEFAULT ('D'),
	[CreateDate] [datetime] NOT NULL CONSTRAINT [DF_b_FileAction_CreateDate]  DEFAULT (getdate()),
	[ExecDate] [datetime] NULL,
	[SuccessExec] [char](1) NOT NULL CONSTRAINT DF_b_FileAction_SuccessExec DEFAULT (' '),
	[RequestDate] [datetime] NULL,
	[StorageId] INT NOT NULL CONSTRAINT DF_b_FileAction_StorageId DEFAULT (0),
	CONSTRAINT [PK_b_FileAction] PRIMARY KEY CLUSTERED ([ID] ASC)
)
GO

IF OBJECT_ID(N'b_FileAction', N'U') IS NOT NULL AND NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_FileAction_SuccessExec')
CREATE NONCLUSTERED INDEX IX_b_FileAction_SuccessExec ON b_FileAction
(
	SuccessExec ASC
)
GO

/****** Object:  Table [b_Custom_Field]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_Custom_Field', N'U') IS NULL
CREATE TABLE [b_Custom_Field]
(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[EntityID] [nvarchar](50) NOT NULL,
	[FieldName] [nvarchar](255) NOT NULL,
	[CustomTypeID] [nvarchar](255) NOT NULL,
	[XmlID] [nvarchar](255) NULL,
	[Sort] [int] NOT NULL,
	[Multiple] [bit] NOT NULL,
	[Mandatory] [bit] NOT NULL,
	[ShowInFilter] [int] NOT NULL,
	[ShowInList] [bit] NOT NULL,
	[EditInList] [bit] NOT NULL,
	[IsSearchable] [bit] NOT NULL,
	[Settings] [nvarchar](max) NULL,
	CONSTRAINT [PK_b_Custom_Fields] PRIMARY KEY CLUSTERED ([ID] ASC)
)
GO

/****** Object:  Table [b_Custom_Field_Localization]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_Custom_Field_Localization', N'U') IS NULL
CREATE TABLE [b_Custom_Field_Localization]
(
	[FieldID] [int] NOT NULL,
	[LanguageId] [char](2) NOT NULL,
	[EditFormLabel] [nvarchar](255) NULL,
	[ListColumnLabel] [nvarchar](255) NULL,
	[ListFilterLabel] [nvarchar](255) NULL,
	[ErrorMessage] [nvarchar](255) NULL,
	[HelpMessage] [nvarchar](255) NULL,
	CONSTRAINT [FK_b_Custom_Field_Localization_b_Custom_Field] FOREIGN KEY([FieldID]) REFERENCES [b_Custom_Field] ([ID])
)
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_Custom_Field_Localization_FieldID')
	CREATE CLUSTERED INDEX IX_b_Custom_Field_Localization_FieldID ON b_Custom_Field_Localization(FieldID ASC);
GO


/****** Object:  Table [b_Custom_Field_Enum]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_Custom_Field_Enum', N'U') IS NULL
CREATE TABLE [b_Custom_Field_Enum](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FieldID] [int] NOT NULL,
	[FieldType] varchar(32) NOT NULL,
	[Value] [nvarchar](255) NOT NULL,
	[Default] [bit] NOT NULL,
	[Sort] [int] NOT NULL,
	[XmlID] [nvarchar](255) NULL,
	CONSTRAINT [PK_b_Custom_Field_Enum] PRIMARY KEY NONCLUSTERED ([ID] ASC)	
)
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_BX_b_Custom_Field_Enum_FieldTypeID')
CREATE CLUSTERED INDEX IX_BX_b_Custom_Field_Enum_FieldTypeID ON b_Custom_Field_Enum
(
	FieldType,
	FieldID
)
GO

/****** Object:  Table [b_Options]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_Options', N'U') IS NULL
CREATE TABLE [b_Options]
(
	[EntityId] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Value] [nvarchar](max) NULL,
	[Description] [nvarchar](255) NULL,
	[SiteId] [varchar](50) NULL CONSTRAINT [DF_b_Options_SiteId]  DEFAULT (NULL),
	CONSTRAINT [IX_b_Options] UNIQUE NONCLUSTERED ([EntityId] ASC, [Name] ASC, [SiteId] ASC),
	CONSTRAINT [FK_b_Options_b_Site] FOREIGN KEY([SiteId]) REFERENCES [b_Site] ([ID])
)
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_Options_EntityId')
	CREATE CLUSTERED INDEX IX_b_Options_EntityId ON b_Options(EntityId ASC);
GO

/****** Object:  Table [b_UsersOptions]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_UsersOptions', N'U') IS NULL
CREATE TABLE [b_UsersOptions]
(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NULL,
	[Category] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Value] [nvarchar](max) NULL,
	[Common] [char](1) NOT NULL CONSTRAINT [DF_b_UsersOptions_Common]  DEFAULT ('N'),
	CONSTRAINT [PK_b_UsersOptions] PRIMARY KEY CLUSTERED ([ID] ASC)
)
GO
IF OBJECT_ID(N'b_UsersOptions', N'U') IS NOT NULL
AND NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_UsersOptions')
CREATE NONCLUSTERED INDEX [IX_b_UsersOptions] ON [b_UsersOptions] 
(
	[Category] ASC,
	[Name] ASC
)
GO


/****** Object:  Table [b_UsersControlsOptions]    Script Date: 02/25/2010 09:38:31 ******/
IF OBJECT_ID(N'b_UsersControlsOptions', N'U') IS NULL
CREATE TABLE [b_UsersControlsOptions]
(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[Common] [nvarchar](1) NOT NULL,
	[ControlID] [nvarchar](128) NOT NULL,
	[PageID] [nvarchar](256) NOT NULL,
	[Parameters] [nvarchar](max) NULL,
	CONSTRAINT [IID] PRIMARY KEY CLUSTERED ([ID] ASC),
	CONSTRAINT [UserUpLink] FOREIGN KEY([UserID]) REFERENCES [b_Users] ([ID])
)
GO
IF OBJECT_ID(N'b_UsersControlsOptions', N'U') IS NOT NULL
AND NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IUserID')
CREATE NONCLUSTERED INDEX [IUserID] ON [b_UsersControlsOptions] 
(
	[UserID] ASC
)
GO


/****** Object:  Trigger [b_Roles_Delete]    Script Date: 02/25/2010 09:38:34 ******/
IF OBJECT_ID(N'b_Roles_Delete', N'TR') IS NOT NULL DROP TRIGGER [b_Roles_Delete]
GO
CREATE TRIGGER [b_Roles_Delete]
ON [b_Roles] 
FOR DELETE
AS
	IF (SELECT COUNT(*) FROM deleted WHERE ID in (1, 2, 3)) > 0
	BEGIN
		ROLLBACK
		RAISERROR('Can not delete the roles #1, #2 and #3', 16, 1)
	END
GO


/****** Object:  Trigger [b_File_Update]    Script Date: 02/25/2010 09:38:33 ******/
IF OBJECT_ID(N'b_File_Update', N'TR') IS NOT NULL DROP TRIGGER [b_File_Update]
GO
CREATE TRIGGER [b_File_Update]
ON [b_File]
FOR UPDATE
AS
	INSERT INTO b_FileAction(FileName, Folder, StorageId, Action)
	SELECT d.FileName, d.Folder, d.StorageId, 'D' FROM inserted i, deleted d
		WHERE i.ID = d.ID AND (i.FileName <> d.FileName OR i.Folder <> d.Folder)
GO


/****** Object:  Trigger [b_File_Delete]    Script Date: 02/25/2010 09:38:33 ******/
IF OBJECT_ID(N'b_File_Delete', N'TR') IS NOT NULL DROP TRIGGER [b_File_Delete]
GO
CREATE TRIGGER [b_File_Delete]
ON [b_File]
FOR DELETE
AS
	INSERT INTO b_FileAction(FileName, Folder, StorageId, Action)
		SELECT d.FileName, d.Folder, d.StorageId, 'D' FROM deleted d
GO


/****** Object:  Trigger [b_Users_Detele]    Script Date: 02/25/2010 09:38:34 ******/
IF OBJECT_ID(N'b_Users_Detele', N'TR') IS NOT NULL DROP TRIGGER [b_Users_Detele]
GO
CREATE TRIGGER [b_Users_Detele]
ON [b_Users] 
FOR DELETE
AS
	IF (SELECT COUNT(*) FROM deleted WHERE ID = 1) > 0
	BEGIN
		ROLLBACK
		RAISERROR('Can not delete the user #1', 16, 1)
	END
GO


/****** Object:  UserDefinedFunction [Roles_ComputePolicyL]    Script Date: 02/25/2010 09:38:33 ******/
IF OBJECT_ID(N'Roles_ComputePolicyL', N'FN') IS NOT NULL DROP FUNCTION [Roles_ComputePolicyL]
GO
CREATE FUNCTION [Roles_ComputePolicyL] 
(
	@Policy1 xml,
	@Policy2 xml
)
RETURNS xml
AS
BEGIN
	DECLARE @Result xml
	SET @Result = NULL

	DECLARE @Policy xml
	SET @Policy = NULL

	DECLARE @PolicySTring1 varchar(1000)
	SELECT @PolicySTring1 = CONVERT(varchar(1000), @Policy1.query('/child::Policy/child::*'))

	DECLARE @PolicySTring2 varchar(1000)
	SELECT @PolicySTring2 = CONVERT(varchar(1000), @Policy2.query('/child::Policy/child::*'))

	IF (@PolicySTring1 IS NULL)
	BEGIN
		SET @PolicySTring1 = ''
	END

	IF (@PolicySTring2 IS NULL)
	BEGIN
		SET @PolicySTring2 = ''
	END

	SELECT @Policy = CONVERT(xml, '<Policy>' + @PolicySTring1 + @PolicySTring2 + '</Policy>')


	SELECT @Result = @Policy.query('
		<Policy>
			{
			if (empty(/Policy/SlidingExpiration/@value)) then ()
			else <SlidingExpiration value="{ min(/Policy/SlidingExpiration/@value) cast as xs:integer? }"/>
			}
			{
			if (empty(/Policy/SessionTimeout/@value)) then ()
			else <SessionTimeout value="{ min(/Policy/SessionTimeout/@value) cast as xs:integer? }"/>
			}
			{
			if (count(/Policy/SessionIPMask/@value) < 1) then ()
			else 
				if (count(/Policy/SessionIPMask/@value) = 1) then 
					<SessionIPMask value="{/Policy/SessionIPMask[1]/@value}"/>
				else
					if (string-length(string((/Policy/SessionIPMask)[1]/@value)) < string-length(string((/Policy/SessionIPMask)[2]/@value))) then 
						<SessionIPMask value="{/Policy/SessionIPMask[1]/@value}"/>
					else
						<SessionIPMask value="{/Policy/SessionIPMask[2]/@value}"/>
			}
			{
			if (empty(/Policy/MaxStoreNum/@value)) then ()
			else <MaxStoreNum value="{ min(/Policy/MaxStoreNum/@value) cast as xs:integer? }"/>
			}
			{
			if (count(/Policy/StoreIPMask/@value) < 1) then ()
			else 
				if (count(/Policy/StoreIPMask/@value) = 1) then 
					<StoreIPMask value="{/Policy/StoreIPMask[1]/@value}"/>
				else
					if (string-length(string((/Policy/StoreIPMask)[1]/@value)) < string-length(string((/Policy/StoreIPMask)[2]/@value))) then 
						<StoreIPMask value="{/Policy/StoreIPMask[1]/@value}"/>
					else
						<StoreIPMask value="{/Policy/StoreIPMask[2]/@value}"/>
			}
			{
			if (empty(/Policy/StoreTimeout/@value)) then ()
			else <StoreTimeout value="{ min(/Policy/StoreTimeout/@value) cast as xs:integer? }"/>
			}
			{
			if (empty(/Policy/CheckwordTimeout/@value)) then ()
			else <CheckwordTimeout value="{ min(/Policy/CheckwordTimeout/@value) cast as xs:integer? }"/>
			}
		</Policy>
	')

	RETURN @Result
END
GO

/****** Object:  UserDefinedFunction [Roles_ComputePolicy]    Script Date: 02/25/2010 09:38:33 ******/
IF OBJECT_ID(N'Roles_ComputePolicy', N'FN') IS NOT NULL DROP FUNCTION [Roles_ComputePolicy]
GO
CREATE FUNCTION [Roles_ComputePolicy] 
(
	@Policy1 xml,
	@Policy2 xml
)
RETURNS xml
AS
BEGIN
	DECLARE @Result xml
	SET @Result = NULL

	DECLARE @Policy xml
	SET @Policy = NULL

	DECLARE @PolicySTring1 varchar(1000)
	SELECT @PolicySTring1 = CONVERT(varchar(1000), @Policy1.query('/child::Policy/child::*'))

	DECLARE @PolicySTring2 varchar(1000)
	SELECT @PolicySTring2 = CONVERT(varchar(1000), @Policy2.query('/child::Policy/child::*'))

	IF (@PolicySTring1 IS NULL)
	BEGIN
		SET @PolicySTring1 = ''
	END

	IF (@PolicySTring2 IS NULL)
	BEGIN
		SET @PolicySTring2 = ''
	END

	SELECT @Policy = CONVERT(xml, '<Policy>' + @PolicySTring1 + @PolicySTring2 + '</Policy>')


	SELECT @Result = @Policy.query('
		<Policy>
			{
			if (empty(/Policy/SlidingExpiration/@value)) then ()
			else <SlidingExpiration value="{ ((/Policy/SlidingExpiration)[1]/@value) cast as xs:integer? }"/>
			}
			{
			if (empty(/Policy/SessionTimeout/@value)) then ()
			else <SessionTimeout value="{ ((/Policy/SessionTimeout)[1]/@value) cast as xs:integer? }"/>
			}
			{
			if (empty(/Policy/SessionIPMask/@value)) then ()
			else <SessionIPMask value="{ (/Policy/SessionIPMask)[1]/@value }"/>
			}
			{
			if (empty(/Policy/MaxStoreNum/@value)) then ()
			else <MaxStoreNum value="{ ((/Policy/MaxStoreNum)[1]/@value) cast as xs:integer? }"/>
			}
			{
			if (empty(/Policy/StoreIPMask/@value)) then ()
			else <StoreIPMask value="{ (/Policy/StoreIPMask)[1]/@value }"/>
			}
			{
			if (empty(/Policy/StoreTimeout/@value)) then ()
			else <StoreTimeout value="{ ((/Policy/StoreTimeout)[1]/@value) cast as xs:integer? }"/>
			}
			{
			if (empty(/Policy/CheckwordTimeout/@value)) then ()
			else <CheckwordTimeout value="{ ((/Policy/CheckwordTimeout)[1]/@value) cast as xs:integer? }"/>
			}
		</Policy>
	')
	RETURN @Result
END
GO


/****** Object:  StoredProcedure [Roles_FillRolesCache]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_FillRolesCache', N'P') IS NOT NULL DROP PROCEDURE [Roles_FillRolesCache]
GO
CREATE PROCEDURE [Roles_FillRolesCache]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	BEGIN TRANSACTION

	DELETE FROM b_RolesCache;
	IF (@@ERROR <> 0)
    BEGIN
        ROLLBACK TRANSACTION
        RETURN (-1)
    END;

	WITH rolesCache(roleId, subRoleId)
	AS
	(
		SELECT ID, ID FROM [b_Roles]
		UNION ALL
		SELECT c.roleId, r.SubRoleID  FROM
			rolesCache c inner join b_RolesInRoles r 
			ON c.subRoleId = r.RoleID
	) 
	INSERT INTO b_RolesCache 
	SELECT DISTINCT roleId, subRoleId FROM rolesCache;

	IF (@@ERROR <> 0)
    BEGIN
        ROLLBACK TRANSACTION
        RETURN (-1)
    END;

	COMMIT TRANSACTION
END
GO


/****** Object:  StoredProcedure [Roles_GetAllTasks]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_GetAllTasks', N'P') IS NOT NULL DROP PROCEDURE [Roles_GetAllTasks]
GO
CREATE PROCEDURE [Roles_GetAllTasks]
	@PageIndex int = 0,
	@PageSize int = -1,
	@ModuleId varchar(128) = NULL
AS
BEGIN
	DECLARE @PageLowerBound int
	DECLARE @PageUpperBound int
	DECLARE @TotalRecords int
	
	IF (@PageSize = -1)
	BEGIN
		SELECT r.TaskName
		FROM dbo.b_RolesTasks r
		WHERE (@ModuleId IS NULL 
			OR @ModuleId IS NOT NULL AND r.ID IN (
				SELECT rto.TaskID
				FROM b_RolesTasksInOperations rto
					INNER JOIN b_RolesOperations ro ON (rto.OperationID = ro.ID)
				WHERE ro.ModuleID = @ModuleId
			)
		)
		ORDER BY r.TaskName
		
		RETURN 0
	END
	ELSE
	BEGIN
		SET @PageLowerBound = @PageSize * @PageIndex
		SET @PageUpperBound = @PageSize - 1 + @PageLowerBound

		CREATE TABLE #PageIndexForTasks
		(
			IndexId int IDENTITY (0, 1) NOT NULL,
			TaskId int
		)

		INSERT INTO #PageIndexForTasks (TaskId)
		SELECT r.ID
		FROM dbo.b_RolesTasks r
		ORDER BY r.TaskName

		SELECT @TotalRecords = @@ROWCOUNT

		SELECT r.TaskName, r.ID, r.Comment,
			(
			SELECT COUNT(*) 
			FROM b_RolesTasksInTasks uir 
			WHERE uir.TaskID = r.ID 
			) as TasksCount,
			(
			SELECT COUNT(*) 
			FROM b_RolesTasksInOperations rir 
			WHERE rir.TaskID = r.ID 
			) as OperationsCount
		FROM dbo.b_RolesTasks r, #PageIndexForTasks p
		WHERE r.ID = p.TaskId
			AND p.IndexId >= @PageLowerBound
			AND p.IndexId <= @PageUpperBound
		ORDER BY r.TaskName
	END

	RETURN @TotalRecords
END
GO


/****** Object:  StoredProcedure [Roles_GetTasksInTask]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_GetTasksInTask', N'P') IS NOT NULL DROP PROCEDURE [Roles_GetTasksInTask]
GO
CREATE PROCEDURE [Roles_GetTasksInTask]
	@TaskName nvarchar(128)
AS
BEGIN

	DECLARE @TaskId int
	SELECT @TaskId = NULL

	SELECT @TaskId = ID
	FROM dbo.b_RolesTasks
	WHERE LOWER(@TaskName) = LoweredTaskName

	IF (@TaskId IS NULL)
	 RETURN(9)

	SELECT r.TaskName
	FROM dbo.b_RolesTasks r, dbo.b_RolesTasksInTasks rr
	WHERE r.ID = rr.SubTaskID
		AND @TaskId = rr.TaskID
	ORDER BY r.TaskName

	RETURN(0)

END
GO


/****** Object:  StoredProcedure [Roles_GetTasksInRole]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_GetTasksInRole', N'P') IS NOT NULL DROP PROCEDURE [Roles_GetTasksInRole]
GO
CREATE PROCEDURE [Roles_GetTasksInRole]
	@RoleName nvarchar(256),
	@ModuleId varchar(128) = NULL,
	@ExternalId varchar(128) = NULL
AS
BEGIN

	DECLARE @RoleId int
	SELECT @RoleId = NULL

	SELECT @RoleId = ID
	FROM [b_Roles]
	WHERE LOWER(@RoleName) = LoweredRoleName

	IF (@RoleId IS NULL)
	 RETURN(2)

	SELECT r.TaskName
	FROM dbo.b_RolesTasks r, dbo.b_RolesInTasks rr
	WHERE r.ID = rr.TaskID
		AND @RoleId = rr.RoleID
		AND (@ModuleId IS NULL AND rr.ModuleID = ''
			OR @ModuleId IS NOT NULL AND rr.ModuleID = @ModuleId AND rr.ExternalID = @ExternalId)
	ORDER BY r.TaskName

	RETURN(0)

END
GO


/****** Object:  StoredProcedure [Roles_GetOperationsInTask]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_GetOperationsInTask', N'P') IS NOT NULL DROP PROCEDURE [Roles_GetOperationsInTask]
GO
CREATE PROCEDURE [Roles_GetOperationsInTask]
	@TaskName nvarchar(128)
AS
BEGIN

	DECLARE @TaskId int
	SELECT @TaskId = NULL

	SELECT @TaskId = ID
	FROM dbo.b_RolesTasks
	WHERE LOWER(@TaskName) = LoweredTaskName

	IF (@TaskId IS NULL)
	 RETURN(9)

	SELECT r.OperationName
	FROM dbo.b_RolesOperations r, dbo.b_RolesTasksInOperations rr
	WHERE r.ID = rr.OperationID
		AND @TaskId = rr.TaskID
	ORDER BY r.OperationName

	RETURN(0)

END
GO


/****** Object:  StoredProcedure [Roles_CreateTask]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_CreateTask', N'P') IS NOT NULL DROP PROCEDURE [Roles_CreateTask]
GO
CREATE PROCEDURE [Roles_CreateTask]
    @TaskName nvarchar(128),
    @TaskTitle nvarchar(255) = null,
    @Comment ntext = null,
    @TaskId int OUTPUT
AS
BEGIN
	SELECT @TaskId = NULL

    DECLARE @ErrorCode int
    SET @ErrorCode = 0

    DECLARE @TranStarted bit
    SET @TranStarted = 0

    IF (@@TRANCOUNT = 0)
    BEGIN
        BEGIN TRANSACTION
        SET @TranStarted = 1
    END
    ELSE
        SET @TranStarted = 0

    IF (EXISTS( SELECT ID FROM dbo.b_RolesTasks WHERE LoweredTaskName = LOWER(@TaskName) ))
    BEGIN
        SET @ErrorCode = 8
        GOTO Cleanup
    END

    INSERT INTO dbo.b_RolesTasks(TaskName, Title, LoweredTaskName, Comment)
         VALUES (@TaskName, @TaskTitle, LOWER(@TaskName), @Comment)

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

	SELECT @TaskId = @@IDENTITY

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
        COMMIT TRANSACTION
    END

    RETURN(0)

Cleanup:

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
        ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode

END
GO


/****** Object:  StoredProcedure [Roles_GetTaskByID]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_GetTaskByID', N'P') IS NOT NULL DROP PROCEDURE [Roles_GetTaskByID]
GO
CREATE PROCEDURE [Roles_GetTaskByID]
	@TaskId int
AS
BEGIN

	SELECT r.TaskName, r.ID, r.Comment,
		(
		SELECT COUNT(*) 
		FROM b_RolesTasksInTasks uir 
		WHERE uir.TaskID = r.ID 
		) as TasksCount,
		(
		SELECT COUNT(*) 
		FROM b_RolesTasksInOperations rir 
		WHERE rir.TaskID = r.ID 
		) as OperationsCount
	FROM dbo.b_RolesTasks r
	WHERE r.ID = @TaskId

    IF (@@ROWCOUNT = 0)
       RETURN -1

    RETURN 0
END
GO


/****** Object:  StoredProcedure [Roles_FillOperationsCache]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_FillOperationsCache', N'P') IS NOT NULL DROP PROCEDURE [Roles_FillOperationsCache]
GO
CREATE PROCEDURE [Roles_FillOperationsCache]
AS
BEGIN
    DECLARE @ErrorCode int
    SET @ErrorCode = 0

    DECLARE @TranStarted bit
    SET @TranStarted = 0

    IF (@@TRANCOUNT = 0)
    BEGIN
        BEGIN TRANSACTION
        SET @TranStarted = 1
    END

	DELETE FROM b_RolesOperationsCache
	WHERE 1 = 1

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END


	DECLARE @tbRoles table(RoleId int NOT NULL, SubRoleId int NOT NULL, PRIMARY KEY (RoleId, SubRoleId))

	DECLARE @RoleId int

	DECLARE roles_cursor CURSOR FOR
	SELECT ID
	FROM [b_Roles]

	OPEN roles_cursor
	
	FETCH NEXT FROM roles_cursor INTO @RoleId
	
	WHILE @@FETCH_STATUS = 0
	BEGIN

		WITH tmp_Roles(ID, Way) AS (
			SELECT ID, ',' + CAST(ID AS VARCHAR(MAX)) + ',' AS Way
			FROM [b_Roles] AS r
			WHERE (ID = @RoleId)
			UNION ALL
			SELECT r.ID, tr.Way + CAST(r.ID AS VARCHAR(MAX)) + ',' AS Way
			FROM dbo.b_RolesInRoles AS rir
				INNER JOIN [b_Roles] AS r ON r.ID = rir.SubRoleID
				INNER JOIN tmp_Roles AS tr ON tr.ID = rir.RoleID
			WHERE (tr.Way NOT LIKE '%,' + CAST(r.ID AS VARCHAR(MAX)) + ',%'))
		INSERT INTO @tbRoles (RoleId, SubRoleId)
		SELECT DISTINCT @RoleId, ID
		FROM tmp_Roles
	
		FETCH NEXT FROM roles_cursor INTO @RoleId
	END

	CLOSE roles_cursor
	DEALLOCATE roles_cursor

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

	DECLARE @tbTasks table(TaskId int NOT NULL, SubTaskId int NOT NULL, PRIMARY KEY (TaskId, SubTaskId))

	DECLARE @TaskId int

	DECLARE tasks_cursor CURSOR FOR
	SELECT ID
	FROM b_RolesTasks

	OPEN tasks_cursor
	
	FETCH NEXT FROM tasks_cursor INTO @TaskId
	
	WHILE @@FETCH_STATUS = 0
	BEGIN

		WITH tmp_Tasks(ID, Way) AS (
			SELECT ID, ',' + CAST(ID AS VARCHAR(MAX)) + ',' AS Way
			FROM dbo.b_RolesTasks AS r
			WHERE (ID = @TaskId)
			UNION ALL
			SELECT r.ID, tr.Way + CAST(r.ID AS VARCHAR(MAX)) + ',' AS Way
			FROM dbo.b_RolesTasksInTasks AS rir
				INNER JOIN dbo.b_RolesTasks AS r ON r.ID = rir.SubTaskID
				INNER JOIN tmp_Tasks AS tr ON tr.ID = rir.TaskID
			WHERE (tr.Way NOT LIKE '%,' + CAST(r.ID AS VARCHAR(MAX)) + ',%'))
		INSERT INTO @tbTasks (TaskId, SubTaskId)
		SELECT DISTINCT @TaskId, ID
		FROM tmp_Tasks
	
		FETCH NEXT FROM tasks_cursor INTO @TaskId
	END

	CLOSE tasks_cursor
	DEALLOCATE tasks_cursor

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

	INSERT INTO b_RolesOperationsCache (RoleID, OperationID, ModuleID, ExternalID)
	SELECT DISTINCT tr.RoleId, rtio.OperationID, rit.ModuleID, rit.ExternalID
	FROM @tbRoles tr
		INNER JOIN b_RolesInTasks rit ON (tr.SubRoleId = rit.RoleID)
		INNER JOIN @tbTasks tt ON (rit.TaskID = tt.TaskId)
		INNER JOIN b_RolesTasksInOperations rtio ON (tt.SubTaskId = rtio.TaskID)
	UNION
	SELECT DISTINCT tr.RoleId, rio.OperationID, rio.ModuleID, rio.ExternalID
	FROM @tbRoles tr
		INNER JOIN b_RolesInOperations rio ON (tr.SubRoleId = rio.RoleID)


    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
        COMMIT TRANSACTION
    END

    RETURN(0)

Cleanup:

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
        ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode

END
GO


/****** Object:  StoredProcedure [Roles_GetOperationsForUser]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_GetOperationsForUser', N'P') IS NOT NULL DROP PROCEDURE [Roles_GetOperationsForUser]
GO
CREATE PROCEDURE [Roles_GetOperationsForUser]
	@UserId int = NULL,
	@UserName nvarchar(256) = NULL,
	@ProviderName nvarchar(128) = NULL,
	@OperationNames nvarchar(4000) = NULL
AS
BEGIN
	IF (@ProviderName IS NULL)
	BEGIN
		SELECT @ProviderName = 'self'
	END

	IF (@UserId IS NULL OR @UserId = 0)
	BEGIN
		SELECT @UserId = NULL
	END

	IF (@UserId IS NULL AND @UserName IS NOT NULL)
	BEGIN
		SELECT @UserId = u.ID
		FROM dbo.b_Users u
		WHERE u.LoweredUserName = LOWER(@UserName)
			AND u.LoweredProviderName = LOWER(@ProviderName)
	END

	DECLARE @tbNames table(Name nvarchar(256) NOT NULL PRIMARY KEY)
	DECLARE @Num int
	DECLARE @Pos int
	DECLARE @NextPos int
	DECLARE @Name nvarchar(128)

	SET @Num = 0
	SET @Pos = 1
	WHILE (@Pos <= LEN(@OperationNames))
	BEGIN
		SELECT @NextPos = CHARINDEX(N',', @OperationNames,  @Pos)
		IF (@NextPos = 0 OR @NextPos IS NULL)
			SELECT @NextPos = LEN(@OperationNames) + 1
		SELECT @Name = RTRIM(LTRIM(SUBSTRING(@OperationNames, @Pos, @NextPos - @Pos)))
		SELECT @Pos = @NextPos + 1

		INSERT INTO @tbNames VALUES (LOWER(@Name))
		SET @Num = @Num + 1
	END

	IF (@UserId = 1)
	BEGIN

		SELECT ro.OperationName, '', ''
		FROM dbo.b_RolesOperations ro
		WHERE @OperationNames IS NULL
			OR (@OperationNames IS NOT NULL AND ro.LoweredOperationName IN (SELECT Name FROM @tbNames))

	END
	ELSE
	BEGIN

		SELECT ro.OperationName, roc.ModuleID, roc.ExternalID
		FROM dbo.b_RolesOperationsCache roc, dbo.b_RolesOperations ro
		WHERE roc.OperationID = ro.ID
			AND (@OperationNames IS NULL
				OR (@OperationNames IS NOT NULL AND ro.LoweredOperationName IN (SELECT Name FROM @tbNames)))
			AND (roc.RoleID = 2
				OR @UserId IS NOT NULL AND (roc.RoleID IN (
						SELECT uir.RoleID
						FROM dbo.b_UsersInRoles uir
						WHERE uir.UserID = @UserId
							AND (uir.ActiveFrom IS NULL OR uir.ActiveFrom <= GETDATE())
							AND (uir.ActiveTo IS NULL OR uir.ActiveTo >= GETDATE())
					) OR roc.RoleID = 3)
				)

	END

	RETURN (0)
END
GO


/****** Object:  StoredProcedure [Roles_GetUserOperations]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_GetUserOperations', N'P') IS NOT NULL DROP PROCEDURE [Roles_GetUserOperations]
GO
CREATE PROCEDURE [Roles_GetUserOperations]
	@UserId int = NULL,
	@UserName nvarchar(256) = NULL,
	@ProviderName nvarchar(128) = NULL,
	@OperationNames nvarchar(4000) = NULL
AS
BEGIN
	IF (@ProviderName IS NULL)
	BEGIN
		SELECT @ProviderName = 'self'
	END

	IF (@UserId IS NULL OR @UserId = 0)
	BEGIN
		SELECT @UserId = NULL
	END

	IF (@UserId IS NULL AND @UserName IS NOT NULL)
	BEGIN
		SELECT @UserId = u.ID
		FROM dbo.b_Users u
		WHERE u.LoweredUserName = LOWER(@UserName)
			AND u.LoweredProviderName = LOWER(@ProviderName)
	END

	DECLARE @tbNames table(Name nvarchar(256) NOT NULL PRIMARY KEY)
	DECLARE @Num int
	DECLARE @Pos int
	DECLARE @NextPos int
	DECLARE @Name nvarchar(128)

	SET @Num = 0
	SET @Pos = 1
	WHILE (@Pos <= LEN(@OperationNames))
	BEGIN
		SELECT @NextPos = CHARINDEX(N',', @OperationNames,  @Pos)
		IF (@NextPos = 0 OR @NextPos IS NULL)
			SELECT @NextPos = LEN(@OperationNames) + 1
		SELECT @Name = RTRIM(LTRIM(SUBSTRING(@OperationNames, @Pos, @NextPos - @Pos)))
		SELECT @Pos = @NextPos + 1

		INSERT INTO @tbNames VALUES (LOWER(@Name))
		SET @Num = @Num + 1
	END

	IF (@UserId = 1)
	BEGIN

		SELECT ro.ID, ro.OperationName, ro.OperationType, ro.ModuleID, ro.Comment, '', ''
		FROM dbo.b_RolesOperations ro
		WHERE @OperationNames IS NULL
			OR (@OperationNames IS NOT NULL AND ro.LoweredOperationName IN (SELECT Name FROM @tbNames))

	END
	ELSE
	BEGIN

		SELECT ro.ID, ro.OperationName, ro.OperationType, ro.ModuleID, ro.Comment, roc.ModuleID, roc.ExternalID
		FROM dbo.b_RolesOperationsCache roc, dbo.b_RolesOperations ro
		WHERE roc.OperationID = ro.ID
			AND (@OperationNames IS NULL
				OR (@OperationNames IS NOT NULL AND ro.LoweredOperationName IN (SELECT Name FROM @tbNames)))
			AND (roc.RoleID = 2
				OR @UserId IS NOT NULL AND (roc.RoleID IN (
						SELECT uir.RoleID
						FROM dbo.b_UsersInRoles uir
						WHERE uir.UserID = @UserId
							AND (uir.ActiveFrom IS NULL OR uir.ActiveFrom <= GETDATE())
							AND (uir.ActiveTo IS NULL OR uir.ActiveTo >= GETDATE())
					) OR roc.RoleID = 3)
				)

	END

	RETURN (0)
END
GO

IF OBJECT_ID(N'Roles_GetRoleOperationLinks', N'P') IS NOT NULL DROP PROCEDURE Roles_GetRoleOperationLinks
GO
CREATE PROCEDURE Roles_GetRoleOperationLinks
	@moduleId varchar(50) = NULL	
AS
BEGIN
	SET NOCOUNT ON;
   
	SELECT 
		l.RoleID, l.OperationID, l.ModuleID, l.ExternalID
	FROM 
		b_RolesInOperations l
		INNER JOIN b_RolesOperations o ON l.OperationID = o.ID
	WHERE 
		@moduleId IS NULL OR o.ModuleID = @moduleId
	ORDER BY
		l.RoleID
END
GO

/****** Object:  StoredProcedure [Roles_IsUserCanOperate]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_IsUserCanOperate', N'P') IS NOT NULL DROP PROCEDURE [Roles_IsUserCanOperate]
GO
CREATE PROCEDURE [Roles_IsUserCanOperate]
    @Operation nvarchar(128),
	@UserId int = NULL,
    @UserName nvarchar(256) = NULL,
    @ProviderName nvarchar(128) = null,
    @ModuleID varchar(50) = null,
    @ExternalID varchar(50) = null
AS
BEGIN
    DECLARE @OperationId int
    SELECT @OperationId = NULL

	IF (@ProviderName IS NULL)
	BEGIN
		SELECT @ProviderName = 'self'
	END

	IF (@ModuleID IS NULL OR @ModuleID = '' OR @ExternalID IS NULL OR @ExternalID = '')
	BEGIN
		SELECT @ModuleID = ''
		SELECT @ExternalID = ''
	END

	IF (@UserId IS NULL OR @UserId = 0)
	BEGIN
		SELECT @UserId = NULL
	END

	IF (@UserId IS NULL AND @UserName IS NOT NULL)
	BEGIN
		SELECT @UserId = ID
		FROM dbo.b_Users
		WHERE LoweredUserName = LOWER(@UserName)
			AND LoweredProviderName = LOWER(@ProviderName)
	END

	IF (@UserId = 1)
		RETURN(1)

    SELECT @OperationId = ID
    FROM dbo.b_RolesOperations
    WHERE LoweredOperationName = LOWER(@Operation)

    IF (@OperationId IS NULL)
        RETURN(3)

    IF (EXISTS( 
			SELECT * 
			FROM dbo.b_RolesOperationsCache roc
			WHERE roc.OperationID = @OperationId
				AND (roc.ModuleID = @ModuleID AND roc.ExternalID = @ExternalID
					OR roc.ModuleID = '' AND roc.ExternalID = '')
				AND (roc.RoleID = 2
					OR @UserId IS NOT NULL AND (roc.RoleID IN (
							SELECT uir.RoleID
							FROM dbo.b_UsersInRoles uir
							WHERE uir.UserID = @UserId
								AND (uir.ActiveFrom IS NULL OR uir.ActiveFrom <= GETDATE())
								AND (uir.ActiveTo IS NULL OR uir.ActiveTo >= GETDATE())
						) OR roc.RoleID = 3)
					)
		))
        RETURN(1)
    ELSE
        RETURN(0)
END
GO


/****** Object:  StoredProcedure [Roles_GetOperationsInRole]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_GetOperationsInRole', N'P') IS NOT NULL DROP PROCEDURE [Roles_GetOperationsInRole]
GO
CREATE PROCEDURE [Roles_GetOperationsInRole]
	@RoleName nvarchar(256),
	@ModuleID varchar(128) = NULL,
	@ExternalId varchar(128) = NULL
AS
BEGIN

	DECLARE @RoleId int
	SELECT @RoleId = NULL

	SELECT @RoleId = ID
	FROM [b_Roles]
	WHERE LOWER(@RoleName) = LoweredRoleName

	IF (@RoleId IS NULL)
	 RETURN(2)

	SELECT r.OperationName
	FROM dbo.b_RolesOperations r, dbo.b_RolesInOperations rr
	WHERE r.ID = rr.OperationID
		AND @RoleId = rr.RoleID
		AND (@ModuleID IS NULL AND rr.ModuleID = ''
			OR @ModuleID IS NOT NULL AND rr.ModuleID = @ModuleID AND rr.ExternalID = @ExternalId
		)
	ORDER BY r.OperationName

	RETURN(0)

END
GO


/****** Object:  StoredProcedure [Roles_GetOperationByID]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_GetOperationByID', N'P') IS NOT NULL DROP PROCEDURE [Roles_GetOperationByID]
GO
CREATE PROCEDURE [Roles_GetOperationByID]
	@OperationId int
AS
BEGIN

	SELECT r.OperationName, r.ID, r.Comment, r.OperationType, r.ModuleID
	FROM dbo.b_RolesOperations r
	WHERE r.ID = @OperationId

    IF (@@ROWCOUNT = 0)
       RETURN -1

    RETURN 0
END
GO


/****** Object:  StoredProcedure [Roles_GetAllOperations]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_GetAllOperations', N'P') IS NOT NULL DROP PROCEDURE [Roles_GetAllOperations]
GO
CREATE PROCEDURE [Roles_GetAllOperations]
	@PageIndex int = 0,
	@PageSize int = -1,
	@ModuleId varchar(128) = null
AS
BEGIN
	DECLARE @PageLowerBound int
	DECLARE @PageUpperBound int
	DECLARE @TotalRecords int
	
	IF (@ModuleId = '')
		SELECT @ModuleId = NULL
	
	IF (@PageSize = -1)
	BEGIN
		SELECT r.OperationName, r.ID, r.Comment, r.OperationType, r.ModuleID
		FROM dbo.b_RolesOperations r
		WHERE (@ModuleId IS NULL OR @ModuleId IS NOT NULL AND r.ModuleID = @ModuleId)
		ORDER BY r.ModuleID, r.OperationName
		
		RETURN 0
	END
	ELSE
	BEGIN
		SET @PageLowerBound = @PageSize * @PageIndex
		SET @PageUpperBound = @PageSize - 1 + @PageLowerBound

		CREATE TABLE #PageIndexForOperations
		(
			IndexId int IDENTITY (0, 1) NOT NULL,
			OperationId int
		)

		INSERT INTO #PageIndexForOperations (OperationId)
		SELECT r.ID
		FROM dbo.b_RolesOperations r
		ORDER BY r.OperationName

		SELECT @TotalRecords = @@ROWCOUNT

		SELECT r.OperationName, r.ID, r.Comment, r.OperationType, r.ModuleID
		FROM dbo.b_RolesOperations r, #PageIndexForOperations p
		WHERE r.ID = p.OperationId
			AND p.IndexId >= @PageLowerBound
			AND p.IndexId <= @PageUpperBound
		ORDER BY r.OperationName
	END

	RETURN @TotalRecords
END
GO


/****** Object:  StoredProcedure [Roles_CreateOperation]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_CreateOperation', N'P') IS NOT NULL DROP PROCEDURE [Roles_CreateOperation]
GO
CREATE PROCEDURE [Roles_CreateOperation]
	@OperationName nvarchar(128),
	@OperationType nvarchar(128),
	@ModuleId nvarchar(128),
    @Comment ntext = null,
    @OperationId int OUTPUT
AS
BEGIN
    SET @OperationId = NULL

    DECLARE @ErrorCode int
    SET @ErrorCode = 0

    DECLARE @TranStarted bit
    SET @TranStarted = 0

    IF (@@TRANCOUNT = 0)
    BEGIN
        BEGIN TRANSACTION
        SET @TranStarted = 1
    END
    ELSE
        SET @TranStarted = 0

    IF (EXISTS( SELECT ID FROM dbo.b_RolesOperations WHERE LoweredOperationName = LOWER(@OperationName) ))
    BEGIN
        SET @ErrorCode = 11
        GOTO Cleanup
    END

    INSERT INTO dbo.b_RolesOperations(OperationName, LoweredOperationName, OperationType, ModuleID, Comment)
         VALUES (@OperationName, LOWER(@OperationName), @OperationType, @ModuleId, @Comment)

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

	SELECT @OperationId = @@IDENTITY

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
        COMMIT TRANSACTION
    END

    RETURN(0)

Cleanup:

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
        ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode

END
GO


/****** Object:  StoredProcedure [Roles_SynchronizeProviderRoles]    Script Date: 02/25/2010 09:38:28 ******/
IF OBJECT_ID(N'Roles_SynchronizeProviderRoles', N'P') IS NOT NULL DROP PROCEDURE [Roles_SynchronizeProviderRoles]
GO
CREATE PROCEDURE [Roles_SynchronizeProviderRoles]
	@ProviderName varchar(128),
	@RoleNames nvarchar(4000)
AS
BEGIN
	DECLARE @TranStarted bit
	SET @TranStarted = 0

	IF (@@TRANCOUNT = 0)
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END

	DECLARE @tbNames table(FName nvarchar(128) NOT NULL PRIMARY KEY)

	DECLARE @Num int
	DECLARE @Pos int
	DECLARE @NextPos int
	DECLARE @Name nvarchar(128)

	SET @Num = 0
	SET @Pos = 1
	WHILE (@Pos <= LEN(@RoleNames))
	BEGIN
		SELECT @NextPos = CHARINDEX(N',', @RoleNames,  @Pos)
		IF (@NextPos = 0 OR @NextPos IS NULL)
			SELECT @NextPos = LEN(@RoleNames) + 1
		SELECT @Name = RTRIM(LTRIM(SUBSTRING(@RoleNames, @Pos, @NextPos - @Pos)))
		SELECT @Pos = @NextPos + 1

		INSERT INTO @tbNames VALUES (@ProviderName + N'\' + @Name)
		SET @Num = @Num + 1
	END

	INSERT INTO [b_Roles](RoleName, LoweredRoleName, Active, Comment, Policy, EffectivePolicy)
	SELECT t.FName, LOWER(t.FName), 1, null, '<Policy/>', null
	FROM @tbNames t
	WHERE LOWER(t.FName) NOT IN (SELECT r.LoweredRoleName FROM [b_Roles] r)

	IF (@TranStarted = 1)
		COMMIT TRANSACTION

	RETURN(0)
END
GO


/****** Object:  StoredProcedure [Roles_RoleExists]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_RoleExists', N'P') IS NOT NULL DROP PROCEDURE [Roles_RoleExists]
GO
CREATE PROCEDURE [Roles_RoleExists]
    @RoleName nvarchar(128)
AS
BEGIN
    IF (EXISTS (SELECT RoleName FROM [b_Roles] WHERE LOWER(@RoleName) = LoweredRoleName))
        RETURN(1)
    ELSE
        RETURN(0)
END
GO


/****** Object:  StoredProcedure [Roles_FindUsersInRole]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_FindUsersInRole', N'P') IS NOT NULL DROP PROCEDURE [Roles_FindUsersInRole]
GO
CREATE PROCEDURE [Roles_FindUsersInRole]
    @RoleName nvarchar(256),
    @UserNameToMatch  nvarchar(256)
AS
BEGIN
     DECLARE @RoleId int
     SELECT @RoleId = NULL

     SELECT @RoleId = ID
     FROM [b_Roles]
     WHERE LOWER(@RoleName) = LoweredRoleName

     IF (@RoleId IS NULL)
         RETURN(2)

    SELECT u.UserName
    FROM dbo.b_Users u, dbo.b_UsersInRoles ur
    WHERE u.ID = ur.UserID
		AND @RoleId = ur.RoleID
		AND LoweredUserName LIKE LOWER(@UserNameToMatch)
    ORDER BY u.UserName

    RETURN(0)
END
GO


/****** Object:  StoredProcedure [Roles_GetAllRoles]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_GetAllRoles', N'P') IS NOT NULL DROP PROCEDURE [Roles_GetAllRoles]
GO
CREATE PROCEDURE [Roles_GetAllRoles]
	@PageIndex int = 0,
	@PageSize int = -1
AS
BEGIN
	DECLARE @PageLowerBound int
	DECLARE @PageUpperBound int
	DECLARE @TotalRecords int
	
	IF (@PageSize = -1)
	BEGIN
		SELECT r.RoleName
		FROM [b_Roles] r
		ORDER BY r.RoleName
		
		RETURN 0
	END
	ELSE
	BEGIN
		SET @PageLowerBound = @PageSize * @PageIndex
		SET @PageUpperBound = @PageSize - 1 + @PageLowerBound

		CREATE TABLE #PageIndexForRoles
		(
			IndexId int IDENTITY (0, 1) NOT NULL,
			RoleId int
		)

		INSERT INTO #PageIndexForRoles (RoleId)
		SELECT r.ID
		FROM [b_Roles] r
		ORDER BY r.RoleName

		SELECT @TotalRecords = @@ROWCOUNT

		SELECT r.RoleName, r.ID, r.Active, r.Comment, r.Policy, r.EffectivePolicy,
			(
			SELECT COUNT(*) 
			FROM b_UsersInRoles uir 
			WHERE uir.RoleID = r.ID 
			) as UsersCount,
			(
			SELECT COUNT(*) 
			FROM b_RolesInRoles rir 
			WHERE rir.RoleID = r.ID 
			) as RolesCount
		FROM [b_Roles] r, #PageIndexForRoles p
		WHERE r.ID = p.RoleId
			AND p.IndexId >= @PageLowerBound
			AND p.IndexId <= @PageUpperBound
		ORDER BY r.RoleName
	END

	RETURN @TotalRecords
END
GO


/****** Object:  StoredProcedure [Roles_AddUserToRole]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_AddUserToRole', N'P') IS NOT NULL DROP PROCEDURE [Roles_AddUserToRole]
GO
CREATE PROCEDURE [Roles_AddUserToRole]
	@UserName nvarchar(256),
	@ProviderName nvarchar(128),
	@RoleName nvarchar(128),
	@ActiveFrom datetime = NULL,
	@ActiveTo datetime = NULL
AS
BEGIN
	DECLARE @TranStarted bit
	SET @TranStarted = 0

	IF (@@TRANCOUNT = 0)
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END

    DECLARE @UserId int
    SELECT @UserId = NULL

	IF (@ProviderName IS NULL)
	BEGIN
		SELECT @ProviderName = 'self'
	END

    SELECT @UserId = ID
    FROM dbo.b_Users
    WHERE LoweredUserName = LOWER(@UserName)
		AND LoweredProviderName = LOWER(@ProviderName)

	IF (@UserId IS NULL)
	BEGIN
		IF (@TranStarted = 1)
			ROLLBACK TRANSACTION
		RETURN(1)
	END

	DECLARE @RoleId int
	SELECT @RoleId = NULL

	SELECT @RoleId = ID
	FROM [b_Roles]
	WHERE LOWER(@RoleName) = LoweredRoleName

	IF (@RoleId IS NULL)
	BEGIN
		IF (@TranStarted = 1)
			ROLLBACK TRANSACTION
		RETURN(2)
	END

	IF (@RoleId = 2 OR @RoleId = 3)
	BEGIN
		IF (@TranStarted = 1)
			COMMIT TRANSACTION

		RETURN(0)
	END

	IF (EXISTS(SELECT * FROM dbo.b_UsersInRoles ur WHERE ur.UserID = @UserId AND ur.RoleID = @RoleId))
	BEGIN
		IF (@TranStarted = 1)
			ROLLBACK TRANSACTION
		RETURN(3)
	END

	INSERT INTO dbo.b_UsersInRoles (UserID, RoleID, ActiveFrom, ActiveTo)
	VALUES (@UserId, @RoleId, @ActiveFrom, @ActiveTo)

	IF (@TranStarted = 1)
		COMMIT TRANSACTION

	RETURN(0)
END
GO


/****** Object:  StoredProcedure [Roles_AddUsersToRoles]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_AddUsersToRoles', N'P') IS NOT NULL DROP PROCEDURE [Roles_AddUsersToRoles]
GO
CREATE PROCEDURE [Roles_AddUsersToRoles]
	@UserNames		  nvarchar(4000),
	@RoleNames		  nvarchar(4000),
	@CurrentTimeUtc   datetime
AS
BEGIN
	DECLARE @TranStarted   bit
	SET @TranStarted = 0

	IF( @@TRANCOUNT = 0 )
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END

	DECLARE @tbNames	table(Name nvarchar(256) NOT NULL PRIMARY KEY)
	DECLARE @tbRoles	table(RoleId int NOT NULL PRIMARY KEY)
	DECLARE @tbUsers	table(UserId int NOT NULL PRIMARY KEY)
	DECLARE @Num		int
	DECLARE @Pos		int
	DECLARE @NextPos	int
	DECLARE @Name		nvarchar(256)

	SET @Num = 0
	SET @Pos = 1
	WHILE (@Pos <= LEN(@RoleNames))
	BEGIN
		SELECT @NextPos = CHARINDEX(N',', @RoleNames,  @Pos)
		IF (@NextPos = 0 OR @NextPos IS NULL)
			SELECT @NextPos = LEN(@RoleNames) + 1
		SELECT @Name = RTRIM(LTRIM(SUBSTRING(@RoleNames, @Pos, @NextPos - @Pos)))
		SELECT @Pos = @NextPos+1

		INSERT INTO @tbNames VALUES (@Name)
		SET @Num = @Num + 1
	END

	INSERT INTO @tbRoles
	  SELECT r.ID
	  FROM   [b_Roles] r, @tbNames t
	  WHERE  LOWER(t.Name) = r.LoweredRoleName

	IF (@@ROWCOUNT <> @Num)
	BEGIN
		SELECT TOP 1 Name
		FROM   @tbNames
		WHERE  LOWER(Name) NOT IN (
				SELECT r.LoweredRoleName 
				FROM [b_Roles] r, @tbRoles tr 
				WHERE tr.RoleId = r.ID)
		IF( @TranStarted = 1 )
			ROLLBACK TRANSACTION
		RETURN(2)
	END

	DELETE FROM @tbNames WHERE 1=1
	SET @Num = 0
	SET @Pos = 1

	WHILE (@Pos <= LEN(@UserNames))
	BEGIN
		SELECT @NextPos = CHARINDEX(N',', @UserNames,  @Pos)
		IF (@NextPos = 0 OR @NextPos IS NULL)
			SELECT @NextPos = LEN(@UserNames) + 1
		SELECT @Name = RTRIM(LTRIM(SUBSTRING(@UserNames, @Pos, @NextPos - @Pos)))
		SELECT @Pos = @NextPos+1

		INSERT INTO @tbNames VALUES (@Name)
		SET @Num = @Num + 1
	END

	INSERT INTO @tbUsers
	  SELECT u.ID
	  FROM   dbo.b_users u, @tbNames t
	  WHERE  LOWER(t.Name) = u.LoweredUserName

	IF (@@ROWCOUNT <> @Num)
	BEGIN
		SELECT TOP 1 Name
		FROM   @tbNames
		WHERE  LOWER(Name) NOT IN (
				SELECT u.LoweredUserName
				FROM dbo.b_users u, @tbUsers tu 
				WHERE tu.UserId = u.ID)
		IF( @TranStarted = 1 )
			ROLLBACK TRANSACTION
		RETURN(1)
	END

	IF (EXISTS (SELECT * FROM dbo.b_UsersInRoles ur, @tbUsers tu, @tbRoles tr WHERE tu.UserId = ur.UserID AND tr.RoleId = ur.RoleID))
	BEGIN
		SELECT TOP 1 UserName, RoleName
		FROM dbo.b_UsersInRoles ur, @tbUsers tu, @tbRoles tr, dbo.b_users u, [b_Roles] r
		WHERE u.ID = tu.UserId 
			AND r.ID = tr.RoleId
			AND tu.UserId = ur.UserId
			AND tr.RoleId = ur.RoleId

		IF (@TranStarted = 1)
			ROLLBACK TRANSACTION
		RETURN(3)
	END

	INSERT INTO dbo.b_UsersInRoles (UserID, RoleID)
	SELECT UserId, RoleId
	FROM @tbUsers, @tbRoles
	WHERE NOT RoleId IN (2, 3)

	IF (@TranStarted = 1)
		COMMIT TRANSACTION
	RETURN(0)
END
GO


/****** Object:  StoredProcedure [Roles_GetRolesInRole]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_GetRolesInRole', N'P') IS NOT NULL DROP PROCEDURE [Roles_GetRolesInRole]
GO
CREATE PROCEDURE [Roles_GetRolesInRole] 
	@RoleName nvarchar(256)
AS
BEGIN

     DECLARE @RoleId int
     SELECT @RoleId = NULL

     SELECT @RoleId = ID
     FROM [b_Roles]
     WHERE LOWER(@RoleName) = LoweredRoleName

     IF (@RoleId IS NULL)
         RETURN(2)

    SELECT r.RoleName
    FROM [b_Roles] r, dbo.b_RolesInRoles rr
    WHERE r.ID = rr.SubRoleID
		AND @RoleId = rr.RoleID
    ORDER BY r.RoleName

    RETURN(0)

END
GO


/****** Object:  StoredProcedure [Roles_FillEffectivePolicy]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_FillEffectivePolicy', N'P') IS NOT NULL DROP PROCEDURE [Roles_FillEffectivePolicy]
GO
CREATE PROCEDURE [Roles_FillEffectivePolicy]
AS
BEGIN

	/*
	Fill effective policy of all roles where effective policy is not set
	*/

	DECLARE @TranStarted bit
	SET @TranStarted = 0

	IF (@@TRANCOUNT = 0)
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END

	DECLARE @RoleId int
	DECLARE @Policy xml

	DECLARE @tbRoles table(RoleId int NOT NULL, Policy xml NULL, PRIMARY KEY (RoleId))

	INSERT INTO @tbRoles(RoleId, Policy)
	SELECT r.ID, r.Policy
	FROM [b_Roles] r
	WHERE r.EffectivePolicy IS NULL
		AND NOT EXISTS(
			SELECT 'x'
			FROM b_RolesInRoles rir, [b_Roles] r1
			WHERE rir.RoleID = r.ID
				AND rir.SubRoleID = r1.ID
				AND r1.EffectivePolicy IS NULL
		)

	WHILE (EXISTS( SELECT RoleId FROM @tbRoles ))
	BEGIN

		DECLARE roles_cursor CURSOR LOCAL FOR
		SELECT tr.RoleId, r.EffectivePolicy
		FROM @tbRoles tr
			LEFT JOIN b_RolesInRoles rir ON (tr.RoleId = rir.RoleID)
			LEFT JOIN [b_Roles] r ON (rir.SubRoleID = r.ID)

		OPEN roles_cursor

		FETCH NEXT FROM roles_cursor INTO @RoleId, @Policy

		WHILE @@FETCH_STATUS = 0
		BEGIN

			UPDATE @tbRoles SET
				Policy = dbo.Roles_ComputePolicy(Policy, @Policy)
			WHERE RoleId = @RoleId

			FETCH NEXT FROM roles_cursor INTO @RoleId, @Policy
		END

		CLOSE roles_cursor
		DEALLOCATE roles_cursor

		UPDATE [b_Roles] SET
			EffectivePolicy = tr.Policy
		FROM [b_Roles] r, @tbRoles tr
		WHERE r.ID = tr.RoleId


		DELETE FROM @tbRoles WHERE 1 = 1

		INSERT INTO @tbRoles(RoleId, Policy)
		SELECT r.ID, r.Policy
		FROM [b_Roles] r
		WHERE r.EffectivePolicy IS NULL
			AND NOT EXISTS(
				SELECT 'x'
				FROM b_RolesInRoles rir, [b_Roles] r1
				WHERE rir.RoleID = r.ID
					AND rir.SubRoleID = r1.ID
					AND r1.EffectivePolicy IS NULL
			)

	END

	IF (@TranStarted = 1)
		COMMIT TRANSACTION

	RETURN(0)
END
GO


/****** Object:  StoredProcedure [Roles_DropEffectivePolicy]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_DropEffectivePolicy', N'P') IS NOT NULL DROP PROCEDURE [Roles_DropEffectivePolicy]
GO
CREATE PROCEDURE [Roles_DropEffectivePolicy]
	@RoleId int
AS
BEGIN

	/*
	Null effective policy of all roles that depend on @RoleId role
	*/

	WITH tmp_Roles(ID, Way) AS (
		SELECT ID, ',' + CAST(ID AS VARCHAR(MAX)) + ',' AS Way
		FROM [b_Roles] AS r
		WHERE (ID = @RoleId)
		UNION ALL
		SELECT r.ID, tr.Way + CAST(r.ID AS VARCHAR(MAX)) + ',' AS Way
		FROM dbo.b_RolesInRoles AS rir
			INNER JOIN [b_Roles] AS r ON r.ID = rir.RoleID
			INNER JOIN tmp_Roles AS tr ON tr.ID = rir.SubRoleID
		WHERE (tr.Way NOT LIKE '%,' + CAST(r.ID AS VARCHAR(MAX)) + ',%')
	)
	UPDATE [b_Roles] SET
		EffectivePolicy = NULL
	FROM [b_Roles] ur, tmp_Roles utr
	WHERE ur.ID = utr.ID

	RETURN
END
GO


/****** Object:  StoredProcedure [Roles_GetEffectivePolicyForUser]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_GetEffectivePolicyForUser', N'P') IS NOT NULL DROP PROCEDURE [Roles_GetEffectivePolicyForUser]
GO
CREATE PROCEDURE [Roles_GetEffectivePolicyForUser] 
    @UserName nvarchar(256),
    @ProviderName nvarchar(128)
AS
BEGIN
    DECLARE @UserId int
    SELECT @UserId = NULL

	DECLARE @ResultPolicy xml
	SELECT @ResultPolicy = NULL

	IF (@ProviderName IS NULL)
	BEGIN
		SELECT @ProviderName = 'self'
	END

    SELECT @UserId = ID
    FROM dbo.b_Users
    WHERE LoweredUserName = LOWER(@UserName)
		AND LoweredProviderName = LOWER(@ProviderName)

    IF (@UserId IS NULL)
        RETURN(1)

	DECLARE @Policy xml

	DECLARE roles_cursor CURSOR LOCAL FOR
	
	SELECT r.EffectivePolicy 
	FROM dbo.[b_Roles] r INNER JOIN dbo.b_UsersInRoles ur ON (r.ID = ur.RoleID) 
	WHERE ur.UserID = @UserId AND r.ID NOT IN (2, 3) AND (ur.ActiveFrom IS NULL OR ur.ActiveFrom <= GETDATE()) AND (ur.ActiveTo IS NULL OR ur.ActiveTo >= GETDATE())
	UNION ALL
	SELECT r.EffectivePolicy FROM dbo.[b_Roles] r LEFT JOIN dbo.b_UsersInRoles ur ON (r.ID = ur.RoleID AND ur.UserID = @UserId)
	WHERE ID IN (2, 3) AND (ur.ActiveFrom IS NULL OR ur.ActiveFrom <= GETDATE()) AND (ur.ActiveTo IS NULL OR ur.ActiveTo >= GETDATE())

	OPEN roles_cursor

	FETCH NEXT FROM roles_cursor INTO @Policy

	WHILE @@FETCH_STATUS = 0
	BEGIN

		SELECT @ResultPolicy = dbo.Roles_ComputePolicyL(@ResultPolicy, @Policy)

		FETCH NEXT FROM roles_cursor INTO @Policy
	END

	CLOSE roles_cursor
	DEALLOCATE roles_cursor

	SELECT @ResultPolicy

	RETURN(0)
END
GO


/****** Object:  StoredProcedure [Roles_RemoveUsersFromRoles]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_RemoveUsersFromRoles', N'P') IS NOT NULL DROP PROCEDURE [Roles_RemoveUsersFromRoles]
GO
CREATE PROCEDURE [Roles_RemoveUsersFromRoles]
	@UserNames nvarchar(4000),
	@RoleNames nvarchar(4000)
AS
BEGIN
	DECLARE @TranStarted bit
	SET @TranStarted = 0

	IF (@@TRANCOUNT = 0)
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END

	DECLARE @tbNames table(Name nvarchar(256) NOT NULL PRIMARY KEY)
	DECLARE @tbRoles table(RoleId int NOT NULL PRIMARY KEY)
	DECLARE @tbUsers table(UserId int NOT NULL PRIMARY KEY)
	DECLARE @Num int
	DECLARE @Pos int
	DECLARE @NextPos int
	DECLARE @Name nvarchar(256)
	DECLARE @CountAll int
	DECLARE @CountU int
	DECLARE @CountR int


	SET @Num = 0
	SET @Pos = 1
	WHILE (@Pos <= LEN(@RoleNames))
	BEGIN
		SELECT @NextPos = CHARINDEX(N',', @RoleNames,  @Pos)
		IF (@NextPos = 0 OR @NextPos IS NULL)
			SELECT @NextPos = LEN(@RoleNames) + 1
		SELECT @Name = RTRIM(LTRIM(SUBSTRING(@RoleNames, @Pos, @NextPos - @Pos)))
		SELECT @Pos = @NextPos + 1

		INSERT INTO @tbNames VALUES (@Name)
		SET @Num = @Num + 1
	END

	INSERT INTO @tbRoles
	SELECT ID
	FROM [b_Roles] ar, @tbNames t
	WHERE LOWER(t.Name) = ar.LoweredRoleName

	SELECT @CountR = @@ROWCOUNT

	IF (@CountR <> @Num)
	BEGIN
		SELECT TOP 1 N'', Name
		FROM @tbNames
		WHERE LOWER(Name) NOT IN (
			SELECT ar.LoweredRoleName
			FROM [b_Roles] ar, @tbRoles r
			WHERE r.RoleId = ar.ID)

		IF (@TranStarted = 1)
			ROLLBACK TRANSACTION
		RETURN(2)
	END


	DELETE FROM @tbNames
	WHERE 1=1


	SET @Num = 0
	SET @Pos = 1


	WHILE (@Pos <= LEN(@UserNames))
	BEGIN
		SELECT @NextPos = CHARINDEX(N',', @UserNames,  @Pos)
		IF (@NextPos = 0 OR @NextPos IS NULL)
			SELECT @NextPos = LEN(@UserNames) + 1
		SELECT @Name = RTRIM(LTRIM(SUBSTRING(@UserNames, @Pos, @NextPos - @Pos)))
		SELECT @Pos = @NextPos+1

		INSERT INTO @tbNames VALUES (@Name)
		SET @Num = @Num + 1
	END

	INSERT INTO @tbUsers
	SELECT ID
	FROM dbo.b_users ar, @tbNames t
	WHERE LOWER(t.Name) = ar.LoweredUserName

	SELECT @CountU = @@ROWCOUNT

	IF (@CountU <> @Num)
	BEGIN
		SELECT TOP 1 Name, N''
		FROM @tbNames
		WHERE LOWER(Name) NOT IN (
			SELECT au.LoweredUserName
			FROM dbo.b_users au, @tbUsers u
			WHERE u.UserId = au.ID)

		IF (@TranStarted = 1)
			ROLLBACK TRANSACTION
		RETURN(1)
	END

	SELECT @CountAll = COUNT(*)
	FROM dbo.b_UsersInRoles ur, @tbUsers u, @tbRoles r
	WHERE ur.UserID = u.UserId
		AND ur.RoleID = r.RoleId

	IF (@CountAll <> @CountU * @CountR)
	BEGIN
		SELECT TOP 1 UserName, RoleName
		FROM @tbUsers tu, @tbRoles tr, dbo.b_users u, [b_Roles] r
		WHERE u.ID = tu.UserId
			AND r.ID = tr.RoleId
			AND tu.UserId NOT IN (
				SELECT ur.UserID 
				FROM dbo.b_UsersInRoles ur 
				WHERE ur.RoleID = tr.RoleId)
			AND tr.RoleId NOT IN (
				SELECT ur.RoleID 
				FROM dbo.b_UsersInRoles ur 
				WHERE ur.UserID = tu.UserId)

		IF (@TranStarted = 1)
			ROLLBACK TRANSACTION
		RETURN(6)
	END

	DELETE FROM dbo.b_UsersInRoles
	WHERE UserID IN (SELECT UserId FROM @tbUsers)
		AND RoleID IN (SELECT RoleId FROM @tbRoles)

	IF (@TranStarted = 1)
		COMMIT TRANSACTION

	RETURN(0)
END
GO


/****** Object:  StoredProcedure [Roles_SynchronizeProviderUserRoles]    Script Date: 02/25/2010 09:38:28 ******/
IF OBJECT_ID(N'Roles_SynchronizeProviderUserRoles', N'P') IS NOT NULL DROP PROCEDURE [Roles_SynchronizeProviderUserRoles]
GO
CREATE PROCEDURE [Roles_SynchronizeProviderUserRoles]
	@UserName nvarchar(256),
	@ProviderName varchar(128),
	@RoleNames nvarchar(4000)
AS
BEGIN
	DECLARE @TranStarted bit
	SET @TranStarted = 0

	IF (@@TRANCOUNT = 0)
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END

	DECLARE @tbNames table(FName nvarchar(128) NOT NULL PRIMARY KEY)

	DECLARE @Num int
	DECLARE @Pos int
	DECLARE @NextPos int
	DECLARE @Name nvarchar(128)

	SET @Num = 0
	SET @Pos = 1
	WHILE (@Pos <= LEN(@RoleNames))
	BEGIN
		SELECT @NextPos = CHARINDEX(N',', @RoleNames,  @Pos)
		IF (@NextPos = 0 OR @NextPos IS NULL)
			SELECT @NextPos = LEN(@RoleNames) + 1
		SELECT @Name = RTRIM(LTRIM(SUBSTRING(@RoleNames, @Pos, @NextPos - @Pos)))
		SELECT @Pos = @NextPos + 1

		INSERT INTO @tbNames VALUES (@ProviderName + N'\' + @Name)
		SET @Num = @Num + 1
	END

	DECLARE @UserId int

	SELECT @UserId = u.ID
	FROM b_Users u
	WHERE u.LoweredUserName = LOWER(@UserName)
		AND u.LoweredProviderName = LOWER(@ProviderName)

    IF (@UserId IS NULL)
        RETURN(1)

	DELETE FROM b_UsersInRoles
	WHERE UserID = @UserId
		AND RoleID IN (
			SELECT r.ID
			FROM [b_Roles] r
				LEFT JOIN @tbNames n ON (r.LoweredRoleName = LOWER(n.FName))
			WHERE r.LoweredRoleName LIKE LOWER(@ProviderName) + N'\%'
				AND n.FName IS NULL
		)

	INSERT INTO [b_Roles](RoleName, LoweredRoleName, Active, Comment, Policy, EffectivePolicy)
	SELECT t.FName, LOWER(t.FName), 1, null, '<Policy/>', null
	FROM @tbNames t
	WHERE LOWER(t.FName) NOT IN (SELECT r.LoweredRoleName FROM [b_Roles] r)


	INSERT INTO b_UsersInRoles (UserID, RoleID)
	SELECT @UserId, r.ID
	FROM [b_Roles] r, @tbNames n
	WHERE r.LoweredRoleName = LOWER(n.FName)
		AND r.ID NOT IN (SELECT uir.RoleID FROM b_UsersInRoles uir WHERE uir.UserID = @UserId)

	IF (@TranStarted = 1)
		COMMIT TRANSACTION

	RETURN(0)
END
GO


/****** Object:  StoredProcedure [Roles_GetRolesForUser]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_GetRolesForUser', N'P') IS NOT NULL DROP PROCEDURE [Roles_GetRolesForUser]
GO
CREATE PROCEDURE [Roles_GetRolesForUser]
	@UserName nvarchar(256) = NULL,
	@ProviderName nvarchar(128) = NULL,
	@RecursiveCheck bit = 1,
	@AllRoles bit = 0
AS
BEGIN
    DECLARE @UserId int
    SELECT @UserId = NULL

	IF (@UserName IS NULL)
	BEGIN
		IF (@RecursiveCheck = 0)
		BEGIN

			SELECT r.RoleName
			FROM [b_Roles] r
			WHERE r.ID = 2
			ORDER BY r.RoleName

		END
		ELSE
		BEGIN

			WITH tmp_Roles(ID, RoleName, Way) AS (
				SELECT r.ID, r.RoleName, ',' + CAST(r.ID AS VARCHAR(MAX)) + ',' AS Way
				FROM [b_Roles] r
				WHERE r.ID = 2
				UNION ALL
				SELECT r.ID, r.RoleName, tr.Way + CAST(r.ID AS VARCHAR(MAX)) + ',' AS Way
				FROM dbo.b_RolesInRoles rir
					INNER JOIN [b_Roles] r ON r.ID = rir.SubRoleID
					INNER JOIN tmp_Roles tr ON tr.ID = rir.RoleID
				WHERE (tr.Way NOT LIKE '%,' + CAST(r.ID AS VARCHAR(MAX)) + ',%'))
			SELECT DISTINCT tmpr.RoleName
			FROM tmp_Roles tmpr

		END

		RETURN (0)
	END

	IF (@ProviderName IS NULL)
	BEGIN
		SELECT @ProviderName = 'self'
	END

    SELECT @UserId = ID
    FROM dbo.b_Users
    WHERE LoweredUserName = LOWER(@UserName)
		AND LoweredProviderName = LOWER(@ProviderName)

    IF (@UserId IS NULL)
        RETURN(1)

	IF (@RecursiveCheck = 0)
	BEGIN

		IF (@AllRoles = 0)
		BEGIN
			SELECT r.RoleName
			FROM [b_Roles] r
			WHERE r.ID IN (2, 3) OR r.ID IN (
				SELECT ur.RoleID
				FROM dbo.b_UsersInRoles ur
				WHERE ur.UserID = @UserId
					AND (ur.ActiveFrom IS NULL OR ur.ActiveFrom <= GETDATE())
					AND (ur.ActiveTo IS NULL OR ur.ActiveTo >= GETDATE())
				)
			ORDER BY r.RoleName
		END
		ELSE
		BEGIN
			SELECT r.RoleName, ur.ActiveFrom, ur.ActiveTo
			FROM [b_Roles] r
				LEFT JOIN dbo.b_UsersInRoles ur ON (r.ID = ur.RoleID)
			WHERE r.ID IN (2, 3)
				OR ur.UserID = @UserId
			ORDER BY r.RoleName
		END
	END
	ELSE
	BEGIN

		WITH tmp_Roles(ID, RoleName, Way) AS (
			SELECT r.ID, r.RoleName, ',' + CAST(r.ID AS VARCHAR(MAX)) + ',' AS Way
			FROM [b_Roles] r
				LEFT JOIN dbo.b_UsersInRoles ur ON (r.ID = ur.RoleID)
			WHERE r.ID IN (2, 3)
				OR (ur.UserID = @UserId
				AND (ur.ActiveFrom IS NULL OR ur.ActiveFrom <= GETDATE())
				AND (ur.ActiveTo IS NULL OR ur.ActiveTo >= GETDATE()))
			UNION ALL
			SELECT r.ID, r.RoleName, tr.Way + CAST(r.ID AS VARCHAR(MAX)) + ',' AS Way
			FROM dbo.b_RolesInRoles rir
				INNER JOIN [b_Roles] r ON r.ID = rir.SubRoleID
				INNER JOIN tmp_Roles tr ON tr.ID = rir.RoleID
			WHERE (tr.Way NOT LIKE '%,' + CAST(r.ID AS VARCHAR(MAX)) + ',%'))
		SELECT DISTINCT tmpr.RoleName
		FROM tmp_Roles tmpr
	END

    RETURN (0)
END
GO


/****** Object:  StoredProcedure [Roles_GetAllRolesForUser]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_GetAllRolesForUser', N'P') IS NOT NULL DROP PROCEDURE [Roles_GetAllRolesForUser]
GO
CREATE PROCEDURE [Roles_GetAllRolesForUser]
	@UserName nvarchar(256) = NULL,
	@ProviderName nvarchar(128) = NULL,
	@RecursiveCheck bit = 1,
	@AllRoles bit = 0
AS
BEGIN
    DECLARE @UserId int
    SELECT @UserId = NULL

	IF (@UserName IS NULL)
	BEGIN
		IF (@RecursiveCheck = 0)
		BEGIN

			SELECT r.RoleName
			FROM [b_Roles] r
			WHERE r.ID = 2
			ORDER BY r.RoleName

		END
		ELSE
		BEGIN

			WITH tmp_Roles(ID, RoleName, Way) AS (
				SELECT r.ID, r.RoleName, ',' + CAST(r.ID AS VARCHAR(MAX)) + ',' AS Way
				FROM [b_Roles] r
				WHERE r.ID = 2
				UNION ALL
				SELECT r.ID, r.RoleName, tr.Way + CAST(r.ID AS VARCHAR(MAX)) + ',' AS Way
				FROM dbo.b_RolesInRoles rir
					INNER JOIN [b_Roles] r ON r.ID = rir.SubRoleID
					INNER JOIN tmp_Roles tr ON tr.ID = rir.RoleID
				WHERE (tr.Way NOT LIKE '%,' + CAST(r.ID AS VARCHAR(MAX)) + ',%'))
			SELECT DISTINCT tmpr.RoleName
			FROM tmp_Roles tmpr

		END

		RETURN (0)
	END

	IF (@ProviderName IS NULL)
	BEGIN
		SELECT @ProviderName = 'self'
	END

    SELECT @UserId = ID
    FROM dbo.b_Users
    WHERE LoweredUserName = LOWER(@UserName)
		AND LoweredProviderName = LOWER(@ProviderName)

    IF (@UserId IS NULL)
        RETURN(1)

	IF (@RecursiveCheck = 0)
	BEGIN

		IF (@AllRoles = 0)
		BEGIN
			SELECT r.RoleName
			FROM [b_Roles] r
			WHERE r.ID IN (2, 3) OR r.ID IN (
				SELECT ur.RoleID
				FROM dbo.b_UsersInRoles ur
				WHERE ur.UserID = @UserId
					AND (ur.ActiveFrom IS NULL OR ur.ActiveFrom <= GETDATE())
					AND (ur.ActiveTo IS NULL OR ur.ActiveTo >= GETDATE())
				)
			ORDER BY r.RoleName
		END
		ELSE
		BEGIN
			SELECT r.RoleName, ur.ActiveFrom, ur.ActiveTo
			FROM [b_Roles] r
				LEFT JOIN dbo.b_UsersInRoles ur ON (r.ID = ur.RoleID)
			WHERE r.ID IN (2, 3)
				OR ur.UserID = @UserId
			ORDER BY r.RoleName
		END
	END
	ELSE
	BEGIN

		WITH tmp_Roles(ID, RoleName, Way) AS (
			SELECT r.ID, r.RoleName, ',' + CAST(r.ID AS VARCHAR(MAX)) + ',' AS Way
			FROM [b_Roles] r
				LEFT JOIN dbo.b_UsersInRoles ur ON (r.ID = ur.RoleID)
			WHERE r.ID IN (2, 3)
				OR (ur.UserID = @UserId
				AND (ur.ActiveFrom IS NULL OR ur.ActiveFrom <= GETDATE())
				AND (ur.ActiveTo IS NULL OR ur.ActiveTo >= GETDATE()))
			UNION ALL
			SELECT r.ID, r.RoleName, tr.Way + CAST(r.ID AS VARCHAR(MAX)) + ',' AS Way
			FROM dbo.b_RolesInRoles rir
				INNER JOIN [b_Roles] r ON r.ID = rir.SubRoleID
				INNER JOIN tmp_Roles tr ON tr.ID = rir.RoleID
			WHERE (tr.Way NOT LIKE '%,' + CAST(r.ID AS VARCHAR(MAX)) + ',%'))
		SELECT DISTINCT tmpr.RoleName
		FROM tmp_Roles tmpr

	END

    RETURN (0)
END
GO


/****** Object:  StoredProcedure [Roles_GetRoleByName]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_GetRoleByName', N'P') IS NOT NULL DROP PROCEDURE [Roles_GetRoleByName]
GO
CREATE PROCEDURE [Roles_GetRoleByName]
    @RoleName nvarchar(128)
AS
BEGIN

	SELECT r.RoleName, r.ID, r.Active, r.Comment, r.Policy, r.EffectivePolicy,
		(
		SELECT COUNT(*) 
		FROM b_UsersInRoles uir 
		WHERE uir.RoleID = r.ID 
		) as UsersCount,
		(
		SELECT COUNT(*) 
		FROM b_RolesInRoles rir 
		WHERE rir.RoleID = r.ID 
		) as RolesCount
	FROM [b_Roles] r
	WHERE r.RoleName = @RoleName

    IF (@@ROWCOUNT = 0)
       RETURN -1

    RETURN 0
END
GO


/****** Object:  StoredProcedure [Roles_GetRoleByID]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_GetRoleByID', N'P') IS NOT NULL DROP PROCEDURE [Roles_GetRoleByID]
GO
CREATE PROCEDURE [Roles_GetRoleByID] 
    @RoleId int
AS
BEGIN

	SELECT r.RoleName, r.ID, r.Active, r.Comment, r.Policy, r.EffectivePolicy,
		(
		SELECT COUNT(*) 
		FROM b_UsersInRoles uir 
		WHERE uir.RoleID = r.ID 
		) as UsersCount,
		(
		SELECT COUNT(*) 
		FROM b_RolesInRoles rir 
		WHERE rir.RoleID = r.ID 
		) as RolesCount
	FROM [b_Roles] r
	WHERE r.ID = @RoleId

    IF (@@ROWCOUNT = 0)
       RETURN -1

    RETURN 0
END
GO


/****** Object:  StoredProcedure [Roles_IsUserInRole]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_IsUserInRole', N'P') IS NOT NULL DROP PROCEDURE [Roles_IsUserInRole]
GO
CREATE PROCEDURE [Roles_IsUserInRole]
    @UserName nvarchar(256),
    @RoleName nvarchar(256),
    @RecursiveCheck bit = 1,
    @ProviderName nvarchar(128) = null
AS
BEGIN
    DECLARE @UserId int
    SELECT @UserId = NULL

    DECLARE @RoleId int
    SELECT @RoleId = NULL

	IF (@ProviderName IS NULL)
	BEGIN
		SELECT @ProviderName = 'self'
	END

    SELECT @UserId = ID
    FROM dbo.b_Users
    WHERE LoweredUserName = LOWER(@UserName)
		AND LoweredProviderName = LOWER(@ProviderName)

    IF (@UserId IS NULL)
        RETURN(2)

    SELECT @RoleId = ID
    FROM [b_Roles]
    WHERE LoweredRoleName = LOWER(@RoleName)

    IF (@RoleId IS NULL)
        RETURN(3)

	IF (@RecursiveCheck = 0)
	BEGIN
		IF (EXISTS( 
			SELECT * 
			FROM dbo.b_UsersInRoles 
			WHERE UserID = @UserId 
				AND RoleID = @RoleId
				AND (ActiveFrom IS NULL OR ActiveFrom <= GETDATE())
				AND (ActiveTo IS NULL OR ActiveTo >= GETDATE())
			))
			RETURN(1)
		ELSE
			RETURN(0)
	END
	ELSE
	BEGIN
	
		DECLARE @cnt int
		SELECT @cnt = null;
	
		WITH tmp_Roles(ID, Way) AS (
			SELECT ID, ',' + CAST(ID AS VARCHAR(MAX)) + ',' AS Way
			FROM [b_Roles] AS r
			WHERE (ID = @RoleId)
			UNION ALL
			SELECT r.ID, tr.Way + CAST(r.ID AS VARCHAR(MAX)) + ',' AS Way
			FROM dbo.b_RolesInRoles AS rir
				INNER JOIN [b_Roles] AS r ON r.ID = rir.RoleID
				INNER JOIN tmp_Roles AS tr ON tr.ID = rir.SubRoleID
			WHERE (tr.Way NOT LIKE '%,' + CAST(r.ID AS VARCHAR(MAX)) + ',%'))
		SELECT @cnt = count(*)
		FROM tmp_Roles tmpr, dbo.b_UsersInRoles uir
		WHERE uir.UserID = @UserId
			AND tmpr.ID = uir.RoleID
			AND (uir.ActiveFrom IS NULL OR uir.ActiveFrom <= GETDATE())
			AND (uir.ActiveTo IS NULL OR uir.ActiveTo >= GETDATE())
	
		IF (@cnt IS NOT NULL AND @cnt > 0)
			RETURN(1)
		ELSE
			RETURN(0)
	END
END
GO


/****** Object:  StoredProcedure [Roles_GetUserRoles]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_GetUserRoles', N'P') IS NOT NULL DROP PROCEDURE [Roles_GetUserRoles]
GO
CREATE PROCEDURE [Roles_GetUserRoles]
	@UserId int = NULL,
	@UserName nvarchar(256) = NULL,
	@ProviderName nvarchar(128) = NULL,
	@RecursiveCheck bit = 1
AS
BEGIN

	IF (@ProviderName IS NULL)
	BEGIN
		SELECT @ProviderName = 'self'
	END

	IF (@UserId IS NULL OR @UserId = 0)
	BEGIN
		SELECT @UserId = NULL
	END

	IF (@UserId IS NULL AND @UserName IS NOT NULL)
	BEGIN
		SELECT @UserId = u.ID
		FROM b_Users u
		WHERE u.LoweredUserName = LOWER(@UserName)
			AND u.LoweredProviderName = LOWER(@ProviderName)
	END

	IF (@RecursiveCheck = 0)
	BEGIN

		SELECT r.ID, r.RoleName, r.Title, r.Active, r.Comment, r.Policy, r.EffectivePolicy, (SELECT COUNT(*) FROM b_UsersInRoles uir WHERE uir.RoleID = r.ID) as UsersCount, (SELECT COUNT(*) FROM b_RolesInRoles rir WHERE rir.RoleID = r.ID) as RolesCount
		FROM dbo.b_Roles r
		WHERE r.Active = 1
			AND (r.ID = 2
				OR @UserId IS NOT NULL 
					AND (r.ID IN (
						SELECT ur.RoleID
						FROM dbo.b_UsersInRoles ur
						WHERE ur.UserID = @UserId
							AND (ur.ActiveFrom IS NULL OR ur.ActiveFrom <= GETDATE())
							AND (ur.ActiveTo IS NULL OR ur.ActiveTo >= GETDATE())
					) OR r.ID = 3)
				)
		ORDER BY r.RoleName

	END
	ELSE
	BEGIN

		WITH tmp_Roles(ID, RoleName, Title, Active, Way) AS (
			SELECT r.ID, r.RoleName, r.Title, r.Active, ',' + CAST(r.ID AS VARCHAR(MAX)) + ',' AS Way
			FROM dbo.b_Roles r
				LEFT JOIN dbo.b_UsersInRoles ur ON (r.ID = ur.RoleID)
			WHERE r.Active = 1
				AND (r.ID = 2
					OR (@UserId IS NOT NULL 
						AND (ur.UserID = @UserId
						AND (ur.ActiveFrom IS NULL OR ur.ActiveFrom <= GETDATE())
						AND (ur.ActiveTo IS NULL OR ur.ActiveTo >= GETDATE())
						OR r.ID = 3)
					)
				)
			UNION ALL
			SELECT r.ID, r.RoleName, r.Title, r.Active, tr.Way + CAST(r.ID AS VARCHAR(MAX)) + ',' AS Way
			FROM dbo.b_RolesInRoles rir
				INNER JOIN dbo.b_Roles r ON r.ID = rir.SubRoleID
				INNER JOIN tmp_Roles tr ON tr.ID = rir.RoleID
			WHERE r.Active = 1 
				AND (tr.Way NOT LIKE '%,' + CAST(r.ID AS VARCHAR(MAX)) + ',%'))
		SELECT DISTINCT tmpr.ID, tmpr.RoleName, tmpr.Title, tmpr.Active, null, null, null, 0, 0
		FROM tmp_Roles tmpr

	END

    RETURN (0)
END
GO


/****** Object:  StoredProcedure [Roles_RemoveUserFromRole]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_RemoveUserFromRole', N'P') IS NOT NULL DROP PROCEDURE [Roles_RemoveUserFromRole]
GO
CREATE PROCEDURE [Roles_RemoveUserFromRole]
	@UserName nvarchar(256),
	@ProviderName nvarchar(128),
	@RoleName nvarchar(128)
AS
BEGIN
	DECLARE @TranStarted bit
	SET @TranStarted = 0

	IF (@@TRANCOUNT = 0)
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END

    DECLARE @UserId int
    SELECT @UserId = NULL

	IF (@ProviderName IS NULL)
	BEGIN
		SELECT @ProviderName = 'self'
	END

    SELECT @UserId = ID
    FROM dbo.b_Users
    WHERE LoweredUserName = LOWER(@UserName)
		AND LoweredProviderName = LOWER(@ProviderName)

	IF (@UserId IS NULL)
	BEGIN
		IF (@TranStarted = 1)
			ROLLBACK TRANSACTION
		RETURN(1)
	END

	DECLARE @RoleId int
	SELECT @RoleId = NULL

	SELECT @RoleId = ID
	FROM [b_Roles]
	WHERE LOWER(@RoleName) = LoweredRoleName

	IF (@RoleId IS NULL)
	BEGIN
		IF (@TranStarted = 1)
			ROLLBACK TRANSACTION
		RETURN(2)
	END

	DELETE FROM dbo.b_UsersInRoles
	WHERE UserID = @UserId
		AND RoleID = @RoleId

	IF (@TranStarted = 1)
		COMMIT TRANSACTION

	RETURN(0)
END
GO


/****** Object:  StoredProcedure [Roles_GetUsersInRoles]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_GetUsersInRoles', N'P') IS NOT NULL DROP PROCEDURE [Roles_GetUsersInRoles]
GO
CREATE PROCEDURE [Roles_GetUsersInRoles]
    @RoleName nvarchar(256)
AS
BEGIN
	DECLARE @RoleId int
	SELECT @RoleId = NULL

	SELECT @RoleId = ID
	FROM [b_Roles]
	WHERE LOWER(@RoleName) = LoweredRoleName

	IF (@RoleId IS NULL)
         RETURN(2)

	SELECT u.UserName
	FROM dbo.b_Users u, dbo.b_UsersInRoles ur
	WHERE u.ID = ur.UserID
		AND @RoleId = ur.RoleID
		AND (ur.ActiveFrom IS NULL OR ur.ActiveFrom <= GETDATE())
		AND (ur.ActiveTo IS NULL OR ur.ActiveTo >= GETDATE())
	ORDER BY u.UserName

	RETURN(0)
END
GO


/****** Object:  StoredProcedure [File_Create]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'File_Create', N'P') IS NOT NULL DROP PROCEDURE [File_Create]
GO
CREATE PROCEDURE [File_Create]
	@ModuleId varchar(128),
	@Height int,
	@Width int,
	@FileSize int,
	@ContentType varchar(255),
	@Folder nvarchar(255),
	@FileName nvarchar(255),
	@FileNameOriginal nvarchar(255),
	@Description nvarchar(255)
AS
BEGIN
    DECLARE @FileId int
    SET @FileId = NULL
	
	INSERT INTO b_File (ModuleId, Height, Width, FileSize, ContentType, Folder, FileName, FileNameOriginal, Description)
	VALUES (@ModuleId, @Height, @Width, @FileSize, @ContentType, @Folder, @FileName, @FileNameOriginal, @Description)

	SELECT @FileId = @@IDENTITY
	
	RETURN @FileId
	
END
GO


/****** Object:  StoredProcedure [File_Delete]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'File_Delete', N'P') IS NOT NULL DROP PROCEDURE [File_Delete]
GO
CREATE PROCEDURE [File_Delete]
	@oldFileId int,
	@newFileId int
AS
BEGIN
	IF (@oldFileId IS NOT NULL AND ISNULL(@oldFileId, 0) <> isnull(@newFileId, 0))
	BEGIN
		DELETE FROM b_File WHERE ID = @oldFileId
	END
END
GO


/****** Object:  StoredProcedure [Users_DeleteUser]    Script Date: 02/25/2010 09:38:28 ******/
IF OBJECT_ID(N'Users_DeleteUser', N'P') IS NOT NULL DROP PROCEDURE [Users_DeleteUser]
GO
CREATE PROCEDURE [Users_DeleteUser]
	@UserId int,
	@DeleteAllRelatedData bit = 1,
	@NumTablesDeletedFrom int OUTPUT
AS
BEGIN
	SELECT @NumTablesDeletedFrom = 0

	DECLARE @TranStarted bit
	SET @TranStarted = 0

	IF (@@TRANCOUNT = 0)
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END
	ELSE
	BEGIN
		SET @TranStarted = 0
	END

	DECLARE @ErrorCode int
	DECLARE @RowCount int

	SET @ErrorCode = 0
	SET @RowCount  = 0

	IF (@UserId IS NULL OR @UserId = 0)
	BEGIN
		SET @ErrorCode = 1
		GOTO Cleanup
	END

	IF (@UserId = 1)
	BEGIN
		SET @ErrorCode = 50
		GOTO Cleanup
	END

	IF (@DeleteAllRelatedData = 1)
	BEGIN
		-- Delete all related data here
		DELETE FROM dbo.b_UsersControlsOptions
		WHERE UserID = @UserId

		SELECT @ErrorCode = @@ERROR,
			@RowCount = @@ROWCOUNT

		IF (@ErrorCode <> 0)
			GOTO Cleanup

		IF (@RowCount <> 0)
			SELECT @NumTablesDeletedFrom = @NumTablesDeletedFrom + 1
	END

	DELETE FROM b_UsersInRoles WHERE UserID = @UserId

	SELECT @ErrorCode = @@ERROR,
		@RowCount = @@ROWCOUNT

	IF (@ErrorCode <> 0)
		GOTO Cleanup

	IF (@RowCount <> 0)
		SELECT @NumTablesDeletedFrom = @NumTablesDeletedFrom + 1

	DECLARE @OldImageId table(ImageId int)
	DELETE FROM dbo.b_Users OUTPUT DELETED.ImageId INTO @OldImageId WHERE @UserId = ID

	SELECT @ErrorCode = @@ERROR,
			@RowCount = @@ROWCOUNT

	IF (@ErrorCode <> 0)
		GOTO Cleanup

	IF (@RowCount <> 0)
		SELECT @NumTablesDeletedFrom = @NumTablesDeletedFrom + 1
    
    
    DECLARE @DeleteId int
    SET @DeleteId = (SELECT ImageId FROM @OldImageId)
    
    IF (@DeleteId IS NOT NULL) BEGIN
		DELETE FROM dbo.b_File WHERE ID = @DeleteId
	END


	IF (@@ERROR <> 0)
        GOTO Cleanup

	IF (@TranStarted = 1)
	BEGIN
		SET @TranStarted = 0
		COMMIT TRANSACTION
	END

	RETURN 0

Cleanup:

	SET @NumTablesDeletedFrom = 0

	IF (@TranStarted = 1)
	BEGIN
		SET @TranStarted = 0
		ROLLBACK TRANSACTION
	END

	RETURN @ErrorCode

END
GO


/****** Object:  StoredProcedure [Users_UpdateUser]    Script Date: 02/25/2010 09:38:28 ******/
IF OBJECT_ID(N'Users_UpdateUser', N'P') IS NOT NULL DROP PROCEDURE [Users_UpdateUser]
GO
CREATE PROCEDURE [Users_UpdateUser]
    @UserId int,
    @UserName nvarchar(256),
    @Email nvarchar(256),
    @Comment ntext,
    @IsApproved bit,
    @LastLoginDate datetime,
    @LastActivityDate datetime,
    @UniqueEmail int,
    @CurrentTimeUtc datetime,
    @ProviderName nvarchar(128) = NULL,
    @FirstName nvarchar(50) = NULL,
    @SecondName nvarchar(50) = NULL,
    @LastName nvarchar(50) = NULL,
    @SiteId varchar(50) = NULL,
    @BirthdayDate datetime = NULL,
    @DisplayName nvarchar(256) = NULL,
    @ImageId int = NULL,
    @Gender char(1) = NULL,
    @ActivationToken nvarchar(15) = NULL
AS
BEGIN

	IF (@ProviderName IS NULL)
	BEGIN
		SELECT @ProviderName = 'self'
	END

	DECLARE @ID int

    SELECT @ID = u.ID
    FROM dbo.b_Users u
    WHERE u.LoweredUserName = LOWER(@UserName)
		AND u.LoweredProviderName = LOWER(@ProviderName)

    IF (@ID IS NULL OR @ID <> @UserId)
        RETURN(1)

    IF (@UniqueEmail = 1)
    BEGIN
        IF (EXISTS (SELECT *
                    FROM  dbo.b_Users WITH (UPDLOCK, HOLDLOCK)
                    WHERE @UserId <> ID
						AND LoweredEmail = LOWER(@Email)))
        BEGIN
            RETURN(7)
        END
    END

    DECLARE @TranStarted bit
    SET @TranStarted = 0

    IF (@@TRANCOUNT = 0)
    BEGIN
	    BEGIN TRANSACTION
	    SET @TranStarted = 1
    END
    ELSE
		SET @TranStarted = 0

	DECLARE @OldImageId table(ImageId int)
	
	UPDATE dbo.b_Users WITH (ROWLOCK) SET
		LastActivityDate = @LastActivityDate,
		Email = @Email,
		LoweredEmail = LOWER(@Email),
		Comment = @Comment,
		IsApproved = @IsApproved,
		LastLoginDate = @LastLoginDate,
		FirstName = @FirstName,
		SecondName = @SecondName,
		LastName = @LastName,
		SiteID = @SiteId,
		BirthdayDate = @BirthdayDate,
		DisplayName = @DisplayName,
		ImageId = @ImageId,
		Gender = @Gender,
		ActivationToken = @ActivationToken
	OUTPUT DELETED.ImageId INTO @OldImageId
	WHERE @UserId = ID

    IF (@@ERROR <> 0)
        GOTO Cleanup
    
    DECLARE @DeleteId int
    SET @DeleteId = (SELECT ImageId FROM @OldImageId WHERE ImageId <> @ImageId)
    
    IF (@DeleteId IS NOT NULL) BEGIN
		DELETE FROM dbo.b_File WHERE ID = @DeleteId
	END
        
	IF (@@ERROR <> 0)
        GOTO Cleanup

    IF (@TranStarted = 1)
    BEGIN
		SET @TranStarted = 0
		COMMIT TRANSACTION
    END

    RETURN 0

Cleanup:

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
    	ROLLBACK TRANSACTION
    END

    RETURN -1
END
GO


/****** Object:  StoredProcedure [Users_UnlockUser]    Script Date: 02/25/2010 09:38:28 ******/
IF OBJECT_ID(N'Users_UnlockUser', N'P') IS NOT NULL DROP PROCEDURE [Users_UnlockUser]
GO
CREATE PROCEDURE [Users_UnlockUser]
    @UserId int
AS
BEGIN
    IF (@UserId IS NULL)
        RETURN 1

	UPDATE dbo.b_users SET
		IsLockedOut = 0,
		FailedPasswordAttemptCount = 0,
		FailedPasswordAttemptWindowStart = CONVERT(datetime, '17540101', 112),
		FailedPasswordAnswerAttemptCount = 0,
		FailedPasswordAnswerAttemptWindowStart = CONVERT(datetime, '17540101', 112),
		LastLockoutDate = CONVERT(datetime, '17540101', 112)
	WHERE @UserId = ID

	IF (@@ROWCOUNT = 0)
    BEGIN
        RETURN(1)
    END

    RETURN 0
END
GO


/****** Object:  StoredProcedure [Users_ChangePasswordQuestionAndAnswer]    Script Date: 02/25/2010 09:38:28 ******/
IF OBJECT_ID(N'Users_ChangePasswordQuestionAndAnswer', N'P') IS NOT NULL DROP PROCEDURE [Users_ChangePasswordQuestionAndAnswer]
GO
CREATE PROCEDURE [Users_ChangePasswordQuestionAndAnswer]
    @UserId int,
    @NewPasswordQuestion nvarchar(256),
    @NewPasswordAnswer nvarchar(128)
AS
BEGIN
    IF (@UserId IS NULL)
    BEGIN
        RETURN(1)
    END

    UPDATE dbo.b_Users SET
		PasswordQuestion = @NewPasswordQuestion,
		PasswordAnswer = @NewPasswordAnswer
    WHERE ID = @UserId

	IF (@@ROWCOUNT = 0)
    BEGIN
        RETURN(1)
    END

    RETURN(0)
END
GO


/****** Object:  StoredProcedure [Users_ChangeCheckWord]    Script Date: 02/25/2010 09:38:28 ******/
IF OBJECT_ID(N'Users_ChangeCheckWord', N'P') IS NOT NULL DROP PROCEDURE [Users_ChangeCheckWord]
GO
CREATE PROCEDURE [Users_ChangeCheckWord]
	@UserId int,
	@NewCheckWord nvarchar(50),
	@CurrentTimeUtc datetime
AS
BEGIN
    
	IF (@UserId IS NULL OR @UserId = 0)
	BEGIN
		RETURN(1)
	END

	UPDATE dbo.b_users
	SET CheckWord = @NewCheckWord, CheckWordWindowStart = @CurrentTimeUtc
	WHERE ID = @UserId

	RETURN(0)
END
GO


/****** Object:  StoredProcedure [Users_GetUserByName]    Script Date: 02/25/2010 09:38:28 ******/
IF OBJECT_ID(N'Users_GetUserByName', N'P') IS NOT NULL DROP PROCEDURE [Users_GetUserByName]
GO
CREATE PROCEDURE [Users_GetUserByName]
    @UserName nvarchar(256),
    @ProviderName nvarchar(128),
    @CurrentTimeUtc datetime,
    @UpdateLastActivity bit = 0
AS
BEGIN

	IF (@ProviderName IS NULL)
	BEGIN
		SELECT @ProviderName = 'self'
	END

	SELECT TOP 1 u.ID AS UserId, u.UserName, u.ProviderName, u.LastActivityDate, u.Email, u.PasswordQuestion, 
		u.IsApproved, u.IsLockedOut, u.CreationDate, u.LastLoginDate, u.LastPasswordChangedDate, 
		u.LastLockoutDate, u.Comment, u.FirstName, u.SecondName,
		u.LastName, u.SiteID AS SiteId, u.BirthdayDate, u.DisplayName, u.ImageId, u.Gender, u.ActivationToken
	FROM dbo.b_Users u
	WHERE LOWER(@UserName) = u.LoweredUserName
		AND LOWER(@ProviderName) = u.LoweredProviderName

    IF (@@ROWCOUNT = 0)
        RETURN -1

    IF (@UpdateLastActivity = 1)
    BEGIN

        UPDATE dbo.b_users SET
			LastActivityDate = @CurrentTimeUtc
        WHERE LOWER(@UserName) = LoweredUserName
			AND LOWER(@ProviderName) = LoweredProviderName

    END

    RETURN 0
END
GO


/****** Object:  StoredProcedure [Users_GetUserByID]    Script Date: 02/25/2010 09:38:28 ******/
IF OBJECT_ID(N'Users_GetUserByID', N'P') IS NOT NULL DROP PROCEDURE [Users_GetUserByID]
GO
CREATE PROCEDURE [Users_GetUserByID]
	@UserId int,
	@CurrentTimeUtc datetime,
	@UpdateLastActivity bit = 0
AS
BEGIN
	IF (@UpdateLastActivity = 1)
	BEGIN
		UPDATE dbo.b_Users SET
			LastActivityDate = @CurrentTimeUtc
		WHERE ID = @UserId

		IF (@@ROWCOUNT = 0)
			RETURN -1
	END

	SELECT u.ID AS UserId, u.UserName, u.ProviderName, u.LastActivityDate, u.Email, u.PasswordQuestion, 
		u.IsApproved, u.IsLockedOut, u.CreationDate, u.LastLoginDate, u.LastPasswordChangedDate, 
		u.LastLockoutDate, u.Comment, u.FirstName, u.SecondName,
		u.LastName, u.SiteID AS SiteId, u.BirthdayDate, u.DisplayName, u.ImageId, u.Gender, u.ActivationToken
	FROM dbo.b_Users u
	WHERE u.ID = @UserId

	IF (@@ROWCOUNT = 0)
		RETURN -1

	RETURN 0
END
GO


/****** Object:  StoredProcedure [Users_SetPassword]    Script Date: 02/25/2010 09:38:28 ******/
IF OBJECT_ID(N'Users_SetPassword', N'P') IS NOT NULL DROP PROCEDURE [Users_SetPassword]
GO
CREATE PROCEDURE [Users_SetPassword]
    @UserId int,
    @NewPassword nvarchar(128),
    @PasswordSalt nvarchar(128),
    @CurrentTimeUtc datetime,
    @PasswordFormat int = 0
AS
BEGIN
    IF (@UserId IS NULL)
        RETURN(1)

    UPDATE dbo.b_Users SET
		Password = @NewPassword,
		PasswordFormat = @PasswordFormat,
		PasswordSalt = @PasswordSalt,
        LastPasswordChangedDate = @CurrentTimeUtc
    WHERE @UserId = ID

	IF (@@ROWCOUNT = 0)
    BEGIN
        RETURN(1)
    END

    RETURN(0)
END
GO


/****** Object:  StoredProcedure [Users_ResetPassword]    Script Date: 02/25/2010 09:38:28 ******/
IF OBJECT_ID(N'Users_ResetPassword', N'P') IS NOT NULL DROP PROCEDURE [Users_ResetPassword]
GO
CREATE PROCEDURE [Users_ResetPassword]
    @UserId int,
    @NewPassword nvarchar(128),
    @MaxInvalidPasswordAttempts int,
    @PasswordAttemptWindow int,
    @CheckWordAttemptWindow int,
    @PasswordSalt nvarchar(128),
    @CurrentTimeUtc datetime,
    @PasswordFormat int = 0,
    @PasswordAnswer nvarchar(128) = NULL,
    @CheckWord nvarchar(50) = NULL
AS
BEGIN
    DECLARE @IsLockedOut bit
    DECLARE @LastLockoutDate datetime
    DECLARE @FailedPasswordAttemptCount int
    DECLARE @FailedPasswordAttemptWindowStart datetime
    DECLARE @CheckWordAttemptWindowStart datetime
    DECLARE @FailedPasswordAnswerAttemptCount int
    DECLARE @FailedPasswordAnswerAttemptWindowStart datetime

    DECLARE @ErrorCode int
    SET @ErrorCode = 0

    DECLARE @TranStarted bit
    SET @TranStarted = 0

    IF (@@TRANCOUNT = 0)
    BEGIN
	    BEGIN TRANSACTION
	    SET @TranStarted = 1
    END
    ELSE
    	SET @TranStarted = 0

    IF (@UserId IS NULL)
    BEGIN
        SET @ErrorCode = 1
        GOTO Cleanup
    END

	SELECT @IsLockedOut = IsLockedOut,
		@LastLockoutDate = LastLockoutDate,
		@FailedPasswordAttemptCount = FailedPasswordAttemptCount,
		@FailedPasswordAttemptWindowStart = FailedPasswordAttemptWindowStart,
		@CheckWordAttemptWindowStart = CheckWordWindowStart,
		@FailedPasswordAnswerAttemptCount = FailedPasswordAnswerAttemptCount,
		@FailedPasswordAnswerAttemptWindowStart = FailedPasswordAnswerAttemptWindowStart
    FROM dbo.b_Users WITH (UPDLOCK)
    WHERE @UserId = ID

	IF (@@ROWCOUNT = 0)
    BEGIN
        SET @ErrorCode = 1
        GOTO Cleanup
    END

    IF (@IsLockedOut = 1)
    BEGIN
        SET @ErrorCode = 99
        GOTO Cleanup
    END

	UPDATE dbo.b_Users SET
		Password = @NewPassword,
		LastPasswordChangedDate = @CurrentTimeUtc,
		PasswordFormat = @PasswordFormat,
		PasswordSalt = @PasswordSalt
	WHERE @UserId = ID
		AND ((@PasswordAnswer IS NULL) OR (LOWER(PasswordAnswer) = LOWER(@PasswordAnswer)))
		AND ((@CheckWord IS NULL) OR ((LOWER(CheckWord) = LOWER(@CheckWord)) AND (@CurrentTimeUtc <= DATEADD(minute, @CheckWordAttemptWindow, @CheckWordAttemptWindowStart))))

	IF (@@ROWCOUNT = 0)
	BEGIN
		IF (@CurrentTimeUtc > DATEADD(minute, @PasswordAttemptWindow, @FailedPasswordAnswerAttemptWindowStart))
		BEGIN
			SET @FailedPasswordAnswerAttemptWindowStart = @CurrentTimeUtc
			SET @FailedPasswordAnswerAttemptCount = 1
		END
		ELSE
		BEGIN
			SET @FailedPasswordAnswerAttemptWindowStart = @CurrentTimeUtc
			SET @FailedPasswordAnswerAttemptCount = @FailedPasswordAnswerAttemptCount + 1
		END

		IF (@FailedPasswordAnswerAttemptCount >= @MaxInvalidPasswordAttempts)
		BEGIN
			SET @IsLockedOut = 1
			SET @LastLockoutDate = @CurrentTimeUtc
		END

		SET @ErrorCode = 3
	END
	ELSE
	BEGIN
		IF (@FailedPasswordAnswerAttemptCount > 0)
		BEGIN
			SET @FailedPasswordAnswerAttemptCount = 0
			SET @FailedPasswordAnswerAttemptWindowStart = CONVERT( datetime, '17540101', 112 )
		END
	END

	IF (NOT(@PasswordAnswer IS NULL))
	BEGIN
		UPDATE dbo.b_Users SET
			IsLockedOut = @IsLockedOut,
			LastLockoutDate = @LastLockoutDate,
            FailedPasswordAttemptCount = @FailedPasswordAttemptCount,
            FailedPasswordAttemptWindowStart = @FailedPasswordAttemptWindowStart,
            FailedPasswordAnswerAttemptCount = @FailedPasswordAnswerAttemptCount,
            FailedPasswordAnswerAttemptWindowStart = @FailedPasswordAnswerAttemptWindowStart
		WHERE @UserId = ID

        IF (@@ERROR <> 0)
        BEGIN
            SET @ErrorCode = -1
            GOTO Cleanup
        END
    END

    IF (@TranStarted = 1)
    BEGIN
		SET @TranStarted = 0
		COMMIT TRANSACTION
    END

    RETURN @ErrorCode

Cleanup:

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
    	ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode

END
GO


/****** Object:  StoredProcedure [Users_GetPasswordWithFormat]    Script Date: 02/25/2010 09:38:28 ******/
IF OBJECT_ID(N'Users_GetPasswordWithFormat', N'P') IS NOT NULL DROP PROCEDURE [Users_GetPasswordWithFormat]
GO
CREATE PROCEDURE [Users_GetPasswordWithFormat] 
	@UserId int = NULL,
    @UserName nvarchar(256) = NULL,
    @ProviderName nvarchar(128) = NULL,
    @UpdateLastLoginActivityDate bit,
    @CurrentTimeUtc datetime
AS
BEGIN
    DECLARE @IsLockedOut bit
    DECLARE @Password nvarchar(128)
    DECLARE @PasswordSalt nvarchar(128)
    DECLARE @ID int
    DECLARE @PasswordFormat int
    DECLARE @FailedPasswordAttemptCount int
    DECLARE @FailedPasswordAnswerAttemptCount int
    DECLARE @IsApproved bit
    DECLARE @LastActivityDate datetime
    DECLARE @LastLoginDate datetime

	IF (@UserId IS NULL OR @UserId = 0)
	BEGIN
		IF (@ProviderName IS NULL)
		BEGIN
			SELECT @ProviderName = 'self'
		END

		SELECT @ID = u.ID, @IsLockedOut = u.IsLockedOut, @Password = u.Password, @PasswordFormat = u.PasswordFormat,
			@PasswordSalt = u.PasswordSalt, @FailedPasswordAttemptCount = u.FailedPasswordAttemptCount,
			@FailedPasswordAnswerAttemptCount = u.FailedPasswordAnswerAttemptCount, @IsApproved = u.IsApproved,
			@LastActivityDate = u.LastActivityDate, @LastLoginDate = u.LastLoginDate
		FROM dbo.b_Users u
		WHERE LOWER(@UserName) = u.LoweredUserName
			AND LOWER(@ProviderName) = u.LoweredProviderName
	END
	ELSE
	BEGIN

		SELECT @ID = u.ID, @IsLockedOut = u.IsLockedOut, @Password = u.Password, @PasswordFormat = u.PasswordFormat,
			@PasswordSalt = u.PasswordSalt, @FailedPasswordAttemptCount = u.FailedPasswordAttemptCount,
			@FailedPasswordAnswerAttemptCount = u.FailedPasswordAnswerAttemptCount, @IsApproved = u.IsApproved,
			@LastActivityDate = u.LastActivityDate, @LastLoginDate = u.LastLoginDate
		FROM dbo.b_Users u
		WHERE @UserId = u.ID

	END

    IF (@ID IS NULL)
        RETURN 1

    IF (@IsLockedOut = 1)
        RETURN 99

	SELECT @Password, @PasswordFormat, @PasswordSalt, @FailedPasswordAttemptCount,
		@FailedPasswordAnswerAttemptCount, @IsApproved, @LastLoginDate, @LastActivityDate

	IF (@UpdateLastLoginActivityDate = 1 AND @IsApproved = 1)
	BEGIN
		UPDATE dbo.b_users SET
			LastLoginDate = @CurrentTimeUtc,
			LastActivityDate = @CurrentTimeUtc
		WHERE ID = @ID
    END

    RETURN 0
END
GO


/****** Object:  StoredProcedure [Users_CreateUser]    Script Date: 02/25/2010 09:38:28 ******/
IF OBJECT_ID(N'Users_CreateUser', N'P') IS NOT NULL DROP PROCEDURE [Users_CreateUser]
GO
CREATE PROCEDURE [Users_CreateUser]
    @UserName nvarchar(256),
    @Password nvarchar(128),
    @PasswordSalt nvarchar(128),
    @Email nvarchar(256),
    @PasswordQuestion nvarchar(256),
    @PasswordAnswer nvarchar(128),
    @CheckWord nvarchar(50),
    @IsApproved bit,
    @CurrentTimeUtc datetime,
    @UniqueEmail int = 0,
    @PasswordFormat int = 0,
    @ProviderName nvarchar(128) = NULL,
    @FirstName nvarchar(50) = NULL,
    @SecondName nvarchar(50) = NULL,
    @LastName nvarchar(50) = NULL,
    @SiteId varchar(50) = NULL,
    @BirthdayDate datetime = NULL,
    @Comment ntext = NULL,
    @DisplayName nvarchar(256) = NULL,
    @ImageId int = NULL,
    @Gender char(1) = NULL,
    @ActivationToken nvarchar(15) = NULL,
    @UserId int OUTPUT
AS
BEGIN
    SELECT @UserId = NULL

    DECLARE @IsLockedOut bit
    SET @IsLockedOut = 0

    DECLARE @LastLockoutDate datetime
    SET @LastLockoutDate = CONVERT(datetime, '17540101', 112)

    DECLARE @FailedPasswordAttemptCount int
    SET @FailedPasswordAttemptCount = 0

    DECLARE @FailedPasswordAttemptWindowStart datetime
    SET @FailedPasswordAttemptWindowStart = CONVERT(datetime, '17540101', 112)

    DECLARE @CheckWordAttemptWindowStart datetime
    SET @CheckWordAttemptWindowStart = CONVERT(datetime, '17540101', 112)

    DECLARE @FailedPasswordAnswerAttemptCount int
    SET @FailedPasswordAnswerAttemptCount = 0

    DECLARE @FailedPasswordAnswerAttemptWindowStart datetime
    SET @FailedPasswordAnswerAttemptWindowStart = CONVERT(datetime, '17540101', 112)

    DECLARE @ReturnValue int
    SET @ReturnValue = 0

    DECLARE @ErrorCode int
    SET @ErrorCode = 0

    DECLARE @TranStarted bit
    SET @TranStarted = 0

    IF (@@TRANCOUNT = 0)
    BEGIN
	    BEGIN TRANSACTION
	    SET @TranStarted = 1
    END
    ELSE
    	SET @TranStarted = 0

	IF (@ProviderName IS NULL)
	BEGIN
		SELECT @ProviderName = 'self'
	END

    SELECT @UserId = ID
    FROM dbo.b_Users
    WHERE LOWER(@UserName) = LoweredUserName
		AND LOWER(@ProviderName) = LoweredProviderName

    IF (@UserId IS NOT NULL)
    BEGIN
        SET @ErrorCode = 6
        GOTO Cleanup
    END

	IF (@UniqueEmail = 1)
	BEGIN
		IF (EXISTS( SELECT *
			FROM  dbo.b_Users WITH (UPDLOCK, HOLDLOCK)
			WHERE LoweredEmail = LOWER(@Email) ))
		BEGIN
			SET @ErrorCode = 7
			GOTO Cleanup
		END
	END

	INSERT INTO dbo.b_Users(UserName, LoweredUserName, LastActivityDate, Password, PasswordSalt, ProviderName,
		LoweredProviderName, Email, LoweredEmail, PasswordQuestion, PasswordAnswer, PasswordFormat, CheckWord,
		CheckWordWindowStart, IsApproved, IsLockedOut, CreationDate, LastLoginDate, LastPasswordChangedDate,
		LastLockoutDate, FailedPasswordAttemptCount, FailedPasswordAttemptWindowStart,
		FailedPasswordAnswerAttemptCount, FailedPasswordAnswerAttemptWindowStart,
		FirstName, SecondName, LastName, SiteID, BirthdayDate, Comment, DisplayName, ImageId, Gender,ActivationToken)
	VALUES (@UserName, LOWER(@UserName), @CurrentTimeUtc, @Password, @PasswordSalt, @ProviderName,
		LOWER(@ProviderName), @Email, LOWER(@Email), @PasswordQuestion, @PasswordAnswer, @PasswordFormat, 
		@CheckWord, @CheckWordAttemptWindowStart, @IsApproved, @IsLockedOut, @CurrentTimeUtc, @CurrentTimeUtc,
		@CurrentTimeUtc, @LastLockoutDate, @FailedPasswordAttemptCount, @FailedPasswordAttemptWindowStart,
		@FailedPasswordAnswerAttemptCount, @FailedPasswordAnswerAttemptWindowStart,
		@FirstName, @SecondName, @LastName, @SiteId, @BirthdayDate, @Comment, @DisplayName, @ImageId, @Gender,@ActivationToken)

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

	SELECT @UserId = @@IDENTITY;

    IF (@TranStarted = 1)
    BEGIN
	    SET @TranStarted = 0
	    COMMIT TRANSACTION
    END

    RETURN 0

Cleanup:

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
    	ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode

END
GO


/****** Object:  StoredProcedure [Users_UpdateUserInfo]    Script Date: 02/25/2010 09:38:28 ******/
IF OBJECT_ID(N'Users_UpdateUserInfo', N'P') IS NOT NULL DROP PROCEDURE [Users_UpdateUserInfo]
GO
CREATE PROCEDURE [Users_UpdateUserInfo]
	@UserId int = NULL,
    @UserName nvarchar(256) = NULL,
    @ProviderName nvarchar(128) = NULL,
    @IsPasswordCorrect bit,
    @UpdateLastLoginActivityDate bit,
    @MaxInvalidPasswordAttempts int,
    @PasswordAttemptWindow int,
    @CurrentTimeUtc datetime,
    @LastLoginDate datetime,
    @LastActivityDate datetime
AS
BEGIN
    DECLARE @IsApproved bit
    DECLARE @IsLockedOut bit
    DECLARE @LastLockoutDate datetime
    DECLARE @ID int
    DECLARE @FailedPasswordAttemptCount int
    DECLARE @FailedPasswordAttemptWindowStart datetime
    DECLARE @FailedPasswordAnswerAttemptCount int
    DECLARE @FailedPasswordAnswerAttemptWindowStart datetime

    DECLARE @ErrorCode int
    SET @ErrorCode = 0

    DECLARE @TranStarted bit
    SET @TranStarted = 0

    IF (@@TRANCOUNT = 0)
    BEGIN
	    BEGIN TRANSACTION
	    SET @TranStarted = 1
    END
    ELSE
    	SET @TranStarted = 0

	IF (@UserId IS NULL OR @UserId = 0)
	BEGIN
		IF (@ProviderName IS NULL)
		BEGIN
			SELECT @ProviderName = 'self'
		END

		SELECT @ID = u.ID,
			   @IsApproved = u.IsApproved,
			   @IsLockedOut = u.IsLockedOut,
			   @LastLockoutDate = u.LastLockoutDate,
			   @FailedPasswordAttemptCount = u.FailedPasswordAttemptCount,
			   @FailedPasswordAttemptWindowStart = u.FailedPasswordAttemptWindowStart,
			   @FailedPasswordAnswerAttemptCount = u.FailedPasswordAnswerAttemptCount,
			   @FailedPasswordAnswerAttemptWindowStart = u.FailedPasswordAnswerAttemptWindowStart
		FROM dbo.b_Users u WITH (UPDLOCK)
		WHERE LOWER(@UserName) = u.LoweredUserName
			AND LOWER(@ProviderName) = u.LoweredProviderName
	END
	ELSE
	BEGIN
		SELECT @ID = u.ID,
			   @IsApproved = u.IsApproved,
			   @IsLockedOut = u.IsLockedOut,
			   @LastLockoutDate = u.LastLockoutDate,
			   @FailedPasswordAttemptCount = u.FailedPasswordAttemptCount,
			   @FailedPasswordAttemptWindowStart = u.FailedPasswordAttemptWindowStart,
			   @FailedPasswordAnswerAttemptCount = u.FailedPasswordAnswerAttemptCount,
			   @FailedPasswordAnswerAttemptWindowStart = u.FailedPasswordAnswerAttemptWindowStart
		FROM dbo.b_Users u WITH (UPDLOCK)
		WHERE @UserId = u.ID
	END

    IF (@@rowcount = 0)
    BEGIN
        SET @ErrorCode = 1
        GOTO Cleanup
    END

    IF (@IsLockedOut = 1)
    BEGIN
        GOTO Cleanup
    END

    IF (@IsPasswordCorrect = 0)
    BEGIN
        IF (@CurrentTimeUtc > DATEADD(minute, @PasswordAttemptWindow, @FailedPasswordAttemptWindowStart))
        BEGIN
            SET @FailedPasswordAttemptWindowStart = @CurrentTimeUtc
            SET @FailedPasswordAttemptCount = 1
        END
        ELSE
        BEGIN
            SET @FailedPasswordAttemptWindowStart = @CurrentTimeUtc
            SET @FailedPasswordAttemptCount = @FailedPasswordAttemptCount + 1
        END

        BEGIN
            IF (@FailedPasswordAttemptCount >= @MaxInvalidPasswordAttempts)
            BEGIN
                SET @IsLockedOut = 1
                SET @LastLockoutDate = @CurrentTimeUtc
            END
        END
    END
    ELSE
    BEGIN
        IF (@FailedPasswordAttemptCount > 0 OR @FailedPasswordAnswerAttemptCount > 0)
        BEGIN
            SET @FailedPasswordAttemptCount = 0
            SET @FailedPasswordAttemptWindowStart = CONVERT(datetime, '17540101', 112)
            SET @FailedPasswordAnswerAttemptCount = 0
            SET @FailedPasswordAnswerAttemptWindowStart = CONVERT(datetime, '17540101', 112)
            SET @LastLockoutDate = CONVERT(datetime, '17540101', 112)
        END
    END

    IF (@UpdateLastLoginActivityDate = 1)
    BEGIN
        UPDATE dbo.b_Users SET
			LastActivityDate = @LastActivityDate,
			LastLoginDate = @LastLoginDate
        WHERE @ID = ID

        IF (@@ERROR <> 0)
        BEGIN
            SET @ErrorCode = -1
            GOTO Cleanup
        END
    END


    UPDATE dbo.b_Users SET
		IsLockedOut = @IsLockedOut, LastLockoutDate = @LastLockoutDate,
        FailedPasswordAttemptCount = @FailedPasswordAttemptCount,
        FailedPasswordAttemptWindowStart = @FailedPasswordAttemptWindowStart,
        FailedPasswordAnswerAttemptCount = @FailedPasswordAnswerAttemptCount,
        FailedPasswordAnswerAttemptWindowStart = @FailedPasswordAnswerAttemptWindowStart
    WHERE @ID = ID

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF (@TranStarted = 1)
    BEGIN
		SET @TranStarted = 0
		COMMIT TRANSACTION
    END

    RETURN @ErrorCode

Cleanup:

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
    	ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode

END
GO


/****** Object:  StoredProcedure [Roles_AddRolesToTasks]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_AddRolesToTasks', N'P') IS NOT NULL DROP PROCEDURE [Roles_AddRolesToTasks]
GO
CREATE PROCEDURE [Roles_AddRolesToTasks]
	@RoleNames nvarchar(4000),
	@TaskNames nvarchar(4000),
	@ModuleID varchar(50) = '',
	@ExternalID varchar(50) = ''
AS
BEGIN
	DECLARE @TranStarted bit
	SET @TranStarted = 0

	IF (@ModuleID IS NULL)
		SELECT @ModuleID = ''
	IF (@ExternalID IS NULL)
		SELECT @ExternalID = ''

	IF (@@TRANCOUNT = 0)
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END

	DECLARE @tbNames table(Name nvarchar(256) NOT NULL PRIMARY KEY)
	DECLARE @tbRoles table(RoleId int NOT NULL PRIMARY KEY)
	DECLARE @tbTasks table(TaskId int NOT NULL PRIMARY KEY)
	DECLARE @Num int
	DECLARE @Pos int
	DECLARE @NextPos int
	DECLARE @Name nvarchar(256)

	SET @Num = 0
	SET @Pos = 1
	WHILE (@Pos <= LEN(@RoleNames))
	BEGIN
		SELECT @NextPos = CHARINDEX(N',', @RoleNames,  @Pos)
		IF (@NextPos = 0 OR @NextPos IS NULL)
			SELECT @NextPos = LEN(@RoleNames) + 1
		SELECT @Name = RTRIM(LTRIM(SUBSTRING(@RoleNames, @Pos, @NextPos - @Pos)))
		SELECT @Pos = @NextPos + 1

		INSERT INTO @tbNames VALUES (@Name)
		SET @Num = @Num + 1
	END

	INSERT INTO @tbRoles
	SELECT r.ID
	FROM [b_Roles] r, @tbNames t
	WHERE LOWER(t.Name) = r.LoweredRoleName

	IF (@@ROWCOUNT <> @Num)
	BEGIN

		SELECT TOP 1 Name
		FROM @tbNames
		WHERE LOWER(Name) NOT IN (
			SELECT r.LoweredRoleName 
			FROM [b_Roles] r, @tbRoles tr 
			WHERE tr.RoleId = r.ID)

		IF (@TranStarted = 1)
			ROLLBACK TRANSACTION

		RETURN(2)
	END

	DELETE FROM @tbNames WHERE 1=1
	SET @Num = 0
	SET @Pos = 1

	WHILE (@Pos <= LEN(@TaskNames))
	BEGIN
		SELECT @NextPos = CHARINDEX(N',', @TaskNames,  @Pos)
		IF (@NextPos = 0 OR @NextPos IS NULL)
			SELECT @NextPos = LEN(@TaskNames) + 1
		SELECT @Name = RTRIM(LTRIM(SUBSTRING(@TaskNames, @Pos, @NextPos - @Pos)))
		SELECT @Pos = @NextPos + 1

		INSERT INTO @tbNames VALUES (@Name)
		SET @Num = @Num + 1
	END

	INSERT INTO @tbTasks
	SELECT u.ID
	FROM dbo.b_RolesTasks u, @tbNames t
	WHERE LOWER(t.Name) = u.LoweredTaskName

	IF (@@ROWCOUNT <> @Num)
	BEGIN
		SELECT TOP 1 Name
		FROM @tbNames
		WHERE LOWER(Name) NOT IN (
			SELECT u.LoweredTaskName
			FROM dbo.b_RolesTasks u, @tbTasks tu 
			WHERE tu.TaskId = u.ID)

		IF (@TranStarted = 1)
			ROLLBACK TRANSACTION

		RETURN(12)
	END

	IF (EXISTS(
		SELECT *
		FROM dbo.b_RolesInTasks ur, @tbRoles tu, @tbTasks tr
		WHERE tu.RoleId = ur.RoleID
			AND tr.TaskId = ur.TaskID
			AND ur.ModuleID = @ModuleID
			AND ur.ExternalID = @ExternalID
	))
	BEGIN

		SELECT TOP 1 RoleName, TaskName
		FROM dbo.b_RolesInTasks ur, @tbRoles tu, @tbTasks tr, [b_Roles] u, dbo.b_RolesTasks r
		WHERE u.ID = tu.RoleId 
			AND r.ID = tr.TaskId
			AND tu.RoleId = ur.RoleID
			AND tr.TaskId = ur.TaskID
			AND ur.ModuleID = @ModuleID
			AND ur.ExternalID = @ExternalID

		IF (@TranStarted = 1)
			ROLLBACK TRANSACTION

		RETURN(14)
	END

	INSERT INTO dbo.b_RolesInTasks (RoleID, TaskID, ModuleID, ExternalID)
	SELECT RoleId, TaskId, @ModuleID, @ExternalID
	FROM @tbRoles, @tbTasks

	EXEC dbo.Roles_FillOperationsCache

	IF (@TranStarted = 1)
		COMMIT TRANSACTION

	RETURN(0)
END
GO


/****** Object:  StoredProcedure [Roles_RemoveTaskFromTasks]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_RemoveTaskFromTasks', N'P') IS NOT NULL DROP PROCEDURE [Roles_RemoveTaskFromTasks]
GO
CREATE PROCEDURE [Roles_RemoveTaskFromTasks]
	@TaskName nvarchar(128)
AS
BEGIN
    DECLARE @ErrorCode int
    SET @ErrorCode = 0

    DECLARE @TaskId int
    SET @TaskId = NULL

	DECLARE @TranStarted bit
	SET @TranStarted = 0

	IF (@@TRANCOUNT = 0)
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END

	SELECT @TaskId = ID
	FROM dbo.b_RolesTasks
	WHERE LoweredTaskName = LOWER(@TaskName)

    IF (@TaskId IS NULL)
    BEGIN
        SELECT @ErrorCode = 9
        GOTO Cleanup
    END

    DELETE FROM dbo.b_RolesTasksInTasks
    WHERE @TaskId = TaskID

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

	EXEC dbo.Roles_FillOperationsCache
	
    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
        COMMIT TRANSACTION
    END

    RETURN(0)

Cleanup:

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
        ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode
END
GO


/****** Object:  StoredProcedure [Roles_RemoveTaskFromOperations]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_RemoveTaskFromOperations', N'P') IS NOT NULL DROP PROCEDURE [Roles_RemoveTaskFromOperations]
GO
CREATE PROCEDURE [Roles_RemoveTaskFromOperations]
	@TaskName nvarchar(128)
AS
BEGIN
    DECLARE @ErrorCode int
    SET @ErrorCode = 0

    DECLARE @TaskId int
    SET @TaskId = NULL

	DECLARE @TranStarted bit
	SET @TranStarted = 0

	IF (@@TRANCOUNT = 0)
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END

	SELECT @TaskId = ID
	FROM dbo.b_RolesTasks
	WHERE LoweredTaskName = LOWER(@TaskName)

    IF (@TaskId IS NULL)
    BEGIN
        SELECT @ErrorCode = 9
        GOTO Cleanup
    END

    DELETE FROM dbo.b_RolesTasksInOperations
    WHERE @TaskId = TaskID

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

	EXEC dbo.Roles_FillOperationsCache
	
    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
        COMMIT TRANSACTION
    END

    RETURN(0)

Cleanup:

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
        ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode
END
GO


/****** Object:  StoredProcedure [Roles_UpdateTask]    Script Date: 02/25/2010 09:38:28 ******/
IF OBJECT_ID(N'Roles_UpdateTask', N'P') IS NOT NULL DROP PROCEDURE [Roles_UpdateTask]
GO
CREATE PROCEDURE [Roles_UpdateTask]
	@TaskId int,
	@TaskName nvarchar(128),
	@TaskTitle nvarchar(255) = null,
	@Comment ntext = null
AS
BEGIN
	DECLARE @ErrorCode int
	SET @ErrorCode = 0

	DECLARE @TranStarted bit
	SET @TranStarted = 0

	IF (@@TRANCOUNT = 0)
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END
	ELSE
		SET @TranStarted = 0

	IF (NOT EXISTS( SELECT ID FROM dbo.b_RolesTasks WHERE ID = @TaskId ))
	BEGIN
		SET @ErrorCode = 9
		GOTO Cleanup
	END

	IF (EXISTS( SELECT ID FROM dbo.b_RolesTasks WHERE LOWER(@TaskName) = LoweredTaskName AND ID <> @TaskId ))
	BEGIN
		SET @ErrorCode = 8
		GOTO Cleanup
	END

	UPDATE dbo.b_RolesTasks SET
		TaskName = @TaskName,
		LoweredTaskName = LOWER(@TaskName),
		Title = @TaskTitle,
		Comment = @Comment
	WHERE ID = @TaskId
	
	IF (@@ERROR <> 0)
	BEGIN
		SET @ErrorCode = -1
		GOTO Cleanup
	END

	EXEC dbo.Roles_FillOperationsCache

	IF (@TranStarted = 1)
	BEGIN
		SET @TranStarted = 0
		COMMIT TRANSACTION
	END

	RETURN(0)

Cleanup:

	IF (@TranStarted = 1)
	BEGIN
		SET @TranStarted = 0
		ROLLBACK TRANSACTION
	END

	RETURN @ErrorCode

END
GO


/****** Object:  StoredProcedure [Roles_DeleteTask]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_DeleteTask', N'P') IS NOT NULL DROP PROCEDURE [Roles_DeleteTask]
GO
CREATE PROCEDURE [Roles_DeleteTask]
	@TaskId int = NULL,
    @TaskName nvarchar(256) = NULL
AS
BEGIN
    DECLARE @ErrorCode int
    SET @ErrorCode = 0

    DECLARE @TranStarted bit
    SET @TranStarted = 0

    IF (@@TRANCOUNT = 0)
    BEGIN
        BEGIN TRANSACTION
        SET @TranStarted = 1
    END
    ELSE
        SET @TranStarted = 0

	IF ((@TaskId IS NULL OR @TaskId = 0) AND @TaskName IS NOT NULL)
	BEGIN
		SELECT @TaskId = ID
		FROM dbo.b_RolesTasks
		WHERE LoweredTaskName = LOWER(@TaskName)
	END

    IF (@TaskId IS NULL OR @TaskId = 0)
    BEGIN
        SELECT @ErrorCode = 9
        GOTO Cleanup
    END


	INSERT INTO dbo.b_RolesTasksInTasks(TaskID, SubTaskID)
    SELECT rir1.TaskID, rir2.SubTaskID
    FROM dbo.b_RolesTasksInTasks rir1, dbo.b_RolesTasksInTasks rir2
    WHERE rir1.SubTaskID = @TaskId
		AND @TaskId = rir2.TaskID
		AND NOT EXISTS(SELECT 'x' FROM dbo.b_RolesTasksInTasks rir3 WHERE rir3.TaskID = rir1.TaskID AND rir3.SubTaskID = rir2.SubTaskID)

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

	INSERT INTO dbo.b_RolesTasksInOperations(TaskID, OperationID)
	SELECT rir.TaskID, rio.OperationID
	FROM b_RolesTasksInTasks rir, b_RolesTasksInOperations rio
	WHERE rir.SubTaskID = @TaskId
		AND @TaskId = rio.TaskID
		AND NOT EXISTS(SELECT 'x' FROM dbo.b_RolesTasksInOperations rio1 WHERE rio1.TaskID = rir.TaskID AND rio1.OperationID = rio.OperationID)

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

	INSERT INTO dbo.b_RolesInTasks(RoleID, TaskID, ModuleID, ExternalID)
	SELECT rit.RoleID, rir.SubTaskID, rit.ModuleID, rit.ExternalID
	FROM b_RolesTasksInTasks rir, b_RolesInTasks rit
	WHERE rir.TaskID = @TaskId
		AND @TaskId = rit.TaskID
		AND NOT EXISTS(SELECT 'x' FROM dbo.b_RolesInTasks rit1 WHERE rit1.RoleID = rit.RoleID AND rit1.TaskID = rir.TaskID AND rit1.ModuleID = rit.ModuleID AND rit1.ExternalID = rit.ExternalID)

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END


    DELETE FROM dbo.b_RolesInTasks
    WHERE @TaskId = TaskID

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    DELETE FROM dbo.b_RolesTasksInTasks
    WHERE @TaskId = TaskID

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    DELETE FROM dbo.b_RolesTasksInTasks
    WHERE @TaskId = SubTaskID

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    DELETE FROM dbo.b_RolesTasksInOperations
    WHERE @TaskId = TaskID

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    DELETE FROM dbo.b_RolesTasks
    WHERE @TaskId = ID

	EXEC dbo.Roles_FillOperationsCache

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
        COMMIT TRANSACTION
    END

    RETURN(0)

Cleanup:

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
        ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode
END
GO


/****** Object:  StoredProcedure [Roles_AddTasksToOperations]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_AddTasksToOperations', N'P') IS NOT NULL DROP PROCEDURE [Roles_AddTasksToOperations]
GO
CREATE PROCEDURE [Roles_AddTasksToOperations]
	@TaskNames		nvarchar(4000),
	@OperationNames	nvarchar(4000),
	@CurrentTimeUtc	datetime
AS
BEGIN
	DECLARE @TranStarted   bit
	SET @TranStarted = 0

	IF( @@TRANCOUNT = 0 )
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END

	DECLARE @tbNames		table(Name nvarchar(256) NOT NULL PRIMARY KEY)
	DECLARE @tbTasks		table(TaskId int NOT NULL PRIMARY KEY)
	DECLARE @tbOperations	table(OperationId int NOT NULL PRIMARY KEY)
	DECLARE @Num			int
	DECLARE @Pos			int
	DECLARE @NextPos		int
	DECLARE @Name			nvarchar(256)

	SET @Num = 0
	SET @Pos = 1
	WHILE(@Pos <= LEN(@TaskNames))
	BEGIN
		SELECT @NextPos = CHARINDEX(N',', @TaskNames,  @Pos)
		IF (@NextPos = 0 OR @NextPos IS NULL)
			SELECT @NextPos = LEN(@TaskNames) + 1
		SELECT @Name = RTRIM(LTRIM(SUBSTRING(@TaskNames, @Pos, @NextPos - @Pos)))
		SELECT @Pos = @NextPos + 1

		INSERT INTO @tbNames VALUES (@Name)
		SET @Num = @Num + 1
	END

	INSERT INTO @tbTasks
	  SELECT r.ID
	  FROM   dbo.b_RolesTasks r, @tbNames t
	  WHERE  LOWER(t.Name) = r.LoweredTaskName

	IF (@@ROWCOUNT <> @Num)
	BEGIN
		SELECT TOP 1 Name
		FROM   @tbNames
		WHERE  LOWER(Name) NOT IN (
				SELECT r.LoweredTaskName 
				FROM dbo.b_RolesTasks r, @tbTasks tr 
				WHERE tr.TaskId = r.ID)
		IF( @TranStarted = 1 )
			ROLLBACK TRANSACTION
		RETURN(9)
	END

	DELETE FROM @tbNames WHERE 1=1
	SET @Num = 0
	SET @Pos = 1

	WHILE (@Pos <= LEN(@OperationNames))
	BEGIN
		SELECT @NextPos = CHARINDEX(N',', @OperationNames,  @Pos)
		IF (@NextPos = 0 OR @NextPos IS NULL)
			SELECT @NextPos = LEN(@OperationNames) + 1
		SELECT @Name = RTRIM(LTRIM(SUBSTRING(@OperationNames, @Pos, @NextPos - @Pos)))
		SELECT @Pos = @NextPos + 1

		INSERT INTO @tbNames VALUES (@Name)
		SET @Num = @Num + 1
	END

	INSERT INTO @tbOperations
	  SELECT u.ID
	  FROM   dbo.b_RolesOperations u, @tbNames t
	  WHERE  LOWER(t.Name) = u.LoweredOperationName

	IF (@@ROWCOUNT <> @Num)
	BEGIN
		SELECT TOP 1 Name
		FROM   @tbNames
		WHERE  LOWER(Name) NOT IN (
				SELECT u.LoweredOperationName
				FROM dbo.b_RolesOperations u, @tbOperations tu 
				WHERE tu.OperationId = u.ID)
		IF( @TranStarted = 1 )
			ROLLBACK TRANSACTION
		RETURN(12)
	END

	IF (EXISTS( SELECT * FROM dbo.b_RolesTasksInOperations ur, @tbTasks tu, @tbOperations tr WHERE tu.TaskId = ur.TaskID AND tr.OperationId = ur.OperationID ))
	BEGIN
		SELECT TOP 1 TaskName, OperationName
		FROM dbo.b_RolesTasksInOperations ur, @tbTasks tu, @tbOperations tr, dbo.b_RolesTasks u, dbo.b_RolesOperations r
		WHERE u.ID = tu.TaskId 
			AND r.ID = tr.OperationId
			AND tu.TaskId = ur.TaskID
			AND tr.OperationId = ur.OperationID

		IF( @TranStarted = 1 )
			ROLLBACK TRANSACTION
		RETURN(13)
	END

	INSERT INTO dbo.b_RolesTasksInOperations (TaskID, OperationID)
	SELECT TaskId, OperationId
	FROM @tbTasks, @tbOperations

	EXEC dbo.Roles_FillOperationsCache

	IF( @TranStarted = 1 )
		COMMIT TRANSACTION
	RETURN(0)
END
GO


/****** Object:  StoredProcedure [Roles_AddTasksToTask]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_AddTasksToTask', N'P') IS NOT NULL DROP PROCEDURE [Roles_AddTasksToTask]
GO
CREATE PROCEDURE [Roles_AddTasksToTask]
	@TaskName		nvarchar(128),
	@SubTaskNames	nvarchar(4000),
	@CurrentTimeUtc	datetime
AS
BEGIN
	DECLARE @TranStarted   bit
	SET @TranStarted = 0

	IF( @@TRANCOUNT = 0 )
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END

	DECLARE @TaskId		int
	SET @TaskId = NULL

	SELECT @TaskId = ID
	FROM dbo.b_RolesTasks
	WHERE LOWER(@TaskName) = LoweredTaskName

	IF (@TaskId IS NULL)
	BEGIN
		ROLLBACK TRANSACTION
		RETURN(9)
	END

	DECLARE @tbNames	table(Name nvarchar(256) NOT NULL PRIMARY KEY)
	DECLARE @tbTasks	table(TaskId int NOT NULL PRIMARY KEY)

	DECLARE @Num		int
	DECLARE @Pos		int
	DECLARE @NextPos	int
	DECLARE @Name		nvarchar(256)

	SET @Num = 0
	SET @Pos = 1
	WHILE(@Pos <= LEN(@SubTaskNames))
	BEGIN
		SELECT @NextPos = CHARINDEX(N',', @SubTaskNames,  @Pos)
		IF (@NextPos = 0 OR @NextPos IS NULL)
			SELECT @NextPos = LEN(@SubTaskNames) + 1
		SELECT @Name = RTRIM(LTRIM(SUBSTRING(@SubTaskNames, @Pos, @NextPos - @Pos)))
		SELECT @Pos = @NextPos+1

		INSERT INTO @tbNames VALUES (@Name)
		SET @Num = @Num + 1
	END

	INSERT INTO @tbTasks
	  SELECT r.ID
	  FROM   dbo.b_RolesTasks r, @tbNames t
	  WHERE  LOWER(t.Name) = r.LoweredTaskName

	IF (@@ROWCOUNT <> @Num)
	BEGIN
		SELECT TOP 1 Name
		FROM   @tbNames
		WHERE  LOWER(Name) NOT IN (
				SELECT r.LoweredTaskName 
				FROM dbo.b_RolesTasks r, @tbTasks tr 
				WHERE tr.TaskId = r.ID)

		IF( @TranStarted = 1 )
			ROLLBACK TRANSACTION
		RETURN(9)
	END

	IF (EXISTS( SELECT * FROM dbo.b_RolesTasksInTasks rir, @tbTasks tr WHERE rir.TaskID = @TaskId AND rir.SubTaskID = tr.TaskId ))
	BEGIN
		SELECT TOP 1 r.TaskName
		FROM dbo.b_RolesTasksInTasks rir, @tbTasks tr, dbo.b_RolesTasks r
		WHERE rir.TaskID = @TaskId 
			AND rir.SubTaskID = tr.TaskId
			AND r.ID = tr.TaskId

		IF( @TranStarted = 1 )
			ROLLBACK TRANSACTION
		RETURN(10)
	END

	INSERT INTO dbo.b_RolesTasksInTasks (TaskID, SubTaskID)
	SELECT @TaskId, TaskId
	FROM @tbTasks

	EXEC dbo.Roles_FillOperationsCache

	IF( @TranStarted = 1 )
		COMMIT TRANSACTION

	RETURN(0)
END
GO


/****** Object:  StoredProcedure [Roles_UpdateOperation]    Script Date: 02/25/2010 09:38:28 ******/
IF OBJECT_ID(N'Roles_UpdateOperation', N'P') IS NOT NULL DROP PROCEDURE [Roles_UpdateOperation]
GO
CREATE PROCEDURE [Roles_UpdateOperation]
    @OperationId int,
	@OperationName nvarchar(128),
	@OperationType nvarchar(128),
	@ModuleId varchar(50),
    @Comment ntext = null
AS
BEGIN
	DECLARE @ErrorCode int
	SET @ErrorCode = 0

	DECLARE @TranStarted bit
	SET @TranStarted = 0

	IF (@@TRANCOUNT = 0)
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END
	ELSE
		SET @TranStarted = 0

	IF (NOT EXISTS( SELECT ID FROM dbo.b_RolesOperations WHERE ID = @OperationId ))
	BEGIN
		SET @ErrorCode = 12
		GOTO Cleanup
	END

	IF (EXISTS( SELECT ID FROM dbo.b_RolesOperations WHERE LOWER(@OperationName) = LoweredOperationName AND ID <> @OperationId ))
	BEGIN
		SET @ErrorCode = 11
		GOTO Cleanup
	END

	UPDATE dbo.b_RolesOperations SET
		OperationName = @OperationName,
		LoweredOperationName = LOWER(@OperationName),
		OperationType = @OperationType,
		ModuleID = @ModuleId,
		Comment = @Comment
	WHERE ID = @OperationId
	
	IF (@@ERROR <> 0)
	BEGIN
		SET @ErrorCode = -1
		GOTO Cleanup
	END

	EXEC dbo.Roles_FillOperationsCache

	IF (@TranStarted = 1)
	BEGIN
		SET @TranStarted = 0
		COMMIT TRANSACTION
	END

	RETURN(0)

Cleanup:

	IF (@TranStarted = 1)
	BEGIN
		SET @TranStarted = 0
		ROLLBACK TRANSACTION
	END

	RETURN @ErrorCode

END
GO


/****** Object:  StoredProcedure [Roles_DeleteOperation]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_DeleteOperation', N'P') IS NOT NULL DROP PROCEDURE [Roles_DeleteOperation]
GO
CREATE PROCEDURE [Roles_DeleteOperation]
	@OperationId int = NULL,
    @OperationName nvarchar(128) = NULL
AS
BEGIN
    DECLARE @ErrorCode int
    SET @ErrorCode = 0

    DECLARE @TranStarted bit
    SET @TranStarted = 0

    IF (@@TRANCOUNT = 0)
    BEGIN
        BEGIN TRANSACTION
        SET @TranStarted = 1
    END
    ELSE
        SET @TranStarted = 0

	IF ((@OperationId IS NULL OR @OperationId = 0) AND @OperationName IS NOT NULL)
	BEGIN
		SELECT @OperationId = ID
		FROM dbo.b_RolesOperations
		WHERE LoweredOperationName = LOWER(@OperationName)
	END

    IF (@OperationId IS NULL OR @OperationId = 0)
    BEGIN
        SELECT @ErrorCode = 9
        GOTO Cleanup
    END


    DELETE FROM dbo.b_RolesInOperations
    WHERE @OperationId = OperationID

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    DELETE FROM dbo.b_RolesTasksInOperations
    WHERE @OperationId = OperationID

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    DELETE FROM dbo.b_RolesOperations
    WHERE @OperationId = ID

	EXEC dbo.Roles_FillOperationsCache

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
        COMMIT TRANSACTION
    END

    RETURN(0)

Cleanup:

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
        ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode
END
GO


/****** Object:  StoredProcedure [Roles_AddRolesToOperations]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_AddRolesToOperations', N'P') IS NOT NULL DROP PROCEDURE [Roles_AddRolesToOperations]
GO
CREATE PROCEDURE [Roles_AddRolesToOperations]
	@RoleNames nvarchar(4000),
	@OperationNames nvarchar(4000),
	@ModuleID varchar(50) = '',
	@ExternalID varchar(50) = ''
AS
BEGIN
	DECLARE @TranStarted bit
	SET @TranStarted = 0

	IF (@ModuleID IS NULL)
		SELECT @ModuleID = ''
	IF (@ExternalID IS NULL)
		SELECT @ExternalID = ''

	IF (@@TRANCOUNT = 0)
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END

	DECLARE @tbNames table(Name nvarchar(256) NOT NULL PRIMARY KEY)
	DECLARE @tbRoles table(RoleId int NOT NULL PRIMARY KEY)
	DECLARE @tbOperations table(OperationId int NOT NULL PRIMARY KEY)
	DECLARE @Num int
	DECLARE @Pos int
	DECLARE @NextPos int
	DECLARE @Name nvarchar(256)

	SET @Num = 0
	SET @Pos = 1
	WHILE (@Pos <= LEN(@RoleNames))
	BEGIN
		SELECT @NextPos = CHARINDEX(N',', @RoleNames,  @Pos)
		IF (@NextPos = 0 OR @NextPos IS NULL)
			SELECT @NextPos = LEN(@RoleNames) + 1
		SELECT @Name = RTRIM(LTRIM(SUBSTRING(@RoleNames, @Pos, @NextPos - @Pos)))
		SELECT @Pos = @NextPos + 1

		INSERT INTO @tbNames VALUES (@Name)
		SET @Num = @Num + 1
	END

	INSERT INTO @tbRoles
	SELECT r.ID
	FROM [b_Roles] r, @tbNames t
	WHERE LOWER(t.Name) = r.LoweredRoleName

	IF (@@ROWCOUNT <> @Num)
	BEGIN

		SELECT TOP 1 Name
		FROM @tbNames
		WHERE LOWER(Name) NOT IN (
			SELECT r.LoweredRoleName 
			FROM [b_Roles] r, @tbRoles tr 
			WHERE tr.RoleId = r.ID)

		IF (@TranStarted = 1)
			ROLLBACK TRANSACTION

		RETURN(2)
	END

	DELETE FROM @tbNames WHERE 1=1
	SET @Num = 0
	SET @Pos = 1

	WHILE (@Pos <= LEN(@OperationNames))
	BEGIN
		SELECT @NextPos = CHARINDEX(N',', @OperationNames,  @Pos)
		IF (@NextPos = 0 OR @NextPos IS NULL)
			SELECT @NextPos = LEN(@OperationNames) + 1
		SELECT @Name = RTRIM(LTRIM(SUBSTRING(@OperationNames, @Pos, @NextPos - @Pos)))
		SELECT @Pos = @NextPos + 1

		INSERT INTO @tbNames VALUES (@Name)
		SET @Num = @Num + 1
	END

	INSERT INTO @tbOperations
	SELECT u.ID
	FROM dbo.b_RolesOperations u, @tbNames t
	WHERE LOWER(t.Name) = u.LoweredOperationName

	IF (@@ROWCOUNT <> @Num)
	BEGIN
		SELECT TOP 1 Name
		FROM @tbNames
		WHERE LOWER(Name) NOT IN (
			SELECT u.LoweredOperationName
			FROM dbo.b_RolesOperations u, @tbOperations tu 
			WHERE tu.OperationId = u.ID)

		IF (@TranStarted = 1)
			ROLLBACK TRANSACTION

		RETURN(12)
	END

	IF (EXISTS(
		SELECT *
		FROM dbo.b_RolesInOperations ur, @tbRoles tu, @tbOperations tr
		WHERE tu.RoleId = ur.RoleID
			AND tr.OperationId = ur.OperationID
			AND ur.ModuleID = @ModuleID
			AND ur.ExternalID = @ExternalID
	))
	BEGIN

		SELECT TOP 1 RoleName, OperationName
		FROM dbo.b_RolesInOperations ur, @tbRoles tu, @tbOperations tr, [b_Roles] u, dbo.b_RolesOperations r
		WHERE u.ID = tu.RoleId 
			AND r.ID = tr.OperationId
			AND tu.RoleId = ur.RoleID
			AND tr.OperationId = ur.OperationID
			AND ur.ModuleID = @ModuleID
			AND ur.ExternalID = @ExternalID

		IF (@TranStarted = 1)
			ROLLBACK TRANSACTION

		RETURN(14)
	END

	INSERT INTO dbo.b_RolesInOperations (RoleID, OperationID, ModuleID, ExternalID)
	SELECT RoleId, OperationId, @ModuleID, @ExternalID
	FROM @tbRoles, @tbOperations

	EXEC dbo.Roles_FillOperationsCache

	IF (@TranStarted = 1)
		COMMIT TRANSACTION

	RETURN(0)
END
GO


/****** Object:  StoredProcedure [Roles_CreateRole]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_CreateRole', N'P') IS NOT NULL DROP PROCEDURE [Roles_CreateRole]
GO
CREATE PROCEDURE [Roles_CreateRole]
    @RoleName nvarchar(128),
    @RoleTitle nvarchar(255) = null,
    @Active bit = 1,
    @Comment ntext = null,
    @Policy nvarchar(4000) = null,
    @RoleId int OUTPUT
AS
BEGIN
	SELECT @RoleId = NULL

	DECLARE @ErrorCode int
	SET @ErrorCode = 0

	DECLARE @TranStarted bit
	SET @TranStarted = 0

	IF (@@TRANCOUNT = 0)
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END
	ELSE
		SET @TranStarted = 0

	IF (EXISTS( SELECT ID FROM dbo.b_Roles WHERE LoweredRoleName = LOWER(@RoleName) ))
	BEGIN
		SET @ErrorCode = 4
		GOTO Cleanup
	END

	IF (@Policy IS NULL OR @Policy = '')
	BEGIN
		SET @Policy = '<Policy/>'
	END

	INSERT INTO b_Roles(RoleName, LoweredRoleName, Active, Comment, Policy, EffectivePolicy, Title)
	VALUES (@RoleName, LOWER(@RoleName), @Active, @Comment, @Policy, NULL, @RoleTitle)

	IF (@@ERROR <> 0)
	BEGIN
		SET @ErrorCode = -1
		GOTO Cleanup
	END

	SELECT @RoleId = @@IDENTITY

	EXEC dbo.Roles_FillEffectivePolicy

	EXEC dbo.Roles_FillRolesCache

	IF (@TranStarted = 1)
	BEGIN
		SET @TranStarted = 0
		COMMIT TRANSACTION
	END

	RETURN(0)

Cleanup:

	IF (@TranStarted = 1)
	BEGIN
		SET @TranStarted = 0
		ROLLBACK TRANSACTION
	END

	RETURN @ErrorCode

END
GO


/****** Object:  StoredProcedure [Roles_AddRolesToRole]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_AddRolesToRole', N'P') IS NOT NULL DROP PROCEDURE [Roles_AddRolesToRole]
GO
CREATE PROCEDURE [Roles_AddRolesToRole]
	@RoleName nvarchar(128),
	@SubRoleNames nvarchar(4000)
AS
BEGIN
	DECLARE @TranStarted bit
	SET @TranStarted = 0

	IF (@@TRANCOUNT = 0)
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END

	DECLARE @RoleId int
	SET @RoleId = NULL

	SELECT @RoleId = ID
	FROM [b_Roles]
	WHERE LOWER(@RoleName) = LoweredRoleName

	IF (@RoleId IS NULL)
	BEGIN
		ROLLBACK TRANSACTION
		RETURN(2)
	END

	DECLARE @tbNames table(Name nvarchar(256) NOT NULL PRIMARY KEY)
	DECLARE @tbRoles table(RoleId int NOT NULL PRIMARY KEY)

	DECLARE @Num int
	DECLARE @Pos int
	DECLARE @NextPos int
	DECLARE @Name nvarchar(256)

	SET @Num = 0
	SET @Pos = 1
	WHILE (@Pos <= LEN(@SubRoleNames))
	BEGIN
		SELECT @NextPos = CHARINDEX(N',', @SubRoleNames,  @Pos)
		IF (@NextPos = 0 OR @NextPos IS NULL)
			SELECT @NextPos = LEN(@SubRoleNames) + 1
		SELECT @Name = RTRIM(LTRIM(SUBSTRING(@SubRoleNames, @Pos, @NextPos - @Pos)))
		SELECT @Pos = @NextPos + 1

		INSERT INTO @tbNames VALUES (@Name)
		SET @Num = @Num + 1
	END

	INSERT INTO @tbRoles
	SELECT r.ID
	FROM [b_Roles] r, @tbNames t
	WHERE LOWER(t.Name) = r.LoweredRoleName

	IF (@@ROWCOUNT <> @Num)
	BEGIN
		SELECT TOP 1 Name
		FROM @tbNames
		WHERE LOWER(Name) NOT IN (
			SELECT r.LoweredRoleName 
			FROM [b_Roles] r, @tbRoles tr 
			WHERE tr.RoleId = r.ID)

		IF (@TranStarted = 1)
			ROLLBACK TRANSACTION

		RETURN(2)
	END

	IF (EXISTS(
		SELECT *
		FROM dbo.b_RolesInRoles rir, @tbRoles tr
		WHERE rir.RoleID = @RoleId
			AND rir.SubRoleID = tr.RoleId
	))
	BEGIN
		SELECT TOP 1 r.RoleName
		FROM dbo.b_RolesInRoles rir, @tbRoles tr, [b_Roles] r
		WHERE rir.RoleID = @RoleId 
			AND rir.SubRoleID = tr.RoleId
			AND r.ID = tr.RoleId

		IF (@TranStarted = 1)
			ROLLBACK TRANSACTION

		RETURN(7)
	END

	INSERT INTO dbo.b_RolesInRoles (RoleID, SubRoleID)
	SELECT @RoleId, RoleId
	FROM @tbRoles



	EXEC dbo.Roles_DropEffectivePolicy @RoleId
	EXEC dbo.Roles_FillEffectivePolicy

	EXEC dbo.Roles_FillOperationsCache
	EXEC dbo.Roles_FillRolesCache

	IF (@TranStarted = 1)
		COMMIT TRANSACTION

	RETURN(0)
END
GO


/****** Object:  StoredProcedure [Roles_RemoveRoleFromTasks]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_RemoveRoleFromTasks', N'P') IS NOT NULL DROP PROCEDURE [Roles_RemoveRoleFromTasks]
GO
CREATE PROCEDURE [Roles_RemoveRoleFromTasks]
	@RoleName nvarchar(128) = NULL,
	@ModuleId varchar(128) = NULL,
	@ExternalId varchar(128) = NULL
AS
BEGIN
    DECLARE @ErrorCode int
    SET @ErrorCode = 0

    DECLARE @RoleId int
    SET @RoleId = NULL

	DECLARE @TranStarted bit
	SET @TranStarted = 0

	IF (@@TRANCOUNT = 0)
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END

	IF (@RoleName IS NOT NULL)
	BEGIN
		SELECT @RoleId = ID
		FROM [b_Roles]
		WHERE LoweredRoleName = LOWER(@RoleName)

		IF (@RoleId IS NULL)
		BEGIN
			SELECT @ErrorCode = 2
			GOTO Cleanup
		END
	END

    DELETE FROM dbo.b_RolesInTasks
    WHERE (@RoleId IS NULL OR @RoleId IS NOT NULL AND @RoleId = RoleID)
		AND (@ModuleId IS NULL AND (ModuleID IS NULL OR ModuleID = '')
			OR @ModuleId IS NOT NULL AND ModuleID = @ModuleId AND ExternalID = @ExternalId)

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

	EXEC dbo.Roles_FillOperationsCache
	
    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
        COMMIT TRANSACTION
    END

    RETURN(0)

Cleanup:

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
        ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode
END
GO


/****** Object:  StoredProcedure [Roles_RemoveRoleFromOperations]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_RemoveRoleFromOperations', N'P') IS NOT NULL DROP PROCEDURE [Roles_RemoveRoleFromOperations]
GO
CREATE PROCEDURE [Roles_RemoveRoleFromOperations]
	@RoleName nvarchar(128) = NULL,
	@OperationName nvarchar(128) = NULL,
	@ModuleId varchar(128) = NULL,
	@ExternalId varchar(128) = NULL
AS
BEGIN
    DECLARE @ErrorCode int
    SET @ErrorCode = 0

    DECLARE @RoleId int
    SET @RoleId = NULL
    
    DECLARE @OperationId int
    SET @OperationId = NULL

	DECLARE @TranStarted bit
	SET @TranStarted = 0

	IF (@@TRANCOUNT = 0)
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END

	IF (@RoleName IS NOT NULL)
	BEGIN
		SELECT @RoleId = ID
		FROM [dbo].[b_Roles]
		WHERE LoweredRoleName = LOWER(@RoleName)

		IF (@RoleId IS NULL)
		BEGIN
			SELECT @ErrorCode = 2
			GOTO Cleanup
		END
	END
	
	IF (@OperationName IS NOT NULL)
	BEGIN
		SELECT @OperationId = ID
		FROM [dbo].[b_RolesOperations]
		WHERE LoweredOperationName = LOWER(@OperationName)

		IF (@OperationId IS NULL)
		BEGIN
			SELECT @ErrorCode = 12
			GOTO Cleanup
		END
	END

    DELETE FROM dbo.b_RolesInOperations
    WHERE 
		(@RoleId IS NULL OR @RoleId IS NOT NULL AND @RoleId = RoleID)
		AND (@OperationId IS NULL OR @OperationId IS NOT NULL AND @OperationId = OperationID) 
		AND (@ModuleId IS NULL AND (ModuleID IS NULL OR ModuleID = '')
			OR @ModuleId IS NOT NULL AND ModuleID = @ModuleId AND ExternalID = @ExternalId)

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

	EXEC dbo.Roles_FillOperationsCache
	
    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
        COMMIT TRANSACTION
    END

    RETURN(0)

Cleanup:

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
        ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode
END
GO

/****** Object:  StoredProcedure [Roles_UpdateRole]    Script Date: 02/25/2010 09:38:28 ******/
IF OBJECT_ID(N'Roles_UpdateRole', N'P') IS NOT NULL DROP PROCEDURE [Roles_UpdateRole]
GO
CREATE PROCEDURE [Roles_UpdateRole] 
	@RoleId int,
	@RoleName nvarchar(128),
	@RoleTitle nvarchar(255) = null,
	@Active bit = 1,
	@Comment ntext = null,
	@Policy nvarchar(4000) = null
AS
BEGIN
	DECLARE @ErrorCode int
	SET @ErrorCode = 0

	DECLARE @TranStarted bit
	SET @TranStarted = 0

	IF (@@TRANCOUNT = 0)
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END
	ELSE
		SET @TranStarted = 0

	IF (NOT EXISTS( SELECT ID FROM dbo.b_Roles WHERE ID = @RoleId ))
	BEGIN
		SET @ErrorCode = 2
		GOTO Cleanup
	END

	IF (EXISTS( SELECT ID FROM dbo.b_Roles WHERE LOWER(@RoleName) = LoweredRoleName AND ID <> @RoleId ))
	BEGIN
		SET @ErrorCode = 4
		GOTO Cleanup
	END

	IF (@Policy IS NULL OR @Policy = '')
	BEGIN
		SET @Policy = '<Policy/>'
	END

	IF (@RoleId = 1 OR @RoleId = 2 OR @RoleId = 3)
	BEGIN
		UPDATE dbo.b_Roles SET
			Active = @Active,
			Title = @RoleTitle,
			Comment = @Comment,
			Policy = @Policy,
			EffectivePolicy = NULL
		WHERE ID = @RoleId
	END
	ELSE
	BEGIN
		UPDATE dbo.b_Roles SET
			RoleName = @RoleName,
			Title = @RoleTitle,
			LoweredRoleName = LOWER(@RoleName),
			Active = @Active,
			Comment = @Comment,
			Policy = @Policy,
			EffectivePolicy = NULL
		WHERE ID = @RoleId
	END
	
	IF (@@ERROR <> 0)
	BEGIN
		SET @ErrorCode = -1
		GOTO Cleanup
	END

	EXEC dbo.Roles_DropEffectivePolicy @RoleId
	EXEC dbo.Roles_FillEffectivePolicy
	EXEC dbo.Roles_FillOperationsCache
	EXEC dbo.Roles_FillRolesCache

	IF (@TranStarted = 1)
	BEGIN
		SET @TranStarted = 0
		COMMIT TRANSACTION
	END

	RETURN(0)

Cleanup:

	IF (@TranStarted = 1)
	BEGIN
		SET @TranStarted = 0
		ROLLBACK TRANSACTION
	END

	RETURN @ErrorCode

END
GO


/****** Object:  StoredProcedure [Roles_RemoveRolesFromRole]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_RemoveRolesFromRole', N'P') IS NOT NULL DROP PROCEDURE [Roles_RemoveRolesFromRole]
GO
CREATE PROCEDURE [Roles_RemoveRolesFromRole]
	@RoleName nvarchar(128)
AS
BEGIN
    DECLARE @ErrorCode int
    SET @ErrorCode = 0

    DECLARE @RoleId int
    SET @RoleId = NULL

	DECLARE @TranStarted bit
	SET @TranStarted = 0

	IF (@@TRANCOUNT = 0)
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END

	SELECT @RoleId = ID
	FROM [b_Roles]
	WHERE LoweredRoleName = LOWER(@RoleName)

    IF (@RoleId IS NULL)
    BEGIN
        SELECT @ErrorCode = 2
        GOTO Cleanup
    END

    DELETE FROM dbo.b_RolesInRoles
    WHERE @RoleId = RoleID

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

	EXEC dbo.Roles_DropEffectivePolicy @RoleId
	EXEC dbo.Roles_FillEffectivePolicy

	EXEC dbo.Roles_FillOperationsCache
	
    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
        COMMIT TRANSACTION
    END

    RETURN(0)

Cleanup:

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
        ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode
END
GO

/****** Object:  StoredProcedure [Roles_DeleteRole]    Script Date: 02/25/2010 09:38:27 ******/
IF OBJECT_ID(N'Roles_DeleteRole', N'P') IS NOT NULL DROP PROCEDURE [Roles_DeleteRole]
GO
CREATE PROCEDURE [Roles_DeleteRole]
	@RoleId int = NULL,
    @RoleName nvarchar(256) = NULL,
    @DeleteOnlyIfRoleIsEmpty bit = 1
AS
BEGIN
    DECLARE @ErrorCode int
    SET @ErrorCode = 0

    DECLARE @TranStarted bit
    SET @TranStarted = 0

    IF (@@TRANCOUNT = 0)
    BEGIN
        BEGIN TRANSACTION
        SET @TranStarted = 1
    END
    ELSE
        SET @TranStarted = 0

	IF ((@RoleId IS NULL OR @RoleId = 0) AND @RoleName IS NOT NULL)
	BEGIN
		SELECT @RoleId = ID
		FROM [b_Roles]
		WHERE LoweredRoleName = LOWER(@RoleName)
	END

    IF (@RoleId IS NULL OR @RoleId = 0)
    BEGIN
        SELECT @ErrorCode = 2
        GOTO Cleanup
    END

	IF (@RoleId = 1 OR @RoleId = 2 OR @RoleId = 3)
	BEGIN
		SET @ErrorCode = 50
		GOTO Cleanup
	END

    IF (@DeleteOnlyIfRoleIsEmpty <> 0)
    BEGIN
        IF (EXISTS(SELECT RoleID FROM dbo.b_UsersInRoles WHERE @RoleId = RoleID))
        BEGIN
            SELECT @ErrorCode = 5
            GOTO Cleanup
        END
    END

    DELETE FROM dbo.b_UsersInRoles
    WHERE @RoleId = RoleID

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END


	INSERT INTO dbo.b_RolesInRoles(RoleID, SubRoleID)
    SELECT rir1.RoleID, rir2.SubRoleID
    FROM dbo.b_RolesInRoles rir1, dbo.b_RolesInRoles rir2
    WHERE rir1.SubRoleID = @RoleId
		AND @RoleId = rir2.RoleID
		AND NOT EXISTS(SELECT 'x' FROM dbo.b_RolesInRoles rir3 WHERE rir3.RoleID = rir1.RoleID AND rir3.SubRoleID = rir2.SubRoleID)

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

	INSERT INTO dbo.b_RolesInTasks(RoleID, TaskID, ModuleID, ExternalID)
	SELECT rir.RoleID, rit.TaskID, rit.ModuleID, rit.ExternalID
	FROM b_RolesInRoles rir, b_RolesInTasks rit
	WHERE rir.SubRoleID = @RoleId
		AND @RoleId = rit.RoleID
		AND NOT EXISTS(SELECT 'x' FROM dbo.b_RolesInTasks rit1 WHERE rit1.RoleID = rir.RoleID AND rit1.TaskID = rit.TaskID AND rit1.ModuleID = rit.ModuleID AND rit1.ExternalID = rit.ExternalID)

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

	INSERT INTO dbo.b_RolesInOperations(RoleID, OperationID, ModuleID, ExternalID)
	SELECT rir.RoleID, rio.OperationID, rio.ModuleID, rio.ExternalID
	FROM b_RolesInRoles rir, b_RolesInOperations rio
	WHERE rir.SubRoleID = @RoleId
		AND @RoleId = rio.RoleID
		AND NOT EXISTS(SELECT 'x' FROM dbo.b_RolesInOperations rio1 WHERE rio1.RoleID = rir.RoleID AND rio1.OperationID = rio.OperationID AND rio1.ModuleID = rio.ModuleID AND rio1.ExternalID = rio.ExternalID)

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

	EXEC dbo.Roles_DropEffectivePolicy @RoleId

    DELETE FROM dbo.b_RolesInRoles
    WHERE @RoleId = RoleID

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    DELETE FROM dbo.b_RolesInRoles
    WHERE @RoleId = SubRoleID

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    DELETE FROM dbo.b_RolesInTasks
    WHERE @RoleId = RoleID

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    DELETE FROM dbo.b_RolesInOperations
    WHERE @RoleId = RoleID

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    DELETE FROM [b_Roles]
    WHERE @RoleId = ID

	EXEC dbo.Roles_FillEffectivePolicy

	EXEC dbo.Roles_FillOperationsCache
	EXEC dbo.Roles_FillRolesCache

    IF (@@ERROR <> 0)
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
        COMMIT TRANSACTION
    END

    RETURN(0)

Cleanup:

    IF (@TranStarted = 1)
    BEGIN
        SET @TranStarted = 0
        ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode
END
GO

IF OBJECT_ID (N'Users_SelectUsers', N'P') IS NOT NULL
	DROP PROCEDURE Users_SelectUsers
GO

CREATE PROCEDURE Users_SelectUsers
	@mode smallint,
	@countToSelect int,
	@query NVARCHAR(100)
AS
BEGIN
DECLARE 
	@count			int,
	@recToSelect	int,
	@SQL			NVARCHAR(MAX),
	@wordCount		int,
	@i				int,
	@likeString		NVARCHAR(300),
	@ch				NVARCHAR(1),
	@separators		NVARCHAR(30),
	@word			NVARCHAR(100)

if (@mode not in (0,1)) 
	RETURN
	
CREATE TABLE #users(Id int IDENTITY(1,1) NOT NULL,UserId int)
	
if (@mode = 0 ) BEGIN
	 INSERT #users ( UserId) SELECT top (@countToSelect)
			 ID
				FROM 
				b_Users
				where 
				(	case when len(DisplayName)>0 then DisplayName
					else case when len(FirstName) > 0 and len(LastName)>0 then FirstName+' '+LastName
						else case when len(FirstName) > 0 then FirstName else case when len(LastName)>0 then LastName
							else UserName end end end end ) like '%'+QUOTENAME(@query)+'%'
				ORDER BY ID
END
if (@mode = 1 ) BEGIN
	SELECT @separators = ''',; '

	SELECT @wordCount = 0,@i=0, @query = LTRIM(RTRIM(@query))

	if ( LEN(@query) > 0 ) BEGIN
		
		CREATE TABLE #words(word NVARCHAR(100))

		SET @word=''

		WHILE ( @i<=Len(@query) AND @wordCount <3) BEGIN
		
			SET @ch = SUBSTRING(@query,@i,1)
			if ( CHARINDEX(@ch,@separators)=0 ) 
					set @word = @word + @ch
	 
			ELSE BEGIN
				if ( LEN(@word) >0 ) BEGIN
					INSERT #words VALUES (@word)
					SET @wordCount = @wordCount + 1
				END
				SET @word= ''
			END
			
			SET @i = @i + 1
		END
		
		if ( LEN(@word) >0 )
			INSERT #words VALUES (@word)


							
		SELECT @likeString=''
		
		SELECT @likeString = @likeString +' DisplayName like N''%'+@word+'%'' OR '
		FROM
			#words
			
		SELECT @likeString = SUBSTRING(@likeString,0,LEN(@likeString) - 2)
							
		SET @SQL = ' INSERT #users ( UserId) select top '
						+CONVERT(VARCHAR,(@countToSelect))+
						' Id from b_users u where '+
						@likeString +
						' order by DisplayName,ID '		
		EXEC (@SQL)

			
		SELECT @count = COUNT(*) from #users

		if ( @count != 20 ) BEGIN
		
			SELECT @likeString = REPLACE(@likeString,' DisplayName',' u.LastName')
			SET @SQL = ' INSERT #users ( UserId) select top '
						+CONVERT(NVARCHAR,(@countToSelect-@count))+
						' u.Id from b_users u left outer join #users uu on uu.UserId = u.Id where ('+
						@likeString +
						') and uu.Id is null order by u.LastName,u.ID '
						
			EXEC (@SQL)
		END

		SELECT @count = COUNT(*) from #users

		if ( @count != 20 ) BEGIN
			
			SELECT @likeString = REPLACE(@likeString,' u.LastName',' u.FirstName')
			SET @SQL = ' INSERT #users ( UserId) select top '
						+CONVERT(NVARCHAR,(@countToSelect-@count))+
						'  u.Id from b_users u left outer join #users uu on uu.UserId = u.Id where  ('+
						@likeString +
						') and uu.Id is null order by u.LastName,u.ID '
						
			EXEC (@SQL)
		END

		SELECT @count = COUNT(*) from #users

		if ( @count != 20 ) BEGIN
			SELECT @likeString = REPLACE(@likeString,' u.FirstName',' u.UserName')
			SET @SQL = ' INSERT #users ( UserId) select top '
						+CONVERT(NVARCHAR,(@countToSelect-@count))+
						'  u.Id from b_users u left outer join #users uu on uu.UserId = u.Id where ('+
						@likeString +
						') and uu.Id is null order by u.UserName,u.ID '
						
			EXEC (@SQL)
		END
	END
	drop table #words
END


SELECT  UserId, u.UserName, u.ProviderName, u.LastActivityDate, u.Email, u.PasswordQuestion, 
		u.IsApproved, u.IsLockedOut, u.CreationDate, u.LastLoginDate, u.LastPasswordChangedDate, 
		u.LastLockoutDate, u.Comment, u.FirstName, u.SecondName,
		u.LastName, u.SiteID AS SiteId, u.BirthdayDate, u.DisplayName, u.ImageId, u.Gender, u.ActivationToken FROM 

#users uu join b_Users u on uu.UserId = u.ID  ORDER BY uu.Id

drop table #users
END

GO
/*--- BEGINNING OF RATINGS   ---*/
IF OBJECT_ID (N'b_Rating', N'U') IS NULL
BEGIN
	CREATE TABLE b_Rating
	(
		ID INT IDENTITY(1,1) NOT NULL,
		BoundEntityTypeHash INT NOT NULL,
		Active BIT NOT NULL,
		Name NVARCHAR(512) NULL,
		BoundEntityTypeId NVARCHAR(256) NOT NULL,
		CustomPropertyEntityId NVARCHAR(64) NOT NULL,
		CurValCustomFieldName NVARCHAR(255) NOT NULL,
		PrevValCustomFieldName NVARCHAR(255) NOT NULL,
		CalculationMethod TINYINT NOT NULL,
		RefreshMethod TINYINT NOT NULL,					
		Created DATETIME NOT NULL,
		LastModified DATETIME NOT NULL,
		LastCalculationStarted DATETIME NOT NULL,
		LastCalculated DATETIME NOT NULL,
		IsUnderCalculation BIT NOT NULL,
		SynchronizerId INT CONSTRAINT DF_b_Rating_SynchronizerId NOT NULL DEFAULT 0,
		ComponentConfigXml XML NULL,
		XmlId VARCHAR(256) NULL,
		CONSTRAINT PK_b_Rating PRIMARY KEY CLUSTERED(ID ASC)
	)
END
GO

IF OBJECT_ID (N'b_RatingCounter', N'U') IS NULL
BEGIN
	CREATE TABLE b_RatingCounter
	(
		ID INT IDENTITY(1,1) NOT NULL,
		RatingId INT NOT NULL,
		BoundEntityHash INT NOT NULL,
		BoundEntityTypeId NVARCHAR(256) NOT NULL,
		BoundEntityId NVARCHAR(64) NOT NULL,
		CustomPropertyEntityId NVARCHAR(64) NOT NULL,
		CurValCustomFieldName NVARCHAR(255) NOT NULL,
		PrevValCustomFieldName NVARCHAR(255) NOT NULL,		
		CalculationMethod TINYINT NOT NULL,
		RefreshMethod TINYINT NOT NULL,	
		CurrentValue FLOAT(53) NOT NULL,		
		PreviousValue FLOAT(53) NOT NULL,						
		Created DATETIME NOT NULL,
		LastModified DATETIME NOT NULL,
		LastCalculated DATETIME NOT NULL,
		IsCalculated BIT NOT NULL,
		IsBuilt BIT NOT NULL,
		IsLockedOut BIT CONSTRAINT DF_b_RatingCounter_IsLockedOut NOT NULL DEFAULT 0,
		LockOutStarted DATETIME NULL,
		LastCalculationError NVARCHAR(MAX) NULL,
		XmlId VARCHAR(256) NULL,
		CONSTRAINT PK_b_RatingCounter PRIMARY KEY CLUSTERED(ID ASC),
		CONSTRAINT FK_RatingCounter_Rating FOREIGN KEY(RatingId) REFERENCES b_Rating(ID)
	)
END
GO


IF OBJECT_ID (N'b_RatingComponent', N'U') IS NULL
BEGIN
	CREATE TABLE b_RatingComponent
	(
		ID INT IDENTITY(1,1) NOT NULL,
		RatingId INT NOT NULL,
		RatingCounterId INT NOT NULL,
		ComponentTypeFullName NVARCHAR(1024) NOT NULL,
		BoundEntityTypeId NVARCHAR(256) NOT NULL,
		BoundEntityId NVARCHAR(64) NOT NULL,
		CreatedUtc DATETIME NOT NULL,
		LastModifiedUtc DATETIME NOT NULL,
		LastCalculatedUtc DATETIME NOT NULL,	
		CurrentValue FLOAT(53) NOT NULL,
		RefreshInterval INT NOT NULL,		
		ConfigXml XML NULL,
		DependencyXml XML NULL,
		IsCalculated BIT NOT NULL,
		XmlId VARCHAR(256) NULL,
		CONSTRAINT PK_b_RatingCounterComponent PRIMARY KEY CLUSTERED(ID ASC),
		CONSTRAINT FK_RatingCounterComponent_Rating FOREIGN KEY(RatingId) REFERENCES b_Rating(ID),
		CONSTRAINT FK_RatingCounterComponent_RatingCounter FOREIGN KEY(RatingCounterId) REFERENCES b_RatingCounter(ID)
	)
END
GO

IF OBJECT_ID (N'RatingCounterComponent_Delete', N'P') IS NOT NULL
	DROP PROCEDURE RatingCounterComponent_Delete;
GO

CREATE PROCEDURE RatingCounterComponent_Delete 
	@RatingCounterId INT
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM b_RatingComponent WHERE RatingCounterId = @RatingCounterId;
END
GO


IF OBJECT_ID (N'Rating_EngageRatings', N'P') IS NOT NULL
	DROP PROCEDURE Rating_EngageRatings;
GO

CREATE PROCEDURE Rating_EngageRatings 
	@BoundEntityId NVARCHAR(64),
	@BoundEntityTypeHash INT,
	@BoundEntityHash INT,
	@ConfigXmlTagName NVARCHAR(64),
	@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @Count INT;
	SET @Count = 0;
						
	UPDATE b_RatingCounter SET IsCalculated = 0, IsBuilt = 0 
	WHERE BoundEntityHash = @BoundEntityHash
		AND RatingId IN(
			SELECT ID FROM b_Rating r
			WHERE r.Active = 1 
				AND r.BoundEntityTypeHash = @BoundEntityTypeHash 
				AND r.ComponentConfigXml.exist(N'/*[local-name()=sql:variable("@ConfigXmlTagName")]') = 1
			);
	SET @Count = @Count + @@ROWCOUNT;
	
	INSERT INTO b_RatingCounter(RatingId, BoundEntityHash, BoundEntityTypeId, BoundEntityId, CustomPropertyEntityId, CurValCustomFieldName, PrevValCustomFieldName, CalculationMethod, RefreshMethod, CurrentValue, PreviousValue, Created, LastModified, LastCalculated, IsCalculated, IsBuilt)
	SELECT r.ID, @BoundEntityHash, r.BoundEntityTypeId, @BoundEntityId, r.CustomPropertyEntityId, r.CurValCustomFieldName, r.PrevValCustomFieldName, r.CalculationMethod, r.RefreshMethod, 0.0, 0.0, @Now, @Now, @Now, 0, 0
		FROM b_Rating r 
		WHERE r.Active = 1 
			AND r.BoundEntityTypeHash = @BoundEntityTypeHash 
			AND r.ComponentConfigXml.exist(N'/*[local-name()=sql:variable("@ConfigXmlTagName")]') = 1
			AND NOT EXISTS (SELECT TOP 1 ID FROM b_RatingCounter c WHERE c.RatingId = r.ID AND c.BoundEntityHash = @BoundEntityHash);	
	SET @Count = @Count + @@ROWCOUNT;
		
	SELECT @Count;
END
GO

IF OBJECT_ID (N'Rating_DisengageRatings', N'P') IS NOT NULL
	DROP PROCEDURE Rating_DisengageRatings;
GO

CREATE PROCEDURE Rating_DisengageRatings 
	@BoundEntityTypeHash INT,
	@BoundEntityHash INT,
	@ConfigXmlTagName NVARCHAR(64)
AS
BEGIN
	SET NOCOUNT ON;
	
	BEGIN TRANSACTION;
	
	DECLARE @c TABLE(ID INT NOT NULL);
	INSERT INTO @c(ID)
		SELECT ID FROM b_RatingCounter 
			WHERE BoundEntityHash = @BoundEntityHash 
				AND RatingId IN (
					SELECT ID 
						FROM b_Rating 
						WHERE Active = 1 
							AND BoundEntityTypeHash = @BoundEntityTypeHash
							AND ComponentConfigXml.exist(N'/*[local-name()=sql:variable("@ConfigXmlTagName")]') = 1
					);		
	
	DELETE FROM b_RatingComponent 
	WHERE RatingCounterId IN (SELECT ID FROM @c)
	AND ConfigXml.exist(N'/*[local-name()=sql:variable("@ConfigXmlTagName")]') = 1; 						
	
	UPDATE b_RatingCounter SET IsCalculated = 0 
	WHERE ID IN (SELECT ID FROM @c);
	
	SELECT @@ROWCOUNT;	
	COMMIT TRANSACTION;
END
GO

IF OBJECT_ID (N'Rating_IsComponentEnabled', N'P') IS NOT NULL
	DROP PROCEDURE Rating_IsComponentEnabled;
GO

CREATE PROCEDURE Rating_IsComponentEnabled
	@ConfigXmlTagName NVARCHAR(64)
AS
BEGIN
	SELECT CASE WHEN EXISTS(SELECT TOP 1 ID FROM b_Rating WHERE Active = 1 AND ComponentConfigXml.exist(N'/*[local-name()=sql:variable("@ConfigXmlTagName")]') = 1) THEN 1 ELSE 0 END;
END
GO

IF OBJECT_ID (N'RatingVoting_CheckPresence', N'P') IS NOT NULL
	DROP PROCEDURE RatingVoting_CheckPresence;
GO

CREATE PROCEDURE RatingVoting_CheckPresence 
	@RatingId INT,
	@BoundEntityHash INT
AS
BEGIN
	SET NOCOUNT ON;
	SELECT (CASE WHEN EXISTS (SELECT c.ID FROM b_RatingCounter c WHERE c.RatingId = @RatingId AND c.BoundEntityHash = @BoundEntityHash) THEN 1 ELSE 0 END) Rslt;
END
GO

IF OBJECT_ID (N'Rating_CheckUnderCalculation', N'P') IS NOT NULL
	DROP PROCEDURE Rating_CheckUnderCalculation;
GO

CREATE PROCEDURE Rating_CheckUnderCalculation 
	@RatingId INT
AS
BEGIN
	SET NOCOUNT ON;
	SELECT 
		CASE 
			WHEN EXISTS(SELECT N'*' FROM b_Rating WHERE ID = @RatingId AND IsUnderCalculation = 1) THEN 1 
			ELSE 0 END;
END
GO

IF OBJECT_ID (N'Rating_PutUnderCalculation', N'P') IS NOT NULL
	DROP PROCEDURE Rating_PutUnderCalculation;
GO

CREATE PROCEDURE Rating_PutUnderCalculation 
	@RatingId INT,
	@SynchronizerId INT,
	@Started DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE b_Rating SET IsUnderCalculation = 0, SynchronizerId = 0 WHERE IsUnderCalculation = 1 AND SynchronizerId NOT IN (SELECT ID FROM b_Scheduler WHERE Active = 1);	
	UPDATE b_Rating SET IsUnderCalculation = 1, SynchronizerId = @SynchronizerId, LastCalculationStarted = @Started WHERE ID = @RatingId AND IsUnderCalculation = 0;
	SELECT @@ROWCOUNT; 
END
GO

IF OBJECT_ID (N'Rating_CheckUnderSpecifiedCalculation', N'P') IS NOT NULL
	DROP PROCEDURE Rating_CheckUnderSpecifiedCalculation;
GO

CREATE PROCEDURE Rating_CheckUnderSpecifiedCalculation 
	@RatingId INT,
	@SynchronizerId INT
AS
BEGIN
	SET NOCOUNT ON;	
	UPDATE b_Rating SET IsUnderCalculation = 0, SynchronizerId = 0 WHERE IsUnderCalculation = 1 AND SynchronizerId NOT IN (SELECT ID FROM b_Scheduler WHERE Active = 1);	
	SELECT CASE WHEN EXISTS(SELECT N'*' FROM b_Rating WHERE ID = @RatingId AND IsUnderCalculation = 1 AND SynchronizerId = @SynchronizerId) THEN 1 ELSE 0 END;
END
GO

IF OBJECT_ID (N'Rating_PutOutOfCalculation', N'P') IS NOT NULL
	DROP PROCEDURE Rating_PutOutOfCalculation;
GO

CREATE PROCEDURE Rating_PutOutOfCalculation 
	@RatingId INT,
	@SynchronizerId INT,
	@Completed DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE b_Rating SET IsUnderCalculation = 0, SynchronizerId = 0, LastCalculated = @Completed WHERE ID = @RatingId AND SynchronizerId = @SynchronizerId;
	SELECT @@ROWCOUNT;
END
GO

IF OBJECT_ID (N'Rating_Refresh', N'P') IS NOT NULL
	DROP PROCEDURE Rating_Refresh;
GO

CREATE PROCEDURE Rating_Refresh 
	@UtcNow DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	IF(@UtcNow IS NULL) SET @UtcNow = GETUTCDATE();
	BEGIN TRANSACTION;
	DECLARE @t TABLE(ID INT, RatingCounterId INT);
	INSERT INTO @t(ID, RatingCounterId)
		SELECT c.ID, c.RatingCounterId FROM b_RatingComponent c INNER JOIN b_Rating r ON c.RatingId = r.ID AND r.Active = 1 AND r.IsUnderCalculation = 0 AND (c.IsCalculated = 0 OR (c.RefreshInterval > 0 AND c.LastCalculatedUtc < @UtcNow AND c.RefreshInterval <= DATEDIFF(minute, c.LastCalculatedUtc, @UtcNow)));
		
	UPDATE b_RatingComponent SET IsCalculated = 0, LastModifiedUtc = @UtcNow WHERE ID IN (SELECT ID FROM @t);	
	UPDATE b_RatingCounter SET IsCalculated = 0, LastModified = @UtcNow WHERE ID IN (SELECT RatingCounterId FROM @t); 	
	COMMIT TRANSACTION;
END
GO

IF OBJECT_ID (N'RatingCounter_Delete', N'P') IS NOT NULL
	DROP PROCEDURE RatingCounter_Delete;
GO

CREATE PROCEDURE RatingCounter_Delete
	@RatingId INT
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM b_RatingComponent WHERE RatingId = @RatingId;	
	DELETE FROM b_RatingCounter WHERE RatingId = @RatingId;
END
GO	

IF OBJECT_ID (N'RatingComponentEntity_Delete', N'P') IS NOT NULL
	DROP PROCEDURE RatingComponentEntity_Delete;
GO

CREATE PROCEDURE RatingComponentEntity_Delete
	@RatingCounterId INT
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM b_RatingComponent WHERE RatingCounterId = @RatingCounterId;	
END
GO	

IF OBJECT_ID (N'RatingCounter_MarkForRebuild', N'P') IS NOT NULL
	DROP PROCEDURE RatingCounter_MarkForRebuild; 
GO

CREATE PROCEDURE RatingCounter_MarkForRebuild
	@RatingId INT,
	@BoundEntityId NVARCHAR(64) = NULL
AS
BEGIN
	SET NOCOUNT ON;
	IF(EXISTS (SELECT N'*' FROM b_Rating WHERE ID = @RatingId AND IsUnderCalculation = 0))
	BEGIN
		IF(@BoundEntityId IS NULL)
			UPDATE b_RatingCounter SET IsBuilt = 0, IsCalculated = 0 WHERE RatingId = @RatingId;
		ELSE
			UPDATE b_RatingCounter SET IsBuilt = 0, IsCalculated = 0 WHERE RatingId = @RatingId AND BoundEntityId = @BoundEntityId;
	END
END
GO

IF OBJECT_ID (N'Rating_GetRequiredForCalculation', N'P') IS NOT NULL
	DROP PROCEDURE Rating_GetRequiredForCalculation;
GO

CREATE PROCEDURE Rating_GetRequiredForCalculation 
	@MaxCount INT = 1,
	@LockOutExpiried DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	SELECT TOP(CASE WHEN @MaxCount > 0 THEN @MaxCount ELSE 1 END) R.ID 
	FROM b_Rating R
	WHERE R.Active = 1 AND EXISTS(SELECT TOP 1 ID FROM b_RatingCounter WHERE RatingId = R.ID AND (IsBuilt = 0 OR IsCalculated = 0) AND (IsLockedOut = 0 OR LockOutStarted <= @LockOutExpiried)) 
	ORDER BY R.ID;
END
GO

IF OBJECT_ID (N'RatingCounter_SynchronizeRating', N'P') IS NOT NULL
	DROP PROCEDURE RatingCounter_SynchronizeRating; 
GO

CREATE PROCEDURE RatingCounter_SynchronizeRating
	@RatingId INT
AS
BEGIN
	SET NOCOUNT ON;
	IF(EXISTS (SELECT N'*' FROM b_Rating WHERE ID = @RatingId))
	BEGIN
		UPDATE b_RatingCounter SET CurValCustomFieldName = r.CurValCustomFieldName, PrevValCustomFieldName = r.PrevValCustomFieldName
		FROM  b_Rating r WHERE r.ID = @RatingId AND RatingId = @RatingId;
	END
END
GO

IF OBJECT_ID (N'RatingVoting_GetEngagingUsers', N'P') IS NOT NULL
	DROP PROCEDURE RatingVoting_GetEngagingUsers; 
GO

CREATE PROCEDURE RatingVoting_GetEngagingUsers
	@RatingId INT,
	@Count INT
AS
BEGIN
	SET NOCOUNT ON;	
	
	IF(@Count IS NULL OR @Count <= 0) SET @Count = 5;
	
	SELECT TOP(@Count) u.ID
	FROM b_RatingVoting v 
	INNER JOIN b_Users u 
		ON v.BoundEntityTypeId = N'USER'
			AND v.BoundEntityId = u.ID 
			AND NOT EXISTS(SELECT ID FROM b_RatingCounter c WHERE c.RatingId = @RatingId AND c.BoundEntityTypeId = N'USER' AND c.BoundEntityId = u.ID);
END
GO

IF OBJECT_ID (N'Rating_IsAnyActive', N'P') IS NOT NULL
	DROP PROCEDURE Rating_IsAnyActive;
GO

CREATE PROCEDURE Rating_IsAnyActive
AS
BEGIN
	SELECT CASE WHEN EXISTS(SELECT TOP 1 ID FROM b_Rating WHERE Active = 1)  THEN 1 ELSE 0 END;
END
GO

IF OBJECT_ID (N'Rating_GetCountersToProcess', N'P') IS NOT NULL
	DROP PROCEDURE Rating_GetCountersToProcess;
GO

CREATE PROCEDURE Rating_GetCountersToProcess 
	@RatingId INT,
	@MaxCount INT,
	@LockOutExpiried DATETIME
AS
BEGIN
	SELECT TOP(CASE WHEN @MaxCount > 0 THEN @MaxCount ELSE 1 END) ID FROM b_RatingCounter 
	WHERE RatingId = @RatingId AND (IsBuilt = 0 OR IsCalculated = 0) AND (IsLockedOut = 0 OR LockOutStarted <= @LockOutExpiried)
	ORDER BY ID ASC;
END
GO
/*--- END OF RATINGS   ---*/
IF OBJECT_ID (N'UserInfo_SelectById', N'P') IS NOT NULL
	DROP PROCEDURE UserInfo_SelectById;
GO

CREATE PROCEDURE UserInfo_SelectById
	@Id INT
AS
BEGIN
	SET NOCOUNT ON;	
	WITH userRoles AS
	(
		SELECT r.ID, r.RoleName 
		FROM b_UsersInRoles ur INNER JOIN b_Roles r on r.ID = ur.RoleID AND ur.UserID = @Id 
		UNION
		SELECT r.ID, r.RoleName FROM b_Roles r WHERE r.ID IN(2,3)	
	)
	SELECT TOP 1 
		u.ID, u.UserName, u.ProviderName, u.IsApproved, u.IsLockedOut,
		(SELECT ID AS '@id', RoleName AS '@name' FROM userRoles FOR XML PATH('role')) RoleXml
	FROM b_Users u
	WHERE u.ID = @Id;	
END
GO
/*--- BEGINNING OF PROMOTING ---*/
IF OBJECT_ID (N'b_PromoRule', N'U') IS NULL
BEGIN
	CREATE TABLE b_PromoRule
	(
		ID INT IDENTITY(1,1) NOT NULL,
		Name NVARCHAR(256) NOT NULL,
		Active BIT NOT NULL,
		BoundEntityTypeId NVARCHAR(256) NOT NULL,
		ConditionTypeFullName NVARCHAR(1024) NOT NULL,
		ConditionXml XML NOT NULL,
		ActionTypeFullName NVARCHAR(1024) NOT NULL,
		ActionXml XML NOT NULL,
		CreatedUtc DATETIME NOT NULL,
		LastModifiedUtc DATETIME NOT NULL,
		LastAppliedUtc DATETIME NULL,
		IsPending BIT NULL,
		LockoutedUtc DATETIME NULL,
		XmlId VARCHAR(256) NULL,
		CONSTRAINT PK_b_PromoRule PRIMARY KEY CLUSTERED(ID ASC)
	)
END
GO

IF OBJECT_ID (N'b_PromoVetting', N'U') IS NULL
BEGIN
	CREATE TABLE b_PromoVetting
	(
		ID INT IDENTITY(1,1) NOT NULL,
		RuleId INT NOT NULL,
		ConditionTypeFullName NVARCHAR(1024) NOT NULL,
		BoundEntityTypeId NVARCHAR(256) NOT NULL,
		BoundEntityId NVARCHAR(64) NOT NULL,
		CreatedUtc DATETIME NOT NULL,
		LastModifiedUtc DATETIME NOT NULL,
		ProcessedUtc DATETIME NULL,
		Result BIT NOT NULL,
		IsUpToDate BIT NOT NULL,
		IsActionTaken BIT NOT NULL,
		CONSTRAINT PK_b_PromoVetting PRIMARY KEY CLUSTERED(ID ASC)
	)
END
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_PromoVetting_RuleId_IsUpToDate')
	CREATE INDEX IX_b_PromoVetting_RuleId_IsUpToDate ON b_PromoVetting(RuleId ASC) INCLUDE(IsUpToDate);
GO

IF OBJECT_ID (N'PromoRule_TryGetLock', N'P') IS NOT NULL
	DROP PROCEDURE PromoRule_TryGetLock;
GO

CREATE PROCEDURE PromoRule_TryGetLock 
	@RuleId INT,
	@LockoutedUtc DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE b_PromoRule SET LockoutedUtc = @LockoutedUtc, IsPending = 1 WHERE ID = @RuleId AND LockoutedUtc IS NULL;
	SELECT @@ROWCOUNT; 
END
GO

IF OBJECT_ID (N'PromoRule_CheckLock', N'P') IS NOT NULL
	DROP PROCEDURE PromoRule_CheckLock;
GO

CREATE PROCEDURE PromoRule_CheckLock 
	@RuleId INT,
	@LockoutedUtc DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	SELECT CASE 
		WHEN EXISTS(SELECT ID FROM b_PromoRule WHERE ID = @RuleId AND LockoutedUtc = @LockoutedUtc) 
		THEN 1 ELSE 0 END;
END
GO

IF OBJECT_ID (N'PromoRule_TryUnlock', N'P') IS NOT NULL
	DROP PROCEDURE PromoRule_TryUnlock;
GO

CREATE PROCEDURE PromoRule_TryUnlock 
	@RuleId INT,
	@LockoutedUtc DATETIME,
	@LastAppliedUtc DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE b_PromoRule SET LockoutedUtc = NULL, LastAppliedUtc = @LastAppliedUtc, IsPending = 0 WHERE ID = @RuleId AND LockoutedUtc = @LockoutedUtc;
	SELECT @@ROWCOUNT; 
END
GO

IF OBJECT_ID (N'PromoRule_SelectRequiredToApply', N'P') IS NOT NULL
	DROP PROCEDURE PromoRule_SelectRequiredToApply;
GO

CREATE PROCEDURE PromoRule_SelectRequiredToApply
	@Top INT
AS
BEGIN
	SET NOCOUNT ON;	
	SELECT DISTINCT TOP(@Top) v.RuleId 
	FROM b_PromoVetting v INNER JOIN b_PromoRule r ON v.RuleId = r.ID
	WHERE r.Active = 1 AND v.IsUpToDate = 0;
END
GO

IF OBJECT_ID (N'PromoVetting_MarkAsProcessed', N'P') IS NOT NULL
	DROP PROCEDURE PromoVetting_MarkAsProcessed;
GO

CREATE PROCEDURE PromoVetting_MarkAsProcessed 
	@VettingId INT,
	@Result BIT,
	@IsActionTaken BIT,
	@ProcessedUtc DATETIME = NULL,
	@PreviousModifiedUtc DATETIME = NULL
AS
BEGIN
	SET NOCOUNT ON;
	IF @ProcessedUtc IS NULL SET @ProcessedUtc = GETUTCDATE();
	IF @PreviousModifiedUtc IS NOT NULL
		UPDATE b_PromoVetting SET Result = @Result, IsActionTaken = @IsActionTaken, ProcessedUtc = @ProcessedUtc, LastModifiedUtc = @ProcessedUtc, IsUpToDate = 1 WHERE ID = @VettingId AND LastModifiedUtc = @PreviousModifiedUtc;
	ELSE
		UPDATE b_PromoVetting SET Result = @Result, IsActionTaken = @IsActionTaken, ProcessedUtc = @ProcessedUtc, LastModifiedUtc = @ProcessedUtc, IsUpToDate = 1 WHERE ID = @VettingId;
	SELECT @@ROWCOUNT;
END
GO

IF OBJECT_ID (N'RatingPromoSyncManager_InvalidateVettings', N'P') IS NOT NULL
	DROP PROCEDURE RatingPromoSyncManager_InvalidateVettings;
GO

CREATE PROCEDURE RatingPromoSyncManager_InvalidateVettings 
	@RatingId INT,
	@BoundEntityTypeId NVARCHAR(256),
	@BoundEntityId NVARCHAR(64),
	@UtcNow DATETIME = NULL
AS
BEGIN
	IF @UtcNow IS NULL SET @UtcNow = GETUTCDATE();
	UPDATE b_PromoVetting SET IsUpToDate = 0, LastModifiedUtc = @UtcNow
	WHERE BoundEntityTypeId = @BoundEntityTypeId
		AND BoundEntityId  = @BoundEntityId  
		AND RuleId IN (
			SELECT ID FROM b_PromoRule 
			WHERE Active = 1 
				AND ConditionXml.exist(N'./ratingPromoCondConfig[@ratingId = sql:variable("@RatingId")]') = 1);
	SELECT @@ROWCOUNT;
END
GO

IF OBJECT_ID (N'RatingPromoSyncManager_IsActive', N'P') IS NOT NULL
	DROP PROCEDURE RatingPromoSyncManager_IsActive;
GO

CREATE PROCEDURE RatingPromoSyncManager_IsActive
AS
BEGIN
	SELECT CASE WHEN EXISTS(SELECT TOP 1 ID FROM b_PromoRule WHERE Active = 1) THEN 1 ELSE 0 END;
END
GO

IF OBJECT_ID (N'PromoRule_IsPending', N'P') IS NOT NULL
	DROP PROCEDURE PromoRule_IsPending;
GO

CREATE PROCEDURE PromoRule_IsPending 
	@RuleId INT
AS
BEGIN
	SET NOCOUNT ON;
	SELECT CASE 
		WHEN EXISTS(SELECT ID FROM b_PromoRule WHERE ID = @RuleId AND IsPending = 1) 
		THEN 1 ELSE 0 END
END;
GO

IF OBJECT_ID(N'UserPromoVettingSync_Create', N'P') IS NOT NULL
	DROP PROCEDURE UserPromoVettingSync_Create;
GO

CREATE PROCEDURE UserPromoVettingSync_Create
	@PromoRuleId INT,
	@UtcNow DATETIME = NULL
AS
BEGIN
	IF @PromoRuleId IS NULL OR @PromoRuleId <= 0 RETURN;
	IF @UtcNow IS NULL SET @UtcNow = GETUTCDATE();
	INSERT INTO b_PromoVetting(RuleId, ConditionTypeFullName, BoundEntityTypeId, BoundEntityId, CreatedUtc, LastModifiedUtc, Result, IsUpToDate, IsActionTaken)
	SELECT r.ID, r.ConditionTypeFullName, r.BoundEntityTypeId, CAST(u.ID AS NVARCHAR(64)), @UtcNow, @UtcNow, 0, 0, 0 FROM b_PromoRule r CROSS JOIN b_Users u WHERE r.ID = @PromoRuleId AND r.Active = 1 AND r.BoundEntityTypeId = N'USER';	
END
GO

IF OBJECT_ID(N'UserPromoVettingSync_Delete', N'P') IS NOT NULL
	DROP PROCEDURE UserPromoVettingSync_Delete;
GO

CREATE PROCEDURE UserPromoVettingSync_Delete
	@PromoRuleId INT
AS
BEGIN
	DELETE FROM b_PromoVetting WHERE RuleId = @PromoRuleId AND BoundEntityTypeId = N'USER';
END

GO

IF OBJECT_ID(N'UserPromoVettingSync_Invalidate', N'P') IS NOT NULL
	DROP PROCEDURE UserPromoVettingSync_Invalidate;
GO

CREATE PROCEDURE UserPromoVettingSync_Invalidate
	@PromoRuleId INT,
	@UtcNow DATETIME = NULL
AS
BEGIN
	IF @UtcNow IS NULL SET @UtcNow = GETUTCDATE();
	UPDATE b_PromoVetting SET IsUpToDate = 0, LastModifiedUtc = @UtcNow WHERE RuleId = @PromoRuleId AND BoundEntityTypeId = N'USER';
END

GO

IF OBJECT_ID(N'UserPromoSyncManager_CreateVettings', N'P') IS NOT NULL
	DROP PROCEDURE UserPromoSyncManager_CreateVettings;
GO

CREATE PROCEDURE UserPromoSyncManager_CreateVettings
	@UserId INT,
	@UtcNow DATETIME = NULL
AS
BEGIN
	IF @UtcNow IS NULL SET @UtcNow = GETUTCDATE();
	IF NOT EXISTS(SELECT ID FROM b_Users WHERE ID = @UserId)
	BEGIN
		SELECT 0;
		RETURN;
	END
	INSERT INTO b_PromoVetting(RuleId, ConditionTypeFullName, BoundEntityTypeId, BoundEntityId, CreatedUtc, LastModifiedUtc, Result, IsUpToDate, IsActionTaken)
	SELECT r.ID, r.ConditionTypeFullName, r.BoundEntityTypeId, CAST(@UserId AS NVARCHAR(64)), @UtcNow, @UtcNow, 0, 0, 0 FROM b_PromoRule r WHERE r.Active = 1 AND r.BoundEntityTypeId = N'USER';	
	SELECT @@ROWCOUNT; 		
END

GO

IF OBJECT_ID(N'UserPromoSyncManager_DeleteVettings', N'P') IS NOT NULL
	DROP PROCEDURE UserPromoSyncManager_DeleteVettings;
GO

CREATE PROCEDURE UserPromoSyncManager_DeleteVettings
	@UserId INT
AS
BEGIN
	DELETE FROM b_PromoVetting WHERE BoundEntityTypeId = N'USER' AND BoundEntityId = CAST(@UserId AS NVARCHAR(64));		
END

GO
/*--- END OF PROMOTING ---*/
/*--- BEGINNING OF LOCALITY---*/
IF OBJECT_ID(N'b_Region', N'U') IS NULL
CREATE TABLE b_Region
(
	ID INT IDENTITY(1,1) NOT NULL,
	CountryCode CHAR(2) NOT NULL,
	NativeName NVARCHAR(512) NOT NULL,
	CreatedUtc DATETIME NOT NULL,
	LastUpdatedUtc DATETIME NOT NULL,
	XmlId VARCHAR(256)
	CONSTRAINT [PK_b_Region_id] PRIMARY KEY CLUSTERED ([ID] ASC)
);
GO

IF OBJECT_ID(N'b_LocationGroup', N'U') IS NULL
CREATE TABLE b_LocationGroup
(
	ID INT IDENTITY(1,1) NOT NULL,
	Name NVARCHAR(512) NOT NULL,
	CreatedUtc DATETIME NOT NULL,
	LastUpdatedUtc DATETIME NOT NULL,
	XmlId VARCHAR(256)
	CONSTRAINT [PK_b_LocationGroup_id] PRIMARY KEY CLUSTERED ([ID] ASC)
);
GO


IF OBJECT_ID(N'b_Location', N'U') IS NULL
CREATE TABLE b_Location
(
	ID INT IDENTITY(1,1) NOT NULL,
	CountryCode CHAR(2) NOT NULL,
	RegionId INT NOT NULL,
	PostalCode NCHAR(9) NOT NULL,
	NativeName NVARCHAR(512) NOT NULL,
	CreatedUtc DATETIME NOT NULL,
	LastUpdatedUtc DATETIME NOT NULL,
	XmlId VARCHAR(256)
	CONSTRAINT [PK_b_Location_id] PRIMARY KEY CLUSTERED ([ID] ASC)
);
GO
/*--- END OF LOCALITY---*/

IF OBJECT_ID(N'CustomField_Create', N'P') IS NOT NULL
DROP PROCEDURE CustomField_Create
GO
CREATE PROCEDURE CustomField_Create
	@entityId nvarchar(128),
	@columnName nvarchar(128),
	@columnType nvarchar(128),
	@columnTypeMax bit = 0
AS
BEGIN
	DECLARE @tableName nvarchar(128)
	DECLARE @query nvarchar(MAX)
	
	SET @tableName = 'b_cts_' + @entityId;
	
	IF OBJECT_ID(@tableName, 'U') IS NULL BEGIN
		SET @query = 
'CREATE TABLE ' + QUOTENAME(@tableName) + '
(
	ValueId int NOT NULL CONSTRAINT ' + QUOTENAME('PK_' + @tableName) + ' PRIMARY KEY CLUSTERED
)'
		EXEC sp_executesql @query
	END	
	
	SET @tableName = 'b_ctm_' + @entityId;
	
	IF OBJECT_ID(@tableName, 'U') IS NULL BEGIN
		SET @query =
'CREATE TABLE ' + QUOTENAME(@tableName) + '
(
	ValueId int NOT NULL,
	FieldId int NOT NULL,
	Value nvarchar(MAX) NULL,
	ValueInt int NULL,
	ValueDouble float(53) NULL,
	ValueDate datetime NULL
)'
		EXEC sp_executesql @query  
	END

	IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_BX_' + @tableName + '_FieldId') BEGIN
		SET @query =
'CREATE NONCLUSTERED INDEX ' + QUOTENAME('IX_BX_' + @tableName + '_FieldId') + ' ON ' + QUOTENAME(@tableName) + '
(
	[FieldId] ASC
)'
		EXEC sp_executesql @query
	END

	IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_BX_' + @tableName + '_ValueId_FieldId') BEGIN
		SET @query =
'CREATE CLUSTERED INDEX ' + QUOTENAME('IX_BX_' + @tableName + '_ValueId_FieldId') + ' ON ' + QUOTENAME(@tableName) + '
(
	[ValueId] ASC,
	[FieldId] ASC
)'
		EXEC sp_executesql @query
	END		
	
	SET @tableName = 'b_cts_' + @entityId;
	DECLARE @type sysname
	SET @type = (SELECT TOP(1) t.name FROM sys.columns c INNER JOIN sys.types t ON c.user_type_id = t.user_type_id WHERE c.object_id = OBJECT_ID(@tableName, 'U') AND c.name = @columnName)
	
	IF (@type IS NULL OR LOWER(@type) != LOWER(@columnType)) BEGIN
		SET @query =
'ALTER TABLE ' + QUOTENAME(@tableName) + ' ADD ' + QUOTENAME(@columnName) + ' ' + QUOTENAME(@columnType) + CASE WHEN @columnTypeMax = 1 THEN '(max)' ELSE '' END + ' NULL'
		EXEC sp_executesql @query 
	END
END
GO
IF OBJECT_ID(N'CustomField_Delete', N'P') IS NOT NULL
DROP PROCEDURE CustomField_Delete
GO
CREATE PROCEDURE CustomField_Delete
	@entityId nvarchar(128),
	@columnName nvarchar(128),
	@fieldId int
AS
BEGIN
	DECLARE @tableName nvarchar(128)
	DECLARE @query nvarchar(MAX)
	
	SET @tableName = 'b_cts_' + @entityId;
	
	IF OBJECT_ID(@tableName, 'U') IS NOT NULL
	AND EXISTS(
		SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
		WHERE 
			TABLE_NAME = @tableName
			AND COLUMN_NAME = @columnName
	) BEGIN
		SET @query =
'ALTER TABLE ' + QUOTENAME(@tableName) + ' DROP COLUMN ' + QUOTENAME(@columnName)
		EXEC sp_executesql @query
	END
	
	SET @tableName = 'b_ctm_' + @entityId;
	IF OBJECT_ID(@tableName, 'U') IS NOT NULL BEGIN
		SET @query =
'DELETE FROM ' + QUOTENAME(@tableName) + ' WHERE FieldId = @fieldId'
		EXEC sp_executesql @query, N'@fieldId int', @fieldId = @fieldId				
	END
END
GO

IF (OBJECT_ID(N'b_UserToken', N'U') IS NULL)
	CREATE TABLE b_UserToken
	(
		Id int NOT NULL IDENTITY(1, 1) CONSTRAINT PK_b_UserToken_Id PRIMARY KEY CLUSTERED,
		UserId int NOT NULL,
		Token varchar(256) NOT NULL,
		Type int NOT NULL,
		DateCreatedUtc datetime NOT NULL,
		DateExpiresUtc datetime NOT NULL,
		Info nvarchar(max)
	)
GO

IF OBJECT_ID (N'b_Undo', N'U') IS NULL
	CREATE TABLE b_Undo
	(
		ID INT IDENTITY(1,1) NOT NULL,
		UserId INT NOT NULL,
		CreatedUtc DATETIME NOT NULL,
		OperationXml XML NULL,
		CONSTRAINT PK_b_Undo PRIMARY KEY(ID ASC)
	)
GO

IF OBJECT_ID(N'Undo_Truncate', N'P') IS NOT NULL DROP PROCEDURE [Undo_Truncate]
GO
CREATE PROCEDURE [Undo_Truncate]
    @Threshold DATETIME
AS
BEGIN
	DELETE FROM [b_Undo] WHERE [CreatedUtc] <= @Threshold;
END
GO

IF OBJECT_ID (N'b_FileCacheTag', N'U') IS NULL
	CREATE TABLE b_FileCacheTag
	(
		SiteId VARCHAR(50) NOT NULL,
		Path NVARCHAR(512) NOT NULL,
		PathHash INT NOT NULL,
		Tag NVARCHAR(128) NOT NULL,
		CreatedUtc DATETIME NOT NULL
	)
GO

IF OBJECT_ID(N'b_FileCacheTag', N'U') IS NOT NULL AND NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_FileCacheTag_PathHash')
CREATE CLUSTERED INDEX IX_b_FileCacheTag_PathHash ON b_FileCacheTag
(
	PathHash ASC
)
GO

IF OBJECT_ID(N'b_FileCacheTag', N'U') IS NOT NULL AND NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_FileCacheTag_Tag')
CREATE NONCLUSTERED INDEX IX_b_FileCacheTag_Tag ON b_FileCacheTag
(
	Tag ASC
)
GO

IF OBJECT_ID(N'b_FileCacheTag', N'U') IS NOT NULL AND NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_FileCacheTag_SiteId_PathHash')
CREATE NONCLUSTERED INDEX IX_b_FileCacheTag_SiteId_PathHash ON b_FileCacheTag
(
	SiteId ASC,
	PathHash ASC
)
GO

IF OBJECT_ID(N'FileCacheTag_GetPaths', N'P') IS NOT NULL DROP PROCEDURE FileCacheTag_GetPaths
GO
CREATE PROCEDURE FileCacheTag_GetPaths
	@TagsXml XML --<tags><tag>iblock:15</tag></tags>
AS
BEGIN
	SELECT [Path] FROM [b_FileCacheTag] WHERE Tag IN (SELECT t.c.value('.', 'nvarchar(128)') AS tag FROM @TagsXml.nodes(N'tags/tag') t(c));	
END
GO

IF OBJECT_ID(N'FileCacheTag_Unregister', N'P') IS NOT NULL DROP PROCEDURE FileCacheTag_Unregister
GO
CREATE PROCEDURE FileCacheTag_Unregister
	@PathHashXml XML --<pathHashs><pathHash>1457</pathHash></pathHashs>
AS
BEGIN
	DELETE FROM [b_FileCacheTag] WHERE PathHash IN (SELECT t.c.value('.', 'int') AS PathHash FROM @PathHashXml.nodes(N'pathHashs/pathHash') t(c));	
END
GO

IF OBJECT_ID(N'FileCacheTag_Register', N'P') IS NOT NULL DROP PROCEDURE [FileCacheTag_Register]
GO
CREATE PROCEDURE [FileCacheTag_Register]
    @Xml XML,
    @UtcNow DATETIME
AS
BEGIN
	DECLARE @tab TABLE(SiteId VARCHAR(50), [Path] NVARCHAR(512), PathHash INT, Tag NVARCHAR(128));

	INSERT INTO @tab(SiteId, [Path], PathHash, Tag)
	SELECT 
		t.c.value('@siteId', 'varchar(50)'), 
		t.c.value('@path', 'nvarchar(512)'),
		t.c.value('@pathHash', 'int'),
		t.c.value('@tag', 'nvarchar(128)')
	FROM @Xml.nodes('fileCacheTags/fileCacheTag') as t(c);

	INSERT INTO b_FileCacheTag(SiteId, Path, PathHash, Tag, CreatedUtc)		
		SELECT newTags.SiteId, newTags.[Path], newTags.PathHash, newTags.Tag, @UtcNow FROM @tab AS newTags 
		WHERE NOT EXISTS(SELECT N'X' FROM b_FileCacheTag AS oldTags where newTags.SiteId = oldTags.SiteId and newTags.PathHash = oldTags.PathHash and newTags.Tag = oldTags.Tag);
END
GO

IF OBJECT_ID(N'Roles_SynchronizeProviderRolesXml', N'P') IS NOT NULL DROP PROCEDURE [Roles_SynchronizeProviderRolesXml]
GO
CREATE PROCEDURE [Roles_SynchronizeProviderRolesXml]
	@ProviderName varchar(128),
	@RoleXml xml --<roles><role>CN=Domain Admims,CN=Users,DC=netforge,DC=local</role></roles>
AS
BEGIN
	DECLARE @TranStarted bit;
	SET @TranStarted = 0;

	IF (@@TRANCOUNT = 0)
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1;
	END;

	WITH Roles(Name) AS(SELECT (@ProviderName + N'\' + LTRIM(RTRIM(t.c.value('.', 'nvarchar(128)')))) AS Name FROM @RoleXml.nodes(N'roles/role') t(c))
		INSERT INTO [b_Roles](RoleName, LoweredRoleName, Active, Comment, Policy, EffectivePolicy)
			SELECT r.Name, LOWER(r.Name), 1, null, '<Policy/>', null 
				FROM Roles r
				WHERE LOWER(r.Name) NOT IN (SELECT r.LoweredRoleName FROM [b_Roles] r);

	IF (@TranStarted = 1)
		COMMIT TRANSACTION;

	RETURN 0;
END
GO

IF OBJECT_ID(N'Roles_SynchronizeProviderUserRolesXml', N'P') IS NOT NULL DROP PROCEDURE [Roles_SynchronizeProviderUserRolesXml]
GO
CREATE PROCEDURE [Roles_SynchronizeProviderUserRolesXml]
	@UserName nvarchar(256),
	@MembershipProviderName varchar(128),
	@RoleProviderName varchar(128),
	@RoleXml xml --<roles><role>CN=Domain Admims,CN=Users,DC=netforge,DC=local</role></roles>
AS
BEGIN

	DECLARE @UserId int;

	SELECT @UserId = u.ID FROM b_Users u
	WHERE u.LoweredUserName = LOWER(@UserName) AND u.LoweredProviderName = LOWER(@MembershipProviderName);
		
    IF (@UserId IS NULL)
        RETURN 1;

	DECLARE @TranStarted bit;
	SET @TranStarted = 0;

	IF (@@TRANCOUNT = 0)
	BEGIN
		BEGIN TRANSACTION;
		SET @TranStarted = 1;
	END;

	DECLARE @Roles table(Name nvarchar(128) NOT NULL PRIMARY KEY);
	
	INSERT INTO @Roles(Name) 
		SELECT (@RoleProviderName + N'\' + LTRIM(RTRIM(t.c.value('.', 'nvarchar(128)')))) FROM @RoleXml.nodes(N'roles/role') t(c);

	DELETE FROM b_UsersInRoles
	WHERE UserID = @UserId
		AND RoleID IN (
			SELECT r.ID FROM [b_Roles] r LEFT JOIN @Roles n ON r.LoweredRoleName = LOWER(n.Name)
			WHERE r.LoweredRoleName LIKE LOWER(@RoleProviderName) + N'\%' AND n.Name IS NULL);

	INSERT INTO b_Roles(RoleName, LoweredRoleName, Active, Comment, Policy, EffectivePolicy)
	SELECT n.Name, LOWER(n.Name), 1, NULL, '<Policy/>', NULL FROM @Roles n
	WHERE LOWER(n.Name) NOT IN (SELECT r.LoweredRoleName FROM [b_Roles] r);

	INSERT INTO b_UsersInRoles(UserID, RoleID)
	SELECT @UserId, r.ID FROM b_Roles r INNER JOIN @Roles n ON r.LoweredRoleName = LOWER(n.Name)
	WHERE r.ID NOT IN (SELECT uir.RoleID FROM b_UsersInRoles uir WHERE uir.UserID = @UserId);

	IF (@TranStarted = 1)
		COMMIT TRANSACTION;

	RETURN 0;
END
GO

IF OBJECT_ID (N'b_StorageConfig', N'U') IS NULL
	CREATE TABLE b_StorageConfig
	(
		ID INT IDENTITY(1,1) NOT NULL,
		IsActive BIT NOT NULL,
		Sort INT NOT NULL,
		StorageTypeName NVARCHAR(512) NOT NULL,
		SettingXml Xml NULL,
		BindingXml Xml NULL,
		CreatedUtc DateTime NOT NULL,
		LastModifiedUtc DateTime NOT NULL,
		XmlId VarChar(256) NULL
		CONSTRAINT PK_b_StorageConfig PRIMARY KEY (ID ASC)
	);  
GO

IF OBJECT_ID(N'Scheduler_GetAgentsToProcessLocked', N'P') IS NOT NULL
	DROP PROCEDURE Scheduler_GetAgentsToProcessLocked
GO

CREATE PROCEDURE Scheduler_GetAgentsToProcessLocked 
	@start datetime,
	@release datetime,
	@locked_by nvarchar(128),
	@lock_release_utc datetime,
	@next_start datetime,
	@next_release datetime,
	@top int,
	@next_agent datetime = NULL OUTPUT
AS BEGIN		
	UPDATE 
		TOP(@top)
		b_Scheduler WITH (TABLOCKX)
	SET
		lock_release_utc = @lock_release_utc,
		last_locked_by = @locked_by
	OUTPUT	
		DELETED.id Id,
		DELETED.name Name,
		DELETED.classname ClassName,
		DELETED.[assembly] [Assembly],
		DELETED.[parameters] [Parameters],
		DELETED.starttime StartTime,
		DELETED.periodic Periodic,
		DELETED.period Period,
		DELETED.active Active,
		DELETED.last_updated LastUpdated
	WHERE
		active = 1
		AND starttime <= @start
		AND lock_release_utc <= @release		

	SET @next_agent = (SELECT TOP(1) starttime FROM b_Scheduler WHERE active = 1 AND starttime <= @next_start AND lock_release_utc <= @next_release)
END
GO

IF OBJECT_ID(N'MailerEvents_GetEventsToLocked', N'P') IS NOT NULL
	DROP PROCEDURE MailerEvents_GetEventsToLocked
GO

CREATE PROCEDURE MailerEvents_GetEventsToLocked 
	@status char(1),
	@locked_by nvarchar(128),
	@lock_release_utc datetime,
	@top int,
	@release datetime
AS BEGIN		
	UPDATE 
		TOP(@top)
		b_MailerEvents WITH (TABLOCKX)
	SET
		lock_release_utc = @lock_release_utc,
		last_locked_by = @locked_by
	OUTPUT	
		DELETED.[id] Id,
		DELETED.[template] template,
		DELETED.[site] [site],
		DELETED.[parameters] [parameters],
		DELETED.[duplicate] duplicate,
		DELETED.[status] [status],
		DELETED.[last_updated] [last_updated],
		DELETED.[template_id] [template_id]
	WHERE
		[status] = @status
		AND lock_release_utc <= @release
END
GO

IF OBJECT_ID(N'File_DeleteTemporary', N'P') IS NOT NULL DROP PROCEDURE [File_DeleteTemporary]
GO
CREATE PROCEDURE [File_DeleteTemporary]
	@Threshold DATETIME,
	@Hours INT
AS
BEGIN
	DELETE FROM b_File WHERE LEN(TempGuid) > 0 AND DATEDIFF(HOUR, UpdateDate, @Threshold) >= @Hours;
END
GO

IF OBJECT_ID(N'File_DeleteActions', N'P') IS NOT NULL DROP PROCEDURE File_DeleteActions
GO
CREATE PROCEDURE File_DeleteActions
	@Hours INT
AS
BEGIN
	DECLARE @Threshold DATETIME;
	SET @Threshold = GETDATE();
	DELETE FROM b_FileAction WHERE SuccessExec = 'Y' AND ExecDate IS NULL OR DATEDIFF(DAY, ExecDate, @Threshold) >= @Hours;
END
GO