<%@ WebHandler Language="C#" Class="FileManDownload" %>

using System;
using System.IO;
using System.Web;
using System.Text;
using System.Globalization;

using Bitrix.Services;
using Bitrix.IO;
using Bitrix.Security;


public class FileManDownload : IHttpHandler 
{
   	void Response404()
    {
        throw new HttpException(404, String.Empty);
    }

    public void ProcessRequest (HttpContext context) 
    {
		if (!BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.FileManage))
			Response404();
		
		string path = context.Request["path"];
        if (path == null)
			Response404();
        path = BXPath.ToVirtualRelativePath(path);
		
		if (!BXSecureIO.FileExists(path))
			Response404();
		BXPrincipal user = (BXPrincipal)context.User;
		if (!BXSecureIO.CheckView(path))
			Response404();
		
		FileInfo file = new FileInfo(BXPath.ToPhysicalPath(path));
		context.Response.ContentType = "application/force-download";
		context.Response.AppendHeader("Content-Disposition", String.Format("attachment; filename=\"{0}\"", file.Name));
		context.Response.TransmitFile(file.FullName);
    }

    public bool IsReusable
    {
        get
        {
            return true;
        }
    }

}