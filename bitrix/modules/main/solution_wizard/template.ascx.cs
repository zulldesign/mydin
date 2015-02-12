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
	public partial class TemplateWizardStep : BXWizardStepStandardHtmlControl
	{
		protected List<TemplateInfo> templates;
		protected string selected;

		protected override BXWizardResult OnActionShow(BXCommonBag parameters)
		{
			if (WizardContext.State.ContainsKey("Installer.Template.GoBack"))
			{
				WizardContext.State.Remove("Installer.Template.GoBack");
				return Result.Previous();
			}
			
			object selectedObj;
			if (WizardContext.State.TryGetValue("Installer.Template", out selectedObj))
				selected = selectedObj.ToString();

			LoadTemplates();
			
			if (templates.Count <= 1)
			{
				WizardContext.State["Installer.Template"] = (templates.Count == 1) ? templates[0].Id : null;
				WizardContext.State["Installer.Template.GoBack"] = "";
				return Result.Next();
			}
			else
			{
				var t = templates.Find(x => x.Id == selected);
				if (t == null)
				{
					var solution = WizardContext.State.GetString("Installer.Solution");
					var siteId = WizardContext.State.GetString("Installer.SiteId");
					selected = BXOptionManager.GetOptionString(solution, "InstalledTemplate", null, siteId);
					t = templates.Find(x => x.Id == selected);
				}
				if (t == null)
					selected = templates[0].Id;
			}

			return PrepareView();
		}

		protected override BXWizardResult OnActionNext(BXCommonBag parameters)
		{
			var selected = parameters.GetString("selected-solution");
			if (selected == null)
				return PrepareView(GetMessage("Error.SelectTemplate"));
			
			WizardContext.State["Installer.Template"] = selected;
			return Result.Next();
		}

		protected override BXWizardResult OnActionPrevious(BXCommonBag parameters)
		{
			return Result.Previous();
		}

		private BXWizardResult PrepareView(params string[] errors)
		{
			var nav = WizardContext.Navigation;
			if (nav["template"] == null)
			{
				nav.Insert(
					nav.IndexOf(nav[nav.Selected]) + 1,
					new BXWizardNavigationStep("template")
					{
						TitleHtml = GetMessage("Navigation.Template")
					}
				);
			}
			nav.Selected = "template";

			LoadTemplates();
			BXWizardResultView view = Result.Render(GetMessage("Title"), errors);
			if(!WizardContext.State.GetBool("Installer.SkipSite")
				|| !WizardContext.State.GetBool("Installer.SkipSolution"))
			{
				view.Buttons.Add("prev", null);
			}
			view.Buttons.Add("next", null);
			return view;
		}

		private void LoadTemplates()
		{
			if (templates != null)
				return;
			string solutionPath = (string)WizardContext.State["Installer.SolutionPath"];
			string templatesPath = BXPath.Combine(solutionPath, "templates");
			templates = new List<TemplateInfo>();
			var templatesDir = new DirectoryInfo(HostingEnvironment.MapPath(templatesPath));
			if (!templatesDir.Exists)
				return;
			foreach (DirectoryInfo dir in templatesDir.GetDirectories())
			{
				if (!File.Exists(Path.Combine(dir.FullName, "template.master")))
					continue;

				try
				{
					TemplateInfo info = new TemplateInfo();
					info.Id = dir.Name;
					
					var loc = new BXResourceFile(Path.Combine(dir.FullName, string.Format("lang\\{0}\\include.lang", WizardContext.Locale)));
					if (!loc.TryGetValue("Template.Name", out info.TitleHtml))
						info.TitleHtml = info.Id;

					loc.TryGetValue("Template.Description", out info.DescriptionHtml);
					
					if (File.Exists(Path.Combine(dir.FullName, "preview.gif")))
						info.ImageUrl = VirtualPathUtility.ToAbsolute(BXPath.Combine(templatesPath, dir.Name, "preview.gif"));

					templates.Add(info);
				}
				catch
				{
					continue;
				}
			}
		}

		public class TemplateInfo
		{
			public string Id;
			public string ImageUrl;
			public string TitleHtml;
			public string DescriptionHtml;
		}
	}
}