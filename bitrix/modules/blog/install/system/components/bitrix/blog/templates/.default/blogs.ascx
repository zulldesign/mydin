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
	CategoryId="<%$Results:CategoryId %>" 		
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
	ID="bloglist" 
	runat="server" 
	ComponentName="bitrix:blog.list" 
	Template=".default"
	ThemeCssFilePath="" 
	ColorCssFilePath="" 
	CategoryId="<%$Results:CategoryId %>" 
	SortByFirst="PostCount" 
	SortOrderFirst="Desc" 
	SortBySecond="Name" 
	SortOrderSecond="Asc" 
	BlogPageUrlTemplate="<%$ Results:PostListUrlTemplate %>"
	PagingRecordsPerPage="<%$ Results:BlogsPerPage %>"
	PagingPageID="<%$ Results:PageId %>" 
	PagingIndexTemplate="<%$ Results:PagingIndexTemplate %>" 
	PagingPageTemplate="<%$ Results:PagingPageTemplate %>"  
	BlogOwnerProfilePageUrlTemplate="<%$ Results:UserProfileUrlTemplate %>" 
/>