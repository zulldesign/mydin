<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.Wizards.BXWizardStepStandardHtmlControl" %>
<%@ Import Namespace="Bitrix.UI.Wizards" %>
<script runat="server">
	protected override BXWizardResult OnActionShow(Bitrix.DataTypes.BXCommonBag parameters)
	{
		System.Collections.Generic.List<string> order = Bitrix.Install.BXInstallHelper.BuildInstallOrder();
		WizardContext.State["InstallModules.Order"] = order;
		UI.SetProgressBarMaxValue("Installer.ProgressBar", "Modules", order.Count * 7);
		return Result.Next();
	}
</script>