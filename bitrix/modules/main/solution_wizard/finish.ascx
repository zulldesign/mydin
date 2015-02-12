<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.Wizards.BXWizardStepStandardHtmlControl" %>
<%@ Import Namespace="Bitrix.UI.Wizards" %>
<script runat="server">
	protected override BXWizardResult OnActionShow(Bitrix.DataTypes.BXCommonBag parameters)
	{
		System.IO.File.SetLastWriteTime(Bitrix.IO.BXPath.MapPath("~/web.config"), DateTime.Now);
		if (!string.IsNullOrEmpty(WizardContext.State.GetString("Installer.Solution")))
		{
			Bitrix.Configuration.BXOptionManager.SetOptionString(
				"main", 
				"InstalledSolution",
				WizardContext.State.GetString("Installer.Solution"),
				WizardContext.State.GetString("Installer.SiteId")
			);
		}
			
		return Result.Action("show");
	}

	protected override BXWizardResult OnWizardAction(string action, Bitrix.DataTypes.BXCommonBag parameters)
	{
		if (action == "show")
		{
			WizardContext.Navigation.Selected = "finish";
			var view = Result.Render(GetMessage("Title"));
			view.Buttons.Add("next", GetMessage("GotoWebsite"));
			return view;
		}
		return base.OnWizardAction(action, parameters);
	}
	
	protected override BXWizardResult OnActionNext(Bitrix.DataTypes.BXCommonBag parameters)
	{
		return new BXWizardResultFinish();
	}
</script>
<%= GetMessage("Finished") %>