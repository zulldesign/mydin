<%@ WebHandler Language="C#" Class="RegBannerRedirection" %>

using System;
using System.Net;
using System.Web;
using System.Collections;
using System.Collections.Generic;
using Bitrix.Advertising;

public class RegBannerRedirection : IHttpHandler, System.Web.SessionState.IRequiresSessionState
{

    private static readonly object _waitForRedirectionRegKey = new object();
    public void ProcessRequest (HttpContext context) 
    {
        HttpResponse response = context.Response;
        response.Expires = 0;
        response.Cache.SetNoStore();
        response.AppendHeader("Pragma", "no-cache");
        
        string code = context.Request.QueryString[RedirectionCodeQueryName];
        if (string.IsNullOrEmpty(code))
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.ContentType = "text/html";
            response.Write("Code is not specified!");
            return;            
        }

        string url = BXAdvertisingManager.ProcessRedirection(code);
        if (string.IsNullOrEmpty(url))
        {
            response.StatusCode = (int)HttpStatusCode.NotFound;
            response.ContentType = "text/html";
            response.Write(string.Format("Url is not found by code '{0}' !", code));
            return;               
        }
        response.Redirect(url);
    }

    /// <summary>
    /// Имя параметра для кода перенаправления
    /// </summary>
    private string RedirectionCodeQueryName
    {
        get { return "code"; }
    }    
    
    public bool IsReusable { get { return true; } }

}