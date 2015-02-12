using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Bitrix.IO;
using Bitrix.Modules;
using Bitrix.UI.Wizards;
using Bitrix.DataTypes;
using Bitrix.Services.Text;
using Bitrix.DataLayer;
using Bitrix.Install;
using Bitrix.Configuration;

namespace Bitrix.Wizards.Solutions
{
	public partial class InstallTemplateWizardStep : BXWizardStepStandardHtmlControl
	{
		List<string> errors;
		private List<string> Errors
		{
			get { return errors ?? (errors = new List<string>()); }
		}
		protected string htmlMessage;

		private BXWizardResult ShowStatus(string htmlMessage, string nextStep, string nextAction)
		{
			this.htmlMessage = htmlMessage;
			BXWizardResultView view = Result.Render(GetMessage("Title"), errors);
			if ((errors == null || errors.Count == 0) && (!string.IsNullOrEmpty(nextStep) || !string.IsNullOrEmpty(nextAction)))
			{
				view.AutoRedirect = true;
				view.RedirectStep = nextStep;
				view.RedirectAction = nextAction;
			}
			WizardContext.Navigation.Selected = "install";
			return view;
		}

		protected override BXWizardResult OnActionShow(BXCommonBag parameters)
		{
			return ShowStatus(GetMessage("SubTitle"), null, "next");
		}

		protected override BXWizardResult OnActionNext(BXCommonBag parameters)
		{
			var siteId = WizardContext.State.GetString("Installer.SiteId");
			var site = BXSite.GetById(siteId, BXTextEncoder.EmptyTextEncoder);
			var solution = WizardContext.State.GetString("Installer.Solution");
			var solutionPath = WizardContext.State.GetString("Installer.SolutionPath");
			var template = WizardContext.State.GetString("Installer.Template");
			var theme = WizardContext.State.GetString("Installer.Theme");

			if (site == null || string.IsNullOrEmpty(solution) || string.IsNullOrEmpty(solutionPath) || string.IsNullOrEmpty(template))
			{
				UI.SetProgressBarValue("Installer.ProgressBar", "InstallTemplate", 1);
				return Result.Next();
			}

			string templateName = solution + "." + template + "." + site.Id;
			var templatePath = BXPath.Combine(solutionPath, "templates", template);
			BXInstallHelper.CopyDirectory(
				BXPath.Combine(solutionPath, "templates", template),
				"~/bitrix/templates/" + templateName, null);
			
			if (!string.IsNullOrEmpty(theme))
			{
				var themePath = BXPath.Combine(templatePath, "themes", theme);
				BXInstallHelper.CopyDirectory(
					themePath,
					"~/bitrix/templates/" + templateName,
					new[] 
					{
						new BXInstallHelperFileRule { Regex = @"^(?:small|big)\.png$" },
						new BXInstallHelperFileRule { Regex = @"^favicon.ico$" }
					}
				);
				var faviconPath = BXPath.MapPath(BXPath.Combine(themePath, "favicon.ico"));
				if (File.Exists(faviconPath))
				{
					try
					{
						File.Copy(faviconPath, BXPath.MapPath(site.DirectoryVirtualPath + "favicon.ico"));
					}
					catch
					{
					}
				}
			}

			RegisterTemplate(site.Id, templateName);
			WizardContext.State["Installer.TemplateName"] = templateName;
			
			BXOptionManager.SetOptionString(solution, "InstalledTemplate", template, siteId);
			BXOptionManager.SetOptionString(solution, "InstalledTemplateTheme", theme, siteId);
			UI.SetProgressBarValue("Installer.ProgressBar", "InstallTemplate", 1);
			return Result.Next();
		}

		private void RegisterTemplate(string siteId, string templateName)
		{
			var conditions = BXTemplateCondition.GetList(
				new BXFilter(new BXFilterItem(BXTemplateCondition.Fields.SiteId, BXSqlFilterOperators.Equal, siteId)),
				new BXOrderBy(new BXOrderByPair(BXTemplateCondition.Fields.Sort, BXOrderByDirection.Asc))
			);
			if (conditions.Count > 0 && conditions[0].ConditionType == 0)
			{
				conditions[0].Template = templateName;
				conditions[0].Save();
			}
			else
			{
				var c = new BXTemplateCondition();
				c.Template = templateName;
				c.SiteId = siteId;
				c.ConditionType = 0;
				c.Condition = "";
				c.Sort = 10;
				c.Save();

				int i = 10;
				foreach(var t in conditions)
				{
					c.Sort = (i += 10);
					c.Save();
				}
			}
		}
	}
}
