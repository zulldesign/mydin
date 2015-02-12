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
using Bitrix.DataLayer;
using Bitrix.Modules;
using Bitrix.Services;
using Bitrix.UI;
using Bitrix.Security;
using Bitrix.Services.Text;

public partial class bitrix_admin_Mailer : BXAdminPage
{
	private bool currentUserCanModify = false;

	protected string GetSitesList(object source)
	{
		BXMailerTemplate data = (BXMailerTemplate)source;
		string result = String.Join("<br/>", data.Sites.ConvertAll<string>(delegate(BXMailerTemplate.BXSite input)
		{
			return HttpUtility.HtmlEncode(input.SiteId);
		}).ToArray());
		if (String.IsNullOrEmpty(result))
			result = "&nbsp;";
		return result;
	}
	protected void MailerGridView_PopupMenuClick(object sender, BXPopupMenuClickEventArgs e)
	{
		BXGridView grid = (BXGridView)sender;
		switch (e.CommandName)
		{
			case "copy":
				Response.Redirect("MailerEdit.aspx?action=add&id=" + (int)grid.DataKeys[e.EventRowIndex]["Id"]);
				break;
		}
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
			BXAuthentication.AuthenticationRequired();

		currentUserCanModify = this.BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);
		if (!currentUserCanModify)
			MailerGridView.PopupCommandMenuId = PopupPanelView.ID;

		foreach (BXMailEventType eventType in BXMailEventTypeManager.EventTypes)
			templateFilter.Values.Add(new ListItem(String.Format("[{1}] == {0}", HttpUtility.HtmlEncode(eventType.DisplayName), HttpUtility.HtmlEncode(eventType.Id)), HttpUtility.HtmlEncode(eventType.Id)));
		BXSiteCollection siteColl = BXSite.GetList(null, null);
		foreach (BXSite site in siteColl)
			siteFilter.Values.Add(new ListItem("[" + HttpUtility.HtmlEncode(site.Id) + "] " + HttpUtility.HtmlEncode(site.Name), HttpUtility.HtmlEncode(site.Id)));
	}
	protected void Page_LoadComplete(object sender, EventArgs e)
	{
		MasterTitle = GetMessage("MasterTitle");
		AddButton.Visible = BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);
	}
	protected void MailerGridView_Select(object sender, BXSelectEventArgs e)
	{
		e.Data = BXMailerTemplate.GetList(
			new BXFilter(Filter.CurrentFilter, BXMailerTemplate.Fields),
			new BXOrderBy(BXMailerTemplate.Fields, e.SortExpression),
			null,
			new BXQueryParams(e.PagingOptions),
			BXTextEncoder.HtmlTextEncoder
		);
	}
	protected void MailerGridView_SelectCount(object sender, BXSelectCountEventArgs e)
	{
		e.Count = BXMailerTemplate.Count(new BXFilter(Filter.CurrentFilter, BXMailerTemplate.Fields));// Manager.GetTemplatesCount();
	}
	protected void MailerGridView_Delete(object sender, BXDeleteEventArgs e)
	{
		try
		{
			if (!currentUserCanModify)
				return;

			BXMailerTemplateCollection templates;

			if (e.Keys != null)
			{
				templates = new BXMailerTemplateCollection();
				templates.Add(BXMailerTemplate.GetById(e.Keys["Id"]));
			}
			else
				templates = BXMailerTemplate.GetList(new BXFilter(Filter.CurrentFilter, BXMailerTemplate.Fields), null);

			foreach (BXMailerTemplate template in templates)
			{
				if (template == null)
					continue;
				template.Delete();
				e.DeletedCount++;
			}
		}
		catch
		{
		}
	}
	protected void MailerGridView_Update(object sender, BXUpdateEventArgs e)
	{
		if (!currentUserCanModify)
			return;
		BXMailerTemplate t = BXMailerTemplate.GetById(e.Keys["Id"]);
		if (t != null)
		{
			if (e.NewValues.Contains("Subject"))
				t.Subject = (string)e.NewValues["Subject"];
			if (e.NewValues.Contains("Active"))
				t.Active = (bool)e.NewValues["Active"];
			t.Save();
			e.UpdatedCount = 1;
		}
	}
	protected void MailerGridView_RowDataBound(object sender, GridViewRowEventArgs e)
	{
		if (e.Row.RowType != DataControlRowType.DataRow)
			return;
		BXGridViewRow row = (BXGridViewRow)e.Row;
		BXMailerTemplate template = (BXMailerTemplate)row.DataItem;
		row.UserData["EditUrl"] = "MailerEdit.aspx?action=edit&id=" + template.Id;
	}
}
