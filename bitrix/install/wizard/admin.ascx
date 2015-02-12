<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.Wizards.BXWizardStepStandardHtmlControl" %>
<%@ Import Namespace="Bitrix.UI.Wizards" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="Bitrix.Security" %>
<script runat="server">	
	protected override void OnWizardInit()
	{
		Bitrix.DataTypes.BXParamsBag<object> bag = new Bitrix.DataTypes.BXParamsBag<object>();
	
		bag["DisplayName"] = GetMessage("DisplayName");	
		bag["Login"] = "admin";
		bag["Email"] = "admin@example.com";
		
		WizardContext.State["Admin"] = bag;
	}
		
	protected override BXWizardResult OnActionShow(Bitrix.DataTypes.BXCommonBag parameters)
	{
		BXUser user;
		if ((user = BXUser.GetById(1, BXTextEncoder.EmptyTextEncoder)) != null)
		{
			WizardContext.State["Install.AdminLogin"] = user.UserName;
			WizardContext.State["Install.UserId"] = user.UserId;
			return Result.Next();
		}
				
		WizardContext.Navigation.Selected = "admin";

		UI.Load("Admin");
		BXWizardResultView view = Result.Render(GetMessage("Title"));
		view.Buttons.Add("next", null);
		return view;
	}

	protected override BXWizardResult OnActionNext(Bitrix.DataTypes.BXCommonBag parameters)
	{
		BXUser user;
		if ((user = BXUser.GetById(1, BXTextEncoder.EmptyTextEncoder)) != null)
		{
			WizardContext.State["Install.AdminLogin"] = user.UserName;
			WizardContext.State["Install.UserId"] = user.UserId;
			return Result.Next();
		}
	
		UI.LoadValues(parameters);
		System.Collections.Generic.List<string> errors = new System.Collections.Generic.List<string>();
		if (!Validate(errors))
		{
			BXWizardResultView view = Result.Render(GetMessage("Title"), errors);
			view.Buttons.Add("next", null);
			return view;
		}
		UI.Overwrite("Admin");
		
		user = new BXUser(BXTextEncoder.EmptyTextEncoder);
		user.UserName = UI.Data.GetString("Login").Trim();
		user.ProviderName = "self";
		user.Password = UI.Data.GetString("Password");
		user.Email = UI.Data.GetString("Email");
		if (!string.IsNullOrEmpty(UI.Data.GetString("DisplayName")))
			user.DisplayName = UI.Data.GetString("DisplayName");
		if (!string.IsNullOrEmpty(UI.Data.GetString("FirstName")))
			user.FirstName = UI.Data.GetString("FirstName");
		if (!string.IsNullOrEmpty(UI.Data.GetString("LastName")))
			user.LastName = UI.Data.GetString("LastName");
		user.IsApproved = true;
		if (!string.IsNullOrEmpty(WizardContext.State.GetString("Install.SiteId")))
			user.SiteId = WizardContext.State.GetString("Install.SiteId");
		user.Save();
		
		
		
		try
		{	
			if (BXRoleManager.GetByName("Admin") != null)
				user.AddToRole("Admin", "", ""); 
		}
		catch
		{
		}
		
		WizardContext.State["Install.AdminLogin"] = user.UserName;
		WizardContext.State["Install.UserId"] = user.UserId;
					
		return Result.Next();
	}

	private bool Validate(System.Collections.Generic.List<string> errors)
	{
		if (BXStringUtility.IsNullOrTrimEmpty(UI.Data.GetString("Login")))
			errors.Add(GetMessage("Error.EmptyLogin"));
		else if (UI.Data.GetString("Login").Trim().Length < 3) 
			errors.Add(GetMessage("Error.LoginTooSmall"));
			
		if (BXStringUtility.IsNullOrTrimEmpty(UI.Data.GetString("Password")))
			errors.Add(GetMessage("Error.EmptyPassword"));
		else 
		{
			if (UI.Data.GetString("Password").Length < 6)
				errors.Add(GetMessage("Error.PasswordTooSmall"));			
				
			if (UI.Data.GetString("Password") != UI.Data.GetString("PasswordConfirmation"))
				errors.Add(GetMessage("Error.PasswordMissmatch"));
		}
		
		if (BXStringUtility.IsNullOrTrimEmpty(UI.Data.GetString("Email")))
			errors.Add(GetMessage("Error.EmptyEmail"));
		else if (!Regex.IsMatch(UI.Data.GetString("Email"), @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$"))
			errors.Add(GetMessage("Error.InvalidEmail"));
		
		return errors.Count == 0;
	}
</script>
<table border="0" class="data-table">
	<tr><td class="header" colspan="2"><%= GetMessage("Header.Admin") %></td></tr>
	<tr>
		<td nowrap="" align="right"><%= GetMessage("Label.DisplayName") %>:</td>
		<td><% UI.InputText("DisplayName", null); %></td>
	</tr>
	<tr>
		<td nowrap="" align="right"><span style="color: red;">*</span><%= GetMessage("Label.Login") %>:</td>
		<td>
			<% UI.InputText("Login", null); %><br />
			<small><%= GetMessage("Hint.Login") %></small>
		</td>
	</tr>
	<tr>
		<td nowrap="" align="right"><span style="color: red;">*</span><%= GetMessage("Label.Password") %>:</td>
		<td>
			<% UI.InputPassword("Password", null); %><br />
			<small><%= GetMessage("Hint.Password") %></small>
		</td>
	</tr>
	<tr>
		<td nowrap="" align="right"><span style="color: red;">*</span><%= GetMessage("Label.PasswordConfirmation") %>:</td>
		<td><% UI.InputPassword("PasswordConfirmation", null); %></td>
	</tr>
	<tr>
		<td nowrap="" align="right"><span style="color: red;">*</span>E-Mail:</td>
		<td><% UI.InputText("Email", null); %></td>
	</tr>
	<tr>
		<td nowrap="" align="right"><%= GetMessage("Label.FirstName") %>:</td>
		<td><% UI.InputText("FirstName", null); %></td>
	</tr>
	<tr>
		<td nowrap="" align="right"><%= GetMessage("Label.LastName") %>:</td>
		<td><% UI.InputText("LastName", null); %></td>
	</tr>
</table>
