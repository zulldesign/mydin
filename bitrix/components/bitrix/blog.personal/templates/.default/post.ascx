<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.personal/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogPersonalTemplate" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>

<% if (Component.DisplayMenu) { %><asp:PlaceHolder runat="server" ID="MenuPlaceholder" /><% } %>

<bx:IncludeComponent 
	ID="Post" 
	runat="server" 
	ComponentName="bitrix:blog.post" 
	ThemeCssFilePath="" 
	ColorCssFilePath="" 

	BlogSlug="<%$ Results:BlogSlug %>" 
	PostId="<%$ Results:PostId %>" 
	RedirectUrlTemplate="" 
	BlogUrlTemplate="<%$ Results:PostsUrlTemplate %>" 
	UserProfileUrlTemplate="<%$ Results:UserProfileUrlTemplate %>" 
	PostViewUrlTemplate="<%$ Results:PostUrlTemplate %>" 
	PostEditUrlTemplate="<%$ Results:PostEditUrlTemplate %>" 
	SearchTagsUrlTemplate="<%$ Results:SearchTagsUrlTemplate %>" 
	RssUrlTemplate="<%$ Results:RssPostCommentsUrlTemplate %>"
	
	SetMasterTitle="<%$ Results:SetMasterTitle %>"
/>

<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.post.comments/component.ascx" %>

<bx:IncludeComponent 
	ID="Comments" 
	runat="server" 
	ComponentName="bitrix:blog.post.comments" 
	ThemeCssFilePath="" 
	ColorCssFilePath="" 
		  
	BlogUrlTemplate="<%$ Results:PostsUrlTemplate %>" 
	UserProfileUrlTemplate="<%$ Results:UserProfileUrlTemplate %>" 
	PostViewUrlTemplate="<%$ Results:PostUrlTemplate %>" 
	PostEditUrlTemplate="<%$ Results:PostEditUrlTemplate %>"
	CommentUrlTemplate="<%$ Results:CommentReadUrlTemplate %>"
	BlogSlug="<%$ Results:BlogSlug %>" 
	PostId="<%$ Results:PostId %>"
	CommentId="<%$ Results:CommentId %>"
	PagingPageID="<%$ Results:PageId %>"
	PagingRecordsPerPage="<%$ Results:CommentsPerPage %>"
	PagingIndexTemplate="<%$ Results:PostUrlTemplate %>" 
	PagingPageTemplate="<%$ Results:PostPageUrlTemplate %>" 
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

<%@ Reference  VirtualPath="~/bitrix/components/bitrix/blog.post/component.ascx"  %>
<%@ Reference  VirtualPath="~/bitrix/components/bitrix/blog.menu/component.ascx"  %>
<script runat="server">
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		Comments.Load += new EventHandler(Comments_Load);
		Menu.Load += new EventHandler(Menu_Load);
	}
	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);
		BlogPostComponent post = (BlogPostComponent)Post.Component;
		if (post.FatalError != BlogPostComponent.ErrorCode.None)
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
		rssUrl = Component.ResolveTemplateUrl(
			Component.ComponentCache["RssPostCommentsUrlTemplate"].ToString(),
			Component.ComponentCache
		);
		if (!string.IsNullOrEmpty(rssUrl))
		{
			BXPage.RegisterLink(
				"alternate", 
				"application/rss+xml", 
				rssUrl,
				new System.Collections.Generic.KeyValuePair<string, string>("title", string.Format(GetMessageRaw("PostCommentsRssTitle"), post.BlogPost.TextEncoder.Decode(post.BlogPost.Title)))
			);
		}
	}
	void Comments_Load(object sender, EventArgs e)
	{
		BlogPostComponent post = (BlogPostComponent)Post.Component;
		if (post.FatalError != BlogPostComponent.ErrorCode.None)
			return;	
		BlogPostCommentsComponent comments = (BlogPostCommentsComponent)Comments.Component;
		comments.Blog = post.Blog;
		comments.BlogPost = post.BlogPost;
		
		if (post.FatalError != BlogPostComponent.ErrorCode.None
			|| !post.Post.Post.IsPublished
			|| post.Post.Post.DatePublished > DateTime.Now)
			comments.Visible = false;
	}
	void Menu_Load(object sender, EventArgs e)
	{
		BlogPostComponent post = (BlogPostComponent)Post.Component;
		if (post.FatalError != BlogPostComponent.ErrorCode.None)
		{
			Menu.Component.Visible = false;
			return;
		}
		MenuPlaceholder.SetRenderMethodDelegate(Menu_Render);
		BlogMenuComponent menu = (BlogMenuComponent)Menu.Component;	
		menu.Blog = post.Blog;
	}
	void Menu_Render(HtmlTextWriter output, Control container)
	{
		Menu.RenderControl(output);
	}
</script>


