using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using Bitrix.UI.Wizards;
using System.Xml;
using Bitrix.Services.Text;
using Bitrix.IO;
using Bitrix.Services;
using Bitrix.DataTypes;
using Bitrix.Configuration;

namespace Bitrix.Wizards.Solutions
{
	public partial class ThemeWizardStep : BXWizardStepStandardHtmlControl
	{
		protected List<ThemeInfo> themes;
		protected string selected;

		protected override BXWizardResult OnActionShow(BXCommonBag parameters)
		{
			if (WizardContext.State.ContainsKey("Installer.Theme.GoBack"))
			{
				WizardContext.State.Remove("Installer.Theme.GoBack");
				return Result.Previous();
			}
			
			object selectedObj;
			if (WizardContext.State.TryGetValue("Installer.Theme", out selectedObj))
				selected = selectedObj.ToString();

			LoadThemes();

			if (themes.Count <= 1)
			{
				WizardContext.State["Installer.Theme"] = (themes.Count == 1) ? themes[0].Id : null;
				WizardContext.State["Installer.Theme.GoBack"] = "";
				WizardContext.Navigation.Selected = "install";
				return Result.Next();
			}
			else
			{
				var t = themes.Find(x => x.Id == selected);
				if (t == null)
				{
					var solution = WizardContext.State.GetString("Installer.Solution");
					var siteId = WizardContext.State.GetString("Installer.SiteId");
					selected = BXOptionManager.GetOptionString(solution, "InstalledTemplateTheme", null, siteId);
					t = themes.Find(x => x.Id == selected);
				}
				if (t == null)
					selected = themes[0].Id;
			}
			
			return PrepareView();
		}

		protected override BXWizardResult OnActionNext(BXCommonBag parameters)
		{
			var selected = parameters.GetString("selected-solution");
			if (selected == null)
				return PrepareView(GetMessage("Error.SelectTheme"));
			
			WizardContext.State["Installer.Theme"] = selected;
			WizardContext.Navigation.Selected = "install";
			return Result.Next();
		}

		protected override BXWizardResult OnActionPrevious(BXCommonBag parameters)
		{
			return Result.Previous();
		}

		private BXWizardResult PrepareView(params string[] errors)
		{
			var nav = WizardContext.Navigation;
			if (nav["theme"] == null)
			{
				nav.Insert(
					nav.IndexOf(nav[nav.Selected]) + 1,
					new BXWizardNavigationStep("theme")
					{
						TitleHtml = GetMessage("Navigation.Theme")
					}
				);
			}
			nav.Selected = "theme";

			LoadThemes();
			BXWizardResultView view = Result.Render(GetMessage("Title"), errors);


			if(!WizardContext.State.GetBool("Installer.SkipSite")
				|| !WizardContext.State.GetBool("Installer.SkipSolution")
				|| !WizardContext.State.ContainsKey("Installer.Template.GoBack"))
			{
				view.Buttons.Add("prev", null);
			}
			view.Buttons.Add("next", null);
			return view;
		}

		private void LoadThemes()
		{
			if (themes != null)
				return;
			string solutionPath = (string)WizardContext.State["Installer.SolutionPath"];
			string templatesPath = BXPath.Combine(
				(string)WizardContext.State["Installer.SolutionPath"],
				"templates",
				(string)WizardContext.State["Installer.Template"],
				"themes"
			);
			themes = new List<ThemeInfo>();
			var templatesDir = new DirectoryInfo(HostingEnvironment.MapPath(templatesPath));
			if (!templatesDir.Exists)
				return;
			foreach (DirectoryInfo dir in templatesDir.GetDirectories())
			{
				try
				{
					var info = new ThemeInfo();
					info.Id = dir.Name;

					var loc = new BXResourceFile(Path.Combine(dir.FullName, string.Format("lang\\{0}\\include.lang", WizardContext.Locale)));
					if (!loc.TryGetValue("Theme.Name", out info.TitleHtml))
						info.TitleHtml = info.Id;
					loc.TryGetValue("Theme.Description", out info.DescriptionHtml);

					if (File.Exists(Path.Combine(dir.FullName, "small.png")))
						info.SmallImageUrl = VirtualPathUtility.ToAbsolute(BXPath.Combine(templatesPath, dir.Name, "small.png"));

					if (File.Exists(Path.Combine(dir.FullName, "big.png")))
						info.BigImageUrl = VirtualPathUtility.ToAbsolute(BXPath.Combine(templatesPath, dir.Name, "big.png"));

					themes.Add(info);
				}
				catch
				{
					continue;
				}
			}
		}

		public class ThemeInfo
		{
			public string Id;
			public string TitleHtml;
			public string DescriptionHtml;
			public string SmallImageUrl;
			public string BigImageUrl;
		}
	}
}