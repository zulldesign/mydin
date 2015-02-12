<%@ Control Language="C#" AutoEventWireup="true" CodeFile="template.ascx.cs" Inherits="bitrix_components_bitrix_system_PasswordRecovery_templates__default_template" %>

<%@ Reference Control="~/bitrix/components/bitrix/system.PasswordRecovery/component.ascx" %>

<bx:BXValidationSummary id="errorMessage" runat="server" CssClass="errorSummary" ValidationGroup="vgPasswordRecoveryFrom" HeaderText="<%$ Loc:Kernel.Error %>" BorderWidth="1px" BorderStyle="Solid" BorderColor="Red" />

<bx:BXMessage id="successMessage" title="<%$ Loc:Kernel.Information %>" runat="server" CssClass="ok" Visible="False" IconClass="ok" Content="<%$ Loc:Message.NewPasswordHasBeenSentByEmail %>" />

<asp:panel ID="divPasswordRecoveryStep1" defaultbutton="Button3" runat="server">
	<div class="auth-form">
		<div class="header"><%= GetMessage("Header.PasswordRecovery") %></div>
		<div class="picture"></div>
		<div class="table">
			<table cellpadding="0" cellspacing="0" border="0">
				<tr>
					<td class="label"><%= GetMessage("Legend.Login") %></td>
					<td><asp:TextBox ID="tbLogin" ValidationGroup="vgPasswordRecoveryFrom1" runat="server"></asp:TextBox>
						<asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ControlToValidate="tbLogin" Display="Dynamic" ValidationGroup="vgPasswordRecoveryFrom1" ErrorMessage="*"></asp:RequiredFieldValidator>
					</td>
				</tr>
				<tr> 
					<td></td>
					<td><asp:Button ID="Button3" runat="server" Text="<%$ Loc:Kernel.Next %>" ValidationGroup="vgPasswordRecoveryFrom1" OnClick="Button1_Click" /></td>
				</tr>
			</table>
		</div>
		<br clear="all"/>
		<div class="footer">
			<p><%= GetMessage("IfYouForgetYourPasswordEnterYourLogin") %></p>
			<p><%= string.Format(GetMessageRaw("Format.Return2AuthorizationForm"), string.Format("<a href=\"{0}\">", Encode(Component.LoginLink)), "</a>")%></p>
		</div>
	</div>
</asp:panel>

<asp:panel ID="divPasswordRecoveryStep2" defaultbutton="Button4" runat="server">
	<div class="auth-form">
		<div class="header"><%= GetMessage("Header.PasswordRecovery") %></div>

		<div class="picture"></div>
		<div class="table">
			<table cellpadding="0" cellspacing="0" border="0">
				<tr>
					<td class="label"><%= GetMessage("Legend.Login") %></td>
					<td>
						&nbsp;<asp:Label ID="lbLogin" runat="server" Text="Label"></asp:Label>
					</td>
				</tr>
				<tr>
					<td class="label"><%= GetMessage("Legend.Question") %></td>
					<td>
						&nbsp;<asp:Label ID="lbQuestion" runat="server" Text="Label"></asp:Label>
					</td>
				</tr>
				<tr>
					<td class="label"><%= GetMessage("Legend.Answer") %></td>
					<td><asp:TextBox ID="tbAnswer" ValidationGroup="vgPasswordRecoveryFrom" runat="server"></asp:TextBox>
						<asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="tbAnswer" Display="Dynamic" ValidationGroup="vgPasswordRecoveryFrom" ErrorMessage="*"></asp:RequiredFieldValidator>
					</td>
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
					<td><asp:Button ID="Button4" runat="server" ValidationGroup="vgPasswordRecoveryFrom" Text="<%$ Loc:ButtonText.ChangePassword %>" OnClick="Button3_Click"/></td>
				</tr>
			</table>
		</div>
		<br clear="all"/>

		<div class="footer">
			<p><%= GetMessage("EnterAswerForSecretQuestion") %></p>
            <p><%= string.Format(GetMessageRaw("Format.Return2AuthorizationForm"), string.Format("<a href=\"{0}\">", Encode(Component.LoginLink)), "</a>")%></p>
		</div>
	</div>
</asp:panel>

<asp:panel ID="divPasswordRecoveryError" runat="server">
	<%= GetMessage("PasswordRecoveryIsUnavailableByMeansOfSecretQuestion") %>
</asp:panel>

