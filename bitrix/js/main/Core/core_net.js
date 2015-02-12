BX.CDialogNet = function(arParams)
{
	BX.CDialogNet.superclass.constructor.apply(this);

	this.PARAMS = arParams || {};

	for (var i in this.defaultParams)
	{
		if (typeof this.PARAMS[i] == 'undefined')
			this.PARAMS[i] = this.defaultParams[i];
	}

	this.PARAMS.width = (!isNaN(parseInt(this.PARAMS.width)))
		? this.PARAMS.width
		: this.defaultParams['width'];
	this.PARAMS.height = (!isNaN(parseInt(this.PARAMS.height)))
		? this.PARAMS.height
		: this.defaultParams['height'];

	if (this.PARAMS.resize_id || this.PARAMS.content_url)
	{
		var arSize = BX.WindowManager.getRuntimeWindowSize(this.PARAMS.resize_id || this.PARAMS.content_url);
		if (arSize)
		{
			this.PARAMS.width = arSize.width;
			this.PARAMS.height = arSize.height;
		}
	}

	BX.addClass(this.DIV, 'bx-core-dialog');

	this.PARTS = {};

	this.DIV.style.height = null;
	this.DIV.style.width = null;

	this.PARTS.CONTENT = this.DIV.appendChild(BX.create('DIV', {
		props: {className: 'dialog-center'}
	}));


	this.PARTS.CONTENT_DATA = this.PARTS.CONTENT.appendChild(BX.create('DIV', {
		props: {className: 'bx-core-dialog-content'}
	}));

	BX.adjust(this.PARTS.CONTENT_DATA, {
		style: {
			height: this.PARAMS.height + 'px',
			width: this.PARAMS.width + 'px'
		}
	});

	this.PARTS.HEAD = this.PARTS.CONTENT_DATA.appendChild(BX.create('DIV', {
		props: {
			className: 'bx-core-dialog-head'
		},
		children: [
			BX.create('DIV', {
				props: {className: 'bx-core-dialog-head-content' + (this.PARAMS.icon ? ' ' + this.PARAMS.icon : '')}
			})
		]
	}));

	this.SetHead(this.PARAMS.head);
	this.SetContent(this.PARAMS.content);

	this.PARTS.TITLEBAR = this.DIV.appendChild(BX.create('DIV', {props: {
			className: 'dialog-head'
		},
		html: '<div class="l"><div class="r"><div class="c"><span>'+this.PARAMS.title+'</span></div></div></div>'
	}));
	this.PARTS.TITLE_CONTAINER = this.PARTS.TITLEBAR.firstChild.firstChild.firstChild.firstChild;
	this.SetTitle(this.PARAMS.title);

	this.PARTS.TITLEBAR_ICONS = this.DIV.appendChild(BX.create('DIV', {
		props: {
			className: 'dialog-head-icons'
		},
		children: (this.PARAMS.resizable ? [
			BX.create('A', {props: {className: 'bx-icon-expand', title: BX.message('JS_CORE_WINDOW_EXPAND')}}),
			BX.create('A', {props: {className: 'bx-icon-close', title: BX.message('JS_CORE_WINDOW_CLOSE')}})
		] : [
			BX.create('A', {props: {className: 'bx-icon-close', title: BX.message('JS_CORE_WINDOW_CLOSE')}})
		])
	}));

	this.SetClose(this.PARTS.TITLEBAR_ICONS.lastChild);

	if (this.PARAMS.resizable)
	{
		this.SetExpand(this.PARTS.TITLEBAR_ICONS.firstChild);
		BX.addCustomEvent(this, 'onWindowExpand', BX.proxy(this.__onexpand, this));
		BX.addCustomEvent(this, 'onWindowNarrow', BX.proxy(this.__onexpand, this));
	}


	this.PARTS.FOOT = this.DIV.appendChild(BX.create('DIV', {props: {
			className: 'dialog-foot'
		},
		html: '<div class="l"><div class="r"><div class="c"><img src="' + jsUtils.Path.ToAbsolute('~/bitrix/js/main/core/images/line.png') + '" height="1" width="90%" border="0" style="position: absolute; top: 0; left: 0;" /><span></span></div></div></div>'
	}));

	this.PARTS.BUTTONS_CONTAINER = this.PARTS.FOOT.firstChild.firstChild.firstChild.lastChild;
	BX.adjust(this.PARTS.BUTTONS_CONTAINER, {
		children: this.ShowButtons()
	});

	if (this.PARAMS.draggable)
		this.SetDraggable(this.PARTS.TITLEBAR);

	this.SetExpand(this.PARTS.TITLEBAR.firstChild, 'dblclick');

	if (this.PARAMS.resizable)
	{
		this.PARTS.RESIZER = this.DIV.appendChild(BX.create('DIV', {
			props: {className: 'bx-core-resizer'}
		}));

		this.SetResize(this.PARTS.RESIZER);

		this.SETTINGS.min_width = this.PARAMS.min_width;
		this.SETTINGS.min_height = this.PARAMS.min_height;
	}
}
BX.extend(BX.CDialogNet, BX.CWindowDialog);

BX.CDialogNet.prototype.defaultParams = {
	width: 700,
	height: 400,
	min_width: 500,
	min_height: 300,

	resizable: true,
	draggable: true,

	title: '',
	icon: ''
}

BX.CDialogNet.prototype.__expandGetSize = function()
{
	var pDocElement = BX.GetDocElement();
	pDocElement.style.overflow = 'hidden';

	var wndSize = BX.GetWindowInnerSize();

	pDocElement.scrollTop = 0;

	this.DIV.style.top = '-' + this.dxShadow + 'px';
	this.DIV.style.left = '-' + this.dxShadow + 'px';

	return {
		width: (wndSize.innerWidth - parseInt(BX.style(this.PARTS.CONTENT, 'padding-right'))),
		height: (wndSize.innerHeight - this.PARTS.TITLEBAR.offsetHeight - (this.PARTS.FOOT.offsetHeight) + this.dxShadow)
	};
}

BX.CDialogNet.prototype.__expand = function()
{
	var pDocElement = BX.GetDocElement();
	this.dxShadow = 7;

	if (!this.bExpanded)
	{
		var wndScroll = BX.GetWindowScrollPos();

		this.__expand_settings = {
			resizable: this.SETTINGS.resizable,
			draggable: this.SETTINGS.draggable,
			width: this.PARTS.CONTENT_DATA.style.width,
			height: this.PARTS.CONTENT_DATA.style.height,
			left: this.DIV.style.left,
			top: this.DIV.style.top,
			scroll: wndScroll.scrollTop,
			overflow: BX.style(pDocElement, 'overflow')
		}

		this.SETTINGS.resizable = false;
		this.SETTINGS.draggable = false;

		var pos = this.__expandGetSize();

		this.PARTS.CONTENT_DATA.style.width = pos.width + 'px';
		this.PARTS.CONTENT_DATA.style.height = pos.height + 'px';

		this.bExpanded = true;

		BX.onCustomEvent(this, 'onWindowExpand');
		BX.onCustomEvent(this, 'onWindowResize');
		BX.onCustomEvent(this, 'onWindowResizeExt', [{'width': pos.width, 'height': pos.height}]);

		BX.bind(window, 'resize', BX.proxy(this.__expand_onresize, this));
	}
	else
	{
		BX.unbind(window, 'resize', BX.proxy(this.__expand_onresize, this));

		this.SETTINGS.resizable = this.__expand_settings.resizable;
		this.SETTINGS.draggable = this.__expand_settings.draggable;

		pDocElement.style.overflow = this.__expand_settings.overflow;

		this.DIV.style.top = this.__expand_settings.top;
		this.DIV.style.left = this.__expand_settings.left;
		this.PARTS.CONTENT_DATA.style.width = this.__expand_settings.width;
		this.PARTS.CONTENT_DATA.style.height = this.__expand_settings.height;
		pDocElement.scrollTop = this.__expand_settings.scroll;
		this.bExpanded = false;

		BX.onCustomEvent(this, 'onWindowNarrow');
		BX.onCustomEvent(this, 'onWindowResize');
		BX.onCustomEvent(this, 'onWindowResizeExt', [{'width': parseInt(this.__expand_settings.width), 'height': parseInt(this.__expand_settings.height)}]);
	}
}

BX.CDialogNet.prototype.__expand_onresize = function()
{
	var pos = this.__expandGetSize();

	this.PARTS.CONTENT_DATA.style.width = pos.width + 'px';
	this.PARTS.CONTENT_DATA.style.height = pos.height + 'px';

	BX.onCustomEvent(this, 'onWindowResize');
	BX.onCustomEvent(this, 'onWindowResizeExt', [pos]);
}

BX.CDialogNet.prototype.__onexpand = function()
{
	var ob = this.PARTS.TITLEBAR_ICONS.firstChild;
	ob.className = BX.toggle(ob.className, ['bx-icon-expand', 'bx-icon-narrow']);
	ob.title = BX.toggle(ob.title, [BX.message('JS_CORE_WINDOW_EXPAND'), BX.message('JS_CORE_WINDOW_NARROW')]);

	if (this.PARTS.RESIZER)
	{
		this.PARTS.RESIZER.style.display = this.bExpanded ? 'none' : 'block';
	}
}


BX.CDialogNet.prototype.__startResize = function(e)
{
	if (!this.SETTINGS.resizable)
		return false;

	if(!e) e = window.event;

	this.wndSize = BX.GetWindowScrollPos();
	this.wndSize.innerWidth = BX.GetWindowInnerSize().innerWidth;

	this.pos = BX.pos(this.PARTS.CONTENT_DATA);

	this.x = e.clientX + this.wndSize.scrollLeft;
	this.y = e.clientY + this.wndSize.scrollTop;

	this.dx = this.pos.left + this.pos.width - this.x;
	this.dy = this.pos.top + this.pos.height - this.y;
	this.dw = this.pos.width - parseInt(this.PARTS.CONTENT_DATA.style.width) + parseInt(BX.style(this.PARTS.CONTENT, 'padding-right'));

	BX.bind(document, "mousemove", BX.proxy(this.__moveResize, this));
	BX.bind(document, "mouseup", BX.proxy(this.__stopResize, this));

	if(document.body.setCapture)
		document.body.setCapture();

	document.onmousedown = BX.False;

	var b = document.body;
	b.ondrag = b.onselectstart = BX.False;
	b.style.MozUserSelect = this.DIV.style.MozUserSelect = 'none';
	b.style.cursor = 'se-resize';

	BX.onCustomEvent(this, 'onWindowResizeStart');
}

BX.CDialogNet.prototype.Resize = function(x, y)
{
	var new_width = Math.max(x - this.pos.left + this.dx, this.SETTINGS.min_width);
	var new_height = Math.max(y - this.pos.top + this.dy, this.SETTINGS.min_height);

	if (this.SETTINGS.resize_restrict)
	{
		var scrollSize = BX.GetWindowScrollSize();

		if (this.pos.left + new_width > scrollSize.scrollWidth - this.dw)
			new_width = scrollSize.scrollWidth - this.pos.left - this.dw;
	}

	this.PARTS.CONTENT_DATA.style.width = new_width + 'px';
	this.PARTS.CONTENT_DATA.style.height = new_height + 'px';

	BX.onCustomEvent(this, 'onWindowResize');
	BX.onCustomEvent(this, 'onWindowResizeExt', [{'height': new_height, 'width': new_width}]);
}

BX.CDialogNet.prototype.SetSize = function(obSize)
{
	this.PARTS.CONTENT_DATA.style.width = obSize.width + 'px';
	this.PARTS.CONTENT_DATA.style.height = obSize.height + 'px';

	BX.onCustomEvent(this, 'onWindowResize');
	BX.onCustomEvent(this, 'onWindowResizeExt', [obSize]);
}


BX.CDialogNet.prototype.GetForm = function()
{
	if (null == this.__form)
	{
		var forms = this.PARTS.CONTENT_DATA.getElementsByTagName('FORM');
		this.__form = forms[0] ? forms[0] : null;
	}

	return this.__form;
}

BX.CDialogNet.prototype._checkButton = function(btn)
{
	var arCustomButtons = ['btnSave', 'btnCancel', 'btnClose'];

	for (var i = 0; i < arCustomButtons.length; i++)
	{
		if (this[arCustomButtons[i]] && (btn == this[arCustomButtons[i]]))
			return arCustomButtons[i];
	}

	return false;
}

BX.CDialogNet.prototype.ShowButtons = function()
{
	var result = [];
	if (this.PARAMS.buttons)
	{
		if (this.PARAMS.buttons.title) this.PARAMS.buttons = [this.PARAMS.buttons];

		for (var i=0, len=this.PARAMS.buttons.length; i<len; i++)
		{
			if (BX.type.isNotEmptyString(this.PARAMS.buttons[i]))
			{
				result.push(this.PARAMS.buttons[i]);
			}
			else if (this.PARAMS.buttons[i])
			{
				//if (!(this.PARAMS.buttons[i] instanceof BX.CWindowButton))
				if (!BX.is_subclass_of(this.PARAMS.buttons[i], BX.CWindowButton))
				{
					var b = this._checkButton(this.PARAMS.buttons[i]); // hack to set links to real CWindowButton object in btnSave etc;
					this.PARAMS.buttons[i] = new BX.CWindowButton(this.PARAMS.buttons[i]);
					if (b) this[b] = this.PARAMS.buttons[i];
				}

				result.push(this.PARAMS.buttons[i].Button(this));
			}
		}
	}

	return result;
}

BX.CDialogNet.prototype.SetTitle = function(title)
{
	this.PARAMS.title = title;
	BX.cleanNode(this.PARTS.TITLE_CONTAINER).innerHTML = this.PARAMS.title;
}

BX.CDialogNet.prototype.SetHead = function(head)
{
	this.PARAMS.head = head;
	this.PARTS.HEAD.firstChild.innerHTML = head || "&nbsp;";
	this.PARTS.HEAD.style.display = this.PARAMS.head ? 'block' : 'none';
	this.adjustSize();
}

BX.CDialogNet.prototype.__adjustHeadToIcon = function()
{
	if (!this.PARTS.HEAD.firstChild.offsetHeight)
	{
		setTimeout(BX.delegate(this.__adjustHeadToIcon, this), 50);
	}
	else
	{
		if (this.icon_image && this.icon_image.height && this.icon_image.height > this.PARTS.HEAD.firstChild.offsetHeight - 5)
		{
			this.PARTS.HEAD.firstChild.style.height = this.icon_image.height + 5 + 'px';
			this.adjustSize();
		}

		this.icon_image.onload = null;
		this.icon_image = null;
	}
}

BX.CDialogNet.prototype.SetIcon = function(icon_class)
{
	if (this.PARAMS.icon != icon_class)
	{
		if (this.PARAMS.icon)
			BX.removeClass(this.PARTS.HEAD.firstChild, this.PARAMS.icon);

		this.PARAMS.icon = icon_class

		if (this.PARAMS.icon)
		{
			BX.addClass(this.PARTS.HEAD.firstChild, this.PARAMS.icon);

			var icon_file = (BX.style(this.PARTS.HEAD.firstChild, 'background-image') || BX.style(this.PARTS.HEAD.firstChild, 'backgroundImage')).replace('url("', '').replace('")', '');
			if (BX.type.isNotEmptyString(icon_file) && icon_file != 'none')
			{
				this.icon_image = new Image();
				this.icon_image.onload = BX.delegate(this.__adjustHeadToIcon, this);
				this.icon_image.src = icon_file;
			}
		}
	}
	this.adjustSize();
}

BX.CDialogNet.prototype.SetIconFile = function(icon_file)
{
	this.icon_image = new Image();
	this.icon_image.onload = BX.delegate(this.__adjustHeadToIcon, this);
	this.icon_image.src = icon_file;

	BX.adjust(this.PARTS.HEAD.firstChild, {style: {backgroundImage: 'url(' + icon_file + ')', backgroundPosition: 'right 9px'/*'99% center'*/}});
	this.adjustSize();
}

/*
BUTTON: {
	title: 'title',
	'action': function executed in window object context
}
BX.CDialogNet.btnSave || BX.CDialogNet.btnCancel - standard buttons
*/

BX.CDialogNet.prototype.SetButtons = function(a)
{
	if (BX.type.isString(a))
	{
		if (a.length > 0)
		{
			this.PARTS.BUTTONS_CONTAINER.innerHTML += a;

			var btns = this.PARTS.BUTTONS_CONTAINER.getElementsByTagName('INPUT');
			if (btns.length > 0)
			{
				this.PARAMS.buttons = [];
				for (var i = 0; i < btns.length; i++)
				{
					this.PARAMS.buttons.push(new BX.CWindowButton({btn: btns[i], parentWindow: this}));
				}
			}
		}
	}
	else
	{
		this.PARAMS.buttons = a;
		BX.adjust(this.PARTS.BUTTONS_CONTAINER, {
			children: this.ShowButtons()
		});
	}
	this.adjustSize();
}

BX.CDialogNet.prototype.ClearButtons = function()
{
	BX.cleanNode(this.PARTS.BUTTONS_CONTAINER);
	this.adjustSize();
}

BX.CDialogNet.prototype.SetContent = function(html) {
	this.__form = null;

	if (BX.type.isElementNode(html)) {
		if (html.parentNode)
			html.parentNode.removeChild(html);
	}

	this.PARAMS.content = html;
	BX.cleanNode(this.PARTS.CONTENT_DATA);

	BX.adjust(this.PARTS.CONTENT_DATA, {
		children: [
			this.PARTS.HEAD,
			BX.create('DIV', {
				props: { className: 'content-inner' },
				children: [this.PARAMS.content || '&nbsp;']
			})
		]
	});
}

BX.CDialogNet.prototype.SwapContent = function(cont)
{
	cont = BX(cont);

	BX.cleanNode(this.PARTS.CONTENT_DATA);
	cont.parentNode.removeChild(cont);
	this.PARTS.CONTENT_DATA.appendChild(cont);
	cont.style.display = 'block';
	this.SetContent(cont.innerHTML);
}

BX.CDialogNet.prototype.adjustSize = function()
{
	return;
	setTimeout(BX.delegate(this.__adjustSize, this), 10);
}

BX.CDialogNet.prototype.__adjustSize = function() {
	BX.onCustomEvent(this, 'onWindowResizeExt', [{ 'width': parseInt(this.PARTS.CONTENT_DATA.style.width), 'height': parseInt(this.PARTS.CONTENT_DATA.style.height) }]);	
}

BX.CDialogNet.prototype.adjustSizeEx = function()
{
	setTimeout(BX.delegate(this.__adjustSizeEx, this), 10);
}

BX.CDialogNet.prototype.__adjustSizeEx = function()
{
	var new_height = this.PARTS.CONTENT_DATA.firstChild.offsetHeight + this.PARTS.CONTENT_DATA.lastChild.offsetHeight;

	if (new_height)
		this.PARTS.CONTENT_DATA.style.height = new_height + 'px';

	return;
	var arMargins = [10, parseInt(BX.style(this.PARTS.CONTENT, 'top')), parseInt(BX.style(this.PARTS.CONTENT.firstChild, 'margin-top')), parseInt(BX.style(this.PARTS.CONTENT.firstChild, 'margin-bottom'))];
	if (BX.browser.IsIE()) arMargins[0] += 5;
	var margins = 0;
	for (var i=0; i < arMargins.length; i++)
		if (!isNaN(arMargins[i]))
			margins += arMargins[i];

	var height = this.PARTS.CONTENT.firstChild.offsetHeight
		+ margins
		+ this.PARTS.TITLEBAR.offsetHeight
		+ (this.PARTS.FOOT ? this.PARTS.FOOT.offsetHeight : 0)
		+ (this.PARTS.HEAD ? this.PARTS.HEAD.offsetHeight : 0);

	this.DIV.style.height = height + 'px';
	this.adjustSize();
}


BX.CDialogNet.prototype.__onResizeFinished = function()
{
	BX.WindowManager.saveWindowSize(
		this.PARAMS.resize_id || this.PARAMS.content_url, {height: parseInt(this.PARTS.CONTENT_DATA.style.height), width: parseInt(this.PARTS.CONTENT_DATA.style.width)}
	);
}

BX.CDialogNet.prototype.Show = function (bNotRegister) {
	if ((!this.PARAMS.content) && this.PARAMS.content_url && BX.ajax && !bNotRegister) {
		var wait = BX.showWait();

		BX.WindowManager.currently_loaded = this;

		this.CreateOverlay(parseInt(BX.style(wait, 'z-index')) - 1);
		this.OVERLAY.style.display = 'block';
		this.OVERLAY.className = 'bx-core-dialog-overlay';
		BX.ajax({ 'method': 'GET', 'processData': false, 'scriptsRunFirst': true, 'emulateOnload': false, 'url': this.PARAMS.content_url, 'data': {}, 'onsuccess': function (data) {
			BX.closeWait(null, wait);
			BX.CDialogNet.__onDataReady(data);
		}
		});
	}
	else {

		BX.WindowManager.currently_loaded = null;
		BX.CDialogNet.superclass.Show.apply(this, arguments);

		this.adjustPos();

		this.OVERLAY.className = 'bx-core-dialog-overlay';

		this.__adjustSize();

		BX.addCustomEvent(this, 'onWindowResize', BX.proxy(this.__adjustSize, this))

		if (this.PARAMS.resizable && (this.PARAMS.content_url || this.PARAMS.resize_id))
			BX.addCustomEvent(this, 'onWindowResizeFinished', BX.delegate(this.__onResizeFinished, this));
	}

	BX.addCustomEvent(this, 'onWindowClose', BX.delegate(this.__onClose, this));
}

BX.CDialogNet.__onDataReady = function (data) {
	if (!BX.type.isString(data)) return;

	var dlg = BX.WindowManager.Get();
	if (!dlg) return;

	var scriptsAllRoughly = /<script[^>]*>[\S\s]*?<\/script>/gi,
		scriptsRemoteFine = /<script[^>]*src\s*=\s*[\'\"]([^\"\']+)[\'\"][^>]*>/i,
		scriptsLocalFine = /<script[^>]*>\s*(\/\/<\!\[CDATA\[)?\s*([\S\s]+?)\s*(\/\/\]\]>)?\s*<\/script>/i;

	var m = null, cap = null, runFirst = false, cnt = null;
	var count = (m = data.match(scriptsAllRoughly)) != null ? m.length : 0;
	if (count == 0) return;

	var scripts = [];

	for (var i = 0; i < count; i++) {
		cap = m[i];
		runFirst = cap.indexOf('bxrunfirst') != '-1';

		if ((cnt = scriptsRemoteFine.exec(cap)) != null) {
			scripts[scripts.length] = { "bxrunfirst": runFirst, "src": cnt[1] };
			continue;
		}
		if ((cnt = scriptsLocalFine.exec(cap)) != null) {
			scripts[scripts.length] = { "bxrunfirst": runFirst, "text": cnt[2] };
			continue;
		}
	}

	if (count > 0)
		BX.CDialogNet.__evalScripts(scripts, true);

	var show = typeof (window.bitrixDialogData) == "object" && window.bitrixDialogData != null && !window.bitrixDialogData.close;

	if (show)
		dlg.__constructFromObject(window.bitrixDialogData);

	if (count > 0)
		BX.CDialogNet.__evalScripts(scripts, false);

	window.bitrixDialogData = null;

	if (typeof (Bitrix.windowManagerCallback) == 'function') {
		var callback = Bitrix.windowManagerCallback;
		Bitrix.windowManagerCallback = undefined;
		callback();
	}

	if (!show) BX.WindowManager.Get().Close(true);
}

BX.CDialogNet.prototype.__constructFromObject = function(obj) {
	if (typeof (obj.sections) == "undefined" || !obj.sections) return;
	
	this.SetTitle(typeof (obj.sections.title) != "undefined" && BX.type.isString(obj.sections.title.innerHTML) ? obj.sections.title.innerHTML : "Untitled");
	this.SetHead(typeof (obj.sections.description) != "undefined" && BX.type.isString(obj.sections.description.innerHTML) ? obj.sections.description.innerHTML : "");
	
	if(typeof (obj.sections.icon) != "undefined" && BX.type.isNotEmptyString(obj.sections.icon.innerHTML))
		this.SetIcon(obj.sections.icon.innerHTML);
	
	if(typeof (obj.sections.iconFile) != "undefined" && BX.type.isNotEmptyString(obj.sections.iconFile.innerHTML))
		this.SetIconFile(obj.sections.iconFile.innerHTML);

	var content = "&nbsp;";
	if (typeof (obj.sections.content) != "undefined" && BX.type.isNotEmptyString(obj.sections.content.innerHTML)) {
		content = obj.sections.content.innerHTML;
		content = content.replace(/\s*<title[^>]*>[\s\S]*<\/title>/gi, "");
	}

	this.SetContent(content);
	
	if (typeof (obj.sections.buttonPanel) != "undefined" && BX.type.isNotEmptyString(obj.sections.buttonPanel.innerHTML)) {
		this.ClearButtons();
		this.SetButtons(obj.sections.buttonPanel.innerHTML);
	}

	if (typeof (obj.scripts) != "undefined" && BX.type.isArray(obj.scripts))
		BX.CDialogNet.__evalScripts(obj.scripts, true);

	if (typeof (obj.externalStylesheets) != "undefined" && BX.type.isArray(obj.externalStylesheets))
		BX.loadCSS(obj.externalStylesheets);

	if (typeof (obj.settings) != "undefined") {
		var settings = obj.settings;
		if (settings.width)
			this.PARAMS.width = settings.width > 0 ? settings.width : this.defaultParams.width;
		if (settings.height)
			this.PARAMS.height = settings.height > 0 ? settings.height : this.defaultParams.height;
		if (settings.resizable)
			this.SETTINGS.resizable = this.PARAMS.resizable = settings.resizable;
		if (settings.min_width)
			this.SETTINGS.min_width = this.PARAMS.min_width = settings.min_width > 0 ? settings.min_width : this.defaultParams.min_width;
		if (settings.min_height)
			this.SETTINGS.min_height = this.PARAMS.min_height = settings.min_height > 0 ? settings.min_height : this.defaultParams.min_height;

		BX.adjust(this.PARTS.CONTENT_DATA, {
			style: {
				height: this.PARAMS.height + 'px',
				width: this.PARAMS.width + 'px'
			}
		});
	}

	var frame = null;
	if (typeof (obj.sections.frame) != "undefined" && typeof (obj.sections.frame.innerHTML) != "undefined")
		frame = obj.sections.frame.innerHTML;

	if (BX.type.isNotEmptyString(frame)) {
		var frameDiv = document.getElementById("bx_popup_frame");
		if (frameDiv == null) {
			frameDiv = document.body.appendChild(document.createElement("DIV"));
			frameDiv.id = "bx_popup_frame";
			frameDiv.style.display = "none";
			frameDiv.style.width = "0";
			frameDiv.style.height = "0";
		}
		frameDiv.innerHTML = frame;
	}

	
	if(BX.type.isBoolean(this.PARAMS.adminStyle) && this.PARAMS.adminStyle) {
		this.PARTS.HEAD.className = 'bx-core-admin-dialog-head';
		this.PARTS.CONTENT.className += ' bx-core-admin-dialog-content';
	}
	
	this.Show();

	if (typeof (obj.scripts) != "undefined" && BX.type.isArray(obj.scripts))
		BX.CDialogNet.__evalScripts(obj.scripts, false);
}

BX.CDialogNet.prototype.__onClose = function(data) 
{
	if (typeof(Bitrix.AspnetFormDispatcher) != "undefined") 
		Bitrix.AspnetFormDispatcher.get_instance().handlePopupDialogClose();
		
	if(this.OVERLAY) //hack
		this.OVERLAY.style.display = 'none';
}

BX.CDialogNet.__evalScripts = function(scripts, runFirst) {
	if (!BX.type.isArray(scripts)) return;
	
	for (var i = 0; i < scripts.length; i++) {
		var script = scripts[i];
		var scriptRunFirst = false;
		if (typeof (script.bxrunfirst) != "undefined") {
			if (BX.type.isBoolean(script.bxrunfirst))
				scriptRunFirst = script.bxrunfirst;
			else if (BX.type.isString(script.bxrunfirst)) {
				var s = script.bxrunfirst.toUpperCase();
				scriptRunFirst = s == "TRUE" || s == "BXRUNFIRST";
			}
			else if (BX.type.isNumber(script.bxrunfirst))
				scriptRunFirst = script.bxrunfirst > 0;
		}

		if (runFirst != scriptRunFirst)
			continue;

		if ("text" in script)
			BX.CDialogNet.__evalScript(script.text);
		else if ("src" in script) {
			BX.ajax({
				url: script.src,
				method: 'GET',
				dataType: 'script',
				processData: false,
				async: false,
				start: true,
				onsuccess: function(result) {
					BX.CDialogNet.__evalScript(result);
				}
			});		
		}
	}
}

BX.CDialogNet.__evalScript = function(script) {
	if (!BX.type.isNotEmptyString(script)) return;

	if (typeof (Bitrix.AspnetFormDispatcher) != "undefined") {
		var args = Bitrix.PopupDialogScriptPreEvaluteArgs.create(script);
		Bitrix.AspnetFormDispatcher.get_instance().handlePopupDialogScriptPreEvalute(args);

		if (args.getCancel()) return;

		if (args.isChanged())
			script = args.getContent();
	}

	if (BX.type.isNotEmptyString(script))
		BX.evalGlobal(script);
}

BX.CDialogNet.prototype.GetInnerPos = function()
{
	return {'width': parseInt(this.PARTS.CONTENT_DATA.style.width), 'height': parseInt(this.PARTS.CONTENT_DATA.style.height)};
}

BX.CDialogNet.prototype.adjustPos = function()
{
	if (!this.bExpanded)
	{
		var windowSize = BX.GetWindowInnerSize();
		var windowScroll = BX.GetWindowScrollPos();

		BX.adjust(this.DIV, {
			style: {
				left: parseInt(windowScroll.scrollLeft + windowSize.innerWidth / 2 - parseInt(this.DIV.offsetWidth) / 2) + 'px',
				top: Math.max(parseInt(windowScroll.scrollTop + windowSize.innerHeight / 2 - parseInt(this.DIV.offsetHeight) / 2), 0) + 'px'
			}
		});
	}
}

BX.CDialogNet.prototype.GetContent = function () {return this.PARTS.CONTENT_DATA};

BX.CDialogNet.prototype.btnSave = BX.CDialogNet.btnSave = {
	title: BX.message('JS_CORE_WINDOW_SAVE'),
	id: 'savebtn',
	name: 'savebtn',
	action: function() {
		this.disableUntilError();
		this.parentWindow.Close();
	}
};

BX.CDialogNet.prototype.btnCancel = BX.CDialogNet.btnCancel = {
	title: BX.message('JS_CORE_WINDOW_CANCEL'),
	id: 'cancel',
	name: 'cancel',
	action: function () {
		this.parentWindow.Close();
	}
};

BX.CDialogNet.prototype.btnClose = BX.CDialogNet.btnClose = {
	title: BX.message('JS_CORE_WINDOW_CLOSE'),
	id: 'close',
	name: 'close',
	action: function () {
		this.parentWindow.Close();
	}
};

BX.CDialogNet.handleIframeLoad = function() {
	var frame = document.getElementById('bx_dialog_form_target');
	if (!frame) return;

	if (/*@cc_on!@*/true && frame.removeEventListener)
		frame.removeEventListener("load", BX.CDialogNet.handleIframeLoad, false);

	var frameDoc = typeof (frame.contentDocument) != 'undefined' ? frame.contentDocument : frame.contentWindow.document;
	if (!frameDoc) return;
	BX.CDialogNet.__onDataReady(frameDoc.documentElement.innerHTML);
};

BX.undo = function(url){		
	BX.ajax({ 'method':'GET',  'dataType':'SCRIPT', 'processData':true, 'scriptsRunFirst': true, 'emulateOnload': false, 'url': url });		
}

BX.displayIncludeAreas = function() { 
	var el = BX("bitrix_include_areas"); 
	return el && el.value == "Y";
}
