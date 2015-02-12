using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Linq;
using Bitrix.Configuration;
using Bitrix.DataLayer;
using Bitrix.Modules;
using Bitrix.Security;
using Bitrix.UI;
using System.Collections.ObjectModel;

public partial class bitrix_admin_AuthRolesEdit : BXAdminPage
{
	private string[] userOperations = new string[]
	{
		BXRoleOperation.Operations.UserCreate,
		BXRoleOperation.Operations.UserModify,
		BXRoleOperation.Operations.UserDelete,
		BXRoleOperation.Operations.UserView
	};


	int roleId = -1;

	BXRole role;

	bool currentUserCanModifySettings = false;

	private void FillPolicyFieldsInt(int val, TextBox tb, CheckBox cb)
	{
		if (val <= 0)
		{
			cb.Checked = true;
			tb.Text = String.Empty;
			tb.Enabled = false;
		}
		else
		{
			cb.Checked = false;
			tb.Text = val.ToString();
			tb.Enabled = true;
		}
	}

	private void FillPolicyFieldsString(string val, TextBox tb, CheckBox cb)
	{
		if (String.IsNullOrEmpty(val))
		{
			cb.Checked = true;
			tb.Text = String.Empty;
			tb.Enabled = false;
		}
		else
		{
			cb.Checked = false;
			tb.Text = val;
			tb.Enabled = true;
		}
	}

	private void WalkThrowControlsSelect(Control cntrl, ref List<string> subOperations)
	{
		if (!String.IsNullOrEmpty(cntrl.ID) && cntrl.ID.StartsWith("lbOperations_", StringComparison.InvariantCultureIgnoreCase))
		{
			CheckBoxList lbCntrl = (CheckBoxList)cntrl;
			foreach (ListItem item in lbCntrl.Items)
				if (subOperations.Contains(item.Value))
					item.Selected = true;
		}

		foreach (Control subCntrl in cntrl.Controls)
			WalkThrowControlsSelect(subCntrl, ref subOperations);
	}

	private Control WalkThrowControlsSearch(Control cntrl, string name)
	{
		foreach (Control subCntrl in cntrl.Controls)
		{
			if (!String.IsNullOrEmpty(subCntrl.ID) && subCntrl.ID.Equals(name, StringComparison.InvariantCultureIgnoreCase))
				return subCntrl;

			Control cntrl1 = WalkThrowControlsSearch(subCntrl, name);
			if (cntrl1 != null)
				return cntrl1;
		}

		return null;
	}

	ReadOnlyCollection<BXRole> activeRoles;
	private ReadOnlyCollection<BXRole> ActiveRoles
	{
		get
		{
			if (activeRoles != null)
				return activeRoles;
			
			var roles = BXRoleManager.GetList(
				new BXFormFilter(
					new BXFormFilterItem("Active", true, BXSqlFilterOperators.Equal)
				),
				new BXOrderBy_old("RoleName", "Asc")
			);

			foreach (int i in new[] { 1, 3, 2 })
			{
				var r = roles.Find(x => x.RoleId == i);
				if (r != null)
				{
					roles.Remove(r);
					roles.Insert(0, r);
				}
			}

			return activeRoles = roles.AsReadOnly();
		}
	}

	ReadOnlyCollection<BXRole> activeRolesNonStandard;
	private ReadOnlyCollection<BXRole> ActiveRolesNonStandard
	{
		get
		{
			return 
				activeRolesNonStandard
				?? (
					activeRolesNonStandard =
					(from r in ActiveRoles where r.RoleId != 2 && r.RoleId != 3 select r).ToList().AsReadOnly()
				);
			
		}
	}


	private void LoadData()
	{
		lbSubRoles.Items.Clear();
		
		foreach (BXRole roleTmp in ActiveRoles)
		{
			if (role == null || !String.Equals(role.RoleName, roleTmp.RoleName, StringComparison.InvariantCultureIgnoreCase))
				lbSubRoles.Items.Add(new ListItem(roleTmp.Title, roleTmp.RoleName));
		}

		lbTasks.Items.Clear();
		BXRoleTaskCollection tasksTmp = BXRoleTaskManager.GetList(null, new BXOrderBy_old("TaskName", "Asc"));
		foreach (BXRoleTask taskTmp in tasksTmp)
			lbTasks.Items.Add(new ListItem(taskTmp.Title, taskTmp.TaskName));

		string moduleIdTmp = null;
		CheckBoxList lbOperationsTmp = new CheckBoxList();
		BXRoleOperationCollection operationsTmp = BXRoleOperationManager.GetList(null, new BXOrderBy_old("ModuleId", "Asc", "OperationName", "Asc"));
		foreach (BXRoleOperation operationTmp in operationsTmp)
		{
			if (String.IsNullOrEmpty(moduleIdTmp) || !moduleIdTmp.Equals(operationTmp.ModuleId, StringComparison.InvariantCultureIgnoreCase))
			{
				moduleIdTmp = operationTmp.ModuleId;
				Control cntrl = WalkThrowControlsSearch(Form, String.Format("lbOperations_{0}", moduleIdTmp));
				lbOperationsTmp = (CheckBoxList)cntrl;
				lbOperationsTmp.Items.Clear();
			}

			//remove special user operations from the common list
			if (!Array.Exists(userOperations, x => string.Equals(x, operationTmp.OperationName, StringComparison.OrdinalIgnoreCase)))
				lbOperationsTmp.Items.Add(new ListItem(Encode(operationTmp.OperationFriendlyName), operationTmp.OperationName));
		}

		if (role == null)
		{
			roleId = -1;
			hfRoleId.Value = roleId.ToString();

			ddProviderName.Visible = true;
			lbProviderName.Visible = false;

			trUsersCount.Visible = false;
			trRolesCount.Visible = false;

			cbCheckwordTimeoutParent.Checked = true;
			cbMaxStoreNumParent.Checked = true;
			cbSessionIPMaskParent.Checked = true;
			cbSessionTimeoutParent.Checked = true;
			cbStoreIPMaskParent.Checked = true;
			cbStoreTimeoutParent.Checked = true;

			tbCheckwordTimeout.Enabled = false;
			tbMaxStoreNum.Enabled = false;
			tbSessionIPMask.Enabled = false;
			tbSessionTimeout.Enabled = false;
			tbStoreIPMask.Enabled = false;
			tbStoreTimeout.Enabled = false;

			cbActive.Checked = true;

			ddProviderName.Items.Add(new ListItem(GetMessageRaw("BuiltIn"), ""));
			foreach (RoleProvider pr in Roles.Providers)
				ddProviderName.Items.Add(pr.Name);
			ddProviderName.SelectedIndex = 0;
		}
		else
		{
			ddProviderName.Visible = false;
			lbProviderName.Visible = true;

			trUsersCount.Visible = true;
			trRolesCount.Visible = true;

			lbProviderName.Text = HttpUtility.HtmlEncode((!String.IsNullOrEmpty(role.ProviderName) ? role.ProviderName : GetMessageRaw("BuiltIn")));
			tbRoleName.Text = role.ProviderRoleName;
			tbRoleTitle.Text = role.RoleTitle;
			tbComment.Text = role.Comment;
			lbUsersCount.Text = role.UsersCount.ToString();
			lbRolesCount.Text = role.RolesCount.ToString();
			cbActive.Checked = role.Active;

			string[] subRolesTmp = BXRoleManager.GetList(new BXFormFilter(new BXFormFilterItem("Parent.RoleName", role.RoleName, BXSqlFilterOperators.Equal)), new BXOrderBy_old("RoleName", "Asc")).ToStringArray();
			List<string> subRolesTmp1 = new List<string>(subRolesTmp);
			foreach (ListItem item in lbSubRoles.Items)
				if (subRolesTmp1.Contains(item.Value))
					item.Selected = true;

			string[] subTasksTmp = BXRoleTaskManager.GetList(
					new BXFormFilter(
						new BXFormFilterItem("Role.RoleName", role.RoleName, BXSqlFilterOperators.Equal),
						new BXFormFilterItem("Role.ModuleId", "", BXSqlFilterOperators.Equal)
					),
					new BXOrderBy_old("TaskName", "Asc")
				).ToStringArray();
			List<string> subTasksTmp1 = new List<string>(subTasksTmp);
			foreach (ListItem item in lbTasks.Items)
				if (subTasksTmp1.Contains(item.Value))
					item.Selected = true;

			string[] subOperationsTmp = BXRoleOperationManager.GetList(
				new BXFormFilter(
					new BXFormFilterItem("Role.RoleName", role.RoleName, BXSqlFilterOperators.Equal),
					new BXFormFilterItem("Role.ModuleId", "", BXSqlFilterOperators.Equal)
				),
				new BXOrderBy_old("OperationName", "Asc")
			)
			.ToStringArray();

			List<string> subOperationsTmp1 = new List<string>(subOperationsTmp);
			foreach (Control cntrl in Form.Controls)
				WalkThrowControlsSelect(cntrl, ref subOperationsTmp1);

			BXSecurityPolicy policy = new BXSecurityPolicy(role.Policy);
			FillPolicyFieldsInt(policy.CheckwordTimeoutDraft, tbCheckwordTimeout, cbCheckwordTimeoutParent);
			FillPolicyFieldsInt(policy.MaxStoreNumDraft, tbMaxStoreNum, cbMaxStoreNumParent);
			FillPolicyFieldsString(policy.SessionIPMaskDraft, tbSessionIPMask, cbSessionIPMaskParent);
			FillPolicyFieldsInt(policy.SessionTimeoutDraft, tbSessionTimeout, cbSessionTimeoutParent);
			FillPolicyFieldsString(policy.StoreIPMaskDraft, tbStoreIPMask, cbStoreIPMaskParent);
			FillPolicyFieldsInt(policy.StoreTimeoutDraft, tbStoreTimeout, cbStoreTimeoutParent);


			LoadUserOperations(subOperationsTmp);
		}
	}

	private void LoadUserOperations(string[] fullOperations)
	{
		bool separateMode = false;
		UserOperationsSeparate.Checked = false;
		UserOperationsLimit.Checked = false;
		
		var fo = 
			fullOperations
			.Where(x => Array.Exists(userOperations, y => string.Equals(x, y, StringComparison.OrdinalIgnoreCase)))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList();
		
		if (fo.Count > 0)
		{
			foreach (RepeaterItem i in UserOperationsContainer.Items)
			{
				var name = userOperationNames[i];

				if (fo.Count < userOperations.Length && !fo.Exists(x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase)))
					continue;

				((CheckBox)i.FindControl("Operation")).Checked = true;
				((CheckBox)i.FindControl("Limit")).Checked = false;
			}
		}
		
		if (fo.Count == userOperations.Length)
			return;

		var leftOver = fo.Count > 0 ? userOperations.Where(x => !fo.Exists(y => string.Equals(x, y, StringComparison.OrdinalIgnoreCase))).ToArray() : userOperations;
		bool hasLeftOver = leftOver.Length < userOperations.Length;
		List<string> currentNonSeparateOps = null;
		foreach (var r in ActiveRoles)
		{
			var userOps = BXRoleOperationManager.GetList(
				new BXFormFilter(
					new BXFormFilterItem("Role.RoleName", role.RoleName, BXSqlFilterOperators.Equal),
					new BXFormFilterItem("Role.ModuleId", "main", BXSqlFilterOperators.Equal),
					new BXFormFilterItem("Role.ExternalId", "r" + r.RoleId.ToString(), BXSqlFilterOperators.Equal)
				),
				null
			)
			.Select(x => x.OperationName)
			.Where(x => Array.Exists(leftOver, y => string.Equals(x, y, StringComparison.OrdinalIgnoreCase)))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList();

			if (userOps.Count == 0)
				continue;

			if (!separateMode)
			{
				bool separate = false;
				if (!hasLeftOver)
				{
					if (currentNonSeparateOps == null)
						currentNonSeparateOps = userOps;
					else 
						separate = 
							userOps.Count != currentNonSeparateOps.Count
							|| userOps.Exists(x => !currentNonSeparateOps.Exists(y => string.Equals(x, y, StringComparison.OrdinalIgnoreCase)))
							|| currentNonSeparateOps.Exists(x => !userOps.Exists(y => string.Equals(x, y, StringComparison.OrdinalIgnoreCase)));
				}
								
				if (separate || (hasLeftOver && userOps.Count != 0 && userOps.Count != userOperations.Length))
				{
					separateMode = true;
					UserOperationsSeparate.Checked = true;

					// transfer selected values from global list to separate lists
					var selected = (from ListItem i in UserOperationsRoles.Items where i.Selected select i.Value).ToArray();
					foreach (ListItem i in UserOperationsRoles.Items)
						i.Selected = false;
					foreach (RepeaterItem i in UserOperationsContainer.Items)
					{
						foreach (ListItem li in ((ListControl)i.FindControl("Roles")).Items)
							li.Selected = (Array.IndexOf(selected, li.Value) >= 0);
					}
				}
			}

			foreach (RepeaterItem i in UserOperationsContainer.Items)
			{
				var name = userOperationNames[i];

				if (!userOps.Exists(x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase)))
					continue;

				((CheckBox)i.FindControl("Operation")).Checked = true;

				if (separateMode)
				{
					((CheckBox)i.FindControl("Limit")).Checked = true;
					foreach (ListItem li in ((ListControl)i.FindControl("Roles")).Items)
					{
						if (string.Equals(li.Value, r.RoleId.ToString(), StringComparison.OrdinalIgnoreCase))
						{
							li.Selected = true;
							break;
						}
					}
				}
			}
		
			if (!separateMode)
			{
				UserOperationsLimit.Checked = true;
				foreach (ListItem li in UserOperationsRoles.Items)
				{
					if (string.Equals(li.Value, r.RoleId.ToString(), StringComparison.OrdinalIgnoreCase))
					{
						li.Selected = true;
						break;
					}
				}
			}
		}
	}


	protected void MoveSelectedUp(ListItemCollection items)
	{
		var selItems = new List<ListItem>();
		for (int i = items.Count - 1; i >= 0; i--)
		{
			if (items[i].Selected)
			{
				selItems.Add(items[i]);
				items.RemoveAt(i);
			}
		}
		foreach (var i in selItems)
			items.Insert(0, i);
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		roleId = base.GetRequestInt("id");
		hfRoleId.Value = Request.Form[hfRoleId.UniqueID];
		if (roleId > 0)
			hfRoleId.Value = roleId.ToString();
		Int32.TryParse(hfRoleId.Value, out roleId);
		if (roleId > 0)
		{
			role = BXRoleManager.GetById(roleId);
			if (role == null)
			{
				roleId = 0;
				hfRoleId.Value = roleId.ToString();
			}
		}

		if (roleId <= 0)
		{
			string roleName = base.GetRequestString("name");
			if (!String.IsNullOrEmpty(roleName))
			{
				role = BXRoleManager.GetByName(roleName);
				if (role != null)
				{
					roleId = role.RoleId;
					hfRoleId.Value = roleId.ToString();
				}
			}
		}

		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.AuthorizationSettingsView))
			BXAuthentication.AuthenticationRequired();

		currentUserCanModifySettings = this.BXUser.IsCanOperate(BXRoleOperation.Operations.AuthorizationSettingsManage);
		
		UserOperationsRoles.Items.Clear();
		UserOperationsRoles.Items.AddRange(ActiveRolesNonStandard.Select(x => new ListItem(Encode(x.Title), x.RoleId.ToString())).ToArray());
		UserOperationsContainer.DataSource = userOperations;
		UserOperationsContainer.DataBind();

		PrepareForInsertScript();
	}
	protected void Page_Load(object sender, EventArgs e)
	{
		((BXAdminMasterPage)Page.Master).Title = Page.Title;

		string success = base.GetRequestString("success");
		if ("Y".Equals(success, StringComparison.InvariantCultureIgnoreCase))
			successMessage.Visible = true;
		else
			successMessage.Visible = false;

		string moduleIdTmp = null;
		BXRoleOperationCollection operationsTmp = BXRoleOperationManager.GetList(null, new BXOrderBy_old("ModuleId", "Asc", "OperationName", "Asc"));
		foreach (BXRoleOperation operationTmp in operationsTmp)
		{
			if (!String.IsNullOrEmpty(moduleIdTmp) && moduleIdTmp.Equals(operationTmp.ModuleId, StringComparison.InvariantCultureIgnoreCase))
				continue;

			moduleIdTmp = operationTmp.ModuleId;
			HtmlTableRow r1 = new HtmlTableRow();
			r1.VAlign = "top";

			HtmlTableCell c1 = new HtmlTableCell();
			c1.Attributes["class"] = "field-name";
			c1.Attributes["width"] = "40%";

			try
			{
				string moduleName = BXModuleManager.GetModule(moduleIdTmp).Name;
				c1.InnerHtml = Encode(moduleName != null ? string.Concat(moduleName, ":") : string.Format(GetMessageRaw("ModuleId"), moduleIdTmp));
			}
			catch
			{
				c1.InnerHtml = Encode(string.Format(GetMessageRaw("ModuleId"), moduleIdTmp));
			}
			r1.Cells.Add(c1);

			c1 = new HtmlTableCell();
			c1.Attributes["width"] = "60%";
			CheckBoxList lbOperationsTmp = new CheckBoxList();
			lbOperationsTmp.ID = String.Format("lbOperations_{0}", moduleIdTmp);
			c1.Controls.Add(lbOperationsTmp);
			r1.Cells.Add(c1);

			tblOperations.Rows.Add(r1);
		}

		if (!Page.IsPostBack)
			LoadData();
	}
	protected void Page_LoadComplete(object sender, EventArgs e)
	{
		if (roleId > 0)
		{
			Page.Title = HttpUtility.HtmlEncode(string.Format(GetMessageRaw("MasterTitle"), roleId));
			((BXAdminMasterPage)Page.Master).Title = Page.Title;
		}
		DeleteRoleButton.Visible = AddRoleButton.Visible = roleId > 0 && currentUserCanModifySettings;
		BXTabControl1.ShowSaveButton = BXTabControl1.ShowApplyButton = currentUserCanModifySettings;
	}

	protected void BXTabControl1_Command(object sender, BXTabControlCommandEventArgs e)
	{
		if (e.CommandName != "save" && e.CommandName != "apply")
			GoBack("AuthRolesList.aspx");

		if (!SaveRole())
			return;

		if (e.CommandName == "save")
			GoBack("AuthRolesList.aspx");

		Response.Redirect(string.Concat(
			"AuthRolesEdit.aspx?id=",
			roleId.ToString(),
			
			"&ok=",

			!string.IsNullOrEmpty(BackUrl)
			? "&" + BXConfigurationUtility.Constants.BackUrl + "=" + HttpUtility.UrlEncode(BackUrl)
			: "",

			BXTabControl1.SelectedIndex > 0
			? "&tabindex=" + BXTabControl1.SelectedIndex.ToString()
			: ""
		));
	}

	private bool SaveRole()
	{
		if (Page.IsValid)
		{
			if (roleId > 0)
				return UpdateRole();
			else
				return CreateRole();
		}
		return false;
	}

	private void WalkThrowControlsGet(Control cntrl, ref List<string> subOperations)
	{
		if (!String.IsNullOrEmpty(cntrl.ID) && cntrl.ID.StartsWith("lbOperations_", StringComparison.InvariantCultureIgnoreCase))
		{
			CheckBoxList lbCntrl = (CheckBoxList)cntrl;
			foreach (ListItem item in lbCntrl.Items)
				if (item.Selected)
					subOperations.Add(item.Value);
		}

		foreach (Control subCntrl in cntrl.Controls)
			WalkThrowControlsGet(subCntrl, ref subOperations);
	}

	private bool CreateRole()
	{
		try
		{
			if (!currentUserCanModifySettings)
				throw new Exception(GetMessageRaw("Message.InsufficientRightsToAddRole"));

			if (String.IsNullOrEmpty(tbRoleName.Text))
				throw new Exception(GetMessageRaw("Message.RoleNameRequired"));

			BXSecurityPolicy policy = new BXSecurityPolicy();
			if (!cbCheckwordTimeoutParent.Checked)
			{
				int checkwordTimeout;
				if (Int32.TryParse(tbCheckwordTimeout.Text, out checkwordTimeout))
					policy.CheckwordTimeoutDraft = checkwordTimeout;
			}
			if (!cbMaxStoreNumParent.Checked)
			{
				int maxStoreNumParent;
				if (Int32.TryParse(tbMaxStoreNum.Text, out maxStoreNumParent))
					policy.MaxStoreNumDraft = maxStoreNumParent;
			}
			if (!cbSessionIPMaskParent.Checked)
			{
				policy.SessionIPMaskDraft = tbSessionIPMask.Text;
			}
			if (!cbStoreIPMaskParent.Checked)
			{
				policy.StoreIPMaskDraft = tbStoreIPMask.Text;
			}
			if (!cbSessionTimeoutParent.Checked)
			{
				int sessionTimeoutParent;
				if (Int32.TryParse(tbSessionTimeout.Text, out sessionTimeoutParent))
					policy.SessionTimeoutDraft = sessionTimeoutParent;
			}
			if (!cbStoreTimeoutParent.Checked)
			{
				int storeTimeoutParent;
				if (Int32.TryParse(tbStoreTimeout.Text, out storeTimeoutParent))
					policy.StoreTimeoutDraft = storeTimeoutParent;
			}

			string roleName = tbRoleName.Text;
			if (!String.IsNullOrEmpty(ddProviderName.SelectedValue))
			{
				if (roleName.StartsWith(ddProviderName.SelectedValue + @"\", StringComparison.InvariantCultureIgnoreCase))
					roleName = roleName.Substring(ddProviderName.SelectedValue.Length + 1);
				roleName = ddProviderName.SelectedValue + @"\" + roleName;
			}

			role = BXRoleManager.Create(roleName, tbRoleTitle.Text, cbActive.Checked, tbComment.Text, policy.Policy);
			if (role == null)
				throw new Exception(GetMessageRaw("Message.UnableToCreateRole"));

			roleId = role.RoleId;
			hfRoleId.Value = roleId.ToString();

			List<string> subRolesTmp1 = new List<string>();
			foreach (ListItem item in lbSubRoles.Items)
				if (item.Selected)
					subRolesTmp1.Add(item.Value);

			if (subRolesTmp1.Count > 0)
			{
				string[] subRolesTmp = subRolesTmp1.ToArray();
				role.AddToSubRoles(subRolesTmp);
			}

			List<string> taskNamesTmp1 = new List<string>();
			foreach (ListItem item in lbTasks.Items)
				if (item.Selected)
					taskNamesTmp1.Add(item.Value);

			if (taskNamesTmp1.Count > 0)
			{
				string[] taskNamesTmp = taskNamesTmp1.ToArray();
				role.AddToTasks(taskNamesTmp);
			}

			List<string> operationNamesTmp1 = new List<string>();
			foreach (Control cntrl in Form.Controls)
				WalkThrowControlsGet(cntrl, ref operationNamesTmp1);

			if (operationNamesTmp1.Count > 0)
			{
				string[] operationNamesTmp = operationNamesTmp1.ToArray();
				role.AddToOperations(operationNamesTmp);
			}

			var separate = UserOperationsSeparate.Checked;
			var userOps =
				from RepeaterItem i in UserOperationsContainer.Items 
				where ((CheckBox)i.FindControl("Operation")).Checked
				select new 
				{ 
					Name = userOperationNames[i], 
					Limited = separate ? ((CheckBox)i.FindControl("Limit")).Checked : UserOperationsLimit.Checked,
					Roles = separate ? (ListControl)i.FindControl("Roles") : UserOperationsRoles
				};
		
			string[] roleNameArray = new[] { role.RoleName };
			foreach (var op in userOps)
			{
				if (!op.Limited)
					role.AddToOperations(new [] { op.Name });
				else
				{
					var opNameArray = new [] { op.Name };

					BXRoleManager.AddRolesToOperations(roleNameArray, opNameArray, "main", "r2");
					BXRoleManager.AddRolesToOperations(roleNameArray, opNameArray, "main", "r3");

					foreach (var opRole in (from ListItem i in op.Roles.Items where i.Selected select i.Value))
						BXRoleManager.AddRolesToOperations(roleNameArray, opNameArray, "main", "r" + opRole);
				}
			}

			return true;
		}
		catch (BXEventException e)
		{
			foreach (string s in e.Messages)
				errorMessage.AddErrorMessage(s);
		}
		catch (Exception e)
		{
			errorMessage.AddErrorMessage(e.Message);
		}
		return false;
	}

	private bool UpdateRole()
	{
		try
		{
			if (!currentUserCanModifySettings)
				throw new Exception(GetMessageRaw("Message.InsufficientRightsToEditRole"));

			if (String.IsNullOrEmpty(tbRoleName.Text))
				throw new Exception(GetMessageRaw("Message.RoleNameRequired"));

			BXSecurityPolicy policy = new BXSecurityPolicy();
			if (!cbCheckwordTimeoutParent.Checked)
			{
				int checkwordTimeout;
				if (Int32.TryParse(tbCheckwordTimeout.Text, out checkwordTimeout))
					policy.CheckwordTimeoutDraft = checkwordTimeout;
			}
			if (!cbMaxStoreNumParent.Checked)
			{
				int maxStoreNumParent;
				if (Int32.TryParse(tbMaxStoreNum.Text, out maxStoreNumParent))
					policy.MaxStoreNumDraft = maxStoreNumParent;
			}
			if (!cbSessionIPMaskParent.Checked)
			{
				policy.SessionIPMaskDraft = tbSessionIPMask.Text;
			}
			if (!cbStoreIPMaskParent.Checked)
			{
				policy.StoreIPMaskDraft = tbStoreIPMask.Text;
			}
			if (!cbSessionTimeoutParent.Checked)
			{
				int sessionTimeoutParent;
				if (Int32.TryParse(tbSessionTimeout.Text, out sessionTimeoutParent))
					policy.SessionTimeoutDraft = sessionTimeoutParent;
			}
			if (!cbStoreTimeoutParent.Checked)
			{
				int storeTimeoutParent;
				if (Int32.TryParse(tbStoreTimeout.Text, out storeTimeoutParent))
					policy.StoreTimeoutDraft = storeTimeoutParent;
			}

			role.Active = cbActive.Checked;
			role.RoleTitle = tbRoleTitle.Text;
			role.Comment = tbComment.Text;
			role.Policy = policy.Policy;

			string roleName = tbRoleName.Text;
			if (!String.IsNullOrEmpty(role.ProviderName))
			{
				if (roleName.StartsWith(role.ProviderName + @"\", StringComparison.InvariantCultureIgnoreCase))
					roleName = roleName.Substring(role.ProviderName.Length + 1);
				roleName = role.ProviderName + @"\" + roleName;
			}
			role.RoleName = roleName;

			role.Update();

			List<string> subRolesTmp1 = new List<string>();
			foreach (ListItem item in lbSubRoles.Items)
				if (item.Selected)
					subRolesTmp1.Add(item.Value);

			role.RemoveFromSubRoles();
			if (subRolesTmp1.Count > 0)
			{
				string[] subRolesTmp = subRolesTmp1.ToArray();
				role.AddToSubRoles(subRolesTmp);
			}

			List<string> taskNamesTmp1 = new List<string>();
			foreach (ListItem item in lbTasks.Items)
				if (item.Selected)
					taskNamesTmp1.Add(item.Value);

			role.RemoveFromTasks();
			if (taskNamesTmp1.Count > 0)
			{
				string[] taskNamesTmp = taskNamesTmp1.ToArray();
				role.AddToTasks(taskNamesTmp);
			}

			List<string> operationNamesTmp1 = new List<string>();
			foreach (Control cntrl in Form.Controls)
				WalkThrowControlsGet(cntrl, ref operationNamesTmp1);

			role.RemoveFromOperations();
			if (operationNamesTmp1.Count > 0)
			{
				string[] operationNamesTmp = operationNamesTmp1.ToArray();
				role.AddToOperations(operationNamesTmp);
			}

						
			foreach (var r in ActiveRoles)
			foreach (var op in userOperations)
			{
				try
				{
					BXRoleManager.RemoveRoleFromOperations(role.RoleName, op, "main", "r" + r.RoleId.ToString());
				}
				catch
				{
		
				}
			}

			var separate = UserOperationsSeparate.Checked;
			var userOps =
				from RepeaterItem i in UserOperationsContainer.Items 
				where ((CheckBox)i.FindControl("Operation")).Checked
				select new 
				{ 
					Name = userOperationNames[i], 
					Limited = separate ? ((CheckBox)i.FindControl("Limit")).Checked : UserOperationsLimit.Checked,
					Roles = separate ? (ListControl)i.FindControl("Roles") : UserOperationsRoles
				};
		
			string[] roleNameArray = new[] { role.RoleName };
			foreach (var op in userOps)
			{
				if (!op.Limited)
					role.AddToOperations(new [] { op.Name });
				else
				{
					var opNameArray = new [] { op.Name };
										
					BXRoleManager.AddRolesToOperations(roleNameArray, opNameArray, "main", "r2");
					BXRoleManager.AddRolesToOperations(roleNameArray, opNameArray, "main", "r3");

					foreach (var opRole in (from ListItem i in op.Roles.Items where i.Selected select i.Value))
						BXRoleManager.AddRolesToOperations(roleNameArray, opNameArray, "main", "r" + opRole);
				}
			}

			return true;
		}
		catch (Exception e)
		{
			errorMessage.AddErrorMessage(e.Message);
		}

		return false;
	}

	private void PrepareForInsertScript()
	{
		if (!ClientScript.IsClientScriptBlockRegistered(GetType(), "InsertScript"))
		{
			StringBuilder sscript = new StringBuilder();

			sscript.AppendLine("var arGroupPolicyKey = new Array('SessionTimeout', 'SessionIPMask', 'MaxStoreNum', 'StoreIPMask', 'StoreTimeout', 'CheckwordTimeout');");
			sscript.AppendLine("var arGroupPolicyLow = new Array('30', '0.0.0.0', '20', '255.0.0.0', '133920', '266400');");
			sscript.AppendLine("var arGroupPolicyMiddle = new Array('20', '255.255.0.0', '10', '255.255.0.0', '43200', '1440');");
			sscript.AppendLine("var arGroupPolicyHigh = new Array('15', '255.255.255.255', '1', '255.255.255.255', '4320', '60');");

			sscript.AppendLine("var arPolicyControls = {" + String.Format("'SessionTimeout' : '{0}', 'SessionIPMask' : '{1}', 'MaxStoreNum' : '{2}', 'StoreIPMask' : '{3}', 'StoreTimeout' : '{4}', 'CheckwordTimeout' : '{5}'", tbSessionTimeout.ClientID, tbSessionIPMask.ClientID, tbMaxStoreNum.ClientID, tbStoreIPMask.ClientID, tbStoreTimeout.ClientID, tbCheckwordTimeout.ClientID) + "};");
			sscript.AppendLine("var arPolicyControlsParent = {" + String.Format("'SessionTimeout' : '{0}', 'SessionIPMask' : '{1}', 'MaxStoreNum' : '{2}', 'StoreIPMask' : '{3}', 'StoreTimeout' : '{4}', 'CheckwordTimeout' : '{5}'", cbSessionTimeoutParent.ClientID, cbSessionIPMaskParent.ClientID, cbMaxStoreNumParent.ClientID, cbStoreIPMaskParent.ClientID, cbStoreTimeoutParent.ClientID, cbCheckwordTimeoutParent.ClientID) + "};");

			sscript.AppendLine("function gpLevel()");
			sscript.AppendLine("{");
			sscript.AppendLine("	var i;");
			sscript.AppendLine(String.Format("	var el = document.{0}.{1};", Form.ClientID, DropDownList1.ClientID));
			sscript.AppendLine(String.Format("	var formId = \"{0}\";", Form.ClientID));
			sscript.AppendLine("	if (el.selectedIndex > 0)");
			sscript.AppendLine("	{");
			sscript.AppendLine("		var sel = el.options[el.selectedIndex].value;");
			sscript.AppendLine("		for (i = 0; i < arGroupPolicyKey.length; i++)");
			sscript.AppendLine("		{");
			sscript.AppendLine("			var el1 = eval(\"document.\" + formId + \".\" + arPolicyControlsParent[arGroupPolicyKey[i]]);");
			sscript.AppendLine("			var el2 = eval(\"document.\" + formId + \".\" + arPolicyControls[arGroupPolicyKey[i]]);");
			sscript.AppendLine("			if (sel == \"parent\")");
			sscript.AppendLine("				el1.checked = true;");
			sscript.AppendLine("			else");
			sscript.AppendLine("				el1.checked = false;");
			sscript.AppendLine("			PolicyParentCheck(arGroupPolicyKey[i]);");
			sscript.AppendLine("			if (sel == \"low\")");
			sscript.AppendLine("				el2.value = arGroupPolicyLow[i];");
			sscript.AppendLine("			else if (sel == \"middle\")");
			sscript.AppendLine("				el2.value = arGroupPolicyMiddle[i];");
			sscript.AppendLine("			else if (sel == \"high\")");
			sscript.AppendLine("				el2.value = arGroupPolicyHigh[i];");
			sscript.AppendLine("			else if (sel == \"parent\")");
			sscript.AppendLine("				el2.value = \"\";");
			sscript.AppendLine("		}");
			sscript.AppendLine("	}");
			sscript.AppendLine("}");

			sscript.AppendLine("function PolicyValidatorCheckInt(cb, tb)");
			sscript.AppendLine("{");
			sscript.AppendLine("	var res;");
			sscript.AppendLine("	if (cb)");
			sscript.AppendLine("	{");
			sscript.AppendLine("		res = true;");
			sscript.AppendLine("	}");
			sscript.AppendLine("	else");
			sscript.AppendLine("	{");
			sscript.AppendLine("		var val = parseInt(tb);");
			sscript.AppendLine("		res = ((val > 0) && (tb == val.toString()));");
			sscript.AppendLine("	}");
			sscript.AppendLine("	return res;");
			sscript.AppendLine("}");

			sscript.AppendLine("function PolicyValidatorCheckString(cb, tb)");
			sscript.AppendLine("{");
			sscript.AppendLine("	var res;");
			sscript.AppendLine("	if (cb)");
			sscript.AppendLine("	{");
			sscript.AppendLine("		res = true;");
			sscript.AppendLine("	}");
			sscript.AppendLine("	else");
			sscript.AppendLine("	{");
			sscript.AppendLine("		res = (tb.length > 0);");
			sscript.AppendLine("	}");
			sscript.AppendLine("	return res;");
			sscript.AppendLine("}");

			sscript.AppendLine("function cvSessionTimeout(oSrc, args)");
			sscript.AppendLine("{");
			sscript.AppendLine(
				string.Format(
					"	args.IsValid = PolicyValidatorCheckInt(document.{0}.{1}.checked, document.{0}.{2}.value);",
					Form.ClientID,
					cbSessionTimeoutParent.ClientID,
					tbSessionTimeout.ClientID
					));
			sscript.AppendLine("}");

			sscript.AppendLine("function cvSessionIPMask(oSrc, args)");
			sscript.AppendLine("{");
			sscript.AppendLine(
				string.Format(
					"	args.IsValid = PolicyValidatorCheckString(document.{0}.{1}.checked, document.{0}.{2}.value);",
					Form.ClientID,
					cbSessionIPMaskParent.ClientID,
					tbSessionIPMask.ClientID
					));
			sscript.AppendLine("}");

			sscript.AppendLine("function cvMaxStoreNum(oSrc, args)");
			sscript.AppendLine("{");
			sscript.AppendLine(
				string.Format(
					"	args.IsValid = PolicyValidatorCheckInt(document.{0}.{1}.checked, document.{0}.{2}.value);",
					Form.ClientID,
					cbMaxStoreNumParent.ClientID,
					tbMaxStoreNum.ClientID
					));
			sscript.AppendLine("}");

			sscript.AppendLine("function cvStoreIPMask(oSrc, args)");
			sscript.AppendLine("{");
			sscript.AppendLine(
				string.Format(
					"	args.IsValid = PolicyValidatorCheckString(document.{0}.{1}.checked, document.{0}.{2}.value);",
					Form.ClientID,
					cbStoreIPMaskParent.ClientID,
					tbStoreIPMask.ClientID
					));
			sscript.AppendLine("}");

			sscript.AppendLine("function cvStoreTimeout(oSrc, args)");
			sscript.AppendLine("{");
			sscript.AppendLine(
				string.Format(
					"	args.IsValid = PolicyValidatorCheckInt(document.{0}.{1}.checked, document.{0}.{2}.value);",
					Form.ClientID,
					cbStoreTimeoutParent.ClientID,
					tbStoreTimeout.ClientID
					));
			sscript.AppendLine("}");

			sscript.AppendLine("function cvCheckwordTimeout(oSrc, args)");
			sscript.AppendLine("{");
			sscript.AppendLine(
				string.Format(
					"	args.IsValid = PolicyValidatorCheckInt(document.{0}.{1}.checked, document.{0}.{2}.value);",
					Form.ClientID,
					cbCheckwordTimeoutParent.ClientID,
					tbCheckwordTimeout.ClientID
					));
			sscript.AppendLine("}");

			sscript.AppendLine("function PolicyParentCheck(val)");
			sscript.AppendLine("{");
			sscript.AppendLine(String.Format("	var formId = \"{0}\";", Form.ClientID));
			sscript.AppendLine("	var el1 = eval(\"document.\" + formId + \".\" + arPolicyControlsParent[val]);");
			sscript.AppendLine("	var el2 = eval(\"document.\" + formId + \".\" + arPolicyControls[val]);");
			sscript.AppendLine("	el2.disabled = el1.checked;");
			sscript.AppendLine("}");

			ClientScript.RegisterClientScriptBlock(GetType(), "InsertScript", sscript.ToString(), true);
		}

		cbCheckwordTimeoutParent.Attributes["onclick"] = "PolicyParentCheck('CheckwordTimeout')";
		cbMaxStoreNumParent.Attributes["onclick"] = "PolicyParentCheck('MaxStoreNum')";
		cbSessionIPMaskParent.Attributes["onclick"] = "PolicyParentCheck('SessionIPMask')";
		cbSessionTimeoutParent.Attributes["onclick"] = "PolicyParentCheck('SessionTimeout')";
		cbStoreIPMaskParent.Attributes["onclick"] = "PolicyParentCheck('StoreIPMask')";
		cbStoreTimeoutParent.Attributes["onclick"] = "PolicyParentCheck('StoreTimeout')";

		DropDownList1.Attributes["onchange"] = "gpLevel()";
	}

	private bool PolicyValidatorCheckInt(bool cb, string tb)
	{
		bool res = false;
		if (cb)
		{
			res = true;
		}
		else
		{
			int val;
			if (Int32.TryParse(tb, out val))
				res = (val > 0);
			else
				res = false;
		}
		return res;
	}

	private bool PolicyValidatorCheckString(bool cb, string tb)
	{
		bool res = false;
		if (cb)
			res = true;
		else
			res = !String.IsNullOrEmpty(tb);
		return res;
	}

	protected void cvSessionTimeout_ServerValidate(object source, ServerValidateEventArgs args)
	{
		args.IsValid = PolicyValidatorCheckInt(cbSessionTimeoutParent.Checked, tbSessionTimeout.Text);
	}

	protected void cvSessionIPMask_ServerValidate(object source, ServerValidateEventArgs args)
	{
		args.IsValid = PolicyValidatorCheckString(cbSessionIPMaskParent.Checked, tbSessionIPMask.Text);
	}

	protected void cvMaxStoreNum_ServerValidate(object source, ServerValidateEventArgs args)
	{
		args.IsValid = PolicyValidatorCheckInt(cbMaxStoreNumParent.Checked, tbMaxStoreNum.Text);
	}

	protected void cvStoreIPMask_ServerValidate(object source, ServerValidateEventArgs args)
	{
		args.IsValid = PolicyValidatorCheckString(cbStoreIPMaskParent.Checked, tbStoreIPMask.Text);
	}

	protected void cvStoreTimeout_ServerValidate(object source, ServerValidateEventArgs args)
	{
		args.IsValid = PolicyValidatorCheckInt(cbStoreTimeoutParent.Checked, tbStoreTimeout.Text);
	}

	protected void cvCheckwordTimeout_ServerValidate(object source, ServerValidateEventArgs args)
	{
		args.IsValid = PolicyValidatorCheckInt(cbCheckwordTimeoutParent.Checked, tbCheckwordTimeout.Text);
	}

	protected void BXContextMenuToolbar1_CommandClick(object sender, CommandEventArgs e)
	{
		switch (e.CommandName.ToLower())
		{
			case "deleterole":
				try
				{
					if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.AuthorizationSettingsManage))
						throw new Exception(GetMessageRaw("Message.InsufficientRightsToDeleteRecord"));

					if (role != null)
					{
						if (!BXRoleManager.Delete(roleId, false, false))
							throw new Exception(GetMessageRaw("Message.DeleteRoleError"));
					}
					else
					{
						throw new Exception(GetMessageRaw("Message.RoleNotFound"));
					}
					Response.Redirect("AuthRolesList.aspx");
				}
				catch (BXEventException ex)
				{
					foreach (string s in ex.Messages)
						errorMessage.AddErrorMessage(s);
				}
				catch (Exception ex)
				{
					errorMessage.AddErrorMessage(ex.Message);
				}

				break;

			default:
				break;
		}
	}

	protected void cvProviderName_ServerValidate(object source, ServerValidateEventArgs args)
	{
		args.IsValid = ((String.IsNullOrEmpty(ddProviderName.SelectedValue)) && (Roles.Providers[ddProviderName.SelectedValue] != null));
	}

	protected Dictionary<RepeaterItem, string> userOperationNames = new Dictionary<RepeaterItem,string>();
	protected void UserOperations_ItemDataBound(object source, RepeaterItemEventArgs args)
	{
		if ((args.Item.ItemType & (ListItemType.Item | ListItemType.AlternatingItem)) == 0)
			return;

		userOperationNames[args.Item] = (string)args.Item.DataItem;

		var roles = (ListControl)args.Item.FindControl("Roles");
		roles.Items.Clear();
		roles.Items.AddRange(ActiveRolesNonStandard.Select(x => new ListItem(Encode(x.Title), x.RoleId.ToString())).ToArray());
	}
}