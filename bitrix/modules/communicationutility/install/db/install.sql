/*--- BEGINNING OF RATINGS   ---*/
IF OBJECT_ID (N'b_RatingVoting', N'U') IS NULL
	CREATE TABLE b_RatingVoting
	(
		ID INT IDENTITY(1,1) NOT NULL,
		BoundEntityTypeHash INT NOT NULL,
		BoundEntityHash INT NOT NULL,
		BoundEntityTypeId NVARCHAR(256) NOT NULL,
		BoundEntityId NVARCHAR(64) NOT NULL,
		CustomPropertyEntityId NVARCHAR(64) NOT NULL,	
		Active BIT NOT NULL, 
		CreatedUtc DATETIME NOT NULL,
		LastCalculatedUtc DATETIME NOT NULL,
		TotalValue FLOAT(53) NOT NULL,	
		TotalVotes INT NOT NULL,
		TotalPositiveVotes INT NOT NULL, 	
		TotalNegativeVotes INT NOT NULL,	
		XmlId VARCHAR(256) NULL,
		CONSTRAINT PK_b_RatingVoting PRIMARY KEY CLUSTERED(ID ASC)
	)
GO

IF OBJECT_ID (N'b_RatingVote', N'U') IS NULL
	CREATE TABLE b_RatingVote
	(
		ID INT IDENTITY(1,1) NOT NULL,
		RatingVotingId INT NOT NULL,
		Value FLOAT(53) NOT NULL,	
		Active BIT NOT NULL,
		CreatedUtc DATETIME NOT NULL,
		UserId INT NOT NULL,
		UserIP NVARCHAR(64) NOT NULL,	
		XmlId VARCHAR(256) NULL,
		CONSTRAINT PK_b_RatingVote PRIMARY KEY CLUSTERED(ID ASC),
		CONSTRAINT FK_RatingVote_RatingVoting FOREIGN KEY(RatingVotingId) REFERENCES b_RatingVoting(ID)
	)
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_RatingVote_RatingVotingId_UserId')
	CREATE NONCLUSTERED INDEX IX_b_RatingVote_RatingVotingId_UserId ON b_RatingVote(RatingVotingId ASC, UserId ASC) INCLUDE (ID)
GO

IF OBJECT_ID (N'b_RatingVotingBoundUnit', N'U') IS NULL
	CREATE TABLE b_RatingVotingBoundUnit
	(
		ID INT IDENTITY(1,1) NOT NULL,
		ParentId INT NOT NULL,	
		BoundEntityTypeId NVARCHAR(256) NOT NULL,
		BoundEntityId NVARCHAR(64) NOT NULL,		
		XmlId VARCHAR(256) NULL,
		CONSTRAINT PK_b_RatingVotingBoundUnit PRIMARY KEY CLUSTERED(ID ASC)
	)
GO

IF OBJECT_ID (N'b_RatingVotingGroupTotal', N'U') IS NULL
	CREATE TABLE b_RatingVotingGroupTotal
	(
		ID INT IDENTITY(1,1) NOT NULL,
		BoundEntityTypeId NVARCHAR(256) NOT NULL,
		BoundEntityId NVARCHAR(64) NOT NULL,
		GroupingEntityTypeId NVARCHAR(256) NOT NULL,	
		TotalValue FLOAT(53) NOT NULL,	
		TotalVotes INT NOT NULL,
		TotalPositiveVotes INT NOT NULL, 	
		TotalNegativeVotes INT NOT NULL,	
		XmlId VARCHAR(256) NULL,
		CONSTRAINT PK_b_RatingVotingGroupTotal PRIMARY KEY CLUSTERED(ID ASC)
	)
GO

IF OBJECT_ID (N'RatingVoting_Synchronize', N'P') IS NOT NULL
	DROP PROCEDURE RatingVoting_Synchronize;
GO

CREATE PROCEDURE RatingVoting_Synchronize 
	@RatingVotingId INT
AS
BEGIN
	SET NOCOUNT ON;
	IF @RatingVotingId IS NULL OR @RatingVotingId <= 0 OR NOT EXISTS(SELECT ID FROM b_RatingVoting WHERE ID = @RatingVotingId AND Active = 1) RETURN;

	DECLARE 
		@curTotalValue FLOAT(53), @prevTotalValue FLOAT(53), 
		@curTotalVotes INT, @prevTotalVotes INT,
		@curTotalPositiveVotes INT, @prevTotalPositiveVotes INT,
		@curTotalNegativeVotes INT, @prevTotalNegativeVotes INT,
		@boundEntityTypeId NVARCHAR(256),
		@boundEntityId NVARCHAR(256);
		
	DECLARE @boundTable TABLE(ID INT, ParentId INT, BoundEntityTypeId NVARCHAR(256), BoundEntityId NVARCHAR(256));
	
	BEGIN TRANSACTION;
	SELECT	@boundEntityTypeId = UPPER(BoundEntityTypeId),
			@boundEntityId = BoundEntityId,
			@prevTotalValue = TotalValue, 
			@prevTotalVotes = TotalVotes,
			@prevTotalPositiveVotes = TotalPositiveVotes,
			@prevTotalNegativeVotes = TotalNegativeVotes
	FROM b_RatingVoting 
	WHERE ID = @RatingVotingId;
		
	SET @curTotalValue = (SELECT ISNULL(SUM(Value), 0) FROM b_RatingVote WHERE RatingVotingId = @RatingVotingId AND Active = 1);
	SET @curTotalVotes = (SELECT COUNT(ID) FROM b_RatingVote WHERE RatingVotingId = @RatingVotingId AND Active = 1);
	SET @curTotalPositiveVotes = (SELECT COUNT(ID) FROM b_RatingVote WHERE RatingVotingId = @RatingVotingId AND Active = 1 AND Value > 0);
	SET @curTotalNegativeVotes = (SELECT COUNT(ID) FROM b_RatingVote WHERE RatingVotingId = @RatingVotingId AND Active = 1 AND Value < 0);
	
	UPDATE b_RatingVoting 
	SET TotalValue = @curTotalValue, 
		TotalVotes = @curTotalVotes,
		TotalPositiveVotes = @curTotalPositiveVotes,
		TotalNegativeVotes = @curTotalNegativeVotes,
		LastCalculatedUtc = GETUTCDATE() 
	WHERE ID = @RatingVotingId;
	
	WITH 
		bound(ID, ParentId, BoundEntityTypeId, BoundEntityId) 
		AS (
			SELECT ID, ParentId, BoundEntityTypeId, BoundEntityId
				FROM b_RatingVotingBoundUnit
				WHERE BoundEntityTypeId = @boundEntityTypeId and BoundEntityId = @boundEntityId
			UNION ALL 
			SELECT parent.ID, parent.ParentId, parent.BoundEntityTypeId, parent.BoundEntityId
				FROM b_RatingVotingBoundUnit parent INNER JOIN  bound ON bound.ParentId = parent.ID
		)
		INSERT INTO @boundTable(ID, ParentId, BoundEntityTypeId, BoundEntityId)
			SELECT ID, ParentId, BoundEntityTypeId, BoundEntityId 
				FROM bound;
			
	UPDATE	b_RatingVotingGroupTotal
		SET	TotalValue = TotalValue - @prevTotalValue + @curTotalValue,
			TotalVotes = TotalVotes - @prevTotalVotes + @curTotalVotes,
			TotalPositiveVotes = TotalPositiveVotes - @prevTotalPositiveVotes + @curTotalPositiveVotes,
			TotalNegativeVotes = TotalNegativeVotes - @prevTotalNegativeVotes + @curTotalNegativeVotes
		WHERE ID IN (
			SELECT t.ID 
				FROM b_RatingVotingGroupTotal t 
				INNER JOIN @boundTable b 
				ON t.BoundEntityTypeId = b.BoundEntityTypeId AND t.BoundEntityId = b.BoundEntityId AND t.GroupingEntityTypeId = @boundEntityTypeId
			);

	UPDATE	b_RatingCounter 
	SET	IsCalculated = 0 
	WHERE (
	(BoundEntityTypeId = @boundEntityTypeId AND BoundEntityId = @boundEntityId)
		OR EXISTS(SELECT N'*' FROM @boundTable bt WHERE bt.BoundEntityTypeId = b_RatingCounter.BoundEntityTypeId AND bt.BoundEntityId = b_RatingCounter.BoundEntityId)
		)
		AND EXISTS(SELECT N'*' FROM b_RatingComponent c WHERE c.RatingCounterId = b_RatingCounter.ID AND DependencyXml.exist(N'./componentDependency[@targetComponentTypeName = "RatingVotingComponent" and @entityTypeId = sql:variable("@boundEntityTypeId")]') = 1);
	SELECT @curTotalValue AS TotalValue, @curTotalVotes AS TotalVotes, @curTotalPositiveVotes AS TotalPositiveVotes, @curTotalNegativeVotes AS TotalNegativeVotes;		
	COMMIT TRANSACTION;	
END
GO

IF OBJECT_ID (N'RatingVote_Count', N'P') IS NOT NULL
	DROP PROCEDURE RatingVote_Count
GO

CREATE PROCEDURE RatingVote_Count 
	@UserId INT,
	@RatingVotingId INT
AS
BEGIN
	SET NOCOUNT ON;
	SELECT COUNT(ID) FROM b_RatingVote WHERE UserId = @UserId AND RatingVotingId = @RatingVotingId;
END
GO

IF OBJECT_ID (N'RatingVoting_GetTotals', N'P') IS NOT NULL
	DROP PROCEDURE RatingVoting_GetTotals
GO

CREATE PROCEDURE RatingVoting_GetTotals 
	@BoundEntityTypeId NVARCHAR(256),
	@BoundEntityId nvarchar(64),
	@UserId INT
AS
BEGIN
	SELECT TOP 1
		voting.ID, 
		voting.TotalValue, 
		voting.TotalVotes, 
		voting.TotalPositiveVotes, 
		voting.TotalNegativeVotes, 
		@UserId UserId,
		(CASE WHEN EXISTS (SELECT vote.ID FROM b_RatingVote vote WHERE vote.RatingVotingId = voting.ID AND vote.UserId = @UserId) THEN 1 ELSE 0 END) UserHasVoted
	FROM b_RatingVoting voting
	WHERE voting.BoundEntityTypeId = UPPER(@BoundEntityTypeId) AND voting.BoundEntityId = @BoundEntityId AND voting.Active = 1;
END
GO

IF OBJECT_ID (N'RatingVote_Delete', N'P') IS NOT NULL
	DROP PROCEDURE RatingVote_Delete;
GO

CREATE PROCEDURE RatingVote_Delete
	@RatingVotingId INT
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM b_RatingVote WHERE RatingVotingId = @RatingVotingId;	
END
GO

/*--- END OF RATINGS   ---*/

IF OBJECT_ID (N'b_PrivateMessageUser', N'U') IS NULL
CREATE TABLE [b_PrivateMessageUser](
	[ID] [int] NOT NULL,
	[MessageCount] [int] NOT NULL DEFAULT(0),
	[NotifyByEmail] [bit] NOT NULL,
	[LastMessageDate] [datetime] NULL,
	[LastNotifiedMessageId] [bigint] NULL
 CONSTRAINT [PK_b_PrivateMessageUser] PRIMARY KEY CLUSTERED ([ID] ASC)
) 

GO


IF OBJECT_ID (N'b_PrivateMessageFolders', N'U') IS NULL
CREATE TABLE [b_PrivateMessageFolders](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[Title] [nvarchar](256) NOT NULL,
	[TopicsCount] [int] NOT NULL DEFAULT(0),
	[Sort] [int] NULL DEFAULT(0),
	CONSTRAINT PK_b_PrivateMessageFolders PRIMARY KEY CLUSTERED (ID ASC),
	CONSTRAINT [FK_b_PrivateMessageFolders_UserId_b_PrivateMessageUser_ID] FOREIGN KEY([UserId])
	REFERENCES [b_PrivateMessageUser] ([ID])
	
) 

GO

IF OBJECT_ID (N'b_PrivateMessageTopics', N'U') IS NULL
CREATE TABLE [b_PrivateMessageTopics](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](256) NOT NULL,
	[StarterId] [int] NOT NULL,
	[StarterName] [nvarchar](256) NOT NULL,
	[MessageCount] [int] NOT NULL,
	[FirstMessageId] [bigint] NOT NULL,
	[LastPosterId] [int] NOT NULL,
	[LastPosterName] [nvarchar](256) NOT NULL,
	[LastMessageId] [bigint] NOT NULL,
	[LastMessageDate] [datetime] NULL,
	CONSTRAINT PK_b_PrivateMessageTopics  PRIMARY KEY CLUSTERED ([ID] ASC)
) 

GO

IF OBJECT_ID (N'b_PrivateMessages', N'U') IS NULL
CREATE TABLE [b_PrivateMessages](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[FromId] [int] NOT NULL,
	[FromUserName] [nvarchar](256) NOT NULL,
	[TopicId] [int] NOT NULL,
	[Body] [nvarchar](max) NOT NULL,
	[FileId] [int] NULL,
	[SentDate] [datetime] NOT NULL,
	[IsSystem] [bit] NOT NULL,
	CONSTRAINT [FK_b_PrivateMessages_TopicId_b_PrivateMessageTopics_ID] FOREIGN KEY([TopicId])
	REFERENCES [b_PrivateMessageTopics] ([ID]),
	CONSTRAINT PK_b_PrivateMessages PRIMARY KEY CLUSTERED ([ID] ASC)
)

GO

IF OBJECT_ID (N'b_PrivateMessageMappings', N'U') IS NULL
CREATE TABLE [b_PrivateMessageMappings](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TopicId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[FolderId] [int] NULL,
	[UnreadMessageCount] [int] NOT NULL,
	[ReadDate] [datetime] NOT NULL,
	[NotifySender] [bit] NOT NULL,
	[NotifyByEmail] [bit] NOT NULL,
	[Deleted] [bit] NOT NULL,
	CONSTRAINT [FK_b_PrivateMessageMappings_TopicId_b_PrivateMessageTopics_ID] FOREIGN KEY([TopicId])
	REFERENCES [b_PrivateMessageTopics] ([ID]),
	CONSTRAINT [FK_b_PrivateMessageMappings_UserId_b_PrivateMessageUser_ID] FOREIGN KEY([UserId])
	REFERENCES [b_PrivateMessageUser] ([ID]),
	CONSTRAINT PK_b_PrivateMessageMappings PRIMARY KEY CLUSTERED ([ID] ASC)
)

GO
IF OBJECT_ID (N'PrivateMessages_FoldersChangeSort', N'P') IS NOT NULL
	DROP PROCEDURE PrivateMessages_FoldersChangeSort
GO

CREATE PROCEDURE [PrivateMessages_FoldersChangeSort]
@UserId int,
@Up bit,
@FolderIds varchar(MAX)=''
AS
BEGIN
	SET NOCOUNT ON
	declare 
		@k				int,
		@ch				varchar(1),
		@allowedChars	varchar(10),
		@dig			varchar(15),
		@intNum			int,
		@FolderId		int,
		@oldSort		int,
		@separators		varchar(100),
		@nearestSort	int,
		@nearestFolderId int,
		@moved int
		
	SELECT @separators=','
	
	CREATE TABLE #folders ( ID int, oldSort int )
		
	SELECT @k=1,@intNum = -1,@dig=''
	
	--Parse string to ints
	WHILE ( @k<=Len(@FolderIds)) BEGIN
	
		SET @ch = SUBSTRING(@FolderIds,@k,1)
		if ( CHARINDEX(@ch,@separators)=0 ) 
				set @dig = @dig + @ch
 
		ELSE BEGIN
			if ( LEN(@dig) >0 )
				BEGIN TRY
					SET @intNum = CONVERT(int,@dig)
				END TRY
				BEGIN CATCH
					
				END CATCH
			SET @dig = ''
		END
		
		IF ( @intNum<>-1 ) 
		BEGIN
			if ( NOT EXISTS ( SELECT ID FROM #folders WHERE ID = @intNum ))
				INSERT #folders (ID) 
				VALUES (@intNum)
			
			SET @intNum = -1
		END
			
		
		SET @k = @k + 1
	END
	
	if ( LEN(@dig) >0 ) BEGIN
		BEGIN TRY
			SET @intNum = CONVERT(int,@dig)

		END TRY
		BEGIN CATCH
			SET @intNum = -1
		END CATCH

		IF @intNum<>-1 AND NOT EXISTS 
		( SELECT ID FROM #folders WHERE ID = @intNum )
			INSERT #folders (ID) 
			VALUES (@intNum)
	END
	
	UPDATE #folders
	SET oldsort = pmf.Sort
	FROM
	#folders ff join b_PrivateMessageFolders pmf on pmf.ID = ff.ID 
	WHERE pmf.UserId = @UserId

	DELETE FROM #folders WHERE oldSort IS NULL
	
	CREATE TABLE #userFolders( ID int, sort int,moved int )
	INSERT #userFolders
	SELECT ID,Sort,1
	FROM b_PrivateMessageFolders WHERE UserId = @UserId
	
	IF (@Up = 1)
		DECLARE foldersCursor CURSOR READ_ONLY FORWARD_ONLY
		FOR SELECT ID, oldsort
		FROM #folders
		ORDER BY oldSort ASC
	ELSE
		DECLARE foldersCursor CURSOR READ_ONLY FORWARD_ONLY
		FOR SELECT ID, oldsort
		FROM #folders
		ORDER BY oldSort DESC
	
	OPEN foldersCursor
	

	
	FETCH NEXT FROM foldersCursor into @FolderId,@oldSort
	
	WHILE @@FETCH_STATUS = 0 BEGIN
		SET @nearestSort = NULL
		select @oldSort = sort from #userFolders where ID = @FolderId
		IF ( @Up = 1 ) 
			SELECT top 1 @nearestSort = sort,@nearestFolderId = ID,@moved = moved 
			FROM #userFolders 
			WHERE sort < @oldSort
			ORDER BY sort DESC
		ELSE
			SELECT top 1 @nearestSort = sort,@nearestFolderId = ID ,@moved = moved
			FROM #userFolders 
			WHERE sort > @oldSort
			ORDER BY sort ASC
		
		IF ( NOT @nearestSort IS NULL and @moved<>0 ) BEGIN
			UPDATE #userFolders SET Sort = @nearestSort,moved = 1
			WHERE ID = @FolderId
			UPDATE #userFolders SET Sort = @oldSort,moved = 1 
			WHERE ID = @nearestFolderId
			
		END
		ELSE UPDATE #userFolders SET moved = 0 WHERE ID = @FolderId
			
		FETCH NEXT FROM foldersCursor into @FolderId,@oldSort
	END
	
	CLOSE foldersCursor
	DEALLOCATE foldersCursor
	
	update b_PrivateMessageFolders SET Sort = f.Sort
	FROM
	b_PrivateMessageFolders pmf join #userFolders f on f.ID = pmf.ID

	DROP TABLE #folders
	DROP TABLE #userFolders
END

GO

IF OBJECT_ID (N'PrivateMessages_MoveTopics', N'P') IS NOT NULL
	DROP PROCEDURE PrivateMessages_MoveTopics
GO

CREATE PROCEDURE [PrivateMessages_MoveTopics]
@UserId int,
@FolderId int =0,
@MappingIds varchar(MAX)=''
AS
BEGIN
	SET NOCOUNT ON
	declare 
		@k				int,
		@ch				varchar(1),
		@allowedChars	varchar(10),
		@dig			varchar(15),
		@intNum			int,
		@curUserId		int,
		@flId			int,
		@count			int,
		@separators		varchar(100)
		
	SELECT @separators=','
	
	CREATE TABLE #mappings ( ID int )
		
	SELECT @k=1,@intNum = -1,@curUserId = 0,@dig=''
	
	IF ( @FolderId<> 0) BEGIN
	
		SELECT @curUserId  = UserId 
		FROM
			b_PrivateMessageFolders 
		WHERE
			ID = @FolderId
		
		IF (@curUserId = 0 or @curUserId<>@UserId) RETURN
	
	END
	--Parse string to ints
	WHILE ( @k<=Len(@MappingIds)) BEGIN
	
		SET @ch = SUBSTRING(@MappingIds,@k,1)
		if ( CHARINDEX(@ch,@separators)=0 ) 
				set @dig = @dig + @ch
 
		ELSE BEGIN
			if ( LEN(@dig) >0 )
				BEGIN TRY
					SET @intNum = CONVERT(int,@dig)
				END TRY
				BEGIN CATCH
					
				END CATCH
			SET @dig = ''
		END
		
		IF ( @intNum<>-1 ) 
		BEGIN
			if ( NOT EXISTS ( SELECT ID FROM #mappings WHERE ID = @intNum ))
				INSERT #mappings (ID) 
				VALUES (@intNum)
			
			SET @intNum = -1
		END
			
		
		SET @k = @k + 1
	END
	
	if ( LEN(@dig) >0 ) BEGIN
		BEGIN TRY
			SET @intNum = CONVERT(int,@dig)

		END TRY
		BEGIN CATCH
			SET @intNum = -1
		END CATCH

		IF @intNum<>-1 AND NOT EXISTS 
		( SELECT ID FROM #mappings WHERE ID = @intNum )
			INSERT #mappings (ID) 
			VALUES (@intNum)
	END

	CREATE TABLE #folders ( ID int )
	
	INSERT #folders(ID)
	SELECT DISTINCT FolderId 
	FROM 
		b_PrivateMessageMappings PMM
		JOIN #mappings M on PMM.ID = M.ID
	WHERE
		PMM.UserId = @UserId
		
	IF NOT EXISTS ( SELECT * FROM #folders WHERE ID=@FolderId )
		INSERT #folders(ID) VALUES (@FolderId)

	-- moving topics to other folder
	select @FolderId,@curUserId
	UPDATE PMM SET FolderId = @FolderId
	FROM
		b_PrivateMessageMappings PMM 
		JOIN #mappings M on PMM.ID = M.ID
	WHERE
		PMM.UserId = @UserId
		
	-- updating counters
	DECLARE foldersCursor CURSOR READ_ONLY FORWARD_ONLY
	FOR 
		SELECT ID FROM #folders
		
	OPEN foldersCursor
		
	FETCH NEXT FROM foldersCursor INTO @flId
	WHILE @@FETCH_STATUS = 0 BEGIN
		
		SELECT 
			@count = COUNT(*) 
		FROM 
			b_PrivateMessageMappings
		WHERE
			FolderId = @flId
		
		UPDATE b_PrivateMessageFolders SET TopicsCount = @count
		WHERE ID = @flId
	
		FETCH NEXT FROM foldersCursor INTO @flId
	END
	
	CLOSE foldersCursor
	DEALLOCATE foldersCursor		
		
	DROP TABLE #mappings
	DROP TABLE #folders
END
GO

IF OBJECT_ID (N'PrivateMessages_OnDeleteTopic', N'P') IS NOT NULL
	DROP PROCEDURE PrivateMessages_OnDeleteTopic
GO

CREATE PROCEDURE [PrivateMessages_OnDeleteTopic] 
	@TopicId int = 0
AS
BEGIN
	SET NOCOUNT ON
	DECLARE	
		@count		int,
		@userId		int,
		@folderId	int

	IF @TopicId = 0 RETURN

	IF NOT EXISTS ( SELECT * 
					FROM b_PrivateMessageTopics 
					WHERE ID = @TopicId )
		RETURN

	CREATE TABLE #usersFolders (	UserId		int,
									FolderId	int 
								)

	INSERT #usersFolders ( UserId ,FolderId )
	SELECT PMM.UserId,PMM.FolderId
	FROM 
		b_PrivateMessageMappings PMM 
	WHERE
		TopicId = @TopicId
		
	DELETE FROM b_PrivateMessageMappings
	WHERE 
		TopicId = @TopicId
		
	DELETE FROM b_PrivateMessages
	WHERE
		TopicId = @TopicId

	DECLARE cr CURSOR READ_ONLY FORWARD_ONLY
	FOR SELECT UserId,FolderId
	FROM 
		#usersFolders
		
	OPEN cr
	FETCH NEXT FROM cr INTO @userId,@folderId
	
	WHILE @@FETCH_STATUS = 0 BEGIN
		
		SELECT @count = COUNT(*)
		FROM
			b_PrivateMessageMappings
		WHERE
			FolderId = @folderId
		
		UPDATE b_PrivateMessageFolders 
		SET TopicsCount = @count
		WHERE
			ID = @folderId
			
		SELECT @count = COUNT(*)
		FROM b_PrivateMessages
		WHERE 
			FromId = @userId
			
		UPDATE b_PrivateMessageUser
		SET MessageCount = @count
		WHERE
			ID = @userId
				
		FETCH NEXT FROM cr INTO @userId,@folderId
	END
	
	CLOSE cr
	DEALLOCATE cr
	
	DROP TABLE #usersFolders

END

GO

IF OBJECT_ID (N'PrivateMessages_SynchronizeFolder', N'P') IS NOT NULL
	DROP PROCEDURE PrivateMessages_SynchronizeFolder
GO

CREATE PROCEDURE [PrivateMessages_SynchronizeFolder]
@FolderId int =0
AS
BEGIN
	SET NOCOUNT ON
	
	DECLARE @count int
	
	IF NOT EXISTS ( SELECT * FROM b_PrivateMessageFolders WHERE ID = @FolderId )
		RETURN
	
	SELECT @count = COUNT(*)
	FROM
		b_PrivateMessageMappings
	WHERE
		FolderId = @FolderId
		AND Deleted = 0
	
	UPDATE b_PrivateMessageFolders SET TopicsCount = @count
	WHERE
		ID = @FolderId
END
GO
IF OBJECT_ID (N'PrivateMessages_SynchronizeMapping', N'P') IS NOT NULL
	DROP PROCEDURE PrivateMessages_SynchronizeMapping
GO

CREATE PROCEDURE [PrivateMessages_SynchronizeMapping]
@MappingId int =0
AS
BEGIN
	SET NOCOUNT ON
	DECLARE 
		@MessageCount int

	IF ( NOT EXISTS ( SELECT * FROM b_PrivateMessageMappings WHERE ID = @MappingId))
		return
		
	CREATE TABLE #Mappings ( TopicId int, UserId int, UnreadCount int )
	
		SELECT @MessageCount = COUNT(*)
		FROM
			b_PrivateMessageMappings PMM join b_PrivateMessages PM on PM.TopicId = PMM.TopicId
		WHERE
			PMM.Id = @MappingId
			AND PM.SentDate>PMM.ReadDate
			AND PM.FromId<>PMM.UserId
			
	UPDATE b_PrivateMessageMappings SET UnreadMessageCount = @MessageCount
	WHERE
		Id = @MappingId
	
END
GO
IF OBJECT_ID (N'PrivateMessages_SynchronizeMappingsByTopic', N'P') IS NOT NULL
	DROP PROCEDURE PrivateMessages_SynchronizeMappingsByTopic
GO

CREATE PROCEDURE [PrivateMessages_SynchronizeMappingsByTopic]
@TopicId int =0
AS
BEGIN
	SET NOCOUNT ON
	DECLARE 
		@MessageCount int

	IF ( NOT EXISTS ( SELECT * FROM b_PrivateMessageTopics WHERE ID = @TopicId))
		return
		
	CREATE TABLE #Mappings ( TopicId int, UserId int, UnreadCount int )
	
	INSERT #Mappings ( TopicId,UserId,UnreadCount )
		SELECT PMM.TopicId,PMM.UserId,COUNT(*)
		FROM
			b_PrivateMessageMappings PMM join b_PrivateMessages PM on PM.TopicId = PMM.TopicId
		WHERE
			PMM.TopicId = @TopicId
			AND PM.SentDate>PMM.ReadDate
			AND PM.FromId<>PMM.UserId
		GROUP BY 
			PMM.TopicId,PMM.UserId
			
	UPDATE PMM SET UnreadMessageCount = M.UnreadCount
		FROM 
			b_PrivateMessageMappings PMM join
			#Mappings M on PMM.UserId = M.UserId AND PMM.TopicId = M.TopicId
		
	DROP TABLE #Mappings
	
END
GO

IF OBJECT_ID (N'PrivateMessages_SynchronizeTopic', N'P') IS NOT NULL
	DROP PROCEDURE PrivateMessages_SynchronizeTopic
GO

CREATE PROCEDURE [PrivateMessages_SynchronizeTopic]
@TopicId int =0
AS
BEGIN
	SET NOCOUNT ON
	DECLARE 
		@LastMessageDate	datetime,
		@LastMessageId		bigint,
		@MessageCount		int,
		@LastPosterId		int,
		@LastPosterName		nvarchar(256)
	
	IF ( NOT EXISTS ( SELECT * FROM b_PrivateMessageTopics WHERE ID = @TopicId))
		return
		
	SELECT @MessageCount = COUNT(*) FROM b_PrivateMessages WHERE TopicId = @TopicId
	
	SELECT TOP 1 
		@LastMessageDate = SentDate,
		@LastMessageId = ID,
		@LastPosterId = FromId,
		@LastPosterName = FromUserName
	FROM
		b_PrivateMessages
	WHERE
		TopicId = @TopicId
	ORDER BY ID Desc 
	
	
	UPDATE b_PrivateMessageTopics
	SET	
		LastPosterId = ISNULL(@LastPosterId,0),
		LastMessageId = ISNULL(@LastMessageId,0),
		LastMessageDate = @LastMessageDate,
		LastPosterName = ISNULL(@LastPosterName,''),
		MessageCount = @MessageCount
	WHERE
		ID = @TopicId
END	
GO

IF OBJECT_ID (N'PrivateMessages_SynchronizeUser', N'P') IS NOT NULL
	DROP PROCEDURE PrivateMessages_SynchronizeUser
GO

CREATE PROCEDURE [PrivateMessages_SynchronizeUser]
@UserId int =0
AS
BEGIN
	
	DECLARE 
		@MessageCount		int

	
	IF ( NOT EXISTS ( SELECT * FROM b_PrivateMessageUser WHERE ID = @UserId))
		return
		
	SELECT @MessageCount = COUNT(*) 
	FROM 
		b_PrivateMessages PM
		JOIN b_PrivateMessageMappings PMM on PMM.TopicId = PM.TopicId AND PMM.UserId = @UserId 
	WHERE 
		FromId = @UserId
		AND	PMM.Deleted = 0 

	UPDATE b_PrivateMessageUser
	SET	
		MessageCount = @MessageCount
	WHERE
		ID = @UserId
END
GO










