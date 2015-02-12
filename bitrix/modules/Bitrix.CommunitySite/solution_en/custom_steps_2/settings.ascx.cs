using Bitrix;
using Bitrix.Blog;
using Bitrix.Configuration;
using Bitrix.DataLayer;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Services.Text;
using Bitrix.UI.Wizards;
using System.Collections.Generic;
using System.IO;
using Bitrix.UI;
using System.Text;
using System.Web;
using System;
using System.Text.RegularExpressions;

public partial class SettingsStep : BXWizardStepStandardHtmlControl
{
	
	protected const string Key = "Bitrix.CommunitySite.Settings";
	protected string logoUrl;

	protected override void OnWizardInit()
	{
		if (WizardContext.State.Get<BXParamsBag<object>>(Key) != null)
			return;

		var parameters = new BXParamsBag<object>();
		WizardContext.State[Key] = parameters;

		var siteId = WizardContext.State.GetString("Installer.SiteId");
		var site = BXSite.GetById(siteId, BXTextEncoder.EmptyTextEncoder);

		var header = BXPath.MapPath(site.DirectoryVirtualPath + "assets/logotext.html");
		parameters["Header"] = File.Exists(header) ? File.ReadAllText(header, Encoding.UTF8) : "<h1>Cyclists</h1>\r\n<span>Bicycle Club</span>";
		parameters["SiteName"] = File.Exists(header) && site != null ? site.Name : "Bicycle Club";

		var footer = BXPath.MapPath(site.DirectoryVirtualPath + "assets/copyright.html");
		parameters["Copyright"] = File.Exists(footer) ? File.ReadAllText(footer, Encoding.UTF8) : "© Club, 2011";

		bool check = !string.Equals(BXOptionManager.GetOptionString("main", "InstalledSolution", null, WizardContext.State.GetString("Installer.SiteId")), "Bitrix.CommunitySite");
		parameters["InstallDemoData"] = check;
		parameters["Overwrite"] = check;
	}		
	protected override BXWizardResult OnActionShow(Bitrix.DataTypes.BXCommonBag parameters)
	{
		UI.Load(Key);
		return PrepareView(null);
	}

	private BXWizardResult PrepareView(List<string> errors)
	{
		var siteId = WizardContext.State.GetString("Installer.SiteId");
		var site = BXSite.GetById(siteId, BXTextEncoder.EmptyTextEncoder);
		var template = WizardContext.State.GetString("Installer.Template");
		var solutionPath = WizardContext.State.GetString("Installer.SolutionPath");

		if (template == "taby")
		{
			var uploadedLogo = WizardContext.State.GetInt("UploadedLogo", 0);
			var currentLogo = BXPath.MapPath(site.DirectoryVirtualPath + "assets/logo.html");
			var defaultLogo = VirtualPathUtility.ToAbsolute(BXPath.Combine(solutionPath, "public/assets/logo.jpg"));

			BXFile file = null;
			if (uploadedLogo > 0 && (file = BXFile.GetById(uploadedLogo, BXTextEncoder.EmptyTextEncoder)) != null)
			{
				logoUrl = HttpUtility.HtmlAttributeEncode(file.FilePath);
			}
			else if (File.Exists(currentLogo))
			{
				var logoFileContent = File.ReadAllText(currentLogo, Encoding.UTF8);
				var regex = new Regex(@"src=[""']([^""']*)[""']", RegexOptions.IgnoreCase);
				Match m = regex.Match(logoFileContent);
				string logoSrcPath = m.Success ? HttpUtility.HtmlDecode(m.Groups[1].Captures[0].Value) : String.Empty;
				Uri url;
				if (!String.IsNullOrEmpty(logoSrcPath) && Uri.TryCreate(HttpContext.Current.Request.Url, logoSrcPath, out url))
				{
					try
					{
						var serverPath = System.Web.Hosting.HostingEnvironment.MapPath(HttpUtility.UrlDecode(url.AbsolutePath));
						if (File.Exists(serverPath))
							logoUrl = logoSrcPath;
					}
					catch { }
				}
			}
			else
				logoUrl = defaultLogo;
		}


		var view = Result.Render("Configure Solution", errors);
		view.Buttons.Add("prev", null);
		view.Buttons.Add("next", null);
		return view;
	}

	protected override BXWizardResult OnWizardAction(string action, BXCommonBag parameters)
	{
		if (action == "upload")
		{
			UI.LoadValues(parameters);

			int fileId = 0;
			List<string> errors = new List<string>();
			try
			{
				var settings = new BXParamsBag<object>();
				settings["maxFileSize"] = 1024 * 1024;
				settings["maxWidth"] = 500;
				settings["maxHeight"] = 105;

				fileId = UI.SaveImage("logoImage", settings, errors);
			}
			catch (Exception ex)
			{
				errors = new List<string>() { {ex.Message} };
			}

			if (fileId > 0)
				WizardContext.State["UploadedLogo"] = fileId;

			UI.Overwrite(Key);

			return PrepareView(errors);
		}
		
		return base.OnWizardAction(action, parameters);
	}
	
	protected override BXWizardResult OnActionPrevious(Bitrix.DataTypes.BXCommonBag parameters)
	{
		UI.LoadValues(parameters);
		UI.Overwrite(Key);
		return new BXWizardResultCancel();
	}
	
	protected override BXWizardResult OnActionNext(Bitrix.DataTypes.BXCommonBag parameters)
	{
		UI.LoadValues(parameters);
		UI.Overwrite(Key);
		UI.SetProgressBarMaxValue("Installer.ProgressBar", "Bitrix.CommunitySite", 9);
		return new BXWizardResultFinish();
	}
}