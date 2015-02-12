GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[IBlockElement_IncViewsCount]') AND type in (N'P', N'PC'))
DROP PROCEDURE [IBlockElement_IncViewsCount]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[IBlock_SortSections]') AND type in (N'P', N'PC'))
DROP PROCEDURE [IBlock_SortSections]

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlock_b_IBlock_Type]') AND parent_object_id = OBJECT_ID(N'[b_IBlock]'))
ALTER TABLE [b_IBlock] DROP CONSTRAINT [FK_b_IBlock_b_IBlock_Type]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlockElement_b_IBlock]') AND parent_object_id = OBJECT_ID(N'[b_IBlockElement]'))
ALTER TABLE [b_IBlockElement] DROP CONSTRAINT [FK_b_IBlockElement_b_IBlock]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlockElementInSection_b_IBlockElement]') AND parent_object_id = OBJECT_ID(N'[b_IBlockElementInSection]'))
ALTER TABLE [b_IBlockElementInSection] DROP CONSTRAINT [FK_b_IBlockElementInSection_b_IBlockElement]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlockElementInSection_b_IBlockSection]') AND parent_object_id = OBJECT_ID(N'[b_IBlockElementInSection]'))
ALTER TABLE [b_IBlockElementInSection] DROP CONSTRAINT [FK_b_IBlockElementInSection_b_IBlockSection]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlock_Site_b_IBlock]') AND parent_object_id = OBJECT_ID(N'[b_IBlockInSite]'))
ALTER TABLE [b_IBlockInSite] DROP CONSTRAINT [FK_b_IBlock_Site_b_IBlock]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlock_Site_b_Site]') AND parent_object_id = OBJECT_ID(N'[b_IBlockInSite]'))
ALTER TABLE [b_IBlockInSite] DROP CONSTRAINT [FK_b_IBlock_Site_b_Site]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlockSection_b_IBlock]') AND parent_object_id = OBJECT_ID(N'[b_IBlockSection]'))
ALTER TABLE [b_IBlockSection] DROP CONSTRAINT [FK_b_IBlockSection_b_IBlock]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlockSection_b_IBlockSection]') AND parent_object_id = OBJECT_ID(N'[b_IBlockSection]'))
ALTER TABLE [b_IBlockSection] DROP CONSTRAINT [FK_b_IBlockSection_b_IBlockSection]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlock_TypeLang_b_Language]') AND parent_object_id = OBJECT_ID(N'[b_IBlockTypeLang]'))
ALTER TABLE [b_IBlockTypeLang] DROP CONSTRAINT [FK_b_IBlock_TypeLang_b_Language]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlock_Site_b_IBlock]') AND parent_object_id = OBJECT_ID(N'[b_IBlockInSite]'))
ALTER TABLE [b_IBlockInSite] DROP CONSTRAINT [FK_b_IBlock_Site_b_IBlock]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlock_Site_b_Site]') AND parent_object_id = OBJECT_ID(N'[b_IBlockInSite]'))
ALTER TABLE [b_IBlockInSite] DROP CONSTRAINT [FK_b_IBlock_Site_b_Site]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[b_IBlockInSite]') AND type in (N'U'))
DROP TABLE [b_IBlockInSite]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlock_TypeLang_b_Language]') AND parent_object_id = OBJECT_ID(N'[b_IBlockTypeLang]'))
ALTER TABLE [b_IBlockTypeLang] DROP CONSTRAINT [FK_b_IBlock_TypeLang_b_Language]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[b_IBlockTypeLang]') AND type in (N'U'))
DROP TABLE [b_IBlockTypeLang]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlockElementInSection_b_IBlockElement]') AND parent_object_id = OBJECT_ID(N'[b_IBlockElementInSection]'))
ALTER TABLE [b_IBlockElementInSection] DROP CONSTRAINT [FK_b_IBlockElementInSection_b_IBlockElement]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlockElementInSection_b_IBlockSection]') AND parent_object_id = OBJECT_ID(N'[b_IBlockElementInSection]'))
ALTER TABLE [b_IBlockElementInSection] DROP CONSTRAINT [FK_b_IBlockElementInSection_b_IBlockSection]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlockSection_b_IBlock]') AND parent_object_id = OBJECT_ID(N'[b_IBlockSection]'))
ALTER TABLE [b_IBlockSection] DROP CONSTRAINT [FK_b_IBlockSection_b_IBlock]
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlockSection_b_IBlockSection]') AND parent_object_id = OBJECT_ID(N'[b_IBlockSection]'))
ALTER TABLE [b_IBlockSection] DROP CONSTRAINT [FK_b_IBlockSection_b_IBlockSection]
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_b_IBlockSection_UpdateDate]') AND type = 'D')
BEGIN
ALTER TABLE [b_IBlockSection] DROP CONSTRAINT [DF_b_IBlockSection_UpdateDate]
END
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_b_IBlockSection_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [b_IBlockSection] DROP CONSTRAINT [DF_b_IBlockSection_CreateDate]
END
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_b_IBlockSection_Active]') AND type = 'D')
BEGIN
ALTER TABLE [b_IBlockSection] DROP CONSTRAINT [DF_b_IBlockSection_Active]
END
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_b_IBlockSection_ActiveGlobal]') AND type = 'D')
BEGIN
ALTER TABLE [b_IBlockSection] DROP CONSTRAINT [DF_b_IBlockSection_ActiveGlobal]
END
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_b_IBlockSection_Sort]') AND type = 'D')
BEGIN
ALTER TABLE [b_IBlockSection] DROP CONSTRAINT [DF_b_IBlockSection_Sort]
END
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_b_IBlockSection_DescriptionType]') AND type = 'D')
BEGIN
ALTER TABLE [b_IBlockSection] DROP CONSTRAINT [DF_b_IBlockSection_DescriptionType]
END
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_b_IBlockElement_b_IBlock]') AND parent_object_id = OBJECT_ID(N'[b_IBlockElement]'))
ALTER TABLE [b_IBlockElement] DROP CONSTRAINT [FK_b_IBlockElement_b_IBlock]
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_b_IBlockElement_UpdateDate]') AND type = 'D')
BEGIN
ALTER TABLE [b_IBlockElement] DROP CONSTRAINT [DF_b_IBlockElement_UpdateDate]
END
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_b_IBlockElement_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [b_IBlockElement] DROP CONSTRAINT [DF_b_IBlockElement_CreateDate]
END
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_b_IBlockElement_Active]') AND type = 'D')
BEGIN
ALTER TABLE [b_IBlockElement] DROP CONSTRAINT [DF_b_IBlockElement_Active]
END
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_b_IBlockElement_Sort]') AND type = 'D')
BEGIN
ALTER TABLE [b_IBlockElement] DROP CONSTRAINT [DF_b_IBlockElement_Sort]
END
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_b_IBlockElement_PreviewTextType]') AND type = 'D')
BEGIN
ALTER TABLE [b_IBlockElement] DROP CONSTRAINT [DF_b_IBlockElement_PreviewTextType]
END
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_b_IBlockElement_DetailTextType]') AND type = 'D')
BEGIN
ALTER TABLE [b_IBlockElement] DROP CONSTRAINT [DF_b_IBlockElement_DetailTextType]
END
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__b_IBlockE__Views__24285DB4]') AND type = 'D')
BEGIN
ALTER TABLE [b_IBlockElement] DROP CONSTRAINT [DF__b_IBlockE__Views__24285DB4]
END
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys 
WHERE object_id = OBJECT_ID(N'[FK_b_IBlock_b_IBlock_Type]') AND parent_object_id = OBJECT_ID(N'[b_IBlock]'))
ALTER TABLE [b_IBlock] DROP CONSTRAINT [FK_b_IBlock_b_IBlock_Type]
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects 
WHERE id = OBJECT_ID(N'[DF_b_IBlock_UpdateDate]') AND type = 'D')
BEGIN
ALTER TABLE [b_IBlock] DROP CONSTRAINT [DF_b_IBlock_UpdateDate]
END
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects 
WHERE id = OBJECT_ID(N'[DF_b_IBlock_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [b_IBlock] DROP CONSTRAINT [DF_b_IBlock_CreateDate]
END
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_b_IBlock_Active]') AND type = 'D')
BEGIN
ALTER TABLE [b_IBlock] DROP CONSTRAINT [DF_b_IBlock_Active]
END
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_b_IBlock_Sort]') AND type = 'D')
BEGIN
ALTER TABLE [b_IBlock] DROP CONSTRAINT [DF_b_IBlock_Sort]
END
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_b_IBlock_DescriptionType]') AND type = 'D')
BEGIN
ALTER TABLE [b_IBlock] DROP CONSTRAINT [DF_b_IBlock_DescriptionType]
END
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_b_IBlock_IndexContent]') AND type = 'D')
BEGIN
ALTER TABLE [b_IBlock] DROP CONSTRAINT [DF_b_IBlock_IndexContent]
END
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_b_IBlock_Type_HaveSections]') AND type = 'D')
BEGIN
ALTER TABLE [b_IBlockType] DROP CONSTRAINT [DF_b_IBlock_Type_HaveSections]
END
GO
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_b_IBlock_Type_Sort]') AND type = 'D')
BEGIN
ALTER TABLE [b_IBlockType] DROP CONSTRAINT [DF_b_IBlock_Type_Sort]
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[b_IBlockInSite]') AND type in (N'U'))
DROP TABLE [b_IBlockInSite]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[b_IBlockTypeLang]') AND type in (N'U'))
DROP TABLE [b_IBlockTypeLang]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[b_IBlockElementInSection]') AND type in (N'U'))
DROP TABLE [b_IBlockElementInSection]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[b_IBlockSection]') AND type in (N'U'))
DROP TABLE [b_IBlockSection]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[b_IBlockElement]') AND type in (N'U'))
DROP TABLE [b_IBlockElement]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[b_IBlock]') AND type in (N'U'))
DROP TABLE [b_IBlock]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[b_IBlockType]') AND type in (N'U'))
DROP TABLE [b_IBlockType]
GO

DECLARE @SQL VARCHAR(MAX)

SET @SQL = ''

SELECT @SQL = @SQL + ' IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''['+
    TABLE_NAME+']'') AND type in (N''U'')) DROP TABLE dbo.' + TABLE_NAME
FROM 
    INFORMATION_SCHEMA.TABLES
WHERE 
    TABLE_SCHEMA='dbo' and (TABLE_NAME LIKE 'b_cts_iblock%' OR TABLE_NAME LIKE 'b_ctm_iblock%')
    
EXEC(@SQL)
    
GO

-- BEGINNING OF XML IMPORT
IF OBJECT_ID (N'b_IBlockXmlImport', N'U') IS NOT NULL	
    DROP TABLE b_IBlockXmlImport;
GO

IF OBJECT_ID (N'b_IBlockXmlImportSection', N'U') IS NOT NULL	
BEGIN
    IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_IBlockXmlImportSection')
        DROP INDEX IX_b_IBlockXmlImportSection ON b_IBlockXmlImportSection;

    DROP TABLE b_IBlockXmlImportSection;
END
GO

IF OBJECT_ID (N'b_IBlockXmlImportElement', N'U') IS NOT NULL	
BEGIN
    IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_b_IBlockXmlImportElement')
        DROP INDEX IX_b_IBlockXmlImportElement ON b_IBlockXmlImportElement;

    DROP TABLE b_IBlockXmlImportElement;
END
GO

IF OBJECT_ID(N'IBlock_XmlImportAddElement', N'P') IS NOT NULL 
    DROP PROCEDURE IBlock_XmlImportAddElement;
GO

IF OBJECT_ID(N'IBlock_XmlImportAddSection', N'P') IS NOT NULL 
    DROP PROCEDURE IBlock_XmlImportAddSection;
GO

IF OBJECT_ID(N'IBlock_XmlImportCreate', N'P') IS NOT NULL 
    DROP PROCEDURE IBlock_XmlImportCreate;
GO

IF OBJECT_ID(N'IBlock_XmlImportDelete', N'P') IS NOT NULL 
    DROP PROCEDURE IBlock_XmlImportDelete;
GO

IF OBJECT_ID(N'IBlock_XmlImportGetOmittedElementIds', N'P') IS NOT NULL 
    DROP PROCEDURE IBlock_XmlImportGetOmittedElementIds;
GO

IF OBJECT_ID(N'IBlock_XmlImportGetOmittedSectionIds', N'P') IS NOT NULL 
    DROP PROCEDURE IBlock_XmlImportGetOmittedSectionIds;
GO

IF OBJECT_ID(N'IBlock_XmlImportPrepareTables', N'P') IS NOT NULL 
    DROP PROCEDURE IBlock_XmlImportPrepareTables;
GO

-- END OF XML IMPORT