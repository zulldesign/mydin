<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PasswordRecovery.ascx.cs" Inherits="bitrix_components_bitrix_system_auth_templates__default_PasswordRecovery" %>

<bx:IncludeComponent 
	runat="server"
	ID="IncludeComponent1" 
	ComponentName="bitrix:system.PasswordRecovery"
	LoginLink="<%$ Results:LoginLink %>"
	CacheMode="None" />

<bx:IncludeComponent 
	runat="server"
	ID="IncludeComponent2" 
	ComponentName="bitrix:system.PasswordRecoveryAlt"
	LoginLink="<%$ Results:LoginLink %>"
	PasswordRecoveryCodeLink="<%$ Results:PasswordRecoveryCodePath %>"
	CacheMode="None" />

<input type="hidden" name="auth_page" value="recovery"/>