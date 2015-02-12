<%@ WebHandler Language="C#" Class="Bitrix.Wizards.Install.SuccessHandler" %>
using System;
using System.Web;

namespace Bitrix.Wizards.Install
{
	public class SuccessHandler : IHttpHandler 
	{
		public void ProcessRequest (HttpContext context) 
		{
			context.Response.ContentType = "text/plain";
			context.Response.Write(context.Request.QueryString["params1"] == "~/.+-%#!@*\\\"%7e" && context.Request.QueryString["params2"] == "%7e%2f.%2b-%25%23!%40*%5c%22%257e" ? "SUCCESS" : "");
		}
	 
		public bool IsReusable 
		{
			get 
			{
				return true;
			}
		}
	}
}