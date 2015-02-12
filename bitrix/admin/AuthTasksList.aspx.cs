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

public partial class bitrix_admin_AuthTasksList : BXAdminPage
{
	bool currentUserCanModifySettings = false;
	protected void Page_Init(object sender, EventArgs e)
	{
		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.AuthorizationSettingsView))
			BXAuthentication.AuthenticationRequired();

		currentUserCanModifySettings = this.BXUser.IsCanOperate(BXRoleOperation.Operations.AuthorizationSettingsManage);
		AddButton.Visible = currentUserCanModifySettings;
		if (!currentUserCanModifySettings)
			TasksGridView.PopupCommandMenuId = PopupPanelView.ID;
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		((BXAdminMasterPage)Page.Master).Title = Page.Title;

		successMessage.Visible = false;

		
	}

	protected void TasksGridView_Select(object sender, Bitrix.UI.BXSelectEventArgs e)
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

		BXRoleTaskCollection taskCollection = BXRoleTaskManager.GetList(filter, sortOrder, (maximumRows > 0 ? startRowIndex + maximumRows : 0));
		e.Data = new DataView(FillTable(taskCollection, startRowIndex, maximumRows));
	}

	private DataTable FillTable(BXRoleTaskCollection taskCollection, int startRowIndex, int maximumRows)
	{
		if (taskCollection == null)
			taskCollection = new BXRoleTaskCollection();

		DataTable result = new DataTable();

		result.Columns.Add("num", typeof(int));
		result.Columns.Add("TaskId", typeof(int));
		result.Columns.Add("TaskName", typeof(string));
		result.Columns.Add("Comment", typeof(string));
		result.Columns.Add("TasksCount", typeof(int));
		result.Columns.Add("OperationsCount", typeof(int));

		int ind = -1;
		foreach (BXRoleTask t in taskCollection)
		{
			ind++;
			if (startRowIndex > ind)
				continue;

			DataRow r = result.NewRow();
			r["num"] = ind;
			r["TaskId"] = t.TaskId;
			r["TaskName"] = t.Title;
			r["Comment"] = t.Comment;
			r["TasksCount"] = t.TasksCount;
			r["OperationsCount"] = t.OperationsCount;
			result.Rows.Add(r);

			if (maximumRows > 0 && ind == startRowIndex + maximumRows - 1)
				break;
		}

		return result;
	}

	protected void AuthTasksGridView_PopupMenuClick(object sender, Bitrix.UI.BXPopupMenuClickEventArgs e)
	{
		BXGridView grid = sender as BXGridView;

		DataKey drv = grid.DataKeys[e.EventRowIndex];
		if (drv == null)
			return;

		int taskId;
		Int32.TryParse(drv.Value.ToString(), out taskId);

		if (taskId > 0)
		{
			switch (e.CommandName)
			{
				case "edit":
					Response.Redirect(String.Format("AuthTasksEdit.aspx?id={0}", taskId));
					break;
				default:
					break;
			}
		}
		else
		{
			errorMessage.AddErrorMessage(GetMessage("ExceptionText.CodeOfTaskIsNotFound"));
		}
	}

	protected void TasksGridView_Delete(object sender, Bitrix.UI.BXDeleteEventArgs e)
	{
		BXGridView grid = (BXGridView)sender;
		try
		{
			if (!currentUserCanModifySettings)
				throw new PublicException(GetMessageRaw("ExceptionText.YouDontHaveRightsToDeleteThisRecord"));

			BXRoleTaskCollection elements;
			if (e.Keys != null) //Delete one element
			{
				elements = new BXRoleTaskCollection();
				elements.Add(BXRoleTaskManager.GetById((int)e.Keys["TaskId"]));
			}
			else //All elements
			{
				elements = BXRoleTaskManager.GetList(BXAdminFilter1.CurrentFilter, null);
			}

			foreach (BXRoleTask element in elements)
			{
				if (element == null)
					throw new PublicException(GetMessageRaw("ExceptionText.TaskIsNotFound"));
				if (!BXRoleTaskManager.Delete(element.TaskId))
					throw new PublicException(GetMessageRaw("ExceptionText.DeletionOfTaskIsFailed"));
				e.DeletedCount++;
			}
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

	protected void TasksGridView_SelectCount(object sender, BXSelectCountEventArgs e)
	{
		BXFormFilter filter = BXAdminFilter1.CurrentFilter;
		e.Count = BXRoleTaskManager.Count(filter);
	}
}
