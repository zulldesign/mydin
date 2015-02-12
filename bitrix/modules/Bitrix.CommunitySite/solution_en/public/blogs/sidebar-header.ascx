<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.UI.BXControl" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/search.tags.cloud/component.ascx" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.Services" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<%@ Import Namespace="Bitrix.UI" %>
<%@ Import Namespace="Bitrix.DataLayer" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="Bitrix.Security" %>
<%@ Import Namespace="Bitrix.Search" %>
<%@ Import Namespace="Bitrix.Search.Components" %>

<div class="rounded-block">
	<div class="corner left-top"></div><div class="corner right-top"></div>
	<div class="block-content">
	
		<% if (!String.IsNullOrEmpty(Blog.Name)) { %>
		<h3><%= Blog.Name %><a href="<%=RssBlogPostsUrlTemplate %>" class="header-feed-icon" title="RSS subscribe"></a></h3>
		<%} %>
		
		<%
		string src, name, profile, occupation; 
		src = name = profile = occupation = null;			
		
		if (Blog.Owner != null && Blog.Owner.User != null)
		{
			BXUser user = Blog.Owner.User;
			if (user.Image != null)
				src = user.Image.FilePath;			
			
			name = user.GetDisplayName();
			profile = UserProfileUrlTemplate;
			occupation = user.CustomPublicValues.GetHtml("OCCUPATION");
		}
			
		if (!String.IsNullOrEmpty(name)) { %>
		
		<div class="content-list user-sidebar">
			<div class="content-item">
				<div class="content-avatar">
					<a<%= !String.IsNullOrEmpty(src) ? " style=\"background:url('" + HttpUtility.HtmlAttributeEncode(src) + "') no-repeat center center;\"": "" %> href="<%= profile %>"></a>
				</div>
				<div class="content-info">
					<div class="content-title"><a href="<%= profile %>"><%= name %></a></div>
					<% if (!String.IsNullOrEmpty(occupation)) { %>
					<div class="content-signature"><%= occupation %></div>
					<% } %>
				</div>
			</div>
		</div>		
		
		<% } %>
		
		<% if (!String.IsNullOrEmpty(Blog.Description)) { %>
		<h5><%= Blog.Description %></h5>
		<%} %>

		<div class="hr"></div>
		
		<ul class="mdash-list">
			<li><a href="<%= IndexUrlTemplate %>" title="Blog">Blog</a></li>
			
			<% if (Auth.CanCreatePost) { %>
			<li><a href="<%= NewPostUrlTemplate %>" title="New Blog Post">Post to Blog</a></li>
			<% } %>	
			
			<% if (Auth.CanReadThisBlogDrafts) { %>
			<li><a href="<%= DraftPostListUrlTemplate %>" title="Drafts">Drafts</a></li>
			<% } %>
			
			<% if (Auth.CanEditThisBlogSettings) { %>
			<li><a href="<%= BlogEditUrlTemplate %>" title="Blog Settings">Blog Settings</a></li>
			<% } %>
			
			<% if (Auth.CanCreatePost && Auth.CanWriteFilteredHtml) { %>
			<li><a href="<%= MetaWebLogUrlTemplate %>" title="Post To Blog Using Windows LiveWriter">MetaWebLog</a></li>
			<% } %>
		</ul>
		
	</div>
	<div class="corner left-bottom"></div><div class="corner right-bottom"></div>
</div>

 
 <bx:IncludeComponent 
	id="SearchTagsCloud" 
	runat="server" 
	componentname="bitrix:search.tags.cloud" 
	template="percentage" 
	Filter="" 
	SelectionSort="TagCount" 
	SelectionOrder="Desc" 
	Moderation="NotRejected" 
	DisplaySort="Name" 
	DisplayOrder="Asc" 
	SizeDistribution="Linear" 
	SizeMin="100" 
	SizeMax="260" 
	SizePeriod="" 
	ColorDistribution="None" 
	ColorMin="" 
	ColorMax="" 
	TagLinkTemplate="" 
	PagingAllow="False" 
	PagingRecordsPerPage="60" 
	/>

<script runat="server">
		private string blogSlug;
		private BXBlog blog;
		private BXBlogAuthorization auth;
		public readonly bool IsSearch = Bitrix.Modules.BXModuleManager.IsModuleInstalled("Search");
		protected BXParamsBag<object> replace;

		public string NewPostUrlTemplate
		{
			get { return GetParameter("NewPostUrlTemplate", ""); }
		}

		public string MetaWebLogUrlTemplate
		{
			get { return GetParameter("BlogMetaWeblogApiUrlTemplate", ""); }
		}
	
		public string DraftPostListUrlTemplate
		{
			get { return GetParameter("DraftPostListUrlTemplate", ""); }
		}
		public string BlogEditUrlTemplate
		{
			get { return GetParameter("BlogEditUrlTemplate", ""); }
		}
		public string IndexUrlTemplate
		{
			get { return GetParameter("PostListUrlTemplate", ""); }
		}
		public string UserProfileUrlTemplate
		{
			get { return GetParameter("UserProfileUrlTemplate", ""); }
		}
		public string RssBlogPostsUrlTemplate
		{
			get { return GetParameter("RssBlogPostsUrlTemplate", ""); }
		}
	
		public string GetParameter(string name, string defaultValue)
		{
			string value = defaultValue;
			if (Context.Items.Contains("CommunitySite.Sidebar." + name))
				value = Context.Items["CommunitySite.Sidebar." + name].ToString();

			return Encode(ResolveTemplateUrl(value, replace));
		}
		
	
		public string BlogSlug
		{
			get 
			{
				if (Context.Items.Contains("CommunitySite.Sidebar.BlogSlug"))
					blogSlug = Context.Items["CommunitySite.Sidebar.BlogSlug"].ToString();
				return blogSlug; 
			}
			set { blogSlug = value; }
		}

		public BXBlog Blog
		{
			get { return blog; }
			set { blog = value; }
		}
		public BXBlogAuthorization Auth
		{
			get { return auth; }
		}
	
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (blog == null && !string.IsNullOrEmpty(BlogSlug))
			{
				blog = BXBlog.GetBySlug(
					BlogSlug, 
					null,
					new BXSelectAdd(
						BXBlog.Fields.Owner.User,
						BXBlog.Fields.Owner.User.Image
					),
					BXTextEncoder.HtmlTextEncoder
				);
			}
			if (blog == null || !blog.Active && !BXPrincipal.Current.IsCanOperate(BXBlog.Operations.BlogApprove))
			{
				blog = null;
				Visible = false;
				return;
			}

			replace = new BXParamsBag<object>();
			replace.Add("UserId", Blog.OwnerId);
			replace.Add("BlogSlug", Blog.Slug);
			replace.Add("BlogId", Blog.Id);
			
			auth = new BXBlogAuthorization(blog);
			
			BXSearchQuery q = new BXSearchQuery();
			BXSearchContentGroupFilter f = new BXSearchContentGroupFilter(Bitrix.DataLayer.BXFilterExpressionCombiningLogic.And);
			f.Add(BXSearchContentFilter.IsActive(DateTime.Now));
			f.Add(new BXSearchContentFilterItem(BXSearchField.ModuleId, Bitrix.DataLayer.BXSqlFilterOperators.Equal, "blog"));
			f.Add(new BXSearchContentFilterItem(BXSearchField.ItemGroup, Bitrix.DataLayer.BXSqlFilterOperators.Equal, blog.Id));
			q.Filter = f;
			SearchTagsCloudComponent cloud = (SearchTagsCloudComponent)SearchTagsCloud.Component;
			cloud.ContentQuery = q;
			cloud.Parameters["TagLinkTemplate"] = GetParameter("SearchTagsUrlTemplate", "");
		}
	
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			Visible = 
				IsSearch
				|| !string.IsNullOrEmpty(RssBlogPostsUrlTemplate)
				|| Blog.Owner != null && Blog.Owner.User != null && Blog.Owner.User.Image != null
				|| Auth.CanEditThisBlogSettings || Auth.CanCreatePost || Auth.CanReadThisBlogDrafts;
			

			if (!string.IsNullOrEmpty(RssBlogPostsUrlTemplate))
			{
				BXPage.RegisterLink(
					"alternate", 
					"application/rss+xml", 
					RssBlogPostsUrlTemplate, 
					new KeyValuePair<string, string>("title", Blog.TextEncoder.Decode(Blog.Name))
				);
			}
		}
		protected string ResolveTemplateUrl(string template, BXParamsBag<object> parameters)
		{
			string url = (parameters != null) ? BXSefUrlUtility.MakeLink(template, parameters, null) : template;
			if (url.StartsWith("~/"))
				url = ResolveUrl(url);
			return url;
		}
</script>