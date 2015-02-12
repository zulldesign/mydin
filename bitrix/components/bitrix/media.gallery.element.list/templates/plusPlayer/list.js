    if(typeof(Bitrix) == "undefined"){
	    var Bitrix = new Object();
	}
        
	Bitrix.MediaGalleryElementListTemplDefault = function Bitrix$MediaGalleryElementListTemplDefault(){
	    this._initialized = false;
	    this._isPlayerCreated = false;
	    this._id = null;
	    this._playerId = null;
	    this._items = null;
	    this._playerStateChangeHandler = Bitrix.TypeUtility.createDelegate(this, this._handlePlayerStateChange);
		this._playerActivationChangeHandler = Bitrix.TypeUtility.createDelegate(this, this._handlePlayerActivationChange);
	    this._playerCreatedHandler = Bitrix.TypeUtility.createDelegate(this, this._handlePlayerCreation);
	}
	Bitrix.MediaGalleryElementListTemplDefault.prototype = {
	    initialize: function(id, playerId){
	        this._id = id;
	        this._playerId = playerId;
	        this._initialized = true; 
	        
	        var player = Bitrix.MediaPlayer.getEntryById(this._playerId);
	        if(player)
	        {
	            player.addStateChangeListener(this._playerStateChangeHandler);
				player.addActivationChangeListener(this._playerActivationChangeHandler);
				this._selectLoadedItem();
	            this._isPlayerCreated = true;
	        }
	        else
	            Bitrix.MediaPlayer.addEntryCreatedListener(this._playerCreatedHandler);
	    },
	    _handlePlayerCreation: function(sender, args){
	        var entry = "entry" in args ? args["entry"] : null;
	        if(!entry || entry.getId() != this._playerId) return;
	        Bitrix.MediaPlayer.removeEntryCreatedListener(this._playerCreatedHandler);  
	        entry.addStateChangeListener(this._playerStateChangeHandler);
			entry.addActivationChangeListener(this._playerActivationChangeHandler);
	        this._isPlayerCreated = true;
	    },
		_handlePlayerActivationChange: function(sender){
			this._selectLoadedItem();
		},
	    _handlePlayerStateChange: function(sender, args){
			this._selectLoadedItem();
	     },
		 _selectLoadedItem: function(){
			this._setAllItemsSelected(false);
			var player = Bitrix.MediaPlayer.getEntryById(this._playerId);
			if(!player || !player.isActivated()) return;
			var file = player.getCurrentUrl();
			if(file.length == 0) return;
			var item = this._getItemByFile(file);
			if(!item) return;
			this._setItemSelected(item, true);			
		 },
	     _setItemSelected: function(item, selected){
            var container = document.getElementById(item.id);
            if(!container) return;
            var innerContainer = null;
            var index = 0;
            while(container.childNodes.length > index && (innerContainer = container.childNodes[index]) && !("tagName" in innerContainer && innerContainer.tagName == "DIV"))
               index++;
            if(innerContainer)
                innerContainer.className = selected ? "bx-media-gallery-element-selected" : "bx-media-gallery-element-normal";
			//container.scrollIntoView(true);
	     },
	     _setAllItemsSelected: function(selected){
	        if(this._items == null) return;
	        for(var id in this._items)
	            this._setItemSelected(this._items[id], selected);
	     },
	    ensureItemCreated: function(id, params){
	        var item = this._items != null && id in this._items ? this._items[id] : null;
	        if(item) return item;
	        if(this._items == null) this._items = new Object();
	        this._items[id] = item = new Object();
	        item.id = id;
			if(params && typeof(params) == "object" && "file" in params)
				item.file = params["file"];
				
			this._selectLoadedItem();
			
	        return item; 
	    },
	    _getItem: function(id){
	        return this._items != null && id in this._items ? this._items[id] : null;
	    },
	    _getItemByFile: function(file){
	        if(this._items == null) return null;
	        for(var id in this._items)
	            if(this._items[id].file == file) return this._items[id];
	        return null;
	    },
	    play: function(itemId, cfg){
	        if(!(cfg && cfg instanceof Object)) throw "Configuration is not valid!";
	        if(!this._isPlayerCreated) return;
	        var item = this.ensureItemCreated(itemId, {file:cfg.file});
	        Bitrix.MediaPlayer.getEntryById(this._playerId).play(cfg);
	    }
	}
	Bitrix.MediaGalleryElementListTemplDefault._entries = null;
	Bitrix.MediaGalleryElementListTemplDefault.create = function(id, playerId){
	    var self = new Bitrix.MediaGalleryElementListTemplDefault();
	    self.initialize(id, playerId);
	    if(this._entries == null) this._entries = new Object();
	    this._entries[id] = self;
	    return self;
	}
	Bitrix.MediaGalleryElementListTemplDefault.getEntryById = function(id){
	    return this._entries != null && id in this._entries ? this._entries[id] : null;
	}

