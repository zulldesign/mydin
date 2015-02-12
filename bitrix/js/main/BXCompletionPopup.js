if(typeof(Bitrix) == "undefined")
	Bitrix = new Object();

if(typeof(Bitrix.UI) == "undefined")
	Bitrix.UI = new Object();

if(typeof(Bitrix.UI.CompletionPopup) == "undefined")
{
    Bitrix.UI.CompletionPopup = function Bitrix$UI$CompletionPopup(options) {
        this._items = [];
        this._options = { width: '100px', height: '100px', onItemChangeState: this.ItemChangeStateDefault };

        if (typeof (options) == 'object') {
            for (var p in options)
                this._options[p] = options[p];
        }
        this._popup = this._itemsContainer = document.createElement('DIV');
        this._visible = false;

        if (this._options.popupStyle) {
            for (var p in this._options.popupStyle)
                this._popup.style[p] = this._options.popupStyle[p];
        }
        else {
            this._popup.style.display = 'none';
            this._popup.style.position = 'absolute';
            this._popup.style.backgroundColor = 'white';
            this._popup.style.border = '1px solid Gray';
        }

        this._itemsContainer.style.width = this._options.width;
        this._itemsContainer.style.height = this._options.height;
        this._itemsContainer.style.overflow = 'auto';
        this._itemsContainer.style.overflowX = 'hidden';
        this._itemsContainer.style.overflowY = 'auto';


        document.body.appendChild(this._popup);

        this._curHighlighted = null;
        this._curSelected = null;
        this._isAttachedKeyPress = false;
    };

    Bitrix.UI.CompletionPopup.prototype = {
    	Clear: function() {
    		this._curHighlighted = null;
    		this._curSelected = null;

    		this._items.length = 0;

    		while (this._itemsContainer.childNodes.length > 0)
    			this._itemsContainer.removeChild(this._itemsContainer.childNodes[0]);
    	},
    	Add: function(html, userData) {
    		this._AddNewItem(html, userData);
    	},
    	AddText: function(text, userData) {
    		if (!text || text == '')
    			text = '\u00A0';
    		this._AddNewItem(document.createTextNode(text), userData);
    	},
    	Show: function(control, options) {
    		if (options) {
    			if (options.reset) {
    				this._HighlightItem(null);
    				this._SelectItem(null);
    			}
    			if (options.width)
    				this._popup.style.width = options.width;
    		}
    		this._popup.style.display = '';
    		this._visible = true;

    		var w = this._popup.offsetWidth;
    		var h = this._popup.offsetHeight;
    		var pos = jsUtils.AlignToPos(jsUtils.GetRealPos(control), w, h);

    		jsFloatDiv.Show(this._popup, pos.left, pos.top, 3);
    	},
    	Hide: function(delay) {
    		if (typeof delay == 'number') {
    			(function(sender) {
    				window.setTimeout(function() { sender.Hide(false); }, delay);
    			})
				(this);
    			return;
    		}

    		if (!this._visible)
    			return;
    		this._visible = false;
    		this._popup.style.display = 'none';

    		jsFloatDiv.Close(this._popup);
    	},
    	ItemChangeStateDefault: function(item, userData, state, oldState) {
    		switch (state) {
    			case 'selected':
    				item.style.backgroundColor = '#6F6F6F';
    				item.style.color = 'White';
    				break;
    			case 'highlighted':
    				item.style.backgroundColor = '#D7D4CC';
    				item.style.color = 'Black';
    				break;
    			case 'normal':
    				if (oldState == '') {
    					item.style.textAlign = 'left';
    					item.style.padding = '0px 0px 0px 5px';
    					item.style.whiteSpace = 'nowrap';
    					item.style.fontFamily = 'Verdana,Arial,Helvetica,sans-serif';
    					item.style.fontSize = '12px';
    					item.style.fontStyle = 'normal';
    					item.style.fontVariant = 'normal';
    					item.style.fontWeight = 'normal';
    					item.style.letterSpacing = 'normal';
    					item.style.wordSpacing = 'normal';
    					item.style.lineHeight = 'normal';
    					item.style.textIndent = '0px';

    				}
    				item.style.backgroundColor = '';
    				item.style.color = '';
    				break;
    		}
    	},

    	MoveDown: function() {
    		if (this._items.length == 0)
    			return;
    		if (this._curSelected == null)
    			this._SelectItem(this._items[0], true);
    		else {
    			var len = this._items.length;
    			for (var i = 0; i < len; i++)
    				if (this._items[i] == this._curSelected) {
    				if (i + 1 < len)
    					this._SelectItem(this._items[i + 1], true);
    				break;
    			}
    		}
    	},
    	MoveUp: function() {
    		if (this._items.length == 0)
    			return;
    		if (this._curSelected == null)
    			this._SelectItem(this._items[this._items.length - 1], true);
    		else {
    			var len = this._items.length;
    			for (var i = 0; i < len; i++)
    				if (this._items[i] == this._curSelected) {
    				if (i - 1 >= 0)
    					this._SelectItem(this._items[i - 1], true);
    				break;
    			}
    		}
    	},
    	AttachFocusHolder: function(control) {
    		Bitrix.EventUtility.addEventListener(control, Bitrix.NavigationHelper.isOpera() ? 'keypress' : 'keydown', this._GetFocusKeyHandler());
    		if (Bitrix.NavigationHelper.isFirefox() && !this._isAttachedKeyPress) {
    			Bitrix.EventUtility.addEventListener(control, 'keypress', this._GetFocusKeyHandlerFirefox());
    			this._isAttachedKeyPress = true;
    		}
    	},
    	DetachFocusHolder: function(control) {
    		Bitrix.EventUtility.removeEventListener(control, Bitrix.NavigationHelper.isOpera() ? 'keypress' : 'keydown', this._GetFocusKeyHandler());
    	},

    	IsVisible: function() {
    		return this._visible;
    	},
    	IndexOf: function(predicate) {
    		var len = this._items.length;
    		for (var i = 0; i < len; i++)
    			if (predicate(this._items[i].internalData.userData))
    			return i;
    		return -1;
    	},
    	_GetFocusKeyHandler: function() {
    		if (this._focusKeyHandler)
    			return this._focusKeyHandler;
    		this._focusKeyHandler = function(sender) {
    			return function(e) {
    				switch (e.keyCode) {
    					case 38: //up
    						sender.MoveUp();
    						return jsUtils.PreventDefault(e);
    					case 40: //down
    						sender.MoveDown();
    						return jsUtils.PreventDefault(e);
    					case 13: //return
    						if (sender._curSelected && typeof sender._options.onItemClick == 'function') {
    							sender._options.onItemClick.call(
									sender,
									sender._curSelected.internalData.userItem,
									sender._curSelected.userData
								);
    							return jsUtils.PreventDefault(e);
    						}
    						break;
    				}
    				return true;
    			}
    		} (this);
    		return this._focusKeyHandler;
    	},
    	_GetFocusKeyHandlerFirefox: function() {
    		if (this._focusKeyHandlerFirefox)
    			return this._focusKeyHandlerFirefox;
    		this._focusKeyHandlerFirefox = function(sender) {
    			return function(e) {
    				if (e.keyCode == 13) return jsUtils.PreventDefault(e);
    			}
    		} (this);
    		return this._focusKeyHandlerFirefox;
    	},
    	_AddNewItem: function(innerControl, userData) {
    		var item = document.createElement('DIV');
    		this._items.push(item);
    		this._itemsContainer.appendChild(item);

    		item.internalData = {
    			selected: false,
    			highlighted: false,
    			userItem: document.createElement('DIV'),
    			state: ''
    		};
    		item.userData = userData;
    		item.appendChild(item.internalData.userItem);
    		if (typeof innerControl == 'string')
    			item.internalData.userItem.innerHTML = (innerControl != '') ? innerControl : '&nbsp'
    		else if (typeof innerControl == 'object')
    			item.internalData.userItem.appendChild(innerControl);
    		item.style.cursor = 'default';
    		Bitrix.TypeUtility.disableSelection(item);

    		(function(sender) {
    			Bitrix.EventUtility.addEventListener(item, 'mouseover', function(e) {
    				sender._ItemMouseOver(this);
    			});
    			Bitrix.EventUtility.addEventListener(item, 'click', function(e) {
    				sender._ItemClick(this);
    				return jsUtils.PreventDefault(e);
    			});
    		})(this);

    		if (typeof (this._options.onItemChangeState) == 'function')
    			this._options.onItemChangeState.call(this, item.internalData.userItem, item.userData, 'normal', '');
    		item.internalData.state = 'normal';
    	},
    	_ItemMouseOver: function(item) {
    		this._HighlightItem(item);
    	},
    	_ItemClick: function(item) {
    		this._SelectItem(item);
    		if (typeof this._options.onItemClick == 'function')
    			this._options.onItemClick.call(this, item.internalData.userItem, item.userData);
    	},
    	_SetItemState: function(item, state) {
    		if (typeof (this._options.onItemChangeState) == 'function')
    			this._options.onItemChangeState(item.internalData.userItem, item.userData, state, item.internalData.state);
    		item.internalData.state = state;
    	},
    	_SelectItem: function(item, scroll) {
    		if (this._curSelected == item)
    			return;
    		if (this._curSelected != null) {
    			this._curSelected.internalData.selected = false;
    			if (!this._curSelected.internalData.highlighted)
    				this._SetItemState(this._curSelected, 'normal');
    		}

    		this._curSelected = item;

    		if (item == null)
    			return;

    		item.internalData.selected = true;
    		if (!item.internalData.highlighted)
    			this._SetItemState(item, 'selected');

    		if (scroll === true) {
    			var top = this._itemsContainer.scrollTop;
    			var bottom = top + this._itemsContainer.clientHeight;
    			var itemTop = item.offsetTop;
    			var itemBottom = itemTop + item.offsetHeight;

    			if (itemTop >= top && itemBottom <= bottom)
    				return;

    			var pos = itemBottom > bottom ? itemBottom - (bottom - top) : itemTop;

    			this._itemsContainer.scrollTop = pos;
    		}
    	},
    	_HighlightItem: function(item) {
    		if (this._curHighlighted == item)
    			return;
    		if (this._curHighlighted != null) {
    			this._curHighlighted.internalData.highlighted = false;
    			this._SetItemState(this._curHighlighted, this._curHighlighted.internalData.selected ? 'selected' : 'normal');
    		}

    		this._curHighlighted = item;

    		if (item == null)
    			return;

    		item.internalData.highlighted = true;
    		this._SetItemState(item, 'highlighted');
    	}
    };
}

//controller class for simple autocomplete

if (typeof (Bitrix.Main) == "undefined")
    Bitrix.Main = {};

if (typeof (Bitrix.Main.SimpleCompletionController) == "undefined") {
	Bitrix.Main.SimpleCompletionController = function Bitrix$UI$SimpleCompletionController(textBox, idContainer, options, customShowData, initialShowData) {
		this._options =
		{
			interval: 500,
			separator: ',',
			minChars: 1,
			ignoredIds: { 0: true }
		}
		this.customShowData = customShowData;
		if (typeof (options) == 'object') {
			for (var p in options)
				this._options[p] = options[p];
		}

		this._options.textBox = textBox;
		this._options.idContainer = idContainer;
		this._currentTimeout = null;

		var po = {};
		if (typeof (this._options.popup) == 'object') {
			for (var p in options.popup)
				po[p] = options.popup[p];
		}
		po.onItemClick = Bitrix.TypeUtility.createDelegate(this, this._ItemClick);

		this._options.popup = po;
		if (po.SetFieldFontSizeToPopup) {

			if (fSize != '' && fSize != null) {
				if (!po["popupStyle"])
					po["popupStyle"] = {};
				po["popupStyle"].fontSize = fSize;
				po["popupStyle"].display = 'none';
				po["popupStyle"].position = 'absolute';
				po["popupStyle"].backgroundColor = 'white';
				po["popupStyle"].border = '1px solid Gray';

			}
		}

		this._popup = new Bitrix.UI.CompletionPopup(po);
		this._queries = {};
		this._ondata = [];
		this._prevValue = "";



		(function(sender) {
			if(sender._options.textBox) {
				Bitrix.EventUtility.addEventListener(sender._options.textBox, "keydown", function(e) {
					var keys = [38, 40, 13, 37, 39, 9, 35, 27, 17, 20, 18, 45, 36, 33,
					34, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 145, 19];
					for (var i = 0; i < keys.length; i++)
						if (keys[i] == e.keyCode)
						return;
					if (sender._options.idContainer)
						sender._options.idContainer.value = "";
					sender._Check(this, e);
				});
				Bitrix.EventUtility.addEventListener(sender._options.textBox, "click", function(e) {
					// sender._Check(this, e);
					sender._textBoxClicked = true;
				});				
			}

			Bitrix.EventUtility.addEventListener(window.document, "click", function(e) {
				if (sender._textBoxClicked) {
					sender._textBoxClicked = false;
					return;
				}
				sender._HidePopup();
			});
		})
		(this);
		if(this._options.textBox)
			this._options.textBox.setAttribute('autocomplete', 'off');

		if (typeof (initialShowData) == "function") 
		{
			this.initialShowData = initialShowData;
			this.initialShowData();
			delete this.initialShowData;
		}
		else if (typeof (initialShowData) == "object" && typeof (this.customShowData) == "function"  && initialShowData.constructor == Array)
		{
			this.initialShowData = function()
			{
				for (var i = 0; i < initialShowData.length; i++)
					this.customShowData(initialShowData[i]);
			};
			this.initialShowData();
			delete this.initialShowData;
		}
	}

    Bitrix.Main.SimpleCompletionController.prototype =
	{
	    _ItemClick: function(item, data) {
	        if (data.id) {
	            if (data.id in this._options.ignoredIds) return;
	        }
	        var t = this._options.textBox;
	        var idc = this._options.idContainer;
	        if (typeof (this.customShowData) == "function") {
	            this.customShowData(data);
	        }
	        else {
	            t.value = data.text;
	            idc.value = data.id;
	        }

	        t.focus();
	        Bitrix.SelectionUtility.setSelection(t);

	        this._lastTextBoxSelection = null;
	        this._popup.DetachFocusHolder(this._options.textBox);
	        this._popup.Hide();
	    },
	    _Check: function(sender, e) {
	        var keys = [38, 40, 13];
	        for (var i = 0; i < keys.length; i++)
	            if (keys[i] == e.keyCode)
	            return;

	        this._HidePopup();

	        this._RequestProcessing();
	    },
	    _HidePopup: function(delay) {
	        if (this._currentTimeout != null) {
	            window.clearTimeout(this._currentTimeout);
	            this._currentTimeout = null;
	        }
	        if (this._popup.IsVisible()) {
	            this._lastTextBoxSelection = null;
	            this._popup.DetachFocusHolder(this._options.textBox);
	            this._popup.Hide(delay);
	        }
	    },
	    _RequestProcessing: function() {
	        if (this._currentTimeout != null) {
	            window.clearTimeout(this._currentTimeout);
	            this._currentTimeout = null;
	        }
	        (function(sender) {
	            sender._currentTimeout = window.setTimeout(function() {
	                sender._currentTimeout = null;
	                sender._Process();
	            }, sender._options.interval);
	        })
			(this);
	    },
	    _Process: function() {
	        var t = this._options.textBox;
	        var sel = Bitrix.SelectionUtility.getSelection(t);

	        var queryVar = jsUtils.trim(t.value).toLowerCase();

	        if (queryVar.length < this._options.minChars) return;

	        (function(sender) {
	            sender._RequestData(queryVar, function(data) {
	                if (data.length > 0)
	                    sender._Offer(data);

	            });
	        })
			(this);

	    },
	    _RequestData: function(query, callback) {

	        if (this._queries[query] && this._queries[query].data) {
	            callback(this._queries[query].data);
	            return;
	        }

	        if (!this._queries[query])
	            this._queries[query] = {};
	        this._queries[query].callback = callback;

	        var http = null;
	        if (window.XMLHttpRequest)
	            try { http = new XMLHttpRequest(); } catch (e) { }
	        else if (window.ActiveXObject) {
	            try { http = new ActiveXObject("Microsoft.XMLHTTP"); } catch (e) { }
	            if (!http)
	                try { http = new ActiveXObject("Msxml2.XMLHTTP"); } catch (e) { }
	        }

	        http.onreadystatechange = (function(query, sender) {
	            return function() {

	                if (http.readyState != 4 || http.status != 200)
	                    return;

	                var data = null;
	                try {
	                    var data = window.eval(http.responseText);
	                }
	                catch (e) {

	                }
	                if (!data) {
	                    sender._queries[query].callback = null;
	                    return;
	                }

	                sender._queries[query].data = [];
	                var len = data.length;

	                for (var i = 0; i < len; i++)
	                {
						var o = { text: data[i].name, id: data[i].id };
						if (data[i].userData)
							o.userData = data[i].userData;
	                    sender._queries[query].data.push(o);
	                }

	                sender._queries[query].callback(sender._queries[query].data);
	                if (len == 1 && data[0].id in sender._options.ignoredIds) sender._queries[query].data = null;
	                sender._queries[query].callback = null;
	            }
	        })(query, this);

	        var sign;
	        if (this._options.url.indexOf("?") != -1) sign = "&"
	        else sign = "?";
	        var url = jsUtils.Path.ToAbsolute(
	            this._options.url) + sign +
	        'query=' + encodeURIComponent(query) +
	            "&" + this._options.sessionTokenPair;

	        http.open('GET', url, true);
	        http.send('');
	    },
	    _Offer: function(items) {
	        this._popup.Clear();
	        for (var i = 0; i < items.length; i++)
	            this._popup.AddText(items[i].text, items[i]);
	        this._lastTextBoxSelection = Bitrix.SelectionUtility.getSelection(this._options.textBox);
	        this._popup.AttachFocusHolder(this._options.textBox);
	        this._popup.Show(this._options.textBox, this._options.popup.width ? this._options.popup.width : { width: this._options.textBox.offsetWidth + 'px' });
	    }

	};
}
Bitrix.UI.CompletionPopup.BuildChangeState = function(input) {
    return function(item, userData, state, oldState) {
        switch (state) {
            case 'selected':
                item.style.backgroundColor = '#6F6F6F';
                item.style.color = 'White';
                break;
            case 'highlighted':
                item.style.backgroundColor = '#D7D4CC';
                item.style.color = 'Black';
                break;
            case 'normal':
                if (oldState == '') {
                    item.style.textAlign = 'left';
                    item.style.padding = '0px 0px 0px 5px';
                    item.style.whiteSpace = 'nowrap';
                    item.style.fontFamily = 'Verdana,Arial,Helvetica,sans-serif';
                    var fSize = '';
                    if (input.runtimeStyle && input.runtimeStyle.fontSize != '' && input.runtimeStyle.fontSize != null)
                        fSize = input.runtimeStyle.fontSize;
                    else if (window.getComputedStyle)
                        fSize = window.getComputedStyle(input, null).fontSize;
                    else if (input.currentStyle && input.currentStyle.fontSize != '' && input.currentStyle.fontSize != null)
                        fSize = input.currentStyle.fontSize;
                    if (fSize != '' && fSize.indexOf("%")==-1)
                        item.style.fontSize = fSize;
                    else item.style.fontSize = '12px';
                    item.style.fontStyle = 'normal';
                    item.style.fontVariant = 'normal';
                    item.style.fontWeight = 'normal';
                    item.style.letterSpacing = 'normal';
                    item.style.wordSpacing = 'normal';
                    item.style.lineHeight = 'normal';
                    item.style.textIndent = '0px';

                }
                item.style.backgroundColor = '';
                item.style.color = '';
                break;
        }
    }
}

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();