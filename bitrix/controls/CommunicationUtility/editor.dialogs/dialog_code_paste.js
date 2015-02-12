if (typeof (Bitrix) == "undefined") {
    var Bitrix = new Object();
}
Bitrix.CodePasteDialog = function Bitrix$CodePasteDialog() {
    this.Bitrix$Dialog();
    this._langSelectElement = null;
    this._sourceTextInputElement = null;
    this._closeHandler = Bitrix.TypeUtility.createDelegate(this, this._handleSelfClose);
}
Bitrix.TypeUtility.copyPrototype(Bitrix.CodePasteDialog, Bitrix.Dialog);
Bitrix.CodePasteDialog.prototype.initialize = function(id, name, title, options) {
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
	
    var langR = table.insertRow(-1);
    var langLabelC = langR.insertCell(-1);
    var langLabelCnr = document.createElement("DIV");
    langLabelCnr.className = this.getOption("_labelContainerClass");
    langLabelC.appendChild(langLabelCnr);
    var langLabel = document.createElement("LABEL");
    langLabel.className = this.getOption("_labelClass");
    langLabel.htmlFor = this._getLangSelectId();
    langLabel.innerHTML = this.getMessage("langLabel") + ":";
    langLabelCnr.appendChild(langLabel);

    var langC = langR.insertCell(-1);
    var langCnr = document.createElement("DIV");
    langCnr.className = this.getOption("_selectContainerClass");
    langC.appendChild(langCnr);
    var lang = this._langSelectElement = document.createElement("SELECT");

    lang.className = this.getOption("_selectClass");
    lang.id = this._getLangSelectId();
    langCnr.appendChild(lang);	
	
	var srcTxtR = table.insertRow(-1);
	var srcTxtC = srcTxtR.insertCell(-1);	
	srcTxtC.colSpan = 2;
    var srcTxtCnr = document.createElement("DIV");
    srcTxtCnr.className = this.getOption("_textAreaContainerClass");
    srcTxtC.appendChild(srcTxtCnr);	
    var txt = this._sourceTextInputElement = document.createElement("TEXTAREA");
    txt.className = this.getOption("_textAreaClass");
    txt.id = this._getSourceTextInputId();	
	srcTxtC.appendChild(txt);
	
	this._settingsFromOptions();
	this.setContent(content);
}
Bitrix.CodePasteDialog.prototype._settingsFromOptions = function() {
	var lang = this._langSelectElement;
	if(lang) {
		lang.setAttribute("wrap", this.getOption("_textWrap", "soft"));
		var opts = this.getOption("_langOpts", null);
		if(opts) {
			while(lang.options.length > 0) lang.remove(0);
			var v = this.getOption("_lang", null);
			for(var i = 0; i < opts.length; i++) {
				var opt = opts[i];
				lang.options.add(opt);
				if(v != null && opt.value == v) {
					opt.selected = true;
					v = null;
				}
			}
		}
	}
	var txt = this._sourceTextInputElement;
	if(txt) txt.value = this.getOption("_text", "");
}
Bitrix.CodePasteDialog.prototype._settingsToOptions = function() {
	var lang = this._langSelectElement;
	if(lang) {
		var val = "";
		for(var i = 0; i < lang.options.length; i++) {
			var opt = lang.options[i];
			if(!opt.selected) continue;
			val = opt.value;
			break;
		}
		this.setOption("_lang", val);
	}
	var txt = this._sourceTextInputElement;
	if(txt) this.setOption("_text", txt.value);	
}
Bitrix.CodePasteDialog.prototype.getLanguage = function() { 
	var lang = this._langSelectElement; 
	return lang ? lang.value : this.getOption("_lang", ""); 
}
Bitrix.CodePasteDialog.prototype.setLanguage = function(val) { 
	this.setOption("_lang", val);
	var lang = this._langSelectElement; 
	if(!lang) return;
	for(var i = 0; i < lang.options.length; i++) {
		var opt = lang.options[i];
		opt.selected = opt.value == val;
	}
}
Bitrix.CodePasteDialog.prototype.getLanguageOptions = function() {
	var lang = this._langSelectElement; 
	return lang ? lang.options : this.getOption("_langOpts", null);
} 
Bitrix.CodePasteDialog.prototype.setLanguageOptions = function(opts) {
	this.setOption("_langOpts", opts);
	var lang = this._langSelectElement; 
	if(!lang) return;
	while(lang.options.length > 0) lang.remove(0);
	if(opts)
		for(var i = 0; i < opts.length; i++) 
			lang.options.add(opts[i]);
} 
Bitrix.CodePasteDialog.prototype.getText = function() { 
	var txt = this._sourceTextInputElement;
	return txt ? txt.value : this.getOption("_text", ""); 
}
Bitrix.CodePasteDialog.prototype.setText = function(val) {
	this.setOption("_text", val)
	var txt = this._sourceTextInputElement;
	if(txt) txt.value = val; 
}
Bitrix.CodePasteDialog.prototype.getTextWrap = function() { 
	return this.getOption("_textWrap", "soft"); 
}
Bitrix.CodePasteDialog.prototype.setTextWrap = function(val) { 
	this.setOption("_textWrap", val);
	var txt = this._sourceTextInputElement;
	if(txt) txt.setAttribute("wrap", val); 	
}
Bitrix.CodePasteDialog.prototype._getChildElementId = function(parentId, id){ return (parentId ? parentId : this.getId()) + "_" + id; }
Bitrix.CodePasteDialog.prototype._getLangSelectId = function(){ return this._getChildElementId(null, "LangSelect"); }
Bitrix.CodePasteDialog.prototype._getSourceTextInputId = function(){ return this._getChildElementId(null, "TextInput"); }
Bitrix.CodePasteDialog.prototype._handleSelfClose = function(e) { this._settingsToOptions(); }
Bitrix.CodePasteDialog.prototype.getOption = function(name, defaultValue) {
	if(!Bitrix.TypeUtility.isNotEmptyString(name)) return defaultValue;
	if (this._options && name in this._options) return this._options[name];
	else if (name in Bitrix.CodePasteDialog.defaults) return Bitrix.CodePasteDialog.defaults[name];	
	else if (name in Bitrix.Dialog.defaults) return Bitrix.Dialog.defaults[name];
	else return defaultValue;
}
Bitrix.CodePasteDialog.prototype._handleSetOptions = function() { this._settingsFromOptions(); }
Bitrix.CodePasteDialog.prototype._getAdditionalContainerWrapperClasses = function(){ return this.getOption("_containerWrapperClass"); }
Bitrix.CodePasteDialog.prototype._getMessageKeyPrefix = function(){ return "CodePasteDialog$msg$"; }
Bitrix.CodePasteDialog.prototype._getMessageContainerName = function(){ return "COMMUNICATION_UTILITY_DIALOG_MSG"; }
Bitrix.CodePasteDialog.create = function(id, name, title, options){
	if(this._items && id in this._items) throw "Item '"+ id +"' already exists!";
	var self = new Bitrix.CodePasteDialog();
	self.initialize(id, name, title, options);
	Bitrix.Dialog._addItem(self);
	return self;
}
Bitrix.CodePasteDialog.defaults = {
	_containerWrapperClass:"bx-code-paste-dialog-container-wrapper",
	_contentContainerClass:"bx-code-paste-dialog-content-container",
	_labelContainerClass:"bx-code-paste-dialog-label-container",
	_labelClass:"bx-code-paste-dialog-label",
	_selectContainerClass:"bx-code-paste-dialog-select-container",
	_selectClass:"bx-code-paste-dialog-select",
	_textAreaContainerClass:"bx-code-paste-dialog-text-area-container",
	_textAreaClass:"bx-code-paste-dialog-text-area",
	_contentContainerTableClass:"bx-table-paste-content-container-table"
}