<%--<%@ Page Language="C#" MasterPageFile="AdminMasterPage.master" AutoEventWireup="true" CodeFile="PasswordRecoveryAlt.aspx.cs" Inherits="PasswordRecoveryAlt" Title="Запрос пароля" Theme="AdminTheme" StylesheetTheme="AdminTheme" %>
zg --%>

<%@ Page Language="C#" MasterPageFile="AdminMasterPage.master" AutoEventWireup="true" CodeFile="PasswordRecoveryAlt.aspx.cs" Inherits="PasswordRecoveryAlt" Title="<%$ Loc:PageTitle.PasswordRequest %>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<bx:BXMessage ID="loginMessage" runat="server" Content="<%$ Loc:Message.YourRegistrationDataHasBeenSentToYourEmail %>"
		IconClass="ok" CssClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False" />
<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" Width="438px" />
<div class="auth-form">
	<div class="header"><%= GetMessage("SendControlstring") %></div>

	<div class="picture"></div>
	<div class="table">
	<table cellpadding="0" cellspacing="0" border="0">
		<tr>
			<td class="label"><%= GetMessage("Legend.Login") %></td>
			<td><asp:TextBox ID="tbLogin" runat="server"></asp:TextBox></td>
		</tr>
		<tr> 
			<td></td>
			<td><label><%= GetMessage("Or") %></label></td>
		</tr>
		<tr>
			<td class="label">E-Mail:</td>
			<td><asp:TextBox ID="tbEMail" runat="server"></asp:TextBox></td>
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
			        <asp:PlaceHolder runat="server" ID="captchaPlaceHolder">
			        </asp:PlaceHolder>
			        	
			</td>
			
		</tr>
		<tr><td></td><td><asp:Image ID="imgCaptcha" runat="server" AlternateText="CAPTCHA" />	</td></tr>
		<tr> 
			<td></td>
			<td><asp:Button ID="Button1" runat="server" ValidationGroup="vgLoginForm" OnClick="Button1_Click" Text="<%$ Loc:ButtonText.Send %>" /></td>
		</tr>
	</table>
	</div>
	<br clear="all"/>

	<div class="footer">
	    <%= string.Format(
            "<p>{0}</p><p>{1}</p><p>{2}</p>",
            GetMessage("IfYouForgetYourPasswordEnterYourLoginOrEmail"),
            string.Format(GetMessage("Go2PasswordChangeForm"), "<a href=\"PasswordRecoveryAlt1.aspx\">", "</a>"),
	        string.Format(GetMessage("Return2LoginForm"), "<a href=\"Login.aspx\">", "</a>")           
            )%>
	</div>
</div>
	
</asp:Content>

