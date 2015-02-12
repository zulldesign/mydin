if (typeof (Bitrix) == "undefined") {
    var Bitrix = new Object();
}

if (typeof (Bitrix.TypeUtility) == "undefined") {
	Bitrix.TypeUtility = function Bitrix$TypeUtility() { }
	Bitrix.TypeUtility.isString = function(item) {
		return typeof (item) == "string" || item instanceof String;
	}
	Bitrix.TypeUtility.isNotEmptyString = function(item) {
		return typeof (item) == "string" || item instanceof String ? item.length > 0 : false;
	}
	Bitrix.TypeUtility.isBoolean = function(item) {
		return typeof (item) == "boolean" || item instanceof Boolean;
	}
	Bitrix.TypeUtility.isNumber = function(item) {
		return typeof (item) == "number" || item instanceof Number;
	}
	Bitrix.TypeUtility.isFunction = function(item) {
		return typeof (item) == "function" || item instanceof Function;
	}
	Bitrix.TypeUtility.isElementNode = function(item) {
		return item && typeof (item) == "object" && "nodeType" in item && item.nodeType == 1; //document.body.ELEMENT_NODE;
	}
	Bitrix.TypeUtility.isDomNode = function(item) {
		return item && typeof (item) == "object" && "nodeType" in item;
	}
	Bitrix.TypeUtility.isDomElement = function(item) {
		return this.isElementNode(item);
	}
	Bitrix.TypeUtility.tryConvertToString = function(item) {
		if (typeof (item) == "string" || item instanceof String)
			return item;
		if (typeof (item.toString) == "function")
			return item.toString();
		if (item == null)
			return "null";
		if (item == undefined)
			return "undefined"
		return "<?>";
	}
	Bitrix.TypeUtility.tryConvertToFloat = function(item, def) {
		if (typeof (item) == "number" || item instanceof Number)
			return item;
		if (item == null || item == undefined)
			return def ? def : Number.Nan;
		return parseFloat(this.isString(item) ? item : item.toString());
	}
	Bitrix.TypeUtility.tryConvertToInt = function(item, def) {
		if (typeof (item) == "number" || item instanceof Number)
			return item;
		if (item == null || item == undefined)
			return def ? def : Number.Nan; ;
		return parseInt(this.isString(item) ? item : item.toString());
	}
	Bitrix.TypeUtility.createDelegate = function(instance, method) {
		return function() {
			if (!method) throw "TypeUtility.createDelegate: method is not defined";
			return method.apply(instance, arguments);
		}
	}
	Bitrix.TypeUtility.createSimpleEventHandlerDelegate = function(instance, method) {
		return function() {
			return method.call(instance, this);
		}
	}
	Bitrix.TypeUtility.returnFalse = function() { return false; }
	Bitrix.TypeUtility.disableSelection = function(target) {

		if (/*@cc_on!@*/false) {//IE route
			target.setAttribute("UNSELECTABLE", "on");
			if (typeof (target.onselectstart) != "undefined")
				target.onselectstart = Bitrix.TypeUtility.returnFalse;
		}
		else if (typeof target.style.MozUserSelect != "undefined") {//Firefox route
			target.style.MozUserSelect = "none";
		}
		else {//All other route (ie: Opera)
			target.onmousedown = Bitrix.TypeUtility.returnFalse;
		}
	}

	Bitrix.TypeUtility.tryGetObjProp = function(obj, prop, defaultVal) {
		if (!this.isNotEmptyString(prop)) return defaultVal;
		return obj && typeof (obj) == "object" && prop in obj ? obj[prop] : defaultVal;
	}

	Bitrix.TypeUtility.csvToArray = function(csv) {
		var result = [];
		var myregexp = /'(?:''|[^'\r\n])*'|[^;\r\n]*/;
		if (str == '')
			return result;
		var m = str.match(myregexp);
		while (m != null) {
			var v = m[0];
			if (v.length >= 2 && v.charAt([0]) == '\'' && v.charAt(v.length - 1) == '\'')
				v = v.slice(1, -1).replace(/''/g, '\'');
			result.push(v);

			if (m.index + m[0].length < str.length)
				str = str.substring(m.index + m[0].length + 1);
			else
				break;
			m = str.match(myregexp);
		}
		return result;
	}
	Bitrix.TypeUtility.arrayToCsv = function(array) {
		var l = array.length;
		if (!l) return '';
		var result = '';
		for (var i = 0; i < l; i++) {
			var s = array[i].toString();
			if (s == '' || jsUtils.trim(s).length != s.length || s.search(/[';]/))
				s = '\'' + s.replace(/'/g, '\'\'') + '\'';
			if (result.length != 0)
				result += ';'
			result += s;
		}
		return result;
	}

	Bitrix.TypeUtility.extend = function(child, parent) {
		var f = function() { };
		f.prototype = parent.prototype;
		child.prototype = new f();
		child.prototype.constructor = child;
		child.superclass = parent.prototype;
	}
	Bitrix.TypeUtility.copyPrototype = function(descendant, parent) {
		var sConstructor = parent.toString();
		var aMatch = sConstructor.match(/\s*function (.*)\(/);
		if (aMatch != null) { descendant.prototype[aMatch[1]] = parent; }
		for (var m in parent.prototype) {
			descendant.prototype[m] = parent.prototype[m];
		}
	}
	if (typeof (Bitrix.TypeUtility.registerClass) == "function" && !Type.isClass(Bitrix.TypeUtility))
		Bitrix.TypeUtility.registerClass("Bitrix.TypeUtility");

	Bitrix.HttpUtility = function Bitrix$HttpUtility() { }
	Bitrix.HttpUtility.createXMLHttpRequest = function() { return window.ActiveXObject ? new ActiveXObject("Microsoft.XMLHTTP") : new XMLHttpRequest(); }
	Bitrix.HttpUtility.htmlDecode = function(text) {
		if (!Bitrix.TypeUtility.isNotEmptyString(text)) return "";
		if (/*@cc_on!@*/true) { //NOT IE
			var tmp = document.createElement("TEXTAREA");
			tmp.innerHTML = text;
			var r = tmp.value;
			delete tmp;
			return r;
		}
		else //IE
			return text.replace(/\&quot;/g, "\"").replace(/\&apos;/g, "\'").replace(/\&gt;/g, ">").replace(/\&lt;/g, "<").replace(/\&shy;/g, "").replace(/\&amp;/g, "&");
	}

	Bitrix.HttpUtility.htmlEncode = function(text) {
		if (!Bitrix.TypeUtility.isNotEmptyString(text)) return "";
		var d = document.createElement('DIV');
		var t = document.createTextNode(text);
		d.appendChild(t);
		var r = d.innerHTML;
		delete d;
		return r.replace(/"/g, "&quot;");
	}

	Bitrix.HttpUtility.jsonEncode = function(text) {
		var escapable = /[\\\"\u0000-\u001f\u007f-\u009f\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff\u003c\u003e]/g;
		var meta = {    // table of character substitutions                '\b': '\\b',
			'\t': '\\t',
			'\n': '\\n',
			'\f': '\\f',
			'\r': '\\r',
			'"': '\\"',
			'\\': '\\\\',
			'<': '\\u003c',
			'>': '\\u003e'
		};
		escapable.lastIndex = 0;
		if (!escapable.test(text)) return text;
		return text.replace(escapable,
		function(o) {
			var c = meta[o];
			return typeof (c) == "string" ? c : "\\u" + ("0000" + o.charCodeAt(0).toString(16)).slice(-4);
		});
	}

	Bitrix.EventUtility = function () {
	    var attachedEvents = [];

	    return {
	        addEventListener:
		(
			window.attachEvent
			? function (target, name, callback) {
			    if (!target) return;
			    var item = {
			        target: target,
			        name: name,
			        callback: callback,
			        handler: function () {
			            callback.call(target, window.event);
			        }
			    };
			    target.attachEvent("on" + name, item.handler);
			    attachedEvents.push(item);
			}
			: function (target, name, callback, capture) {
			    if (target) target.addEventListener(name, callback, capture);
			}
		),
	        removeEventListener:
		(
			window.detachEvent
			? function (target, name, callback) {
			    var l = attachedEvents.length;
			    for (var i = 0; i < l; i++) {
			        var item = attachedEvents[i];
			        if (item.target == target && item.name == name && item.callback == callback) {
			            target.detachEvent("on" + name, item.handler);
			            attachedEvents.splice(i, 1);
			            break;
			        }
			    }
			}
			: function (target, name, callback, capture) {
			    target.removeEventListener(name, callback, capture);
			}
		),
	        stopEventPropagation: function (e) {
	            if (!e) e = window.event;
	            if (e.stopPropagation) e.stopPropagation();  // DOM Level 2
	            else e.cancelBubble = true;                  // IE		
	        }
	    }
	} ();

	Bitrix.WebAppHelper = function Bitrix$WebAppHelper() {
		if (typeof (Bitrix.WebAppHelper.initializeBase) == "function")
			Bitrix.WebAppHelper.initializeBase(this);
		this._initialized = false;
	}

	Bitrix.WebAppHelper.prototype = {
		//initialize: function(){ this.this._initialized = true;},
		getPath: function() {
			if (Bitrix.TypeUtility.isString(window["bitrixWebAppPath"]))
				return window["bitrixWebAppPath"];
			if (Bitrix.TypeUtility.isString(window["APPPath"]))
				return window["APPPath"];
			return "";
		}
	}

	Bitrix.WebAppHelper._instance = null;
	Bitrix.WebAppHelper.getInstance = function() {
		if (this._instance == null) {
			this._instance = new Bitrix.WebAppHelper();
			//this._instance.initialize();
		}
		return this._instance;
	}
	Bitrix.WebAppHelper.getPath = function() {
		return this.getInstance().getPath();
	}

	Bitrix.NavigationHelper = function() { }
	Bitrix.NavigationHelper.isOpera = function() {
		try { return "opera" in window && navigator.userAgent.indexOf("Opera") >= 0; }
		catch (e) { return false; }
	}

	Bitrix.NavigationHelper.isSafari = function() {
		if (!("navigator" in window))
			return false;
		var nav = window.navigator;
		if ("vendor" in nav && Bitrix.TypeUtility.isNotEmptyString(nav.vendor) && nav.vendor.indexOf("Apple") < 0)
			return false;
		return "userAgent" in nav && Bitrix.TypeUtility.isNotEmptyString(nav.userAgent) && nav.userAgent.indexOf("Safari") >= 0;
	}

	Bitrix.NavigationHelper.isFirefox = function() {
		if (!("navigator" in window))
			return false;
		var ua = window.navigator.userAgent.toLowerCase();
		return Bitrix.TypeUtility.isNotEmptyString(ua) && ua.indexOf("firefox") >= 0;
	}

	if (typeof (Bitrix.WebAppHelper.registerClass) == "function" && !Type.isClass(Bitrix.WebAppHelper))
		Bitrix.WebAppHelper.registerClass("Bitrix.WebAppHelper");

	Bitrix.GoodbyeWindow = function Bitrix$GoodbyeWindow() {
		if (typeof (Bitrix.GoodbyeWindow.initializeBase) == "function")
			Bitrix.GoodbyeWindow.initializeBase(this);
		this._initialized = false;
		this._containerDivId = "bx_googbye_window_container";
		this._content = "";
		this._topPanelDivId = "bx_googbye_window_topPanel";
		this._contentDivId = "bx_googbye_window_content";
		this._contentSpanId = "bx_googbye_window_content_text";
		this._timeToLive = 1;
		this._layoutType = "";
		this._onShowClientScript = null;
		this._onHideClientScript = null;
		this._isVisible = false;

		this._containerDivCssClass = "";
		this._topPanelDivCssClass = "";
		this._contentDivCssClass = "";
		this._contentSpanCssClass = "";
		this._layoutCssClassSuccess = "";
		this._layoutCssClassError = "";
	}

	Bitrix.GoodbyeWindow.prototype = {
		initialize: function() {
			if (this._initialized) return;
			this.reset();
			this._initialized = true;
		},

		getContent: function() {
			return this._content;
		},
		setContent: function(value) {
			this._content = value;
		},

		getLayoutType: function() {
			return this._layoutType;
		},
		setLayoutType: function(value) {
			this._layoutType = value;
		},

		getTimeToLive: function() {
			return this._timeToLive;
		},
		setTimeToLive: function(value) {
			if (isNaN(value))
				throw String.concat("'", value, "'", " is not a number!");

			this._timeToLive = parseInt(value);
		},

		getContainerDivCssClass: function() {
			return this._containerDivCssClass;
		},

		setContainerDivCssClass: function(val) {
			this._containerDivCssClass = val;
		},

		getTopPanelDivCssClass: function() {
			return this._topPanelDivCssClass;
		},

		setTopPanelDivCssClass: function(val) {
			this._topPanelDivCssClass = val;
		},

		getContentDivCssClass: function() {
			return this._contentDivCssClass;
		},

		setContentDivCssClass: function(val) {
			this._contentDivCssClass = val;
		},

		getContentSpanCssClass: function() {
			return this._contentSpanCssClass;
		},

		setContentSpanCssClass: function(val) {
			this._contentSpanCssClass = val;
		},

		getLayoutCssClassSuccess: function() {
			return this._layoutCssClassSuccess;
		},

		setLayoutCssClassSuccess: function(val) {
			this._layoutCssClassSuccess = val;
		},

		getLayoutCssClassError: function() {
			return this._layoutCssClassError;
		},

		setLayoutCssClassError: function(val) {
			this._layoutCssClassError = val;
		},

		getOnHideClientScript: function() {
			return this._onHideClientScript;
		},
		setOnHideClientScript: function(value) {
			this._onHideClientScript = value;
		},

		getOnShowClientScript: function() {
			return this._onShowClientScript;
		},
		setOnShowClientScript: function(value) {
			this._onShowClientScript = value;
		},

		getDefaultTimeToLive: function() {
			return 1800;
		},

		reset: function() {
			this._content = "Everything Is Fine...";
			this._timeToLive = this.getDefaultTimeToLive();
			this._layoutType = "SUCCESS";
			this._onShowClientScript = null;
			this._onHideClientScript = null;
		},

		show: function() {
			if (this._isVisible)
				return;

			if (this._onShowClientScript != null && this._onShowClientScript.length > 0)
				window.eval(this._onShowClientScript);

			var containerDiv = document.getElementById(this._containerDivId);

			if (containerDiv == null) {
				containerDiv = document.createElement("DIV");
				containerDiv.style.visibility = "hidden";
				containerDiv = document.body.appendChild(containerDiv);
				containerDiv.id = this._containerDivId;
				if (Bitrix.TypeUtility.isNotEmptyString(this._containerDivCssClass))
					containerDiv.className = this._containerDivCssClass;
				else {
					containerDiv.style.backgroundColor = "#edf1f3";
					containerDiv.style.borderWidth = "1px";
					containerDiv.style.borderStyle = "solid";
					containerDiv.style.borderColor = "#ad9634";
					containerDiv.style.padding = "1px";
					containerDiv.style.margin = "0px";
					containerDiv.style.position = "absolute";
					containerDiv.style.textAlign = "left";
					containerDiv.style.fontFamily = "Tahoma";
					containerDiv.style.fontSize = "12px";
					containerDiv.style.fontWeight = "normal";
					containerDiv.style.zIndex = "1001";
				}
			}

			var topPanelDiv = document.getElementById(this._topPanelDivId);

			if (topPanelDiv == null) {
				topPanelDiv = document.createElement("DIV");
				containerDiv.appendChild(topPanelDiv);
				topPanelDiv.id = this._topPanelDivId;
				if (Bitrix.TypeUtility.isNotEmptyString(this._topPanelDivCssClass))
					topPanelDiv.className = this._topPanelDivCssClass;
				else {
					topPanelDiv.onclick = function() { Bitrix.GoodbyeWindow.getInstance().hide(); };
					topPanelDiv.style.width = "10px";
					topPanelDiv.style.height = "10px";
					topPanelDiv.style.margin = "0px";
					topPanelDiv.style.padding = "0px";
					topPanelDiv.style.position = "absolute";
					topPanelDiv.style.top = "1px";
					topPanelDiv.style.right = "1px";
					var imgUrl = Bitrix.PathHelper.combine(Bitrix.PathHelper.getApplicationPath(), "bitrix/images/close_window_mini_button.gif");
					topPanelDiv.style.background = "transparent url(" + imgUrl + ") no-repeat scroll 0% 0%";
					topPanelDiv.style.cursor = "pointer";
				}
			}


			var contentDiv = document.getElementById(this._contentDivId);
			if (contentDiv == null) {
				contentDiv = document.createElement("DIV");
				containerDiv.appendChild(contentDiv);
				contentDiv.id = this._contentDivId;
				if (Bitrix.TypeUtility.isNotEmptyString(this._contentDivCssClass))
					contentDiv.className = this._contentDivCssClass;
				else {
					contentDiv.style.padding = "0px";
					contentDiv.style.margin = "5px 10px 5px 10px";
				}
			}


			var contentSpan = document.getElementById(this._contentSpanId);
			if (contentSpan == null) {
				contentSpan = document.createElement("SPAN");
				contentDiv.appendChild(contentSpan);
				contentSpan.id = this._contentSpanId;
				if (Bitrix.TypeUtility.isNotEmptyString(this._contentSpanCssClass))
					contentSpan.className = this._contentSpanCssClass;
				else {
					contentSpan.style.padding = "0px";
					contentSpan.style.margin = "0px";
				}
			}

			contentSpan.innerHTML = "";
			contentSpan.innerHTML = this._content;

			if (Bitrix.TypeUtility.isNotEmptyString(this._layoutCssClassSuccess)) {
				var rxSuccess = new RegExp(this._layoutCssClassSuccess, "gi");
				contentSpan.className = contentSpan.className.replace(rxSuccess, "");
			}

			if (Bitrix.TypeUtility.isNotEmptyString(this._layoutCssClassError)) {
				var rxError = new RegExp(this._layoutCssClassError, "gi");
				contentSpan.className = contentSpan.className.replace(rxError, "");
			}

			if (this._layoutType == "SUCCESS") {
				if (Bitrix.TypeUtility.isNotEmptyString(this._layoutCssClassSuccess))
					contentSpan.className = contentSpan.className.concat(" ", this._layoutCssClassSuccess);
				else
					contentSpan.style.color = "Green";
			}
			else if (this._layoutType == "ERROR") {
				if (Bitrix.TypeUtility.isNotEmptyString(this._layoutCssClassError))
					contentSpan.className = contentSpan.className.concat(" ", this._layoutCssClassError);
				else
					contentSpan.style.color = "Red";
			}

			var windowSize = jsUtils.GetWindowInnerSize();
			var windowScroll = jsUtils.GetWindowScrollPos();

			containerDiv.style.visibility = "visible";

			var textWidth = parseInt(contentDiv.offsetWidth);
			if (textWidth > 250) {
				var textHeight = parseInt(contentDiv.offsetHeight);
				var textWidth = Math.round(Math.sqrt(2.1 * textWidth * textHeight));
				contentDiv.style.width = textWidth + "px";
			}

			var left = parseInt(windowScroll.scrollLeft + windowSize.innerWidth / 2 - containerDiv.offsetWidth / 2);
			var top = parseInt(windowScroll.scrollTop + windowSize.innerHeight / 2 - containerDiv.offsetHeight / 2);

			containerDiv.style.top = top + "px";
			containerDiv.style.left = left + "px";

			if (this._timeToLive >= 0)
				window.setTimeout(Bitrix.TypeUtility.createDelegate(this, this.hide), this._timeToLive > 0 ? this._timeToLive : this.getDefaultTimeToLive());

			this._isVisible = true;
		},

		hide: function() {
			if (!this._isVisible)
				return;

			var containerDiv = document.getElementById(this._containerDivId);
			if (containerDiv == null) return;
			containerDiv.style.visibility = "hidden";

			var onHideScript = this._onHideClientScript;
			this.reset();

			if (onHideScript != null && onHideScript.length > 0)
				window.eval(onHideScript);

			this._isVisible = false;
		},

		_getStyleAttributeValue: function(srcElem, attrName) {
			if (!srcElem) throw "Source element is not defined!";
			if (Sys.Browser.agent === Sys.Browser.InternetExplorer) { // IE
				var elmStyle = srcElem.currentStyle;
				return elmStyle.getAttribute(attrName);
			}
			else {
				var elmCompStyle = document.defaultView.getComputedStyle(srcElem, '');
				return elmCompStyle.getPropertyValue(attrName);
			}
		},

		_parseInt: function(sourceValue, defaultValue) {
			if (!sourceValue || isNaN(sourceValue)) return defaultValue;
			return parseInt(sourceValue);
		}
	}

	Bitrix.GoodbyeWindow._instance = null;
	Bitrix.GoodbyeWindow.getInstance = function() {
		if (this._instance == null) {
			this._instance = new Bitrix.GoodbyeWindow();
			this._instance.initialize();
		}
		return this._instance;
	}

	Bitrix.GoodbyeWindow.show = function(content, layout, ttl) {
		var instance = this.getInstance();
		instance.setContent(content);
		instance.setLayoutType(layout);
		instance.setTimeToLive(ttl);
		instance.show();
	}

	Bitrix.GoodbyeWindow.getContainerDivCssClass = function() {
		return this.getInstance().getContainerDivCssClass();
	}

	Bitrix.GoodbyeWindow.setContainerDivCssClass = function(val) {
		this.getInstance().setContainerDivCssClass(val);
	}

	Bitrix.GoodbyeWindow.getTopPanelDivCssClass = function() {
		return this.getInstance().getTopPanelDivCssClass();
	}

	Bitrix.GoodbyeWindow.setTopPanelDivCssClass = function(val) {
		this.getInstance().setTopPanelDivCssClass(val);
	}

	Bitrix.GoodbyeWindow.getContentDivCssClass = function() {
		return this.getInstance().getContentDivCssClass();
	}

	Bitrix.GoodbyeWindow.setContentDivCssClass = function(val) {
		this.getInstance().setContentDivCssClass(val);
	}

	Bitrix.GoodbyeWindow.getContentSpanCssClass = function() {
		return this.getInstance().getContentSpanCssClass();
	}

	Bitrix.GoodbyeWindow.setContentSpanCssClass = function(val) {
		this.getInstance().setContentSpanCssClass(val);
	}

	Bitrix.GoodbyeWindow.getLayoutCssClassSuccess = function() {
		return this.getInstance().getLayoutCssClassSuccess();
	}

	Bitrix.GoodbyeWindow.setLayoutCssClassSuccess = function(val) {
		this.getInstance().setLayoutCssClassSuccess(val);
	}

	Bitrix.GoodbyeWindow.getLayoutCssClassError = function() {
		return this.getInstance().getLayoutCssClassError();
	}

	Bitrix.GoodbyeWindow.setLayoutCssClassError = function(val) {
		this.getInstance().setLayoutCssClassError(val);
	}

	if (typeof (Bitrix.GoodbyeWindow.registerClass) == "function" && !Type.isClass(Bitrix.GoodbyeWindow))
		Bitrix.GoodbyeWindow.registerClass("Bitrix.GoodbyeWindow");

	Bitrix.ObjectHelper = function Bitrix$ObjectHelper() {
		this._initialized = false;
	}

	Bitrix.ObjectHelper.prototype = {
		initialize: function() {
			this._initialized = true;
		},
		tryGetBoolean: function(object, name, def) {
			if (typeof (object) != "object" || !object) throw "object is not valid!";
			if (!Bitrix.TypeUtility.isString(name)) throw "name is not valid!";
			if (!(name in object))
				return def;
			var v = object[name];
			if (Bitrix.TypeUtility.isBoolean(v))
				return v;
			if (Bitrix.TypeUtility.isString(v))
				return v.toUpperCase() == "TRUE";
			return def;
		},
		tryGetString: function(object, name, def) {
			if (typeof (object) != "object") throw "object is not valid!";
			if (!Bitrix.TypeUtility.isString(name)) throw "name is not valid!";
			return object && name in object && Bitrix.TypeUtility.isString(object[name]) ? object[name] : def;
		},
		tryGetBooleanByNameArray: function(object, nameArr, def) {
			if (typeof (object) != "object" || !object) throw "object is not valid!";
			if (!(nameArr instanceof Array)) throw "nameArr is not valid!";
			if (nameArr.length == 0)
				return def;

			var r = undefined;
			for (var i = 0; i < nameArr.length; i++) {
				if (!(nameArr[i] in object))
					continue;
				var v = object[nameArr[i]];
				if (Bitrix.TypeUtility.isBoolean(v)) {
					r = v;
					break;
				}
				if (Bitrix.TypeUtility.isString(v)) {
					r = v.toUpperCase() == "TRUE";
					break;
				}
			}
			return r != undefined ? r : def;
		},
		tryGetStringByNameArray: function(object, nameArr, def) {
			if (typeof (object) != "object" || !object) throw "object is not valid!";
			if (!(nameArr instanceof Array)) throw "nameArr is not valid!";
			if (nameArr.length == 0)
				return def;
			var r = undefined;
			for (var i = 0; i < nameArr.length; i++) {
				if (!(nameArr[i] in object))
					continue;
				var v = object[nameArr[i]];
				if (Bitrix.TypeUtility.isString(v)) {
					r = v;
					break;
				}
			}
			return r != undefined ? r : def;
		},
		tryGetIntByNameArray: function(object, nameArr, def) {
			if (typeof (object) != "object" || !object) throw "object is not valid!";
			if (!(nameArr instanceof Array)) throw "nameArr is not valid!";
			if (nameArr.length == 0)
				return def;
			var r = undefined;
			for (var i = 0; i < nameArr.length; i++) {
				if (!(nameArr[i] in object))
					continue;
				var v = object[nameArr[i]];
				if (Bitrix.TypeUtility.isNumber(v)) {
					r = v;
					break;
				}
				if (Bitrix.TypeUtility.isString(v)) {
					var r = parseInt(v);
					if (!isNaN(r))
						break;
					r = undefined;
				}
			}
			return r != undefined ? r : def;
		},
		tryGetInt: function(object, name, def) {
			if (typeof (object) != "object" || !object) throw "object is not valid!";
			if (!Bitrix.TypeUtility.isString(name)) throw "name is not valid!";
			if (!(name in object))
				return def;
			var v = object[name];
			if (Bitrix.TypeUtility.isNumber(v))
				return v;
			if (Bitrix.TypeUtility.isString(v)) {
				var r = parseInt(v);
				if (!isNaN(r))
					return r;
			}
			return def;
		},
		tryGetFloat: function(object, name, def) {
			if (typeof (object) != "object" || !object) throw "object is not valid!";
			if (!Bitrix.TypeUtility.isString(name)) throw "name is not valid!";
			if (!(name in object))
				return def;
			var v = object[name];
			if (Bitrix.TypeUtility.isNumber(v))
				return v;
			if (Bitrix.TypeUtility.isString(v)) {
				var r = parseFloat(v);
				if (!isNaN(r))
					return r;
			}
			return def;
		},
		setStringIfNotEmpty: function(object, name, val) {
			if (typeof (object) != "object" || !object) throw "object is not valid!";
			if (!Bitrix.TypeUtility.isString(name)) throw "name is not valid!";
			if (Bitrix.TypeUtility.isNotEmptyString(val))
				object[name] = val;
		},
		setString: function(object, name, val) {
			if (typeof (object) != "object" || !object) throw "object is not valid!";
			if (!Bitrix.TypeUtility.isString(name)) throw "name is not valid!";
			if (Bitrix.TypeUtility.isString(val))
				object[name] = val;
		},
		setBoolean: function(object, name, val) {
			if (typeof (object) != "object" || !object) throw "object is not valid!";
			if (!Bitrix.TypeUtility.isString(name)) throw "name is not valid!";
			if (Bitrix.TypeUtility.isNumber(val))
				object[name] = val > 0;
			else if (Bitrix.TypeUtility.isBoolean(val))
				object[name] = val;
		},
		setNumber: function(object, name, val) {
			if (typeof (object) != "object" || !object) throw "object is not valid!";
			if (!Bitrix.TypeUtility.isString(name)) throw "name is not valid!";
			if (Bitrix.TypeUtility.isNumber(val))
				object[name] = val;
		}
	}


	Bitrix.ObjectHelper._instance = null;
	Bitrix.ObjectHelper.getInstance = function() {
		if (this._instance == null) {
			this._instance = new Bitrix.ObjectHelper();
			this._instance.initialize();
		}
		return this._instance;
	}

	Bitrix.ObjectHelper.tryGetBoolean = function(object, name, def) {
		return this.getInstance().tryGetBoolean(object, name, def);
	}

	Bitrix.ObjectHelper.tryGetBooleanByNameArray = function(object, nameArr, def) {
		return this.getInstance().tryGetBooleanByNameArray(object, nameArr, def);
	}

	Bitrix.ObjectHelper.tryGetString = function(object, name, def) {
		return this.getInstance().tryGetString(object, name, def);
	}
	Bitrix.ObjectHelper.tryGetStringByNameArray = function(object, nameArr, def) {
		return this.getInstance().tryGetStringByNameArray(object, nameArr, def);
	}
	Bitrix.ObjectHelper.tryGetInt = function(object, name, def) {
		return this.getInstance().tryGetInt(object, name, def);
	}
	Bitrix.ObjectHelper.tryGetFloat = function(object, name, def) {
		return this.getInstance().tryGetFloat(object, name, def);
	}
	Bitrix.ObjectHelper.tryGetIntByNameArray = function(object, nameArr, def) {
		return this.getInstance().tryGetIntByNameArray(object, nameArr, def);
	}
	Bitrix.ObjectHelper.setStringIfNotEmpty = function(object, name, val) {
		return this.getInstance().setStringIfNotEmpty(object, name, val);
	}

	Bitrix.ObjectHelper.setString = function(object, name, val) {
		return this.getInstance().setString(object, name, val);
	}

	Bitrix.ObjectHelper.setBoolean = function(object, name, val) {
		return this.getInstance().setBoolean(object, name, val);
	}

	Bitrix.ObjectHelper.setNumber = function(object, name, val) {
		return this.getInstance().setNumber(object, name, val);
	}

	if (typeof (Bitrix.ObjectHelper.registerClass) == "function" && !Type.isClass(Bitrix.ObjectHelper))
		Bitrix.ObjectHelper.registerClass("Bitrix.ObjectHelper");

	Bitrix.ArrayHelper = {};
	Bitrix.ArrayHelper.toString = function(src) {
		if (Bitrix.TypeUtility.isString(src)) return src;
		if (src instanceof Array) {
			var r = "";
			for (var i = 0; i < src.length; i++) {
				var n = src[i].toString();
				if (!n) continue;
				if (r.length > 0) r += ",";
				r += n;
			}
			return r;
		}
		var n = this.getName(values, src);
		return n ? n : "";
	}
	Bitrix.ArrayHelper.fromString = function(str) {
		var r = [];
		var parts = Bitrix.TypeUtility.isNotEmptyString(str) ? str.split(",") : null;
		if (parts && parts.length > 0)
			for (var i = 0; i < parts.length; i++) {
			var name = jsUtils.trim(parts[i]);
			if (!name) continue;
			r.push(name);
		}
		return r;
	}

	Bitrix.ArrayHelper.findInArray = function(value, array) {
		if (array instanceof Array)
			for (var i = 0; i < array.length; i++)
			if (array[i] === value) return i;
		return -1;
	}

	Bitrix.DomHelper = {};
	Bitrix.DomHelper.addToSelect = function(selectEl, array) {
		if (!(Bitrix.TypeUtility.isDomNode(selectEl) && array instanceof Array)) return;
		for (var i = 0; i < array.length; i++) {
			var val = array[i];
			var o = null;
			if (Bitrix.TypeUtility.isDomNode(val))
				o = val;
			else if (typeof (val) == 'object' && 'text' in val && 'value' in val) {
				var o = document.createElement("OPTION");
				o.text = val['text'];
				o.value = val['value'];
				if ('disabled' in val && val['disabled'])
					o.disabled = true;
			}
			if (o) {
				try {
					selectEl.add(o, null); // standards compliant; doesn't work in IE
				}
				catch (ex) {
					selectEl.add(o); // IE only
				}
			}
		}
	}
	Bitrix.DomHelper.removeAllFromSelect = function(selectEl) {
		if (!Bitrix.TypeUtility.isDomNode(selectEl)) return;
		while (selectEl.length > 0) selectEl.remove(0);
	}
	Bitrix.DomHelper.selectOption = function(selectEl, val, ignoreCase) {
		if (!(selectEl && Bitrix.TypeUtility.isDomNode(selectEl) && Bitrix.TypeUtility.isNotEmptyString(val))) return;
		for (var i = 0; i < selectEl.options.length; i++) {
			if (ignoreCase)
				selectEl.options[i].selected = selectEl.options[i].value.toUpperCase() == val.toUpperCase();
			else
				selectEl.options[i].selected = selectEl.options[i].value == val;
		}
	}

	Bitrix.DomHelper.selectOptions = function(selectEl, ary) {
		if (!(Bitrix.TypeUtility.isDomNode(selectEl) && ary instanceof Array)) return;
		for (var i = 0; i < selectEl.options.length; i++) {
			var o = selectEl.options[i];
			var sel = false;
			for (var j = 0; j < ary.length; j++)
				if (o.value == ary[j]) {
				sel = true;
				break;
			}
			o.selected = sel;
		}
	}

	Bitrix.DomHelper.display = function(el, display) {
		display = !!display;
		if (el && Bitrix.TypeUtility.isDomNode(el)) el.style.display = display ? "" : "none";
	}

	Bitrix.DomHelper.innerHTML = function(el, html) {
		if (el && Bitrix.TypeUtility.isDomNode(el)) el.innerHTML = html;
	}

	Bitrix.PathHelper = function Bitrix$PathHelper() {
		this._initialized = false;
		this._isAbsoluteUrlRx = null;
		this._absoluteUrlPathRx = null;
	}

	Bitrix.PathHelper.prototype = {
		initialize: function() {
			this._initialized = true;
			this._isAbsoluteUrlRx = new RegExp("^[a-z][a-z,0-9]+([\.][a-z,0-9]+)?\:", "i");
			this._absoluteUrlPathRx = new RegExp("^([a-z\.0-9]+)\:(//)?([a-z_0-9\:\@\.]+)/(.+)", "i");
		},
		getApplicationPath: function() {
			return "bitrixWebAppPath" in window && Bitrix.TypeUtility.isNotEmptyString(window["bitrixWebAppPath"]) ? window["bitrixWebAppPath"] : "/";
		},
		isVirtual: function(path) {
			if (!Bitrix.TypeUtility.isString(path)) return false;
			return path.length > 0 && path.charAt(0) == "~";
		},
		isAbsolute: function(path) {
			if (!Bitrix.TypeUtility.isString(path)) return false;
			return this._isAbsoluteUrlRx.test(path);
		},
		isRelative: function(path) {
			return !this.isAbsolute(path);
		},
		parse: function(path) {
			if (!Bitrix.TypeUtility.isString(path)) throw "path is not valid!";

			if (this.isAbsolute(path)) {
				var m = this._absoluteUrlPathRx.exec(path);
				if (m == null) return null;
				return { scheme: m[1], schemeSeparator: m[2], hostAndPort: m[3], path: m[4] };
			}

			if (this.isRelative(path))
				return { scheme: "", schemeSeparator: "", hostAndPort: "", path: path };

			if (this.isVirtual(path))
				throw "Virtual paths is not supported!";

			throw path + " could not be parsed!";
		},
		resolveVirtualPath: function(path) {
			if (!this.isVirtual(path))
				return path;
			var appPath = this.getApplicationPath();
			appPath = this.appendTrailingSlash(appPath);

			path = path.substr(1);
			while (path.length > 0 && path.charAt(0) == "/")
				path = path.substr(1);

			return appPath.concat(path);
		},
		makeVirtual: function(path) {
			if (!Bitrix.TypeUtility.isString(path)) throw "path is not valid!";
			if (this.isVirtual(path))
				return path;
			var appPath = this.getApplicationPath();
			appPath = this.appendTrailingSlash(appPath);
			if (path.indexOf(appPath) != 0)
				return path;
			return "~/" + path.substr(appPath.length);
		},
		appendTrailingSlash: function(path) {
			if (!Bitrix.TypeUtility.isString(path)) throw "path is not valid!";
			if (path.length == 0)
				return "/";
			if (path.charAt(path.length - 1) != "/")
				path += "/";
			return path;
		},
		combine: function(path1, path2) {
			if (!path1 && !path2) return "";

			if (path1 && !Bitrix.TypeUtility.isString(path1)) throw "path1 is not valid!";
			if (path2 && !Bitrix.TypeUtility.isString(path2)) throw "path2 is not valid!";

			if (!path2) return path1;
			if (!path1) return path2;

			if (this.isVirtual(path2) || !this.isRelative(path2))
				return path2;

			var toUp = 0;
			if (path2.charAt(0) == "/")
				return path2;
			while (path2.length > 0) {
				if (path2.indexOf("../") == 0) {
					path2 = path2.substr(3);
					toUp++;
					continue;
				}
				if (path2.indexOf("./") == 0) {
					path2 = path2.substr(2);
					continue;
				}
				if (path2.indexOf("/") == 0) {
					path2 = path2.substr(1);
					continue;
				}
				break;
			}



			if (toUp > 0) {
				var path1IsVirtual = this.isVirtual(path1);
				if (path1IsVirtual)
					path1 = this.resolveVirtualPath(path1);

				var fragments = this.parse(path1);
				var path = fragments != null ? fragments.path : "";
				if (path.length > 0) {
					while (toUp > 0) {
						var partIndex = path.lastIndexOf("/");
						if (partIndex < 0) {
							path = "";
							break;
						}
						path = path.substr(0, partIndex);
						toUp--;
					}
					path1 = fragments.scheme + ":" + fragments.schemeSeparator + fragments.hostAndPort + "/";
					if (path.length > 0)
						path1 += path;
				}

				if (path1IsVirtual)
					path1 = this.makeVirtual(path1);
			}

			var r = path1;
			if (path2.length > 0) {
				r = this.appendTrailingSlash(r) + path2;
			}

			return r;
		},
		getFileExtension: function(path) {
			if (!Bitrix.TypeUtility.isString(path)) throw "path is not valid!";
			var ext = "";
			var queryIndex = path.indexOf("?");
			if (queryIndex >= 0)
				path = path.substr(0, queryIndex);

			var dotIndex = path.lastIndexOf(".");
			if (dotIndex < 0) return "";
			return path.substr(dotIndex, path.length - dotIndex);
		}
	}

	Bitrix.PathHelper._instance = null;
	Bitrix.PathHelper.getInstance = function() {
		if (this._instance == null) {
			this._instance = new Bitrix.PathHelper();
			this._instance.initialize();
		}
		return this._instance;
	}

	Bitrix.PathHelper.getApplicationPath = function() {
		return this.getInstance().getApplicationPath();
	}

	Bitrix.PathHelper.isVirtual = function(path) {
		return this.getInstance().isVirtual(path);
	}

	Bitrix.PathHelper.resolveVirtualPath = function(path) {
		return this.getInstance().resolveVirtualPath(path);
	}

	Bitrix.PathHelper.isRelative = function(path) {
		return this.getInstance().isRelative(path);
	}

	Bitrix.PathHelper.getUrlPath = function(path) {
		return this.getInstance().getUrlPath(path);
	}

	Bitrix.PathHelper.combine = function(path1, path2) {
		return this.getInstance().combine(path1, path2);
	}

	Bitrix.PathHelper.getFileExtension = function(path) {
		return this.getInstance().getFileExtension(path);
	}

	if (typeof (Bitrix.PathHelper.registerClass) == "function" && !Type.isClass(Bitrix.PathHelper))
		Bitrix.PathHelper.registerClass("Bitrix.PathHelper");

	Bitrix.EnumHelper = function Bitrix$EnumHelper() { }

	Bitrix.EnumHelper.getName = function(values, id) {
		if (!(values instanceof Object)) throw "Bitrix.EnumHelper.getName: values are not specified!";
		if (!(typeof (id) == "number" || id instanceof Number)) throw "Bitrix.EnumHelper.getName: id is not specified!";
		for (var key in values)
			if (values[key] === id)
			return key;
		return null;
	}

	Bitrix.EnumHelper.getId = function(values, name, ignoreCase) {
		if (!(values instanceof Object)) throw "Bitrix.EnumHelper.getId: values are not specified!";
		if (!(typeof (name) == "string" || name instanceof String)) throw "Bitrix.EnumHelper.getId: name is not specified!";

		for (var key in values)
			if (key === name || (ignoreCase === true && key.toUpperCase() === name.toUpperCase()))
			return values[key];
		return null;
	}

	Bitrix.EnumHelper.checkPresenceById = function(values, id) {
		return this.getName(values, id) != null;
	}

	Bitrix.EnumHelper.fromString = function(str, values) {
		var r = new Array();
		var parts = Bitrix.TypeUtility.isNotEmptyString(str) ? str.split(",") : null;
		if (parts && parts.length > 0)
			for (var i = 0; i < parts.length; i++) {
			var name = jsUtils.trim(parts[i]);
			if (!name) continue;
			var id = this.getId(values, name, true);
			if (id) r.push(id);
		}
		return r;
	}

	Bitrix.EnumHelper.toString = function(src, values) {
		if (Bitrix.TypeUtility.isString(src)) return src;
		if (src instanceof Array) {
			var r = "";
			for (var i = 0; i < src.length; i++) {
				var n = this.getName(values, src[i]);
				if (!n) continue;
				if (r.length > 0) r += ",";
				r += n;
			}
			return r;
		}
		var n = this.getName(values, src);
		return n ? n : "";
	}
	Bitrix.EnumHelper.findValue = function(val, values) {
		if (!Bitrix.TypeUtility.isNumber(val)) return null;
		for (var key in values)
			if (values[key] == val)
			return val;
		return null;
	}
	Bitrix.EnumHelper.tryParse = function(obj, values) {
		if (Bitrix.TypeUtility.isString(obj)) return this.fromString(obj, values);
		var r = [];
		if (Bitrix.TypeUtility.isNumber(obj)) {
			var val = this.findValue(obj, values);
			if (val) r.push(r);
		}
		return r;
	}

	if (typeof (Bitrix.EnumHelper.registerClass) == "function" && !Type.isClass(Bitrix.EnumHelper))
		Bitrix.EnumHelper.registerClass("Bitrix.EnumHelper");

	Bitrix.EventPublisher = function Bitrix$EventPublisher() {
		this._listeners = null;
	}

	Bitrix.EventPublisher.prototype.addListener = function(listener) {
		if (!Bitrix.TypeUtility.isFunction(listener)) throw "listener is not valid!";
		if (this._listeners == null)
			this._listeners = new Array();
		this._listeners.push(listener);
	}


	Bitrix.EventPublisher.prototype.removeListener = function(listener) {
		if (!Bitrix.TypeUtility.isFunction(listener)) throw "listener is not valid!";
		if (this._listeners == null) return;
		var index = -1;
		for (var i = 0; i < this._listeners.length; i++) {
			if (this._listeners[i] != listener) continue;
			index = i;
			break;
		}
		if (index < 0) return;
		this._listeners.splice(index, 1);
	}

	Bitrix.EventPublisher.prototype.fire = function() {
		if (this._listeners == null) return;
		for (var i = 0; i < this._listeners.length; i++) {
			this._listeners[i].apply(this, arguments);
		}
	}

	if (typeof (Bitrix.EventPublisher.registerClass) == "function" && !Type.isClass(Bitrix.EventPublisher))
		Bitrix.EventPublisher.registerClass("Bitrix.EventPublisher");

	Bitrix.ElementPositioningUtility = function() {
		if (typeof (Bitrix.ElementPositioningUtility.initializeBase) == "function")
			Bitrix.ElementPositioningUtility.initializeBase(this);
	}

	Bitrix.ElementPositioningUtility.prototype = {
		initialize: function() {
			this._isInitialized = true;
		},

		getElementRect: function(el) {
			var r = { top: 0, right: 0, bottom: 0, left: 0, width: 0, height: 0 };
			if (!el)
				return r;
			if (typeof (el.getBoundingClientRect) != "undefined") {
				var clientRect = el.getBoundingClientRect();
				var root = document.documentElement;
				var body = document.body;

				r.top = clientRect.top + root.scrollTop + body.scrollTop;
				r.left = clientRect.left + root.scrollLeft + body.scrollLeft;
				r.width = clientRect.right - clientRect.left;
				r.height = clientRect.bottom - clientRect.top;
				r.right = clientRect.right + root.scrollLeft + body.scrollLeft;
				r.bottom = clientRect.bottom + root.scrollTop + body.scrollTop;
			}
			else {
				var x = 0, y = 0, w = el.offsetWidth, h = el.offsetHeight;
				var first = true;
				for (; el != null; el = el.offsetParent) {
					x += el.offsetLeft;
					y += el.offsetTop;
					if (first) {
						first = false;
						continue;
					}
					var elBorderLeftWidth = 0, elBorderTopWidth = 0;
					//if(typeof(document.all) == "undefined" || typeof(window.opera) != "undefined"){
					if (Sys.Browser.agent != Sys.Browser.InternetExplorer) {
						var elCompStyle = document.defaultView.getComputedStyle(el, '');
						elBorderLeftWidth = parseInt(elCompStyle.getPropertyValue('border-left-width'));
						elBorderTopWidth = parseInt(elCompStyle.getPropertyValue('border-top-width'));
					}
					else {
						var elStyle = el.currentStyle;
						elBorderLeftWidth = parseInt(elStyle.getAttribute('borderLeftWidth'));
						elBorderTopWidth = parseInt(elStyle.getAttribute('borderTopWidth'));
					}
					if (!isNaN(elBorderLeftWidth) && elBorderLeftWidth > 0)
						x += elBorderLeftWidth;
					if (!isNaN(elBorderTopWidth) && elBorderTopWidth > 0)
						y += elBorderTopWidth;
				}

				r.top = y;
				r.left = x;
				r.width = w;
				r.height = h;
				r.right = r.left + w;
				r.bottom = r.top + h;
			}
			return r;
		},
		getOffset: function(offsetEl, sourceEl) {
			if (!offsetEl) throw "Bitrix.ElementPositioningUtility.getOffset: Offset element is not defined!";
			if (!sourceEl) throw "Bitrix.ElementPositioningUtility.getOffset: Source element is not defined!";

			var offsetElRect = this.getElementRect(offsetEl);
			var sourceElRect = this.getElementRect(sourceEl);

			return Bitrix.Point.create(offsetElRect.top - sourceElRect.top, offsetElRect.left - sourceElRect.left);
		},

		_getNumberOrZero: function(val) {
			return !isNaN(val) ? val : 0;
		},

		getElementMargins: function(el) {
			var r = { top: 0, right: 0, bottom: 0, left: 0 };
			if (!el)
				return r;

			if (/*@cc_on!@*/false) { //IE	
				var elStyle = el.runtimeStyle;

				r.top = this._getNumberOrZero(parseInt(elStyle.getAttribute('marginTop')));
				r.right = this._getNumberOrZero(parseInt(elStyle.getAttribute('marginRight')));
				r.bottom = this._getNumberOrZero(parseInt(elStyle.getAttribute('marginBottom')));
				r.left = this._getNumberOrZero(parseInt(elStyle.getAttribute('marginLeft')));
			}
			else {
				var elCompStyle = document.defaultView.getComputedStyle(el, '');
				r.top = this._getNumberOrZero(parseInt(elCompStyle.getPropertyValue('margin-top')));
				r.right = this._getNumberOrZero(parseInt(elCompStyle.getPropertyValue('margin-right')));
				r.bottom = this._getNumberOrZero(parseInt(elCompStyle.getPropertyValue('margin-bottom')));
				r.left = this._getNumberOrZero(parseInt(elCompStyle.getPropertyValue('margin-left')));
			}
			return r;
		}
	}

	Bitrix.ElementPositioningUtility._instance = null;
	Bitrix.ElementPositioningUtility.getInstance = function() {
		if (this._instance == null) {
			this._instance = new Bitrix.ElementPositioningUtility();
			this._instance.initialize();
		}
		return this._instance;
	}

	Bitrix.ElementPositioningUtility.getElementRect = function(el) {
		return this.getInstance().getElementRect(el);
	}

	Bitrix.ElementPositioningUtility.getElementMargins = function(el) {
		return this.getInstance().getElementMargins(el);
	}

	Bitrix.ElementPositioningUtility.getWindowScrollPos = function(doc) {
		if (self.pageYOffset) return { scrollLeft: self.pageXOffset, scrollTop: self.pageYOffset };
		if (!doc) doc = document;
		var vp = this.getViewPortElement(doc);
		return { scrollLeft: vp.scrollLeft, scrollTop: vp.scrollTop };
	}

	Bitrix.ElementPositioningUtility.getViewPortElement = function(doc) {
		if (!doc) doc = document;
		return document.compatMode === "CSS1Compat" ? document.documentElement : document.body;
	}

	if (typeof (Bitrix.ElementPositioningUtility.registerClass) == "function" && !Type.isClass(Bitrix.ElementPositioningUtility))
		Bitrix.ElementPositioningUtility.registerClass("Bitrix.ElementPositioningUtility");

	Bitrix.SwfUtility = function Bitrix$SwfUtility() {
		if (typeof (Bitrix.SwfUtility.initializeBase) == "function")
			Bitrix.SwfUtility.initializeBase(this);

		this._isInitialized = false;

		this._codeBaseUrl = "http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=";
		this._version = null;
	}

	Bitrix.SwfUtility.prototype = {
		initialize: function() {
			this._isInitialized = true;
		},

		getVersion: function() {
			if (this._version != null)
				return this._version;

			if (/*@cc_on!@*/false) {
				for (var i = 15; i > 1; i--) {
					try {
						if (!(new ActiveXObject("ShockwaveFlash.ShockwaveFlash." + i)))
							continue;
						return (this._version = i);
					}
					catch (e) { }
				}
			}
			else if (navigator.plugins) {
				for (var i = 0, l = navigator.plugins.length; i < l; i++)
					if (navigator.plugins[i].name.indexOf('Flash') != -1) {
					var r = parseInt(navigator.plugins[i].description.substr(16, 2));
					return (this._version = !isNaN(r) ? r : -1);
				}
			}
			else
				this._version = -1;

			return this._version;
		},

		_createNewParameter: function(name, value) {
			if (/*@cc_on!@*/false) //IE
				return '<PARAM name="' + name + '" value="' + value + '" />'
			var r = document.createElement('PARAM');
			r.setAttribute("name", name);
			r.setAttribute("value", value.toString());
			return r;
		},

		createElement: function(parentId, url, width, height, wmode, ver, altImgUrl) {
			var parent = document.getElementById(parentId);
			if (!parent)
				return;

			var container = document.createElement('DIV');
			container.style.height = height + "px";
			container.style.width = width + "px";
			container.style.border.width = "0px";
			container.style.border.style = "none";

			var curVer = this.getVersion();
			if (curVer < 1 || (ver && curVer < parseInt(ver))) {
				if (Bitrix.TypeUtility.isNotEmptyString(altImgUrl)) {
					var altImage = document.createElement('IMG');
					altImage.src = altImgUrl;
					altImage.style.width = width + "px";
					altImage.style.height = height + "px";
					container.appendChild(altImage);
					parent.appendChild(container);
				}
				else
					parent.style.display = "none";
				return;
			}

			if (!wmode)
				wmode = "transparent";

			if (/*@cc_on!@*/false) {
				var objectStr = '<OBJECT classid="clsid:D27CDB6E-AE6D-11cf-96B8-444553540000"  codebase="' + this._codeBaseUrl + '"';
				objectStr = objectStr.concat(' width="', width, '" height="', height, '"><PARAM name="movie" value="', url, '" />');

				objectStr = objectStr.concat(this._createNewParameter("quality", "high"));
				objectStr = objectStr.concat(this._createNewParameter("bgcolor", "#ffffff"));
				if (wmode)
					objectStr = objectStr.concat(this._createNewParameter("wmode", wmode));

				objectStr = objectStr.concat('</OBJECT>');
				container.innerHTML = objectStr;
			}
			else {
				var objectElement = document.createElement('OBJECT');
				objectElement.setAttribute("type", "application/x-shockwave-flash");
				objectElement.setAttribute("data", url);
				//objectElement.setAttribute("codebase", this._codeBaseUrl);	
				objectElement.setAttribute("width", width);
				objectElement.setAttribute("height", height);

				objectElement.appendChild(this._createNewParameter("movie", url));
				objectElement.appendChild(this._createNewParameter("quality", "high"));
				objectElement.appendChild(this._createNewParameter("bgcolor", "#ffffff"));

				if (wmode)
					objectElement.appendChild(this._createNewParameter("wmode", wmode));

				if (Bitrix.TypeUtility.isNotEmptyString(altImgUrl)) {
					var altImage = document.createElement('IMG');
					altImage.src = altImgUrl;
					altImage.style.width = width + "px";
					altImage.style.height = height + "px";
					objectElement.appendChild(altImage);
				}
				container.appendChild(objectElement);
			}
			parent.appendChild(container);
		}
	}

	Bitrix.SwfUtility._instance = null;
	Bitrix.SwfUtility.getInstance = function() {
		if (this._instance == null) {
			this._instance = new Bitrix.SwfUtility();
			this._instance.initialize();
		}
		return this._instance;
	}

	if (typeof (Bitrix.SwfUtility.registerClass) == "function" && !Type.isClass(Bitrix.SwfUtility))
		Bitrix.SwfUtility.registerClass("Bitrix.SwfUtility");

	Bitrix.SilverlightUtility = function Bitrix$SilverlightUtility() {
		if (typeof (Bitrix.SilverlightUtility.initializeBase) == "function")
			Bitrix.SilverlightUtility.initializeBase(this);

		this._isInitialized = false;
		this._version = null;
	}

	Bitrix.SilverlightUtility.prototype = {
		initialize: function() {
			this._isInitialized = true;
		},

		getVersion: function() {
			if (this._version != null)
				return this._version;

			//if (/*@cc_on!@*/false) {
			//    for (var i = 15; i > 1; i--) {
			//        try {
			//            if (!(new ActiveXObject("ShockwaveFlash.ShockwaveFlash." + i)))
			//                continue;
			//            return (this._version = i);
			//        }
			//        catch (e) { }
			//    }
			//}
			//else if (navigator.plugins) {
			//    for (var i = 0, l = navigator.plugins.length; i < l; i++)
			//        if (navigator.plugins[i].name.indexOf('Flash') != -1) {
			//        var r = parseInt(navigator.plugins[i].description.substr(16, 2));
			//        return (this._version = !isNaN(r) ? r : -1);
			//    }
			//}
			//else
			this._version = -1;

			return this._version;
		},

		_createNewParameter: function(name, value) {
			if (/*@cc_on!@*/false) //IE
				return '<PARAM name="' + name + '" value="' + value + '" />'
			var r = document.createElement('PARAM');
			r.setAttribute("name", name);
			r.setAttribute("value", value.toString());
			return r;
		},

		isSilverlightIstalled: function() {
			return typeof (Silverlight) != "undefined" && Silverlight.available;
		},
		// creating silverlight app through javascript for determining it's size and error handling in admin page
		createElement: function(parentId, url, width, height, hfWidth, hfHeight, widthCaptionId,
                                heightCaptionId, version, background, errorMsgId) {
			if (!this.isSilverlightIstalled()) return;
			var parent = document.getElementById(parentId);
			parent.style.width = "";
			parent.style.height = "";
			if (!parent)
				return;

			var w, h;
			if (height != null && height > 0) h = height
			else h = 1;
			if (width != null && width > 0) w = width
			else w = 1;
			var prop = {};
			prop.width = w;
			prop.height = h;
			if (version != null && version != "" && typeof (version) != "undefined") prop.minRuntimeVersion = version;
			prop.background = "transparent";

			Silverlight.createObjectEx({
				source: url,
				parentElement: parent,
				id: parentId + "_SilverlightControl",
				properties: prop,
				events: {
					"onResize": function(sender, args) {
						var sl = document.getElementById(parent.id + "_SilverlightControl");

						if (parent.style.width == "" && sender.content.actualWidth != 0) {
							parent.style.width = sender.content.actualWidth + "px";

							if (sl) sl.setAttribute("width", sender.content.actualWidth);
							var w = document.getElementById(hfWidth);
							if (w) w.value = sender.content.actualWidth;
							w = document.getElementById(widthCaptionId);
							if (w) {
								w.textContent = sender.content.actualWidth;
								w.innerText = sender.content.actualWidth;
							}
						}

						if (parent.style.height == "" && sender.content.actualHeight != 0) {
							parent.style.height = sender.content.actualHeight + "px";

							if (sl) sl.setAttribute("height", sender.content.actualHeight);
							var h = document.getElementById(hfHeight);
							if (h)
								h.value = sender.content.actualHeight;

							h = document.getElementById(heightCaptionId);
							if (h) {
								h.innerText = sender.content.actualHeight;
								h.textContent = sender.content.actualHeight;
							}
						}
					},
					"onError": function(sender, errorArgs) {

						var sl = document.getElementById(parent.id + "_SilverlightControl");
						if (sl) {
							sl.setAttribute("height", "0px");
							sl.setAttribute("width", "0px");
						}
						var h = document.getElementById(hfHeight);
						if (h)
							h.value = "0";

						h = document.getElementById(heightCaptionId);
						if (h) {
							h.innerText = "0";
							h.textContent = "0";
						}
						var w = document.getElementById(hfWidth);
						if (w) w.value = "0";
						w = document.getElementById(widthCaptionId);
						if (w) {
							w.textContent = "0";
							w.innerText = "0";
						}

						// Create the error message to display.
						var errorMsg = Bitrix.AdminSilverlightError.ErrorTitle + "\n";

						// Specify error information common to all errors.
						errorMsg += Bitrix.AdminSilverlightError.ErrorType + ":     " + errorArgs.errorType + "\n";
						errorMsg += Bitrix.AdminSilverlightError.ErrorMessage + ": " + errorArgs.errorMessage + "\n";
						errorMsg += Bitrix.AdminSilverlightError.ErrorCode + ":    " + errorArgs.errorCode + "\n";
						errorMsg += Bitrix.AdminSilverlightError.ChooseAnotherFile;
						var er = document.getElementById(errorMsgId);

						if (er) {
							er.innerText = errorMsg;
							er.textContent = errorMsg;
							er.style.visibilty = "visible";
							er.style.display = "block";
						}


					}
				}
			});

		}

	}


	Bitrix.SilverlightUtility._instance = null;
	Bitrix.SilverlightUtility.getInstance = function() {
		if (this._instance == null) {
			this._instance = new Bitrix.SilverlightUtility();
			this._instance.initialize();
		}
		return this._instance;
	}

	if (typeof (Bitrix.SilverlightUtility.registerClass) == "function" && !Type.isClass(Bitrix.SilverlightUtility))
		Bitrix.SilverlightUtility.registerClass("Bitrix.SilverlightUtility");

	Bitrix.SelectionUtility =
{
	getSelection: function(inputElement) {
		if (inputElement.createTextRange) {
			var result = {};

			var r = document.selection.createRange().duplicate()
			r.moveEnd('character', inputElement.value.length)
			result.start = (r.text == '') ? inputElement.value.length : inputElement.value.lastIndexOf(r.text);

			var r = document.selection.createRange().duplicate()
			r.moveStart('character', -inputElement.value.length)
			result.end = r.text.length;

			return result;
		}
		else if (typeof inputElement.selectionStart != 'undefined') {
			return {
				start: inputElement.selectionStart,
				end: inputElement.selectionEnd
			}
		}
	},
	setSelection: function(inputElement, start, end) {
		if (typeof end != 'number' || end < start)
			end = start;

		if (inputElement.createTextRange) {
			var range = inputElement.createTextRange();
			range.moveStart("character", start);
			range.moveEnd("character", end - inputElement.value.length);
			range.select();
		}
		else if (inputElement.setSelectionRange) {
			inputElement.setSelectionRange(start, end);
		}
	}
}
}

Bitrix.SecurityHelper = {
	getTokenPair: function() {
		return dotNetVars.securityTokenPair;
	},
	getTokenName: function() {
		var s = dotNetVars.securityTokenPair;
		return s.substring(0, s.indexOf('='));
	},
	getToken: function() {
		var s = dotNetVars.securityTokenPair;
		return s.substring(s.indexOf('=') + 1);
	}
}


if (typeof(Sys) !== "undefined") Sys.Application.notifyScriptLoaded();


