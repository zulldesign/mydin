using System.Collections.Generic;
using System.IO;
using System.Text;
using Bitrix.Install;
using Bitrix.IO;
using Bitrix.UI.Wizards;
using SiteUpdater;
using System;
using Bitrix.DataTypes;

namespace Bitrix.Wizards.Install
{
	public partial class InstallUpdaterWizardStep : BXWizardStepStandardHtmlControl
	{
		protected override BXWizardResult OnWizardAction(string action, BXCommonBag parameters)
		{
			switch (action)
			{
				case "":
					return WizardContext.State.GetBool("Install.UpdateSystemInstalled") ? Result.Next() : Result.Action("installfiles");
				case "installfiles":
					return InstallFiles();
				case "installconfig":
					return InstallConfig();
				case "finalize":
					return Finalize();
				default:
					return base.OnWizardAction(action, parameters);
			}
		}

		private void CopyFiles(string sourcePath, string destinationPath)
		{
			DirectoryInfo sourceDir = new DirectoryInfo(sourcePath);

			if (!sourceDir.Exists)
				return;

			foreach (FileInfo file in sourceDir.GetFiles("*", SearchOption.AllDirectories))
			{
				string relativeDestPath = file.Directory.FullName.Substring(sourceDir.FullName.Length).Trim(new char[] { '\\', '/' });
				string fullDestPath = Path.Combine(destinationPath, relativeDestPath);
				Directory.CreateDirectory(fullDestPath);
				file.CopyTo(Path.Combine(fullDestPath, file.Name), true);
			}
		}
		private BXWizardResult InstallFiles()
		{
			CopyFiles(BXPath.MapPath("~/bitrix/install/updater/admin"), BXPath.MapPath("~/bitrix/admin"));
			CopyFiles(BXPath.MapPath("~/bitrix/install/updater/lang"), BXPath.MapPath("~/bitrix/lang"));			
			return Result.Action("installconfig");
		}
		private BXWizardResult InstallConfig()
		{
			Directory.CreateDirectory(BXPath.MapPath("~/bitrix/updates"));
			if (!File.Exists(BXPath.MapPath("~/bitrix/updates/updater.config")))
			{
				BXUpdaterConfig config = SiteUpdater.BXSiteUpdater.GetConfig();

				config.UpdateUrl = GetMessage("Updater.Server") ?? "http://www.bitrixsoft.com";
				config.Language = WizardContext.Locale;
				config.Key = "";
				System.Reflection.AssemblyName name = new System.Reflection.AssemblyName(typeof(BXUpdaterConfig).Assembly.FullName);
				config.Version = new BXUpdaterVersion(name.Version.Major, name.Version.Minor, name.Version.Build);
				
				string proxy = WizardContext.State.GetString("Options.UpdaterProxy");
				if (!string.IsNullOrEmpty(proxy))
				{
					config.UseProxy = true;
					config.ProxyAddress = proxy;
					
					string user = WizardContext.State.GetString("Options.UpdaterProxyUsername");
					if (!string.IsNullOrEmpty(user))
					{
						config.ProxyUsername = user;
						string password = WizardContext.State.GetString("Options.UpdaterProxyPassword");
						if (!string.IsNullOrEmpty(password))
							config.ProxyPassword = password;						
					}				
				}

				config.Update();
			}

			return Result.Action("finalize");
		}		
		private BXWizardResult Finalize()
		{			
			WizardContext.State["Install.UpdateSystemInstalled"] = true;
			return Result.Next();
		}
	}
}