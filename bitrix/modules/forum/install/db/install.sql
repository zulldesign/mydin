IF OBJECT_ID (N'b_ForumCategory', N'U') IS NULL
	CREATE TABLE b_ForumCategory
	(
		ID INT IDENTITY(1,1) NOT NULL,
		Name NVARCHAR(255) NOT NULL,
		Sort INT NOT NULL CONSTRAINT DF_b_ForumCategory_Sort  DEFAULT ((10)),
		XmlId NVARCHAR(255) NULL,
		CONSTRAINT PK_b_ForumCategory PRIMARY KEY (ID ASC)
	)
GO

IF OBJECT_ID (N'b_Forum', N'U') IS NULL
	CREATE TABLE b_Forum
	(
		ID INT IDENTITY(1,1) NOT NULL,
		Active BIT NOT NULL CONSTRAINT DF_b_Forum_Active  DEFAULT ((1)),
		Name NVARCHAR(255) NOT NULL,
		DateCreate DATETIME NOT NULL CONSTRAINT DF_b_Forum_DateCreate  DEFAULT (getdate()),
		Description NVARCHAR(255) NULL,
		CategoryId INT NOT NULL CONSTRAINT DF_b_Forum_CategoryId  DEFAULT ((0)),
		Sort INT NOT NULL CONSTRAINT DF_b_Forum_Sort  DEFAULT ((10)),
		Topics INT NOT NULL CONSTRAINT DF_b_Forum_Topics  DEFAULT ((0)),
		QueuedTopics INT NOT NULL CONSTRAINT DF_b_Forum_QueuedTopics  DEFAULT ((0)),
		Replies INT NOT NULL CONSTRAINT DF_b_Forum_Posts  DEFAULT ((0)),
		QueuedReplies INT NOT NULL CONSTRAINT DF_b_Forum_QueuedPosts  DEFAULT ((0)),
		Code NVARCHAR(255) NULL,
		XmlId VARCHAR(255) NULL,
		AllowBBCode BIT NOT NULL CONSTRAINT DF_b_Forum_AllowBBCode  DEFAULT ((1)),
		AllowSmiles BIT NOT NULL CONSTRAINT DF_b_Forum_AllowSmiles  DEFAULT ((1)),
		AllowVotingForTopic BIT NOT NULL CONSTRAINT DF_b_Forum_AllowVotingForTopic DEFAULT (1),
		AllowVotingForPost BIT NOT NULL CONSTRAINT DF_b_Forum_AllowVotingForPost DEFAULT (1),	
		LastPostId BIGINT NOT NULL CONSTRAINT DF_b_Forum_LastPostId  DEFAULT ((0)),
		LastPostDate DATETIME NULL,
		LastPosterId INT NOT NULL CONSTRAINT DF_b_Forum_LastPosterId  DEFAULT ((0)),
		LastPosterName NVARCHAR(256) NULL,
		IndexContent BIT NOT NULL CONSTRAINT DF_b_Forum_IndexContent  DEFAULT ((1)),
		CONSTRAINT PK_b_Forum PRIMARY KEY (ID ASC)
	)

GO

IF OBJECT_ID (N'b_ForumInSite', N'U') IS NULL
	CREATE TABLE b_ForumInSite
	(
		ForumId INT NOT NULL,
		SiteId VARCHAR(50) NOT NULL,
		CONSTRAINT PK_b_ForumInSite PRIMARY KEY (ForumId ASC, SiteId ASC),
		CONSTRAINT FK_ForumInSite_ForumId_Forum_Id FOREIGN KEY(ForumId) REFERENCES b_Forum (ID),
		CONSTRAINT FK_ForumInSite_SiteId_Site_Id FOREIGN KEY(SiteId) REFERENCES b_Site (ID)
	)
GO

IF OBJECT_ID (N'b_ForumUser', N'U') IS NULL
	CREATE TABLE b_ForumUser
	(
		ID INT NOT NULL,
		Posts INT NOT NULL CONSTRAINT DF_b_ForumUser_Posts  DEFAULT ((0)),
		Signature NVARCHAR(400) NULL,
		OwnPostNotification BIT NOT NULL CONSTRAINT DF_b_ForumUser_OwnPostNotification  DEFAULT ((0)),
		CONSTRAINT PK_b_ForumUser PRIMARY KEY (ID ASC)
	)
GO

IF OBJECT_ID (N'b_ForumTopic', N'U') IS NULL
	CREATE TABLE b_ForumTopic
	(
		ID INT IDENTITY(1,1) NOT NULL,
		ForumId INT NOT NULL,
		Name NVARCHAR(255) NOT NULL,
		Description NVARCHAR(255) NULL,
		DateCreate DATETIME NOT NULL CONSTRAINT DF_b_ForumTopic_DateCreate  DEFAULT (getdate()),
		AuthorId INT NOT NULL CONSTRAINT DF_b_ForumTopic_StarterId  DEFAULT ((0)),
		AuthorName NVARCHAR(256) NOT NULL,
		Closed BIT NOT NULL CONSTRAINT DF_b_ForumTopic_Open  DEFAULT ((0)),
		Approved BIT NOT NULL CONSTRAINT DF_b_ForumTopic_Approved  DEFAULT ((1)),
		StickyIndex INT NOT NULL CONSTRAINT DF_b_ForumTopic_StickyIndex  DEFAULT ((0)),
		Replies INT NOT NULL CONSTRAINT DF_b_ForumTopic_Posts  DEFAULT ((0)),
		QueuedReplies INT NOT NULL CONSTRAINT DF_b_ForumTopic_QueuedPosts  DEFAULT ((0)),
		Views bigINT NOT NULL CONSTRAINT DF_b_ForumTopic_Views  DEFAULT ((0)),
		MovedTo INT NOT NULL CONSTRAINT DF_b_ForumTopic_MovedTo  DEFAULT ((0)),
		FirstPostId bigINT NOT NULL CONSTRAINT DF_b_ForumTopic_FirstPostId  DEFAULT ((0)),
		LastPostId bigINT NOT NULL CONSTRAINT DF_b_ForumTopic_LastPostId  DEFAULT ((0)),
		LastPostDate DATETIME NULL,
		LastPosterId INT NOT NULL CONSTRAINT DF_b_ForumTopic_LastPosterId  DEFAULT ((0)),
		LastPosterName NVARCHAR(256) NULL,
		XmlId VARCHAR(255) NULL,
		CONSTRAINT PK_b_ForumTopic PRIMARY KEY (ID ASC),
		CONSTRAINT FK_ForumTopic_ForumId_Forum_Id FOREIGN KEY(ForumId) REFERENCES b_Forum (ID)
	)

GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_ForumTopic_TopicList')
	CREATE INDEX IX_b_ForumTopic_TopicList ON b_ForumTopic ( ForumId ASC, Approved ASC, StickyIndex ASC, LastPostDate ASC, ID ASC)
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_ForumTopic_TopicCount')
	CREATE INDEX IX_b_ForumTopic_TopicCount ON b_ForumTopic ( ForumId ASC, Approved ASC, MovedTo ASC)
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_ForumTopic_ForumId')
	CREATE INDEX IX_b_ForumTopic_ForumId ON b_ForumTopic ( ForumId ASC )
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_ForumTopic_UserPosts')
	CREATE INDEX IX_b_ForumTopic_UserPosts ON b_ForumTopic ( AuthorId ASC, Approved ASC, ForumId ASC, DateCreate ASC )
GO

IF OBJECT_ID (N'b_ForumPost', N'U') IS NULL
	CREATE TABLE b_ForumPost
	(
		ID BIGINT IDENTITY(1,1) NOT NULL,
		TopicId INT NOT NULL,
		ForumId INT NOT NULL,
		AuthorId INT NOT NULL CONSTRAINT DF_b_ForumPost_AuthorId  DEFAULT ((0)),
		ParentPostId bigINT NOT NULL CONSTRAINT DF_b_ForumPost_ParentPostId  DEFAULT ((0)),
		AuthorName NVARCHAR(256) NOT NULL,
		AuthorEmail NVARCHAR(256) NULL,
		AuthorIp VARCHAR(255) NULL,
		Post NVARCHAR(max) NOT NULL,
		Approved BIT NOT NULL CONSTRAINT DF_b_ForumPost_Approved  DEFAULT ((1)),
		DateCreate DATETIME NOT NULL CONSTRAINT DF_b_ForumPost_DateCreate  DEFAULT (getdate()),
		DateUpdate DATETIME NOT NULL CONSTRAINT DF_b_ForumPost_DateUpdate  DEFAULT (getdate()),
		AllowSmiles BIT NOT NULL CONSTRAINT DF_b_ForumPost_AllowSmiles  DEFAULT ((1)),
		XmlId VARCHAR(255) NULL,
		CONSTRAINT PK_b_ForumPost PRIMARY KEY (ID ASC),
		CONSTRAINT FK_ForumPost_ForumId_Forum_Id FOREIGN KEY(ForumId) REFERENCES b_Forum(ID),
		CONSTRAINT FK_ForumPost_TopicId_ForumTopic_Id FOREIGN KEY(TopicId) REFERENCES b_ForumTopic(ID)
	)

GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_ForumPost_PostList')
	CREATE INDEX IX_b_ForumPost_PostList ON b_ForumPost ( TopicId ASC, ForumId ASC, Approved ASC, ID ASC, AuthorId ASC)
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_ForumPost_TopicId')
	CREATE INDEX IX_b_ForumPost_TopicId ON b_ForumPost ( TopicId ASC )
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_ForumPost_ForumId')
	CREATE INDEX IX_b_ForumPost_ForumId ON b_ForumPost ( ForumId ASC )
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_ForumPost_UserPosts')
	CREATE INDEX IX_b_ForumPost_UserPosts ON b_ForumPost (AuthorId ASC, Approved ASC, ForumId ASC, DateCreate ASC) INCLUDE (ID, TopicId)
GO

IF OBJECT_ID (N'b_ForumSubscription', N'U') IS NULL
	CREATE TABLE b_ForumSubscription
	(
		ID INT IDENTITY(1,1) NOT NULL,
		TopicId INT NOT NULL CONSTRAINT DF_b_ForumSubscription_TopicId  DEFAULT ((0)),
		ForumId INT NOT NULL,
		SubscriberId INT NOT NULL,
		OnlyTopic BIT NOT NULL CONSTRAINT DF_b_ForumSubscription_OnlyTopic  DEFAULT ((0)),
		DateStart DATETIME NOT NULL CONSTRAINT DF_b_ForumSubscription_DateStart  DEFAULT (getdate()),
		SiteId varchar(50) NOT NULL,
		CONSTRAINT PK_b_ForumSubscription PRIMARY KEY (ID ASC),
		CONSTRAINT FK_ForumSubscription_ForumId_Forum_Id FOREIGN KEY(ForumId) REFERENCES b_Forum(ID)
	)
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_ForumSubscription_SubscriberId')
	CREATE INDEX IX_b_ForumSubscription_SubscriberId ON b_ForumSubscription (SubscriberId ASC) INCLUDE (TopicId, ForumId, OnlyTopic)
GO

IF OBJECT_ID (N'Forum_SynchronizeForum', N'P') IS NOT NULL
	DROP PROCEDURE Forum_SynchronizeForum
GO

CREATE PROCEDURE Forum_SynchronizeForum
	@ForumId int 
AS
BEGIN
	DECLARE @AllTopics int, @ApprovedTopics int, @Replies int, @QueuedReplies int

	SELECT
		@AllTopics = COUNT(TOPIC.ID), 
		@ApprovedTopics = SUM(CASE TOPIC.Approved WHEN 1 THEN 1 ELSE 0 END), 
		@Replies = SUM(CASE TOPIC.Approved WHEN 1 THEN TOPIC.Replies ELSE 0 END),
		@QueuedReplies = SUM(TOPIC.QueuedReplies)
	FROM 
		b_ForumTopic TOPIC
	WHERE 
		TOPIC.ForumId = @ForumId AND TOPIC.MovedTo = 0

	IF @ApprovedTopics IS NULL SET @ApprovedTopics = 0
	IF @Replies IS NULL SET @Replies = 0
	IF @QueuedReplies IS NULL SET @QueuedReplies = 0	
	
	DECLARE @LastPostDate datetime, @LastPostId int, @LastPosterId int, @LastPosterName nvarchar(256)

	SELECT TOP 1
		@LastPostDate = TOPIC.LastPostDate,
		@LastPostId = TOPIC.LastPostId, 
		@LastPosterId = TOPIC.LastPosterId, 
		@LastPosterName = TOPIC.LastPosterName
	FROM
		b_ForumTopic TOPIC
	WHERE
		TOPIC.ForumId = @ForumId AND TOPIC.MovedTo = 0 AND TOPIC.Approved = 1
	ORDER BY
		TOPIC.LastPostDate DESC

	IF @LastPostId IS NULL SET @LastPostId = 0
	IF @LastPosterId IS NULL SET @LastPosterId = 0	

	UPDATE 
		b_Forum
	SET
		Replies = @Replies,
		QueuedReplies = @QueuedReplies,
		Topics = @ApprovedTopics,
		QueuedTopics = @AllTopics - @ApprovedTopics,
		LastPostDate = @LastPostDate,
		LastPostId = @LastPostId, 
		LastPosterId = @LastPosterId, 
		LastPosterName = @LastPosterName
	WHERE
		ID = @ForumId
END
GO

IF OBJECT_ID (N'Forum_SynchronizeTopic', N'P') IS NOT NULL
	DROP PROCEDURE Forum_SynchronizeTopic
GO

CREATE PROCEDURE Forum_SynchronizeTopic 
	@TopicId int = 0
AS
BEGIN
	
	DECLARE @FirstPostId bigint
	SET @FirstPostId = 0
	
	SELECT
		@FirstPostId = FirstPostId
	FROM 
		b_ForumTopic
	WHERE 
		ID = @TopicId
	
	IF @@ROWCOUNT = 0
		RETURN
	
	DECLARE @ApprovedPosts int, @QueuedPosts int, @LastPostDate datetime, @LastPostId int, @LastPosterId int, @LastPosterName nvarchar(256)

	SELECT 
		@ApprovedPosts = SUM(CASE POST.Approved WHEN 1 THEN 1 ELSE 0 END),
		@QueuedPosts = SUM(CASE POST.Approved WHEN 0 THEN 1 ELSE 0 END)
	FROM 
		b_ForumPost POST
	WHERE 
		POST.TopicId = @TopicId AND POST.ID <> @FirstPostId  

	SELECT TOP 1
		@LastPostDate = POST.DateCreate,
		@LastPostId = POST.ID, 
		@LastPosterId = POST.AuthorId, 
		@LastPosterName = POST.AuthorName
	FROM
		b_ForumPost POST
	WHERE
		POST.TopicId = @TopicId AND POST.Approved = 1
	ORDER BY
		POST.ID DESC
	
	IF @ApprovedPosts IS NULL SET @ApprovedPosts = 0
	IF @QueuedPosts IS NULL SET @QueuedPosts = 0	
	IF @LastPostId IS NULL SET @LastPostId = 0
	IF @LastPosterId IS NULL SET @LastPosterId = 0	

	UPDATE 
		b_ForumTopic
	SET
		Replies = @ApprovedPosts,
		QueuedReplies = @QueuedPosts,
		LastPostDate = @LastPostDate,
		LastPostId = @LastPostId, 
		LastPosterId = @LastPosterId, 
		LastPosterName = @LastPosterName
	WHERE
		ID = @TopicId
END

GO


IF OBJECT_ID (N'Forum_SynchronizeUser', N'P') IS NOT NULL
	DROP PROCEDURE Forum_SynchronizeUser
GO

CREATE PROCEDURE Forum_SynchronizeUser
	@UsersId varchar(2000)
AS
BEGIN

	DECLARE @TranStarted bit
	SET @TranStarted = 0

	IF (@@TRANCOUNT = 0)
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END

	DECLARE @Pos int, @NextPos int, @Id nvarchar(15)
	DECLARE @Posts int
	SET @Pos = 1
	
	WHILE (@Pos <= LEN(@UsersId))
	BEGIN
		SELECT @NextPos = CHARINDEX(N',', @UsersId,  @Pos)
		IF (@NextPos = 0 OR @NextPos IS NULL)
			SELECT @NextPos = LEN(@UsersId) + 1
			
		SELECT @Id = RTRIM(LTRIM(SUBSTRING(@UsersId, @Pos, @NextPos - @Pos)))
		
		SELECT	@Posts = COUNT(ID)
		FROM	b_ForumPost Post
		WHERE	Post.AuthorId = @Id AND 
			Post.Approved = 1 AND 
			Post.ForumId IN (SELECT ID FROM b_Forum WHERE Active = 1)
		
		UPDATE b_ForumUser SET Posts = @Posts WHERE ID = @Id
		IF @@ROWCOUNT = 0 AND NOT EXISTS(SELECT ID FROM b_ForumUser WHERE ID = @Id)
			INSERT INTO b_ForumUser (ID, Posts) VALUES(@Id, @Posts)
		
		SET @Pos = @NextPos + 1
	END

	IF (@TranStarted = 1)
		COMMIT TRANSACTION

	RETURN(0)
	
END
GO

IF OBJECT_ID(N'ForumTopic_IncrementViews', N'P') IS NOT NULL
	DROP PROCEDURE ForumTopic_IncrementViews;
GO
CREATE PROCEDURE ForumTopic_IncrementViews 
	@TopicId int
AS
BEGIN
	SET NOCOUNT ON;
	IF @TopicId IS NULL OR @TopicId <= 0 RETURN;
	UPDATE b_ForumTopic SET [Views] = [Views] + 1 WHERE ID = @TopicId;
END
GO

/*--- BEGINNING OF RATINGS   ---*/
IF OBJECT_ID (N'b_ForumActivity', N'U') IS NULL
BEGIN
	CREATE TABLE b_ForumActivity
	(
		ID INT IDENTITY(1,1) NOT NULL,
		RatingId INT NOT NULL,
		Active BIT NOT NULL,
		UserId INT NOT NULL,
		CreatedUtc DATETIME NOT NULL,
		LastCalculatedUtc DATETIME NOT NULL,
		IsCalculated BIT NOT NULL,
		Value FLOAT(53) NOT NULL,	
		TopicValue FLOAT(53) NOT NULL,
		PostValue FLOAT(53) NOT NULL,
		TodayTopicCoef FLOAT(53) NOT NULL,		
		WeekTopicCoef FLOAT(53) NOT NULL,		
		MonthTopicCoef FLOAT(53) NOT NULL,			
		TodayPostCoef FLOAT(53) NOT NULL,		
		WeekPostCoef FLOAT(53) NOT NULL,		
		MonthPostCoef FLOAT(53) NOT NULL,	
		XmlId VARCHAR(256) NULL,
		CONSTRAINT PK_b_ForumActivity PRIMARY KEY CLUSTERED(ID ASC)
	)
END
GO

IF OBJECT_ID (N'ForumActivity_GetTopicCountInRange', N'FN') IS NOT NULL
	DROP FUNCTION dbo.ForumActivity_GetTopicCountInRange
GO

CREATE FUNCTION dbo.ForumActivity_GetTopicCountInRange
(
	@UserId INT,
	@Now DATETIME,
	@Days INT
)
RETURNS INT
AS
BEGIN
	DECLARE @Datetime DATETIME;
	IF(@Now IS NULL) SET @Now = GETDATE();
	IF(@Days IS NULL OR @Days = 0)
		SET @Datetime = DATEADD(DAY, 0, DATEDIFF(DAY, 0, @Now));
	ELSE
		SET @Datetime = DATEADD(DAY, -1 * @Days, @Now);
		
	DECLARE @Result INT;
	SET @Result = (SELECT COUNT(ID) 
		FROM b_ForumTopic 
		WHERE Approved = 1 AND DateCreate >= @Datetime AND DateCreate <= @Now AND AuthorId = @UserId
		);		
	RETURN @Result;
END
GO

IF OBJECT_ID (N'ForumActivity_GetPostCountInRange', N'FN') IS NOT NULL
	DROP FUNCTION dbo.ForumActivity_GetPostCountInRange
GO

CREATE FUNCTION dbo.ForumActivity_GetPostCountInRange
(
	@UserId INT,
	@Now DATETIME,
	@Days INT
)
RETURNS INT
AS
BEGIN
	DECLARE @Datetime DATETIME;
	IF(@Now IS NULL) SET @Now = GETDATE();
	IF(@Days IS NULL OR @Days = 0)
		SET @Datetime = DATEADD(DAY, 0, DATEDIFF(DAY, 0, @Now));
	ELSE
		SET @Datetime = DATEADD(DAY, -1 * @Days, @Now);
		
	DECLARE @Result INT;
	SET @Result = (SELECT COUNT(*) 
		FROM b_ForumPost
		WHERE Approved = 1 AND DateCreate >= @Datetime AND DateCreate <= @Now AND AuthorId = @UserId
		);	
	RETURN @Result;
END
GO

IF OBJECT_ID (N'ForumActivity_GetTopicValue', N'FN') IS NOT NULL
	DROP FUNCTION dbo.ForumActivity_GetTopicValue
GO
CREATE FUNCTION dbo.ForumActivity_GetTopicValue
(
	@UserId INT,
	@Now DATETIME,
	@MonthCoef FLOAT,
	@WeekCoef FLOAT,
	@TodayCoef FLOAT
)
RETURNS FLOAT(53)
AS
BEGIN
	IF(@Now IS NULL) SET @Now = GETDATE();	
	DECLARE @Result FLOAT(53);
	WITH topic30(ID, DateCreate) AS
	(
		SELECT ID, DateCreate
		FROM b_ForumTopic 
		WHERE Approved = 1 AND DateCreate >= DATEADD(DAY, -30, @Now) AND DateCreate <= @Now AND AuthorId = @UserId
	)
	SELECT @Result = (SELECT COUNT(ID) FROM topic30) * @MonthCoef +
		(SELECT COUNT(ID) FROM topic30 WHERE DateCreate >= DATEADD(DAY, -7, @Now)) * @WeekCoef + 
		(SELECT COUNT(ID) FROM topic30 WHERE DateCreate >= DATEADD(DAY, 0, DATEDIFF(DAY, 0, @Now))) * @TodayCoef;
		
	RETURN @Result;
END
GO

IF OBJECT_ID (N'ForumActivity_GetPostValue', N'FN') IS NOT NULL
	DROP FUNCTION dbo.ForumActivity_GetPostValue
GO
CREATE FUNCTION dbo.ForumActivity_GetPostValue
(
	@UserId INT,
	@Now DATETIME,
	@MonthCoef FLOAT,
	@WeekCoef FLOAT,
	@TodayCoef FLOAT
)
RETURNS FLOAT(53)
AS
BEGIN
	IF(@Now IS NULL) SET @Now = GETDATE();	
	DECLARE @Result FLOAT(53);
	WITH post30(ID, DateCreate) AS
	(
		SELECT ID, DateCreate
		FROM b_ForumPost
		WHERE Approved = 1 AND DateCreate >= DATEADD(DAY, -30, @Now) AND DateCreate <= @Now AND AuthorId = @UserId	
	)
	SELECT @Result = (SELECT COUNT(ID) FROM post30) * @MonthCoef +
		(SELECT COUNT(ID) FROM post30 WHERE DateCreate >= DATEADD(DAY, -7, @Now)) * @WeekCoef + 
		(SELECT COUNT(ID) FROM post30 WHERE DateCreate >= DATEADD(DAY, 0, DATEDIFF(DAY, 0, @Now))) * @TodayCoef;
		
	RETURN @Result;	
END
GO

IF OBJECT_ID (N'ForumActivity_Synchronize', N'P') IS NOT NULL
	DROP PROCEDURE ForumActivity_Synchronize;
GO

CREATE PROCEDURE ForumActivity_Synchronize
	@ForumActivityId INT,
	@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @TopicResult FLOAT(53), @PostResult FLOAT(53);
		
	BEGIN TRANSACTION;
	SELECT @TopicResult = dbo.ForumActivity_GetTopicValue(UserId, @Now, MonthTopicCoef, WeekTopicCoef, TodayTopicCoef),
		@PostResult = dbo.ForumActivity_GetPostValue(UserId, @Now, MonthPostCoef, WeekPostCoef, TodayPostCoef)  
	FROM b_ForumActivity 
	WHERE ID = @ForumActivityId;			
			
	UPDATE b_ForumActivity 
	SET Value = @TopicResult + @PostResult, 
		TopicValue = @TopicResult,
		PostValue = @PostResult, 
		LastCalculatedUtc = GETUTCDATE(),
		IsCalculated = 1 
	WHERE ID = @ForumActivityId;
	COMMIT TRANSACTION;
		
	SELECT @TopicResult + @PostResult;
END
GO

IF OBJECT_ID (N'ForumActivity_SynchronizePostByUser', N'P') IS NOT NULL
	DROP PROCEDURE ForumActivity_SynchronizePostByUser;
GO

CREATE PROCEDURE ForumActivity_SynchronizePostByUser
	@UserId INT,
	@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	
	IF(@UserId IS NULL OR NOT EXISTS(SELECT N'*' FROM b_ForumActivity WHERE UserId = @UserId AND Active = 1)) RETURN;
	IF(@Now IS NULL) SET @Now = GETDATE();
	
	DECLARE @monthBeforeCount INT, @weekBeforeCount INT, @todayCount INT;
	BEGIN TRANSACTION;
	SET @monthBeforeCount = dbo.ForumActivity_GetPostCountInRange(@UserId, @Now, 30);
	SET @weekBeforeCount = dbo.ForumActivity_GetPostCountInRange(@UserId, @Now, 7);
	SET @todayCount = dbo.ForumActivity_GetPostCountInRange(@UserId, @Now, 0);	
		
	UPDATE b_ForumActivity 
	SET 
		Value = Value - PostValue + (@monthBeforeCount * MonthPostCoef + @weekBeforeCount * WeekPostCoef + @todayCount * TodayPostCoef), 
		PostValue = (@monthBeforeCount * MonthPostCoef + @weekBeforeCount * WeekPostCoef + @todayCount * TodayPostCoef), 
		LastCalculatedUtc = GETUTCDATE()
	WHERE UserId = @UserId;

	UPDATE b_RatingCounter SET IsCalculated = 0 
	WHERE RatingId IN (SELECT DISTINCT RatingId FROM b_ForumActivity WHERE UserId = @UserId) 
		AND BoundEntityId = CAST(@UserId AS NVARCHAR(64));
	
	COMMIT TRANSACTION;
END
GO

IF OBJECT_ID (N'ForumActivity_SynchronizeTopicByUser', N'P') IS NOT NULL
	DROP PROCEDURE ForumActivity_SynchronizeTopicByUser;
GO

CREATE PROCEDURE ForumActivity_SynchronizeTopicByUser
	@UserId INT,
	@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	
	IF(@UserId IS NULL OR NOT EXISTS(SELECT N'*' FROM b_ForumActivity WHERE UserId = @UserId AND Active = 1)) RETURN;
	IF(@Now IS NULL) SET @Now = GETDATE();
	
	DECLARE @monthBeforeCount INT, @weekBeforeCount INT, @todayCount INT;
	BEGIN TRANSACTION;	
	SET @monthBeforeCount = dbo.ForumActivity_GetTopicCountInRange(@UserId, @Now, 30);
	SET @weekBeforeCount = dbo.ForumActivity_GetTopicCountInRange(@UserId, @Now, 7);
	SET @todayCount = dbo.ForumActivity_GetTopicCountInRange(@UserId, @Now, 0);
	
	UPDATE b_ForumActivity 
	SET 
		Value = Value - TopicValue + (@monthBeforeCount * MonthTopicCoef + @weekBeforeCount * WeekTopicCoef + @todayCount * TodayTopicCoef), 
		TopicValue = (@monthBeforeCount * MonthTopicCoef + @weekBeforeCount * WeekTopicCoef + @todayCount * TodayTopicCoef), 
		LastCalculatedUtc = GETUTCDATE() 
	WHERE UserId = @UserId;
	
	UPDATE b_RatingCounter SET IsCalculated = 0 
	WHERE RatingId IN (SELECT DISTINCT RatingId FROM b_ForumActivity WHERE UserId = @UserId) 
		AND BoundEntityId = CAST(@UserId AS NVARCHAR(64));	
	
	COMMIT TRANSACTION;
END
GO

IF OBJECT_ID (N'ForumActivity_SynchronizeByUser', N'P') IS NOT NULL
	DROP PROCEDURE ForumActivity_SynchronizeByUser;
GO

CREATE PROCEDURE ForumActivity_SynchronizeByUser
	@UserId INT,
	@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	
	IF(@UserId IS NULL OR NOT EXISTS(SELECT N'*' FROM b_ForumActivity WHERE UserId = @UserId AND Active = 1)) RETURN;
	IF(@Now IS NULL) SET @Now = GETDATE();
	
	DECLARE @monthBeforeTopicCount INT, @weekBeforeTopicCount INT, @todayTopicCount INT, @monthBeforePostCount INT, @weekBeforePostCount INT, @todayPostCount INT;
	BEGIN TRANSACTION;
	SET @monthBeforeTopicCount = dbo.ForumActivity_GetTopicCountInRange(@UserId, @Now, 30);
	SET @weekBeforeTopicCount = dbo.ForumActivity_GetTopicCountInRange(@UserId, @Now, 7);
	SET @todayTopicCount = dbo.ForumActivity_GetTopicCountInRange(@UserId, @Now, 0);
		
	SET @monthBeforePostCount = dbo.ForumActivity_GetPostCountInRange(@UserId, @Now, 30);
	SET @weekBeforePostCount = dbo.ForumActivity_GetPostCountInRange(@UserId, @Now, 7);
	SET @todayPostCount = dbo.ForumActivity_GetPostCountInRange(@UserId, @Now, 0);	
	
	UPDATE b_ForumActivity 
	SET 
		Value = (@monthBeforeTopicCount * MonthTopicCoef + @weekBeforeTopicCount * WeekTopicCoef + @todayTopicCount * TodayTopicCoef) + (@monthBeforePostCount * MonthPostCoef + @weekBeforePostCount * WeekPostCoef + @todayPostCount * TodayPostCoef), 
		TopicValue = (@monthBeforeTopicCount * MonthTopicCoef + @weekBeforeTopicCount * WeekTopicCoef + @todayTopicCount * TodayTopicCoef),		
		PostValue = (@monthBeforePostCount * MonthPostCoef + @weekBeforePostCount * WeekPostCoef + @todayPostCount * TodayPostCoef),
		LastCalculatedUtc = GETUTCDATE()
	WHERE UserId = @UserId;
	
	UPDATE b_RatingCounter SET IsCalculated = 0 
	WHERE RatingId IN (SELECT DISTINCT RatingId FROM b_ForumActivity WHERE UserId = @UserId) 
		AND BoundEntityId = CAST(@UserId AS NVARCHAR(64));	
	
	COMMIT TRANSACTION;
END
GO

IF OBJECT_ID (N'ForumActivity_Engage', N'P') IS NOT NULL
	DROP PROCEDURE ForumActivity_Engage;
GO

CREATE PROCEDURE ForumActivity_Engage
	@RatingId INT
AS
BEGIN
	SET NOCOUNT ON;	
	DECLARE @ConfigXml XML;
	SELECT @ConfigXml = ComponentConfigXml  FROM b_Rating WHERE ID = @RatingId AND Active = 1 AND ComponentConfigXml.exist(N'/forumActivityConfig') = 1;
	IF(@ConfigXml IS NULL) RETURN;
	
	DECLARE @TodayTopicCoef FLOAT(53), @WeekTopicCoef FLOAT(53), @MonthTopicCoef FLOAT(53), @TodayPostCoef FLOAT(53), @WeekPostCoef FLOAT(53), @MonthPostCoef FLOAT(53);
	SELECT @TodayTopicCoef = t.c.value(N'@todayTopicCoef', N'FLOAT(53)'), @WeekTopicCoef = t.c.value(N'@weekTopicCoef', N'FLOAT(53)'), @MonthTopicCoef = t.c.value(N'@monthTopicCoef', N'FLOAT(53)'), @TodayPostCoef = t.c.value(N'@todayPostCoef', 'FLOAT(53)'), @WeekPostCoef = t.c.value(N'@weekPostCoef', N'FLOAT(53)'), @MonthPostCoef = t.c.value(N'@monthPostCoef', N'FLOAT(53)') FROM @ConfigXml.nodes(N'/forumActivityConfig') t(c);

	DELETE FROM b_ForumActivity WHERE RatingId = @RatingId;
	INSERT INTO b_ForumActivity(RatingId, Active, IsCalculated, UserId, CreatedUtc, LastCalculatedUtc, Value, TopicValue, PostValue, TodayTopicCoef, WeekTopicCoef, MonthTopicCoef, TodayPostCoef, WeekPostCoef, MonthPostCoef, XmlId)
		SELECT @RatingId, 1, 0, U.ID, GETUTCDATE(), GETUTCDATE(), 0.0, 0.0, 0.0, @TodayTopicCoef, @WeekTopicCoef, @MonthTopicCoef, @TodayPostCoef, @WeekPostCoef, @MonthPostCoef, NULL
			FROM b_ForumUser U;			
END
GO

IF OBJECT_ID (N'ForumActivity_Disengage', N'P') IS NOT NULL
	DROP PROCEDURE ForumActivity_Disengage;
GO

CREATE PROCEDURE ForumActivity_Disengage
	@RatingId INT
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM b_ForumActivity WHERE RatingId = @RatingId;
END
GO

IF OBJECT_ID (N'ForumActivity_Calculate', N'P') IS NOT NULL
	DROP PROCEDURE ForumActivity_Calculate;
GO

CREATE PROCEDURE ForumActivity_Calculate
	@RatingId INT,
	@Now DATETIME,
	@Count INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @MonthBefore DATETIME, @WeekBefore DATETIME, @DayStart DATETIME;
	

	IF(@Now IS NULL) SET @Now = GETDATE();
	IF(@Count IS NULL OR @Count <= 0) SET @Count = 5; -- DEFAULT
	IF(@Count > 100) SET @Count = 100; -- MAX
	
	SET @MonthBefore = DATEADD(DAY, -30, @Now);
	SET @WeekBefore = DATEADD(DAY, -7, @Now);
	SET @DayStart = DATEADD(DAY, 0, DATEDIFF(DAY, 0, @Now));
	
	
	WITH tgt(ID, UserId) AS
	(
		SELECT TOP(@Count) ID, UserId
		FROM b_ForumActivity 
		WHERE RatingId = @RatingId AND Active = 1 AND IsCalculated = 0
		--ORDER BY UserId 
	),
	topic30(ID, AuthorId, DateCreate) AS
	(
		SELECT tp.ID, tp.AuthorId, tp.DateCreate 
		FROM b_ForumTopic tp INNER JOIN tgt t ON tp.AuthorId = t.UserId WHERE tp.Approved = 1 AND tp.DateCreate >= @MonthBefore AND tp.DateCreate <= @Now
	),
	topic30g(AuthorId, Qty) AS
	(
		SELECT AuthorId, COUNT(AuthorId) FROM topic30 GROUP BY(AuthorId)
	),	
	topic7(ID, AuthorId, DateCreate) AS
	(
		SELECT ID, AuthorId, DateCreate FROM topic30 WHERE DateCreate >= @WeekBefore
	),
	topic7g(AuthorId, Qty) AS
	(
		SELECT AuthorId, COUNT(AuthorId) FROM topic7 GROUP BY(AuthorId)
	),		
	topic1g(AuthorId, Qty) AS
	(
		SELECT AuthorId, COUNT(AuthorId) FROM topic7 WHERE DateCreate >= @DayStart GROUP BY(AuthorId)			
	),
	post30(ID, AuthorId, DateCreate) AS
	(
		SELECT p.ID, p.AuthorId, p.DateCreate 
		FROM b_ForumPost p INNER JOIN tgt t ON p.AuthorId = t.UserId WHERE p.Approved = 1 AND p.DateCreate >= @MonthBefore AND p.DateCreate <= @Now
	),			
	post30g(AuthorId, Qty) AS
	(
		SELECT AuthorId, COUNT(AuthorId) FROM post30 GROUP BY(AuthorId)
	),
	post7(ID, AuthorId, DateCreate) AS
	(
		SELECT ID, AuthorId, DateCreate FROM post30 WHERE DateCreate >= @WeekBefore
	),
	post7g(AuthorId, Qty) AS
	(
		SELECT AuthorId, COUNT(AuthorId) FROM post7 GROUP BY(AuthorId)
	),
	post1g(AuthorId, Qty) AS
	(
		SELECT AuthorId, COUNT(AuthorId) FROM post7 WHERE DateCreate >= @DayStart GROUP BY(AuthorId)			
	)
	UPDATE b_ForumActivity
	SET TopicValue = ISNULL(t30g.Qty, 0) * MonthTopicCoef + ISNULL(t7g.Qty, 0) * WeekTopicCoef + ISNULL(t1g.Qty, 0) * TodayTopicCoef,
		PostValue = ISNULL(p30g.Qty, 0) * MonthPostCoef + ISNULL(p7g.Qty, 0) * WeekPostCoef + ISNULL(p1g.Qty, 0) * TodayPostCoef,
		Value =  ISNULL(t30g.Qty, 0) * MonthTopicCoef + ISNULL(t7g.Qty, 0) * WeekTopicCoef + ISNULL(t1g.Qty, 0) * TodayTopicCoef + ISNULL(p30g.Qty, 0) * MonthPostCoef + ISNULL(p7g.Qty, 0) * WeekPostCoef + ISNULL(p1g.Qty, 0) * TodayPostCoef,
		LastCalculatedUtc = GETUTCDATE(),
		IsCalculated = 1
	FROM b_ForumActivity a INNER JOIN tgt t ON a.ID = t.ID
		LEFT OUTER JOIN topic30g t30g ON t30g.AuthorId = t.UserId
		LEFT OUTER JOIN topic7g t7g ON t7g.AuthorId = t.UserId 
		LEFT OUTER JOIN topic1g t1g ON t1g.AuthorId = t.UserId 	
		LEFT OUTER JOIN post30g p30g ON p30g.AuthorId = t.UserId
		LEFT OUTER JOIN post7g p7g ON p7g.AuthorId = t.UserId 
		LEFT OUTER JOIN post1g p1g ON p1g.AuthorId = t.UserId 

	SELECT @@ROWCOUNT;		
END
GO

IF OBJECT_ID (N'ForumActivity_EngageForUser', N'P') IS NOT NULL
	DROP PROCEDURE ForumActivity_EngageForUser;
GO

CREATE PROCEDURE ForumActivity_EngageForUser
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	IF(NOT EXISTS(SELECT N'*' FROM b_BlogUser WHERE ID = @UserId)) RETURN;
	
	DELETE FROM b_ForumActivity WHERE UserId = @UserId;
	INSERT INTO b_ForumActivity(RatingId, Active, IsCalculated, UserId, CreatedUtc, LastCalculatedUtc, Value, TopicValue, PostValue, TodayTopicCoef, WeekTopicCoef, MonthTopicCoef, TodayPostCoef, WeekPostCoef, MonthPostCoef, XmlId)	
	SELECT ID, 
		1, 0, 
		@UserId, 
		GETUTCDATE(), GETUTCDATE(), 
		0.0, 0.0, 0.0, 
		ComponentConfigXml.value(N'(/forumActivityConfig/@todayTopicCoef)[1]', 'FLOAT(53)'),
		ComponentConfigXml.value(N'(/forumActivityConfig/@weekTopicCoef)[1]', 'FLOAT(53)'),
		ComponentConfigXml.value(N'(/forumActivityConfig/@monthTopicCoef)[1]', 'FLOAT(53)'),		
		ComponentConfigXml.value(N'(/forumActivityConfig/@todayPostCoef)[1]', 'FLOAT(53)'),
		ComponentConfigXml.value(N'(/forumActivityConfig/@weekPostCoef)[1]', 'FLOAT(53)'),
		ComponentConfigXml.value(N'(/forumActivityConfig/@monthPostCoef)[1]', 'FLOAT(53)'),
		NULL		   
	FROM b_Rating WHERE Active = 1 AND ComponentConfigXml.exist(N'/forumActivityConfig') = 1;			
END
GO

IF OBJECT_ID (N'ForumActivity_DisengageForUser', N'P') IS NOT NULL
	DROP PROCEDURE ForumActivity_DisengageForUser;
GO

CREATE PROCEDURE ForumActivity_DisengageForUser
	@UserId INT
AS
BEGIN
	DELETE FROM b_ForumActivity WHERE UserId = @UserId;
END
GO

IF OBJECT_ID (N'ForumTopicVoting_GetEngagingUsers', N'P') IS NOT NULL
	DROP PROCEDURE ForumTopicVoting_GetEngagingUsers;
GO

CREATE PROCEDURE ForumTopicVoting_GetEngagingUsers
	@RatingId INT,
	@Count INT
AS
BEGIN
	SET NOCOUNT ON;	
	
	IF(@Count IS NULL OR @Count < 0) SET @Count = 5;
	
	SELECT TOP(@Count) t.AuthorId
	FROM b_RatingVoting v 
	INNER JOIN b_ForumTopic t 
		ON v.BoundEntityTypeId = N'FORUMTOPIC'
			AND v.BoundEntityId = t.ID 
			AND NOT EXISTS(SELECT ID FROM b_RatingCounter c WHERE c.RatingId = @RatingId AND c.BoundEntityTypeId = N'USER' AND c.BoundEntityId = t.AuthorId);
END
GO

IF OBJECT_ID (N'ForumPostVoting_GetEngagingUsers', N'P') IS NOT NULL
	DROP PROCEDURE ForumPostVoting_GetEngagingUsers;
GO

CREATE PROCEDURE ForumPostVoting_GetEngagingUsers
	@RatingId INT,
	@Count INT
AS
BEGIN
	SET NOCOUNT ON;	
	
	IF(@Count IS NULL OR @Count < 0) SET @Count = 5;
	
	SELECT TOP(@Count) p.AuthorId
	FROM b_RatingVoting v 
	INNER JOIN b_ForumPost p 
		ON v.BoundEntityTypeId = N'FORUMPOST'
			AND v.BoundEntityId = p.ID 
			AND NOT EXISTS(SELECT ID FROM b_RatingCounter c WHERE c.RatingId = @RatingId AND c.BoundEntityTypeId = N'USER' AND c.BoundEntityId = p.AuthorId);
END
GO

IF OBJECT_ID (N'ForumActivity_GetEngagingUsers', N'P') IS NOT NULL
	DROP PROCEDURE ForumActivity_GetEngagingUsers;
GO

CREATE PROCEDURE ForumActivity_GetEngagingUsers
	@RatingId INT,
	@Count INT
AS
BEGIN
	SET NOCOUNT ON;	
	
	IF(@Count IS NULL OR @Count < 0) SET @Count = 5;
	
	SELECT TOP(@Count) V.UserId 
		FROM b_ForumActivity V 
		WHERE V.RatingId = @RatingId
			AND NOT EXISTS(SELECT TOP 1 ID FROM b_RatingCounter WHERE RatingId = @RatingId AND BoundEntityTypeId = N'USER' AND BoundEntityId = V.UserId);
END
GO
/*--- END OF RATINGS   ---*/
GO

IF OBJECT_ID (N'Comments_GetRootPostId', N'P') IS NOT NULL
	DROP PROCEDURE Comments_GetRootPostId;
GO
CREATE PROCEDURE Comments_GetRootPostId
@postId int
AS

CREATE TABLE #t (postId int, parentId int, num  bigint);

	INSERT #t(postId,parentId,num)
	SELECT ID,parentPostID,null
	FROM b_ForumPost p

	WHERE 
	TopicId = (select TopicId from b_ForumPost where ID = @postId)
	order by id;


with cte(id,parentPostId) as (
	select Id,parentPostId from b_ForumPost where ID = @postId
	union all
	select parents.ID,parents.parentPostId from b_ForumPost parents join cte c on c.parentPostId = parents.ID)
select top 1 cte.id from cte where parentPostId = 0

drop table #t
GO

IF OBJECT_ID (N'Comments_GetTree', N'P') IS NOT NULL
	DROP PROCEDURE Comments_GetTree;
GO

CREATE PROCEDURE Comments_GetTree
@topicId int,
@firstPostId bigint,
@rootPostId bigint,
@firstNumber bigint,
@lastNumber bigint,
@showHidden bit
AS
	SET NOCOUNT ON
	CREATE TABLE #t (postId int, parentId int, num  bigint,approved bit);
	
	if ( @topicId = 0 and @rootPostId <> 0)
		SELECT @topicId = ISNULL(topicId,0)
		FROM b_ForumPost where ID = @rootPostId

	INSERT #t(postId,parentId,num,approved)
	SELECT ID,parentPostID,null,Approved
	FROM b_ForumPost p
	WHERE 
	TopicId = @topicId and ID <> @firstPostId and (@showHidden=1 or Approved=1)
	ORDER BY id;

	WITH _BXFLTRCTE_0 AS (select postId, DENSE_RANK() OVER ( ORDER BY postId asc, postId asc ) as RNK FROM #t where parentId = 0)
	update p set num = (select rnk from _BXFLTRCTE_0 where postId = p.postId)
	from #t p
	where parentId = 0
	
	CREATE TABLE #result (id int identity(1,1), postId bigint,parentId bigint,indent int,approved bit);

 
	With posts(postId,parentId,lvl,approved) AS
	( SELECT postId,parentId,0 as lvl,approved
		FROM #t WHERE (@rootPostId= 0 and parentId = 0 and num between @firstNumber and @lastNumber) 
		or (@rootPostId <> 0 and postId=@rootPostId)
		UNION ALL
		SELECT childs.postId,childs.parentId,lvl+1,childs.approved
		FROM #t childs join posts parent on childs.parentId = parent.postId
		)
	INSERT #result(postId,parentId,indent,approved) SELECT posts.postId,posts.parentId,lvl,approved FROM posts

	SELECT postId,parentId,indent,approved from #result

	drop table #t
	drop table #result
GO