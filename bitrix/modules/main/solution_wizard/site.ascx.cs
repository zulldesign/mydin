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
using Bitrix.DataLayer;

namespace Bitrix.Wizards.Solutions
{
	public partial class SiteWizardStep : BXWizardStepStandardHtmlControl
	{
		protected override void OnWizardInit()
		{
			string siteId;
			if (WizardContext.State.TryGetString("Installer.SiteId", out siteId) && WizardContext.State.ContainsKey("Installer.Solution"))
				WizardContext.State["Installer.SkipSite"] = true;

			BXParamsBag<object> data = WizardContext.State.Get<BXParamsBag<object>>("Site");
			if (data == null)
				WizardContext.State["Site"] = data = new BXParamsBag<object>();

			var site = !string.IsNullOrEmpty(siteId) ? BXSite.GetById(siteId) : null;
			if (site != null)
			{
				data["SiteMode"] = "existing";
				data["ExistingSiteId"] = site.TextEncoder.Decode(site.Id);
			}
			else
				data["SiteMode"] = "new";

			var langs = BXLanguage.GetList(
				new BXFilter(
					new BXFilterItem(BXLanguage.Fields.Active, BXSqlFilterOperators.Equal, true),
					new BXFilterItem(BXLanguage.Fields.Default, BXSqlFilterOperators.Equal, true)
				),
				null,
				new BXSelect(BXLanguage.Fields.ID),
				null,
				BXTextEncoder.EmptyTextEncoder
			);
			if (langs.Count != 0)
				data["NewSiteLang"] = langs[0].Id;
		}

		protected override BXWizardResult OnActionShow(BXCommonBag parameters)
		{
			if (WizardContext.State.GetBool("Installer.SkipSite"))
			{
				var site = BXSite.GetById(WizardContext.State["Installer.SiteId"]);
				if (site == null)
					WizardContext.State.Remove("Installer.SkipSite");
				else
					return Result.Next();
			}

			var nav = WizardContext.Navigation;
			if (nav["site"] == null)
			{
				nav.Insert(
					0,
					new BXWizardNavigationStep("site")
					{
						TitleHtml = GetMessage("Navigation.Site")
					}
				);
			}
			nav.Selected = "site";
			UI.Load("Site");
			BXParamsBag<object> data = WizardContext.State.Get<BXParamsBag<object>>("Site");
			if (data == null)
				WizardContext.State["Site"] = data = new BXParamsBag<object>();

			if(data.GetString("SiteMode", "new") == "new")
			{
				string siteId = string.Empty;
				Random r = new Random(DateTime.Now.Millisecond);
				/*
				 * Строим id сайта site_ + 2 символа латиского алфавита
				 * Возможных сочетаний по 2 из 26 символов: 676
				 */
				for(int i = 1; i <= 676; i++)
				{
					string s = string.Concat("site_", (char)r.Next(97, 123), (char)r.Next(97, 123));
					if(BXSite.GetById(s) != null || Directory.Exists(HostingEnvironment.MapPath(string.Concat("~/", s, "/"))))
						continue;

					siteId = s;
					break;
				}

				if(!string.IsNullOrEmpty(siteId))
				{
					UI.Data["NewSiteId"] = siteId;
					UI.Data["NewSiteFolder"] = string.Concat("/", siteId, "/");
					UI.Data["NewSiteName"] = GetMessage("DefaultSiteName");
				}
			}
			return PrepareView();
		}

		protected override BXWizardResult OnActionNext(BXCommonBag parameters)
		{
			UI.LoadValues(parameters);

			if (UI.Data.GetString("SiteMode") == "existing")
			{
				var exSiteId = UI.Data.GetString("ExistingSiteId");
				BXSite site;
				if (string.IsNullOrEmpty(exSiteId) || (site = BXSite.GetById(exSiteId, BXTextEncoder.EmptyTextEncoder)) == null)
					return PrepareView(string.Format(GetMessage("Error.SiteDoesntExist"), exSiteId));
				WizardContext.State["Installer.SiteId"] = site.Id;

				UI.Overwrite("Site");
				return Result.Action("validate_site", "", null);
			}


			List<string> errors = new List<string>();
			var siteId = UI.Data.GetString("NewSiteId");
			if (BXStringUtility.IsNullOrTrimEmpty(siteId))
				errors.Add(GetMessage("Error.SiteIdRequired"));
			else
			{
				siteId = siteId.Trim();
				if (!Regex.IsMatch(siteId, @"^[a-z0-9_]+$", RegexOptions.IgnoreCase))
					errors.Add(GetMessage("Error.InvalidSiteId"));
				else if (BXSite.GetById(siteId) != null)
					errors.Add(string.Format(GetMessage("Error.SiteExists"), siteId));
			}

			var siteName = UI.Data.GetString("NewSiteName");
			if (BXStringUtility.IsNullOrTrimEmpty(siteName))
				errors.Add(GetMessage("Error.SiteNameRequired"));
			else
				siteName = siteName.Trim();

			var siteFolder = UI.Data.GetString("NewSiteFolder") ?? "";
			siteFolder = Regex.Replace(siteFolder.Trim(), @"[\\/]+", "/").Trim('/');
			if (string.IsNullOrEmpty(siteFolder))
				errors.Add(GetMessage("Error.SiteFolderRequired"));
			else if (!Regex.IsMatch(siteFolder, Bitrix.IO.BXPath.PathValidationRegexString))
				errors.Add(GetMessage("Error.InvalidSiteFolder"));

			var langId = UI.Data.GetString("NewSiteLang");
			BXLanguage lang;
			if (string.IsNullOrEmpty(langId) || (lang = BXLanguage.GetById(langId, BXTextEncoder.EmptyTextEncoder)) == null)
				return PrepareView(string.Format(GetMessage("Error.InvalidLanguage"), siteId));


			if (errors.Count != 0)
				return PrepareView(errors.ToArray());

			try
			{
				string dir = HostingEnvironment.MapPath(Bitrix.IO.BXPath.Combine("~/", siteFolder));
				if (!Directory.Exists(dir))
				{
					try
					{
						Directory.CreateDirectory(dir);
					}
					catch
					{
						return PrepareView(GetMessage("Error.UnableToCreateSiteFolder"));
					}
				}
			}
			catch
			{
			}


			try
			{
				BXSite site = new BXSite(BXTextEncoder.EmptyTextEncoder);
				site.Id = siteId;
				site.LanguageId = lang.Id;
				site.Culture = lang.Culture;
				site.Name = siteName;
				site.SiteName = siteName;
				site.Default = false;
				site.Active = true;
				site.Sort = 100;
				site.Directory = siteFolder;
				site.DocRoot = "";
				site.ServerName = "";
				site.DomainLimited = false;
				site.Save();

				try
				{
					string dir = HostingEnvironment.MapPath(site.DirectoryVirtualPath);
					if (!Directory.Exists(dir))
						Directory.CreateDirectory(dir);
				}
				catch
				{
				}
			}
			catch
			{
				return PrepareView(GetMessage("Error.UnableToCreateSite"));
			}

			WizardContext.State["Installer.SiteId"] = siteId;
			UI.Data["SiteMode"] = "existing";
			UI.Data["ExistingSiteId"] = siteId;


			UI.Overwrite("Site");
			return Result.Action("validate_site", "", null);
		}

		private BXWizardResult PrepareView(params string[] errors)
		{
			BXWizardResultView view = Result.Render(GetMessage("Title"), errors);
			view.Buttons.Add("next", null);
			return view;
		}
	}
}