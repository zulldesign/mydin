<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.Wizards.BXWizardStepStandardHtmlControl" %>
<%@ Import Namespace="Bitrix.UI.Wizards" %>
<script runat="server">
	protected override BXWizardResult OnActionShow(Bitrix.DataTypes.BXCommonBag parameters)
	{
		WizardContext.Navigation.Selected = "welcome";
		
		BXWizardResultView view = Result.Render(GetMessage("Title"));
		view.Buttons.Add("next", null);
		return view;
	}

	protected override BXWizardResult OnActionNext(Bitrix.DataTypes.BXCommonBag parameters)
	{
		return Result.Next();
	}

	protected override BXWizardResult OnActionPrevious(Bitrix.DataTypes.BXCommonBag parameters)
	{
		return Result.Previous();
	}
</script>

<%= GetMessage("Body") %>