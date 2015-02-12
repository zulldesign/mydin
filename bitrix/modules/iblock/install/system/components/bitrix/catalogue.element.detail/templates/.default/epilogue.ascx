<%@ Reference Control="~/bitrix/components/bitrix/catalogue.element.detail/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.IBlock.Components.CatalogueElementDetailTemplate" %>
<%@ Import Namespace="Bitrix.Services.Js" %>
<%@ Import Namespace="Bitrix.IBlock" %>
<%@ Import Namespace="Bitrix.IBlock.Components" %>

<% if(Component.DisplayStockCatalogData && Component.IsSellingAllowed) {%>
<script type="text/javascript">
<%	if(Component.DisplayAllPriceTiers) {%>
Bitrix.CatalogueElementDetailPriceData.tiers = <%= BXJsonUtility.Serialize<IEnumerable<CatalogueElementDetailComponent.CatalogClientPriceTierInfo>>(Component.ClientPriceInfoSet.GetTiers()) %>;
<%	}%>
<%	else {%>
<%	var prices = new List<CatalogueElementDetailComponent.CatalogClientPriceInfo>();
	for (int i = 0; i < Component.ClientPriceInfoSet.PriceTypes.Length; i++) {
		var priceInfo = Component.ClientPriceInfoSet.GetPriceInfoByPriceTypeId(Component.ClientPriceInfoSet.PriceTypes[i].Id);
		if(priceInfo != null)
			prices.Add(priceInfo);
		}%>
Bitrix.CatalogueElementDetailPriceData.prices = <%= BXJsonUtility.Serialize<IEnumerable<CatalogueElementDetailComponent.CatalogClientPriceInfo>>(prices) %>;
<%	}%>
(function() { 
	Bitrix.CatalogueElementDetail.userCartUrl = "<%= Component.UserCartUrl %>";
	var item = Bitrix.CatalogueElementDetail.getByElementId("<%= Component.ElementId %>");
	var opts = {};
	opts.inStock = <%= Component.IsInStock.ToString().ToLowerInvariant() %>;
	<% if(Component.CurrentSkuId > 0) {%>
	opts.skuId = <%= Component.CurrentSkuId	%>;
	<%} %>
	opts.callBackScript = "<%= Component.GetAddToCartUrl(true) %>";
	<% if(Component.CurrentSellingPrice != null) {%>
	opts.priceHtml = "<%= Component.CurrentSellingPrice.SellingPriceHtml %>";
	<% var discount = Component.GetDiscountByPriceInfo(Component.CurrentSellingPrice); %>
	<% if(discount != null && discount.HasInfo) {%>
	opts.discountHtml = "<%= discount.DisplayHtml %>";
	<% }%>
	<% } %>
	opts.qtyInCart = <%= Component.QuantityInCart %>;
	item.setOptions(opts);
	item.layout();
})();	
</script>
<% }%>
