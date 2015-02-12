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
	ID="newblog" 
	runat="server" 
	ComponentName="bitrix:blog.edit" 
	Template=".default" 
	ThemeCssFilePath="" 
	ColorCssFilePath="" 
	RedirectUrlTemplate="<%$ Results:RedirectUrlTemplate %>" 
	BlogSlug="<%$ Results:BlogSlug %>"
	CategoryId="<%$Parameters:CategoryId %>" 
	BlogAbsoluteUrlTemplate="<%$ Results:BlogAbsoluteUrlTemplate %>" 
/>