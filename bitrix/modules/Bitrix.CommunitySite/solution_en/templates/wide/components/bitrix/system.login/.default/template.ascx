<%@ Reference Control="~/bitrix/components/bitrix/system.login/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.Main.Components.SystemLoginTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.Security" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.DataLayer" %>
<%@ Import Namespace="Bitrix.Configuration" %>
<%@ Import Namespace="Bitrix.UI" %>

<script runat="server">

	BXParamsBag<object> replace = new BXParamsBag<object>();
	BXBlog blog = null;
	
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);

		replace["UserId"] = BXIdentity.Current.Id;

		if (BXIdentity.Current.Id > 0)
		{
			BXBlogCollection blogs = BXBlog.GetList(
				new BXFilter(
					new BXFilterItem(BXBlog.Fields.Active, BXSqlFilterOperators.Equal, true),
					new BXFilterItem(BXBlog.Fields.Owner.Id, BXSqlFilterOperators.Equal, BXIdentity.Current.Id),
					new BXFilterItem(BXBlog.Fields.Categories.Category.Sites.Site.ID, BXSqlFilterOperators.Equal, BXSite.Current.Id)
				),
				null
			);

			if (blogs.Count > 0)
			{
				blog = blogs[0];
				replace["BlogSlug"] = blog.Slug;
			}

			PrivateMessages.Component.Parameters["MessagesReadUrl"] = UserMailUrl;
			PrivateMessages.Component.Parameters["MessageReadUrlTemplate"] = UserMailReadUrl;
		}
	}

	protected string UserProfileUrl
	{
		get
		{
			return Component.ResolveTemplateUrl(BXOptionManager.GetOptionString("Bitrix.CommunitySite", "UserProfileUrlTemplate", "", BXSite.Current.Id), replace);
		}
	}

	protected string BlogUrl
	{
		get
		{
			return Component.ResolveTemplateUrl(BXOptionManager.GetOptionString("Bitrix.CommunitySite", "BlogUrlTemplate", "", BXSite.Current.Id), replace);
		}
	}

	protected string UserMailUrl
	{
		get
		{
			return Component.ResolveTemplateUrl(BXOptionManager.GetOptionString("Bitrix.CommunitySite", "UserMailUrlTemplate", "", BXSite.Current.Id), replace);
		}
	}

	protected string NewBlogUrl
	{
		get
		{
			return Component.ResolveTemplateUrl(BXOptionManager.GetOptionString("Bitrix.CommunitySite", "NewBlogUrlTemplate", "", BXSite.Current.Id), replace);
		}
	}

	protected string NewPostUrl
	{
		get
		{
			return Component.ResolveTemplateUrl(BXOptionManager.GetOptionString("Bitrix.CommunitySite", "NewPostUrlTemplate", "", BXSite.Current.Id), replace);
		}
	}

	protected string UserMailReadUrl
	{
		get
		{
			// Hack to retain the escaped ##
			return Bitrix.Services.Text.BXStringUtility.ReplaceIgnoreCase(BXOptionManager.GetOptionString("Bitrix.CommunitySite", "UserMailReadUrlTemplate", "", BXSite.Current.Id), "#UserId#", BXIdentity.Current.Id.ToString());
		}
	}
	
	protected string LogoutUrl
	{
		get
		{
			UriBuilder uri = new UriBuilder(Bitrix.Services.BXSefUrlManager.CurrentUrl);
			NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);
			query.Remove("ReturnUrl");
			uri.Query = query.ToString();
			replace["ReturnUrl"] = uri.Uri.PathAndQuery;
			return Component.ResolveTemplateUrl(BXOptionManager.GetOptionString("Bitrix.CommunitySite", "LogoutUrlTemplate", "", BXSite.Current.Id), replace);
		}
	}

	protected string LoginUrl
	{
		get
		{
			UriBuilder uri = new UriBuilder(Bitrix.Services.BXSefUrlManager.CurrentUrl);
			NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);
			query.Remove("ReturnUrl");
			uri.Query = query.ToString();
			replace["ReturnUrl"] = uri.Uri.PathAndQuery;
			return Component.ResolveTemplateUrl(BXOptionManager.GetOptionString("Bitrix.CommunitySite", "LoginUrlTemplate", "", BXSite.Current.Id), replace);
		}
	}	
	
	
</script>

<ul id="user-menu">
					
<% if (BXIdentity.Current.IsAuthenticated) {%>

	<li><%= string.Format(GetMessageRaw("Welcome"), UserProfileUrl, HttpUtility.HtmlEncode(BXIdentity.Current.User.GetDisplayName())) %></li>
	
	<% if (BXPrincipal.Current.IsCanOperate(BXBlog.Operations.Blogging)) {%>
		<% if (blog != null) { %>
			<li><a href="<%= BlogUrl %>" title="<%= GetMessageRaw("BlogUrlTooltip") %>"><%= GetMessageRaw("BlogUrl") %></a></li>
			<%--<li><a href="<%= NewPostUrl %>" title="Написать новую запись в блог">Написать в блог</a></li>--%>
		<% } else { %>
			<li><a href="<%= NewBlogUrl %>" title="<%= GetMessageRaw("CreateUrlTooltip") %>"><%= GetMessageRaw("CreateUrl") %></a></li>
		<% } 
	} %>
	<li><a href="<%= UserMailUrl %>" title="<%= GetMessageRaw("BlogUrlTooltip") %>"><%= GetMessageRaw("MessagesUrl") %></a><bx:IncludeComponent id="PrivateMessages" runat="server" MessagesReadUrl="" MessageReadUrlTemplate="" componentname="bitrix:pmessages.alert" template="new-messages" /></li>	
	<li><a href="<%= LogoutUrl %>" title="<%= GetMessageRaw("LogoutUrl") %>"><%= GetMessageRaw("LogoutUrl") %></a></li>

<% } else { %>

	<li><%= GetMessageRaw("WelcomeGuest") %></li>
	<li><a href="<%= LoginUrl %>" title="<%= GetMessageRaw("LoginTooltip") %>"><%= GetMessageRaw("ButtonText.Login") %></a></li>
	<li><a href="<%= Component.RegisterPath %>" title="<%= GetMessageRaw("RegisterTooltip") %>"><%= GetMessageRaw("Register") %></a></li>	
	
<%} %>

</ul>

<script type="text/javascript">
function <%=ClientID %>switch(from,to)
{
	var fromElement = document.getElementById(from);
	var toElement = document.getElementById(to);
	if( fromElement && toElement )
	{
		fromElement.style.display="none";
		toElement.style.display = "block";
	}
	return false;
}

</script>

