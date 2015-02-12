using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Bitrix.DataLayer;
using Bitrix.Services;
using Bitrix;
using Bitrix.Security;
using Bitrix.UI;
using System.Security;
using Bitrix.Services.Js;

public partial class bitrix_admin_UrlRewriting : Bitrix.UI.BXAdminPage
{
	bool updating;
	private bool currentUserCanModify = false;
	int showMessage;

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


	protected void BXGridView1_Select(object sender, Bitrix.UI.BXSelectEventArgs e)
	{
		e.Data = BXSefUrlRuleManager.GetList(BXAdminFilter1.CurrentFilter, BXDatabaseHelper.ConvertOrderBy(e.SortExpression ?? "page asc"), new BXQueryParams(e.PagingOptions));
	}
	protected void BXGridView1_SelectCount(object sender, Bitrix.UI.BXSelectCountEventArgs e)
	{
		e.Count = BXSefUrlRuleManager.Count(BXAdminFilter1.CurrentFilter);
	}
	protected void BXContextMenuToolbar1_CommandClick(object sender, CommandEventArgs e)
	{
		if (!BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage))
		{
			ShowError(GetMessage("Exception.YouDontHaveRightsToPerformThisOperation"));
			return;
		}
		switch (e.CommandName)
		{
			case "reload":
				BXSefUrlRuleManager.Load();
				BXGridView1.MarkAsChanged();
				break;
			case "refreshsef":
				{
					bool error = false;
					try
					{
						BXSefUrlManager.RefreshComponentSefState();
					}
					catch (Exception ex)
					{
						error = true;
						BXLogService.LogAll(ex, 0, BXLogMessageType.Error, string.Format("{0} ({1})", AppRelativeVirtualPath, "RefreshComponentSefState"));
					}
					ScriptManager.RegisterStartupScript(
						this,
						GetType(),
						"RefreshSef",
						string.Format("alert('{0}');", BXJSUtility.Encode(error ? GetMessageRaw("Error.Unknown") : GetMessageRaw("JsMessage.SefManagerRefreshHasBeenCompleted"))),
						true
					);
					BXGridView1.MarkAsChanged();
				}
				break;
		}
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		if (!BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
			BXAuthentication.AuthenticationRequired();

		currentUserCanModify = this.BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);

		AddButton.Visible = AddSeparator.Visible = currentUserCanModify;
		RefreshSefButton.Visible = RefreshSefSeparator.Visible = currentUserCanModify;
		if (!currentUserCanModify)
			BXGridView1.PopupCommandMenuId = PopupPanelView.ID;

		BXSiteCollection siteColl = BXSite.GetList(null, null);
		foreach (BXSite site in siteColl)
			siteIdFilter.Values.Add(new ListItem(site.Name, site.Id));
	}
	protected void Page_LoadComplete(object sender, EventArgs e)
	{
		if (updating)
			try
			{
				BXSefUrlRuleManager.EndUpdate();
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		MasterTitle = GetMessage("MasterTitle");
		PrepareResultMessage();
	}

	protected void BXGridView1_Delete(object sender, Bitrix.UI.BXDeleteEventArgs e)
	{
		try
		{
			if (!currentUserCanModify)
				throw new SecurityException(GetMessageRaw("Exception.YouDontHaveRightsToPerformThisOperation"));
			if (!updating)
			{
				updating = true;
				BXSefUrlRuleManager.BeginUpdate();
			}
			BXFormFilter filter = (e.Keys != null) ? new BXFormFilter(new BXFormFilterItem("id", e.Keys["Id"], BXSqlFilterOperators.Equal)) : BXAdminFilter1.CurrentFilter;
			e.DeletedCount = BXSefUrlRuleManager.Delete(filter);
			ShowOk();
		}
		catch (Exception ex)
		{
			ShowError(ex);
		}
	}
	protected void BXGridView1_Update(object sender, Bitrix.UI.BXUpdateEventArgs e)
	{
		try
		{
			if (!currentUserCanModify)
				throw new SecurityException(GetMessageRaw("Exception.YouDontHaveRightsToPerformThisOperation"));
			BXSefUrlRule rule = BXSefUrlRuleManager.Get((int)e.Keys["Id"]);
			if (rule == null)
				return;
			if (!updating)
			{
				updating = true;
				BXSefUrlRuleManager.BeginUpdate();
			}
			if (e.NewValues.Contains("MatchExpression"))
				rule.MatchExpression = (string)e.NewValues["MatchExpression"];
			if (e.NewValues.Contains("ReplaceExpression"))
				rule.ReplaceExpression = (string)e.NewValues["ReplaceExpression"];
			if (e.NewValues.Contains("SiteId"))
				rule.SiteId = (string)e.NewValues["SiteId"];
			if (e.NewValues.Contains("Sort"))
			{
				int i;
				if (int.TryParse((string)e.NewValues["Sort"], out i))
					rule.Sort = i;
			}
			if (e.NewValues.Contains("HelperId"))
				rule.HelperId = (string)e.NewValues["HelperId"];
			if (e.NewValues.Contains("Ignore"))
				rule.Ignore = (bool)e.NewValues["Ignore"];
			ShowOk();
		}
		catch (Exception ex)
		{
			ShowError(ex);
		}
	}
	protected void BXGridView1_RowUpdating(object sender, GridViewUpdateEventArgs e)
	{
		GridView g = (GridView)sender;
		DropDownList l = g.Rows[e.RowIndex].FindControl("siteIdEdit") as DropDownList;
		if (l != null)
			e.NewValues["SiteId"] = l.SelectedValue;
	}
	protected void BXGridView1_RowDataBound(object sender, GridViewRowEventArgs e)
	{
		if (e.Row.RowType != DataControlRowType.DataRow)
			return;
		BXGridViewRow row = (BXGridViewRow)e.Row;
		BXSefUrlRule rule = (BXSefUrlRule)row.DataItem;

		row.UserData.Add("id", rule.Id);

		if ((row.RowState & DataControlRowState.Edit) > 0)
		{
			DropDownList l = row.FindControl("siteIdEdit") as DropDownList;
			if (l != null)
				l.SelectedValue = rule.SiteId;
		}
	}
}
