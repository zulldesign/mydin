<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.personal/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogPersonalTemplate" %>
<bx:IncludeComponent
	ID="metaweblog" 
	runat="server" 
	ComponentName="bitrix:blog.metaWeblog" 
	Template=".default"
		
	BlogSlug="<%$ Results:BlogSlug %>" 
	
	BlogUrlTemplate="<%$ Results:PostsUrlTemplate %>" 
	PostViewUrlTemplate="<%$ Results:PostUrlTemplate %>" 
	EnableExtendedEntries="True"
/>