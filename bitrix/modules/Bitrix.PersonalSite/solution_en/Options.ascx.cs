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

public partial class bitrix_modules_main_Options : BXControl
{
	string siteId;
	bool canManageSettings;

	private void CheckSite()
	{
		if (siteId != null)
			return;

		siteId = Request.QueryString["site"];		
	}


	protected void Page_Init(object sender, EventArgs e)
	{
		canManageSettings = BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);
		TabControl.ShowApplyButton = TabControl.ShowSaveButton = canManageSettings;
		TabControl.ShowCancelButton = Request.QueryString[BXConfigurationUtility.Constants.BackUrl] != null;
	}
	protected void Page_PreRender(object sender, EventArgs e)
	{
		if (Page.IsPostBack)
			return;

		CheckSite();
		LoadData();
	}
	protected void TabControl_Command(object sender, BXTabControlCommandEventArgs e)
	{
		CheckSite();
		
		if (e.CommandName == "cancel")
		{
			Response.Redirect(Request.QueryString[BXConfigurationUtility.Constants.BackUrl] ?? BXSefUrlManager.CurrentUrl.ToString());
			return;
		}

		if (!Page.IsValid)
			return;

		try
		{
			if (!canManageSettings)
				throw new UnauthorizedAccessException(GetMessageRaw("Exception.YouDontHaveRightsToPerformThisAction"));
			SaveData();
		}
		catch (Exception ex)
		{
			errorMessage.AddErrorMessage(Encode(ex.Message));
			return;
		}

		if (e.CommandName == "apply")
		{
			var query = HttpUtility.ParseQueryString(BXSefUrlManager.CurrentUrl.Query);
			query["status"] = "ok";

			var uri = new UriBuilder(BXSefUrlManager.CurrentUrl);
			uri.Query = query.ToString();

			Response.Redirect(uri.Uri.AbsoluteUri);
			return;
		}

		Response.Redirect(Request.QueryString[BXConfigurationUtility.Constants.BackUrl] ?? BXSefUrlManager.CurrentUrl.ToString());
	}
	
	private void LoadData()
	{
		foreach (var control in Tab.Controls)
		{
			var t = control as TextBox;
			if (t != null)
			{
				string key = t.ID;
				string type = null;
				int i = key.IndexOf('_');
				if (i != -1)
				{
					type = key.Substring(i + 1);
					key = key.Remove(i);
				}

				switch (type)
				{
					case "int":
						t.Text = BXOptionManager.GetOptionInt("Bitrix.PersonalSite", key, 0, siteId).ToString();
						break;
					default:
						t.Text = BXOptionManager.GetOptionString("Bitrix.PersonalSite", t.ID, "", siteId);
						break;
				}
				
			}
		}
	}

	private void SaveData()
	{
		foreach (var control in Tab.Controls)
		{
			var t = control as TextBox;
			if (t != null)
			{
				string key = t.ID;
				string type = null;
				int i = key.IndexOf('_');
				if (i != -1)
				{
					type = key.Substring(i + 1);
					key = key.Remove(i);
				}

				switch (type)
				{
					case "int":
						int j;
						BXOptionManager.SetOptionInt("Bitrix.PersonalSite", key, int.TryParse(t.Text, out j) ? j : 0, siteId);
						break;
					default:
						BXOptionManager.SetOptionString("Bitrix.PersonalSite", t.ID, t.Text, siteId);
						break;
				}			
			}
		}
	}
}
