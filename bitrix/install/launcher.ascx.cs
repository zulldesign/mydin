using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Xml;
using System.Reflection;
using System.IO;
using System.Text;
using System.Web;

namespace Bitrix.Installer
{
	public partial class InstallLauncher : UserControl
	{
		private Dictionary<string, string> phrases;

		private string locale;
		public string Locale
		{
			get
			{
				return locale;
			}
			set
			{
				locale = value;
			}
		}

		private string url;
		public string Url
		{
			get
			{
				return url ?? "";
			}
			set
			{
				url = value;
			}
		}

		private string finishUrl;
		public string FinishUrl
		{
			get
			{
				return finishUrl ?? "";
			}
			set
			{
				finishUrl = value;
			}
		}

		protected List<string> errors;
		
		protected Dictionary<string, string> Phrases
		{
			get
			{
				if (phrases == null)
				{
					phrases = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
					LoadPhrases("~/bitrix/install/lang/" + Locale + "/installer.ascx.lang");
				}
				return phrases;
			}
		}

		private void LoadPhrases(string virtualPath)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(MapPath(virtualPath));
			foreach(XmlElement data in doc.DocumentElement.SelectNodes("data"))
				phrases[data.Attributes["name"].Value] = data.InnerText;
		}
	
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			if (Request.Form["launch"] == null)
				return;

			errors = new List<string>();
			try
			{
				Assembly.Load("System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
			}
			catch
			{
				errors.Add(Phrases["Launcher.Problem.Framework"]);
			}

			string path = MapPath("~/installer.session");
			try
			{
				if (File.Exists(path))
					File.Delete(path);
			}
			catch
			{
				errors.Add(Phrases["Launcher.Problem.FileAccessDelete"]);
				return;
			}

			try
			{
				File.WriteAllText(path, "bitrix", Encoding.UTF8);
			}
			catch
			{
				errors.Add(Phrases["Launcher.Problem.FileAccessCreate"]);
				return;
			}

			try
			{
				if (File.Exists(path) && File.ReadAllText(path, Encoding.UTF8) != "bitrix")
					throw new Exception("File content is invalid");
			}
			catch
			{
				errors.Add(Phrases["Launcher.Problem.FileAccessRead"]);
			}

			try
			{
				if (File.Exists(path))
					File.Delete(path);
			}
			catch
			{
				errors.Add(Phrases["Launcher.Problem.FileAccessDelete"]);
			}

			path = MapPath("~/web.config");
			try
			{
				if (File.Exists(path))
					File.WriteAllText(path, File.ReadAllText(path, Encoding.UTF8), Encoding.UTF8);
			}
			catch
			{
				errors.Add(Phrases["Launcher.Problem.ConfigWrite"]);
			}

			if (errors.Count > 0)
				return;

			try
			{
				string bin = MapPath("~/bin");
				Directory.CreateDirectory(bin);
				foreach (FileInfo file in new DirectoryInfo(MapPath("~/bitrix/install/updater/bin")).GetFiles())
					File.Copy(file.FullName, Path.Combine(bin, file.Name), true);
				foreach (FileInfo file in new DirectoryInfo(MapPath("~/bitrix/modules/Main/install/bin")).GetFiles())
					File.Copy(file.FullName, Path.Combine(bin, file.Name), true);
				
				XmlDocument doc = new XmlDocument();
				doc.Load(MapPath("~/web.config"));
				XmlElement root = doc.DocumentElement;
				
				{
					XmlElement node = EnsureElement(root, "system.web", null);
					node = EnsureElement(node, "httpModules", null);
					node = EnsureElement(node, "add", "[@name='BXInstallerHttpModule']");
					EnsureAttribute(node, "name").Value = "BXInstallerHttpModule";
					EnsureAttribute(node, "type").Value = "Bitrix.Install.BXInstallerHttpModule";
				}

				{
					XmlElement webServer = EnsureElement(root, "system.webServer", null);
					XmlElement node = EnsureElement(webServer, "validation", null);
					EnsureAttribute(node, "validateIntegratedModeConfiguration").Value = "false";

					node = EnsureElement(webServer, "handlers", null);
					node = EnsureElement(node, "add", "[@name='All To ASP.NET']");
					EnsureAttribute(node, "name").Value = "All To ASP.NET";
					EnsureAttribute(node, "path").Value = "*";
					EnsureAttribute(node, "verb").Value = "*";
					EnsureAttribute(node, "type").Value = "";
					EnsureAttribute(node, "modules").Value = "IsapiModule";
					EnsureAttribute(node, "scriptProcessor").Value = string.Format(
						@"%windir%\Microsoft.NET\Framework\{0}\aspnet_isapi.dll",
						Environment.Version.Major < 4 ? "v2.0.50727" : "v4.0.30319"
					);
					EnsureAttribute(node, "resourceType").Value = "Unspecified";
					EnsureAttribute(node, "requireAccess").Value = "None";
					EnsureAttribute(node, "preCondition").Value = "classicMode";

					XmlElement modules = EnsureElement(webServer, "modules", null);
					node = EnsureElement(modules, "add", "[@name='BXInstallerHttpModule']");
					EnsureAttribute(node, "name").Value = "BXInstallerHttpModule";
					EnsureAttribute(node, "type").Value = "Bitrix.Install.BXInstallerHttpModule";
					node = EnsureElement(modules, "add", "[@name='BXInstallerIMCheckerHttpModule']");
					EnsureAttribute(node, "name").Value = "BXInstallerIMCheckerHttpModule";
					EnsureAttribute(node, "type").Value = "Bitrix.Install.BXInstallerIMCheckerHttpModule";
					EnsureAttribute(node, "preCondition").Value = "integratedMode";
				}
				doc.Save(MapPath("~/web.config"));

				File.WriteAllText(
					MapPath(Page.AppRelativeVirtualPath), 
					string.Format(
@"<%@ Page Language=""C#"" AutoEventWireup=""true"" Inherits=""System.Web.UI.Page"" %>
<%@ Reference VirtualPath=""~/bitrix/install/wizardhost.ascx"" %>
<%@ Register Src=""~/bitrix/install/installer.ascx"" TagName=""DefaultInstaller"" TagPrefix=""installer"" %>
<installer:DefaultInstaller ID=""Installer"" runat=""server"" Locale=""{0}"" Url=""{1}"" WizardPath=""~/bitrix/install/wizard"" OnFinish=""Installer_Finish"" OnInitState=""Installer_InitState"" />
<script runat=""server"">
	protected override void OnPreRender(EventArgs e)
	{{
		base.OnPreRender(e);
		
		Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
		Response.Cache.SetValidUntilExpires(false); 
		Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
		Response.Cache.SetCacheability(HttpCacheability.NoCache);
		Response.Cache.SetNoStore();
	}}
	void Installer_InitState(object sender, Bitrix.Installer.WizardHostInitStateEventArgs e)
	{{
		string configPath = System.Web.Hosting.HostingEnvironment.MapPath(""{4}install.config"");
		if (System.IO.File.Exists(configPath))
		{{
			try
			{{
				System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
				doc.Load(configPath);
				foreach (System.Xml.XmlNode option in doc.DocumentElement)
					e.State[""Options."" + option.Name] = option.InnerText;
			}}
			catch
			{{
			}}
		}}
	}}
	void Installer_Finish(object sender, Bitrix.Installer.WizardHostFinishEventArgs e)
	{{
 		e.CancelDefaultFinish = true;
		
		string configPath = System.Web.Hosting.HostingEnvironment.MapPath(""{4}install.config"");
		if (System.IO.File.Exists(configPath))
			System.IO.File.Delete(configPath);
			
		string editionPath = System.Web.Hosting.HostingEnvironment.MapPath(""{4}edition"");
		if (System.IO.File.Exists(editionPath))
			System.IO.File.Delete(editionPath);
		
		System.IO.File.WriteAllText(
			System.Web.Hosting.HostingEnvironment.MapPath(""{2}""), 
			string.Format(
@""<%@ Page Language=""""C#"""" AutoEventWireup=""""true"""" Inherits=""""System.Web.UI.Page"""" %>
<%@ Reference VirtualPath=""""~/bitrix/install/wizardhost.ascx"""" %>
<%@ Register Src=""""~/bitrix/install/installer.ascx"""" TagName=""""DefaultInstaller"""" TagPrefix=""""installer""""  %>
<installer:DefaultInstaller ID=""""Installer"""" runat=""""server"""" Locale=""""{0}"""" Url=""""{1}"""" WizardPath=""""~/bitrix/modules/Main/solution_wizard"""" OnFinish=""""Installer_Finish"""" OnInitState=""""Installer_InitState"""" />
<script runat=""""server"""">
	protected override void OnPreRender(EventArgs e)
	{{{{
		base.OnPreRender(e);

		Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
		Response.Cache.SetValidUntilExpires(false); 
		Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
		Response.Cache.SetCacheability(HttpCacheability.NoCache);
		Response.Cache.SetNoStore();
	}}}}
	void Installer_InitState(object sender, Bitrix.Installer.WizardHostInitStateEventArgs e)
	{{{{
		e.State[""""Installer.SiteId""""] = """"{{0}}"""";
		e.State[""""Installer.UserId""""] = {{1}};
		e.State[""""Installer.SkipSite""""] = true;
	}}}}
	void Installer_Finish(object sender, Bitrix.Installer.WizardHostFinishEventArgs e)
	{{{{	
		e.CancelDefaultFinish = true;
		System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(Page.AppRelativeVirtualPath));


		string returnUrl = e.State.GetString(Bitrix.Configuration.BXConfigurationUtility.Constants.BackUrl);
		if (string.IsNullOrEmpty(returnUrl))
			returnUrl = """"{3}"""";
		
		Response.Redirect(returnUrl);
	}}}}
</""+""script>"",
				e.State.GetString(""Install.SiteId"", ""default""),
				e.State.GetInt(""Install.UserId"", 1).ToString()
			),
			Encoding.UTF8
		);

		System.IO.File.WriteAllText(
			System.Web.Hosting.HostingEnvironment.MapPath(""~/default.aspx""),
			Bitrix.Services.Text.BXDefaultFiles.BuildAspx(
				Bitrix.Services.BXLoc.GetMessage(""{0}"", ""~/bitrix/install/installer.ascx"", ""DefaultPageTitle""),
				""~/default.aspx"",
				"".default"",
				string.Format(Bitrix.Services.BXLoc.GetMessage(""{0}"", ""~/bitrix/install/installer.ascx"", ""DefaultPageContent""), VirtualPathUtility.ToAbsolute(""~/""))
			),
			Encoding.UTF8
		);

		Response.Redirect(""{1}"");
	}}
</script>",
						Locale,
						Url,
						Page.AppRelativeVirtualPath,
						FinishUrl,
						Page.AppRelativeTemplateSourceDirectory
					),
					Encoding.UTF8
				);
			}
			catch(Exception ex)
			{
				errors.Add(string.Format(Phrases["Launcher.Problem.Exception"], HttpUtility.HtmlEncode(ex.ToString())));
				return;
				
			}
			Response.Redirect(Url);
		}

		XmlElement EnsureElement(XmlElement parent, string name, string condition)
		{
			return (XmlElement)(parent.SelectSingleNode(name + condition) ?? parent.AppendChild(parent.OwnerDocument.CreateElement(name)));
		}

		XmlAttribute EnsureAttribute(XmlElement parent, string name)
		{
			return parent.Attributes[name] ?? parent.Attributes.Append(parent.OwnerDocument.CreateAttribute(name));
		}
	}
}