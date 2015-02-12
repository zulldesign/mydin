<%@ Page Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.BXPublicPage, Main" Title="" %>
<asp:Content ID="Content1" ContentPlaceHolderID="bxcontent" runat="server" >

	<bx:IncludeComponent 
		id="AuthForm" 
		runat="server" 
		componentname="bitrix:system.auth" 
		template="main" 
		ProfilePath="" 
		RegistrationAllow="True" 
		RegistrationDoAuthentication="True" 
		RegistrationRedirectUrl="<%$ Options:Bitrix.CommunitySite:SiteFolder%>" 
		SendConfirmationRequest="False" 
		UrlToConfirmationPage="" 
		FirstNameFieldMode="hide" 
		LastNameFieldMode="hide" 
		DisplayNameFieldMode="show" 
		EnableSEF="True" 
		SEFFolder="<%$ Options:Bitrix.CommunitySite:LoginSefFolder%>" 
		ActionVariable="auth_page" 
		RegisterTemplate="/register/" 
		PasswordRecoveryTemplate="/recovery/" 
		PasswordResetTemplate="/reset/" 
		ConfirmationTemplate="/confirmation/" 
		UseCaptcha="True" 
	/> 



</asp:Content>
