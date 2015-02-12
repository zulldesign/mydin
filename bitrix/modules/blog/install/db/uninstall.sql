IF OBJECT_ID (N'Blog_SynchronizeBlog', N'P') IS NOT NULL
	DROP PROCEDURE Blog_SynchronizeBlog
GO

IF OBJECT_ID (N'BlogCategory_DeleteBlogInCategory', N'P') IS NOT NULL
	DROP PROCEDURE BlogCategory_DeleteBlogInCategory
GO

IF OBJECT_ID (N'BlogComment_SynchronizeNestedSets', N'P') IS NOT NULL
	DROP PROCEDURE BlogComment_SynchronizeNestedSets
GO

IF OBJECT_ID (N'BlogPost_SynchronizePost', N'P') IS NOT NULL
	DROP PROCEDURE BlogPost_SynchronizePost
GO

IF OBJECT_ID (N'BlogUser_Synchronize', N'P') IS NOT NULL
	DROP PROCEDURE BlogUser_Synchronize
GO

IF OBJECT_ID (N'BlogFile_DeleteFile', N'P') IS NOT NULL
	DROP PROCEDURE BlogFile_DeleteFile
GO

IF OBJECT_ID (N'BlogFile_DeleteFiles', N'P') IS NOT NULL
	DROP PROCEDURE BlogFile_DeleteFiles
GO

IF OBJECT_ID (N'BlogFile_Save', N'P') IS NOT NULL
	DROP PROCEDURE BlogFile_Save
GO

IF OBJECT_ID (N'BlogFile_Synchronize', N'P') IS NOT NULL
	DROP PROCEDURE BlogFile_Synchronize
GO

IF OBJECT_ID(N'BlogPost_IncrementViewCount', N'P') IS NOT NULL
	DROP PROCEDURE BlogPost_IncrementViewCount;
GO

IF OBJECT_ID(N'BlogPostSyndication_Synchronize', N'P') IS NOT NULL
	DROP PROCEDURE BlogPostSyndication_Synchronize;
GO

IF OBJECT_ID(N'BlogPostSyndication_FindByGuidAndBlogId', N'P') IS NOT NULL
	DROP PROCEDURE BlogPostSyndication_FindByGuidAndBlogId;
GO

IF OBJECT_ID(N'BlogPostSyndication_Delete', N'P') IS NOT NULL
	DROP PROCEDURE BlogPostSyndication_Delete;
GO

IF OBJECT_ID(N'BlogSyndication_Delete', N'P') IS NOT NULL
	DROP PROCEDURE BlogSyndication_Delete;
GO

IF OBJECT_ID(N'FK_b_BlogCategoryInSite_b_BlogCategory', N'F') IS NOT NULL 
	ALTER TABLE b_BlogCategoryInSite DROP CONSTRAINT FK_b_BlogCategoryInSite_b_BlogCategory
GO

IF OBJECT_ID(N'FK_b_BlogCategoryInSite_b_Site', N'F') IS NOT NULL 
	ALTER TABLE b_BlogCategoryInSite DROP CONSTRAINT FK_b_BlogCategoryInSite_b_Site
GO

IF OBJECT_ID(N'FK_b_BlogUser_b_Users', N'F') IS NOT NULL 
	ALTER TABLE b_BlogUser DROP CONSTRAINT FK_b_BlogUser_b_Users
GO

IF OBJECT_ID(N'FK_b_Blog_b_BlogUser', N'F') IS NOT NULL 
	ALTER TABLE b_Blog DROP CONSTRAINT FK_b_Blog_b_BlogUser
GO

IF OBJECT_ID(N'FK_b_BlogInCategory_b_Blog', N'F') IS NOT NULL 
	ALTER TABLE b_BlogInCategory DROP CONSTRAINT FK_b_BlogInCategory_b_Blog
GO

IF OBJECT_ID(N'FK_b_BlogInCategory_b_BlogCategory', N'F') IS NOT NULL 
	ALTER TABLE b_BlogInCategory DROP CONSTRAINT FK_b_BlogInCategory_b_BlogCategory
GO

IF OBJECT_ID(N'FK_b_BlogPost_b_Blog', N'F') IS NOT NULL 
	ALTER TABLE b_BlogPost DROP CONSTRAINT FK_b_BlogPost_b_Blog
GO

IF OBJECT_ID(N'FK_b_Post_b_BlogUser', N'F') IS NOT NULL 
	ALTER TABLE b_BlogPost DROP CONSTRAINT FK_b_Post_b_BlogUser
GO

IF OBJECT_ID(N'FK_b_BlogComment_b_Blog', N'F') IS NOT NULL 
	ALTER TABLE b_BlogComment DROP CONSTRAINT FK_b_BlogComment_b_Blog
GO

IF OBJECT_ID(N'FK_b_BlogComment_b_BlogPost', N'F') IS NOT NULL 
	ALTER TABLE b_BlogComment DROP CONSTRAINT FK_b_BlogComment_b_BlogPost
GO

IF OBJECT_ID(N'FK_b_BlogFile_b_Blog', N'F') IS NOT NULL 
	ALTER TABLE b_BlogFile DROP CONSTRAINT FK_b_BlogFile_b_Blog
GO

IF OBJECT_ID(N'FK_b_BlogFile_b_BlogPost', N'F') IS NOT NULL 
	ALTER TABLE b_BlogFile DROP CONSTRAINT FK_b_BlogFile_b_BlogPost
GO

IF OBJECT_ID(N'FK_b_BlogPostSyndication_b_Blog', N'F') IS NOT NULL 
	ALTER TABLE b_BlogPostSyndication DROP CONSTRAINT FK_b_BlogPostSyndication_b_Blog
GO

IF OBJECT_ID(N'FK_b_BlogPostSyndication_b_BlogPost', N'F') IS NOT NULL 
	ALTER TABLE b_BlogPostSyndication DROP CONSTRAINT FK_b_BlogPostSyndication_b_BlogPost
GO

IF OBJECT_ID(N'b_BlogPostSyndication', N'U') IS NOT NULL 
	DROP TABLE b_BlogPostSyndication
GO

IF OBJECT_ID(N'FK_b_BlogSyndication_b_Blog', N'F') IS NOT NULL 
	ALTER TABLE b_BlogSyndication DROP CONSTRAINT FK_b_BlogSyndication_b_Blog
GO

IF OBJECT_ID(N'b_BlogSyndication', N'U') IS NOT NULL 
	DROP TABLE b_BlogSyndication
GO

IF OBJECT_ID(N'b_BlogFile', N'U') IS NOT NULL 
	DROP TABLE b_BlogFile
GO

IF OBJECT_ID(N'b_BlogCategory', N'U') IS NOT NULL 
	DROP TABLE b_BlogCategory
GO

IF OBJECT_ID(N'b_BlogCategoryInSite', N'U') IS NOT NULL 
	DROP TABLE b_BlogCategoryInSite
GO

IF OBJECT_ID(N'b_BlogUser', N'U') IS NOT NULL 
	DROP TABLE b_BlogUser
GO

IF OBJECT_ID(N'b_Blog', N'U') IS NOT NULL 
	DROP TABLE b_Blog
GO

IF OBJECT_ID(N'b_BlogInCategory', N'U') IS NOT NULL 
	DROP TABLE b_BlogInCategory
GO

IF OBJECT_ID(N'b_BlogPost', N'U') IS NOT NULL 
	DROP TABLE b_BlogPost
GO

IF OBJECT_ID(N'b_BlogComment', N'U') IS NOT NULL 
	DROP TABLE b_BlogComment
GO

/*--- BEGINNING OF RATINGS   ---*/
IF OBJECT_ID (N'BlogVoting_GetEngagingUsers', N'P') IS NOT NULL
	DROP PROCEDURE BlogVoting_GetEngagingUsers;
GO

IF OBJECT_ID (N'BlogPostVoting_GetEngagingUsers', N'P') IS NOT NULL
	DROP PROCEDURE BlogPostVoting_GetEngagingUsers;
GO

IF OBJECT_ID (N'BlogCommentVoting_GetEngagingUsers', N'P') IS NOT NULL
	DROP PROCEDURE BlogCommentVoting_GetEngagingUsers;
GO

IF OBJECT_ID (N'BloggingActivity_GetPostCountInRange', N'FN') IS NOT NULL
	DROP FUNCTION dbo.BloggingActivity_GetPostCountInRange
GO

IF OBJECT_ID (N'BloggingActivity_GetCommentCountInRange', N'FN') IS NOT NULL
	DROP FUNCTION dbo.BloggingActivity_GetCommentCountInRange
GO

IF OBJECT_ID (N'BloggingActivity_SynchronizePost', N'P') IS NOT NULL
	DROP PROCEDURE BloggingActivity_SynchronizePost;
GO

IF OBJECT_ID (N'BloggingActivity_SynchronizeComment', N'P') IS NOT NULL
	DROP PROCEDURE BloggingActivity_SynchronizeComment
GO

IF OBJECT_ID (N'BloggingActivity_SynchronizePostByUser', N'P') IS NOT NULL
	DROP PROCEDURE BloggingActivity_SynchronizePostByUser
GO

IF OBJECT_ID (N'BloggingActivity_Synchronize', N'P') IS NOT NULL
	DROP PROCEDURE BloggingActivity_Synchronize;
GO

IF OBJECT_ID (N'BloggingActivity_SynchronizeCommentByUser', N'P') IS NOT NULL
	DROP PROCEDURE BloggingActivity_SynchronizeCommentByUser
GO

IF OBJECT_ID (N'BloggingActivity_SynchronizeByUser', N'P') IS NOT NULL
	DROP PROCEDURE BloggingActivity_SynchronizeByUser;
GO

IF OBJECT_ID (N'BloggingActivity_Engage', N'P') IS NOT NULL
	DROP PROCEDURE BloggingActivity_Engage
GO


IF OBJECT_ID (N'BloggingActivity_EngageForUser', N'P') IS NOT NULL
	DROP PROCEDURE BloggingActivity_EngageForUser;
GO

IF OBJECT_ID (N'BloggingActivity_DisengageForUser', N'P') IS NOT NULL
	DROP PROCEDURE BloggingActivity_DisengageForUser;
GO

IF OBJECT_ID (N'BloggingActivity_Disengage', N'P') IS NOT NULL
	DROP PROCEDURE BloggingActivity_Disengage
GO

IF OBJECT_ID (N'BloggingActivity_Calculate', N'P') IS NOT NULL
	DROP PROCEDURE BloggingActivity_Calculate;
GO

IF OBJECT_ID (N'BloggingActivity_GetEngagingUsers', N'P') IS NOT NULL
	DROP PROCEDURE BloggingActivity_GetEngagingUsers;
GO

IF OBJECT_ID (N'b_BloggingActivity', N'U') IS NOT NULL
	DROP TABLE b_BloggingActivity;
GO
/*--- END OF RATINGS   ---*/

/*--- BEGINNING OF MODERATION   ---*/
IF OBJECT_ID(N'b_BlogSettings', N'U') IS NOT NULL
	DROP TABLE b_BlogSettings;
GO
/*--- END OF MODERATION   ---*/



/*--- TEAM BLOGS ---*/
IF OBJECT_ID (N'b_BlogUserGroup', N'U') IS NOT NULL
	DROP TABLE b_BlogUserGroup	
GO

IF OBJECT_ID (N'b_BlogUser2Group', N'U') IS NOT NULL
	DROP TABLE b_BlogUser2Group	
GO

IF OBJECT_ID (N'b_BlogUserPermission', N'U') IS NOT NULL
	DROP TABLE b_BlogUserPermission	
GO

IF OBJECT_ID (N'BlogUser_GetPermissionsGuest', N'P') IS NOT NULL
DROP PROCEDURE BlogUser_GetPermissionsGuest
GO

IF OBJECT_ID (N'BlogUser_GetPermissions', N'P') IS NOT NULL
DROP PROCEDURE BlogUser_GetPermissions
GO

IF OBJECT_ID (N'BlogUser_GetPermittedBlogsGuest', N'P') IS NOT NULL
DROP PROCEDURE BlogUser_GetPermittedBlogsGuest
GO

IF OBJECT_ID (N'BlogUser_GetPermittedBlogs', N'P') IS NOT NULL
DROP PROCEDURE BlogUser_GetPermittedBlogs
GO
/*--- END OF TEAM BLOGS ---*/