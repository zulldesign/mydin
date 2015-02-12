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
    else if (Component.Items == null || Component.Items.Count == 0)
        return;
	                                                                       
%>

<% if (Component.Items.Count > 0) { %>
<% var item = Component.Items[(new Random()).Next(Component.Items.Count)]; %>
<% string itemContainerId = GetItemContainerClientID(item.ElementId); %>
<div id="<%= itemContainerId %>" class="special-product">
    <h3><%=GetMessage("SpecialProduct")%></h3>
    <div class="special-product-title">
        <a href="<%=item.ElementDetailUrl %>"><%= item.Element.Name%></a>
    </div>
    <div class="special-product-image">
        <% if (item.Element.PreviewImage != null)
           { %>
        <a href="<%=item.ElementDetailUrl %>">
            <img border="0" src="<%= item.Element.PreviewImage.FilePath %>" width="<%= Math.Min(item.Element.PreviewImage.Width, 100) %>"  alt="<%= item.Element.PreviewImage.Description %>" enableajax="true" />
        </a>
        <%} %>
    </div>
    <div class="special-product">
    <%=GetMessage("Price")%>: <%= item.Element.CustomPublicValues.Get<int>("PRICE", 0).ToString() + " " + item.Element.CustomPublicValues.GetHtml("PRICECURRENCY")%>
    </div>
</div>
<% } %>
<% else { %>
	<div class="special-product"></div>
<% } %>
