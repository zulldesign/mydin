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
		var p = WizardContext.State.Get<BXParamsBag<object>>("Bitrix.CommunitySite.Settings");
		
		var rules = new List<BXInstallHelperFileRule>(new []
		{
			new BXInstallHelperFileRule { Regex = @"^assets/"},
			new BXInstallHelperFileRule { Regex = @"^assets/errors/", Include = true},
			new BXInstallHelperFileRule { Regex = @"^sef\.community\.config$" },
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
		
		
		File.WriteAllText(Path.Combine(assets, "logotext.html"), p.GetString("Header"));
		File.WriteAllText(Path.Combine(assets, "copyright.html"), p.GetString("Copyright"));

		var template = WizardContext.State.GetString("Installer.Template");
		if (template == "taby")
		{
			var pathToLogoHtml = Path.Combine(assets, "logo.html");
			var uploadedLogo = WizardContext.State.GetInt("UploadedLogo", 0);
			BXFile file = null;
			if (uploadedLogo > 0 && (file = BXFile.GetById(uploadedLogo)) != null)
			{
				var fileInfo = new FileInfo(BXPath.MapPath(file.FileVirtualPath));
				var destination = site.DirectoryVirtualPath + "assets/" + "logo" + fileInfo.Extension;

				UI.CopyFile(uploadedLogo, BXPath.MapPath(destination), true);
				
				var logoHtml = BXPath.MapPath(site.DirectoryVirtualPath + "assets/logo.html");
				File.WriteAllText(logoHtml, "<img src=\"" + VirtualPathUtility.ToAbsolute(destination) + "\"/>");
			}
			else if (!File.Exists(pathToLogoHtml))
			{
				//Если установка в первый раз, копируем дефолтный логотип
				File.Copy(BXPath.MapPath(BXPath.Combine(solutionPath, "public/assets/logo.jpg")), Path.Combine(assets, "logo.jpg"), true);
				File.WriteAllText(pathToLogoHtml, "<img src=\"" + site.DirectoryAbsolutePath + "assets/logo.jpg\"/>");
			}
		}
		
		UI.SetProgressBarValue("Installer.ProgressBar", "Bitrix.CommunitySite", 1);
		return Result.Next();
	}
</script>
Copy Files
<% UI.ProgressBar("Installer.ProgressBar"); %>