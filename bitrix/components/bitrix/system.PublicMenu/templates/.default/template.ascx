<%@ Reference VirtualPath="~/bitrix/components/bitrix/system.PublicMenu/component.ascx" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.Main.Components.SystemPublicMenuTemplate" %>
<%@ Import Namespace="Bitrix" %>
<% 
if (Component.Menu != null)
{ 
%>
<div class="left-menu">
	<%
	foreach (Bitrix.BXPublicMenuItem item in Component.Menu)
	{
		if (item.IsAccessible)
		{
			%><div class="bl"><div class="br"><div class="tl"><div class="tr">
				<a <% if(item.IsSelected){ %> class="selected" <% } %> href="<%= Encode(item.Href) %>"><%= Encode(item.Title)%></a>
			</div></div></div></div><%
		}
	}
	%>
</div><%
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

