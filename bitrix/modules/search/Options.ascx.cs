using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Bitrix;
using Bitrix.Modules;
using Bitrix.Services;
using Bitrix.UI;
using Bitrix.Security;
using Bitrix.Search;
using Bitrix.Configuration;
using Bitrix.Services.Text;

public partial class bitrix_modules_Search_Options : BXControl
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
			LoadData();
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
		MaxFileSize.Text = BXSearchModule.Options.MaxFileSizeKB.ToString();
		IncludeMask.Text = BXSearchModule.Options.IncludeMask;
		ExcludeMask.Text = BXSearchModule.Options.ExcludeMask;
		FolderIncludeMask.Text = BXSearchModule.Options.FolderIncludeMask;
		FolderExcludeMask.Text = BXSearchModule.Options.FolderExcludeMask;
		UseStemming.Checked = BXSearchModule.Options.UseStemming;
		StemmingWordChars.Text = BXSearchModule.Options.StemmingWordChars;
		MaxSearchResults.Text = BXSearchModule.Options.MaxSearchResults.ToString();
		UseSimplifiedRanking.Checked = BXSearchModule.Options.UseSimplifiedRanking;
	}

	private bool SaveSettings()
	{
		try
		{
			if (!canManageSettings)
				throw new UnauthorizedAccessException(GetMessageRaw("Auth.UnauthorizedAccessException"));

			int i;
			BXSearchModule.Options.MaxFileSizeKB = (int.TryParse(MaxFileSize.Text, out i) && i > 0) ? i : 0;
			BXSearchModule.Options.IncludeMask = IncludeMask.Text;
			BXSearchModule.Options.ExcludeMask = ExcludeMask.Text;
			BXSearchModule.Options.FolderIncludeMask = FolderIncludeMask.Text;
			BXSearchModule.Options.FolderExcludeMask = FolderExcludeMask.Text;
			BXSearchModule.Options.UseStemming = UseStemming.Checked;
			BXSearchModule.Options.StemmingWordChars = StemmingWordChars.Text;
			BXSearchModule.Options.MaxSearchResults = int.TryParse(MaxSearchResults.Text, out i) ? i : 0;
			BXSearchModule.Options.UseSimplifiedRanking = UseSimplifiedRanking.Checked;

			foreach (BXSite site in BXSite.GetList(null, null, null, null, BXTextEncoder.EmptyTextEncoder))
			{
				string key = ClientID + "_PageTagsKeywords_" + site.Id;
				string value = Request.Form[key];
				if (value != null) 
					BXSearchModule.Options.PageTagsKeyword[site.Id] = value;
			}

			return true;
		}
		catch (Exception e)
		{
			errorMessage.AddErrorMessage(Encode(e.Message));
		}
		return false;
	}
}
