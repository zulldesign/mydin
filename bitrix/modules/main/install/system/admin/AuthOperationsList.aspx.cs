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
using Bitrix.Modules;
using System.Collections.ObjectModel;
using Bitrix.Services;

public partial class bitrix_admin_AuthOperationsList : BXAdminPage
{
	bool currentUserCanModifySettings = false;

	protected void Page_Init(object sender, EventArgs e)
	{
		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.AuthorizationSettingsView))
			BXAuthentication.AuthenticationRequired();

		currentUserCanModifySettings = this.BXUser.IsCanOperate(BXRoleOperation.Operations.AuthorizationSettingsManage);
		AddButton.Visible = currentUserCanModifySettings;
		if (!currentUserCanModifySettings)
			AuthOperationsGridView.PopupCommandMenuId = PopupPanelView.ID;

		BXDropDownFilter1.Values.Clear();
		BXDropDownFilter1.Values.Add(new ListItem(GetMessageRaw("FilterValue.All"), ""));
		foreach (BXModule moduleTmp in BXModuleManager.InstalledModules)
			BXDropDownFilter1.Values.Add(new ListItem(moduleTmp.Name, moduleTmp.ModuleId));
	}
	protected void Page_Load(object sender, EventArgs e)
	{
		((BXAdminMasterPage)Page.Master).Title = Page.Title;
	}

	protected void AuthOperationsGridView_Select(object sender, Bitrix.UI.BXSelectEventArgs e)
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

		BXRoleOperationCollection operationCollection = BXRoleOperationManager.GetList(filter, sortOrder, (maximumRows > 0 ? startRowIndex + maximumRows : 0));
		e.Data = new DataView(FillTable(operationCollection, startRowIndex, maximumRows));
	}

	private DataTable FillTable(BXRoleOperationCollection operationCollection, int startRowIndex, int maximumRows)
	{
		if (operationCollection == null)
			operationCollection = new BXRoleOperationCollection();

		DataTable result = new DataTable();

		result.Columns.Add("num", typeof(int));
		result.Columns.Add("OperationId", typeof(int));
		result.Columns.Add("OperationName", typeof(string));
		result.Columns.Add("OperationType", typeof(string));
		result.Columns.Add("ModuleID", typeof(string));
		result.Columns.Add("ModuleName", typeof(string));
		result.Columns.Add("Comment", typeof(string));

		int ind = -1;
		foreach (BXRoleOperation t in operationCollection)
		{
			ind++;
			if (startRowIndex > ind)
				continue;

			DataRow r = result.NewRow();
			r["num"] = ind;
			r["OperationId"] = t.OperationId;
			r["OperationName"] = t.OperationFriendlyName;
			r["OperationType"] = t.OperationType;
			r["ModuleID"] = t.ModuleId;
			r["ModuleName"] = t.ModuleId;
			if (!string.IsNullOrEmpty(t.ModuleId))
			{
				BXModule m = BXModuleManager.GetModule(t.ModuleId);
				if (m != null)
					r["ModuleName"] = m.Name;
			}
			r["Comment"] = t.Comment;
			result.Rows.Add(r);

			if (maximumRows > 0 && ind == startRowIndex + maximumRows - 1)
				break;
		}

		return result;
	}

	protected void AuthOperationsGridView_PopupMenuClick(object sender, Bitrix.UI.BXPopupMenuClickEventArgs e)
	{
		BXGridView grid = sender as BXGridView;

		DataKey drv = grid.DataKeys[e.EventRowIndex];
		if (drv == null)
			return;

		int operationId;
		Int32.TryParse(drv.Value.ToString(), out operationId);
		
		if (operationId > 0)
		{
			switch (e.CommandName)
			{
				case "edit":
					e.Cancel = true;
					Response.Redirect(String.Format("AuthOperationsEdit.aspx?id={0}", operationId));
					break;
				default:
					break;
			}
		}
		else
		{
			errorMessage.AddErrorMessage(GetMessageRaw("ErrorMessage.CodeOfOperationIsNotFound"));
		}
	}

	protected void AuthOperationsGridView_SelectCount(object sender, BXSelectCountEventArgs e)
	{
		BXFormFilter filter = BXAdminFilter1.CurrentFilter;
		e.Count = BXRoleOperationManager.Count(filter);
	}
	protected void AuthOperationsGridView_Delete(object sender, BXDeleteEventArgs e)
	{
		BXGridView grid = (BXGridView)sender;
		try
		{
			if (!currentUserCanModifySettings)
				throw new PublicException(GetMessageRaw("ExceptionText.YouDontHaveRightsToDeleteThisRecord"));

			BXRoleOperationCollection elements;
			if (e.Keys != null) //Delete one element
			{
				elements = new BXRoleOperationCollection();
				elements.Add(BXRoleOperationManager.GetById((int)e.Keys["OperationId"]));
			}
			else //All elements
			{
				elements = BXRoleOperationManager.GetList(BXAdminFilter1.CurrentFilter, null);
			}

			foreach(BXRoleOperation element in elements)
			{
				if (element == null)
					throw new PublicException(GetMessageRaw("ExceptionText.OperationIsNotFound"));
				if (!BXRoleOperationManager.Delete(element.OperationId))
					throw new PublicException(GetMessageRaw("ExceptionText.DeletionIsFailed"));
				e.DeletedCount++;
			}
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
		grid.MarkAsChanged();
	}
}
