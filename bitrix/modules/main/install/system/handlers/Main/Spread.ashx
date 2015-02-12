<%@ WebHandler Language="C#"  Class="Bitrix.Services.SpreadHandler" %>

using System;
using System.Web;

namespace Bitrix.Services
{
	public class SpreadHandler : IHttpHandler
	{
		public void ProcessRequest(HttpContext context) 
		{
			try
			{
				Bitrix.Services.BXSpreadCookies.Restore(
					Convert.FromBase64String(context.Request.QueryString["d"]),
					Convert.FromBase64String(context.Request.QueryString["s"])
				);
		
				context.Response.AddHeader(
					"P3P",
					string.Format(
						@"policyref=""{0}"", CP=""NON DSP COR CUR ADM DEV PSA PSD OUR UNR BUS UNI COM NAV INT DEM STA""",
						VirtualPathUtility.ToAbsolute("~/bitrix/p3p.xml")
					)
				);
			}
			catch
			{
			}
					
			context.Response.StatusCode = 204;
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