<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogTemplate" %>

<bx:IncludeComponent
	ID="metaweblog" 
	runat="server" 
	ComponentName="bitrix:blog.metaWeblog" 
	Template=".default"
		
	BlogSlug="<%$ Results:BlogSlug %>" 
	
	BlogUrlTemplate="<%$ Results:PostListUrlTemplate %>" 
	PostViewUrlTemplate="<%$ Results:PostUrlTemplate %>" 
	EnableExtendedEntries="True"
/>