<%@ Reference Control="~/bitrix/components/bitrix/news.list/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="NewsListTemplate" %>
<%@ Import namespace="Bitrix" %>
<%@ Import namespace="Bitrix.IBlock" %>
<%@ Import namespace="System.Collections.Generic" %>

<%
if (Component.Items == null)
   return;
%>
<h3><%=GetMessage("CompanyNews") %></h3>

<dl class="block-list">
<%	   
foreach (NewsListItem item in Component.Items)
{%>
	<% string itemContainerId = GetItemContainerClientID(item.ElementId); %>
    <dt id="<%= itemContainerId %>">
        <%= item.DisplayDate %>
		<% RenderElementToolbar(item.Element, itemContainerId); %>
    </dt>
    <dd><a href="<%=item.DetailUrl %>"><%=item.PreviewText %></a></dd>
<%}%>
</dl>
