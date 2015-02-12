<%@ Reference Control="~/bitrix/components/bitrix/system.login/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.Main.Components.SystemLoginTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<script runat="server">
	CaptchaValidator _captchaValidator = null;
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		
		var identity = Bitrix.Security.BXPrincipal.Current.GetIdentity();
		if (identity.IsAuthenticated)
			Response.Redirect(Bitrix.BXSite.Current.DirectoryAbsolutePath);
		
		ErrorMessage.ValidationGroup = ClientID;
		LoginRequired.ValidationGroup = ClientID;
		PasswordRequired.ValidationGroup = ClientID;
		LoginButton.ValidationGroup = ClientID;

		if (Component.UseCaptcha)
		{
			_captchaValidator = new CaptchaValidator(hfCaptchaGuid, tbxCaptcha, ErrorMessage);
			captchaPlaceholder.Controls.Add(_captchaValidator);
			_captchaValidator.ValidationGroup = ClientID;
			rfvCaptcha.ValidationGroup = ClientID;
		}
		else rfvCaptcha.ValidationGroup = "";
	}
	
	protected void LoginButton_Click(object sender, EventArgs e)
	{
		if (Page.IsValid)
		{
			var parameters = new BXParamsBag<object>();
			parameters.Add("Login", LoginField.Text);
			parameters.Add("Password", PasswordField.Text);
			parameters.Add("Remember", CheckBoxRemember.Checked);
			var errors = new List<string>();
			if (!Component.ProcessCommand("login", parameters, errors))
			{
				foreach (var error in errors)
					ErrorMessage.AddErrorMessage(error);
			}
		}
	}

	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);
		if (Component.UseCaptcha)
		{
			BXCaptchaEngine captchaEng = _captchaValidator.CaptchaEngine;
			tbxCaptcha.MaxLength = captchaEng.Captcha.Length;
			tbxCaptcha.Attributes.Add("length", (2 * captchaEng.Captcha.Length).ToString());
			imgCaptcha.ImageUrl = _captchaValidator.GetCaptchaImageUrl();
		}

	}
</script>
<div class="content-form login-form" onkeypress="return FireDefaultButton(event, '<%= LoginButton.ClientID %>');">
	<div class="fields">
	
		<bx:BXValidationSummary runat="server" ID="ErrorMessage" CssClass="errortext" ForeColor="" />
	
		<div class="field">
			<label class="field-title" for="<%= LoginField.ClientID %>"><%= GetMessageRaw("Login") %><asp:RequiredFieldValidator ID="LoginRequired" runat="server" ControlToValidate="LoginField" ErrorMessage="<%$ LocRaw:Message.LoginRequired %>" >*</asp:RequiredFieldValidator></label>
			<div class="form-input"><asp:TextBox ID="LoginField" runat="server" CssClass="input-field" /></div>
		</div>

		<div class="field">
			<label class="field-title" for="<%= PasswordField.ClientID %>"><%= GetMessageRaw("Password") %><asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="PasswordField" ErrorMessage="<%$ LocRaw:Message.PasswordRequired %>" >*</asp:RequiredFieldValidator></label>
			<div class="form-input"><asp:TextBox ID="PasswordField" runat="server" TextMode="Password" CssClass="input-field" /></div>
		</div>
		
		<div class="field field-option">
			<asp:CheckBox ID="CheckBoxRemember" runat="server" Text="<%$ LocRaw:CheckBoxText.RememberMe %>" />
		</div>
		<% if (Component.UseCaptcha)
		{ %>
		<div class="field">
			<label class="field-title" for="<%= tbxCaptcha.ClientID %>"><%= GetMessage("CaptchaPrompt.EnterTheCodeDisplayedOnPicture")%><asp:RequiredFieldValidator ID="rfvCaptcha" runat="server" ControlToValidate="tbxCaptcha" CssClass="starrequired"
				ErrorMessage="<%$ Loc:MessageText.CaptchaCodeMustBeSpecified %>" ToolTip="<%$ Loc:MessageToolTip.CaptchaCodeMustBeSpecified %>"
				ValidationGroup="vgLoginForm">*</asp:RequiredFieldValidator></label>
			<asp:HiddenField ID="hfCaptchaGuid" runat="server" />
			<div class = "form-input">
				<asp:TextBox ID="tbxCaptcha" runat="server" ></asp:TextBox>
			</div>
	    
			<br />
		<asp:Image ID="imgCaptcha" runat="server" AlternateText="CAPTCHA" />
		<asp:PlaceHolder ID="captchaPlaceholder" runat="server"></asp:PlaceHolder>		
		</div>
		<%} %>
		
		<div class="field field-button">
			<asp:Button ID="LoginButton" runat="server" Text="<%$ LocRaw:ButtonText.Login %>" OnClick="LoginButton_Click" CssClass="input-submit" />
		</div>
		
		<div class="field">
			<a href="<%= Encode(Parameters["PasswordRecoveryPath"]) %>"><%= GetMessageRaw("ForgotPassword") %></a>
		</div>
	</div>
</div>
