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

<h3><%=GetMessage("Services") %></h3>

<%

foreach (CatalogueElementListComponent.ElementListItem listItem in Component.Items)
{

            %>
			<% string itemContainerId = GetItemContainerClientID(listItem.ElementId); %>			
            <div id="<%= itemContainerId %>" class="product">
                <div class="product-overlay"></div>
                <div class="product-image" style="<%=(listItem.Element.PreviewImage != null) ? "background-image: url(" + listItem.Element.PreviewImage.FilePath + ")" :"" %>"></div>
                <a class="product-desc" href="<%= listItem.ElementDetailUrl %>">
                    <b><%=listItem.Element.Name %></b>
                    <p><%=listItem.Element.PreviewText %></p>
                </a>
			<% RenderElementToolbar(listItem.Element, itemContainerId); %>
            </div>
            <%

}
%>
