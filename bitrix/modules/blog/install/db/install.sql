IF OBJECT_ID (N'b_BlogCategory', N'U') IS NULL
	CREATE TABLE b_BlogCategory
	(
		ID INT IDENTITY(1,1) NOT NULL,
		Name NVARCHAR(256) NOT NULL,
		Sort INT NOT NULL,
		XmlId VARCHAR(256) NULL,
		CONSTRAINT PK_b_BlogCategory PRIMARY KEY(ID ASC)
	)
GO

IF OBJECT_ID (N'b_BlogCategoryInSite', N'U') IS NULL
	CREATE TABLE b_BlogCategoryInSite
	(
		CategoryId INT NOT NULL,
		SiteId VARCHAR(50) NOT NULL,
		CONSTRAINT PK_b_BlogCategoryInSite PRIMARY KEY (CategoryId ASC, SiteId ASC),
		CONSTRAINT FK_b_BlogCategoryInSite_b_BlogCategory FOREIGN KEY(CategoryId) REFERENCES b_BlogCategory (ID),
		CONSTRAINT FK_b_BlogCategoryInSite_b_Site FOREIGN KEY(SiteId) REFERENCES b_Site (ID)
	)
GO

IF OBJECT_ID (N'b_BlogUser', N'U') IS NULL
	CREATE TABLE b_BlogUser
	(
		ID INT NOT NULL,
		PostCount INT NOT NULL,
		CONSTRAINT PK_b_BlogUser PRIMARY KEY (ID ASC),
		CONSTRAINT FK_b_BlogUser_b_Users FOREIGN KEY(ID) REFERENCES b_Users (ID)
	) 
GO

IF OBJECT_ID (N'b_Blog', N'U') IS NULL
	CREATE TABLE b_Blog
	(
		ID INT IDENTITY(1,1) NOT NULL,
		Name NVARCHAR(256) NOT NULL,
		Description NVARCHAR(2048) NULL,
		OwnerId INT NOT NULL,
		Active BIT NOT NULL,
		DateCreated DATETIME NOT NULL,
		DateLastPosted DATETIME NOT NULL,
		Slug NVARCHAR(128) NOT NULL,
		Sort INT NOT NULL,
		NotifyOfComments BIT NOT NULL,
		XmlId VARCHAR(256) NULL,
		PostCount INT NULL,
		CommentCount INT NULL,
		IndexContent int NOT NULL CONSTRAINT DF_b_Blog_IndexContent DEFAULT(1),
		IsTeam BIT NOT NULL CONSTRAINT DF_b_Blog_IsTeam DEFAULT (0),
		CONSTRAINT PK_b_Blog PRIMARY KEY (ID ASC),
		CONSTRAINT FK_b_Blog_b_BlogUser FOREIGN KEY(OwnerId) REFERENCES b_BlogUser (ID)
	)
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_Blog_OwnerId')
	CREATE INDEX IX_b_Blog_OwnerId ON b_Blog (OwnerId ASC, Active ASC)
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_Blog_Active')
	CREATE INDEX IX_b_Blog_Active ON b_Blog (Active ASC, ID ASC)
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_Blog_PostCountList')
	CREATE INDEX IX_b_Blog_PostCountList ON b_Blog (PostCount DESC, Name ASC, Active ASC) 
	INCLUDE (ID, Description, OwnerId, DateCreated, DateLastPosted, Slug, Sort, NotifyOfComments, XmlId, CommentCount)
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_Blog_NewBlogList')
	CREATE INDEX IX_b_Blog_NewBlogList ON b_Blog (DateCreated DESC, Name ASC, Active ASC) 
	INCLUDE (ID, Description, OwnerId, DateLastPosted, Slug, Sort, NotifyOfComments, XmlId, PostCount, CommentCount)
GO

IF OBJECT_ID (N'b_BlogInCategory', N'U') IS NULL
	CREATE TABLE b_BlogInCategory
	(
		BlogId INT NOT NULL,
		CategoryId INT NOT NULL,
		CONSTRAINT PK_b_BlogInCategory PRIMARY KEY (BlogId ASC, CategoryId ASC),
		CONSTRAINT FK_b_BlogInCategory_b_Blog FOREIGN KEY(BlogId) REFERENCES b_Blog (ID),
		CONSTRAINT FK_b_BlogInCategory_b_BlogCategory FOREIGN KEY(CategoryId) REFERENCES b_BlogCategory (ID)
	)
GO

IF OBJECT_ID (N'b_BlogPost', N'U') IS NULL
	CREATE TABLE b_BlogPost
	(
		ID INT IDENTITY(1,1) NOT NULL,
		BlogId INT NOT NULL,
		AuthorId INT NOT NULL,
		AuthorName NVARCHAR(256) NOT NULL,
		AuthorEmail NVARCHAR(320) NULL,
		AuthorIP VARCHAR(64) NULL,
		Title NVARCHAR(512) NULL,
		Contents NVARCHAR(max) NULL,
		DateCreated DATETIME NOT NULL,
		IsPublished BIT NOT NULL,
		DatePublished DATETIME NOT NULL,
		DateUpdated DATETIME NOT NULL,
		NotifyOfComments BIT NOT NULL,
		CommentCount INT NOT NULL,
		ViewCount INT NOT NULL,
		XmlId VARCHAR(256) NULL,
		Tags nvarchar(1024) NULL,
		ContentType int NOT NULL CONSTRAINT DF_b_BlogPost_ContentType DEFAULT (0),
		Flags int NOT NULL CONSTRAINT DF_b_BlogPost_Flags DEFAULT (0),
		CONSTRAINT PK_b_BlogPost PRIMARY KEY (ID ASC),
		CONSTRAINT FK_b_BlogPost_b_Blog FOREIGN KEY(BlogId) REFERENCES b_Blog (ID),
		CONSTRAINT FK_b_Post_b_BlogUser FOREIGN KEY(AuthorId) REFERENCES b_BlogUser (ID)
	)
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_BlogPost_PostList')
	CREATE INDEX IX_b_BlogPost_PostList ON b_BlogPost (BlogId ASC, IsPublished ASC, DatePublished ASC) INCLUDE (ID)
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_BlogPost_NewPosts')
	CREATE INDEX IX_b_BlogPost_NewPosts ON b_BlogPost (IsPublished ASC, DatePublished ASC, BlogId ASC, ID ASC) 
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_BlogPost_PopularPosts')
	CREATE INDEX IX_b_BlogPost_PopularPosts ON b_BlogPost (ViewCount ASC, DatePublished ASC, IsPublished ASC, ID ASC, BlogId ASC)
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_BlogPost_CommentedPosts')
	CREATE INDEX IX_b_BlogPost_CommentedPosts ON b_BlogPost (IsPublished ASC, DatePublished ASC, CommentCount ASC, BlogId ASC, ID ASC)
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_BlogPost_BlogId')
	CREATE INDEX IX_b_BlogPost_BlogId ON b_BlogPost (BlogId ASC)
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_BlogPost_AuthorId')
	CREATE INDEX IX_b_BlogPost_AuthorId ON b_BlogPost (AuthorId ASC)
GO

IF OBJECT_ID (N'b_BlogComment', N'U') IS NULL
	CREATE TABLE b_BlogComment
	(
		ID INT IDENTITY(1,1) NOT NULL,
		PostId INT NOT NULL,
		BlogId INT NOT NULL,
		ParentId INT NOT NULL,
		AuthorId INT NOT NULL,
		AuthorName NVARCHAR(256) NOT NULL,
		AuthorEmail NVARCHAR(320) NULL,
		AuthorWebsite NVARCHAR(2048) NULL,
		AuthorIP VARCHAR(64) NULL,
		Contents NVARCHAR(max) NULL,
		DateCreated DATETIME NOT NULL,
		DateModified DATETIME NOT NULL,
		IsApproved BIT NOT NULL,
		MarkedForDelete BIT NOT NULL,
		LeftMargin INT NOT NULL,
		RightMargin INT NOT NULL,
		DepthLevel INT NOT NULL,
		RootNodeIndex INT NOT NULL,
		LiveRootNodeIndex INT NULL,
		XmlId VARCHAR(256) NULL,
		AuthorBlogId INT NOT NULL,
		CONSTRAINT PK_b_BlogComment PRIMARY KEY (ID ASC),
		CONSTRAINT FK_b_BlogComment_b_Blog FOREIGN KEY(BlogId) REFERENCES b_Blog(ID),
		CONSTRAINT FK_b_BlogComment_b_BlogPost FOREIGN KEY(PostId) REFERENCES b_BlogPost(ID)
	) 
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_BlogComment_CommentList')
	CREATE INDEX IX_b_BlogComment_CommentList ON b_BlogComment (PostId ASC, MarkedForDelete ASC, ID ASC) INCLUDE (ParentId, LiveRootNodeIndex) 
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_BlogComment_CommentTree')
	CREATE INDEX IX_b_BlogComment_CommentTree ON b_BlogComment (PostId ASC, LiveRootNodeIndex ASC, AuthorId ASC, AuthorBlogId ASC, LeftMargin ASC)
	INCLUDE (
		ID, 
		BlogId, 
		ParentId, 
		AuthorName, 
		AuthorEmail, 
		AuthorWebsite, 
		AuthorIP, 
		Contents, 
		DateCreated, 
		DateModified, 
		IsApproved,
		MarkedForDelete,
		RightMargin,
		DepthLevel,
		RootNodeIndex,
		XmlId
	)
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_BlogComment_BlogId')
	CREATE INDEX IX_b_BlogComment_BlogId ON b_BlogComment (MarkedForDelete ASC, BlogId ASC)
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_BlogComment_AuthorBlogId')
	CREATE INDEX IX_b_BlogComment_AuthorBlogId ON b_BlogComment (AuthorBlogId ASC)
GO

IF OBJECT_ID (N'b_BlogFile', N'U') IS NULL
	CREATE TABLE [b_BlogFile]
	(
		ID int NOT NULL,
		UserId int NOT NULL,
		BlogId int NOT NULL,
		PostId int NULL,
		FileName nvarchar(255) NULL,
		Folder nvarchar(255) NULL,
		FileSize int NULL,
		CONSTRAINT [PK_b_BlogFile] PRIMARY KEY CLUSTERED([ID] ASC),
		CONSTRAINT [FK_b_BlogFile_b_Blog] FOREIGN KEY([BlogId]) REFERENCES [b_Blog] ([ID]),
		CONSTRAINT [FK_b_BlogFile_b_BlogPost] FOREIGN KEY([PostId]) REFERENCES [b_BlogPost] ([ID])
	)
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_BlogFile_Blog_Post')
	CREATE INDEX IX_b_BlogFile_Blog_Post ON b_BlogFile ([BlogId] ASC, [PostId] ASC) INCLUDE([ID])
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_BlogFile_User')
	CREATE INDEX IX_b_BlogFile_User ON b_BlogFile ([UserId] ASC) INCLUDE([ID])
GO

IF OBJECT_ID (N'b_BlogSyndication', N'U') IS NULL
	CREATE TABLE [b_BlogSyndication](
		BlogId int NOT NULL,
		Enabled bit NOT NULL,
		FeedUrl nvarchar(2083) NOT NULL,
		Updateable bit NOT NULL,
		RedirectToComments bit NOT NULL,
	 CONSTRAINT [PK_b_BlogSyndication] PRIMARY KEY CLUSTERED([BlogId] ASC),
	 CONSTRAINT [FK_b_BlogSyndication_b_Blog] FOREIGN KEY([BlogId]) REFERENCES [b_Blog] ([ID])
	)
GO

IF OBJECT_ID (N'b_BlogPostSyndication', N'U') IS NULL
	CREATE TABLE [b_BlogPostSyndication](
		PostId int NOT NULL,
		BlogId int NOT NULL,
		[Hash] int NOT NULL DEFAULT 0,
		[Guid] nvarchar(MAX) NOT NULL,
		IsPermaLinkGuid bit NOT NULL DEFAULT 0,
		PostUrl nvarchar(2083) NOT NULL,
		CommentsUrl nvarchar(2083) NOT NULL,
		CONSTRAINT [PK_b_BlogPostSyndication] PRIMARY KEY CLUSTERED(PostId ASC),
		CONSTRAINT [FK_b_BlogPostSyndication_b_Blog] FOREIGN KEY([BlogId]) REFERENCES [b_Blog] ([ID]),
		CONSTRAINT [FK_b_BlogPostSyndication_b_BlogPost] FOREIGN KEY([PostId]) REFERENCES [b_BlogPost] ([ID])		
	)
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_BlogPostSyndication_BlogId_Hash')
	CREATE NONCLUSTERED INDEX [IX_b_BlogPostSyndication_BlogId_Hash] ON [b_BlogPostSyndication] (BlogId ASC, [Hash] ASC) INCLUDE ( [PostId])
GO

IF OBJECT_ID (N'Blog_SynchronizeBlog', N'P') IS NOT NULL
	DROP PROCEDURE Blog_SynchronizeBlog
GO

CREATE PROCEDURE Blog_SynchronizeBlog 
	@BlogId int
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @DateLastPosted datetime, @Now datetime, @PostCount int, @CommentCount int;	
	SET @Now = GETDATE(); 
	
	IF (@BlogId IS NULL) RETURN;

	SELECT @PostCount = COUNT(ID) FROM b_BlogPost WHERE BlogId = @BlogId AND IsPublished = 1 AND DatePublished <= @Now;
	IF(@PostCount > 0)
	BEGIN
		SELECT TOP 1 @DateLastPosted = DatePublished FROM b_BlogPost WHERE BlogId = @BlogId AND IsPublished = 1 AND DatePublished <= @Now ORDER BY DatePublished DESC;
		SELECT @CommentCount = COUNT(ID) FROM b_BlogComment WHERE BlogId = @BlogId AND MarkedForDelete = 0 AND IsApproved = 1;
		UPDATE b_Blog SET DateLastPosted = @DateLastPosted, PostCount = @PostCount, CommentCount = @CommentCount WHERE ID = @BlogId;
	END
	ELSE
	BEGIN
		UPDATE b_Blog SET DateLastPosted = DateCreated, PostCount = 0, CommentCount = 0 WHERE ID = @BlogId;
	END	
END
GO

IF OBJECT_ID (N'BlogCategory_DeleteBlogInCategory', N'P') IS NOT NULL
	DROP PROCEDURE BlogCategory_DeleteBlogInCategory
GO

CREATE PROCEDURE BlogCategory_DeleteBlogInCategory
	@CategoryId int
AS
BEGIN
	DELETE FROM b_BlogInCategory WHERE CategoryId = @CategoryId;	
END
GO

IF OBJECT_ID (N'BlogComment_SynchronizeNestedSets', N'P') IS NOT NULL
	DROP PROCEDURE BlogComment_SynchronizeNestedSets
GO

CREATE PROCEDURE BlogComment_SynchronizeNestedSets
	@Id int,
	@Mode int = 1
AS
BEGIN
SET NOCOUNT ON;
/*
@Mode = 1 - process addition;
@Mode = 2 - process deletion;
@Mode = 3 - process modification;
*/
declare @postId int, @currentLeft int, @currentRight int, @currentDepth int, @currentParentId int, @currentRootIndex int, @currentLiveRootIndex int, @currentMarkedForDelete bit, @currentLiveNestedCount int, @parentLeft int, @parentRight int, @parentDepth int, @parentRootIndex int, @parentLiveRootIndex int, @parentMarkedForDelete bit, @parentParentId int;


if @Mode < 1 OR @Mode > 3 return;
--set @currentLiveRootIndex = 0;
--set @currentLiveNestedCount = 0;
if @Mode = 1 --addition
begin
	select top(1) @postId = c.PostId, @currentMarkedForDelete = c.MarkedForDelete, @currentParentId = c.ParentId from [b_BlogComment] c where c.ID = @Id;

	if @currentParentId = 0
	begin
		set @currentDepth = 1;
		
		select top(1) @currentLeft = c.RightMargin + 1, @currentRootIndex = c.RootNodeIndex + 1 from [b_BlogComment] c where c.ID <> @Id and c.LeftMargin > 0 AND c.DepthLevel = 1 and c.PostId = @postId order by c.RightMargin desc;
				
		if @currentLeft is NULL
			set @currentLeft = 1;

		if @currentRootIndex is NULL
			set @currentRootIndex = 1;

		set @currentRight = @currentLeft + 1;
		
		--LiveRootNodeIndex
		if @currentMarkedForDelete = 0
			select top(1) @currentLiveRootIndex = c.LiveRootNodeIndex + 1 from [b_BlogComment] c where c.ID <> @Id and c.DepthLevel = 1 and c.PostId = @postId and c.MarkedForDelete = 0 order by c.RightMargin desc;		

		if(@currentLiveRootIndex is NULL)
			set @currentLiveRootIndex = 1;			
	end
	else
	begin
		select top(1) @parentLeft = c.LeftMargin, @parentRight = c.RightMargin, @parentDepth = c.DepthLevel, @parentRootIndex = c.RootNodeIndex, @parentLiveRootIndex = c.LiveRootNodeIndex from [b_BlogComment] c where c.ID = @currentParentId and c.PostId = @postId;	
		set @currentDepth = @parentDepth + 1;
		
		set @currentLeft = @parentRight;
		set @currentRight = @currentLeft + 1;
		set @currentRootIndex = @parentRootIndex;
		set @currentLiveRootIndex = @parentLiveRootIndex;
		
		
		update [b_BlogComment] set LeftMargin = LeftMargin + 2 where LeftMargin > @parentRight and PostId= @postId;
		update [b_BlogComment] set RightMargin = RightMargin + 2 where RightMargin >= @parentRight and PostId= @postId;	
	end

	update [b_BlogComment] set LeftMargin = @currentLeft, RightMargin = @currentRight, DepthLevel = @currentDepth, RootNodeIndex = @currentRootIndex, LiveRootNodeIndex = @currentLiveRootIndex where ID = @Id;
end
else if @Mode = 2 --deletion
begin
	if not exists (select c.ID from [b_BlogComment] c where c.ID = @Id)
		return;
		
	select top(1) @postId = c.PostId, @currentMarkedForDelete = c.MarkedForDelete, @currentLeft = c.LeftMargin, @currentRight = c.RightMargin, @currentDepth = c.DepthLevel, @currentRootIndex = c.RootNodeIndex, @currentLiveRootIndex = c.LiveRootNodeIndex, @currentParentId = c.ParentId  from [b_BlogComment] c where c.ID = @Id;	
	if @postId = 0 or @currentLeft = 0 or @currentRight = 0 or @currentDepth = 0 or @currentRootIndex = 0
		return;

	update [b_BlogComment] set LeftMargin = 0, RightMargin = 0, DepthLevel = 0, RootNodeIndex = 0 where ID = @Id;

	--RootNodeIndex
	--process inner sets (deletion nested sests)
	--update [b_BlogComment] set LeftMargin = 0, RightMargin = 0, DepthLevel = 0, RootNodeIndex = 0 where LeftMargin > @currentLeft and RightMargin < @currentRight and PostId = @postId;
	delete from [b_BlogComment] where LeftMargin > @currentLeft and RightMargin < @currentRight and PostId = @postId;
	--process outer sets
	declare @shift int;
	set @shift = @currentRight - @currentLeft + 1;
	update [b_BlogComment] set LeftMargin = LeftMargin - @shift, RightMargin = RightMargin - @shift where LeftMargin > @currentRight and RightMargin > @currentRight and PostId = @postId;
	update [b_BlogComment] set RightMargin = RightMargin - @shift where LeftMargin < @currentLeft and RightMargin > @currentRight and PostId = @postId;	
	--root node index shift
	if @currentDepth = 1
		update [b_BlogComment] set RootNodeIndex = RootNodeIndex - 1 where RootNodeIndex > @currentRootIndex and PostId = @postId;		
		
	--LiveRootNodeIndex
	if @currentLiveRootIndex > 0
	begin
		if @currentDepth = 1
			update [b_BlogComment] set LiveRootNodeIndex = LiveRootNodeIndex - 1 where LiveRootNodeIndex > @currentLiveRootIndex and PostId = @postId;
		else
		begin
			if @currentParentId is null 
				set @currentParentId = 0;
			
			while @currentParentId > 0
			begin
				set @parentParentId = null;
				select top(1) @parentParentId = c.ParentId from [b_BlogComment] c where c.ID = @currentParentId and c.PostId = @postId;			
				if @parentParentId is null
					set @currentParentId = 0;
				else
				begin
					set @Id = @currentParentId;
					set @currentParentId = @parentParentId;		
				end
			end
			
			select top(1) @currentLeft = c.LeftMargin, @currentRight = c.RightMargin, @currentLiveRootIndex = c.LiveRootNodeIndex from [b_BlogComment] c where c.ID = @Id;
			
			update [b_BlogComment] set LiveRootNodeIndex = 0 where ID = @Id;
			update [b_BlogComment] set LiveRootNodeIndex = 0 where LeftMargin > @currentLeft and RightMargin < @currentRight and PostId = @postId;
			update [b_BlogComment] set LiveRootNodeIndex = LiveRootNodeIndex - 1 where LiveRootNodeIndex > @currentLiveRootIndex and PostId = @postId;						
		end
	end			
end
else --modification
begin
	--print 'modification mode'
	select top(1) @postId = c.PostId, @currentParentId = c.ParentId from [b_BlogComment] c where c.ID = @Id;
	
	if @postId is null return;	
	if @currentParentId is null 
		set @currentParentId = 0;
	
	while @currentParentId > 0
	begin
		set @parentParentId = null;
		select top(1) @parentParentId = c.ParentId from [b_BlogComment] c where c.ID = @currentParentId and c.PostId = @postId;			
		if @parentParentId is null
			set @currentParentId = 0;
		else
		begin
			set @Id = @currentParentId;
			set @currentParentId = @parentParentId;		
		end
	end
	
	--print '@Id = ' + cast(@Id as varchar(20));				
	--print '@currentParentId = ' + cast(@currentParentId as varchar(20));				

	select top(1) @currentLeft = c.LeftMargin, @currentRight = c.RightMargin, @currentMarkedForDelete = c.MarkedForDelete, @currentLiveRootIndex = c.LiveRootNodeIndex from [b_BlogComment] c where c.ID = @Id;
	--print '@currentLiveRootIndex = ' + cast(@currentLiveRootIndex as varchar(20));

	--LiveRootNodeIndex
	if @currentMarkedForDelete = 1
	begin
		select @currentLiveNestedCount = count(c.ID) from [b_BlogComment] c where c.PostId = @postId and c.LeftMargin > @currentLeft and c.RightMargin < @currentRight and c.MarkedForDelete = 0;					
		--if(@currentLiveNestedCount is NULL)
		--	set @currentLiveNestedCount = 0;			
	end	
	
	if(@currentLiveNestedCount is NULL)
		set @currentLiveNestedCount = 0;
		
	--print '@currentMarkedForDelete = ' + cast(@currentMarkedForDelete as varchar(20));				
	--print '@currentLiveNestedCount = ' + cast(@currentLiveNestedCount as varchar(20));


	if @currentMarkedForDelete = 1 and (@currentLiveNestedCount is null or @currentLiveNestedCount = 0)
	begin
		if @currentLiveRootIndex > 0
		begin
			--print 'LiveRootIndex to 0';

			update [b_BlogComment] set LiveRootNodeIndex = 0 where ID = @Id;
			update [b_BlogComment] set LiveRootNodeIndex = 0 where LeftMargin > @currentLeft and RightMargin < @currentRight and PostId = @postId;
			update [b_BlogComment] set LiveRootNodeIndex = LiveRootNodeIndex - 1 where LiveRootNodeIndex > @currentLiveRootIndex and PostId = @postId;			
		end
	end
	else if @currentLiveRootIndex = 0
	begin
		with NewLiveRootIndexes as (select c.ID, c.Contents, c.RootNodeIndex, c.LiveRootNodeIndex, ROW_NUMBER() over(order by c.RootNodeIndex asc) NewLiveRootNodeIndex from b_BlogComment c where c.PostId = @postId and c.DepthLevel = 1 and (c.MarkedForDelete = 0 or exists(select ID from b_BlogComment where MarkedForDelete = 0 and LeftMargin > c.LeftMargin and RightMargin < c.RightMargin)))
			update b_BlogComment set LiveRootNodeIndex = NewLiveRootIndexes.NewLiveRootNodeIndex from NewLiveRootIndexes where NewLiveRootIndexes.ID = b_BlogComment.ID;
		with NewNestedLiveRootIndexes as (select c.ID, c.PostId, c.RootNodeIndex, c.LiveRootNodeIndex from b_BlogComment c where c.PostId = @postId and c.DepthLevel = 1 and (c.MarkedForDelete = 0 or exists(select ID from b_BlogComment where MarkedForDelete = 0 and LeftMargin > c.LeftMargin and RightMargin < c.RightMargin)))
			update b_BlogComment set b_BlogComment.LiveRootNodeIndex = NewNestedLiveRootIndexes.LiveRootNodeIndex from NewNestedLiveRootIndexes where b_BlogComment.DepthLevel > 1 and b_BlogComment.PostId = NewNestedLiveRootIndexes.PostId and b_BlogComment.RootNodeIndex = NewNestedLiveRootIndexes.RootNodeIndex;		
	end	
end
END
GO

IF OBJECT_ID (N'BlogPost_SynchronizePost', N'P') IS NOT NULL
	DROP PROCEDURE BlogPost_SynchronizePost
GO

CREATE PROCEDURE BlogPost_SynchronizePost
	@PostId int,
	@BlogId int = NULL	
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @CommentCount int;
	IF (@PostId IS NULL) RETURN;
	IF (@BlogId IS NULL) SELECT @BlogId = BlogId FROM b_BlogPost WHERE ID = @PostId;
	IF (@BlogId IS NULL) RETURN;
	SELECT @CommentCount = COUNT(ID) FROM b_BlogComment WHERE PostId = @PostId AND MarkedForDelete = 0 AND IsApproved = 1;
	UPDATE b_BlogPost SET CommentCount = @CommentCount WHERE ID = @PostId AND BlogId = @BlogId;
END
GO

IF OBJECT_ID (N'BlogUser_Synchronize', N'P') IS NOT NULL
	DROP PROCEDURE BlogUser_Synchronize
GO

CREATE PROCEDURE BlogUser_Synchronize
	@UserId int,
	@BlogId int
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @PostCount int;
	SELECT @PostCount = COUNT(p.ID) FROM [b_BlogPost] p WHERE p.AuthorId = @UserId;
	
	UPDATE  [b_BlogUser] SET PostCount = @PostCount WHERE ID = @UserId;
END
GO

IF OBJECT_ID (N'BlogFile_Synchronize', N'P') IS NOT NULL
	DROP PROCEDURE BlogFile_Synchronize
GO

CREATE PROCEDURE BlogFile_Synchronize
	@Id int
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE [b_BlogFile] SET [FileName] = f.[FileName], [Folder] = f.[Folder], [FileSize] = f.[FileSize] FROM b_File as f WHERE [b_BlogFile].[ID] = f.ID AND [b_BlogFile].[ID] = @Id;
END
GO

IF OBJECT_ID (N'BlogFile_Delete', N'P') IS NOT NULL
	DROP PROCEDURE BlogFile_Delete
GO

CREATE PROCEDURE BlogFile_Delete
	@Id int = 0,
	@PostId int = 0,
	@BlogId int = 0
AS
BEGIN
	SET NOCOUNT ON;
	IF @Id IS NOT NULL AND @Id > 0
	BEGIN
		DELETE FROM [b_File] WHERE ID = @Id;
		DELETE FROM [b_BlogFile] WHERE ID = @Id;
	END
	ELSE IF @PostId IS NOT NULL AND @PostId > 0
	BEGIN
		DELETE FROM [b_File] WHERE ID IN (SELECT f.[ID] FROM b_BlogFile f WHERE f.[PostId] = @PostId);
		DELETE FROM [b_BlogFile] WHERE [PostId] = @PostId;
	END	
	ELSE IF @BlogId IS NOT NULL AND @BlogId > 0
	BEGIN
		DELETE FROM [b_File] WHERE ID IN (SELECT f.[ID] FROM b_BlogFile f WHERE f.[BlogId] = @BlogId);
		DELETE FROM [b_BlogFile] WHERE [BlogId] = @BlogId;		
	END
END
GO

IF OBJECT_ID (N'BlogFile_DeleteFiles', N'P') IS NOT NULL
	DROP PROCEDURE BlogFile_DeleteFiles
GO

CREATE PROCEDURE BlogFile_DeleteFiles
	@Id int = 0,
	@IdXml xml = null, --<file id="307"></file>
	@PostId int = 0,
	@BlogId int = 0
AS
BEGIN
	SET NOCOUNT ON;
	IF @Id IS NOT NULL AND @Id > 0
		DELETE FROM [b_File] WHERE ID = @Id;
	ELSE IF @IdXml IS NOT NULL
		DELETE FROM [b_File] WHERE ID IN (SELECT t.c.value(N'@id', N'int') FROM @IdXml.nodes(N'/file') t(c));	
	ELSE IF @PostId IS NOT NULL AND @PostId > 0
		DELETE FROM [b_File] WHERE ID IN (SELECT f.[ID] FROM b_BlogFile f WHERE f.[PostId] = @PostId);
	ELSE IF @BlogId IS NOT NULL AND @BlogId > 0
		DELETE FROM [b_File] WHERE ID IN (SELECT f.[ID] FROM b_BlogFile f WHERE f.[BlogId] = @BlogId);		
END
GO

IF OBJECT_ID (N'BlogFile_Save', N'P') IS NOT NULL
	DROP PROCEDURE BlogFile_Save
GO

CREATE PROCEDURE BlogFile_Save
	@Id int,
	@UserId int = NULL,
	@BlogId int = NULL,
	@PostId int = NULL,
	@Synchronize bit = 0
AS
BEGIN
	SET NOCOUNT ON;
	IF NOT EXISTS(SELECT N'X' FROM [b_BlogFile] WHERE [ID] = @Id)
		INSERT INTO [b_BlogFile]([ID], [UserId], [BlogId], [PostId]) VALUES(@Id, @UserId, @BlogId, @PostId);
	ELSE
		UPDATE [b_BlogFile] SET [UserId] = @UserId, [BlogId] = @BlogId, [PostId] = @PostId WHERE [ID] = @Id;
	IF @@ROWCOUNT > 0 AND @Synchronize = 1
		EXECUTE [BlogFile_Synchronize] @Id = @Id;
END
GO

IF OBJECT_ID(N'BlogPost_IncrementViewCount', N'P') IS NOT NULL
	DROP PROCEDURE BlogPost_IncrementViewCount;
GO
CREATE PROCEDURE BlogPost_IncrementViewCount 
	@PostId int
AS
BEGIN
	SET NOCOUNT ON;
	IF @PostId IS NULL OR @PostId <= 0 RETURN;
	UPDATE b_BlogPost SET ViewCount = ViewCount + 1 WHERE ID = @PostId;
END
GO

IF OBJECT_ID(N'BlogPostSyndication_Synchronize', N'P') IS NOT NULL
	DROP PROCEDURE BlogPostSyndication_Synchronize;
GO
CREATE PROCEDURE BlogPostSyndication_Synchronize
	@PostId int
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE b_BlogPostSyndication SET [Hash] = CHECKSUM([Guid]) WHERE PostId = @PostId;

END
GO

IF OBJECT_ID(N'BlogPostSyndication_FindByGuidAndBlogId', N'P') IS NOT NULL
	DROP PROCEDURE BlogPostSyndication_FindByGuidAndBlogId;
GO

CREATE PROCEDURE BlogPostSyndication_FindByGuidAndBlogId
	@BlogId int,
	@Guid nvarchar(MAX)
AS
BEGIN
	SET NOCOUNT ON;
	SELECT [PostId], [BlogId], [Guid], [PostUrl], [CommentsUrl] FROM [b_BlogPostSyndication] WHERE [BlogId] = @BlogId AND [Hash] = CHECKSUM(@Guid) AND [Guid] = @Guid;
END
GO

IF OBJECT_ID(N'BlogPostSyndication_Delete', N'P') IS NOT NULL
	DROP PROCEDURE BlogPostSyndication_Delete;
GO

CREATE PROCEDURE BlogPostSyndication_Delete
	@BlogId int,
	@PostId int
AS
BEGIN
	SET NOCOUNT ON;
	IF @PostId IS NOT NULL AND @PostId > 0
		DELETE FROM [b_BlogPostSyndication] WHERE [BlogId] = @BlogId AND [PostId] = @PostId;
	ELSE
		DELETE FROM [b_BlogPostSyndication] WHERE [BlogId] = @BlogId;
END
GO

IF OBJECT_ID(N'BlogSyndication_Delete', N'P') IS NOT NULL
	DROP PROCEDURE BlogSyndication_Delete;
GO

CREATE PROCEDURE BlogSyndication_Delete
	@BlogId int
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM [b_BlogPostSyndication] WHERE [BlogId] = @BlogId;
	DELETE FROM [b_BlogSyndication] WHERE [BlogId] = @BlogId;
END
GO

/*--- BEGINNING OF RATINGS   ---*/
IF OBJECT_ID (N'b_BlogPost', N'U') IS NOT NULL AND NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_AuthorId_IsPublished_DatePublished')
	CREATE NONCLUSTERED INDEX IX_AuthorId_IsPublished_DatePublished ON b_BlogPost(
		AuthorId ASC,
		IsPublished ASC,
		DatePublished ASC
		)
	INCLUDE(ID); 
GO

IF OBJECT_ID (N'b_BlogComment', N'U') IS NOT NULL AND NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_BlogComment_AuthorId_MarkedForDelete_DateCreated')
	CREATE NONCLUSTERED INDEX IX_b_BlogComment_AuthorId_MarkedForDelete_DateCreated ON b_BlogComment (
		AuthorId ASC,
		MarkedForDelete ASC,
		DateCreated ASC
	)
	INCLUDE(ID);
GO

IF OBJECT_ID (N'b_BloggingActivity', N'U') IS NULL
BEGIN
	CREATE TABLE b_BloggingActivity
	(
		ID INT IDENTITY(1,1) NOT NULL,
		RatingId INT NOT NULL,
		Active BIT NOT NULL,
		UserId INT NOT NULL,
		CreatedUtc DATETIME NOT NULL,
		LastCalculatedUtc DATETIME NOT NULL,
		IsCalculated BIT NOT NULL,
		Value FLOAT(53) NOT NULL,	
		PostValue FLOAT(53) NOT NULL,
		CommentValue FLOAT(53) NOT NULL,
		TodayPostCoef FLOAT(53) NOT NULL,		
		WeekPostCoef FLOAT(53) NOT NULL,		
		MonthPostCoef FLOAT(53) NOT NULL,		
		TodayCommentCoef FLOAT(53) NOT NULL,		
		WeekCommentCoef FLOAT(53) NOT NULL,		
		MonthCommentCoef FLOAT(53) NOT NULL,		
		XmlId VARCHAR(256) NULL,
		CONSTRAINT PK_b_BloggingActivity PRIMARY KEY CLUSTERED(ID ASC)
	)
END
GO

IF OBJECT_ID (N'BlogVoting_GetEngagingUsers', N'P') IS NOT NULL
	DROP PROCEDURE BlogVoting_GetEngagingUsers;
GO

CREATE PROCEDURE BlogVoting_GetEngagingUsers
	@RatingId INT,
	@Count INT
AS
BEGIN
	SET NOCOUNT ON;	
	
	IF(@Count IS NULL OR @Count < 0) SET @Count = 5;
	
	SELECT TOP(@Count) b.OwnerId
	FROM b_RatingVoting v 
	INNER JOIN b_Blog b 
		ON v.BoundEntityTypeId = N'BLOG'
			AND v.BoundEntityId = b.ID 
			AND NOT EXISTS(SELECT ID FROM b_RatingCounter c WHERE c.RatingId = @RatingId AND c.BoundEntityTypeId = N'USER' AND c.BoundEntityId = b.OwnerId);
END
GO

IF OBJECT_ID (N'BlogPostVoting_GetEngagingUsers', N'P') IS NOT NULL
	DROP PROCEDURE BlogPostVoting_GetEngagingUsers;
GO

CREATE PROCEDURE BlogPostVoting_GetEngagingUsers
	@RatingId INT,
	@Count INT
AS
BEGIN
	SET NOCOUNT ON;	
	
	IF(@Count IS NULL OR @Count < 0) SET @Count = 5;
	
	SELECT TOP(@Count) b.AuthorId
	FROM b_RatingVoting v 
	INNER JOIN b_BlogPost b 
		ON v.BoundEntityTypeId = N'BLOGPOST'
			AND v.BoundEntityId = b.ID 
			AND NOT EXISTS(SELECT ID FROM b_RatingCounter c WHERE c.RatingId = @RatingId AND c.BoundEntityTypeId = N'USER' AND c.BoundEntityId = b.AuthorId);
END
GO

IF OBJECT_ID (N'BlogCommentVoting_GetEngagingUsers', N'P') IS NOT NULL
	DROP PROCEDURE BlogCommentVoting_GetEngagingUsers;
GO

CREATE PROCEDURE BlogCommentVoting_GetEngagingUsers
	@RatingId INT,
	@Count INT
AS
BEGIN
	SET NOCOUNT ON;	
	
	IF(@Count IS NULL OR @Count < 0) SET @Count = 5;
	
	SELECT TOP(@Count) b.AuthorId
	FROM b_RatingVoting v 
	INNER JOIN b_BlogComment b 
		ON v.BoundEntityTypeId = N'BLOGCOMMENT'
			AND v.BoundEntityId = b.ID 
			AND NOT EXISTS(SELECT ID FROM b_RatingCounter c WHERE c.RatingId = @RatingId AND c.BoundEntityTypeId = N'USER' AND c.BoundEntityId = b.AuthorId);
END
GO


IF OBJECT_ID (N'BloggingActivity_GetPostCountInRange', N'FN') IS NOT NULL
	DROP FUNCTION dbo.BloggingActivity_GetPostCountInRange
GO

CREATE FUNCTION dbo.BloggingActivity_GetPostCountInRange
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
		
	DECLARE @result INT;
	SET @result = (SELECT COUNT(ID) 
		FROM b_BlogPost 
		WHERE IsPublished = 1 AND DatePublished >= @Datetime AND DatePublished <= @Now AND AuthorId = @UserId
		);	
	RETURN @result;
END
GO

IF OBJECT_ID (N'BloggingActivity_GetCommentCountInRange', N'FN') IS NOT NULL
	DROP FUNCTION dbo.BloggingActivity_GetCommentCountInRange
GO

CREATE FUNCTION dbo.BloggingActivity_GetCommentCountInRange
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
		
	DECLARE @result INT;
	SET @result = (SELECT COUNT(C.ID) 
		FROM b_BlogComment C /*INNER JOIN b_BlogPost P ON C.PostId = P.ID AND P.IsPublished = 1*/  
		WHERE C.MarkedForDelete = 0 AND C.DateCreated >= @Datetime AND C.DateCreated <= @Now AND C.AuthorId = @UserId
		);		
	RETURN @result;
END
GO

IF OBJECT_ID (N'BloggingActivity_GetPostValue', N'FN') IS NOT NULL
	DROP FUNCTION dbo.BloggingActivity_GetPostValue
GO
CREATE FUNCTION dbo.BloggingActivity_GetPostValue
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
	WITH post30(ID, DatePublished) AS
	(
		SELECT P.ID, P.DatePublished
		FROM b_BlogPost P INNER JOIN b_Blog B ON P.BlogId = B.ID 
		WHERE B.Active = 1 AND P.IsPublished = 1 AND P.DatePublished >= DATEADD(DAY, -30, @Now) AND P.DatePublished <= @Now AND P.AuthorId = @UserId		
	)
	SELECT @Result = (SELECT COUNT(ID) FROM post30) * @MonthCoef +
		(SELECT COUNT(ID) FROM post30 WHERE DatePublished >= DATEADD(DAY, -7, @Now)) * @WeekCoef + 
		(SELECT COUNT(ID) FROM post30 WHERE DatePublished >= DATEADD(DAY, 0, DATEDIFF(DAY, 0, @Now))) * @TodayCoef;
		
	RETURN @Result;	
END
GO

IF OBJECT_ID (N'BloggingActivity_GetCommentValue', N'FN') IS NOT NULL
	DROP FUNCTION dbo.BloggingActivity_GetCommentValue
GO
CREATE FUNCTION dbo.BloggingActivity_GetCommentValue
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
	WITH comment30(ID, DateCreated) AS
	(
		SELECT C.ID, C.DateCreated
		FROM b_BlogComment C INNER JOIN b_Blog B ON C.BlogId = B.ID INNER JOIN b_BlogPost P ON C.PostId = P.ID AND P.IsPublished = 1
		WHERE b.Active = 1 AND P.IsPublished = 1 AND C.MarkedForDelete = 0 AND C.DateCreated >= DATEADD(DAY, -30, @Now) AND C.DateCreated <= @Now AND C.AuthorId = @UserId	
	)
	SELECT @Result = (SELECT COUNT(ID) FROM comment30) * @MonthCoef +
		(SELECT COUNT(ID) FROM comment30 WHERE DateCreated >= DATEADD(DAY, -7, @Now)) * @WeekCoef + 
		(SELECT COUNT(ID) FROM comment30 WHERE DateCreated >= DATEADD(DAY, 0, DATEDIFF(DAY, 0, @Now))) * @TodayCoef;
		
	RETURN @Result;
END
GO

IF OBJECT_ID (N'BloggingActivity_Synchronize', N'P') IS NOT NULL
	DROP PROCEDURE BloggingActivity_Synchronize;
GO

CREATE PROCEDURE BloggingActivity_Synchronize
	@BloggingActivityId INT,
	@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @PostResult FLOAT(53), @CommentResult FLOAT(53);
		
	BEGIN TRANSACTION;
	SELECT @PostResult = dbo.BloggingActivity_GetPostValue(UserId, @Now, MonthPostCoef, WeekPostCoef, TodayPostCoef),
		@CommentResult = dbo.BloggingActivity_GetCommentValue(UserId, @Now, MonthCommentCoef, WeekCommentCoef, TodayCommentCoef)  
	FROM b_BloggingActivity 
	WHERE ID = @BloggingActivityId;			
			
	UPDATE b_BloggingActivity 
	SET Value = @PostResult + @CommentResult, 
		PostValue = @PostResult, 
		CommentValue = @CommentResult,
		LastCalculatedUtc = GETUTCDATE(),
		IsCalculated = 1 
	WHERE ID = @BloggingActivityId;
	COMMIT TRANSACTION;
		
	SELECT @PostResult + @CommentResult;
END
GO

IF OBJECT_ID (N'BloggingActivity_SynchronizePostByUser', N'P') IS NOT NULL
	DROP PROCEDURE BloggingActivity_SynchronizePostByUser
GO

CREATE PROCEDURE BloggingActivity_SynchronizePostByUser
	@UserId INT,
	@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	IF(@UserId IS NULL OR NOT EXISTS(SELECT N'*' FROM b_BloggingActivity WHERE UserId = @UserId AND Active = 1)) RETURN;
	IF(@Now IS NULL) SET @Now = GETDATE();
	
	DECLARE @monthBeforeCount INT, @weekBeforeCount INT, @todayCount INT;
	BEGIN TRANSACTION;
	SET @monthBeforeCount = dbo.BloggingActivity_GetPostCountInRange(@UserId, @Now, 30);
	SET @weekBeforeCount = dbo.BloggingActivity_GetPostCountInRange(@UserId, @Now, 7);
	SET @todayCount = dbo.BloggingActivity_GetPostCountInRange(@UserId, @Now, 0);	
		
	UPDATE b_BloggingActivity 
	SET Value = Value - PostValue + (@monthBeforeCount * MonthPostCoef + @weekBeforeCount * WeekPostCoef + @todayCount * TodayPostCoef), 
		PostValue = (@monthBeforeCount * MonthPostCoef + @weekBeforeCount * WeekPostCoef + @todayCount * TodayPostCoef), 
		LastCalculatedUtc = GETUTCDATE()
	WHERE UserId = @UserId;

	UPDATE b_RatingCounter SET IsCalculated = 0 
	WHERE RatingId IN (SELECT DISTINCT RatingId FROM b_BloggingActivity WHERE UserId = @UserId) 
		AND BoundEntityId = CAST(@UserId AS NVARCHAR(64));
	
	COMMIT TRANSACTION;
END
GO

IF OBJECT_ID (N'BloggingActivity_SynchronizeCommentByUser', N'P') IS NOT NULL
	DROP PROCEDURE BloggingActivity_SynchronizeCommentByUser
GO

CREATE PROCEDURE BloggingActivity_SynchronizeCommentByUser
	@UserId INT,
	@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	
	IF(@UserId IS NULL OR NOT EXISTS(SELECT N'*' FROM b_BloggingActivity WHERE UserId = @UserId AND Active = 1)) RETURN;
	IF(@Now IS NULL) SET @Now = GETDATE();
	
	DECLARE @monthBeforeCount INT, @weekBeforeCount INT, @todayCount INT;
	BEGIN TRANSACTION;	
	SET @monthBeforeCount = dbo.BloggingActivity_GetCommentCountInRange(@UserId, @Now, 30);
	SET @weekBeforeCount = dbo.BloggingActivity_GetCommentCountInRange(@UserId, @Now, 7);
	SET @todayCount = dbo.BloggingActivity_GetCommentCountInRange(@UserId, @Now, 0);
	
	UPDATE b_BloggingActivity 
	SET 
		Value = Value - CommentValue + (@monthBeforeCount * MonthCommentCoef + @weekBeforeCount * WeekCommentCoef + @todayCount * TodayCommentCoef), 
		CommentValue = (@monthBeforeCount * MonthCommentCoef + @weekBeforeCount * WeekCommentCoef + @todayCount * TodayCommentCoef), 
		LastCalculatedUtc = GETUTCDATE()
	WHERE UserId = @UserId;
	
	UPDATE b_RatingCounter SET IsCalculated = 0 
	WHERE RatingId IN (SELECT DISTINCT RatingId FROM b_BloggingActivity WHERE UserId = @UserId) 
		AND BoundEntityId = CAST(@UserId AS NVARCHAR(64));	
	
	COMMIT TRANSACTION;
END
GO

IF OBJECT_ID (N'BloggingActivity_SynchronizeByUser', N'P') IS NOT NULL
	DROP PROCEDURE BloggingActivity_SynchronizeByUser;
GO

CREATE PROCEDURE BloggingActivity_SynchronizeByUser
	@UserId INT,
	@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	
	IF(@UserId IS NULL OR NOT EXISTS(SELECT N'*' FROM b_BloggingActivity WHERE UserId = @UserId AND Active = 1)) RETURN;
	IF(@Now IS NULL) SET @Now = GETDATE();
	
	DECLARE @monthBeforePostCount INT, @weekBeforePostCount INT, @todayPostCount INT, @monthBeforeCommentCount INT, @weekBeforeCommentCount INT, @todayCommentCount INT;
	BEGIN TRANSACTION;
	SET @monthBeforePostCount = dbo.BloggingActivity_GetPostCountInRange(@UserId, @Now, 30);
	SET @weekBeforePostCount = dbo.BloggingActivity_GetPostCountInRange(@UserId, @Now, 7);
	SET @todayPostCount = dbo.BloggingActivity_GetPostCountInRange(@UserId, @Now, 0);	
		
	SET @monthBeforeCommentCount = dbo.BloggingActivity_GetCommentCountInRange(@UserId, @Now, 30);
	SET @weekBeforeCommentCount = dbo.BloggingActivity_GetCommentCountInRange(@UserId, @Now, 7);
	SET @todayCommentCount = dbo.BloggingActivity_GetCommentCountInRange(@UserId, @Now, 0);
	
	UPDATE b_BloggingActivity 
	SET 
		Value = (@monthBeforePostCount * MonthPostCoef + @weekBeforePostCount * WeekPostCoef + @todayPostCount * TodayPostCoef) + (@monthBeforeCommentCount * MonthCommentCoef + @weekBeforeCommentCount * WeekCommentCoef + @todayCommentCount * TodayCommentCoef), 
		PostValue = (@monthBeforePostCount * MonthPostCoef + @weekBeforePostCount * WeekPostCoef + @todayPostCount * TodayPostCoef),
		CommentValue = (@monthBeforeCommentCount * MonthCommentCoef + @weekBeforeCommentCount * WeekCommentCoef + @todayCommentCount * TodayCommentCoef), 
		LastCalculatedUtc = GETUTCDATE(),
		IsCalculated = 1 
	WHERE UserId = @UserId;
	
	UPDATE b_RatingCounter SET IsCalculated = 0 
	WHERE RatingId IN (SELECT DISTINCT RatingId FROM b_BloggingActivity WHERE UserId = @UserId) 
		AND BoundEntityId = CAST(@UserId AS NVARCHAR(64));	
	
	COMMIT TRANSACTION;
END
GO


IF OBJECT_ID (N'BloggingActivity_Engage', N'P') IS NOT NULL
	DROP PROCEDURE BloggingActivity_Engage
GO

CREATE PROCEDURE BloggingActivity_Engage
	@RatingId INT
AS
BEGIN
	SET NOCOUNT ON;	
	DECLARE @ConfigXml XML;
	SELECT @ConfigXml = ComponentConfigXml  FROM b_Rating WHERE ID = @RatingId AND Active = 1 AND ComponentConfigXml.exist(N'/bloggingActivityConfig') = 1;
	IF(@ConfigXml IS NULL) RETURN;
	
	DECLARE @TodayPostCoef FLOAT(53), @WeekPostCoef FLOAT(53), @MonthPostCoef FLOAT(53), @TodayCommentCoef FLOAT(53), @WeekCommentCoef FLOAT(53), @MonthCommentCoef FLOAT(53);
	SELECT @TodayPostCoef = t.c.value(N'@todayPostCoef', 'FLOAT(53)'), @WeekPostCoef = t.c.value(N'@weekPostCoef', N'FLOAT(53)'), @MonthPostCoef = t.c.value(N'@monthPostCoef', N'FLOAT(53)'), @TodayCommentCoef = t.c.value(N'@todayCommentCoef', N'FLOAT(53)'), @WeekCommentCoef = t.c.value(N'@weekCommentCoef', N'FLOAT(53)'), @MonthCommentCoef = t.c.value(N'@monthCommentCoef', N'FLOAT(53)') FROM @ConfigXml.nodes(N'/bloggingActivityConfig') t(c);

	DELETE FROM b_BloggingActivity WHERE RatingId = @RatingId;
	INSERT INTO b_BloggingActivity(RatingId, Active, IsCalculated, UserId, CreatedUtc, LastCalculatedUtc, Value, PostValue, CommentValue, TodayPostCoef, WeekPostCoef, MonthPostCoef, TodayCommentCoef, WeekCommentCoef, MonthCommentCoef, XmlId)
		SELECT @RatingId, 1, 0, U.ID, GETUTCDATE(), GETUTCDATE(), 0.0, 0.0, 0.0, @TodayPostCoef, @WeekPostCoef, @MonthPostCoef, @TodayCommentCoef, @WeekCommentCoef, @MonthCommentCoef, NULL
			FROM b_BlogUser U;		
END
GO

IF OBJECT_ID (N'BloggingActivity_EngageForUser', N'P') IS NOT NULL
	DROP PROCEDURE BloggingActivity_EngageForUser;
GO

CREATE PROCEDURE BloggingActivity_EngageForUser
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	IF(NOT EXISTS(SELECT N'*' FROM b_BlogUser WHERE ID = @UserId)) RETURN;
	
	DELETE FROM b_BloggingActivity WHERE UserId = @UserId;
	INSERT INTO b_BloggingActivity(RatingId, Active, IsCalculated, UserId, CreatedUtc, LastCalculatedUtc, Value, PostValue, CommentValue, TodayPostCoef, WeekPostCoef, MonthPostCoef, TodayCommentCoef, WeekCommentCoef, MonthCommentCoef, XmlId)	
	SELECT ID, 
		1, 0, 
		@UserId, 
		GETUTCDATE(), GETUTCDATE(), 
		0.0, 0.0, 0.0, 
		ComponentConfigXml.value(N'(/bloggingActivityConfig/@todayPostCoef)[1]', 'FLOAT(53)'),
		ComponentConfigXml.value(N'(/bloggingActivityConfig/@weekPostCoef)[1]', 'FLOAT(53)'),
		ComponentConfigXml.value(N'(/bloggingActivityConfig/@monthPostCoef)[1]', 'FLOAT(53)'),
		ComponentConfigXml.value(N'(/bloggingActivityConfig/@todayCommentCoef)[1]', 'FLOAT(53)'),
		ComponentConfigXml.value(N'(/bloggingActivityConfig/@weekCommentCoef)[1]', 'FLOAT(53)'),
		ComponentConfigXml.value(N'(/bloggingActivityConfig/@monthCommentCoef)[1]', 'FLOAT(53)'),
		NULL		   
	FROM b_Rating WHERE Active = 1 AND ComponentConfigXml.exist(N'/bloggingActivityConfig') = 1;			
END
GO

IF OBJECT_ID (N'BloggingActivity_DisengageForUser', N'P') IS NOT NULL
	DROP PROCEDURE BloggingActivity_DisengageForUser;
GO

CREATE PROCEDURE BloggingActivity_DisengageForUser
	@UserId INT
AS
BEGIN
	DELETE FROM b_BloggingActivity WHERE UserId = @UserId;
END
GO

IF OBJECT_ID (N'BloggingActivity_Disengage', N'P') IS NOT NULL
	DROP PROCEDURE BloggingActivity_Disengage
GO

CREATE PROCEDURE BloggingActivity_Disengage
	@RatingId INT
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM b_BloggingActivity WHERE RatingId = @RatingId;
END
GO

IF OBJECT_ID (N'BloggingActivity_Calculate', N'P') IS NOT NULL
	DROP PROCEDURE BloggingActivity_Calculate;
GO

CREATE PROCEDURE BloggingActivity_Calculate
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
		FROM b_BloggingActivity 
		WHERE RatingId = @RatingId AND Active = 1 AND IsCalculated = 0
		--ORDER BY UserId 
	),
	post30(ID, AuthorId, DatePublished) AS
	(
		SELECT p.ID, p.AuthorId, p.DatePublished 
		FROM b_BlogPost p INNER JOIN b_Blog b ON p.BlogId = b.ID INNER JOIN tgt t ON p.AuthorId = t.UserId WHERE b.Active = 1 AND p.IsPublished = 1 AND p.DatePublished >= @MonthBefore AND p.DatePublished <= @Now
	),
	post30g(AuthorId, Qty) AS
	(
		SELECT AuthorId, COUNT(AuthorId)
				FROM post30
				GROUP BY(AuthorId)
	),
	post7(ID, AuthorId, DatePublished) AS
	(
		SELECT ID, AuthorId, DatePublished
				FROM post30
				WHERE DatePublished >= @WeekBefore
	),
	post7g(AuthorId, Qty) AS
	(
		SELECT AuthorId, count(AuthorId)
				FROM post7
				GROUP BY(AuthorId)
	),
	post1g(AuthorId, Qty) AS
	(
		SELECT AuthorId, count(AuthorId)
				FROM post7
				WHERE DatePublished >= @DayStart
				GROUP BY(AuthorId)			
	),
	comment30(ID, AuthorId, DateCreated) AS
	(
		SELECT c.ID, c.AuthorId, c.DateCreated
				FROM b_BlogComment c INNER JOIN b_Blog b ON c.BlogId = b.ID INNER JOIN b_BlogPost p ON c.PostId = p.ID INNER JOIN tgt t ON c.AuthorId = t.UserId
				WHERE b.Active = 1 AND p.IsPublished = 1 AND c.MarkedForDelete = 0 AND c.DateCreated >= @MonthBefore AND c.DateCreated <= @Now					
	),
	comment30g(AuthorId, Qty) AS
	(
		SELECT AuthorId, COUNT(AuthorId)
				FROM comment30
				GROUP BY(AuthorId)
	),
	comment7(ID, AuthorId, DateCreated) AS
	(
		SELECT ID, AuthorId, DateCreated
				FROM comment30
				WHERE DateCreated >= @WeekBefore
	),
	comment7g(AuthorId, Qty) AS
	(
		SELECT AuthorId, count(AuthorId)
				FROM comment7
				GROUP BY(AuthorId)
	),
	comment1g(AuthorId, Qty) AS
	(
		SELECT AuthorId, count(AuthorId)
				FROM comment7
				WHERE DateCreated >= @DayStart
				GROUP BY(AuthorId)			
	)
	UPDATE b_BloggingActivity 
	SET PostValue = ISNULL(p30g.Qty, 0) * MonthPostCoef + ISNULL(p7g.Qty, 0) * WeekPostCoef + ISNULL(p1g.Qty, 0) * TodayPostCoef,
		CommentValue = ISNULL(c30g.Qty, 0) * MonthCommentCoef + ISNULL(c7g.Qty, 0) * WeekCommentCoef + ISNULL(c1g.Qty, 0) * TodayCommentCoef,
		Value =  ISNULL(p30g.Qty, 0) * MonthPostCoef + ISNULL(p7g.Qty, 0) * WeekPostCoef + ISNULL(p1g.Qty, 0) * TodayPostCoef + ISNULL(c30g.Qty, 0) * MonthCommentCoef + ISNULL(c7g.Qty, 0) * WeekCommentCoef + ISNULL(c1g.Qty, 0) * TodayCommentCoef,
		IsCalculated = 1
	FROM b_BloggingActivity a INNER JOIN tgt t ON a.ID = t.ID
		LEFT OUTER JOIN post30g p30g ON p30g.AuthorId = t.UserId
		LEFT OUTER JOIN post7g p7g ON p7g.AuthorId = t.UserId 
		LEFT OUTER JOIN post1g p1g ON p1g.AuthorId = t.UserId 
		LEFT OUTER JOIN comment30g c30g ON c30g.AuthorId = t.UserId 
		LEFT OUTER JOIN comment7g c7g ON c7g.AuthorId  = t.UserId
		LEFT OUTER JOIN comment1g c1g ON c1g.AuthorId = t.UserId;

	SELECT @@ROWCOUNT;
END
GO

IF OBJECT_ID (N'BloggingActivity_GetEngagingUsers', N'P') IS NOT NULL
	DROP PROCEDURE BloggingActivity_GetEngagingUsers;
GO

CREATE PROCEDURE BloggingActivity_GetEngagingUsers
	@RatingId INT,
	@Count INT
AS
BEGIN
	SET NOCOUNT ON;	
	
	IF(@Count IS NULL OR @Count < 0) SET @Count = 5;
	
	SELECT TOP(@Count) V.UserId 
		FROM b_BloggingActivity V 
		WHERE V.RatingId = @RatingId
			AND NOT EXISTS(SELECT TOP 1 ID FROM b_RatingCounter WHERE RatingId = @RatingId AND BoundEntityTypeId = N'USER' AND BoundEntityId = V.UserId);
END
GO

IF OBJECT_ID (N'b_BlogPostingActivity', N'U') IS NULL
BEGIN
	CREATE TABLE b_BlogPostingActivity
	(
		ID INT IDENTITY(1,1) NOT NULL,
		RatingId INT NOT NULL,
		Active BIT NOT NULL,
		BlogId INT NOT NULL,
		CreatedUtc DATETIME NOT NULL,
		LastCalculatedUtc DATETIME NOT NULL,
		IsCalculated BIT NOT NULL,
		Value FLOAT(53) NOT NULL,	
		TodayPostCoef FLOAT(53) NOT NULL,		
		WeekPostCoef FLOAT(53) NOT NULL,		
		MonthPostCoef FLOAT(53) NOT NULL,				
		XmlId VARCHAR(256) NULL,
		CONSTRAINT PK_b_BlogPostingActivity PRIMARY KEY CLUSTERED(ID ASC)
	)
END
GO

IF OBJECT_ID (N'BlogPostingActivity_GetPostValue', N'FN') IS NOT NULL
	DROP FUNCTION dbo.BlogPostingActivity_GetPostValue
GO

CREATE FUNCTION dbo.BlogPostingActivity_GetPostValue
(
	@BlogId INT,
	@Now DATETIME,
	@MonthCoef FLOAT,
	@WeekCoef FLOAT,
	@TodayCoef FLOAT
)
RETURNS FLOAT(53)
AS
BEGIN
	IF(NOT EXISTS(SELECT ID FROM b_Blog WHERE ID = @BlogId AND Active = 1)) 
		RETURN 0.0;
		
	IF(@Now IS NULL) SET @Now = GETDATE();	
			
	DECLARE @Result FLOAT(53);
	WITH post30(ID, DatePublished) AS
	(
		SELECT ID, DatePublished
		FROM b_BlogPost
		WHERE IsPublished = 1 AND DatePublished >= DATEADD(DAY, -30, @Now) AND DatePublished <= @Now AND BlogId = @BlogId		
	)
	SELECT @Result = (SELECT COUNT(ID) FROM post30) * @MonthCoef +
		(SELECT COUNT(ID) FROM post30 WHERE DatePublished >= DATEADD(DAY, -7, @Now)) * @WeekCoef + 
		(SELECT COUNT(ID) FROM post30 WHERE DatePublished >= DATEADD(DAY, 0, DATEDIFF(DAY, 0, @Now))) * @TodayCoef;
		
	RETURN @Result;	
END
GO

IF OBJECT_ID (N'BlogPostingActivity_Synchronize', N'P') IS NOT NULL
	DROP PROCEDURE BlogPostingActivity_Synchronize;
GO

CREATE PROCEDURE BlogPostingActivity_Synchronize
	@BlogPostingActivityId INT,
	@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;			
	UPDATE b_BlogPostingActivity 
	SET Value = dbo.BlogPostingActivity_GetPostValue(BlogId, @Now, MonthPostCoef, WeekPostCoef, TodayPostCoef), 
		LastCalculatedUtc = GETUTCDATE(),
		IsCalculated = 1 
	WHERE ID = @BlogPostingActivityId;
		
	SELECT Value FROM b_BlogPostingActivity WHERE ID = @BlogPostingActivityId;
END
GO

IF OBJECT_ID (N'BlogPostingActivity_SynchronizeByBlog', N'P') IS NOT NULL
	DROP PROCEDURE BlogPostingActivity_SynchronizeByBlog
GO

CREATE PROCEDURE BlogPostingActivity_SynchronizeByBlog
	@BlogId INT,
	@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	IF(@BlogId IS NULL OR NOT EXISTS(SELECT ID FROM b_BlogPostingActivity WHERE BlogId = @BlogId AND Active = 1)) RETURN;	
	IF(@Now IS NULL) SET @Now = GETDATE();
		
	BEGIN TRANSACTION;

	UPDATE b_BlogPostingActivity 
	SET Value = dbo.BlogPostingActivity_GetPostValue(BlogId, @Now, MonthPostCoef, WeekPostCoef, TodayPostCoef), 
		LastCalculatedUtc = GETUTCDATE(),
		IsCalculated = 1 
	WHERE BlogId = @BlogId;	

	UPDATE b_RatingCounter SET IsCalculated = 0 
	WHERE RatingId IN (SELECT DISTINCT RatingId FROM b_BlogPostingActivity WHERE BlogId = @BlogId) 
		AND BoundEntityId = CAST(@BlogId AS NVARCHAR(64));
	
	COMMIT TRANSACTION;
END
GO

IF OBJECT_ID (N'BlogPostingActivity_Engage', N'P') IS NOT NULL
	DROP PROCEDURE BlogPostingActivity_Engage
GO

CREATE PROCEDURE BlogPostingActivity_Engage
	@RatingId INT
AS
BEGIN
	SET NOCOUNT ON;	
	DECLARE @ConfigXml XML;
	SELECT @ConfigXml = ComponentConfigXml FROM b_Rating WHERE ID = @RatingId AND Active = 1 AND ComponentConfigXml.exist(N'/blogPostingActivityConfig') = 1;
	IF(@ConfigXml IS NULL) RETURN;
	
	DECLARE @TodayPostCoef FLOAT(53), @WeekPostCoef FLOAT(53), @MonthPostCoef FLOAT(53);
	SELECT @TodayPostCoef = t.c.value(N'@todayPostCoef', 'FLOAT(53)'), @WeekPostCoef = t.c.value(N'@weekPostCoef', N'FLOAT(53)'), @MonthPostCoef = t.c.value(N'@monthPostCoef', N'FLOAT(53)') FROM @ConfigXml.nodes(N'/blogPostingActivityConfig') t(c);
	
	DELETE FROM b_BlogPostingActivity WHERE RatingId = @RatingId;
	INSERT INTO b_BlogPostingActivity(RatingId, Active, IsCalculated, BlogId, CreatedUtc, LastCalculatedUtc, Value, TodayPostCoef, WeekPostCoef, MonthPostCoef, XmlId)
		SELECT @RatingId, 1, 0, B.ID, GETUTCDATE(), GETUTCDATE(), 0.0, @TodayPostCoef, @WeekPostCoef, @MonthPostCoef, NULL
			FROM b_Blog B WHERE B.Active = 1;		
END
GO

IF OBJECT_ID (N'BlogPostingActivity_GetEngagingBlogs', N'P') IS NOT NULL
	DROP PROCEDURE BlogPostingActivity_GetEngagingBlogs;
GO

CREATE PROCEDURE BlogPostingActivity_GetEngagingBlogs
	@RatingId INT,
	@Count INT
AS
BEGIN
	SET NOCOUNT ON;	
	IF(@Count IS NULL OR @Count < 0) SET @Count = 5;
	SELECT TOP(@Count) V.BlogId 
		FROM b_BlogPostingActivity V 
		WHERE V.RatingId = @RatingId
			AND V.BlogId NOT IN(SELECT CAST(BoundEntityId AS INT) FROM b_RatingCounter WHERE RatingId = @RatingId AND BoundEntityTypeId = N'BLOG');
END
GO

IF OBJECT_ID (N'BlogPostingActivity_Calculate', N'P') IS NOT NULL
	DROP PROCEDURE BlogPostingActivity_Calculate;
GO

CREATE PROCEDURE BlogPostingActivity_Calculate
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
	
	WITH tgt(ID, BlogId) AS
	(
		SELECT TOP(@Count) ID, BlogId
		FROM b_BlogPostingActivity 
		WHERE RatingId = @RatingId AND Active = 1 AND IsCalculated = 0
		--ORDER BY BlogId 
	),
	post30(ID, BlogId, DatePublished) AS
	(
		SELECT p.ID, p.BlogId, p.DatePublished 
		FROM b_BlogPost p INNER JOIN b_Blog b ON p.BlogId = b.ID INNER JOIN tgt t ON p.BlogId = t.BlogId WHERE b.Active = 1 AND p.IsPublished = 1 AND p.DatePublished >= @MonthBefore AND p.DatePublished <= @Now
	),
	post30g(BlogId, Qty) AS
	(
		SELECT BlogId, COUNT(BlogId)
				FROM post30
				GROUP BY(BlogId)
	),
	post7(ID, BlogId, DatePublished) AS
	(
		SELECT ID, BlogId, DatePublished
				FROM post30
				WHERE DatePublished >= @WeekBefore
	),
	post7g(BlogId, Qty) AS
	(
		SELECT BlogId, COUNT(BlogId)
				FROM post7
				GROUP BY(BlogId)
	),
	post1g(BlogId, Qty) AS
	(
		SELECT BlogId, COUNT(BlogId)
				FROM post7
				WHERE DatePublished >= @DayStart
				GROUP BY(BlogId)			
	)
	UPDATE b_BlogPostingActivity 
	SET Value = ISNULL(p30g.Qty, 0) * MonthPostCoef + ISNULL(p7g.Qty, 0) * WeekPostCoef + ISNULL(p1g.Qty, 0) * TodayPostCoef,
		IsCalculated = 1
	FROM b_BlogPostingActivity a INNER JOIN tgt t ON a.ID = t.ID
		LEFT OUTER JOIN post30g p30g ON p30g.BlogId = t.BlogId
		LEFT OUTER JOIN post7g p7g ON p7g.BlogId = t.BlogId
		LEFT OUTER JOIN post1g p1g ON p1g.BlogId = t.BlogId
	SELECT @@ROWCOUNT;
END
GO

IF OBJECT_ID (N'BlogPostingActivity_Disengage', N'P') IS NOT NULL
	DROP PROCEDURE BlogPostingActivity_Disengage
GO

CREATE PROCEDURE BlogPostingActivity_Disengage
	@RatingId INT
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM b_BlogPostingActivity WHERE RatingId = @RatingId;
END
GO

IF OBJECT_ID (N'BlogPostingActivity_EngageForBlog', N'P') IS NOT NULL
	DROP PROCEDURE BlogPostingActivity_EngageForBlog;
GO

CREATE PROCEDURE BlogPostingActivity_EngageForBlog
	@BlogId INT
AS
BEGIN
	SET NOCOUNT ON;
	IF(NOT EXISTS(SELECT ID FROM b_Blog WHERE ID = @BlogId)) RETURN;
	
	DELETE FROM b_BlogPostingActivity WHERE BlogId = @BlogId;
	INSERT INTO b_BlogPostingActivity(RatingId, Active, IsCalculated, BlogId, CreatedUtc, LastCalculatedUtc, Value, TodayPostCoef, WeekPostCoef, MonthPostCoef, XmlId)	
	SELECT ID, 1, 0, @BlogId, GETUTCDATE(), GETUTCDATE(), 0.0,  
		ComponentConfigXml.value(N'(/blogPostingActivityConfig/@todayPostCoef)[1]', 'FLOAT(53)'),
		ComponentConfigXml.value(N'(/blogPostingActivityConfig/@weekPostCoef)[1]', 'FLOAT(53)'),
		ComponentConfigXml.value(N'(/blogPostingActivityConfig/@monthPostCoef)[1]', 'FLOAT(53)'),
		NULL		   
	FROM b_Rating WHERE Active = 1 AND ComponentConfigXml.exist(N'/blogPostingActivityConfig') = 1;			
END
GO

IF OBJECT_ID (N'BlogPostingActivity_DisengageForBlog', N'P') IS NOT NULL
	DROP PROCEDURE BlogPostingActivity_DisengageForBlog;
GO

CREATE PROCEDURE BlogPostingActivity_DisengageForBlog
	@BlogId INT
AS
BEGIN
	DELETE FROM b_BlogPostingActivity WHERE BlogId = @BlogId;
END
GO

IF OBJECT_ID (N'RatingVoting_GetEngagingBlogs', N'P') IS NOT NULL
	DROP PROCEDURE RatingVoting_GetEngagingBlogs;
GO

CREATE PROCEDURE RatingVoting_GetEngagingBlogs
	@RatingId INT,
	@Count INT
AS
BEGIN
	SET NOCOUNT ON;	
	
	IF(@Count IS NULL OR @Count <= 0) SET @Count = 5;
	
	SELECT TOP(@Count) b.ID
	FROM b_RatingVoting v 
	INNER JOIN b_Blog b 
		ON v.BoundEntityTypeId = N'BLOG'
			AND v.BoundEntityId = b.ID 
			AND NOT EXISTS(SELECT ID FROM b_RatingCounter c WHERE c.RatingId = @RatingId AND c.BoundEntityTypeId = N'BLOG' AND c.BoundEntityId = b.ID);
END
GO
/*--- END OF RATINGS   ---*/

/*--- BEGINNING OF MODERATION   ---*/
IF OBJECT_ID(N'b_BlogSettings', N'U') IS NULL
CREATE TABLE [b_BlogSettings]
(
	[BlogId] INT NOT NULL CONSTRAINT [DF_b_BlogSettings_BlogId]  DEFAULT (0),
	[EnableCommentModeration] BIT NOT NULL CONSTRAINT [DF_b_BlogSettings_EnableCommentModeration]  DEFAULT (0),
	[CommentModerationMode] INT NOT NULL CONSTRAINT [DF_b_BlogSettings_CommentModerationMode]  DEFAULT (0),
	[CommentModerationFilterXml] XML NULL,
	CONSTRAINT [PK_b_BlogSettings] PRIMARY KEY CLUSTERED ([BlogId] ASC)
)
GO
/*--- END OF MODERATION   ---*/

/*--- TEAM BLOGS ---*/
IF OBJECT_ID (N'b_BlogUserGroup', N'U') IS NULL BEGIN
	CREATE TABLE b_BlogUserGroup
	(
		Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_b_BlogUserGroup PRIMARY KEY,
		BlogId INT NOT NULL CONSTRAINT DF_b_BlogUserGroup_BlogId DEFAULT (0),
		Type INT NOT NULL CONSTRAINT DF_b_BlogUserGroup_Type DEFAULT (0),
		Name NVARCHAR(256) NULL
	)
	SET IDENTITY_INSERT b_BlogUserGroup ON
	INSERT INTO b_BlogUserGroup (Id, Type) VALUES (1, 1);
	INSERT INTO b_BlogUserGroup (Id, Type) VALUES (2, 2);
	SET IDENTITY_INSERT b_BlogUserGroup OFF
END
GO

IF OBJECT_ID (N'b_BlogUser2Group', N'U') IS NULL
	CREATE TABLE b_BlogUser2Group
	(
		BlogUserId INT NOT NULL,
		BlogUserGroupId INT NOT NULL,
		IsAuto BIT NOT NULL CONSTRAINT DF_b_BlogUser2Group_IsAuto DEFAULT(0)	
	)
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_BlogUser2Group_IdPair')
	CREATE CLUSTERED INDEX IX_b_BlogUser2Group_IdPair ON b_BlogUser2Group(BlogUserId ASC, BlogUserGroupId ASC);
GO


IF OBJECT_ID (N'b_BlogUserPermission', N'U') IS NULL
	CREATE TABLE b_BlogUserPermission
	(
		Id INT IDENTITY(1, 1) CONSTRAINT PK_b_BlogUserPermission PRIMARY KEY,
		BlogUserGroupId INT NOT NULL,
		BlogId INT NOT NULL,
		Type INT NOT NULL,
		PermissionLevel INT NOT NULL
	)
GO

IF OBJECT_ID (N'BlogUser_GetPermissionsGuest', N'P') IS NOT NULL
DROP PROCEDURE BlogUser_GetPermissionsGuest
GO

CREATE PROCEDURE BlogUser_GetPermissionsGuest
	@blogId int
AS
BEGIN	
	SET NOCOUNT ON;

	SELECT Type, MAX(PermissionLevel) FROM b_BlogUserPermission
	WHERE
		BlogId = @blogId
		AND BlogUserGroupId IN (SELECT Id FROM b_BlogUserGroup WHERE Type = 1)
	GROUP BY Type
END
GO

IF OBJECT_ID (N'BlogUser_GetPermissions', N'P') IS NOT NULL
DROP PROCEDURE BlogUser_GetPermissions
GO

CREATE PROCEDURE BlogUser_GetPermissions
	@blogId int, 
	@userId int
AS
BEGIN	
	SET NOCOUNT ON;

	SELECT Type, MAX(PermissionLevel) FROM b_BlogUserPermission
	WHERE 
		BlogId = @blogId
		AND 	
		(
			BlogUserGroupId IN (SELECT Id FROM b_BlogUserGroup WHERE Type IN (1, 2))
			OR BlogUserGroupId IN (SELECT BlogUserGroupId FROM b_BlogUser2Group WHERE BlogUserId = @userId)		
		)
	GROUP BY Type
END
GO

IF OBJECT_ID (N'BlogUser_GetPermittedBlogsGuest', N'P') IS NOT NULL
DROP PROCEDURE BlogUser_GetPermittedBlogsGuest
GO

CREATE PROCEDURE BlogUser_GetPermittedBlogsGuest
	@type int
AS
BEGIN
	SELECT p.BlogId, MAX(p.PermissionLevel)
	FROM b_BlogUserGroup g
	INNER JOIN b_BlogUserPermission p ON p.BlogUserGroupId = g.Id
	WHERE p.Type = @type AND g.Id = 1		
	GROUP BY p.BlogId
END
GO

IF OBJECT_ID (N'BlogUser_GetPermittedBlogs', N'P') IS NOT NULL
DROP PROCEDURE BlogUser_GetPermittedBlogs
GO

CREATE PROCEDURE BlogUser_GetPermittedBlogs
	@type int,
	@userId int
AS
BEGIN
	SELECT p.BlogId, MAX(p.PermissionLevel)
	FROM b_BlogUserGroup g
	INNER JOIN b_BlogUserPermission p ON p.BlogUserGroupId = g.Id
	WHERE 
		p.Type = @type
		AND (
			g.Id IN (1, 2)
			OR g.Id IN (SELECT BlogUserGroupId FROM b_BlogUser2Group WHERE BlogUserId = @userId)
		)
	GROUP BY p.BlogId
END
GO
/*--- END OF TEAM BLOGS ---*/