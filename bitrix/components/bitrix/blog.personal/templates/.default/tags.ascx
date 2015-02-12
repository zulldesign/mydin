<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.personal/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogPersonalTemplate" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>
<%@ Import Namespace="Bitrix.Blog" %>

<% if (Component.DisplayMenu) { %><asp:PlaceHolder runat="server" ID="MenuPlaceholder" /><% } %>

<bx:IncludeComponent 
	ID="Posts" 
	runat="server" 
	ComponentName="bitrix:blog.posts" 
	ThemeCssFilePath="" 
	ColorCssFilePath=""  

	Tags="<%$ Results:SearchTags %>"
    BlogSlug="<%$ Results:BlogSlug %>"

	SortBy="<%$ Results:PostsSortBy %>" 
	BlogUrlTemplate="<%$ Results:PostsUrlTemplate %>" 
	UserProfileUrlTemplate="<%$ Results:UserProfileUrlTemplate %>" 
	PostViewUrlTemplate="<%$ Results:PostUrlTemplate %>" 
	PostEditUrlTemplate="<%$ Results:PostEditUrlTemplate %>"
	PostRssUrlTemplate="<%$ Results:RssPostCommentsUrlTemplate %>"
	SearchTagsUrlTemplate="<%$ Results:SearchTagsUrlTemplate %>" 
	
	PagingPageId="<%$ Results:PageId %>"
	PagingRecordsPerPage="<%$ Results:PostsPerPage %>" 
	PagingPageTemplate="<%$ Results:PagingPageTemplate %>" 
	PagingIndexTemplate="<%$ Results:PagingIndexTemplate %>" 
/>

<% if (false) { //just to initialize the menu component after the main component %>
<bx:IncludeComponent 
	ID="Menu" 
	runat="server" 
	ComponentName="bitrix:blog.menu" 
	ThemeCssFilePath="" 
	ColorCssFilePath="" 
	
	CheckUserPermissions="True"
	BlogSlug="<%$ Results:BlogSlug %>"		
	NewPostUrlTemplate="<%$ Results:NewPostUrlTemplate %>" 
	UserBlogUrlTemplate="" 
	UserBlogSettingsUrlTemplate="<%$ Results:BlogEditUrlTemplate %>" 
	BlogIndexUrl="" 
	UserProfileUrlTemplate=""
	DraftPostListUrlTemplate="<%$ Results:DraftsUrlTemplate%>"
	Visible="<%$ Results:DisplayMenu %>"
/>
<% } %>

<%@ Reference  VirtualPath="~/bitrix/components/bitrix/blog.posts/component.ascx"  %>
<%@ Reference  VirtualPath="~/bitrix/components/bitrix/blog.menu/component.ascx"  %>
<script runat="server">
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		Menu.Load += new EventHandler(Menu_Load);
	}
	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);
		BlogPostsComponent posts = (BlogPostsComponent)Posts.Component;	
		if (posts.FatalError != BlogPostsComponent.ErrorCode.None)
			return;
		
		string rssUrl = Component.ResolveTemplateUrl(
			Component.ComponentCache["RssPostsUrlTemplate"].ToString(),
			Component.ComponentCache
		);
		if (!string.IsNullOrEmpty(rssUrl))
		{
			BXPage.RegisterLink(
				"alternate", 
				"application/rss+xml", 
				rssUrl,
				new System.Collections.Generic.KeyValuePair<string, string>("title", GetMessageRaw("PostsRssTitle"))
			);
		}
	}
	void Menu_Render(HtmlTextWriter output, Control container)
	{
		Menu.RenderControl(output);
	}
	void Menu_Load(object sender, EventArgs e)
	{
		BlogPostsComponent posts = (BlogPostsComponent)Posts.Component;	
		if (posts.FatalError != BlogPostsComponent.ErrorCode.None)
		{
			Menu.Component.Visible = false;
			return;
		}
		MenuPlaceholder.SetRenderMethodDelegate(Menu_Render);
		if (posts.HasPosts)
		{
			BXBlog blog = BXBlog.GetById(posts.ComponentCache.GetInt("DefaultBlogId", 0));
			if(blog != null)
				((BlogMenuComponent)Menu.Component).Blog = blog;
		}
	}
</script>