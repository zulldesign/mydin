<%@ Reference Control="~/bitrix/components/bitrix/system.login/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" CodeFile="template.ascx.cs" Inherits="bitrix_components_bitrix_system_login_templates__default_template" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<script runat="server">
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
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		if (Component.Errors.Count > 0)
			foreach (var err in Component.Errors)
				ErrorMessage.AddErrorMessage(err);
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
</script>
<div class="content-rounded-box">
	<div class="auth-box">
		<b class="r1"></b><b class="r0"></b>
		<div class="inner-box">
				<%
				if (Component.SendConfirmationRequest && !String.IsNullOrEmpty(Component.UserEmail) && Component.Errors.Count == 0 )
				{  %>

      <%=String.Format(GetMessage("ConfirmationRequestWasSent"), Component.UserEmail)%>
			<% }
				else
				{ %>		
			<div class="content-form login-form" onkeypress="return FireDefaultButton(event, '<%= LoginButton.ClientID %>');">
				<div class="legend"><%= GetMessageRaw("Title") %></div>	
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
					
					<% if (Component.UseCaptcha) { %>
					<div class="field">
						<label class="field-title" for="<%= tbxCaptcha.ClientID %>"><%= GetMessage("CaptchaPrompt.EnterTheCodeDisplayedOnPicture")%><asp:RequiredFieldValidator ID="rfvCaptcha" runat="server" ControlToValidate="tbxCaptcha" CssClass="starrequired"
							ErrorMessage="<%$ Loc:MessageText.CaptchaCodeMustBeSpecified %>" ToolTip="<%$ Loc:MessageToolTip.CaptchaCodeMustBeSpecified %>"
							ValidationGroup="vgLoginForm">*</asp:RequiredFieldValidator></label>
						<asp:HiddenField ID="hfCaptchaGuid" runat="server" />
						<div class = "form-input">
						<asp:TextBox ID="tbxCaptcha" runat="server" ></asp:TextBox>
						</div>
			        
						<br />
			        <asp:Image ID="imgCaptcha" runat="server"  AlternateText="CAPTCHA" />
			        <asp:PlaceHolder ID="captchaPlaceholder" runat="server"></asp:PlaceHolder>		
					</div>
					<%} %>
					
					<div class="field field-option">
						<asp:CheckBox ID="CheckBoxRemember" runat="server" Text="<%$ LocRaw:CheckBoxText.RememberMe %>" />
					</div>
					

					
					<div class="field field-button">
						<asp:Button ID="LoginButton" runat="server" Text="<%$ LocRaw:ButtonText.Login %>" OnClick="LoginButton_Click" CssClass="input-submit" />
					</div>
					
					<div class="field">
						<a href="<%= Encode(Parameters["PasswordRecoveryPath"]) %>"><%= GetMessageRaw("ForgotPassword") %></a>
					</div>
					
					<% if (Component.UseOpenIdAuth)
					{ %>
					<div class="field field-openid" onkeypress = "return  <%=ClientID %>FireDefaultButton(event,'<%=OpenIdLoginButton.ClientID %>');">
						<label class="field-title" for="<%= OpenIdLoginField.ClientID %>">OpenID<asp:RequiredFieldValidator ID="rfOpenIdLogin" Display="Dynamic" runat="server" ControlToValidate="OpenIdLoginField"
						ErrorMessage="<%$ Loc:MessageText.OpenIdLoginMustBeSpecified %>" ToolTip="<%$ Loc:MessageToolTip.OpenIdLoginMustBeSpecified %>"
						ValidationGroup="vgOpenIdLoginForm">*</asp:RequiredFieldValidator></label>
						<div class="form-input">
							<asp:TextBox ID="OpenIdLoginField" Width="40%" runat="server"></asp:TextBox>
							<button ID="OpenIdLoginButton"  runat="server" type="button" validationgroup="vgOpenIdLoginForm" OnServerClick="OpenIdLoginButton_Click"><%= GetMessageRaw("ButtonText.Login") %></button>
						</div>	
					</div>
					<%} %>
					<% if (Component.UseLiveIdAuth)
					{ %>
					<div class="field field-liveid">
						<label class="field-title">Windows Live ID</label>
						<div class="form-input">
							<button ID="LiveIdLoginButton" runat="server" type="button" OnServerClick="LiveIdLoginButton_Click"><%= GetMessageRaw("ButtonText.Login") %></button>
						</div>
					</div>
					<%} %>
				</div>
			</div>
			<%} %>
		</div>
		<b class="r0"></b><b class="r1"></b>
	</div>
</div>

<script type="text/javascript">
	function <%=ClientID %>FireDefaultButton(event,target)
	{
	if (event.keyCode == 13) 
		{
        var src = event.srcElement || event.target;
        if (!src || (src.tagName.toLowerCase() != "textarea")) 
        {
            var defaultButton = document.getElementById(target);
            if (defaultButton && typeof(defaultButton.click) != "undefined") 
            {
                defaultButton.click();
                event.cancelBubble = true;
                
                if (event.stopPropagation) 
                    event.stopPropagation();
                    
                return false;
            }
        }
    }
    return true;
	}
</script>
