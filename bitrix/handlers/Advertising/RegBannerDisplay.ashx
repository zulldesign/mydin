<%@ WebHandler Language="C#" Class="RegBannerDisplay" %>

using System;
using System.Web;
using System.Net;
using Bitrix.Advertising;

public class RegBannerDisplay : IHttpHandler, System.Web.SessionState.IRequiresSessionState 
{
    
    public void ProcessRequest (HttpContext context) 
    {
        HttpResponse response = context.Response;
        response.ContentType = "text/html";
        response.Expires = 0;
        response.Cache.SetNoStore();
        response.AppendHeader("Pragma", "no-cache");

        string code = context.Request.QueryString[DisplayBatchCodeQueryName];
        if (!string.IsNullOrEmpty(code))
            BXAdvertisingManager.ProcessDisplayBatch(code);
        response.StatusCode = (int)HttpStatusCode.OK;
        response.Write("<html style=\"border:0\"><body style=\"border:0\">OK</body></html>");          
    }

    /// <summary>
    /// Имя параметра для кода пакета регистрации показа
    /// </summary>
    private string DisplayBatchCodeQueryName
    {
        get { return "code"; }
    } 
    
    public bool IsReusable { get { return true; } }
}