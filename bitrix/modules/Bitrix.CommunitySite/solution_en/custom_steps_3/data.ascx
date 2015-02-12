<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.Wizards.BXWizardStepStandardHtmlControl" %>
<%@ Reference VirtualPath="../tools/User.ascx" %>
<%@ Reference VirtualPath="../tools/Blog.ascx" %>
<%@ Reference VirtualPath="../tools/Forum.ascx" %>
<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<%@ Import Namespace="Bitrix.IO" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="Bitrix.UI.Wizards" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.Install.Internal" %>
<%@ Import Namespace="Bitrix.Security"%>
<%@ Import Namespace="Bitrix.DataLayer"%>
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
		if (!WizardContext.State.Get<BXParamsBag<object>>("Bitrix.CommunitySite.Settings").GetBool("InstallDemoData"))
			return Result.Next();
	
		var siteId = WizardContext.State.GetString("Installer.SiteId");
		var userId = WizardContext.State.GetInt("Installer.UserId");
		var solutionPath = WizardContext.State.GetString("Installer.SolutionPath");

		UserXmlImporter userImport = new UserXmlImporter();
		userImport.LoadUsers(BXPath.MapPath(BXPath.Combine(solutionPath, "data/user_data.xml")));

		ForumXmlImporter forumImport = new ForumXmlImporter(siteId);
		forumImport.LoadForums(BXPath.MapPath(BXPath.Combine(solutionPath, "data/forum_data.xml")));

		
		BlogXmlImporter blogImport = new BlogXmlImporter(siteId, 1, "Bitrix.CommunitySite");

		var morrison = GetUser("morrison");
		blogImport.Replace["morrison.UserId"] = morrison != null ? morrison.UserId.ToString() : "1";

		var joel = GetUser("joel");
		blogImport.Replace.Add("joel.UserId", joel != null ? joel.UserId.ToString() : "1");

		blogImport.LoadBlogs(BXPath.MapPath(BXPath.Combine(solutionPath, "data/blog_data.xml")));		
		
		UI.SetProgressBarValue("Installer.ProgressBar", "Bitrix.CommunitySite", 3);
		return Result.Next();
	}


	private BXUser GetUser(string login)
	{
		BXUserCollection users = Bitrix.Security.BXUser.GetList(
			new BXFilter(new BXFilterItem(Bitrix.Security.BXUser.Fields.UserName, BXSqlFilterOperators.Equal, login)), null);

		return users.Count > 0 ? users[0] : null;
	}	
</script>
Download Demo Data
<% UI.ProgressBar("Installer.ProgressBar"); %>