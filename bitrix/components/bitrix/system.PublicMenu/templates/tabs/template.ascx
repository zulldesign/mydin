<%@ Reference VirtualPath="~/bitrix/components/bitrix/system.PublicMenu/component.ascx" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.Main.Components.SystemPublicMenuTemplate" %>
<%@ Import Namespace="Bitrix" %>

<script runat="server">
	public override string Title
	{
		get
		{
			return GetMessageRaw("Title");
		}
	}
</script>

<% 
if (Component.Menu != null)
{ 
%>
<div class="image-load-left"></div>
<div class="image-load-right"></div>
<div class="image-load-bg"></div>
<div class="blue-tabs-menu" id="blue-tabs-menu">
    <ul>
        <% 
		foreach (BXPublicMenuItem item in Component.Menu)
			if (item.IsAccessible)
			{
				%><li <%= item.IsSelected ? " class=\"selected\"" : string.Empty %>><a href="<%=Encode(BXUri.ToRelativeUri(item.Link))%>"><nobr><%=Encode(item.Title)%></nobr></a></li><%
            } 
        %>
    </ul>
</div>
<div class="menu-clear-left"></div><%
}
%>