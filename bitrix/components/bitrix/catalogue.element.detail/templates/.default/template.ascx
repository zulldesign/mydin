<%@ Reference Control="~/bitrix/components/bitrix/catalogue.element.detail/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.IBlock.Components.CatalogueElementDetailTemplate" %>
<%@ Import Namespace="Bitrix.IBlock.Components" %>
<%@ Import Namespace="Bitrix.Services.Js" %>

<script runat="server">
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		
		RequiresEpilogue = true;
		Voting.Visible = Component.EnableVotingForElement;
	}

    protected string GetItemControlClientId(string id)
    {
        return string.Concat(ClientID, ClientIDSeparator, Component.ElementId.ToString(), ClientIDSeparator, id);
    }    
    
	protected override void PreLoadTemplateDefinition(Bitrix.Components.BXParametersDefinition def)
	{
		def["Template_BackTitle"] = new Bitrix.Components.BXParamText(GetMessageRaw("Param.BackTitle"), GetMessageRaw("DefaultBackTitle"), Bitrix.Components.BXCategory.UrlSettings);
		def["Template_BackUrlTemplate"] = new Bitrix.Components.BXParamText(GetMessageRaw("Param.BackUrlTemplate"), "", Bitrix.Components.BXCategory.UrlSettings);
	}           
</script>

<%
if (!String.IsNullOrEmpty(Component.errorMessage))
{ 
	%><span class="errortext"><%=Component.errorMessage%></span><%
}
else if (Component.Element != null)
{
	%><div class="catalog-element">
	
	<table width="100%" cellspacing="0" cellpadding="2" border="0">
	<tr>
		<% 
		if (Component.DetailImage != null && Component.PreviewImage != null) 
		{ 
			%><td width="0%" valign="top">
				<img border="0" src="<%= Component.PreviewImage.FilePath %>" width="<%= Component.PreviewImage.Width %>" title="<%= Component.ElementName %>" height="<%= Component.PreviewImage.Height %>" alt="<%= Component.PreviewImage.Description %>" id="image_<%= Component.PreviewImage.Id %>" style="display:block;cursor:pointer;cursor: hand;" onclick="document.getElementById('image_<%= Component.PreviewImage.Id %>').style.display='none';document.getElementById('image_<%= Component.DetailImage.Id %>').style.display='block'" />
				<img border="0" src="<%= Component.DetailImage.FilePath %>" width="<%= Component.DetailImage.Width %>" title="<%= Component.ElementName %>" height="<%= Component.DetailImage.Height %>" alt="<%= Component.DetailImage.Description %>" id="image_<%= Component.DetailImage.Id %>"  style="display:none;cursor:pointer; cursor: hand;" onclick="document.getElementById('image_<%= Component.DetailImage.Id %>').style.display='none';document.getElementById('image_<%= Component.PreviewImage.Id %>').style.display='block'" />
			</td><%
		}
		else if (Component.DetailImage != null)
		{
			%><td width="0%" valign="top"><img class="detail-picture" border="0" title="<%= Component.ElementName %>" src="<%= Component.DetailImage.FilePath %>" width="<%= Component.DetailImage.Width %>"
				height="<%= Component.DetailImage.Height %>" alt="<%= Component.DetailImage.Description %>" /></td><%
		}
		else if (Component.PreviewImage != null)
		{
			%><td width="0%" valign="top"><img class="preview-picture" border="0" title="<%= Component.ElementName %>" src="<%= Component.PreviewImage.FilePath %>" width="<%= Component.PreviewImage.Width %>"
				height="<%= Component.PreviewImage.Height %>" alt="<%= Component.PreviewImage.Description %>" /></td><%
		}
		%>
		<td width="100%" valign="top">
		<div class="bx-catalog-element-properties-conatiner">
	        <% if (Component.Properties.Count > 0) {%>
	        <table width="100%" cellspacing="0" cellpadding="6" border="0">
		        <% foreach (CatalogueElementDetailComponent.ElementDetailProperty property in Component.Properties) { %>
			        <% if (!String.IsNullOrEmpty(property.DisplayValue) && Component.ShowProperties.Contains(property.Code)) {%>
			        <tr>
			            <td width="40%"><span class="catalog-element-property"><%=property.Name%></span></td>
			            <td><span class="catalog-element-value"><%=property.DisplayValue%></span></td>
			        </tr>
			        <%} %>
		        <%} %>
		        <%if(Component.DisplayStockCatalogData) {%> 
					<% foreach (CatalogueElementDetailComponent.ElementDetailProperty property in Component.CatalogItemProperties) {%>
						<% if (!String.IsNullOrEmpty(property.DisplayValue) && Component.ShowCatalogItemProperties.Contains(property.Code)) {%>					
	            <tr>
	                <td width="40%"><span class="catalog-element-property"><%= property.Name%></span></td>
	                <td><span class="catalog-element-value"><%= property.DisplayValue %></span></td>
	            </tr>
						<%} %>
					<%} %>
	            <%} %>	    
	        </table>
	        <%} %>	
	        <% CatalogDiscountInfo discount; %>	        
	        <%if (Component.DisplayStockCatalogData && Component.ClientPriceInfoSet != null && Component.ClientPriceInfoSet.Items != null) {%>
	        <div class="bx-catalog-element-stock-catalog-price-container">
	        <table class="bx-catalog-element-stock-catalog-price-table" id="<%= GetItemControlClientId("PriceTable") %>" width="100%" cellspacing="0" cellpadding="6" border="0">
	        </table> 
	        </div>   
	        <%} %>

			<% if(Component.DisplayStockCatalogData && Component.SkuItems.Length > 0) {%>
			<div class="bx-catalog-element-stock-catalog-sku-container">
				<div><span class="catalog-sku-legend"><%= GetMessage("SkuLegend") %></span></div>
				<div>
					<select id="<%= GetItemControlClientId("SelectSku") %>" style="width:100%;" >
					<% foreach(CatalogueElementDetailComponent.CatalogSKUItem skuItem in Component.SkuItems) {%>
						<option value="<%= skuItem.Id.ToString() %>">
							<%= skuItem.GetName(true) %>
						</option>
					<%} %>
					</select>
				</div>
			</div>
			<script type="text/javascript">
				Bitrix.EventUtility.addEventListener(document.getElementById("<%= GetItemControlClientId("SelectSku") %>"), "change", function() { window.location.href = "<%= Component.GetSelectSkuUrl("_SKU_ID_") %>".replace(/_SKU_ID_/, this.value.toString()); });
			</script>
			<%} %> 
	          
            <% if (Component.DisplayStockCatalogData && Component.IsSellingAllowed) { %>
			<% discount = Component.GetDiscountByPriceInfo(Component.CurrentSellingPrice); %>
			<div class="bx-catalog-element-stock-catalog-add2cart-container">
				<table id="<%= GetItemControlClientId("Add2CartPriceContainerId") %>" style="display:none;" width="100%" cellspacing="0" cellpadding="6" border="0">
					<tr>
						<td align="left"><%= GetMessage("YourPrice")%></td>
						<td align="right">
						<span id="<%= GetItemControlClientId("Add2CartPriceId") %>" class="bx-catalog-element-stock-catalog-your-price"></span>
						<span id="<%= GetItemControlClientId("Add2CartDiscountId") %>" style="display:none;" class="bx-catalog-element-stock-catalog-your-price"></span>
						</td>
					</tr>
					<% if (Component.AcceptQuantity){%>
					<tr id="<%= GetItemControlClientId("QtyContainer") %>">
						<td align="left"><%= GetMessage("YourQuantity") %></td>
						<td align="right">
							<input type="text" id="<%= GetItemControlClientId("QtyTbx") %>" class="bx-catalog-element-stock-catalog-quantity" value="<%= Component.InitQuantity.ToString() %>" />   
						</td>
					</tr>
					<%} %>  
					<tr>
						<td align="right" colspan="2" >
							 <input type="button" id="<%= GetItemControlClientId("Add2CartBtn") %>" class="bx-catalog-element-stock-catalog-button"  value="<%= GetMessage("Add2CartBtnText") %>" />
							 <span id="<%= GetItemControlClientId("InCartLbl") %>" style="display:none;" class="bx-catalog-element-stock-catalog-already-in-cart" ><%=  string.Format(GetMessage("AlreadyInCartWithQty"), "#QTY#") %></span>
							 <input type="button" id="<%= GetItemControlClientId("BuyBtn") %>"  class="bx-catalog-element-stock-catalog-button" value="<%= GetMessage("BuyBtnText") %>" />                                                                                                                  
						</td>
				 </tr>
				</table>
				<span id="<%= GetItemControlClientId("Add2CartPriceStubId") %>" style="display:none;" class="bx-catalog-element-stock-catalog-unavailable"><%=GetMessage("Product.Unavailable") %></span>
            </div>
            <%} %>	
         </div>		        	
		</td>
	</tr>
	</table>
	<%
	if (Component.Element.DetailText.Length > 0)
	{
		%><p><%= Component.Element.DetailText%></p><%
	}
	else if (Component.Element.PreviewText.Length > 0)
	{
		%><p><%= Component.Element.PreviewText%></p>
	<%}%>
	<% if (Component.EnableVotingForElement) {%>
	<div class="catalog-element-voting">
        <bx:IncludeComponent 
            id="Voting" 
            runat="server" 
            componentname="bitrix:rating.vote" 
            Template=".default" 
            BoundEntityTypeId="IBlockElement" 
            BoundEntityId="<%$ Results:ElementId %>" 
            CustomPropertyEntityId = "<%$ Results:CustomPropertyEntityId %>"
            RolesAuthorizedToVote="<%$ Parameters:RolesAuthorizedToVote %>" 
            BannedUsers = "<%$ Results:UsersBannedToVote %>" />		    
	</div>
	<%} %>
	<% if (Component.DisplayStockCatalogData && Component.IsSellingAllowed) { %>
	<script type="text/javascript">
		(function() {
			var opts = { 
				'elementId': '<%= Component.ElementId.ToString() %>',
				'isInCart': <%= Component.IsInCart.ToString().ToLowerInvariant() %>,
				'qtyContainerId': '<%= GetItemControlClientId("QtyContainer") %>',
				'qtyTbxId': '<%= GetItemControlClientId("QtyTbx") %>',
				'add2CartBtnId': '<%= GetItemControlClientId("Add2CartBtn") %>',
				'buyBtnId': '<%= GetItemControlClientId("BuyBtn") %>',
				'inCartLbl': '<%= GetItemControlClientId("InCartLbl") %>',
				'qtyParamName': '<%= Component.CatalogItemQuantityParamName %>',
				'skuSelectorId': '<%= GetItemControlClientId("SelectSku") %>',
				'add2CartPriceContainerId': '<%= GetItemControlClientId("Add2CartPriceContainerId") %>',				
				'add2CartPriceStubId': '<%= GetItemControlClientId("Add2CartPriceStubId") %>',			
				'add2CartPriceId': '<%= GetItemControlClientId("Add2CartPriceId") %>',			
				'add2CartDiscountId': '<%= GetItemControlClientId("Add2CartDiscountId") %>',
				'priceTable': '<%= GetItemControlClientId("PriceTable") %>'	
			}
			Bitrix.CatalogueElementDetail.create('<%= Component.ClientID +  "_" +  Component.ElementId.ToString() %>', opts);
		})();	
	</script>
	<%} %>	
	</div>
	<%} %>

<% 
	string title = Parameters.Get("Template_BackTitle");
	string url = Parameters.Get("Template_BackUrlTemplate");
	bool link = false;
	if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(url))
	{
		Bitrix.DataTypes.BXParamsBag<object> replace = new Bitrix.DataTypes.BXParamsBag<object>();
		
		replace["ElementId"] = replace["ELEMENT_ID"] = Component.ElementId;
		replace["ElementCode"] = replace["ELEMENT_CODE"] = Component.ElementCode;
		
		int sectionId = Parameters.GetInt("SectionId");
		if (sectionId == 0 && Component.Element != null &&  Component.Element.Sections.Count != 0)
			sectionId = Component.Element.Sections[0].SectionId;
		replace["SectionId"] = replace["SECTION_ID"] = sectionId;
		
		replace["SectionCode"] = replace["SECTION_CODE"] = Parameters.GetString("SectionCode", "");
		url = Component.ResolveTemplateUrl(url, replace);
		link = true;
	}
%>
<% if (link) { %>
	<p><a enableajax="true" href="<%= Encode(url) %>"><%= Encode(title) %></a></p>
<% } %>

<script type="text/javascript">
	if (typeof (Bitrix) == "undefined") {
		var Bitrix = new Object();
	}
	
	Bitrix.CatalogueElementDetailDefaultHandler = {
		handlePutInCart: function(item, result) {
			if (!("isInCart" in result && result.isInCart)) return;

			var btn = item.getElement("add2CartBtnId");
			if (btn) btn.style.display = "none";

			var lbl = item.getElement("inCartLbl");
			if (lbl && "addedQty" in result && result.addedQty > 0) {
				lbl.innerHTML = lbl.innerHTML.replace(/\#QTY\#/i, result.addedQty.toString());
				lbl.style.display = "";
			}

			var qty = item.getElement("qtyContainerId");
			if (qty) qty.style.display = "none";
		}
	}
	
	Bitrix.CatalogueElementDetailStyles = { 
		"priceTableHead": "bx-catalog-element-stock-catalog-price-table-head",
		"priceTableData": "bx-catalog-element-stock-catalog-price-table-data",
		"priceTableQauntity": "bx-catalog-element-stock-catalog-price-table-quantity",
		"priceName": "bx-catalog-element-stock-catalog-price-name",
		"price": "bx-catalog-element-stock-catalog-price"
	}
	
	Bitrix.CatalogueElementDetailMessages = { 
		"quantity": "<%= GetMessage("Quantity") %>",
		"fromQuantity": "<%= string.Format(GetMessage("FromQuantity"), "#QTY#") %>",
		"priceVATIncl": "<%= GetMessage("PriceVATIncl") %>",
		"priceVATExcl": "<%= GetMessage("PriceVATExcl") %>"
	}

	Bitrix.CatalogueElementDetailPriceData = {};
	<% if(Component.DisplayStockCatalogData && Component.IsSellingAllowed) {%>
	Bitrix.CatalogueElementDetailPriceData.includeVATInPrice = <%= Component.IncludeVATInPrice.ToString().ToLowerInvariant() %>;
	Bitrix.CatalogueElementDetailPriceData.displayVAT = <%= Component.DisplayVAT.ToString().ToLowerInvariant() %>;
	Bitrix.CatalogueElementDetailPriceData.types = <%= BXJsonUtility.Serialize<IEnumerable<CatalogueElementDetailComponent.CatalogPriceTypeInfo>>(Component.ClientPriceInfoSet.PriceTypes) %>;	
	
	(function(){ Bitrix.CatalogueElementDetail.addPutInCartListener(Bitrix.TypeUtility.createDelegate(Bitrix.CatalogueElementDetailDefaultHandler, Bitrix.CatalogueElementDetailDefaultHandler.handlePutInCart)); })();	
	<% }%>
</script>
