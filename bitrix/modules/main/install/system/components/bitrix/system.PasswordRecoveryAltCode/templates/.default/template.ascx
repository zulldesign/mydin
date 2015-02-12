<%@ Control Language="C#" AutoEventWireup="true" CodeFile="template.ascx.cs" Inherits="bitrix_components_bitrix_system_RasswordRecovery_templates__default_template" %>

<%@ Reference Control="~/bitrix/components/bitrix/system.PasswordRecoveryAltCode/component.ascx" %>

<bx:BXValidationSummary id="errorMessage" runat="server" CssClass="errorSummary" ValidationGroup="vgPasswordRecoveryFrom" HeaderText="<%$ Loc:Kernel.Error %>" BorderWidth="1px" BorderStyle="Solid" BorderColor="Red" />

<bx:BXMessage id="successMessage" title="<%$ Loc:Kernel.Information %>" runat="server" CssClass="ok" Visible="False" IconClass="ok" Content="<%$ Loc:Message.PasswordHasBeenChanged %>" />

<asp:panel ID="divPasswordRecovery" defaultbutton="Button2" runat="server">
	<div class="auth-form">
		<div class="header"><%= GetMessage("Password Change") %></div>

		<div class="picture"></div>
		<div class="table">
		<table cellpadding="0" cellspacing="0" border="0">
			<tr> 
				<td class="label" valign="top"><%= GetMessage("Legend.Login") %></td>
				<td valign="top"><asp:TextBox ID="tbLogin" ValidationGroup="vgPasswordRecoveryFrom" runat="server"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="tbLogin" ValidationGroup="vgPasswordRecoveryFrom"
			ErrorMessage="<%$ Loc:Message.LoginIsRequired %>">*</asp:RequiredFieldValidator></td>
			</tr>
			<tr> 
				<td class="label" valign="top"><%= GetMessage("Legend.ControlString") %></td>
				<td valign="top"><asp:TextBox ID="tbCheckWord" ValidationGroup="vgPasswordRecoveryFrom" runat="server"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="tbCHeckWord" ValidationGroup="vgPasswordRecoveryFrom"
			ErrorMessage="<%$ Loc:Message.ControlStringIsRequired %>">*</asp:RequiredFieldValidator></td>
			</tr>
			<tr> 
				<td class="label" valign="top"><%= GetMessage("Legend.NewPassword") %></td>
				<td valign="top"><asp:TextBox ID="tbPassword" runat="server" ValidationGroup="vgPasswordRecoveryFrom" TextMode="Password"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="tbPassword" ValidationGroup="vgPasswordRecoveryFrom"
			ErrorMessage="<%$ Loc:Message.NewPasswordIsRequired %>">*</asp:RequiredFieldValidator></td>
			</tr>
			<tr> 
				<td class="label" valign="top"><%= GetMessage("Legend.PasswordConfirmation") %></td>
				<td valign="top"><asp:TextBox ID="tbPasswordConf" ValidationGroup="vgPasswordRecoveryFrom" runat="server" TextMode="Password"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="tbPasswordConf" ValidationGroup="vgPasswordRecoveryFrom"
			ErrorMessage="<%$ Loc:Message.PasswordConfirmationIsRequired %>">*</asp:RequiredFieldValidator></td>
			</tr>
			<% if (Component.UseCaptcha) { %>
			<tr>
				<td align="right" valign="top">
					<span style="color: red;">*</span><asp:Label ID="lblCaptcha" runat="server" AssociatedControlID="tbxCaptcha" ><%= GetMessage("CaptchaPrompt.EnterTheCodeDisplayedOnPicture")%></asp:Label>				    
			    </td>
			    <td valign="top">
			        <asp:HiddenField ID="hfCaptchaGuid" runat="server" />
			        <asp:TextBox ID="tbxCaptcha" runat="server" ></asp:TextBox>
					<asp:RequiredFieldValidator ID="rfvCaptcha" runat="server" ControlToValidate="tbxCaptcha"
						ErrorMessage="<%$ Loc:MessageText.CaptchaCodeMustBeSpecified %>" ToolTip="<%$ Loc:MessageToolTip.CaptchaCodeMustBeSpecified %>"
						ValidationGroup="vgPasswordRecoveryFrom">*</asp:RequiredFieldValidator>				        
			        <br />
			        <asp:Image ID="imgCaptcha" runat="server" AlternateText="CAPTCHA" />			        
			    </td>
			</tr>
			<% } %>
			<tr> 
				<td></td>
				<td><asp:Button ID="Button2" runat="server" ValidationGroup="vgPasswordRecoveryFrom" OnClick="Button1_Click" Text="<%$ Loc:ButtonText.ChangePassword %>" /></td>
			</tr>
		</table>
		</div>
		<br clear="all"/>

		<div class="footer">
			<p><%= GetMessage("AllOfFieldsAreRequired") %></p>
			<p><%= string.Format(GetMessage("Format.Return2AuthorizationForm"), string.Format("<a href=\"{0}\">", Encode(Component.LoginLink)), "</a>") %></p>
		</div>
	</div>
</asp:panel>

<asp:panel ID="divPasswordRecoveryError" runat="server">
	<%= GetMessage("PasswordRecoveryIsUnavailableByMeansOfControlString") %>
</asp:panel>
