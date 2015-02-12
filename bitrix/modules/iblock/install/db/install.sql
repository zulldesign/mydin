GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[b_IBlock]') AND type in (N'U'))
BEGIN
CREATE TABLE [b_IBlock](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TypeId] [int] NOT NULL,
	[UpdateDate] [datetime] NOT NULL CONSTRAINT [DF_b_IBlock_UpdateDate]  DEFAULT (getdate()),
	[CreateDate] [datetime] NOT NULL CONSTRAINT [DF_b_IBlock_CreateDate]  DEFAULT (getdate()),
	[Code] [nvarchar](128) NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Active] [char](1) NOT NULL CONSTRAINT [DF_b_IBlock_Active]  DEFAULT ('Y'),
	[Sort] [int] NOT NULL CONSTRAINT [DF_b_IBlock_Sort]  DEFAULT ((50)),
	[ImageId] [int] NULL,
	[Description] [nvarchar](max) NULL,
	[DescriptionType] [char](1) NOT NULL CONSTRAINT [DF_b_IBlock_DescriptionType]  DEFAULT ('T'),
	[XmlId] [varchar](255) NULL,
	[SectionsName] [nvarchar](255) NULL,
	[SectionName] [nvarchar](255) NULL,
	[ElementsName] [nvarchar](255) NULL,
	[ElementName] [nvarchar](255) NULL,
	[IndexContent] [char](1) NOT NULL CONSTRAINT [DF_b_IBlock_IndexContent]  DEFAULT ('Y'),
	[XmlCaptionsInfo] [xml] NULL,
 CONSTRAINT [PK_b_IBlock] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[b_IBlock]') AND name = N'IX_b_IBlock')
CREATE NONCLUSTERED INDEX [IX_b_IBlock] ON [b_IBlock] 
(
	[TypeId] ASC,
	[Active] ASC
)
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[b_IBlockElement]') AND type in (N'U'))
BEGIN
CREATE TABLE [b_IBlockElement](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UpdateDate] [datetime] NOT NULL CONSTRAINT [DF_b_IBlockElement_UpdateDate]  DEFAULT (getdate()),
	[CreateDate] [datetime] NOT NULL CONSTRAINT [DF_b_IBlockElement_CreateDate]  DEFAULT (getdate()),
	[ModifiedBy] [int] NULL,
	[CreatedBy] [int] NULL,
	[IBlockID] [int] NOT NULL,
	[Active] [char](1) NOT NULL CONSTRAINT [DF_b_IBlockElement_Active]  DEFAULT ('Y'),
	[ActiveFromDate] [datetime] NULL,
	[ActiveToDate] [datetime] NULL,
	[Sort] [int] NOT NULL CONSTRAINT [DF_b_IBlockElement_Sort]  DEFAULT ((50)),
	[Name] [nvarchar](255) NOT NULL,
	[PreviewImageId] [int] NULL,
	[PreviewText] [nvarchar](max) NULL,
	[PreviewTextType] [char](1) NOT NULL CONSTRAINT [DF_b_IBlockElement_PreviewTextType]  DEFAULT ('T'),
	[DetailImageId] [int] NULL,
	[DetailText] [nvarchar](max) NULL,
	[DetailTextType] [char](1) NOT NULL CONSTRAINT [DF_b_IBlockElement_DetailTextType]  DEFAULT ('T'),
	[XmlId] [varchar](255) NULL,
	[Code] [varchar](255) NULL,
	[Tags] [nvarchar](1024) NULL,
	[ViewsCount] [int] NOT NULL DEFAULT ((0)),
 CONSTRAINT [PK_b_IBlockElement] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[b_IBlockElement]') AND name = N'IX_BX_b_IBlockElement_IBlock')
CREATE NONCLUSTERED INDEX [IX_BX_b_IBlockElement_IBlock] ON [b_IBlockElement] 
(
	[IBlockID] ASC
)
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[b_IBlockSection]') AND type in (N'U'))
BEGIN
CREATE TABLE [b_IBlockSection](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UpdateDate] [datetime] NOT NULL CONSTRAINT [DF_b_IBlockSection_UpdateDate]  DEFAULT (getdate()),
	[CreateDate] [datetime] NOT NULL CONSTRAINT [DF_b_IBlockSection_CreateDate]  DEFAULT (getdate()),
	[IBlockId] [int] NOT NULL,
	[SectionId] [int] NULL,
	[Active] [char](1) NOT NULL CONSTRAINT [DF_b_IBlockSection_Active]  DEFAULT ('Y'),
	[ActiveGlobal] [char](1) NOT NULL CONSTRAINT [DF_b_IBlockSection_ActiveGlobal]  DEFAULT ('Y'),
	[Sort] [int] NOT NULL CONSTRAINT [DF_b_IBlockSection_Sort]  DEFAULT ((50)),
	[Name] [nvarchar](255) NOT NULL,
	[ImageId] [int] NULL,
	[DetailImageId] [int] NULL,
	[LeftMargin] [int] NULL,
	[RightMargin] [int] NULL,
	[DepthLevel] [int] NULL,
	[Description] [nvarchar](max) NULL,
	[DescriptionType] [char](1) NOT NULL CONSTRAINT [DF_b_IBlockSection_DescriptionType]  DEFAULT ('T'),
	[Code] [varchar](255) NULL,
	[XmlId] [varchar](255) NULL,
	[CreatedBy] [int] NULL,
	[ModifiedBy] [int] NULL,
 CONSTRAINT [PK_b_IBlockSection] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[b_IBlockSection]') AND name = N'IX_b_IBlockSection')
CREATE NONCLUSTERED INDEX [IX_b_IBlockSection] ON [b_IBlockSection] 
(
	[IBlockId] ASC,
	[SectionId] ASC
)
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[b_IBlockSection]') AND name = N'IX_b_IBlockSection_1')
CREATE NONCLUSTERED INDEX [IX_b_IBlockSection_1] ON [b_IBlockSection] 
(
	[IBlockId] ASC,
	[LeftMargin] ASC,
	[RightMargin] ASC
)
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[b_IBlockElementInSection]') AND type in (N'U'))
BEGIN
CREATE TABLE [b_IBlockElementInSection](
	[SectionId] [int] NOT NULL,
	[ElementId] [int] NOT NULL,
 CONSTRAINT [PK_b_IBlockElementInSection] PRIMARY KEY CLUSTERED 
(
	[SectionId] ASC,
	[ElementId] ASC
)
)
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[b_IBlockTypeLang]') AND type in (N'U'))
BEGIN
CREATE TABLE [b_IBlockTypeLang](
	[TypeId] [int] NOT NULL,
	[LanguageId] [char](2) NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[SectionName] [nvarchar](128) NULL,
	[ElementName] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_b_IBlock_TypeLang] PRIMARY KEY CLUSTERED 
(
	[TypeId] ASC,
	[LanguageId] ASC
)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[b_IBlockInSite]') AND type in (N'U'))
BEGIN
CREATE TABLE [b_IBlockInSite](
	[IBlockId] [int] NOT NULL,
	[SiteId] [varchar](50) NOT NULL,
 CONSTRAINT [PK_b_IBlock_Site] PRIMARY KEY CLUSTERED 
(
	[IBlockId] ASC,
	[SiteId] ASC
)
)
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[b_IBlockType]') AND type in (N'U'))
BEGIN
CREATE TABLE [b_IBlockType](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NULL,
	[HaveSections] [char](1) NOT NULL,
	[Sort] [int] NOT NULL,
	[XmlId] [nvarchar](255) NULL,
 CONSTRAINT [PK_b_IBlock_Type] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)
)
END
GO


IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlock_b_IBlock_Type]') AND parent_object_id = OBJECT_ID(N'[b_IBlock]'))
ALTER TABLE [b_IBlock]  WITH CHECK ADD  CONSTRAINT [FK_b_IBlock_b_IBlock_Type] FOREIGN KEY([TypeId])
REFERENCES [b_IBlockType] ([ID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlock_b_IBlock_Type]') AND parent_object_id = OBJECT_ID(N'[b_IBlock]'))
ALTER TABLE [b_IBlock] CHECK CONSTRAINT [FK_b_IBlock_b_IBlock_Type]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlockElement_b_IBlock]') AND parent_object_id = OBJECT_ID(N'[b_IBlockElement]'))
ALTER TABLE [b_IBlockElement]  WITH CHECK ADD  CONSTRAINT [FK_b_IBlockElement_b_IBlock] FOREIGN KEY([IBlockID])
REFERENCES [b_IBlock] ([ID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlockElement_b_IBlock]') AND parent_object_id = OBJECT_ID(N'[b_IBlockElement]'))
ALTER TABLE [b_IBlockElement] CHECK CONSTRAINT [FK_b_IBlockElement_b_IBlock]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlockElementInSection_b_IBlockElement]') AND parent_object_id = OBJECT_ID(N'[b_IBlockElementInSection]'))
ALTER TABLE [b_IBlockElementInSection]  WITH CHECK ADD  CONSTRAINT [FK_b_IBlockElementInSection_b_IBlockElement] FOREIGN KEY([ElementId])
REFERENCES [b_IBlockElement] ([ID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlockElementInSection_b_IBlockElement]') AND parent_object_id = OBJECT_ID(N'[b_IBlockElementInSection]'))
ALTER TABLE [b_IBlockElementInSection] CHECK CONSTRAINT [FK_b_IBlockElementInSection_b_IBlockElement]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlockElementInSection_b_IBlockSection]') AND parent_object_id = OBJECT_ID(N'[b_IBlockElementInSection]'))
ALTER TABLE [b_IBlockElementInSection]  WITH CHECK ADD  CONSTRAINT [FK_b_IBlockElementInSection_b_IBlockSection] FOREIGN KEY([SectionId])
REFERENCES [b_IBlockSection] ([ID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlockElementInSection_b_IBlockSection]') AND parent_object_id = OBJECT_ID(N'[b_IBlockElementInSection]'))
ALTER TABLE [b_IBlockElementInSection] CHECK CONSTRAINT [FK_b_IBlockElementInSection_b_IBlockSection]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlock_Site_b_IBlock]') AND parent_object_id = OBJECT_ID(N'[b_IBlockInSite]'))
ALTER TABLE [b_IBlockInSite]  WITH CHECK ADD  CONSTRAINT [FK_b_IBlock_Site_b_IBlock] FOREIGN KEY([IBlockId])
REFERENCES [b_IBlock] ([ID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlock_Site_b_IBlock]') AND parent_object_id = OBJECT_ID(N'[b_IBlockInSite]'))
ALTER TABLE [b_IBlockInSite] CHECK CONSTRAINT [FK_b_IBlock_Site_b_IBlock]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlock_Site_b_Site]') AND parent_object_id = OBJECT_ID(N'[b_IBlockInSite]'))
ALTER TABLE [b_IBlockInSite]  WITH CHECK ADD  CONSTRAINT [FK_b_IBlock_Site_b_Site] FOREIGN KEY([SiteId])
REFERENCES [b_Site] ([ID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlock_Site_b_Site]') AND parent_object_id = OBJECT_ID(N'[b_IBlockInSite]'))
ALTER TABLE [b_IBlockInSite] CHECK CONSTRAINT [FK_b_IBlock_Site_b_Site]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlockSection_b_IBlock]') AND parent_object_id = OBJECT_ID(N'[b_IBlockSection]'))
ALTER TABLE [b_IBlockSection]  WITH CHECK ADD  CONSTRAINT [FK_b_IBlockSection_b_IBlock] FOREIGN KEY([IBlockId])
REFERENCES [b_IBlock] ([ID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlockSection_b_IBlock]') AND parent_object_id = OBJECT_ID(N'[b_IBlockSection]'))
ALTER TABLE [b_IBlockSection] CHECK CONSTRAINT [FK_b_IBlockSection_b_IBlock]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlockSection_b_IBlockSection]') AND parent_object_id = OBJECT_ID(N'[b_IBlockSection]'))
ALTER TABLE [b_IBlockSection]  WITH CHECK ADD  CONSTRAINT [FK_b_IBlockSection_b_IBlockSection] FOREIGN KEY([SectionId])
REFERENCES [b_IBlockSection] ([ID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlockSection_b_IBlockSection]') AND parent_object_id = OBJECT_ID(N'[b_IBlockSection]'))
ALTER TABLE [b_IBlockSection] CHECK CONSTRAINT [FK_b_IBlockSection_b_IBlockSection]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlock_TypeLang_b_Language]') AND parent_object_id = OBJECT_ID(N'[b_IBlockTypeLang]'))
ALTER TABLE [b_IBlockTypeLang]  WITH CHECK ADD  CONSTRAINT [FK_b_IBlock_TypeLang_b_Language] FOREIGN KEY([LanguageId])
REFERENCES [b_Language] ([ID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlock_TypeLang_b_Language]') AND parent_object_id = OBJECT_ID(N'[b_IBlockTypeLang]'))
ALTER TABLE [b_IBlockTypeLang] CHECK CONSTRAINT [FK_b_IBlock_TypeLang_b_Language]
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[IBlockElement_IncViewsCount]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'
CREATE PROCEDURE [IBlockElement_IncViewsCount](
	@elementId	INT,
	@inc		INT
	)
 AS
	SET NOCOUNT ON;
	
	UPDATE dbo.[b_IBlockElement] SET ViewsCount = ViewsCount + @inc WHERE ID = @elementId
' 
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[IBlock_SortSections]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'
CREATE Procedure [IBlock_SortSections]
(
	@IBlockId int,
	@SectionId int = 0,
	@Num int = 0,
	@Depth int = 0,
	@Active char(1) = ''Y''
)
AS
BEGIN
	IF (@SectionId > 0)
	BEGIN
		UPDATE b_IBlockSection SET
			UpdateDate = UpdateDate,
			LeftMargin = @Num
		WHERE ID = @SectionId
			AND IBlockId = @IBlockId
	END

	SET @Num = @Num + 1

	DECLARE @TempId int, @TempActive char(1), @TempActive1 char(1), @Depth1 int

	SET @Depth1 = @Depth + 1

	DECLARE tempCursor CURSOR LOCAL FOR 
		SELECT ID, Active
		FROM b_IBlockSection
		WHERE IBlockId = @IBlockId
			AND ((SectionId IS NULL AND @SectionId = 0)
				OR (SectionId = @SectionId AND @SectionId > 0))
		ORDER BY Sort, Name

	OPEN tempCursor

	FETCH NEXT FROM tempCursor INTO @TempId, @TempActive
	WHILE (@@FETCH_STATUS <> -1)
	BEGIN
		SET @TempActive1 = ''N''
		IF (@Active = ''Y'' AND @TempActive = ''Y'')
			SET @TempActive1 = ''Y''

		EXEC @Num = IBlock_SortSections @IBlockId, @TempId, @Num, @Depth1, @TempActive1

		FETCH NEXT FROM tempCursor INTO @TempId, @TempActive
	END

	CLOSE tempCursor
	DEALLOCATE tempCursor

	IF (@SectionId > 0)
	BEGIN
		UPDATE b_IBlockSection SET
			UpdateDate = UpdateDate,
			RightMargin = @Num,
			DepthLevel = @Depth,
			ActiveGlobal = @Active
		WHERE ID = @SectionId
			AND IBlockId = @IBlockId
	END

	SET @Num = @Num + 1
	        
	return @Num
END
' 
END
GO


/****** Object:  Trigger [b_IBlockElement_Update]    Script Date: 11/10/2009 19:59:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (
select * from sys.objects where object_id = OBJECT_ID(N'[b_IBlockElement_Update]') and type=N'TR'
)
BEGIN
EXEC dbo.sp_executesql @statement = N'
CREATE TRIGGER [b_IBlockElement_Update]
ON [b_IBlockElement] 
FOR UPDATE
AS
	IF (NOT UPDATE(UpdateDate))
	BEGIN
		UPDATE b_IBlockElement SET
			UpdateDate = GETDATE()
		FROM b_IBlockElement U, INSERTED I
		WHERE U.ID = I.ID
	END

	IF (UPDATE(PreviewImageId))
	BEGIN

		DELETE FROM b_File 
		WHERE ID IN (
			SELECT D.PreviewImageId
			FROM INSERTED I, DELETED D
			WHERE I.ID = D.ID
				AND I.PreviewImageId <> D.PreviewImageId
				AND D.PreviewImageId IS NOT NULL
		)

	END

	IF (UPDATE(DetailImageId))
	BEGIN

		DELETE FROM b_File 
		WHERE ID IN (
			SELECT D.DetailImageId
			FROM INSERTED I, DELETED D
			WHERE I.ID = D.ID
				AND I.DetailImageId <> D.DetailImageId
				AND D.DetailImageId IS NOT NULL
		)

	END'
END
GO
/****** Object:  Trigger [b_IBlockElement_Delete]    Script Date: 11/10/2009 19:59:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (
select * from sys.objects where object_id = OBJECT_ID(N'[b_IBlockElement_Delete]') and type=N'TR')
BEGIN
EXEC dbo.sp_executesql @statement = N'
CREATE TRIGGER [b_IBlockElement_Delete]
ON [b_IBlockElement]
FOR DELETE
AS

	DELETE FROM b_File 
	WHERE ID IN (
		SELECT D.PreviewImageId
		FROM DELETED D
		WHERE D.PreviewImageId IS NOT NULL
	)

	DELETE FROM b_File 
	WHERE ID IN (
		SELECT D.DetailImageId
		FROM DELETED D
		WHERE D.DetailImageId IS NOT NULL
	)'
END
GO


GO
IF NOT EXISTS (
select * from sys.objects where object_id = OBJECT_ID(N'[b_IBlockSection_Delete]') and type=N'TR')
BEGIN
EXEC dbo.sp_executesql @statement = N'
CREATE TRIGGER [b_IBlockSection_Delete]
ON [b_IBlockSection] 
FOR DELETE
AS
	DECLARE @IBlockId int, @RightMargin int

	IF (SELECT COUNT(DISTINCT IBlockId) FROM deleted) > 1
	BEGIN
		ROLLBACK
		RAISERROR(''Can not delete multiple records from more than one catalog'', 16, 1)
	END

	UPDATE b_IBlock	SET
		UpdateDate = UpdateDate
	WHERE ID IN (SELECT IBlockId FROM deleted)

	DECLARE C1 CURSOR LOCAL FOR
		SELECT IBlockId, RightMargin
		FROM deleted
		ORDER BY RightMargin DESC

	OPEN C1

	FETCH NEXT FROM C1 INTO @IBlockId, @RightMargin

	WHILE (@@FETCH_STATUS <> -1)
	BEGIN
		UPDATE b_IBlockSection SET
			LeftMargin = CASE WHEN s.LeftMargin >= @RightMargin THEN s.LeftMargin - 2 ELSE s.LeftMargin END,
			RightMargin = s.RightMargin - 2,
			UpdateDate = s.UpdateDate
		FROM b_IBlockSection s
		WHERE s.IBlockId = @IBlockId
			AND s.RightMargin >= @RightMargin

		FETCH NEXT FROM C1 INTO @IBlockId, @RightMargin
	END

	CLOSE C1
	DEALLOCATE C1


	DELETE FROM b_File 
	WHERE ID IN (
		SELECT D.ImageId
		FROM DELETED D
		WHERE D.ImageId IS NOT NULL
	)

	DELETE FROM b_File 
	WHERE ID IN (
		SELECT D.DetailImageId
		FROM DELETED D
		WHERE D.DetailImageId IS NOT NULL
	)'
END
GO
/****** Object:  Trigger [b_IBlockSection_Update]    Script Date: 11/10/2009 19:59:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (
select * from sys.objects where object_id = OBJECT_ID(N'[b_IBlockSection_Update]') and type=N'TR')
BEGIN
EXEC dbo.sp_executesql @statement = N'
CREATE TRIGGER [b_IBlockSection_Update]
ON [b_IBlockSection] 
FOR UPDATE
AS
	IF (NOT UPDATE(UpdateDate))
	BEGIN
		UPDATE b_IBlockSection SET
			UpdateDate = GETDATE()
		FROM b_IBlockSection U, INSERTED I, DELETED D
		WHERE U.ID = I.ID
			AND U.ID = D.ID
	END

	IF (UPDATE(IBlockId))
	BEGIN
		IF ((SELECT COUNT(''x'') FROM INSERTED i, DELETED d WHERE i.ID = d.ID AND i.IBlockId <> d.IBlockId) > 0)
		BEGIN
			ROLLBACK
			RAISERROR(''Can not change parent infoblock of the section'', 16, 1)
		END
	END

	IF (NOT UPDATE(LeftMargin) AND NOT UPDATE(RightMargin))
	BEGIN
		IF (UPDATE(Sort) OR UPDATE(Name) OR UPDATE(SectionId) OR UPDATE(Active))
		BEGIN
			DECLARE @IBlockId int

			UPDATE b_IBlock SET
				UpdateDate = UpdateDate
			WHERE ID IN (SELECT IBlockId FROM inserted)

			DECLARE C1 CURSOR LOCAL FOR
				SELECT DISTINCT IBlockId
				FROM INSERTED i

			OPEN C1
			FETCH NEXT FROM C1 INTO @IBlockId
			WHILE (@@FETCH_STATUS <> -1)
			BEGIN
				EXEC IBlock_SortSections @IBlockId
				FETCH NEXT FROM C1 INTO @IBlockId
			END
		END
	END

	IF (UPDATE(ImageId))
	BEGIN

		DELETE FROM b_File 
		WHERE ID IN (
			SELECT D.ImageId
			FROM INSERTED I, DELETED D
			WHERE I.ID = D.ID
				AND I.ImageId <> D.ImageId
				AND D.ImageId IS NOT NULL
		)

	END

	IF (UPDATE(DetailImageId))
	BEGIN

		DELETE FROM b_File 
		WHERE ID IN (
			SELECT D.DetailImageId
			FROM INSERTED I, DELETED D
			WHERE I.ID = D.ID
				AND I.DetailImageId <> D.DetailImageId
				AND D.DetailImageId IS NOT NULL
		)

	END'
END
GO
/****** Object:  Trigger [b_IBlock_Delete]    Script Date: 11/10/2009 19:59:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
GO
IF NOT EXISTS (
select * from sys.objects where object_id = OBJECT_ID(N'[b_IBlock_Delete]') and type=N'TR')
BEGIN
EXEC dbo.sp_executesql @statement = N'
CREATE TRIGGER [b_IBlock_Delete]
ON [b_IBlock]
FOR DELETE
AS

	DELETE FROM b_File 
	WHERE ID IN (
		SELECT D.ImageId
		FROM DELETED D
		WHERE D.ImageId IS NOT NULL
	)'
END
GO

GO
IF NOT EXISTS (
select * from sys.objects where object_id = OBJECT_ID(N'[b_IBlockSection_Insert]') and type=N'TR')
BEGIN
EXEC dbo.sp_executesql @statement = N'
CREATE TRIGGER [b_IBlockSection_Insert] 
ON [b_IBlockSection] 
FOR INSERT
AS
	DECLARE @IBlockId int

	UPDATE b_IBlock SET
		UpdateDate = UpdateDate
	WHERE ID IN (SELECT IBlockId FROM inserted)

	DECLARE C1 CURSOR LOCAL FOR
		SELECT DISTINCT IBlockId
		FROM INSERTED i

	OPEN C1
	FETCH NEXT FROM C1 INTO @IBlockId
	WHILE (@@FETCH_STATUS <> -1)
	BEGIN
		EXEC IBlock_SortSections @IBlockId
		FETCH NEXT FROM C1 INTO @IBlockId
	END


--	DECLARE tempCursor CURSOR LOCAL FOR
--		SELECT i.ID, i.SectionId, i.IBlockId, i.Name, i.Sort
--		FROM INSERTED i 
--
--	OPEN tempCursor
--
--	FETCH NEXT FROM temp_cursor INTO @ID, @ParentSectionId, @IBlockId, @Name, @Sort
--	WHILE (@@FETCH_STATUS <> -1)
--	BEGIN
--		SET @insertPoint = NULL
--
--		SELECT @insertPoint = s.LeftMargin
--		FROM b_IBlockSection s 
--		WHERE s.ID IN (
--			SELECT TOP 1 st.ID 
--			FROM b_IBlockSection st
--			WHERE st.LeftMargin IS NOT NULL
--				AND (
--					(st.SectionId = @ParentSectionId AND st.ID <> @ID 
--						AND (@Sort < st.Sort OR (@Sort = st.Sort AND @Name < st.Name))
--					)
--					OR (st.SectionId IS NULL AND @ParentSectionId IS NULL AND st.ID <> @ID 
--						AND (@Sort < st.Sort OR (@Sort = st.Sort AND @Name < st.Name))
--					)
--				)
--			ORDER BY CASE WHEN (st.SectionId <> @ParentSectionId OR @ParentSectionId IS NULL) THEN 1 ELSE 0 END, st.Sort, st.Name
--		)
--
--		IF (@insertPoint IS NULL)
--		BEGIN
--			SELECT @insertPoint = RightMargin
--			FROM b_IBlockSection
--			WHERE IBlockId = @IBlockId
--				AND ID = @ParentSectionId
--
--			IF (@insertPoint IS NULL)
--			BEGIN
--				SELECT @insertPoint = COUNT(''x'') * 2 + 1
--				FROM b_IBlockSection
--				WHERE IBlockId = @IBlockId
--					AND LeftMargin IS NOT NULL
--			END
--		END
--
--		UPDATE b_IBlockSection SET
--			LeftMargin = CASE WHEN LeftMargin >= @insertPoint THEN LeftMargin + 2 ELSE LeftMargin END,
--			RightMargin = RightMargin + 2
--		WHERE IBlockId = @IBlockId
--			AND RightMargin >= @insertPoint
--
--		UPDATE b_IBlockSection SET
--			LeftMargin = @insertPoint,
--			RightMargin = @insertPoint + 1
--		WHERE ID = @ID
--
--		FETCH NEXT FROM temp_cursor INTO @ID, @ParentSectionId, @IBlockId, @Name, @Sort
--	END'
END
GO
IF NOT EXISTS (
select * from sys.objects where object_id = OBJECT_ID(N'[b_IBlock_Update]') and type=N'TR'
)
BEGIN
EXEC dbo.sp_executesql @statement = N'
CREATE TRIGGER [b_IBlock_Update]
ON [b_IBlock]
FOR UPDATE
AS
	IF (NOT UPDATE(UpdateDate))
	BEGIN
		UPDATE b_IBlock SET
			UpdateDate = GETDATE()
		FROM b_IBlock U, INSERTED I, DELETED D
		WHERE U.ID = I.ID
			AND U.ID = D.ID
	END

	IF (UPDATE(ImageId))
	BEGIN

		DELETE FROM b_File 
		WHERE ID IN (
			SELECT D.ImageId
			FROM INSERTED I, DELETED D
			WHERE I.ID = D.ID
				AND I.ImageId <> D.ImageId
				AND D.ImageId IS NOT NULL
		)

	END'
END
GO

-- BEGINNING OF XML IMPORT
IF OBJECT_ID(N'IBlock_XmlImportPrepareTables', N'P') IS NOT NULL 
	DROP PROCEDURE IBlock_XmlImportPrepareTables;
GO
CREATE PROCEDURE IBlock_XmlImportPrepareTables
AS
BEGIN
	IF OBJECT_ID(N'b_IBlockXmlImport', N'U') IS NULL
	BEGIN
		CREATE TABLE b_IBlockXmlImport
		(
			ID INT IDENTITY(1,1) NOT NULL,
			CONSTRAINT PK_b_IBlockXmlImport PRIMARY KEY CLUSTERED(ID ASC)		
		);	
	END
	
	IF OBJECT_ID(N'b_IBlockXmlImportSection', N'U') IS NULL
	BEGIN
		CREATE TABLE b_IBlockXmlImportSection
		(
			ImportId INT NOT NULL,
			SectionId INT NOT NULL
		);
		CREATE CLUSTERED INDEX IX_b_IBlockXmlImportSection ON b_IBlockXmlImportSection (ImportId ASC, SectionId ASC);			
	END
	
	IF OBJECT_ID(N'b_IBlockXmlImportElement', N'U') IS NULL
	BEGIN
		CREATE TABLE b_IBlockXmlImportElement
		(
			ImportId INT NOT NULL,
			ElementId INT NOT NULL
		);
		CREATE CLUSTERED INDEX IX_b_IBlockXmlImportElement ON b_IBlockXmlImportElement (ImportId ASC, ElementId ASC);
	END	
END
GO

IF OBJECT_ID(N'IBlock_XmlImportCreate', N'P') IS NOT NULL 
	DROP PROCEDURE IBlock_XmlImportCreate;
GO

CREATE PROCEDURE IBlock_XmlImportCreate
AS
BEGIN
	INSERT INTO b_IBlockXmlImport DEFAULT VALUES;
	SELECT @@IDENTITY;
END
GO

IF OBJECT_ID(N'IBlock_XmlImportDelete', N'P') IS NOT NULL 
	DROP PROCEDURE IBlock_XmlImportDelete;
GO

CREATE PROCEDURE IBlock_XmlImportDelete
	@ImportId INT
AS
BEGIN
	DELETE FROM b_IBlockXmlImportSection WHERE ImportId = @ImportId;
	DELETE FROM b_IBlockXmlImportElement WHERE ImportId = @ImportId;
	DELETE FROM b_IBlockXmlImport WHERE ID = @ImportId;
END
GO


IF OBJECT_ID(N'IBlock_XmlImportAddSection', N'P') IS NOT NULL 
	DROP PROCEDURE IBlock_XmlImportAddSection;
GO

CREATE PROCEDURE IBlock_XmlImportAddSection
	@ImportId INT,
	@SectionId INT
AS
BEGIN
	IF NOT EXISTS(SELECT 'X' FROM b_IBlockXmlImportSection WHERE ImportId = @ImportId AND SectionId = @SectionId)
		INSERT INTO b_IBlockXmlImportSection(ImportId, SectionId) VALUES(@ImportId, @SectionId);
END
GO

IF OBJECT_ID(N'IBlock_XmlImportAddElement', N'P') IS NOT NULL 
	DROP PROCEDURE IBlock_XmlImportAddElement;
GO

CREATE PROCEDURE IBlock_XmlImportAddElement
	@ImportId INT,
	@ElementId INT
AS
BEGIN
	IF NOT EXISTS(SELECT 'X' FROM b_IBlockXmlImportElement WHERE ImportId = @ImportId AND ElementId = @ElementId)
		INSERT INTO b_IBlockXmlImportElement(ImportId, ElementId) VALUES(@ImportId, @ElementId);
END
GO

IF OBJECT_ID(N'IBlock_XmlImportGetOmittedElementIds', N'P') IS NOT NULL 
	DROP PROCEDURE IBlock_XmlImportGetOmittedElementIds;
GO

CREATE PROCEDURE IBlock_XmlImportGetOmittedElementIds
	@ImportId INT,
	@BlockId INT
AS
BEGIN
	SELECT ID FROM b_IBlockElement WHERE IBlockId = @BlockId AND ID NOT IN(SELECT ElementId FROM b_IBlockXmlImportElement WHERE ImportId = @ImportId);	
END
GO

IF OBJECT_ID(N'IBlock_XmlImportGetOmittedSectionIds', N'P') IS NOT NULL 
	DROP PROCEDURE IBlock_XmlImportGetOmittedSectionIds;
GO

CREATE PROCEDURE IBlock_XmlImportGetOmittedSectionIds
	@ImportId INT,
	@BlockId INT
AS
BEGIN
	SELECT ID FROM b_IBlockSection WHERE IBlockId = @BlockId AND ID NOT IN(SELECT SectionId FROM b_IBlockXmlImportSection WHERE ImportId = @ImportId);
END
GO

-- END OF XML IMPORT
