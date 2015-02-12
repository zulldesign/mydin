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
	ID="postedit" 
	runat="server" 
	ComponentName="bitrix:blog.post.form" 
	Template=".default"
	ThemeCssFilePath="" 
	ColorCssFilePath="" 
	
	BlogSlug="<%$ Results:BlogSlug %>" 
	PostId="<%$ Results:PostId %>" 
	PublishUrlTemplate="<%$ Results:PostUrlTemplate %>" 
	DraftUrlTemplate="<%$ Results:DraftPostListUrlTemplate %>" 
	RedirectUrlTemplate="<%$ Results:RedirectUrlTemplate %>" 
	CutThreshold = "<%$ Parameters:PostCutThreshold%>"
/>
