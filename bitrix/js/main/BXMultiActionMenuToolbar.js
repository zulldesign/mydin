function BXContextToolBar(rList, options)
{
	// {'ID':'', 'COMMAND':'', 'CSSOFF':'', 'CSSON':'', 'DISABLEALL':true/false, 'CONFIRM':'', 'CONFIRMALL':''}
	this.CommandList = rList;
	
	this.ForAllCBId = null;
	this.ApplyButtonId = null;
	this.operationsListId = null;
	
	this.CommandsCanRun = true;	
	
	this.SetCommandsActive = function(status)
	{
		this.CommandsCanRun = status;
		this.UpdateCommandStatus();
	}
	
	this.UpdateCommandStatus = function()
	{
	    var cb = document.getElementById(this.ForAllCBId);
	    var forall = cb ? cb.checked : false;
	    var obj = document.getElementById(this.ApplyButtonId);
		if (obj) obj.disabled = !this.CommandsCanRun;

		for (var i = 0; i<this.CommandList.length; i++)
		{
			if (this.CommandList[i]['ID'])
			    obj = document.getElementById(this.CommandList[i]['ID']);
			if (obj == null) continue;			
			if (this.CommandsCanRun)
			{
				if (forall && this.CommandList[i]['DISABLEALL'])
					obj.canClick = false;
				else if (this.CommandList[i]['CSSON'])
					obj.canClick = true;
			}
			else if (!this.CommandsCanRun)
				obj.canClick = false;
			
			if (obj.canClick && this.CommandList[i]['CSSON'])
				obj.className = this.CommandList[i]['CSSON'];
			else if (!obj.canClick && this.CommandList[i]['CSSOFF'])
				obj.className = this.CommandList[i]['CSSOFF'];
		}
	}
	
	this.ConfirmButton = function(obj)
	{
		for (var i = 0; i<this.CommandList.length; i++)
		{
			if (this.CommandList[i].ID == obj.id)
			{
				var item = this.CommandList[i];
				var confirmText = 
					document.getElementById(this.ForAllCBId).checked 
					? (item.CONFIRMALL != '' ? item.CONFIRMALL : options.confirmAllText) 
					: (item.CONFIRM != '' ? item.CONFIRM : options.confirmText);
				return window.confirm(confirmText);
			}
		}
		return true;
	}
	
	this.ConfirmOption = function(obj)
	{
		if (obj == null)
			obj = document.getElementById(this.operationsListId);
		if (obj.selectedIndex < 0)
			return true;
		var opt = obj.options[obj.selectedIndex];
		if (opt.getAttribute == null || opt.getAttribute('showconfirmdialog') == null)
			return true;
		
		var confirmText = 
			document.getElementById(this.ForAllCBId).checked 
			? (opt.getAttribute('confirmall') != null ? opt.getAttribute('confirmall') : options.confirmAllText) 
			: (opt.getAttribute('confirm') != null ? opt.getAttribute('confirm') : options.confirmText);
		return window.confirm(confirmText);
	}
}

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded(); 