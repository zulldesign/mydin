<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogTemplate" %>

<% if (Component.DisplayMenu) {%>
<bx:IncludeComponent 
	ID="menu" 
	runat="server" 
	ComponentName="bitrix:blog.menu" 
	Template=".default"
	ThemeCssFilePath="" 
	ColorCssFilePath="" 
	CategoryId="<%$Parameters:CategoryId %>" 		
	NewPostUrlTemplate="<%$ Results:NewPostUrlTemplate %>" 
	UserBlogUrlTemplate="<%$ Results:PostListUrlTemplate %>" 
	UserBlogSettingsUrlTemplate="<%$ Results:BlogEditUrlTemplate %>" 
	BlogIndexUrl="<%$ Results:IndexUrlTemplate %>" 
	UserProfileUrlTemplate="<%$ Results:UserProfileUrlTemplate %>"
	DraftPostListUrlTemplate="<%$ Results:DraftPostListUrlTemplate%>" 
	Visible="<%$ Results:DisplayMenu%>"
	NewBlogUrlTemplate="<%$ Results:NewBlogUrlTemplate %>"
/>
<%} %>
<bx:IncludeComponent 
	ID="blogpost" 
	runat="server" 
	ComponentName="bitrix:blog.post" 
	Template=".default"
	ThemeCssFilePath="" 
	ColorCssFilePath="" 
	CategoryId="<%$Parameters:CategoryId %>" 
	BlogSlug="<%$ Results:BlogSlug %>" 
	PostId="<%$ Results:PostId %>" 
	RedirectUrlTemplate="" 
	BlogUrlTemplate="<%$ Results:PostListUrlTemplate %>" 
	UserProfileUrlTemplate="<%$ Results:UserProfileUrlTemplate %>" 
	PostViewUrlTemplate="<%$ Results:PostUrlTemplate %>" 
	PostEditUrlTemplate="<%$ Results:PostEditUrlTemplate %>" 
	SearchTagsUrlTemplate="<%$ Results:SearchTagsUrlTemplate %>" 
	RssUrlTemplate="<%$ Results:RssBlogPostCommentsUrlTemplate %>"
	
	SetMasterTitle="<%$ Results:SetMasterTitle %>"
/>

<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.post.comments/component.ascx" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>
<% if (((BlogPostCommentsComponent)postcomments.Component).FatalError == BlogPostCommentsComponent.ErrorCode.None) { %>

<bx:IncludeComponent 
	ID="postcomments" 
	runat="server" 
	ComponentName="bitrix:blog.post.comments" 
	Template=".default"
	ThemeCssFilePath="" 
	ColorCssFilePath="" 
	CategoryId="<%$Parameters:CategoryId %>" 	  
	BlogUrlTemplate="<%$ Results:PostListUrlTemplate %>" 
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
<%} %>

