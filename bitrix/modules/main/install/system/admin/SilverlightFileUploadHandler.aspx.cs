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

public partial class BXAjaxFileUploadHandler : BXJsPage
{
    protected bool JSCodeMode;
    protected bool UploadMode;
    protected bool HasError;
    protected string ErrorMessage = String.Empty;
    protected string OwnerId;
    protected string SavedName;
    protected string DisplaySize;
    protected string FilePath;
    protected string FileContentType;

    public string GenerateNewName()
    {
        byte[] data = new byte[16];
        new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(data);
        StringBuilder s = new StringBuilder();
        foreach (byte b in data)
            s.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
        return s.ToString();
    }

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

        HttpPostedFile file = Request.Files[OwnerId + "_ValueUpload"];
        
        if (file == null || file.ContentLength == 0)
        {
            ErrorMessage += JSEncode(HtmlEncode(GetMessageRaw("Error.NoFile")) + "<br/>");
            return;
        }
        string token = Request.Form["csrfToken"];
        if (String.IsNullOrEmpty(token) || !Bitrix.Security.BXCsrfToken.CheckToken(token))
        {
                ErrorMessage += JSEncode(HtmlEncode(GetMessageRaw("Error.InsufficientRights")));
                return;
        }

        string fileExtension = Path.GetExtension(file.FileName);
        if ( !fileExtension.Equals(".XAP",StringComparison.OrdinalIgnoreCase)){
            ErrorMessage+=JSEncode(HtmlEncode(GetMessageRaw("Error.IncorrectFileExtension")));
            return;
        }

        try
        {
            string cachename = GenerateNewName() + Path.GetExtension(file.FileName);
            string savepath = BXPath.Combine(BXConfigurationUtility.Constants.FileCachePath, cachename);
            if (!BXSecureIO.DirectoryExists(BXConfigurationUtility.Constants.FileCachePath))
                BXSecureIO.DirectoryCreate((BXConfigurationUtility.Constants.FileCachePath));
            bool success = false;
            try
            {
                BXPrincipal user = (BXPrincipal)User;
                if (BXSecureIO.CheckUpload(savepath))
                {
                    string filePath = BXPath.ToPhysicalPath(savepath);
                    file.SaveAs(filePath);
                    FilePath =System.Web.Hosting.HostingEnvironment.VirtualPathProvider.CombineVirtualPaths(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath, savepath);
                    success = true;
                }
                else
                {
                    ErrorMessage += JSEncode(HtmlEncode(GetMessageRaw("Error.InsufficientRights")) + "<br/>");
                }
            }
            catch
            {
            }
            if (success)
            {

                //Set New File Params
                FileContentType = file.ContentType;
                //CachedName = cachename;
                SavedName = Path.GetFileName(file.FileName.Replace('/', Path.DirectorySeparatorChar));
                DisplaySize = BXStringUtility.BytesToString(file.ContentLength);
            }
            else
                return;
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

        string filename = Request.QueryString["delete"];
        if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            return;
        string filePath = BXPath.Combine(BXConfigurationUtility.Constants.FileCachePath, filename);
        if (BXSecureIO.FileExists(filePath))
            BXSecureIO.FileDelete(filePath);
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

