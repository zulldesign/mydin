<%@ Reference VirtualPath="~/bitrix/components/bitrix/system.PublicMenu/component.ascx" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.Main.Components.SystemPublicMenuTemplate" %>
<%@ Import Namespace="Bitrix" %>
<%  if (Component.Menu != null && Component.Menu.Count > 0)
	{ %>
	<% 
		var uriComparer = new BXUriComparer();
		BXPublicMenuItem selected = null;
		foreach (var item in Component.Menu)
		{
			if (!item.IsAccessible || !item.IsSelected)
				continue;
			if (selected == null || uriComparer.Compare(item.Link, selected.Link) >= 0)
				selected = item;
		}
	%>
	<ul id="submenu">
	<% foreach (var item in Component.Menu) { %>
		<% if (!item.IsAccessible) continue; %>
		<li <% if (item == selected) { %>class="selected"<% } %>><a href="<%= Encode(item.Href) %>"><span><%= Encode(item.Title) %></span></a></li>
	<% } %>
	</ul>
	<div id="submenu-border"></div>
<% } %>