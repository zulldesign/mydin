using System;
using System.Text;
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
using Bitrix.UI;
using Bitrix.Services;
using Bitrix.Main;
using System.Collections.Generic;
using Bitrix;
using Bitrix.Security;
using Bitrix.DataLayer;
using Bitrix.Services.Text;

public partial class bitrix_admin_Language : Bitrix.UI.BXAdminPage
{
	private bool currentUserCanModify = false;

	protected void Page_Init(object sender, EventArgs e)
	{
		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
			BXAuthentication.AuthenticationRequired();

		currentUserCanModify = this.BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);
		AddButton.Visible = currentUserCanModify;
		if (!currentUserCanModify)
			GridView.PopupCommandMenuId = PopupPanelView.ID;
	}
	protected void Page_Load(object sender, EventArgs e)
	{
		Title = GetMessage("PageTitle");
		((BXAdminMasterPage)Master).Title = GetMessage("MasterTitle");
	}

	protected void GridView_PopupMenuClick(object sender, BXPopupMenuClickEventArgs e)
	{
		BXGridView grid = sender as BXGridView;
		DataKey drv = grid.DataKeys[e.EventRowIndex];
		if (drv == null)
			return;

		switch (e.CommandName)
		{
			case "copy":
				Response.Redirect(string.Format("LanguageEdit.aspx?copy={0}", drv.Value));
				break;
			case "edit":
				Response.Redirect(string.Format("LanguageEdit.aspx?id={0}", drv.Value));
				break;
		}
	}
	protected void Toolbar_CommandClick(object sender, CommandEventArgs e)
	{
		switch (e.CommandName.ToLower())
		{
			case "add":
				Response.Redirect("LanguageEdit.aspx");
				break;
			default:
				break;
		}
	}
	protected void GridView_Select(object sender, BXSelectEventArgs e)
	{
		e.Data = BXLanguage.GetList(
			null,
			new BXOrderBy(BXLanguage.Fields, e.SortExpression),
			null,
			new BXQueryParams(e.PagingOptions),
			BXTextEncoder.EmptyTextEncoder
		);
	}
	protected void GridView_SelectCount(object sender, BXSelectCountEventArgs e)
	{
		e.Count = BXLanguage.Count(null);
	}
	protected void GridView_Delete(object sender, BXDeleteEventArgs e)
	{
		try
		{
			if (!currentUserCanModify)
				throw new UnauthorizedAccessException(GetMessageRaw("Exception.YouDontHaveRightsToPerformThisOperation"));
			if (e.Keys != null)
				BXLanguage.Delete(e.Keys["Id"]);
			else
				BXLanguage.Delete(null);
			e.DeletedCount++;
		}
		catch (Exception ex)
		{
			Summary.AddErrorMessage(ex.Message);
		}
	}
}