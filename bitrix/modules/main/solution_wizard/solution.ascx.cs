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

namespace Bitrix.Wizards.Solutions
{
	public partial class SolutionWizardStep : BXWizardStepStandardHtmlControl
	{
		protected List<SolutionInfo> solutions;
		protected string selected;
		private static readonly string[] steps = {"site", "solution", "finish", "install"};

		protected override void OnWizardInit()
		{
			string sol;
			if (!WizardContext.State.TryGetString("Installer.Solution", out sol))
				return;
			WizardContext.State["Installer.SkipSolution"] = true;
			LoadSolutions();
			var solution = solutions.Find(x => x.Id == sol);
			if (solution != null)
			{
				WizardContext.State["Installer.Solution"] = solution.Id;
				WizardContext.State["Installer.SolutionPath"] = solution.Path;
			}
			else 
				WizardContext.State.Remove("Installer.Solution");

			var n = WizardContext.Navigation["install"];
			if (n != null)
				n.TitleHtml = GetMessage("Navigation.SetupSolutions");
			n = WizardContext.Navigation["finish"];
			if (n != null)
				n.TitleHtml = GetMessage("Navigation.SetupFinalization");
		}

		protected override BXWizardResult OnActionShow(BXCommonBag parameters)
		{
			if (WizardContext.State.ContainsKey("Installer.Solution.GoBack"))
			{
				WizardContext.State.Remove("Installer.Solution.GoBack");
				return Result.Previous();
			}
			
			if (WizardContext.State.GetBool("Installer.SkipSolution"))
			{
				WizardContext.Navigation.Remove(WizardContext.Navigation["solution"]);
				

				WizardContext.State["Installer.Solution.GoBack"] = "";
				return 
					string.IsNullOrEmpty(WizardContext.State.GetString("Installer.Solution"))
					? Result.Last()
					: Result.Next();
			}
			
			int count = WizardContext.Navigation.Count;
			for (int i = count - 1; i >= 0; i--)
			{
				var nav = WizardContext.Navigation[i];
				if (Array.IndexOf(steps, nav.Id) == -1)
					WizardContext.Navigation.RemoveAt(i);
			}
			WizardContext.Navigation.Selected = "solution";

			return PrepareView();
		}

		protected override BXWizardResult OnActionNext(BXCommonBag parameters)
		{
			var selected = HttpContext.Current.Request.Form["selected-solution"];
			if (selected == null)
				return PrepareView(GetMessage("Error.SelectSolution"));
			if (selected == "")
				return Result.Last();//Action("default", "", null);
			LoadSolutions();
			var solution = solutions.Find(x => x.Id == selected);
			if (solution == null)
				return PrepareView(GetMessage("Error.InvalidSolution"));
			WizardContext.State["Installer.Solution"] = solution.Id;
			WizardContext.State["Installer.SolutionPath"] = solution.Path;

			return Result.Next();
		}

		protected override BXWizardResult OnActionPrevious(BXCommonBag parameters)
		{
			return Result.Previous();
		}

		private BXWizardResult PrepareView(params string[] errors)
		{
			LoadSolutions();
			solutions.Sort((a, b) => a.Sort - b.Sort);
			AddEmptySolution();

			object selectedObj;
			if (WizardContext.State.TryGetValue("Installer.Solution", out selectedObj))
				selected = selectedObj.ToString();
			else 
				selected = BXOptionManager.GetOptionString("main", "InstalledSolution", null, WizardContext.State.GetString("Installer.SiteId"));
		
			BXWizardResultView view = Result.Render(GetMessage("Title"), errors);
			if (!WizardContext.State.GetBool("Installer.SkipSite"))
				view.Buttons.Add("prev", null);
			view.Buttons.Add("next", null);
			return view;
		}

		private void LoadSolutions()
		{
			if (solutions != null)
				return;
			solutions = new List<SolutionInfo>();
			foreach (DirectoryInfo dir in new DirectoryInfo(HostingEnvironment.MapPath("~/bitrix/modules/")).GetDirectories())
			{
				string root = string.Format("~/bitrix/modules/{0}/", dir.Name);
				if (!BXModuleManager.IsModuleInstalled(dir.Name))
					continue;
				string file = Path.Combine(dir.FullName, "solutions." + WizardContext.Locale + ".config");
				if (!File.Exists(file))
					continue;

				try
				{
					XmlDocument doc = new XmlDocument();
					doc.Load(file);
					foreach (XmlNode solution in doc.DocumentElement.SelectNodes("solution"))
					{
						SolutionInfo info = new SolutionInfo();
						
						var attr = solution.Attributes["sort"];
						int sort;
						if (attr != null && attr.Value != null && int.TryParse(attr.Value, out sort))
							info.Sort = sort;

						info.Id = solution.Attributes["id"].Value;
						info.Path = solution.Attributes["path"].Value;
						if (string.IsNullOrEmpty(info.Path))
							info.Path = root;
						else if (!info.Path.StartsWith("~/") && info.Path != "~")
							info.Path = Bitrix.IO.BXPath.Combine(root, info.Path);

						XmlNode title = solution.SelectSingleNode("title");
						if (title != null)
							info.TitleHtml = title.InnerText;
						else
							info.TitleHtml = Encode(info.Id);

						XmlNode description = solution.SelectSingleNode("description");
						if (description != null)
							info.DescriptionHtml = description.InnerText;

						XmlNode image = solution.SelectSingleNode("image");
						if (image != null)
						{
							info.ImageUrl = image.InnerText;
							
							if (!string.IsNullOrEmpty(info.ImageUrl))
							{
								if (info.ImageUrl.StartsWith("~/"))
									info.ImageUrl = VirtualPathUtility.ToAbsolute(info.ImageUrl);

								Uri baseUrl;
								if (!Uri.TryCreate(HttpContext.Current.Request.Url, VirtualPathUtility.ToAbsolute(string.Format("~/bitrix/modules/{0}/solutions.{1}.config", dir.Name, WizardContext.Locale)), out baseUrl))
									baseUrl = HttpContext.Current.Request.Url;

								Uri imageUri;
								if (Uri.TryCreate(baseUrl, info.ImageUrl, out imageUri))
									info.ImageUrl = imageUri.AbsoluteUri;
							}
						}

						solutions.Add(info);
					}
				}
				catch
				{
					continue;
				}
			}
		}

		private void AddEmptySolution()
		{

			SolutionInfo info = new SolutionInfo();
			info.Id = "";

			info.TitleHtml = GetMessage("NoSolution.Title");
			info.DescriptionHtml = GetMessage("NoSolution.Description");

			
			solutions.Add(info);

		}

		public class SolutionInfo
		{
			public string Id;
			public string Path;
			public string ImageUrl;
			public string TitleHtml;
			public string DescriptionHtml;
			public string WizardPath;
			public int Sort;
		}
	}
}