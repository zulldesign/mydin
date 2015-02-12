if(typeof(Bitrix) == "undefined") {
	var Bitrix = new Object();
}
Bitrix.CatalogueElementDetail = function Bitrix$CatalogueElementDetail() {
	this._id = "";
	this._redirectToCart = false;
	this._options = {};
}
Bitrix.CatalogueElementDetail.prototype = {
	initialize: function(id, options) {
		this._id = id;
		this._options = options;

		var add2cartBtn = this.getElement("add2CartBtnId");
		if (add2cartBtn)
			Bitrix.EventUtility.addEventListener(add2cartBtn, "click", Bitrix.TypeUtility.createDelegate(this, this._handleAdd2CartBtnClick));

		var buyBtn = this.getElement("buyBtnId");
		if (buyBtn)
			Bitrix.EventUtility.addEventListener(buyBtn, "click", Bitrix.TypeUtility.createDelegate(this, this._handleBuyBtnClick));
	},
	layout: function() {
		var skuId = this.getOption("skuId", 0);
		if(skuId > 0) Bitrix.DomHelper.selectOption(this.getElement("skuSelectorId"), skuId.toString());
		
		var inStock = this.getOption("inStock", false);
		
		Bitrix.DomHelper.display(this.getElement("add2CartPriceContainerId"), inStock);
		Bitrix.DomHelper.display(this.getElement("add2CartPriceStubId"), !inStock);	
		
		if(inStock) {
			var disc = this.getOption("discountHtml", null);
			if(Bitrix.TypeUtility.isNotEmptyString(disc)) {
				Bitrix.DomHelper.innerHTML(this.getElement("add2CartPriceId"), "<s>" + this.getOption("priceHtml", "-") + "</s>");
				Bitrix.DomHelper.display(this.getElement("add2CartDiscountId"), true);
				Bitrix.DomHelper.innerHTML(this.getElement("add2CartDiscountId"), disc);
			}
			else {
				Bitrix.DomHelper.innerHTML(this.getElement("add2CartPriceId"), this.getOption("priceHtml", "-"));
				Bitrix.DomHelper.display(this.getElement("add2CartDiscountId"), false);
			}
			
			var qty = this.getOption("qtyInCart", 0);
			if(qty > 0 && Bitrix.CatalogueElementDetail._putInCartListener)
				Bitrix.CatalogueElementDetail._putInCartListener.fire(this, { 'isInCart': true, 'id': this.id, 'addedQty': qty });
		}
		
		var priceLayout = this.getOption("priceLayout", null);
		if(typeof(priceLayout) == "function"){
			try {
				priceLayout(this.getOption("priceContainer", null));
			}
			catch(e) {
			}
		}		
		else {
			var priceTypes;
			if((priceTypes = Bitrix.CatalogueElementDetailPriceData.types)) {
				var tab = this.getElement("priceTable");
				var head = tab.insertRow(-1);
				
				var priceTiers, prices;
				if((priceTiers = Bitrix.CatalogueElementDetailPriceData.tiers)) {
					var c = head.insertCell(-1);
					c.align = "left";
					c.className = Bitrix.CatalogueElementDetailStyles.priceTableHead;
					c.innerHTML = Bitrix.CatalogueElementDetailMessages.quantity;
					
					for(var k in priceTypes) {
						c = head.insertCell(-1);
						c.align = "right";
						c.className = Bitrix.CatalogueElementDetailStyles.priceTableHead;
						c.innerHTML = priceTypes[k].name;					
					}
					
					for(var n in priceTiers) {
						var tier = priceTiers[n];
						var content = tab.insertRow(-1);
						c = content.insertCell(-1);
						c.align = "left";
						c.className = Bitrix.CatalogueElementDetailStyles.priceTableData;
						
						var span = document.createElement("SPAN");
						c.appendChild(span);
						span.className = Bitrix.CatalogueElementDetailStyles.priceTableQauntity;
						span.innerHTML = Bitrix.CatalogueElementDetailMessages.fromQuantity.replace(/\#QTY\#/i, tier.qtyFrom.toString());
						
						for(var p in priceTypes) {
							c = content.insertCell(-1);
							c.align = "right";
							c.className = Bitrix.CatalogueElementDetailStyles.priceTableData;
							
							if(!tier.prices) continue;
							
							var price = null;
							for(var i = 0; i < tier.prices.length; i++) {
								if(tier.prices[i].typeId != priceTypes[p].id) continue;
								price = tier.prices[i];
							}
							
							if(price) this.createPriceMarkup(price, c);
						}					
					}
				}
				else if((prices = Bitrix.CatalogueElementDetailPriceData.prices)) {
					for(var k in priceTypes) {
						var price = null;
						for(var i = 0; i < prices.length; i++) {
							if(prices[i].typeId != priceTypes[k].id) continue;
							price = prices[i];
						}				
						
						if(!price) continue;
						
						var content = tab.insertRow(-1);
						var c = content.insertCell(-1);
						c.align = "left";
						c.width = "40%";
						c.className = Bitrix.CatalogueElementDetailStyles.priceTableData;
						
						var span = document.createElement("SPAN");
						c.appendChild(span);
						span.className = Bitrix.CatalogueElementDetailStyles.priceName;
						span.innerHTML = priceTypes[k].name;						
																										
						c = content.insertCell(-1);
						c.align = "left";
						c.className = Bitrix.CatalogueElementDetailStyles.priceTableData;
						
						this.createPriceMarkup(price, c);
					}				
				}
			}
		}
	},
	createPriceMarkup: function(price, container, options) {
		if(!price || !container) return;
		
		var className = options && typeof(options) == "object" && typeof(options.className) != "undefined" ? options.className : Bitrix.CatalogueElementDetailStyles.price;
		
		if(Bitrix.TypeUtility.isNotEmptyString(price.discountHtml)) {
			var spanPrice = document.createElement("SPAN");
			container.appendChild(spanPrice);
			spanPrice.className = className;
			spanPrice.innerHTML = "<s>" + price.priceHtml; + "</s>";
			
			var spanDisc = document.createElement("SPAN");
			container.appendChild(spanDisc);
			spanDisc.className = className;
			spanDisc.innerHTML = price.discountHtml;														
		}
		else {
			var spanPrice = document.createElement("SPAN");
			container.appendChild(spanPrice);
			spanPrice.className = className;
			spanPrice.innerHTML = price.priceHtml;						
		}
		
		if(Bitrix.CatalogueElementDetailPriceData.displayVAT) {
			var span = document.createElement("SPAN");
			container.appendChild(span);						
			span.innerHTML += "(" + Bitrix.CatalogueElementDetailMessages[Bitrix.CatalogueElementDetailPriceData.includeVATInPrice ? "priceVATIncl" : "priceVATExcl"] + ": "  + price.vatHtml + ")";
		}
	},
	getOption: function(name, defVal) { return this._options && name in this._options ? this._options[name] : defVal; },
	setOption: function(name, val) {
		if (!this._options) this._options = {};
		this._options[name] = val;
	},
	setOptions: function(options) {
		if(!(options instanceof Object)) return;
		if (!this._options) this._options = {};
		for(var k in options)
			this._options[k] = options[k]; 
	},
	getElement: function(opt) {
		var id = this.getOption(opt);
		return id ? document.getElementById(id) : null;
	},
	getElementId: function() {
		return this.getOption("elementId", 0);
	},
	add2Cart: function() {
		var elementId = this.getOption("elementId");
		if (!elementId) return;

		var qty = 0;
		var qtyTbx = this.getElement("qtyTbxId");
		if (qtyTbx) qty = parseInt(qtyTbx.value);
		if (isNaN(qty) || qty <= 0) qty = 1;

		var callBack = this.getOption("callBackScript");
		if (!callBack) return;
		callBack += "&" + this.getOption("qtyParamName", "qty") + "=" + qty.toString();
		var request = Bitrix.HttpUtility.createXMLHttpRequest();
		request.open("GET", callBack, true);

		request.onreadystatechange = function() {
			if (request.readyState != 4) return;

			try {
				var r = eval("(" + request.responseText + ")");
				if ("error" in r) return;

				var item = "id" in r ? Bitrix.CatalogueElementDetail.get(r.id) : null;
				if (!item) return;

				if (!("action" in r)) return;
				var res = r.result ? r.result : {};
				if (r.action == "ADD2CART") {
					if (Bitrix.CatalogueElementDetail._putInCartListener)
						Bitrix.CatalogueElementDetail._putInCartListener.fire(item, res);
					item._handleAddActionCompletion("ADD2CART", "isInCart" in res ? res.isInCart : false, "addedQty" in res ? res.addedQty : 0);
				}
			}
			catch (e) { }
		};
		request.send();
	},
	_handleAdd2CartBtnClick: function() { this.add2Cart(); },
	_handleBuyBtnClick: function() {
		if (this.getOption("isInCart", false)) {
			window.location.href = Bitrix.CatalogueElementDetail.userCartUrl;
			return;
		}
		this._redirectToCart = true;
		this.add2Cart();
	},
	_handleAddActionCompletion: function(name, isInCart, addedQty) {
		if (this._redirectToCart) window.location.href = Bitrix.CatalogueElementDetail.userCartUrl;
	}
}
Bitrix.CatalogueElementDetail._items = null;
Bitrix.CatalogueElementDetail.create = function(id, options) {
	if(this._items != null && (id in this._items))
		return this._items[id];
	
	var self = (this._items ? this._items : (this._items = new Object()))[id] = new Bitrix.CatalogueElementDetail();
	self.initialize(id, options);
	return self;
}
Bitrix.CatalogueElementDetail.get = function(id) {
	return this._items != null && (id in this._items) ? this._items[id] : null;
}

Bitrix.CatalogueElementDetail.getByElementId = function(elId) {
	if (!this._items) return null;
	for (var k in this._items)
		if (this._items[k].getElementId() == elId)
		return this._items[k];

	return null;
}

Bitrix.CatalogueElementDetail.userCartUrl = "";

Bitrix.CatalogueElementDetail._putInCartListener = null;
Bitrix.CatalogueElementDetail.addPutInCartListener = function(listener) {
	(this._putInCartListener ? this._putInCartListener : (this._putInCartListener = new Bitrix.EventPublisher())).addListener(listener);
}
Bitrix.CatalogueElementDetail.removePutInCartListener = function(listener) {
	if(this._putInCartListener) this._putInCartListener.removeListener(listener);
}