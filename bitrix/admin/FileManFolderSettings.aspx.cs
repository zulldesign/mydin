using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Bitrix.UI;
using Bitrix.Configuration;
using Bitrix.Security;
using Bitrix.IO;
using System.Collections.Generic;
using Bitrix.DataTypes;
using Bitrix.Services;
using System.IO;
using System.Text.RegularExpressions;
using Bitrix.Modules;
using Bitrix.Components;
using Bitrix;

public partial class bitrix_admin_FileManFolderSettings : BXAdminPage, IBXFileManPage
{
	protected class Keyword
	{
		public string Name;
		public string Value;
		public string Inherited;
	}
	class BigException : Exception
	{
		public BigException(string message, Exception innerException)
			: base(message, innerException)
		{

		}
		public BigException(Exception innerException)
			: base(innerException.Message, innerException)
		{

		}
		public BigException(string message)
			: base(message)
		{

		}
	}

	int showMessage;
	bool bigError;
	bool isStateRead;
	string curSite;
	protected string curPath;
	string curDir;
	protected List<Keyword> state = new List<Keyword>();
	protected BXParamsBag<string> keywords;

	protected override string BackUrl
	{
		get
		{
			return base.BackUrl ?? ("FileMan.aspx?path=" + UrlEncode(curDir));
		}
	}

	void PrepareResultMessage()
	{
		SuccessMessage.Visible = showMessage > 0;
	}
	void ShowError(string encodedMessage)
	{
		showMessage = -1;
		ErrorMessage.AddErrorMessage(encodedMessage);
	}
	void ShowError(Exception ex)
	{
		showMessage = -1;
		ErrorMessage.AddErrorMessage(Encode(ex.Message));
	}
	void ShowOk()
	{
		if (showMessage == 0)
			showMessage = 1;
	}
	
	void BigError(string messageText)
	{
		bigError = true;
		MainTabControl.Visible = false;
		MainActionBar.Visible = false;
		ErrorMessage.AddErrorMessage(Encode(messageText));
	}
	void BigError(Exception ex)
	{
		BigError(ex.Message);
	}

	void ValidateInit()
	{
		try
		{
			if (!BXUser.IsCanOperate(BXRoleOperation.Operations.FileManage))
				BXAuthentication.AuthenticationRequired();


			curPath = Request.QueryString["path"];

			if (string.IsNullOrEmpty(curPath))
				throw new BigException(GetMessageRaw("Error.NotEnoughData"));

			curPath = BXPath.ToVirtualRelativePath(curPath ?? "~");
			if (!BXSecureIO.DirectoryExists(curPath))
				throw new BigException(string.Format(GetMessageRaw("Error.FolderDoesNotExist"), curPath));
			curDir = (curPath == "~" ? string.Empty : BXPath.GetDirectory(curPath));

			BXSecureIO.DemandWriteDirectory(curPath);

			BXSite site = BXSite.GetCurrentSite(curPath, Request.Url.Host);
			if (site == null)
				throw new BigException(GetMessageRaw("Error.UnableToResolveSite"));
			curSite = site.TextEncoder.Decode(site.Id);
		}
		catch (BXSecurityException ex)
		{
			throw new BigException(ex);
		}
	}

	void DoSave()
	{
		try
		{
			Save();
			ShowOk();
		}
		catch (BXSecurityException ex)
		{
			ShowError(ex);
			BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
		}
		catch (Exception ex)
		{
			ShowError(GetMessage("Error.SaveUnknown"));
			BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
		}
	}

	void Save()
	{
		BXSecureIO.DemandWriteDirectory(curPath);
		BXSectionInfo info = BXSectionInfo.GetSection(curPath);
		info.Name = Name.Text;
		info.Keywords.Clear();
		foreach (Keyword k in state)
			if (!string.IsNullOrEmpty(k.Name) && !string.IsNullOrEmpty(k.Value))
				info.Keywords[k.Name] = k.Value;
		info.Save();
	}
	private void ReadState()
	{
		if (isStateRead)
			return;

		string uid = EditTable.UniqueID;
		string[] keys = Request.Form.GetValues(uid + "$CODE");
		string[] values = Request.Form.GetValues(uid + "$VALUE");
		if (keys.Length != values.Length)
			return;

		state.Clear();
		
		for (int i = 0; i < keys.Length; i++)
		{
			Keyword k = new Keyword();
			k.Name = keys[i];
			k.Value = values[i];
			k.Inherited = null;
			state.Add(k);
		}

		isStateRead = true;
	}
	protected void Page_Init(object sender, EventArgs e)
	{
		try
		{
			ValidateInit();
		}
		catch (System.Threading.ThreadAbortException)
		{
		}
		catch (BigException ex)
		{
			BigError(ex);
		}
		catch (Exception ex)
		{
			BigError(GetMessageRaw("Error.Unknown"));
			BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
		}
	}
	protected void Page_Load(object sender, EventArgs e)
	{
		if (bigError)
			return;

		if (IsPostBack)
			ReadState();
	}
	protected void Page_LoadComplete(object sender, EventArgs e)
	{
		MasterTitle = GetMessage("MasterTitle");
		if (bigError)
			return;

		keywords = keywords ?? BXPageManager.GetKeywords(curSite);
		BXSectionInfo inherited = !string.IsNullOrEmpty(curDir) ? BXSectionInfo.GetCumulativeSection(curDir) : null;

		if (!IsPostBack)
		{
			BXSectionInfo info = BXSectionInfo.GetSection(curPath);
			Name.Text = info.Name;

			
			foreach (string keyword in keywords.Keys)
			{
				Keyword k = new Keyword();
				k.Name = keyword;
				info.Keywords.TryGetValue(keyword, out k.Value);
				state.Add(k);
			}
			foreach (KeyValuePair<string, string> kvp in info.Keywords)
				if (!keywords.ContainsKey(kvp.Key) && !string.IsNullOrEmpty(kvp.Value))
				{
					Keyword k = new Keyword();
					k.Name = kvp.Key;
					k.Value = kvp.Value;
					state.Add(k);
				}

			if (inherited != null)
				foreach (KeyValuePair<string, string> kvp in inherited.Keywords)
					if (!keywords.ContainsKey(kvp.Key) && (!info.Keywords.ContainsKey(kvp.Key) || string.IsNullOrEmpty(info.Keywords[kvp.Key])) && !string.IsNullOrEmpty(kvp.Value))
					{
						Keyword k = new Keyword();
						k.Name = kvp.Key;
						k.Value = null;
						state.Add(k);
					}

			for (int i = 0; i < 5; i++)
				state.Add(new Keyword());
		}

		if (inherited != null)
			foreach(Keyword k in state)
				if (!string.IsNullOrEmpty(k.Name))
					inherited.Keywords.TryGetValue(k.Name, out k.Inherited);

		PrepareResultMessage();
	}
	protected void More_Click(object sender, EventArgs e)
	{
		for (int i = 0; i < 5; i++)
			state.Add(new Keyword());
	}
	protected void mainTabControl_Command(object sender, BXTabControlCommandEventArgs e)
	{
		if (bigError)
			return;
		switch (e.CommandName)
		{
			case "save":
				DoSave();
				if (showMessage != -1)
					GoBack();
				break;
			case "apply":
				DoSave();
				if (showMessage != -1)
					Response.Redirect(Request.RawUrl);
				break;
			case "cancel":
				GoBack();
				break;
		}
	}

	#region IBXFileManPage Members
	public string ProvidePath()
	{
		string path = Request.QueryString["path"];
		if (string.IsNullOrEmpty(path))
			return "~";
		return BXPath.ToVirtualRelativePath(path).ToLowerInvariant();
	}
	#endregion
}
