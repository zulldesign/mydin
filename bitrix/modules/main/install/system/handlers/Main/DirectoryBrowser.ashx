<%@ WebHandler Language="C#" Class="DirectoryBrowser" %>

using System;
using System.IO;
using System.Data;
using System.Web;
using System.Web.Hosting;
using System.Text;
using System.Collections.Generic;

using Bitrix.Security;
using Bitrix.Services;
using Bitrix.IO;

public class DirectoryBrowser : IHttpHandler, System.Web.SessionState.IRequiresSessionState
{
	void BuildDir(string path, string name, bool files, string extList, StringBuilder result)
	{
		int subCount = BXSecureIO.DirectoryList(path, files, extList, new string[0]).Count;
		if (result.Length > 0)
			result.Append('|');	
		result.AppendFormat("{0}:{1}", name, (subCount > 0) ? "dt" : "df");
	}

	void BuildFile(string name, StringBuilder result)
	{
		if (result.Length > 0)
			result.Append('|');
		result.AppendFormat("{0}:f", name);
	}
	
	public void ProcessRequest (HttpContext context) 
	{
		context.Response.StatusCode = 200;
		context.Response.ContentType = "text/plain";
		
		context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
		context.Response.Cache.SetNoStore();
		context.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
		context.Response.Cache.SetLastModified(DateTime.Now);
		context.Response.Cache.AppendCacheExtension("post-check=0");
		context.Response.Cache.AppendCacheExtension("pre-check=0");
		context.Response.Cache.AppendCacheExtension("false");
		context.Response.Cache.SetLastModified(DateTime.Now);
		context.Response.AddHeader("Pragma", "no-cache");
		
		string path = context.Request.QueryString["path"];
		if (path == null)
			return;
		path = BXPath.ToVirtualRelativePath(path);

		bool listFiles = (context.Request.QueryString["files"] != null);
		
		string extList = context.Request.QueryString["ext"];
		
		StringBuilder result = new StringBuilder();

		if (!BXSecureIO.DirectoryExists(path))
			return;

		DataView list = BXSecureIO.DirectoryList(path, listFiles, extList, null);
		
		foreach (DataRowView item in list)
			if ((bool)item["file"])
				BuildFile((string)item["name"], result);
			else
				BuildDir((string)item["path"], (string)item["name"], listFiles, extList, result);
		
		context.Response.Write("FILE|");
		context.Response.Write(result.ToString());
    }
 
    public bool IsReusable {
        get 
		{
            return true;
        }
    }

}