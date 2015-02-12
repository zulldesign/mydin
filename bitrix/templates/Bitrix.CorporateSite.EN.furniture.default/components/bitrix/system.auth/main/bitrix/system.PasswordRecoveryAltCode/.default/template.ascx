<%@ Reference VirtualPath="~/bitrix/components/bitrix/system.PasswordRecoveryAltCode/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.Main.Components.SystemPasswordRecoveryAltCodeTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<script runat="server">
	bool success;
	CaptchaValidator _captchaValidator = null;
	protected override void OnInit(EventArgs e)
    {
		base.OnInit(e);
		Page.Title = GetMessageRaw("PageTitle");
		
		if (Component.FeatureEnabled)
			CheckWordField.Text = Component.Checkword;
		
		ErrorMessage.ValidationGroup = ClientID;
		LoginValidator.ValidationGroup = ClientID;	
		CheckWordValidator.ValidationGroup = ClientID;
		PasswordValidator.ValidationGroup = ClientID;
		PasswordConfValidator.ValidationGroup = ClientID;
		ChangeButton.ValidationGroup = ClientID;

		if (Component.UseCaptcha)
		{
			_captchaValidator = new CaptchaValidator(hfCaptchaGuid, tbxCaptcha, ErrorMessage);
			captchaPlaceholder.Controls.Add(_captchaValidator);
			_captchaValidator.ValidationGroup = ClientID;
			rfvCaptcha.ValidationGroup = ClientID;
		}
		else rfvCaptcha.ValidationGroup = "";
	}
	protected void ChangeButton_Click(object sender, EventArgs e)
	{
		if (!Page.IsValid)
			return;
		
		BXParamsBag<object> parameters = new BXParamsBag<object>();
		parameters.Add("Login", LoginField.Text);
		parameters.Add("Checkword", CheckWordField.Text);
		parameters.Add("Password", PasswordField.Text);
		parameters.Add("PasswordConf", PasswordConfField.Text);
		List<string> errors = new List<string>();
		if (!Component.ProcessCommand("recovery", parameters, errors))
		{
			foreach (var error in errors)
				ErrorMessage.AddErrorMessage(error);
			return;
		}
		
		success = true;
	}

    protected override void PrepareDesignMode()
    {
        MinimalWidth = "510";
        MinimalHeight = "325";
        StartWidth = "510";
        StartHeight = "325";
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
<% if (Component.FeatureEnabled) { %>
<div class="content-form recovery-form" onkeypress="return FireDefaultButton(event, '<%= ChangeButton.ClientID %>')">
	<div class="fields">
		<% if (success) { %>
		<div class="field"><span class="notetext"><%= GetMessageRaw("SuccessMessage") %></span></div>
		<% } %>
		
		<bx:BXValidationSummary id="ErrorMessage" runat="server" CssClass="errortext" ValidationGroup="vgPasswordRecoveryFrom" ForeColor="" />
	
		<div class="field">
			<label class="field-title" for="<%= LoginField.ClientID %>"><%= GetMessageRaw("Login") %><asp:RequiredFieldValidator ID="LoginValidator" runat="server" ControlToValidate="LoginField" ErrorMessage="<%$ LocRaw:Message.LoginRequired %>">*</asp:RequiredFieldValidator></label>
			<div class="form-input"><asp:TextBox ID="LoginField" runat="server" CssClass="input-field"/></div>
		</div>
		
		<div class="field">		
			<label class="field-title" for="<%= CheckWordField.ClientID %>"><%= GetMessageRaw("Checkword") %><asp:RequiredFieldValidator ID="CheckWordValidator" runat="server" ControlToValidate="CheckWordField" ErrorMessage="<%$ LocRaw:Message.CheckwordRequired %>">*</asp:RequiredFieldValidator></label>
			<div class="form-input"><asp:TextBox ID="CheckWordField" autocomplete="off" runat="server" CssClass="input-field"/></div>
		</div>
			
		<div class="field">	
			<label class="field-title" for="<%= PasswordField.ClientID %>"><%= GetMessageRaw("NewPassword") %><asp:RequiredFieldValidator ID="PasswordValidator" runat="server" ControlToValidate="PasswordField" ErrorMessage="<%$ LocRaw:Message.NewPasswordRequired %>">*</asp:RequiredFieldValidator></label>
			<div class="form-input"><asp:TextBox ID="PasswordField" runat="server" autocomplete="off" TextMode="Password" CssClass="input-field" /></div>
		</div>
		
		<div class="field">
			<label class="field-title" for="<%= PasswordConfField.ClientID %>"><%= GetMessageRaw("PasswordConfirmation") %><asp:RequiredFieldValidator ID="PasswordConfValidator" runat="server" ControlToValidate="PasswordConfField" ErrorMessage="<%$ LocRaw:Message.PasswordConfirmationRequired %>">*</asp:RequiredFieldValidator></label>
			<div class="form-input"><asp:TextBox ID="PasswordConfField" autocomplete="off" runat="server" TextMode="Password" CssClass="input-field" /></div>
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
	</div>
	<div class="button">
		<asp:Button ID="ChangeButton" runat="server" OnClick="ChangeButton_Click" CssClass="input-submit" Text="<%$ LocRaw:ButtonText.ChangePassword %>" />
	</div>	
	
	<p><%= string.Format(GetMessageRaw("Legend"), Encode(Component.LoginLink)) %></p>
</div>
<% } else { %>
<div class="content-form forgot-form" >
	<div class="fields">
		<span class="notetext"><%= GetMessageRaw("Disabled") %></span>
	</div>
</div>
<% } %>