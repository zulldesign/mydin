<%@ Control Language="C#" AutoEventWireup="true" CodeFile="login.ascx.cs" Inherits="bitrix_components_bitrix_system_auth_templates__default_login" %>

<bx:IncludeComponent 
	runat="server"
	ID="IncludeComponent1" 
	ComponentName="bitrix:system.login"
	CacheMode="None"
	LoginRedirectPath="<%$ Results:LoginRedirectPath %>"
	PasswordRecoveryPath="<%$ Results:PasswordRecoveryPath %>"
	PasswordRecoveryCodePath="<%$ Results:PasswordRecoveryCodePath %>"
	RegisterPath="<%$ Results:RegisterPath %>"
	ProfilePath="<%$ Parameters:ProfilePath %>"
	FirstNameFieldMode="<%$ Parameters:FirstNameFieldMode %>"
	LastNameFieldMode="<%$ Parameters:LastNameFieldMode %>"
	DisplayNameFieldMode="<%$ Parameters:DisplayNameFieldMode %>"
	TryRegisterNewExternalUser ="<%$ Parameters: TryRegisterNewExternalUser %>"
	UrlToConfirmationPage = "<%$ Results:UrlToConfirmationPage %>"
	 />
<input type="hidden" name="auth_page" value="login"/>