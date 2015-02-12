<%@ Reference VirtualPath="~/bitrix/components/bitrix/system.PublicBreadCrumb/component.ascx" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.Main.Components.SystemPublicBreadCrumbTemplate" %>
<%@ Import Namespace="Bitrix" %>
<% 
if (Component.BreadCrumb != null)
{ 
	%><ul class="breadcrumb-navigation"><% 
	
	for (int i = 0; i < Component.BreadCrumb.Length; i++)
	{
		BXPublicMenuItem item = Component.BreadCrumb[i];
		if (!item.IsAccessible)
			continue;

		if (i > 0)
		{
			%><li><span>&nbsp;&gt;&nbsp;</span></li><%
		}
		
		%><li><a href="<%= Encode(item.Href) %>" title="<%= Encode(item.Title)%>"><%= Encode(item.Title)%></a></li><%
		
	}
	%>
	</ul><%
}
%>

<script runat="server" type="text/C#">
	protected override void PrepareDesignMode()
	{
		MinimalWidth = "175";
		MinimalHeight = "45";
		StartHeight = "170px";
	}
</script>

