<%@ Reference Control="~/bitrix/components/bitrix/pmessages.menu/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.CommunicationUtility.Components.PrivateMessagesMenuTemplate" %>
<%@ Import Namespace="Bitrix.CommunicationUtility.Components" %>
<%@ Import Namespace="Bitrix.CommunicationUtility" %>

<%
    if (Component.FatalError != PrivateMessagesMenuComponent.ErrorCode.None)
		return;

%>

<div class="pmessages-content">
<% if (Component.MenuItems.Count > 0)
   {  %>
	<div class="forum-header-box<%= (Component.Folders.Count <= 0 ? " forum-header-box-single":"")%>">
		<div class="forum-header-menu">
			<% for (int i = 0; i < Component.MenuItems.Count; i++)
      { %>
			<% PrivateMessagesMenuComponent.MenuItem item = Component.MenuItems[i]; %>
			<span class="forum-menu-item<%= (i == 0) ? " forum-menu-item-first" : ""%><%= (i == Component.MenuItems.Count - 1) ? " forum-menu-item-last" : "" %><%= (i == Component.MenuItems.Count - 1) ? " forum-menu-item-last" : "" %> forum-menu-<%= item.ClassName %>"><a title="<%= item.TooltipHtml %>" href="<%= item.Href %>"><span><%= item.TitleHtml%></span></a>&nbsp;</span>
			<% } %>
		</div>
	</div>
	<% } %>
<% if (Component.ShowFolders && Component.Auth.CanManageFolders && Component.Folders.Count > 0)
   {%>
	<div class="forum-info-box <%=(Component.MenuItems.Count > 0) ? "forum-menu-box":""%>">
		<div class="forum-info-box-inner">

			<div class="pmessages-menu-items">

<%
    for (int i = 0; i < Component.Folders.Count; i++)
    {
        Bitrix.CommunicationUtility.Components.PrivateMessagesMenuComponent.FolderInfo folder = Component.Folders[i];
                    %>
				<span class="forum-menu-item<%= (i == 0) ? " forum-menu-item-first" : ""%><%= (i == Component.Folders.Count - 1) ? " forum-menu-item-last" : "" %><%= (i == Component.Folders.Count - 1) ? " forum-menu-item-last" : "" %>">&nbsp<a href="<%=folder.Href%>"><%=folder.Folder.Title%></a> </span>
				<% } %>
       
			</div>
		</div>
	</div>
<%} %>
</div>