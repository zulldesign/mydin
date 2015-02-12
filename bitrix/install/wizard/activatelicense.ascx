<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.Wizards.BXWizardStepStandardHtmlControl" %>
<%@ Import Namespace="Bitrix.UI.Wizards" %>
<%@ Import Namespace="SiteUpdater" %>
<%@ Import Namespace="System.Collections.Generic" %>

<script runat="server">
	protected override BXWizardResult OnActionShow(Bitrix.DataTypes.BXCommonBag parameters)
	{
		try
		{
			BXSiteUpdater updater = BXSiteUpdater.GetUpdater();
			updater.CheckInitialize();
			updater.GetLicenseData();
		}
		catch(Exception ex)
		{
			BXWizardResultView view = Result.Render(GetMessage("Title"), new string[] { ex.Message });
			view.Buttons.Add("prev", null);			
			return view;
		}
		return Result.Next();
	}

	protected override BXWizardResult OnActionPrevious(Bitrix.DataTypes.BXCommonBag parameters)
	{
		return Result.Action("licensekey", "", null);
	}
</script>