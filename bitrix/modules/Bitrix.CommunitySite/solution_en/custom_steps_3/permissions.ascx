<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.Wizards.BXWizardStepStandardHtmlControl" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.Install" %>
<%@ Import Namespace="Bitrix.IO" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="Bitrix.UI.Wizards" %>
<%@ Import Namespace="Bitrix.Security" %>
<%@ Import Namespace="Bitrix.Forum" %>
<%@ Import Namespace="Bitrix.DataLayer" %>

<script runat="server">
	protected override BXWizardResult OnActionShow(Bitrix.DataTypes.BXCommonBag parameters)
	{
		var view = Result.Render("Solution Installation");
		view.AutoRedirect = true;
		view.RedirectAction = "next";
		return view;
	}

	protected override BXWizardResult OnActionNext(Bitrix.DataTypes.BXCommonBag parameters)
	{
		var siteId = WizardContext.State.GetString("Installer.SiteId");

		BXRole registerUser = BXRoleManager.GetRole("User");
		BXRole anonymousUser = BXRoleManager.GetRole("Guest");

		if (registerUser == null || anonymousUser == null)
			return Result.Next();
		
		//Edit Profile
		try
		{
			if (!registerUser.GetOperations().Exists
				(
					delegate(BXRoleOperation operation) {
						return String.Equals(operation.OperationName, "UserModifySelf", StringComparison.OrdinalIgnoreCase);
					}
				)
			)
				registerUser.AddToOperations(new string[] { "UserModifySelf" });
		}
		catch
		{
		}
		
		
		//Forums
		var generalForum = GetForum("Bitrix.CommunitySite.Forum.#SiteId#.general".Replace("#SiteId#", siteId));
		var aboutForum = GetForum("Bitrix.CommunitySite.Forum.#SiteId#.about".Replace("#SiteId#", siteId));
		foreach (var forum in new[] { generalForum, aboutForum })
		{
			try
			{		
				if (!registerUser.GetTasks("forum", forum.Id.ToString()).Exists(delegate(BXRoleTask task) { return String.Equals(task.TaskName, "BXForumMember", StringComparison.OrdinalIgnoreCase); }))	
					BXRoleManager.AddRolesToTasks(new string[] { registerUser.RoleName }, new string[] {"BXForumMember"}, "forum", forum.Id.ToString());
			}
			catch
			{
			}

			try
			{
				if (!anonymousUser.GetTasks("forum", forum.Id.ToString()).Exists(delegate(BXRoleTask task) { return String.Equals(task.TaskName, "BXForumGuest", StringComparison.OrdinalIgnoreCase); }))
					BXRoleManager.AddRolesToTasks(new string[] { anonymousUser.RoleName }, new string[] { "BXForumGuest" }, "forum", forum.Id.ToString());
			}
			catch
			{
			}
		}
		
		//Blogs
		try
		{
			if (!registerUser.GetTasks().Exists(delegate(BXRoleTask task) { return String.Equals(task.TaskName, "BXBlogMember", StringComparison.OrdinalIgnoreCase);}))
			{
				registerUser.AddToTasks(new string[] {"BXBlogMember"});
			}
		}
		catch
		{
		}
	
		//Personal Messages
		try
		{
			if (!registerUser.GetTasks().Exists(delegate(BXRoleTask task) { return String.Equals(task.TaskName, "BXPMessagesUser", StringComparison.OrdinalIgnoreCase);}))
			{
				registerUser.AddToTasks(new string[] {"BXPMessagesUser"});
			}		
		}
		catch
		{
		}
		
		UI.SetProgressBarValue("Installer.ProgressBar", "Bitrix.CommunitySite", 8);
		return Result.Next();
	}
	
	private BXForum GetForum(string xmlId)
	{
		BXForum forum = null;
		if (!string.IsNullOrEmpty(xmlId))
		{
			var forums = BXForum.GetList(
				new BXFilter(new BXFilterItem(BXForum.Fields.XmlId, BXSqlFilterOperators.Equal, xmlId)),
				null,
				null,
				null,
				BXTextEncoder.EmptyTextEncoder
			);
			if (forums.Count > 0)
				forum = forums[0];
		}

		return forum;
	}	
	
</script>
Assign Access Permissions
<% UI.ProgressBar("Installer.ProgressBar"); %>