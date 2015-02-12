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

public partial class bitrix_modules_bitrix_corporatesite_Options : BXControl
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
		TabControl_Bank.ShowApplyButton = TabControl_Bank.ShowSaveButton = canManageSettings;
		TabControl_Bank.ShowCancelButton = Request.QueryString[BXConfigurationUtility.Constants.BackUrl] != null;
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
			var tControl = sender as BXTabControl;
			if (tControl != null)
			{
				SaveData(tControl.Tabs[0]);
			}
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
		List<Control> cList = new List<Control>();
		
		foreach (Control c in Tab_Bank.Controls)
			cList.Add(c);

		foreach (var control in cList)
		{
			var t = control as TextBox;

			if (t != null)
			{

				string key = t.ID;
				string name = string.Empty;
				string type = null;
				int i = key.IndexOf('_');
				if (i != -1)
				{
					name = key.Substring(0,i);
					key = key.Substring(i + 1);
				}
				i = key.IndexOf('_');
				if (i != -1)
				{
					type = key.Substring(i + 1);
					key = key.Remove(i);
				}

				switch (type)
				{
					case "int":
						t.Text = BXOptionManager.GetOptionInt("Bitrix.BankSite", key, 0, siteId).ToString();
						break;
					default:
						t.Text = BXOptionManager.GetOptionString("Bitrix.BankSite", key, "", siteId);
						break;
				}
			}
		}
	}

	private void SaveData(BXTabControlTab Tab)
	{
		if (Tab == null) return;
		foreach (var control in Tab.Controls)
		{
			var t = control as TextBox;
			if (t != null)
			{
				string key = t.ID;
				string type = null;
				string name = string.Empty;
				int i = key.IndexOf('_');
				if (i != -1)
				{
					name = key.Substring(0, i);
					key = key.Substring(i + 1);
				}
				i = key.IndexOf('_');
				if (i != -1)
				{
					type = key.Substring(i + 1);
					key = key.Remove(i);
				}

				switch (type)
				{
					case "int":
						int j;
						BXOptionManager.SetOptionInt("Bitrix.BankSite", key, int.TryParse(t.Text, out j) ? j : 0, siteId);
						break;
					default:
						BXOptionManager.SetOptionString("Bitrix.BankSite", key, t.Text, siteId);
						break;
				}			
			}
		}
	}
}
