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

public partial class bitrix_admin_FileManSecurity : BXAdminPage, IBXFileManPage
{
	class Entry
	{
		string path;
		string name;
		bool operable;
		bool file;

		public string Path
		{
			get
			{
				return path;
			}
		}
		public string Name
		{
			get
			{
				return name;
			}
		}
		public bool Operable
		{
			get
			{
				return operable;
			}
		}
		public bool File
		{
			get
			{
				return file;
			}
		}

		public Entry(string path, string name, bool operable)
		{
			this.path = path;
			this.name = name;
			this.operable = operable;
			this.file = operable && BXSecureIO.FileExists(path);
		}
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
	List<Entry> items = new List<Entry>();
	List<Entry> validItems;
	protected string curDir;

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

			string path = Request.QueryString["path"];
			string entries = Request.QueryString["items"];

			if (string.IsNullOrEmpty(path))
				throw new BigException(GetMessageRaw("Error.NotEnoughData"));
				
			if (entries == null)
			{
				path = BXPath.ToVirtualRelativePath(path ?? "~");
				path = BXPath.Trim(path);
				if (!BXSecureIO.FileOrDirectoryExists(path))
					throw new BigException(string.Format(GetMessageRaw("Error.FileOrFolderMissing"), path));
				curDir = (path == "~" ? string.Empty : BXPath.GetDirectory(path));

				BXSecureIO.DemandSecurity(path);
				items.Add(new Entry(path, ".", true));
			}
			else if (entries != null)
			{
				Regex dots = new Regex(@"\A[\.\s]*\Z", RegexOptions.Compiled);
				path = BXPath.ToVirtualRelativePath(path);
				path = BXPath.Trim(path);
				if (!BXSecureIO.DirectoryExists(path))
					throw new BigException(string.Format(GetMessageRaw("Error.FolderDoesNotExist"), path));
				curDir = path;
				string[] files = entries.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string f in files)
				{
					string name = f.Trim();
					if (string.IsNullOrEmpty(name))
						continue;
					string vp = null;
					bool valid =
						name.IndexOfAny(Path.GetInvalidFileNameChars()) == -1					// 1. Entry name contains only valid symbols
						&& !dots.IsMatch(name)													// 2. Entry name is not only dots and spaces, like '..' or '. . . .'
						&& BXSecureIO.FileOrDirectoryExists(vp = BXPath.Combine(path, name))	// 3. Entry exists
						&& BXSecureIO.CheckSecurity(vp);										// 4. Security management for entry is allowed
					items.Add(new Entry(vp, name, valid));
				}
			}
				
			validItems = items.FindAll(delegate(Entry item)
			{
				return item.Operable;
			});
			if (validItems.Count == 0)
				throw new BigException(GetMessageRaw("Error.NoElements"));
			items.Sort(delegate(Entry a, Entry b)
			{
				int i = -a.Operable.CompareTo(b.Operable);
				if (i == 0)
					i = a.File.CompareTo(b.File);
				if (i == 0)
					i = a.Name.CompareTo(b.Name);
				return i;
			});
		}
		catch (BXSecurityException ex)
		{
			throw new BigException(ex);
		}
	}
	void LoadAuthorizationState()
	{
		OperationsEdit.State.Clear();
		BXFileAuthorizationState state = BXFileAuthorizationManager.BuildAuthorizationState(delegate(string path)
		{
			foreach (Entry entry in validItems)
				if (BXPathComparer.IsSubDir(path, entry.Path))
					return true;
			return false;
		});
		//List<string> changedRoles = new List<string>();

		Dictionary<Entry, BXOperationsEditOperationState> fileState = new Dictionary<Entry, BXOperationsEditOperationState>(); //moved out of section to save memory
		foreach (KeyValuePair<string, BXOperationsEditRoleInfo> role in OperationsEdit.Roles)
		{
			int roleId = int.Parse(role.Key);
			bool changed = false;
			foreach (string op in OperationsEdit.Operations.Keys)
			{
				BXOperationsEditInheritedOperationState inheritedOperationState = BXOperationsEditInheritedOperationState.Denied;
				BXOperationsEditOperationState currentOperationState = BXOperationsEditOperationState.Inherited;

				//Calculate inherited state
				if (!string.IsNullOrEmpty(curDir))
					foreach (KeyValuePair<string, BXParamsBag<Dictionary<int, bool>>> pathState in state)
						if (BXPathComparer.IsSubDir(pathState.Key, curDir))
						{
							Dictionary<int, bool> operationState;
							bool allowed;
							if (pathState.Value.TryGetValue(op, out operationState) && operationState.TryGetValue(roleId, out allowed))
							{
								changed = true;
								inheritedOperationState = allowed ? BXOperationsEditInheritedOperationState.Allowed : BXOperationsEditInheritedOperationState.Denied;
							}
						}


				List<string> allowedEntries = new List<string>();
				List<string> deniedEntries = new List<string>();
				List<string> inheritedEntries = new List<string>();

				foreach (Entry entry in validItems)
				{
					BXParamsBag<Dictionary<int, bool>> pathState;
					Dictionary<int, bool> operationState;
					bool allowed;
					if (state.TryGetValue(entry.Path, out pathState)
						&& pathState.TryGetValue(op, out operationState)
						&& operationState.TryGetValue(roleId, out allowed))
					{
						changed = true;
						(allowed ? allowedEntries : deniedEntries).Add(entry.Name);
					}
					else
						inheritedEntries.Add(entry.Name);
				}

				//only one list should contain elements
				int containers =
					(inheritedEntries.Count == 0 ? 0 : 1)
					+ (allowedEntries.Count == 0 ? 0 : 1)
					+ (deniedEntries.Count == 0 ? 0 : 1);
				if (containers > 1)
					currentOperationState = BXOperationsEditOperationState.DontModify;
				else if (allowedEntries.Count > 0)
					currentOperationState = BXOperationsEditOperationState.Allowed;
				else if (deniedEntries.Count > 0)
					currentOperationState = BXOperationsEditOperationState.Denied;

				if (currentOperationState != BXOperationsEditOperationState.Inherited || inheritedOperationState != BXOperationsEditInheritedOperationState.Denied)
				{
					BXOperationsEditOperationInfo info = new BXOperationsEditOperationInfo(currentOperationState, inheritedOperationState);
					if (currentOperationState == BXOperationsEditOperationState.DontModify)
					{
						info.AllowDontModify = true;
						OperationsEdit.ShowLegendDontModify = true;
						StringBuilder note = new StringBuilder();
						if (allowedEntries.Count > 0)
						{
							note.AppendFormat("<font style=\"color: Green; font-weight: bold\" >{0} : </font>", GetMessage("OperationState.Allowed"));
							note.AppendLine(Encode(string.Join(", ", allowedEntries.ToArray())));
							note.AppendLine("<br/>");
						}
						if (deniedEntries.Count > 0)
						{
							note.AppendFormat("<font style=\"color: Red; font-weight: bold\" >{0} : </font>", GetMessage("OperationState.Denied"));
							note.AppendLine(Encode(string.Join(", ", deniedEntries.ToArray())));
							note.AppendLine("<br/>");
						}
						if (inheritedEntries.Count > 0)
						{
							note.AppendFormat(
								"<font style=\"color: Gray; font-weight: bold\" >{0} &quot;{1}&quot;: </font>", 
								GetMessage("OperationState.Inherited"),
								inheritedOperationState == BXOperationsEditInheritedOperationState.Allowed ? GetMessage("OperationState.Allowed") : GetMessage("OperationState.Denied")	
							);
							if (inheritedEntries.Count > Math.Max(allowedEntries.Count, deniedEntries.Count))
							{
								note.AppendFormat("<i>{0}</i>", GetMessage("OtherFilesAndFolder"));
								note.AppendLine();
							}
							else
								note.AppendLine(Encode(string.Join(", ", inheritedEntries.ToArray())));
							note.AppendLine("<br/>");
						}

						info.NoteHtml = note.ToString();
						note.Length = 0;
					}
					role.Value.Operations.Add(op, info);
				}
			}
			if (changed)
				OperationsEdit.State.SetRoleFromData(role.Key);
		}
	}
	void DoSave()
	{
		try
		{
			ValidateSave();
			Save();
			ShowOk();
		}
		catch (Exception ex)
		{
			ShowError(GetMessageRaw("Error.SaveUnknown"));
			BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
		}
	}
	void ValidateSave()
	{
		OperationsEdit.State.Validate(false);
		//BXSecureIO.DemandSecurity(curPath);
	}
	void Save()
	{
		bool changed = false;
		Dictionary<string, BXAuthorizationFile> fileCache = new Dictionary<string, BXAuthorizationFile>();

		foreach (Entry entry in validItems)
			foreach (BXRole role in BXRoleManager.GetList(null, null))
			{
				string roleId = role.RoleId.ToString();
				bool deleteAll = !OperationsEdit.State.ContainsRole(roleId);
				foreach (string op in BXFileOperation.Operations)
				{
					BXOperationsEditOperationState opState = deleteAll ? BXOperationsEditOperationState.Inherited : OperationsEdit.State[roleId, op];
					if (opState == BXOperationsEditOperationState.Inherited)
						BXFileAuthorizationManager.DeleteLocalRight(entry.Path, role.RoleName, op, fileCache, false);
					else if (opState != BXOperationsEditOperationState.DontModify)
						BXFileAuthorizationManager.SetLocalRight(entry.Path, role.RoleName, op, opState == BXOperationsEditOperationState.Allowed, fileCache, false);
				}
			}

		try
		{
			foreach (KeyValuePair<string, BXAuthorizationFile> file in fileCache)
			{
				file.Value.PlaceAllAndNotAuthorizedFirst();
				file.Value.RemoveEmptyLocations();

				List<string> messages = new List<string>();


				BXCommand command = new BXCommand("Bitrix.Security.OnBeforeSaveAuthorizationFile");
				command.Parameters.Add("path", file.Key);
				command.Parameters.Add("authorizationFile", file.Value);
				command.Send();
				foreach (KeyValuePair<string, BXCommandResult> kvp in command.CommandResultDictionary)
					if (kvp.Value.CommandResult == BXCommandResultType.Cancel)
						messages.Add(kvp.Value.Result.ToString());
				if (messages.Count > 0)
					throw new BXEventException(messages);

				command = new BXCommand("Bitrix.Security.OnSaveAuthorizationFile");
				command.Parameters.Add("path", file.Key);
				command.Parameters.Add("authorizationFile", file.Value);
				command.Send();
				foreach (KeyValuePair<string, BXCommandResult> kvp in command.CommandResultDictionary)
					if (kvp.Value.CommandResult == BXCommandResultType.Error)
						messages.Add(kvp.Value.Result.ToString());
				if (messages.Count > 0)
					throw new BXEventException(messages);

				if (file.Value.Save(file.Key))
					changed = true;

				command = new BXCommand("Bitrix.Security.OnAfterSaveAuthorizationFile");
				command.Parameters.Add("path", file.Key);
				command.Parameters.Add("authorizationFile", file.Value);
				command.Send();
				foreach (KeyValuePair<string, BXCommandResult> kvp in command.CommandResultDictionary)
					if (kvp.Value.CommandResult == BXCommandResultType.Error)
						messages.Add(kvp.Value.Result.ToString());
				if (messages.Count > 0)
					throw new BXEventException(messages);
			}
		}
		finally
		{
			if (changed)
				new BXCommand("Bitrix.Security.FileAuthorizationStateChanged").Send();
		}
	}

	string GetEntryTD(Entry entry)
	{
		return string.Format(
			"<td>{0}</td><td class=\"{1}\">{2}</td>",
			GetEntryImg(entry),
			(entry.Operable ? "enabled" : "disabled"),
			HttpUtility.HtmlEncode(entry.Name)
		);
	}
	string GetEntryImg(Entry entry)
	{
		string file;

		if (!entry.Operable)
			file = "disabled.gif";
		else if (entry.File)
			file = "file.gif";
		else
			file = "folder.gif";

		return string.Format(
			"<img src=\"{0}\" />",
			Encode(BXThemeHelper.AddAbsoluteThemePath("images/security/admin/" + file))
		);
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		try
		{
			ValidateInit();

			OperationsEdit.FillStandardRoles(false);
			foreach (string op in BXFileOperation.Operations)
				OperationsEdit.Operations[op] = BXFileAuthorizationManager.GetActionTypeDisplayName(op);
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
	protected void Page_LoadComplete(object sender, EventArgs e)
	{
		MasterTitle = GetMessage("MasterTitle");
		BXPage.RegisterThemeStyle("FileManSecurity.css");
		if (bigError)
			return;

		if (items.Count == 1)
		{
			SecurityLiteralTop.Text = string.Format(
				GetMessage("SingleFileDescription"),
				items[0].File ? GetMessage("ForFile") : GetMessage("ForFolder"),
				string.Format("<i>{0}</i>", items[0].Path)
			);
		}
		else
			SecurityLiteralTop.Text = GetEntryListHtml();

		LoadAuthorizationState();

		PrepareResultMessage();
	}

	string GetEntryListHtml()
	{
		StringBuilder label = new StringBuilder();
		
		StringBuilder s = new StringBuilder();
		int cf = 0;
		int cd = 0;
		int cu = 0;
		
		foreach(Entry e in items)
			if (e.Operable)
				if (e.File)
					cf++;
				else
					cd++;
			else
				cu++;
		if (cf > 0)
		{
			s.AppendFormat("{0} ({1})", GetMessage("ForFiles"), cf);
		}
		if (cd > 0)
		{
			if (cf > 0)
				if (cu > 0)
					s.Append(", ");
				else
				{
					s.Append(' ');
					s.AppendFormat(GetMessage("And"));
					s.Append(' ');
				}

			s.AppendFormat("{0} ({1})", GetMessage("ForFolders"), cd);
		}
		if (cu > 0)
		{
			if (cf > 0 || cd > 0)
			{
				s.Append(' ');
				s.AppendFormat(GetMessage("And"));
				s.Append(' ');
			}

			s.AppendFormat("{0} ({1})", GetMessage("ForUnknown"), cu);
		}
		
		label.AppendFormat(
			GetMessage("ManyFileDescription"), 
			s, 
			string.Format("<i>{0}</i>", curDir == "~" ? GetMessage("InSiteRoot") : Encode(curDir))
		);
		s.Length = 0;

		label.Append(" ");
		label.Append("<a href=\"javascript: void(0);\" onclick=\"");
		
		label.Append("var list = document.getElementById('bx_edit_security_filelist');");
		label.Append("var pos = jsUtils.GetRealPos(this);");
		label.Append("list.style.left = pos.left + 'px';");
		label.Append("list.style.top = (pos.bottom + 2) + 'px';");
		label.Append("list.style.display = (list.style.display == 'none') ? 'block' : 'none';");

		label.Append("\">");
		label.Append(GetMessage("Details"));
		label.Append("</a>");
		

		List<string> entries = items.ConvertAll<string>(delegate(Entry entry)
		{
			return GetEntryTD(entry);
		});
		label.AppendFormat(
			"<div id=\"bx_edit_security_filelist\" style=\"display: none;\"><table class=\"edit-security-filelist\" ><tr>{0}</tr></table></div>", 
			string.Join("</tr><tr>", entries.ToArray())
		);
		//label.Append("</div>");

		string result = label.ToString();
		label.Length = 0;
		return result;
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
		path = BXPath.ToVirtualRelativePath(path);
		if (Request.QueryString["items"] == null && path != "~")
			path = BXPath.GetDirectory(path);
		return path.ToLowerInvariant();
	}

	#endregion
}
