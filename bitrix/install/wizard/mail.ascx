<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.Wizards.BXWizardStepStandardHtmlControl" %>
<%@ Import Namespace="Bitrix.Configuration" %>
<%@ Import Namespace="Bitrix.UI.Wizards" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="Bitrix.Security" %>

<script runat="server">	
	protected override void OnWizardInit()
	{
		Bitrix.DataTypes.BXParamsBag<object> bag = new Bitrix.DataTypes.BXParamsBag<object>();
		WizardContext.State["Mail"] = bag;
		
		if (WizardContext.State.ContainsKey("Options.SmtpHost"))
			WizardContext.Navigation.Remove(WizardContext.Navigation["mail"]);
	}

	protected override BXWizardResult OnWizardAction(string action, Bitrix.DataTypes.BXCommonBag parameters)
	{
		switch (action)
		{
			case "test":
				return OnActionTest(parameters);
			default:
				return base.OnWizardAction(action, parameters);
		}
	}

	protected BXWizardResult OnActionTest(Bitrix.DataTypes.BXCommonBag parameters)
	{
		UI.LoadValues(parameters);
		
		System.Collections.Generic.List<string> errors = new System.Collections.Generic.List<string>();
		System.Net.Mail.SmtpClient client;	
		if (!Validate(errors) || (client = ValidateClient(errors)) == null)
			return ShowView(errors);
		
		if (BXStringUtility.IsNullOrTrimEmpty(UI.Data.GetString("TestEmail")))
			errors.Add(GetMessage("Error.EmptyTestEmail"));
		else if (!Regex.IsMatch(UI.Data.GetString("From"), @"^(?:[A-Za-z0-9._%+-]+@[A-Za-z0-9-]+(\.[A-Za-z0-9-]+)*|[^<>]*<[A-Za-z0-9._%+-]+@[A-Za-z0-9-]+(\.[A-Za-z0-9-]+)*>)$"))
			errors.Add(GetMessage("Error.InvalidTestEmail"));
		if (errors.Count != 0)
			return ShowView(errors);
		
		try
		{
			System.Net.Mail.MailMessage email = new System.Net.Mail.MailMessage();
			email.From = new System.Net.Mail.MailAddress(UI.Data.GetString("From", ""));
			email.To.Add(UI.Data.GetString("TestEmail"));
		
			email.Subject = GetMessage("Email.Subject");
			email.IsBodyHtml = false;
			email.Body = GetMessage("Email.Body");

			client.Send(email);
			UI.Data["MailSuccess"] = true;
		}
		catch
		{
			errors.Add(GetMessage("Error.TestEmail"));
			return ShowView(errors);	
		}
		
		return ShowView(null);
	}

	protected override BXWizardResult OnActionShow(Bitrix.DataTypes.BXCommonBag parameters)
	{
		if (!WizardContext.State.ContainsKey("Install.MailStepFirstTime"))
		{
			WizardContext.State["Install.MailStepFirstTime"] = "";
			
			string host;
			if (WizardContext.State.TryGetString("Options.SmtpHost", out host))
			{
				BXOptionManager.SetOptionString("main", "MailerSmtpHost", host);
				BXOptionManager.SetOptionInt("main", "MailerSmtpPort", WizardContext.State.GetInt("Options.SmtpPort", 25));
				BXOptionManager.SetOptionString("main", "MailerSmtpUsername", WizardContext.State.GetString("Options.SmtpUsername", ""));
				BXOptionManager.SetOptionString("main", "MailerSmtpPassword", WizardContext.State.GetString("Options.SmtpPassword", ""));
				BXOptionManager.SetOptionString("main", "MailerDefaultEmailFrom", WizardContext.State.GetString("Options.SmtpEmailFrom", ""));
				BXOptionManager.SetOption("main", "MailerUseSsl", WizardContext.State.GetBool("Options.SmtpUseSsl", false));
			}
			
			Bitrix.DataTypes.BXParamsBag<object> bag = WizardContext.State.Get<Bitrix.DataTypes.BXParamsBag<object>>("Mail");
			bag["Host"] = BXOptionManager.GetOptionString("main", "MailerSmtpHost", "");
			bag["Port"] = BXOptionManager.GetOptionInt("main", "MailerSmtpPort", 25).ToString();
			bag["Username"] = BXOptionManager.GetOptionString("main", "MailerSmtpUsername", "");
			bag["Password"] = BXOptionManager.GetOptionString("main", "MailerSmtpPassword", "");
			bag["From"] = BXOptionManager.GetOptionString("main", "MailerDefaultEmailFrom", "");
			bag["UseSsl"] = BXOptionManager.GetOption("main", "MailerUseSsl", false);
		}
		if (WizardContext.State.ContainsKey("Options.SmtpHost"))
			return Result.Next();
		
		
		WizardContext.Navigation.Selected = "mail";

		UI.Load("Mail");
		if (string.IsNullOrEmpty(UI.Data.GetString("From")))
		{
			BXUser user = BXUser.GetById(1, BXTextEncoder.EmptyTextEncoder);
			if (user != null)
			{
				string name = user.GetDisplayName();
				if (!string.IsNullOrEmpty(name) && name.IndexOfAny(new char[] {'<', '>'}) < 0)
					UI.Data["From"] = name + " <" + user.Email + ">";
				else
					UI.Data["From"] = user.Email;
			}
		}
		
		return ShowView(null);
	}

	private BXWizardResultView ShowView(System.Collections.Generic.IEnumerable<string> errors)
	{
		BXWizardResultView view = Result.Render(GetMessage("Title"), errors);
		view.Buttons.Add("test", GetMessage("Button.Test"));
		view.Buttons.Add("next", null);
		return view;
	}

	protected override BXWizardResult OnActionNext(Bitrix.DataTypes.BXCommonBag parameters)
	{
		UI.LoadValues(parameters);
		UI.Data.Remove("TestEmail");

		System.Collections.Generic.List<string> errors = new System.Collections.Generic.List<string>();
		if (BXStringUtility.IsNullOrTrimEmpty(UI.Data.GetString("Host")))
			return Result.Next();
		
		
		if (!Validate(errors) || ValidateClient(errors) == null)
			return ShowView(errors);

		UI.Overwrite("Mail");

		BXOptionManager.SetOptionString("main", "MailerSmtpHost", UI.Data.GetString("Host", "").Trim());
		BXOptionManager.SetOptionInt("main", "MailerSmtpPort", UI.Data.GetInt("Port", 25));
		BXOptionManager.SetOptionString("main", "MailerSmtpUsername", UI.Data.GetString("Username", "").Trim());
		BXOptionManager.SetOptionString("main", "MailerSmtpPassword", UI.Data.GetString("Password", ""));
		BXOptionManager.SetOptionString("main", "MailerDefaultEmailFrom", UI.Data.GetString("From", ""));
		BXOptionManager.SetOption("main", "MailerUseSsl", UI.Data.GetBool("UseSsl"));
		Bitrix.Services.BXMailer.ReloadSettings();

		return Result.Next();
	}
	private bool Validate(System.Collections.Generic.List<string> errors)
	{
		if (BXStringUtility.IsNullOrTrimEmpty(UI.Data.GetString("From")))
			errors.Add(GetMessage("Error.EmptyEmail"));
		else if (!Regex.IsMatch(UI.Data.GetString("From"), @"^(?:[A-Za-z0-9._%+-]+@[A-Za-z0-9-]+(\.[A-Za-z0-9-]+)*|[^<>]*<[A-Za-z0-9._%+-]+@[A-Za-z0-9-]+(\.[A-Za-z0-9-]+)*>)$"))
			errors.Add(GetMessage("Error.InvalidEmail"));

		return errors.Count == 0;
	}

	private System.Net.Mail.SmtpClient ValidateClient(System.Collections.Generic.List<string> errors)
	{
		try
		{
			System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient();
			client.Host = UI.Data.GetString("Host").Trim();
			try
			{
				client.Port = UI.Data.GetInt("Port", 25);
			}
			catch (System.Net.Mail.SmtpException)
			{
				errors.Add(GetMessage("Error.IncorrectPort"));
				return null;
			}
			client.EnableSsl = UI.Data.GetBool("UseSsl");

			if (!BXStringUtility.IsNullOrTrimEmpty(UI.Data.GetString("Username")))
			{
				System.Net.NetworkCredential credential = new System.Net.NetworkCredential();
				credential.UserName = UI.Data.GetString("Username").Trim();

				if (!String.IsNullOrEmpty(UI.Data.GetString("Password")))
					credential.Password = UI.Data.GetString("Password");

				client.Credentials = credential;
			}
			return client;
		}
		catch
		{
			errors.Add(GetMessage("Error.UnableToSetupClient"));
			return null;
		}
	}
	
	
</script>
<% if (UI.Data.GetBool("MailSuccess")) { %>
<div style="color:Green; margin-bottom:10px;">
	<%= GetMessage("MailSuccess") %>
</div>
<% } %>
<% 
	System.Collections.Generic.KeyValuePair<string, string>[] input = new System.Collections.Generic.KeyValuePair<string, string>[]
	{
		new System.Collections.Generic.KeyValuePair<string, string>("size", "35")
	};
%>

<table class="data-table">
	<tr><td width="0%"><%= GetMessage("MailNote") %></td></tr>
</table>

<br/>

<table border="0" class="data-table">
	<tr>
		<td class="header" colspan="2">
			<%= GetMessage("Header.Settings") %>
		</td>
	</tr>
	<tr>
		<td nowrap="" align="right">
			<%= GetMessage("Label.Host") %>:
		</td>
		<td>
			<% UI.InputText("Host", input); %>
		</td>
	</tr>
	<tr>
		<td nowrap="" align="right">
			<%= GetMessage("Label.Port") %>:
		</td>
		<td>
			<% UI.InputText("Port", input); %>
		</td>
	</tr>
	<tr>
		<td nowrap="" align="right">
			<%= GetMessage("Label.Username") %>:
		</td>
		<td>
			<% UI.InputText("Username", input); %>
		</td>
	</tr>
	<tr>
		<td nowrap="" align="right"><%= GetMessage("Label.Password") %>:</td>
		<td>
			<% UI.InputPassword("Password", input); %>
		</td>
	</tr>
	<tr>
		<td nowrap="" align="right">&nbsp;</td>
		<td>
			<% UI.CheckBox("UseSsl", GetMessage("Label.UseSsl"), null); %>
		</td>
	</tr>
	<tr>
		<td nowrap="" align="right">
			<%= GetMessage("Label.From") %>:
		</td>
		<td>
			<% UI.InputText("From", input); %><br />
			<small><%= GetMessage("Comment.From") %></small>
		</td>
	</tr>
	<tr>
		<td class="header" colspan="2">
			<%= GetMessage("Header.Test") %>
		</td>
	</tr>
	<tr>
		<td nowrap="" align="right"><%= GetMessage("Label.TestEmail") %>:</td>
		<td>
			<% UI.InputText("TestEmail", input); %>
		</td>
	</tr>
</table>
