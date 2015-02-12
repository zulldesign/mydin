/*--- BEGINNING OF RATINGS   ---*/
IF OBJECT_ID (N'RatingVoting_Synchronize', N'P') IS NOT NULL
	DROP PROCEDURE RatingVoting_Synchronize;
GO

IF OBJECT_ID (N'RatingVote_Count', N'P') IS NOT NULL
	DROP PROCEDURE RatingVote_Count;
GO

IF OBJECT_ID (N'RatingVoting_GetTotals', N'P') IS NOT NULL
	DROP PROCEDURE RatingVoting_GetTotals;
GO

IF OBJECT_ID (N'b_RatingVote', N'U') IS NOT NULL
	DROP TABLE b_RatingVote;
GO

IF OBJECT_ID (N'b_RatingVoting', N'U') IS NOT NULL
	DROP TABLE b_RatingVoting;
GO

IF OBJECT_ID (N'b_RatingVotingBoundUnit', N'U') IS NOT NULL
	DROP TABLE b_RatingVotingBoundUnit;
GO

IF OBJECT_ID (N'b_RatingVotingGroupTotal', N'U') IS NOT NULL
	DROP TABLE b_RatingVotingGroupTotal;
GO
/*--- END OF RATINGS   ---*/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_b_PrivateMessageMappings_TopicId_b_PrivateMessageTopics_ID') 
AND parent_object_id = OBJECT_ID(N'b_PrivateMessageMappings'))
ALTER TABLE [b_PrivateMessageMappings] DROP CONSTRAINT [FK_b_PrivateMessageMappings_TopicId_b_PrivateMessageTopics_ID]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_b_PrivateMessageMappings_UserId_b_PrivateMessageUser_ID') 
AND parent_object_id = OBJECT_ID(N'b_PrivateMessageMappings'))
ALTER TABLE [b_PrivateMessageMappings] DROP CONSTRAINT [FK_b_PrivateMessageMappings_UserId_b_PrivateMessageUser_ID]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'b_PrivateMessageMappings') AND type in (N'U'))
DROP TABLE [b_PrivateMessageMappings]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_b_PrivateMessages_TopicId_b_PrivateMessageTopics_ID') 
AND parent_object_id = OBJECT_ID(N'b_PrivateMessages'))
ALTER TABLE [b_PrivateMessages] DROP CONSTRAINT [FK_b_PrivateMessages_TopicId_b_PrivateMessageTopics_ID]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'b_PrivateMessages') AND type in (N'U'))
DROP TABLE [b_PrivateMessages]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'b_PrivateMessageTopics') AND type in (N'U'))
DROP TABLE [b_PrivateMessageTopics]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_b_PrivateMessageFolders_UserId_b_PrivateMessageUser_ID') 
AND parent_object_id = OBJECT_ID(N'b_PrivateMessageFolders'))
ALTER TABLE [b_PrivateMessageFolders] DROP CONSTRAINT [FK_b_PrivateMessageFolders_UserId_b_PrivateMessageUser_ID]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'b_PrivateMessageFolders') AND type in (N'U'))
DROP TABLE [b_PrivateMessageFolders]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'b_PrivateMessageUser') AND type in (N'U'))
DROP TABLE [b_PrivateMessageUser]
GO

IF OBJECT_ID (N'PrivateMessages_FoldersChangeSort', N'P') IS NOT NULL
	DROP PROCEDURE PrivateMessages_FoldersChangeSort
GO

IF OBJECT_ID (N'PrivateMessages_MoveTopics', N'P') IS NOT NULL
	DROP PROCEDURE PrivateMessages_MoveTopics
GO

IF OBJECT_ID (N'PrivateMessages_OnDeleteTopic', N'P') IS NOT NULL
	DROP PROCEDURE PrivateMessages_OnDeleteTopic
GO

IF OBJECT_ID (N'PrivateMessages_SynchronizeFolder', N'P') IS NOT NULL
	DROP PROCEDURE PrivateMessages_SynchronizeFolder
GO

IF OBJECT_ID (N'PrivateMessages_SynchronizeMapping', N'P') IS NOT NULL
	DROP PROCEDURE PrivateMessages_SynchronizeMapping
GO

IF OBJECT_ID (N'PrivateMessages_SynchronizeMappingsByTopic', N'P') IS NOT NULL
	DROP PROCEDURE PrivateMessages_SynchronizeMappingsByTopic
GO

IF OBJECT_ID (N'PrivateMessages_SynchronizeTopic', N'P') IS NOT NULL
	DROP PROCEDURE PrivateMessages_SynchronizeTopic
GO

IF OBJECT_ID (N'PrivateMessages_SynchronizeUser', N'P') IS NOT NULL
	DROP PROCEDURE PrivateMessages_SynchronizeUser
GO
