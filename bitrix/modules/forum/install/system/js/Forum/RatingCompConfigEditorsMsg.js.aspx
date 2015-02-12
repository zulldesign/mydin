<%@ Page Language="C#" EnableEventValidation="false" EnableViewStateMac="false" Inherits="Bitrix.UI.BXJavaScriptLocalizationPage" %>
<%@ OutputCache Duration="2592000" Location="ServerAndClient" VaryByParam="lang;t" %>
<script runat="server" type="text/C#">
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		LanguageId = Request.QueryString["lang"];
        JavaScriptVariable = "window.FORUM_RATING_COMP_CFG_ED_MSG";
	}
</script>
