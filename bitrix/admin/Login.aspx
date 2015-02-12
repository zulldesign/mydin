
<%@ Page Language="C#" MasterPageFile="AdminMasterPage.master" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login1" Title="<%$ Loc:PageTitle.Authorization %>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" Width="438px" ValidationGroup="vgLoginForm"  />


<div class="auth-form">
	<div class="header"><%= GetMessage("PleaseLogin") %></div>

	<div class="picture"></div>
	<div class="table">
	<table cellpadding="0" cellspacing="0" border="0">
		<tr>
			<td class="label"><%= GetMessage("LegendLogin") + ":" %></td>
			<td><asp:TextBox ID="LoginField" runat="server" ValidationGroup="vgLoginForm"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="LoginField"
					Display="Dynamic" ValidationGroup="vgLoginForm">*</asp:RequiredFieldValidator></td>
		</tr>
		<tr>
			<td class="label"><%= GetMessage("LegendPassword") + ":" %></td>
			<td><asp:TextBox ID="PasswordField" runat="server" TextMode="Password" ValidationGroup="vgLoginForm"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="PasswordField"
					Display="Dynamic" ValidationGroup="vgLoginForm">*</asp:RequiredFieldValidator></td>
		</tr>
		
		<tr> 
			<td></td>
			<td><asp:CheckBox ID="CheckBoxRemember" runat="server" Text="<%$ Loc:CheckBoxText.RememberMe %>" ValidationGroup="vgLoginForm" /></td>
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
		<tr><td></td><td><asp:Image ID="imgCaptcha" runat="server" AlternateText="CAPTCHA" /></td></tr>
		<tr> 
			<td></td>
			<td><asp:Button ID="LoginButton" runat="server" Text="<%$ Loc:ButtonText.Login %>" OnClick="LoginButton_Click" ValidationGroup="vgLoginForm" /></td>
		</tr>
	</table>
	</div>
	<br clear="all"/>

	<div class="footer" id="divLoginFooterHint" runat="server">	
       <%--<p><b><%= GetMessage("DoYouForgetPassword") %></b></p>
		<p>
		    <asp:Label ID="lbFogotHint" runat="server" Text=""></asp:Label>
		    <%= string.Format(GetMessage("FormatGo2PasswordRecoveryForm"), "<a href=\"PasswordRecoveryAlt.aspx\">", "</a>") %>
		</p>
		<p>
		    <%= string.Format(GetMessage("FormatGo2PasswordChangeForm"), "<a href=\"PasswordRecoveryAlt1.aspx\">", "</a>") %>
		</p>--%>
		<%= GetFotterHintHtml() %>
	</div>
</div>

</asp:Content>