using System;
using System.Collections;
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
using Bitrix.Main;
using Bitrix.Services;
using Bitrix.UI;
using Bitrix.Security;
using Bitrix.Services.Js;
using Bitrix.IO;
using Bitrix.Configuration;
using Bitrix.Services.Text;
using System.Collections.Generic;
using System.Threading;

public partial class bitrix_admin_FileManNewFolder : BXAdminPage, IBXFileManPage
{
	private const string DefaultPageName = "Default";

	private string curPath;
	private string curSiteId;
	private StringBuilder _ErrorText = new StringBuilder();
	private int _ShowMessage;
	private Dictionary<string, string> menuOptions;
	private bool fatalError;

	private string backUrl;
	protected override string BackUrl
	{
		get
		{
			backUrl = base.BackUrl;
			if (backUrl == null)
				if (curPath == null)
					backUrl = "FileMan.aspx";
				else
					backUrl = "FileMan.aspx?path=" + HttpUtility.UrlEncode(curPath);
			return backUrl;
		}
	}

	private void RegisterJavaScript()
	{
		string script;

		script = String.Format(
			@"
			function fileMan_CheckDefaultPage(cb)
			{{
				var state = cb.checked;
				var item;
				item = document.getElementById('{0}');
				item.disabled = !state;
				item = document.getElementById('{1}');
				item.disabled = !state;
				item.checked = state;
			}}",
			defaultTemplate.ClientID,
			editDefault.ClientID
		);
		if (!ClientScript.IsClientScriptBlockRegistered(GetType(), "fileMan_CheckDefaultPage"))
			ClientScript.RegisterClientScriptBlock(GetType(), "fileMan_CheckDefaultPage", script, true);

		script = String.Format(
			@"
			function fileMan_CheckCreateMenuItem(cb)
			{{
				var item;
				item = document.getElementById('{0}');
				item.disabled = !cb.checked;
				item = document.getElementById('{1}');
				item.disabled = !cb.checked;
				if (cb.checked)
					item.value = document.getElementById('{2}').value;
			}}",
			MenuType.ClientID,
			MenuItemName.ClientID,
			sectionName.ClientID
		);
		if (!ClientScript.IsClientScriptBlockRegistered(GetType(), "fileMan_CheckCreateMenuItem"))
			ClientScript.RegisterClientScriptBlock(GetType(), "fileMan_CheckCreateMenuItem", script, true);

		script = String.Format(
			@"
			function fileMan_GoBack()
			{{
				location.href='{0}'
			}}",
			BXJSUtility.Encode(BackUrl)
		);

		if (!ClientScript.IsClientScriptBlockRegistered(GetType(), "fileMan_GoBack"))
			ClientScript.RegisterClientScriptBlock(GetType(), "fileMan_GoBack", script, true);
	}
	private void PrepareResultMessage()
	{
		if (_ShowMessage == 0)
		{
			resultMessage.Visible = false;
			return;
		}
		resultMessage.Visible = true;
		resultMessage.CssClass = (_ShowMessage > 0) ? "Ok" : "Error";
		resultMessage.IconClass = (_ShowMessage > 0) ? "Ok" : "Error";
		resultMessage.Title = (_ShowMessage > 0) ? GetMessage("Message.OperationSuccessful") : GetMessage("Message.OperationErrors");
		resultMessage.Content = (_ShowMessage > 0) ? String.Empty : _ErrorText.ToString();
	}
	private void ShowError(string encodedMessage)
	{
		_ShowMessage = -1;
		_ErrorText.AppendLine("<br />");
		_ErrorText.AppendLine(encodedMessage);
	}
	private void ShowOk()
	{
		if (_ShowMessage == 0)
			_ShowMessage = 1;
	}
	private void FatalError(string messageText)
	{
		fatalError = true;
		BXContextMenuToolbar1.Visible = false;
		mainTabControl.Visible = false;
		ShowError(Encode(messageText));
	}
	private void FatalErrorCheck(Exception ex)
	{
		if (ex is ThreadAbortException)
			return;

		PublicException pub = ex as PublicException;
		if (pub != null)
		{
			FatalError(ex.Message);
			if (ex.InnerException != null)
				BXLogService.LogAll(ex.InnerException, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
			return;
		}

		BXSecurityException sec = ex as BXSecurityException;
		if (sec != null)
		{
			FatalError(ex.Message);
			BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
			return;
		}

		FatalError(GetMessageRaw("Error.Unknown"));
		BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
	}
	private void ErrorCheck(Exception ex)
	{
		if (ex is ThreadAbortException)
			return;

		PublicException pub = ex as PublicException;
		if (pub != null)
		{
			ShowError(Encode(ex.Message));
			if (ex.InnerException != null)
				BXLogService.LogAll(ex.InnerException, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
			return;
		}

		BXSecurityException sec = ex as BXSecurityException;
		if (sec != null)
		{
			ShowError(Encode(ex.Message));
			BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
			return;
		}

		ShowError(GetMessage("Error.Unknown"));
		BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
	}

	void DoCreateDefaultFile(string path)
	{
		string aspxPath = BXPath.Combine(path, DefaultPageName + ".aspx");
		string aspxcsPath = BXPath.Combine(path, DefaultPageName + ".aspx.cs");

		try
		{
			BXSecureIO.FileWriteAllText(aspxPath, BXDefaultFiles.BuildAspx(sectionName.Text, aspxPath, defaultTemplate.SelectedValue), BXConfigurationUtility.DefaultEncoding);
		}
		catch (Exception ex)
		{
			throw new PublicException(string.Format("{0}: '{1}'", GetMessageRaw("Message.UnableToCreateDefault"), aspxPath), ex);
		}

		if (!editDefault.Checked)
		{
			if (Request.QueryString["showsaved"] != null)
				Response.Redirect(BXPath.TrimEnd(BXPath.ToVirtualAbsolutePath(path)) + BXPath.AltDirectorySeparatorChar);
			return;
		}

		if (!BXSecureIO.FileExists(aspxPath))
			return;

		StringBuilder url = new StringBuilder();
		url.Append(BXPath.ToVirtualAbsolutePath("~/bitrix/admin/FileManEdit.aspx"));
		url.AppendFormat(
			"?path={0}",
			HttpUtility.UrlEncode(aspxPath)
		);
		if (Request.QueryString[BXConfigurationUtility.Constants.BackUrl] != null)
			url.AppendFormat(
				"&{0}={1}",
				BXConfigurationUtility.Constants.BackUrl,
				HttpUtility.UrlEncode(Request.QueryString[BXConfigurationUtility.Constants.BackUrl])
			);
		if (Request.QueryString["showsaved"] != null)
			url.Append("&showsaved=");

		Response.Redirect(url.ToString());
	}
	void DoCreateMenuItem(string dirPath)
	{
		if (string.IsNullOrEmpty(MenuItemName.Text))
			throw new PublicException(GetMessageRaw("Message.EmptyMenuItemName"));
		if (string.IsNullOrEmpty(MenuType.SelectedValue) || (MenuType.SelectedValue.IndexOfAny(Path.GetInvalidFileNameChars()) != -1))
			throw new PublicException(GetMessageRaw("Message.IllegalMenuType"));
		try
		{
			BXUser.DemandOperations(BXRoleOperation.Operations.FileManage);

			bool isNewMenu = false;
			BXPublicMenuItemCollection menuItemCol = BXPublicMenu.Menu.GetMenuByUri(MenuType.SelectedValue, dirPath);
			if (menuItemCol == null)
			{
				menuItemCol = new BXPublicMenuItemCollection();
				isNewMenu = true;
			}
			BXPublicMenuItem menuItem = new BXPublicMenuItem();
			menuItem.Link = BXSiteRemapUtility.UnmapVirtualPath(VirtualPathUtility.AppendTrailingSlash(BXPath.TrimEnd(dirPath)));
			menuItem.Title = MenuItemName.Text;

			menuItemCol.Add(menuItem);

			int menuItemCount = menuItemCol.Count;
			for (int i = 0; i < menuItemCount; i++)
				menuItemCol[i].Sort = (i + 1) * 10;

			string menuDirPath = dirPath;
			if (!isNewMenu && !string.IsNullOrEmpty(menuItemCol.MenuFilePath))
				menuDirPath = BXPath.GetDirectory(menuItemCol.MenuFilePath);

			BXPublicMenu.Menu.Save(menuDirPath, MenuType.SelectedValue, menuItemCol);
		}
		catch(Exception ex)
		{
			throw new PublicException(GetMessageRaw("Message.UnableToCreateMenu"), ex);
		}
	}
	void DoCreateFolder()
	{
		try
		{
			if (!folderNameValidator.IsValid)
				throw new PublicException(GetMessageRaw("Message.InvalidName"));

			string dirName = folderName.Text.Trim();
			if (String.IsNullOrEmpty(dirName))
				throw new PublicException(GetMessageRaw("Message.InvalidName"));

			string dirPath = BXPath.Combine(curPath, dirName);
			if (BXSecureIO.FileOrDirectoryExists(dirPath))
				throw new PublicException(GetMessageRaw("Message.ItemExists"));

			try
			{
				BXSecureIO.DirectoryCreate(dirPath);
			}
			catch (Exception ex)
			{
				throw new PublicException(GetMessageRaw("Message.UnableToCreate"), ex);
			}

			BXSectionInfo section = BXSectionInfo.GetSection(dirPath);
			section.Name = sectionName.Text;
			section.Save();

			if (CreateMenu.Checked)
				DoCreateMenuItem(dirPath);
			if (addDefault.Checked)
				DoCreateDefaultFile(dirPath);

			ShowOk();
		}
		catch (Exception ex)
		{
			ErrorCheck(ex);
		}
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		try
		{
			if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.FileManage))
				BXAuthentication.AuthenticationRequired();

			curPath = BXPath.ToVirtualRelativePath(Request["path"] ?? "~");
			if (!BXSecureIO.DirectoryExists(curPath))
				throw new PublicException(GetMessageRaw("Message.DirectoryDoesntExist"));

			BXSecureIO.DemandWriteDirectory(curPath);

			folderNameValidator.ValidationExpression = BXPath.NameValidationRegexString;
			folderNameValidator.ErrorMessage = GetMessage("IllegalCharactersIsDecetedInFolderName");
			folderNameRequired.ErrorMessage = GetMessage("EmptyFolderName");

			defaultTemplate.Items.Add(new ListItem(GetMessageRaw("StandardTemplate"), string.Empty));

			BXSite site = BXSite.GetCurrentSite(curPath, Request.Url.Host);
			curSiteId = (site == null) ? null : site.Id;
			menuOptions = BXPublicMenu.GetMenuTypes(curSiteId);
			foreach (KeyValuePair<string, string> p in menuOptions)
				MenuType.Items.Add(new ListItem(p.Value, p.Key));

			RegisterJavaScript();
		}
		catch (Exception ex)
		{
			FatalErrorCheck(ex);
		}
	}
	protected void Page_Load(object sender, EventArgs e)
	{
		if (fatalError)
			return;
		if (IsPostBack)
			Validate();
	}
	protected void Page_LoadComplete(object sender, EventArgs e)
	{
		if (fatalError)
			return;

		MasterTitle = string.Format(
			"{0}: <font style=\"font-weight: normal;\">{1}</font>",
			GetMessage("MasterTitle"),
			Encode(curPath)
		);
		PrepareResultMessage();

		if (!IsPostBack)
			editDefault.Checked = true;
	}
	protected void Page_PreRender(object sender, EventArgs e)
	{
		if (fatalError)
			return;
		ClientScriptManager csm = this.ClientScript;
		if (csm == null)
			throw new InvalidOperationException("Could not find ClientScriptManager!");
		//---
		csm.RegisterStartupScript(this.GetType(), "startup",
			string.Format(@"
            document.getElementById('{0}').disabled = document.getElementById('{1}').disabled = {2};
            document.getElementById('{3}').disabled = document.getElementById('{4}').disabled = {5};
            ",
			 MenuType.ClientID,
			 MenuItemName.ClientID,
			 (!CreateMenu.Checked).ToString().ToLowerInvariant(),
			 editDefault.ClientID,
			 defaultTemplate.ClientID,
			 (!addDefault.Checked).ToString().ToLowerInvariant()),
			true
			);
	}
	protected void mainTabControl_Command(object sender, BXTabControlCommandEventArgs e)
	{
		if (fatalError)
			return;
		if (e.CommandName != "cancel" && !IsValid)
			return;

		switch (e.CommandName)
		{
			case "save":
				DoCreateFolder();
				if (_ShowMessage != -1)
					GoBack();
				break;
			case "apply":
				DoCreateFolder();
				break;
			case "cancel":
				GoBack();
				break;
		}
	}

	#region IBXFileManPage Members

	public string ProvidePath()
	{
		return BXPath.ToVirtualRelativePath(Request["path"] ?? "~").ToLowerInvariant();
	}

	#endregion
}
