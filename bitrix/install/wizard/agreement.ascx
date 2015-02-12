<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.Wizards.BXWizardStepStandardHtmlControl" %>
<%@ Import Namespace="Bitrix.UI.Wizards" %>
<%@ Import Namespace="System.Collections.Generic" %>
<script runat="server">
	protected override BXWizardResult OnActionShow(Bitrix.DataTypes.BXCommonBag parameters)
	{
		WizardContext.Navigation.Selected = "agreement";		
	
		UI.Load("Agreement");	
		BXWizardResultView view = Result.Render(GetMessage("Title"));
		view.Buttons.Add("prev", null);	
		view.Buttons.Add("next", null);
		return view;
	}

	protected override BXWizardResult OnActionNext(Bitrix.DataTypes.BXCommonBag parameters)
	{
		UI.LoadValues(parameters);
		System.Collections.Generic.List<string> errors = new System.Collections.Generic.List<string>();
		if (!Validate(errors))
		{
			BXWizardResultView view = Result.Render(GetMessage("Title"), errors);
			view.Buttons.Add("prev", null);	
			view.Buttons.Add("next", null);
			return view;
		}
		
		UI.Overwrite("Agreement");
		return Result.Next();
	}
	
	protected override BXWizardResult OnActionPrevious(Bitrix.DataTypes.BXCommonBag parameters)
	{
		return Result.Previous();
	}
	
	private bool Validate(System.Collections.Generic.List<string> errors)
	{
		if (!UI.Data.GetBool("Accepted"))
		{
			errors.Add(GetMessage("Error.Agreement"));
			return false;
		}
		return true;
	}
</script>
<iframe name="license_text" src="<%= ResolveClientUrl("../lang/" + WizardContext.Locale + "/license.html") %>" width="100%" height="250" border="0" frameBorder="1" scrolling="yes"></iframe><br />
<br />
<% UI.CheckBox("Accepted", GetMessage("Checkbox.Agreement"), new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("id", "agree_license_id") }); %>
<script type="text/javascript">setTimeout(function() {var obj = document.getElementById("agree_license_id"); if (obj) obj.focus();}, 500);</script>