<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.Wizards.BXWizardStepStandardHtmlControl" %>
<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<%@ Import Namespace="Bitrix.Install" %>
<%@ Import Namespace="Bitrix.IO" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="Bitrix.UI.Wizards" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.IO" %>
<script runat="server">
	protected override BXWizardResult OnActionShow(Bitrix.DataTypes.BXCommonBag parameters)
	{
		var	view = Result.Render("Solution Installation");
		view.AutoRedirect = true;
		view.RedirectAction = "next";
		return view;
	}

	protected override BXWizardResult OnActionNext(Bitrix.DataTypes.BXCommonBag parameters)
	{
		var siteId = WizardContext.State.GetString("Installer.SiteId");
		var site = BXSite.GetById(siteId, BXTextEncoder.EmptyTextEncoder);
		var solutionPath = WizardContext.State.GetString("Installer.SolutionPath");
		var p = WizardContext.State.Get<BXParamsBag<object>>("Bitrix.PersonalSite.Settings");
		
		var rules = new List<BXInstallHelperFileRule>(new []
		{
			new BXInstallHelperFileRule { Regex = @"^assets/"},
			new BXInstallHelperFileRule { Regex = @"^assets/about.jpg$", Include = true },
			new BXInstallHelperFileRule { Regex = @"^sef\.personal\.config$" },
			new BXInstallHelperFileRule { Regex = @"\.menu$" }
		});
		if (!p.GetBool("Overwrite"))
			rules.Insert(0, new BXInstallHelperFileRule { Overwrite = false, Include = true });
		
		BXInstallHelper.CopyDirectory(
			BXPath.Combine(solutionPath, "public"),
			site.DirectoryVirtualPath,
			rules
		);
		string assets = BXPath.MapPath(site.DirectoryVirtualPath + "assets");
		Directory.CreateDirectory(assets);
		
		
		File.WriteAllText(Path.Combine(assets, "header.html"), p.GetString("Header"));
		File.WriteAllText(Path.Combine(assets, "footer.html"), p.GetString("Footer"));
		
		UI.SetProgressBarValue("Installer.ProgressBar", "Bitrix.PersonalSite", 1);
		return Result.Next();
	}
</script>
Copy Files
<% UI.ProgressBar("Installer.ProgressBar"); %>