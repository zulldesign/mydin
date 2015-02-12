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
<% 
if ( Parameters.GetBool("SetPageTitle",false)){
%>
    <h1><%=Encode(Component.Page.Title) %></h1>
<%} %>
<%= GetMessageRaw("Content") %>
<p><%= GetMessageRaw("WeRequire") %>:</p>

<% if ( Component.Items.Count == 0){ %>
<p><i><%=GetMessage("NoVacancies") %></i></p>
<%
    return;
   } %>

<ul>
<%
int itemPerLine = 1;
int itemNumber = 0;
foreach (CatalogueElementListComponent.ElementListItem listItem in Component.Items)
{
	
%>
<% string itemContainerId = GetItemContainerClientID(listItem.ElementId); %>
<li id="<%= itemContainerId %>">
	<a href="<%=listItem.ElementDetailUrl %>"><%= listItem.Element.Name%></a>
	<% RenderElementToolbar(listItem.Element, itemContainerId); %>
</li>


<%} %>
</ul>

<bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager" CurrentPosition="bottom" Template="<%$ Parameters:PagingTemplate %>"/>

