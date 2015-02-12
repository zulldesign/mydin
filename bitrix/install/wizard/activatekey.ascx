<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.Wizards.BXWizardStepStandardHtmlControl" %>
<%@ Import Namespace="Bitrix.UI.Wizards" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="SiteUpdater" %>
<%@ Import Namespace="System.Collections.Generic" %>

<script runat="server">
	protected override void OnWizardInit()
	{
		Bitrix.DataTypes.BXParamsBag<object> bag = new Bitrix.DataTypes.BXParamsBag<object>();
		WizardContext.State["ActivateKey"] = bag;
		bag["CreateUser"] = true;
	}

	protected override BXWizardResult OnActionShow(Bitrix.DataTypes.BXCommonBag parameters)
	{
		BXSiteUpdater updater = BXSiteUpdater.GetUpdater();
		updater.CheckInitialize();

		BXUpdaterServerManifest manifest = new BXUpdaterServerManifest(updater);
		manifest.Load("client", updater.Config.Language, updater.GetDownloadedModulesVersions(), updater.GetDownloadedLanguagesVersions(), updater.GetDownloadedUpdaterVersion());

		UI.Load("ActivateKey");

		BXWizardResultView view = Result.Render(GetMessage("Title"));
		view.Buttons.Add("prev", null);
		if (WizardContext.State.ContainsKey("Options.ConnectionString"))
			view.Buttons.Add("finish", GetMessage("Finish"));
		else
			view.Buttons.Add("next", null);
		return view;
	}

	protected override BXWizardResult OnActionFinish(Bitrix.DataTypes.BXCommonBag parameters)
	{
		return OnActionNext(parameters);
	}
	protected override BXWizardResult OnActionNext(Bitrix.DataTypes.BXCommonBag parameters)
	{
		UI.LoadValues(parameters);

		BXSiteUpdater updater = BXSiteUpdater.GetUpdater();
		updater.CheckInitialize();

		BXUpdaterServerManifest manifest = new BXUpdaterServerManifest(updater);
		manifest.Load("client", updater.Config.Language, updater.GetDownloadedModulesVersions(), updater.GetDownloadedLanguagesVersions(), updater.GetDownloadedUpdaterVersion());

		if (!manifest.Client.reserved)
			return Result.Action("activatelicense", "", null);


		System.Collections.Generic.List<string> errors = new System.Collections.Generic.List<string>();
		if (!Validate(errors))
		{
			BXWizardResultView view = Result.Render(GetMessage("Title"), errors);
			view.Buttons.Add("prev", null);
			if (WizardContext.State.ContainsKey("Options.ConnectionString"))
				view.Buttons.Add("finish", GetMessage("Finish"));
			else
				view.Buttons.Add("next", null);
			return view;
		}

		try
		{
			updater.ActivateSystem(
				UI.Data.GetString("OwnerName").Trim(),
				UI.Data.GetString("OwnerPhone").Trim(),
				UI.Data.GetString("OwnerEmail").Trim(),
				UI.Data.GetString("SiteAddress".Trim()),
				UI.Data.GetString("ContactPerson").Trim(),
				UI.Data.GetString("ContactPhone").Trim(),
				UI.Data.GetString("ContactEmail").Trim(),
				UI.Data.GetString("ContactInfo") ?? "",
				UI.Data.GetBool("CreateUser"),
				UI.Data.GetString("UserFirstName").Trim(),
				UI.Data.GetString("UserLastName").Trim(),
				UI.Data.GetString("UserLogin").Trim(),
				UI.Data.GetString("UserPassword")
			);
		}
		catch (Exception ex)
		{
			BXWizardResultView view = Result.Render(GetMessage("Title"), new string[] { ex.Message });
			view.Buttons.Add("prev", null);
			if (WizardContext.State.ContainsKey("Options.ConnectionString"))
				view.Buttons.Add("finish", GetMessage("Finish"));
			else
				view.Buttons.Add("next", null);
			return view;
		}

		UI.Overwrite("ActivateKey");

		return Result.Action("activatelicense", "", null);
	}

	protected override BXWizardResult OnActionPrevious(Bitrix.DataTypes.BXCommonBag parameters)
	{
		return Result.Action("licensekey", "", null);
	}

	private bool Validate(System.Collections.Generic.List<string> errors)
	{
		if (BXStringUtility.IsNullOrTrimEmpty(UI.Data.GetString("OwnerName")))
			errors.Add(GetMessage("Error.OwnerNameRequired"));

		if (BXStringUtility.IsNullOrTrimEmpty(UI.Data.GetString("SiteAddress")))
			errors.Add(GetMessage("Error.SiteAddressRequired"));

		if (BXStringUtility.IsNullOrTrimEmpty(UI.Data.GetString("OwnerPhone")))
			errors.Add(GetMessage("Error.OwnerPhone.Required"));

		if (BXStringUtility.IsNullOrTrimEmpty(UI.Data.GetString("OwnerEmail")))
			errors.Add(GetMessage("Error.EmailRequired"));
		else if (!Regex.IsMatch(UI.Data.GetString("OwnerEmail"), @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"))
			errors.Add(GetMessage("Error.OwnerEmailInvalid"));

		if (BXStringUtility.IsNullOrTrimEmpty(UI.Data.GetString("ContactPerson")))
			errors.Add(GetMessage("Error.ContactPersonRequired"));

		if (BXStringUtility.IsNullOrTrimEmpty(UI.Data.GetString("ContactEmail")))
			errors.Add(GetMessage("Error.EmailRequired"));
		else if (!Regex.IsMatch(UI.Data.GetString("ContactEmail"), @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"))
			errors.Add(GetMessage("Error.ContactEmailInvalid"));

		if (BXStringUtility.IsNullOrTrimEmpty(UI.Data.GetString("ContactPhone")))
			errors.Add(GetMessage("Error.ContactPhoneRequired"));


		if (UI.Data.GetBool("CreateUser"))
		{
			if (BXStringUtility.IsNullOrTrimEmpty(UI.Data.GetString("UserFirstName")))
				errors.Add(GetMessage("Error.UserFirstNameRequired"));

			if (BXStringUtility.IsNullOrTrimEmpty(UI.Data.GetString("UserLastName")))
				errors.Add(GetMessage("Error.UserLastNameRequired"));

			if (BXStringUtility.IsNullOrTrimEmpty(UI.Data.GetString("UserLogin")))
				errors.Add(GetMessage("Error.UserLoginRequired"));
			else if (UI.Data.GetString("UserLogin").Trim().Length < 3)
				errors.Add(GetMessage("Error.UserLoginTooShort"));

			if (string.IsNullOrEmpty(UI.Data.GetString("UserPassword")))
				errors.Add(GetMessage("Error.UserPasswordRequired"));
			else if (UI.Data.GetString("UserPassword") != UI.Data.GetString("UserPasswordConfirmation"))
				errors.Add(GetMessage("Error.UserPasswordMismatch"));
		}


		return errors.Count == 0;
	}
</script>

<script type="text/javascript">
	function ShowTableRow(row, visible) 
	{
		row.style.display = visible ? "" : "none";
	}

	function RefreshGui() 
	{
		var obj;
		obj = document.getElementById("create_user");
		var createUser = obj && obj.checked;

		for (var i = 0; i < 5; i++) 
		{
			obj = document.getElementById("createuser_row_" + i);
			if (obj)
				obj.style.display = createUser ? "" : "none";
		}
	}
</script>

<table border="0" class="data-table">
	<tr>
		<td colspan="2" class="header">
			<%= GetMessage("Header.RegistrationData") %>
		</td>
	</tr>
	<tr>
		<td align="right" width="40%" valign="top">
			<span style="color: red">*</span><%= GetMessage("Label.OwnerName") %>:
		</td>
		<td width="60%" valign="top">
			<% UI.InputText("OwnerName", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("style", "width:100%") }); %>
			<br />
			<small><%= GetMessage("Hint.OwnerName") %></small>
		</td>
	</tr>
	<tr>
		<td align="right" width="40%" valign="top">
			<span style="color: red">*</span><%= GetMessage("Label.SiteAddress") %>:
		</td>
		<td valign="top">
			<% UI.InputText("SiteAddress", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("style", "width:100%") }); %>
			<br />
			<small><%= GetMessage("Hint.SiteAddress") %></small>
		</td>
	</tr>
	<tr>
		<td align="right" width="40%" valign="top">
			<span style="color: red">*</span><%= GetMessage("Label.OwnerPhone") %>:
		</td>
		<td valign="top">
			<% UI.InputText("OwnerPhone", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("style", "width:100%") }); %>
			<br />
			<small><%= GetMessage("Hint.OwnerPhone") %></small>
		</td>
	</tr>
	<tr>
		<td align="right" width="40%" valign="top">
			<span style="color: red">*</span><%= GetMessage("Label.OwnerEmail") %>:
		</td>
		<td valign="top">
			<% UI.InputText("OwnerEmail", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("style", "width:100%") }); %>
			<br />
			<small><%= GetMessage("Hint.OwnerEmail") %></small>
		</td>
	</tr>
	<tr>
		<td align="right" width="40%" valign="top">
			<span style="color: red">*</span><%= GetMessage("Label.ContactPerson") %>:
		</td>
		<td valign="top">
			<% UI.InputText("ContactPerson", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("style", "width:100%") }); %>
			<br />
			<small><%= GetMessage("Hint.ContactPerson") %></small>
		</td>
	</tr>
	<tr>
		<td align="right" width="40%" valign="top">
			<span style="color: red">*</span><%= GetMessage("Label.ContactEmail") %>:
		</td>
		<td valign="top">
			<% UI.InputText("ContactEmail", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("style", "width:100%") }); %>
			<br />
			<small><%= GetMessage("Hint.ContactEmail") %></small>
		</td>
	</tr>
	<tr>
		<td align="right" width="40%" valign="top">
			<span style="color: red">*</span><%= GetMessage("Label.ContactPhone") %>:
		</td>
		<td valign="top">
			<% UI.InputText("ContactPhone", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("style", "width:100%") }); %>
		</td>
	</tr>
	<tr>
		<td align="right" width="40%" valign="top">
			<%= GetMessage("Label.ContactInfo") %>:
		</td>
		<td valign="top">
			<% UI.Textarea("ContactInfo", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("style", "width:100%") }); %>
			<br />
			<small><%= GetMessage("Hint.ContactInfo") %></small>
		</td>
	</tr>
</table>
<br />
<%= GetMessage("UserAccount") %>
<br />
<br />
<table border="0" class="data-table">
	<tr>
		<td colspan="2" class="header">
			<%= GetMessage("Header.UserAccount") %>
		</td>
	</tr>
	<tr>
		<td colspan="2" align="center">
			<% 
				UI.CheckBox(
					"CreateUser",
					GetMessage("CheckBox.CreateAccount"),
					new KeyValuePair<string, string>[] 
					{ 
						new KeyValuePair<string, string>("id", "create_user"),
						new KeyValuePair<string, string>("onclick", "RefreshGui();")
					}
				); 
			%>
		</td>
	</tr>
	<tr id="createuser_row_0">
		<td align="right" width="40%" valign="top">
			<span style="color: red">*</span><%= GetMessage("Label.AccountFistName") %>:
		</td>
		<td width="60%" valign="top">
			<% UI.InputText("UserFirstName", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("style", "width:100%") }); %>
		</td>
	</tr>
	<tr id="createuser_row_1">
		<td align="right" width="40%" valign="top">
			<span style="color: red">*</span><%= GetMessage("Label.AccountLastName") %>:
		</td>
		<td width="60%" valign="top">
			<% UI.InputText("UserLastName", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("style", "width:100%") }); %>
		</td>
	</tr>
	<tr id="createuser_row_2">
		<td align="right" width="40%" valign="top">
			<span style="color: red">*</span><%= GetMessage("Label.AccountLogin") %>:
		</td>
		<td width="60%" valign="top">
			<% UI.InputText("UserLogin", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("style", "width:100%") }); %>
			<br />
			<small><%= GetMessage("Hint.AccountLogin") %></small>
		</td>
	</tr>
	<tr id="createuser_row_3">
		<td align="right" width="40%" valign="top">
			<span style="color: red">*</span><%= GetMessage("Label.Password") %>:
		</td>
		<td width="60%" valign="top">
			<% UI.InputText("UserPassword", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("style", "width:100%") }); %>
		</td>
	</tr>
	<tr id="createuser_row_4">
		<td align="right" width="40%" valign="top">
			<span style="color: red">*</span><%= GetMessage("Label.PasswordConfirmation") %>:
		</td>
		<td width="60%" valign="top">
			<% UI.InputText("UserPasswordConfirmation", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("style", "width:100%") }); %>
		</td>
	</tr>
</table>

<script type="text/javascript">	window.setTimeout(function() { RefreshGui(); }, 0);</script>