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
using System.Collections.Generic;

public partial class bitrix_admin_ContentIndex : BXAdminPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		((BXAdminMasterPage)Page.Master).Title = Page.Title;

		if (!Page.IsPostBack)
		{
			StringBuilder sb = new StringBuilder();
			List<string> modules = new List<string>();
			foreach (BXModule m in BXModuleManager.InstalledModules)
				modules.Add(m.GetType().FullName);
			BXAdminMenuManager.ShowSectionIndex(sb, "global_menu_content", modules.ToArray(), Request["show_mode"], Request["mode"]);
			divBody.InnerHtml = sb.ToString();
			sb.Length = 0;
		}
	}

	protected void BXContextMenuToolbar1_CommandClick(object sender, CommandEventArgs e)
	{
		StringBuilder sb = new StringBuilder();
		List<string> modules = new List<string>();
		foreach (BXModule m in BXModuleManager.InstalledModules)
			modules.Add(m.GetType().FullName);

		switch (e.CommandName)
		{
			case "icon":
			case "list":
				BXAdminMenuManager.ShowSectionIndex(sb, "global_menu_content", modules.ToArray(), e.CommandName, "list");
				divBody.InnerHtml = sb.ToString();
				sb.Length = 0;
				break;
		}
	}
}
