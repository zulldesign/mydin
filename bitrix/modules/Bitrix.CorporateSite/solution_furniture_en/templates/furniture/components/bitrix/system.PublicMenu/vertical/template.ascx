<%@ Reference VirtualPath="~/bitrix/components/bitrix/system.PublicMenu/component.ascx" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.Main.Components.SystemPublicMenuTemplate" %>
<%@ Import Namespace="Bitrix" %>
<% 
if (Component.Menu != null && Component.Menu.Count > 0)
{ 
%>
<div class="content-block">
<div class="content-block-inner">
<ul id = "left-menu">
	<%
    var uriComparer = new BXUriComparer();
    int firstNum=-1,lastNum=0;
    BXPublicMenuItem selected = null;
    for (int i = 0;i< Component.Menu.Count;i++)
        if (Component.Menu[i].IsAccessible)
        {
            if (firstNum == -1) firstNum = i;
            lastNum = i;
            if (Component.Menu[i].IsSelected && (selected == null || uriComparer.Compare(Component.Menu[i].Link, selected.Link) >= 0))
                selected = Component.Menu[i];
        }
            
	for (int i = 0;i< Component.Menu.Count;i++)
	{
        Bitrix.BXPublicMenuItem item  = Component.Menu[i];
        
		if (item.IsAccessible)
		{
            string cssClass = String.Empty;
            if (i == firstNum) cssClass += "first-item";
            if (item==selected) cssClass +=(cssClass!=string.Empty ? " selected":"selected");

            if (i == lastNum) cssClass += (cssClass != string.Empty ? " last-item" : "last-item");
			%>
				<li <% if(cssClass!=String.Empty){ %> class="<%=cssClass%>" <% } %>><a href="<%= Encode(item.Href) %>"><%= Encode(item.Title)%></a></li>
			<%
		}
	}
	%>
    </ul>
</div>
</div>
    <%
}
%>

<script runat="server" type="text/C#">
	public override string Title
	{
		get
		{
			return GetMessageRaw("Title");
		}
	}
</script>

