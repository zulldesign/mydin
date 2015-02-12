<%@ Reference Control="~/bitrix/components/bitrix/catalogue.element.list/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.IBlock.Components.CatalogueElementListTemplate" %>
<%@ Import Namespace="Bitrix.IBlock" %>
<%@ Import Namespace="Bitrix.IBlock.Components" %>
<%@ Import Namespace="Bitrix.Services" %>
<%@ Import Namespace="Bitrix.Services.Js" %>
<script runat="server">
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		
		RequiresEpilogue = true;
	}
	//Template name
	public override string Title
	{
		get { return GetMessageRaw("TemplateTitle"); }
	}

    protected string GetItemControlClientId(int itemId, string id)
    {
        return string.Concat(ClientID, ClientIDSeparator, itemId.ToString(), ClientIDSeparator, id);
    }
</script>

<%
if (Component.isErrorOccured)
{
	%><span class="errortext"><%= Component.errorMessage %></span><%
	return;
}
else if (Component.Items == null)
   return;
%>

<div class="news-list-pager">
<bx:IncludeComponent runat="server" ID="HeaderPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="top" />
</div>


<table cellpadding="0" cellspacing="0" border="0" class="catalog-element-list" width="100%">

<% int itemPerLine = 2, 
       itemNumber = 0;
	bool enebleCatalogSkuSupport = false;
	
foreach (CatalogueElementListComponent.ElementListItem listItem in Component.Items)	
	if(listItem.HasSkuItems)
	{
		enebleCatalogSkuSupport = true;
		break;
	}	
	
foreach (CatalogueElementListComponent.ElementListItem listItem in Component.Items)	
{   
	if(itemNumber % itemPerLine == 0) {%>
	    <tr>
  <%} %>

	<% var itemContainerId = GetItemControlClientId(listItem.ElementId, "ItemContainer"); %>
	
	<td id="<%= itemContainerId %>" valign="top" width="<%= String.Format(System.Globalization.NumberFormatInfo.InvariantInfo, "{0:.##}", 100.0/itemPerLine) %>%">
		<div style="padding:1.5em;">
				<%if (listItem.Element.PreviewImage != null) { %>
                    <a style="float:left; margin:0 1em 1em 0;" href="<%= listItem.ElementDetailUrl %>"><img border="0" src="<%= listItem.Element.PreviewImage.FilePath %>" width="<%= Math.Min(listItem.Element.PreviewImage.Width, 100) %>" alt="<%= listItem.Element.PreviewImage.Description %>" enableajax="true" /></a>
			    <%} %>
				<% else if (listItem.Element.DetailImage != null) { %>
				    <a style="float:left; margin:0 1em 1em 0;" href="<%= listItem.ElementDetailUrl %>"><img border="0" src="<%= listItem.Element.DetailImage.FilePath %>" width="<%= Math.Min(listItem.Element.DetailImage.Width, 100) %>"  alt="<%= listItem.Element.DetailImage.Description %>" enableajax="true" /></a>
				<%} %>
				
				<div style="padding:0 0 1em 0; margin:0;">
				<% if (!string.IsNullOrEmpty(listItem.ElementDetailUrl)) { %>
			        <a style="font-size:1.25em;" href="<%= listItem.ElementDetailUrl %>" enableajax="true"><%=listItem.Element.Name %></a>
				<%} %>
				<% else { %>
                    <%= listItem.Element.Name %>
				<%} %>						    
				</div>
				
				<%if(listItem.Properties.Count > 0) {%>
		        <div style="padding:0 0 1em 0; margin:0;">
					<% foreach (CatalogueElementListComponent.ElementListItemProperty property in listItem.Properties) { %>
						<% if (!String.IsNullOrEmpty(property.DisplayValue) && Component.ShowProperties.Contains(property.Code)) { %>
							<div style="padding:0 0 0.25em 0;"><%= property.Name %>: <%= property.DisplayValue %></div>
						<%} 
					   } %>	
                   	<%if(Component.DisplayStockCatalogData) {%>	        
					<% foreach (CatalogueElementListComponent.ElementListItemProperty property in listItem.CatalogProperties) { %>
						<% if (!String.IsNullOrEmpty(property.DisplayValue) && Component.ShowCatalogItemProperties.Contains(property.Code)) { %>
							<div style="padding:0 0 0.25em 0;"><%= property.Name %>: <%= property.DisplayValue %></div>
						<%} 
					   } 
					}%>			    
		        </div>
			    <%} %>
			    <div style="font-size:1em; text-align:justify;" >
				    <%= listItem.Element.PreviewText %>			    
			    </div>
			    <%if(!IsComponentDesignMode && Component.AllowComparison) {%>
			    <% var add2CompareContainerId = GetItemControlClientId(listItem.ElementId, "add2CompareContainer"); %>
			    <% var add2CompareButtonId = GetItemControlClientId(listItem.ElementId, "add2CompareButton"); %>
			    <div id="<%= add2CompareContainerId %>" class="bx-catalog-element-list-stock-catalog-add2compare-container">
					<%if(listItem.HasSkuItems) {%>
						<a href="#" onclick="Bitrix.CatalogSku.get('<%= listItem.ElementId.ToString() %>').add2SkuComparison(); return false;"><%= GetMessage("Add2ComparisonList") %></a>							
			        <%} else {%>
						<a id="<%= add2CompareButtonId %>" href="#"><%= GetMessage("Add2ComparisonList")%></a>
			        <%} %>
			    </div>
				<script type="text/javascript">
	    			BX.ready(function() { Bitrix.CatalogueElementUtility.adjustComparison(<%= Component.IBlockId %>, <%= listItem.ElementId %>, "<%= add2CompareContainerId %>", "<%= add2CompareButtonId %>", "<%= listItem.ElementAdd2ComparisonListUrlTemplate %>", false); });
				</script>		    
			    <%} %>
                <% if (!IsComponentDesignMode && Component.DisplayStockCatalogData && listItem.IsSellingAllowed) {%>
                <div class="bx-catalog-element-list-stock-catalog-add2cart-container">
                    <table width="100%" cellspacing="0" cellpadding="6" border="0">
                        <tr>
                            <td align="left"><%= GetMessage("YourPrice")%></td>
                            <td align="right">
                              
                                <% if (listItem.DiscountInfo.HasInfo)
								   { %>
								     <span class="bx-catalog-element-list-stock-catalog-your-price"><%= listItem.DiscountInfo.DisplayHtml%></span>
                                <span class="bx-catalog-element-list-stock-catalog-your-price"><s><%=listItem.CurrentSellingPrice.SellingPriceHtml%></s></span>
                                <%}
								   else
								   { %>
								     <span class="bx-catalog-element-list-stock-catalog-your-price"><%= listItem.CurrentSellingPrice.SellingPriceHtml%></span>
                                <%} %>
                            </td>
                        </tr>
                        <% if (Component.AcceptQuantity && listItem.CanPutInCart) {%>
                        <tr id="<%= GetItemControlClientId(listItem.ElementId, "QtyContainer") %>">
                            <td align="left"><%= GetMessage("YourQuantity")%></td>
                            <td align="right">
                                <input type="text" id="<%= GetItemControlClientId(listItem.ElementId, "QtyTbx") %>" class="bx-catalog-element-list-stock-catalog-quantity" value="<%= Component.InitQuantity.ToString() %>" />
                            </td>
                        </tr>
                        <%} %>  
                        <tr>
                            <td colspan="2" align="right" >
                            <% if (listItem.CanPutInCart) { %>
                                <input type="button" id="<%= GetItemControlClientId(listItem.ElementId, "Add2CartBtn") %>"  class="bx-catalog-element-list-stock-catalog-button" value="<%= GetMessage("Add2CartBtnText") %>" />
                             <%} %>   
                                <span id="<%= GetItemControlClientId(listItem.ElementId, "InCartLbl") %>" style="display:none;" class="bx-catalog-element-list-stock-catalog-already-in-cart" ><%= string.Format(GetMessage("AlreadyInCartWithQty"), "#QTY#")%></span>
                             <% if (listItem.CanPutInCart) { %>    
                                <input type="button" id="<%= GetItemControlClientId(listItem.ElementId, "BuyBtn") %>"  class="bx-catalog-element-list-stock-catalog-button" value="<%= GetMessage("BuyBtnText") %>" />                                                          
							<%}
								else {%>
								<span class="bx-catalog-element-list-stock-catalog-unavailable"><%=GetMessage("Product.Unavailable")%></span>
							<%  }%>
                            </td>
                     </tr>
                    </table>
                </div>
                <%} %>	
		<% if (Component.DisplayStockCatalogData && listItem.IsSellingAllowed)
	 { %>
		<script type="text/javascript">
			BX.ready(function() {
				var opts = { 
					'elementId': '<%= listItem.ElementId %>',
					'isInCart': <%= listItem.InCart.ToString().ToLowerInvariant() %>,
					'qtyContainerId': '<%= GetItemControlClientId(listItem.ElementId, "QtyContainer") %>',
					'qtyTbxId': '<%= GetItemControlClientId(listItem.ElementId, "QtyTbx") %>',
					'add2CartBtnId': '<%= GetItemControlClientId(listItem.ElementId, "Add2CartBtn") %>',
					'buyBtnId': '<%= GetItemControlClientId(listItem.ElementId, "BuyBtn") %>',
					'inCartLbl': '<%= GetItemControlClientId(listItem.ElementId, "InCartLbl") %>',
					'idPlaceHolder': '#ElementId#', 'qtyPlaceHolder': '#Quantity#', "csrfTokenPlaceHolder": '#CsrfToken#',
					'add2CartUrl': '<%= Component.GetAddToCartUrlTemplate("#ElementId#", "#Quantity#", "#CsrfToken#", listItem.ElementDetailUrl, true) %>',
					'hasSKUs': <%= listItem.HasSkuItems.ToString().ToLowerInvariant() %>				
				}
				Bitrix.CatalogueElementListItem.create('<%= Component.ClientID +  "_" + listItem.ElementId.ToString() %>', opts);
			});	
		</script>
		<%} %>                                 		                  
		</div>
		
	<% RenderElementToolbar(listItem.Element, itemContainerId); %>	
	
	</td><%
	
																		  itemNumber++;

																		  if (itemNumber % itemPerLine == 0)
																		  {
		%></tr><%
																		  }
}

if (itemNumber % itemPerLine != 0)
{
	while ((itemNumber++) % itemPerLine != 0)
	{
		%><td>&nbsp;</td><%
																		  }
	%></tr><%
																		  }
%>

</table>

<div class="news-list-pager">
<bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager" CurrentPosition="bottom" Template="<%$ Parameters:PagingTemplate %>"/>
</div>

<% if (!IsComponentDesignMode && Component.DisplayStockCatalogData)
   {%>
<script type="text/javascript">
	if (typeof(Bitrix) == "undefined") { var Bitrix = {}; }

	Bitrix.CatalogueElementListItemDefaultHandler = {
		handlePutInCart: function(item, result) {
			if (!("isInCart" in result && result.isInCart)) return;
			if (!("id" in result)) return;
			if (item.getElementId() != result.id.toString()) return;

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
	BX.ready(function() {
		Bitrix.CatalogueElementListItem.addPutInCartListener(Bitrix.TypeUtility.createDelegate(Bitrix.CatalogueElementListItemDefaultHandler, Bitrix.CatalogueElementListItemDefaultHandler.handlePutInCart));
		
		var cartItemAry = Bitrix.CatalogueElementListCartItems;
		if (!cartItemAry) return;
		for (var i = 0; i < cartItemAry.length; i++) {
			var cartItem = cartItemAry[i];
			var item = Bitrix.CatalogueElementListItem.getByElementId(cartItem.id);
			if (item) Bitrix.CatalogueElementListItemDefaultHandler.handlePutInCart(item, { 'isInCart': true, 'id': cartItem.id, 'addedQty': cartItem.qty });
		}
	});
</script>

<script type="text/javascript">
	Bitrix.CatalogSku = function() {
		this._add2SkuComparisonUrlTemplate = "<%= Component.GetAddSku2ComparisonListUrlTemplate("#SkuId#", "#ResponseType#", "#CsrfToken#") %>";
		this._skuItemsUrl = "<%= Component.GetSkuItemsUrlTemplate("#ElementId#", "#CsrfToken#") %>";
		
		this._dlgTitleAdd2ComparisonList = "<%= GetMessage("DlgTitle.Add2ComparisonList") %>";
		this._dlgTitleAdd2Cart = "<%= GetMessage("DlgTitle.Add2Cart") %>";
		this._skuSelectDialogCloseHandler = Bitrix.TypeUtility.createDelegate(this, this._handleSkuSelectDialogClose);
		this._id;
	}
	Bitrix.CatalogSku.prototype = {
		initialize: function(id) {
			this._id = id;
		},
		add2SkuComparison: function() {
			this.openSkuSelectDialog("add2Comparison", null);
		},
		openSkuSelectDialog: function(mode, options) {		
			var title = "[" + mode + "]";
			if(mode == "add2Comparison") title = this._dlgTitleAdd2ComparisonList;
			else if(mode == "add2Cart") title = this._dlgTitleAdd2Cart;
						
			var request = Bitrix.HttpUtility.createXMLHttpRequest();
			request.open("GET", this._skuItemsUrl.replace("#ElementId#", this._id).replace("#CsrfToken#", dotNetVars.securityTokenPair), true);
			
			var self = this;
			request.onreadystatechange = function() {
				if(request.readyState != 4) return;
			
				try {
					var skuItems = eval("(" + request.responseText + ")"); 
				
					
					if(skuItems.length == 0) {
						var callback = options && "add2CartCallback" in options ? options.add2CartCallback : null;
						self._completeSkuRequest(mode, callback, null, false);		
						return;
					}
					
					if(!options) options = {};
										
					options["_elementId"] = this._id;
					options["_mode"] = mode;
					options["_skuItems"] = skuItems;
					options["_acceptQuantity"] = <%= Component.AcceptQuantity.ToString().ToLowerInvariant() %>;
					options["_initQuantity"] = <%= Component.InitQuantity.ToString() %>;
															
					var dlg = Bitrix.Dialog.get("BXCatalogSkuSelectDialog");
					if (dlg == null) {
						dlg = Bitrix.CatalogSkuSelectDialog.create("BXCatalogSkuSelectDialog", "BXCatalogSkuSelectDialog", title, options);
						dlg.addCloseListener(self._skuSelectDialogCloseHandler);
					}
					else 
					{
						dlg.setTitle(title);
						dlg.setOptions(options)
					}
					dlg.open();					
				}
				catch(e) {}
			};
			request.send(); 		
		},
		_completeSkuRequest: function(mode, callback, skuId, canceled) {
			if(canceled) return;
			if(skuId == null) skuId = this._id;
			if(callback)
				try {
					callback(skuId);
				}
				catch(e){}
			
			if(mode == "add2Comparison")
				window.location.href = this._add2SkuComparisonUrlTemplate.replace("#SkuId#", Bitrix.TypeUtility.isNotEmptyString(skuId) ? skuId : this._id).replace("#ResponseType#", "HTLM").replace("#CsrfToken#", dotNetVars.securityTokenPair);
		},
		_handleSkuSelectDialogClose: function(sender, args) {			
			this._completeSkuRequest(sender.getOption("_mode"), sender.getOption("completionCallback"), sender.getSkuId(), !("buttonId" in args && args["buttonId"] == Bitrix.Dialog.button.bContinue));
		}		
	}
	
	Bitrix.CatalogSku._items = {}
	Bitrix.CatalogSku.get = function(id) {
		return id in this._items ? this._items[id] : null;
	}
	
	Bitrix.CatalogSku.create = function(id) {
		var self = new Bitrix.CatalogSku();
		self.initialize(id);
		return (this._items[id] = self);
	}
	
 <% foreach (CatalogueElementListComponent.ElementListItem listItem in Component.Items)	{%>
	Bitrix.CatalogSku.create('<%= listItem.ElementId.ToString() %>');
<%} %>

	Bitrix.CatalogueElementListItemDefaultSkuSelector = {
		process: function(itemId, mode, options) {
			var sku = Bitrix.CatalogSku.get(itemId);
			if(!sku) {
				var callback = options && "add2CartCallback" in options ? options["add2CartCallback"] : null;
				if(callback) callback(itemId, itemId);
				return;
			}
			sku.openSkuSelectDialog(mode, options);
		}
	}
	
	Bitrix.CatalogueElementUtility = {
		adjustComparison: function(iblockId, elementId, containerId, buttonId, url, jsonResponse) {
			if (typeof (Bitrix.CatalogueComparer) == "undefined") return;
			
			var container = containerId ? BX(containerId) : null;
			if(container && Bitrix.CatalogueComparer.containsElementId(iblockId, elementId))
				container.style.display = "none";
				
			var button = buttonId ? BX(buttonId) : null;
			if(button)
				button.href = url.replace("#ResponseType#", jsonResponse ? "JSON" : "HTML").replace("#CsrfTokenPair#", dotNetVars.securityTokenPair);				
		}
	}
				
	window.setTimeout(function() { Bitrix.CatalogueElementListItem.setSkuSelector(Bitrix.TypeUtility.createDelegate(Bitrix.CatalogueElementListItemDefaultSkuSelector, Bitrix.CatalogueElementListItemDefaultSkuSelector.process)); }, 300);			
</script>
<%} %>
