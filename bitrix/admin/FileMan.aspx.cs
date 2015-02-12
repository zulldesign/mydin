using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
using Bitrix.Configuration;
using Bitrix.DataLayer;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.UI;

public partial class bitrix_admin_FileMan : BXAdminPage, IBXFileManPage
{
	[Flags]
	enum FileOperation : int
	{
		Nothing = 0,
		Delete = 1,
		Copy = 2,
		Move = Copy | Delete
	}

	protected string curPath;
	private StringBuilder errorText = new StringBuilder();
	private int showMessage;
	private string curSiteId;
	public string SiteId
	{
		get
		{
			if (curSiteId == null)
			{
				BXSite site = BXSite.GetCurrentSite(curPath, Request.Url.Host);
				curSiteId = site != null ? site.Id : string.Empty;
			}
			return curSiteId;
		}
	}

	private string errorTemplateNoRightsToDelete;
	private string errorTemplateTargetExists;
	private string errorTemplateUnableToDelete;
	private string errorTemplateUnableToWrite;
	private string errorTemplateUnknownSource;
	private string errorTemplateCantCopyIntoItself;

	private void PrepareResultMessage()
	{
		if (this.showMessage == 0)
		{
			resultMessage.Visible = false;
			return;
		}
		resultMessage.Visible = true;
		resultMessage.CssClass = (this.showMessage > 0) ? "Ok" : "Error";
		resultMessage.IconClass = (this.showMessage > 0) ? "Ok" : "Error";
		resultMessage.Title = (this.showMessage > 0) ? GetMessage("Message.OperationSuccessful") : GetMessage("Message.OperationErrors");
		resultMessage.Content = (this.showMessage > 0) ? String.Empty : this.errorText.ToString();
	}
	private void ShowError(string encodedMessage)
	{
		this.showMessage = -1;
		this.errorText.AppendLine("<br />");
		this.errorText.AppendLine(encodedMessage);
	}
	private void ShowOk()
	{
		if (this.showMessage == 0)
			this.showMessage = 1;
	}

	protected string GetActionLink(object param)
	{
		DataRowView drv = param as DataRowView;
		if (drv != null)
		{
			
			string path = (string)drv["path"];

			if (BXSecureIO.FileExists(path))
			{
				if (BXSecureIO.CheckWrite(path))
				{
					BXFileType type = BXFileInfo.GetFileType(path);
					if ((type & BXFileType.Menu) != BXFileType.Unknown)
						return "MenuEdit.aspx?path=" + HttpUtility.UrlEncode(path);
					else if ((type & (BXFileType.Text | BXFileType.Page)) != BXFileType.Unknown)
						return "FileManEdit.aspx?path=" + HttpUtility.UrlEncode(path);
				}
				if (BXSecureIO.CheckView(path))
					return "FileManView.aspx?path=" + HttpUtility.UrlEncode(path);
				return string.Empty;
			}
			else
				return "FileMan.aspx?path=" + HttpUtility.UrlEncode(path) + "&role=" + HttpUtility.UrlEncode(rolesList.SelectedValue);
		}

		return String.Empty;
	}
	protected string GetDisplayText(object param)
	{
		if (param is DataRowView)
		{
			DataRowView drv = param as DataRowView;
			string path = BXPath.ToRelativePath((string)drv["path"]);

			if (File.Exists(BXPath.ToPhysicalPath(path)))
			{
				FileInfo f = new FileInfo(BXPath.ToPhysicalPath(path));
				string ext = BXPath.GetExtension(f.Name);
				if (ext == BXConfigurationUtility.Constants.MenuFileExt)
					return GetMessageFormat("MenuOfType", f.Name.Replace("." + BXConfigurationUtility.Constants.MenuFileExt, String.Empty));
			}
			return HttpUtility.HtmlEncode((string)drv["name"]);
		}

		return String.Empty;
	}
	protected string GetImageTitle(object param)
	{

		DataRowView drv = param as DataRowView;
		if (drv != null)
		{
			if (drv["type"].Equals(String.Empty))
				return GetMessage("Up");

            string link = GetActionLink(param);
            if (link.Contains("MenuEdit.aspx?path=")) return GetMessage("PopupTitle.EditMenu");
            if (link.Contains("FileManEdit.aspx?path=")) return GetMessage("PopupTitle.EditFileContent");
            if (link.Contains("FileManView.aspx?path=")) return GetMessage("PopupText.View");
            if (link.Contains("FileMan.aspx?path=")) return GetMessage("PopupText.Browse");

		}
        return String.Empty;
	}
	protected string GetImageUrl(object param)
	{
		string prefix = AppRelativeTemplateSourceDirectory + Path.DirectorySeparatorChar + "img" + Path.DirectorySeparatorChar;
		string url = prefix + "file.gif";
		if (param is DataRowView)
		{
			DataRowView drv = param as DataRowView;

			if (drv["type"].Equals(String.Empty))
				url = prefix + "folder_up.gif";
			else if (drv["type"].Equals("f"))
				url = prefix + "file.gif";
			else
				url = prefix + "folder.gif";
		}
		return VirtualPathUtility.ToAbsolute(url);
	}
	protected string GetDownloadTD(object param)
	{
		string image = VirtualPathUtility.ToAbsolute(AppRelativeTemplateSourceDirectory + Path.DirectorySeparatorChar + "img" + Path.DirectorySeparatorChar + "download.gif");
		if (param is DataRowView)
		{
			DataRowView drv = param as DataRowView;

			if (drv["type"].Equals("f")
			&& BXSecureIO.CheckView((string)drv["path"]))
				return String.Format(@"<td><a href=""FileManDownload.ashx?path={0}""><img alt=""download"" border=""0"" src=""{1}"" title=""{2}"" /></a>&nbsp;&nbsp;</td>", HttpUtility.UrlEncode((string)drv["path"]), image, GetMessage("ImageToolTip.Download"));
		}
		return String.Empty;
	}
	protected string GetBrowseTD(object param)
	{
		try
		{
			string image = BXPath.ToVirtualAbsolutePath(BXPath.Combine(AppRelativeTemplateSourceDirectory, "img/browse.gif"));
			if (param is DataRowView)
			{
				DataRowView drv = param as DataRowView;
				string path = (string)drv["path"];

				if (drv["type"].Equals("f")
				&& path.EndsWith(".aspx", StringComparison.InvariantCultureIgnoreCase))
				{
					BXSecureIO.DemandRead(path);
					string redirect = BXSite.GetUrlForPath(BXPath.ToVirtualRelativePath(path), null);
					return String.Format(@"<td>&nbsp;&nbsp;<a href=""{0}""><img alt=""browse"" border=""0"" src=""{1}"" title=""{2}"" /></a></td>", redirect, image, GetMessage("ImageToolTip.Browse"));
				}
			}
		}
		catch
		{
		}
		return String.Empty;
	}

	bool CheckForCopyOperation(string source, string target)
	{
		//Check for empty directory
		if (String.IsNullOrEmpty(targetFolder.Text.Trim()))
		{
			ShowError(GetMessage("Message.TargetDirectoryEmpty"));
			return false;
		}

		//Check for self directory
		if (BXPathComparer.Instance.Compare(target, source) == 0)
		{
			ShowError(String.Format("{0}:<br/><i>{1}</i><br/>", GetMessage("Message.CantCopyIntoThemselves"), Path.DirectorySeparatorChar + HttpUtility.HtmlEncode(target)));
			return false;
		}

		//Check for target directory
		if (!Directory.Exists(BXPath.ToPhysicalPath(target)))
		{
			ShowError(String.Format("{0}:<br/><i>{1}</i><br/>", GetMessage("Message.TargetDirectoryDoesntExist"), Path.DirectorySeparatorChar + HttpUtility.HtmlEncode(target)));
			return false;
		}

		//Check for target directory rights
		if (!BXSecureIO.CheckWriteDirectory(target))
		{
			ShowError(String.Format("{0}:<br/><i>{1}</i><br/>", GetMessage("Message.InsufficientRightsForTargetDir"), Path.DirectorySeparatorChar + HttpUtility.HtmlEncode(target)));
			return false;
		}
		return true;
	}

	List<string> FormFileList(UserMadeChoiceEventArgs e)
	{
		List<string> files = new List<string>();
		string fullCurPath = BXPath.ToPhysicalPath(this.curPath);

		if (!Directory.Exists(fullCurPath))
			return files;

		if (!e.ApplyForAll)
		{
			int[] sel = fileManGrid.GetSelectedRowsIndices();
			foreach (int i in sel)
			{
				DataKey key = fileManGrid.DataKeys[i];
				if (String.IsNullOrEmpty((string)key["type"]))
					continue;

				files.Add((string)key["name"]);
			}
		}
		else
		{
			List<string> entities = new List<string>();


			DataView view = BXSecureIO.DirectoryList(this.curPath, true);
			view.RowFilter = Filter.CurrentFilter.BuildDataViewFilter();

			foreach (DataRowView v in view)
				files.Add((string)v["name"]);
		}
		return files;
	}
	void DoFileCopyMove(string sourcePath, string targetPath, bool delete, bool isFile)
	{
		if (BXPathComparer.IsSubDir(sourcePath, targetPath))
			throw new Exception(String.Format(this.errorTemplateCantCopyIntoItself, sourcePath, targetPath));

		if (BXSecureIO.FileOrDirectoryExists(targetPath))
			throw new Exception(String.Format(this.errorTemplateTargetExists, sourcePath, targetPath));

		//Check if copy or move
		if (delete && (isFile ? BXSecureIO.CheckWrite(sourcePath) : BXSecureIO.CheckWriteDirectory(sourcePath)))
		{
			try
			{
				if (isFile)
					BXSecureIO.FileMove(sourcePath, targetPath);
				else
					BXSecureIO.DirectoryMove(sourcePath, targetPath);
			}
			catch (Exception ex)
			{
				throw new Exception(String.Format(this.errorTemplateUnableToWrite, sourcePath, targetPath), ex);
			}
		}
		else
		{
			//Copy
			try
			{
				if (isFile)
					BXSecureIO.FileCopy(sourcePath, targetPath);
				else
					BXSecureIO.DirectoryCopy(sourcePath, targetPath);
			}
			catch (Exception ex)
			{
				throw new Exception(String.Format(this.errorTemplateUnableToWrite, sourcePath, targetPath), ex);
			}
			if (delete)
				DoFileDelete(sourcePath, isFile);
		}
	}
	void DoFileDelete(string sourcePath, bool isFile)
	{
		if (!(isFile ? BXSecureIO.CheckWrite(sourcePath) : BXSecureIO.CheckWriteDirectory(sourcePath)))
			throw new Exception(String.Format(this.errorTemplateNoRightsToDelete, sourcePath));

		try
		{
			if (isFile)
				BXSecureIO.FileDelete(sourcePath);
			else
				BXSecureIO.DirectoryDelete(sourcePath, true);
		}
		catch (Exception ex)
		{
			throw new Exception(String.Format(this.errorTemplateUnableToDelete, sourcePath), ex);
		}
	}
	void DoFileOperation(FileOperation operation, UserMadeChoiceEventArgs e, string singleEntry)
	{
		if (operation == FileOperation.Nothing)
			return;

		string source = this.curPath;
		string target = targetFolder.Text;
		try
		{
			target = target.Trim().Trim('/', '\\');
			target = BXPath.ToVirtualRelativePath(target);
		}
		catch
		{
			ShowError(Encode(string.Format(GetMessageRaw("FormattedErrorMassage.SpecifiedPathIsInvalid"), targetFolder.Text)));
			return;
		}

		List<string> files;
		if (e == null)
		{
			if (singleEntry == null)
				return;
			files = new List<string>();
			files.Add(singleEntry);
		}
		else
			files = FormFileList(e);

		if (files.Count == 0)
			return;

		this.errorTemplateNoRightsToDelete = String.Format("{0}: {{0}}", GetMessageRaw("Message.InsufficientRightsToDelete"));
		this.errorTemplateUnknownSource = String.Format("{0}: {{0}}", GetMessageRaw("Message.UnknownItem"));
		this.errorTemplateUnableToDelete = String.Format("{0}: {{0}}", GetMessageRaw("Message.UnableToDelete"));
		this.errorTemplateTargetExists = String.Format("{0}: {{0}} -> {{1}}", GetMessageRaw("Message.TargetItemAlreadyExists"));
		this.errorTemplateUnableToWrite = String.Format("{0}: {{0}} -> {{1}}", GetMessageRaw("Message.UnableToWrite"));
		this.errorTemplateCantCopyIntoItself = String.Format("{0}: {{0}} -> {{1}}", GetMessageRaw("Message.CantCopyIntoItself"));

		if ((operation & FileOperation.Copy) > 0 && !CheckForCopyOperation(source, target))
			return;

		for (int i = 0; i < files.Count; i++)
		{
			try
			{
				string filename = files[i];
				string sourcePath = BXPath.Combine(source, filename);

				bool isFile;
				if (BXSecureIO.FileExists(sourcePath))
					isFile = true;
				else if (BXSecureIO.DirectoryExists(sourcePath))
					isFile = false;
				else
					throw new Exception(String.Format(this.errorTemplateUnknownSource, sourcePath));

				if ((operation & FileOperation.Copy) > 0)
					DoFileCopyMove(sourcePath, BXPath.Combine(target, filename), (operation & FileOperation.Delete) > 0, isFile);
				else if ((operation & FileOperation.Delete) > 0)
					DoFileDelete(sourcePath, isFile);
			}
			catch (Exception ex)
			{
				ShowError(Encode(ex.Message));
			}
		}
		ShowOk();
		fileManGrid.MarkAsChanged();
	}
	void DoFileSecurity(UserMadeChoiceEventArgs e, string singleEntry)
	{
		StringBuilder s = new StringBuilder();
		if (singleEntry != null && e == null)
			s.Append(HttpUtility.UrlEncode(singleEntry));
		else if (e.ApplyForAll)
		{
			DataView entities = null;
			if (BXSecureIO.DirectoryExists(this.curPath))
				entities = BXSecureIO.DirectoryList(this.curPath, true, null, new string[] { "name" });

			if (entities != null)
				foreach (DataRowView r in entities)
				{
					if (s.Length > 0)
						s.Append('|');
					s.Append(HttpUtility.UrlEncode((string)r["name"]));
				}
		}
		else
		{
			int[] sel = fileManGrid.GetSelectedRowsIndices();
			foreach (int i in sel)
			{
				DataKey key = fileManGrid.DataKeys[i];
				if (String.IsNullOrEmpty((string)key["type"]))
					continue;
				if (s.Length > 0)
					s.Append('|');
				s.Append(HttpUtility.UrlEncode((string)key["name"]));
			}
		}
		Response.Redirect("FileManSecurity.aspx?path=" + this.curPath + "&items=" + s.ToString());
	}
	
	protected void Page_Init(object sender, EventArgs e)
	{
		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.FileManage))
			BXAuthentication.AuthenticationRequired();
		try
		{
			this.curPath = BXPath.ToVirtualRelativePath(Request["path"] ?? string.Empty);
			if (!BXSecureIO.DirectoryExists(this.curPath))
				throw new Exception();
		}
		catch
		{
			Response.Redirect("FileMan.aspx");
		}

		NewFileButton.Visible = BXSecureIO.CheckWriteExecutable(curPath);
		NewFolderButton.Visible = BXSecureIO.CheckWriteDirectory(curPath);
		UploadButton.Visible = BXSecureIO.CheckUploadNonExecutable(curPath) || BXSecureIO.CheckUploadExecutable(curPath);
		AddMenuButton.Visible = string.IsNullOrEmpty(SiteId) ? BXUser.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit) : BXUser.IsCanOperate(BXRoleOperation.Operations.MenuItemsEdit, "main", SiteId);
	
		FolderSettingsButton.Visible = BXSecureIO.CheckView(curPath);
		RootSecurityButton.Visible = BXSecureIO.CheckSecurity(this.curPath);

		rolesList.Items.Add(new ListItem(GetMessage("List.CurrentUser"), String.Empty));
		rolesList.SelectedIndex = 0;

		GotoPath.Text = curPath;

		BXRoleCollection roles1 = BXRoleManager.GetList(new BXFormFilter(new BXFormFilterItem("Active", true, BXSqlFilterOperators.Equal)), new BXOrderBy_old("RoleName", "Asc"));
		foreach (BXRole role in roles1)
			rolesList.Items.Add(new ListItem(role.Title, role.RoleName));

		string curRole = Request["role"];
		if (curRole != null)
		{
			ListItem item = rolesList.Items.FindByValue(curRole);
			if (item != null)
				rolesList.SelectedValue = curRole;
		}

		resultMessage.Visible = false;

		fileManGrid.AddColumnKeys(0, "path", "type", "name");
		fileManGrid.AddColumnKeys(1, "size");
		fileManGrid.AddColumnKeys(4, "type");

		this.selectDirDialog.DefaultPath = /*"\\" + */curPath;

		//zg, bitrix, 27.05.2008
		BXPage.RegisterThemeStyle("FileMan.css");
	}
	protected void Page_LoadComplete(object sender, EventArgs e)
	{
		//newFileName.Value = String.Empty;
		MasterTitle = string.Format(
			"{0}: <font style=\"font-weight: normal;\">{1}</font>",
			GetMessage("MasterTitle"),
			Encode(this.curPath)
		);
		PrepareResultMessage();
	}

	protected void fileManGrid_MultiOperationActionRun(object sender, Bitrix.UI.UserMadeChoiceEventArgs e)
	{
		switch (e.CommandOfChoice.CommandName)
		{
			case "security":
				DoFileSecurity(e, null);
				break;
			case "delete":
				DoFileOperation(FileOperation.Delete, e, null);
				e.Cancel = true;
				break;
			case "copy":
				DoFileOperation(FileOperation.Copy, e, null);
				break;
			case "move":
				DoFileOperation(FileOperation.Move, e, null);
				break;
		}
	}
	protected void fileManGrid_PopupMenuClick(object sender, Bitrix.UI.BXPopupMenuClickEventArgs e)
	{
		DataKey drv = fileManGrid.DataKeys[e.EventRowIndex];
		string encodedPath = HttpUtility.UrlEncode((string)drv["path"]);
		switch (e.CommandName)
		{
			case "browse":
				Response.Redirect("FileMan.aspx?path=" + encodedPath + "&role=" + HttpUtility.UrlEncode(rolesList.SelectedValue ?? string.Empty));
				break;
			case "view":
				Response.Redirect("FileManView.aspx?path=" + encodedPath);
				break;
			case "edit":
				Response.Redirect("FileManEdit.aspx?path=" + encodedPath);
				break;
			case "editastext":
				Response.Redirect("FileManEdit.aspx?path=" + encodedPath + "&editor=text");
				break;
			case "editraw":
				Response.Redirect("FileManEdit.aspx?path=" + encodedPath + "&editor=text&raw=");
				break;
			case "editmenu":
				Response.Redirect("MenuEdit.aspx?path=" + encodedPath);
				break;
			case "security":
				DoFileSecurity(null, (string)drv["name"]);
				break;
			case "delete":
				DoFileOperation(FileOperation.Delete, null, (string)drv["name"]);
				e.Cancel = true;
				break;
		}
	}
	protected void fileManGrid_RowDataBound(object sender, GridViewRowEventArgs e)
	{
		BXGridView grid = (BXGridView)sender;
		BXGridViewRow row = (BXGridViewRow)e.Row;
		DataRowView drv = (DataRowView)row.DataItem;
		if (e.Row.RowType != DataControlRowType.DataRow)
			return;
		List<string> allowed = new List<string>();
		if (drv["type"].Equals(String.Empty))
		{

			allowed.Add("browse");
			row.AllowedCommandsList = allowed.ToArray();
			return;
			//row.PopupMenu = null;
			//return;

		}
		string path = (string)drv["path"];
		BXPrincipal user = (BXPrincipal)User;
		string root = BXPath.AppPath;
		//row.PopupMenu = BXPopupPanel1;
		//grid.PopupCommandMenuId = BXPopupPanel1.ID;


		if (drv["type"].Equals("d"))
			allowed.Add("browse");
		else
		{
			BXFileType type = BXFileInfo.GetFileType(path);
			if ((type & BXFileType.Menu) != BXFileType.Unknown)
			{
				if (BXSecureIO.CheckWrite(path))
				{
					allowed.Add("editmenu");
					allowed.Add("editraw");
				}
			}
			else if ((type & BXFileType.TextFormat) != BXFileType.Unknown && (type & BXFileType.Page) == BXFileType.Unknown)
			{
				if (BXSecureIO.CheckWrite(path))
					allowed.Add("editraw");
			}
			else if ((type & BXFileType.Page) != BXFileType.Unknown)
			{
				if (BXSecureIO.CheckWrite(path))
				{
					allowed.Add("edit");
					allowed.Add("editastext");
					allowed.Add("editraw");
				}
			}
			else if ((type & BXFileType.Template) != BXFileType.Unknown)
			{
				if (BXSecureIO.CheckWrite(path))
					allowed.Add("editraw");
			}

			if (BXSecureIO.CheckView(path))
			allowed.Add("view");
		}
			

		allowed.Add("");


		if (drv["type"].Equals("d") ? BXSecureIO.CheckWriteDirectory(path) : BXSecureIO.CheckWrite(path))
		{
			allowed.Add("inline");
			allowed.Add("delete");
		}

		allowed.Add("");

		if (BXSecureIO.CheckSecurity(path))
			allowed.Add("security");

		if (!allowed.Exists(delegate(string item)
		{ 
			return !string.IsNullOrEmpty(item);
		}))
			allowed.Add("nop");
		row.AllowedCommandsList = allowed.ToArray();
	}
	
	#region Data Adapter
	private static string CurrentDirectory
	{
		get
		{
			return BXPath.ToPhysicalPath(HttpContext.Current.Request["path"] ?? string.Empty);
			//string path = "";
			//if (HttpContext.Current.Request["path"] != null)
			//{
			//    path = BXPath.ToRelativePath(HttpContext.Current.Request["path"]);
			//}
			//if (!String.IsNullOrEmpty(path))
			//    return BXPath.ToPhysicalPath(path);
			//else
			//    return BXPath.AppPath;
		}
	}
	private static bool IsRoot
	{
		get
		{
			return BXPath.Equals(CurrentDirectory, BXPath.AppPath);//, StringComparison.InvariantCultureIgnoreCase);
		}
	}

	private static string GetAccessString(string path, string role)
	{
		string[] types;
		if (!String.IsNullOrEmpty(role))
			types = BXFileAuthorizationManager.GetGlobalRights(path, role, true, true);
		else
			types = BXFileAuthorizationManager.GetUserOperations(path, HttpContext.Current.User, HttpContext.Current.Request.RequestType);
		return HttpUtility.HtmlEncode(String.Join(", ", types));
	}
	private static void AddParentRow(DataTable result)
	{
		DataRow row = result.NewRow();

		row["isbackbutton"] = true;
		DirectoryInfo dir = new DirectoryInfo(CurrentDirectory).Parent;

		row["name"] = "...";
		string p = BXPath.ToVirtualRelativePath(dir.FullName);//.Replace(BXPath.AppPath, String.Empty));
		row["path"] = p;
		row["type"] = string.Empty;
		row["file"] = false;

		result.Rows.InsertAt(row, 0);
	}
	#endregion

	string[] GetColumns()
	{
		return BXSet.Union(
			fileManGrid.GetVisibleColumnsKeys(),
			Filter.CurrentFilter.ConvertAll<string>(delegate(BXFormFilterItem item)
			{
				return item.filterName;
			}).ToArray()
		);
	}
	protected void fileManGrid_Select(object sender, BXSelectEventArgs e)
	{
		string role = rolesList.SelectedValue;
		if (role == null)
			role = String.Empty;

		string[] columns = GetColumns();
		bool access = BXSet.Contains(columns, "access");

		DataView view = BXSecureIO.DirectoryList(curPath, true, null, columns);
		view.RowFilter = Filter.CurrentFilter.BuildDataViewFilter();
		DataTable result = view.ToTable();
		result.Columns.Add("isbackbutton", typeof(bool));
		if (access)
			result.Columns.Add("access", typeof(string));
		foreach (DataRow r in result.Rows)
		{
			r["path"] = ((string)r["path"]);//.Substring(2).ToLower();
			r["isbackbutton"] = false;
			if (access)
				r["access"] = GetAccessString((string)r["path"], role);
		}

		if (!IsRoot) //Don't need paging options, because it's a normal element of list
			AddParentRow(result);

		DataView dv = new DataView(result);
		dv.Sort = "isbackbutton DESC, file ASC" + (string.IsNullOrEmpty(e.SortExpression) ? string.Empty : (", " + e.SortExpression));
		result = dv.ToTable();

		if (e.PagingOptions != null)
		{
			if (!IsRoot && e.PagingOptions.startRowIndex == 0)
				e.PagingOptions.maximumRows++;

			result.Columns.Add("isvisible", typeof(bool));

			for (int i = 0; i < result.Rows.Count; i++)
				result.Rows[i]["isvisible"] = (i >= e.PagingOptions.startRowIndex && i < (e.PagingOptions.startRowIndex + e.PagingOptions.maximumRows));

			dv = new DataView(result);
			dv.RowFilter = "isvisible = true";
			result = dv.ToTable();
		}

		e.Data = new DataView(result);
	}
	protected void fileManGrid_SelectCount(object sender, BXSelectCountEventArgs e)
	{
		string[] columns = GetColumns();

		DataView view = BXSecureIO.DirectoryList(curPath, true, null, columns);
		view.RowFilter = Filter.CurrentFilter.BuildDataViewFilter();
		e.Count = view.Count;

		if (!IsRoot)
			e.Count++;
	}
	protected void fileManGrid_Update(object sender, BXUpdateEventArgs e)
	{
		try
		{
			string source = BXPath.Combine(this.curPath, (string)e.Keys["name"]);
            string newName = (string)e.NewValues["name"];
			string destination = BXPath.Combine(this.curPath, newName);

			if ((string)e.Keys["type"] != "f" && (string)e.Keys["type"] != "d")
				throw new Exception(String.Format("{0}: {1}", GetMessageRaw("Message.UnknownItem"), source));

			bool isFile;
			if (BXSecureIO.FileExists(source))
				isFile = true;
			else if (BXSecureIO.DirectoryExists(source))
				isFile = false;
			else
				throw new Exception(String.Format("{0}: {1}", GetMessageRaw("Message.UnknownItem"), source));

			if (BXPathComparer.Instance.Equals(source, destination))
			{
				ShowOk();
				return;
			}

			if (BXSecureIO.FileOrDirectoryExists(destination))
				throw new Exception(String.Format("{0}: {1} -> {2}", GetMessageRaw("Message.TargetItemAlreadyExists"), source, destination));

            Regex validationRx = new Regex(BXPath.NameValidationRegexString, RegexOptions.CultureInvariant | RegexOptions.Compiled);
            if (!validationRx.IsMatch(newName))
                throw new Exception(string.Format(GetMessageRaw("IllegalCharactersIsDecetedInFileName"), newName));

			try
			{
				if (isFile)
					BXSecureIO.FileMove(source, destination);
				else
					BXSecureIO.DirectoryMove(source, destination);
			}
			catch (Exception ex)
			{
				throw new Exception(String.Format("{0}: {1} -> {2}", GetMessageRaw("Message.UnableToRename"), source, destination), ex);
			}

			ShowOk();
			e.UpdatedCount = 1;
		}
		catch (Exception ex)
		{
			ShowError(Encode(ex.Message));
		}
	}

	#region IBXFileManPage Members

	public string ProvidePath()
	{
		return BXPath.ToVirtualRelativePath(Request["path"] ?? "~").ToLowerInvariant();
	}

	#endregion

	protected void rolesList_SelectedIndexChanged(object sender, EventArgs e)
	{
		fileManGrid.MarkAsChanged();
	}
}
