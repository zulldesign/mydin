<%@ Reference Control="~/bitrix/components/bitrix/iblock.element.random/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.IBlock.Components.IBlockElementRandomTemplate" %>

<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errortext" ValidationGroup="" Visible="false" HeaderText="<%$ Loc:Kernel.Error %>" />

<script runat="server">
	
	protected void Page_Load(object sender, EventArgs e)
	{
		if (Component.Error != Bitrix.IBlock.Components.IBlockElementRandomComponentError.None)
			errorMessage.Visible = true;
		errorMessage.ValidationGroup = ClientID;
	}

protected void Page_PreRender(object sender, EventArgs e)
{
	if (Component.Error != Bitrix.IBlock.Components.IBlockElementRandomComponentError.None)
	{
		errorMessage.AddErrorMessage(Component.GetErrorMessage());
		return;
	}
}

</script>

<% if (Component.Error != Bitrix.IBlock.Components.IBlockElementRandomComponentError.None) return; %>

<%
	var element = Component.Elements.Count > 0 ? Component.Elements[0] : null;

 %>
<% if (element != null)
   {
	   var image = element.Image != null ? Bitrix.Services.Image.BXImageUtility.GetResizedImage(element.Image, 75, 75) : null;
	   var imagePath = image != null ? image.GetUri() : String.Empty;
	   
	   %>
<div class="content-block content-block-special">
	<h3><%= GetMessage("Label.SpecialOffer") %></h3>
	
	<% 
	
	if (image != null)
	{ %>
	<div class="item-image"><a href="<%= element.DetailUrl %>"><img src="<%= imagePath %>" /></a></div>
	<%} %>
	<div class="item-name"><a href="<%= element.DetailUrl %>"><%= element.Element.Name %></a></div>
	<% if (!String.IsNullOrEmpty(element.Element.PreviewText))
	{ %>
	<div class="item-desc"><%= element.Element.PreviewText%></div>
	<%} %>
	
	<% if (!String.IsNullOrEmpty(element.PriceHtml))
	{ %>
	<div class="item-price"><span><%= element.PriceHtml%></span></div>
	
	<%} %>
</div>

<%} %>