IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[b_Search_ContentTags]') AND type in (N'U'))
BEGIN
CREATE TABLE [b_Search_ContentTags](
	[contentId] [int] NOT NULL,
	[tagId] [int] NOT NULL
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[b_Search_ContentTags]') AND name = N'IX_BX_b_Search_ContentTags_Content')
CREATE CLUSTERED INDEX [IX_BX_b_Search_ContentTags_Content] ON [b_Search_ContentTags] 
(
	[contentId] ASC,
	[tagId] ASC
)

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[b_Search_Tags]') AND type in (N'U'))
BEGIN
CREATE TABLE [b_Search_Tags](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[nameLower] [nvarchar](64) NOT NULL,
	[status] [int] NOT NULL,
 CONSTRAINT [PK_b_Search_Tags] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)
)
END

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[b_Search_Content]') AND type in (N'U'))
BEGIN
CREATE TABLE [b_Search_Content](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[moduleId] [varchar](128) NOT NULL,
	[itemGroup] [nvarchar](64) NOT NULL,
	[itemId] [nvarchar](300) NOT NULL,
	[contentDate] [datetime] NOT NULL,
	[title] [nvarchar](max) NULL,
	[body] [ntext] NULL,
	[searchableContent] [ntext] NULL,
	[dateFrom] [datetime] NOT NULL CONSTRAINT DF_b_Search_Content_dateFrom DEFAULT(CONVERT(datetime, '1753-01-01T00:00:00.000', 126)),
	[dateTo] [datetime] NOT NULL CONSTRAINT DF_b_Search_Content_dateTo DEFAULT(CONVERT(datetime, '9999-12-31T23:59:59.997', 126)),
	[operationLoweredName] [nvarchar](128) NULL,
	[operationModuleId] [varchar](50) NOT NULL,
	[operationExternalId] [varchar](50) NOT NULL,
	[filePermission] [nvarchar](256) NULL,
	[param1] [nvarchar](1024) NULL,
	[param2] [nvarchar](1024) NULL,
	[updateSessionId] [int] NOT NULL CONSTRAINT [DF_b_Search_Content_updateSessionId] DEFAULT (0)
 CONSTRAINT [PK_b_search_content] PRIMARY KEY CLUSTERED 
(
	[id] ASC
),
 CONSTRAINT [IX_BX_b_Search_Content] UNIQUE NONCLUSTERED 
(
	[moduleId] ASC,
	[itemGroup] ASC,
	[itemId] ASC
)
)
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[b_Search_ContentStemIndex]') AND type in (N'U'))
BEGIN
CREATE TABLE [b_Search_ContentStemIndex](
	[contentId] [int] NOT NULL,
	[languageId] [char](2) NOT NULL,
	[stem] [nvarchar](128) NOT NULL,
	[frequency] [float] NOT NULL
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[b_Search_ContentStemIndex]') AND name = N'IX_BX_b_Search_ContentStemIndex')
CREATE NONCLUSTERED INDEX [IX_BX_b_Search_ContentStemIndex] ON [b_Search_ContentStemIndex] 
(
	[stem] ASC,
	[languageId] ASC,
	[contentId] ASC
)
INCLUDE ( [frequency])
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[b_Search_ContentStemIndex]') AND name = N'IX_BX_b_Search_ContentStemIndex_Content')
CREATE NONCLUSTERED INDEX [IX_BX_b_Search_ContentStemIndex_Content] ON [b_Search_ContentStemIndex] 
(
	[contentId] ASC,
	[languageId] ASC
)
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[b_Search_ContentStemIndex]') AND name = N'IX_BX_b_Search_ContentStemIndex_ContentId')
CREATE CLUSTERED INDEX [IX_BX_b_Search_ContentStemIndex_ContentId] ON [b_Search_ContentStemIndex] 
(
	[contentId] ASC
)
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[b_Search_ContentSites]') AND type in (N'U'))
BEGIN
CREATE TABLE [b_Search_ContentSites](
	[contentId] [int] NOT NULL,
	[siteId] [varchar](50) NOT NULL
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[b_Search_ContentSites]') AND name = N'IX_BX_b_Search_ContentSiteIds_Content')
CREATE CLUSTERED INDEX [IX_BX_b_Search_ContentSiteIds_Content] ON [b_Search_ContentSites] 
(
	[contentId] ASC
)
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[b_Search_ContentSites]') AND name = N'IX_b_Search_ContentSites')
CREATE NONCLUSTERED INDEX [IX_b_Search_ContentSites] ON [b_Search_ContentSites] 
(
	[contentId] ASC
)
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[b_Search_ContentSites]') AND name = N'IX_BX_b_Search_ContentSites_Full')
CREATE NONCLUSTERED INDEX [IX_BX_b_Search_ContentSites_Full] ON [b_Search_ContentSites] 
(
	[siteId] ASC,
	[contentId] ASC
)
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[b_Search_ContentFileRoles]') AND type in (N'U'))
BEGIN
CREATE TABLE [b_Search_ContentFileRoles](
	[contentId] [int] NOT NULL,
	[roleId] [int] NOT NULL
)
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[b_Search_ContentFileRoles]') AND name = N'IX_BX_b_Search_ContentFileRoles_ContentId')
CREATE CLUSTERED INDEX [IX_BX_b_Search_ContentFileRoles_ContentId] ON [b_Search_ContentFileRoles] 
(
	[contentId] ASC
)
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Search_SetTags]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'
CREATE PROCEDURE [Search_SetTags]
	@contentId int,
	@tags xml
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @TranStarted bit
	SET @TranStarted = 0

	IF (@@TRANCOUNT = 0)
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END
	ELSE
		SET @TranStarted = 0
	
	DECLARE @tagsCache TABLE(name nvarchar(64))
	INSERT INTO @tagsCache
	SELECT 
		Tags.name.value(''.'', ''NVARCHAR(64)'') AS tag
	FROM @tags.nodes(''/tags/t'') as Tags(name)
			
	DECLARE @tagsCount int
	SET @tagsCount = @@ROWCOUNT

	IF (@tagsCount > 0) BEGIN
		INSERT INTO b_Search_Tags(nameLower)
		SELECT name
		FROM @tagsCache
		WHERE NOT name IN (SELECT nameLower FROM b_Search_Tags)
	END

	DELETE FROM b_Search_ContentTags WHERE contentId = @contentId

	IF (@tagsCount > 0) BEGIN
		INSERT INTO b_Search_ContentTags(contentId, tagId) 
		SELECT @contentId, id
		FROM b_Search_Tags 
		WHERE nameLower IN (SELECT name FROM @tagsCache)
	END

	IF (@TranStarted = 1)
	BEGIN
		SET @TranStarted = 0
		COMMIT TRANSACTION
	END
END
' 
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Search_CreateTag]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'
CREATE PROCEDURE [Search_CreateTag]
	@nameLower nvarchar(64),
	@status int = 0,
	@id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @id = (SELECT id FROM b_Search_Tags WHERE nameLower = @nameLower)
	
	IF (@id IS NULL) BEGIN
		INSERT INTO b_Search_Tags (nameLower, [status]) VALUES (@nameLower, @status)
		SET @id = @@IDENTITY
	END ELSE
		UPDATE b_Search_Tags SET [status] = @status WHERE id = @id
END
' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Search_GetInvalidTags]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'
CREATE PROCEDURE [Search_GetInvalidTags] 
	@tags xml,
	@minStatus int = 0
AS
BEGIN
	SET NOCOUNT ON;
	
	IF (@minStatus <= 0) BEGIN
		SELECT
			id AS Id, 
			nameLower AS Name, 
			status AS Status 
		FROM b_Search_Tags 
		WHERE 
			nameLower IN 
			(
				SELECT 
					Tags.name.value(''.'', ''NVARCHAR(64)'')
				FROM @tags.nodes(''/tags/t'') AS Tags(name)
			)
			AND
			status < @minStatus	
	END ELSE BEGIN
		WITH tags AS
		(
			SELECT Tags.name.value(''.'', ''NVARCHAR(64)'') AS name
			FROM @tags.nodes(''/tags/t'') AS Tags(name)
		)
		SELECT 
			id AS Id, 
			nameLower AS Name, 
			status AS Status
		FROM b_Search_Tags 
		WHERE 
			nameLower IN (SELECT name FROM tags)
			AND status < @minStatus	
		UNION
			SELECT 
				NULL AS Id, 
				name AS Name, 
				0 AS Status
			FROM tags
			WHERE NOT name IN (SELECT nameLower FROM b_Search_Tags)
	END
END
' 
END
GO

IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[DF_b_search_content_moduleId]') AND parent_object_id = OBJECT_ID(N'[b_Search_Content]'))
BEGIN
ALTER TABLE [b_Search_Content] ADD  CONSTRAINT [DF_b_search_content_moduleId]  DEFAULT ('') FOR [moduleId]
END
GO
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[DF_b_search_content_itemGroup]') AND parent_object_id = OBJECT_ID(N'[b_Search_Content]'))
BEGIN
ALTER TABLE [b_Search_Content] ADD  CONSTRAINT [DF_b_search_content_itemGroup]  DEFAULT ('') FOR [itemGroup]
END

GO
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[DF_b_search_content_itemId]') AND parent_object_id = OBJECT_ID(N'[b_Search_Content]'))
BEGIN
ALTER TABLE [b_Search_Content] ADD  CONSTRAINT [DF_b_search_content_itemId]  DEFAULT ('') FOR [itemId]
END

GO
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[DF_b_Search_Content_contentDate]') AND parent_object_id = OBJECT_ID(N'[b_Search_Content]'))
BEGIN
ALTER TABLE [b_Search_Content] ADD  CONSTRAINT [DF_b_Search_Content_contentDate]  DEFAULT (getdate()) FOR [contentDate]
END

GO
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[DF_b_Search_Content_operationModuleId]') AND parent_object_id = OBJECT_ID(N'[b_Search_Content]'))
BEGIN
ALTER TABLE [b_Search_Content] ADD  CONSTRAINT [DF_b_Search_Content_operationModuleId]  DEFAULT ('') FOR [operationModuleId]
END

GO
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[DF_b_Search_Content_operationExternalId]') AND parent_object_id = OBJECT_ID(N'[b_Search_Content]'))
BEGIN
ALTER TABLE [b_Search_Content] ADD  CONSTRAINT [DF_b_Search_Content_operationExternalId]  DEFAULT ('') FOR [operationExternalId]
END

GO
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[DF_b_Search_Tags_approved]') AND parent_object_id = OBJECT_ID(N'[b_Search_Tags]'))
BEGIN
ALTER TABLE [b_Search_Tags] ADD  CONSTRAINT [DF_b_Search_Tags_approved]  DEFAULT ((0)) FOR [status]
END

GO

IF OBJECT_ID('[b_Search_StemFrequencyCache]', 'U') IS NULL
CREATE TABLE [b_Search_StemFrequencyCache](
	[languageId] [char](2) NOT NULL,
	[stem] [nvarchar](128) NOT NULL,
	[freqLimit] [float] NOT NULL,
	[stemCount] [int] NOT NULL
)
GO
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_BX_b_Search_StemFrequencyCache')
CREATE NONCLUSTERED INDEX [IX_BX_b_Search_StemFrequencyCache] ON [b_Search_StemFrequencyCache] 
(
	[languageId] ASC,
	[stem] ASC
)
GO

IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_BX_b_Search_StemFrequencyCache_LanguageId')
CREATE CLUSTERED INDEX [IX_BX_b_Search_StemFrequencyCache_LanguageId] ON [b_Search_StemFrequencyCache] 
(
	[languageId] ASC
)
GO


IF OBJECT_ID('[Search_GetStemsFreq]', 'P') IS NOT NULL DROP PROCEDURE [Search_GetStemsFreq]
GO
CREATE PROCEDURE [Search_GetStemsFreq]
	@languageId char(2),
	@stems xml,
	@maxCount int,
	@buckets int = 100
AS
BEGIN
	DECLARE empty_stems CURSOR LOCAL FAST_FORWARD
	FOR
		SELECT
			Stems.s.value('.', 'NVARCHAR(128)')
		FROM 
			@stems.nodes('/stems/s') as Stems(s)
			LEFT JOIN b_Search_StemFrequencyCache c ON c.stem = Stems.s.value('.', 'NVARCHAR(128)') AND c.languageId = @languageId
		WHERE 
			c.freqLimit IS NULL
			
	OPEN empty_stems
	
	DECLARE @stem nvarchar(128)
	
	WHILE (1 = 1) BEGIN
		FETCH empty_stems INTO @stem
		IF @@FETCH_STATUS <> 0 BREAK
		
		DECLARE freq_cursor CURSOR LOCAL FAST_FORWARD
		FOR 
			SELECT COUNT(*), FLOOR(frequency * @buckets) 
			FROM b_Search_ContentStemIndex
			WHERE
				languageId = @languageId
				AND stem = @stem
			GROUP BY FLOOR(frequency * @buckets)
			ORDER BY FLOOR(frequency * @buckets) DESC

		OPEN freq_cursor

		DECLARE @count int
		DECLARE @freq int
		DECLARE @sum int

		SET @freq = @buckets
		SET @sum = 0

		WHILE (1 = 1) BEGIN
			FETCH freq_cursor INTO @count, @freq
			IF @@FETCH_STATUS <> 0 BREAK
		
			SET @sum = @sum + @count * @freq;
			
			IF @sum > @maxCount * @buckets BREAK

			FETCH freq_cursor
			INTO @count, @freq
		END

		CLOSE freq_cursor
		DEALLOCATE freq_cursor

		DECLARE @stemCount int;
		SET @stemCount = (SELECT COUNT(*) FROM b_Search_ContentStemIndex WHERE languageId = @languageId AND stem = @stem);
		INSERT INTO b_Search_StemFrequencyCache (languageId, stem, freqLimit, stemCount) 
		VALUES (
			@languageId, 
			@stem, 
			@freq / CONVERT(FLOAT, @buckets), 
			@stemCount
		)
	END
	
	CLOSE empty_stems
	DEALLOCATE empty_stems
	
	SELECT
		Stems.s.value('.', 'NVARCHAR(128)') AS stem,
		c.freqLimit AS freqLimit
	FROM 
		@stems.nodes('/stems/s') as Stems(s)
		LEFT JOIN b_Search_StemFrequencyCache c ON c.stem = Stems.s.value('.', 'NVARCHAR(128)') AND c.languageId = @languageId
	WHERE 
		c.freqLimit IS NOT NULL
END
GO
IF OBJECT_ID('[Search_SetStems]', 'P') IS NOT NULL DROP PROCEDURE [Search_SetStems]
GO
CREATE PROCEDURE [Search_SetStems]
	@contentId  int, 
	@languageId char(2),
	@stems xml
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM b_Search_ContentStemIndex WHERE contentId = @contentId AND languageId = @languageId

	INSERT INTO b_Search_ContentStemIndex (contentId, languageId, stem, frequency)
	SELECT
		@contentId AS contentId,
		@languageId AS languageId,
		Stems.s.value('.', 'NVARCHAR(128)') AS stem,
		Stems.s.value('@f', 'FLOAT') AS frequency
	FROM 
		@stems.nodes('/stems/s') as Stems(s)
	
	DELETE FROM b_Search_StemFrequencyCache 
	WHERE 
		languageId = @languageId 
		AND stem IN (SELECT Stems.s.value('.', 'NVARCHAR(128)') FROM @stems.nodes('/stems/s') as Stems(s))
END
GO
IF OBJECT_ID('[Search_DeleteAll]', 'P') IS NOT NULL DROP PROCEDURE [Search_DeleteAll]
GO
CREATE PROCEDURE [Search_DeleteAll]
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRAN
		TRUNCATE TABLE b_Search_Content
		TRUNCATE TABLE b_Search_ContentSites
		TRUNCATE TABLE b_Search_ContentFileRoles
		TRUNCATE TABLE b_Search_ContentStemIndex
		TRUNCATE TABLE b_Search_ContentTags
		TRUNCATE TABLE b_Search_StemFrequencyCache
	COMMIT TRAN
END
GO
IF OBJECT_ID('[Search_DeleteContent]', 'P') IS NOT NULL DROP PROCEDURE [Search_DeleteContent] 
GO
CREATE PROCEDURE [Search_DeleteContent] 
	@id int = -1 
AS
BEGIN
	DELETE FROM b_Search_Content WHERE id = @id; 
	SET NOCOUNT ON;
	
	DELETE FROM b_Search_StemFrequencyCache
	FROM b_Search_StemFrequencyCache c
	INNER JOIN b_Search_ContentStemIndex i ON i.languageId = c.languageId AND i.stem = c.stem
	WHERE i.contentId = @id
	
	DELETE FROM b_Search_ContentStemIndex WHERE contentId = @id 
	DELETE FROM b_Search_ContentSites WHERE contentId = @id;
	DELETE FROM b_Search_ContentFileRoles WHERE contentId = @id;
	DELETE FROM b_Search_ContentTags WHERE contentId = @id
END
GO
IF OBJECT_ID('[Search_DeleteSites]', 'P') IS NOT NULL DROP PROCEDURE [Search_DeleteSites]
GO
CREATE PROCEDURE [Search_DeleteSites]
	@sites xml
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @sitesTable table(id varchar(50))
	INSERT INTO @sitesTable 
	SELECT
		Sites.s.value('.', 'VARCHAR(50)') AS id
	FROM 
		@sites.nodes('/sites/s') as Sites(s)
		
	DECLARE @content table(id int)
	INSERT INTO @content
	SELECT contentId FROM b_Search_ContentSites WHERE siteId IN (SELECT id FROM @sitesTable)
	EXCEPT
	SELECT contentId FROM b_Search_ContentSites WHERE NOT siteId IN (SELECT id FROM @sitesTable)
		
	DELETE FROM b_Search_ContentFileRoles  WHERE contentId IN (SELECT id FROM @content)
	
	DELETE FROM b_Search_StemFrequencyCache
	FROM b_Search_StemFrequencyCache c
	INNER JOIN b_Search_ContentStemIndex i ON i.languageId = c.languageId AND i.stem = c.stem
	WHERE i.contentId IN (SELECT id FROM @content)
	
	DELETE FROM b_Search_ContentStemIndex  WHERE contentId IN (SELECT id FROM @content)
	
	DELETE FROM b_Search_ContentTags WHERE contentId IN (SELECT id FROM @content)
	
	DELETE FROM b_Search_ContentSites WHERE siteId IN (SELECT id FROM @sitesTable)
	SET NOCOUNT OFF;
	DELETE FROM b_Search_Content WHERE id IN (SELECT id FROM @content)
END
GO