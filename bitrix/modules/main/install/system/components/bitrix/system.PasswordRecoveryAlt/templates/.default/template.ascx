<%@ Control Language="C#" AutoEventWireup="true" CodeFile="template.ascx.cs" Inherits="bitrix_components_bitrix_system_PasswordRecoveryAlt_templates__default_template" %>

<%@ Reference Control="~/bitrix/components/bitrix/system.PasswordRecoveryAlt/component.ascx" %>

<bx:BXValidationSummary id="errorMessage" runat="server" CssClass="errorSummary" ValidationGroup="vgPasswordRecoveryFrom" HeaderText="<%$ Loc:Kernel.Error %>" BorderWidth="1px" BorderStyle="Solid" BorderColor="Red" />

<bx:BXMessage id="successMessage" title="<%$ Loc:Kernel.Information %>" runat="server" CssClass="ok" Visible="False" IconClass="ok" Content="<%$ Loc:Message.ControlStringAndYourRegistrationDataHasBeenSentByEmail %>" />

<asp:panel ID="divPasswordRecovery" defaultbutton="Button1" runat="server">

	<div class="auth-form">
		<div class="header"><%= GetMessage("Header.SendControlString") %></div>

		<div class="picture"></div>
		<div class="table">
		<table cellpadding="0" cellspacing="0" border="0">
			<tr>
				<td class="label"><%= GetMessage("Legend.Login") %></td>
				<td><asp:TextBox ID="tbLogin" ValidationGroup="vgPasswordRecoveryFrom" runat="server"></asp:TextBox></td>
			</tr>
			<tr> 
				<td></td>
				<td><label><%= GetMessage("Or") %></label></td>
			</tr>
			<tr>
				<td class="label">E-Mail:</td>
				<td><asp:TextBox ID="tbEMail" ValidationGroup="vgPasswordRecoveryFrom" runat="server"></asp:TextBox></td>
			</tr>
			<% if (Component.UseCaptcha ) { %>
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
			<%} %>
			<tr> 
				<td></td>
				<td><asp:Button ID="Button1" ValidationGroup="vgPasswordRecoveryFrom" runat="server" OnClick="Button1_Click" Text="<%$ Loc:ButtonText.Send %>" /></td>
			</tr>
		</table>
		</div>
		<br clear="all"/>

		<div class="footer">
			<p><%= GetMessage("IfYouForgetYourPasswordEnterYourLoginOrEmail") %></p>
			<p><%= string.Format(GetMessage("Format.AfterYouGetControlStringGo2PasswordChangeForm"), string.Format("<a href=\"{0}\">", Encode(Component.PasswordRecoveryCodeLink)), "</a>")%></p>
			<p><%= string.Format(GetMessage("Format.Return2AuthorizationForm"), string.Format("<a href=\"{0}\">", Encode(Component.LoginLink)), "</a>")%></p>
		</div>
	</div>
</asp:panel>

<asp:panel ID="divPasswordRecoveryError" runat="server">
	<%= GetMessage("PasswordRecoveryIsUnavailableByMeansOfControlString") %>
</asp:panel>
