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
using Bitrix.Services;
using System.IO;
using System.Text;
using Bitrix.Security;
using Bitrix;
using Bitrix.UI;

using Bitrix.IO;
using Bitrix.Configuration;
using Bitrix.Services.Text;

public partial class BXCustomTypeFileEditExtra : BXJsPage
{
	protected bool JSCodeMode;
	protected bool UploadMode;
	protected bool HasError;
	protected string ErrorMessage = String.Empty;
	protected string OwnerId;
	protected string CachedId;
	protected BXFile Uploaded;
	//protected string SavedName;
	//protected string DisplaySize;
	//protected string ContentType;

	
	private void ProcessUploadMode()
	{
		HasError = true;
		UploadMode = true;
		JSCodeMode = false;

		if (string.IsNullOrEmpty(Request.QueryString["id"]))
		{
			ErrorMessage += JSEncode(HtmlEncode(GetMessageRaw("Error.NoId")) + "<br/>");
			return;
		}

		OwnerId = Request.QueryString["id"];
		CachedId = Request.QueryString["old"];

		HttpPostedFile file = Request.Files[OwnerId + "_ValueUpload"];
		if (file == null || file.ContentLength == 0)
		{
			ErrorMessage += JSEncode(HtmlEncode(GetMessageRaw("Error.NoFile")) + "<br/>");
			return;
		}

		try
		{
			string cacheid = Guid.NewGuid().ToString("N");			
			bool success = false;
			try
			{
				Uploaded = new BXFile(BXTextEncoder.EmptyTextEncoder, file, "uf", "main", "");
				Uploaded.TempGuid = cacheid;
				Uploaded.DemandFileUpload();
				Uploaded.Save();
				success = true;				
			}
			catch(BXSecurityException)
			{
				ErrorMessage += JSEncode(HtmlEncode(GetMessageRaw("Error.InsufficientRights")) + "<br/>");
			}
			catch
			{
			}

			if (!success)
				return;
			
			//Delete Old File
			BXFile old;
			if (!string.IsNullOrEmpty(CachedId) && (old = BXFile.GetByTempGuid(CachedId, BXTextEncoder.EmptyTextEncoder)) != null)
			{
				try
				{
					old.Delete();
				}
				catch
				{
				}
			}

			//Set New File Params
			CachedId = cacheid;						
				
			HasError = false;
		}
		catch
		{
		}
	}
	private void ProcessDeleteMode()
	{
		JSCodeMode = false;
		UploadMode = false;

		if (string.IsNullOrEmpty(Request.QueryString["delete"]))
			return;

		var file = BXFile.GetByTempGuid(Request.QueryString["delete"], BXTextEncoder.EmptyTextEncoder);		
		if (file != null)
			file.Delete();
	}
	private void ProcessJSCodeMode()
	{
		JSCodeMode = true;
		UploadMode = false;
	}
	protected void Page_Init(object sender, EventArgs e)
	{
		if (Request.QueryString["id"] != null)
			ProcessUploadMode();
		else if (Request.QueryString["delete"] != null)
			ProcessDeleteMode();
		else 
			ProcessJSCodeMode();
	}	
}
