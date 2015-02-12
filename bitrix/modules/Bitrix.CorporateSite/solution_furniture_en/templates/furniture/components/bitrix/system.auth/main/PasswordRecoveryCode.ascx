<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PasswordRecoveryCode.ascx.cs" Inherits="bitrix_components_bitrix_system_auth_templates__default_PasswordRecoveryCode" %>
<bx:IncludeComponent 
	runat="server"
	ID="IncludeComponent1" 
	ComponentName="bitrix:system.PasswordRecoveryAltCode"
	LoginLink="<%$ Results:LoginLink %>"
	CacheMode="None" />

<input type="hidden" name="auth_page" value="recoverycode"/>