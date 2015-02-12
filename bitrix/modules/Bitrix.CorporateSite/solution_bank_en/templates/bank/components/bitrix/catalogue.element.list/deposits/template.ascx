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

<% if ( Component.Items.Count == 0){

    return;
   } %>


<bx:IncludeComponent runat="server" ID="HeaderPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="top" />




<table class="data-table" cellspacing="0">
	<tr>
	<th valign="top"><%=GetMessage("DepositName")%></th>
	<% 
		CatalogueElementListComponent.ElementListItem listItem = Component.Items[0];

		foreach (Bitrix.IBlock.Components.CatalogueElementListComponent.ElementListItemProperty prop in listItem.Properties)
		{
			if ( Component.ShowProperties.Contains(prop.Code)) 
			{ %>
		<th valign="top"><%=prop.Name%></th>
	<% }
		} %>
	<%--	<th valign="top">Мин. сумма, руб.</th>
		<th valign="top">Попол- нение</th>
		<th valign="top">Начисление процентов</th>
		<th valign="top">Cтавка</th>
		<th valign="top">Открытие в иностранной валюте</th>--%>
	</tr>
<%
int itemPerLine = 1;
int itemNumber = 0;
foreach (CatalogueElementListComponent.ElementListItem item in Component.Items)
{
	
%>
<% string itemContainerId = GetItemContainerClientID(item.ElementId); %>
<tr id="<%= itemContainerId %>" <%= ( itemNumber % 2 == 0 ? " class = \"alt-row\"":"" )%>>
	<td>
		<a href="<%=item.ElementDetailUrl %>"><%= item.Element.Name%></a>
		<% RenderElementToolbar(item.Element, itemContainerId); %>
	</td>
	<% foreach (Bitrix.IBlock.Components.CatalogueElementListComponent.ElementListItemProperty prop in item.Properties)
	{
	if ( Component.ShowProperties.Contains(prop.Code)) {
			 %>
	<td><%=prop.DisplayValue%></td>
	<%}
	} %>
</tr>


<%itemNumber++;
} %>
</table>


<bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager" CurrentPosition="bottom" Template="<%$ Parameters:PagingTemplate %>"/>
