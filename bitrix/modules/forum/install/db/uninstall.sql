IF OBJECT_ID (N'Forum_SynchronizeForum', N'P') IS NOT NULL
	DROP PROCEDURE Forum_SynchronizeForum
GO

IF OBJECT_ID (N'Forum_SynchronizeTopic', N'P') IS NOT NULL
	DROP PROCEDURE Forum_SynchronizeTopic
GO

IF OBJECT_ID (N'Forum_SynchronizeUser', N'P') IS NOT NULL
	DROP PROCEDURE Forum_SynchronizeUser
GO

IF OBJECT_ID(N'ForumTopic_IncrementViews', N'P') IS NOT NULL
	DROP PROCEDURE ForumTopic_IncrementViews;
GO

IF OBJECT_ID(N'FK_ForumInSite_ForumId_Forum_Id', N'F') IS NOT NULL 
	ALTER TABLE b_ForumInSite DROP CONSTRAINT FK_ForumInSite_ForumId_Forum_Id
GO

IF OBJECT_ID(N'FK_ForumInSite_SiteId_Site_Id', N'F') IS NOT NULL 
	ALTER TABLE b_ForumInSite DROP CONSTRAINT FK_ForumInSite_SiteId_Site_Id
GO

IF OBJECT_ID(N'FK_ForumTopic_ForumId_Forum_Id', N'F') IS NOT NULL 
	ALTER TABLE b_ForumTopic DROP CONSTRAINT FK_ForumTopic_ForumId_Forum_Id
GO

IF OBJECT_ID(N'FK_ForumPost_ForumId_Forum_Id', N'F') IS NOT NULL 
	ALTER TABLE b_ForumPost DROP CONSTRAINT FK_ForumPost_ForumId_Forum_Id
GO

IF OBJECT_ID(N'FK_ForumPost_TopicId_ForumTopic_Id', N'F') IS NOT NULL 
	ALTER TABLE b_ForumPost DROP CONSTRAINT FK_ForumPost_TopicId_ForumTopic_Id
GO

IF OBJECT_ID(N'FK_ForumSubscription_ForumId_Forum_Id', N'F') IS NOT NULL 
	ALTER TABLE b_ForumSubscription DROP CONSTRAINT FK_ForumSubscription_ForumId_Forum_Id
GO

IF OBJECT_ID(N'b_ForumSubscription', N'U') IS NOT NULL 
	DROP TABLE b_ForumSubscription;
GO

IF OBJECT_ID(N'b_ForumCategory', N'U') IS NOT NULL 
	DROP TABLE b_ForumCategory;
GO

IF OBJECT_ID(N'b_Forum', N'U') IS NOT NULL 
	DROP TABLE b_Forum;
GO

IF OBJECT_ID(N'b_ForumInSite', N'U') IS NOT NULL 
	DROP TABLE b_ForumInSite;
GO

IF OBJECT_ID(N'b_ForumUser', N'U') IS NOT NULL 
	DROP TABLE b_ForumUser;
GO

IF OBJECT_ID(N'b_ForumTopic', N'U') IS NOT NULL 
	DROP TABLE b_ForumTopic;
GO

IF OBJECT_ID(N'b_ForumPost', N'U') IS NOT NULL 
	DROP TABLE b_ForumPost;
GO

/*--- BEGINNING OF RATINGS   ---*/
IF OBJECT_ID (N'ForumActivity_GetTopicCountInRange', N'FN') IS NOT NULL
	DROP FUNCTION dbo.ForumActivity_GetTopicCountInRange;
GO

IF OBJECT_ID (N'ForumActivity_GetPostCountInRange', N'FN') IS NOT NULL
	DROP FUNCTION dbo.ForumActivity_GetPostCountInRange;
GO

IF OBJECT_ID (N'ForumActivity_Synchronize', N'P') IS NOT NULL
	DROP PROCEDURE ForumActivity_Synchronize;
GO

IF OBJECT_ID (N'ForumActivity_SynchronizePostByUser', N'P') IS NOT NULL
	DROP PROCEDURE ForumActivity_SynchronizePostByUser
GO

IF OBJECT_ID (N'ForumActivity_SynchronizeTopicByUser', N'P') IS NOT NULL
	DROP PROCEDURE ForumActivity_SynchronizeTopicByUser
GO

IF OBJECT_ID (N'ForumActivity_SynchronizeByUser', N'P') IS NOT NULL
	DROP PROCEDURE ForumActivity_SynchronizeByUser;
GO

IF OBJECT_ID (N'ForumActivity_Engage', N'P') IS NOT NULL
	DROP PROCEDURE ForumActivity_Engage
GO

IF OBJECT_ID (N'ForumActivity_Disengage', N'P') IS NOT NULL
	DROP PROCEDURE ForumActivity_Disengage
GO

IF OBJECT_ID (N'ForumActivity_Calculate', N'P') IS NOT NULL
	DROP PROCEDURE ForumActivity_Calculate;
GO

IF OBJECT_ID (N'ForumActivity_EngageForUser', N'P') IS NOT NULL
	DROP PROCEDURE ForumActivity_EngageForUser;
GO

IF OBJECT_ID (N'ForumActivity_DisengageForUser', N'P') IS NOT NULL
	DROP PROCEDURE ForumActivity_DisengageForUser;
GO

IF OBJECT_ID (N'ForumTopicVoting_GetEngagingUsers', N'P') IS NOT NULL
	DROP PROCEDURE ForumTopicVoting_GetEngagingUsers;
GO

IF OBJECT_ID (N'ForumPostVoting_GetEngagingUsers', N'P') IS NOT NULL
	DROP PROCEDURE ForumPostVoting_GetEngagingUsers;
GO

IF OBJECT_ID (N'ForumActivity_GetEngagingUsers', N'P') IS NOT NULL
	DROP PROCEDURE ForumActivity_GetEngagingUsers;
GO

IF OBJECT_ID (N'b_ForumActivity', N'U') IS NOT NULL
	DROP TABLE b_ForumActivity;
GO
/*--- END OF RATINGS   ---*/
