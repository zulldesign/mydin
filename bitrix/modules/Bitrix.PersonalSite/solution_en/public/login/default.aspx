<%@ Page Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.BXPublicPage, Main"  Title="Authorization" %>

<asp:Content ID="Content" ContentPlaceHolderID="BXContent" runat="server" > 
<bx:IncludeComponent 
	id="Login" 
	runat="server" 
	componentname="bitrix:system.auth" 
	template=".default" 
	ProfilePath="<%$ Options:Bitrix.PersonalSite:ProfileUrl %>" 
	RegistrationDoAuthentication="False" 
	RegistrationRedirectUrl="" 
	FirstNameFieldMode="require" 
	LastNameFieldMode="require" 
	DisplayNameFieldMode="hide" 
	RegistrationAllow="False" 
	EnableSEF="True" 
	SEFFolder="<%$ Options:Bitrix.PersonalSite:LoginSefFolder %>" 
	ActionVariable="act" 
	RegisterTemplate="/register/" 
	PasswordRecoveryTemplate="/recovery/" 
	PasswordResetTemplate="/reset/" 
	SendConfirmationRequest="False" 
	UrlToConfirmationPage="" 
	ConfirmationTemplate="/confirmation/" 
	UseCaptcha="True" 
/> </asp:Content>
