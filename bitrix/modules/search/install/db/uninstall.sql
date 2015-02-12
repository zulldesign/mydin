IF OBJECT_ID (N'Search_SetStems', N'P') IS NOT NULL
	DROP PROCEDURE [Search_SetStems]
GO

IF OBJECT_ID (N'Search_GetInvalidTags', N'P') IS NOT NULL
	DROP PROCEDURE [Search_GetInvalidTags]
GO

IF OBJECT_ID (N'Search_CreateTag', N'P') IS NOT NULL
	DROP PROCEDURE [Search_CreateTag]
GO

IF OBJECT_ID(N'Search_SetTags', N'P') IS NOT NULL
	DROP PROCEDURE [Search_SetTags]
GO

IF OBJECT_ID (N'Search_DeleteAll', N'P') IS NOT NULL
	DROP PROCEDURE [Search_DeleteAll]
GO

IF OBJECT_ID(N'Search_DeleteContent', N'P') IS NOT NULL
	DROP PROCEDURE Search_DeleteContent
GO

IF OBJECT_ID(N'Search_GetStemsFreq', N'P') IS NOT NULL
	DROP PROCEDURE Search_GetStemsFreq
GO

IF OBJECT_ID(N'Search_DeleteSites', N'P') IS NOT NULL
	DROP PROCEDURE Search_DeleteSites
GO

IF OBJECT_ID(N'b_Search_ContentFileRoles', N'U') IS NOT NULL 
	DROP TABLE b_Search_ContentFileRoles;
GO

IF OBJECT_ID(N'b_Search_ContentStemIndex', N'U') IS NOT NULL 
	DROP TABLE b_Search_ContentStemIndex;
GO

IF OBJECT_ID(N'b_Search_Content', N'U') IS NOT NULL 
	DROP TABLE b_Search_Content;
GO

IF OBJECT_ID(N'b_Search_ContentSites', N'U') IS NOT NULL 
	DROP TABLE b_Search_ContentSites;
GO

IF OBJECT_ID(N'b_Search_ContentTags', N'U') IS NOT NULL 
	DROP TABLE b_Search_ContentTags;
GO

IF OBJECT_ID(N'b_Search_Tags', N'U') IS NOT NULL 
	DROP TABLE b_Search_Tags;
GO

IF OBJECT_ID(N'b_Search_StemFrequencyCache', N'U') IS NOT NULL 
	DROP TABLE b_Search_StemFrequencyCache;
GO