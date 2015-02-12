<%@ Control Language="C#" AutoEventWireup="true" CodeFile="register.ascx.cs" Inherits="bitrix_components_bitrix_system_auth_templates__default_register" %>
<bx:IncludeComponent 
	runat="server"
	ID="IncludeComponent1" 
	ComponentName="bitrix:system.register"
	CacheMode="None"
	DoAuthentication = "<%$ Parameters:RegistrationDoAuthentication %>"
	RedirectUrl = "<%$ Parameters:RegistrationRedirectUrl %>"
	UrlToConfirmationPage = "<%$ Results:UrlToConfirmationPage %>"
	 />

<input type="hidden" name="auth_page" value="register"/>
