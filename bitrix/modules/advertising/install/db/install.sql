IF OBJECT_ID (N'b_AdvSpace', N'U') IS NULL
BEGIN
	CREATE TABLE [b_AdvSpace](
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[Code] [nvarchar](256) NOT NULL,
		[Active] [bit] NOT NULL,
		[AuthorId] [int] NOT NULL,
		[DateCreated] [datetime] NOT NULL,
		[LastModificationAuthorId] [int] NOT NULL,
		[DateLastModified] [datetime] NOT NULL,
		[Name] [nvarchar](256) NOT NULL,
		[Description] [nvarchar](2048) NULL,
		[Sort] [int] NOT NULL,
		[XmlId] [varchar](256) NULL,
		CONSTRAINT [PK_b_AdvSpace] PRIMARY KEY CLUSTERED([ID] ASC)
	)
END
GO

IF OBJECT_ID (N'b_AdvBanner', N'U') IS NULL
BEGIN
	CREATE TABLE [b_AdvBanner](
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[Code] [nvarchar](256) NOT NULL,
		[Active] [bit] NOT NULL,
		[SpaceId] [int] NOT NULL,
		[Status] [tinyint] NOT NULL,
		[Weight] [int] NOT NULL,
		[EnableFixedRotation] [bit] NOT NULL,
		[EnableUniformRotationVelocity] [bit] NOT NULL,
		[EnableRedirectionCount] [bit] NOT NULL,
		[EnableRotationForVisitorRoles] [bit] NOT NULL,
		[DisplayCount] [int] NOT NULL,
		[VisitorCount] [int] NOT NULL,
		[RedirectionCount] [int] NOT NULL,
		[MaxVisitorCount] [int] NOT NULL,
		[MaxDisplayCount] [int] NOT NULL,
		[MaxDisplayCountPerVisitor] [int] NOT NULL,
		[MaxRedirectionCount] [int] NOT NULL,
		[DateOfRotationStart] [datetime] NULL,
		[DateOfRotationFinish] [datetime] NULL,
		[DateOfFirstDisplay] [datetime] NULL,
		[DateOfLastDisplay] [datetime] NULL,
		[Name] [nvarchar](256) NOT NULL,
		[DateCreated] [datetime] NOT NULL,
		[DateLastModified] [datetime] NOT NULL,
		[AuthorId] [int] NOT NULL,
		[LastModificationAuthorId] [int] NOT NULL,
		[ContentType] [tinyint] NOT NULL,
		[ContentFileId] [int] NULL,
		[LinkUrl] [nvarchar](2048) NULL,
		[LinkTarget] [nvarchar](64) NULL,
		[ToolTip] [nvarchar](512) NULL,
		[FlashWMode] [nvarchar](128) NULL,
		[FlashDynamicCreation] [bit] NULL,
		[FlashVersion] [nvarchar](128) NULL,
		[FlashAltImageFileId] [int] NULL,
		[FlashUseCustomUrl] [bit] NULL,
		[TextContentType] [tinyint] NULL,
		[TextContent] [nvarchar](4000) NULL,
		[NotifyStatusChange] [bit] NOT NULL,
		[RequiredPathList] [nvarchar](4000) NULL,
		[IgnoredPathList] [nvarchar](4000) NULL,
		[Description] [nvarchar](2048) NULL,
		[XmlId] [varchar](256) NULL,
		CONSTRAINT [PK_b_AdvBanner] PRIMARY KEY CLUSTERED([ID] ASC),
		CONSTRAINT [FK_b_AdvBanner_b_AdvSpace] FOREIGN KEY([SpaceId]) REFERENCES [b_AdvSpace] ([ID]),
		CONSTRAINT [FK_b_AdvBanner_b_File] FOREIGN KEY([ContentFileId]) REFERENCES [b_File] ([ID]),
		CONSTRAINT [FK_b_AdvBanner_b_Author] FOREIGN KEY([AuthorId]) REFERENCES [b_Users] ([ID]),
		CONSTRAINT [FK_b_AdvBanner_b_LMAuthor] FOREIGN KEY([LastModificationAuthorId]) REFERENCES [b_Users] ([ID])							 
	)
END
GO

IF OBJECT_ID (N'b_AdvBannerInRole', N'U') IS NULL
BEGIN
	CREATE TABLE [b_AdvBannerInRole](
		[BannerId] [int] NOT NULL,
		[RoleId] [int] NOT NULL,
		CONSTRAINT [PK_b_AdvBannerInRole] PRIMARY KEY CLUSTERED([BannerId] ASC, [RoleId] ASC),
		CONSTRAINT [FK_b_AdvBannerInRole_b_AdvBanner] FOREIGN KEY([BannerId]) REFERENCES [b_AdvBanner] ([ID]),
		CONSTRAINT [FK_b_AdvBannerInRole_b_Roles] FOREIGN KEY([RoleId]) REFERENCES [b_Roles] ([ID])		
		)
END
GO

IF OBJECT_ID (N'b_AdvBannerInSite', N'U') IS NULL
BEGIN
	CREATE TABLE [b_AdvBannerInSite](
		[BannerId] [int] NOT NULL,
		[SiteId] [varchar](50) NOT NULL,
		CONSTRAINT [PK_b_AdvBannerInSite] PRIMARY KEY CLUSTERED([BannerId] ASC, [SiteId] ASC),
		CONSTRAINT [FK_b_AdvBannerInSite_b_AdvBanner] FOREIGN KEY([BannerId]) REFERENCES [b_AdvBanner] ([ID]),
		CONSTRAINT [FK_b_AdvBannerInSite_b_Site] FOREIGN KEY([SiteId]) REFERENCES [b_Site] ([ID])		
	)
END
GO

IF OBJECT_ID(N'b_AdvBannerUrlTemplate', N'U') IS NULL
BEGIN
	CREATE TABLE [b_AdvBannerUrlTemplate](
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[BannerId] [int] NOT NULL,
		[IsPermitted] [bit] NOT NULL,
		[UrlTemplate] [nvarchar](2048) NOT NULL,
		CONSTRAINT [PK_b_AdvBannerUrlTemplate] PRIMARY KEY CLUSTERED([ID] ASC),
		CONSTRAINT [FK_b_AdvBannerUrlTemplate_b_AdvBanner] FOREIGN KEY([BannerId]) REFERENCES [b_AdvBanner] ([ID])		
	)
END
GO

IF OBJECT_ID(N'b_AdvBannerWeekScheduleHourSpan', N'U') IS NULL
BEGIN
	CREATE TABLE [b_AdvBannerWeekScheduleHourSpan](
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[BannerId] [int] NOT NULL,
		[FromHourOfWeek] [tinyint] NOT NULL,
		[TillHourOfWeek] [tinyint] NOT NULL,
		CONSTRAINT [PK_b_AdvBannerRotationPeriod] PRIMARY KEY CLUSTERED([ID] ASC),
		CONSTRAINT [FK_b_AdvBannerWeekScheduleHourSpan_b_AdvBanner] FOREIGN KEY([BannerId]) REFERENCES [b_AdvBanner] ([ID])		
	)
END
GO

IF OBJECT_ID (N'Adv_GetBanners', N'P') IS NOT NULL
	DROP PROCEDURE [Adv_GetBanners]
GO

CREATE PROCEDURE [Adv_GetBanners]
	@UserId int,
	@SiteId varchar(50),
	@Url nvarchar(2048),
	@DisplayedItemsXml xml,
	@DateTime datetime,
	@HourOfWeek tinyint
AS
BEGIN
	SET NOCOUNT ON;
	SELECT 
		b.ID, 
		b.SpaceId,
		s.Code [SpaceCode],
		b.[Weight], 
		b.EnableUniformRotationVelocity, 
		b.DateOfRotationStart,
		b.DateOfRotationFinish,
		b.DateOfFirstDisplay,
		b.DisplayCount,
		b.MaxDisplayCount,
		(SELECT hsp.BannerId [@BannerId], hsp.FromHourOfWeek [@FromHourOfWeek], hsp.TillHourOfWeek [@TillHourOfWeek] FROM b_AdvBannerWeekScheduleHourSpan hsp WHERE hsp.BannerId = b.ID FOR XML PATH ('RotationHourSpan')) [RotationHourSpanXml]
		FROM [b_AdvSpace] s
		INNER JOIN [b_AdvBanner] b 
			ON b.SpaceId = s.ID 
				AND b.Active = 1
				AND(b.DateOfRotationStart IS NULL OR b.DateOfRotationStart <= @DateTime)
				AND(b.DateOfRotationFinish IS NULL OR b.DateOfRotationFinish > @DateTime)
				AND(
					b.EnableRedirectionCount = 0 
					OR b.MaxRedirectionCount = 0
					OR b.MaxRedirectionCount > b.RedirectionCount 
				)				
				AND(
					b.EnableFixedRotation = 0
					OR(
						(b.MaxDisplayCount = 0 OR b.MaxDisplayCount > b.DisplayCount)
						AND(b.MaxVisitorCount = 0 OR b.MaxVisitorCount > b.VisitorCount)
						AND( --MaxDisplayCountPerVisitor
							b.MaxDisplayCountPerVisitor = 0
							OR NOT EXISTS(
								SELECT TOP 1 N'x' 
									FROM @DisplayedItemsXml.nodes('/item') t(c) 
									WHERE t.c.value('@id', 'int') = b.ID
										AND t.c.value('@count', 'int') >= b.MaxDisplayCountPerVisitor
							)
						) 						
					)
				)				
		WHERE
			(
				NOT EXISTS(
					SELECT TOP 1 N'x'
						FROM [b_AdvBannerInSite]
						WHERE BannerId = b.ID
				)
				OR EXISTS(
					SELECT TOP 1 N'x'
						FROM [b_AdvBannerInSite]
						WHERE BannerId = b.ID AND SiteId = @SiteId					
				)
			)
			AND EXISTS(
				SELECT TOP 1 N'x' 
					FROM [b_AdvBannerWeekScheduleHourSpan] wsh 
					WHERE wsh.BannerId = b.ID
						AND wsh.FromHourOfWeek <= @HourOfWeek
						AND wsh.TillHourOfWeek > @HourOfWeek
			)
			AND(--UserRoles
					(
						b.EnableRotationForVisitorRoles = 1
						AND EXISTS(
							SELECT TOP 1 N'x' 
								FROM [b_AdvBannerInRole] b2r
								INNER JOIN [b_UsersInRoles] u2r 
									ON u2r.RoleID = b2r.RoleId
										AND u2r.UserID = @UserId
										AND (u2r.ActiveFrom IS NULL OR u2r.ActiveFrom <= @DateTime)
										AND (u2r.ActiveTo IS NULL OR u2r.ActiveTo > @DateTime)
								WHERE b2r.BannerId = b.ID						
						)
					)
					OR(
						b.EnableRotationForVisitorRoles = 0
						AND NOT EXISTS(
							SELECT TOP 1 N'x' 
								FROM [b_AdvBannerInRole] b2r
								INNER JOIN [b_UsersInRoles] u2r 
									ON u2r.RoleID = b2r.RoleId
										AND u2r.UserID = @UserId
										AND (u2r.ActiveFrom IS NULL OR u2r.ActiveFrom <= @DateTime)
										AND (u2r.ActiveTo IS NULL OR u2r.ActiveTo > @DateTime)
								WHERE b2r.BannerId = b.ID						
						)					
					)				
			)
			AND(--WhiteList&BlackList
				NOT EXISTS(--BlackList
					SELECT TOP 1 N'x' 
					FROM [b_AdvBannerUrlTemplate] ut
					WHERE ut.BannerId = b.ID 
						AND ut.IsPermitted = 0
						AND @Url LIKE ut.UrlTemplate + N'%'
				)
				AND(--WhiteList
					NOT EXISTS(
						SELECT TOP 1 N'x' 
						FROM [b_AdvBannerUrlTemplate] ut
						WHERE ut.BannerId = b.ID 
							AND ut.IsPermitted = 1			
					)
					OR EXISTS(
						SELECT TOP 1 N'x' 
						FROM [b_AdvBannerUrlTemplate] ut
						WHERE ut.BannerId = b.ID 
							AND ut.IsPermitted = 1
							AND @Url LIKE ut.UrlTemplate + N'%'					
					)
				)
			)
	ORDER BY b.SpaceId ASC, b.EnableUniformRotationVelocity DESC, b.[Weight] DESC	
END

GO

IF OBJECT_ID (N'Adv_ProcessClientRedirection', N'P') IS NOT NULL
	DROP PROCEDURE [Adv_ProcessClientRedirection]
GO

CREATE PROCEDURE [Adv_ProcessClientRedirection]
	@BannerId int
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE [b_AdvBanner] 
	SET [RedirectionCount] = [RedirectionCount] + 1
	WHERE [ID] = @BannerId AND [Active] = 1 AND [EnableRedirectionCount] = 1;
	
	IF @@ROWCOUNT > 0
		SELECT [LinkUrl] FROM [b_AdvBanner]  WHERE [ID] = @BannerId AND [Active] = 1 AND [EnableRedirectionCount] = 1;
END

GO


IF OBJECT_ID (N'Adv_ProcessDisplay', N'P') IS NOT NULL
	DROP PROCEDURE [Adv_ProcessDisplay]
GO

CREATE PROCEDURE [Adv_ProcessDisplay] 
	@DisplayedItemsXml xml, 
	@Datetime datetime
AS
BEGIN
	SET NOCOUNT ON;
	WITH di AS
		(
			SELECT 
				t.c.value('@id', 'int') [ID], 
				t.c.value('@incrementedTotalCount', 'int') [IncrementedTotalCount], 
				t.c.value('if((@isNew cast as xs:byte?) = xs:byte(1)) then xs:integer(1) else xs:integer(0)', 'int') [InctementedVisitorCount]  
			FROM @DisplayedItemsXml.nodes('/item') t(c)	
		)
	UPDATE [b_AdvBanner] 
	SET 
		[DisplayCount] = [DisplayCount] + di.[IncrementedTotalCount], 
		[VisitorCount] = [VisitorCount] + di.[InctementedVisitorCount],
		[DateOfLastDisplay] = @Datetime,
		[DateOfFirstDisplay] = CASE WHEN [DateOfFirstDisplay] IS NULL THEN @Datetime ELSE [DateOfFirstDisplay] END
	FROM di INNER JOIN [b_AdvBanner] b ON di.ID = b.ID	
END

GO