using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Bitrix;

using Bitrix.Modules;
using Bitrix.Services;
using Bitrix.UI;
using Bitrix.Security;
using Bitrix.IO;
using Bitrix.Configuration;
using System.Threading;

public partial class bitrix_admin_TemplateEdit : Bitrix.UI.BXAdminPage
{
	private string templateLangFile = "~/bitrix/templates/{0}/lang/{1}/include.lang";
	string templatesPath = BXConfigurationUtility.Constants.TemplatesFolderPath;
	string tempId;
	private bool currentUserCanModify = false;
	bool fatalError;

	public string TemplateId
	{
		get
		{
			if (tempId == null && Request.QueryString["id"] != null)
			{
				string id = Request.QueryString["id"].Trim().ToLowerInvariant();
				if (!string.IsNullOrEmpty(id)
					&& id.IndexOfAny(Path.GetInvalidFileNameChars()) == -1
					&& BXSecureIO.DirectoryExists(BXPath.Combine(templatesPath, id)))
					tempId = id;
			}
			return tempId;
		}
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsView))
			BXAuthentication.AuthenticationRequired();

		currentUserCanModify = this.BXUser.IsCanOperate(BXRoleOperation.Operations.ProductSettingsManage);
		//TemplateEditor.TemplateId = Request.QueryString["id"];

		AddTemplateSeparator.Visible = AddTemplateButton.Visible =
			CopyTemplateSeparator.Visible = CopyTemplateButton.Visible =
			DeleteTemplateSeparator.Visible = DeleteTemplateButton.Visible = !string.IsNullOrEmpty(TemplateId) && currentUserCanModify;

		BXTabControl1.ShowSaveButton = BXTabControl1.ShowApplyButton = currentUserCanModify;
	}
	protected void Page_Load(object sender, EventArgs e)
	{
		if (IsPostBack)
			return;

		try
		{
			LoadData();
		}
		catch(PublicException ex)
		{
			ErrorMessage.Visible = true;
			ErrorMessage.Content += Encode(ex.Message);
			BXLogService.LogAll(ex.InnerException, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
			fatalError = true;
		}
		catch(Exception ex)
		{
			ErrorMessage.Visible = true;
			ErrorMessage.Content += GetMessage("Error.LoadUnknown");
			BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
			fatalError = true;
		}
	}
	protected void Page_LoadComplete(object sender, EventArgs e)
	{
		MasterTitle = GetMessage("PageTitle");
		if (fatalError)
		{
			BXTabControl1.Visible = false;
			ContextMenuToolbar.Visible = false;
			return;
		}
		
		SiteStyle.Title = string.Format(GetMessageRaw("Tabs.SiteStyle.Title"), BXConfigurationUtility.Constants.SiteStyleFileName);
		TemplateStyle.Title = string.Format(GetMessageRaw("Tabs.TemplateStyle.Title"), BXConfigurationUtility.Constants.TemplateStyleFileName);
		//BXPage.Scripts.RequireUtils();
	}

	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);
		BXPage.RegisterScriptInclude("~/bitrix/js/main/utils_net.js");

		ScriptManager sm = ScriptManager.GetCurrent(Page);
		if (sm != null)
		{
			string dispatcherScriptVirtualPath = "~/bitrix/js/main/BXAspnetFormDispatcher.js";
			string dispatcherScriptUrl = string.Concat(VirtualPathUtility.ToAbsolute(dispatcherScriptVirtualPath), "?t=", HttpUtility.UrlEncode(System.IO.File.GetLastWriteTimeUtc(System.Web.Hosting.HostingEnvironment.MapPath(dispatcherScriptVirtualPath)).Ticks.ToString()));

			bool isFound = false;
			foreach (ScriptReference scripRef in sm.Scripts)
			{
				if (!string.Equals(scripRef.Path, dispatcherScriptUrl, StringComparison.CurrentCultureIgnoreCase))
					continue;
				isFound = true;
				break;
			}
			if (!isFound)
				sm.Scripts.Add(new ScriptReference(dispatcherScriptUrl));

		}
		else
			BXPage.RegisterScriptInclude("~/bitrix/js/main/BXAspnetFormDispatcher.js");
	}
	
	private void LoadData()
	{
		try
		{
            string tmplId = TemplateId;
			//ID
			if (!string.IsNullOrEmpty(tmplId))
			{
				string templatePath = BXPath.Combine(BXConfigurationUtility.Constants.TemplatesFolderPath, tmplId);
				hlPath.Visible = true;
				hlPath.Text = Encode(templatePath);
				hlPath.NavigateUrl = string.Format("FileMan.aspx?path={0}", HttpUtility.UrlEncode(templatePath));
				lbID.Visible = true;
				lbID.Text = tmplId;
				txtID.Visible = false;
				Left.Visible = true;
				Right.Visible = true;
			}
			else
			{
				hlPath.Visible = false;
				lbID.Visible = false;
				txtID.Visible = true;
				Left.Visible = false;
				Right.Visible = false;
			}

			//NAME
			BXResourceFile res = new BXResourceFile(MapPath(string.Format(templateLangFile, tmplId, BXLoc.CurrentLocale)));


			if (res.ContainsKey("Template.Name"))
				Name.Text = res["Template.Name"];

			if (res.ContainsKey("Template.Description"))
				Description.Text = res["Template.Description"];

			string masterFile = BXPath.Combine(BXConfigurationUtility.Constants.TemplatesFolderPath, tmplId, BXConfigurationUtility.Constants.TemplateFileName);
			if (BXSecureIO.FileExists(masterFile))
				TemplateEditor.Content = BXSecureIO.FileReadAllText(masterFile);
            else if (string.IsNullOrEmpty(tmplId))
            {
                TemplateEditor.Content = Bitrix.Services.Text.BXDefaultFiles.BuildMaster();
            }

			TemplateEditor.TemplateId = Request.QueryString["id"];

			string siteStyleFile = BXPath.Combine(BXConfigurationUtility.Constants.TemplatesFolderPath, TemplateId, BXConfigurationUtility.Constants.SiteStyleFileName);
			if (BXSecureIO.FileExists(siteStyleFile))
				txtSiteStyle.Text = BXSecureIO.FileReadAllText(siteStyleFile);

			string templateStyleFile = BXPath.Combine(BXConfigurationUtility.Constants.TemplatesFolderPath, TemplateId, BXConfigurationUtility.Constants.TemplateStyleFileName);
			if (BXSecureIO.FileExists(templateStyleFile))
				txtTemplateStyle.Text = BXSecureIO.FileReadAllText(templateStyleFile);

			//FILES
			FileInfo[] allFiles = new DirectoryInfo(BXPath.ToPhysicalPath(BXPath.Combine(BXConfigurationUtility.Constants.TemplatesFolderPath, TemplateId))).GetFiles();
			List<FileInfo> filteredFiles = new List<FileInfo>();
			foreach (FileInfo file in allFiles)
			{
				if (!file.Name.Equals(BXConfigurationUtility.Constants.SiteStyleFileName, StringComparison.CurrentCultureIgnoreCase)
					&& !file.Name.Equals(BXConfigurationUtility.Constants.TemplateFileName, StringComparison.CurrentCultureIgnoreCase)
					&& !file.Name.Equals(BXConfigurationUtility.Constants.TemplateStyleFileName, StringComparison.CurrentCultureIgnoreCase))
					filteredFiles.Add(file);
			}
			repFiles.DataSource = filteredFiles;
			repFiles.DataBind();
		}
		catch (BXSecurityException ex)
		{
			throw new PublicException(ex);
		}
	}
	protected void ValidateSave(out string templateId)
	{
		ErrorMessage.Visible = false;
		ErrorMessage.Content = string.Empty;

		if (!currentUserCanModify)
			throw new PublicException(GetMessageRaw("Error.InsifficientRights"));

		//ID
		if (TemplateId == null)
			if (txtID.Text == null || txtID.Text.Trim().Length == 0)
				throw new PublicException(GetMessageRaw("Error.TemplateIdMissing"));

		templateId = string.IsNullOrEmpty(TemplateId) ? txtID.Text.Trim() : TemplateId;
		if (templateId.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
			throw new PublicException(GetMessageRaw("Error.TemplateIdContainsInvalidSymbols"));


		//Template Content
		if (TemplateEditor.Content == null || TemplateEditor.Content.Trim().Length == 0)
			throw new PublicException(GetMessageRaw("Error"));
	}
	private void DoSave(out string templateId)
	{
		try
		{
			ValidateSave(out templateId);

			string templateDirectory = BXPath.Combine(templatesPath, templateId);

			BXSecureIO.DirectoryEnsureExists(templateDirectory);

			BXSecureIO.FileWriteAllText(BXPath.Combine(templateDirectory, BXConfigurationUtility.Constants.TemplateFileName), TemplateEditor.Content);
			BXSecureIO.FileWriteAllText(BXPath.Combine(templateDirectory, BXConfigurationUtility.Constants.SiteStyleFileName), txtSiteStyle.Text);
			BXSecureIO.FileWriteAllText(BXPath.Combine(templateDirectory, BXConfigurationUtility.Constants.TemplateStyleFileName), txtTemplateStyle.Text);


			string langDirectory = BXPath.Combine(templateDirectory, "lang", BXLoc.CurrentLocale);
			BXSecureIO.DirectoryEnsureExists(langDirectory);

			BXResourceFile res = new BXResourceFile(BXPath.ToPhysicalPath(string.Format(templateLangFile, templateId, BXLoc.CurrentLocale)));

			if (res.ContainsKey("Template.Name"))
				res["Template.Name"] = Name.Text;
			else
				res.Add("Template.Name", Name.Text);

			if (res.ContainsKey("Template.Description"))
				res["Template.Description"] = Description.Text;
			else
				res.Add("Template.Description", Description.Text);

			res.Save();
		}
		catch (BXSecurityException ex)
		{
			throw new PublicException(ex);
		}
	}

	protected void BXTabControl1_Command(object sender, BXTabControlCommandEventArgs e)
	{
		if (fatalError)
			return;
		bool needRedirect = false;
		switch (e.CommandName)
		{
			case "apply":
			case "save":
				try
				{
					bool save = e.CommandName.Equals("save");
					string templateId;
					DoSave(out templateId);
					if (!save)
						Response.Redirect("TemplateEdit.aspx?id=" + templateId);
					needRedirect = true;
				}
				catch (ThreadAbortException)
				{
				}
				catch (PublicException ex)
				{
					ErrorMessage.Visible = true;
					ErrorMessage.Content += Encode(ex.Message);
				}
				catch (Exception ex)
				{
					ErrorMessage.Visible = true;
					ErrorMessage.Content += GetMessage("Error.SaveUnknown");
					BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
				}
				break;
			case "cancel":
				needRedirect = true;
				break;
			default:
				break;
		}
		if (needRedirect)
		{
			string backUrl = Request.QueryString[BXConfigurationUtility.Constants.BackUrl];
			Response.Redirect(!string.IsNullOrEmpty(backUrl) ? backUrl : "~/bitrix/admin/Template.aspx");
		}
	}
	protected void ContextMenuToolbar_CommandClick(object sender, CommandEventArgs e)
	{
		if (fatalError)
			return;
		try
		{
			switch (e.CommandName.ToLower())
			{
				case "delete":
					BXUser.DemandOperations(BXRoleOperation.Operations.ProductSettingsManage);
					if (TemplateId != null)
					{
						string templateDirectory = BXPath.Combine(BXConfigurationUtility.Constants.TemplatesFolderPath, TemplateId);
						BXSecureIO.DirectoryDelete(templateDirectory, true);
						Response.Redirect("Template.aspx");
					}
					break;
				case "copy":
					BXUser.DemandOperations(BXRoleOperation.Operations.ProductSettingsManage);
					if (TemplateId != null)
					{
						string sourceDirectory = BXPath.Combine(BXConfigurationUtility.Constants.TemplatesFolderPath, TemplateId);
						string suffix = null;
						string targetDirectory = null;
						do
						{
							suffix += "_copy";
							targetDirectory = BXPath.Combine(BXConfigurationUtility.Constants.TemplatesFolderPath, TemplateId + suffix);
						}
						while (BXSecureIO.FileOrDirectoryExists(targetDirectory));
						BXSecureIO.DirectoryCopy(sourceDirectory, targetDirectory);
						Response.Redirect("TemplateEdit.aspx?id=" + UrlEncode(TemplateId + suffix));
					}
					break;
				default:
					break;
			}
		}
		catch (BXSecurityException ex)
		{
			ErrorMessage.Visible = true;
			ErrorMessage.Content += Encode(ex.Message);
			BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
		}
		catch (Exception ex)
		{
			ErrorMessage.Visible = true;
			ErrorMessage.Content += GetMessage("Error.LoadUnknown");
			BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
		}
	}
	protected void TemplateEditor_IncludeWebEditorScript(object sender, BXWebEditor.IncludeWebEditorScriptArgs e)
	{
		e.Writer.WriteLine("<script>");
		e.Writer.WriteLine("TE_MESS = {}");
		e.Writer.WriteLine("TE_MESS.CPH_ID = \"" + GetMessageJS("CPH_ID") + "\";");
		e.Writer.WriteLine("TE_MESS.INSERT_CPH = \"" + GetMessageJS("InsertCPH") + "\";");
		e.Writer.WriteLine("TE_MESS.templateToolbar = \"" + GetMessageJS("PageTitle") + "\";");
		e.Writer.WriteLine("TE_MESS.FILEMAN_EDIT_HBF = \"" + GetMessageJS("EditHBF") + "\";");

		//e.Writer.WriteLine("window.DotNetTemplate = true;");
		e.Writer.WriteLine("window.fullEditMode = true;");
		e.Writer.WriteLine("BXContentType = 'MasterPage';");
		e.Writer.WriteEndTag("script");

		//zg, 2008.06.03
		//string v = Bitrix.IO.BXFile.GetFileTimestamp(BXPath.MapPath("~/bitrix/ui/editor/js/TemplateEdit_editor.js")).ToString();
		//e.Writer.WriteLine("<script type=\"text/javascript\" src=\"" + VirtualPathUtility.ToAbsolute("~/bitrix/ui/editor/js/TemplateEdit_editor.js") + "?v=" + v + "\"></script>");
		string v = Bitrix.IO.BXFile.GetFileTimestamp(BXPath.MapPath("~/bitrix/controls/main/editor/js/TemplateEdit_editor.js")).ToString();
		e.Writer.WriteLine("<script type=\"text/javascript\" src=\"" + VirtualPathUtility.ToAbsolute("~/bitrix/controls/main/editor/js/TemplateEdit_editor.js") + "?v=" + v + "\"></script>");

	}
}
