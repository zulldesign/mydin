<%@ WebHandler Language="C#" Class="BXUsersHandler" %>

using System;
using System.Web;
using Bitrix.Services;
using System.Collections.Generic;
using System.Collections.Specialized;
using Bitrix.Configuration;
using Bitrix.Security;
using Bitrix.DataLayer;
using Bitrix.Services.Js;
using Bitrix.UI;

public class BXUsersHandler : IHttpHandler, System.Web.SessionState.IRequiresSessionState
{

	private HttpContext mContext = null;

	#region PrivateProperties
	private HttpContext Context
	{
		get
		{
			if (mContext == null)
				throw new InvalidOperationException("Could not find context!");
			//---
			return mContext;
		}
		set
		{
			mContext = value;
		}
	}

	private HttpRequest Request
	{
		get
		{
			HttpRequest request = Context.Request;
			if (request == null)
				throw new InvalidOperationException("Could not find request!");
			//--- 
			return request;
		}
	}
	private HttpResponse Response
	{
		get
		{
			HttpResponse response = Context.Response;
			if (response == null)
				throw new InvalidOperationException("Could not find response!");
			//--- 

			return response;
		}
	}

	private string Query
	{
		get
		{
			return Request.QueryString["query"];
		}
	}

	private bool IsCanOperateAccessAdminSystem
	{
		get
		{
			return (Context.User as BXPrincipal).IsCanOperate(BXRoleOperation.Operations.AccessAdminSystem);
		}
	}

	#endregion

	private void SetResponseStatus(int aCode, string aDescription)
	{
		HttpResponse response = Response;
		response.StatusCode = aCode;
		response.StatusDescription = aDescription;
	}

	private bool userHaveRights()
	{
		return (Context.User.Identity.IsAuthenticated && IsCanOperateAccessAdminSystem);

	}

	public void ProcessRequest(HttpContext context)
	{
		Context = context;
		try
		{
			HttpRequest request = Request;

			if (userHaveRights())
			{
				BXUserListHandler bl = new BXUserListHandler();
				if (!BXCsrfToken.CheckTokenFromRequest(request.QueryString))
				{
					Response.Write("(");
					Response.Write(bl.SessionIsIncorrectMessage);
					Response.Write(")");
				}
				else
				{
					bl.ProcessRequest(Context);
					return;
				}
			}
			else
				SetResponseStatus(401, "Unauthorized");

		}
		catch (Exception)
		{
			SetResponseStatus(500, "Internal Server Error");
		}

	}

	public bool IsReusable
	{
		get
		{
			return true;
		}
	}

}