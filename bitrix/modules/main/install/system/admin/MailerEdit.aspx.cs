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

public partial class bitrix_admin_MailerEdit : Bitrix.UI.BXAdminPage
{
	protected bool editTemplate;
	protected int templateId = -1;
	BXMailerTemplate currentTemplate;
	string requestAction;
	int requestId;
	StringBuilder errorText = new StringBuilder();
	int showMessage;
	string PredefinedFields;

	bool currentUserCanModify = false;

	private void PrepareResultMessage()
	{
		if (this.showMessage <= 0)
		{
			successMessage.Visible = false;
			return;
		}
		successMessage.Visible = true;
	}
	private void ShowError(string encodedMessage)
	{
		this.showMessage = -1;
		errorMessage.AddErrorMessage(encodedMessage);
	}
	private void ShowOk()
	{
		if (this.showMessage == 0)
			this.showMessage = 1;
	}
	
	protected void LoadData(BXMailerTemplate t)
    {
        if (t == null)
            return;
                
        ActiveCheckBox.Checked = t.Active;

		FromTextBox.Text = t.EmailFrom;
        ToTextBox.Text = t.EmailTo;
        SubjectTextBox.Text = t.Subject;

        RadioText.Checked = RadioHtml.Checked = false;
        if (t.BodyType == BXMailerTemplateBodyType.Html)
            RadioHtml.Checked = true;
        else
            RadioText.Checked = true;

        BccTextBox.Text = t.Bcc;
        MessageTextBox.Text = t.Message;

		foreach (ListItem sel in mailEventTypes.Items)
			if (sel.Value.Equals(t.Name, StringComparison.InvariantCultureIgnoreCase))
			{
				mailEventTypes.SelectedValue = sel.Value;
				break;
			}

		foreach (ListItem item in siteList.Items)
			item.Selected = t.Sites.Contains(item.Value);
    }

	void PrepareForInsertScript()
	{
		if (!ClientScript.IsClientScriptBlockRegistered(GetType(), "InsertScript"))
		{
			StringBuilder sscript = new StringBuilder();
			sscript.AppendLine("current_focus = null");
			sscript.AppendLine("function PutString(str)");
			sscript.AppendLine("{");
			sscript.AppendLine("  if(!current_focus)");
			sscript.AppendLine("    return;");
			sscript.AppendFormat(
				"  if(current_focus.id==\"{0}\" || current_focus.id==\"{1}\" || current_focus.id==\"{2}\" || current_focus.id==\"{3}\" || current_focus.id==\"{4}\")",
				FromTextBox.ClientID,
				ToTextBox.ClientID,
				SubjectTextBox.ClientID,
				BccTextBox.ClientID,
				MessageTextBox.ClientID
			); 
			sscript.AppendLine();
			sscript.AppendLine("    current_focus.value += str;");
			sscript.AppendLine("}");
			
			ClientScript.RegisterClientScriptBlock(GetType(), "InsertScript", sscript.ToString(), true);
		}
		FromTextBox.Attributes["onfocus"] = "current_focus = this";
		ToTextBox.Attributes["onfocus"] = "current_focus = this";
		SubjectTextBox.Attributes["onfocus"] = "current_focus = this";
		BccTextBox.Attributes["onfocus"] = "current_focus = this";
		MessageTextBox.Attributes["onfocus"] = "current_focus = this";
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		PredefinedFields = string.Format(
			@"#DEFAULT_EMAIL_FROM# - {0}
            #SITE_NAME# - {1}
            #SERVER_NAME# - {2}
			#APPLICATION_PATH# - {3}",
			GetMessageRaw("DefaultEmail"),
			GetMessageRaw("SiteName"),
			GetMessageRaw("ServerUrl"),
			GetMessageRaw("ApplicationPath")
		);

		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
			BXAuthentication.AuthenticationRequired();

		currentUserCanModify = this.BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);

		int parsedId;
		requestId = -1;
        if (Int32.TryParse(Page.Request["id"], out parsedId))
			requestId = parsedId;

		requestAction = Page.Request["action"];
		if (requestAction != null)
			requestAction = requestAction.ToLowerInvariant();
		if (requestAction != "edit" && requestAction != "add") 
			requestAction = "add";

		if (requestAction == "add")
			foreach (BXMailEventType eventType in BXMailEventTypeManager.EventTypes)
				mailEventTypes.Items.Add(new ListItem(String.Format("[{1}] == {0}", eventType.DisplayName, eventType.Id), eventType.Id));
		PrepareForInsertScript();

		BXSiteCollection siteColl = BXSite.GetList(null, null);
		foreach (BXSite site in siteColl)
			siteList.Items.Add(new ListItem("[" + HttpUtility.HtmlEncode(site.Id) + "] " + HttpUtility.HtmlEncode(site.Name), site.Id));

		if (!IsPostBack && requestId >= 0)
		{
			if (requestAction == "edit")
			{
				templateId = requestId;
				IdHiddenField.Value = templateId.ToString();
			}
			LoadData(BXMailerTemplate.GetById(requestId));
		}
	}
    protected void Page_Load(object sender, EventArgs e)
    {
		int parsedId = 0;
		editTemplate = Int32.TryParse(IdHiddenField.Value, out parsedId);
		if (editTemplate)
			templateId = parsedId;

		mailEventTypes.Visible = !editTemplate;
		typeIdLabel.Visible = editTemplate;
    }
	protected void Page_LoadComplete(object sender, EventArgs e)
    {
        MasterTitle = GetMessage("MasterTitle");

		SetTabTitle();

		int parsedId = 0;
		editTemplate = Int32.TryParse(IdHiddenField.Value, out parsedId);
		if (editTemplate)
		{
			templateId = parsedId;
			editTemplate = parsedId >= 0;
		}

		mailEventTypes.Visible = !editTemplate;
		typeIdLabel.Visible = editTemplate;
		
		string description = "";
		if (!editTemplate)
		{
			if (mailEventTypes.SelectedValue != null)
			{
				BXMailEventType et = BXMailEventTypeManager.FindEventTypeById(mailEventTypes.SelectedValue);
				if (et != null)
					description = et.DisplayDescription;
			}
		}
		else
		{
			if (currentTemplate == null)
				currentTemplate = BXMailerTemplate.GetById(templateId);
			if (currentTemplate == null)
				Response.Redirect("Mailer.aspx");
			lastUpdated.Text = HttpUtility.HtmlEncode(currentTemplate.LastUpdated.ToString());
			BXMailEventType et = BXMailEventTypeManager.FindEventTypeById(currentTemplate.Name);
			typeIdLabel.Text = "[" + currentTemplate.Name + "]";
			if (et != null)
			{
				typeIdLabel.Text += " == " + et.DisplayName;
				description = et.DisplayDescription;
			}
		}

		description = PredefinedFields + Environment.NewLine + description;
		availableFields.Text = (Regex.Replace(Encode(description), "(#(.*?)#)", "<a title=\"" + GetMessage("Kernel.Insert") + "\" href=\"javascript:PutString('$1')\">$1</a>")).Replace(Environment.NewLine, "<br/>");

		AddSeparator.Visible = editTemplate && currentUserCanModify;
		AddButton.Visible = editTemplate && currentUserCanModify;
		CopySeparator.Visible = editTemplate && currentUserCanModify;
		CopyButton.Visible = editTemplate && currentUserCanModify;
		DeleteSeparator.Visible = editTemplate && currentUserCanModify;
		DeleteButton.Visible = editTemplate && currentUserCanModify;

		BXTabControl1.ShowSaveButton = currentUserCanModify;
		BXTabControl1.ShowApplyButton = currentUserCanModify;
		PrepareResultMessage();
    }

    protected void SetTabTitle()
    {        
        string title = GetMessageRaw("TabTitle.Template");
        if (!String.IsNullOrEmpty(IdHiddenField.Value))
            title += " #" + IdHiddenField.Value;
        edittab.Title = title;        
    }

	private void Save()
	{
		int id;
		bool newTemplate;

		newTemplate = !int.TryParse(IdHiddenField.Value, out id);

		currentTemplate = newTemplate ? new BXMailerTemplate(mailEventTypes.SelectedValue) : BXMailerTemplate.GetById(id);
		if (currentTemplate == null)
			return;

		currentTemplate.Active = ActiveCheckBox.Checked;
		if (!editTemplate)
			currentTemplate.Name = mailEventTypes.SelectedValue;
		currentTemplate.EmailFrom = FromTextBox.Text;
		currentTemplate.EmailTo = ToTextBox.Text;
		currentTemplate.Subject = SubjectTextBox.Text;
		currentTemplate.BodyType = RadioHtml.Checked ? BXMailerTemplateBodyType.Html : BXMailerTemplateBodyType.Text;
		currentTemplate.Bcc = BccTextBox.Text;
		currentTemplate.Message = MessageTextBox.Text;
		currentTemplate.Sites.Clear();
		foreach (ListItem item in siteList.Items)
			if (item.Selected)
				currentTemplate.Sites.Add(item.Value);
		currentTemplate.Save();
		IdHiddenField.Value = currentTemplate.Id.ToString();
	}
	protected void SaveButton_Click(object sender, EventArgs e)
    {
		if (currentUserCanModify)
			Save();
		else
			ShowError(Encode(BXLoc.GetModuleMessage("main", "Auth.UnauthorizedAccessException")));
    }

    protected void ApplyButton_Click(object sender, EventArgs e)
    {
		if (currentUserCanModify)
		{
			Save();
			SetTabTitle();
		}
		else
			ShowError(Encode(BXLoc.GetModuleMessage("main", "Auth.UnauthorizedAccessException")));
    }

    protected bool InputMainValidator()
    {
        bool answer = true;

        if (String.IsNullOrEmpty(FromTextBox.Text))
        {
            answer = false;
			fromStar.Visible = true;
			ShowError(GetMessageFormat("Message.FieldIsEmpty", GetMessageRaw("FromLabel")));
        }

        if (String.IsNullOrEmpty(ToTextBox.Text))
        {
			answer = false;
			toStar.Visible = true;
			ShowError(GetMessageFormat("Message.FieldIsEmpty", GetMessageRaw("ToLabel")));
        }
		
		if (siteList.SelectedItem == null)
		{
			answer = false;
			siteStar.Visible = true;
			ShowError(GetMessageFormat("Message.FieldIsEmpty", GetMessageRaw("SiteLabel")));
		}

        if (!editTemplate && String.IsNullOrEmpty(mailEventTypes.SelectedValue))
        {
			answer = false;
			nameStar.Visible = true;
			ShowError(GetMessageFormat("Message.FieldIsEmpty", GetMessageRaw("MailEventType")));
        }
		return answer;
    }

    protected void BXTabControl1_Command(object sender, BXTabControlCommandEventArgs e)
    {
        bool redirect = true;
        if (e.CommandName == "save")
        {
            if (InputMainValidator())
                SaveButton_Click(sender, e);
            else
                redirect = false;
        }
        else if (e.CommandName == "apply")
        {
            if (InputMainValidator())
                ApplyButton_Click(sender, e);
            redirect = false;
        }
        if (redirect)
            Page.Response.Redirect("Mailer.aspx");
		ShowOk();
    }
	protected void BXContextMenuToolbar1_CommandClick(object sender, CommandEventArgs e)
	{
		switch (e.CommandName)
		{
			case "delete":
				if (currentUserCanModify)
				{
					if (templateId >= 0)
						BXMailerTemplate.Delete(templateId);
					Response.Redirect("Mailer.aspx");
				}
				else
				{
					ShowError(Encode(BXLoc.GetModuleMessage("main", "Auth.UnauthorizedAccessException")));
				}
				break;
			case "copy":
				Response.Redirect("MailerEdit.aspx?action=new&id=" + templateId);
				break;
		}
	}
}
