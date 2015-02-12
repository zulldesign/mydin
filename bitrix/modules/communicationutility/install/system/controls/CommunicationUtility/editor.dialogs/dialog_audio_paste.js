if(typeof(Bitrix) == "undefined"){
	var Bitrix = new Object();
}

Bitrix.AudioPasteDialog = function Bitrix$AudioPasteDialog()
{
	this.Bitrix$Dialog();
	this._fileUrlInputElement = null;
	this._player = null; 
	this._previewButtonClickHandler = Bitrix.TypeUtility.createDelegate(this, this._handlePreviewButtonClick);
	this._resetButtonClickHandler = Bitrix.TypeUtility.createDelegate(this, this._handleResetButtonClick);
	this._closeHandler = Bitrix.TypeUtility.createDelegate(this, this._handleSelfClose);
	this._previewButtonEl = null;
	this._resetButtonEl = null;
}
Bitrix.TypeUtility.copyPrototype(Bitrix.AudioPasteDialog, Bitrix.Dialog);
Bitrix.AudioPasteDialog.prototype.initialize = function(id, name, title, options)
{
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
	
	var fileUrlR = table.insertRow(-1);
	var fileUrlLabelC = fileUrlR.insertCell(-1);
	var fileUrlLabelCnr = document.createElement("DIV");
	fileUrlLabelCnr.className = this.getOption("_labelContainerClass");	
	fileUrlLabelC.appendChild(fileUrlLabelCnr);
	var fileUrlLabel = document.createElement("LABEL");
	fileUrlLabel.className = this.getOption("_labelClass");
	fileUrlLabel.htmlFor = this._getUrlInputId();
	fileUrlLabel.innerHTML = this.getMessage("fileUrlLabel") + ":";	
	fileUrlLabelCnr.appendChild(fileUrlLabel);	
	
	var fileUrlC = fileUrlR.insertCell(-1);
	var fileUrlCnr = document.createElement("DIV");
	fileUrlCnr.className = this.getOption("_inputContainerClass");	
	fileUrlC.appendChild(fileUrlCnr);
	
	var fileUrl = this._fileUrlInputElement = document.createElement("INPUT");
	fileUrl.className = this.getOption("_inputTextClass");
	fileUrl.type = "text";
	fileUrl.id = this._getUrlInputId();
	var url = this.getOption("_audioUrl", "");
	fileUrl.value = Bitrix.TypeUtility.isNotEmptyString(url) ? url : this.getDefaultAudioUrl();
	fileUrlCnr.appendChild(fileUrl);
	
	var fileUrlButtonC = fileUrlR.insertCell(-1);
	fileUrlButtonC.align = "right";
	var previewButton = this._previewButtonEl = document.createElement("BUTTON");
	previewButton.className = this.getOption("_buttonClass");
	previewButton.innerHTML = this.getMessage("previewButton");
	fileUrlButtonC.appendChild(previewButton);
	Bitrix.EventUtility.addEventListener(previewButton, "click", this._previewButtonClickHandler);

	var resetButton = this._resetButtonEl = document.createElement("BUTTON");
	resetButton.className = this.getOption("_buttonClass");
	resetButton.innerHTML = this.getMessage("resetButton");
	resetButton.style.display = "none";
	fileUrlButtonC.appendChild(resetButton);
	Bitrix.EventUtility.addEventListener(resetButton, "click", this._resetButtonClickHandler);
	
	var fileUrlLegendR = table.insertRow(-1);
	var fileUrlLegendC = fileUrlLegendR.insertCell(-1);
	fileUrlLegendC.colSpan = 3;
	var fileUrlLegend = document.createElement("SPAN");
	fileUrlLegend.className = this.getOption("_legendClass");
	fileUrlLegend.innerHTML = this.getMessage("fileUrlLegend");	
	fileUrlLegendC.appendChild(fileUrlLegend);		
	
	var playerR = table.insertRow(-1);
	var playerC = playerR.insertCell(-1);
	playerC.colSpan = 3;
	var playerContainer = document.createElement("DIV");
	playerC.appendChild(playerContainer);
	playerContainer.className = this.getOption("_playerContainerClass");
	var playerScreenStub = document.createElement("DIV");
	playerScreenStub.className = this.getOption("_playerScrenStubClass");
	playerContainer.appendChild(playerScreenStub);
	
	var playerInnerContainer = document.createElement("DIV");
	playerInnerContainer.className = this.getOption("_playerInnerContainerClass");
	playerInnerContainer.id = this._getPlayerContainerId();
	playerContainer.appendChild(playerInnerContainer);
	
	this.setContent(content);
}
Bitrix.AudioPasteDialog.prototype.construct = function()
{
	if (this._isConstructed) return;
	this.Bitrix$Dialog.prototype.construct.call(this);
	this.addCloseListener(this._closeHandler);
}
Bitrix.AudioPasteDialog.prototype._getAdditionalContainerWrapperClasses = function(){ return this.getOption("_containerWrapperClass"); }
Bitrix.AudioPasteDialog.prototype.getOption = function(name, defaultValue) {
	if(!Bitrix.TypeUtility.isNotEmptyString(name)) return defaultValue;
	if (this._options && name in this._options) return this._options[name];
	else if (name in Bitrix.AudioPasteDialog.defaults) return Bitrix.AudioPasteDialog.defaults[name];	
	else if (name in Bitrix.Dialog.defaults) return Bitrix.Dialog.defaults[name];
	else return defaultValue;
}
Bitrix.AudioPasteDialog.prototype._handleSetOptions = function() {
	if(this._fileUrlInputElement && this.getOption("_audioUrl")) this._fileUrlInputElement.value = this.getOption("_audioUrl");
}
Bitrix.AudioPasteDialog.prototype._getMessageKeyPrefix = function(){ return "AudioPasteDialog$msg$"; }
Bitrix.AudioPasteDialog.prototype._getMessageContainerName = function(){ return "COMMUNICATION_UTILITY_DIALOG_MSG"; }
Bitrix.AudioPasteDialog.prototype.getDefaultAudioUrl = function(){ return this.getOption("defaultAudioUrl", "http://"); }
Bitrix.AudioPasteDialog.prototype.setDefaultAudioUrl = function(url){ this.setOption("defaultAudioUrl", url); }
Bitrix.AudioPasteDialog.prototype.getAudioUrl = function(){
	return this._fileUrlInputElement ? this._fileUrlInputElement.value : this.getOption("_audioUrl", "");
}
Bitrix.AudioPasteDialog.prototype.setAudioUrl = function(url) { 
	this.setOption("_audioUrl", url);
	if(this._fileUrlInputElement) this._fileUrlInputElement.value = url;
}
Bitrix.AudioPasteDialog.prototype.reset = function()
{
	this._handleResetButtonClick();
}
Bitrix.AudioPasteDialog.prototype._handlePreviewButtonClick = function(e)
{
	if(!this._previewButtonEl || !this._fileUrlInputElement) return;
	var url = this.getAudioUrl();
	if(!Bitrix.TypeUtility.isNotEmptyString(url)) return;
	this._play(url);
}
Bitrix.AudioPasteDialog.prototype._play = function(url){
	if(!this._player){
		var dataParams = new Object();
		dataParams["type"] = Bitrix.MediaPlayerType.auto;
		dataParams["width"] = "405px";
		dataParams["height"] = "30px";
		dataParams["url"] = url;
		dataParams["enableautostart"] = true;
		dataParams["enabledownloading"] = false;
		dataParams["fullscreen"] = false;
		dataParams["flvwmode"] = "opaque";
		var data = Bitrix.MediaPlayerData.create(dataParams);
		this._player = Bitrix.MediaPlayer.create(this._getPlayerId(), this._getPlayerContainerId(), data);
		this._player.activate();
	}
	else{
		this._player.activate();
		var item = new Object();
		item["type"] = Bitrix.MediaPlayerType.auto;
		dataParams["width"] = "405px";
		dataParams["height"] = "25px";
		item["url"] = url;
		item["enableautostart"] = true;
		item["enabledownloading"] = false;
		item["fullscreen"] = false;
		item["flvwmode"] = "opaque";
		this._player.play(item);
	}
	if(this._resetButtonEl) this._resetButtonEl.style.display = "";
	this._previewButtonEl.style.display = "none";	
}
Bitrix.AudioPasteDialog.prototype._handleResetButtonClick = function(e)
{
	if(!this._resetButtonEl) return;
	if(this._player){
		this._player.stop();
		this._player.deactivate();
		delete this._player;
		this._player = null;
	}
	this.setAudioUrl(this.getDefaultAudioUrl());
	if(this._previewButtonEl) this._previewButtonEl.style.display = "";
	this._resetButtonEl.style.display = "none";
}
Bitrix.AudioPasteDialog.prototype._handleSelfClose = function(e)
{
	if(!this._player) return;
	this._player.stop();
	this._player.deactivate();
	delete this._player;
	this._player = null;
	if(this._previewButtonEl) this._previewButtonEl.style.display = "";
	if(this._resetButtonEl) this._resetButtonEl.style.display = "none";
	if(this._fileUrlInputElement) this.setOption("_audioUrl", this._fileUrlInputElement.value);
}
Bitrix.AudioPasteDialog.prototype._getChildElementId = function(parentId, id){ return (parentId ? parentId : this.getId()) + "_" + id; }
Bitrix.AudioPasteDialog.prototype._getPlayerContainerId = function(){ return this._getChildElementId(null, "PlayerContainer"); }
Bitrix.AudioPasteDialog.prototype._getPlayerId = function(){ return this._getChildElementId(null, "Player"); }
Bitrix.AudioPasteDialog.prototype._getUrlInputId = function(){ return this._getChildElementId(null, "UrlInput"); }
Bitrix.AudioPasteDialog.prototype._getVideoWidthInputId = function(){ return this._getChildElementId(null, "VideoWidthInput"); }
Bitrix.AudioPasteDialog.prototype._getVideoHeightInputId = function(){ return this._getChildElementId(null, "VideoHeightInput"); }
Bitrix.AudioPasteDialog.create = function(id, name, title, options){
	if(this._items && id in this._items) throw "Item '"+ id +"' already exists!";
	var self = new Bitrix.AudioPasteDialog();
	self.initialize(id, name, title, options);
	Bitrix.Dialog._addItem(self);
	return self;
}
Bitrix.AudioPasteDialog.defaults = {
	_containerWrapperClass:"bx-audio-paste-dialog-container-wrapper",
	_contentContainerClass:"bx-audio-paste-dialog-content-container",
	_labelContainerClass: "bx-audio-paste-dialog-label-container",
	_labelClass:"bx-audio-paste-dialog-label",
	_legendClass:"bx-audio-paste-dialog-legend",
	_inputContainerClass: "bx-audio-paste-dialog-input-container",
	_inputTextClass:"bx-audio-paste-dialog-input-text",
	_inputNumberClass:"bx-audio-paste-dialog-input-number",
	_playerContainerClass: "bx-audio-player-container",
	_playerScrenStubClass: "bx-audio-player-screen-stub",
	_playerInnerContainerClass: "bx-audio-player-inner-container",
	_buttonClass:"bx-audio-paste-dialog-button",
	_contentContainerTableClass:"content-container-table"
}