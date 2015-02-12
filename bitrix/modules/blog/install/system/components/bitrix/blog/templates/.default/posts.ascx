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
	ID="posts" 
	runat="server" 
	ComponentName="bitrix:blog.posts" 
	Template=".default"
	ThemeCssFilePath="" 
	ColorCssFilePath=""  
	CategoryId="<%$Parameters:CategoryId %>" 
	SortBy="<%$ Results:PostsSortBy %>" 
	BlogUrlTemplate="<%$ Results:PostListUrlTemplate %>" 
	UserProfileUrlTemplate="<%$ Results:UserProfileUrlTemplate %>" 
	PostViewUrlTemplate="<%$ Results:PostUrlTemplate %>"
	PostEditUrlTemplate="<%$ Results:PostEditUrlTemplate %>" 
	PostRssUrlTemplate="<%$ Results:RssBlogPostCommentsUrlTemplate %>"
	SearchTagsUrlTemplate="<%$ Results:SearchTagsUrlTemplate %>" 
	PagingPageId="<%$ Results:PageId %>"
	PagingRecordsPerPage="<%$ Results:PostsPerPage %>" 
	PagingPageTemplate="<%$ Results:PagingPageTemplate %>" 
	PagingIndexTemplate="<%$ Results:PagingIndexTemplate %>" 
/>