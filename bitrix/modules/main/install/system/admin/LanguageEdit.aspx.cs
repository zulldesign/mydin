using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Bitrix.UI;
using Bitrix.Services;
using Bitrix.Modules;
using Bitrix;
using Bitrix.Security;
using Bitrix.Services.Text;

public partial class bitrix_admin_LanguageEdit : Bitrix.UI.BXAdminPage
{
	private bool currentUserCanModify = false;

	#region Page_Load()
	protected void Page_Load(object sender, EventArgs e)
	{
		//LOCALIZATION
		if (!string.IsNullOrEmpty(LanguageId))
			MasterTitle = string.Format(GetMessage("EditPageTitle"), Encode(LanguageId));
		else
			MasterTitle = GetMessage("CreatePageTitle");

		Title = MasterTitle;

		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
			BXAuthentication.AuthenticationRequired();

		currentUserCanModify = this.BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);

		if (!string.IsNullOrEmpty(LanguageId))
		{
			txtID.Visible = false;
			lbID.Visible = true;
			lbID.Text = Encode(LanguageId);

			if (!IsPostBack)
			{
				BindLanguage(LanguageId);
			}
		}
		AddSeparator.Visible =
			AddAction.Visible =
			CopySeparator.Visible = 
			CopyAction.Visible =
			DeleteSeparator.Visible =
			DeleteAction.Visible = !string.IsNullOrEmpty(LanguageId) && currentUserCanModify;

		BXTabControl1.ShowSaveButton = BXTabControl1.ShowApplyButton = currentUserCanModify;

		if (!string.IsNullOrEmpty(SourceLanguageId))
		{

			if (!IsPostBack)
			{
				BindLanguage(SourceLanguageId);
			}
		}
	}
	#endregion

	#region BindLanguage()
	private void BindLanguage(string languageId)
	{
		BXLanguage lang = BXLanguage.GetById(languageId);
		if (lang == null)
			lang = new BXLanguage();
		txtID.Text =  lang.TextEncoder.Decode(lang.Id);
		Active.Checked = lang.Active;
		Default.Checked = lang.Default;
		Default.Enabled = !lang.Default;
		Name.Text = lang.Name;
		Sort.Text = lang.Sort.ToString();
		Culture.SelectedCulture = lang.Culture;
	}
	#endregion

	protected void SetTabTitle()
	{
		BXTabControl1.Tabs[0].Title = GetMessage("TabTitle");
	}

	#region ValidateFields()
	protected bool ValidateFields()
	{
		//SET DEFAULTS
		starId.Visible = false;
		starName.Visible = false;
		starSort.Visible = false;
		//errorMessage.Visible = false;
		bool errors = false;

		//ID
		if (string.IsNullOrEmpty(LanguageId))
		{
			txtID.Text = txtID.Text.Trim().ToLower();

			//EMPTY OR TOO LONG
			if (string.IsNullOrEmpty(txtID.Text.Trim()) || txtID.Text.Trim().Length != 2)
			{
				starId.Visible = true;
				errors = true;
				errorMessage.AddErrorMessage(GetMessage("ErrorID"));
			}
			//DUPLICATE
			else
			{
				BXLanguage lang = BXLanguage.GetById(txtID.Text.Trim());
				if (lang != null)
				{
					starId.Visible = true;
					errors = true;
					errorMessage.AddErrorMessage(GetMessage("ErrorDuplicate"));
				}
			}
		}

		//NAME
		if (string.IsNullOrEmpty(Name.Text.Trim()))
		{
			starName.Visible = true;
			errors = true;
			errorMessage.AddErrorMessage(GetMessage("ErrorName"));
		}

		//SORT
		int sort;
		if (!int.TryParse(Sort.Text, out sort))
		{
			starSort.Visible = true;
			errors = true;
			errorMessage.AddErrorMessage(GetMessage("ErrorSort"));
		}

		return !errors;
	}
	#endregion

	protected void BXTabControl1_Command(object sender, BXTabControlCommandEventArgs e)
	{
		switch (e.CommandName.ToLower())
		{
			case "save":
				if (ValidateFields())
				{
					SaveLanguage(false);
				}
				break;
			case "apply":
				if (ValidateFields())
				{
					SaveLanguage(true);
				}
				break;
			case "cancel":
				Response.Redirect("Language.aspx");
				break;
			default:
				break;
		}
	}

	#region SaveLanguage()
	private void SaveLanguage(bool isApply)
	{
		if (!currentUserCanModify)
		{
			errorMessage.AddErrorMessage(GetMessage("Exception.YouDontHaveRightsToPerformThisOperation"));
			return;
		}

		BXLanguage lang = BXLanguage.GetById(LanguageId, BXTextEncoder.EmptyTextEncoder);
		if (lang == null)
			lang = new BXLanguage(txtID.Text, BXTextEncoder.EmptyTextEncoder);

		lang.Name = Name.Text.Trim();
		lang.Active = Active.Checked;
		if (!lang.Default)
			lang.Default = Default.Checked;
		lang.Culture = Culture.SelectedCulture;
		lang.Sort = int.Parse(Sort.Text);

		lang.Save();

		BXLoc.ResetCultureCache();

		if (isApply)
			Response.Redirect(string.Format("LanguageEdit.aspx?id={0}", UrlEncode(lang.Id)));
		else
			Response.Redirect("Language.aspx");
	}
	#endregion

	#region LanguageId
	public string LanguageId
	{
		get
		{
			if (!string.IsNullOrEmpty(Request.QueryString["id"]))
			{
				return Request.QueryString["id"];
			}
			return string.Empty;
		}
	}
	#endregion

	#region SourceLanguageId
	public string SourceLanguageId
	{
		get
		{
			if (!string.IsNullOrEmpty(Request.QueryString["copy"]))
			{
				return Request.QueryString["copy"];
			}
			return string.Empty;
		}
	}
	#endregion

	protected void BXContextMenuToolbar1_CommandClick(object sender, CommandEventArgs e)
	{
		switch (e.CommandName.ToLower())
		{
			case "getlist":
				Response.Redirect("Language.aspx");
				break;
			case "new":
				Response.Redirect("LanguageEdit.aspx");
				break;
			case "copy":
				Response.Redirect("LanguageEdit.aspx?copy=" + LanguageId);
				break;
			case "delete":
				if (currentUserCanModify)
				{
					BXLanguage lang = BXLanguage.GetById(LanguageId);
					if (!lang.IsNew)
						lang.Delete();
					Response.Redirect("Language.aspx");
				}
				else
				{
					errorMessage.AddErrorMessage(GetMessage("Exception.YouDontHaveRightsToPerformThisOperation"));
				}
				break;
			default:
				break;
		}
	}
}