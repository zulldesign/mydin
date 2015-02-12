function BXTabControl(cssClass, selectedStorageId, stripCellsArray)
{
    this.cssClass = cssClass;
    this.selectedStorage = document.getElementById(selectedStorageId);
    this.stripCells = stripCellsArray ? stripCellsArray : [];
    
    this.SetFirstSplitter = function(divid, display)
    {
        var obj = document.getElementById(divid);
        if (!obj) return;
        t = jsUtils.FindChildObject(obj, "TABLE", null);
        if (!t || (t && t.className != this.cssClass+"-Tab-Title")) return;
        t.rows[0].style.display = display;
    }
    
    this.HideTabs = function(tableid)
    {
        var obj = document.getElementById(tableid);
        if (!obj) return;
        td = obj.rows[0].cells[0];
        for (var i = 0; i < td.childNodes.length; i++)
            if (td.childNodes[i].tagName == "DIV")
                td.childNodes[i].style.display = "none";
    }
    
    this.ShowTab = function(divid)
    {
        var obj = document.getElementById(divid);
        if (!obj) return;
        obj.style.display = "block";
    }
    
    this.HideTab = function(divid)
    {
        var obj = document.getElementById(divid);
        if (!obj) return;
        obj.style.display = "none";
    }
    
    this.NoSel = function(tableid)
    {    
        var obj = document.getElementById(tableid);
        if (!obj) return;
        for (var i=0;i<obj.rows[0].cells.length;i++)
        {            
            td = obj.rows[0].cells[i];
            if (td.className.indexOf("tab-container-s") >= 0)
            {
                td.className = "inner tab-container";
                tab = jsUtils.FindChildObject(td, "TABLE", null);
                if (tab)
                {
                    var last = false;
                    if (i == obj.rows[0].cells.length - 2)
                        last = true;
                    this.SetTabMode(tab, false, last);
                }
            }            
        }
    }
    
    this.TabSel = function(id)
    {
        if (!this.stripCells[id])
			return;
        var td = document.getElementById(this.stripCells[id]);
        if (!td) return;
        td.className = "inner tab-container-selected";
        var last = false;
        if (td.nextSibling && td.nextSibling.className == "inner tab-indent-last")
            last = true;
        this.SetTabMode(jsUtils.FindChildObject(td, "TABLE", null), true, last);
        if (this.selectedStorage)
			this.selectedStorage.value = id;
    }
    
    this.SetTabMode = function(t, selected, last)
    {
        var x = "";
        if (!t) return;        
        if (selected) x = "-selected";         
        t.rows[0].cells[0].className = "inner tab-left" + x;
        t.rows[0].cells[1].className = "inner tab" + x;
        if (last) { x = "-last"; if (selected) x += "-selected"; }
        t.rows[0].cells[2].className = "inner tab-right" + x;
    }
    
    this.HighlightTab = function(td, light)
    {        
        t = jsUtils.FindChildObject(td, "TABLE", null);
        if (!t) return;
        var h = "";
        if (light) h = "-hover";
        t.rows[0].cells[0].className = "inner tab-left" + h;        
        t.rows[0].cells[1].className = "inner tab" + h;
        var x = "";
        if (td.nextSibling && td.nextSibling.className == "inner tab-indent-last")
            x = "-last";
        t.rows[0].cells[2].className = "inner tab-right" + x + h;
    }
    
    this.MouseOver = function(tdid)
    {
        td = document.getElementById(tdid);
        if (!td) return;        
        if (td.className.indexOf("tab-container-s") == -1)
        this.HighlightTab(td, true);
    }
    
    this.MouseOut = function(tdid)
    {
        td = document.getElementById(tdid);
        if (!td) return;
        if (td.className.indexOf("tab-container-s") == -1)
        this.HighlightTab(td, false);
    }
    
    this.DisableTabs = function(tableid, disabled)
    {
        var obj = document.getElementById(tableid);
        if (!obj) return;
        var seltd = '';
        if (disabled) 
			this.NoSel(tableid);                
        for (var i=0;i<obj.rows[0].cells.length;i++)
        {            
            td = obj.rows[0].cells[i];
            if (td.className.indexOf("tab-container") == -1) continue;            
            if (disabled)
            {
                td.oldcn = td.className;
                td.className = "inner tab-container-disabled";
            } else 
            	if (td.oldcn) 
            		td.className = td.oldcn; 
            	else 
            		td.className = "inner tab-container";
        }
        if (!disabled)
			if (this.selectedStorage && !isNaN(this.selectedStorage.value))
				this.TabSel(this.selectedStorage.value);
			else
				this.TabSel(0);
    }
    
    this.ToggleTabs = function(button, tableid)
    {
        if (button.className.indexOf("down") >= 0)
            this.DisableTabs(tableid, true);
        else
            this.DisableTabs(tableid, false);
    }
    
    this.ToggleView = function(button, tabs)
    {        
        if (!button) return;
        if (button.className.indexOf("down") >= 0)
        {
            button.className = button.className.replace(/down/, "up");
            for (var i=0;i<tabs.length;i++)
            if (tabs[i] && tabs[i].length>1)
            {
                this.ShowTab(tabs[i]);
                this.SetFirstSplitter(tabs[i], "");
            }
        }
        else
        {
            button.className = button.className.replace(/up/, "down");
            for (var i=0;i<tabs.length;i++)
            if (tabs[i] && tabs[i].length>1)
            {
                this.HideTab(tabs[i]);  
                this.SetFirstSplitter(tabs[i], "none");              
            }
            var s = this.selectedStorage ? this.selectedStorage.value : 0;
            var it = 0;
            for (var i=0;i<tabs.length;i++)
				if (tabs[i] && tabs[i].length > 1)
				{
					if (s == it) 
					{
						this.ShowTab(tabs[i]);
						break;
					}
					it++;
				}
        }
    }
    
    this.AllowedTabClick = function(tdid)
    {
        td = document.getElementById(tdid);
        if (!td) return; 
        return (td.className.indexOf("tab-container-d") == -1)?true:false;
    }
    
}

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded(); 