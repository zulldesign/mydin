if(typeof(Bitrix) == "undefined") {
	var Bitrix = {};
}

Bitrix.CatalogueElementListItem = function Bitrix$CatalogueElementListItem() {
	this._id = "";
	this._redirectToCart = false;
	this._options = {};
}
Bitrix.CatalogueElementListItem.prototype = {
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
	getId: function() { return this._id; },
	getOption: function(name, defVal) { return this._options && name in this._options ? this._options[name] : defVal; },
	setOption: function(name, val) {
		if (!this._options) this._options = {};
		this._options[name] = val;
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
		if (this.getOption("hasSKUs", false) && Bitrix.CatalogueElementListItem.skuSelector) {
			var qtyTbx = this.getElement("qtyTbxId");
			var qty = qtyTbx ? parseInt(qtyTbx.value) : 1;
			if (isNaN(qty) || qty <= 0) qty = 1;
			Bitrix.CatalogueElementListItem.skuSelector(elementId, "add2Cart", { "quantity": qty, "add2CartCallback": Bitrix.TypeUtility.createDelegate(this, this._processAdd2Cart) });
		}
		else
			this._processAdd2Cart(elementId);
	},
	_handleAdd2CartBtnClick: function() { this.add2Cart(); },
	_handleBuyBtnClick: function() {
		if (this.getOption("isInCart", false)) {
			window.location.href = Bitrix.CatalogueElementListItem.userCartUrl;
			return;
		}
		this._redirectToCart = true;
		this.add2Cart();
	},
	_handleAddActionCompletion: function(name, isInCart, addedQty) {
		if (this._redirectToCart) window.location.href = Bitrix.CatalogueElementListItem.userCartUrl;
	},
	_processAdd2Cart: function(elementId, options) {
		if (!options) options = {};
		var qty = 0;
		if ("quantity" in options) qty = options.quantity;
		else {
			var qtyTbx = this.getElement("qtyTbxId");
			if (qtyTbx) qty = parseInt(qtyTbx.value);
		}
		if (isNaN(qty) || qty <= 0) qty = 1;

		var url = this.getOption("add2CartUrl");
		if (!url) return;
		url = url.replace(this.getOption("idPlaceHolder"), elementId).replace(this.getOption("qtyPlaceHolder"), qty.toString()).replace(this.getOption("csrfTokenPlaceHolder"), dotNetVars.securityTokenPair);
		var request = Bitrix.HttpUtility.createXMLHttpRequest();
		request.open("GET", url, true);

		var self = this;
		request.onreadystatechange = function() {
			if (request.readyState != 4) return;

			try {
				var r = eval("(" + request.responseText + ")");
				if ("error" in r) return;

				if (!("action" in r)) return;
				var res = r.result ? r.result : {};

				if (r.action == "ADD2CART") {
					if ("callback" in options && typeof (options.callback) == "function")
						options.callback(res);

					if (Bitrix.CatalogueElementListItem._putInCartListener)
						Bitrix.CatalogueElementListItem._putInCartListener.fire(self, res);
					self._handleAddActionCompletion("ADD2CART", "isInCart" in res ? res.isInCart : false, "addedQty" in res ? res.addedQty : 0);
				}
			}
			catch (e) { }
		};
		request.send();
	}
}
Bitrix.CatalogueElementListItem._items = null;
Bitrix.CatalogueElementListItem.create = function(id, options) {
	if(this._items != null && (id in this._items))
		return this._items[id];
	
	var self = (this._items ? this._items : (this._items = new Object()))[id] = new Bitrix.CatalogueElementListItem();
	self.initialize(id, options);
	return self;
}
Bitrix.CatalogueElementListItem.get = function(id) {
	return this._items != null && (id in this._items) ? this._items[id] : null;
}

Bitrix.CatalogueElementListItem.getByElementId = function(elId) {
	if (!this._items) return null;
	for (var k in this._items)
		if (this._items[k].getElementId() == elId)
			return this._items[k];
			
	return null;
}

Bitrix.CatalogueElementListItem.userCartUrl = "";

Bitrix.CatalogueElementListItem._putInCartListener = null;
Bitrix.CatalogueElementListItem.addPutInCartListener = function(listener) {
	(this._putInCartListener ? this._putInCartListener : (this._putInCartListener = new Bitrix.EventPublisher())).addListener(listener);
}
Bitrix.CatalogueElementListItem.removePutInCartListener = function(listener) {
	if(this._putInCartListener) this._putInCartListener.removeListener(listener);
}

Bitrix.CatalogueElementListItem.skuSelector = null;
Bitrix.CatalogueElementListItem.setSkuSelector = function(skuSelector) { this.skuSelector = skuSelector; }

if (typeof (Bitrix.CatalogueComparer) == "undefined") {
	Bitrix.CatalogueComparer = function() {
		this.iblockId = 0;
		this.elementIdAry = [];
	},
		Bitrix.CatalogueComparer.prototype = {
			initialize: function(options) {
				if (!(options instanceof Object)) return;
				if ("iblockId" in options) this.iblockId = options.iblockId;
				if ("elementIdAry" in options) this.elementIdAry = options.elementIdAry;
			},
			containsElementId: function(id) {
				ary = this.elementIdAry;
				if (!ary) return false;
				for (var i = 0; i < ary.length; i++)
					if (ary[i] == id) return true;
				return false;
			}
		}

	Bitrix.CatalogueComparer.items = {};
	Bitrix.CatalogueComparer.getItem = function(iblockId) {
		if (!(this.items instanceof Object)) return null;
		var s = iblockId.toString();
		return s in this.items ? this.items[s] : null;
	}
	Bitrix.CatalogueComparer.create = function(options) {
		var self = new Bitrix.CatalogueComparer();
		self.initialize(options);

		if (options instanceof Object && "iblockId" in options) {
			if (!(this.items instanceof Object)) this.items = {};
			this.items[options.iblockId.toString()] = self;
		}
		return self;
	}
	Bitrix.CatalogueComparer.containsElementId = function(iblockId, elementId) {
		var item = this.getItem(iblockId);
		return item ? item.containsElementId(elementId) : false;
	}
}
