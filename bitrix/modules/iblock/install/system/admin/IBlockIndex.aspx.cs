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
using Bitrix.Modules;
using System.Text;
using Bitrix.IBlock;
using Bitrix.Security;

public partial class bitrix_admin_IBlockIndex : BXAdminPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		if (!this.BXUser.IsCanOperate(BXIBlock.Operations.IBlockAdminRead))
			BXAuthentication.AuthenticationRequired();

		((BXAdminMasterPage)Page.Master).Title = Page.Title;
		//BXCmImageButton1.OnClickScript = String.Format("window.location.href='{0}?show_mode={1}&mode=list';", Request.CurrentExecutionFilePath, "icon");
		//BXCmImageButton2.OnClickScript = String.Format("window.location.href='{0}?show_mode={1}&mode=list';", Request.CurrentExecutionFilePath, "list");

		if (!Page.IsPostBack)
		{
			StringBuilder sb = new StringBuilder();
			BXAdminMenuManager.ShowSectionIndex(sb, "menu_iblock", BXModuleManager.GetModule("iblock").GetType().FullName, Request["show_mode"], Request["mode"]);
			divBody.InnerHtml = sb.ToString();
		}
	}

	protected void BXContextMenuToolbar1_CommandClick(object sender, CommandEventArgs e)
	{
		StringBuilder sb = new StringBuilder();
		switch (e.CommandName)
		{
			case "icon":
				BXAdminMenuManager.ShowSectionIndex(sb, "menu_iblock", BXModuleManager.GetModule("iblock").GetType().FullName, "icon", "list");
				divBody.InnerHtml = sb.ToString();
				break;
			case "list":
				BXAdminMenuManager.ShowSectionIndex(sb, "menu_iblock", BXModuleManager.GetModule("iblock").GetType().FullName, "list", "list");
				divBody.InnerHtml = sb.ToString();
				break;
		}
	}
}
