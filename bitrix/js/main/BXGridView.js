function BXGridView(rList)
{
    // ROWNAME, CBNAME, BASECOLOR
    var _this = this;
    this.RowsList = rList;
    
    this.RowsGlobalSelectorId = "";
    this.StatusWindowId = "";
	
	this.IContextToolbar = null;

    this.SetRows = function(rList)
    {
        this.RowsList = rList;   	    
    }
    
	this.MultiActionCBValueChanged = function(obj)
	{
		swnd = document.getElementById(this.StatusWindowId);
	
		var status = obj.checked;
        if (swnd)
		{
			count = parseInt(swnd.innerHTML);			
			if (count > 0)
				status = true;
		}
		
		if (this.IContextToolbar != null)
			this.IContextToolbar.SetCommandsActive(status);
	}

	this.ResetCheckBoxes = function()
	{
	    if (obj = document.getElementById(this.RowsGlobalSelectorId))
	        obj.checked = false;
	
        for (var i=0;i<this.RowsList.length;i++)
        {
            if (obj = document.getElementById(this.RowsList[i]['CBNAME']))
				obj.checked = false;
        }
	}
	
	this.GetSelectedStatus = function(rowNode)
	{
	    if (!rowNode['CBNAME']) return false;
	    obj = document.getElementById(rowNode['CBNAME']);
	    if (!obj) return false;
	    return obj.checked;	    
	}
	
	this.GetGlobalSelected = function()
	{
    	var obj = document.getElementById(this.RowsGlobalSelectorId);
    	if (obj)
    	    return obj.checked;
    	return false;
	}
	
    this.GetSelectedCount = function()
    {
        count = 0;
        for (var i=0;i<this.RowsList.length;i++)
        {
            if (this.GetSelectedStatus(this.RowsList[i])) count++;
        }
        return count;
    }
    
    this.UpdateStatusWindow = function()
    {
		var count = this.GetSelectedCount();
        var obj = document.getElementById(this.StatusWindowId);        
        var obj1 = document.getElementById(this.RowsGlobalSelectorId);
        if (obj)
		{          
            obj.innerHTML = count;
            
			if (count == 0)
			{
			    obj.innerHTML = "0";
			}
		}	
		
        if (obj1 && count == 0)
		{
		    obj1.checked = false;
		}
		
		if (this.IContextToolbar != null)
		{
			var status = false;
			if (count > 0) status = true;
			
			this.IContextToolbar.SetCommandsActive(status);
			
			if (count == 0)
			{
			    obj2 = document.getElementById(this.IContextToolbar.ForAllCBId);
			    if (obj2 && obj2.checked == true)
			        this.MultiActionCBValueChanged(obj2);
			}
		}
    }
    
    this.ToggleSelectionAll = function(checkStatus)
    {
        var RowsGlobalSelectorStatus = this.GetGlobalSelected();
        var status = false;
        
        for (var i=0;i<this.RowsList.length;i++)
        {
            status = this.GetSelectedStatus(this.RowsList[i]);
            if (status != RowsGlobalSelectorStatus)            
                this.ColorRowSelection(i, RowsGlobalSelectorStatus);            
        }
        
        this.UpdateStatusWindow();
    }
    
    this.ColorRowSelection = function(rowIndex, status)
    {
        obj = document.getElementById(this.RowsList[rowIndex]['ROWNAME']);
        cb = document.getElementById(this.RowsList[rowIndex]['CBNAME']);
        if (!obj || !cb) return;
        cb.checked = status;
        if (status)
        {
            obj.className += " selected";            
        }
        else
        {
            obj.className = obj.className.replace(/selected/, "");
        }
    }
    
    this.ToggleSelection = function(rowIndex)
    {
        var status = this.GetSelectedStatus(this.RowsList[rowIndex]);
        this.ColorRowSelection(rowIndex, status);
        this.UpdateStatusWindow();
    }   
                                   
    this.MouseOver = function(ri)
    {
        var item = this.RowsList[ri];
        if (item == null || item['ROWNAME'] == null)
			return;
        var obj = document.getElementById(item['ROWNAME']);
        if (obj)
            obj.className += " over";
    }
    
    this.MouseOut = function(ri)
    {
        var item = this.RowsList[ri];
        if (item == null || item['ROWNAME'] == null)
			return;
        obj = document.getElementById(item['ROWNAME']);
        if (obj)
        {
            obj.className = obj.className.replace(/over/, "");            
        }
    }
    
    this.MouseRClk = function(ri, gridid, popup, event, range)
    {           
        _this.PrepareClick(ri, gridid, popup);
        if (event.ctrlKey) return true;
        var pos = {
            'left': event.clientX+document.body.scrollLeft, 
            'top': event.clientY+document.body.scrollTop, 
            'right':event.clientX+2+document.body.scrollLeft, 
            'bottom':event.clientY+2+document.body.scrollTop
        }
      	popup.ShowMenuRangedAt(pos, range);
        return false;
    }
    
    this.CommandButtonClick = function(control, ri, gridid, popup, range)
    {
		control.blur();
		_this.PrepareClick(ri, gridid, popup)
		popup.ShowMenuRanged(control, range)
    }
    
    this.PrepareClick = function(ri, gridid, popup)
    {           
        popup.onclickArgument = ri + "$" + gridid;        
        if (_this.userData)
			popup.currentUserData = _this.userData[ri];
    }
}

/************************************************/

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded(); 