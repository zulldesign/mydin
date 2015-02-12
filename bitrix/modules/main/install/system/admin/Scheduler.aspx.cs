using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.ObjectModel;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Threading;
using System.Text;
using Bitrix.Services;
using Bitrix.UI;
using Bitrix.Security;
using Bitrix.DataLayer;
using System.Security;


public partial class bitrix_admin_Scheduler : Bitrix.UI.BXAdminPage
{
	int showMessage;
	bool currentUserCanModify;

	void PrepareResultMessage()
	{
		successMessage.Visible = showMessage > 0;
	}
	void ShowError(string encodedMessage)
	{
		showMessage = -1;
		errorMessage.AddErrorMessage(encodedMessage);
	}
	void ShowError(Exception ex)
	{
		showMessage = -1;
		errorMessage.AddErrorMessage(Encode(ex.Message));
	}
	void ShowOk()
	{
		if (showMessage == 0)
			showMessage = 1;
	}

	protected void Grid_PopupMenuClick(object sender, Bitrix.UI.BXPopupMenuClickEventArgs e)
	{
		try
		{
			switch (e.CommandName)
			{
				case "active":
					if (!currentUserCanModify)
						throw new SecurityException(GetMessageRaw("Exception.YouDontHaveRightsToPerformThisOperation"));
					BXSchedulerAgent agent = BXSchedulerAgent.GetById(Grid.DataKeys[e.EventRowIndex].Value);
					if (agent != null)
					{
						agent.Active = !agent.Active;
						agent.Save();
					}
					Grid.MarkAsChanged();
					ShowOk();
					break;
			}
		}
		catch (Exception ex)
		{
			ShowError(ex);
		}
	}
	protected void Page_Init(object sender, EventArgs e)
	{
		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
			BXAuthentication.AuthenticationRequired();
		currentUserCanModify = this.BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);
		if (!currentUserCanModify)
			BXPopupPanel1.SetNoOperations();
	}
	protected void Page_LoadComplete(object sender, EventArgs e)
	{
		MasterTitle = GetMessage("MasterTitle");
		PrepareResultMessage();
	}

	protected void Grid_Select(object sender, BXSelectEventArgs e)
	{
		e.Data = BXSchedulerAgent.GetList(
			new BXFilter(Filter.CurrentFilter, BXSchedulerAgent.Fields),
			new BXOrderBy(BXSchedulerAgent.Fields, e.SortExpression),
			null,
			new BXQueryParams(e.PagingOptions)
		);
	}
	protected void Grid_SelectCount(object sender, BXSelectCountEventArgs e)
	{
		e.Count = BXSchedulerAgent.Count(new BXFilter(Filter.CurrentFilter, BXSchedulerAgent.Fields));
	}
	protected void Grid_Delete(object sender, BXDeleteEventArgs e)
	{
		try
		{
			if (!currentUserCanModify)
				throw new SecurityException(GetMessageRaw("Exception.YouDontHaveRightsToPerformThisOperation"));

			if (e.Keys == null)
				BXSchedulerAgent.Delete(null);
			else
				BXSchedulerAgent.Delete(e.Keys["Id"]);
			e.DeletedCount = 1;
			ShowOk();
		}
		catch (Exception ex)
		{
			ShowError(ex);
		}
	}
}
