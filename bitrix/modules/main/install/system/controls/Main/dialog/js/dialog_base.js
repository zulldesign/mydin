if(typeof(Bitrix) == "undefined"){
	var Bitrix = new Object();
}
Bitrix.Dialog = function Bitrix$Dialog()
{
	this._initialized = false;
	this._id = "";
	this._name = "";
	this._isModal = false;
	this._isOpened = false;
	this._isConstructed = false;
	this._containerEl = null;
	this._errorContainerEl = null;
	this._errorCntEl = null;
	this._titleEl = "";
	this._width = 400;
	this._height = 200;
	this._posX = 0;
	this._posY = 0;
	this._title = "";
	this._content = "";
	this._buttonLayout = Bitrix.Dialog.buttonLayout.cancelOk;
	this._cancelHandler = Bitrix.TypeUtility.createDelegate(this, this._handleCancel);
	this._okHandler = Bitrix.TypeUtility.createDelegate(this, this._handleOk);
	this._yesHandler = Bitrix.TypeUtility.createDelegate(this, this._handleYes);
	this._noHandler = Bitrix.TypeUtility.createDelegate(this, this._handleNo);
	this._continueHandler = Bitrix.TypeUtility.createDelegate(this, this._handleContinue);
	this._mouseDownHandler = Bitrix.TypeUtility.createDelegate(this, this._handleMouseDown);
	this._mouseMoveHandler = Bitrix.TypeUtility.createDelegate(this, this._handleMouseMove);
	this._mouseUpHandler = Bitrix.TypeUtility.createDelegate(this, this._handleMouseUp);
	this._mouseOutHandler = Bitrix.TypeUtility.createDelegate(this, this._handleMouseOut);
	this._options = null;
	this._closeEvent = null;
	this._titleBarContainer = null;
	this._isInDraggingMode = false;
	this._mouseStartDraggingPos = null;
	this._isAtUserPosition = false;
}

Bitrix.Dialog.prototype = {
    initialize: function(id, name, title, content, buttonLayout, options) {
        if (this._initialized) return;
        this._id = id;
        this._name = name;
        this.setTitle(title);
        this._content = content;
        if (buttonLayout || buttonLayout == Bitrix.Dialog.buttonLayout.none)
            this._buttonLayout = buttonLayout;
        this._options = options;
        this._initialized = true;
    },
    getId: function() { return this._id; },
    getName: function() { return this._name; },
    getTitle: function() { return this._title; },
    setTitle: function(title) { 
		this._title = Bitrix.TypeUtility.isNotEmptyString(title) ? title : this.getDefaultTitle(); 
		if(this._titleEl) this._titleEl.innerHTML = this._title;
	},
    getDefaultTitle: function() { return "<Untitled>"; },
    getContent: function() { return this._content; },
    setContent: function(content) { this._content = content; },
    getButtonLayout: function() { return this._buttonLayout; },
    setButtonLayout: function(layout) { this._buttonLayout = layout; },
    getOptions: function() { return this._options ? this._options : (this._options = new Object()); },
    getOption: function(name, defaultValue) {
        if (this._options && name in this._options) return this._options[name];
        else if (name in Bitrix.Dialog.defaults) return Bitrix.Dialog.defaults[name];
        else return defaultValue;
    },
    setOptions: function(options) { this._options = options; this._handleSetOptions(); },
    _handleSetOptions: function() { },
    setOption: function(name, value) { if (!Bitrix.TypeUtility.isNotEmptyString(name)) return; var opts = this.getOptions(); opts[name] = value; },
    _getMessageKeyPrefix: function() { return ""; },
    _getMessageContainerName: function() { return "DIALOG_MSG"; },
    getMessage: function(key) {
        var c = null, name = this._getMessageContainerName();
        if (Bitrix.TypeUtility.isNotEmptyString(name)) c = typeof (window[name]) != "undefined" ? window[name] : null;
        if (!c) return "";
        var fullKey = this._getMessageKeyPrefix() + key;
        return fullKey in c ? c[fullKey] : "";
    },
    isModal: function() { return this._isModal; },
    isOpened: function() { return this._isOpened; },
    isConstructed: function() { return this._isConstructed; },
    getWidth: function() { return this._width; },
    setWidth: function(width) { this._width = width; },
    getHeight: function() { return this._height; },
    setHeight: function(height) { this._height = height; },
    getPosX: function() { return this._posX; },
    setPosX: function(posX) { this._posX = posX; },
    getPosY: function() { return this._posY; },
    setPosY: function(posY) { this._posY = posY; },
    addCloseListener: function(listener) {
        (this._closeEvent ? this._closeEvent : (this._closeEvent = new Bitrix.EventPublisher())).addListener(listener);
    },
    removeCloseListener: function(listener) {
        if (this._closeEvent) this._closeEvent.removeListener(listener);
    },
    _handleClose: function(buttonId) {
        if (!this._closeEvent) return;
        var args = new Object();
        args["buttonId"] = buttonId;
        try { this._closeEvent.fire(this, args); } catch (e) { }
    },
    _handleCancel: function() {
        this._handleClose(Bitrix.Dialog.button.bCancel);
        this.close();
    },
    _handleOk: function() {
        this._handleClose(Bitrix.Dialog.button.bOk);
        this.close();
    },
    _handleYes: function() {
        this._handleClose(Bitrix.Dialog.button.bYes);
        this.close();
    },
    _handleNo: function() {
        this._handleClose(Bitrix.Dialog.button.bNo);
        this.close();
    },
    _handleContinue: function() {
        this._handleClose(Bitrix.Dialog.button.bContinue);
        this.close();
    },
    construct: function() {
        if (this._isConstructed) return;
        if (!this._initialized) throw "Can't be constructed - is not initialized!";

        var containerWrapper = this._containerEl = document.createElement("DIV");
        containerWrapper.className = this.getOption("containerWrapperClass");
        var addContainerWrapperClasses = this._getAdditionalContainerWrapperClasses();
        if (Bitrix.TypeUtility.isNotEmptyString(addContainerWrapperClasses))
            containerWrapper.className += " " + addContainerWrapperClasses;
        containerWrapper.style.display = "none";
        document.body.appendChild(containerWrapper);
        var container = document.createElement("DIV");
        containerWrapper.appendChild(container);
        container.className = this.getOption("containerClass");

        if (this._width)
            containerWrapper.style.width = this._width;

        var titleBarContainer = this._titleBarContainer = document.createElement("DIV");
        container.appendChild(titleBarContainer);
        titleBarContainer.className = "bx-dialog-titlebar";
        var title = this._titleEl = document.createElement("DIV");
        titleBarContainer.appendChild(title);
        title.className = "bx-dialog-title-text";
        title.innerHTML = this._title;
        Bitrix.TypeUtility.disableSelection(title);
        closeBtnContainer = document.createElement("DIV");
        closeBtnContainer.className = "bx-dialog-title-close-container";
        titleBarContainer.appendChild(closeBtnContainer);
        var closeBtn = document.createElement("DIV");
        closeBtnContainer.appendChild(closeBtn);
        closeBtn.className = "bx-dialog-title-close";
        Bitrix.EventUtility.addEventListener(closeBtnContainer, "click", this._cancelHandler);
        Bitrix.EventUtility.addEventListener(this._titleBarContainer, "mousedown", this._mouseDownHandler);
        //Bitrix.EventUtility.addEventListener(this._titleBarContainer, "mouseout", this._mouseOutHandler);

        var errorContainer = this._errorContainerEl = document.createElement("DIV");
        errorContainer.className = this.getOption("errorContainerClass");
        errorContainer.style.display = "none";
        container.appendChild(errorContainer);
        var errorCnt = this._errorCntEl = document.createElement("DIV");
        errorCnt.className = this.getOption("errorContentClass");
        errorCnt.style.display = "none";
        errorContainer.appendChild(errorCnt);

        var contentContainer = document.createElement("DIV");
        container.appendChild(contentContainer);
        contentContainer.className = this.getOption("contentContainerClass");
        if (this._content) {
            if (Bitrix.TypeUtility.isString(this._content))
                contentContainer.innerHTML = this._content;
            else if (typeof (this._content) == "object")
                for (var contentKey in this._content) {
                var contentProp = this._content[contentKey];
                if (Bitrix.TypeUtility.isDomNode(contentProp))
                    contentContainer.appendChild(contentProp);
            }
        }
        var btnPane = document.createElement("DIV");
        container.appendChild(btnPane);
        btnPane.className = this.getOption("buttonPanelClass");
        var btnContainer = document.createElement("DIV");
        btnPane.appendChild(btnContainer);
        btnContainer.className = this.getOption("panelContainer");

        var btnWrapper = document.createElement("DIV");
        btnContainer.appendChild(btnWrapper);
        btnWrapper.className = this.getOption("paneButtonWrapperClass");
        
        switch (this._buttonLayout) {
            case Bitrix.Dialog.buttonLayout.cancelOk:
            	{
            		btnWrapper.appendChild(this._createPaneButton(Bitrix.Dialog.buttonText.txtOk, this._okHandler));					
                	btnWrapper.appendChild(this._createPaneButton(Bitrix.Dialog.buttonText.txtCancel, this._cancelHandler));
                }
                break;
            case Bitrix.Dialog.buttonLayout.yesNo:
                {
                	btnWrapper.appendChild(this._createPaneButton(Bitrix.Dialog.buttonText.txtYes, this._yesHandler));
                	btnWrapper.appendChild(this._createPaneButton(Bitrix.Dialog.buttonText.txtNo, this._noHandler));
                }
                break;
            case Bitrix.Dialog.buttonLayout.continueLayout:
                {
                	btnWrapper.appendChild(this._createPaneButton(Bitrix.Dialog.buttonText.txtContinue, this._continueHandler));
                }
                break;
        }
        var stubBtnPane = document.createElement("DIV");
        btnWrapper.appendChild(stubBtnPane);
        stubBtnPane.className = "bx-dialog-clear";
        this._isConstructed = true;
    },
    _getAdditionalContainerWrapperClasses: function() { return ""; },
    _createPaneButton: function(caption, handler) {
        var b = document.createElement("BUTTON");
        b.className = this.getOption("paneButtonClass");
        b.setAttribute("type", "button");
        b.innerHTML = caption;
        if (handler)
            Bitrix.EventUtility.addEventListener(b, "click", handler);
        return b;
    },
    showError: function(errorTxt) {
        if (!this._errorCntEl || !this._errorContainerEl)
            return;

        if (!Bitrix.TypeUtility.isNotEmptyString(errorTxt)) {
            this.hideError();
            return;
        }
        this._errorContainerEl.style.display = "block";
        this._errorCntEl.innerHTML = errorTxt;
        this._errorCntEl.style.display = "block";
    },
    hideError: function() {
        if (!this._errorCntEl || !this._errorContainerEl)
            return;
        this._errorCntEl.innerHTML = "";

        this._errorContainerEl.style.display = "none";
        this._errorCntEl.style.display = "none";
    },
    _handleMouseDown: function(e) {
        if (!e) e = window.event;
        mousePos = new Object();
        mousePos.top = e.clientY + document.body.scrollTop;
        mousePos.left = e.clientX + document.body.scrollLeft;
        this._startDragging(mousePos);
    },
    _handleMouseMove: function(e) {
        if (!e) e = window.event;
        mousePos = new Object();
        mousePos.top = e.clientY + document.body.scrollTop;
        mousePos.left = e.clientX + document.body.scrollLeft;
        this._drag(mousePos);
    },
    _handleMouseUp: function(e) {
        this._stopDragging();
    },
    _handleMouseOut: function(e) {
        this._stopDragging();
    },
    _startDragging: function(mousePos) {
        if (!(this._containerEl && this._titleBarContainer)) return;
        this._isInDraggingMode = true;

        this._mouseStartDraggingPos = mousePos;
        Bitrix.EventUtility.addEventListener(document.body, "mousemove", this._mouseMoveHandler);
        Bitrix.EventUtility.addEventListener(document.body, "mouseup", this._mouseUpHandler);
    },
    _drag: function(mousePos) {
        if (!(this._containerEl && this._titleBarContainer) || !this._isInDraggingMode || !this._mouseStartDraggingPos) return;

        var shiftY = mousePos.top - this._mouseStartDraggingPos.top;
        var shiftX = mousePos.left - this._mouseStartDraggingPos.left;
        if (Math.abs(shiftY) < 3 && Math.abs(shiftX) < 3) return;

        var curLeft = parseInt(this._containerEl.style.left);
        var curTop = parseInt(this._containerEl.style.top);
        if (isNaN(curLeft) || isNaN(curTop)) {
            var r = Bitrix.ElementPositioningUtility.getElementRect(this._containerEl);
            curLeft = r.left;
            curTop = r.top;
        }

        var left = curLeft + shiftX;
        var top = curTop + shiftY;

        this._containerEl.style.left = left + 'px';
        this._containerEl.style.top = top + 'px';

        delete this._mouseStartDraggingPos;
        this._mouseStartDraggingPos = mousePos;
        if (!this._isAtUserPosition) this._isAtUserPosition = true;
    },
    _stopDragging: function() {
        if (!(this._containerEl && this._titleBarContainer)) return;
        this._isInDraggingMode = false;
        //Bitrix.EventUtility.removeEventListener(this._titleBarContainer, "mousemove", this._mouseMoveHandler);
        //Bitrix.EventUtility.removeEventListener(this._titleBarContainer, "mouseup", this._mouseUpHandler);	
        Bitrix.EventUtility.removeEventListener(document.body, "mousemove", this._mouseMoveHandler);
        Bitrix.EventUtility.removeEventListener(document.body, "mouseup", this._mouseUpHandler);
        delete this._mouseStartDraggingPos;
        this._mouseStartDraggingPos = null;
    },
    open: function() {
        if (this._isOpened) return;
        this.construct();
        this._containerEl.style.display = "";
        if (!this._isAtUserPosition) {
            var rDoc = Bitrix.ElementPositioningUtility.getElementRect(Bitrix.ElementPositioningUtility.getViewPortElement());
            var rContainer = Bitrix.ElementPositioningUtility.getElementRect(this._containerEl);
            var windowScroll = Bitrix.ElementPositioningUtility.getWindowScrollPos();
            var top = parseInt(((rDoc.bottom - rDoc.top) - (rContainer.bottom - rContainer.top)) / 2) + windowScroll.scrollTop;
            var left = parseInt(((rDoc.right - rDoc.left) - (rContainer.right - rContainer.left)) / 2) + windowScroll.scrollLeft;
            this._containerEl.style.top = top + "px";
            this._containerEl.style.left = left + "px";
            var zIndex = this.getOption("zIndex", -1);
            if (zIndex >= 0) this._containerEl.style.zIndex = zIndex;
        }
        this._isOpened = true;
    },
    close: function(removeElement) {
        if (!this._isOpened) return;
        if (!(typeof (removeElement) == 'boolean' || removeElement instanceof Boolean)) removeElement = false;
        if (removeElement && this._containerEl) this._release();
        this._containerEl.style.display = "none";
        this._isOpened = false;
    },
	_release: function() {
        if (!this._containerEl) return;
		document.body.removeChild(this._containerEl);
		delete this._containerEl;
		this._containerEl = null;
		this._isConstructed = false;
	}
}
Bitrix.Dialog._items = null;
Bitrix.Dialog._addItem = function(item){
	if(!item || (this._items && item.getId() in this._items)) return false;
	if(!this._items) this._items = new Object();
	(this._items ? this._items : (this._items = new Object()))[item.getId()] = item;
	return true;
}
Bitrix.Dialog.create = function(id, name, title, content, buttonLayout, options){
	if(this._items && id in this._items) throw "Item '"+ id +"' already exists!";
	var self = new Bitrix.Dialog();
	self.initialize(id, name, title, content, buttonLayout, options);
	this._addItem(self);
	return self;
}
Bitrix.Dialog.remove = function(id){
	if(this._items && id in this._items){
		try {
			this._items[id]._release();
		}
		catch(e) {
		}
		delete this._items[id];
		return true;
	}
	return false;
}
Bitrix.Dialog.get = function(id){
	return this._items && id in this._items ? this._items[id] : null; 
}
Bitrix.Dialog.defaults = {
	containerWrapperClass: "bx-dialog-container-wrapper",
	containerClass: "bx-dialog-container",
	titleContainerClass: "bx-dialog-titlebar", 
	titleClass: "bx-dialog-title-text", 
	titleCloseButtonContainerClass: "bx-dialog-title-close-container", 
	titleCloseButtonClass: "bx-dialog-title-close",
	errorContainerClass: "bx-dialog-error-container",
	errorContentClass: "bx-dialog-error-content",
	contentContainerClass: "bx-dialog-content-container",
	buttonPanelClass: "bx-dialog-button-pane",
	paneButtonClass: "bx-dialog-button",
	panelContainer: "bx-dialog-button-container",
	paneButtonWrapperClass: "bx-dialog-button-wrapper"	
}
Bitrix.Dialog.button = { bCancel: 1, bOk: 2, bYes: 3, bNo: 4,  bContinue: 5}
Bitrix.Dialog.buttonLayout = { none: 0, cancelOk: 1, yesNo: 2, continueLayout: 3 }
Bitrix.Dialog.buttonText = { txtCancel: "Cancel", txtOk: "&nbsp;&nbsp;OK&nbsp;&nbsp;", txtYes: "Yes", txtNo: "No", txtContinue: "Continue" }

if (window.BITRIX_DIALOG_MSG)
{ 
	Bitrix.Dialog.buttonText.txtCancel = window.BITRIX_DIALOG_MSG["Button.Cancel"] || Bitrix.Dialog.buttonText.txtCancel;
	Bitrix.Dialog.buttonText.txtOk = window.BITRIX_DIALOG_MSG["Button.OK"] || Bitrix.Dialog.buttonText.txtOk;
	Bitrix.Dialog.buttonText.txtYes = window.BITRIX_DIALOG_MSG["Button.Yes"] || Bitrix.Dialog.buttonText.txtOk;
	Bitrix.Dialog.buttonText.txtNo = window.BITRIX_DIALOG_MSG["Button.No"] || Bitrix.Dialog.buttonText.txtNo;
	Bitrix.Dialog.buttonText.txtContinue = window.BITRIX_DIALOG_MSG["Button.Continue"] || Bitrix.Dialog.buttonText.txtContinue;
}
