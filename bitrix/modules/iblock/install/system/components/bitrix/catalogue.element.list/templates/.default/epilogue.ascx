<%@ Reference Control="~/bitrix/components/bitrix/catalogue.element.list/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.IBlock.Components.CatalogueElementListTemplate" %>
<%@ Import Namespace="Bitrix.IBlock" %>
<%@ Import Namespace="Bitrix.IBlock.Components" %>

<% if(Component.DisplayStockCatalogData) {%>
<script type="text/javascript">
	BX.ready(function() { Bitrix.CatalogueElementListItem.userCartUrl = "<%= Component.UserCartUrl %>"; });
	Bitrix.CatalogueElementListCartItems = [];
	<% var cartItems = Component.GetSaleCartItems(); %>
	<% if(cartItems != null) for(int i = 0; i < cartItems.Length; i++) {%>
	Bitrix.CatalogueElementListCartItems.push({ 'id':<%= cartItems[i].ElementId %>, 'qty':<%= cartItems[i].Quantity %> });
	<%	} %>	
	<% var compare = Component.GetComparisonSettings(); %>
	<% if(compare != null) {%>
	var opts = {};
	opts.iblockId = <%= Component.IBlockId %>;
	opts.elementIdAry = [];
	<% for(int i = 0; i < compare.IdList.Count; i++) {%>
	opts.elementIdAry.push(<%= compare.IdList[i] %>);
	<% } %>
	Bitrix.CatalogueComparer.create(opts);
	<% } %>		
</script>
<% }%>
