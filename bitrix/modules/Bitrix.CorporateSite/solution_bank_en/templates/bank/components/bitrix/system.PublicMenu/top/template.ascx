<%@ Reference VirtualPath="~/bitrix/components/bitrix/system.PublicMenu/component.ascx" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.Main.Components.SystemPublicMenuTemplate" %>
<%@ Import Namespace="Bitrix" %>
<%  if (Component.Menu != null && Component.Menu.Count > 0) { %>
	<% 
        var uriComparer = new BXUriComparer();
        Action<BXPublicMenuItemCollection,bool> render = null;
        render = delegate(BXPublicMenuItemCollection items,bool isRoot)
        {
            BXPublicMenuItem selected = null;
            for (int i = 0; i < items.Count; i++)
            {
                if (!items[i].IsAccessible) continue;

                if (items[i].IsSelected && (selected == null || uriComparer.Compare(items[i].Link, selected.Link) >= 0))
                    selected = items[i];
            }

            for (int i = 0; i < items.Count;i++ )
            {
                var item = items[i];
                if (!item.IsAccessible) continue;
       
                %>
		            <li <%=( item == selected ? "class=\"selected\"" : "" ) %>>
		            <b class="r1"></b><b class="r0"></b>
		            <a href="<%= Encode(item.Href) %>"><%= Encode(item.Title)%></a>
		            <% if (item.Children != null && item.Children.Count > 0)
                 { %> 
		            <ul>
                    <% render(item.Children,false); %>
                    </ul>
		            <% }%>
		            <b class="r0"></b><b class="r1"></b>
		            </li>
	            <% 
        } 
        };
    %>
    <ul id="top-menu">
    <% render(Component.Menu,true); %>
    </ul>
<% } %>