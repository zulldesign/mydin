<%@ Reference Control="~/bitrix/components/bitrix/blog.menu/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.Blog.Components.BlogMenuTemplate" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>

<%
	if (Component.FatalError != BlogMenuComponent.ErrorCode.None || Component.MenuItems.Count < 1)
		return;
%>

<div class="blog-content">
	<div class="blog-menu-box">
			<% for (int i = 0; i < Component.MenuItems.Count; i++) { %>
			<% BlogMenuComponent.MenuItem item = Component.MenuItems[i]; %>
			<span class="blog-menu-item<%= (i == 0) ? " blog-menu-item-first" : ""%><%= (i == Component.MenuItems.Count - 1) ? " blog-menu-item-last" : "" %><%= (i == Component.MenuItems.Count - 1) ? " blog-menu-item-last" : "" %> blog-menu-<%= item.ClassName %>" ><a title="<%= item.TooltipHtml %>" href="<%= item.Href %>"><span><%= item.TitleHtml %></span></a> </span>
			<% } %>
	</div>
</div>