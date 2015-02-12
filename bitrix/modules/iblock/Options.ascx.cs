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
using Bitrix.Services;
using System.Collections.Generic;
using Bitrix.UI;
using System.IO;
using Bitrix;
using Bitrix.Security;
using Bitrix.Configuration;

public partial class bitrix_modules_iblock_Options : BXControl
{
	bool canManageSettings;

	protected void Page_Init(object sender, EventArgs e)
	{
		canManageSettings = BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);
		BXTabControl1.ShowApplyButton = BXTabControl1.ShowSaveButton = canManageSettings;
		BXTabControl1.ShowCancelButton = Request.QueryString[BXConfigurationUtility.Constants.BackUrl] != null;
	}
	protected void Page_Load(object sender, EventArgs e)
	{
		if (!Page.IsPostBack)
		{
			LoadData();
		}
		else
		{
			Control c = (this.Page as BXAdminPage).FindControlRecursive("ddlEntitySelector");
			if (c != null)
			{
				DropDownList ddl = (c as DropDownList);
				if (ddl != null)
				{
					string t = Server.UrlDecode(Page.Request.Params["__EVENTTARGET"]);
					if (t.Equals(ddl.UniqueID, StringComparison.InvariantCultureIgnoreCase))
						LoadData();
				}
			}
		}
	}

	protected void BXTabControl1_Command(object sender, Bitrix.UI.BXTabControlCommandEventArgs e)
	{
		bool noRedirect = false;
		bool successAction = true;
			
		if (e.CommandName == "cancel")
			Response.Redirect(Page.Request.Params[BXConfigurationUtility.Constants.BackUrl] ?? BXSefUrlManager.CurrentUrl.ToString());
		else if (Page.IsValid)
		{

			if (e.CommandName == "save")
			{
				if (!SaveSettings())
				{
					successAction = false;
					noRedirect = true;
				}
			}
			else if (e.CommandName == "apply")
			{
				if (!SaveSettings())
					successAction = false;
				noRedirect = true;
			}
		}
		if (!noRedirect)
		{
			if (Page.Request.Params[BXConfigurationUtility.Constants.BackUrl] != null)
			{
				Page.Response.Redirect(Page.Request.QueryString[BXConfigurationUtility.Constants.BackUrl]);
			}
			else
			{
				if (successAction)
				{
					successMessage.Visible = (e.CommandName != "cancel");
					LoadData();
				}
			}
		}
		else
		{
			if (successAction)
			{
				successMessage.Visible = (e.CommandName != "cancel");
				LoadData();
			}
		}
	}

	private void LoadData()
	{
		cbUserVisualEditor.Checked = "Y".Equals(BXOptionManager.GetOptionString("iblock", "use_visual_editor", "N"), StringComparison.InvariantCultureIgnoreCase);
		//cbCombinedListMode.Checked = "Y".Equals(BXOptionManager.GetOptionString("iblock", "combined_list_mode", "Y"), StringComparison.InvariantCultureIgnoreCase);
		//cbCombinedListMode.Enabled = false;
		tbMenuMaxDepth.Text = BXOptionManager.GetOptionInt("iblock", "iblock_menu_max_depth", 0).ToString();
	}

	private bool SaveSettings()
	{
		try
		{
			if (!canManageSettings)
				throw new UnauthorizedAccessException(GetMessageRaw("Exception.YouDontHaveRightsToPerformThisAction"));

			int val;
			int.TryParse(tbMenuMaxDepth.Text, out val);
			BXOptionManager.SetOptionInt("iblock", "iblock_menu_max_depth", val);
			//BXOptionManager.SetOptionString("main", "combined_list_mode", (cbCombinedListMode.Checked ? "Y" : "N"));
			BXOptionManager.SetOptionString("iblock", "combined_list_mode", "Y");
			BXOptionManager.SetOptionString("iblock", "use_visual_editor", (cbUserVisualEditor.Checked ? "Y" : "N"));

			return true;
		}
		catch (Exception e)
		{
			errorMessage.AddErrorMessage(Encode(e.Message));
		}
		return false;
	}
}
