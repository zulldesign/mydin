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
using Bitrix.DataLayer;

namespace Bitrix.Wizards.Install
{
	public partial class InstallMainWizardStep : BXWizardStepStandardHtmlControl
	{
		protected override void OnWizardInit()
		{
			UI.SetProgressBarMaxValue("Installer.ProgressBar", "Main", 7);
		}

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
					return ShowStatus(GetMessage("SubTitle.Database"), null, "database");
				case "database":
					return Database();
				case "systemfiles":
					return SystemFiles();
				case "defaulttemplate":
					return DefaultTemplate();
				case "installapplication":
					return InstallApplication();
				case "applicationconfiguration":
					return ApplicationConfiguration();
				case "afterrestart":
					return AfterRestart();
				case "data":
					return Data();
				case "finalize":
					return Finalize();
				default:
					return base.OnWizardAction(action, parameters);
			}
		}

		private string ConnectionString
		{
			get { return (string)WizardContext.State["Install.ConnectionString"]; }
		}

		private BXWizardResult Database()
		{
			Bitrix.Services.BXLoc.CurrentLocale = WizardContext.Locale;
			Bitrix.DataLayer.BXSqlManager.OriginalConnectionString = ConnectionString;

			Bitrix.Main.BXMain m = new Bitrix.Main.BXMain();
			Bitrix.Modules.BXModuleInstaller mi = m.GetInstaller();
			
			mi.InstallDB();
			UI.SetProgressBarValue("Installer.ProgressBar", "Main", 1);
			return ShowStatus(GetMessage("SubTitle.Files"), null, "systemfiles");
		}
		private BXWizardResult SystemFiles()
		{
			Bitrix.Services.BXLoc.CurrentLocale = WizardContext.Locale;
			Bitrix.DataLayer.BXSqlManager.OriginalConnectionString = ConnectionString;

			Bitrix.Main.BXMain m = new Bitrix.Main.BXMain();
			Bitrix.Modules.BXModuleInstaller mi = m.GetInstaller();
						
			mi.InstallFiles();
			UI.SetProgressBarValue("Installer.ProgressBar", "Main", 2);
			return ShowStatus(GetMessage("SubTitle.Files"), null, "defaulttemplate");
		}

		private BXWizardResult DefaultTemplate()
		{
			Bitrix.Install.BXInstallHelper.CopyDirectory(
				AppRelativeTemplateSourceDirectory + "template",
				"~/bitrix/templates/.default",
				null
			);
			UI.SetProgressBarValue("Installer.ProgressBar", "Main", 3);
			return ShowStatus(GetMessage("SubTitle.AppConfiguration"), null, "applicationconfiguration");
		}

		private BXWizardResult ApplicationConfiguration()
		{
			Bitrix.Services.BXLoc.CurrentLocale = WizardContext.Locale;
			Bitrix.DataLayer.BXSqlManager.OriginalConnectionString = ConnectionString;

			XmlDocument doc = new XmlDocument();
			doc.Load(BXPath.MapPath("~/web.config"));
			XmlElement root = doc.DocumentElement;
			
			foreach (XmlNode node in root.SelectNodes("system.web/httpModules/add[@name='BXInstallerHttpModule']"))
				node.ParentNode.RemoveChild(node);

			foreach (XmlNode node in root.SelectNodes("system.webServer/modules/add[@name='BXInstallerHttpModule' or @name='BXInstallerIMCheckerHttpModule']"))
				node.ParentNode.RemoveChild(node);
			
			Bitrix.Main.BXMain m = new Bitrix.Main.BXMain();
			Bitrix.Modules.BXModuleInstaller mi = m.GetInstaller();
						
			mi.InstallAppConfiguration(doc);

			// Inject connection string
			XmlElement configuration = doc.DocumentElement;
			XmlElement connectionStrings = Ensure(configuration, "connectionStrings");
			XmlElement bxConnectionString = Ensure(connectionStrings, "add", "[@name='BXConnectionString']");
			EnsureAttribute(bxConnectionString, "name").Value = "BXConnectionString";
			EnsureAttribute(bxConnectionString, "connectionString").Value = (string)WizardContext.State["Install.ConnectionString"];

			// Inject machine key
			if (configuration.SelectSingleNode("system.web/machineKey") == null)
			{
				XmlElement machineKey = Ensure(Ensure(configuration, "system.web"), "machineKey");
				EnsureAttribute(machineKey, "validationKey").Value = GenKey(64);
				EnsureAttribute(machineKey, "decryptionKey").Value = GenKey(32);
				EnsureAttribute(machineKey, "validation").Value = "SHA1";
				EnsureAttribute(machineKey, "decryption").Value = "AES";
			}

			doc.Save(BXPath.MapPath("~/web.config"));
			UI.SetProgressBarValue("Installer.ProgressBar", "Main", 4);
			return ShowStatus(GetMessage("SubTitle.AppConfiguration"), null, "installapplication");
		}
		private BXWizardResult InstallApplication()
		{
			File.Copy(BXPath.MapPath("~/bitrix/modules/Main/install/public/global.asax"), BXPath.MapPath("~/global.asax"), true); 
			UI.SetProgressBarValue("Installer.ProgressBar", "Main", 5);
			return ShowStatus(GetMessage("SubTitle.Restart"), null, "afterrestart");
			
		}
		
		
		private BXWizardResult AfterRestart()
		{
			if (!BXApplicationHelper.Started)
			{
				File.SetLastWriteTime(BXPath.MapPath("~/web.config"), DateTime.Now);
				return ShowStatus(GetMessage("SubTitle.Restart"), null, "afterrestart");
			}
			UI.SetProgressBarValue("Installer.ProgressBar", "Main", 6);
			return ShowStatus(GetMessage("SubTitle.Configuration"), null, "data");
		}
		private BXWizardResult Data()
		{
			Bitrix.Services.BXLoc.CurrentLocale = WizardContext.Locale;

			Bitrix.Main.BXMain m = new Bitrix.Main.BXMain();
			Bitrix.Modules.BXModuleInstaller mi = m.GetInstaller();

			mi.InstallData();
			mi.InstallEvents();
			UI.SetProgressBarValue("Installer.ProgressBar", "Main", 7);
			return ShowStatus(GetMessage("SubTitle.Configuration"), null, "finalize");
		}
		private BXWizardResult Finalize()
		{
			BXModuleManager.RegisterModuleInstallation("main");
			
			File.Copy(BXPath.MapPath("~/bitrix/modules/Main/install/public/login.ashx"), BXPath.MapPath("~/login.ashx"), true); 

			BXSite site = BXSite.GetDefaultSite();
			if (site != null)
			{
				RegisterTemplate(site.TextEncoder.Decode(site.Id), ".default");
				WizardContext.State["Install.SiteId"] = site.TextEncoder.Decode(site.Id);
			}

			XmlDocument doc = new XmlDocument();
			doc.Load(BXPath.MapPath("~/web.config"));
			XmlElement root = doc.DocumentElement;
			EnsureAttribute(Ensure(Ensure(Ensure(root, "system.web"), "authentication"), "forms"), "loginUrl").Value = "~/login.ashx";
			doc.Save(BXPath.MapPath("~/web.config"));
						
			return Result.Next();
		}

		static XmlElement Ensure(XmlElement e, string name, string suffixPath)
		{
			return (XmlElement)(e.SelectSingleNode(name + suffixPath) ?? e.AppendChild(e.OwnerDocument.CreateElement(name)));
		}
		static XmlElement Ensure(XmlElement e, string name)
		{
			return Ensure(e, name, null);
		}
		static XmlAttribute EnsureAttribute(XmlElement e, string name)
		{
			return e.Attributes[name] ?? (e.Attributes.Append(e.OwnerDocument.CreateAttribute(name)));
		}
		static void RegisterTemplate(string siteId, string templateName)
		{
			BXTemplateConditionCollection conditions = BXTemplateCondition.GetList(
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
				BXTemplateCondition c = new BXTemplateCondition();
				c.Template = templateName;
				c.SiteId = siteId;
				c.ConditionType = 0;
				c.Condition = "";
				c.Sort = 10;
				c.Save();

				int i = 10;
				foreach(BXTemplateCondition t in conditions)
				{
					c.Sort = (i += 10);
					c.Save();
				}
			}
		}
		static string GenKey(int bytes)
		{
			byte[] buff = new byte[bytes];
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			rng.GetBytes(buff);
			StringBuilder sb = new StringBuilder(bytes * 2);
			for (int i = 0; i < buff.Length; i++)
				sb.Append(string.Format("{0:X2}", buff[i]));
			return sb.ToString();
		}
	}
}