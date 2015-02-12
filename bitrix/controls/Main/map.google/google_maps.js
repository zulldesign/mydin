if(typeof(Bitrix) == "undefined"){
	var Bitrix = {};
}
	
Bitrix.GoogleMapsCoordinate = function Bitrix$GoogleMapsCoordinate(){
    this._initialized = false;
    this._latitude = 55.75;
	this._longitude = 37.6;
}
Bitrix.GoogleMapsCoordinate.prototype={
	initialize: function(lat, lng){
	    if(this._initialized) return;
		
		this._latitude = Bitrix.TypeUtility.tryConvertToFloat(lat, 55.75);
		this._longitude = Bitrix.TypeUtility.tryConvertToFloat(lng, 37.6);
		this._initialized = true;
	},
	getLatitude: function(){ return this._latitude; },
	setLatitude: function(lat){ this._latitude = lat; },	
	getLongitude: function(){ return this._longitude },
	setLongitude: function(lng){ this._longitude = lng; },
	toString: function(format) { 
		if(!Bitrix.TypeUtility.isNotEmptyString(format)) format = "";
		return format.toUpperCase() == "S" ? this._latitude.toFixed(5) + ", " + this._longitude.toFixed(5) : "{ lat: " + this._latitude + ", lng: " + this._longitude + " }"; 
	},
	toObject: function(){ return { lat: this._latitude, lng: this._longitude }; },
	toGoogleMapsLatLng: function(){ return new google.maps.LatLng(this._latitude, this._longitude); }
}
Bitrix.GoogleMapsCoordinate.fromGoogleMapsLatLng = function(latLng, defaultValue){
	if(latLng)
		return this.create(latLng.lat(), latLng.lng());
	return defaultValue;
}
Bitrix.GoogleMapsCoordinate.fromObject = function(obj, defaultVal){
	if(obj instanceof Bitrix.GoogleMapsCoordinate) return obj;
	if(obj && typeof(obj) == "object" && "lat" in obj && "lng" in obj)
		return this.create(obj["lat"], obj["lng"]);
	return defaultVal;
}
Bitrix.GoogleMapsCoordinate.create = function(lat, lng){
	var self = new Bitrix.GoogleMapsCoordinate();
	self.initialize(lat, lng);
	return self;
}
Bitrix.GoogleMapsCoordinate.equals = function(first, second) {
	return first._latitude == second._latitude && first._longitude == second._longitude;
}
Bitrix.GoogleMapsCoordinate.getMSK = function(){
	return new Bitrix.GoogleMapsCoordinate.create(55.75, 37.6);
}

Bitrix.GoogleMapsMarker = function Bitrix$GoogleMapsMarker() {
    this._initialized = false;
	this._map = null;
	this._pos = null;
	this._ttl = "";
	this._icon = "";
	this._created = new Date();
	this._draggable = false;
	this._clickable = false;
	this._sourceMarker = null;
	this._markerChangeHandler = Bitrix.TypeUtility.createDelegate(this, this._handleMarkerChange);
	this._markerDragEndHandler = Bitrix.TypeUtility.createDelegate(this, this._handleMarkerDragEnd);
	this._markerClickHandler = Bitrix.TypeUtility.createDelegate(this, this._handleMarkerClick);
	this._changeListener = null;
	this._dragEndListener = null;
	this._clickListener = null;
	this._suppressMarkerChange = false;
}
Bitrix.GoogleMapsMarker.prototype = {
	initialize: function(pos, ttl){
		this.setPosition(pos ? pos : Bitrix.GoogleMapsCoordinate.getMSK());
		this.setTitle(ttl);
		this._initialized = true;
	},
	getPosition: function() { return this._pos; },
	setPosition: function(pos) { 
		if(!pos) throw "GoogleMapsMarker: pos is not defined!";
		this._pos = pos; 
		if(!this._sourceMarker) return;
		this._suppressMarkerChange = true;
		this._sourceMarker.setPosition(pos.toGoogleMapsLatLng());
		this._suppressMarkerChange = false;
	},	
	getTitle: function() { return this._ttl; },
	setTitle: function(ttl) { 
		this._ttl = Bitrix.TypeUtility.isNotEmptyString(ttl) ? ttl : ""; 
		if(this._sourceMarker) this._sourceMarker.setTitle(this._ttl);
	},
	getDefaultTitle: function() { return this._pos.toString("S"); },
	getIcon: function() { return this._icon; },
	setIcon: function(icon) { 
	    this._icon = Bitrix.TypeUtility.isNotEmptyString(icon) ? icon : ""; 
	    if(this._sourceMarker && this._icon.length > 0) this._sourceMarker.setIcon(this._icon);
	},
	getDraggable: function() { return  this._sourceMarker ? this._sourceMarker.getDraggable() : false; },
	setDraggable: function(draggable) { this._draggable = draggable; if(this._sourceMarker) this._sourceMarker.setDraggable(draggable); },
	getClickable: function() { return  this._sourceMarker ? this._sourceMarker.getClickable() : false; },
	setClickable: function(clickable) { this._clickable = clickable; if(this._sourceMarker) this._sourceMarker.setClickable(clickable); },
	created: function() { this._created(); },
	addChangeListener: function(listener) {
		(this._changeListener ? this._changeListener : (this._changeListener = new Bitrix.EventPublisher())).addListener(listener);
	},
	removeChangeListener: function(listener) {
		if(this._changeListener) this._changeListener.removeListener(listener);
	},	
	addDragEndListener: function(listener) {
		(this._dragEndListener ? this._dragEndListener : (this._dragEndListener = new Bitrix.EventPublisher())).addListener(listener);
	},
	removeDragEndListener: function(listener) {
		if(this._dragEndListener) this._dragEndListener.removeListener(listener);
	},	
	addClickListener: function(listener) {
		(this._clickListener ? this._clickListener : (this._clickListener = new Bitrix.EventPublisher())).addListener(listener);
	},
	removeClickListener: function(listener) {
		if(this._clickListener) this._clickListener.removeListener(listener);
	},		
	getSourceMarker: function() { return this._sourceMarker; },
	setupByGoogleMapsMarker: function(srcMarker) {
		if(!srcMarker) throw "GoogleMapsMarker: srcMarker is not defined!";
		if(this._sourceMarker) {
			google.maps.event.clearListeners(this._sourceMarker, 'position_changed');
			google.maps.event.clearListeners(this._sourceMarker, 'title_changed');
			google.maps.event.clearListeners(this._sourceMarker, 'dragend');
			google.maps.event.clearListeners(this._sourceMarker, 'click');
			this._sourceMarker = null;
		}
		this._pos = Bitrix.GoogleMapsCoordinate.fromGoogleMapsLatLng(srcMarker.getPosition());
		this._ttl = srcMarker.getTitle(); 		
		this._sourceMarker = srcMarker;
		google.maps.event.addListener(this._sourceMarker, 'position_changed', this._markerChangeHandler);		
		google.maps.event.addListener(this._sourceMarker, 'title_changed', this._markerChangeHandler);
		google.maps.event.addListener(this._sourceMarker, 'dragend', this._markerDragEndHandler);
		google.maps.event.addListener(this._sourceMarker, 'click', this._markerClickHandler);
	},
	getMap: function() { return this._map; },
	setMap: function(map) {
		this._map = map;
		if(this._sourceMarker) this._sourceMarker.setMap(map ? map.getSourceMap() : null);
	},
	_handleMarkerChange: function() {
		if(this._suppressMarkerChange) return;
		if(!this._sourceMarker) return;
		this._pos = Bitrix.GoogleMapsCoordinate.fromGoogleMapsLatLng(this._sourceMarker.getPosition());
		this._ttl = this._sourceMarker.getTitle(); 
		if(this._changeListener) this._changeListener.fire(this);
	},
	_handleMarkerDragEnd: function() { if(this._dragEndListener) this._dragEndListener.fire(this); },
	_handleMarkerClick: function() { if(this._clickListener) this._clickListener.fire(this); },	
	release: function() { 
		this.setMap(null); 
		var srcMarker = this._sourceMarker;
		if(!srcMarker) return;		
		google.maps.event.clearListeners(srcMarker, 'position_changed');
		google.maps.event.clearListeners(srcMarker, 'title_changed');		
		google.maps.event.clearListeners(srcMarker, 'dragend');
		google.maps.event.clearListeners(srcMarker, 'click');
	},
	toObject: function() { return { title: this.getTitle(), position: this.getPosition().toObject() }; },
	toString: function() {
		var ttl = this.getTitle().replace(/[\/\\\"\']/g, function(str){ return '\\' + str; });
		return "{ title: \"" + ttl + "\", position: " + this.getPosition().toString() + " }"; 
	}
}
Bitrix.GoogleMapsMarker.create = function(options) {
	var pos = Bitrix.TypeUtility.tryGetObjProp(options, "position", null);
	if(!pos) pos = Bitrix.GoogleMapsCoordinate.getMSK();
	var ttl = Bitrix.TypeUtility.tryGetObjProp(options, "title", "");
	var srcMarker = new google.maps.Marker({
		map: null,
		title: ttl,
		position: pos.toGoogleMapsLatLng(),
		icon: Bitrix.TypeUtility.tryGetObjProp(options, "icon", "//maps.gstatic.com/intl/en_ALL/mapfiles/ms/micons/blue-dot.png"),
		flat: Bitrix.TypeUtility.tryGetObjProp(options, "flat", true),
		draggable: Bitrix.TypeUtility.tryGetObjProp(options, "draggable", false),
		clickable: Bitrix.TypeUtility.tryGetObjProp(options, "clickable", false)
		});
	var self = new Bitrix.GoogleMapsMarker();
	self.initialize(pos, ttl);
	self.setupByGoogleMapsMarker(srcMarker);
	return self;
}
Bitrix.GoogleMapsMarker.fromGoogleMapsMarker = function(srcMarker) {
	var self = new Bitrix.GoogleMapsMarker();
	self.initialize(null, "");
	self.setupByGoogleMapsMarker(srcMarker);
	return self;	
}
Bitrix.GoogleMapsMarker.fromObject = function(obj) {
	if(obj instanceof Bitrix.GoogleMapsMarker) return obj;
	if(!obj) throw "GoogleMapsMarker: obj is not defined!";
	return this.create( { position: "position" in obj ? Bitrix.GoogleMapsCoordinate.fromObject(obj.position) : null, title: "title" in obj ? obj.title : "" });	
}
Bitrix.GoogleMapType ={
	roadmap: 	1,
	satellite:	2,
	hybrid:		3,
	terrain:	4
}
Bitrix.GoogleMapType.fromString = function(str){
	return Bitrix.EnumHelper.fromString(str, this);
}
Bitrix.GoogleMapType.toString = function(src){
	return Bitrix.EnumHelper.toString(src, this);
}
Bitrix.GoogleMapType.tryParse = function(src){
	return Bitrix.EnumHelper.tryParse(src, this);
}
Bitrix.GoogleMapType.fromGoogleMapTypeId = function(src) {
	switch(src){
		case google.maps.MapTypeId.HYBRID: return Bitrix.GoogleMapType.hybrid;
		case google.maps.MapTypeId.SATELLITE: return Bitrix.GoogleMapType.satellite;
		case google.maps.MapTypeId.TERRAIN: return Bitrix.GoogleMapType.terrain;
		case google.maps.MapTypeId.ROADMAP: 
		default: 
			return Bitrix.GoogleMapType.roadmap;
	}
}
Bitrix.GoogleMapType.toGoogleMapTypeId = function(src) {
	switch(src){
		case Bitrix.GoogleMapType.hybrid: return google.maps.MapTypeId.HYBRID;
		case Bitrix.GoogleMapType.satellite: return google.maps.MapTypeId.SATELLITE;
		case Bitrix.GoogleMapType.terrain: return google.maps.MapTypeId.TERRAIN;
		case Bitrix.GoogleMapType.roadmap: 
		default: 
			return google.maps.MapTypeId.ROADMAP;
	}
}
Bitrix.GoogleMapControlType = {
    largeMap:				1,
    smallMap: 				2,
    horBarMapType: 			3,
    dropDownMenuMapType:	4,
    scale: 					5
}
Bitrix.GoogleMapControlType.fromString = function(str) {
	return Bitrix.EnumHelper.fromString(str, this);
}
Bitrix.GoogleMapControlType.toString = function(src) {
	return Bitrix.EnumHelper.toString(src, this);
}
Bitrix.GoogleMapOption = {
	disableDefaultUI: 			1,
	disableDoubleClickZoom:		2,
	disableDragging:			3,
	disableScrollWheel:			4,
	disableKeyboardShortcuts:	5
}
Bitrix.GoogleMapOption.fromString = function(str){
	return Bitrix.EnumHelper.fromString(str, this);
}
Bitrix.GoogleMapOption.toString = function(src){
	return Bitrix.EnumHelper.toString(src, this);
}
Bitrix.GoogleMapsData = function Bitrix$GoogleMapsData(){
    this._initialized = false;
    this._parameters = null;
}
Bitrix.GoogleMapsData.prototype={
	initialize: function(parameters){
	    if(this._initialized) return;
		if(typeof(parameters) != "object") throw "'parameters' must be Object!";
		this._parameters = parameters;
		this._initialized = true;
	},
	getParameter: function(key, defaultValue, ignoreUndefined){ 
		if(!(this._parameters && key in this._parameters)) return defaultValue;
		var r = this._parameters[key];
		return ignoreUndefined === true && !r ? defaultValue : r;
	},
	setParameter: function(key, value){ (this._parameters ? this._parameters : (this._parameters = new Object()))[key] = value; },
	getEntityId: function(){ return this.getParameter("entityId", "", true); },
	setEntityId: function(id){ this.setParameter("entityId", id); },
	getCenter: function(){
		var c = this.getParameter("center", null);
		return c ? c : Bitrix.GoogleMapsCoordinate.getMSK();
	},
	setCenter: function(center){ this.setParameter("center", center); },
   	getZoom: function(){ return this.getParameter("zoom", 8); },
	setZoom: function(zoom){ this.setParameter("zoom", zoom); },
   	getMapType: function(){ return this.getParameter("mapType", Bitrix.GoogleMapType.roadmap, true); },
	setMapType: function(type){ this.setParameter("mapType", type); },
	getMapControlTypes: function(){ return Bitrix.GoogleMapControlType.fromString(this.getParameter("mapControlTypes", null)); },
	setMapControlTypes: function(types){ this.setParameter("mapControlTypes", Bitrix.GoogleMapControlType.toString(types)); },
	getMapOptions: function(){ return Bitrix.GoogleMapOption.fromString(this.getParameter("mapOptions", null)); },
	setMapOptions: function(options){ this.setParameter("mapOptions", Bitrix.GoogleMapOption.toString(options)); },
	getDraggableCursor: function(){ return this.getParameter("draggableCursor", "", true); },
	setDraggableCursor: function(cursor){ this.setParameter("draggableCursor", cursor); },
	getDraggingCursor: function(){ return this.getParameter("draggingCursor", "", true); },
	setDraggingCursor: function(cursor){ this.setParameter("draggingCursor", cursor); },
	getMarkers: function() { 
		var r = this.getParameter("markers", null); 
		if(!r) this.setParameter("markers", (r = []));
		return r;
	},
	setMarkers: function(markers) { this.setParameter("markers", markers); },
	getMarkupPanel: function() { return this.getParameter("markupPanel", false); },
	setMarkupPanel: function(markupPanel) { this.setParameter("markupPanel", markupPanel); },
	getSearchPanel: function() { return this.getParameter("searchPanel", false); },
	setSearchPanel: function(searchPanel) { this.setParameter("searchPanel", searchPanel); },	
	toObject: function() {
		var r = {};
		r.mapType = Bitrix.GoogleMapType.toString(this.getMapType());
		r.center = this.getCenter().toObject();
		r.zoom = this.getZoom();
		r.markers = [];
		var markers = this.getMarkers();
		for(var i = 0; i < markers.length; i++)
			r.markers.push(markers[i].toObject());
		return r;
	},
	toString: function() {
		var r = "";
		r += "mapType: \"" + Bitrix.GoogleMapType.toString(this.getMapType()) + "\"";
		r += ", center: " + this.getCenter().toString();
		r += ", zoom: " + this.getZoom();
		r += ", markers: [ ";
		var markers = this.getMarkers();
		for(var i = 0; i < markers.length; i++) {
			if(i != 0) r += ", ";
			r += markers[i].toString();
		}
		r += " ]";
		return "{ " + r + " }";
	}	
}
Bitrix.GoogleMapsData.create = function(parameters){
	var self = new Bitrix.GoogleMapsData();
	self.initialize(parameters);
	return self;
}
Bitrix.GoogleMapsData.fromObject = function(obj) {
	if(obj instanceof Bitrix.GoogleMapsData) return obj;
	var self = new Bitrix.GoogleMapsData();
	
	var parameters = {};
	if(obj && typeof(obj) == "object") {
		var initState = "initialState" in obj ? obj.initialState : null;
		if(initState && typeof(initState) == "object") {
			if("mapType" in initState) {
				var t = Bitrix.GoogleMapType.tryParse(initState.mapType);
				parameters.mapType = t.length > 0 ? t[0] : Bitrix.GoogleMapType.roadmap;
			}		
			if("center" in initState) {
				var c = Bitrix.GoogleMapsCoordinate.fromObject(initState.center, null);
				parameters.center = c ? c : Bitrix.GoogleMapsCoordinate.getMSK();
			}
			if("zoom" in initState) 
				parameters.zoom = Bitrix.TypeUtility.isNumber(initState.zoom) ? initState.zoom : 12;
				
			if("markers" in initState && initState.markers instanceof Array) {
				var markerData = initState.markers;
				var markers = parameters.markers = [];
				for(var i = 0; i < markerData.length; i++) 
					markers.push(Bitrix.GoogleMapsMarker.fromObject(markerData[i]));
			}
		}
	}
	self.initialize(parameters);
	if(obj && typeof(obj) == "object") {
		if("id" in obj) self.setEntityId(obj.id);
		if("mapControlTypes" in obj) self.setMapControlTypes(obj.mapControlTypes);
		if("mapOptions" in obj) self.setMapOptions(obj.mapOptions);
		if("draggableCursor" in obj) self.setDraggableCursor(obj.draggableCursor);
		if("draggingCursor" in obj) self.setDraggingCursor(obj.draggingCursor);
		if("markupPanel" in obj) self.setMarkupPanel(obj.markupPanel);
		if("searchPanel" in obj) self.setSearchPanel(obj.searchPanel);		
	}
	return self;	
}
Bitrix.GoogleMapsEntity = function Bitrix$GoogleMapsEntity(){
    this._initialized = false;
	this._constructed = false;
    this._data = null;
	this._parentNode = null;
	this._map = null;
	this._markerPane = null;
	this._searchPane = null;
	this._centerChangeListener = null;
	this._zoomChangeListener = null;
	this._mapTypeChangeListener = null;
	this._markerChangeListener = null;
	this._markerClickListener = null;
}
Bitrix.GoogleMapsEntity.prototype={
	initialize: function(data, parentNode){
	    if(this._initialized) return;
		this.setData(data);
		this.setParentNode(parentNode);
		this._initialized = true;		
	},
	construct: function() {
		if(this._constructed) return;
		var pNode = this._parentNode;
		if(!pNode) throw "GoogleMapsEntity: parentNode is not assigned!";
		var d = this._data;
		if(!d) throw "GoogleMapsEntity: data is not assigned!";
		var options = {
			center: d.getCenter().toGoogleMapsLatLng(),
			zoom: d.getZoom(),
			mapTypeId: Bitrix.GoogleMapType.toGoogleMapTypeId(d.getMapType())
		};
		
		var dataControlTypes = d.getMapControlTypes();
		var dataOptions = d.getMapOptions();
		
		options.disableDefaultUI = Bitrix.ArrayHelper.findInArray(Bitrix.GoogleMapOption.disableDefaultUI, dataOptions) >= 0;
		options.disableDoubleClickZoom = Bitrix.ArrayHelper.findInArray(Bitrix.GoogleMapOption.disableDoubleClickZoom, dataOptions) >= 0;
		options.draggable = Bitrix.ArrayHelper.findInArray(Bitrix.GoogleMapOption.disableDragging, dataOptions) < 0;
		options.scrollwheel = Bitrix.ArrayHelper.findInArray(Bitrix.GoogleMapOption.disableScrollWheel, dataOptions) < 0;
		options.keyboardShortcuts = Bitrix.ArrayHelper.findInArray(Bitrix.GoogleMapOption.disableKeyboardShortcuts, dataOptions) < 0;
		
		var draggableCur = d.getDraggableCursor();
		options.draggableCursor = draggableCur.length > 0 ? draggableCur : !jsUtils.IsOpera() ? "url(//maps.gstatic.com/intl/en_us/mapfiles/openhand_8_8.cur), auto" : "pointer";
				
		var draggingCur = d.getDraggingCursor();
		options.draggingCursor = draggingCur.length > 0 ? draggingCur : !jsUtils.IsOpera() ? "url(//maps.gstatic.com/intl/en_us/mapfiles/closedhand_8_8.cur), auto" : "pointer";

		if(dataControlTypes.length > 0)
			for(var i = 0; i < dataControlTypes.length; i++){
				switch(dataControlTypes[i]){
					case Bitrix.GoogleMapControlType.largeMap:
						options.navigationControl = true;
						options.navigationControlOptions = { style: google.maps.NavigationControlStyle.ZOOM_PAN };
						break;
					case Bitrix.GoogleMapControlType.smallMap:
						options.navigationControl = true;
						options.navigationControlOptions = { style: google.maps.NavigationControlStyle.SMALL };
						break;
					case Bitrix.GoogleMapControlType.horBarMapType:
						options.mapTypeControl = true;
						options.mapTypeControlOptions = { style: google.maps.MapTypeControlStyle.HORIZONTAL_BAR };
						break;
					case Bitrix.GoogleMapControlType.dropDownMenuMapType:
						options.mapTypeControl = true;
						options.mapTypeControlOptions = { style: google.maps.MapTypeControlStyle.DROPDOWN_MENU };
						break;							
					case Bitrix.GoogleMapControlType.scale:
						options.scaleControl = true;
						options.scaleControlOptions = { style: google.maps.ScaleControlStyle.DEFAULT };
						break;							
				}							
			}
		
		this._map = new google.maps.Map(pNode, options);
		if(d.getMarkupPanel()) {
			var overlay = Bitrix.GoogleMapsMarkerOverlay.create( { map:this._map, parentNode:pNode } );
			this._markerPane = new Bitrix.GoogleMapsMarkupPanel({ map: this, overlay: overlay });
		}
		
		if(d.getSearchPanel())
			this._searchPane = new Bitrix.GoogleMapsSearchPanel({ map: this });		
		
		var markers = d.getMarkers();
		for(var i = 0; i < markers.length; i++)
			this._setupMarker(markers[i]);
			
		google.maps.event.addListener(this._map, "center_changed", Bitrix.TypeUtility.createDelegate(this, this._handleCenterChange));
		google.maps.event.addListener(this._map, "zoom_changed", Bitrix.TypeUtility.createDelegate(this, this._handleZoomChange));
		google.maps.event.addListener(this._map, "maptypeid_changed", Bitrix.TypeUtility.createDelegate(this, this._handleMapTypeChange));

		this._constructed = true;	
	},
	_getSetting: function(name) {
		return typeof(Bitrix.GoogleMapsSettings) == "object" && name in Bitrix.GoogleMapsSettings ? Bitrix.GoogleMapsSettings[name] : "";
	},		
	getSourceMap: function() { return this._map; },
	getData: function(){ return this._data ? this._data : (this._data = new Bitrix.GoogleMapsData()); },
	setData: function(data){ this._data = data; },	
	getParentNode: function(){ return this._parentNode; },
	setParentNode: function(parentNode) { 
		if(!Bitrix.TypeUtility.isDomElement(parentNode)) throw "GoogleMapsEntity: parentNode is not valid!";
		this._parentNode = parentNode; 
	},
	addCenterChangeListener: function(listener){
		if(this._centerChangeListener == null)
			this._centerChangeListener = new Bitrix.EventPublisher();
		this._centerChangeListener.addListener(listener);
	},
	removeCenterChangeListener: function(listener){
		if(this._centerChangeListener == null) return;
		this._centerChangeListener.removeListener(listener);
	},	
	_handleCenterChange: function() {
		if(!(this._data && this._map)) return;
		this._data.setCenter(Bitrix.GoogleMapsCoordinate.fromGoogleMapsLatLng(this._map.getCenter(), null));
		if(this._centerChangeListener) this._centerChangeListener.fire(this);
	},
	addZoomChangeListener: function(listener) {
		if(this._zoomChangeListener == null)
			this._zoomChangeListener = new Bitrix.EventPublisher();
		this._zoomChangeListener.addListener(listener);
	},
	removeZoomChangeListener: function(listener) {
		if(this._zoomChangeListener == null) return;
		this._zoomChangeListener.removeListener(listener);
	},	
	_handleZoomChange: function() {
		if(!(this._data && this._map)) return;
		this._data.setZoom(this._map.getZoom());
		if(this._zoomChangeListener) this._zoomChangeListener.fire(this);
	},	
	addMapTypeChangeListener: function(listener) {
		if(this._mapTypeChangeListener == null)
			this._mapTypeChangeListener = new Bitrix.EventPublisher();
		this._mapTypeChangeListener.addListener(listener);
	},
	removeMapTypeChangeListener: function(listener) {
		if(this._mapTypeChangeListener == null) return;
		this._mapTypeChangeListener.removeListener(listener);
	},	
	addMarkerChangeListener: function(listener) {
		if(this._markerChangeListener == null)
			this._markerChangeListener = new Bitrix.EventPublisher();
		this._markerChangeListener.addListener(listener);
	},
	removeMarkerChangeListener: function(listener) {
		if(this._markerChangeListener == null) return;
		this._markerChangeListener.removeListener(listener);
	},	
	addMarkerClickListener: function(listener) {
		if(this._markerClickListener == null)
			this._markerClickListener = new Bitrix.EventPublisher();
		this._markerClickListener.addListener(listener);
	},
	removeMarkerClickListener: function(listener) {
		if(this._markerClickListener == null) return;
		this._markerClickListener.removeListener(listener);
	},	
	_handleMapTypeChange: function() {
		if(!(this._data && this._map)) return;
		this._data.setMapType(Bitrix.GoogleMapType.fromGoogleMapTypeId(this._map.getMapTypeId()));
		if(this._mapTypeChangeListener) this._mapTypeChangeListener.fire(this);
	},
	_handleMarkerClick: function(sender) {
		if(this._markerClickListener) this._markerClickListener.fire(this, { marker: sender });
	},	
	getMarkers: function() { return this.getData().getMarkers(); },
	addMarker: function(marker) {
		if(!marker) throw "marker is not defined";
		this.getMarkers().push(marker);
		this._setupMarker(marker);
		if(this._markerChangeListener) this._markerChangeListener.fire(this, { marker: marker, action:Bitrix.GoogleMapsAction.add });
	},
	_setupMarker: function(marker) {
		var isDesignMode = this.isDesignMode();
		marker.setDraggable(isDesignMode);
		//marker.setClickable(isDesignMode);
		marker.setClickable(true);
	    marker.setIcon(this._getSetting("standardMarkerImageUrl"));
		marker.setMap(this);
		marker.addClickListener(Bitrix.TypeUtility.createDelegate(this, this._handleMarkerClick));			
	},
	removeMarker: function(marker) {
		var markers = this.getMarkers();
		for(var i = 0; i < markers.length; i++) {
			if(markers[i] != marker) continue;
			if(this._markerChangeListener) this._markerChangeListener.fire(this, { marker: marker, action:Bitrix.GoogleMapsAction.remove });
			markers.splice(i, 1);
			marker.release();
			return;
		}
	},
	openMarkerEditor: function(marker) {	
		var p = this._markerPane;
		if(p) p.openMarkerEditor(marker);
	},
	startMarkerAdd: function() {
		var p = this._markerPane;
		p.startMarkerAdd();
	},
	startDrag: function() {
		var p = this._markerPane;
		p.startDrag();
	},
	setMapType: function(mapType) { this._map.setMapTypeId(Bitrix.GoogleMapType.toGoogleMapTypeId(mapType)); },
	getCenter: function() { return Bitrix.GoogleMapsCoordinate.fromGoogleMapsLatLng(this._map.getCenter()); },
	setCenter: function(coord) {
		this._map.setCenter(coord.toGoogleMapsLatLng());
	},
	setCenterByGoogleLatLng: function(latLng) {
		this._map.setCenter(latLng);
	},	
	setZoom: function(zoom) { this._map.setZoom(zoom); },
	isDesignMode: function() { 
		var d = this._data;
		return d && (d.getMarkupPanel() || d.getSearchPanel());	
	}
}
Bitrix.GoogleMapsEntity.create = function(data, parentNode) {
    var self = new Bitrix.GoogleMapsEntity();
    self.initialize(data, parentNode);
    return self;
}
Bitrix.GoogleMapsEntity.addMarker = function(marker, map, options) {
	if(!marker) throw "GoogleMapsEntity: marker is not defined!";
	if(!map) throw "GoogleMapsEntity: map is not defined!";
	var imgUrl = Bitrix.TypeUtility.tryGetObjProp(options, "standardMarkerImageUrl", "//maps.gstatic.com/intl/en_ALL/mapfiles/ms/micons/blue-dot.png");	
	var gmMarker = new google.maps.Marker({
		position: marker.getPosition().toGoogleMapsLatLng(),
		icon: imgUrl,
		flat: true,
		title:  marker.getTitle(),
		map: map,
		draggable: Bitrix.TypeUtility.tryGetObjProp(options, "draggable", false),
		clickable: Bitrix.TypeUtility.tryGetObjProp(options, "clickable", false)
		});	
	var clickHandler = Bitrix.TypeUtility.tryGetObjProp(options, "clickHandler", null);
	if(clickHandler) google.maps.event.addListener(gmMarker, 'click', clickHandler);
	if(Bitrix.TypeUtility.tryGetObjProp(options, "bind", false)) marker.setupByGoogleMapsMarker(gmMarker);	
}

Bitrix.GoogleMapsMarkerOverlay = function Bitrix$GoogleMapsMarkerOverlay() {
	this._map = null;
	this._dropCallback = null;
	this._soaringDiv = null; 
	this._soaringStartPos = null;
	this._mousePos = null;
	this._parentNode = null;
	this._mouseClickHandler = Bitrix.TypeUtility.createDelegate(this, this._handleMouseClick);
	this._mouseMoveHandler = Bitrix.TypeUtility.createDelegate(this, this._handleMouseMove);
}
Bitrix.TypeUtility.copyPrototype(Bitrix.GoogleMapsMarkerOverlay, google.maps.OverlayView);
Bitrix.GoogleMapsMarkerOverlay.prototype.initialize = function(options) {
	Bitrix.TypeUtility.copyPrototype(Bitrix.GoogleMapsMarkerOverlay, google.maps.OverlayView);
	if(!options) throw "Options is not defined!";
	if(!("map" in options)) throw "Map is not found!";
	this._map = options.map;
	this.setMap(this._map);
	
	if(!"parentNode" in options) throw "Could not find parent node!";
	this._parentNode = options.parentNode;
	Bitrix.EventUtility.addEventListener(document.body, "mousemove", this._mouseMoveHandler, false);	
}
Bitrix.GoogleMapsMarkerOverlay.prototype.onAdd = function() {}
Bitrix.GoogleMapsMarkerOverlay.prototype.onRemove = function() {}
Bitrix.GoogleMapsMarkerOverlay.prototype.draw = function() {}

Bitrix.GoogleMapsMarkerOverlay.prototype.startSoaring = function(imgUrl, mousePos, dropCallback) {
	if(!Bitrix.TypeUtility.isNotEmptyString(imgUrl)) throw "GoogleMapsMarkerOverlay: imgUrl is not defined!";
	var c = this._parentNode;
	var d = this._soaringDiv = document.createElement("DIV");
	c.appendChild(d);
	d.style.position = "absolute";
	d.style.left = d.style.top = "0px";
	d.style.cursor = "none";
	
	var img = document.createElement("IMG");
	d.appendChild(img);	
	img.src = imgUrl;
	img.style.left = img.style.top = "0px"; 
	img.style.cursor = "none";
	
	if(!mousePos) mousePos = this._mousePos;
	if(mousePos) {
		var startPos = this._toRelativePos(mousePos);
		var r = Bitrix.ElementPositioningUtility.getElementRect(d);
		startPos.x -= r.width / 2;
		startPos.y -= r.height + 3;
		d.style.left = startPos.x.toFixed(0) + "px";
		d.style.top = startPos.y.toFixed(0) + "px";
		this._soaringStartPos = startPos;
	}
	else
		this._soaringStartPos = { x:0, y:0 };
	
	this._dropCallback = dropCallback;
	Bitrix.EventUtility.addEventListener(c, "click", this._mouseClickHandler, false);	
}
Bitrix.GoogleMapsMarkerOverlay.prototype.stopSoaring = function() {
	Bitrix.EventUtility.removeEventListener(this._parentNode, "click", this._mouseClickHandler);
	
	this._dropCallback = null;
	
	if(this._soaringStartPos) {
		delete this._soaringStartPos;
		this._soaringStartPos = null;	
	}
	
	if(this._soaringDiv) {
		this._soaringDiv.parentNode.removeChild(this._soaringDiv);
		this._soaringDiv = null;
	}
}
Bitrix.GoogleMapsMarkerOverlay.prototype._toRelativePos = function(mousePos) {
	var r = Bitrix.ElementPositioningUtility.getElementRect(this._parentNode);
	return { y:mousePos.y + document.body.scrollTop + document.documentElement.scrollTop - r.top, x:mousePos.x + document.body.scrollLeft + document.documentElement.scrollLeft - r.left }
}
Bitrix.GoogleMapsMarkerOverlay.prototype._handleMouseMove = function(e) {
	if (!e && "event" in window) e = window.event;
	var mousePos = this._mousePos = this._toRelativePos({ y: e.clientY, x: e.clientX });
	
	var d = this._soaringDiv;
	if(!d) return;
	var r = Bitrix.ElementPositioningUtility.getElementRect(d);
	var curPos = { x: mousePos.x - r.width / 2, y: mousePos.y - r.height - 3 }
	var startPos = this._soaringStartPos ? this._soaringStartPos : curPos;	
	var shiftY = curPos.y - startPos.y;
    var shiftX = curPos.x - startPos.x;
	
	var curLeft = parseInt(d.style.left);
	var curTop = parseInt(d.style.top);
	
	if (isNaN(curLeft) || isNaN(curTop)) {
		var r = Bitrix.ElementPositioningUtility.getElementRect(d);
		curTop = r.top;
		curLeft = r.left;
	}

	var top = curTop + shiftY;	
	var left = curLeft + shiftX;
	
	d.style.top = top.toFixed(0) + "px";
	d.style.left = left.toFixed(0) + "px";
	
	delete this._soaringStartPos;
	this._soaringStartPos = curPos;
}

Bitrix.GoogleMapsMarkerOverlay.prototype._handleMouseClick = function(e) {	
	Bitrix.EventUtility.stopEventPropagation(e);	
	var d = this._soaringDiv;		
	var callback = this._dropCallback;
	if(callback) { 
		var latLng = null;
		if(d) {
			var parRect = Bitrix.ElementPositioningUtility.getElementRect(this._parentNode);
			var setupRect = Bitrix.ElementPositioningUtility.getElementRect(d);
		
			var y = setupRect.top  - parRect.top + setupRect.height;
			var x = setupRect.left - parRect.left + setupRect.width / 2;
			latLng = this.getProjection().fromContainerPixelToLatLng(new google.maps.Point(x, y));
		}
		callback(latLng);
	}
}

Bitrix.GoogleMapsMarkerOverlay.create = function(options) {
	var self = new Bitrix.GoogleMapsMarkerOverlay();
	self.initialize(options);
	return self;
}

Bitrix.GoogleMapsMarkupPanelMode = {
	drag: 1,
	centerSetup: 2,
	markerAdd: 3
}

Bitrix.GoogleMapsAction = {
	add: 1,
	modify: 2,
	remove: 3
}

Bitrix.GoogleMapsMarkupPanel = function Bitrix$GoogleMapsMarkupPanel(options) {
	if(!options) throw "Bitrix.GoogleMapsMarkupPanel: options is not defined!";
	
	var map = this._map = "map" in options ? options.map : null;
	if(!map) throw "GoogleMapsMarkupPanel: map is not found!";
	
	var overlay = this._overlay = "overlay" in options ? options.overlay : null;
	if(!overlay) throw "GoogleMapsMarkupPanel: overlay is not found!";
	
	this._mode = Bitrix.GoogleMapsMarkupPanelMode.drag;
	
	var container = this._container = document.createElement("DIV");
	var tab = document.createElement("TABLE");
	container.appendChild(tab);
	var r = tab.insertRow(-1);
	
	var dragContainer = this._dragContainer = document.createElement("DIV");
	r.insertCell(-1).appendChild(dragContainer);
	var dragImg = this._dragImg = document.createElement("IMG");
	dragContainer.appendChild(dragImg);
	dragContainer.title = this._getMessage("MapDragging");
	
	var markerContainer = this._markerContainer = document.createElement("DIV");
	r.insertCell(-1).appendChild(markerContainer);
	var markerImg = this._markerImg = document.createElement("IMG");
	markerContainer.appendChild(markerImg);
	markerContainer.title = this._getMessage("MarkerAdding");
	
	var centerContainer = this._centerContainer = document.createElement("DIV");
	r.insertCell(-1).appendChild(centerContainer);
	var centerImg = this._centerImg = document.createElement("IMG");
	centerContainer.appendChild(centerImg);
	centerContainer.title = this._getMessage("CenterSetting");	
	
	this._isSoaring = false;
	this._centerDropHandler = Bitrix.TypeUtility.createDelegate(this, this._handleCenterDrop);
	this._markerDropHandler = Bitrix.TypeUtility.createDelegate(this, this._handleMarkerDrop);
	this._centerMarkerChangeHandler = Bitrix.TypeUtility.createDelegate(this, this._handleCenterMarkerChange);
	this._markerClickHandler = Bitrix.TypeUtility.createDelegate(this, this._handleMarkerClick);
	
	this._centerMarker = null;
	this._suppressCenterMarkerChange = false;
	this._suppressMapCenterChange = false;
	this._markers = [];
	this._infoWin = null;
	
	this._setupCenterMarkerByPos(map.getCenter());
	map.addMarkerClickListener(this._markerClickHandler);
	var srcMap = map.getSourceMap();
	google.maps.event.addListener(srcMap, "center_changed", Bitrix.TypeUtility.createDelegate(this, this._handleMapCenterChange));
	Bitrix.EventUtility.addEventListener(dragContainer, "click", Bitrix.TypeUtility.createDelegate(this, this._handleDragMouseClick), false);
	Bitrix.EventUtility.addEventListener(centerContainer, "click", Bitrix.TypeUtility.createDelegate(this, this._handleCenterMouseClick), false);
	Bitrix.EventUtility.addEventListener(markerContainer, "click", Bitrix.TypeUtility.createDelegate(this, this._handleMarkerMouseClick), false);
	
	srcMap.controls[google.maps.ControlPosition.TOP].push(this._container);
	this.layout();
}
Bitrix.GoogleMapsMarkupPanel.prototype = {
	getMode: function() { return this._mode; },
	_getSetting: function(name) {
		return typeof(Bitrix.GoogleMapsSettings) == "object" && name in Bitrix.GoogleMapsSettings ? Bitrix.GoogleMapsSettings[name] : "";
	},
	layout: function() {
		this._dragImg.src = this._getSetting(this._mode == Bitrix.GoogleMapsMarkupPanelMode.drag ? "dragPanelIconSelImageUrl" : "dragPanelIconImageUrl");
		this._centerImg.src = this._getSetting(this._mode == Bitrix.GoogleMapsMarkupPanelMode.centerSetup ? "centerPanelIconSelImageUrl" : "centerPanelIconImageUrl");
		this._markerImg.src = this._getSetting(this._mode == Bitrix.GoogleMapsMarkupPanelMode.markerAdd ? "markerPanelIconSelImageUrl" : "markerPanelIconImageUrl");
	},
	openMarkerEditor: function(marker) {
		if(!marker) throw "GoogleMapsMarkupPanel: marker is not defined!";
		this._openMarkerEditor(marker);			
	},
	_openMarkerEditor: function(marker) {
		if(this._mode != Bitrix.GoogleMapsMarkupPanelMode.drag) return;
		if(!marker) throw "GoogleMapsMarkupPanel: marker is not defined!";
		var win = this._getInfoWin();
		var winOpt = win.getOptions();
		winOpt.marker = marker.getSourceMarker();
		winOpt.mode = Bitrix.GoogleMapsMarkupPanelMode.drag;
		win.open();			
	},	
	_setupCenterMarkerByPos: function(pos) {
		if(!pos) throw "GoogleMapsMarkupPanel: pos is not defined!";
		if(this._centerMarker) {
			this._centerMarker.removeDragEndListener(this._centerMarkerChangeHandler);
			this._centerMarker.release();
			delete this._centerMarker;
		}
		this._centerMarker = Bitrix.GoogleMapsMarker.create({ position: pos, icon: this._getSetting("centerMarkerImageUrl"), draggable: true, clickable: true });
		this._centerMarker.setMap(this._map);
		this._centerMarker.addDragEndListener(this._centerMarkerChangeHandler);
	},	
	_getInfoWin: function() { 
		if(this._infoWin) return this._infoWin;
		var win = this._infoWin = new Bitrix.GoogleMapsMarkerInfoWindow({ map: this._map.getSourceMap() }); 
		win.addCloseListener(Bitrix.TypeUtility.createDelegate(this, this._handleInfoWinClose));
		return win;
	},
	_stopSoaring: function() {
		if(!this._isSoaring) return;
		this._overlay.stopSoaring();
		this._isSoaring = false;		
	},
	_handleDragMouseClick: function(e) {
		Bitrix.EventUtility.stopEventPropagation(e);
		this._stopSoaring();
		this._mode = Bitrix.GoogleMapsMarkupPanelMode.drag;
		this.layout();
	},
	_handleCenterMouseClick: function(e) {
		if (!e && "event" in window) e = window.event;
		Bitrix.EventUtility.stopEventPropagation(e);
		this._stopSoaring();
		this._mode = this._mode != Bitrix.GoogleMapsMarkupPanelMode.centerSetup ? Bitrix.GoogleMapsMarkupPanelMode.centerSetup : Bitrix.GoogleMapsMarkupPanelMode.drag;
		this.layout();
		if(this._mode == Bitrix.GoogleMapsMarkupPanelMode.centerSetup) {
			this._overlay.startSoaring(this._getSetting("centerMarkerImageUrl"), { y:e.clientY, x:e.clientX }, this._centerDropHandler);
			this._isSoaring = true;
		}
	},
	_handleCenterMenuSetHereClick: function() {},
	_handleMarkerMouseClick: function(e) {
		if (!e && "event" in window) e = window.event;
		Bitrix.EventUtility.stopEventPropagation(e);	
		this.startMarkerAdd({ y: e.clientY, x: e.clientX });
	},
	startMarkerAdd: function(mousePos) {
		this._stopSoaring();
		this._mode = this._mode != Bitrix.GoogleMapsMarkupPanelMode.markerAdd ? Bitrix.GoogleMapsMarkupPanelMode.markerAdd : Bitrix.GoogleMapsMarkupPanelMode.drag;
		this.layout();
		if(this._mode == Bitrix.GoogleMapsMarkupPanelMode.markerAdd) {
			var imgUrl = this._getSetting("standardMarkerImageUrl");
			if(!Bitrix.TypeUtility.isNotEmptyString(imgUrl)) imgUrl = "//maps.gstatic.com/intl/en_ALL/mapfiles/ms/micons/blue-dot.png";
			this._overlay.startSoaring(imgUrl, mousePos, this._markerDropHandler);
			this._isSoaring = true;
		}		
	},
	startDrag: function() {
		this._stopSoaring();
		this._mode = Bitrix.GoogleMapsMarkupPanelMode.drag;
		this.layout();		
	},
	_handleCenterDrop: function(latLng) {
		this.startDrag();
		var pos = Bitrix.GoogleMapsCoordinate.fromGoogleMapsLatLng(latLng);
		this._setupCenterMarkerByPos(pos);
		this._map.setCenter(pos);		
	},
	_handleMarkerDrop: function(latLng) {
		this._stopSoaring();
		this._mode = Bitrix.GoogleMapsMarkupPanelMode.drag;
		this.layout();
		
		var marker = Bitrix.GoogleMapsMarker.create({ position: Bitrix.GoogleMapsCoordinate.fromGoogleMapsLatLng(latLng), icon: this._getSetting("standardMarkerImageUrl") });
		this._map.addMarker(marker);
		var win = this._getInfoWin();
		var winOpt = win.getOptions();
		winOpt.marker = marker.getSourceMarker();
		winOpt.mode = Bitrix.GoogleMapsMarkupPanelMode.markerAdd;
		win.open();
	},
	_handleCenterMarkerChange: function() { 
		if(this._suppressCenterMarkerChange || !this._centerMarker) return;
		this._suppressMapCenterChange = true;
		this._map.setCenter(this._centerMarker.getPosition()); 
		this._suppressMapCenterChange = false;
	},
	_handleMapCenterChange: function() { 
		if(this._suppressMapCenterChange || !this._centerMarker) return;
		this._suppressCenterMarkerChange = true;
		this._centerMarker.setPosition(this._map.getCenter()); 
		this._suppressCenterMarkerChange = false;
	},	
	_handleMarkerClick: function(sender, args) { this._openMarkerEditor(args.marker); },
	_handleInfoWinClose: function(sender, args) {
		var marker = sender.getMarker();
		if(!marker) throw "GoogleMapsMarkupPanel: Could not find marker!";
		var markerInd = -1;
		var markers = this._map.getMarkers();
		for(var i = 0; i < markers.length; i++) {
			if(markers[i].getSourceMarker() != marker) continue;
			markerInd = i;			
			break;
		}		
		if(markerInd < 0) throw "GoogleMapsMarkupPanel: Could not find marker index!";
		
		if(args.buttonId == Bitrix.GoogleMapsInfoWindowButton.bDelete)
			this._map.removeMarker(markers[markerInd]);
			
		var senderOpt = sender.getOptions();
		if("mode" in senderOpt && senderOpt.mode == Bitrix.GoogleMapsMarkupPanelMode.markerAdd)
			window.setTimeout(Bitrix.TypeUtility.createDelegate(this, function(){ this.startMarkerAdd(null); }), 100);
	},
    _getMessage: function(id) {
        var id = "MapsMarkupPanel$" + id;
	    return "BITRIX_GOOGLE_MAPS_MSG" in window && id in window.BITRIX_GOOGLE_MAPS_MSG ? window.BITRIX_GOOGLE_MAPS_MSG[id] : id;
    }	
}
Bitrix.GoogleMapsSearchPanel = function Bitrix$GoogleMapsSearchPanel(options) {
	if(!options) throw "GoogleMapsSearchPanel: options is not defined!";
	
	var map = this._map = "map" in options ? options.map : null;
	if(!map) throw "GoogleMapsSearchPanel: map is not found!";
	
	this._geocoder = null;
	this._relults = null;
	this._enableBounds = true;
	
	var container = this._container = document.createElement("DIV");
	container.className = Bitrix.GoogleMapsSearchPanel.styles.container;

	var resultContainerWrapper = this._resultContainerWrapper = document.createElement("DIV");
	container.appendChild(resultContainerWrapper);	
	resultContainerWrapper.className = Bitrix.GoogleMapsSearchPanel.styles.resultContainerWrapper;
	resultContainerWrapper.style.display = "none";	
	
	var resultContainer = this._resultContainer = document.createElement("DIV");
	resultContainerWrapper.appendChild(resultContainer);
	resultContainer.className = Bitrix.GoogleMapsSearchPanel.styles.resultContainer;

	var cmdBarWrapper = this._cmdBarWrapper = document.createElement("DIV");	
	container.appendChild(cmdBarWrapper);	
	cmdBarWrapper.className = Bitrix.GoogleMapsSearchPanel.styles.cmdBarWrapper;
	cmdBarWrapper.style.display = "none";
	var cmdBar = document.createElement("DIV");	
	cmdBarWrapper.appendChild(cmdBar);
	cmdBar.className = Bitrix.GoogleMapsSearchPanel.styles.cmdBar;	
	var clearResultsLnk = document.createElement("A");
	cmdBar.appendChild(clearResultsLnk);
	clearResultsLnk.href = "";
	clearResultsLnk.onclick = Bitrix.TypeUtility.createDelegate(this, this._handleClearResultsLinkClick);
	clearResultsLnk.innerHTML = this._getMessage("Clear");	
	
	var tbl = document.createElement("TABLE");
	tbl.className = "bx-gmaps-search-panel-control-tab";
	container.appendChild(tbl);
	tbl.border = tbl.cellPadding = tbl.cellSpacing ="0";
	
	var row = tbl.insertRow(-1);
	
	var inputC = row.insertCell(-1);
	inputC.className = Bitrix.GoogleMapsSearchPanel.styles.placeCell;
	
	var input = this._input = document.createElement("INPUT");
	input.type = "text";
	inputC.appendChild(input);
	input.onkeypress = Bitrix.TypeUtility.createDelegate(this, this._handleSearchInputEnter);
	
	var btnC = row.insertCell(-1);
	btnC.className = Bitrix.GoogleMapsSearchPanel.styles.buttonCell;
	
	var btnCnr =  document.createElement("DIV");
	btnC.appendChild(btnCnr);
	btnCnr.className = Bitrix.GoogleMapsSearchPanel.styles.buttonContainer;
	
	var btn = this._btn = document.createElement("I");
	//btn.type = "button";
	btn.innerHTML = "<b>" + this._getMessage("Search") + "</b>";
	btnCnr.appendChild(btn);
	
	Bitrix.EventUtility.addEventListener(btnCnr, "click", Bitrix.TypeUtility.createDelegate(this, this._handleSearchBtnClick));	
	
	var srcMap = map.getSourceMap();
	srcMap.controls[google.maps.ControlPosition.BOTTOM_LEFT].push(this._container);
	this.layout();	
	this._zIndexChecks = 0;
	window.setTimeout(Bitrix.TypeUtility.createDelegate(this, this._setContainerZIndex), 50);
}
Bitrix.GoogleMapsSearchPanel.prototype = {
	layout: function() { },
	_setContainerZIndex: function() {
		var zIndex = 1200;
		if(this._container.style.zIndex == zIndex && this._zIndexChecks >= 10) return; 
		if(this._container.style.zIndex == zIndex) 
			this._zIndexChecks++;
		else {
			this._container.style.zIndex = zIndex;
			this._zIndexChecks = 1;
		}
		window.setTimeout(Bitrix.TypeUtility.createDelegate(this, this._setContainerZIndex), 100 * this._zIndexChecks);
	},
	_getGeocoder: function() { return this._geocoder ? this._geocoder : (this._geocoder = new google.maps.Geocoder()); },
	_handleSearchInputEnter: function(e) { 
		if(!(e ? e : (e = window.event))) return;
		if(e.keyCode != 13) return true;
		this._enableBounds = true;
		this._launch();
		Bitrix.EventUtility.stopEventPropagation(e);
		return false; //disable form submit
	},
	_handleSearchBtnClick: function() { this._enableBounds = true; this._launch(); },
	_launch: function() {
		var place = this._input.value;
		if(place.length == 0) return;
		var cdr = this._getGeocoder();
		if(this._enableBounds)
			cdr.geocode({'address':place, 'bounds': this._map.getSourceMap().getBounds(), 'language':this._getSetting("language", "ru")}, Bitrix.TypeUtility.createDelegate(this, this._handleSearchRequestComplete));		
		else
			cdr.geocode({'address':place, 'language':this._getSetting("language", "ru")}, Bitrix.TypeUtility.createDelegate(this, this._handleSearchRequestComplete));					
	},
	_handleSearchRequestComplete: function(results, status) {
		var resultC = this._resultContainer;
		resultC.innerHTML = "";
		
		if (status != google.maps.GeocoderStatus.OK) { 
			if(status == google.maps.GeocoderStatus.ZERO_RESULTS) {
				if(this._enableBounds) {
					this._enableBounds = false; 
					this._launch();
					return;
				}				
				resultC.innerHTML = this._getMessage("ZeroResults");
			}
			else if(status == google.maps.GeocoderStatus.OVER_QUERY_LIMIT)
				resultC.innerHTML = this._getMessage("OverQueryLimit");
			else if(status == google.maps.GeocoderStatus.REQUEST_DENIED)
				resultC.innerHTML = this._getMessage("RequestDenied");	
			else if(status == google.maps.GeocoderStatus.INVALID_REQUEST)
				resultC.innerHTML = this._getMessage("InvalidRequest");				
			else
				resultC.innerHTML = this._getMessage("GeneralServerError");
		}
		else {
			var tab = document.createElement("TABLE");
			tab.className = "bx-gmaps-search-panel-result-tab";
			resultC.appendChild(tab);
			var resultArr = this.getResults();
			for(var i = 0; i < results.length; i++) {
				var r = new Bitrix.GoogleMapsSearchResult({ result: results[i], parentEl: tab, map: this._map });
				resultArr.push(r);
			}		
		}
		var cmdBarWrapper = this._cmdBarWrapper;
		cmdBarWrapper.style.display = "";
		cmdBarWrapper.style.left = "0px";
		var cmdBarWrapperRect = Bitrix.ElementPositioningUtility.getElementRect(cmdBarWrapper);
		cmdBarWrapper.style.top = "-" + (cmdBarWrapperRect.height + 2) + "px";	
		
		var resultW = this._resultContainerWrapper;
		resultW.style.display = "";
		resultW.style.left = "0px";
		resultW.style.top = "-" + (cmdBarWrapperRect.height + Bitrix.ElementPositioningUtility.getElementRect(resultW).height + 2) + "px";	
		
		if(!this._enableBounds) this._enableBounds = true;
	},
	_handleClearResultsLinkClick: function() {
		this._resultContainerWrapper.style.display = "none";
		this._resultContainer.innerHTML = "";
		var input = this._input;
		input.value = "";
		input.focus();
		this._cmdBarWrapper.style.display = "none";
		return false;
	},
	getResults: function() { return this._relults ? this._relults : (this._relults = []); },
    _getMessage: function(id) {
        var id = "SearchPanel$" + id;
	    return "BITRIX_GOOGLE_MAPS_MSG" in window && id in window.BITRIX_GOOGLE_MAPS_MSG ? window.BITRIX_GOOGLE_MAPS_MSG[id] : id;
    },
	_getSetting: function(name, defaultVal) {
		return typeof(Bitrix.GoogleMapsSettings) == "object" && name in Bitrix.GoogleMapsSettings ? Bitrix.GoogleMapsSettings[name] : defaultVal;
	}	
}
Bitrix.GoogleMapsSearchPanel.styles = {
	container: "bx-gmaps-search-panel-container",
	resultContainerWrapper: "bx-gmaps-search-panel-result-container-wrapper",
	resultContainer: "bx-gmaps-search-panel-result-container",
	cmdBarWrapper: "bx-gmaps-search-panel-cmd-bar-wrapper",
	cmdBar: "bx-gmaps-search-panel-cmd-bar",
	placeCell: "bx-gmaps-search-panel-input-tab-place", 
	buttonCell: "bx-gmaps-search-panel-input-tab-button",
	buttonContainer: "bx-gmaps-search-panel-input-tab-button-container"
}
Bitrix.GoogleMapsSearchResult = function Bitrix$GoogleMapsSearchResult(options) {
	this._markerChangeHandler = Bitrix.TypeUtility.createDelegate(this, this._handleMarkerChange);
	this._markerRemoveHandler = Bitrix.TypeUtility.createDelegate(this, this._handleMarkerRemove);
	
	this._result = options.result;
	if(!this._result) throw "Could not find result!";
	
	this._position = Bitrix.GoogleMapsCoordinate.fromGoogleMapsLatLng(this._result.geometry.location);
	this._descr = this._result.formatted_address;
	this._parentEl = options.parentEl;
	if(!this._parentEl) throw "Could not find parent element!";
	this._map = options.map;
	this._marker = null;
	this._infoWin = null;

	var r = this._container =  this._parentEl.insertRow(-1);
	var markerC = r.insertCell(-1);
	markerC.className = "bx-gmaps-search-panel-result-marker";
	
	var markerImg = document.createElement("IMG");
	markerC.appendChild(markerImg);
	markerImg.title = markerImg.alt = this._getMessage("AddMarkerToolTip");
	markerImg.src = this._getSetting("standardMarkerImageUrl", "http://maps.gstatic.com/intl/en_ALL/mapfiles/ms/micons/blue-dot.png");	
	markerImg.onclick = Bitrix.TypeUtility.createDelegate(this, this._handleCreateMarkerClick);
	
	var contentC = r.insertCell(-1);
	contentC.className = "bx-gmaps-search-panel-result-content";
	
	var goToLink = document.createElement("A");
	contentC.appendChild(goToLink);
	goToLink.href = "";
	goToLink.title = this._getMessage("GoToPlaceToolTip");
	goToLink.onclick = Bitrix.TypeUtility.createDelegate(this, this._handleLinkClick);
	goToLink.innerHTML = this.getDescription();	
}

Bitrix.GoogleMapsSearchResult.prototype = {
	_getSetting: function(name, defaultVal) {
		return typeof(Bitrix.GoogleMapsSettings) == "object" && name in Bitrix.GoogleMapsSettings ? Bitrix.GoogleMapsSettings[name] : defaultVal;
	},
	_getMessage: function(id) {
        var id = "SearchResult$" + id;
	    return "BITRIX_GOOGLE_MAPS_MSG" in window && id in window.BITRIX_GOOGLE_MAPS_MSG ? window.BITRIX_GOOGLE_MAPS_MSG[id] : id;
    },
	_handleCreateMarkerClick: function() {
		this._map.setCenterByGoogleLatLng(this._result.geometry.location);
		this.createMarker();
	},
	_handleLinkClick: function() {
		this._map.setCenterByGoogleLatLng(this._result.geometry.location);
		var win = this._getInfoWin();
		win.open();
		return false;
	},
	_getInfoWin: function() { return this._infoWin ? this._infoWin : (this._infoWin = new Bitrix.GoogleMapsSearchResultInfoWindow({ searchResult: this })); },
	//getHtml: function() { return this._container; },
	getDescription: function() { return this._descr; },
	getMap: function() { return this._map; },
	getPosition: function() { return this._position; },
	getMarker: function() { return this._marker; },
	createMarker: function() {
		if(this._marker) return;
		this._marker = Bitrix.GoogleMapsMarker.create({ position: this._position, title: this._descr });
		this._map.addMarker(this._marker);	
		this._marker.addChangeListener(this._markerChangeHandler);
		this._map.addMarkerChangeListener(this._markerRemoveHandler);
	},
	_handleMarkerChange: function() {
		if(Bitrix.GoogleMapsCoordinate.equals(this._position, this._marker.getPosition())) return;
		this._marker.removeChangeListener(this._markerChangeHandler);
		this._map.removeMarkerChangeListener(this._markerRemoveHandler);
		this._marker = null;
	},
	_handleMarkerRemove: function(sender, args) {
		if(!this._marker || args.marker != this._marker || args.action != Bitrix.GoogleMapsAction.remove) return;
		this._marker.removeChangeListener(this._markerChangeHandler);
		this._map.removeMarkerChangeListener(this._markerRemoveHandler);
		this._marker = null;		
	}
}
Bitrix.GoogleMapsSearchResult.styles = {
	wrapper: "bx-gmaps-search-panel-result-item-wrapper",
	container: "bx-gmaps-search-panel-result-item-container"
}
Bitrix.GoogleMapsInfoWindowButton = { bSave: 1, bDelete: 2, bCancel: 3 }
Bitrix.GoogleMapsMarkerInfoWindow = function Bitrix$GoogleMapsMarkerInfoWindow(options) {
	this._options = options;
	this._infoWin = null;
	this._contentDiv = null;
	this._titleEl = null;
	this._closeEvent = null;
}
Bitrix.GoogleMapsMarkerInfoWindow.prototype = {
	open: function() {
		var map = this.getMap();
		if(!map) throw "GoogleMapsMarkerInfoWindow: could not find map!";
		var marker = this.getMarker();
		if(!marker) throw "GoogleMapsMarkerInfoWindow: could not find marker!";
		var content = this._prepareContent();
		this.reset();
		var win = this._prepareInfoWin();
		win.setContent(content);
		win.open(map, marker);
		
	},
	close: function(buttonId) {
		if(!buttonId) buttonId = Bitrix.GoogleMapsInfoWindowButton.bCancel;
		if(!this._infoWin) return;
		this._infoWin.close();
		
        if (!this._closeEvent) return;
        try { this._closeEvent.fire(this, { buttonId: buttonId }); } catch (e) { }		
	},
    addCloseListener: function(listener) {
        (this._closeEvent ? this._closeEvent : (this._closeEvent = new Bitrix.EventPublisher())).addListener(listener);
    },
    removeCloseListener: function(listener) {
        if (this._closeEvent) this._closeEvent.removeListener(listener);
    },
	getOptions: function() { return this._options ? this._options : (this._options = new Object()); },
	getMap: function() { var o = this.getOptions(); return "map" in o ? o.map : null; },
	getMarker: function() { var o = this.getOptions(); return "marker" in o ? o.marker : null; },		
	_prepareInfoWin: function() {
		if(this._infoWin) return this._infoWin;
		this._infoWin = new google.maps.InfoWindow();	
		google.maps.event.addListener(this._infoWin, 'domready', Bitrix.TypeUtility.createDelegate(this, function(){ window.setTimeout(Bitrix.TypeUtility.createDelegate(this, this._setupInfoWin), 100); }));
		google.maps.event.addListener(this._infoWin, 'closeclick', Bitrix.TypeUtility.createDelegate(this, function(){ window.setTimeout(Bitrix.TypeUtility.createDelegate(this, function(){ this.close(Bitrix.GoogleMapsInfoWindowButton.bCancel); }), 100); }));
		return this._infoWin;
	},
	_prepareContent: function() {
		var marker = this.getMarker();
		if(!marker) throw "GoogleMapsMarkerInfoWindow: marker is not found!";
		if(this._contentDiv) return this._contentDiv; 
		var c = this._contentDiv = document.createElement("DIV");
		var tab = document.createElement("TABLE");
		c.appendChild(tab);
		var ttlR = tab.insertRow(-1);
		var ttlLblC = ttlR.insertCell(-1);
		var ttlLbl = document.createElement("SPAN");
		ttlLblC.appendChild(ttlLbl);
		ttlLbl.innerHTML = this._getMessage("Name") + ":";
		var ttlCntC = ttlR.insertCell(-1);
		var ttl = this._titleEl = document.createElement("INPUT");
		ttl.type = "text";
		ttlCntC.appendChild(ttl);
		ttl.style.width = "200px";
		
		var hintR = tab.insertRow(-1);
		var hintC = hintR.insertCell(-1);
		hintC.colSpan = 2;	
		var hint = document.createElement("DIV");
		hintC.appendChild(hint);
		hint.style.width = "250px";
		hint.innerHTML = this._getMessage("NameHint");
		
		
		var btnR = tab.insertRow(-1);
		var btnC = btnR.insertCell(-1);
		btnC.colSpan = 2;
		var saveBtn = document.createElement("INPUT");
		saveBtn.type = "button";
		saveBtn.value = this._getMessage("Save");
		Bitrix.EventUtility.addEventListener(saveBtn, "click", Bitrix.TypeUtility.createDelegate(this, this._handleSaveBtnClick), false);
		btnC.appendChild(saveBtn);
		var delBtn = document.createElement("INPUT");
		delBtn.type = "button";
		delBtn.value = this._getMessage("Delete");
		Bitrix.EventUtility.addEventListener(delBtn, "click", Bitrix.TypeUtility.createDelegate(this, this._handleDeleteBtnClick), false);
		btnC.appendChild(delBtn);
		var cancBtn = document.createElement("INPUT");
		cancBtn.type = "button";
		cancBtn.value = this._getMessage("Cancel");
		Bitrix.EventUtility.addEventListener(cancBtn, "click", Bitrix.TypeUtility.createDelegate(this, this._handleCancelBtnClick), false);
		btnC.appendChild(cancBtn);
		return this._contentDiv;
	},
	_setupInfoWin: function() {
		var marker = this.getMarker();
		if(!marker) throw "GoogleMapsMarkerInfoWindow: marker is not found!";
		
		var ttl = this._titleEl;
		if(!ttl) throw "GoogleMapsMarkerInfoWindow: could not find ttile element!";	
		ttl.value = marker.getTitle();		
		ttl.select();
		ttl.focus();
	},
	reset: function() {
		ttl = this._titleEl;
		if(ttl) ttl.value = "";
	},
	_handleSaveBtnClick: function(e) {
		Bitrix.EventUtility.stopEventPropagation(e);
		var marker = this.getMarker();
		if(!marker) throw "GoogleMapsMarkerInfoWindow: marker is not found!";
		ttl = this._titleEl;
		if(!ttl) throw "GoogleMapsMarkerInfoWindow: marker is not defined!";
		marker.setTitle(ttl.value);
		this.close(Bitrix.GoogleMapsInfoWindowButton.bSave);
	},
	_handleDeleteBtnClick: function(e) {
		Bitrix.EventUtility.stopEventPropagation(e);
		this.close(Bitrix.GoogleMapsInfoWindowButton.bDelete);
	},
	_handleCancelBtnClick: function(e) {
		Bitrix.EventUtility.stopEventPropagation(e);
		this.close(Bitrix.GoogleMapsInfoWindowButton.bCancel);
	},
    _getMessage: function(id) {
        var id = "InfoWindow$" + id;
	    return "BITRIX_GOOGLE_MAPS_MSG" in window && id in window.BITRIX_GOOGLE_MAPS_MSG ? window.BITRIX_GOOGLE_MAPS_MSG[id] : id;
    }			
}

Bitrix.GoogleMapsSearchResultInfoWindow = function Bitrix$GoogleMapsSearchResultInfoWindow(options) {
	this._options = options;
	if(!this.getSearchResult()) throw "GoogleMapsSearchResultInfoWindow: Could not find search result";
	this._infoWin = null;
	this._contentDiv = null;
	this._titleEl = null;
	this._closeEvent = null;
}
Bitrix.GoogleMapsSearchResultInfoWindow.prototype = {
	open: function() {
		var win = this._prepareInfoWin();
		win.setContent(this._prepareContent());
		win.setPosition(this.getSearchResult().getPosition().toGoogleMapsLatLng());
		win.open(this.getSearchResult().getMap().getSourceMap());	
	},
	close: function(buttonId) {
		if(!buttonId) buttonId = Bitrix.GoogleMapsInfoWindowButton.bCancel;
		if(!this._infoWin) return;
		this._infoWin.close();
		
        if (!this._closeEvent) return;
        try { this._closeEvent.fire(this, { buttonId: buttonId }); } catch (e) { }		
	},
    addCloseListener: function(listener) {
        (this._closeEvent ? this._closeEvent : (this._closeEvent = new Bitrix.EventPublisher())).addListener(listener);
    },
    removeCloseListener: function(listener) {
        if (this._closeEvent) this._closeEvent.removeListener(listener);
    },
	getOptions: function() { return this._options ? this._options : (this._options = new Object()); },
	getSearchResult: function() { var o = this.getOptions(); return "searchResult" in o ? o.searchResult : null; },
	_prepareInfoWin: function() {
		if(this._infoWin) return this._infoWin;
		this._infoWin = new google.maps.InfoWindow();	
		google.maps.event.addListener(this._infoWin, 'closeclick', Bitrix.TypeUtility.createDelegate(this, function(){ window.setTimeout(Bitrix.TypeUtility.createDelegate(this, function(){ this.close(Bitrix.GoogleMapsInfoWindowButton.bCancel); }), 100); }));
		return this._infoWin;
	},
	_prepareContent: function() {
		if(!this._contentDiv) {
			var c = this._contentDiv = document.createElement("DIV");
			var tab = document.createElement("TABLE");
			c.appendChild(tab);
			var infoR = tab.insertRow(-1);
			var infoC = infoR.insertCell(-1);
			
			var r = this.getSearchResult()._result;
			var name = "", info = "", country = "";		
			for(var i = 0; i < r.address_components.length; i++) {
				var c = r.address_components[i];
				if(i == 0) name = c.long_name;
				else {
					var isCountry = false;
					if(country.length == 0)
						for(var j = 0; j < c.types.length; j++) {
							if(c.types[j] != "country") continue;
							country = c.long_name;
							isCountry = true;
							break;
						}
					if(isCountry) continue;
					if(info.length > 0) info += ", ";
					info += c.long_name;
				}
			}
			infoC.innerHTML += "<p><b>" + name + "</b></p>" + "<p>" + info + "</p>";
			if(country.length > 0)
				infoC.innerHTML += "<p>" + country + "</p>";
			
			var btnR = tab.insertRow(-1);
			var btnC = btnR.insertCell(-1);
			btnC.colSpan = 2;
			var createMarkerBtn = this._createMarkerBtn =  document.createElement("INPUT");
			createMarkerBtn.type = "button"; //prior appendChild for IE
			btnC.appendChild(createMarkerBtn);
			createMarkerBtn.value = this._getMessage("CreateMarker");
			Bitrix.EventUtility.addEventListener(createMarkerBtn, "click", Bitrix.TypeUtility.createDelegate(this, this._handlecreateMarkerBtnClick), false);	
		}
		this._createMarkerBtn.disabled = this.getSearchResult().getMarker() != null;
		return this._contentDiv;
	},
	_handlecreateMarkerBtnClick: function(e) {
		Bitrix.EventUtility.stopEventPropagation(e);
		this.getSearchResult().createMarker();
		this._createMarkerBtn.disabled = true;
		this.close(Bitrix.GoogleMapsInfoWindowButton.bSave);
	},
    _getMessage: function(id) {
        var id = "SearchResultInfoWindow$" + id;
	    return "BITRIX_GOOGLE_MAPS_MSG" in window && id in window.BITRIX_GOOGLE_MAPS_MSG ? window.BITRIX_GOOGLE_MAPS_MSG[id] : id;
    }	
}

Bitrix.GoogleMapsManager = new Object();
Bitrix.GoogleMapsManager._entities = null;
Bitrix.GoogleMapsManager._getEntities = function() { return this._entities ? this._entities : (this._entities = new Object()); }
Bitrix.GoogleMapsManager.getEntity = function(id) { return this._entities != null && id in this._entities ? this._entities[id] : null; }
Bitrix.GoogleMapsManager.createEntity = function(data, parentNode) {
	if(!data) throw "Bitrix.GoogleMapsManager: data is not defined!";
	var id = data.getEntityId();
	if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Could not find entity id";
	var entities = this._getEntities();
	if(id in entities) throw "Bitrix.GoogleMapsManager: map '" + id + "' already exits!";
    var r = Bitrix.GoogleMapsEntity.create(data, parentNode);
    entities[id] = r;
    return r;
}