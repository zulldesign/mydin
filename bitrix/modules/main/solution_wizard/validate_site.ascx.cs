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
using Bitrix.DataTypes;
using Bitrix.Modules;
using Bitrix.Configuration;
using Bitrix.Install;

namespace Bitrix.Wizards.Solutions
{
	public partial class ValidateSiteWizardStep : BXWizardStepStandardHtmlControl
	{
		protected BXSite site;

		protected override BXWizardResult OnActionShow(BXCommonBag parameters)
		{
			site = BXSite.GetById(WizardContext.State.GetString("Installer.SiteId"), BXTextEncoder.EmptyTextEncoder);

			bool found = false;
			BXInstallHelper.ProcessFiles(
				site.DirectoryVirtualPath,
				delegate(FileInfo file, string relativePath)
				{
					found = true;
					return false;
				},
				new[] {
					new BXInstallHelperFileRule { Regex = @"(?:^|/)section\.config$" },
					new BXInstallHelperFileRule { Regex = @"(?:^|/)authorization\.config$" },
					new BXInstallHelperFileRule { Regex = @"^default.aspx$" }
				}
			);

			if (!found)
				return Result.Next();

			BXWizardResultView view = Result.Render(GetMessage("Title"));
			view.Buttons.Add("prev", null);
			view.Buttons.Add("next", null);
			return view;
		}

		protected override BXWizardResult OnActionNext(BXCommonBag parameters)
		{
			return Result.Next();
		}

		protected override BXWizardResult OnActionPrevious(BXCommonBag parameters)
		{
			return Result.Previous();
		}
	}
}