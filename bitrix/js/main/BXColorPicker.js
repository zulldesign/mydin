/* This code is used internally in Bitrix API and is not intended for public use. This file is subject to change. */

if(typeof(Bitrix) == "undefined")
	Bitrix = new Object();

if(typeof(Bitrix.UI) == "undefined")
	Bitrix.UI = new Object();

if(typeof(Bitrix.UI.ColorPicker) == "undefined")
{
	Bitrix.UI.ColorPicker = function(oPar)
	{
		if (!oPar)
			oPar = {};
		if (!oPar.Strings)
			oPar.Strings = {};
			
		if (!oPar.Colors)
			oPar.Colors = [
				'#FF0000', '#FFFF00', '#00FF00', '#00FFFF', '#0000FF', '#FF00FF', '#FFFFFF', '#EBEBEB', '#E1E1E1', '#D7D7D7', '#CCCCCC', '#C2C2C2', '#B7B7B7', '#ACACAC', '#A0A0A0', '#959595',
				'#EE1D24', '#FFF100', '#00A650', '#00AEEF', '#2F3192', '#ED008C', '#898989', '#7D7D7D', '#707070', '#626262', '#555', '#464646', '#363636', '#262626', '#111', '#000000',
				'#F7977A', '#FBAD82', '#FDC68C', '#FFF799', '#C6DF9C', '#A4D49D', '#81CA9D', '#7BCDC9', '#6CCFF7', '#7CA6D8', '#8293CA', '#8881BE', '#A286BD', '#BC8CBF', '#F49BC1', '#F5999D',
				'#F16C4D', '#F68E54', '#FBAF5A', '#FFF467', '#ACD372', '#7DC473', '#39B778', '#16BCB4', '#00BFF3', '#438CCB', '#5573B7', '#5E5CA7', '#855FA8', '#A763A9', '#EF6EA8', '#F16D7E',
				'#EE1D24', '#F16522', '#F7941D', '#FFF100', '#8FC63D', '#37B44A', '#00A650', '#00A99E', '#00AEEF', '#0072BC', '#0054A5', '#2F3192', '#652C91', '#91278F', '#ED008C', '#EE105A',
				'#9D0A0F', '#A1410D', '#A36209', '#ABA000', '#588528', '#197B30', '#007236', '#00736A', '#0076A4', '#004A80', '#003370', '#1D1363', '#450E61', '#62055F', '#9E005C', '#9D0039',
				'#790000', '#7B3000', '#7C4900', '#827A00', '#3E6617', '#045F20', '#005824', '#005951', '#005B7E', '#003562', '#002056', '#0C004B', '#30004A', '#4B0048', '#7A0045', '#7A0026'
			];
		this.disabled = false;
		this.bCreated = false;
		this.bOpened = false;
		this.zIndex = 1000;
		
		this.oPar = oPar;
		
		this._documentSelectedRange = null;
		this._documentBookmark = null;
	}

	Bitrix.UI.ColorPicker.prototype.Instantiate = function ()
	{
		var _this = this;
		this._keyPressHandler = function(e){_this.OnKeyPress(e);};
		this._outClickHandler = function(e){_this.Hide();};
		this.pColCont = document.body.appendChild(jsUtils.CreateElement("DIV", {className: "bx-colorpicker-container"}, {zIndex: this.zIndex}));

		var
			arColors = this.oPar.Colors,
			row, cell, colorCell,
			tbl = jsUtils.CreateElement("TABLE", {className: 'bx-colorpicker-table'}),
			i, l = arColors.length;

		row = tbl.insertRow(-1);
		if (this.oPar.EnableDefault)
		{
			cell = row.insertCell(-1);
		
			cell.colSpan = 8;
			var defBut = cell.appendChild(jsUtils.CreateElement("SPAN", {className: 'bx-colorpicker-default-button'}));
			defBut.innerHTML = oPar.Strings.Default || 'Default';
			defBut.onmouseover = function()
			{
				this.className = 'bx-colorpicker-default-button bx-colorpicker-default-button-over';
				colorCell.style.backgroundColor = 'transparent';
			};
			defBut.onmouseout = function(){this.className = 'bx-colorpicker-default-button';};
			defBut.onclick = function(e){_this.Select(false);}
		}
		colorCell = row.insertCell(-1);
		colorCell.colSpan = this.oPar.EnableDefault ? 8 : 16;
		colorCell.className = 'bx-colorpicker-preview-cell';
		colorCell.style.backgroundColor = arColors[38];

		for(i = 0; i < l; i++)
		{
			if (Math.round(i / 16) == i / 16) // new row
				row = tbl.insertRow(-1);

			cell = row.insertCell(-1);
			cell.innerHTML = '&nbsp;';
			cell.className = 'bx-colorpicker-cell';
			cell.style.backgroundColor = arColors[i];
			cell.colorId = i;

			cell.onmouseover = function (e)
			{
				this.className = 'bx-colorpicker-cell bx-colorpicker-cell-over';
				colorCell.style.backgroundColor = arColors[this.colorId];
			};
			cell.onmouseout = function (e){this.className = 'bx-colorpicker-cell';};
			cell.onclick = function (e)
			{
				_this.Select(arColors[this.colorId]);
			};
		}

		this.pColCont.appendChild(tbl);
		this.bCreated = true;
	};

	Bitrix.UI.ColorPicker.prototype.Destroy = function ()
	{
		if (this.bOpened)
			this.Hide();
		if (this.bCreated)
		{
			document.body.removeChild(this.pColCont);
			this.bCreated = false;
		}
	};


	Bitrix.UI.ColorPicker.prototype.Toggle = function (control, callback)
	{
		if(this.disabled)
			return false;

		if (!this.bCreated)
			this.Instantiate();

		if (this.bOpened)
			return this.Hide();

		this.Show(control, callback);
	};


	Bitrix.UI.ColorPicker.prototype.Show = function (control, callback)
	{
		if(/*@cc_on !@*/false){ //IE
			this._documentSelectedRange = null;
			this._documentBookmark = null;
			try
			{
				this._documentSelectedRange = document.selection.createRange().duplicate();
				if(this._documentSelectedRange && this._documentSelectedRange.text.length == 0 )
					this._documentBookmark  = this._documentSelectedRange.getBookmark();
				else
					this._documentBookmark = null;			
			}
			catch(e)
			{
				this._documentSelectedRange = null;
				this._documentBookmark = null;
			}
		}
		
		var
			pos = jsUtils.AlignToPos(Bitrix.ElementPositioningUtility.getElementRect(control) /*jsUtils.GetRealPos(control) - doesn't take into account the scrolling of divs */, 325, 155),
			_this = this;

		if (callback != null && typeof(callback) == 'function')
			this.oPar.OnSelect = callback;

		window.setTimeout(
			function()
			{
				if (_this.bOpened)
				{
					jsUtils.addEvent(window, "keypress", _this._keyPressHandler );
					jsUtils.addEvent(window.document, "click", _this._outClickHandler);
				}
			}, 
			0
		);

		this.pColCont.style.display = 'block';
		this.pColCont.style.top = pos.top + 'px';
		this.pColCont.style.left = pos.left + 'px';
		this.bOpened = true;
	}

	Bitrix.UI.ColorPicker.prototype.Hide = function ()
	{
		this.pColCont.style.display = 'none';
		jsUtils.removeEvent(window, "keypress", this._keyPressHandler );
		jsUtils.removeEvent(window.document, "click", this._outClickHandler);

		this.bOpened = false;
	}

	Bitrix.UI.ColorPicker.prototype.OnKeyPress = function(e)
	{
		if(!e) e = window.event
		if(e.keyCode == 27)
			this.Hide();
	};

	Bitrix.UI.ColorPicker.prototype.Select = function (color)
	{
		this.Hide();
		if(/*@cc_on !@*/false){ //IE
			if(this._documentSelectedRange){
				if(this._documentBookmark)
					this._documentSelectedRange.moveToBookmark(this._documentBookmark);
				else
					this._documentSelectedRange.select();
			}
			
			this._documentSelectedRange = null;
			this._documentBookmark = null;
		}	
		
		if (this.oPar.OnSelect && typeof this.oPar.OnSelect == 'function')
			this.oPar.OnSelect(this, color);
		
	};
	
	Bitrix.UI.ColorPicker.Instantiate = function () 
	{
        var self = new Bitrix.UI.ColorPicker();
        self.Instantiate();
        return self;		
	};
}


if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
