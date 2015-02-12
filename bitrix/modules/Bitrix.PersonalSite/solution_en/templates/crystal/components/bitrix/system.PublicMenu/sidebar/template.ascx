<%@ Reference VirtualPath="~/bitrix/components/bitrix/system.PublicMenu/component.ascx" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.Main.Components.SystemPublicMenuTemplate" %>
<%@ Import Namespace="Bitrix" %>
<%  if (Component.Menu != null) { %>
	<% 
		bool show = false;
		var uriComparer = new BXUriComparer();
		BXPublicMenuItem selected = null;
		foreach (var item in Component.Menu)
		{
			if (!item.IsAccessible)
				continue;
			show = true;		
			if (!item.IsSelected)
				continue;
			if (selected == null || uriComparer.Compare(item.Link, selected.Link) >= 0)
				selected = item;
		}
	%>
	<% if (show) { %>
	<ul id="user-menu">
	<% foreach (var item in Component.Menu) { %>
		<% if (!item.IsAccessible) continue; %>
		<li <% if (item == selected) { %>class="selected"<% } %>>
			<b class="r2"></b><b class="r1"></b><b class="r0"></b>
			<a href="<%= Encode(item.Href) %>"><%= Encode(item.Title) %></a>
			<b class="r0"></b><b class="r1"></b><b class="r2"></b>
		</li>
	<% } %>
	</ul>
	<% } %>
<% } %>