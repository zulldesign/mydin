<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.Wizards.BXWizardStepStandardHtmlControl" %>
<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.Configuration" %>
<%@ Import Namespace="Bitrix.IO" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="Bitrix.UI.Wizards" %>
<%@ Import Namespace="System.Collections.Generic" %>
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
		var template = WizardContext.State.GetString("Installer.TemplateName");
		var userId = WizardContext.State.GetInt("Installer.UserId");
		var user = Bitrix.Security.BXUser.GetById(userId, BXTextEncoder.EmptyTextEncoder);

		var sef = site.UrlVirtualPath.Substring(1);
		var options = new Dictionary<string, string>
		{
			{ "Blog", "personal" },
			{ "BlogColorScheme", "~/bitrix/templates/" + WizardContext.State["Installer.TemplateName"] + "/blog.css" },
			{ "BlogUrl", site.UrlVirtualPath },
			{ "BlogSefFolder", sef },
			{ "PostUrl", site.UrlVirtualPath + "#PostId#/" },
			{ "PostEditUrl", site.UrlVirtualPath + "#PostId#/edit" },
			{ "PostRssUrl", site.UrlVirtualPath + "#PostId#/rss" },
			{ "BlogTagsUrl", site.UrlVirtualPath + "tags/?#SearchTags#" },
			{ "CommentUrl", site.UrlVirtualPath + "#PostId#/#CommentId###comment#CommentId#" },

			{ "PhotosSefFolder", sef + "photos/" },

			{ "ContactsEmail", user.Email },

			{ "TemplateTitleIncludeArea", site.DirectoryVirtualPath + "assets/header.html" },
			{ "TemplateSignatureIncludeArea", site.DirectoryVirtualPath + "assets/footer.html" },

			{ "ProfileUrl", site.UrlVirtualPath + "about.aspx" },
			{ "LoginSefFolder", sef + "login/" }
		};
		
		foreach(var p in options)
			BXOptionManager.SetOptionString("Bitrix.PersonalSite", p.Key, p.Value, site.Id);
		
		var h = WizardContext.State.Get<Bitrix.DataTypes.BXParamsBag<object>>("Bitrix.PersonalSite.Settings").GetString("Header");
		if (!string.IsNullOrEmpty(h))
		{
			site.Name = site.SiteName = h;
			site.Save();
			
			if (siteId == "default")
				BXOptionManager.SetOptionString("main", "site_name", h);
		}
						
		BXOptionManager.SetOptionString("main", "SiteLoginUrl", "login/", site.TextEncoder.Decode(site.Id));

		BXSiteOptions.EnsureDefaultPageProperties(siteId);
		
		UI.SetProgressBarValue("Installer.ProgressBar", "Bitrix.PersonalSite", 5);
		return Result.Next();
	}
</script>
Configure Solution
<% UI.ProgressBar("Installer.ProgressBar"); %>