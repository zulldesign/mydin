if(typeof(Bitrix) == "undefined"){
	var Bitrix = new Object();
}

Bitrix.MediaPlayerType = { auto : 1, flv : 2, wmv: 3 };
Bitrix.MediaPlayerStretchingMode = { none : 0, proportionally : 1, disproportionally: 2, fill: 3 };

if(typeof(Bitrix.EventPublisher) == "undefined"){
    Bitrix.EventPublisher = function Bitrix$EventPublisher(){
	    this._listeners = null;
    }

    Bitrix.EventPublisher.prototype.addListener = function(listener){
	    if(!Bitrix.TypeUtility.isFunction(listener)) throw "listener is not valid!";
	    if(this._listeners == null)
		    this._listeners = new Array();
	    this._listeners.push(listener);	
    }

    Bitrix.EventPublisher.prototype.removeListener = function(listener){
	    if(!Bitrix.TypeUtility.isFunction(listener)) throw "listener is not valid!";
	    if(this._listeners == null) return;
	    var index = -1;
	    for(var i = 0; i < this._listeners.length; i++){
		    if(this._listeners[i] != listener) continue;
		    index = i;
		    break;
	    }
	    if(index < 0) return;
	    this._listeners.splice(index, 1);	
    }

    Bitrix.EventPublisher.prototype.fire = function(){
	    if(this._listeners == null) return;
	    for(var i = 0; i < this._listeners.length; i++){
		    this._listeners[i].apply(this, arguments);
	    }	
    }

    //Bitrix.EventPublisher.registerClass("Bitrix.EventPublisher");
}
Bitrix.FlvMediaPlayerEventDispatcher = function Bitrix$FlvMediaPlayerEventDispatcher(){
	this._initialized = false;
	this._loaded = false;
	this._playerId = null;
	this._playerInfo = null;
	this._loadingCompletionEvent = null;
	this._modelStateChangeEvent = null;
}

Bitrix.FlvMediaPlayerEventDispatcher.prototype = {
	initialize: function(playerId){
		if(!Bitrix.TypeUtility.isNotEmptyString(playerId)) throw "playerId is not valid!";
		this._playerId = playerId;
		this._initialized = true;
	},
	isLoaded: function(){
		return this._loaded;
	},
	getPlayerId: function(){
		return this._playerId;
	},
	addModelStateChangeListener: function(listener){
		if(this._modelStateChangeEvent == null)
			this._modelStateChangeEvent = new Bitrix.EventPublisher();
		this._modelStateChangeEvent.addListener(listener);
	},
	removeModelStateChangeListener: function(listener){
		if(this._modelStateChangeEvent == null) return;
		this._modelStateChangeEvent.removeListener(listener);
	},
	addLoadingCompletionListener: function(listener){
		if(this._loadingCompletionEvent == null)
			this._loadingCompletionEvent = new Bitrix.EventPublisher();
		this._loadingCompletionEvent.addListener(listener);
	},
	removeLoadingCompletionListener: function(listener){
		if(this._loadingCompletionEvent == null) return;
		this._loadingCompletionEvent.removeListener(listener);
	},	
	_handleLoadingCompletion: function(obj){
		if(!obj || typeof(obj) != "object" || !("id" in obj)) throw "obj is not valid!";
		
		if(this._playerId != obj.id) throw "Identifier are mismatched!";
		this._playerInfo = obj;
		this._loaded = true;
		var player = document.getElementById(this._playerId);
		if(!player) throw "Could not find player '" + this._playerId +"'!";
		player.addModelListener("STATE", "Bitrix.FlvMediaPlayerEventDispatcher.modelStateChanged");	
		if(this._loadingCompletionEvent != null) this._loadingCompletionEvent.fire(this);
	},
	_handleModelStateChange: function(obj){
		if(this._playerInfo == null) return;
		var id = obj && "id" in obj ? obj["id"] : null;
		if(!id || id != this._playerId) return;
		if(this._modelStateChangeEvent != null) this._modelStateChangeEvent.fire(obj);	
	},
	play: function(){
		if(!this.isLoaded()) throw "Player '" + this._playerId +"' is not loaded!";
		var player = document.getElementById(this._playerId);
		if(!player) throw "Could not find player '" + this._playerId +"'!";
		player.sendEvent("PLAY", "true");
	},		
	stop: function(){
		if(!this.isLoaded()) throw "Player '" + this._playerId +"' is not loaded!";
		var player = document.getElementById(this._playerId);
		if(!player) throw "Could not find player '" + this._playerId +"'!";	
		player.sendEvent("STOP");
	}	
}

Bitrix.FlvMediaPlayerEventDispatcher._entries = null;
Bitrix.FlvMediaPlayerEventDispatcher.getEntryByPlayerId = function(playerId){
	if(!Bitrix.TypeUtility.isNotEmptyString(playerId)) throw "playerId is not valid!";
	return this._entries != null && playerId in this._entries ? this._entries[playerId] : null;
}

Bitrix.FlvMediaPlayerEventDispatcher.handleLoadingCompletion = function(playerInfo){
	if(!playerInfo || typeof(playerInfo) != "object" || !("id" in playerInfo)) throw "playerInfo is not valid!";
	this.ensureEntryCreated(playerInfo.id)._handleLoadingCompletion(playerInfo);
}
Bitrix.FlvMediaPlayerEventDispatcher.ensureEntryCreated = function(playerId){
	if(!Bitrix.TypeUtility.isNotEmptyString(playerId)) throw "playerId is not valid!";
	var self = this.getEntryByPlayerId(playerId);
	if(self == null){
		self = new Bitrix.FlvMediaPlayerEventDispatcher();
		self.initialize(playerId);
		if(this._entries == null)
			this._entries = new Object();
		this._entries[self.getPlayerId()] = self;		
	}
	return self;
}

Bitrix.FlvMediaPlayerEventDispatcher.removeEntry = function(playerId){
	if(!Bitrix.TypeUtility.isNotEmptyString(playerId)) throw "playerId is not valid!";
	if(!(this._entries != null && playerId in this._entries)) return;
	delete this._entries[playerId];
}

Bitrix.FlvMediaPlayerEventDispatcher.addModelStateChangeListener = function(playerId, listener){
	this.ensureEntryCreated(playerId).addModelStateChangeListener(listener);
}

Bitrix.FlvMediaPlayerEventDispatcher.removeModelStateChangeListener = function(playerId, listener){
	var entry = this.getEntryByPlayerId(playerId);
	if(entry == null) return;
	entry.removeModelStateChangeListener(listener);
}

Bitrix.FlvMediaPlayerEventDispatcher.modelStateChanged = function(obj){
	var id = obj != null && "id" in obj ? obj["id"] : null;
	if(!id) return;
	var entry = this.getEntryByPlayerId(obj.id);
	if(!entry) return;
	entry._handleModelStateChange(obj);
};

//Bitrix.FlvMediaPlayerEventDispatcher.registerClass("Bitrix.FlvMediaPlayerEventDispatcher");

//JW Flash Player load complete global handler
function playerReady(obj){
	Bitrix.FlvMediaPlayerEventDispatcher.handleLoadingCompletion(obj);
};

Bitrix.WmvMediaPlayerEventDispatcher = function Bitrix$WmvMediaPlayerEventDispatcher(){
	this._initialized = false;
	this._loaded = false;
	this._playerElementId = null;
	this._playerObject = null;	
	this._stateChangeEvent = null;
	this._loadingCompletionEvent = null;
	this._stateChangeHandler = Bitrix.TypeUtility.createDelegate(this, this._handleStateChange);
	this._loadingHandler = Bitrix.TypeUtility.createDelegate(this, this.ensureLoaded);
	this._loadingTryCount = 0;
}

Bitrix.WmvMediaPlayerEventDispatcher.prototype = {
	initialize: function(playerElementId, playerObject){
		if(!Bitrix.TypeUtility.isNotEmptyString(playerElementId)) throw "playerElementId is not valid!";
		if(typeof(playerObject) != "object") throw "playerObject is not valid!";
		this._playerElementId = playerElementId;
		this._playerObject = playerObject;
		this._initialized = true;
	},
	getPlayerElementId: function(){
		return this._playerElementId;
	},
	getPlayerObject: function(){
		return this._playerObject;
	},	
	isLoaded: function(){
		return this._loaded;
	},
	addStateChangeListener: function(listener){
		if(this._stateChangeEvent == null)
			this._stateChangeEvent = new Bitrix.EventPublisher();
		this._stateChangeEvent.addListener(listener);
	},
	removeStateChangeListener: function(listener){
		if(this._stateChangeEvent == null) return;
		this._stateChangeEvent.removeListener(listener);
	},
	ensureLoaded: function(){
		if(this._loaded) return;
		//if(this._loadingTryCount >= 150)
		//	throw "Limit of tries has been exceeded!";
		if("view" in this._playerObject){
			this._loadingTryCount = 0;
			this._playerObject.addListener("STATE", this._stateChangeHandler);
			this._loaded = true;
			this._playerObject.addListener("LOAD", this._fileLoadingHandler);
			if(this._loadingCompletionEvent != null) this._loadingCompletionEvent.fire(this);
			return;
		}
		this._loadingTryCount++;
		window.setTimeout(this._loadingHandler, this._loadingTryCount * 5);
	},
	addLoadingCompletionListener: function(listener){
		if(this._loadingCompletionEvent == null)
			this._loadingCompletionEvent = new Bitrix.EventPublisher();
		this._loadingCompletionEvent.addListener(listener);
	},
	removeLoadingCompletionListener: function(listener){
		if(this._loadingCompletionEvent == null) return;
		this._loadingCompletionEvent.removeListener(listener);
	},	
	_handleStateChange: function(oldState, newState){
		if(this._stateChangeEvent != null) this._stateChangeEvent.fire(oldState, newState);	
	},
	loadFile: function(url){
		if(!this.isLoaded()) throw "Player '" + this._playerId +"' is not loaded!";
		this._playerObject.sendEvent("LOAD", url);		
	},
	play: function(){
		if(!this.isLoaded()) throw "Player '" + this._playerId +"' is not loaded!";
		this._playerObject.sendEvent("PLAY");
	},	
	stop: function(){
		if(!this.isLoaded()) throw "Player '" + this._playerId +"' is not loaded!";
		this._playerObject.sendEvent("STOP");
	}
}

Bitrix.WmvMediaPlayerEventDispatcher._entries = null;
Bitrix.WmvMediaPlayerEventDispatcher.getEntryByPlayerElementId = function(playerElementId){
	if(!Bitrix.TypeUtility.isNotEmptyString(playerElementId)) throw "playerElementId is not valid!";
	return this._entries != null && playerElementId in this._entries ? this._entries[playerElementId] : null;
}

Bitrix.WmvMediaPlayerEventDispatcher.ensureEntryCreated = function(playerElementId, playerObject){
	if(!Bitrix.TypeUtility.isNotEmptyString(playerElementId)) throw "playerElementId is not valid!";
	var self = this.getEntryByPlayerElementId(playerElementId);
	if(self == null){
		self = new Bitrix.WmvMediaPlayerEventDispatcher();
		self.initialize(playerElementId, playerObject);
		if(this._entries == null)
			this._entries = new Object();
		this._entries[self.getPlayerElementId()] = self;	
		self.ensureLoaded();
	}
	return self;
}

Bitrix.WmvMediaPlayerEventDispatcher.removeEntry = function(playerElementId){
	if(!Bitrix.TypeUtility.isNotEmptyString(playerElementId)) throw "playerElementId is not valid!";
	if(!(this._entries != null && playerElementId in this._entries)) return;
	delete this._entries[playerElementId];
}

Bitrix.WmvMediaPlayerEventDispatcher.addStateChangeListener = function(playerElementId, playerObject, listener){
	this.ensureEntryCreated(playerElementId, playerObject).addStateChangeListener(listener);
}

Bitrix.WmvMediaPlayerEventDispatcher.removeStateChangeListener = function(playerElementId, listener){
	var entry = this.getEntryByPlayerElementId(playerElementId);
	if(!entry) return;
	entry.removeStateChangeListener(listener);
}

//Bitrix.WmvMediaPlayerEventDispatcher.registerClass("Bitrix.WmvMediaPlayerEventDispatcher");

Bitrix.FlvMediaPlayerView = function Bitrix$FlvMediaPlayerView(){
	//Bitrix.FlvMediaPlayerView.initializeBase(this);
	this._initialized = false;
	this._activated = false;
	this._id = null;
	this._element = null;
	this._containerElement = null;
	this._jwPlayerFileRelativePath = "~/bitrix/controls/main/media.player/flv/player.swf";
	this._playerInfo = null;
	this._visible = true;
}

Bitrix.FlvMediaPlayerView.prototype = {
	initialize: function(id){
		this.setId(id);
		this._initialized = true;	
	},
	isActivated: function(){
		return this._activated;
	},	
	getPlayerInfo: function(){
		return this._playerInfo;
	},
	getId: function(id){
		return this._id;
	},
	setId: function(id){
		if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "id is not valid!";
		this._id = id;
	},
	getElement: function(){
		return this._element;
	},
	getElementId: function(){
		return this._element ? this._element.id : this._id + '_object';
	},	
	getMediaPlayerType: function(){
		return Bitrix.MediaPlayerType.flv;
	},
	_getCodeBaseUrl: function(){
		return "http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=9,0,0,0";
	},
	stub: function(parentNode, msg){
		if(!Bitrix.TypeUtility.isDomElement(parentNode)) throw "parentNode is not valid!";
		parentNode.innerHTML = msg;
	},
	getContainerElementId: function(){
		return this._containerElement != null ? this._containerElement.id : this._id + "_container";
	},	
	activate: function(parentNode, url, width, height, params){
		if(!Bitrix.TypeUtility.isDomElement(parentNode)) throw "parentNode is not valid!";
		if(typeof(params) != "object") throw "params is not valid!";
		if(this._activated) return;
		if(!Bitrix.TypeUtility.isNotEmptyString(width)) width = "425px";
		if(!Bitrix.TypeUtility.isNotEmptyString(height)) height = "344px";
		try{
			var elId = this.getElementId(); 	
			var containerElId = this.getContainerElementId();
			var hostingName = Bitrix.VideoHostingUtility.getHostingName(url);
			if(hostingName == "YOUTUBE" || hostingName == "VIMEO" || hostingName == "RUTUBE" || hostingName == "YANDEX"){
				var playerUrl = Bitrix.VideoHostingUtility.getVideoDataUrl(url);
				
				this._containerElement = document.createElement('DIV');
				this._containerElement.id = containerElId;
				this._containerElement.style.display = this._visible ? "" : "none";
				parentNode.appendChild(this._containerElement);
				
				var movieParamName = "movie"; //youtube & vimeo & rutube
				if(hostingName == "YANDEX") movieParamName = "video";

				var flashObjContainer = this._containerElement;
				if("audioOnly" in params && params["audioOnly"]){					
					var playerContainer = document.createElement("DIV");
					this._containerElement.appendChild(playerContainer);
					//playerContainer.className = this.getOption("_playerContainerClass");
					playerContainer.className = "bx-audio-player-container";
					var playerScreenStub = document.createElement("DIV");
					//playerScreenStub.className = this.getOption("_playerScrenStubClass");
					playerScreenStub.className = "bx-audio-player-screen-stub";
					playerScreenStub.style.width = width;
					playerContainer.appendChild(playerScreenStub);
					
					var playerInnerContainer = document.createElement("DIV");
					//playerInnerContainer.className = this.getOption("_playerInnerContainerClass");
					playerInnerContainer.className = "bx-audio-player-inner-container";
					playerContainer.appendChild(playerInnerContainer);
					flashObjContainer = playerInnerContainer;
				}

				if(/*@cc_on!@*/false){ //IE	
					var objectStr = '<OBJECT';
					if(hostingName != "YANDEX")
						objectStr = objectStr.concat(' classid="clsid:D27CDB6E-AE6D-11cf-96B8-444553540000" codebase="', this._getCodeBaseUrl(), '"');
					objectStr = objectStr.concat(' id="', elId, '" name="', elId + '" width="', width.toString(), '" height="', height.toString(), '" >');
					objectStr = objectStr.concat(this._createNewParameter(movieParamName, playerUrl));
					objectStr = objectStr.concat(this._createNewParameter("allowscriptaccess", "always"));
			
					if(hostingName == "YANDEX")
						objectStr = objectStr.concat(this._createNewParameter("scale", "noscale"));
					else if("wmode" in params)
						objectStr = objectStr.concat(this._createNewParameter("wmode", params["wmode"]));
				
					objectStr = objectStr.concat( this._createNewParameter("allowfullscreen", ("fullscreen" in params ? params["fullscreen"] : true)));								
					objectStr = objectStr.concat('<EMBED');
					objectStr = objectStr.concat(' id="', this._id, '_embed', '"');
					objectStr = objectStr.concat(' name="', this._id, '_embed', '"');
					if(hostingName == "YANDEX")
						objectStr = objectStr.concat(' scale="noscale"');						
					else{
						objectStr = objectStr.concat(' pluginspage="http://www.adobe.com/go/getflashplayer"');
						if("wmode" in params)
							objectStr = objectStr.concat(' wmode="' + params["wmode"] + '"');						
					}
					objectStr = objectStr.concat(' width="', width.toString(), '"');
					objectStr = objectStr.concat(' height="', height.toString(), '"');
					objectStr = objectStr.concat(' src="', playerUrl, '"');
					objectStr = objectStr.concat(' type="application/x-shockwave-flash"');
					objectStr = objectStr.concat(' allowscriptaccess="always"');
					objectStr = objectStr.concat('></EMBED>');					
					
					objectStr = objectStr.concat('</OBJECT>');
					flashObjContainer.innerHTML = objectStr;
					this._element = flashObjContainer.firstChild;				
				}
				else{
					this._element = document.createElement('OBJECT');
					this._element.setAttribute("id", elId);	
					this._element.setAttribute("name", elId);				
					this._element.setAttribute("width", width);
					this._element.setAttribute("height", height);				
				
					this._element.appendChild(this._createNewParameter(movieParamName, playerUrl));						
					if("wmode" in params)
						this._element.appendChild(this._createNewParameter("wmode", params["wmode"]));	
					this._element.appendChild(this._createNewParameter("allowfullscreen", "fullscreen" in params ? params["fullscreen"] : true));	
					this._element.appendChild(this._createNewParameter("allowscriptaccess", "always"));
					
					var embed = document.createElement('EMBED');
					embed.setAttribute("src", playerUrl);
					embed.setAttribute("id", this._id + '_emb');
					embed.setAttribute("name", this._id + '_emb');
					embed.setAttribute("width", width);
					embed.setAttribute("height", height);			
					embed.setAttribute("type", "application/x-shockwave-flash");
					embed.setAttribute("pluginspage", "http://www.adobe.com/go/getflashplayer");
					embed.setAttribute("allowscriptaccess", "always");
					if(hostingName == "YANDEX")
						embed.setAttribute("scale", "noscale");
					
					if("wmode" in params)
						embed.setAttribute("wmode", params["wmode"]);
					this._element.appendChild(embed);	
					flashObjContainer.appendChild(this._element);				
				}

				this._activated = true;
				return;
			}
				
			var flashVars = "";

			var flashVarsArr = new Array();
			//flashVarsArr.push(this._createFlashVarsItem("appname", "Adobe Media Player"));
			//flashVarsArr.push(this._createFlashVarsItem("appurl", "http://airdownload.adobe.com/air/amp/pdc/adobe_media_player.air"));
			flashVarsArr.push(this._createFlashVarsItem("file", Bitrix.VideoHostingUtility.getVideoDataUrl(url)));
			
			this._addFlashVarParameter("autostart", params, flashVarsArr);	
			this._addFlashVarParameter("fullscreen", params, flashVarsArr);
			this._addFlashVarParameter("skin", params, flashVarsArr);
			this._addFlashVarParameter("controlbar", params, flashVarsArr);
			this._addFlashVarParameter("repeat", params, flashVarsArr);
			this._addFlashVarParameter("volume", params, flashVarsArr);
			this._addFlashVarParameter("mute", params, flashVarsArr);
			this._addFlashVarParameter("quality", params, flashVarsArr);
			this._addFlashVarParameter("bufferlength", params, flashVarsArr);
			this._addFlashVarParameter("link", params, flashVarsArr);
			this._addFlashVarParameter("linktarget", params, flashVarsArr);
			this._addFlashVarParameter("abouttext", params, flashVarsArr);
			this._addFlashVarParameter("aboutlink", params, flashVarsArr);
			this._addFlashVarParameter("type", params, flashVarsArr);
			this._addFlashVarParameter("image", params, flashVarsArr);
			this._addFlashVarParameter("logo", params, flashVarsArr);	
			this._addFlashVarParameter("displayclick", params, flashVarsArr);				
			this._addFlashVarParameter("stretching", params, flashVarsArr);	 
			this._addFlashVarParameter("backcolor", params, flashVarsArr);	 
			this._addFlashVarParameter("frontcolor", params, flashVarsArr);	 
			this._addFlashVarParameter("lightcolor", params, flashVarsArr);	 
			this._addFlashVarParameter("screencolor", params, flashVarsArr);	 			
			
			flashVars = flashVarsArr.join("&");
			delete flashVarsArr;
			var playerFileUrl = Bitrix.PathHelper.resolveVirtualPath(this._jwPlayerFileRelativePath); 			
			
			this._containerElement = document.createElement('DIV');
			this._containerElement.id = containerElId;
			this._containerElement.style.display = this._visible ? "" : "none";
			parentNode.appendChild(this._containerElement);			
			if(/*@cc_on!@*/false){ //IE	
				var objectStr = '<OBJECT classid="clsid:D27CDB6E-AE6D-11cf-96B8-444553540000"' + ' id="' + elId + '" name="' + elId + '" codebase="' + this._getCodeBaseUrl() + '"';
				objectStr = objectStr.concat(' width="', width, '" height="', height, '"><PARAM name="movie" value="', playerFileUrl, '" />');
				
				if("wmode" in params)
					objectStr = objectStr.concat(this._createNewParameter("wmode", params["wmode"]));		
					
				if("menu" in params)
					objectStr = objectStr.concat(this._createNewParameter("menu", params["menu"]));
				
				objectStr = objectStr.concat(this._createNewParameter("allowfullscreen", "fullscreen" in params ? params["fullscreen"] : true));			
				objectStr = objectStr.concat(this._createNewParameter("allowScriptAccess", "always"));
				
				if("volume" in params)
					objectStr = objectStr.concat(this._createNewParameter("volume", params["volume"]));			
				
				if(flashVars.length > 0)
					objectStr = objectStr.concat(this._createNewParameter("flashvars", flashVars));
				
			    objectStr = objectStr.concat('</OBJECT>');
			    this._containerElement.innerHTML = objectStr;
				this._element = this._containerElement.firstChild;
			}
			else{
				this._element = document.createElement('OBJECT');
				this._element.setAttribute("type", "application/x-shockwave-flash");
				this._element.setAttribute("codebase", + this._getCodeBaseUrl() +"'");
				this._element.setAttribute("id", elId);	
				this._element.setAttribute("name", elId);				
				this._element.setAttribute("width", width);
				this._element.setAttribute("height", height);				
				this._element.setAttribute("data", playerFileUrl);
				
				if("wmode" in params)
					this._element.appendChild(this._createNewParameter("wmode", params["wmode"]));		

				if("menu" in params)
					this._element.appendChild(this._createNewParameter("menu", params["menu"]));
				
				this._element.appendChild(this._createNewParameter("allowfullscreen", "fullscreen" in params ? params["fullscreen"] : true));			
				this._element.appendChild(this._createNewParameter("allowScriptAccess", "always"));
				
				if("volume" in params)
					this._element.appendChild(this._createNewParameter("volume", params["volume"]));	
					
				//this._element.appendChild(this._createNewParameter("play", "false"));	
				if(flashVars.length > 0)
					this._element.appendChild(this._createNewParameter("flashvars", flashVars));		
				
				this._containerElement.appendChild(this._element);								
			}
			this._activated = true;
		}
		catch(e){
			this.stub(parentNode, Bitrix.MediaPlayer.messages["couldntCreateFlashPlayer"]);
			this._containerElement = null;
			this._element = null;			
		}	
	},
	deactivate: function(){
		if(!this._activated) return;
		var parentNode = this._containerElement != null ? this._containerElement.parentNode : null;
		if(!parentNode) 
			return;
		parentNode.removeChild(this._containerElement);
		this._containerElement = null;
		this._element = null;
		this._activated = false;
	},	
	_createNewParameter: function(name, value){
		if(/*@cc_on !@*/false) //IE
			return '<PARAM name="' + name + '" value="' + value + '" />'
		var r = document.createElement('PARAM');
		r.setAttribute("name", name);
		r.setAttribute("value", value.toString());
		return r;
	},	
	_addFlashVarParameter: function(name, srcDic, dstArr){
		if(!(name in srcDic))
			return;
		dstArr.push(this._createFlashVarsItem(name, srcDic[name]));			
	},
	_createFlashVarsItem: function(name, value){
		if(!Bitrix.TypeUtility.isString(value))
			value = Bitrix.TypeUtility.tryConvertToString(value);
			
		if(value.length > 0){
			value = value.replace(/[\s]/g, "+");
			value = value.replace(/[\?]/g, "%3F");
			value = value.replace(/[\=]/g, "%3D");
			value = value.replace(/[\&]/, "%26");
		}
		return name + "=" + value;
	},
	isVisible: function(){
		return this._visible;
	},
	setVisible: function(visible){
		if(this._visible == visible) return;
		this._visible = visible;
		if(!this._containerElement) return;
		this._containerElement.style.display = visible ? "" : "none"; 
	}	
}

Bitrix.FlvMediaPlayerView.create = function(id){
	var self = new Bitrix.FlvMediaPlayerView();
	self.initialize(id);
	return self;
}

//Bitrix.FlvMediaPlayerView.registerClass("Bitrix.FlvMediaPlayerView");
	
Bitrix.FlvMediaPlayerController = function Bitrix$FlvMediaPlayerController(){
	//Bitrix.FlvMediaPlayerController.initializeBase(this);
	this._initialized = false;
	this._activated = false;
	this._id = null;
	this._data = null;
	this._parentNode = null;
	this._activeView = null;
	this._modelStateChangeEvent = null;
	this._loadingCompletionEvent = null;
	this._modelStateChangeHandler = Bitrix.TypeUtility.createDelegate(this, this._handleModelStateChange);
	this._loadingCompletionHandler = Bitrix.TypeUtility.createDelegate(this, this._handleLoadingCompletionHandler);
	this._playAfterLoading = false;
}

Bitrix.FlvMediaPlayerController.prototype = {
    initialize: function(id, data) {
        this.setId(id);
        this.setData(data);
        this._initialized = true;
    },
    isActivated: function() {
        return this._activated;
    },
    getId: function(id) {
        return this._id;
    },
    setId: function(id) {
        if (!Bitrix.TypeUtility.isNotEmptyString(id)) throw "id is not valid!";
        this._id = id;
    },
    getData: function() {
        return this._data;
    },
    setData: function(data) {
        if (!data) throw "data is not valid!";
        var type = data.getMediaPlayerType();
        if (type != Bitrix.MediaPlayerType.flv)
            throw "Bitrix.MediaPlayerType '" + type + "' is not supported!";
        this._data = data;
    },
    getMediaPlayerType: function() {
        return Bitrix.MediaPlayerType.flv;
    },
    activate: function(parentNode) {
        if (!this._initialized) throw "Is not initialized!";
        if (this._activated) return;
        var viewId = this._id + "_flv";
        this._activeView = Bitrix.FlvMediaPlayerView.create(viewId);

        //this._activeView.setVisible(false);
        this._parentNode = parentNode;
        var flashVer = this._getFlashVersion();
        if (flashVer < 9) {
            this._activeView.stub(parentNode, Bitrix.MediaPlayer.messages["installFlashPlayer"]);
            return;
        }

        this._activeView.activate(parentNode, this._data.getUrl(), this._data.getWidth(), this._data.getHeight(), this._data.getParamsByMediaPlayerType(Bitrix.MediaPlayerType.flv));
        this._parentNode = parentNode;

        var dispatcher = Bitrix.FlvMediaPlayerEventDispatcher.ensureEntryCreated(this._activeView.getElementId());
        dispatcher.addLoadingCompletionListener(this._loadingCompletionHandler);
        dispatcher.addModelStateChangeListener(this._modelStateChangeHandler);
        this._activated = true;
    },
    deactivate: function() {
        if (!this._initialized) throw "Is not initialized!";
        if (!this._activated) return;
        if (this._activeView == null) return;
        //this.stop();
        var elId = this._activeView.getElementId();
        Bitrix.FlvMediaPlayerEventDispatcher.removeModelStateChangeListener(elId, this._modelStateChangeHandler);
        Bitrix.FlvMediaPlayerEventDispatcher.removeEntry(elId);
        this._activeView.deactivate();
        delete this._activeView;
        this._activeView = null;
        this._parentNode = null;
        this._activated = false;
    },
    _getFlashVersion: function() {
        var n = navigator;
        if (/*@cc_on!@*/false) {
            for (var i = 9; i > 2; i--) {
                try {
                    if (new ActiveXObject("ShockwaveFlash.ShockwaveFlash." + i))
                        return i;
                }
                catch (e) { }
            }
        }
        else if (n.plugins) {
            for (var i = 0, l = n.plugins.length; i < l; i++)
                if (n.plugins[i].name.indexOf('Flash') != -1) {
                var r = parseInt(n.plugins[i].description.substr(16, 2));
                return !isNaN(r) ? r : -1;
            }
        }
        return -1;
    },
    addStateChangeListener: function(listener) {
        if (this._modelStateChangeEvent == null)
            this._modelStateChangeEvent = new Bitrix.EventPublisher();
        this._modelStateChangeEvent.addListener(listener);
    },
    removeStateChangeListener: function(listener) {
        if (this._modelStateChangeEvent == null) return;
        this._modelStateChangeEvent.removeListener(listener);
    },
    addLoadingCompletionListener: function(listener) {
        if (this._loadingCompletionEvent == null)
            this._loadingCompletionEvent = new Bitrix.EventPublisher();
        this._loadingCompletionEvent.addListener(listener);
    },
    removeLoadingCompletionListener: function(listener) {
        if (this._loadingCompletionEvent == null) return;
        this._loadingCompletionEvent.removeListener(listener);
    },
    _handleModelStateChange: function(obj) {
        var args = new Object();
        args["obj"] = obj;
        if (this._modelStateChangeEvent != null) this._modelStateChangeEvent.fire(this, args);
    },
    _handleLoadingCompletionHandler: function(dispatcher) {
        if (!dispatcher) return;
        dispatcher.removeLoadingCompletionListener(this._loadingCompletionHandler);
        if (this._playAfterLoading) {
            this.play();
            this._playAfterLoading = false;
        }
        if (this._loadingCompletionEvent != null) this._loadingCompletionEvent.fire(this);
    },
    isLoaded: function() {
        var dispatcher = this._activeView ? Bitrix.FlvMediaPlayerEventDispatcher.getEntryByPlayerId(this._activeView.getElementId()) : null;
        return dispatcher ? dispatcher.isLoaded() : false;
    },
    play: function() {
        var dispatcher = this._activeView ? Bitrix.FlvMediaPlayerEventDispatcher.getEntryByPlayerId(this._activeView.getElementId()) : null;
        if (!dispatcher) return;
        if (!dispatcher.isLoaded()) {
            this._playAfterLoading = true;
            return;
        }
        dispatcher.play();
    },
    stop: function() {
        var dispatcher = this._activeView ? Bitrix.FlvMediaPlayerEventDispatcher.getEntryByPlayerId(this._activeView.getElementId()) : null;
        if (!dispatcher) return;
        dispatcher.stop();
    },
    setViewVisible: function(visible) {
        if (this._activeView)
            this._activeView.setVisible(visible);
    }
}

Bitrix.FlvMediaPlayerController.create = function(id, data){
	var self = new Bitrix.FlvMediaPlayerController();
	self.initialize(id, data);
	return self;
}

//Bitrix.FlvMediaPlayerController.registerClass("Bitrix.FlvMediaPlayerController");

Bitrix.WmvMediaPlayerView = function Bitrix$WmvMediaPlayerView(){
	//Bitrix.WmvMediaPlayerView.initializeBase(this);
	this._initialized = false;
	this._activated;
	this._id = null;
	this._element = null;
	this._object = null;
	this._jwPlayerFileRelativePath = "~/bitrix/controls/Main/media.player/wmv/wmvplayer.xaml";
	this._containerElement = null;
}

Bitrix.WmvMediaPlayerView.prototype = {
	initialize: function(id){
		if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "id is not valid!";
		this._id = id;	
		this._initialized = true;	
	},
	isActivated: function(){
		return this._activated;
	},
	getId: function(id){
		return this._id;
	},
	getPlayerElement: function(){
		return this._element;
	},
	getPlayerElementId: function(){
		return this._element ? this._element.id : this._id + '_view';
	},	
	getPlayerObject: function(){
		return this._object;
	},	
	getMediaPlayerType: function(){
		return Bitrix.MediaPlayerType.wmv;
	},
	stub: function(parentNode, msg){
		if(!Bitrix.TypeUtility.isDomElement(parentNode)) throw "parentNode is not valid!";
		parentNode.innerHTML = mgs;
	},
	getContainerElementId: function(){
		return this._containerElement != null ? this._containerElement.id : this._id + "_container";
	},
	activate: function(parentNode, url, width, height, params){	
		if(!Bitrix.TypeUtility.isDomElement(parentNode)) throw "parentNode is not valid!";
		if(typeof(params) != "object") throw "params is not valid!";
		if(this._activated) return;	
		try{
			//var index = 1;
			var containerId = this.getContainerElementId();
			//while(document.getElementById(containerId + index.toString()))
			//	index++;
			this._containerElement = document.createElement("DIV");
			//this._containerElement.id = containerId + index.toString();			
			this._containerElement.id = containerId;	
			
			var presentationSettingsFilePath = Bitrix.PathHelper.resolveVirtualPath(this._jwPlayerFileRelativePath);
			var config = {};
			config["file"] = url;
			if(Bitrix.TypeUtility.isNotEmptyString(width)) config["width"] = width;
			if(Bitrix.TypeUtility.isNotEmptyString(height)) config["height"] = height;
			
			for(var key in params){
			    if(key == "windowless" && (Bitrix.NavigationHelper.isSafari() || Bitrix.NavigationHelper.isOpera())){
			        config["windowless"] = false; //due to Safari and Opera bugs
			        continue;
			    }
				this._tryAddParam2Config(key, params, config);	
			}
					
			this._object = new jeroenwijering.Player(this._containerElement, presentationSettingsFilePath,  config);
			
			var elementId = this.getPlayerElementId();
			this._element = this._containerElement.firstChild;
			//this._element.id = elementId + index.toString();
			//this._element.setAttribute("name", elementId + index.toString());
			
			this._element.id = elementId;
			this._element.setAttribute("name", elementId);			
			parentNode.appendChild(this._containerElement);	
			this._activated = true;
		}
		catch(e){
			this.stub(parentNode, Bitrix.MediaPlayer.messages["couldntCreateSilverlightPlayer"]);
			this._element = null;
		}
	},
	_tryAddParam2Config: function(paramName, params, config){
		if(!(paramName in params))
			return false;
		var value = Bitrix.TypeUtility.tryConvertToString(params[paramName]);
		config[paramName] = value;
		return true;
	},
	deactivate: function(){
		if(!this._activated) return;
		var parentNode = this._containerElement != null ? this._containerElement.parentNode : null;
		if(!parentNode) 
			return;
		parentNode.removeChild(this._containerElement);
		this._containerElement = null;
		this._element = null;
		this._object = null;
		this._activated = false;		
	},
	setVisible: function(visible){
		if(!this._containerElement) return;
		this._containerElement.style.display = visible ? "" : "none"; 
	}
}

Bitrix.WmvMediaPlayerView.create = function(id){
	var self = new Bitrix.WmvMediaPlayerView();
	self.initialize(id);
	return self;
}
	
//Bitrix.WmvMediaPlayerView.registerClass("Bitrix.WmvMediaPlayerView");

Bitrix.WmvMediaPlayerController = function Bitrix$WmvMediaPlayerController(){
	//Bitrix.WmvMediaPlayerController.initializeBase(this);
	this._initialized = false;
	this._activated = false;
	this._id = null;
	this._data = null;
	this._parentNode = null;
	this._activeView = null;
	this._stateChangeEvent = null;
	this._loadingCompletionEvent = null;
	this._stateChangeHandler = Bitrix.TypeUtility.createDelegate(this, this._handleStateChange);	
	this._loadingCompletionHandler = Bitrix.TypeUtility.createDelegate(this, this._handleLoadingCompletionHandler);
	this._playAfterLoading = false;
	this._loadFileAfterLoading = false;
}

Bitrix.WmvMediaPlayerController.prototype = {
	initialize: function(id, data){
		this.setId(id);
		this.setData(data);
		this._initialized = true;	
	},
	isActivated: function(){
		return this._activated;
	},	
	getId: function(id){
		return this._id;
	},
	setId: function(id){
		if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "id is not valid!";
		this._id = id;
	},	
	getData: function(){
		return this._data;
	},
	setData: function(data){
		if(!data) throw "data is not valid!";
		var type = data.getMediaPlayerType();
		if(type != this.getMediaPlayerType())
			throw "Bitrix.MediaPlayerType '" + type + "' is not supported!";	
		this._data = data;
	},
	getMediaPlayerType: function(){
		return Bitrix.MediaPlayerType.wmv;
	},
	activate: function(parentNode){
		if(!this._initialized) throw "Is not initialized!";
		if(this._activated) return;
		var viewId = this._id + "_wmv";
		if(this._activeView == null)
			this._activeView = Bitrix.WmvMediaPlayerView.create(viewId);
		this._activeView.activate(parentNode, this._data.getUrl(), this._data.getWidth(), this._data.getHeight(), this._data.getParamsByMediaPlayerType(this.getMediaPlayerType()));
		this._parentNode = parentNode;
		
		var dispatcher = Bitrix.WmvMediaPlayerEventDispatcher.ensureEntryCreated(this._activeView.getPlayerElementId(), this._activeView.getPlayerObject());	
		dispatcher.addLoadingCompletionListener(this._loadingCompletionHandler);
		dispatcher.addStateChangeListener(this._stateChangeHandler);
		this._activated = true;
	},
	deactivate: function(){
		if(this._activeView == null) return;
		if(!this._activated) return;
		//this.stop();
		Bitrix.WmvMediaPlayerEventDispatcher.removeStateChangeListener(this._activeView.getPlayerElementId(), this._stateChangeHandler);
		Bitrix.WmvMediaPlayerEventDispatcher.removeEntry(this._activeView.getPlayerElementId());
		this._activeView.deactivate();
		//delete this._activeView;
		//this._activeView = null;
		this._parentNode = null;
		this._activated = false;
	},
	addStateChangeListener: function(listener){
		if(this._stateChangeEvent == null)
			this._stateChangeEvent = new Bitrix.EventPublisher();
		this._stateChangeEvent.addListener(listener);
	},
	removeStateChangeListener: function(listener){
		if(this._stateChangeEvent == null) return;
		this._stateChangeEvent.removeListener(listener);
	},
	addLoadingCompletionListener: function(listener){
		if(this._loadingCompletionEvent == null)
			this._loadingCompletionEvent = new Bitrix.EventPublisher();
		this._loadingCompletionEvent.addListener(listener);
	},
	removeLoadingCompletionListener: function(listener){
		if(this._loadingCompletionEvent == null) return;
		this._loadingCompletionEvent.removeListener(listener);
	},	
	_handleLoadingCompletionHandler: function(dispatcher){
		if(!dispatcher) return;
		if(this._loadFileAfterLoading){
			this.load();
			this._loadFileAfterLoading = false;
		}
		if(this._playAfterLoading){
			this.play();
			this._playAfterLoading = false;
		}
		dispatcher.removeLoadingCompletionListener(this._loadingCompletionHandler);
		if(this._loadingCompletionEvent != null) this._loadingCompletionEvent.fire(this);
	},
	_handleStateChange: function(oldState, newState){
		var args = new Object();
		args["oldState"] = oldState;
		args["newState"] = newState;
		if(this._stateChangeEvent != null) this._stateChangeEvent.fire(this, args);		
	},
	isLoaded: function(){
		var dispatcher = this._activeView ? Bitrix.WmvMediaPlayerEventDispatcher.getEntryByPlayerElementId(this._activeView.getPlayerElementId()) : null;
		return dispatcher ? dispatcher.isLoaded() : false;
	},
	loadFile: function(){
		var dispatcher = this._activeView ? Bitrix.WmvMediaPlayerEventDispatcher.getEntryByPlayerElementId(this._activeView.getPlayerElementId()) : null;
		if(!dispatcher) return;
		if(!dispatcher.isLoaded()){
			this._loadFileAfterLoading = true;
			return;
		}		
		dispatcher.loadFile(this._data.getUrl());		
	},
	play: function(){
		var dispatcher = this._activeView ? Bitrix.WmvMediaPlayerEventDispatcher.getEntryByPlayerElementId(this._activeView.getPlayerElementId()) : null;
		if(!dispatcher) return;
		if(!dispatcher.isLoaded()){
			this._playAfterLoading = true;
			return;
		}		
		dispatcher.play();
	},	
	stop: function(){
		var dispatcher = this._activeView ? Bitrix.WmvMediaPlayerEventDispatcher.getEntryByPlayerElementId(this._activeView.getPlayerElementId()) : null;
		if(!dispatcher) return;
		dispatcher.stop();
	},
	setViewVisible: function(visible){
		if(this._activeView)
			this._activeView.setVisible(visible);
	}		
}

Bitrix.WmvMediaPlayerController.create = function(id, data){
	var self = new Bitrix.WmvMediaPlayerController();
	self.initialize(id, data);
	return self;
}

//Bitrix.WmvMediaPlayerController.registerClass("Bitrix.WmvMediaPlayerController");

Bitrix.MediaPlayerState = { UNDEFINED:0, IDLE:1, BUFFERING:2, PLAYING:3, PAUSED:4, COMPLETED:5 };

Bitrix.MediaPlayer = function Bitrix$MediaPlayer(){
    if(typeof(Bitrix.MediaPlayer.initializeBase) == "function")
        Bitrix.MediaPlayer.initializeBase(this);
	this._initialized = false;
	this._activated = false;
	//this._swichToPlayingOfNewFile = false;
	this._id = null;
	this._data = null;
	this._state = Bitrix.MediaPlayerState.UNDEFINED;
	this._activeController = null;
	this._controllers = null;
	this._parentElementId = null;
	this._parentNode = null;
	this._playerStateChangeHandler = Bitrix.TypeUtility.createDelegate(this, this._handlePlayerStateChange);
	this._stateChangeEvent = null;
	this._activationChangeEvent = null;
	this._loadingCompletionHandler = Bitrix.TypeUtility.createDelegate(this, this._handleLoadingCompletionHandler);
}

Bitrix.MediaPlayer.prototype = {
    initialize: function(id, parentElementId, data) {
        if (!Bitrix.TypeUtility.isNotEmptyString(id)) throw "id is not valid!";
        this._id = id;
        if (!Bitrix.TypeUtility.isNotEmptyString(parentElementId)) throw "parentElementId is not valid!";
        this._parentElementId = parentElementId;
        this._data = data;
        this._initialized = true;
    },
    getId: function() {
        return this._id;
    },
    isActivated: function() {
        return this._activated;
    },
    getState: function() {
        return this._state;
    },
    play: function(item) {
        if (this._state != Bitrix.MediaPlayerState.IDLE && this._state != Bitrix.MediaPlayerState.UNDEFINED) {
            Bitrix.MediaPlayerPlaybackItemSwitcher.getInstance().execute(this, item);
            //this.stop();
            return;
        }
		
        this._data.setMediaPlayerTypeFromObject(item);
        this._data.setUrlFromObject(item);
        this._data.setPreviewImageUrlFromObject(item);
		this._data.setSizeFromObject(item);
        this._data.setEnableAutostart(false);
		
		this.activate();
		if(this._activeController)
			this._activeController.play();
    },
    stop: function() {
        if (this._state == Bitrix.MediaPlayerState.IDLE || this._state == Bitrix.MediaPlayerState.UNDEFINED)
            return;
        if (!this._activeController) throw "Could not find active controller!";
        this._activeController.stop();
    },
    getMediaPlayerType: function() {
        return this._activeController != null ? this._activeController.getMediaPlayerType() : Bitrix.MediaPlayerType.auto;
    },
    getCurrentUrl: function() {
        return this._data.getOriginalUrl();
    },
    addStateChangeListener: function(listener) {
        if (this._stateChangeEvent == null)
            this._stateChangeEvent = new Bitrix.EventPublisher();
        this._stateChangeEvent.addListener(listener);
    },
    removeStateChangeListener: function(listener) {
        if (this._stateChangeEvent == null) return;
        this._stateChangeEvent.removeListener(listener);
    },
    addActivationChangeListener: function(listener) {
        if (this._activationChangeEvent == null)
            this._activationChangeEvent = new Bitrix.EventPublisher();
        this._activationChangeEvent.addListener(listener);
    },
    removeActivationChangeListener: function(listener) {
        if (this._activationChangeEvent == null) return;
        this._activationChangeEvent.removeListener(listener);
    },
    _setActiveController: function(controller) {
        //if(this._activeController == controller) return;
        if (this._activeController) {
            this._activeController.removeLoadingCompletionListener(this._loadingCompletionHandler);
            this._activeController.removeStateChangeListener(this._playerStateChangeHandler);
            //this._activeController.setViewVisible(false);
            if (this._activeController.isActivated())
                this._activeController.deactivate();
        }
        this._activeController = controller;
        if (this._activeController) {
            this._activeController.addLoadingCompletionListener(this._loadingCompletionHandler);
            this._activeController.addStateChangeListener(this._playerStateChangeHandler);
            //this._activeController.setViewVisible(true);
            if (!this._activeController.isActivated()) {
                var parentElement = document.getElementById(this._parentElementId);
                if (!Bitrix.TypeUtility.isDomElement(parentElement)) throw "parentElement is not valid!";
                this._activeController.activate(parentElement);
            }
        }
    },
    _prepareControllerForCurrentPlaybackItem: function() {
        var url = this._data.getUrl();
        var type = Bitrix.TypeUtility.isNotEmptyString(url) ? Bitrix.MediaPlayerData.getPlayerTypeByUrl(url) : Bitrix.MediaPlayerType.flv;
        this._data.setMediaPlayerType(type);
        var controller = this._ensureControllerCreated(type);
        return controller;
    },
    _prepareControllerByCurrentPlayerType: function() {
        var type = this._data.getMediaPlayerType();
        return type != Bitrix.MediaPlayerType.auto ? this._ensureControllerCreated(type) : null;
    },
    activate: function() {
        if (!this._initialized) throw "Is not initialized!";
        if (this._activated) return;
        try {
            var controller = this._prepareControllerByCurrentPlayerType();
            if (!controller)
                controller = this._prepareControllerForCurrentPlaybackItem();
            this._setActiveController(controller);
            this._activated = true;
            if (this._activationChangeEvent)
                this._activationChangeEvent.fire(this);
        }
        catch (e) {
            this._activeController = null;
        }
    },
    deactivate: function() {
        if (!this._activated) return;
        if (this._activeController == null) return;
        this._activeController.deactivate();
        this._setActiveController(null);
        this._activated = false;
		this._state = Bitrix.MediaPlayerState.UNDEFINED;
        if (this._activationChangeEvent)
            this._activationChangeEvent.fire(this);
    },
    _ensureControllerCreated: function(type) {
        if (this._activeController && this._activeController.getMediaPlayerType() == type)
            return this._activeController;
        if (this._controllers != null && type.toString() in this._controllers)
            return this._controllers[type.toString()];
        var r = null;
        if (this._controllers == null)
            this._controllers = new Object();
        if (type == Bitrix.MediaPlayerType.flv)
            r = this._controllers["flv"] = Bitrix.FlvMediaPlayerController.create(this._id + "_flvCtrl", this._data);
        else if (type == Bitrix.MediaPlayerType.wmv)
            r = this._controllers["wmv"] = Bitrix.WmvMediaPlayerController.create(this._id + "_wmvCtrl", this._data);
        else
            throw "Bitrix.MediaPlayerType '" + Bitrix.EnumHelper.getName(Bitrix.MediaPlayerType, type) + "' is not supported!";

        if (this._controllers == null)
            this._controllers = new Object();
        this._controllers[type.toString()] = r;
        return r;
    },
    _handleLoadingCompletionHandler: function(controller) {
        if (!controller || controller != this._activeController) return;
        this._activeController.removeLoadingCompletionListener(this._loadingCompletionHandler);
        //if(this._swichToPlayingOfNewFile){
        //	this._activeController.play();
        //	this._swichToPlayingOfNewFile = false;
        //}
    },
    _handlePlayerStateChange: function(sender, args) {
        if (this._activeController != sender)
            return;

        var r = Bitrix.MediaPlayerState.UNDEFINED;
        var type = this.getMediaPlayerType();
        if (type == Bitrix.MediaPlayerType.flv) {
            var state = args["obj"]["newstate"];
            if (state == "IDLE")
                r = Bitrix.MediaPlayerState.IDLE;
            else if (state == "BUFFERING")
                r = Bitrix.MediaPlayerState.BUFFERING;
            else if (state == "PLAYING")
                r = Bitrix.MediaPlayerState.PLAYING;
            else if (state == "PAUSED")
                r = Bitrix.MediaPlayerState.PAUSED;
            else if (state == "COMPLETED")
                r = Bitrix.MediaPlayerState.COMPLETED;
        }
        else if (type == Bitrix.MediaPlayerType.wmv) {
            var stateOld = args["oldState"];
            var stateNew = args["newState"];
            if (stateOld == "Opening" && stateNew == "Closed")
                r = Bitrix.MediaPlayerState.IDLE;
            else if (stateNew == "Buffering")
                r = Bitrix.MediaPlayerState.BUFFERING;
            else if (stateNew == "Playing")
                r = Bitrix.MediaPlayerState.PLAYING;
            else if (stateNew == "Paused")
                r = Bitrix.MediaPlayerState.PAUSED;
            else if (stateOld == "Completed" && stateNew == "Completed")
                r = Bitrix.MediaPlayerState.COMPLETED;
        }

        //if(r == Bitrix.MediaPlayerState.UNDEFINED) 
        //	return;
        this._state = r;
        //this._handleStateChangeInternal();
        var args = new Object();
        args["state"] = r;
        if (this._stateChangeEvent)
            this._stateChangeEvent.fire(this, args);
    }
}

Bitrix.MediaPlayer._entries = null;
Bitrix.MediaPlayer.create = function(id, parentElementId, data){
	var self = new Bitrix.MediaPlayer();
	self.initialize(id, parentElementId, data);
	if(this._entries == null)
		this._entries = new Object();
	this._entries[id] = self;
	if(this._entryCreatedEvent) {
		var args = new Object();
		args["entry"] = self;
		this._entryCreatedEvent.fire(this, args);
	}
	return self;
}

Bitrix.MediaPlayer._entryCreatedEvent = null;

Bitrix.MediaPlayer.addEntryCreatedListener = function(listener){
	if(this._entryCreatedEvent == null)
		this._entryCreatedEvent = new Bitrix.EventPublisher();
	this._entryCreatedEvent.addListener(listener);
}

Bitrix.MediaPlayer.removeEntryCreatedListener = function(listener){
	if(this._entryCreatedEvent == null) return;
	this._entryCreatedEvent.removeListener(listener);
}

Bitrix.MediaPlayer.getEntryById = function(id){
	if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "id is not valid!";
	return this._entries != null && id in this._entries ? this._entries[id] : null;
}
Bitrix.MediaPlayer.messages = { 
	installFlashPlayer:"Please <a  target='_blank' href='http://www.adobe.com/go/getflashplayer' title='Get Flash player'>install Flash Player</a>  9 or better to use player.", 
	couldntCreatePlayer:"Could not create player!",
	couldntCreateFlashPlayer:"Could not create Flash player!",
	couldntCreateSilverlightPlayer:"Could not create Silverlight player!"	
}

if(typeof(Bitrix.MediaPlayer.registerClass) == "function")
	Bitrix.MediaPlayer.registerClass("Bitrix.MediaPlayer");

Bitrix.MediaPlayerPlaybackItemSwitcher = function Bitrix$MediaPlayerPlaybackItemSwitcher(){
	this._initialized = false;
	this._activated = false;
	this._playerStateChangeHandler = Bitrix.TypeUtility.createDelegate(this, this._handlePlayerStateChange);
	this._player = null;
	this._item = null;
}
Bitrix.MediaPlayerPlaybackItemSwitcher.prototype = {
	initialize: function(){
		this._initialized = false;
	},
	isActivated: function(){
		return this._activated;
	},
	execute: function(player, item){
		if(this._activated) throw "Already activated!";
		if(!player) throw "player is not defined!";
		if(!item) throw "item is not defined!";
		this._activated = true;
		this._player = player;
		this._item = item;
		this._execute();
	},
	_execute: function(){
		if(!this._activated) throw "Is not activated";
		var state = this._player.getState();
		if(state == Bitrix.MediaPlayerState.IDLE || state == Bitrix.MediaPlayerState.UNDEFINED)
			this._player.play(this._item);
		else
		{
			this._player.addStateChangeListener(this._playerStateChangeHandler);		
			this._player.stop();
		}
	},
	_handlePlayerStateChange: function(sender, args){
		if(!this._activated) throw "Is not activated!";
		if(sender != this._player) "Unknown sender!";
		var state = this._player.getState();
		if(!(state == Bitrix.MediaPlayerState.IDLE || state == Bitrix.MediaPlayerState.UNDEFINED))
			return;
		this._player.removeStateChangeListener(this._playerStateChangeHandler);
		this._activated = false;
		var player = this._player;
		var item = this._item;
		window.setTimeout(function(){player.play(item);}, 500);
	}
}

Bitrix.MediaPlayerPlaybackItemSwitcher._instances  = null;
Bitrix.MediaPlayerPlaybackItemSwitcher.getInstance = function(){	
	var instance = null;
	if(this.instances != null)
		for(var i = 0; i < this.instances.length; i++){
			if(this.instances[i].isActivated()) continue;
			instance = this.instances[i];
			break;
		}
	if(instance == null){
		instance = new Bitrix.MediaPlayerPlaybackItemSwitcher();
		instance.initialize();
		if(this.instances == null) 
			this.instances = new Array();
		this.instances.push(instance);
	}
	return instance;
}
	
Bitrix.MediaPlayerData = function Bitrix$Components$MediaPlayerData(){
	//Bitrix.MediaPlayerData.initializeBase(this);
	this._initialized = false;
	this._componentParams = null;
}

Bitrix.MediaPlayerData.prototype = {
	initialize: function(componentParams){
		this.setComponentParams(componentParams);
		this._initialized = true;	
	},
	getComponentParams: function(){
		return this._componentParams;
	},
	setComponentParams: function(componentParams){
		this._componentParams = componentParams;
	},
	getMediaPlayerType: function(){
		this._checkComponentParams();
		var typeName = Bitrix.ObjectHelper.tryGetStringByNameArray(this._componentParams, ["playertype", "player", "type"], "auto").toLowerCase();
		var type = Bitrix.EnumHelper.getId(Bitrix.MediaPlayerType, typeName);
		return type != null ? type : Bitrix.MediaPlayerType.auto;	
	},
	setMediaPlayerType: function(type){
		this._componentParams["playertype"] = Bitrix.EnumHelper.getName(Bitrix.MediaPlayerType, type);
	},
	getMediaPlayerStretchingMode: function(){
		this._checkComponentParams();
		var name = Bitrix.ObjectHelper.tryGetString(this._componentParams, "stretching", "Proportionally").toLowerCase();
		var id = Bitrix.EnumHelper.getId(Bitrix.MediaPlayerStretchingMode, name);
		return id != null ? id : Bitrix.MediaPlayerStretchingMode.proportionally;	
	},	
	setMediaPlayerTypeFromObject: function(obj){
		var typeName = Bitrix.ObjectHelper.tryGetStringByNameArray(obj, ["playertype", "player", "type"], "auto");
		this._componentParams["playertype"] = typeName;
	},	
	getWidth: function(){
		this._checkComponentParams();
		if("width" in this._componentParams)
			return this._componentParams["width"];
		//return "425px";
		return "";
	},
	getHeight: function(){
		this._checkComponentParams();
		if("height" in this._componentParams)
			return this._componentParams["height"];
		//return "344px";
		return "";
	},
	setSizeFromObject: function(obj){
		var height = Bitrix.ObjectHelper.tryGetString(obj, "height", "");
		if(Bitrix.TypeUtility.isNotEmptyString(height))
			this._componentParams["height"] = height;
		var width = Bitrix.ObjectHelper.tryGetString(obj, "width", "");
		if(Bitrix.TypeUtility.isNotEmptyString(width))
			this._componentParams["width"] = width;
	},	
	getUrl: function(){
		this._checkComponentParams();
		var r = this.getOriginalUrl();
		if(r.length > 0 && Bitrix.PathHelper.isVirtual(r))
			r = Bitrix.PathHelper.resolveVirtualPath(r);			
		return r;
	},
	getOriginalUrl: function(){
		this._checkComponentParams();
		var r = "";
		var isPlayListEnabled = Bitrix.ObjectHelper.tryGetBoolean(this._componentParams, "enableplaylist", false);
		if(isPlayListEnabled)
			r = Bitrix.ObjectHelper.tryGetString(this._componentParams, "playlistfilepath", "");
		if(r.length == 0)
			r = Bitrix.ObjectHelper.tryGetStringByNameArray(this._componentParams, ["sourcefilepath", "file", "path", "url"], "");	
		return r;
	},
	setUrl: function(url){
		this._componentParams["sourcefilepath"] = url;
	},
	setUrlFromObject: function(obj){
		var url = Bitrix.ObjectHelper.tryGetStringByNameArray(obj, ["sourcefilepath", "file", "path", "url"], "");
		this._componentParams["sourcefilepath"] = url;
	},	
	setPreviewImageUrlFromObject: function(obj){
		var url = Bitrix.ObjectHelper.tryGetStringByNameArray(obj, ["previewimagefilepath", "preview", "image"], "");
		this._componentParams["previewimagefilepath"] = url;
	},	
	getEnableAutostart: function(){
		return this._componentParams["enableautostart"];
	},				
	setEnableAutostart: function(enable){
		this._componentParams["enableautostart"] = enable;
	},
	getEnableDownloading: function(){
	    return Bitrix.ObjectHelper.tryGetBoolean(this._componentParams, "enabledownloading", true);
	},	
	setEnableDownloading: function(enable){
	    this._componentParams["enabledownloading"] = enable;
	},			
	getParamsByMediaPlayerType: function(mediaPlayerType){
		this._checkComponentParams();
		var previewImageFilePath = Bitrix.PathHelper.resolveVirtualPath(Bitrix.ObjectHelper.tryGetStringByNameArray(this._componentParams, ["previewimagefilepath", "preview", "image"], ""));
		var stretching = this.getMediaPlayerStretchingMode();
		var enableDownloading = this.getEnableDownloading();
		var linkForDownloadUrl = "", linklinkForDownloadUrlTarget = "";
		if(enableDownloading){
		    linkForDownloadUrl = Bitrix.PathHelper.resolveVirtualPath(Bitrix.ObjectHelper.tryGetStringByNameArray(this._componentParams, ["linkfordownloadsourcefileurl", "link"], ""));
		    if(!Bitrix.TypeUtility.isNotEmptyString(linkForDownloadUrl))
		        linkForDownloadUrl = this.getUrl();
		    linklinkForDownloadUrlTarget = Bitrix.PathHelper.resolveVirtualPath(Bitrix.ObjectHelper.tryGetStringByNameArray(this._componentParams, ["linkfordownloadsourcefiletargetwindow", "linktarget"], ""));
		}
		var showcontrolpanel = Bitrix.ObjectHelper.tryGetBoolean(this._componentParams, "showcontrolpanel", true);
		var volumeLevel = Bitrix.ObjectHelper.tryGetIntByNameArray(this._componentParams, ["volumelevelinpercents", "volume"], undefined);
		var fullScreen = Bitrix.ObjectHelper.tryGetBooleanByNameArray(this._componentParams, ["enablefullscreenmodeswitch", "fullscreen"], undefined);
		var autostart = Bitrix.ObjectHelper.tryGetBooleanByNameArray(this._componentParams, ["enableautostart", "autostart"], undefined);
		var repeat = Bitrix.ObjectHelper.tryGetBooleanByNameArray(this._componentParams, ["enablerepeatmode", "repeat"], false);
		
		if(mediaPlayerType == Bitrix.MediaPlayerType.flv){
			var r = new Object();
			Bitrix.ObjectHelper.setStringIfNotEmpty(r, "image", previewImageFilePath);			
			var logoImageFilePath = Bitrix.PathHelper.resolveVirtualPath(Bitrix.ObjectHelper.tryGetString(this._componentParams, "logoimagefilepath", ""));
			Bitrix.ObjectHelper.setStringIfNotEmpty(r, "logo", logoImageFilePath);			
			
			Bitrix.ObjectHelper.setBoolean(r, "fullscreen", fullScreen);
			Bitrix.ObjectHelper.setBoolean(r, "autostart", autostart);			
			Bitrix.ObjectHelper.setStringIfNotEmpty(r, 	"backcolor", 		Bitrix.ObjectHelper.tryGetString(this._componentParams, "controlpanelbackgroundcolor", ""));	
			Bitrix.ObjectHelper.setStringIfNotEmpty(r, 	"frontcolor", 		Bitrix.ObjectHelper.tryGetString(this._componentParams, "controlscolor", ""));	
			Bitrix.ObjectHelper.setStringIfNotEmpty(r, 	"lightcolor", 		Bitrix.ObjectHelper.tryGetString(this._componentParams, "controlsovercolor", ""));	
			Bitrix.ObjectHelper.setStringIfNotEmpty(r, 	"screencolor", 		Bitrix.ObjectHelper.tryGetString(this._componentParams, "screencolor", ""));
			
			var skinPath = Bitrix.ObjectHelper.tryGetString(this._componentParams, "flvskinfolderpath", "");
			var skinName = Bitrix.ObjectHelper.tryGetString(this._componentParams, "flvskinname", "");
			
			if(skinPath.length > 0 && skinName.length > 0){
				if(skinName.search(/\.swf$/i) < 0)
					skinName += ".swf";
				skinPath = Bitrix.PathHelper.resolveVirtualPath(skinPath);
				var skinPath = Bitrix.PathHelper.combine(skinPath, skinName);
				Bitrix.ObjectHelper.setStringIfNotEmpty(r, "skin", skinPath);
			}

			if(!showcontrolpanel)
				r["controlbar"] = "none";
			else
				Bitrix.ObjectHelper.setStringIfNotEmpty(r, "controlbar", Bitrix.ObjectHelper.tryGetString(this._componentParams, "flvcontrolbarlocation", ""));	
			
			if(!repeat)
				Bitrix.ObjectHelper.setString(r, "repeat", "none");
			else
				Bitrix.ObjectHelper.setString(r, "repeat", "always");			

			Bitrix.ObjectHelper.setNumber(r, "volume", volumeLevel);
			Bitrix.ObjectHelper.setBoolean(r, "mute", Bitrix.ObjectHelper.tryGetBoolean(this._componentParams, "flvstartinmutemode", undefined));
			Bitrix.ObjectHelper.setBoolean(r, "quality", Bitrix.ObjectHelper.tryGetBoolean(this._componentParams, "flvenablehighquality", undefined));	
			Bitrix.ObjectHelper.setNumber(r, "bufferlength", Bitrix.ObjectHelper.tryGetIntByNameArray(this._componentParams, ["bufferlengthinseconds", "bufferlength"], undefined));	

			Bitrix.ObjectHelper.setStringIfNotEmpty(r, "link", 			linkForDownloadUrl);	
			Bitrix.ObjectHelper.setStringIfNotEmpty(r, "linktarget", 	linklinkForDownloadUrlTarget);	
			
			Bitrix.ObjectHelper.setStringIfNotEmpty(r, "abouttext", Bitrix.ObjectHelper.tryGetString(this._componentParams, "abouttext", ""));	
			Bitrix.ObjectHelper.setStringIfNotEmpty(r, "aboutlink", Bitrix.ObjectHelper.tryGetString(this._componentParams, "aboutlink", ""));	
			Bitrix.ObjectHelper.setStringIfNotEmpty(r, "wmode", Bitrix.ObjectHelper.tryGetString(this._componentParams, "flvwmode", ""));		
			Bitrix.ObjectHelper.setBoolean(r, "menu", Bitrix.ObjectHelper.tryGetBoolean(this._componentParams, "flvhidecontextmenu", undefined));								
			Bitrix.ObjectHelper.setStringIfNotEmpty(r, "displayclick", Bitrix.ObjectHelper.tryGetString(this._componentParams, "flvclientclickactionname", ""));

			if(Bitrix.ObjectHelper.tryGetBoolean(this._componentParams, "audioOnly", false))
				Bitrix.ObjectHelper.setBoolean(r, "audioOnly", true);								
				
			switch(stretching){
				case Bitrix.MediaPlayerStretchingMode.none:
					r["stretching"] = "none";
					break;
				case Bitrix.MediaPlayerStretchingMode.fill:
					r["stretching"] = "fill";
					break;				
				case Bitrix.MediaPlayerStretchingMode.disproportionally:
					r["stretching"] = "exactfit";
					break;
				default:
					r["stretching"] = "uniform";
			}
			
			var url = this.getUrl();
			if(url.length > 0){
				var ext = Bitrix.PathHelper.getFileExtension(url);
				if(ext.length > 0){
					ext = ext.toUpperCase();
					if(ext == ".FLV")
						r["type"] = "video";
					else if(ext == ".MP3")
						r["type"] = "sound";
					else if(ext == ".JPG" || ext == ".PNG" || ext == ".GIF")
						r["type"] = "image";
				}	
			}			
			return r;
		}
		if(mediaPlayerType == Bitrix.MediaPlayerType.wmv){
			var r = new Object();
			Bitrix.ObjectHelper.setStringIfNotEmpty(r, "image", 			previewImageFilePath);			
			var logoImageFilePath = Bitrix.PathHelper.resolveVirtualPath(Bitrix.ObjectHelper.tryGetString(this._componentParams, "logoimagefilepath", ""));
			Bitrix.ObjectHelper.setStringIfNotEmpty(r, "logo", 				logoImageFilePath);	
		
			Bitrix.ObjectHelper.setBoolean(r, "usefullscreen", fullScreen);	
			if(Bitrix.ObjectHelper.tryGetString(this._componentParams, "wmvwmode", "") == "windowless")
				r["windowless"] = true;
			else
				r["windowless"] = false;

			Bitrix.ObjectHelper.setBoolean(r, 			"autostart", 		autostart);
			Bitrix.ObjectHelper.setStringIfNotEmpty(r, 	"backcolor", 		Bitrix.ObjectHelper.tryGetString(this._componentParams, "controlpanelbackgroundcolor", ""));	
			Bitrix.ObjectHelper.setStringIfNotEmpty(r, 	"frontcolor", 		Bitrix.ObjectHelper.tryGetString(this._componentParams, "controlscolor", ""));	
			Bitrix.ObjectHelper.setStringIfNotEmpty(r, 	"lightcolor", 		Bitrix.ObjectHelper.tryGetString(this._componentParams, "controlsovercolor", ""));	
			Bitrix.ObjectHelper.setStringIfNotEmpty(r, 	"screencolor", 		Bitrix.ObjectHelper.tryGetString(this._componentParams, "screencolor", ""));
			if(!showcontrolpanel)
				r["shownavigation"] = false;
			else
				Bitrix.ObjectHelper.setBoolean(r, 		"shownavigation", 	Bitrix.ObjectHelper.tryGetBoolean(this._componentParams, "wmvshowcontrolpanel", undefined));	
			Bitrix.ObjectHelper.setBoolean(r, 			"showstop", 		Bitrix.ObjectHelper.tryGetBoolean(this._componentParams, "wmvshowstopbutton", undefined));	
			Bitrix.ObjectHelper.setBoolean(r, 			"showdigits", 		Bitrix.ObjectHelper.tryGetBoolean(this._componentParams, "wmvshowdigits", undefined));	
			Bitrix.ObjectHelper.setBoolean(r, 			"repeat", 			repeat);	
			Bitrix.ObjectHelper.setNumber(r, 			"volume", 			volumeLevel);	
			Bitrix.ObjectHelper.setNumber(r, 			"bufferlength", Bitrix.ObjectHelper.tryGetIntByNameArray(this._componentParams, ["bufferlengthinseconds", "bufferlength"], undefined));

			Bitrix.ObjectHelper.setStringIfNotEmpty(r, 	"link", 			linkForDownloadUrl);	
			Bitrix.ObjectHelper.setStringIfNotEmpty(r, 	"linktarget", 		linklinkForDownloadUrlTarget);
				
			switch(stretching){
				case Bitrix.MediaPlayerStretchingMode.none:
					r["overstretch"] = "none";
					break;
				case Bitrix.MediaPlayerStretchingMode.fill:
					r["overstretch"] = "true";
					break;				
				case Bitrix.MediaPlayerStretchingMode.disproportionally:
					r["overstretch"] = "fit";
					break;
				default:
					r["overstretch"] = "false";
			}				
				
			return r;
		}		
		throw "Bitrix.MediaPlayerType '" + mediaPlayerType + "' is not supported!";
	},
	_checkComponentParams: function(){
		if(this._componentParams == null) throw "ComponentParams is not found!";
	}
}

Bitrix.MediaPlayerData.create = function(componentParams){
	var self = new Bitrix.MediaPlayerData();
	self.initialize(componentParams);
	return self;
}

Bitrix.MediaPlayerData.getPlayerTypeByUrl = function(url){
	if(!Bitrix.TypeUtility.isNotEmptyString(url)) throw "url is not valid!";
	var ext = Bitrix.PathHelper.getFileExtension(url);	
	if(ext.length == 0) throw "Could not find ext in " + url + "!";
	ext = ext.toUpperCase();
	return ext == ".WMA" || ext == ".WMV" ? Bitrix.MediaPlayerType.wmv : Bitrix.MediaPlayerType.flv;
}
//Bitrix.MediaPlayerData.registerClass("Bitrix.MediaPlayerData");
Bitrix.VideoHostingUtility = function(){}
Bitrix.VideoHostingUtility._rx = {
	youtubeHtml:/(?:http:\/\/)?(?:www\.)?(?:.*?\.)?youtube\.(?:com|[a-z]+)\/watch\?(?:[^\&]+?\&)*v\=([^\&]*)/i,
	youtubePlayer:/(?:http\:\/\/)?(?:www\.)?youtube\.com\/v\/.+/i,
	vimeoHtml:/(?:http\:\/\/)?(?:www\.)?vimeo\.com\/([^\?\&\/]+)/i,
	vimeoPlayer:/(?:http\:\/\/)?(?:www\.)?vimeo\.com\/moogaloop\.swf\?clip_id\=[^\&]+/i,
	rutubeHtml:/(?:http\:\/\/)?(?:www\.)?rutube\.ru\/tracks\/.+?\.html\?(?:[^\&]+?\&)*v\=([^\&]*)/i,
	rutubePlayer:/(?:http\:\/\/)?video.rutube\.ru\/.+/i,
	yandexPlayer:/(?:http\:\/\/)?static\.video\.yandex\.ru\/.+/i
}

Bitrix.VideoHostingUtility.getHostingName = function(url){
	if(!Bitrix.TypeUtility.isNotEmptyString(url)) return "";
	if(this._rx.youtubeHtml.test(url) || this._rx.youtubePlayer.test(url)) return "YOUTUBE";
	if(this._rx.vimeoHtml.test(url) || this._rx.vimeoPlayer.test(url)) return "VIMEO";
	if(this._rx.rutubeHtml.test(url) || this._rx.rutubePlayer.test(url)) return "RUTUBE";
	if(this._rx.yandexPlayer.test(url)) return "YANDEX";
	return "";
}

Bitrix.VideoHostingUtility.getVideoDataUrl = function(url){
	if(!Bitrix.TypeUtility.isNotEmptyString(url)) return "";
	var m = null;
	if((m = this._rx.youtubeHtml.exec(url)) && m.length > 1)
		return "http://www.youtube.com/v/" + m[1];
	if(!this._rx.vimeoPlayer.test(url) && (m = this._rx.vimeoHtml.exec(url)) && m.length > 1)
		return "http://vimeo.com/moogaloop.swf?clip_id=" + m[1] + "&server=vimeo.com&show_title=1&show_byline=1&show_portrait=0&color=&fullscreen=1";		
	else if((m = this._rx.rutubeHtml.exec(url)) && m.length > 1)
		return "http://video.rutube.ru/" + m[1];	
	
	return (/^[a-z0-9]+\:\/\//i).test(url) ? url : 'http://' + url;
}
if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded(); 
