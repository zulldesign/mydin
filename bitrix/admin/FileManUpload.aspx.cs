using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Bitrix.UI;
using Bitrix.Services;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Bitrix.Security;
using Bitrix.Services.Js;
using Bitrix.IO;
using Bitrix.Configuration;
using System.Text.RegularExpressions;

public partial class bitrix_admin_FileManUpload : BXAdminPage, IBXFileManPage
{
	public const int FilesCount = 5;

	class ErrorPair
	{
		public string filename;
		public OperationError error;

		public ErrorPair(string fname, OperationError err)
		{
			filename = fname;
			error = err;
		}
	}
	enum OperationError
	{
		Unknown = 0,
		IllegalFileName,
		ItemExists,
	}

	int showMessage;
	string backUrl;

	void PrepareResultMessage()
	{
		successMessage.Visible = showMessage > 0;
	}
	void ShowError(string encodedMessage)
	{
		showMessage = -1;
		errorMessage.AddErrorMessage(encodedMessage);
	}
	void ShowError(Exception ex)
	{
		showMessage = -1;
		errorMessage.AddErrorMessage(Encode(ex.Message));
	}
	void ShowOk()
	{
		if (showMessage == 0)
			showMessage = 1;
	}
	

	protected override string BackUrl
	{
		get
		{
			backUrl = base.BackUrl;
			if (backUrl == null)
			{
				backUrl = "FileMan.aspx";
				if (curPath != null)
					backUrl += "?path=" + HttpUtility.UrlEncode(curPath);
			}
			return backUrl;
		}
	}

	protected string curPath;
	string fullPath;

	List<ErrorPair> _Errors = new List<ErrorPair>();

	void DoUploadFiles()
	{
		int count;
		if (!int.TryParse(fileCount.Value, out count))
			count = FilesCount;

		for(int i = 0; i < count; i++)
		{
			try
			{
				var file = Request.Files.Get("fileManUpload_uploadsTable_itemFile_" + i.ToString());
				if (file == null)
					continue;
				
				string filename = (Request.Form["fileManUpload_uploadsTable_itemText_" + i] ?? "").Trim();
				if (string.IsNullOrEmpty(filename))
					filename = file.FileName;
				filename = filename.Trim();

				if (string.IsNullOrEmpty(filename))
					continue;

				string filePath = BXPath.Combine(curPath, filename);
				if (!Regex.IsMatch(filename, BXPath.NameValidationRegexString))
					throw new InvalidOperationException(GetErrorMessage(filename, OperationError.IllegalFileName));

				if (BXSecureIO.DirectoryExists(filePath))
					throw new InvalidOperationException(GetErrorMessage(filename, OperationError.ItemExists));
				
				bool overwrite = false;

				if (BXSecureIO.FileExists(filePath))
				{
					if (!string.IsNullOrEmpty(Overwrite.Value))
						overwrite = true;
					else
						throw new InvalidOperationException(GetErrorMessage(filename, OperationError.ItemExists));
				}

				if (overwrite)
					BXSecureIO.DemandWrite(filePath);
				BXSecureIO.FileWritePostedFile(filePath, file, false, true);
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}
		ShowOk();
	}

	string GetErrorMessage(string filename, OperationError operationError)
	{
		switch (operationError)
		{
			case OperationError.IllegalFileName:
				return string.Format(GetMessageRaw("Message.FileNameRequirements"), filename);
			case OperationError.ItemExists:
				return string.Format("{0} ({1})", GetMessageRaw("Message.TargetItemAlreadyExists"), BXPath.Combine(curPath, filename));
		}
		return string.Empty;
	}

	public override void ProcessRequest(HttpContext context)
	{
		if (context.Request.QueryString["check"] != null)
		{
			context.Response.StatusCode = 200;
			context.Response.Write(CheckFiles(context.Request));
			context.Response.End();
			return;
		}
		base.ProcessRequest(context);
	}

	private string CheckFiles(HttpRequest request)
	{
		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.FileManage))
			BXAuthentication.AuthenticationRequired();

		List<string> exist = new List<string>();

		curPath = BXPath.ToVirtualRelativePath(request.QueryString["path"]);

		string[] files = request.Form.GetValues("file");
		string[] names = request.Form.GetValues("name");
		for (int i = 0; i < names.Length; i++)
		{
			if (string.IsNullOrEmpty(files[i]))
				continue;

			string filename = names[i].Trim();
			if (string.IsNullOrEmpty(filename))
				filename = Path.GetFileName(files[i]).Trim();
			
			if (!Regex.IsMatch(filename, BXPath.NameValidationRegexString))
				continue;

			if (BXSecureIO.FileExists(BXPath.Combine(curPath, filename)))
				exist.Add(filename);
		}

		return BXJSUtility.BuildJSArray(exist);
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.FileManage))
			BXAuthentication.AuthenticationRequired();

		if (Request["path"] == null)
			GoBack();
		curPath = BXPath.ToVirtualRelativePath(Request["path"]);

		if (!BXSecureIO.CheckUploadNonExecutable(curPath) && !BXSecureIO.CheckUploadExecutable(curPath))
			BXAuthentication.AuthenticationRequired();

		fullPath = BXPath.ToPhysicalPath(curPath);
		if (!Directory.Exists(fullPath))
			GoBack();
	
	}
	
	protected void Page_LoadComplete(object sender, EventArgs e)
	{
		MasterTitle = MasterTitle = GetMessage("MasterTitle") + ": <font style=\"font-weight: normal;\">" + HttpUtility.HtmlEncode(curPath) + "</font>";
		PrepareResultMessage();
		fileCount.Value = FilesCount.ToString();
		Form.Enctype = "multipart/form-data";
		Overwrite.Value = string.Empty;
	}

	protected void mainTabControl_Command(object sender, BXTabControlCommandEventArgs e)
	{
		switch (e.CommandName)
		{
			case "save":
				DoUploadFiles();
				if (showMessage != -1)
					GoBack();
				break;
			case "apply":
				DoUploadFiles();
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
