<%@ Control Language="C#" AutoEventWireup="true" CodeFile="template.ascx.cs" Inherits="bitrix_components_bitrix_system_login_templates_web20_template" %>

<%@ Reference Control="~/bitrix/components/bitrix/system.login/component.ascx" %>

<div id="formTypeLogin" runat="server">
	<div id="login-form-window">
	<asp:Panel runat="server" ID="DefaultButtonWrapper" DefaultButton="LoginButton" >
		<a href="" onclick="return CloseLoginForm()" style="float:right;"><%= GetMessage("Close") %></a>

		<bx:BXValidationSummary ID="errorMessage" runat="server" BorderColor="Red"
			BorderStyle="Solid" BorderWidth="1px" CssClass="errorSummary" HeaderText="<%$ Loc:AuthorizationError %>"
			ValidationGroup="vgLoginForm11" ShowMessageBox="true" ShowSummary="false" />
	

		<table width="95%">
			<tr>
				<td colspan="2">
					<%= GetMessage("Legend.Login") %><br />
					<asp:TextBox ID="LoginField" runat="server" ValidationGroup="vgLoginForm11"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" 
						ControlToValidate="LoginField" Display="Dynamic" 
						ErrorMessage="<%$ LocRaw:Message.LoginIsRequired  %>" 
						ValidationGroup="vgLoginForm11">*</asp:RequiredFieldValidator>
				</td>
			</tr>
			<tr>
				<td colspan="2">
					<%= GetMessage("Legend.Password") %><br />
					<asp:TextBox ID="PasswordField" runat="server" TextMode="Password" 
						ValidationGroup="vgLoginForm11"></asp:TextBox><asp:RequiredFieldValidator 
						ID="RequiredFieldValidator4" runat="server" ControlToValidate="PasswordField"
						Display="Dynamic" ErrorMessage="<%$ LocRaw:Message.PasswordIsRequired %>"
						ValidationGroup="vgLoginForm11">*</asp:RequiredFieldValidator>
				</td>
			</tr>
			<tr>
				<td valign="top" colspan="2">
					<asp:CheckBox ID="CheckBoxRemember" runat="server" 
						Text="<%$ Loc:CheckBoxText.RememberMe %>" ValidationGroup="vgLoginForm11" />
				</td>
			</tr>
			<% if (Component.UseCaptcha) { %>
			    <tr>
				<td colspan="2" valign="top">
					<span style="color: red;">*</span><asp:Label ID="lblCaptcha" runat="server" AssociatedControlID="tbxCaptcha" ><%= GetMessage("CaptchaPrompt.EnterTheCodeDisplayedOnPicture")%></asp:Label>				    
			    </td>
			    </tr>
			    <tr>
			    <td valign="top" colspan="2">
			        <asp:HiddenField ID="hfCaptchaGuid" runat="server" />
			        <asp:TextBox ID="tbxCaptcha" runat="server" ></asp:TextBox>
					<asp:RequiredFieldValidator ID="rfvCaptcha" runat="server" Display="Dynamic" ControlToValidate="tbxCaptcha"
						ErrorMessage="<%$ LocRaw:MessageText.CaptchaCodeMustBeSpecified %>" ToolTip="<%$ Loc:MessageToolTip.CaptchaCodeMustBeSpecified %>"
						ValidationGroup="vgLoginForm11">*</asp:RequiredFieldValidator>				        
			        <br />
			        <asp:Image ID="imgCaptcha" runat="server" AlternateText="CAPTCHA" />			        
			    </td>
			    </tr>
			<% } %>
			<tr>
				<td colspan="2">
					<asp:Button ID="LoginButton" runat="server" Text="<%$ Loc:ButtonText.Login %>" 
						ValidationGroup="vgLoginForm11" OnClick="LoginButton_Click" />
				</td>
			</tr>
			<% if (Component.UseOpenIdAuth ){ %>
			<tr onkeypress = "return  <%=ClientID %>FireDefaultButton(event,'<%=OpenIdLoginButton.ClientID %>');"><td colspan="2"><%= GetMessage("Legend.OpenId") %><br/>
				<asp:TextBox ID="OpenIdLoginField" runat="server"></asp:TextBox>
				<asp:RequiredFieldValidator ID="rfOpenIdLogin" Display="Dynamic" runat="server" ControlToValidate="OpenIdLoginField"
						ErrorMessage="<%$ Loc:MessageText.OpenIdLoginMustBeSpecified %>" ToolTip="<%$ Loc:MessageToolTip.OpenIdLoginMustBeSpecified %>"
						ValidationGroup="vgOpenIdLoginForm">*</asp:RequiredFieldValidator>		
			<asp:Button ID="OpenIdLoginButton" runat="server" Text="<%$ Loc:ButtonText.Login %>" ValidationGroup="vgOpenIdLoginForm" OnClick="OpenIdLoginButton_Click" /></td>
			</tr>
			<%} %>
			<% if (Component.UseLiveIdAuth){ %>
				<tr><td colspan="2"><%= GetMessage("ButtonText.LiveID") %>
				<asp:Button ID="LiveIdLoginButton" runat="server" Text="<%$ Loc:ButtonText.Login %>" OnClick="LiveIdLoginButton_Click" /></td></tr>
			<%} %>
			<tr>
				<td colspan="2">
					<asp:HyperLink ID="hlForgotPassword" runat="server"><%= GetMessage("DoYouForgetYourRassword") %></asp:HyperLink>
				</td>
			</tr>
			<tr>
				<td colspan="2">
					<asp:HyperLink ID="hlRegister" runat="server"><%= GetMessage("Registration") %></asp:HyperLink>
					<br />
				</td>
			</tr>
		</table>	
		</asp:Panel>
	</div>
	

	<asp:Image ID="imgLogin" Width="10" Height="11" BorderWidth="0" runat="server" />&nbsp;&nbsp;<a href="" onclick="return ShowLoginForm();"><%= GetMessage("ButtonText.Login") %></a>&nbsp;&nbsp;&nbsp;&nbsp;<asp:Image ID="imgRegister" Width="8" Height="11" BorderWidth="0" runat="server" />&nbsp;&nbsp;<asp:HyperLink ID="hlRegister1" runat="server"><%= GetMessage("Registration") %></asp:HyperLink>

</div>


<div id="formTypeNonLogin" runat="server">

	<%= HttpUtility.HtmlEncode((HttpContext.Current.User.Identity as Bitrix.Security.BXIdentity).User.FirstName) %> <%= HttpUtility.HtmlEncode((HttpContext.Current.User.Identity as Bitrix.Security.BXIdentity).User.LastName) %> [<a href="<%= Component.ProfilePath %>" class="profile-link" title="<%= GetMessage("Profile") %>"><%= HttpUtility.HtmlEncode((HttpContext.Current.User.Identity as Bitrix.Security.BXIdentity).Name) %></a>]
	<asp:ImageButton ID="ibLogout" runat="server" AlternateText="<%$ Loc:Kernel.TopPanel.Logout %>" OnClick="ibLogout_Click" />

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
	
		<%
				if (Component.SendConfirmationRequest && !String.IsNullOrEmpty(Component.UserEmail) && Component.Errors.Count == 0 )
				{  %>

     alert(' <%=String.Format(GetMessage("ConfirmationRequestWasSent"), Component.UserEmail)%> ');
			<% } %>
</script>