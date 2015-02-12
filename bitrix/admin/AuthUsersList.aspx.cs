using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Bitrix;
using Bitrix.DataLayer;
using Bitrix.Modules;
using Bitrix.Services;
using Bitrix.UI;
using Bitrix.Security;
using Bitrix.Services.Text;

public partial class bitrix_admin_UsersList : BXAdminPage
{
	bool currentUserCanModifySelfUser;
	bool currentUserCanCreateUser;
	int[] userModifyRoles;
	int[] userDeleteRoles;
	int[] userViewRoles;


	protected void Page_Init(object sender, EventArgs e)
	{
		userViewRoles = this.BXUser.GetUserOperationRoles(BXRoleOperation.Operations.UserView);
		if (userViewRoles.Length == 0)
			BXAuthentication.AuthenticationRequired();

		currentUserCanModifySelfUser = this.BXUser.IsCanOperate(BXRoleOperation.Operations.UserModifySelf);
		currentUserCanCreateUser = this.BXUser.GetUserOperationRoles(BXRoleOperation.Operations.UserCreate).Length != 0;
		userModifyRoles = this.BXUser.GetUserOperationRoles(BXRoleOperation.Operations.UserModify);
		userDeleteRoles = this.BXUser.GetUserOperationRoles(BXRoleOperation.Operations.UserDelete);

		InitPage();

		BXAdminFilter1.CreateCustomFilters(Bitrix.Security.BXUser.GetCustomFieldsKey());
		AuthUserGridView.CreateCustomColumns(Bitrix.Security.BXUser.GetCustomFieldsKey());
	}

	private void InitPage()
	{
		filterSite.Values.Add(new ListItem(GetMessage("ListItemText.All"), ""));
		BXSiteCollection siteColl = BXSite.GetList(null, null);
		foreach (BXSite site in siteColl)
			filterSite.Values.Add(new ListItem("[" + HttpUtility.HtmlEncode(site.Id) + "] " + HttpUtility.HtmlEncode(site.Name), HttpUtility.HtmlEncode(site.Id)));


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
				if (i == 1)
					roles.Insert(0, r);
			}
		}		
		
		RolesList.Items.Clear();
		RolesList.Items.AddRange((from r in roles select new ListItem(r.Title, r.RoleId.ToString())).ToArray());
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		((BXAdminMasterPage)Page.Master).Title = Page.Title;
		AddButton.Visible = currentUserCanCreateUser;
	}
	protected void AuthUserGridView_RowDataBound(object sender, GridViewRowEventArgs e)
	{
		BXGridView grid = (BXGridView)sender;
		BXGridViewRow row = (BXGridViewRow)e.Row;
		Bitrix.Security.BXUser u = (Bitrix.Security.BXUser)row.DataItem;
		if (e.Row.RowType != DataControlRowType.DataRow)
			return;
		List<string> allowed = new List<string>();
		
		int[] roles = null;
		if (CanModify(u.UserId, ref roles))
			allowed.Add("edit");
		else
			allowed.Add("view");


		if (CanDelete(u.UserId, ref roles))
		{
			allowed.Add("");
			allowed.Add("delete");
			allowed.Add("deleteProvider");
		}

		row.AllowedCommandsList = allowed.ToArray();
	}


	protected void Roles_BuildFilter(object sender, BXCustomAdminFilter.BuildFilterEventArgs e)
	{
		List<int> roles = new List<int>();
		foreach(ListItem li in RolesList.Items)
		{
			int i;
			if (li.Selected && int.TryParse(li.Value, out i) && i > 0 && !roles.Contains(i))
				roles.Add(i);
		}
		if (roles.Count == 0)
			return;

		e.Result.Add("CheckRoles", roles, IncludeSubRoles.Checked ? BXSqlFilterOperators.In : BXSqlFilterOperators.Equal);
	}
	protected void Roles_LoadState(object sender, BXCustomAdminFilter.StateEventArgs e)
	{
		if (e.State.Contains("roles"))
		{
			var roles = e.State["roles"] as string[];
			if (roles != null)
			{
				foreach (ListItem li in RolesList.Items)
					li.Selected = Array.IndexOf(roles, li.Value) >= 0;
			}			
		}
		if (e.State.Contains("subroles"))
		{
			var v = e.State["subroles"];
			if (v != null && v is bool)
				IncludeSubRoles.Checked = (bool)v;			
		}
	}
	protected void Roles_SaveState(object sender, BXCustomAdminFilter.StateEventArgs e)
	{
		e.State["roles"] = (from ListItem li in RolesList.Items where li.Selected select li.Value).ToArray();
		e.State["subroles"] = IncludeSubRoles.Checked;
	}
	protected void Roles_Reset(object sender, EventArgs e)
	{
		foreach (ListItem li in RolesList.Items)
			li.Selected = false;
		IncludeSubRoles.Checked = false;		
	}
	protected void Roles_Init(object sender, BXCustomAdminFilter.InitEventArgs e)
	{
		var val = HttpContext.Current.Request.QueryString["filter_roles"];
		if (val != null)
		{
			foreach (ListItem li in RolesList.Items)
			{
				if (li.Value == val)
				{
					li.Selected = true;
					e.Initialized = true;
					return;
				}
			}			
		}
	}


	private bool CanModify(int userId)
	{
		int[] roles = null;
		return CanModify(userId, ref roles);
	}
	private bool CanDelete(int userId)
	{
		int[] roles = null;
		return CanDelete(userId, ref roles);
	}
	private bool CanModify(int userId, ref int[] roles)
	{

		bool canModify = true;
		BXIdentity user = (BXIdentity)BXUser.Identity;
		if (user.Id != userId || !currentUserCanModifySelfUser)
		{
			if (userModifyRoles.Length == 0)
				canModify = false;
			else if (userModifyRoles.Length > 1 || userModifyRoles[0] != 0)
			{
				if (roles == null)
					roles = BXRoleManager.GetRolesForUser(userId, false).Select(x => x.RoleId).ToArray();
				foreach (int i in roles)
				{
					if (Array.IndexOf(userModifyRoles, i) < 0)
					{
						canModify = false;
						break;
					}
				}
			}
		}
		return canModify;
	}
	private bool CanDelete(int userId, ref int[] roles)
	{
		bool canDelete = true;
		if (userDeleteRoles.Length == 0)
			canDelete = false;
		else if (userDeleteRoles.Length > 1 || userDeleteRoles[0] != 0)
		{
			if (roles == null)
				roles = BXRoleManager.GetRolesForUser(userId, false).Select(x => x.RoleId).ToArray();
			foreach (int i in roles)
			{
				if (Array.IndexOf(userDeleteRoles, i) < 0)
				{
					canDelete = false;
					break;
				}
			}
		}
		return canDelete;
	}

	protected void AuthUserGridView_Select(object sender, Bitrix.UI.BXSelectEventArgs e)
	{
		var filter = new BXFilter(BXAdminFilter1.CurrentFilter, Bitrix.Security.BXUser.Fields);
		var self = currentUserCanModifySelfUser ? new BXFilterItem(Bitrix.Security.BXUser.Fields.UserId, BXSqlFilterOperators.Equal, BXIdentity.Current.Id) : null;
		var roles = (userViewRoles.Length > 1 || userViewRoles[0] != 0) ? new BXFilterItem(Bitrix.Security.BXUser.Fields.GetFieldByKey("CheckRoles"), BXSqlFilterOperators.LessOrEqual, userViewRoles) : null;

		if (self != null && roles != null)
			filter.Add(new BXFilterOr(self, roles));
		else if (roles != null)
			filter.Add(roles);

		e.Data = Bitrix.Security.BXUser.GetList(
			filter,
			new BXOrderBy(Bitrix.Security.BXUser.Fields, e.SortExpression),
			new BXSelectAdd(Bitrix.Security.BXUser.Fields.CustomFields.DefaultFields),
			new BXQueryParams(e.PagingOptions),
			BXTextEncoder.EmptyTextEncoder
		);
	}
	protected void AuthUserGridView_SelectCount(object sender, BXSelectCountEventArgs e)
	{
		var filter = new BXFilter(BXAdminFilter1.CurrentFilter, Bitrix.Security.BXUser.Fields);
		var self = currentUserCanModifySelfUser ? new BXFilterItem(Bitrix.Security.BXUser.Fields.UserId, BXSqlFilterOperators.Equal, BXIdentity.Current.Id) : null;
		var roles = (userViewRoles.Length > 1 || userViewRoles[0] != 0) ? new BXFilterItem(Bitrix.Security.BXUser.Fields.GetFieldByKey("CheckRoles"), BXSqlFilterOperators.LessOrEqual, userViewRoles) : null;

		if (self != null && roles != null)
			filter.Add(new BXFilterOr(self, roles));
		else if (roles != null)
			filter.Add(roles);

		e.Count = Bitrix.Security.BXUser.Count(filter);
	}
	protected void AuthUserGridView_PopupMenuClick(object sender, Bitrix.UI.BXPopupMenuClickEventArgs e)
	{
		successMessage.Visible = false;
		e.Cancel = true;
		BXGridView grid = AuthUserGridView;
		DataKey drv = grid.DataKeys[e.EventRowIndex];

		int userId = 0;
		try
		{
			userId = (int)drv.Value;
		}
		catch
		{
			errorMessage.AddErrorMessage(GetMessage("ErrorMessage.CodeOfUserIsNotFound"));
			return;
		}


		if (e.CommandName == "edit" || e.CommandName == "view")
		{
			Response.Redirect(string.Format("AuthUsersEdit.aspx?id={0}", userId));
			return;
		}

		if (e.CommandName != "delete" && e.CommandName != "deleteProvider")
			return;

		//Delete
		try
		{
			if (!CanDelete(userId))
				throw new PublicException(GetMessageRaw("ExceptionText.YouDontHaveRightsToDeleteUsers"));

			BXUserCollection users = new BXUserCollection();
			users.Add(Bitrix.Security.BXUser.GetById(userId));
			DeleteUsers(users, e.CommandName == "deleteProvider");
			successMessage.Visible = true;
		}
		catch (Exception ex)
		{
			ProcessException(ex);
		}
		grid.MarkAsChanged();
	}
	protected void AuthUserGridView_MultiOperationActionRun(object sender, UserMadeChoiceEventArgs e)
	{
		if (e.CommandOfChoice.CommandName != "delete" && e.CommandOfChoice.CommandName != "deleteProvider")
			return;
		successMessage.Visible = false;
		e.Cancel = true;
		BXGridView grid = AuthUserGridView;
		try
		{


			BXUserCollection users;
			if (e.ApplyForAll)
			{
				users = Bitrix.Security.BXUser.GetList(
					new BXFilter(BXAdminFilter1.CurrentFilter, Bitrix.Security.BXUser.Fields),
					null
				);
			}
			else
			{
				List<object> keys = new List<object>();
				foreach (int index in grid.GetSelectedRowsIndices())
					keys.Add(grid.DataKeys[index]["UserId"]);

				users = Bitrix.Security.BXUser.GetList(
					new BXFilter(
						new BXFilterItem(Bitrix.Security.BXUser.Fields.UserId, BXSqlFilterOperators.In, keys)
					),
					null
				);
			}

			foreach (var user in users)
			{
				if (!CanDelete(user.UserId))
					throw new PublicException(GetMessageRaw("ExceptionText.YouDontHaveRightsToDeleteUsers"));
			}

			DeleteUsers(users, e.CommandOfChoice.CommandName == "deleteProvider");
			successMessage.Visible = true;
		}
		catch (Exception ex)
		{
			ProcessException(ex);
		}
		grid.MarkAsChanged();
	}

	private void DeleteUsers(BXUserCollection users, bool deleteProvider)
	{
		//if (!currentUserCanDeleteUser)
		//    throw new PublicException(GetMessageRaw("ExceptionText.YouDontHaveRightsToDeleteUsers"));

		foreach (Bitrix.Security.BXUser u in users)
		{
			if (u == null)
				throw new PublicException(GetMessageRaw("ExceptionText.UserIsNotFound"));

			try
			{
				u.Delete(deleteProvider);
			}
			catch (Exception ex)
			{
				throw new PublicException(GetMessageRaw("ExceptionText.DeletionOfUserFailed"), ex);
			}
		}
	}
	private void ProcessException(Exception ex)
	{
		if (ex is PublicException)
		{
			errorMessage.AddErrorMessage(Encode(ex.Message));
			if (ex.InnerException != null)
				BXLogService.LogAll(ex.InnerException, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
			return;
		}

		BXEventException eventException = ex as BXEventException;
		if (eventException != null)
		{
			foreach (string msg in eventException.Messages)
				errorMessage.AddErrorMessage(Encode(msg));
			return;
		}

		errorMessage.AddErrorMessage(GetMessage("Kernel.UnknownError"));
		BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
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
}
