<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.Wizards.BXWizardStepStandardHtmlControl" %>
<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.Configuration" %>
<%@ Import Namespace="Bitrix.DataLayer" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<%@ Import Namespace="Bitrix.IO" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="Bitrix.UI.Wizards" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.IO" %>
<script runat="server">
	const string Key = "Bitrix.PersonalSite.Settings";
	
	protected override void OnWizardInit()
	{
		if (WizardContext.State.Get<BXParamsBag<object>>(Key) != null)
			return;
		
		var parameters = new BXParamsBag<object>();
		WizardContext.State[Key] = parameters;
		
		var siteId = WizardContext.State.GetString("Installer.SiteId");
		var site = BXSite.GetById(siteId, BXTextEncoder.EmptyTextEncoder);
	
		var header = BXPath.MapPath(site.DirectoryVirtualPath + "assets/header.html");
		parameters["Header"] = File.Exists(header) ? File.ReadAllText(header, Encoding.UTF8) : "Blog of Victoria Morrison";
		
		var footer = BXPath.MapPath(site.DirectoryVirtualPath + "assets/footer.html");
		parameters["Footer"] = File.Exists(footer) ? File.ReadAllText(footer, Encoding.UTF8) : "© Victoria Morrison, 2011";
		
		var xmlId = "Bitrix.PersonalSite.Blog." + site.Id;
		BXBlog blog = null;
		var blogs = BXBlog.GetList(
			new BXFilter(new BXFilterItem(BXBlog.Fields.XmlId, BXSqlFilterOperators.Equal, xmlId)),
			null,
			new BXSelectAdd(BXBlog.Fields.Categories, BXBlog.Fields.CustomFields.DefaultFields),
			null,
			BXTextEncoder.EmptyTextEncoder
		);
		if (blogs.Count > 0)
			blog = blogs[0];
		
		bool check = !string.Equals(BXOptionManager.GetOptionString("main", "InstalledSolution", null, WizardContext.State.GetString("Installer.SiteId")), "Bitrix.PersonalSite");
		parameters["InstallDemoData"] = check;
		parameters["Overwrite"] = check;
	}		
	protected override BXWizardResult OnActionShow(Bitrix.DataTypes.BXCommonBag parameters)
	{
		UI.Load(Key);
		
		var	view = Result.Render("Configure Solution");
		view.Buttons.Add("prev", null); 
		view.Buttons.Add("next", null);
		return view;
	}

	protected override BXWizardResult OnActionPrevious(Bitrix.DataTypes.BXCommonBag parameters)
	{
		return new BXWizardResultCancel();
	}
	
	protected override BXWizardResult OnActionNext(Bitrix.DataTypes.BXCommonBag parameters)
	{
		UI.LoadValues(parameters);
		UI.Overwrite(Key);
		UI.SetProgressBarMaxValue("Installer.ProgressBar", "Bitrix.PersonalSite", 7);
		return new BXWizardResultFinish();
	}
</script>
<div class="wizard-input-form">
	<div class="wizard-input-form-block">
		<h4>Site Name</h4>
		<div class="wizard-input-form-block-content">
		<div class="wizard-input-form-field wizard-input-form-field-text"><% UI.InputText("Header", null); %></div>
		</div>
	</div>

	<div class="wizard-input-form-block">
		<h4>Copyrights</h4>
		<div class="wizard-input-form-block-content">
		<div class="wizard-input-form-field wizard-input-form-field-text"><% UI.InputText("Footer", null); %></div>
		</div>
	</div>

	<div class="wizard-input-form-block">
		<div class="wizard-input-form-block-content">
			<div class="wizard-input-form-field wizard-input-form-field-checkbox"><% UI.CheckBox("InstallDemoData", "Install Demo Data", null); %></div>
			<div class="wizard-input-form-field wizard-input-form-field-checkbox"><% UI.CheckBox("Overwrite", "Overwrite Existing Files", null); %></div>
		</div>
	</div>
</div>