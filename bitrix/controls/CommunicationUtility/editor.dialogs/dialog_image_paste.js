if(typeof(Bitrix) == "undefined"){
	var Bitrix = new Object();
}

Bitrix.ImagePasteDialogMode = {
	file: 1,
	url: 2
}
Bitrix.ImagePasteDialog = function Bitrix$ImagePasteDialog()
{
	this.Bitrix$Dialog();
	this._fileRow = null;
	this._waitRow = null;
	this._fileInputHandler = Bitrix.TypeUtility.createDelegate(this, this._handleFileInputChange);
	this._fileLoadedHandler = Bitrix.TypeUtility.createDelegate(this, this._waitForFileLoading);
	this._fileFormContainer = null;
	this._fileFormCount = 0;
	this._thumbnails = null;
	this._selectedThumbnails = null;
	this._uploadQueue = null;
	this._currentFileFormId = null;
	this._currentFileFormName = null;
	this._mode = Bitrix.ImagePasteDialogMode.file;
}
Bitrix.TypeUtility.copyPrototype(Bitrix.ImagePasteDialog, Bitrix.Dialog);
Bitrix.ImagePasteDialog.prototype.initialize = function(id, name, title, mode, options)
{
	this.Bitrix$Dialog.prototype.initialize.call(this, id, name, title, null, Bitrix.Dialog.buttonLayout.cancelOk, options);
	if(mode) this._mode = mode; 
	var content = new Array();
	
	var container = document.createElement("DIV");
	container.id = id + "_" + "container";
	container.className = this.getOption("_contentContainerClass");
	content.push(container);
	
	var table = document.createElement("TABLE");
	container.appendChild(table);
	
	if(this._mode == Bitrix.ImagePasteDialogMode.file){
		var waitR = this._waitRow = table.insertRow(-1);
		waitR.style.display = "none";
		var waitC = waitR.insertCell(-1);
		waitC.colSpan = 2;
		var waitMsg = document.createElement("SPAN");
		waitMsg.className = this.getOption("_waitForLoadingCompletionClass");
		waitMsg.innerHTML = this.getMessage("waitForFileLoadingCompletion");
		waitC.appendChild(waitMsg);
		var fileR = this._fileRow = table.insertRow(-1);
		var fileLableC = fileR.insertCell(-1);
		var fileLabel = document.createElement("LABEL");
		fileLabel.className = this.getOption("_labelClass");
		fileLabel.htmlFor = this._getFileInputId();
		fileLabel.innerHTML = this.getMessage("fileLabel") + ":";	
		fileLableC.appendChild(fileLabel);
		var fileC = this._fileFormContainer = fileR.insertCell(-1);
		var fileLegendR = table.insertRow(-1);
		var fileLegendC = fileLegendR.insertCell(-1);
		fileLegendC.colSpan = 2;
		var fileLegend = document.createElement("SPAN");
		fileLegend.className = this.getOption("_legendClass");
		fileLegend.innerHTML = this.getMessage("fileLegend");
		fileLegendC.appendChild(fileLegend);		
		var thumbsId = this.getThumbContainerId();
		var thumbR = table.insertRow(-1);
		var thumbLableC = thumbR.insertCell(-1);
		var thumbLabel = document.createElement("LABEL");
		thumbLabel.className = this.getOption("_labelClass");
		thumbLabel.htmlFor = thumbsId;
		thumbLabel.innerHTML = this.getMessage("thumbLabel") + ":";	
		thumbLableC.appendChild(thumbLabel);	
		
		var thumbContainerC = thumbR.insertCell(-1);
		
		var thumbContainerWrapper = document.createElement("DIV");
		thumbContainerWrapper.className = this.getOption("_thumbContainerWrapperClass");
		thumbContainerC.appendChild(thumbContainerWrapper);	
		
		var thumbContainer = document.createElement("DIV");
		thumbContainer.id = thumbsId;
		thumbContainer.className = className = this.getOption("_thumbContainerClass");
		thumbContainerWrapper.appendChild(thumbContainer);
	}
	else if(this._mode == Bitrix.ImagePasteDialogMode.url){
		var fileUrlR = table.insertRow(-1);
		var fileUrlLabelC = fileUrlR.insertCell(-1);
		var fileUrlLabel = document.createElement("LABEL");
		fileUrlLabel.className = this.getOption("_labelClass");
		fileUrlLabel.htmlFor = this._getUrlInputId();
		fileUrlLabel.innerHTML = this.getMessage("fileUrlLabel") + ":";
		fileUrlLabelC.appendChild(fileUrlLabel);	
		
		var fileUrlC = fileUrlR.insertCell(-1);
		var fileUrl = document.createElement("INPUT");
		fileUrl.className = this.getOption("_inputTextClass");
		fileUrl.type = "text";
		fileUrl.id = this._getUrlInputId();
		fileUrl.name = this._getUrlInputName();	
		fileUrl.value = this.getOption("_imageUrl", "http://");
		fileUrlC.appendChild(fileUrl);
		
		var fileUrlLegendR = table.insertRow(-1);
		var fileUrlLegendC = fileUrlLegendR.insertCell(-1);
		fileUrlLegendC.colSpan = 2;
		var fileUrlLegend = document.createElement("SPAN");
		fileUrlLegend.className = this.getOption("_legendClass");
		fileUrlLegend.innerHTML = this.getMessage("fileUrlLegend");	
		fileUrlLegendC.appendChild(fileUrlLegend);		
	}
	else throw "Bitrix.ImagePasteDialogMode '" + this._mode.toString() + "' is unknown in current context!";
	this.setContent(content);
}
Bitrix.ImagePasteDialog.prototype.construct = function()
{
	if (this._isConstructed) return;
	this.Bitrix$Dialog.prototype.construct.call(this);
	if(this._mode == Bitrix.ImagePasteDialogMode.file){
		this._constructFileForm();
		this._constructImageList();
	}
}
Bitrix.ImagePasteDialog.prototype._getAdditionalContainerWrapperClasses = function(){ return this.getOption("_containerWrapperClass"); }
Bitrix.ImagePasteDialog.prototype.getMode = function(){ return this._mode; }
Bitrix.ImagePasteDialog.prototype.getOption = function(name, defaultValue) {
	if(!Bitrix.TypeUtility.isNotEmptyString(name)) return defaultValue;
	if (this._options && name in this._options) return this._options[name];
	if (name in Bitrix.ImagePasteDialog.defaults) return Bitrix.ImagePasteDialog.defaults[name];	
	if (name in Bitrix.Dialog.defaults) return Bitrix.Dialog.defaults[name];
	return defaultValue;
}
Bitrix.ImagePasteDialog.prototype._getMessageKeyPrefix = function(){ return "ImagePasteDialog$msg$"; }
Bitrix.ImagePasteDialog.prototype._getMessageContainerName = function(){ return "COMMUNICATION_UTILITY_DIALOG_MSG"; }
Bitrix.ImagePasteDialog.prototype.getSelectedImages = function(){
	if(!(this._selectedThumbnails && this._selectedThumbnails.length > 0)) return null;
	var result = new Array();
	for(var i = 0; i < this._selectedThumbnails.length; i++)
		result.push(this._selectedThumbnails[i].getImageUrl());
	return result;
}
Bitrix.ImagePasteDialog.prototype.resetSelectedImages = function(){
	if(!(this._thumbnails && this._thumbnails.length > 0)) return;
	for(var i = 0; i < this._thumbnails.length; i++)
		this._thumbnails[i].setSelected(false);
}
Bitrix.ImagePasteDialog.prototype.createThumbnail = function(imgId, imgUrl, thumbUrl, selected, scrollIntoView){
	var thumbnail = Bitrix.ImagePasteDialogThumbnail.create(this, imgId, imgUrl, thumbUrl);
	if(!this._thumbnails) this._thumbnails = new Array();
	this._thumbnails.push(thumbnail);
	thumbnail.construct();
	thumbnail.setSelected(selected);
	if(scrollIntoView)
		thumbnail.scrollIntoView();
}
Bitrix.ImagePasteDialog.prototype.clearThumbnailsSelection = function() {
    if (!this._selectedThumbnails) return;
    for (var i = 0; i < this._selectedThumbnails.length; i++) this._selectedThumbnails[i].setSelected(false);
    this._selectedThumbnails = null;
}
Bitrix.ImagePasteDialog.prototype.handleThumbnailSelectionChange = function(source){
	if(!this._selectedThumbnails)
		this._selectedThumbnails = new Array();
	if(source.isSelected())
		this._selectedThumbnails.push(source);
	else{
		for(var i = 0; i < this._selectedThumbnails.length; i++){
			if(this._selectedThumbnails[i].getImageUrl() != source.getImageUrl()) continue;
			this._selectedThumbnails.splice(i, 1);
			break;
		}
	}
}
Bitrix.ImagePasteDialog.prototype.handleThumbnailDeletion = function(source){
	var thumbnails = this._thumbnails;
	if(!thumbnails || thumbnails.length == 0) return;
	var params = "action=delete&imgId=" + source.getImageId() + "&imgUrl=" + source.getImageUrl();
	var self = this;
	this._makeRequest(params, 
		function(status, result){ 
			if(!(result && "action" in result && result.action.toUpperCase() == "DELETE" && (params = "params" in result ? result.params : null))) return; 
			if(status != 200){
				self.showError("error" in params ? params["error"] : "");
				return;
			}
			self.hideError();			
			var imgUrl = "imgUrl" in params ? params.imgUrl : null; 
			if(!imgUrl) return; 
			for(var i = 0; i < thumbnails.length; i++){ 
				var thumbnail = thumbnails[i]; 
				if(thumbnail.getImageUrl() != imgUrl) continue; 
				thumbnail.destruct(); 
				thumbnails.splice(i, 1); 
				delete thumbnail; break; 
			}
		});
}
Bitrix.ImagePasteDialog.prototype._handleFileInputChange = function(){
	var form = document.getElementById(this._getFileFormId());
	if(!form) return;
	var frameId = "Bitrix$ImagePasteDialog$Frame$" + Math.floor(Math.random() * 99999).toString();
	var frame = null;
	if(/*@cc_on!@*/false) //fix submit in new window in IE
	{
		try
		{
			frame = document.createElement("<iframe id=\"" + frameId + "\" name=\"" + frameId + "\" src=\"about:blank\" style=\"display:none;\"></frame>");
		}
		catch(e)
		{
		}
	}
	
	if (!frame)
	{
		frame = document.createElement("IFRAME");
		frame.name = frame.id = frameId;
		frame.src = "about:blank";
		frame.style.display = "none";
	}
	document.body.appendChild(frame);
	
	var uploadItem = new Object();
	uploadItem["frameId"] = frameId;
	uploadItem["formId"] = form.id;
	if(!this._uploadQueue)
		this._uploadQueue = new Object();
	this._uploadQueue[frameId] = uploadItem;
	
	this._fileLoadingWaitInterval = 0;
	Bitrix.EventUtility.addEventListener(frame, "load", this._fileLoadedHandler);
	form.setAttribute("target", frameId);
	form.submit();
	this._fileRow.style.display = "none";
	this._waitRow.style.display = "";
}

Bitrix.ImagePasteDialog.prototype._fileLoadingWaitInterval = 0;
Bitrix.ImagePasteDialog.prototype._waitForFileLoading = function(e, target){
    if(this._fileLoadingWaitInterval >= 1000) return false;
    this._fileLoadingWaitInterval += 100;
    var self = this;
    if(!target)
        target = /*@cc_on!@*/false ? (window.event ? window.event.srcElement: null) : e ? e.target : null; //iframe element
    setTimeout( function(){ self._handleFileLoaded(target); }, 100);
    return true;
}
Bitrix.ImagePasteDialog.prototype._handleFileLoaded = function(target){
	var uploadItem = null;
	if(!(target && this._uploadQueue && target.id in this._uploadQueue && (uploadItem = this._uploadQueue[target.id]))) return;
	
	var targetDocument = null;
	try{
		targetDocument = target.contentDocument ? target.contentDocument : target.contentWindow ? target.contentWindow.document : window.frames[target.id].document;
	}
	catch(e){
		this.showError("Could not find response document!");
	}
	
	if(targetDocument){
		var resultEl = targetDocument.getElementById(this.getOption("_handlerResultElementId"));
		var isResValid = resultEl != null;
		if(!isResValid) {
			if(this._waitForFileLoading(null, target)) return;
			this.showError(this.getMessage("uploadGeneralError"));
		}
		else {
			var result = null;
			try{ result = eval("(" + resultEl.value + ")"); } catch(e){}
			var params = null;
			isResValid = result && "action" in result && result.action.toUpperCase() == "UPLOAD" && (params = "params" in result ? result.params : null) != null;
			var err = "error" in params ? params["error"] : "";
			if(!isResValid || Bitrix.TypeUtility.isNotEmptyString(err))
				this.showError(err);
			else{
				this.hideError();
				try{
					this.createThumbnail("imgId" in params ? params.imgId : 0, "imgUrl" in params ? params.imgUrl : "", "thumbUrl" in params ? params.thumbUrl : "", true, true);
				}
				catch(e){}
			}
		}
	}
	var frameId = target.id;
	window.setTimeout(function(){document.body.removeChild(target);}, 1000);
	var formId = uploadItem["formId"];
	if(formId && this._fileFormContainer)
		this._fileFormContainer.removeChild(document.getElementById(formId));
	delete this._uploadQueue[frameId];
	this._constructFileForm();   
}

Bitrix.ImagePasteDialog.prototype._makeRequest = function(params, callBack){
	var contextParams = this.getOption("_contextRequestParams");
	if(contextParams){
		var contextParamsStr = "";
		for(var key in contextParams){
			if(contextParamsStr.length != 0) contextParamsStr += "&";
			contextParamsStr += encodeURIComponent(key) + "=" + encodeURIComponent(contextParams[key]);
		}
		
		if(Bitrix.TypeUtility.isNotEmptyString(contextParamsStr)){
			if(Bitrix.TypeUtility.isNotEmptyString(params))
				params += "&" + contextParamsStr;
			else
				params = contextParamsStr;	
		}
	}
	var request = Bitrix.HttpUtility.createXMLHttpRequest();
	request.open("POST", this.getOption("_handlerPath"), true);
	request.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
	request.setRequestHeader("Content-length", Bitrix.TypeUtility.isNotEmptyString(params) ? params.length : 0);
	request.setRequestHeader("Connection", "close");
	var self = this;
	request.onreadystatechange = function(){
		if(request.readyState != 4) return;
		var r = "", 
			txt = request.responseText;
		if(Bitrix.TypeUtility.isNotEmptyString(txt)){
			var rx = new RegExp("<input[^>]*?id=\"" + self.getOption("_handlerResultElementId") + "\"[^>]*?value=\"(.*)\"", "i");
			var m = rx.exec(txt);
			if(m && m.length > 1){
				txt = Bitrix.HttpUtility.htmlDecode(m[1]);
				try{ r = eval("(" + txt + ")"); } 
				catch(e){}		
			}
		}
		if(callBack) callBack(request.status, r);
	};
	request.send(params);
}
Bitrix.ImagePasteDialog.prototype._constructImageList = function(){
	if(this._mode != Bitrix.ImagePasteDialogMode.file) return;
	var url = this.getOption("_handlerPath");
	var params = "action=list&thumbWidth=" + this.getOption("_thumbnailWidthInPixels") + "&thumbHeight=" + this.getOption("_thumbnailHeightInPixels");
	var self = this;
	this._makeRequest(params, 
		function(status, result){ 
			if(!(result && "action" in result && result.action.toUpperCase() == "LIST" && (params = "params" in result ? result.params : null))) return;
			if(status != 200){
				self.showError("error" in params ? params["error"] : "");
				return;
			}
			self.hideError();
			var thumbs = "thumbs" in params ? params["thumbs"] : null;
			if(!thumbs) return;
			for(var i = 0; i < thumbs.length; i++){
				var thumb = thumbs[i];
				var imgId = "imgId" in thumb ? thumb.imgId : null,
					imgUrl = "imgUrl" in thumb ? thumb["imgUrl"] : null,
					thumbUrl = "thumbUrl" in thumb ? thumb["thumbUrl"] : null;
				if(!imgId || !imgUrl || !thumbUrl) continue;
				try{ self.createThumbnail(imgId, imgUrl, thumbUrl, false, false); } catch(e){}
			}
		});
}
Bitrix.ImagePasteDialog.prototype._constructFileForm = function(){
	if(this._mode != Bitrix.ImagePasteDialogMode.file) return;
	var num = (this._fileFormCount += 1);
	this._currentFileFormId = this._getChildElementId(null, "FileForm" + num.toString());
	this._currentFileFormName = this._getChildElementName(null, "FileForm" + num.toString());
		
	var result = "<form id=\"" + this._currentFileFormId + "\" name=\"" + this._currentFileFormName + "\" method=\"post\" action=\"" + this.getOption("_handlerPath") + "\" enctype=\"multipart/form-data\"><input type=\"hidden\" id=\"action\" name=\"action\" value=\"upload\" /><input type=\"hidden\" id=\"thumbWidth\" name=\"thumbWidth\" value=\"" + this.getOption("_thumbnailWidthInPixels") + "\" /><input type=\"hidden\" id=\"thumbHeight\" name=\"thumbHeight\" value=\"" + this.getOption("_thumbnailHeightInPixels") + "\" />";
	var contextParams = this.getOption("_contextRequestParams");
	if(contextParams)
		for(var key in contextParams)
			result += "<input type=\"hidden\" id=\"" + key + "\" name=\"" + key + "\" value=\"" + contextParams[key] + "\" />";
						
	result += "<input type=\"file\" id=\"" + this._getFileInputId() + "\" name=\"" + this._getFileInputName() + "\" class=\"" + this.getOption("_inputFileClass") + "\"/></form>";
	this._fileFormContainer.innerHTML = result;
	Bitrix.EventUtility.addEventListener(document.getElementById(this._getFileInputId()), "change", this._fileInputHandler);
	this._fileRow.style.display = "";
	this._waitRow.style.display = "none";
}
Bitrix.ImagePasteDialog.prototype._getChildElementId = function(parentId, id){
	return (parentId ? parentId : this.getId()) + "_" + id;
}
Bitrix.ImagePasteDialog.prototype._handleSetOptions = function() {
	var element = document.getElementById(this._getUrlInputId());
	if(element && this.getOption("_imageUrl")) element.value = this.getOption("_imageUrl");
}
Bitrix.ImagePasteDialog.prototype.getImageUrl = function(){
	var element = document.getElementById(this._getUrlInputId());
	return element ? element.value : this.getOption("_imageUrl", "");
}
Bitrix.ImagePasteDialog.prototype.setImageUrl = function(url){
	this.setOption("_imageUrl", url);
	var element = document.getElementById(this._getUrlInputId());
	if(element) element.value = url;	
}
Bitrix.ImagePasteDialog.prototype.resetImageUrl = function(){
	var element = document.getElementById(this._getUrlInputId());
	if(element) element.value = "http://";	
}
Bitrix.ImagePasteDialog.prototype._getFileFormId = function(){
	return this._currentFileFormId;
}
Bitrix.ImagePasteDialog.prototype._getFileInputId = function(){
	return this._getChildElementId(this._getFileFormId(), "FileInput");
}
Bitrix.ImagePasteDialog.prototype._getUrlInputId = function(){
	return this._getChildElementId(null, "UrlInput");
}
Bitrix.ImagePasteDialog.prototype._getChildElementName = function(parentName, name){
	return (parentName ? parentName : this.getName()) + "$" + name;
}
Bitrix.ImagePasteDialog.prototype._getFileFormName = function(){
	return this._currentFileFormName;
}
Bitrix.ImagePasteDialog.prototype._getFileInputName = function(){
	return this._getChildElementName(this._getFileFormName(), "FileInput");
}
Bitrix.ImagePasteDialog.prototype._getUrlInputName = function(){
	return this._getChildElementName(null, "UrlInput");
}
Bitrix.ImagePasteDialog.prototype.getThumbContainerId = function(){
	return this._getChildElementId(null, "Thumbs");
}
Bitrix.ImagePasteDialog.create = function(id, name, title, mode, options){
	if(this._items && id in this._items) throw "Item '"+ id +"' already exists!";
	var self = new Bitrix.ImagePasteDialog();
	self.initialize(id, name, title, mode, options);
	Bitrix.Dialog._addItem(self);
	return self;
}
Bitrix.ImagePasteDialog.defaults = {
	_containerWrapperClass: "bx-image-paste-dialog-container-wrapper",
	_contentContainerClass: "bx-image-paste-dialog-content-container",
	_labelClass: "bx-image-paste-dialog-label",
	_legendClass: "bx-image-paste-dialog-legend",
	_inputFileClass: "bx-image-paste-dialog-input-file",
	_inputTextClass: "bx-image-paste-dialog-input-text",
	_thumbContainerWrapperClass: "bx-image-paste-dialog-thumb-container-wrapper",
	_thumbContainerClass: "bx-image-paste-dialog-thumb-container",
	_thumbnailWrapper1Class: "bx-image-paste-dialog-thumbnail-wrapper1",
	_thumbnailWrapper2Class: "bx-image-paste-dialog-thumbnail-wrapper2",
	_thumbnailWrapper3Class: "bx-image-paste-dialog-thumbnail-wrapper3",
	_thumbnailContainerClass: "bx-image-paste-dialog-thumbnail-container",
	_thumbnailDeleteClass: "bx-image-paste-dialog-thumbnail-delete",
	_thumbnailImgClass: "bx-image-paste-dialog-thumbnail-img",
	_thumbnailSelectedClass: "bx-image-paste-dialog-thumbnail-selected",
	_waitForLoadingCompletionClass: "bx-image-paste-dialog-wait-for-loading-completion",
	_thumbnailWidthInPixels: 90,
	_thumbnailHeightInPixels: 90,
	_handlerPath: "",
	_handlerResultElementId: "r",
	_contextRequestParams: {"blogId":0, "postId":0}
}

Bitrix.ImagePasteDialogThumbnail = function Bitrix$ImagePasteDialogThumbnail() {
    this._dlg = null;
    this._imgId = null;
    this._imgUrl = "";
    this._thumbUrl = "";
    this._isSelected = true;
    this._wrapperElement = null;
    this._wrapperBorderElement = null;
    this._selectedIcon = null;
    this._deleteHandler = Bitrix.TypeUtility.createDelegate(this, this._handleDeletion);
    this._clickHandler = Bitrix.TypeUtility.createDelegate(this, this._handleClickSelection);
    this._dblclickHandler = Bitrix.TypeUtility.createDelegate(this, this._handleDblClickSelection);
    this._clickHaddlerTimerId = null;
}
Bitrix.ImagePasteDialogThumbnail.prototype =
{
    initialize: function(dlg, imgId, imgUrl, thumbUrl) {
        this._dlg = dlg;
        this._imgId = imgId;
        this._imgUrl = imgUrl;
        if (!Bitrix.TypeUtility.isNotEmptyString(thumbUrl)) throw "Thumbnail URL is not defined!";
        this._thumbUrl = thumbUrl;
    },
    construct: function() {
        var wrapper1 = this._wrapperElement = document.createElement("DIV");
        wrapper1.className = this._dlg.getOption("_thumbnailWrapper1Class");
        var wrapper2 = this._wrapperBorderElement = document.createElement("DIV");
        wrapper2.className = this._dlg.getOption("_thumbnailWrapper2Class");
        wrapper1.appendChild(wrapper2);
        var wrapper3 = document.createElement("DIV");
        wrapper3.className = this._dlg.getOption("_thumbnailWrapper3Class");
        wrapper2.appendChild(wrapper3);

        var container = document.createElement("DIV");
        container.className = "bx-image-paste-dialog-thumbnail-container";
        wrapper3.appendChild(container);
        var deleteButton = document.createElement("DIV");
        container.appendChild(deleteButton);
        deleteButton.className = this._dlg.getOption("_thumbnailDeleteClass");
        Bitrix.EventUtility.addEventListener(deleteButton, "click", this._deleteHandler);
        var img = document.createElement("IMG");
        container.appendChild(img);
        img.className = this._dlg.getOption("_thumbnailImgClass");
        img.src = this._thumbUrl;
        document.getElementById(this._dlg.getThumbContainerId()).appendChild(wrapper1);
        Bitrix.EventUtility.addEventListener(img, "click", this._clickHandler);
        Bitrix.EventUtility.addEventListener(img, "dblclick", this._dblclickHandler);
    },
    destruct: function() {
        this.setSelected(false);
        if (!this._wrapperElement) return;
        var container = document.getElementById(this._dlg.getThumbContainerId());
        if (!container) return;
        container.removeChild(this._wrapperElement);
    },
    scrollIntoView: function() {
		var c = document.getElementById(this._dlg.getThumbContainerId());	
		c.scrollTop = c.scrollHeight;
    },
    getImageId: function() { return this._imgId; },
    getImageUrl: function() { return this._imgUrl; },
    getThumbnailUrl: function() { return this._thumbUrl; },
    isSelected: function() { return this._isSelected; },
    setSelected: function(selected) {
        //this._isSelected = selected;  if(this._selectedIcon) this._selectedIcon.style.display = this._isSelected ? "" : "none";
        this._isSelected = selected;
        if (!this._wrapperBorderElement) return;
        this._wrapperBorderElement.className = this._isSelected ? "bx-image-paste-dialog-thumbnail-wrapper2-selected" : "bx-image-paste-dialog-thumbnail-wrapper2";
        this._dlg.handleThumbnailSelectionChange(this);
    },
    _handleDeletion: function(e) {
        this._dlg.handleThumbnailDeletion(this);
    },
    _handleClickSelection: function(e) {
        if (this._clickHaddlerTimerId) {
            clearTimeout(this._clickHaddlerTimerId);
            this._clickHaddlerTimerId = null;
        }
        var self = this;
        this._clickHaddlerTimerId = setTimeout(function() { self._clickHaddlerTimerId = null; self.setSelected(!self.isSelected()); }, 100);
    },
    _handleDblClickSelection: function(e) {
        if (this._clickHaddlerTimerId) {
            clearTimeout(this._clickHaddlerTimerId);
            this._clickHaddlerTimerId = null;
        }
        this._dlg.clearThumbnailsSelection(this);
        this.setSelected(true);
        this._dlg._handleClose(Bitrix.Dialog.button.bOk);
        this._dlg.close();
    }
}
Bitrix.ImagePasteDialogThumbnail.create = function(dlg, imgId, imgUrl, thumbUrl){
	var self = new Bitrix.ImagePasteDialogThumbnail();
	self.initialize(dlg, imgId, imgUrl, thumbUrl);
	return self;
}