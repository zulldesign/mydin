function DirectoryNode()
{
	this.loaded = false;
	this.expandable = false;
	this.expanded = false;
	this.parent = null;
	this.file = false;
	this.value = '';
	this.title = '';
	this.num = 0;
	this.loading = false;
	this.childTable = null;
	this.onExpanded = [];
	this.childNodes = [];
	
	this.Expanded = function()
	{
		for(var i = this.onExpanded.length - 1; i >= 0; i--)
			this.onExpanded[i](this, this.onExpanded[i], this.expandable);
	}
	this.AttachExpanded = function(handler)
	{
		for(var i = this.onExpanded.length - 1; i >= 0; i--)
			if (this.onExpanded[i] == handler)
				return;
		this.onExpanded.push(handler);
	}
	this.DetachExpanded = function(handler)
	{
		for(var i = this.onExpanded.length - 1; i >= 0; i--)
			if (this.onExpanded[i] == handler)
				this.onExpanded.splice(i, 1);
	}
}

//params = {self:'', handler:'', resizer:'', appPath:'', containerWidth:0, extrasWidth:0, dialogObj:'', controls:{}, images:{}, strings:{}};
//params.controls = {selector, target, preview, extras, extrasImage, container}
//Properties = {ShowFiles:false, ItemsToSelect:0, ExtensionsList:'', ShowExtras:false, EnableExtras:false, DefaultUploadDirectory:''};

function DirectoryBrowser(browserParams, customizableProperties)
{
	var _this = this;
	this.params = browserParams;
	this.Properties = customizableProperties;
	
	this.dialog = window[this.params.dialogObj];
	this.extrasVisible = false;
	this.container = this.params.controls.container;
	this.divElement = this.params.controls.selector;
	this.selectorId = this.divElement.id;
	this.divElement.innerHTML = '<table id="' + this.selectorId + '_table_root" border="0" cellspacing="0" cellpadding="0"></table>';
	this.childTable = document.getElementById(this.selectorId + "_table_root"); 
	this.nodeCount = 0;
	this.nodes = [];
	this.selectedNode = -1;
	this.targetElement = this.params.controls.target;
	this.upload = this.params.controls.uploadFrame;
	this.previewContainer = this.params.controls.preview;
	this.extras = this.params.controls.extras;
	this.extrasImg = this.params.controls.extrasImage;
	this.freeNodes = [];
	
	this.waitImage = new Image();
	this.waitImage.alt = this.params.strings.wait;
	this.waitImage.src = this.params.images.wait;
	this.waitImage.style.border = 'none';
	
	this.SelectFolders = 1;
	this.SelectFiles = 2;
	
	this._BuildPath = function(node)
	{
		var n = this.nodes[node];
		var path = '';
		if (n.value != '')
			path = '/' + n.value;
		node = n.parent
		while (node != -1)
		{
			n = this.nodes[node];
			if (n.value != '')
				path = '/' + n.value + path;
			node = n.parent;
		}
		return '~' + path;
	}
	
	this._GetChildContainer = function(parentnode)
	{
		var cid;
		if (parentnode >= 0)
		{
			if (!this.nodes[parentnode].childTable)
				this.nodes[parentnode].childTable = document.getElementById(this.selectorId + "_table_" + parentnode); 
			return this.nodes[parentnode].childTable
		}
		else
			return this.childTable;
	}
	
	this._AddNode = function(value, title, expandable, file, parentnode)
	{
		var container = this._GetChildContainer(parentnode);
		
		var num = 0;
		if (this.freeNodes.length > 0)
			num = this.freeNodes.pop();
		else
			num = this.nodeCount++;
		var n = this.nodes[num] = new DirectoryNode();
		n.expandable = expandable;
		n.file = file;
		n.parent = parentnode;
		n.value = value;
		n.title = value;
		n.num = num;
		
		if (parentnode >= 0)
			this.nodes[parentnode].childNodes.push(num); 
		
		var result = '<table border="0" cellpadding="0" cellspacing="0"><tr>';
		
		if (file)
			result += '<td><img border="0" align="absbottom" src="' + this.params.images.empty + '" /></td>';
		else if (!expandable)
			result += '<td><img border="0" align="absbottom" src="' + this.params.images.dot + '" /></td>';
		else
			result += '<td><img border="0" id="' + this.selectorId + '_item_dot_' + num + '" align="absbottom" src="' + this.params.images.plus + '" onclick="' + this.params.self + '.ToggleNode(' + num + ');" /></td>';
		var itemToSelect = file ? this.SelectFiles : this.SelectFolders;
		var allowSelect = ((this.Properties.ItemsToSelect & itemToSelect) > 0 || itemToSelect == this.SelectFolders)
		var selectScript = allowSelect  ? (this.params.self + '.SelectNode(' + num + ');') : '';
		result += '<td onclick="' 
			+ selectScript 
			+ '"  ondblclick="' 
			+ selectScript 
			+ (expandable ? (this.params.self + '.ExpandNode(' + num + ');') : '') 
			+ '" style="white-space: nowrap;' 
			+ ' cursor: '
			+ (allowSelect ?  'pointer' : 'default')
			+ '" >';
		var imgSrc;
		if (parentnode == -1)
			imgSrc = this.params.images.root;
		else if (file) 
			imgSrc = this.params.images.file;
		else
			imgSrc = this.params.images.closed;
		result += '<img src="' + imgSrc + '" id="' + this.selectorId + '_item_img_' + num + '" align="absbottom" border="0" style="padding-left: 3px; padding-right: 2px" />';
		//result += '</td>';
		result += '&nbsp;'//</td>'
		result += '<span id="' + this.selectorId + '_item_txt_' + num + '" style="font-family: sans-serif; font-size: 12px; vertical-align: bottom; padding-left: 2px; padding-right: 3px; color: Black;" >' + title + '</span></td>';
		result += '</tr></table>\n';
		if (expandable)
			result += '<table id="' + this.selectorId + '_table_' + num + '" border="0" cellpadding="0" cellspacing="0" style="display: none; margin-left: 15px;"></table>\n';
		
		var row = container.insertRow(-1);
		var cell = row.insertCell(-1);
		cell.innerHTML = result;
		return n;
	}
	
	this.ExpandTotal = function(node)
	{
		var n = this.nodes[node];
		var h = function(n, f, expandable)
		{
			n.DetachExpanded(f);
			if (!expandable)
				return;
			var list = [];
			for(var i = 0; i < n.childNodes.length; i++)
				if (_this.nodes[n.childNodes[i]].expandable)
					list.push(n.childNodes[i]);
			for(var i = 0; i < list.length; i++)
			{
				_this.nodes[list[i]].AttachExpanded(h);
				_this.ExpandNode(list[i]);
			}
			delete list;
		}
		n.AttachExpanded(h);
		this.ExpandNode(node);
	}
	
	this.ExpandPath = function(path, start, writeMissing)
	{
		var s = (start == null) ? 0 : start
		path = jsUtils.Path.Prepare(path).toLowerCase();
		var list = path.split('/');
		var n = this.nodes[s];
		var idx = 1; //because first is '~'
		var h = function(n, f, expandable)
		{
			n.DetachExpanded(f);
//			if (!expandable)
//				return;
			if (idx >= list.length)
				return;
			
			for(var i = 0; i < n.childNodes.length; i++)
				if ((_this.nodes[n.childNodes[i]].value.toLowerCase() == list[idx]) && (writeMissing || _this.nodes[n.childNodes[i]].expandable || (idx == list.length - 1)))
				{
					idx++;
					
					//if (idx == list.length)
					_this.SelectNode(n.childNodes[i]);
					if (idx != list.length)//else
					{
						_this.nodes[n.childNodes[i]].AttachExpanded(h);
						_this.ExpandNode(n.childNodes[i]);
					}
					return;
				}
			if (writeMissing)
			{
				var path = _this._BuildPath(n.num);
				while (idx < list.length)
				{
					path += '/' + list[idx];
					idx++;
				}
				if (_this.targetElement)
					_this.targetElement.value = path;
			}
		}
		if (path != '')
			n.AttachExpanded(h);
		this.SelectNode(s);
		this.ExpandNode(s);
	}
	
	this.ExpandNode = function(node)
	{
		try
		{
			var _this = this;
			var n = this.nodes[node];
			if (!n.expandable)
			{
				n.Expanded();
				return;
			}
			var childs = this._GetChildContainer(n.num);
			var img = document.getElementById(this.selectorId + '_item_img_' + node);
			var dot = document.getElementById(this.selectorId + '_item_dot_' + node);
			if (!n.loaded)
			{
				while (childs.rows.length > 0)
					childs.deleteRow(childs.rows.length - 1);
				var row = childs.insertRow(-1);
				var cell = row.insertCell(-1);
				cell.innerHTML = '<span style="font-family: sans-serif; font-size: 12px;">' + this.params.strings.loading + '</span>';
				var http = new JCHttpRequest();
				http.Action = function(result)
				{
					try 
					{
						if (n.loading || n.loaded)
							return;
						n.loading = true;
						
						while (childs.rows.length > 0)
							childs.deleteRow(childs.rows.length - 1);
						if (result.substring(0, 5) != 'FILE|')
							throw 0;
						var items = result.substring(5).split('|');
						for (var i in items)
						{
							if (items[i] == null || items[i] == '')
								continue;
							var data = items[i].split(':');
							_this._AddNode(data[0], data[0], data[1] == "dt", data[1] == 'f', n.num);
						}
						n.loaded = true;
						n.loading = false;
						setTimeout(function(){
							n.Expanded();
						}, 1);
					} 
					catch(e)
					{
						n.loading = false;
					}
				}
				http.Send(this.params.handler + '?' + 'path=' + encodeURIComponent(this._BuildPath(node)) + (this.Properties.ShowFiles ? '&files=' : '') + ((this.Properties.ExtensionsList != '') ? ('&ext=' + encodeURIComponent(this.Properties.ExtensionsList)) : '') + '&nocache=' + new Date().getTime());
			}
			childs.style.display = 'block';
			if (n.parent != -1)
				img.src = this.params.images.opened;
			dot.src = this.params.images.minus;
			n.expanded = true;
			if (n.loaded)
				setTimeout(function(){
					n.Expanded();
				}, 1);
				
		}
		catch(e)
		{}
	}
	
	this.CollapseNode = function(node)
	{
		try
		{
			var n = this.nodes[node];
			if (!n.expandable)
				return;
			var childs = document.getElementById(this.selectorId + '_table_' + node);
			var img = document.getElementById(this.selectorId + '_item_img_' + node);
			var dot = document.getElementById(this.selectorId + '_item_dot_' + node);
			childs.style.display = 'none';
			img.src = (n.parent == -1) ? this.params.images.root : this.params.images.closed;
			dot.src = this.params.images.plus;
			n.expanded = false;
		}
		catch(e)
		{}
	}
	
	this.ResetNode = function(node)
	{
		var n = this.nodes[node];
		var expand = n.expanded;
		
		if (expand)	this.CollapseNode(node);
		
		for (var i = 0; i < n.childNodes.length; i++)
		{
			this.freeNodes.push(n.childNodes[i]);
			this.nodes[n.childNodes[i]] = null;
		}
		n.childNodes = [];
		
		var childs = this._GetChildContainer(node);
		while (childs.rows.length > 0)
			childs.deleteRow(childs.rows.length - 1);
		n.loaded = false;
		
		if (expand)	this.ExpandNode(node);
	}

	this.CollapseToRoot = function()
	{
		for (var i in this.nodes)
			this.CollapseNode(this.nodes[i].num);
		//this.SelectNode(0);
		this.ExpandNode(0);	
	}

	this.Reset = function()
	{
		for (var i in this.nodes)
		{
			this.nodes[i].parent = null;
			this.nodes[i].childTable = null;
			this.nodes[i] = null;
		}
		this.nodes = [];
		this.nodeCount = 0;
		while (this.childTable.rows.length > 0)
			this.childTable.deleteRow(this.childTable.rows.length - 1);
			
		this.selectedNode = -1;
		this.targetElement.value = '';
		this.freeNodes = [];
		this._ClearPreview();
		
		this.upload.src = this.params.uploader + '?defaultDir=' + encodeURIComponent(this.Properties.DefaultUploadDirectory) + '&rnd=' + Math.random().toString().substring(2);
		
		this.extrasImg.style.display = this.Properties.EnableExtras ? 'block' : 'none';
		if (((!this.Properties.ShowExtras || !this.Properties.EnableExtras) && this.extrasVisible)
		|| (this.Properties.ShowExtras && this.Properties.EnableExtras && !this.extrasVisible))
			setTimeout(function() {
				_this.ToggleExtras();
			}, 1);
		this._AddNode('', this.params.strings.root, true, false, -1);
		
		if (this.Properties.DefaultPath)
			this.ExpandPath(this.Properties.DefaultPath, 0, true);		
	}

	this.GetTargetValue = function()
	{
		return this.targetElement.value;
	}
	
	this.ToggleNode = function(node)
	{
		var n = this.nodes[node];
		if (!n.expandable)
			return;
		if (n.expanded)
			this.CollapseNode(node);
		else
			this.ExpandNode(node);
	}
	
	this.SelectNode = function(node)
	{
		var n = this.nodes[node]
		
		//If select is forbidden			
		if (n.file && ((this.Properties.ItemsToSelect & this.SelectFiles) == 0))
			return;
		
		var grayDir = (!n.file && ((this.Properties.ItemsToSelect & this.SelectFolders) == 0));
		
		this._ClearPreview();
		
		//Clear old selection
		if (this.selectedNode != -1 && this.selectedNode != node)
		{
			var oldText = document.getElementById(this.selectorId + '_item_txt_' + this.selectedNode);
			//oldText.style.fontWeight = 'normal';
			//oldText.style.color = 'black';
			oldText.parentNode.style.backgroundColor = '';
			this.selectedNode = -1;
		}	
		
		var buildPath = this._BuildPath(node);
		if (this.targetElement && !grayDir)
			this.targetElement.value = buildPath + ((!n.file && this.Properties.AppendSlashToFolders) ? '/' : '');
		
		if (this.selectedNode !== node)
		{
			var newText = document.getElementById(this.selectorId + '_item_txt_' + node);
			//newText.style.fontWeight = 'bold';
			//newText.style.color = grayDir ? 'gray' : 'black';
			newText.parentNode.style.backgroundColor = '#E0E7F9';
			this.selectedNode = node;
		}
		
		if (this.extrasVisible && this.nodes[this.selectedNode].file)
			this._ShowPreview(buildPath);
			
		if ((!n.file || (n.parent != -1 && !this.nodes[n.parent].file)) && this.extrasVisible) 
		{
			var doc = this.upload.contentWindow.document;
			var p = n.file ? this._BuildPath(n.parent) : buildPath;
			var t1 = doc.getElementById('dir');
			var t2 = doc.getElementById('path');
			if (t1 && t2)
			{
				t1.value = p;
				t2.value = p;
			}
		}
	}
	
	this.ToggleExtras = function()
	{
		var toVisible = jsUtils.IsDisplayNone(this.extras);
		
		if (this.extrasVisible == toVisible)
			return;
		var td;
		jsUtils.ShowTableCell(this.extras, toVisible, "inline");
		this.extrasVisible = toVisible;
		this.extrasImg.className = 'divider ' + (this.extrasVisible ? 'left' : 'right');
		/*if (!this.extrasVisible)
			this._ClearPreview();*/
			
		if (this.container)
		{
			var pos = jsUtils.GetRealPos(_this.container);
			_this.container.style.width = _this.params.containerWidth + (toVisible ? _this.params.extrasWidth : 0) + "px";
			_this.dialog.AdjustShadow();
		}
	}
	
	this._ClearPreview = function()
	{
		for (var i = this.previewContainer.childNodes.length - 1; i >= 0; i--)
			this.previewContainer.removeChild(this.previewContainer.childNodes[i]);
	}
	
	this._ShowPreviewImage = function(path, w, h)
	{
		this.previewContainer.appendChild(this.waitImage);
		this.waitImage.style.display = 'inline';
		var path = jsUtils.Path.Prepare(path);
		var img = new Image();
		img.style.visibility = 'hidden';
		img.style.display = 'none';
		this.previewContainer.appendChild(img);
		img.onload = function()
		{
			if (this.width > w || this.height > h)
			{
				var p = this.width / this.height;
				var k = (p > (w/h)) ? (w / this.width) : (h / this.height);
				this.width *= k;
				this.height *= k;
			}
			_this.waitImage.style.display = 'none';
			img.style.visibility = 'visible';
			img.style.display = 'inline';
		};
		img.src = this.params.resizer + '?path=' + encodeURIComponent(path) + '&width=' + w + '&height=' + h + '&fit=true';
	}
	
	this._ShowPreviewFlash = function(path, w, h)
	{
		var path = this.params.appPath + '/' + path.replace(/\\/g, '/');
		this.previewContainer.innerHTML = 
			'<embed ' +
			'src="' + bxhtmlspecialchars(path) + '" ' +
			'width="' + w + '" ' +
			'height="' + h + '" ' +
			'type="application/x-shockwave-flash" ' +
			'pluginspage="http://www.macromedia.com/go/getflashplayer" ' +
			'></embed>';
	}
	
	this._ShowPreview = function(buildPath)
	{
		this._ClearPreview();
		
		var w = 250, h = 150;
		var buildPath = jsUtils.Path.Prepare(buildPath);
		var ext = jsUtils.Path.ExtractExtension(buildPath);
		
		switch (ext)
		{
			case "jpg":
			case "png":
			case "bmp":
			case "gif":
				this._ShowPreviewImage(buildPath, w, h);
				break;
			case "swf":
				this._ShowPreviewFlash(buildPath, w, h);
				break;
		}
	}
	
	this.extrasImg.onclick = function()
	{
		_this.ToggleExtras();
		this.className = 'divider left-sel';
	}
	this.extrasImg.onmouseover = function()
	{
		this.className = 'divider ' + (_this.extrasVisible ? 'left' : 'right') + '-sel';
	}
	this.extrasImg.onmouseout = function()
	{
		this.className = 'divider ' + (_this.extrasVisible ? 'left' : 'right');
	}
	this.upload.DirectoryBrowser = this;
	
	this.Reset();
	/*setTimeout(function(){
		_this.ToggleExtras();
	}, 1);*/
}

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded(); 