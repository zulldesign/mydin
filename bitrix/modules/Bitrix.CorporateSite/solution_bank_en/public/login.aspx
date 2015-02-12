<%@ Page Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.BXPublicPage, Main" Title="Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="bxcontent" runat="server" > 

 <bx:IncludeComponent 
	 id="systemauth1" 
	 runat="server" 
	 componentname="bitrix:system.auth" 
	 template="main" 
	 ProfilePath="" 
	 RegistrationDoAuthentication="False" 
	 RegistrationRedirectUrl="" 
	 FirstNameFieldMode="require" 
	 LastNameFieldMode="require" 
	 DisplayNameFieldMode="hide" 
	 RegistrationAllow="False" 
	 EnableSEF="True" 
	 SEFFolder="<%$ Options:Bitrix.BankSite:LoginSefFolder %>" 
	 ActionVariable="act" 
	 RegisterTemplate="/register/" 
	 PasswordRecoveryTemplate="/recovery/" 
	 PasswordResetTemplate="/reset/" 
	 SendConfirmationRequest="False" 
	 UrlToConfirmationPage="" 
	 ConfirmationTemplate="/confirmation/" 
	 UseCaptcha="True" 
 /> 
 
 </asp:Content>
