using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Bitrix.UI;
using Bitrix.Forum;
using Bitrix.Services.Text;
using Bitrix;
using Bitrix.DataLayer;
using Bitrix.Modules;
using System.Collections.Generic;
using Bitrix.Security;
using Bitrix.Configuration;
using Bitrix.Services;

public partial class BXForumAdminPageForumEdit : BXAdminPage
{
	private BXForum forum;
	private int id = 0;
	protected int Id
	{
		get { return id; }
		set { id = value; }
	}


	BXRoleCollection userRoles;
	BXRoleCollection UserRoles
	{
		get
		{
			if (userRoles == null)
			{
				userRoles = BXRoleManager.GetList(
					new BXFormFilter(new BXFormFilterItem("Active", true, BXSqlFilterOperators.Equal)),
					new BXOrderBy_old("RoleName", "Asc")
				);
			}
			return userRoles;
		}
	}

	BXRoleTaskCollection allTasks;
	BXRoleTaskCollection Tasks
	{
		get
		{
			if (allTasks == null)
			{
				allTasks = BXRoleTaskManager.GetList(
					new BXFormFilter(new BXFormFilterItem("Operation.ModuleId", "forum", BXSqlFilterOperators.Equal)),
					new BXOrderBy_old("TaskName", "Asc")
				);
			}
			return allTasks;
		}
	}

	BXRoleOperationCollection allOperations;
	BXRoleOperationCollection Operations
	{
		get
		{
			if (allOperations == null)
			{
				allOperations = BXRoleOperationManager.GetList(
					new BXFormFilter(new BXFormFilterItem("ModuleId", "forum", BXSqlFilterOperators.Equal)),
					new BXOrderBy_old("OperationName", "Asc")
				);
			}

			return allOperations;
		}
	}

	protected override string BackUrl
	{
		get
		{
			return base.BackUrl ?? "ForumList.aspx";
		}
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		InitFormElements();

		LoadForumData();

		bool canManageForum = false;
		if (forum != null)
			canManageForum = this.BXUser.IsCanOperate(BXForum.Operations.ForumAdminManage, "forum", forum.Id);
		else
			canManageForum = this.BXUser.IsCanOperate(BXForum.Operations.ForumAdminManage);

		if (!canManageForum)
			BXAuthentication.AuthenticationRequired();

		if (!this.BXUser.IsCanOperate(BXForum.Operations.ForumAdminManage))
		{
			AddButton.Visible = false;
			ForumCategoryNewLink.Visible = false;
		}

		PrepareAccessState();	

		InsertJavascript();
	}

	private void LoadForumData()
	{
		int requestId;
		if (int.TryParse(Request.QueryString["id"], out requestId) && requestId > 0)
			id = requestId;

		if (id > 0)
		{
			forum = BXForum.GetById(Id, BXTextEncoder.EmptyTextEncoder);
			if (forum == null)
			{
				errorMessage.AddErrorMessage(GetMessage("Error.ForumNotFound"));
				TabControl.Visible = false;
				return;
			}

			BXForum.BXForumSiteCollection currentSites = forum.Sites;
			foreach (ListItem item in ForumSites.Items)
			{
				if (currentSites.Exists(
						delegate(BXForum.BXForumSite forumSite) { return String.Equals(forumSite.SiteId, item.Value, StringComparison.OrdinalIgnoreCase); }))
					item.Selected = true;
			}

			if (forum.CategoryId > 0)
				ForumCategory.SelectedValue = forum.CategoryId.ToString();

			ForumActive.Checked = forum.Active;
			ForumName.Text = forum.Name;
			ForumDescription.Text = forum.Description;
			ForumSort.Text = forum.Sort.ToString();

			ForumBBCode.Checked = forum.AllowBBCode;
			ForumSmiles.Checked = forum.AllowSmiles;
            ForumVotingForTopic.Checked = forum.AllowVotingForTopic;
            ForumVotingForPost.Checked = forum.AllowVotingForPost;
			ForumIndexContent.Checked = forum.IndexContent;
			ForumCode.Text = forum.Code;
			ForumXmlId.Text = forum.XmlId;
		}
		else
		{
			ForumSort.Text = "10";
			ForumActive.Checked = true;
			ForumIndexContent.Checked = true;
			ForumBBCode.Checked = true;
			ForumSmiles.Checked = true;
		}
	}

	private void InitFormElements()
	{
		ForumSites.Items.Clear();
		BXSiteCollection siteColl = BXSite.GetList(null, null, null, null, BXTextEncoder.EmptyTextEncoder);
		foreach (BXSite site in siteColl)
			ForumSites.Items.Add(new ListItem("[" + HttpUtility.HtmlEncode(site.Id) + "] " + HttpUtility.HtmlEncode(site.Name), HttpUtility.HtmlEncode(site.Id)));

		BXForumCategoryCollection categoryCollection = BXForumCategory.GetList(null, new BXOrderBy(new BXOrderByPair(BXForumCategory.Fields.Sort, BXOrderByDirection.Asc)), null, null, BXTextEncoder.EmptyTextEncoder);
		foreach (BXForumCategory category in categoryCollection)
			ForumCategory.Items.Add(new ListItem(category.Name, category.Id.ToString()));

		ForumCategoryNewLink.HRef = String.Format("~/bitrix/admin/ForumCategoryEdit.aspx?{0}={1}", BXConfigurationUtility.Constants.BackUrl, UrlEncode(Request.RawUrl));

	}

	private bool SaveForumData()
	{
		bool result = false;
		try
		{
			Page.Validate("ForumEdit");
			if (!Page.IsValid)
				throw new Exception();

			AccessEdit.State.Validate(false);

			if (forum == null)
				forum = new BXForum();

			forum.Active = ForumActive.Checked;

			int categoryId;
			if (int.TryParse(ForumCategory.SelectedValue, out categoryId) && (categoryId == 0 || BXForumCategory.GetById(categoryId) != null))
				forum.CategoryId = categoryId;

			forum.Sites.Clear();
			foreach (ListItem item in ForumSites.Items)
				if (item.Selected)
					forum.Sites.Add(item.Value);
			
			forum.Name = ForumName.Text;
			forum.Description = ForumDescription.Text;

			int sort;
			if (int.TryParse(ForumSort.Text, out sort))
				forum.Sort = sort;

			forum.AllowBBCode = ForumBBCode.Checked;
			forum.AllowSmiles = ForumSmiles.Checked;
            forum.AllowVotingForTopic = ForumVotingForTopic.Checked;
            forum.AllowVotingForPost = ForumVotingForPost.Checked;

			forum.IndexContent = ForumIndexContent.Checked;
			forum.Code = ForumCode.Text;
			forum.XmlId = ForumXmlId.Text;

			forum.Save();

            if (IsSearchModuleInstalled && ForumRebuildSearchIndex.Checked)
            {
                BXSchedulerAgent a = new BXSchedulerAgent();
                a.SetClassNameAndAssembly(typeof(Bitrix.Forum.BXForum.IndexSynchronizer));
                a.Parameters.Add("Action", Bitrix.Forum.BXForum.IndexSynchronizerAction.Rebuild.ToString("G"));
                a.Parameters.Add("ForumId", Id);
                a.StartTime = DateTime.Now.AddSeconds(5D);
                a.Save();
            }

			if (forum.Id > 0)
			{
				BXRoleManager.RemoveRoleFromTasks("forum", forum.Id.ToString());
				BXRoleManager.RemoveRoleFromOperations("forum", forum.Id.ToString());
			}
			
			SaveAccessState();

			id = forum.Id;
			result = true;
		}
		catch (Exception ex)
		{
			errorMessage.AddErrorMessage(ex.Message);
		}

		return result;
	}

	private void PrepareAccessState()
	{
		AccessEdit.FillStandardRoles(UserRoles, false);

		if (Tasks.Count > 0)
		{
			AccessEdit.Operations.AddSeparator(GetMessage("TasksSeparatorName"));
			foreach (BXRoleTask task in Tasks)
				AccessEdit.Operations.Add("t" + task.TaskId.ToString(), task.Title);
		}

		AccessEdit.Operations.AddSeparator(GetMessage("OperationsSeparatorName"));
		foreach (BXRoleOperation op in Operations)
			AccessEdit.Operations.Add("o" + op.OperationId.ToString(), op.OperationTitle);

		if (!IsPostBack)
			LoadAccessState();
	}

	private void LoadAccessState()
	{
		AccessEdit.State.Clear();

		foreach (BXRole role in UserRoles)
		{
			string stringRoleId = role.RoleId.ToString();

			#region Get Inherited Tasks
			BXRoleTaskCollection inheritedTasks = BXRoleTaskManager.GetList(
				new BXFormFilter(
					new BXFormFilterItem("Role.RoleName", role.RoleName, BXSqlFilterOperators.Equal),
					new BXFormFilterItem("Role.ModuleId", string.Empty, BXSqlFilterOperators.Equal)
				),
				new BXOrderBy_old("TaskName", "Asc")
			);
			inheritedTasks.RemoveAll(delegate(BXRoleTask task)
			{
				return !AccessEdit.Operations.ContainsKey("t" + task.TaskId.ToString());
			});
			#endregion

			#region Get Current Tasks
			BXRoleTaskCollection currentTasks =
				forum != null
				? BXRoleTaskManager.GetList(
					new BXFormFilter(
						new BXFormFilterItem("Role.RoleName", role.RoleName, BXSqlFilterOperators.Equal),
						new BXFormFilterItem("Role.ModuleId", "forum", BXSqlFilterOperators.Equal),
						new BXFormFilterItem("Role.ExternalId", forum.Id.ToString(), BXSqlFilterOperators.Equal)
					),
					new BXOrderBy_old("TaskName", "Asc")
				)
				: new BXRoleTaskCollection();
			currentTasks.RemoveAll(delegate(BXRoleTask task)
			{
				return !AccessEdit.Operations.ContainsKey("t" + task.TaskId.ToString());
			});
			#endregion

			#region Get Inherited Operations
			BXRoleOperationCollection inheritedOperations = BXRoleOperationManager.GetList(
					new BXFormFilter(
						new BXFormFilterItem("Role.RoleName", role.RoleName, BXSqlFilterOperators.Equal),
						new BXFormFilterItem("Role.ModuleId", string.Empty, BXSqlFilterOperators.Equal)
					),
					new BXOrderBy_old("OperationName", "Asc")
				);
			inheritedOperations.RemoveAll(delegate(BXRoleOperation operation)
			{
				return !AccessEdit.Operations.ContainsKey("o" + operation.OperationId.ToString());
			});
			#endregion

			#region Get Current Operations
			BXRoleOperationCollection currentOperations =
				forum != null
				? BXRoleOperationManager.GetList(
					new BXFormFilter(
						new BXFormFilterItem("Role.RoleName", role.RoleName, BXSqlFilterOperators.Equal),
						new BXFormFilterItem("Role.ModuleId", "forum", BXSqlFilterOperators.Equal),
						new BXFormFilterItem("Role.ExternalId", forum.Id.ToString(), BXSqlFilterOperators.Equal)
					),
					new BXOrderBy_old("OperationName", "Asc")
				)
				: new BXRoleOperationCollection();
			currentOperations.RemoveAll(delegate(BXRoleOperation operation)
			{
				return !AccessEdit.Operations.ContainsKey("o" + operation.OperationId.ToString());
			});
			#endregion


			if (inheritedTasks.Count > 0 || currentTasks.Count > 0 || inheritedOperations.Count > 0 || currentOperations.Count > 0)
			{
				BXOperationsEditRoleInfo roleInfo = AccessEdit.Roles[stringRoleId];

				foreach (BXRoleTask task in inheritedTasks)
				{
					string stringTaskId = "t" + task.TaskId.ToString();
					BXOperationsEditOperationInfo opInfo;
					if (!roleInfo.Operations.TryGetValue(stringTaskId, out opInfo))
						roleInfo.Operations.Add(stringTaskId, opInfo = new BXOperationsEditOperationInfo());
					opInfo.InheritedState = BXOperationsEditInheritedOperationState.Allowed;
				}
				foreach (BXRoleTask task in currentTasks)
				{
					string stringTaskId = "t" + task.TaskId.ToString();
					BXOperationsEditOperationInfo opInfo;
					if (!roleInfo.Operations.TryGetValue(stringTaskId, out opInfo))
						roleInfo.Operations.Add(stringTaskId, opInfo = new BXOperationsEditOperationInfo());
					opInfo.State = BXOperationsEditOperationState.Allowed;
				}
				foreach (BXRoleOperation operation in inheritedOperations)
				{
					string stringOperationId = "o" + operation.OperationId.ToString();
					BXOperationsEditOperationInfo opInfo;
					if (!roleInfo.Operations.TryGetValue(stringOperationId, out opInfo))
						roleInfo.Operations.Add(stringOperationId, opInfo = new BXOperationsEditOperationInfo());
					opInfo.InheritedState = BXOperationsEditInheritedOperationState.Allowed;
				}
				foreach (BXRoleOperation operation in currentOperations)
				{
					string stringOperationId = "o" + operation.OperationId.ToString();
					BXOperationsEditOperationInfo opInfo;
					if (!roleInfo.Operations.TryGetValue(stringOperationId, out opInfo))
						roleInfo.Operations.Add(stringOperationId, opInfo = new BXOperationsEditOperationInfo());
					opInfo.State = BXOperationsEditOperationState.Allowed;
				}

				AccessEdit.State.SetRoleFromData(stringRoleId);
			}
		}
	}

	private void SaveAccessState()
	{
		List<string> allowed = new List<string>();
		foreach (KeyValuePair<string, Dictionary<string, BXOperationsEditOperationState>> role in AccessEdit.State)
		{
			int id = int.Parse(role.Key);
			BXRole r = UserRoles.Find(delegate(BXRole item)
			{
				return item.RoleId == id;
			});

			allowed.Clear();
			foreach (BXRoleTask task in Tasks)
				if (role.Value["t" + task.TaskId.ToString()] == BXOperationsEditOperationState.Allowed)
					allowed.Add(task.TaskName);

			if (allowed.Count > 0)
				BXRoleManager.AddRolesToTasks(new string[] { r.RoleName }, allowed.ToArray(), "forum", forum.Id.ToString());

			allowed.Clear();
			foreach (BXRoleOperation operation in Operations)
				if (role.Value["o" + operation.OperationId.ToString()] == BXOperationsEditOperationState.Allowed)
					allowed.Add(operation.OperationName);

			if (allowed.Count > 0)
				BXRoleManager.AddRolesToOperations(new string[] { r.RoleName }, allowed.ToArray(), "forum", forum.Id.ToString());
		}
	}

	private void InsertJavascript()
	{
		ScriptManager.RegisterClientScriptBlock(this, this.GetType(), ClientID,
		String.Format(@"
			function CheckSites(sender, args)
			{{
				var obj = document.getElementById(""{0}"");
				var result = false;
				if (obj)
				{{
					var inputs = obj.getElementsByTagName(""INPUT"");
					if (inputs.length > 0)
					{{
						for (var i = 0; i < inputs.length; i++)
						{{
							if (inputs[i].type == ""checkbox"" && inputs[i].checked)
							{{
								result = true;
								break;
							}}
						}}
					}}	
				}}
				args.IsValid = result;		
			}}
		", ForumSites.ClientID),
		true);
	}

    protected void Page_Load(object sender, EventArgs e)
    {
		if (forum == null)
		{
			AddButton.Visible = false;
			DeleteButton.Visible = false;
		}

		string title = forum != null ? String.Format(GetMessage("EditPageTitle"), HttpUtility.HtmlEncode(forum.Name)) : GetMessage("CreatePageTitle");

		MasterTitle = title;
		Page.Title = title;
    }

	protected void OnForumEdit(object sender, BXTabControlCommandEventArgs e)
	{
		switch (e.CommandName)
		{
			case "save":
				if (SaveForumData())
					GoBack();
				break;
			case "apply":
				if (SaveForumData())
					Response.Redirect(String.Format("ForumEdit.aspx?id={0}&tabindex={1}", id.ToString(), TabControl.SelectedIndex));
				break;
			case "cancel":
				GoBack();
				break;
		}
	}

	protected void OnToolBarButtonClick(object sender, CommandEventArgs e)
	{
		if (e.CommandName == "delete")
		{
			try
			{
				if (forum != null)
					forum.Delete();

				GoBack();
			}
			catch (Exception ex)
			{
				errorMessage.AddErrorMessage(ex.Message);
			}
		}
	}

	protected void ForumSites_ServerValidate(object source, ServerValidateEventArgs args)
	{
		args.IsValid = false;
		foreach (ListItem item in ForumSites.Items)
		{
			if (item.Selected)
			{
				args.IsValid = true;
				break;
			}
		}
	}

    protected bool IsSearchModuleInstalled
    {
        get
        {
            return Bitrix.Modules.BXModuleManager.IsModuleInstalled("search");
        }
    }
}
