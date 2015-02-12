<%@ Reference VirtualPath="~/bitrix/components/bitrix/system.PublicMenu/component.ascx" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.Main.Components.SystemPublicMenuTemplate" %>
<%@ Import Namespace="Bitrix" %>
<%  if (Component.Menu != null) { %>
	<% 
		var uriComparer = new BXUriComparer();

        Action<BXPublicMenuItemCollection,bool> render = null;
        render = delegate(BXPublicMenuItemCollection items,bool isRoot)
        {
            int firstNum = 0; int lastNum = 0;
        	BXPublicMenuItem selected = null;

            for (int i = 0; i < items.Count; i++)
            {
                if (!items[i].IsAccessible) continue;
                if (firstNum == 0) firstNum = i;
                lastNum = i;
                if (items[i].IsSelected && (selected == null || uriComparer.Compare(items[i].Link, selected.Link) >= 0))
				    selected = items[i];
            }
            
            for (int i = 0; i < items.Count;i++ )
            {
                var item = items[i];
                if (!item.IsAccessible) continue;
                string cssClass = String.Empty;
                string rootClass =items[i].IsSelected ? "root-item-selected":"root-item";
                if (i == firstNum) cssClass += (cssClass != string.Empty ? " first-item":"first-item");
                if (items[i].IsSelected) cssClass += (cssClass != string.Empty ? " selected" : "selected");
                if (i == lastNum) cssClass += (cssClass != string.Empty ? " last-item" : "last-item");
                
                %>
		            <li <% if (!String.IsNullOrEmpty(cssClass)) { %>class="<%=cssClass %>"<% } %>>
		            <a <% if (isRoot){ %> class="<%=rootClass %>"<%} %> href="<%= Encode(item.Href) %>"><span><%= Encode(item.Title)%></span></a>
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
    <ul id="horizontal-multilevel-menu">
    <% render(Component.Menu,true); %>
    </ul>
<% } %>