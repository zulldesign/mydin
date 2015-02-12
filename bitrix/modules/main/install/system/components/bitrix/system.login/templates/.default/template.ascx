<%@ Control Language="C#" AutoEventWireup="true" CodeFile="template.ascx.cs" Inherits="bitrix_components_bitrix_system_login_templates__default_template" %>

<%@ Reference Control="~/bitrix/components/bitrix/system.login/component.ascx" %>

<asp:panel ID="AuthComponentSelector" defaultbutton="LoginButton" runat="server">

	<bx:BXValidationSummary ID="errorMessage" runat="server" BorderColor="Red"
			BorderStyle="Solid" BorderWidth="1px" CssClass="errorSummary" HeaderText="<%$ Loc:AuthorizationError %>" ValidationGroup="vgLoginForm" />

	<div class="auth-form">
			<%
				if (Component.SendConfirmationRequest && !String.IsNullOrEmpty(Component.UserEmail) && Component.Errors.Count == 0 )
				{  %>

      <%=String.Format(GetMessage("ConfirmationRequestWasSent"), Component.UserEmail)%>
			<% }
				else
				{ %>
		<div class="header"><%= GetMessage("PleaseLogin")%></div>

		<div class="picture"></div>
		<div class="table">
		<table cellpadding="0" cellspacing="0" border="0">
			<tr>
				<td class="label"><%= GetMessage("Legend.Login")%></td>
				<td><asp:TextBox ID="LoginField" runat="server" ValidationGroup="vgLoginForm"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="LoginField"
						Display="Dynamic" ErrorMessage="<%$ Loc:Message.LoginIsRequired %>" ValidationGroup="vgLoginForm">*</asp:RequiredFieldValidator></td>
			</tr>
			<tr>
				<td class="label"><%= GetMessage("Legend.Password")%></td>
				<td><asp:TextBox ID="PasswordField" runat="server" TextMode="Password" ValidationGroup="vgLoginForm"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="PasswordField"
						Display="Dynamic" ErrorMessage="<%$ Loc:Message.PasswordIsRequired %>" ValidationGroup="vgLoginForm">*</asp:RequiredFieldValidator></td>
			</tr>
			<tr> 
				<td></td>
				<td><asp:CheckBox ID="CheckBoxRemember" runat="server" Text="<%$ Loc:CheckBoxText.RememberMe %>" ValidationGroup="vgLoginForm" /></td>
			</tr>
			<% if (Component.UseCaptcha)
	  { %>
			    <tr>
				<td align="right" valign="top">
					<span style="color: red;">*</span><asp:Label ID="lblCaptcha" runat="server" AssociatedControlID="tbxCaptcha" ><%= GetMessage("CaptchaPrompt.EnterTheCodeDisplayedOnPicture")%></asp:Label>				    
			    </td>
			    <td valign="top">
			        <asp:HiddenField ID="hfCaptchaGuid" runat="server" />
			        <asp:TextBox ID="tbxCaptcha" runat="server" ></asp:TextBox>
					<asp:RequiredFieldValidator ID="rfvCaptcha" runat="server" ControlToValidate="tbxCaptcha"
						ErrorMessage="<%$ Loc:MessageText.CaptchaCodeMustBeSpecified %>" ToolTip="<%$ Loc:MessageToolTip.CaptchaCodeMustBeSpecified %>"
						ValidationGroup="vgLoginForm">*</asp:RequiredFieldValidator>				        
			        <br />
			        <asp:Image ID="imgCaptcha" runat="server" AlternateText="CAPTCHA" />			        
			    </td>
			    </tr>
			<% } %>
			<tr> 
				<td style="height: 24px"></td>
				<td style="height: 24px"><asp:Button ID="LoginButton" runat="server" Text="<%$ Loc:ButtonText.Login %>" ValidationGroup="vgLoginForm" OnClick="LoginButton_Click" />
				</td>
			</tr>
			<% if (Component.UseOpenIdAuth)
	  { %>
			<tr onkeypress = "return  <%=ClientID %>FireDefaultButton(event,'<%=OpenIdLoginButton.ClientID %>');"><td class="label"><%= GetMessage("Legend.OpenId")%></td>
				<td><asp:TextBox ID="OpenIdLoginField" runat="server"></asp:TextBox>
				<asp:RequiredFieldValidator ID="rfOpenIdLogin" Display="Dynamic" runat="server" ControlToValidate="OpenIdLoginField"
						ErrorMessage="<%$ Loc:MessageText.OpenIdLoginMustBeSpecified %>" ToolTip="<%$ Loc:MessageToolTip.OpenIdLoginMustBeSpecified %>"
						ValidationGroup="vgOpenIdLoginForm">*</asp:RequiredFieldValidator>
				<asp:Button ID="OpenIdLoginButton" runat="server" Text="<%$ Loc:ButtonText.Login %>" ValidationGroup="vgOpenIdLoginForm" OnClick="OpenIdLoginButton_Click" />		
				</td>
			</tr>
			<%} %>
			<% if (Component.UseLiveIdAuth)
	  { %>
				<tr  onkeypress = "return  <%=ClientID %>FireDefaultButton(event,'<%=LiveIdLoginButton.ClientID %>');"><td class="label"><%= GetMessage("ButtonText.LiveID")%></td><td><asp:Button ID="LiveIdLoginButton" runat="server" Text="<%$ Loc:ButtonText.Login %>" CausesValidation="false"  OnClick="LiveIdLoginButton_Click" /></td></tr>
			<%} %>
		</table>
		</div>

		<br clear="all"/>

		<div class="footer" id="divLoginFooterHint" runat="server">
			<%--<p><b><%= GetMessage("DoYouForgetYourRassword") %></b></p>
			<p><asp:Label ID="lbFogotHint" runat="server" Text=""></asp:Label></p>--%>
		</div>
	<% } %>
	</div>
</asp:panel>
<asp:PlaceHolder ID="CaptchaValidatorPlaceHolder" runat="server"></asp:PlaceHolder>
<asp:panel ID="AuthComponentSelectorSuccess" runat="server" Visible="false">
	<div class="auth-form">
		<div class="header">
			<%= GetMessage("YouHaveBeenAuthorizedByName") %>
			<b><asp:Label ID="lCurUserName" runat="server" Text=""></asp:Label></b>
			[<asp:LinkButton ID="LinkButton1" runat="server" OnClick="LinkButton1_Click"><%= GetMessage("Kernel.TopPanel.Logout") %></asp:LinkButton>]
		</div>
	</div>
</asp:panel>

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


