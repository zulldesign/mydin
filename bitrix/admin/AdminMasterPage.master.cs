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
using Bitrix.Services;
using Bitrix.Security;
using Bitrix.Modules;
using Bitrix.IO;
using Bitrix.Configuration;

public partial class BXDefaultAdminMasterPage : BXAdminMasterPage
{
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		BXCsrfToken.ValidatePost();
		var menu = AdminMenu1;
		if (menu != null && !string.IsNullOrEmpty(menu.ActiveMenuKey))
		{
			var col = menu.Menu;
			if (menu.ActiveSections != null)
				foreach (string id in menu.ActiveSections)
				{
					var item = col[id];
					if (!string.IsNullOrEmpty(item.PageIcon))
						PageIconId = item.PageIcon;
					col = item.Children;
				}
		}
	}

	protected override void OnPreRender(EventArgs e)
	{
		AdminMenu1.Visible = AboutAdminMenuShow;

		BXAdminPage page = Page as BXAdminPage;
		if (page == null)
			throw new InvalidOperationException("Page must inherit from BXAdminPage!");
		//---
		//BXPage.RegisterThemeStyle("pubstyles.css");
		//BXPage.RegisterThemeStyle("pubstyles_net.css");
		//BXPage.RegisterThemeStyle("stylesheet.css");
		BXPage.RegisterThemeStyle("adminstyles.css");

		foreach (BXModule module in BXModuleManager.InstalledModules)
		{
			if (module == null || string.IsNullOrEmpty(module.ModuleId))
				continue;
			string fileName = module.ModuleId + ".css";
			if (BXSecureIO.FileExists(BXThemeHelper.SimpleCombineWithCurrentThemePath(fileName)))
				BXPage.RegisterThemeStyle(fileName);
		}
		//BXPage.RegisterThemeStyle("start_menu.css");

		base.OnPreRender(e);
	}

	protected bool AboutAdminMenuShow
	{
		get { return HttpContext.Current != null && HttpContext.Current.User.Identity.IsAuthenticated; }
	}
}
