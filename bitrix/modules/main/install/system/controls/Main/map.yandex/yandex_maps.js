if (typeof (Bitrix) == "undefined") {
    var Bitrix = {};
}
Bitrix.MapCoord = function Bitrix$MapCoord() {
    this._initialized = false;
    this._latitude = 55.75;
    this._longitude = 37.6;
}
Bitrix.MapCoord.prototype = {
    initialize: function (lat, lng) {
        if (this._initialized) return;

        this._latitude = Bitrix.TypeUtility.tryConvertToFloat(lat, 55.75);
        this._longitude = Bitrix.TypeUtility.tryConvertToFloat(lng, 37.6);
        this._initialized = true;
    },
    getLatitude: function () { return this._latitude; },
    setLatitude: function (lat) { this._latitude = lat; },
    getLongitude: function () { return this._longitude },
    setLongitude: function (lng) { this._longitude = lng; },
    toString: function (format) {
        if (!Bitrix.TypeUtility.isNotEmptyString(format)) format = "";
        return format.toUpperCase() == "S" ? this._latitude.toFixed(5) + ", " + this._longitude.toFixed(5) : "{ lat: " + this._latitude + ", lng: " + this._longitude + " }";
    },
    toObject: function () { return { lat: this._latitude, lng: this._longitude }; },
    toYMapsGeoPoint: function () { return new YMaps.GeoPoint(this._longitude, this._latitude); }
}
Bitrix.MapCoord.fromYMapsGeoPoint = function (geoPoint, defaultValue) {
    return geoPoint ? this.create(geoPoint.getLat(), geoPoint.getLng()) : defaultValue;
}
Bitrix.MapCoord.fromObject = function (obj, defaultVal) {
    if (obj instanceof Bitrix.MapCoord) return obj;
    if (obj && typeof (obj) == "object" && "lat" in obj && "lng" in obj) return this.create(obj["lat"], obj["lng"]);
    return defaultVal;
}
Bitrix.MapCoord.create = function (lat, lng) {
    var self = new Bitrix.MapCoord();
    self.initialize(lat, lng);
    return self;
}
Bitrix.MapCoord.equals = function (first, second) {
    return first._latitude == second._latitude && first._longitude == second._longitude;
}
Bitrix.MapCoord.getMSK = function () {
    return new Bitrix.MapCoord.create(55.75, 37.6);
}

Bitrix.YandexMapType = {
    map: 1,
    satellite: 2,
    hybrid: 3
}
Bitrix.YandexMapType.fromString = function (str) {
    return Bitrix.EnumHelper.fromString(str, this);
}
Bitrix.YandexMapType.toString = function (src) {
    return Bitrix.EnumHelper.toString(src, this);
}
Bitrix.YandexMapType.tryParse = function (src) {
    return Bitrix.EnumHelper.tryParse(src, this);
}
Bitrix.YandexMapType.fromYandexMapType = function (src) {
    switch (src) {
        case YMaps.MapType.HYBRID: return Bitrix.YandexMapType.hybrid;
        case YMaps.MapType.SATELLITE: return Bitrix.YandexMapType.satellite;
        case YMaps.MapType.MAP:
        default:
            return Bitrix.YandexMapType.map;
    }
}
Bitrix.YandexMapType.toYandexMapType = function (src) {
    switch (src) {
        case Bitrix.YandexMapType.hybrid: return YMaps.MapType.HYBRID;
        case Bitrix.YandexMapType.satellite: return YMaps.MapType.SATELLITE;
        case Bitrix.YandexMapType.map:
        default:
            return YMaps.MapType.MAP;
    }
}

Bitrix.YandexMapControlType = {
    toolBar: 1,
    zoomStandard: 2,
    zoomCompact: 3,
    miniMap: 4,
    typeControl: 5,
    scaleLine: 6,
    search: 7
}
Bitrix.YandexMapControlType.fromString = function (str) {
    return Bitrix.EnumHelper.fromString(str, this);
}
Bitrix.YandexMapControlType.toString = function (src) {
    return Bitrix.EnumHelper.toString(src, this);
}

Bitrix.YandexMapOption = {
    disableDragging: 1,
    disableDoubleClickZoom: 2,
    enableScrollZoom: 3,
    enableHotKeys: 4,
    enableMagnifier: 5,
    enableRightButtonMagnifier: 6,
    enableRuler: 7
}
Bitrix.YandexMapOption.fromString = function (str) {
    return Bitrix.EnumHelper.fromString(str, this);
}
Bitrix.YandexMapOption.toString = function (src) {
    return Bitrix.EnumHelper.toString(src, this);
}

Bitrix.YandexMapMarker = function Bitrix$YandexMapMarker() {
    this._initialized = false;
    this._map = null;
    this._pos = null;
    this._ttl = "";
    this._created = new Date();
    this._draggable = false;
    this._clickable = false;
    this._sourceMarker = null;
    this._markerChangeHandler = null;
    this._markerDragEndHandler = null;
    this._markerClickHandler = null;
    this._changeListener = null;
    this._dragEndListener = null;
    this._clickListener = null;
    this._suppressMarkerChange = false;
}
Bitrix.YandexMapMarker.prototype = {
    initialize: function (pos, ttl) {
        this.setPosition(pos ? pos : Bitrix.MapCoord.getMSK());
        this.setTitle(ttl);
        this._initialized = true;
    },
    getPosition: function () { return this._pos; },
    setPosition: function (pos) {
        if (!pos) throw "YandexMapMarker: pos is not defined!";
        this._pos = pos;
        if (!this._sourceMarker) return;
        this._suppressMarkerChange = true;
        this._sourceMarker.setGeoPoint(pos.toYMapsGeoPoint());
        this._suppressMarkerChange = false;
    },
    getTitle: function () { return this._ttl; },
    setTitle: function (ttl) {
        this._ttl = Bitrix.TypeUtility.isNotEmptyString(ttl) ? ttl : "";
        if (this._sourceMarker) this._sourceMarker.name = this._ttl;
        this._handleMarkerChange();
    },
    getDefaultTitle: function () { return this._pos.toString("S"); },
    getDraggable: function () { return this._sourceMarker ? this._sourceMarker.getOptions().draggable : this._draggable; },
    setDraggable: function (draggable) { this._draggable = draggable; this._setSourceMarkerOption("draggable", draggable); },
    _setSourceMarkerOption: function (name, val) {
        var s = this._sourceMarker;
        if (!s) return;
        var o = s.getOptions();
        if (name in o) {
            o[name] = val;
            s.setOptions(o);
            this._handleMarkerChange();
        }
    },
    created: function () { this._created(); },
    getIcon: function () { return this._icon; },
    setIcon: function (icon) {
        this._icon = icon;
        if (!(this._sourceMarker && this._icon && "url" in icon && Bitrix.TypeUtility.isNotEmptyString(icon.url))) return;
        var s = new YMaps.Style();
        s.iconStyle = new YMaps.IconStyle();
        s.iconStyle.href = icon.url;
        if ("width" in icon && "height" in icon)
            s.iconStyle.size = new YMaps.Point(icon.width, icon.height);
        this._sourceMarker.setStyle(s);
        this._handleMarkerChange();
    },
    addChangeListener: function (listener) {
        (this._changeListener ? this._changeListener : (this._changeListener = new Bitrix.EventPublisher())).addListener(listener);
    },
    removeChangeListener: function (listener) {
        if (this._changeListener) this._changeListener.removeListener(listener);
    },
    addDragEndListener: function (listener) {
        (this._dragEndListener ? this._dragEndListener : (this._dragEndListener = new Bitrix.EventPublisher())).addListener(listener);
    },
    removeDragEndListener: function (listener) {
        if (this._dragEndListener) this._dragEndListener.removeListener(listener);
    },
    addClickListener: function (listener) {
        (this._clickListener ? this._clickListener : (this._clickListener = new Bitrix.EventPublisher())).addListener(listener);
    },
    removeClickListener: function (listener) {
        if (this._clickListener) this._clickListener.removeListener(listener);
    },
    getSourceMarker: function () { return this._sourceMarker; },
    setupByYMapsPlacemark: function (srcMarker) {
        if (!srcMarker) throw "YandexMapMarker: srcMarker is not defined!";
        if (this._sourceMarker) {
            if (this._markerChangeHandler) this._markerChangeHandler.cleanup();
            if (this._markerDragEndHandler) this._markerDragEndHandler.cleanup();
            if (this._markerClickHandler) this._markerClickHandler.cleanup();
            this._sourceMarker = null;
        }
        this._pos = Bitrix.MapCoord.fromYMapsGeoPoint(srcMarker.getGeoPoint());
        this._ttl = srcMarker.name;
        this._sourceMarker = srcMarker;

        this._markerChangeHandler = YMaps.Events.observe(this._sourceMarker, this._sourceMarker.Events.PositionChange, this._handleMarkerChange, this);
        this._markerDragEndHandler = YMaps.Events.observe(this._sourceMarker, this._sourceMarker.Events.DragEnd, this._handleMarkerDragEnd, this);
        this._markerClickHandler = YMaps.Events.observe(this._sourceMarker, this._sourceMarker.Events.Click, this._handleMarkerClick, this);
    },
    getMap: function () { return this._map; },
    setMap: function (map) {
        if (this._sourceMarker) {
            if (this._map && this._map.getSourceMap()) this._map.getSourceMap().removeOverlay(this._sourceMarker);
            if (map && map.getSourceMap()) map.getSourceMap().addOverlay(this._sourceMarker);
        }
        this._map = map;
    },
    _handleMarkerChange: function () {
        if (this._suppressMarkerChange) return;
        if (!this._sourceMarker) return;
        this._pos = Bitrix.MapCoord.fromYMapsGeoPoint(this._sourceMarker.getGeoPoint());
        if (this._changeListener) this._changeListener.fire(this);
    },
    _handleMarkerDragEnd: function () { if (this._dragEndListener) this._dragEndListener.fire(this); },
    _handleMarkerClick: function () { if (this._clickListener) this._clickListener.fire(this); },
    release: function () {
        this.setMap(null);
        if (this._markerChangeHandler) this._markerChangeHandler.cleanup();
        if (this._markerDragEndHandler) this._markerDragEndHandler.cleanup();
        if (this._markerClickHandler) this._markerClickHandler.cleanup();
    },
    toObject: function () { return { title: this.getTitle(), position: this.getPosition().toObject() }; },
    toString: function () {
        var ttl = this.getTitle().replace(/[\/\\\"\']/g, function (str) { return '\\' + str; });
        return "{ title: \"" + ttl + "\", position: " + this.getPosition().toString() + " }";
    }
}
Bitrix.YandexMapMarker.create = function (options) {
    var pos = Bitrix.TypeUtility.tryGetObjProp(options, "position", null);
    if (!pos) pos = Bitrix.MapCoord.getMSK();
    var ttl = Bitrix.TypeUtility.tryGetObjProp(options, "title", "");

    var self = new Bitrix.YandexMapMarker();
    self.initialize(pos, ttl);
    if (typeof (YMaps) != "undefined" && typeof (YMaps.Placemark) != "undefined") {
        var srcMarker = new YMaps.Placemark(pos.toYMapsGeoPoint(), { 'hasBalloon': false, 'hasHint': true, 'draggable': Bitrix.TypeUtility.tryGetObjProp(options, "draggable", false) });
        srcMarker.name = srcMarker.description = ttl;
        srcMarker.setHintContent(ttl);
        self.setupByYMapsPlacemark(srcMarker);
    }
    var icon = Bitrix.TypeUtility.tryGetObjProp(options, "icon", null);
    if (icon) self.setIcon(icon);
    return self;
}
Bitrix.YandexMapMarker.fromYandexMapPlaceMark = function (srcMarker) {
    var self = new Bitrix.YandexMapMarker();
    self.initialize(null, "");
    self.setupByYMapsPlacemark(srcMarker);
    return self;
}
Bitrix.YandexMapMarker.fromObject = function (obj) {
    if (obj instanceof Bitrix.YandexMapMarker) return obj;
    if (!obj) throw "YandexMapMarker: obj is not defined!";
    return this.create({ position: "position" in obj ? Bitrix.MapCoord.fromObject(obj.position) : null, title: "title" in obj ? obj.title : "" });
}

Bitrix.YandexMapData = function Bitrix$YandexMapData() {
    this._initialized = false;
    this._parameters = null;
}
Bitrix.YandexMapData.prototype = {
    initialize: function (parameters) {
        if (this._initialized) return;
        if (typeof (parameters) != "object") throw "'parameters' must be Object!";
        this._parameters = parameters;
        this._initialized = true;
    },
    getParameter: function (key, defaultValue, ignoreUndefined) {
        if (!(this._parameters && key in this._parameters)) return defaultValue;
        var r = this._parameters[key];
        return ignoreUndefined === true && !r ? defaultValue : r;
    },
    setParameter: function (key, value) { (this._parameters ? this._parameters : (this._parameters = {}))[key] = value; },
    getEntityId: function () { return this.getParameter("entityId", "", true); },
    setEntityId: function (id) { this.setParameter("entityId", id); },
    getCenter: function () {
        var c = this.getParameter("center", null);
        return c ? c : Bitrix.MapCoord.getMSK();
    },
    setCenter: function (center) { this.setParameter("center", center); },
    getZoom: function () { return this.getParameter("zoom", 8); },
    setZoom: function (zoom) { this.setParameter("zoom", zoom); },
    getMapType: function () { return this.getParameter("mapType", Bitrix.YandexMapType.map, true); },
    setMapType: function (type) { this.setParameter("mapType", type); },
    getMarkers: function () {
        var r = this.getParameter("markers", null);
        if (!r) this.setParameter("markers", (r = []));
        return r;
    },
    setMarkers: function (markers) { this.setParameter("markers", markers); },
    getMapControlTypes: function () { return Bitrix.YandexMapControlType.fromString(this.getParameter("mapControlTypes", null)); },
    setMapControlTypes: function (types) { this.setParameter("mapControlTypes", Bitrix.YandexMapControlType.toString(types)); },
    getMapOptions: function () { return Bitrix.YandexMapOption.fromString(this.getParameter("mapOptions", null)); },
    setMapOptions: function (options) { this.setParameter("mapOptions", Bitrix.YandexMapOption.toString(options)); },
    getMarkupPanel: function () { return this.getParameter("markupPanel", false); },
    setMarkupPanel: function (markupPanel) { this.setParameter("markupPanel", markupPanel); },
    getSearchPanel: function () { return this.getParameter("searchPanel", false); },
    setSearchPanel: function (searchPanel) { this.setParameter("searchPanel", searchPanel); },
    toObject: function () {
        var r = {};
        r.mapType = Bitrix.YandexMapType.toString(this.getMapType());
        r.center = this.getCenter().toObject();
        r.zoom = this.getZoom();
        r.markers = [];
        var markers = this.getMarkers();
        for (var i = 0; i < markers.length; i++)
            r.markers.push(markers[i].toObject());
        return r;
    },
    toString: function () {
        var r = "";
        r += "mapType: \"" + Bitrix.YandexMapType.toString(this.getMapType()) + "\"";
        r += ", center: " + this.getCenter().toString();
        r += ", zoom: " + this.getZoom();
        r += ", markers: [ ";
        var markers = this.getMarkers();
        for (var i = 0; i < markers.length; i++) {
            if (i != 0) r += ", ";
            r += markers[i].toString();
        }
        r += " ]";
        return "{ " + r + " }";
    }
}
Bitrix.YandexMapData.create = function (parameters) {
    var self = new Bitrix.YandexMapData();
    self.initialize(parameters);
    return self;
}
Bitrix.YandexMapData.fromObject = function (obj) {
    if (obj instanceof Bitrix.YandexMapData) return obj;
    var self = new Bitrix.YandexMapData();
    var parameters = {};
    var initState = obj && typeof (obj) == "object" && "initialState" in obj ? obj.initialState : null;
    if (initState && typeof (initState) == "object") {
        if ("mapType" in initState) {
            var t = Bitrix.YandexMapType.tryParse(initState.mapType);
            parameters.mapType = t.length > 0 ? t[0] : Bitrix.YandexMapType.map;
        }
        if ("center" in initState) {
            var c = Bitrix.MapCoord.fromObject(initState.center, null);
            parameters.center = c ? c : Bitrix.MapCoord.getMSK();
        }
        if ("zoom" in initState)
            parameters.zoom = Bitrix.TypeUtility.isNumber(initState.zoom) ? initState.zoom : 12;
        if ("markers" in initState && initState.markers instanceof Array) {
            var markerData = initState.markers;
            var markers = parameters.markers = [];
            for (var i = 0; i < markerData.length; i++)
                markers.push(Bitrix.YandexMapMarker.fromObject(markerData[i]));
        }
    }
    self.initialize(parameters);
    if (obj && typeof (obj) == "object") {
        if ("id" in obj) self.setEntityId(obj.id);
        if ("mapControlTypes" in obj) self.setMapControlTypes(obj.mapControlTypes);
        if ("mapOptions" in obj) self.setMapOptions(obj.mapOptions);
        if ("markupPanel" in obj) self.setMarkupPanel(obj.markupPanel);
        if ("searchPanel" in obj) self.setSearchPanel(obj.searchPanel);
    }
    return self;
}

Bitrix.YandexMapEntity = function Bitrix$YandexMapEntity() {
    this._initialized = this._constructed = this._isDesignMode = false;
    this._data = this._parentNode = this._map = this._markerPane = this._searchPane = this._zoomHandler = this._dragEndHandler = this._mapTypeChangeHandler = this._centerChangeListener = this._mapTypeChangeListener = this._zoomChangeListener = this._markerChangeListener = this._markerClickListener = null;
}
Bitrix.YandexMapEntity.prototype = {
    initialize: function (data, parentNode) {
        if (this._initialized) return;
        this.setData(data);
        this.setParentNode(parentNode);
        this._initialized = true;
    },
    construct: function () {
        if (this._constructed) return;
        var pNode = this.getParentNode();
        if (!pNode) throw "YandexMapEntity: parentNode is not assigned!";
        var d = this.getData();

        var map = this._map = new YMaps.Map(pNode);
        map.setCenter(d.getCenter().toYMapsGeoPoint(), d.getZoom(), Bitrix.YandexMapType.toYandexMapType(d.getMapType()));
        var opts = d.getMapOptions();
        if (Bitrix.ArrayHelper.findInArray(Bitrix.YandexMapOption.disableDragging, opts) >= 0) map.disableDragging();
        if (Bitrix.ArrayHelper.findInArray(Bitrix.YandexMapOption.disableDoubleClickZoom, opts) >= 0) map.disableDblClickZoom();
        if (Bitrix.ArrayHelper.findInArray(Bitrix.YandexMapOption.enableScrollZoom, opts) >= 0) map.enableScrollZoom();
        if (Bitrix.ArrayHelper.findInArray(Bitrix.YandexMapOption.enableHotKeys, opts) >= 0) map.enableHotKeys();
        var isMagnifierEnabled = false;
        if (Bitrix.ArrayHelper.findInArray(Bitrix.YandexMapOption.enableMagnifier, opts) >= 0) {
            map.enableMagnifier();
            isMagnifierEnabled = true;
        }
        if (isMagnifierEnabled && Bitrix.ArrayHelper.findInArray(Bitrix.YandexMapOption.enableRightButtonMagnifier, opts) >= 0) map.enableRightButtonMagnifier();
        if (!isMagnifierEnabled && Bitrix.ArrayHelper.findInArray(Bitrix.YandexMapOption.enableRuler, opts) >= 0) map.enableRuler();

        var controlTypes = d.getMapControlTypes();
        if (controlTypes.length > 0) {
            var isZoomAdded = false;
            for (var i = 0; i < controlTypes.length; i++) {
                switch (controlTypes[i]) {
                    case Bitrix.YandexMapControlType.toolBar:
                        map.addControl(new YMaps.ToolBar());
                        break;
                    case Bitrix.YandexMapControlType.zoomStandard: 
                        {
                            if (!isZoomAdded) {
                                map.addControl(new YMaps.Zoom());
                                isZoomAdded = true;
                            }
                        }
                        break;
                    case Bitrix.YandexMapControlType.zoomCompact: 
                        {
                            if (!isZoomAdded) {
                                map.addControl(new YMaps.SmallZoom());
                                isZoomAdded = true;
                            }
                        }
                        break;
                    case Bitrix.YandexMapControlType.miniMap:
                        map.addControl(new YMaps.MiniMap());
                        break;
                    case Bitrix.YandexMapControlType.typeControl:
                        map.addControl(new YMaps.TypeControl());
                        break;
                    case Bitrix.YandexMapControlType.scaleLine:
                        map.addControl(new YMaps.ScaleLine());
                        break;
                    case Bitrix.YandexMapControlType.search:
                        map.addControl(new YMaps.SearchControl());
                        break;
                }
            }
        }

        this._zoomHandler = YMaps.Events.observe(map, map.Events.Update, this._handleZoomChange, this);
        this._dragEndHandler = YMaps.Events.observe(map, map.Events.DragEnd, this._handleCenterChange, this);
        this._mapTypeChangeHandler = YMaps.Events.observe(map, map.Events.TypeChange, this._handleMapTypeChange, this);

        if (d.getMarkupPanel()) {
            var overlay = Bitrix.YandexMapMarkerLayer.create({ 'map': this, 'parentNode': pNode });
            map.addLayer(overlay);
            this._markerPane = new Bitrix.YandexMapMarkupPanel({ 'map': this, 'overlay': overlay });
        }

        if (d.getSearchPanel())
            this._searchPane = new Bitrix.YandexMapSearchPanel({ 'map': this });

        this._isDesignMode = d.getMarkupPanel() || d.getSearchPanel();
        var markers = d.getMarkers();
        for (var i = 0; i < markers.length; i++)
            this._setupMarker(markers[i]);

        this._constructed = true;
    },
    _getSetting: function (name) {
        return typeof (Bitrix.YandexMapSettings) == "object" && name in Bitrix.YandexMapSettings ? Bitrix.YandexMapSettings[name] : "";
    },
    getSourceMap: function () { return this._map; },
    getData: function () { return this._data ? this._data : (this._data = new Bitrix.YandexMapData()); },
    setData: function (data) { this._data = data; },
    getParentNode: function () { return this._parentNode; },
    setParentNode: function (parentNode) {
        if (!Bitrix.TypeUtility.isDomElement(parentNode)) throw "YandexMapEntity: parentNode is not valid!";
        this._parentNode = parentNode;
    },
    getMarkers: function () { return this.getData().getMarkers(); },
    addMarker: function (marker) {
        if (!marker) throw "marker is not defined";
        this.getMarkers().push(marker);
        this._setupMarker(marker);
        if (this._markerChangeListener) this._markerChangeListener.fire(this, { marker: marker, action: Bitrix.YandexMapAction.add });
    },
    _setupMarker: function (marker) {
        marker.setDraggable(this._isDesignMode);
        marker.setIcon(this._getSetting("standardMarkerImage"));
        marker.setMap(this);
        marker.addClickListener(Bitrix.TypeUtility.createDelegate(this, this._handleMarkerClick));
    },
    removeMarker: function (marker) {
        var markers = this.getMarkers();
        for (var i = 0; i < markers.length; i++) {
            if (markers[i] != marker) continue;
            if (this._markerChangeListener) this._markerChangeListener.fire(this, { marker: marker, action: Bitrix.YandexMapAction.remove });
            markers.splice(i, 1);
            marker.release();
            return;
        }
    },
    addMarkerChangeListener: function (listener) {
        if (this._markerChangeListener == null)
            this._markerChangeListener = new Bitrix.EventPublisher();
        this._markerChangeListener.addListener(listener);
    },
    removeMarkerChangeListener: function (listener) {
        if (this._markerChangeListener == null) return;
        this._markerChangeListener.removeListener(listener);
    },
    addMarkerClickListener: function (listener) {
        if (this._markerClickListener == null)
            this._markerClickListener = new Bitrix.EventPublisher();
        this._markerClickListener.addListener(listener);
    },
    removeMarkerClickListener: function (listener) {
        if (this._markerClickListener == null) return;
        this._markerClickListener.removeListener(listener);
    },
    _handleMarkerClick: function (sender) {
        if (this._markerClickListener) this._markerClickListener.fire(this, { marker: sender });
    },
    addMapTypeChangeListener: function (listener) {
        if (this._mapTypeChangeListener == null)
            this._mapTypeChangeListener = new Bitrix.EventPublisher();
        this._mapTypeChangeListener.addListener(listener);
    },
    removeMapTypeChangeListener: function (listener) {
        if (this._mapTypeChangeListener == null) return;
        this._mapTypeChangeListener.removeListener(listener);
    },
    _handleMapTypeChange: function () {
        if (!(this._data && this._map)) return;
        this._data.setMapType(Bitrix.YandexMapType.fromYandexMapType(this._map.getType()));
        if (this._mapTypeChangeListener) this._mapTypeChangeListener.fire(this);
    },
    addCenterChangeListener: function (listener) {
        if (this._centerChangeListener == null)
            this._centerChangeListener = new Bitrix.EventPublisher();
        this._centerChangeListener.addListener(listener);
    },
    removeCenterChangeListener: function (listener) {
        if (this._centerChangeListener == null) return;
        this._centerChangeListener.removeListener(listener);
    },
    _handleCenterChange: function () {
        if (!(this._data && this._map)) return;
        this._data.setCenter(Bitrix.MapCoord.fromYMapsGeoPoint(this._map.getCenter(), null));
        if (this._centerChangeListener) this._centerChangeListener.fire(this);
    },
    addZoomChangeListener: function (listener) {
        if (this._zoomChangeListener == null)
            this._zoomChangeListener = new Bitrix.EventPublisher();
        this._zoomChangeListener.addListener(listener);
    },
    removeZoomChangeListener: function (listener) {
        if (this._zoomChangeListener == null) return;
        this._zoomChangeListener.removeListener(listener);
    },
    _handleZoomChange: function () {
        if (!(this._data && this._map)) return;
        this._data.setZoom(this._map.getZoom());
        if (this._zoomChangeListener) this._zoomChangeListener.fire(this);
    },
    openMarkerEditor: function (marker) {
        var p = this._markerPane;
        if (p) p.openMarkerEditor(marker);
    },
    startMarkerAdd: function () {
        var p = this._markerPane;
        p.startMarkerAdd();
    },
    startDrag: function () {
        var p = this._markerPane;
        p.startDrag();
    },
    setMapType: function (mapType) { this._map.setType(Bitrix.YandexMapType.toYandexMapType(mapType)); },
    getCenter: function () { return Bitrix.MapCoord.fromYMapsGeoPoint(this._map.getCenter()); },
    setCenter: function (coord) { this._map.setCenter(coord.toYMapsGeoPoint()); },
    setCenterByYMapsGeoPoint: function (gPoint) { this._map.setCenter(gPoint); },
    setZoom: function (zoom) { this._map.setZoom(zoom); },
    openBalloon: function (point, content, options) {
        var m = this._map;
        if (!m) return;
        m.openBalloon(point.toYMapsGeoPoint(), content, options);
    },
    closeBalloon: function () { return this._map ? this._map.closeBalloon() : false; },
    getBalloon: function () { return this._map ? this._map.getBalloon() : null; }
}
Bitrix.YandexMapEntity.create = function (data, parentNode) {
    if (typeof (YMaps) == 'undefined') return null;
    var self = new Bitrix.YandexMapEntity();
    self.initialize(data, parentNode);
    return self;
}

Bitrix.YandexMapAction = {
    add: 1,
    modify: 2,
    remove: 3
}

Bitrix.YandexMapManager = {};
Bitrix.YandexMapManager._entities = null;
Bitrix.YandexMapManager._getEntities = function () { return this._entities ? this._entities : (this._entities = {}); }
Bitrix.YandexMapManager.getEntity = function (id) { return this._entities != null && id in this._entities ? this._entities[id] : null; }
Bitrix.YandexMapManager.createEntity = function (data, parentNode) {
    if (!data) throw "YandexMapManager: data is not defined!";
    var id = data.getEntityId();
    if (!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Could not find entity id";
    var entities = this._getEntities();
    if (id in entities) throw "YandexMapManager: map '" + id + "' already exits!";
    var r = Bitrix.YandexMapEntity.create(data, parentNode);
    entities[id] = r;
    return r;
}
Bitrix.YandexMapMarkerLayer = function Bitrix$YandexMapMarkerLayer() {
    this._map = null;
    this._parentNode = this._soaringDiv = null;
    this._mouseClickHandler = this._mouseMoveHandler = null;
    this._mousePos = this._soaringStartPos = null;
    this._dropCallback = null;
}
Bitrix.YandexMapMarkerLayer.prototype.initialize = function (options) {
    if (!options) throw "Options is not defined!";
    if (!("map" in options)) throw "Map is not found!";
    this._map = options.map;
}
Bitrix.YandexMapMarkerLayer.prototype.getCopyright = function (bounds, zoom) { return ''; }
Bitrix.YandexMapMarkerLayer.prototype.getZoomRange = function (bounds) { return { 'min': 1, 'max': 15 }; }
Bitrix.YandexMapMarkerLayer.prototype.onAddToMap = function (map, parentContainer) {
    this._parentNode = parentContainer;
}
Bitrix.YandexMapMarkerLayer.prototype.onRemoveFromMap = function () {
    this._parentNode = null;
    if (this._mouseClickHandler) this._mouseClickHandler.cleanup();
    if (this._mouseMoveHandler) this._mouseMoveHandler.cleanup();
}
Bitrix.YandexMapMarkerLayer.prototype.onMapUpdate = function () { }
Bitrix.YandexMapMarkerLayer.prototype.onMove = function (position, offset) { }
Bitrix.YandexMapMarkerLayer.prototype.onSmoothZoomEnd = function () { }
Bitrix.YandexMapMarkerLayer.prototype.onSmoothZoomStart = function () { }
Bitrix.YandexMapMarkerLayer.prototype.onSmoothZoomTick = function (params) { }

Bitrix.YandexMapMarkerLayer.prototype._handleMouseMove = function (map, e) {
    var domEvent = e.getEvent();
    if (!domEvent && "event" in window) domEvent = window.event;
    var mousePos = this._mousePos = this._toRelativePos({ y: domEvent.clientY, x: domEvent.clientX });

    var d = this._soaringDiv;
    if (!d) return;
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
Bitrix.YandexMapMarkerLayer.prototype._handleMouseClick = function (map, e) {
    if (!this._soaringDiv) return;
    var domEvent = e.getEvent();
    if (!domEvent && "event" in window) domEvent = window.event;
    Bitrix.EventUtility.stopEventPropagation(domEvent);
    if (this._dropCallback) this._dropCallback(Bitrix.MapCoord.fromYMapsGeoPoint(e.getCoordPoint()));
}
Bitrix.YandexMapMarkerLayer.prototype._toRelativePos = function (mousePos) {
    var r = Bitrix.ElementPositioningUtility.getElementRect(this._parentNode);
    return { y: mousePos.y + document.body.scrollTop + document.documentElement.scrollTop - r.top, x: mousePos.x + document.body.scrollLeft + document.documentElement.scrollLeft - r.left }
}
Bitrix.YandexMapMarkerLayer.prototype.startSoaring = function (imgUrl, mousePos, dropCallback) {
    if (!Bitrix.TypeUtility.isNotEmptyString(imgUrl)) throw "YandexMapMarkerLayer: imgUrl is not defined!";
    var c = this._parentNode;
    var d = this._soaringDiv = document.createElement("DIV");
    c.appendChild(d);
    d.style.position = "absolute";
    d.style.zIndex = 1009;
    d.style.left = d.style.top = "0px";
    d.style.cursor = "none";

    var img = document.createElement("IMG");
    d.appendChild(img);
    img.src = imgUrl;
    img.style.left = img.style.top = "0px";
    img.style.cursor = "none";

    if (!mousePos) mousePos = this._mousePos;
    if (mousePos) {
        var startPos = this._toRelativePos(mousePos);
        var r = Bitrix.ElementPositioningUtility.getElementRect(d);
        startPos.x -= r.width / 2;
        startPos.y -= r.height + 3;
        d.style.left = startPos.x.toFixed(0) + "px";
        d.style.top = startPos.y.toFixed(0) + "px";
        this._soaringStartPos = startPos;
    }
    else
        this._soaringStartPos = { x: 0, y: 0 };

    this._dropCallback = dropCallback;
    var m = this._map.getSourceMap();
    this._mouseMoveHandler = YMaps.Events.observe(m, m.Events.MouseMove, this._handleMouseMove, this);
    this._mouseClickHandler = YMaps.Events.observe(m, m.Events.Click, this._handleMouseClick, this);
}
Bitrix.YandexMapMarkerLayer.prototype.stopSoaring = function () {
    if (this._mouseClickHandler) this._mouseClickHandler.cleanup();
    if (this._mouseMoveHandler) this._mouseMoveHandler.cleanup();

    this._dropCallback = null;

    if (this._soaringStartPos) {
        delete this._soaringStartPos;
        this._soaringStartPos = null;
    }

    if (this._soaringDiv) {
        this._soaringDiv.parentNode.removeChild(this._soaringDiv);
        this._soaringDiv = null;
    }
}
Bitrix.YandexMapMarkerLayer.create = function (options) {
    var self = new Bitrix.YandexMapMarkerLayer();
    self.initialize(options);
    return self;
}

Bitrix.YandexMarkupPanelMode = {
    drag: 1,
    centerSetup: 2,
    markerAdd: 3
}

Bitrix.YandexMapMarkupPanel = function Bitrix$YandexMapMarkupPanel(options) {
    if (!options) throw "YandexMapMarkupPanel: options is not defined!";

    var map = this._map = "map" in options ? options.map : null;
    if (!map) throw "YandexMapMarkupPanel: map is not found!";

    var overlay = this._overlay = "overlay" in options ? options.overlay : null;
    if (!overlay) throw "YandexMapMarkupPanel: overlay is not found!";

    this._mode = Bitrix.YandexMarkupPanelMode.drag;
    map.getSourceMap().addControl(this);
}
Bitrix.YandexMapMarkupPanel.prototype = {
    onAddToMap: function (map, position) {
        var container = this._container = document.createElement("DIV");
        container.style.position = "absolute";
        container.style.zIndex = 1009;

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
        this._balloon = null;

        var map = this._map;
        this._setupCenterMarkerByPos(map.getCenter());
        map.addMarkerClickListener(this._markerClickHandler);
        var srcMap = map.getSourceMap();
        this._mapUpdateHandler = YMaps.Events.observe(srcMap, srcMap.Events.Update, this._handleMapCenterChange, this);
        this._mapMoveEndHandler = YMaps.Events.observe(srcMap, srcMap.Events.MoveEnd, this._handleMapCenterChange, this);

        Bitrix.EventUtility.addEventListener(dragContainer, "click", Bitrix.TypeUtility.createDelegate(this, this._handleDragMouseClick), false);
        Bitrix.EventUtility.addEventListener(centerContainer, "click", Bitrix.TypeUtility.createDelegate(this, this._handleCenterMouseClick), false);
        Bitrix.EventUtility.addEventListener(markerContainer, "click", Bitrix.TypeUtility.createDelegate(this, this._handleMarkerMouseClick), false);

        srcMap.getContainer().appendChild(container);
        this.layout();
        this.position = position || new YMaps.ControlPosition(YMaps.ControlPosition.TOP_LEFT, new YMaps.Size(5, 5));
        this.position.apply(container);
    },
    onRemoveFromMap: function () {
        if (this._mapUpdateHandler) {
            this._mapUpdateHandler.cleanup();
            this._mapUpdateHandler = null;
        }

        if (this._mapMoveEndHandler) {
            this._mapMoveEndHandler.cleanup();
            this._mapMoveEndHandler = null;
        }

        var m = this._map, c = this._container;
        if (!m || !c) return;
        map.getSourceMap().getContainer().removeChild(c);
        this._map = this._container = null;
    },
    getMode: function () { return this._mode; },
    _getSetting: function (name) {
        return typeof (Bitrix.YandexMapSettings) == "object" && name in Bitrix.YandexMapSettings ? Bitrix.YandexMapSettings[name] : "";
    },
    layout: function () {
        var o = this._getSetting(this._mode == Bitrix.YandexMarkupPanelMode.drag ? "dragPanelIconSelImage" : "dragPanelIconImage");
        if (o && "url" in o) this._dragImg.src = o.url;
        o = this._getSetting(this._mode == Bitrix.YandexMarkupPanelMode.centerSetup ? "centerPanelIconSelImage" : "centerPanelIconImage");
        if (o && "url" in o) this._centerImg.src = o.url;
        o = this._getSetting(this._mode == Bitrix.YandexMarkupPanelMode.markerAdd ? "markerPanelIconSelImage" : "markerPanelIconImage");
        if (o && "url" in o) this._markerImg.src = o.url;
    },
    openMarkerEditor: function (marker) {
        if (!marker) throw "YandexMapMarkupPanel: marker is not defined!";
        this._openMarkerEditor(marker);
    },
    _openMarkerEditor: function (marker) {
        if (this._mode != Bitrix.YandexMarkupPanelMode.drag) return;
        if (!marker) throw "YandexMapMarkupPanel: marker is not defined!";
        var bln = this._getBalloon();
        var opt = bln.getOptions();
        opt.marker = marker;
        opt.mode = Bitrix.YandexMarkupPanelMode.drag;
        bln.open();
    },
    _setupCenterMarkerByPos: function (pos) {
        if (!pos) throw "YandexMapMarkupPanel: pos is not defined!";
        if (this._centerMarker) {
            this._centerMarker.removeChangeListener(this._centerMarkerChangeHandler);
            this._centerMarker.release();
            delete this._centerMarker;
        }
        this._centerMarker = Bitrix.YandexMapMarker.create({ 'position': pos, 'icon': this._getSetting("centerMarkerImage"), 'draggable': true });
        this._centerMarker.setMap(this._map);
        this._centerMarker.addChangeListener(this._centerMarkerChangeHandler);
    },
    _getBalloon: function () {
        if (this._balloon) return this._balloon;
        var bln = this._balloon = new Bitrix.YandexMapMarkerBalloon({ map: this._map });
        bln.addCloseListener(Bitrix.TypeUtility.createDelegate(this, this._handleBalloonClose));
        return bln;
    },
    _stopSoaring: function () {
        if (!this._isSoaring) return;
        this._overlay.stopSoaring();
        this._isSoaring = false;
    },
    _handleDragMouseClick: function (e) {
        Bitrix.EventUtility.stopEventPropagation(e);
        this._stopSoaring();
        this._mode = Bitrix.YandexMarkupPanelMode.drag;
        this.layout();
    },
    _handleCenterMouseClick: function (e) {
        if (!e && "event" in window) e = window.event;
        Bitrix.EventUtility.stopEventPropagation(e);
        this._stopSoaring();
        this._mode = this._mode != Bitrix.YandexMarkupPanelMode.centerSetup ? Bitrix.YandexMarkupPanelMode.centerSetup : Bitrix.YandexMarkupPanelMode.drag;
        this.layout();
        var info = this._getSetting("centerMarkerImage");
        if (info && this._mode == Bitrix.YandexMarkupPanelMode.centerSetup) {
            this._overlay.startSoaring(info.url, { y: e.clientY, x: e.clientX }, this._centerDropHandler);
            this._isSoaring = true;
        }
    },
    _handleCenterMenuSetHereClick: function () { },
    _handleMarkerMouseClick: function (e) {
        if (!e && "event" in window) e = window.event;
        Bitrix.EventUtility.stopEventPropagation(e);
        this.startMarkerAdd({ y: e.clientY, x: e.clientX });
    },
    startMarkerAdd: function (mousePos) {
        this._stopSoaring();
        this._mode = this._mode != Bitrix.YandexMarkupPanelMode.markerAdd ? Bitrix.YandexMarkupPanelMode.markerAdd : Bitrix.YandexMarkupPanelMode.drag;
        this.layout();
        var info = this._getSetting("standardMarkerImage");
        if (info && this._mode == Bitrix.YandexMarkupPanelMode.markerAdd) {
            this._overlay.startSoaring(info.url, mousePos, this._markerDropHandler);
            this._isSoaring = true;
        }
    },
    startDrag: function () {
        this._stopSoaring();
        this._mode = Bitrix.YandexMarkupPanelMode.drag;
        this.layout();
    },
    _handleCenterDrop: function (latLng) {
        this.startDrag();
        this._setupCenterMarkerByPos(latLng);
        this._map.setCenter(latLng);
    },
    _handleMarkerDrop: function (latLng) {
        this.startDrag();

        var marker = Bitrix.YandexMapMarker.create({ 'position': latLng, 'icon': this._getSetting("standardMarkerImage"), 'draggable': true });
        this._map.addMarker(marker);
        var bln = this._getBalloon();
        var opt = bln.getOptions();
        opt.marker = marker;
        opt.mode = Bitrix.YandexMarkupPanelMode.markerAdd;
        bln.open();
    },
    _handleCenterMarkerChange: function () {
        if (this._suppressCenterMarkerChange || !this._centerMarker) return;
        this._suppressMapCenterChange = true;
        this._map.setCenter(this._centerMarker.getPosition());
        this._suppressMapCenterChange = false;
    },
    _handleMapCenterChange: function () {
        if (this._suppressMapCenterChange || !this._centerMarker) return;
        this._suppressCenterMarkerChange = true;
        this._centerMarker.setPosition(this._map.getCenter());
        this._suppressCenterMarkerChange = false;
    },
    _handleMarkerClick: function (sender, args) { this._openMarkerEditor(args.marker); },
    _handleBalloonClose: function (sender, args) {
        var marker = sender.getMarker();
        if (!marker) throw "YandexMapMarkupPanel: Could not find marker!";
        var markerInd = -1;
        var markers = this._map.getMarkers();
        for (var i = 0; i < markers.length; i++) {
            if (markers[i] != marker) continue;
            markerInd = i;
            break;
        }
        if (markerInd < 0) throw "YandexMapMarkupPanel: Could not find marker index!";

        if (args.buttonId == Bitrix.YandexMapMarkerBalloonButton.bDelete)
            this._map.removeMarker(markers[markerInd]);

        var senderOpt = sender.getOptions();
        if ("mode" in senderOpt && senderOpt.mode == Bitrix.YandexMarkupPanelMode.markerAdd)
            window.setTimeout(Bitrix.TypeUtility.createDelegate(this, function () { this.startMarkerAdd(null); }), 100);
    },
    _getMessage: function (id) {
        var id = "MapsMarkupPanel$" + id;
        return "BITRIX_YANDEX_MAPS_MSG" in window && id in window.BITRIX_YANDEX_MAPS_MSG ? window.BITRIX_YANDEX_MAPS_MSG[id] : id;
    }
}

Bitrix.YandexMapMarkerBalloonButton = { bSave: 1, bDelete: 2, bCancel: 3 }
Bitrix.YandexMapMarkerBalloon = function Bitrix$YandexMapMarkerBalloonButton(options) {
    this._options = options;
    this._balloon = null;
    this._contentDiv = null;
    this._titleEl = null;
    this._closeEvent = null;
}
Bitrix.YandexMapMarkerBalloon.prototype = {
    open: function () {
        var map = this.getMap();
        if (!map) throw "YandexMapMarkerBalloon: could not find map!";
        var marker = this.getMarker();
        if (!marker) throw "YandexMapMarkerBalloon: could not find marker!";
        this.reset();
        map.openBalloon(marker.getPosition(), this._prepareContent(), null);
        if (this._balloon = map.getBalloon()) this._setupBallon();
    },
    close: function (buttonId) {
        if (!buttonId) buttonId = Bitrix.YandexMapMarkerBalloonButton.bCancel;
        var map = this.getMap();
        if (map && map.closeBalloon() && this._closeEvent)
            try { this._closeEvent.fire(this, { buttonId: buttonId }); }
            catch (e) { }
    },
    addCloseListener: function (listener) {
        (this._closeEvent ? this._closeEvent : (this._closeEvent = new Bitrix.EventPublisher())).addListener(listener);
    },
    removeCloseListener: function (listener) {
        if (this._closeEvent) this._closeEvent.removeListener(listener);
    },
    getOptions: function () { return this._options ? this._options : (this._options = new Object()); },
    getMap: function () { var o = this.getOptions(); return "map" in o ? o.map : null; },
    getMarker: function () { var o = this.getOptions(); return "marker" in o ? o.marker : null; },
    _prepareContent: function () {
        var marker = this.getMarker();
        if (!marker) throw "YandexMapMarkerBalloon: marker is not found!";
        if (this._contentDiv) return this._contentDiv;
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
    _setupBallon: function () {
        var marker = this.getMarker();
        if (!marker) throw "YandexMapMarkerBalloon: marker is not found!";

        var ttl = this._titleEl;
        if (!ttl) throw "YandexMapMarkerBalloon: could not find ttile element!";
        ttl.value = marker.getTitle();
        ttl.select();
        ttl.focus();
    },
    reset: function () {
        ttl = this._titleEl;
        if (ttl) ttl.value = "";
    },
    _handleSaveBtnClick: function (e) {
        Bitrix.EventUtility.stopEventPropagation(e);
        var marker = this.getMarker();
        if (!marker) throw "YandexMapMarkerBalloon: marker is not found!";
        ttl = this._titleEl;
        if (!ttl) throw "YandexMapMarkerBalloon: marker is not defined!";
        marker.setTitle(ttl.value);
        this.close(Bitrix.YandexMapMarkerBalloonButton.bSave);
    },
    _handleDeleteBtnClick: function (e) {
        Bitrix.EventUtility.stopEventPropagation(e);
        this.close(Bitrix.YandexMapMarkerBalloonButton.bDelete);
    },
    _handleCancelBtnClick: function (e) {
        Bitrix.EventUtility.stopEventPropagation(e);
        this.close(Bitrix.YandexMapMarkerBalloonButton.bCancel);
    },
    _getMessage: function (id) {
        var id = "InfoWindow$" + id;
        return "BITRIX_YANDEX_MAPS_MSG" in window && id in window.BITRIX_YANDEX_MAPS_MSG ? window.BITRIX_YANDEX_MAPS_MSG[id] : id;
    }
}

Bitrix.YandexMapSearchResultBalloon = function Bitrix$YandexMapSearchResultBalloon(options) {
    this._options = options;
    if (!this.getSearchResult()) throw "YandexMapSearchResultBalloon: Could not find search result";
    this._balloon = null;
    this._contentDiv = null;
    this._titleEl = null;
    this._closeEvent = null;
}
Bitrix.YandexMapSearchResultBalloon.prototype = {
    open: function () {
        var map = this.getMap();
        if (!map) throw "YandexMapSearchResultBalloon: could not find map!";
        map.openBalloon(this.getSearchResult().getPosition(), this._prepareContent(), null);
        this._balloon = map.getBalloon();
    },
    close: function (buttonId) {
        if (!buttonId) buttonId = Bitrix.YandexMapMarkerBalloonButton.bCancel;
        var map = this.getMap();
        if (map && map.closeBalloon() && this._closeEvent)
            try { this._closeEvent.fire(this, { buttonId: buttonId }); }
            catch (e) { }
    },
    addCloseListener: function (listener) {
        (this._closeEvent ? this._closeEvent : (this._closeEvent = new Bitrix.EventPublisher())).addListener(listener);
    },
    removeCloseListener: function (listener) {
        if (this._closeEvent) this._closeEvent.removeListener(listener);
    },
    getOptions: function () { return this._options ? this._options : (this._options = new Object()); },
    getMap: function () { var o = this.getOptions(); return "map" in o ? o.map : null; },
    getSearchResult: function () { var o = this.getOptions(); return "searchResult" in o ? o.searchResult : null; },
    _prepareContent: function () {
        var r = this.getSearchResult()
        if (!this._contentDiv) {
            var c = this._contentDiv = document.createElement("DIV");
            var tab = document.createElement("TABLE");
            c.appendChild(tab);
            var infoR = tab.insertRow(-1);
            var infoC = infoR.insertCell(-1);
            infoC.innerHTML += "<p>" + r.getDescription() + "</p>";
            var btnR = tab.insertRow(-1);
            var btnC = btnR.insertCell(-1);
            btnC.colSpan = 2;
            var createMarkerBtn = this._createMarkerBtn = document.createElement("INPUT");
            createMarkerBtn.type = "button"; //prior appendChild for IE
            btnC.appendChild(createMarkerBtn);
            createMarkerBtn.value = this._getMessage("CreateMarker");
            Bitrix.EventUtility.addEventListener(createMarkerBtn, "click", Bitrix.TypeUtility.createDelegate(this, this._handleCreateMarkerBtnClick), false);
        }
        this._createMarkerBtn.disabled = r.getMarker() != null;
        return this._contentDiv;
    },
    _handleCreateMarkerBtnClick: function (e) {
        Bitrix.EventUtility.stopEventPropagation(e);
        this.getSearchResult().createMarker();
        this._createMarkerBtn.disabled = true;
        this.close(Bitrix.YandexMapMarkerBalloonButton.bSave);
    },
    _getMessage: function (id) {
        var id = "SearchResultInfoWindow$" + id;
        return "BITRIX_YANDEX_MAPS_MSG" in window && id in window.BITRIX_YANDEX_MAPS_MSG ? window.BITRIX_YANDEX_MAPS_MSG[id] : id;
    }
}

Bitrix.YandexMapSearchResult = function Bitrix$YandexMapSearchResult(options) {
    this._markerChangeHandler = Bitrix.TypeUtility.createDelegate(this, this._handleMarkerChange);
    this._markerRemoveHandler = Bitrix.TypeUtility.createDelegate(this, this._handleMarkerRemove);

    var r = this._result = options.result;
    if (!r) throw "YandexMapSearchResult: Could not find result!";

    this._position = Bitrix.MapCoord.fromYMapsGeoPoint(r.getGeoPoint());
    this._descr = r.text;
    this._parentEl = options.parentEl;
    if (!this._parentEl) throw "Could not find parent element!";
    this._map = options.map;
    this._marker = null;

    var c = this._container = this._parentEl.insertRow(-1);
    var markerC = c.insertCell(-1);
    markerC.className = "bx-ymaps-search-panel-result-marker";

    var markerImg = document.createElement("IMG");
    markerC.appendChild(markerImg);
    markerImg.title = markerImg.alt = this._getMessage("AddMarkerToolTip");

    var imgInfo = this._getSetting("standardMarkerImage");
    if (imgInfo && 'url' in imgInfo) markerImg.src = imgInfo.url;
    markerImg.onclick = Bitrix.TypeUtility.createDelegate(this, this._handleCreateMarkerClick);

    var contentC = c.insertCell(-1);
    contentC.className = "bx-ymaps-search-panel-result-content";

    var goToLink = document.createElement("A");
    contentC.appendChild(goToLink);
    goToLink.href = "";
    goToLink.title = this._getMessage("GoToPlaceToolTip");
    goToLink.onclick = Bitrix.TypeUtility.createDelegate(this, this._handleLinkClick);
    goToLink.innerHTML = this.getDescription();
}

Bitrix.YandexMapSearchResult.prototype = {
    _getSetting: function (name, defaultVal) {
        return typeof (Bitrix.YandexMapSettings) == "object" && name in Bitrix.YandexMapSettings ? Bitrix.YandexMapSettings[name] : defaultVal;
    },
    _getMessage: function (id) {
        var id = "SearchResult$" + id;
        return "BITRIX_YANDEX_MAPS_MSG" in window && id in window.BITRIX_YANDEX_MAPS_MSG ? window.BITRIX_YANDEX_MAPS_MSG[id] : id;
    },
    _handleCreateMarkerClick: function () {
        this._map.setCenter(this._position);
        this.createMarker();
    },
    _handleLinkClick: function () {
        var bln = new Bitrix.YandexMapSearchResultBalloon({ 'map': this._map, 'searchResult': this });
        bln.open();
        return false;
    },
    getDescription: function () { return this._descr; },
    getMap: function () { return this._map; },
    getPosition: function () { return this._position; },
    getMarker: function () { return this._marker; },
    createMarker: function () {
        if (this._marker) return;
        this._marker = Bitrix.YandexMapMarker.create({ 'position': this._position, 'title': this._descr, 'icon': this._getSetting("standardMarkerImage"), 'draggable': true });
        this._map.addMarker(this._marker);
        this._marker.addChangeListener(this._markerChangeHandler);
        this._map.addMarkerChangeListener(this._markerRemoveHandler);
    },
    _handleMarkerChange: function () {
        if (Bitrix.MapCoord.equals(this._position, this._marker.getPosition())) return;
        this._marker.removeChangeListener(this._markerChangeHandler);
        this._map.removeMarkerChangeListener(this._markerRemoveHandler);
        this._marker = null;
    },
    _handleMarkerRemove: function (sender, args) {
        if (!this._marker || args.marker != this._marker || args.action != Bitrix.YandexMapAction.remove) return;
        this._marker.removeChangeListener(this._markerChangeHandler);
        this._map.removeMarkerChangeListener(this._markerRemoveHandler);
        this._marker = null;
    }
}
Bitrix.YandexMapSearchResult.styles = {
    wrapper: "bx-ymaps-search-panel-result-item-wrapper",
    container: "bx-ymaps-search-panel-result-item-container"
}

Bitrix.YandexMapSearchPanel = function Bitrix$YandexMapSearchPanel(options) {
    this._geocoder = this._relults = null;

    if (!options) throw "YandexMapSearchPanel: options is not defined!";

    var srcMap = (this._map = "map" in options ? options.map : null) ? this._map.getSourceMap() : null;
    if (!srcMap) throw "YandexMapSearchPanel: source map is not found!";
    srcMap.addControl(this);
}
Bitrix.YandexMapSearchPanel.prototype = {
    onAddToMap: function (map, position) {
        var container = this._container = document.createElement("DIV");
        container.style.position = "absolute";
        container.style.zIndex = 1009;
        container.className = Bitrix.YandexMapSearchPanel.styles.container;

        var resultContainerWrapper = this._resultContainerWrapper = document.createElement("DIV");
        container.appendChild(resultContainerWrapper);
        resultContainerWrapper.className = Bitrix.YandexMapSearchPanel.styles.resultContainerWrapper;
        resultContainerWrapper.style.display = "none";

        var resultContainer = this._resultContainer = document.createElement("DIV");
        resultContainerWrapper.appendChild(resultContainer);
        resultContainer.className = Bitrix.YandexMapSearchPanel.styles.resultContainer;

        var cmdBarWrapper = this._cmdBarWrapper = document.createElement("DIV");
        container.appendChild(cmdBarWrapper);
        cmdBarWrapper.className = Bitrix.YandexMapSearchPanel.styles.cmdBarWrapper;
        cmdBarWrapper.style.display = "none";
        var cmdBar = document.createElement("DIV");
        cmdBarWrapper.appendChild(cmdBar);
        cmdBar.className = Bitrix.YandexMapSearchPanel.styles.cmdBar;
        var clearResultsLnk = document.createElement("A");
        cmdBar.appendChild(clearResultsLnk);
        clearResultsLnk.href = "";
        clearResultsLnk.onclick = Bitrix.TypeUtility.createDelegate(this, this._handleClearResultsLinkClick);
        clearResultsLnk.innerHTML = this._getMessage("Clear");

        var tbl = document.createElement("TABLE");
        tbl.className = "bx-ymaps-search-panel-control-tab";
        container.appendChild(tbl);
        tbl.border = tbl.cellPadding = tbl.cellSpacing = "0";

        var row = tbl.insertRow(-1);

        var inputC = row.insertCell(-1);
        inputC.className = Bitrix.YandexMapSearchPanel.styles.placeCell;

        var input = this._input = document.createElement("INPUT");
        input.type = "text";
        inputC.appendChild(input);
        input.onkeypress = Bitrix.TypeUtility.createDelegate(this, this._handleSearchInputEnter);

        var btnC = row.insertCell(-1);
        btnC.className = Bitrix.YandexMapSearchPanel.styles.buttonCell;

        var btnCnr = document.createElement("DIV");
        btnC.appendChild(btnCnr);
        btnCnr.className = Bitrix.YandexMapSearchPanel.styles.buttonContainer;

        var btn = this._btn = document.createElement("I");
        btn.innerHTML = "<b>" + this._getMessage("Search") + "</b>";
        btnCnr.appendChild(btn);

        Bitrix.EventUtility.addEventListener(btnCnr, "click", Bitrix.TypeUtility.createDelegate(this, this._handleSearchBtnClick));
        //this.layout();
        var srcMap = this._map ? this._map.getSourceMap() : null;
        if (!srcMap) throw "YandexMapSearchPanel: source map is not found!";
        srcMap.getContainer().appendChild(container);

        this.position = position || new YMaps.ControlPosition(YMaps.ControlPosition.BOTTOM_LEFT, new YMaps.Size(5, 5));
        this.position.apply(container);
    },
    onRemoveFromMap: function () {
        var m = this._map, c = this._container;
        if (!m || !c) return;
        map.getSourceMap().getContainer().removeChild(c);
        this._map = this._container = null;
    },
    //layout: function() { },
    _handleSearchInputEnter: function (e) {
        if (!(e ? e : (e = window.event))) return;
        if (e.keyCode != 13) return true;
        this._launch();
        Bitrix.EventUtility.stopEventPropagation(e);
        return false; //disable form submit
    },
    _handleSearchBtnClick: function () { this._launch(); },
    _launch: function () {
        var place = this._input.value;
        if (place.length == 0) return;

        var cdr = this._geocoder = new YMaps.Geocoder(place, { 'prefLang': this._getSetting('language', 'ru'), 'strictBounds': false, 'boundedBy': YMaps.GeoBounds.fromCenterAndSpan(this._map.getCenter().toYMapsGeoPoint(), new YMaps.Size(1, 1)) });
        YMaps.Events.observe(cdr, cdr.Events.Load, this._handleRequestLoad, this);
        YMaps.Events.observe(cdr, cdr.Events.Fault, this._handleRequestFault, this);
    },
    _handleRequestLoad: function (geocoder) {
        var cdr = this._geocoder;
        if (cdr != geocoder) return;

        var c = this._resultContainer;
        c.innerHTML = "";

        var tbl = document.createElement("TABLE");
        tbl.className = "bx-ymaps-search-panel-result-tab";
        c.appendChild(tbl);
        for (var i = 0; i < cdr.length(); i++)
            var r = new Bitrix.YandexMapSearchResult({ 'result': cdr.get(i), 'parentEl': tbl, 'map': this._map });

        var cmdBarW = this._cmdBarWrapper;
        cmdBarW.style.display = "";
        cmdBarW.style.left = "0px";
        var cmdBarWRect = Bitrix.ElementPositioningUtility.getElementRect(cmdBarW);
        cmdBarW.style.top = "-" + (cmdBarWRect.height + 2) + "px";

        var resultW = this._resultContainerWrapper;
        resultW.style.display = "";
        resultW.style.left = "0px";
        resultW.style.top = "-" + (cmdBarWRect.height + Bitrix.ElementPositioningUtility.getElementRect(resultW).height + 2) + "px";
    },
    _handleRequestFault: function (geocoder, errorMessage) {
        resultC.innerHTML = errorMessage;

        var cmdBarW = this._cmdBarWrapper;
        cmdBarW.style.display = "";
        cmdBarW.style.left = "0px";
        var cmdBarWRect = Bitrix.ElementPositioningUtility.getElementRect(cmdBarW);
        cmdBarW.style.top = "-" + (cmdBarWRect.height + 2) + "px";

        var resultW = this._resultContainerWrapper;
        resultW.style.display = "";
        resultW.style.left = "0px";
        resultW.style.top = "-" + (cmdBarWRect.height + Bitrix.ElementPositioningUtility.getElementRect(resultW).height + 2) + "px";
    },
    _handleClearResultsLinkClick: function () {
        this._resultContainerWrapper.style.display = "none";
        this._resultContainer.innerHTML = "";
        var input = this._input;
        input.value = "";
        input.focus();
        this._cmdBarWrapper.style.display = "none";
        return false;
    },
    _getMessage: function (id) {
        var id = "SearchPanel$" + id;
        return "BITRIX_YANDEX_MAPS_MSG" in window && id in window.BITRIX_YANDEX_MAPS_MSG ? window.BITRIX_YANDEX_MAPS_MSG[id] : id;
    },
    _getSetting: function (name, defaultVal) {
        return typeof (Bitrix.YandexMapSettings) == "object" && name in Bitrix.YandexMapSettings ? Bitrix.YandexMapSettings[name] : defaultVal;
    }
}
Bitrix.YandexMapSearchPanel.styles = {
    container: "bx-ymaps-search-panel-container",
    resultContainerWrapper: "bx-ymaps-search-panel-result-container-wrapper",
    resultContainer: "bx-ymaps-search-panel-result-container",
    cmdBarWrapper: "bx-ymaps-search-panel-cmd-bar-wrapper",
    cmdBar: "bx-ymaps-search-panel-cmd-bar",
    placeCell: "bx-ymaps-search-panel-input-tab-place",
    buttonCell: "bx-ymaps-search-panel-input-tab-button",
    buttonContainer: "bx-ymaps-search-panel-input-tab-button-container"
}