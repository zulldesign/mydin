using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Bitrix.IO;
using Bitrix.Modules;
using Bitrix.Services;
using Bitrix.UI.Wizards;
using Bitrix.DataTypes;

namespace Bitrix.Wizards.Install
{
	public partial class InstallModulesWizardStep : BXWizardStepStandardHtmlControl
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

		protected override BXWizardResult OnWizardAction(string action, BXCommonBag parameters)
		{
			switch (action)
			{
				case "":
					return ShowStatus(GetMessage("SubTitle.BuildOrder"), null, "buildorder");
				case "buildorder":
					return BuildOrder();
				case "advance":
					return Advance();
				case "finalize":
					return Finalize();
				default:
					return base.OnWizardAction(action, parameters);
			}
		}
			
		private BXWizardResult BuildOrder()
		{
			List<string> order = WizardContext.State.Get<List<string>>("InstallModules.Order");
			if (order == null)
				WizardContext.State["InstallModules.Order"] = order = Bitrix.Install.BXInstallHelper.BuildInstallOrder();
			UI.SetProgressBarMaxValue("Installer.ProgressBar", "Modules", order.Count * 7);
						
			return ShowStatus(GetMessage("SubTitle.BuildOrder"), null, "advance");
		}

		private BXWizardResult Advance()
		{
			List<string> modules;
			object o;
			if (!WizardContext.State.TryGetValue("InstallModules.Order", out o) || (modules = o as List<string>) == null)
				return Result.Action("buildorder");

			int moduleIndex;
			InstallationPhase modulePhase;
			if (!WizardContext.State.TryGetValue("InstallModules.Index", out o))
			{
				moduleIndex = -1;
				modulePhase = InstallationPhase.Finish;
			}
			else
			{
				moduleIndex = (int)o;
				if (WizardContext.State.TryGetValue("InstallModules.Phase", out o))
					modulePhase = (InstallationPhase)(int)o;
				else 
					modulePhase = InstallationPhase.Start;
			}
			
			if (moduleIndex >= modules.Count)
				return Finalize();

			switch (modulePhase)
			{
				case InstallationPhase.DB:
					InstallDB(modules[moduleIndex]);
					break;
				case InstallationPhase.Files:
					InstallFiles(modules[moduleIndex]);
					break;
				case InstallationPhase.Data:
					InstallData(modules[moduleIndex]);
					break;
				case InstallationPhase.Events:
					InstallEvents(modules[moduleIndex]);
					break;
				case InstallationPhase.Assemblies:
					InstallAssembly(modules[moduleIndex]);
					break;
				case InstallationPhase.Configuration:
					InstallConfiguration(modules[moduleIndex]);
					break;
				case InstallationPhase.Finalize:
					InstallFinalize(modules[moduleIndex]);
					break;
			}
			if (modulePhase >= InstallationPhase.Start && modulePhase < InstallationPhase.Finish)
				UI.SetProgressBarValue("Installer.ProgressBar", "Modules", moduleIndex * 7 + (int)modulePhase + 1);

			
			if (modulePhase == InstallationPhase.Finish)
			{
				modulePhase = InstallationPhase.Start;
				moduleIndex++; 
				if (moduleIndex >= modules.Count)
					return Finalize();
			}
			else
			{
				modulePhase++;
			}

			string messageTemplate;
			switch (modulePhase)
			{
				case InstallationPhase.DB:
					messageTemplate = GetMessage("SubTitle.DB");
					break;
				case InstallationPhase.Files:
					messageTemplate = GetMessage("SubTitle.Files");
					break;
				case InstallationPhase.Data:
					messageTemplate = GetMessage("SubTitle.Data");
					break;
				case InstallationPhase.Events:
					messageTemplate = GetMessage("SubTitle.Events");
					break;
				case InstallationPhase.Assemblies:
					messageTemplate = GetMessage("SubTitle.Assemblies");
					break;
				case InstallationPhase.Configuration:
					messageTemplate = GetMessage("SubTitle.Configuration");
					break;
				case InstallationPhase.Finalize:
					messageTemplate = GetMessage("SubTitle.Finalize");
					break;
				default:
					messageTemplate = GetMessage("SubTitle.Default");
					break;
			}

			string name = BXLoc.GetModuleMessage(WizardContext.Locale, modules[moduleIndex], "Module.Name");
			WizardContext.State["InstallModules.Index"] = moduleIndex;
			WizardContext.State["InstallModules.Phase"] = (int)modulePhase;
			return ShowStatus(string.Format(messageTemplate, Encode(name)), null, "advance");
		}

		private void InstallDB(string moduleId)
		{
			BXModuleManager.LoadModule(moduleId).GetInstaller().InstallDB();
		}

		private void InstallFiles(string moduleId)
		{
			BXModuleManager.LoadModule(moduleId).GetInstaller().InstallFiles();
		}

		private void InstallData(string moduleId)
		{
			BXModuleManager.LoadModule(moduleId).GetInstaller().InstallData();
		}

		private void InstallEvents(string moduleId)
		{
			BXModuleManager.LoadModule(moduleId).GetInstaller().InstallEvents();
		}

		private void InstallAssembly(string moduleId)
		{
			string binPath = BXPath.MapPath("~/bin");
			DirectoryInfo moduleBin = new DirectoryInfo(BXPath.MapPath(String.Format("~/bitrix/modules/{0}/install/bin/", moduleId)));

			if (moduleBin.Exists)
			{
				string asmName = moduleId;
				string configPath = BXPath.MapPath(string.Format("~/bitrix/modules/{0}/module.config", moduleId));
				BXModuleConfig config = File.Exists(configPath) ? new BXModuleConfig(configPath) : null;
				if (config != null)
					asmName = config.ModuleAssembly ?? moduleId;

				foreach (FileInfo dll in moduleBin.GetFiles(String.Format("{0}.*", asmName)))
				{
					File.SetAttributes(dll.FullName, FileAttributes.Normal);
					dll.CopyTo(Path.Combine(binPath, dll.Name), true);
				}

				if (config != null)
				{
					foreach (BXModuleLibraryInfo l in config.PrivateLibraries)
					{
						foreach (FileInfo dll in moduleBin.GetFiles(String.Format("{0}.*", l.Name)))
						{
							File.SetAttributes(dll.FullName, FileAttributes.Normal);
							dll.CopyTo(Path.Combine(binPath, dll.Name), true);
						}
					}
				}				
			}
		}

		private void InstallConfiguration(string moduleId)
		{
			string path = BXPath.MapPath("~/web.config");
			XmlDocument xml = new XmlDocument();
			xml.Load(path);
			if (BXModuleManager.LoadModule(moduleId).GetInstaller().InstallAppConfiguration(xml))
				xml.Save(path);
		}

		private void InstallFinalize(string moduleId)
		{
			BXModuleManager.RegisterModuleInstallation(moduleId);
		}

		private BXWizardResult Finalize()
		{
			File.SetLastWriteTime(BXPath.MapPath("~/web.config"), DateTime.Now);
			return Result.Next();
		}

		enum InstallationPhase
		{
			Start = 0,
			DB = Start,
			Files,
			Data,
			Events,
			Assemblies,
			Configuration,
			Finalize,
			Finish
		}
	}
}