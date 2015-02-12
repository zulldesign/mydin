<%@ Control Language="C#" %>
<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.Security" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.DataLayer" %>
<%@ Import Namespace="Bitrix.CommunicationUtility" %>
<%@ Import Namespace="Bitrix.Configuration" %>
<%@ Import Namespace="Bitrix.UI" %>
<%@ Import Namespace="Bitrix.DataTypes" %>

<script runat="server">

	int userId = 0;
	BXUser user = null;
	BXBlog blog;
	protected BXParamsBag<object> replace = new BXParamsBag<object>(); 
	
	protected override void OnLoad(EventArgs e)
	{
 		base.OnLoad(e);
		
		string query = Request.QueryString["user"];
		if (String.IsNullOrEmpty(query) || !int.TryParse(query, out userId) || userId < 1)
		{
			Visible = false;
			return;
		}

		BXUserCollection users = BXUser.GetList(
			new BXFilter(
				new BXFilterItem(BXUser.Fields.UserId, BXSqlFilterOperators.Equal, userId),
				new BXFilterItem(BXUser.Fields.IsApproved, BXSqlFilterOperators.Equal, true)
			), 
			null,
			new BXSelectAdd(BXUser.Fields.Image),
			new BXQueryParams(new BXPagingOptions(0,1))
		);

		user = users.Count > 0 ? users[0] : null;
		if (user == null)
		{
			Visible = false;
			return;
		}
		
		replace["UserId"] = userId = user.UserId;
		
		BXBlogCollection blogs = BXBlog.GetList(
			new BXFilter(
				new BXFilterItem(BXBlog.Fields.Owner.Id, BXSqlFilterOperators.Equal, userId),
				new BXFilterItem(BXBlog.Fields.Active, BXSqlFilterOperators.Equal, true)
			),
			null
		);	
		
		if (blogs.Count > 0)
		{
			blog = blogs[0];
			replace["BlogSlug"] = blog.Slug;
		}
		
	}

	private string ResolveTemplateUrl(string template, BXParamsBag<object> parameters)
	{
		string url = (parameters != null) ? Bitrix.Services.BXSefUrlUtility.MakeLink(template, parameters, null) : template;
		if (url.StartsWith("~/"))
			url = ResolveUrl(url);
		return url;
	}
	
	protected string UserProfileUrl
	{
		get 
		{
			return ResolveTemplateUrl(BXOptionManager.GetOptionString("Bitrix.CommunitySite", "UserProfileUrlTemplate", "", BXSite.Current.Id), replace);
		}
	}

	protected string BlogUrl
	{
		get 
		{
			return ResolveTemplateUrl(BXOptionManager.GetOptionString("Bitrix.CommunitySite", "BlogUrlTemplate", "", BXSite.Current.Id), replace);
		}
	}
	
	protected string NewBlogUrl
	{
		get 
		{
			return ResolveTemplateUrl(BXOptionManager.GetOptionString("Bitrix.CommunitySite", "NewBlogUrlTemplate", "", BXSite.Current.Id), replace);
		}
	}
	
	protected string UserMailUrl
	{
		get 
		{
			return ResolveTemplateUrl(BXOptionManager.GetOptionString("Bitrix.CommunitySite", "UserMailUrlTemplate", "", BXSite.Current.Id), replace);
		}
	}	
	
	protected string UserMailNewForUsersUrl
	{
		get 
		{
			var newReplace = new BXParamsBag<object>(replace);
			newReplace["UserId"] = BXIdentity.Current.Id;
			newReplace["Receivers"] = userId;
			return ResolveTemplateUrl(BXOptionManager.GetOptionString("Bitrix.CommunitySite", "UserMailNewForUsersUrlTemplate", "", BXSite.Current.Id), newReplace);
		}
	}
	
</script>

<div class="rounded-block">
	<div class="corner left-top"></div><div class="corner right-top"></div>
	<div class="block-content">
	
	<div class="content-list user-sidebar">
		<div class="content-item">
			<div class="content-avatar">
				<a<%= user.Image != null && !String.IsNullOrEmpty(user.Image.FilePath) ? " style=\"background:url('" + HttpUtility.HtmlAttributeEncode(user.Image.FilePath) + "') no-repeat center center;\"": "" %> href="<%= UserProfileUrl %>"></a>
			</div>
			<div class="content-info">
				<div class="content-title"><a href="<%= UserProfileUrl %>"><%= user.GetDisplayName() %></a></div>
				<% if (user.CustomPublicValues["OCCUPATION"] != null) { %>
				<div class="content-signature"><%= user.CustomPublicValues.GetHtml("OCCUPATION") %></div>
				<% } %>
			</div>
		</div>
	</div>
	
	<div class="hr"></div>
	
	<ul class="mdash-list">
	
		<% if (blog != null){ %>
		<li><a title="Blog" href="<%= BlogUrl %>">Blog</a></li>
		<%} %>
		
		<% if (userId == BXIdentity.Current.Id){ %>
			<li><a title="Personal Messages" href="<%= UserMailUrl %>">Personal Messages</a></li>
		<% } else if (BXIdentity.Current.Id > 0 && BXPrincipal.Current.IsCanOperate(BXPrivateMessage.Operations.PMessagesTopicCreate)){ %>
			<li><a title="Send Message" href="<%= UserMailNewForUsersUrl %>">Send Message</a></li>
		<%} %>			
		
		<% if (userId == BXIdentity.Current.Id && blog == null && BXPrincipal.Current.IsCanOperate(BXBlog.Operations.Blogging)){ %>
			<li><a title="New Message" href="<%= NewBlogUrl %>">New Message</a></li>
		<%} %>
			
		</ul>
	</div>
	<div class="corner left-bottom"></div><div class="corner right-bottom"></div>
</div>