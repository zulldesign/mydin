if (typeof (Bitrix) == "undefined") {
    var Bitrix = new Object();
}
Bitrix.LinkPasteDialogLayout = { full: 1, urlOnly: 2 }
Bitrix.LinkPasteDialog = function Bitrix$LinkPasteDialog() {
    this.Bitrix$Dialog();
    this._urlInputElement = null;
    this._textInputElement = null;
    this._textContainerElement = null;
    this._closeHandler = Bitrix.TypeUtility.createDelegate(this, this._handleSelfClose);
}
Bitrix.TypeUtility.copyPrototype(Bitrix.LinkPasteDialog, Bitrix.Dialog);
Bitrix.LinkPasteDialog.prototype.initialize = function(id, name, title, options) {
    this.Bitrix$Dialog.prototype.initialize.call(this, id, name, title, null, Bitrix.Dialog.buttonLayout.cancelOk, options);
    var content = new Array();

    var container = document.createElement("DIV");
    container.id = id + "_" + "container";
    container.className = this.getOption("_contentContainerClass");
    content.push(container);

    var table = document.createElement("TABLE");
    table.className = this.getOption("_contentContainerTableClass");
    table.border = 0;
    table.cellPadding = 0;
    table.cellSpacing = 0;
    container.appendChild(table);

    var urlR = table.insertRow(-1);
    var urlLabelC = urlR.insertCell(-1);
    var urlLabelCnr = document.createElement("DIV");
    urlLabelCnr.className = this.getOption("_labelContainerClass");
    urlLabelC.appendChild(urlLabelCnr);
    var urlLabel = document.createElement("LABEL");
    urlLabel.className = this.getOption("_labelClass");
    urlLabel.htmlFor = this._getUrlInputId();
    urlLabel.innerHTML = this.getMessage("urlLabel") + ":";
    urlLabelCnr.appendChild(urlLabel);

    var urlC = urlR.insertCell(-1);
    var urlCnr = document.createElement("DIV");
    urlCnr.className = this.getOption("_inputContainerClass");
    urlC.appendChild(urlCnr);
    var url = this._urlInputElement = document.createElement("INPUT");
    url.className = this.getOption("_inputTextClass");
    url.type = "text";
    url.id = this._getUrlInputId();
    var urlStr = this.getOption("_url", "");
    url.value = Bitrix.TypeUtility.isNotEmptyString(urlStr) ? urlStr : this.getDefaultUrl();
    urlCnr.appendChild(url);

    var urlLegendR = table.insertRow(-1);
    var urlLegendC = urlLegendR.insertCell(-1);
    urlLegendC.colSpan = 2;
    var urlLegend = document.createElement("SPAN");
    urlLegend.className = this.getOption("_legendClass");
    urlLegend.innerHTML = this.getMessage("urlLegend");
    urlLegendC.appendChild(urlLegend);

    if (this.getLinkPasteDialogLayout() == Bitrix.LinkPasteDialogLayout.full) {
        var textR = this._textContainerElement = table.insertRow(-1);
        textR.id = this._getTextContainerId();
        var textLabelC = textR.insertCell(-1);
        var textLabelCnr = document.createElement("DIV");
        textLabelCnr.className = this.getOption("_labelContainerClass");
        textLabelC.appendChild(textLabelCnr);
        var textLabel = document.createElement("LABEL");
        textLabel.className = this.getOption("_labelClass");
        textLabel.htmlFor = this._getTextInputId();
        textLabel.innerHTML = this.getMessage("textLabel") + ":";
        textLabelCnr.appendChild(textLabel);

        var textC = textR.insertCell(-1);
        var textCnr = document.createElement("DIV");
        textCnr.className = this.getOption("_inputContainerClass");
        textC.appendChild(textCnr);
        var text = this._textInputElement = document.createElement("INPUT");
        text.className = this.getOption("_inputTextClass");
        text.type = "text";
        text.id = this._getTextInputId();
        var textStr = this.getOption("_text", "");
        text.value = Bitrix.TypeUtility.isNotEmptyString(textStr) ? textStr : this.getDefaultText();
        textCnr.appendChild(text);
    }
    this.setContent(content);
}
Bitrix.LinkPasteDialog.prototype._getChildElementId = function(parentId, id){ return (parentId ? parentId : this.getId()) + "_" + id; }
Bitrix.LinkPasteDialog.prototype._getUrlInputId = function(){ return this._getChildElementId(null, "UrlInput"); }
Bitrix.LinkPasteDialog.prototype._getTextInputId = function() { return this._getChildElementId(null, "TextInput"); }
Bitrix.LinkPasteDialog.prototype._getTextContainerId = function() { return this._getChildElementId(null, "TextContainer"); }
Bitrix.LinkPasteDialog.prototype._getAdditionalContainerWrapperClasses = function(){ return this.getOption("_containerWrapperClass"); }
Bitrix.LinkPasteDialog.prototype.getOption = function(name, defaultValue) {
	if(!Bitrix.TypeUtility.isNotEmptyString(name)) return defaultValue;
	if (this._options && name in this._options) return this._options[name];
	else if (name in Bitrix.LinkPasteDialog.defaults) return Bitrix.LinkPasteDialog.defaults[name];	
	else if (name in Bitrix.Dialog.defaults) return Bitrix.Dialog.defaults[name];
	else return defaultValue;
}

Bitrix.LinkPasteDialog.prototype._handleSetOptions = function() {
	if(this._urlInputElement && this.getOption("_url")) this._urlInputElement.value = this.getOption("_url");
	if(this._textInputElement && this.getOption("_text")) this._textInputElement.value = this.getOption("_text");
}
Bitrix.LinkPasteDialog.prototype._getMessageKeyPrefix = function(){ return "LinkPasteDialog$msg$"; }
Bitrix.LinkPasteDialog.prototype._getMessageContainerName = function(){ return "COMMUNICATION_UTILITY_DIALOG_MSG"; }
Bitrix.LinkPasteDialog.prototype.getUrl = function(){ return this._urlInputElement ? this._urlInputElement.value : this.getOption("_url", "");  }
Bitrix.LinkPasteDialog.prototype.setUrl = function(url){ this.setOption("_url", url); if(this._urlInputElement)this._urlInputElement.value = url; }
Bitrix.LinkPasteDialog.prototype.getDefaultUrl = function(){ return this.getOption("_defaultUrl", "http://"); }
Bitrix.LinkPasteDialog.prototype.setDefaultUrl = function(url){ this.setOption("_defaultUrl", url); }

Bitrix.LinkPasteDialog.prototype.getText = function(){ return this._textInputElement ? this._textInputElement.value : this.getOption("_text", "");  }
Bitrix.LinkPasteDialog.prototype.setText = function(text){ this.setOption("_text", text); if(this._textInputElement)this._textInputElement.value = text; }
Bitrix.LinkPasteDialog.prototype.getDefaultText = function(){ return this.getOption("_defaultText", "");  }
Bitrix.LinkPasteDialog.prototype.setDefaultText = function(text){ this.setOption("_defaultText", text); }
Bitrix.LinkPasteDialog.prototype.getLinkPasteDialogLayout = function(){ return this.getOption("linkPasteDialogLayout", Bitrix.LinkPasteDialogLayout.full); }
Bitrix.LinkPasteDialog.prototype.setLinkPasteDialogLayout = function(layout) {
    this.setOption("linkPasteDialogLayout", layout);
    if (this._textContainerElement) this._textContainerElement.style.display = layout == Bitrix.LinkPasteDialogLayout.full ? "" : "none";
}
Bitrix.LinkPasteDialog.prototype._handleSelfClose = function(e)
{
	if(this._urlInputElement) this.setOption("_url", this._urlInputElement.value);
	if(this._textInputElement) this.setOption("_text", this._textInputElement.value);
}
Bitrix.LinkPasteDialog.create = function(id, name, title, options){
	if(this._items && id in this._items) throw "Item '"+ id +"' already exists!";
	var self = new Bitrix.LinkPasteDialog();
	self.initialize(id, name, title, options);
	Bitrix.Dialog._addItem(self);
	return self;
}

Bitrix.LinkPasteDialog.defaults = {
	_containerWrapperClass:"bx-link-paste-dialog-container-wrapper",
	_contentContainerClass:"bx-link-paste-dialog-content-container",
	_labelContainerClass: "bx-link-paste-dialog-label-container",
	_labelClass:"bx-link-paste-dialog-label",
	_legendClass:"bx-link-paste-dialog-legend",
	_inputContainerClass: "bx-link-paste-dialog-input-container",
	_inputTextClass:"bx-link-paste-dialog-input-text",
	_contentContainerTableClass:"bx-link-paste-content-container-table"
}