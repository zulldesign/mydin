if(typeof(Bitrix) == "undefined")
	Bitrix = {};
	
if(typeof(Bitrix.Search) == "undefined")
	Bitrix.Search = {};

if(typeof(Bitrix.Search.CompletionController) == "undefined")
{
	Bitrix.Search.CompletionController = function Bitrix$UI$CompletionController(textBox, options)
	{
		this._options = 
		{
			interval: 500,
			separator: ','
		} 
		if (typeof(options) == 'object')
		{
			for(var p in options)
				this._options[p] = options[p];
		}
		this._options.textBox = textBox;
		this._currentTimeout = null;
		
		var po = { };
		if (typeof(this._options.popup) == 'object')
		{
			for (var p in options.popup)
				po[p] = options.popup[p];
		}
		po.onItemClick = Bitrix.TypeUtility.createDelegate(this, this._ItemClick);
		this._options.popup = po;
		this._popup = new Bitrix.UI.CompletionPopup(po);
		
		this._queries = {};
		this._ondata = [];
				
		(function (sender)
		{
			Bitrix.EventUtility.addEventListener(sender._options.textBox, "keydown", function(e) 
			{ 
				var keys = [38, 40, 13];
				for(var i = 0; i < keys.length; i++)
					if (keys[i] == e.keyCode)
						return;
						
				sender._Check(this, e); 
			});
			Bitrix.EventUtility.addEventListener(sender._options.textBox, "click", function(e) 
			{ 
                sender._Check(this, e); 
				sender._textBoxClicked = true;
			});
			Bitrix.EventUtility.addEventListener(window.document, "click", function(e) 
			{
                if (sender._textBoxClicked)
				{
					sender._textBoxClicked = false;
					return;
				}
				sender._HidePopup();
            });
            
			Bitrix.EventUtility.addEventListener(sender._options.textBox, "blur", function(e) 
			{ 
                window.setTimeout(function() { // firefox calls blur before all events
                    sender._HidePopup();
                }, 200);
			});            
		})
		(this);
		this._options.textBox.setAttribute('autocomplete', 'off');
	}

	Bitrix.Search.CompletionController.prototype =
	{
		_ItemClick: function(item, data)
		{
			var t = this._options.textBox;
			var val = t.value;
			var sel = this._lastTextBoxSelection || Bitrix.SelectionUtility.getSelection(t);

			var i1 = 0;
			if (sel.start > 0)
			{
				i1 = val.lastIndexOf(',', sel.start - 1);
				i1 = (i1 != -1) ? i1 + 1 : 0;
			}

			var i2 = val.length;
			if (sel.end < val.length)
			{
				i2 = val.indexOf(',', sel.end);
				i2 = (i2 != -1) ? i2 : val.length;
			}

			var insert = (i1 != 0 ? ' ' : '') + data.text;
			t.value = val.slice(0, i1) + insert + val.slice(i2);

			t.focus();
			Bitrix.SelectionUtility.setSelection(t, i1 + insert.length);

			this._lastTextBoxSelection = null;
			this._popup.DetachFocusHolder(this._options.textBox);
			this._popup.Hide();
		},
		_Check: function(sender, e)
		{
			var keys = [38, 40, 13];
			for (var i = 0; i < keys.length; i++)
				if (keys[i] == e.keyCode)
				return;

			this._HidePopup();

			this._RequestProcessing();
		},
		_HidePopup: function(delay)
		{
			if (this._currentTimeout != null)
			{
				window.clearTimeout(this._currentTimeout);
				this._currentTimeout = null;
			}
			if (this._popup.IsVisible())
			{
				this._lastTextBoxSelection = null;
				this._popup.DetachFocusHolder(this._options.textBox);
				this._popup.Hide(delay);
			}
		},
		_RequestProcessing: function()
		{
			if (this._currentTimeout != null)
			{
				window.clearTimeout(this._currentTimeout);
				this._currentTimeout = null;
			}
			(function(sender)
			{
				sender._currentTimeout = window.setTimeout(function()
				{
					sender._currentTimeout = null;
					sender._Process();
				}, sender._options.interval);
			})
			(this);
		},
		_Process: function()
		{
			var t = this._options.textBox;
			var sel = Bitrix.SelectionUtility.getSelection(t);
			var val = t.value;

			var i1 = 0;
			if (sel.start > 0)
			{
				i1 = val.lastIndexOf(',', sel.start - 1);
				i1 = (i1 != -1) ? i1 + 1 : 0;
			}

			var i2 = val.length;
			if (sel.end < val.length)
			{
				i2 = val.indexOf(',', sel.end);
				i2 = (i2 != -1) ? i2 : val.length;
			}

			var tag = jsUtils.trim(val.slice(i1, i2)).toLowerCase();

			(function(sender)
			{
				sender._RequestData(tag, function(data)
				{
					if (data.length > 0)
						sender._Offer(data);
				});
			})
			(this);

		},
		_RequestData: function(tag, callback)
		{
		    var query = '';
		    if (tag.length == 1)
		        query = tag;
		    else if (tag.length >= 2)
		        query = tag.slice(0, 2);
			
			if (this._queries[query] && this._queries[query].data)
			{
			    this._FilterData(tag, this._queries[query].data, callback);
				return;
			}

			if (!this._queries[query])
				this._queries[query] = {};
			this._queries[query].callback = callback;

			var http = null;
			if (window.XMLHttpRequest)
				try { http = new XMLHttpRequest(); } catch (e) { }
			else if (window.ActiveXObject)
			{
				try { http = new ActiveXObject("Microsoft.XMLHTTP"); } catch (e) { }
				if (!http)
					try { http = new ActiveXObject("Msxml2.XMLHTTP"); } catch (e) { }
			}

			http.onreadystatechange = (function(query, sender)
			{
				return function()
				{
					if (http.readyState != 4 || http.status != 200)
						return;


					var data = window.eval(http.responseText);
					if (!data)
					{
						sender._queries[query].callback = null;
						return;
					}

					sender._queries[query].data = [];
					var len = data.length;
					for (var i = 0; i < len; i++)
						sender._queries[query].data.push({ text: data[i].name, tag: data[i].name.toLowerCase() });

					var callback = sender._queries[query].callback;
					if (callback != null)
					    sender._FilterData(tag, sender._queries[query].data, callback);
					sender._queries[query].callback = null;
				}
			})(query, this);
			var url = jsUtils.Path.ToAbsolute('~/bitrix/handlers/Search/GetTags.ashx') + '?query=' + encodeURIComponent(query);
			if (this._options.siteId)
				url += '&site=' + encodeURIComponent(this._options.siteId);
			if (this._options.filterId)
				url += '&filter=' + encodeURIComponent(this._options.filterId);
			if (this._options.moduleId)
				url += '&m=' + encodeURIComponent(this._options.moduleId);
			if (this._options.itemGroup)
				url += '&g=' + encodeURIComponent(this._options.itemGroup);
			if (this._options.itemId)
				url += '&i=' + encodeURIComponent(this._options.itemId);
			http.open('GET', url, true);
			http.send('');
		},
		_FilterData: function(tag, data, callback) 
		{
		    if (callback)
				callback(data);
		},
		_Offer: function(items)
		{
			this._popup.Clear();
			for (var i = 0; i < items.length; i++)
				this._popup.AddText(items[i].text, items[i]);

			this._lastTextBoxSelection = Bitrix.SelectionUtility.getSelection(this._options.textBox);
			this._popup.AttachFocusHolder(this._options.textBox);
			this._popup.Show(this._options.textBox, this._options.popup.width ? null : { width: this._options.textBox.offsetWidth + 'px' });
		}
	};
}

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();