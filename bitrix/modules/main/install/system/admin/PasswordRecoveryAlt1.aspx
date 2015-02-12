<%--<%@ Page Language="C#" MasterPageFile="AdminMasterPage.master" AutoEventWireup="true" CodeFile="PasswordRecoveryAlt1.aspx.cs" Inherits="PasswordRecoveryAlt1" Title="Изменение пароля" Theme="AdminTheme" StylesheetTheme="AdminTheme" %>
zg, 25.04.2008
--%>
<%@ Page Language="C#" MasterPageFile="AdminMasterPage.master" AutoEventWireup="true" CodeFile="PasswordRecoveryAlt1.aspx.cs" Inherits="PasswordRecoveryAlt1" Title="<%$ Loc:PageTitle.ChangePassword %>" %>


<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<bx:BXMessage ID="successMessage" runat="server" CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False"
		Width="438px" />
<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" Width="438px"  />
<div class="auth-form">
	<div class="header"><%= GetMessage("ChangePassword") %></div>

	<div class="picture"></div>
	<div class="table">
	<table cellpadding="0" cellspacing="0" border="0">
		<tr> 
			<td class="label" valign="top"><%= GetMessage("Legend.Login") + ":" %></td>
			<td valign="top"><asp:TextBox ID="tbLogin" runat="server"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="tbLogin"
		ErrorMessage="<%$ Loc:Error.ThisFieldIsRequired %>" >*</asp:RequiredFieldValidator></td>
		</tr>
		<tr> 
			<td class="label" valign="top"><%= GetMessage("Legend.ControlString") + ":" %></td>
			<td valign="top"><asp:TextBox ID="tbCheckWord" runat="server"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="tbCHeckWord"
		ErrorMessage="<%$ Loc:Error.ThisFieldIsRequired %>" >*</asp:RequiredFieldValidator></td>
		</tr>
		<tr> 
			<td class="label" valign="top"><%= GetMessage("Legend.NewPassword") + ":" %></td>
			<td valign="top"><asp:TextBox ID="tbPassword" runat="server" TextMode="Password"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="tbPassword"
		ErrorMessage="<%$ Loc:Error.ThisFieldIsRequired %>" >*</asp:RequiredFieldValidator></td>
		</tr>
		<tr> 
			<td class="label" valign="top"><%= GetMessage("Legend.PasswordConfirmation") + ":" %></td>
			<td valign="top"><asp:TextBox ID="tbPasswordConf" runat="server" TextMode="Password"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="tbPasswordConf"
		ErrorMessage="<%$ Loc:Error.ThisFieldIsRequired %>" >*</asp:RequiredFieldValidator></td>
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
			<td><asp:Button ID="Button1" runat="server" ValidationGroup="vgLoginForm" OnClick="Button1_Click" Text="<%$ Loc:ButtonText.ChangePassword %>" /></td>
		</tr>
	</table>
	</div>
	<br clear="all"/>

	<div class="footer">
		<p><%= GetMessage("AllFieldsIsRequired") %></p>
		<p><%= string.Format(GetMessage("Return2LoginForm"), "<a href=\"Login.aspx\">", "</a>") %></p>
	</div>
</div>

</asp:Content>

