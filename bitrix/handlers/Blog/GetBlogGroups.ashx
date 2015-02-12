<%@ WebHandler Language="C#" Class="Bitrix.Blog.GetBlogGroupsHandler" %>

using System;
using System.Web;
using Bitrix.DataLayer;
using Bitrix.Services.Js;
using Bitrix.Services.Text;

namespace Bitrix.Blog
{
	public class GetBlogGroupsHandler : IHttpHandler, System.Web.SessionState.IRequiresSessionState 
	{
		public void ProcessRequest (HttpContext context) 
		{
			Bitrix.Security.BXCsrfToken.ValidateTokenFromRequest();
			
			var principal = Bitrix.Security.BXPrincipal.Current;
			if (!principal.IsCanOperate(Bitrix.Security.BXRoleOperation.Operations.AccessAdminSystem))
				Bitrix.Security.BXAuthentication.AuthenticationRequired();
			
			context.Response.ContentType = "text/plain";
			
			int i;
			if (context.Request.QueryString["id"] == null || !int.TryParse(context.Request.QueryString["id"], out i) || i <= 0)
			{
				context.Response.Write("[]");
				return;
			}
					
			var groups = BXBlogUserGroup.GetList(
				new BXFilter(
					new BXFilterItem(BXBlogUserGroup.Fields.BlogId, BXSqlFilterOperators.Equal, i),							
					new BXFilterItem(BXBlogUserGroup.Fields.Type, BXSqlFilterOperators.Equal, BXBlogUserGroupType.UserDefined)
				),
				new BXOrderBy(new BXOrderByPair(BXBlogUserGroup.Fields.Name, BXOrderByDirection.Asc)),
				new BXSelect(BXBlogUserGroup.Fields.Id, BXBlogUserGroup.Fields.Name),
				null,
				BXTextEncoder.EmptyTextEncoder
			)
			.ConvertAll(x => string.Format("{{title:'{0}',value:'{1}'}}", BXJSUtility.Encode(x.Name), x.Id.ToString()))
			.ToArray();
			
			context.Response.Write("[" + string.Join(",", groups) + "]");														
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