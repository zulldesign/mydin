using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Bitrix;
using Bitrix.Main;
using Bitrix.Services;
using Bitrix.UI;
using Bitrix.Security;
using Bitrix.DataLayer;
using Bitrix.Modules;
using Bitrix.Services.Text;

public partial class bitrix_admin_Site : Bitrix.UI.BXAdminPage
{
	bool currentUserCanModify;
	protected void Page_Init(object sender, EventArgs e)
	{
		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
			BXAuthentication.AuthenticationRequired();

		currentUserCanModify = BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);
		AddButton.Visible = currentUserCanModify;
		if (!currentUserCanModify)
			GridView.PopupCommandMenuId = PopupPanelView.ID;

		filterLanguageId.Values.Add(new ListItem(GetMessage("Any"), ""));
		BXLanguageCollection lc = BXLanguage.GetList(null, new BXOrderBy(new BXOrderByPair(BXLanguage.Fields.ID, BXOrderByDirection.Asc)));
		foreach (BXLanguage lang in lc)
			filterLanguageId.Values.Add(new ListItem("[" + lang.Id + "] " + lang.Name, lang.Id));
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		Page.Title = GetMessage("Title.SiteList");
		((BXAdminMasterPage)Page.Master).Title = Page.Title;
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
				Response.Redirect(string.Format("SiteEdit.aspx?copy={0}", drv.Value));
				break;
			case "edit":
				Response.Redirect(string.Format("SiteEdit.aspx?id={0}", drv.Value));
				break;
			default:
				break;
		}
	}

	protected void Toolbar_CommandClick(object sender, CommandEventArgs e)
	{
		switch (e.CommandName.ToLower())
		{
			case "add":
				Response.Redirect("SiteEdit.aspx");
				break;
		}
	}

	protected void GridView_Select(object sender, BXSelectEventArgs e)
	{
		e.Data = BXSite.GetList(
			new BXFilter(
				BXAdminFilter1.CurrentFilter,
				BXSite.Fields
			),
			new BXOrderBy(BXSite.Fields, e.SortExpression),
			null,
			new BXQueryParams(e.PagingOptions),
			BXTextEncoder.EmptyTextEncoder
		);
	}

	protected void GridView_SelectCount(object sender, BXSelectCountEventArgs e)
	{
		e.Count = BXSite.Count(
			new BXFilter(
				BXAdminFilter1.CurrentFilter,
				BXSite.Fields
			)
		);
	}

	protected void GridView_Delete(object sender, BXDeleteEventArgs e)
	{
		try
		{
			if (!currentUserCanModify)
				throw new PublicException(GetMessageRaw("Exception.YouDontHaveRightsToDeleteThisRecord"));

			BXSiteCollection sites;

			if (e.Keys != null)
			{
				sites = new BXSiteCollection();
				sites.Add(BXSite.GetById(e.Keys["ID"]));
			}
			else
				sites = BXSite.GetList(new BXFilter(BXAdminFilter1.CurrentFilter, BXSite.Fields), null);

			foreach (BXSite site in sites)
			{
				if (site == null)
					continue;
                try
                {
                    site.Delete();
                }
                catch (InvalidOperationException ex)
                {
                    throw new PublicException(ex);
                }
				e.DeletedCount++;
			}
		}
		catch (Exception ex)
		{
			ProcessException(ex, errorMessage.AddErrorText);
		}
	}
}