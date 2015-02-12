<%@ Reference VirtualPath="~/bitrix/components/bitrix/system.PublicMenu/component.ascx" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.Main.Components.SystemPublicMenuTemplate" %>
<%@ Import Namespace="Bitrix" %>
<%  if (Component.Menu != null) { %>
	<% 

        Action<BXPublicMenuItemCollection,bool> render = null;
        render = delegate(BXPublicMenuItemCollection items,bool isRoot)
        {

            for (int i = 0; i < items.Count;i++ )
            {
                var item = items[i];
                if (!item.IsAccessible) continue; 
                

                %>
		            <li>
		            <a href="<%= Encode(item.Href) %>"><span><%= Encode(item.Title)%></span></a>
		            <% if (item.Children != null && item.Children.Count > 0)
                 { %> 
		            <ul>
                    <% render(item.Children,false); %>
                    </ul>
		            <% }%>
		            </li>
	            <% 
        } 
        };
    %>
    <ul id="footer-links">
    <% render(Component.Menu,true); %>
    </ul>
<% } %>