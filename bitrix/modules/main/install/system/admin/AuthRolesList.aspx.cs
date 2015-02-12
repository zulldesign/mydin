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
using Bitrix.Security;
using Bitrix.DataLayer;
using Bitrix.Services;

public partial class bitrix_admin_RolesList : BXAdminPage
{
	bool currentUserCanModifySettings = false;
	protected void Page_Init(object sender, EventArgs e)
	{
		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.AuthorizationSettingsView))
			BXAuthentication.AuthenticationRequired();

		currentUserCanModifySettings = this.BXUser.IsCanOperate(BXRoleOperation.Operations.AuthorizationSettingsManage);
		SynchronizeButton.Visible = AddButton.Visible = currentUserCanModifySettings;
		if (!currentUserCanModifySettings)
			AuthRolesGridView.PopupCommandMenuId = PopupPanelView.ID;
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		((BXAdminMasterPage)Page.Master).Title = Page.Title;
	}

	protected void AuthRolesGridView_Select(object sender, Bitrix.UI.BXSelectEventArgs e)
	{
		int startRowIndex = 0;
		int maximumRows = 0;
		if (e.PagingOptions != null)
		{
			startRowIndex = e.PagingOptions.startRowIndex;
			maximumRows = e.PagingOptions.maximumRows;
		}

		BXOrderBy_old sortOrder = BXDatabaseHelper.ConvertOrderBy(e.SortExpression);

		BXFormFilter filter = BXAdminFilter1.CurrentFilter;

		BXRoleCollection roleCollection = BXRoleManager.GetList(filter, sortOrder, (maximumRows > 0 ? startRowIndex + maximumRows : 0));
		e.Data = new DataView(FillTable(roleCollection, startRowIndex, maximumRows));
	}

	private DataTable FillTable(BXRoleCollection roleCollection, int startRowIndex, int maximumRows)
	{
		if (roleCollection == null)
			roleCollection = new BXRoleCollection();

		DataTable result = new DataTable();

		result.Columns.Add("num", typeof(int));
		result.Columns.Add("RoleId", typeof(int));
		result.Columns.Add("RoleName", typeof(string));
		result.Columns.Add("Active", typeof(string));
		result.Columns.Add("Comment", typeof(string));
		result.Columns.Add("Policy", typeof(string));
		result.Columns.Add("EffectivePolicy", typeof(string));
		result.Columns.Add("UsersCount", typeof(int));
		result.Columns.Add("RolesCount", typeof(int));

		int ind = -1;
		foreach (BXRole t in roleCollection)
		{
			ind++;
			if (startRowIndex > ind)
				continue;

			DataRow r = result.NewRow();
			r["num"] = ind;
			r["RoleId"] = t.RoleId;
			r["RoleName"] = t.Title;
			r["Active"] = (t.Active ? GetMessageRaw("Kernel.Yes") : GetMessageRaw("Kernel.No"));
			r["Comment"] = t.Comment;
			r["Policy"] = t.Policy;
			r["EffectivePolicy"] = t.EffectivePolicy;
			r["UsersCount"] = t.UsersCount;
			r["RolesCount"] = t.RolesCount;
			result.Rows.Add(r);

			if (maximumRows > 0 && ind == startRowIndex + maximumRows - 1)
				break;
		}

		return result;
	}

	private void DeleteRoles(BXRoleCollection roles, bool deleteProvider)
	{
		if (!currentUserCanModifySettings)
			throw new PublicException(GetMessageRaw("ExceptionText.YouDontHaveRightsToDeleteThisRecord"));

		foreach (BXRole r in roles)//BXRoleManager.GetById(roleId);
		{
			if (r == null)
				throw new PublicException(GetMessageRaw("ExceptionText.RoleIsNotFound"));

			if (!BXRoleManager.Delete(r.RoleId, false, deleteProvider))
				throw new PublicException(GetMessageRaw("ExceptionText.DeletionOfRoleIsFailed"));
		}
	}

	protected void AuthRolesGridView_PopupMenuClick(object sender, Bitrix.UI.BXPopupMenuClickEventArgs e)
	{
		successMessage.Visible = false;
		e.Cancel = true;
		BXGridView grid = (BXGridView)sender;
		DataKey drv = grid.DataKeys[e.EventRowIndex];
		
		int roleId = 0;
		try
		{
			 roleId = (int)drv.Value;
		}
		catch
		{
			errorMessage.AddErrorMessage(GetMessage("ErrorMessage.CodeOfRoleIsNotFound"));
			return;
		}


		if (e.CommandName == "edit")
		{
			Response.Redirect(string.Format("AuthRolesEdit.aspx?id={0}", roleId));
			return;
		}

		if (e.CommandName != "delete" && e.CommandName != "deleteProvider")
			return;

		//Delete
		try
		{
			if (!currentUserCanModifySettings)
				throw new PublicException(GetMessageRaw("ExceptionText.YouDontHaveRightsToDeleteThisRecord"));

			BXRoleCollection roles = new BXRoleCollection();
			roles.Add(BXRoleManager.GetById(roleId));
			DeleteRoles(roles, e.CommandName == "deleteProvider");
			successMessage.Visible = true;
		}
		catch (PublicException ex)
		{
			errorMessage.AddErrorMessage(Encode(ex.Message));
		}
		catch (Exception ex)
		{
			errorMessage.AddErrorMessage(GetMessage("Kernel.UnknownError"));
			BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
		}
		grid.MarkAsChanged();
	}

	protected void AuthRolesGridView_SelectCount(object sender, BXSelectCountEventArgs e)
	{
		BXFormFilter filter = BXAdminFilter1.CurrentFilter;
		e.Count = BXRoleManager.Count(filter);
	}


	protected void AuthRolesGridView_MultiOperationActionRun(object sender, UserMadeChoiceEventArgs e)
	{

		if (e.CommandOfChoice.CommandName != "delete" && e.CommandOfChoice.CommandName != "deleteProvider")
			return;
		successMessage.Visible = false;
		e.Cancel = true;
		try
		{
			if (!currentUserCanModifySettings)
				throw new PublicException(GetMessageRaw("ExceptionText.YouDontHaveRightsToDeleteThisRecord"));

			BXRoleCollection roles;
			if (e.ApplyForAll)
				roles = BXRoleManager.GetList(BXAdminFilter1.CurrentFilter, null);
			else
			{
				roles = new BXRoleCollection();
				foreach (int i in AuthRolesGridView.GetSelectedRowsIndices())
				{
					DataKey row = AuthRolesGridView.DataKeys[i];
					roles.Add(BXRoleManager.GetById((int)row.Value));
				}
			}
			DeleteRoles(roles, e.CommandOfChoice.CommandName == "deleteProvider");
			successMessage.Visible = true;
		}
		catch (PublicException ex)
		{
			errorMessage.AddErrorMessage(Encode(ex.Message));
		}
		catch(Exception ex)
		{
			errorMessage.AddErrorMessage(GetMessage("Kernel.UnknownError"));
			BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
		}
		AuthRolesGridView.MarkAsChanged();
	}
}
