<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.Wizards.BXWizardStepStandardHtmlControl" %>
<%@ Import Namespace="Bitrix.UI.Wizards" %>
<script runat="server">
	protected override BXWizardResult OnActionShow(Bitrix.DataTypes.BXCommonBag parameters)
	{
		FormsAuthentication.SetAuthCookie((string)WizardContext.State["Install.AdminLogin"], false);
				
		return new BXWizardResultFinish();
	}
</script>