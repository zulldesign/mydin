function BXPopupDialog(dialog_id)
{    
    var _this = this;
    this.DialogId = dialog_id;
    this.dxShadowImg = '';
    this.closeButtons = [];
    this.hideId = '';
    
    this.SetTitle = function(title)
    {
        document.getElementById(this.DialogId + '_title').innerHTML = bxhtmlspecialchars(title);
    }
    
    this.ShowPopupDialog = function(focus, deferred)
	{
	    if (deferred)
	    {
			setTimeout(function(){
				_this.ShowPopupDialog(focus);
			}, 1)
			return;
	    }
	    var div = document.getElementById(_this.DialogId);
		//Чтобы размеры приняли нормальную форму
		if (div)
			div.style.display = "block";
		var left = parseInt(document.body.scrollLeft + document.body.clientWidth/2 - div.offsetWidth/2);
		var top = parseInt(document.body.scrollTop + document.body.clientHeight/2 - div.offsetHeight/2);
		if (left < 20) 
			left = 20;
		if (top < 20)
			top = 20;
		if (div)
		    div.style.visibility = "visible";
		jsFloatDiv.Show(div, left, top, 5, this.dxShadowImg);
        jsUtils.addEvent(document, "keypress", _this.SettingsOnKeyPress);
        jsUtils.addEvent(document, "click", _this.SettingsOnClick);
        if (focus && div)
        {
			var inputs = div.getElementsByTagName("input");
			for (var i = 0; i < inputs.length; i++)
				try
				{
					inputs[i].focus();
					break;
				}
				catch (e)
				{
				}
		}
		
	}
	
	this.ClosePopupDialog =  function(stayShown)
	{
		jsUtils.removeEvent(document, "keypress", _this.SettingsOnKeyPress);
		jsUtils.removeEvent(document, "click", _this.SettingsOnClick);
		var div = document.getElementById(_this.DialogId);
		jsFloatDiv.Close(div);
		if (div)
		{
		    div.style.visibility = "hidden";
		    div.style.display = "none";
		}
		if (!stayShown)
		{
			var hide = document.getElementById(this.hideId);
			if (hide)
				hide.value = '';
		}
	}
		
	this._UpdateShadow = function()
	{
		var obj = document.getElementById(this.DialogId);	    
	    var sh = document.getElementById(this.DialogId+"_shadow");
	    var frame = document.getElementById(this.DialogId+"_frame");	    
	    if (!obj || !sh) return;
	    if (obj.offsetHeight != sh.offsetHeight && sh)
	    {
	        sh.style.height = obj.offsetHeight;
	        if (frame) frame.style.height = obj.offsetHeight;
	    }
	    if (obj.offsetWidth != sh.offsetWidth && sh)
	    {
	        sh.style.width = obj.offsetWidth;
	        if (frame) frame.style.height = obj.offsetWidth;
	    }
	}	
		
	this.AdjustShadow = function()
	{
		jsFloatDiv.AdjustShadow(document.getElementById(_this.DialogId), 5);
	}
		
	this.SettingsOnClick = function(e)
	{
	    if (!e) e = window.event;
	    if (!e) return;
	    if (e.button == 1 && e.target && e.target.id)
	    {
	        var parent = jsUtils.FindParentObjectId(e.target, _this.DialogId);
	        if (!parent) return;
	    
			for (var i in _this.closeButtons)
				if (e.target.id == _this.closeButtons[i])
				{
					_this.ClosePopupDialog();
					break;
				}
	    }
	    _this._UpdateShadow();
	}

	this.SettingsOnKeyPress = function(e)
	{
		if(!e) e = window.event
		if(!e) return;
		if(e.keyCode == 27)
			_this.ClosePopupDialog();
	}
}

var BXPopupDialogUtility = 
{
	KillShadow : function(id)
	{
		var sh = document.getElementById(id + '_shadow');
		if (sh) 
			sh.style.visibility = 'hidden';
		var frame = document.getElementById(id + '_frame');
		if(frame) 
			frame.style.visibility = 'hidden';
	}
}

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded(); 