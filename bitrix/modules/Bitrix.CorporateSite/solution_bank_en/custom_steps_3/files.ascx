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
		var p = WizardContext.State.Get<BXParamsBag<object>>("Bitrix.BankSite.Settings");
		
		var rules = new List<BXInstallHelperFileRule>(new []
		{
			new BXInstallHelperFileRule { Regex = @"^src/" },
			new BXInstallHelperFileRule { Regex = @"^src/phone\.html$", Include = true, Overwrite = false },
			new BXInstallHelperFileRule { Regex = @"^src/schedule\.html$", Include = true, Overwrite = false },
			new BXInstallHelperFileRule { Regex = @"^src/sidebar\.html$", Include = false },
			new BXInstallHelperFileRule { Regex = @"^sef\.bank_site\.config$" },
			new BXInstallHelperFileRule { Regex = @"\.menu$" }
		});
		if (!p.GetBool("Overwrite"))
			rules.Insert(0, new BXInstallHelperFileRule { Overwrite = false, Include = true });
		
		BXInstallHelper.CopyDirectory(
			BXPath.Combine(solutionPath, "public"),
			site.DirectoryVirtualPath,
			rules
		);
		
		
		string assets = BXPath.MapPath(site.DirectoryVirtualPath + "src");
		Directory.CreateDirectory(assets);
		
		var sidebarSource = Path.Combine(BXPath.MapPath(solutionPath), "public\\src\\sidebar.html");
		var sidebarTarget = Path.Combine(assets, "sidebar.html");
		if (!File.Exists(sidebarTarget) && File.Exists(sidebarSource))
			File.WriteAllText(sidebarTarget, File.ReadAllText(sidebarSource).Replace("#SITE_DIR#", site.DirectoryAbsolutePath));
		
		File.WriteAllText(Path.Combine(assets, "slogan.html"), p.GetString("Slogan"));
		File.WriteAllText(Path.Combine(assets, "copyright.html"), p.GetString("Copyright"));
		File.WriteAllText(Path.Combine(assets, "bannertext.html"), p.GetString("BannerText"));
		UI.SetProgressBarValue("Installer.ProgressBar", "Bitrix.BankSite", 1);


		var pathToLogoHtml = Path.Combine(assets, "logo.html");
		var template = WizardContext.State.GetString("Installer.Template");
		var uploadedLogo = WizardContext.State.GetInt("UploadedLogo", 0);
		var defaultLogo = System.Web.Hosting.HostingEnvironment.MapPath(BXPath.Combine(solutionPath, "templates/" + template + "/themes/" + WizardContext.State.GetString("Installer.Theme") + "/images/logo.gif"));
		BXFile file = null;
		if (uploadedLogo > 0 && (file = BXFile.GetById(uploadedLogo)) != null)
		{
			var fileInfo = new FileInfo(BXPath.MapPath(file.FileVirtualPath));
			var destination = site.DirectoryVirtualPath + "src/" + "logo" + fileInfo.Extension;

			UI.CopyFile(uploadedLogo, BXPath.MapPath(destination), true);

			var logoHtml = BXPath.MapPath(site.DirectoryVirtualPath + "src/logo.html");
			File.WriteAllText(logoHtml, "<img src=\"" + VirtualPathUtility.ToAbsolute(destination) + "\"/>");
		}
		else if (!File.Exists(pathToLogoHtml))
		{
			//Если установка в первый раз, копируем дефолтный лого
			File.Copy(defaultLogo, Path.Combine(assets, "default_logo.gif"), true);
			File.WriteAllText(pathToLogoHtml, "<img src=\"" + site.DirectoryAbsolutePath + "src/default_logo.gif\"/>");
		}
		else
		{
			//Если файл есть, нужно проверить какой лого был загружен. Если дефолтовый, то нужно его заменить на дефолтовый из правильной цветовой схемы
			var logoFileContent = File.ReadAllText(pathToLogoHtml, Encoding.UTF8);
			var regex = new Regex(@"src=[""']([^""']*)[""']", RegexOptions.IgnoreCase);
			Match m = regex.Match(logoFileContent);
			string logoSrcPath = m.Success ? HttpUtility.HtmlDecode(m.Groups[1].Captures[0].Value) : String.Empty;
			if (logoSrcPath.EndsWith("default_logo.gif"))
			{
				File.Copy(defaultLogo, Path.Combine(assets, "default_logo.gif"), true);
				File.WriteAllText(pathToLogoHtml, "<img src=\"" + site.DirectoryAbsolutePath + "src/default_logo.gif\"/>");
			}
		}

		var pathToBannerHtml = Path.Combine(assets, "banner.html");
		var uploadedBanner = WizardContext.State.GetInt("UploadedBanner", 0);
		file = null;
		if (uploadedBanner > 0 && (file = BXFile.GetById(uploadedBanner)) != null)
		{
			var fileInfo = new FileInfo(BXPath.MapPath(file.FileVirtualPath));
			var destination = site.DirectoryAbsolutePath + "src/" + "banner" + fileInfo.Extension;

			UI.CopyFile(uploadedBanner, BXPath.MapPath(destination), true);

			var bannerHtml = BXPath.MapPath(site.DirectoryVirtualPath + "src/banner.html");
			File.WriteAllText(bannerHtml, "<img src=\"" + VirtualPathUtility.ToAbsolute(destination) + "\"/>");
		}
		else if (!File.Exists(pathToBannerHtml))
		{
			//Если установка в первый раз, копируем дефолтный баннер
			File.Copy(BXPath.MapPath(BXPath.Combine(solutionPath, "public/src/banner.png")), Path.Combine(assets, "banner.png"), true);
			File.WriteAllText(pathToBannerHtml, "<img src=\"" + site.DirectoryAbsolutePath + "src/banner.png\"/>");
		}
		
		return Result.Next();
	}
</script>
Copy Files
<% UI.ProgressBar("Installer.ProgressBar"); %>