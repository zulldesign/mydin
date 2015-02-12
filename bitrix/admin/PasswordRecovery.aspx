<%--<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="PasswordRecovery.aspx.cs" Inherits="bitrix_admin_PasswordRecovery" Title="Восстановление пароля" Theme="AdminTheme" StylesheetTheme="AdminTheme" %>
zg--%>

<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="PasswordRecovery.aspx.cs" Inherits="bitrix_admin_PasswordRecovery" Title="<%$ Loc:PageTitle.PasswordRecovery %>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<bx:BXMessage ID="successMessage" runat="server" EnableViewState="False" IconClass="ok" CssClass="ok" Title="<%$ Loc:Kernel.Information %>" Content="<%$ Loc:YourNewPasswordHasBeenSentToYourEmail %>" Visible="False" />
	<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" Width="438px" />
	<div id="divPasswordRecoveryStep1" runat="server">
		<div class="auth-form">
			<div class="header"><%= GetMessage("PageTitle.PasswordRecovery") %></div>
			<div class="picture"></div>
			<div class="table">
				<table cellpadding="0" cellspacing="0" border="0">
					<tr>
						<td class="label"><%= GetMessage("Legend.Login") %></td>
						<td><asp:TextBox ID="tbLogin" runat="server"></asp:TextBox>
							<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="tbLogin" Display="Dynamic" ErrorMessage="<%$ Loc:Message.EnterLogin %>">*</asp:RequiredFieldValidator>
						</td>
					</tr>
					<tr> 
						<td></td>
						<td><asp:Button ID="Button1" runat="server" Text="<%$ Loc:Kernel.Next %>" OnClick="Button1_Click" /></td>
					</tr>
				</table>
			</div>
			<br clear="all"/>
			<div class="footer">
				<p><%= GetMessage("IfYouForgetPasswordEnterYouLogin") %></p>
				<p><%= string.Format(GetMessage("Return2LoginForm"), "<a href=\"Login.aspx\">", "</a>") %></p>
			</div>
		</div>
	</div>
		<asp:PlaceHolder runat="server" ID="captchaPlaceHolder">
	</asp:PlaceHolder>
	<div id="divPasswordRecoveryStep2" visible="false" runat="server">
		<div class="auth-form">
			<div class="header"><%= GetMessage("PageTitle.PasswordRecovery") %></div>

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
						<td><asp:TextBox ID="tbAnswer" runat="server"></asp:TextBox>
							<asp:RequiredFieldValidator ID="RequiredFieldValidator2" ValidationGroup="vgLoginForm" runat="server" ControlToValidate="tbAnswer" Display="Dynamic" ErrorMessage="<%$ Loc:Message.EnterAnswer %>">*</asp:RequiredFieldValidator>
						</td>
					</tr>
					<tr>
						<td class="label">
								<%= GetMessage("CaptchaPrompt.EnterTheCodeDisplayedOnPicture")%>				    
						</td>
						<td>
							 <asp:HiddenField ID="hfCaptchaGuid" runat="server" />
								<asp:TextBox ID="tbxCaptcha" runat="server" ></asp:TextBox>
								<asp:RequiredFieldValidator ID="rfvCaptcha" runat="server" ControlToValidate="tbxCaptcha"
									ErrorMessage="<%$ Loc:MessageText.CaptchaCodeMustBeSpecified %>" ToolTip="<%$ Loc:MessageToolTip.CaptchaCodeMustBeSpecified %>"
									ValidationGroup="vgLoginForm">*</asp:RequiredFieldValidator>		
								<br />

						        	
						</td>
						
					</tr>
					<tr><td></td><td><asp:Image ID="imgCaptcha" runat="server" AlternateText="CAPTCHA" />	</td></tr>
					<tr> 
						<td></td>
						<td><asp:Button ID="Button3" ValidationGroup="vgLoginForm" runat="server" Text="<%$ Loc:ButtonText.ChangePassword %>" OnClick="Button3_Click"/></td>
					</tr>
				</table>
			</div>
			<br clear="all"/>

			<div class="footer">
				<p><%= GetMessage("EnterAnswerToYourSecretQuestion") %></p>
				<p><%= string.Format(GetMessage("Return2LoginForm"), "<a href=\"Login.aspx\">", "</a>") %></p>
			</div>
		</div>
	</div>

</asp:Content>