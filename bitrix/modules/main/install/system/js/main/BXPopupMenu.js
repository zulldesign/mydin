
function BXPopupMenu(id, shadowPath, cssClass)
{
	var _this = this;
	this.menu_id = id;
	this.controlDiv = null;
	this.zIndex = 100;
	this.dxShadow = 3;	
	this.menuItems = null;
	this.submenus = [];	
	this.bDoHide = false;
	this.parentItem = null;
	this.parentMenu = null;
	this.submenuIndex = null;
	this.bHasSubmenus = false;

	this.OnClose = null;
	
	this.shadowPath = shadowPath;
	this.cssClass = cssClass;
	this.rebuild = true;	

	this.Create = function(zIndex, dxShadow)
	{
		if(!isNaN(dxShadow))
			this.dxShadow = dxShadow;
		if(!isNaN(zIndex))
			this.zIndex = zIndex;

		var div = document.createElement("DIV");
		div.id = this.menu_id;
		div.style.position = 'absolute';
		div.style.zIndex = this.zIndex;
		div.style.left = '-1000px';
		div.style.top = '-1000px';
		div.style.visibility = 'hidden';
		document.body.appendChild(div);
		
		div.innerHTML = 
			'<table cellpadding="0" cellspacing="0" border="0">'+
			'<tr><td class="' + (this.cssClass ? this.cssClass : 'BXPopupMenu') + '">'+
			'<table cellpadding="0" cellspacing="0" border="0" id="'+this.menu_id+'_items">'+
			'<tr><td></td></tr>'+
			'</table>'+
			'</td></tr>'+
			'</table>';
	}
	
	this.ClearItemsStyle = function()
	{
		var tbl = document.getElementById(this.menu_id+'_items');
		for(var i=0; i<tbl.rows.length; i++)
		{
			var div = jsUtils.FindChildObject(tbl.rows[i].cells[0], "div");
			if(div && div.className.indexOf('popupitemover') != -1)
			{
				div.className = div.className.replace(/\s*popupitemover/i, '');
				break;
			}
		}
	}

	this.PopupShow = function(pos)
	{
		var div = document.getElementById(this.menu_id);
		if(!div || this.rebuild)
		{
			this.BuildItems();
			div = document.getElementById(this.menu_id);
		}
		if (!div)
			return;

		this.ClearItemsStyle();
			
		setTimeout(function(){jsUtils.addEvent(document, "click", _this.CheckClick)}, 10);
		jsUtils.addEvent(document, "keypress", _this.OnKeyPress);

		var w = div.offsetWidth;
		var h = div.offsetHeight;
		pos = jsUtils.AlignToPos(pos, w, h);

		div.style.width = w + 'px';
		div.style.visibility = 'visible';

		jsFloatDiv.Show(div, pos["left"], pos["top"], this.dxShadow, this.shadowPath);

	    div.ondrag = jsUtils.False;
	    div.onselectstart = jsUtils.False;
	    div.style.MozUserSelect = 'none';
	}

	this.PopupHide = function()
	{
		for(var i in this.submenus)
			if(this.submenus[i] && this.submenus[i].IsVisible())
				this.submenus[i].PopupHide();

		if(this.parentMenu)
			this.parentMenu.submenuIndex = null;
				
		var div = document.getElementById(this.menu_id);
		if(div)
		{
			jsFloatDiv.Close(div);
			div.style.visibility = 'hidden';
		}

		if(this.OnClose)
			this.OnClose();

		this.controlDiv = null;
		jsUtils.removeEvent(document, "click", _this.CheckClick);
		jsUtils.removeEvent(document, "keypress", _this.OnKeyPress);
	}

	this.CheckClick = function(e)
	{
		for(var i in _this.submenus)
			if(_this.submenus[i] && !_this.submenus[i].CheckClick(e))
				return false;
		
		var div = document.getElementById(_this.menu_id);
		if(!div)
			return true;

		if (div.style.visibility != 'visible')
			return true;

		var x = e.clientX + document.body.scrollLeft;
		var y = e.clientY + document.body.scrollTop;

		/*menu region*/
		var posLeft = parseInt(div.style.left);
		var posTop = parseInt(div.style.top);
		var posRight = posLeft + div.offsetWidth;
		var posBottom = posTop + div.offsetHeight;
		if(x >= posLeft && x <= posRight && y >= posTop && y <= posBottom)
			return false;

		if(_this.controlDiv)
		{
			var pos = jsUtils.GetRealPos(_this.controlDiv);
			if(x >= pos['left'] && x <= pos['right'] && y >= pos['top'] && y <= pos['bottom'])
				return false;
		}
		_this.PopupHide();
		return true;
	}

	this.OnKeyPress = function(e)
	{
		if(!e) e = window.event
		if(!e) return;
		if(e.keyCode == 27)
			_this.PopupHide();
	}
		
	this.GetItemIndex = function(item)
	{
		var item_id = _this.menu_id+'_item_';
		var item_index = parseInt(item.id.substr(item_id.length));
		return item_index;
	}

	this.ShowSubmenu = function(item, bMouseOver)
	{
		if(!item)
			item = this;
		var item_index = _this.GetItemIndex(item);

		if(bMouseOver == true)
		{
			if(!_this.menuItems[item_index]["__time"])
				return;
			var dxTime = (new Date()).valueOf() - _this.menuItems[item_index]["__time"];
			if(dxTime < 500)
				return;
		}

		var menu;
		if(!_this.submenus[item_index])
		{
			menu = new PopupMenu(_this.menu_id+'_sub_'+item_index);
			menu.Create(_this.zIndex+10, _this.dxShadow);
			menu.SetItems(_this.menuItems[item_index].MENU);
			menu.BuildItems();
			menu.parentItem = document.getElementById(_this.menu_id+'_item_'+item_index);
			menu.parentMenu = _this;
			menu.OnClose = function()
			{
				jsUtils.addEvent(document, "keypress", _this.OnKeyPress);
			}
			_this.submenus[item_index] = menu;
		}
		else
			menu = _this.submenus[item_index];

		_this.submenuIndex = item_index;
			
		if(menu.IsVisible())
			return;

		var item_pos = jsUtils.GetRealPos(item);
		var menu_pos = jsUtils.GetRealPos(document.getElementById(_this.menu_id));
		var pos = {'left': menu_pos["right"]-1, 'right': menu_pos["left"]+1, 'top': item_pos["bottom"]+1, 'bottom': item_pos["top"]};

		jsUtils.removeEvent(document, "keypress", _this.OnKeyPress);
		menu.controlDiv = item;
		menu.PopupShow(pos);
	}

	this.OnSubmenuMouseOver = function()
	{
		_this.OnItemMouseOver(this);

		var item_index = _this.GetItemIndex(this);
		if(!_this.menuItems[item_index]["__time"])
			_this.menuItems[item_index]["__time"] = (new Date()).valueOf();
	
		var div = this;
		setTimeout(function(){_this.ShowSubmenu(div, true)}, 550);
	}

	this.OnItemMouseOver = function(item)
	{
		if(_this.bHasSubmenus)
			_this.ClearItemsStyle();
	
		var div = (item? item:this);
		div.className="popupitem popupitemover";

		if(_this.parentItem)
		{
			_this.bDoHide = false;
			if(_this.parentItem.className != "popupitem popupitemover")
			{
				_this.parentMenu.ClearItemsStyle();
				_this.parentItem.className = "popupitem popupitemover";
			}
		}
		
		if(_this.submenuIndex != null)
		{
			var item_index = _this.GetItemIndex(div);
			if(_this.submenuIndex != item_index && _this.submenus[_this.submenuIndex])
			{
				_this.submenus[_this.submenuIndex].bDoHide = true;
				setTimeout(function(){_this.HideSubmenu()}, 500);
			}
		}
	}

	this.OnSubmenuMouseOut = function()
	{
		var item_index = _this.GetItemIndex(this);
		_this.menuItems[item_index]["__time"] = null;
	}

	this.OnItemMouseOut = function()
	{
		this.className="popupitem";
	}

	this.HideSubmenu = function()
	{
		if(_this.submenus[_this.submenuIndex].bDoHide != true)
			return;
		_this.submenus[_this.submenuIndex].PopupHide();
	}

	this.SetItems = function(items)
	{
		this.menuItems = items;
	}

	this.SetItemIcon = function(item_id, icon)
	{
		for(var i in this.menuItems)
		{
			if(this.menuItems[i].ID && this.menuItems[i].ID == item_id)
			{
				this.menuItems[i].ICONCLASS = icon;
				var item_td = document.getElementById(item_id);
				if(item_td)
				{
					var div = jsUtils.FindChildObject(item_td, "div");
					if(div)
						div.className = "icon "+icon;
				}
				break;
			}
		}
	}
	
	this.BuildItems = function()
	{
		var items = this.menuItems;
		if(!items || items.length == 0)
			return;

		var div = document.getElementById(this.menu_id);
		if(!div)
		{
			this.Create();
			div = document.getElementById(this.menu_id);
		}
		div.style.left='-1000px';
		div.style.top='-1000px';
		div.style.width='auto';

		this.bHasSubmenus = false;
		var tbl = document.getElementById(this.menu_id+'_items');
		while(tbl.rows.length>0)
			tbl.deleteRow(0);

		var n = items.length;
		var hasDefault = false;
		for(var i=0; i<n; i++)
		{
			var row = tbl.insertRow(-1);
			var cell = row.insertCell(-1);
			if(items[i]['SEPARATOR'])
			{
				cell.innerHTML = '<div class="popupseparator"><div class="empty"></div></div>';
			}
			else
			{
				var s = 
					'<div id="'+this.menu_id+'_item_'+i+'" class="popupitem"'+(items[i]['DISABLED']!=true && items[i]['ONCLICK']? ' onClick="'+items[i]['ONCLICK']+'"':'')+'>'+
					'	<div style="width:100%;"><table width="100%" cellpadding="0" cellspacing="0" border="0">'+
					'		<tr>'+
					'			<td class="gutter"' + (items[i]['ID'] ? ' id="'+items[i]['ID'] + '"' : '') + '><div class="icon' + (items[i]['ICONCLASS'] ? ' ' + items[i]['ICONCLASS'] : '') + '"' + (items[i]['BACKGROUND_IMAGE'] ? ' style="background-image:url(' + items[i]['BACKGROUND_IMAGE'] + ');"' : '') + '></div></td>' +

					'			<td class="item'+(items[i]['DISABLED'] == true? ' disabled' : '')+(!hasDefault && items[i]['DEFAULT'] == true ? ' default' : '')+'"'+(items[i]["TITLE"]? ' title="'+items[i]["TITLE"]+'"' : '')+'>'+items[i]['TEXT']+'</td>';

				if(items[i]['MENU'])
					s += '<td class="arrow"></td>';

				s +=
					'		</tr>'+
					'	</table></div></div>';
				cell.innerHTML = s;
				if(items[i]['DISABLED']!=true)
				{
					var item_div = jsUtils.FindChildObject(cell, "div");
					if(items[i]['MENU'])
					{
						if(!items[i]['ONCLICK'])
							item_div.onclick = function(){_this.ShowSubmenu(this)};
						item_div.onmouseover = _this.OnSubmenuMouseOver;
						item_div.onmouseout = _this.OnSubmenuMouseOut;
						this.bHasSubmenus = true;
					}
					else
					{
						item_div.onmouseover = function(){_this.OnItemMouseOver(this)};
						item_div.onmouseout = _this.OnItemMouseOut;
						if(items[i]['ONCLICK'] && (items[i]['AUTOHIDE'] == null || items[i]['AUTOHIDE'] == true))
							jsUtils.addEvent(item_div, "click",	function(){_this.PopupHide();});
					}
				}
				items[i]['__id'] = this.menu_id+'_item_'+i;
				if (items[i]['DEFAULT'] == true)
					hasDefault = true;

			}
		}
		
		div.style.width = tbl.parentNode.offsetWidth;
		this.rebuild = false;
	}
	
	this.UpdateIcons = function()
	{
		for(var i in this.menuItems)
		{
			if(this.menuItems[i].GUTTERID)
			{
				var item_td = document.getElementById(this.menu_id + "_gutter_" + gutter_id);
				if(item_td)
				{
					var div = jsUtils.FindChildObject(item_td, "div");
					if(div)
						div.className = "icon " + this.menuItems[i].ICONCLASS;
				}
				break;
			}
		}	
	}
	
	this.GetItemInfo = function(item)
	{
		var td = jsUtils.FindChildObject(item, "td", "item", true);
		if(td)
		{
			var icon = '';
			var icon_div = jsUtils.FindChildObject(jsUtils.FindChildObject(item, "td", "gutter", true), "div");
			//<div class="icon class">
			if(icon_div.className.length > 5)
				icon = icon_div.className.substr(5);
			return {'TEXT': td.innerHTML, 'TITLE':td.title, 'ICON':icon};
		}
		return null;
	}

	this.GetMenuByItemId = function(item_id)
	{
		var i;
		for(i in this.menuItems)
			if(this.menuItems[i]['__id'] && this.menuItems[i]['__id'] == item_id)
				return this;

		var menu;
		for(i in this.submenus)
			if(this.submenus[i] && (menu = this.submenus[i].GetMenuByItemId(item_id)) != false)
				return menu;
	
		return false;
	}
	
	this.IsVisible = function()
	{
		return (document.getElementById(this.menu_id).style.visibility != 'hidden');
	}

	this.ShowMenu = function(control, items, bFixed, dPos)
	{
		if(this.controlDiv == control)
			this.PopupHide();
		else
		{
			this.PopupHide();
			if(items)
			{
				this.SetItems(items);
				this.BuildItems();
			}

			control.className += ' pressed';
			var pos = jsUtils.GetRealPos(control);
			pos["bottom"]+=2;

			if(dPos)
			{
				pos["left"] += dPos["left"];
				pos["right"] += dPos["right"];
				pos["top"] += dPos["top"];
				pos["bottom"] += dPos["bottom"];
			}
						
			if(bFixed == true && !jsUtils.IsIE())
			{
				pos["top"] += document.body.scrollTop;
				pos["bottom"] += document.body.scrollTop;
				pos["left"] += document.body.scrollLeft;
				pos["right"] += document.body.scrollLeft;
			}

			this.controlDiv = control;
			this.OnClose = function()
			{
				control.className = control.className.replace(/\s*pressed/ig, "");
			}
			this.PopupShow(pos);
		}
	}
	
	this.ShowMenuAt = function(pos, items)
	{
		this.PopupHide();
		if(items)
		{
			this.SetItems(items);
			this.BuildItems();
		}
		this.PopupShow(pos);
	}
}

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded(); 