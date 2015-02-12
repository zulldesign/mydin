<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.Wizards.BXWizardStepStandardHtmlControl" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.Install" %>
<%@ Import Namespace="Bitrix.IO" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="Bitrix.UI.Wizards" %>

<script runat="server">
	const string LinkPrefix = "site";
	static readonly KeyValuePair<string, string>[] MenuTypes = 
	{
		new KeyValuePair<string, string>("top", "Top menu"),
		new KeyValuePair<string, string>("left", "Left menu"),
		new KeyValuePair<string, string>("bottom", "Bottom menu"),
	};

	protected override BXWizardResult OnActionShow(Bitrix.DataTypes.BXCommonBag parameters)
	{
		var view = Result.Render("Solution Installation");
		view.AutoRedirect = true;
		view.RedirectAction = "next";
		return view;
	}

	protected override BXWizardResult OnActionNext(Bitrix.DataTypes.BXCommonBag parameters)
	{
		var siteId = WizardContext.State.GetString("Installer.SiteId");
		var site = BXSite.GetById(siteId, BXTextEncoder.EmptyTextEncoder);
		var menuItems = BXPublicMenu.GetMenuTypes(siteId, true);
		foreach(var p in MenuTypes)
		{
			if (!menuItems.ContainsKey(p.Key))
				menuItems[p.Key] = p.Value;
		}
		BXPublicMenu.SetMenuTypes(siteId, menuItems);

		var solutionPath = WizardContext.State.GetString("Installer.SolutionPath");
		var rootDir = BXPath.Combine(solutionPath, "public");
		BXInstallHelper.ProcessFiles(
			rootDir,
			delegate(FileInfo file, string rel)
			{

				BXPublicMenuItemCollection items;
				using (var stream = file.OpenRead())
				using (var reader = System.Xml.XmlReader.Create(stream))
					items = BXPublicMenu.LoadXml(reader);

				foreach (var item in items)
				{
					for (int i = item.Links.Count - 1; i >= 0; i--)
					{
						var link = item.Links[i];
						if (!link.StartsWith(LinkPrefix, StringComparison.OrdinalIgnoreCase))
							continue;

						if (link.Length == LinkPrefix.Length)
							link = "";
						else
						{
							char c = link[LinkPrefix.Length];
							if (c != '/' && c != '\\')
								continue;

							if (link.Length == LinkPrefix.Length + 1)
								link = "";
							else
								link = link.Substring(LinkPrefix.Length).Replace('\\', '/');
						}
						item.Links[i] = site.UrlVirtualPath + link;
					}
				}

				var settings = new System.Xml.XmlWriterSettings();
				settings.Encoding = Encoding.UTF8;
				settings.Indent = true;
				settings.IndentChars = "\t";
				using (var writer = System.Xml.XmlWriter.Create(BXPath.MapPath(site.DirectoryVirtualPath + rel), settings))
				{
					writer.WriteStartDocument();
					BXPublicMenu.SaveXml(writer, items);
				}
			},
			new[]
			{
				new BXInstallHelperFileRule(),
				new BXInstallHelperFileRule { Regex = @"\.menu$", Include = true }
			}
		);
		
		BXPublicMenu.Menu.RefreshSettings();
		UI.SetProgressBarValue("Installer.ProgressBar", "Bitrix.CorporateSite", 4);
		return Result.Next();
	}
	
	
</script>
Configure Menu
<% UI.ProgressBar("Installer.ProgressBar"); %>