function BXAdminFilter(filter_id, aRows)
{
	var _this = this;
	this.filter_id = filter_id;
	this.aRows = aRows;
	
	this.ToggleFilterRow = function(row_id, on, bSave)
	{
		var row = document.getElementById(row_id);
		var delimiter = document.getElementById(row_id+'_delimiter');
		var visible = document.getElementById(row_id+'_visible');
		if(!row)
			return;

		var short_id = row_id.substr((this.filter_id+'_row_').length);

		if(on != true && on != false)
			on = (row.style.display == 'none');
		for (var i in this.aRows)
			if (this.aRows[i] == short_id)
			{
				on = true;
				break;
			}

		//filter popup menu
		var filterMenu = window[this.filter_id+"_menu"];
		if(on == true)
		{
			try{
				row.style.display = 'table-row';
				delimiter.style.display = 'table-row';
			}
			catch(e){
				row.style.display = 'block';
				delimiter.style.display = 'block';
			}
			if(filterMenu)
				filterMenu.SetItemIcon(short_id, "checked");
			visible.value = 'T';
		}
		else
		{
			row.style.display = 'none';
			delimiter.style.display = 'none';
			if(filterMenu)
				filterMenu.SetItemIcon(short_id, "");
			visible.value = 'F';
		}
	}
	
	this.ToggleAllFilterRows = function(on)
	{
		var tbl = document.getElementById(this.filter_id + "_items");
		if(!tbl)
			return;

		var n = tbl.rows.length;
		for(var i=0; i<n; i++)
		{
			var row = tbl.rows[i];
			if(row.id && row.cells[0].className != 'delimiter')
				this.ToggleFilterRow(row.id, on, false);
		}
	}
}

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded(); 