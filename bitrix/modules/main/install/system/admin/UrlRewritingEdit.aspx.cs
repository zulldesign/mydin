using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Bitrix.UI;
using Bitrix.Services;
using Bitrix;

using System.Security;
using Bitrix.Security;
using Bitrix.Configuration;
using System.Text.RegularExpressions;

public partial class bitrix_admin_UrlRewritingEdit : BXAdminPage
{
	int showMessage;
	string componentType;
	string containerPage;
	bool? isEditMode;
	int? id;
	bool currentUserCanModify;

	void PrepareResultMessage()
	{
		successMessage.Visible = showMessage > 0;
	}
	void ShowError(string encodedMessage)
	{
		showMessage = -1;
		errorMessage.AddErrorMessage(encodedMessage);
	}
	void ShowError(Exception ex)
	{
		showMessage = -1;
		errorMessage.AddErrorMessage(Encode(ex.Message));
	}
	void ShowOk()
	{
		if (showMessage == 0)
			showMessage = 1;
	}
	

	protected override string BackUrl
	{
		get
		{
			return base.BackUrl ?? "UrlRewriting.aspx";
		}
	}
	bool IsEditMode
	{
		get
		{
			if (isEditMode == null)
			{
				if (!IsPostBack)
					isEditMode = (Request.QueryString["edit"] != null);
				else
					isEditMode = !string.IsNullOrEmpty(Request.Form["@EditMode"]);
			}
			return isEditMode.Value;
		}
		set
		{
			isEditMode = value;
		}
	}
	int Id
	{
		get
		{
			int i;
			if (id == null)
			{
				id = -1;
				if (!IsPostBack)
				{
					if (IsEditMode && int.TryParse(Request.QueryString["id"], out i) && i >= 0)
						id = i;
				}
				else
				{
					if (IsEditMode && int.TryParse(Request.Form["@Id"], out i) && i >= 0)
						id = i;
				}
			}
			return id.Value;
		}
		set
		{
			id = value;
		}
	}
	protected string ComponentType
	{
		get
		{
			if (componentType == null)
				componentType = Request.Form["@ComponentType"] ?? string.Empty;
			return componentType;
		}
		set
		{
			componentType = value;
		}
	}
	protected string ContainerPage
	{
		get
		{
			if (containerPage == null)
				containerPage = Request.Form["@ContainerPage"] ?? string.Empty;
			return containerPage;
		}
		set
		{
			containerPage = value;
		}
	}

	void DoSave()
	{
		if (!Page.IsValid)
		{
			showMessage = -1;
			return;
		}
		try
		{
			if (!currentUserCanModify)
				throw new SecurityException(GetMessageRaw("Exception.YouDontHaveRightsToPerformThisOperation"));
			
			BXSefUrlRule rule;
			if (IsEditMode)
				rule = BXSefUrlRuleManager.Get(Id);
			else
				rule = new BXSefUrlRule();

			rule.SiteId = Site.SelectedValue;
			rule.MatchExpression = MatchTemplate.Text;
			rule.ReplaceExpression = ReplaceTemplate.Text;
			int i;
			if (int.TryParse(Sort.Text, out i))
				rule.Sort = i;
			rule.HelperId = HelperId.Text;
			rule.Ignore = Ignore.Checked;

			BXSefUrlRuleManager.BeginUpdate();
			if (!IsEditMode)
				BXSefUrlRuleManager.Add(rule);
			BXSefUrlRuleManager.EndUpdate();

			Id = rule.Id;
			IsEditMode = true;
			ShowOk();
		}
		catch(Exception ex)
		{
			ShowError(ex);
		}
	}
	void DoLoad()
	{
		BXSefUrlRule rule;
		if (Id >= 0 && (rule = BXSefUrlRuleManager.Get(Id)) != null)
		{
			Site.SelectedValue = rule.SiteId;
			MatchTemplate.Text = rule.MatchExpression;
			ReplaceTemplate.Text = rule.ReplaceExpression;
			Sort.Text = rule.Sort.ToString();
			HelperId.Text = rule.HelperId;
			Ignore.Checked = rule.Ignore;

			ComponentType = rule.ComponentName;
			ContainerPage = rule.Path;
		}
		else
			IsEditMode = false;
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		if (!BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
			BXAuthentication.AuthenticationRequired();
		currentUserCanModify = BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);
		foreach (BXSite site in BXSite.GetList(null, null))
			Site.Items.Add(new ListItem(site.Name, site.Id));
		
		if (!IsPostBack && IsEditMode)
			DoLoad();

		ReplaceTemplateValidator.Enabled = string.IsNullOrEmpty(ContainerPage);
	}
	protected void Page_LoadComplete(object sender, EventArgs e)
	{
		MasterTitle = IsEditMode ? GetMessage("RuleModification") : GetMessage("RuleCreation");
		PrepareResultMessage();
		if (IsEditMode)
		{
			ScriptManager.RegisterHiddenField(this, "@EditMode", "true");
			ScriptManager.RegisterHiddenField(this, "@Id", Id.ToString());
		}
		if (!string.IsNullOrEmpty(ComponentType))
			ScriptManager.RegisterHiddenField(this, "@ComponentType", ComponentType);
		if (!string.IsNullOrEmpty(ContainerPage))
			ScriptManager.RegisterHiddenField(this, "@ContainerPage", ContainerPage);
		mainTabControl.ShowSaveButton = mainTabControl.ShowApplyButton = currentUserCanModify;
	}
	protected void mainTabControl_Command(object sender, BXTabControlCommandEventArgs e)
	{
		switch (e.CommandName)
		{
			case "save":
				DoSave();
				if (showMessage != -1)
					GoBack();
				break;
			case "apply":
				DoSave();
				break;
			case "cancel":
				GoBack();
				break;
		}
	}

	protected void RegexValidator_ServerValidate(object source, ServerValidateEventArgs args)
	{
		args.IsValid = false;
		try
		{
			new Regex(args.Value);
			args.IsValid = true;
		}
		catch
		{
		}
	}
}
