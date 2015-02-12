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
using System.Text;
using Bitrix.Modules;

public partial class bitrix_admin_SettingsIndex : BXAdminPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		((BXAdminMasterPage)Page.Master).Title = Page.Title;

		if (!Page.IsPostBack)
		{
			StringBuilder sb = new StringBuilder();
			BXAdminMenuManager.ShowSectionIndex(sb, "main_settings", BXModuleManager.GetModule("main").GetType().FullName, Request["show_mode"], Request["mode"]);
			divBody.InnerHtml = sb.ToString();
		}
	}

	protected void BXContextMenuToolbar1_CommandClick(object sender, CommandEventArgs e)
	{
		StringBuilder sb = new StringBuilder();
		switch (e.CommandName)
		{
			case "icon":
				BXAdminMenuManager.ShowSectionIndex(sb, "main_settings", BXModuleManager.GetModule("main").GetType().FullName, "icon", "list");
				divBody.InnerHtml = sb.ToString();
				break;
			case "list":
				BXAdminMenuManager.ShowSectionIndex(sb, "main_settings", BXModuleManager.GetModule("main").GetType().FullName, "list", "list");
				divBody.InnerHtml = sb.ToString();
				break;
		}
	}
}
