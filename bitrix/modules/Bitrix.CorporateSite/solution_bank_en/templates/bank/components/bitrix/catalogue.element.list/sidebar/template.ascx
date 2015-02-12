<%@ Reference Control="~/bitrix/components/bitrix/catalogue.element.list/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.IBlock.Components.CatalogueElementListTemplate" %>
<%@ Import Namespace="Bitrix.IBlock.Components" %>

<script runat="server">
	//Template name
	public override string Title
	{
		get { return GetMessageRaw("TemplateTitle");}
	}
</script>

<%
if (Component.isErrorOccured)
{
	%><span class="errortext"><%= Component.errorMessage%></span><%
	return;
}
else if (Component.Items == null)
   return;
%>
<h5><%=GetMessage("Deposits") %></h5>
<dl class="block-list">

<%

foreach (CatalogueElementListComponent.ElementListItem listItem in Component.Items)	
{
	
%>
				<% string itemContainerId = GetItemContainerClientID(listItem.ElementId); %>
				<dt id="<%= itemContainerId %>">
				<%
				if (!String.IsNullOrEmpty(listItem.ElementDetailUrl))
				{
					%><a href="<%= listItem.ElementDetailUrl %>" enableajax="true"><%=listItem.Element.Name %></a><%
				}
				else
				{
					%><%=listItem.Element.Name %><%
				}
				%>
				<% RenderElementToolbar(listItem.Element, itemContainerId); %>
                </dt>
				<dd>
				<%= listItem.Element.PreviewText %>
                </dd>		
<%	
	
}

%>

</dl>