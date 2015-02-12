IF OBJECT_ID (N'Adv_GetBanners', N'P') IS NOT NULL
	DROP PROCEDURE [Adv_GetBanners];
GO

IF OBJECT_ID (N'Adv_ProcessClientRedirection', N'P') IS NOT NULL
	DROP PROCEDURE [Adv_ProcessClientRedirection];
GO

IF OBJECT_ID (N'Adv_ProcessDisplay', N'P') IS NOT NULL
	DROP PROCEDURE [Adv_ProcessDisplay];
GO

IF OBJECT_ID(N'FK_b_AdvBanner_b_AdvSpace', N'F') IS NOT NULL 
	ALTER TABLE [b_AdvBanner] DROP CONSTRAINT [FK_b_AdvBanner_b_AdvSpace]
GO

IF OBJECT_ID(N'FK_b_AdvBanner_b_File', N'F') IS NOT NULL 
	ALTER TABLE [b_AdvBanner] DROP CONSTRAINT [FK_b_AdvBanner_b_File]
GO

IF OBJECT_ID(N'FK_b_AdvBanner_b_Author', N'F') IS NOT NULL 
	ALTER TABLE [b_AdvBanner] DROP CONSTRAINT [FK_b_AdvBanner_b_Author]
GO

IF OBJECT_ID(N'FK_b_AdvBanner_b_LMAuthor', N'F') IS NOT NULL 
	ALTER TABLE [b_AdvBanner] DROP CONSTRAINT [FK_b_AdvBanner_b_LMAuthor]
GO

IF OBJECT_ID(N'FK_b_AdvBannerInRole_b_AdvBanner', N'F') IS NOT NULL 
	ALTER TABLE [b_AdvBannerInRole] DROP CONSTRAINT [FK_b_AdvBannerInRole_b_AdvBanner]
GO

IF OBJECT_ID(N'FK_b_AdvBannerInRole_b_Roles', N'F') IS NOT NULL 
	ALTER TABLE [b_AdvBannerInRole] DROP CONSTRAINT [FK_b_AdvBannerInRole_b_Roles]
GO

IF OBJECT_ID(N'FK_b_AdvBannerInSite_b_AdvBanner', N'F') IS NOT NULL 
	ALTER TABLE [b_AdvBannerInSite] DROP CONSTRAINT [FK_b_AdvBannerInSite_b_AdvBanner]
GO

IF OBJECT_ID(N'FK_b_AdvBannerInSite_b_Site', N'F') IS NOT NULL 
	ALTER TABLE [b_AdvBannerInSite] DROP CONSTRAINT [FK_b_AdvBannerInSite_b_Site]
GO

IF OBJECT_ID(N'FK_b_AdvBannerUrlTemplate_b_AdvBanner', N'F') IS NOT NULL 
	ALTER TABLE [b_AdvBannerUrlTemplate] DROP CONSTRAINT [FK_b_AdvBannerUrlTemplate_b_AdvBanner]
GO

IF OBJECT_ID(N'FK_b_AdvBannerWeekScheduleHourSpan_b_AdvBanner', N'F') IS NOT NULL 
	ALTER TABLE [b_AdvBannerWeekScheduleHourSpan] DROP CONSTRAINT [FK_b_AdvBannerWeekScheduleHourSpan_b_AdvBanner]
GO

IF OBJECT_ID (N'b_AdvBannerWeekScheduleHourSpan', N'U') IS NOT NULL
	DROP TABLE [b_AdvBannerWeekScheduleHourSpan];
GO

IF OBJECT_ID (N'b_AdvBannerUrlTemplate', N'U') IS NOT NULL
	DROP TABLE [b_AdvBannerUrlTemplate];
GO

IF OBJECT_ID (N'b_AdvBannerInSite', N'U') IS NOT NULL
	DROP TABLE [b_AdvBannerInSite];
GO

IF OBJECT_ID (N'b_AdvBannerInRole', N'U') IS NOT NULL
	DROP TABLE [b_AdvBannerInRole];
GO

IF OBJECT_ID (N'b_AdvBanner', N'U') IS NOT NULL
	DROP TABLE [b_AdvBanner];
GO

IF OBJECT_ID (N'b_AdvSpace', N'U') IS NOT NULL
	DROP TABLE [b_AdvSpace];
GO