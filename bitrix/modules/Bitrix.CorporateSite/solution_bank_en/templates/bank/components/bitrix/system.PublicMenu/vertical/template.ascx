<%@ Reference VirtualPath="~/bitrix/components/bitrix/system.PublicMenu/component.ascx" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.Main.Components.SystemPublicMenuTemplate" %>
<%@ Import Namespace="Bitrix" %>
<%  if (Component.Menu != null) { %>
	<% 
		var uriComparer = new BXUriComparer();

		Action<BXPublicMenuItemCollection, bool, BXPublicMenuItem, BXPublicMenuItem> render = null;
		bool showAll = Parameters.GetBool("Template_ShowAllLevels");
        render = delegate(BXPublicMenuItemCollection items,bool isRoot, BXPublicMenuItem selectedParent,BXPublicMenuItem parent)
        {
            int firstNum = 0; int lastNum = 0;

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
                string cssClass = String.Empty;
				bool isFull = false;
				bool tAccessible = showAll || isRoot || parent == selectedParent;
				if (!tAccessible) continue;
				
				bool containsSelectedChildren = item.Children!=null && item.Children.Exists(x => x.IsSelected);
				
				if (items[i].IsSelected && (containsSelectedChildren || isRoot) )
					cssClass = "selected";
				else 
					if (items[i]==selected && ! containsSelectedChildren )
						cssClass = "current";
                %>
                
                <% if (!isFull && !isRoot) { isFull = true; %><ul><% } %>
					<li  <% if (cssClass!=String.Empty) { %> class="<%=cssClass %>"<% } %>><% if (items[i]==selected && ! containsSelectedChildren && !isRoot) { %><b class="r0"></b><i class="selected"><%= Encode(item.Title)%></i><b class="r0"></b><% } else { %><a href=<%= item.Href %> <%= (items[i].IsSelected && (parent==null || parent.Href!=item.Href) ? "class=\"selected\"":"") %>><%= Encode(item.Title)%></a><% } %>
					<% if (item.Children != null && item.Children.Count> 0) { %> 
						<% render(item.Children, false, selected, item); %>
					<% } %>
		            </li>
	            <% if (isFull && !isRoot) { %></ul><% } %>
				
				<%
			} 
        };
    %>
    <ul class="left-menu">
    <% render(Component.Menu, true, null, null); %>
    </ul>
<% } %>
<script runat="server">
	protected override void PreLoadTemplateDefinition(Bitrix.Components.BXParametersDefinition def)
	{
		def["Template_ShowAllLevels"] = new Bitrix.Components.BXParamYesNo(GetMessageRaw("Param.ShowAllLevels"), false, Bitrix.Components.BXCategory.AdditionalSettings);
	}
</script>
