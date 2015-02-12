if(typeof(Bitrix) == "undefined"){
	var Bitrix = new Object();
}

Bitrix.MoviePasteDialog = function Bitrix$MoviePasteDialog()
{
	this.Bitrix$Dialog();
	this._fileUrlInputElement = null;
	this._player = null; 
	this._previewButtonClickHandler = Bitrix.TypeUtility.createDelegate(this, this._handlePreviewButtonClick);
	this._resetButtonClickHandler = Bitrix.TypeUtility.createDelegate(this, this._handleResetButtonClick);
	this._closeHandler = Bitrix.TypeUtility.createDelegate(this, this._handleSelfClose);
	this._previewButtonEl = null;
	this._resetButtonEl = null;
	this._videoWidthInputEl = null;
	this._videoHeightInputEl = null;
}
Bitrix.TypeUtility.copyPrototype(Bitrix.MoviePasteDialog, Bitrix.Dialog);
Bitrix.MoviePasteDialog.prototype.initialize = function(id, name, title, options)
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
	var fileUrlLabel = document.createElement("LABEL");
	fileUrlLabel.className = this.getOption("_labelClass");
	fileUrlLabel.htmlFor = this._getUrlInputId();
	fileUrlLabel.innerHTML = this.getMessage("fileUrlLabel") + ":";	
	fileUrlLabelC.appendChild(fileUrlLabel);	
	
	var fileUrlC = fileUrlR.insertCell(-1);
	var fileUrl = this._fileUrlInputElement = document.createElement("INPUT");
	fileUrl.className = this.getOption("_inputTextClass");
	fileUrl.type = "text";
	fileUrl.id = this._getUrlInputId();
	var url = this.getOption("_movieUrl", "");
	fileUrl.value = Bitrix.TypeUtility.isNotEmptyString(url) ? url : this.getDefaultMovieUrl();	
	fileUrlC.appendChild(fileUrl);
	
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

	var videoWidthR = table.insertRow(-1);
	
	var videoWidthLabelC = videoWidthR.insertCell(-1);
	var videoWidthLabel = document.createElement("LABEL");
	videoWidthLabel.className = this.getOption("_labelClass");
	videoWidthLabel.htmlFor = this._getVideoWidthInputId();
	videoWidthLabel.innerHTML = this.getMessage("videoWidthLabel") + ":";	
	videoWidthLabelC.appendChild(videoWidthLabel);		
	
	var videoWidthC = videoWidthR.insertCell(-1);
	videoWidthC.colSpan = 2;
	var videoWidthInput = this._videoWidthInputEl = document.createElement("INPUT");
	videoWidthInput.type = "text";
	videoWidthInput.className = this.getOption("_inputNumberClass");
	videoWidthInput.value = this.getOption("_previewWidth").toString();
	videoWidthC.appendChild(videoWidthInput);	
	
	var videoHeightR = table.insertRow(-1);
	
	var videoHeightLabelC = videoHeightR.insertCell(-1);
	var videoHeightLabel = document.createElement("LABEL");
	videoHeightLabel.className = this.getOption("_labelClass");
	videoHeightLabel.htmlFor = this._getVideoHeightInputId();
	videoHeightLabel.innerHTML = this.getMessage("videoHeightLabel") + ":";	
	videoHeightLabelC.appendChild(videoHeightLabel);
	
	var videoHeightC = videoHeightR.insertCell(-1);
	videoHeightC.colSpan = 2;
	var videoHeightInput = this._videoHeightInputEl = document.createElement("INPUT");
	videoHeightInput.type = "text";
	videoHeightInput.className = this.getOption("_inputNumberClass");
	videoHeightInput.value = this.getOption("_previewHeight").toString();
	videoHeightC.appendChild(videoHeightInput);	
	
	var playerR = table.insertRow(-1);
	var playerC = playerR.insertCell(-1);
	playerC.id = this._getPlayerContainerId();
	playerC.colSpan = 3;
	this.setContent(content);
}
Bitrix.MoviePasteDialog.prototype.construct = function()
{
	if (this._isConstructed) return;
	this.Bitrix$Dialog.prototype.construct.call(this);
	this.addCloseListener(this._closeHandler);
}
Bitrix.MoviePasteDialog.prototype._getAdditionalContainerWrapperClasses = function(){ return this.getOption("_containerWrapperClass"); }
Bitrix.MoviePasteDialog.prototype.getOption = function(name, defaultValue) {
	if(!Bitrix.TypeUtility.isNotEmptyString(name)) return defaultValue;
	if (this._options && name in this._options) return this._options[name];
	else if (name in Bitrix.MoviePasteDialog.defaults) return Bitrix.MoviePasteDialog.defaults[name];	
	else if (name in Bitrix.Dialog.defaults) return Bitrix.Dialog.defaults[name];
	else return defaultValue;
}
Bitrix.MoviePasteDialog.prototype._handleSetOptions = function() {
	if(this._fileUrlInputElement && this.getOption("_movieUrl")) this._fileUrlInputElement.value = this.getOption("_movieUrl");
}
Bitrix.MoviePasteDialog.prototype._getMessageKeyPrefix = function(){ return "MoviePasteDialog$msg$"; }
Bitrix.MoviePasteDialog.prototype._getMessageContainerName = function(){ return "COMMUNICATION_UTILITY_DIALOG_MSG"; }
Bitrix.MoviePasteDialog.prototype.getDefaultMovieUrl = function(){ return this.getOption("_defaultMovieUrl", "http://"); }
Bitrix.MoviePasteDialog.prototype.setDefaultMovieUrl = function(url){ this.setOption("_defaultMovieUrl", url); }
Bitrix.MoviePasteDialog.prototype.getMovieUrl = function() { return this._fileUrlInputElement ? this._fileUrlInputElement.value : this.getOption("_movieUrl", ""); }
Bitrix.MoviePasteDialog.prototype.setMovieUrl = function(url) { this.setOption("_movieUrl", url); if(this._fileUrlInputElement) this._fileUrlInputElement.value = url; }
Bitrix.MoviePasteDialog.prototype.getMovieWidthInPixels = function(){
	if(this._videoWidthInputEl){
		var r = parseInt(this._videoWidthInputEl.value);
		if(!isNaN(r)) return r; 
	}
	return this.getOption("_previewWidth", 425);
}
Bitrix.MoviePasteDialog.prototype.getMovieHeightInPixels = function(){
	if(this._videoHeightInputEl){
		var r = parseInt(this._videoHeightInputEl.value);
		if(!isNaN(r)) return r; 
	}
	return this.getOption("_previewHeight", 340);
}
Bitrix.MoviePasteDialog.prototype.setMovieWidthInPixels = function(value)
{
	if (this._videoWidthInputEl)
		this._videoWidthInputEl.value = value;
	this.setOption("_previewWidth", value);
}
Bitrix.MoviePasteDialog.prototype.setMovieHeightInPixels = function(value)
{
	if (this._videoHeightInputEl)
		this._videoHeightInputEl.value = value;
	this.setOption("_previewHeight", value);
}
Bitrix.MoviePasteDialog.prototype.reset = function()
{
	if(this._player){
		this._player.stop();
		this._player.deactivate();
		delete this._player;
		this._player = null;
	}
	this.setMovieUrl(this.getDefaultMovieUrl());
	if(this._previewButtonEl) this._previewButtonEl.style.display = "";
	this._resetButtonEl.style.display = "none";
	
	if (this._videoWidthInputEl)
		this._videoWidthInputEl.value = this.getOption("_previewWidth", 425);	
	if (this._videoHeightInputEl)
		this._videoHeightInputEl.value = this.getOption("_previewHeight", 340);		

}
Bitrix.MoviePasteDialog.prototype._handlePreviewButtonClick = function(e)
{
	if(!this._previewButtonEl || !this._fileUrlInputElement) return;
	var url = this.getMovieUrl();
	if(!Bitrix.TypeUtility.isNotEmptyString(url)) return;
	this._play(url);
}
Bitrix.MoviePasteDialog.prototype._play = function(url){
	if(!this._player){
		var dataParams = new Object();
		dataParams["type"] = Bitrix.MediaPlayerType.auto;
		var w = this.getMovieWidthInPixels();
		if(w > 0) dataParams["width"] = w + "px";
		var h = this.getMovieHeightInPixels();
		if(h > 0) dataParams["height"] = h + "px";
		dataParams["url"] = url;
		dataParams["enableautostart"] = true;
		dataParams["enabledownloading"] = false;
		var data = Bitrix.MediaPlayerData.create(dataParams);
		this._player = Bitrix.MediaPlayer.create(this._getPlayerId(), this._getPlayerContainerId(), data);
		this._player.activate();
	}
	else{
		this._player.activate();
		var item = new Object();
		item["type"] = Bitrix.MediaPlayerType.auto;
		item["width"] = this.getMovieWidthInPixels() + "px";
		item["height"] = this.getMovieHeightInPixels() + "px";;		
		item["url"] = url;
		item["enableautostart"] = true;
		item["enabledownloading"] = false;
		this._player.play(item);
	}
	if(this._resetButtonEl) this._resetButtonEl.style.display = "";
	this._previewButtonEl.style.display = "none";	
}
Bitrix.MoviePasteDialog.prototype._handleResetButtonClick = function(e) {
	if(!this._resetButtonEl) return;
	this.reset();
}
Bitrix.MoviePasteDialog.prototype._handleSelfClose = function(e)
{
	if(!this._player) return;
	this._player.stop();
	this._player.deactivate();
	if(this._previewButtonEl) this._previewButtonEl.style.display = "";
	if(this._resetButtonEl) this._resetButtonEl.style.display = "none";
	if(this._fileUrlInputElement) this.setOption("_movieUrl", this._fileUrlInputElement.value);
}
Bitrix.MoviePasteDialog.prototype._getChildElementId = function(parentId, id){ return (parentId ? parentId : this.getId()) + "_" + id; }
Bitrix.MoviePasteDialog.prototype._getPlayerContainerId = function(){ return this._getChildElementId(null, "PlayerContainer"); }
Bitrix.MoviePasteDialog.prototype._getPlayerId = function(){ return this._getChildElementId(null, "Player"); }
Bitrix.MoviePasteDialog.prototype._getUrlInputId = function(){ return this._getChildElementId(null, "UrlInput"); }
Bitrix.MoviePasteDialog.prototype._getVideoWidthInputId = function(){ return this._getChildElementId(null, "VideoWidthInput"); }
Bitrix.MoviePasteDialog.prototype._getVideoHeightInputId = function(){ return this._getChildElementId(null, "VideoHeightInput"); }
Bitrix.MoviePasteDialog.create = function(id, name, title, options){
	if(this._items && id in this._items) throw "Item '"+ id +"' already exists!";
	var self = new Bitrix.MoviePasteDialog();
	self.initialize(id, name, title, options);
	Bitrix.Dialog._addItem(self);
	return self;
}
Bitrix.MoviePasteDialog.defaults = {
	_containerWrapperClass:"bx-movie-paste-dialog-container-wrapper",
	_contentContainerClass:"bx-movie-paste-dialog-content-container",
	_labelClass:"bx-movie-paste-dialog-label",
	_legendClass:"bx-movie-paste-dialog-legend",
	_inputTextClass:"bx-movie-paste-dialog-input-text",
	_inputNumberClass:"bx-movie-paste-dialog-input-number",
	_buttonClass:"bx-movie-paste-dialog-button",
	_contentContainerTableClass:"content-container-table",
	_previewWidth:425,
	_previewHeight:340
}