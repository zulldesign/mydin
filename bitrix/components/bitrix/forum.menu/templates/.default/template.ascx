<%@ Reference Control="~/bitrix/components/bitrix/forum.menu/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.Forum.Components.ForumMenuTemplate" %>
<%@ Import Namespace="Bitrix.Forum.Components" %>

<div class="forum-content">
	<div class="forum-header-box">
		<div class="forum-header-menu">
			<% for (int i = 0; i < Component.MenuItems.Count; i++) { %>
			<% ForumMenuComponent.MenuItem item = Component.MenuItems[i]; %>
			<span class="forum-menu-item<%= (i == 0) ? " forum-menu-item-first" : ""%><%= (i == Component.MenuItems.Count - 1) ? " forum-menu-item-last" : "" %><%= (i == Component.MenuItems.Count - 1) ? " forum-menu-item-last" : "" %> forum-menu-<%= item.ClassName %>"><a title="<%= item.TooltipHtml %>" href="<%= item.Href %>"><span><%= item.TitleHtml %></span></a>&nbsp;</span>
			<% } %>
		</div>
	</div>
	
	<div class="forum-info-box forum-menu-box">
		<div class="forum-info-box-inner">
			<% if (Component.SubMenuItems.Count > 0) { %>
			<div class="forum-menu-items">
				<% for (int i = 0; i < Component.SubMenuItems.Count; i++) { %>
				<% ForumMenuComponent.MenuItem item = Component.SubMenuItems[i]; %>
				<span class="forum-menu-item<%= (i == 0) ? " forum-menu-item-first" : ""%><%= (i == Component.SubMenuItems.Count - 1) ? " forum-menu-item-last" : "" %><%= (i == Component.SubMenuItems.Count - 1) ? " forum-menu-item-last" : "" %> forum-menu-<%= item.ClassName %>">&nbsp;<a title="<%= item.TooltipHtml %>" href="<%= item.Href %>"><span><%= item.TitleHtml %></span></a></span>
				<% } %>
			</div>
			<%} %>
			<div class="forum-user-auth-info">
				<% if (Component.CurrentUser != null) {%>
				<span><%= Component.UserMenuText %></span>
				<%} else { %>
				<span><%= Component.GuestMenuText %></span>
				<%} %>
			</div>
		</div>
	</div>
</div>