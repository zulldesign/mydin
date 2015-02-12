<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.Wizards.BXWizardStepStandardHtmlControl" %>
<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.Configuration" %>
<%@ Import Namespace="Bitrix.IO" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="Bitrix.UI.Wizards" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.DataLayer" %>

<script runat="server">
	protected override BXWizardResult OnActionShow(Bitrix.DataTypes.BXCommonBag parameters)
	{
		var	view = Result.Render("Solution Installation");
		view.AutoRedirect = true;
		view.RedirectAction = "next";
		return view;
	}

	protected override BXWizardResult OnActionNext(Bitrix.DataTypes.BXCommonBag parameters)
	{
		var siteId = WizardContext.State.GetString("Installer.SiteId");
		var site = BXSite.GetById(siteId, BXTextEncoder.EmptyTextEncoder);
		var templateName = WizardContext.State.GetString("Installer.Template");
		var templateDirName = WizardContext.State.GetString("Installer.TemplateName");
		var themeName = WizardContext.State.GetString("Installer.Theme");
		var userId = WizardContext.State.GetInt("Installer.UserId");
		var user = Bitrix.Security.BXUser.GetById(userId, BXTextEncoder.EmptyTextEncoder);
		var sef = site.UrlVirtualPath.Substring(1);

		var forumsDir = site.UrlVirtualPath + "forums/";
		var peopleDir = site.UrlVirtualPath + "people/";
		var blogsDir = site.UrlVirtualPath + "blogs/";

		var forumTheme = "default";
		if (templateName == "taby")
		{
			if (themeName == "orange")
				forumTheme = "white";
			else if (themeName == "red" || themeName == "green" || themeName == "gray")
				forumTheme = "default";
			else
				forumTheme = themeName;
		}
		else if (templateName == "wide")
		{
			if (themeName == "blue")
				forumTheme = "white";
			else if (themeName == "gray")
				forumTheme = "default";
			else
				forumTheme = themeName;
		}
		string blogCategoryId = "1";
		var categories = BXBlogCategory.GetList(
			new BXFilter(new BXFilterItem(BXBlogCategory.Fields.XmlId, BXSqlFilterOperators.Equal, "Bitrix.Solutions." + siteId + ".PersonalBlogs")),
			null
		);

		if (categories.Count > 0)
			blogCategoryId = categories[0].Id.ToString();
		

		var options = new System.Collections.Generic.Dictionary<string, string>
		{
			{ "SiteFolder", site.UrlVirtualPath },
			
			{"TemplateLogoIncludeArea", site.DirectoryVirtualPath + "assets/logo.html"},
			{"TemplateLogoText", site.DirectoryVirtualPath + "assets/logotext.html"},
			{"CopyrightText", site.DirectoryVirtualPath + "assets/copyright.html"},
			
			{ "LoginSefFolder", sef + "login/"},
			{ "LogoutUrlTemplate", site.UrlVirtualPath + "login/signout/?ReturnUrl=#ReturnUrl#"},
			{ "LoginUrlTemplate", site.UrlVirtualPath + "login/?ReturnUrl=#ReturnUrl#"},
			{ "RegistrationUrlTemplate", site.UrlVirtualPath + "login/register/?ReturnUrl=#ReturnUrl#"},
			{ "RecoveryUrlTemplate", site.UrlVirtualPath + "login/recovery/"},
			
			{ "PeopleSefFolder", peopleDir.Substring(1) },
			{ "UserMailUrlTemplate", peopleDir + "#userId#/mail/"},
			{ "UserMailNewUrlTemplate", peopleDir + "#userId#/mail/new/"},
			{ "UserMailReadUrlTemplate", peopleDir + "#userId#/mail/#TopicId#/#MessageId#/##msg#MessageId#"},
			
			{ "UserMailNewForUsersUrlTemplate", peopleDir + "#userId#/mail/new/#Receivers#/"},
			{ "UserProfileUrlTemplate", peopleDir + "#userId#/"},
			{ "UserProfileEditUrlTemplate", peopleDir + "#userId#/edit/"},
			
			{ "ForumSefFolder", forumsDir.Substring(1) },
			{ "ForumColorScheme", "~/bitrix/components/bitrix/forum/templates/.default/themes/" + forumTheme + "/style.css" },
			{ "TopicReadUrlTemplate", forumsDir + "#ForumId#/#TopicId#/"},
			{ "ForumReadUrlTemplate", forumsDir + "#ForumId#/"},
			
			{ "SearchPageUrlTemplate", site.UrlVirtualPath + "search/?q=#query#"},
			
			{ "BlogColorScheme", "~/bitrix/templates/" + templateDirName  + "/blog.css" },
			{ "BlogSefFolder", blogsDir.Substring(1) },
			{ "BlogUrlTemplate", blogsDir + "#blogSlug#/"},
			{ "PostViewUrlTemplate", blogsDir + "#blogSlug#/#postId#/"},
			{ "SearchTagsUrlTemplate", blogsDir + "tags/?#SearchTags#"},
			{ "PostRssUrlTemplate", blogsDir + "#blogSlug#/#postId#/rss/"},
			{ "PostEditUrlTemplate", blogsDir + "#blogSlug#/#postId#/edit/"},
			{ "NewPostUrlTemplate", blogsDir + "#BlogSlug#/new/"},
			{ "NewBlogUrlTemplate", blogsDir + "new/"},
			{ "BlogCategoryId", blogCategoryId}			
		};

		var authOptions = new System.Collections.Generic.Dictionary<string, string>
		{
			{"LiveIDCustomFieldCode", "LIVEID"},
			{"OpenIDCustomFieldCode", "OPENID"}
		};
		
		foreach(var p in options)
			BXOptionManager.SetOptionString("Bitrix.CommunitySite", p.Key, p.Value, site.Id);

		foreach (var p in authOptions)
		{
			if (String.IsNullOrEmpty(BXOptionManager.GetOptionString("main",p.Key,"",null))) 		
				BXOptionManager.SetOptionString("main", p.Key, p.Value, null);
		}
		
		var h = WizardContext.State.Get<Bitrix.DataTypes.BXParamsBag<object>>("Bitrix.CommunitySite.Settings").GetString("SiteName");
		if (!String.IsNullOrEmpty(h))
		{
			site.Name = site.SiteName = h;
			site.Save();
			
			if (siteId == "default")
				BXOptionManager.SetOptionString("main", "site_name", h);
		}
						
		BXOptionManager.SetOptionString("main", "SiteLoginUrl", "login/", site.TextEncoder.Decode(site.Id));

		BXConfigurationUtility.Options.Site[siteId].SiteMapMenuTypes = "main,submenu";

		BXConfigurationUtility.Options.User.AvatarMaxSizeKB = 50;
		BXConfigurationUtility.Options.User.AvatarMaxWidth = 75;
		BXConfigurationUtility.Options.User.AvatarMaxHeight = 75;
		
		BXSiteOptions.EnsureDefaultPageProperties(siteId);		
			
		UI.SetProgressBarValue("Installer.ProgressBar", "Bitrix.CommunitySite", 5);
		return Result.Next();
	}
</script>
Configure Solution
<% UI.ProgressBar("Installer.ProgressBar"); %>