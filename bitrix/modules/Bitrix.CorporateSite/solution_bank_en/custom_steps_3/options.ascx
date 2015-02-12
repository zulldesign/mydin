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
			{"TemplateLogoIncludeArea", site.DirectoryVirtualPath+ "src/logo.html"},
			{"TemplateSloganIncludeArea", site.DirectoryVirtualPath+ "src/slogan.html"},
			{"TemplateScheduleIncludeArea", site.DirectoryVirtualPath+ "src/schedule.html"},
			{"TemplatePhoneIncludeArea", site.DirectoryVirtualPath+ "src/phone.html"},
			{"TemplateSidebarIncludeArea", site.DirectoryVirtualPath+ "src/sidebar.html"},
			{"BannerIncludeArea", site.DirectoryVirtualPath+ "src/banner.html"},
			{"BannerTextIncludeArea", site.DirectoryVirtualPath+ "src/bannertext.html"},
			{"TemplateCopyrightIncludeArea", site.DirectoryVirtualPath+ "src/copyright.html"},
			{"NewsSefFolder",sef+  "news/"},
			{"VacanciesSefFolder",sef+  "about/vacancies/"},
			{"LoginSefFolder",sef+ "login/"},
			{ "ContactsEmail", user.Email },
			{"SearchUrl", site.UrlVirtualPath+ "search/?q=#query#"},
			{"NewsDetailUrl",site.DirectoryAbsolutePath+"news/#SectionId#/item-#ELEMENT_ID#/"},
			{"ManagementDetailUrl", site.DirectoryAbsolutePath+"about/management/"},
			{"DefaultPageMenuPath", site.UrlVirtualPath+"services/"}
		};
		
		foreach(var p in options)
			BXOptionManager.SetOptionString("Bitrix.BankSite", p.Key, p.Value, site.Id);

		BXOptionManager.SetOptionString("main", "SiteLoginUrl", "login/", site.TextEncoder.Decode(site.Id));

		BXSiteOptions.EnsureDefaultPageProperties(site.Id);				
		
		var h = WizardContext.State.Get<Bitrix.DataTypes.BXParamsBag<object>>("Bitrix.BankSite.Settings").GetString("Header");
		if (!string.IsNullOrEmpty(h))
		{
			site.Name = site.SiteName = h;
			site.Save();
			
			if (siteId == "default")
				BXOptionManager.SetOptionString("main", "site_name", h);
		}
		UI.SetProgressBarValue("Installer.ProgressBar", "Bitrix.BankSite", 5);				
		return Result.Next();
	}
</script>
Configure Solution
<% UI.ProgressBar("Installer.ProgressBar"); %>