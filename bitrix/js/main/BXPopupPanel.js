/************************************************/

function BXPopupPanel(confirmMessage)
{
	var _this = this;
	this.commandItems = [];
	this.internalMenu = null;
	this.onclickArgument = null;
	
    this.OnClose = null;
	
	this._BuildItemsList = function(range)
	{
		var items = null;
		
		if (range != null)
		{
			items = [];
			for(var i = 0; i < range.length; i++)
			{
				var o = null;
				if (range[i] < 0)
					o = {'SEPARATOR':true};
				else
					o = _this.commandItems[range[i]];
				items.push(o);
			}
		}
		else 
			items = _this.commandItems;
		return items;
	}
	
	this.PreClick = function(showConfirm)
	{
		_this.internalMenu.PopupHide();
		if (!showConfirm)
			return true;
		return window.confirm(confirmMessage);
	}
	
	this.PreClickWithCustomConfirm = function(text)
	{
		_this.internalMenu.PopupHide();
		return window.confirm(text);	
	}
	
	this.DoClick = function(cmdId, postBack)
	{
		var cmd = cmdId + '$';
		if (_this.onclickArgument)
			cmd += _this.onclickArgument;
		eval(postBack);
		return true;
	}

	 // range = [1,2,3,4,5,..] 
	this.ShowMenuRanged = function(control, range)
	{
		var items = _this._BuildItemsList(range);
		_this.internalMenu.ShowMenu(control, items, false);
	}
		
	// range = [1,2,3,4,5,..] 
	this.ShowMenuRangedAt = function(pos, range)
	{
	    var items = _this._BuildItemsList(range);
		_this.internalMenu.PopupHide();
		if(items)
		{
			_this.internalMenu.SetItems(items);
			_this.internalMenu.BuildItems();
		}
		_this.internalMenu.PopupShow(pos);
	}
}


