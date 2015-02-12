<%@ Reference VirtualPath="~/bitrix/components/bitrix/system.PasswordRecoveryAlt/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.Main.Components.SystemPasswordRecoveryAltTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<script runat="server">
	bool success;
	CaptchaValidator _captchaValidator = null;
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		
		ErrorMessage.ValidationGroup = ClientID;
		LoginValidator.ValidationGroup = ClientID;
		SendButton.ValidationGroup = ClientID;
		if (Component.UseCaptcha)
		{
			_captchaValidator = new CaptchaValidator(hfCaptchaGuid, tbxCaptcha, ErrorMessage);
			captchaPlaceholder.Controls.Add(_captchaValidator);
			_captchaValidator.ValidationGroup = ClientID;
			rfvCaptcha.ValidationGroup = ClientID;
		}
		else rfvCaptcha.ValidationGroup = "";

	}
	protected void SendButton_Click(object sender, EventArgs e)
	{
		if (!Page.IsValid)
			return;
		
		var parameters = new BXParamsBag<object>();
		if (Regex.IsMatch(LoginField.Text, @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", RegexOptions.IgnoreCase))
		{
			parameters.Add("Email", LoginField.Text);
			parameters.Add("Login", "");
		}
		else
		{
			parameters.Add("Email", "");
			parameters.Add("Login", LoginField.Text);
		}
		List<string> errors = new List<string>();
		if (!Component.ProcessCommand("validate", parameters, errors))
		{
			foreach (var error in errors)
				ErrorMessage.AddErrorMessage(error);
			return;
		}
		
		success = true;
	}

    protected override void PrepareDesignMode()
    {
        MinimalWidth = "525";
        MinimalHeight = "380";
        StartWidth = "525";
        StartHeight = "380";
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
<div class="auth-box">
	<div class="content-rounded-box">
		<b class="r1"></b><b class="r0"></b>
		<div class="inner-box">		
			<div class="content-form forgot-form" onkeypress="return FireDefaultButton(event, '<%= SendButton.ClientID %>')">
			<div class="legend"><%= GetMessageRaw("Title") %></div>
			<div class="fields">
				<% if (success) { %>	
				<div class="field"><span class="notetext"><%= GetMessageRaw("SuccessMessage") %></span></div>
				<% } %>
				
				<bx:BXValidationSummary ID="ErrorMessage" runat="server" CssClass="errortext" ForeColor="" />

				<div class="field">
					<label class="field-title" for="<%= LoginField.ClientID %>"><%= GetMessageRaw("UserNameOrEmail") %><asp:RequiredFieldValidator ID="LoginValidator" runat="server" ControlToValidate="LoginField" ErrorMessage="<%$ LocRaw:Message.UserNameOrEmailRequired %>">*</asp:RequiredFieldValidator></label>
					<div class="form-input"><asp:TextBox ID="LoginField" runat="server" CssClass="input-field" /></div>
				</div>
			</div>
			
			<% if (Component.UseCaptcha) { %>
			<div class="field">
				<label class="field-title" for="<%= tbxCaptcha.ClientID %>"><%= GetMessage("CaptchaPrompt.EnterTheCodeDisplayedOnPicture")%><asp:RequiredFieldValidator ID="rfvCaptcha" runat="server" ControlToValidate="tbxCaptcha" CssClass="starrequired"
					ErrorMessage="<%$ Loc:MessageText.CaptchaCodeMustBeSpecified %>" ToolTip="<%$ Loc:MessageToolTip.CaptchaCodeMustBeSpecified %>"
					ValidationGroup="vgLoginForm">*</asp:RequiredFieldValidator></label>
				<asp:HiddenField ID="hfCaptchaGuid" runat="server" />
				<div class = "form-input">
				<asp:TextBox ID="tbxCaptcha" runat="server"></asp:TextBox>
				</div>
	        
				<br />
				<asp:Image ID="imgCaptcha" runat="server" AlternateText="CAPTCHA" />
				<asp:PlaceHolder ID="captchaPlaceholder" runat="server"></asp:PlaceHolder>		
			</div>
			<%} %>
			
			<div class="button">
				<asp:Button ID="SendButton" ValidationGroup="vgPasswordRecoveryFrom" runat="server" OnClick="SendButton_Click" Text="<%$ LocRaw:ButtonText.Send %>" CssClass="input-submit" />
			</div>

			<p><%= GetMessageRaw("Instructions") %></p>
			<p><%= string.Format(GetMessageRaw("FollowToChangePasswordForm"), Encode(Component.PasswordRecoveryCodeLink)) %></p>
			<p><%= string.Format(GetMessageRaw("ReturnToAuth"), Encode(Component.LoginLink)) %></p>
			</div>
		</div>
		<b class="r0"></b><b class="r1"></b>
	</div>
</div>							
<% } else { %>
<div class="content-form forgot-form" >
	<div class="fields">
		<span class="notetext"><%= GetMessageRaw("Disabled") %></span>
	</div>
</div>
<% } %>