<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.personal/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogPersonalTemplate" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>

<% if (Component.DisplayMenu) { %><asp:PlaceHolder runat="server" ID="MenuPlaceholder" /><% } %>

<bx:IncludeComponent 
	ID="Posts"
	runat="server" 
	ComponentName="bitrix:blog.post.list" 
	ThemeCssFilePath="" 
	ColorCssFilePath="" 
	
	BlogSlug="<%$ Results:BlogSlug %>" 
	PublishMode="<%$ Results:PublishMode %>" 
	BlogUrlTemplate="<%$ Results:PostsUrlTemplate %>" 
	UserProfileUrlTemplate="<%$ Results:UserProfileUrlTemplate %>" 
	PostViewUrlTemplate="<%$ Results:PostUrlTemplate %>" 
	PostRssUrlTemplate="<%$ Results:RssPostCommentsUrlTemplate %>"
	PostEditUrlTemplate="<%$ Results:PostEditUrlTemplate %>"
	SearchTagsUrlTemplate="<%$ Results:SearchTagsUrlTemplate %>" 
	PagingRecordsPerPage="<%$ Results:PostsPerPage %>" 
	PagingPageID="<%$ Results:PageId %>" 
	PagingIndexTemplate="<%$ Results:PagingIndexTemplate %>" 
	PagingPageTemplate="<%$ Results:PagingPageTemplate %>" 
	
	SetMasterTitle="<%$ Results:SetMasterTitle %>"
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

<%@ Reference  VirtualPath="~/bitrix/components/bitrix/blog.post.list/component.ascx"  %>
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
		BlogPostListComponent postList = (BlogPostListComponent)Posts.Component;	
		if (postList.FatalError != BlogPostListComponent.ErrorCode.None)
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
				
		var url = Component.ComponentCache.GetString("MetaWeblogManifestUrlTemplate");
		if (!string.IsNullOrEmpty(url))
		{
			var replace = new Bitrix.DataTypes.BXParamsBag<object>
			{
				{ "BlogSlug", postList.BlogSlug }
			};
			
			Bitrix.UI.BXPage.RegisterLink(
				"wlwmanifest",
				"application/wlwmanifest+xml",
				postList.ResolveTemplateUrl(url, replace)
			);
		}
	}
	void Menu_Render(HtmlTextWriter output, Control container)
	{
		Menu.RenderControl(output);
	}
	void Menu_Load(object sender, EventArgs e)
	{
		BlogPostListComponent postList = (BlogPostListComponent)Posts.Component;	
		if (postList.FatalError != BlogPostListComponent.ErrorCode.None)
		{
			Menu.Component.Visible = false;
			return;
		}
		MenuPlaceholder.SetRenderMethodDelegate(Menu_Render);
		BlogMenuComponent menu = (BlogMenuComponent)Menu.Component;
		if (postList.PostPublishMode == BlogPostListComponent.PublishMode.Draft)
			menu.Parameters["DraftPostListUrlTemplate"] = "";
		menu.Blog = postList.Blog;
	}
</script>