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

<div class="catalog-list">

<%

foreach (CatalogueElementListComponent.ElementListItem listItem in Component.Items)	
{
%>
	<% string itemContainerId = GetItemContainerClientID(listItem.ElementId); %>
	<div id="<%= itemContainerId %>" class="catalog-item">
				<%
				if (listItem.Element.PreviewImage != null) 
				{
					%><div class="catalog-item-image">
						<a href="<%= listItem.ElementDetailUrl %>"><img border="0" src="<%= listItem.Element.PreviewImage.FilePath %>" width="<%= Math.Min(listItem.Element.PreviewImage.Width, 100) %>" alt="<%= listItem.Element.PreviewImage.Description %>" enableajax="true" /></a><br />
					</div><%
				}
				else if (listItem.Element.DetailImage != null)
				{
					 %><div class="catalog-item-image">
						<a href="<%= listItem.ElementDetailUrl %>"><img border="0" src="<%= listItem.Element.DetailImage.FilePath %>" width="<%= Math.Min(listItem.Element.DetailImage.Width, 100) %>"  alt="<%= listItem.Element.DetailImage.Description %>" enableajax="true" /></a><br />
						</div><%
				}
				%>
					
				<div class="catalog-item-title">
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
				</div>
				<div class="catalog-item-desc-float">
				    <%
				    foreach (CatalogueElementListComponent.ElementListItemProperty property in listItem.Properties)
				    {
					    if (!String.IsNullOrEmpty(property.DisplayValue) && Component.ShowProperties.Contains(property.Code))
					    {
						%><%= property.Name %>: <%= property.DisplayValue %><br /><%
					    }
				    }
				    %>
					
				    <%= listItem.Element.PreviewText %>
				</div>	
			<% RenderElementToolbar(listItem.Element, itemContainerId); %>	
			</div>
	<%

}%>

<bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager" CurrentPosition="bottom" Template="<%$ Parameters:PagingTemplate %>"/>

</div>