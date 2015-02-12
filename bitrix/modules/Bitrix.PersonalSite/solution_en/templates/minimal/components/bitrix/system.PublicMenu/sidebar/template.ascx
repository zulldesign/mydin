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
	<% for (int i = 0; i < Component.Menu.Count; i++) { %>
		<% 
			var item = Component.Menu[i];
			if (!item.IsAccessible) continue; 
		
			string css = 
				(item == selected ? "selected " : "")
				+ (i == 0 ? "first-item " : "")
				+ (i == Component.Menu.Count - 1 ? "last-item " : "");
		%>
		<li <% if (!string.IsNullOrEmpty(css)) { %>class="<%= css %>"<% } %>><a href="<%= Encode(item.Href) %>"><%= Encode(item.Title) %></a></li>
	<% } %>
	</ul>
	<% } %>
<% } %>