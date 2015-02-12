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
using System.ComponentModel;
using Bitrix.Services;
using System.Collections.Generic;
using Bitrix.Modules;

using Bitrix.Configuration;
using Bitrix.IO;

public partial class bitrix_kernel_AdminBreadCrumb : BXControl
{
	protected List<BreadCrumbNode> crumbs = new List<BreadCrumbNode>();
	string adminMenuName = "AdminMenu1";

	[DefaultValue("AdminMenu1")]
	public string AdminMenuId
	{
		get
		{
			return adminMenuName;
		}
		set
		{
			adminMenuName = value;
		}
	}

	protected string FromAdminTheme(string path)
	{
		//return FromVirtual("~/App_Themes/" + BXConfigurationUtility.Constants.AdminTheme + "/" + path);
        //zg, 25.04.2008
        //return FromVirtual("~/App_Themes/" + BXConfigurationUtility.Constants.AdminTheme + "/" + path);
        return FromVirtual(Bitrix.UI.BXThemeHelper.SimpleCombineWithCurrentThemePath(path));
	}
	protected string FromVirtual(string path)
	{
		return HttpUtility.HtmlEncode(VirtualPathUtility.ToAbsolute(path));
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		if (DesignMode)
			return;
		bitrix_kernel_AdminMenu menu = BXControlUtility.FindControlInContainer(Page, AdminMenuId) as bitrix_kernel_AdminMenu;
        if (menu == null)
            return;
		string selected = menu.ActiveMenuKey;

		BreadCrumbNode node;
		node.Text = GetMessageRaw("BreadCrumbNodeText.Desktop");
		node.Url = BXPath.VirtualRootPath + "/bitrix/admin/";
		crumbs.Add(node);

		if (string.Equals("~/bitrix/admin/Default.aspx", Page.AppRelativeVirtualPath, StringComparison.InvariantCultureIgnoreCase))
			return;

		BXAdminMenuItemCollection col = menu.Menu;
		if (menu.ActiveSections != null)
			foreach (string id in menu.ActiveSections)
			{
				BXAdminMenuItem item = col[id];
				node.Text = item.Title;
				node.Url = string.IsNullOrEmpty(item.Url) ? crumbs[crumbs.Count - 1].Url : (BXPath.VirtualRootPath + item.Url);
				crumbs.Add(node);
				col = item.Children;
			}
	}

	protected struct BreadCrumbNode
	{
		public string Text;
		public string Url;
	}
}
