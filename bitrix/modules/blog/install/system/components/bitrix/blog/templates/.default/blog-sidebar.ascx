<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.UI.BXControl" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.Services" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<%@ Import Namespace="Bitrix.UI" %>
<%@ Import Namespace="Bitrix.DataLayer" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="Bitrix.Security" %>
<%@ Import Namespace="Bitrix.IO" %>
<%@ Import Namespace="Bitrix.Configuration" %>
<div class="blog-content">
	<%
		string name = null;			
		string profile = null;
        BXUser user = null;   
        BXFile avatar = null;
        if (Blog.Owner != null && (user = Blog.Owner.User) != null && (avatar = user.Image) != null)
		{ 		
			name = user.GetDisplayName();
			profile = Encode(ResolveTemplateUrl(UserProfileUrlTemplate, replace));
		}
	    
	 //Blog.Owner.User.Image.Width   
	%>
	<% if (avatar != null) { %>
	<ul>
		<li class="blog-tags">
			<div class="blog-sidebar-avatar">
				<% if (!string.IsNullOrEmpty(profile)) { %><a href="<%= profile %>"><% } %>
				<img src="<%= HttpUtility.HtmlAttributeEncode(avatar.FilePath) %>" title="<%= name %>" border="0" width="<%= avatar.Width.ToString() %>" height="<%= avatar.Height.ToString() %>" alt="" />
				<% if (!string.IsNullOrEmpty(profile)) { %></a><% } %>
			</div>
		</li>
	</ul>
	<% } %>
	<% if (Auth.CanEditThisBlogSettings || Auth.CanCreatePost || Auth.CanReadThisBlogDrafts) { %>
	<ul>
		<li class="blog-settings">
			<h3 class="blog-sidebar-title"><%= GetMessage("Header.Blog") %></h3>
			<ul>
				<% if (!string.IsNullOrEmpty(NewPostUrlTemplate) && Auth.CanCreatePost) { %>
				<li><a href="<%= Encode(ResolveTemplateUrl(NewPostUrlTemplate, replace)) %>" title="<%= GetMessage("ToolTip.NewPost") %>"><%= GetMessage("NewPost") %></a></li>
				<% } %>
				<% if (!string.IsNullOrEmpty(DraftPostListUrlTemplate) && Auth.CanReadThisBlogDrafts) { %>
				<li><a href="<%= Encode(ResolveTemplateUrl(DraftPostListUrlTemplate, replace)) %>" title="<%= GetMessage("ToolTip.Drafts") %>"><%= GetMessage("Drafts") %></a></li>
				<% } %>
				<% if (!string.IsNullOrEmpty(BlogEditUrlTemplate) && Auth.CanEditThisBlogSettings) { %>
				<li><a href="<%= Encode(ResolveTemplateUrl(BlogEditUrlTemplate, replace)) %>" title="<%= GetMessage("ToolTip.Settings") %>"><%= GetMessage("Settings") %></a></li>
				<% } %>
				<% if (!string.IsNullOrEmpty(MetaWeblogUrlTemplate) && Auth.CanWriteFilteredHtml && Auth.CanCreatePost) { %>
				<li><a href="<%= Encode(ResolveTemplateUrl(MetaWeblogUrlTemplate, replace)) %>" title="<%= GetMessage("ToolTip.MetaWeblog") %>">MetaWeblog</a></li>
				<% } %>
			</ul>
		</li>
	</ul>
	<% } %>
	<% if (IsSearch) { %>
	<ul>
		<li class="blog-tags-cloud">
			<h3 class="blog-sidebar-title">
				<%= GetMessage("Header.TagCloud") %></h3>
			<div align="center">
				<div runat="server" id="TagsCloudContainer" class="search-tags-cloud" style="width: 100%;"></div>
			</div>
		</li>
	</ul>
	<% } %>
	<% if (!string.IsNullOrEmpty(RssBlogPostsUrlTemplate)) { %>
	<% string url = Encode(ResolveTemplateUrl(RssBlogPostsUrlTemplate, replace)); %>
	<ul>
		<li class="blog-rss">
			<h3 class="blog-sidebar-title">
				<span style="float: right;"><a href="<%= url %>" title="<%= GetMessage("ToolTip.Rss") %>" class="blog-rss-icon"></a></span>
				<a href="<%= url %>" title="<%= GetMessage("ToolTip.Rss") %>"><%= GetMessage("Header.Rss") %></a>
			</h3>
		</li>
	</ul>
	<% } %>
</div>
<script runat="server">
		private string newPostUrlTemplate;
		private string draftPostListUrlTemplate;
		private string userBlogSettingsUrlTemplate;
		private string blogIndexUrl;
		private string userProfileUrlTemplate;
		private string rssBlogPostsUrlTemplate;
		private string metaWeblogUrlTemplate;
		private string blogSlug;
		private BXBlog blog;
		private BXBlogAuthorization auth;
		public readonly bool IsSearch = Bitrix.Modules.BXModuleManager.IsModuleInstalled("Search");
		protected BXParamsBag<object> replace;
	

		public string NewPostUrlTemplate
		{
			get { return newPostUrlTemplate; }
			set { newPostUrlTemplate = value; }
		}
		public string DraftPostListUrlTemplate
		{
			get { return draftPostListUrlTemplate; }
			set { draftPostListUrlTemplate = value; }
		}
		public string BlogEditUrlTemplate
		{
			get { return userBlogSettingsUrlTemplate; }
			set { userBlogSettingsUrlTemplate = value; }
		}
		public string IndexUrlTemplate
		{
			get { return blogIndexUrl; }
			set { blogIndexUrl = value; }
		}
		public string UserProfileUrlTemplate
		{
			get { return userProfileUrlTemplate; }
			set { userProfileUrlTemplate = value; }
		}
		public string RssBlogPostsUrlTemplate
		{
			get { return rssBlogPostsUrlTemplate; }
			set { rssBlogPostsUrlTemplate = value; }
		}
		public string MetaWeblogUrlTemplate
		{
			get { return metaWeblogUrlTemplate; }
			set { metaWeblogUrlTemplate = value; }
		}
		public string BlogSlug
		{
			get { return blogSlug; }
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
			if (!Visible)
				return;
			
			base.OnLoad(e);
			if (blog == null && !string.IsNullOrEmpty(BlogSlug))
			{
				string key = string.Concat("_BX_BLOG_SIDE_BAR_BLOG_", BlogSlug);
				if(BXTagCachingManager.IsTagCachingEnabled())				
					blog = BXCacheManager.MemoryCache.Application.Get(key) as BXBlog;
				
				if(blog == null)
				{
					blog = BXBlog.GetBySlug(
						BlogSlug, 
						null,
						new BXSelectAdd(
							BXBlog.Fields.Owner.User.Image
						),
						BXTextEncoder.DefaultEncoder
					);
				
					if(blog != null && BXTagCachingManager.IsTagCachingEnabled())
						BXCacheManager.MemoryCache.Application.Insert(key, blog, new BXTagCachingDependency(BXBlog.CreateTagById(blog.Id)));
				}
				
			}
			if (blog == null || !blog.Active && !BXPrincipal.Current.IsCanOperate(BXBlog.Operations.BlogApprove))
			{
				blog = null;
				Visible = false;
				return;
			}
			auth = new BXBlogAuthorization(blog);

			if (IsSearch)
			{
				UserControl control = (UserControl)LoadControl("index-tagcloud.ascx");
				control.Attributes["BlogId"] = Blog.Id.ToString();
                control.Attributes["BlogSlug"] = Blog.Slug;
                
				TagsCloudContainer.Controls.Add(control);
			}
		}
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			Visible = 
				IsSearch
				|| !string.IsNullOrEmpty(RssBlogPostsUrlTemplate)
				|| Blog.Owner != null && Blog.Owner.User != null && Blog.Owner.User.Image != null
				|| Auth.CanEditThisBlogSettings || Auth.CanCreatePost || Auth.CanReadThisBlogDrafts;
			
			replace = new BXParamsBag<object>();
			replace.Add("UserId", Blog.OwnerId);
			replace.Add("BlogSlug", Blog.Slug);
			replace.Add("BlogId", Blog.Id);

			if (!string.IsNullOrEmpty(RssBlogPostsUrlTemplate))
			{
				BXPage.RegisterLink(
					"alternate", 
					"application/rss+xml", 
					ResolveTemplateUrl(RssBlogPostsUrlTemplate, replace), 
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