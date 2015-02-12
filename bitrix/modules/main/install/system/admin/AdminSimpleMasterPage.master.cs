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
using Bitrix.Services.Js;
using Bitrix.Security;

public partial class bitrix_admin_AdminSimpleMasterPage : BXAdminMasterPage
{
	protected override void OnInit(EventArgs e)
	{
		BXCsrfToken.ValidatePost();
		base.OnInit(e);
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		//BXPage.RegisterThemeStyle("pubstyles.css");
		//BXPage.RegisterThemeStyle("pubstyles_net.css");
		//BXPage.RegisterThemeStyle("stylesheet.css");
		BXPage.RegisterThemeStyle("adminstyles.css");

		//if (!Page.ClientScript.IsClientScriptIncludeRegistered("JsBlock"))
		//    Page.ClientScript.RegisterClientScriptInclude("JsBlock", VirtualPathUtility.ToAbsolute("~/bitrix/kernel/js/admin_tools.js") + "?12");
		if (!Page.ClientScript.IsStartupScriptRegistered(this.GetType(), "JsBlock1"))
			Page.ClientScript.RegisterStartupScript(this.GetType(), "JsBlock1", "jsUtils.addEvent(window, \"keypress\", PopupOnKeyPress);", true);
	}
}
