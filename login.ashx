<%@ WebHandler Language="C#" Class="login" %>
using System;
using System.Web;

public class login : IHttpHandler
{
	public void ProcessRequest(HttpContext context)
	{
		string redirectUrl = System.Web.Security.FormsAuthentication.GetRedirectUrl("", false);
		Uri originalUri;
		if (string.IsNullOrEmpty(redirectUrl) || !Uri.TryCreate(Bitrix.Services.BXSefUrlManager.CurrentUrl, redirectUrl, out originalUri))
			originalUri = Bitrix.Services.BXSefUrlManager.CurrentUrl;

		string redirect = null;
		try
		{	
			string vp = VirtualPathUtility.ToAppRelative(HttpUtility.UrlDecode(originalUri.AbsolutePath));
			if (Bitrix.IO.BXPathComparer.IsSubDir("~/bitrix", vp))
				redirect = VirtualPathUtility.ToAbsolute("~/bitrix/admin/Login.aspx");
		}
		catch
		{
		}
		
		if (redirect == null)
		{
			var site = Bitrix.BXSite.GetCurrentSite(originalUri) ?? Bitrix.BXSite.Current;
			if (site != null)
			{
				redirect = Bitrix.Configuration.BXConfigurationUtility.Options.GetLoginVirtualPath(site);
				
				if (redirect == null && System.IO.File.Exists(Bitrix.IO.BXPath.MapPath(site.DirectoryVirtualPath + "login/default.aspx")))
					redirect = site.DirectoryAbsolutePath + "login/";
					
				if (redirect == null && System.IO.File.Exists(Bitrix.IO.BXPath.MapPath(site.DirectoryVirtualPath + "login.aspx")))
					redirect = site.DirectoryAbsolutePath + "login.aspx";
					
				if (redirect == null && System.IO.File.Exists(Bitrix.IO.BXPath.MapPath(site.DirectoryVirtualPath + "auth.aspx")))
					redirect = site.DirectoryAbsolutePath + "auth.aspx";
			}
		}
		if (redirect == null)
			redirect = VirtualPathUtility.ToAbsolute("~/bitrix/admin/Login.aspx");
		
		context.Response.Redirect(redirect + originalUri.Query);
	}

	public bool IsReusable
	{
		get
		{
			return true;
		}
	}
}