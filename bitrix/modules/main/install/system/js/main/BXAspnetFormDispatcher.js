if(typeof(Bitrix) == "undefined"){
	var Bitrix = new Object();
}

Bitrix.PopupDialogScriptPreEvaluteArgs = function() {
	if (typeof (Bitrix.PopupDialogScriptPreEvaluteArgs.initializeBase) == "function")
		Bitrix.PopupDialogScriptPreEvaluteArgs.initializeBase(this);
	this._content = null;
	this._cancel = false;
	this._isChanged = false;
	this._isInitialized = false;
}

Bitrix.PopupDialogScriptPreEvaluteArgs.prototype = {
    initialize: function(sourceScript){
        if(this._isInitialized) return;
		this._content = sourceScript;
        this._isInitialized = true;
    },
	getContent: function(){
		return this._content;
	},
	setContent: function(value){
		this._content = value;
		if(!this._isChanged)
			this._isChanged = true;
	},
	getCancel: function(){
		return this._cancel;
	},
	setCancel: function(value){
		this._cancel = value;
	},
	isChanged: function(){
		return this._isChanged;
	}	
}


Bitrix.PopupDialogScriptPreEvaluteArgs.create = function(sourceScript){
	var self = new Bitrix.PopupDialogScriptPreEvaluteArgs();
	self.initialize(sourceScript);
	return self;
}

if (typeof (Bitrix.PopupDialogScriptPreEvaluteArgs.registerClass) == "function")
	Bitrix.PopupDialogScriptPreEvaluteArgs.registerClass("Bitrix.PopupDialogScriptPreEvaluteArgs");

Bitrix.AntiCsrfUtility = function() {}
Bitrix.AntiCsrfUtility.registerForm = function(form, tokenName, tokenValue) {
	if(!(Bitrix.TypeUtility.isElementNode(form) && Bitrix.TypeUtility.isNotEmptyString(tokenName) && Bitrix.TypeUtility.isNotEmptyString(tokenValue) && form.elements && typeof(form.elements[tokenName]) == "undefined")) return;
	var el = document.createElement('INPUT'); 
	el.type = "hidden";
	el.name = tokenName; 
	el.value = tokenValue; 
	form.appendChild(el);
}

Bitrix.AspnetFormDispatcher = function() {
	if (typeof (Bitrix.AspnetFormDispatcher.initializeBase) == "function")
		Bitrix.AspnetFormDispatcher.initializeBase(this);
	this._previousForm = null;
	this._previousScriptManagerID = null;
	this._previousScriptManagerUpdatePanelIDs = null;
	this._previousScriptManagerUpdatePanelClientIDs = null;
	this._previousScriptManagerUpdatePanelHasChildrenAsTriggers = null;
	this._previousScriptManagerAsyncPostBackTimeout = null;
	this._previousScriptManagerAsyncPostBackControlIDs = null;
	this._previousScriptManagerAsyncPostBackControlClientIDs = null;
	this._previousScriptManagerPostBackControlIDs = null;
	this._previousScriptManagerPostBackControlClientIDs = null;
	this._previousValidatorsArray = null;	
	this._previousDoPostBack = null;	
	this._previousApplication = null;
	this._ajaxLoaded = false;
	
	this._pageRequestManagerRx = /Sys\.WebForms\.PageRequestManager\._initialize\('[^']+',\s*document\.getElementById\('[^']+'\)\);/;
	this._initialized = false;
}

Bitrix.AspnetFormDispatcher.prototype = {
    initialize: function(){
        if(this._initialized) return;

        this._initialized = true;
    },	

    handlePopupDialogScriptPreEvalute: function(args) {
		if(typeof(Bitrix.PopupDialogScriptPreEvaluteArgs.isInstanceOfType) == "function" && !Bitrix.PopupDialogScriptPreEvaluteArgs.isInstanceOfType(args)) 
			throw "Bitrix.AspnetFormDispatcherargs--handlePopupDialogScriptPreEvalute: is invalid argument!";		
		
		var srcScript = args.getContent();
		if(srcScript == null || srcScript.length == 0) return;
		
		if(srcScript.indexOf("BXAspnetFormDispatcher.js") >= 0){
				args.setCancel(true);
			return;
		}
		
				
		// MicrosoftAjax.js "Microsoft AJAX Framework"
		if(srcScript.indexOf("MicrosoftAjax.js") >= 0 && !this._ajaxLoaded){
			this._ajaxLoaded = true;
			if(typeof(Sys) != 'undefined' && typeof(Sys.Application) != 'undefined' && !this._previousApplication){
				this._previousApplication = Sys.Application;
				args.setContent("Sys.Application = new Sys._Application();");
			}
			return;
		}
		// MicrosoftAjaxWebForms.js "Microsoft AJAX ASP.NET WebForms Framework"
		if(srcScript.indexOf("MicrosoftAjaxWebForms.js") >= 0){
			if(typeof(window['Type']) == 'function' && Type.isNamespace(Sys.WebForms)){
				args.setCancel(true);
			}
			return;
		}
		
		if(this._pageRequestManagerRx.test(srcScript) && !this._previousScriptManagerID) {
			var mgr = Sys.WebForms.PageRequestManager._instance;
			if(mgr) {
				this._previousScriptManagerID = mgr._scriptManagerID;
				this._previousScriptManagerUpdatePanelIDs = mgr._updatePanelIDs;
				this._previousScriptManagerUpdatePanelClientIDs = mgr._updatePanelClientIDs;
				this._previousScriptManagerUpdatePanelHasChildrenAsTriggers = mgr._updatePanelHasChildrenAsTriggers;
				this._previousScriptManagerAsyncPostBackTimeout = mgr._asyncPostBackTimeout;
				this._previousScriptManagerAsyncPostBackControlIDs = mgr._asyncPostBackControlIDs;
				this._previousScriptManagerAsyncPostBackControlClientIDs = mgr._asyncPostBackControlClientIDs;
				this._previousScriptManagerPostBackControlIDs = mgr._postBackControlIDs;
				this._previousScriptManagerPostBackControlClientIDs = mgr._postBackControlClientIDs;
				mgr.dispose();
				delete Sys.WebForms.PageRequestManager._instance;
				Sys.WebForms.PageRequestManager._instance = null;
			}				
		}
		
		//theForm
		if(srcScript.indexOf("var theForm =") >= 0){
			if(typeof(window['theForm']) != 'undefined' && this._previousForm == null){
				this._previousForm = theForm;
				if(this._previousForm){
					this._previousForm.id = "$" + this._previousForm.id;
					this._previousForm.name = "$" + this._previousForm.name;
					var viewstate = document.getElementById("__VIEWSTATE");
					if(viewstate)
						viewstate.id = viewstate.name = "$VIEWSTATE$";
					
					var viewstateenc = document.getElementById("__VIEWSTATEENCRYPTED");
					if(viewstateenc)
						viewstateenc.id = viewstateenc.name = "$VIEWSTATEENCRYPTED$";						
					
					var eventvalidation = document.getElementById("__EVENTVALIDATION");
					if(eventvalidation)
						eventvalidation.id = eventvalidation.name = "$EVENTVALIDATION$";	

					var eventtarget = document.getElementById("__EVENTTARGET");
					if(eventtarget)
						eventtarget.id = eventtarget.name = "$EVENTTARGET$";							

					var eventargument = document.getElementById("__EVENTARGUMENT");
					if(eventargument)
						eventargument.id = eventargument.name = "$EVENTARGUMENT$";

					this._previousDoPostBack = window.__doPostBack;
				}
			}
			
			if(typeof(window['Page_Validators']) != "undefined")
			{
				this._previousValidatorsArray = window['Page_Validators'];
				//for correct work of 'Page_ClientValidate'
				window['Page_Validators'] = undefined;
			}
		    else
		        this._previousValidatorsArray = undefined;	

			return;
		}
		
		//WebFormFunctions
		if(srcScript.indexOf("function WebForm_PostBackOptions") >= 0){
			if(typeof(window['WebForm_PostBackOptions']) == 'function'){
				args.setCancel(true);
			}
			return;			
		}
		
		//Sys.WebForms.PageRequestManager
		if(srcScript.indexOf("Sys.WebForms.PageRequestManager._initialize") >= 0){	
			if (typeof(Sys) != "undefined" && typeof(Sys.WebForms) != "undefined" && Sys.WebForms.PageRequestManager.getInstance()) {			
				var rxInitialize = new RegExp("Sys\\.WebForms\\.PageRequestManager\\._initialize\\('([^']+)', document\\.getElementById\\('([^']+)'\\)\\);", "i");
				var m1 = rxInitialize.exec(srcScript);
				if(!m1)
				 throw "Could not find Sys.WebForms.PageRequestManager._initialize() parameters! Please validate regexp...";
				var rxUpdateControls = new RegExp("Sys\\.WebForms\\.PageRequestManager\\.getInstance\\(\\)\\._updateControls\\((.+?)\\);", "i");
				var m2 = rxUpdateControls.exec(srcScript);
				if(!m2)
				 throw "Could not find Sys.WebForms.PageRequestManager._updateControls() parameters! Please validate regexp...";
				
				var content = new Sys.StringBuilder();
				content.appendLine("//<![CDATA[");

				//content.appendLine("Sys.WebForms.PageRequestManager.getInstance().dispose();");
				content.appendLine("Sys.WebForms.PageRequestManager._instance = new Sys.WebForms.PageRequestManager();");

				content.append("Sys.WebForms.PageRequestManager.getInstance()._initializeInternal('");
				content.append(m1[1]);
				content.append("', document.getElementById('");
				content.append(m1[2]);
				content.appendLine("'));");
				content.append("Sys.WebForms.PageRequestManager.getInstance()._updateControls(");
				content.append(m2[1]);
				content.append(");");
				content.appendLine("//]]>");
				args.setContent(content.toString());
			}
		}
	},
	
	handlePopupDialogClose: function(){
		if(typeof(window['theForm']) != undefined && 
			this._previousForm != null){
			this._previousForm.id = this._previousForm.id.substr(1);
			this._previousForm.name = this._previousForm.name.substr(1);			
			theForm = this._previousForm;
			
			var viewstate = document.getElementById("$VIEWSTATE$");
			if(viewstate)
				viewstate.id = viewstate.name = "__VIEWSTATE"; 			
		
			var viewstateenc = document.getElementById("$VIEWSTATEENCRYPTED$");
			if(viewstateenc)
				viewstateenc.id = viewstateenc.name = "__VIEWSTATEENCRYPTED";		
		
			var eventvalidation = document.getElementById("$EVENTVALIDATION$");
			if(eventvalidation)
				eventvalidation.id = eventvalidation.name = "__EVENTVALIDATION";		
		
			var eventtarget = document.getElementById("$EVENTTARGET$");
			if(eventtarget)
				eventtarget.id = eventtarget.name = "__EVENTTARGET";							

			var eventargument = document.getElementById("$EVENTARGUMENT$");
			if(eventargument)
				eventargument.id = eventargument.name = "__EVENTARGUMENT";			
		
			if(typeof(Sys) != "undefined" && 
				typeof(Sys.WebForms) != "undefined" && 
				typeof(Sys.WebForms.PageRequestManager) != "undefined"){
				var mgrOld = Sys.WebForms.PageRequestManager._instance;
				if(mgrOld != null) {
					mgrOld.dispose();
					delete Sys.WebForms.PageRequestManager._instance;
					Sys.WebForms.PageRequestManager._instance = new Sys.WebForms.PageRequestManager();
				}
				if(this._previousScriptManagerID) {
					var mgr = Sys.WebForms.PageRequestManager._instance = new Sys.WebForms.PageRequestManager();
					mgr._initializeInternal(this._previousScriptManagerID, this._previousForm);
					mgr._updatePanelIDs = this._previousScriptManagerUpdatePanelIDs;
					mgr._updatePanelClientIDs = this._previousScriptManagerUpdatePanelClientIDs;
					mgr._updatePanelHasChildrenAsTriggers = this._previousScriptManagerUpdatePanelHasChildrenAsTriggers;
					mgr._asyncPostBackTimeout = this._previousScriptManagerAsyncPostBackTimeout;
					mgr._asyncPostBackControlIDs = this._previousScriptManagerAsyncPostBackControlIDs;
					mgr._asyncPostBackControlClientIDs = this._previousScriptManagerAsyncPostBackControlClientIDs;
					mgr._postBackControlIDs = this._previousScriptManagerPostBackControlIDs;
					mgr._postBackControlClientIDs = this._previousScriptManagerPostBackControlClientIDs;
				}				
			}
			
		    //if(typeof(window['Page_Validators']) != "undefined")Page_Validators = this._previousValidatorsArray;
		    window['Page_Validators'] = this._previousValidatorsArray;
		}
		
		if(this._ajaxLoaded) {
			Sys.Application.dispose();
			if(this._previousApplication)
				Sys.Application = this._previousApplication;
			else
				Sys.Application = new Sys._Application();
		}

		
		if(this._previousDoPostBack)
			window.__doPostBack = this._previousDoPostBack;
		
		this._previousForm = null;
		this._previousScriptManagerID = null;
		this._previousScriptManagerUpdatePanelIDs = null;
		this._previousScriptManagerUpdatePanelClientIDs = null;
		this._previousScriptManagerUpdatePanelHasChildrenAsTriggers = null;
		this._previousScriptManagerAsyncPostBackTimeout = null;
		this._previousScriptManagerAsyncPostBackControlIDs = null;
		this._previousScriptManagerAsyncPostBackControlClientIDs = null;
		this._previousScriptManagerPostBackControlIDs = null;
		this._previousScriptManagerPostBackControlClientIDs = null;	
		this._previousValidatorsArray == null;	
		this._webRequestManager = null;
		this._previousDoPostBack = null;
		this._previousApplication = null;
		this._ajaxLoaded = false;
	}
}

Bitrix.AspnetFormDispatcher.errorArgNull = function(paramName, message) {
	alert("errorArgNull");
	debugger;
}

Bitrix.AspnetFormDispatcher._instance = null;
Bitrix.AspnetFormDispatcher.get_instance = function(){
	if(this._instance == null){
        this._instance = new Bitrix.AspnetFormDispatcher();
		this._instance.initialize();
	}
    return this._instance;
}

if (typeof (Bitrix.AspnetFormDispatcher.registerClass) == "function")
	Bitrix.AspnetFormDispatcher.registerClass("Bitrix.AspnetFormDispatcher");


Bitrix.ComponentParametersSectionSettings = function() {
	if (typeof (Bitrix.ComponentParametersSectionSettings.initializeBase) == "function")	
		Bitrix.ComponentParametersSectionSettings.initializeBase(this);
	this._initialized = false;
	this._displayState = ""; //style.display
	this._id = null;
}

Bitrix.ComponentParametersSectionSettings.prototype = {
	initialize: function(){
		this._initialized = true;
	},
	get_id: function(){
		return this._id;
	},
	set_id: function(value){
		this._id = value;
	},
	get_displayState: function(){
		return this._displayState;
	},
	set_displayState: function(value){
		this._displayState = value;
	},
	set: function(section){
		if(!section) return;
		if(typeof(section.style) != "undefined")
			this._displayState = section.style.display;
	},
	get: function(section){
		if(!section) return;
		if(typeof(section.style) != "undefined")
			section.style.display = this._displayState;
	}
}

if (typeof (Bitrix.ComponentParametersSectionSettings.registerClass) == "function")
	Bitrix.ComponentParametersSectionSettings.registerClass("Bitrix.ComponentParametersSectionSettings");

Bitrix.ComponentParameterSection = function() {
	if (typeof (Bitrix.ComponentParameterSection.initializeBase) == "function")		
		Bitrix.ComponentParameterSection.initializeBase(this);
	this._initialized = false;
	this._id = "";
	this._elementId = "";
	this._expanderElementId = "";
	this._paramContainerClassName = "paramcontainer";
	this._expanded = null;
}

Bitrix.ComponentParameterSection.prototype = {
	initialize: function(id, elementId, expanderElementId){
		this.setId(id);
		this.setElementId(elementId);
		this.setExpanderElementId(expanderElementId);
		this._initialized = true;
	},
	getId: function(){
		return this._id;
	},
	setId: function(id){
		if(!Bitrix.TypeUtility.isString(id)) throw "Bitrix.ComponentParameterSection.setId: id is not valid!";
		this._id = id;
	},
	getElementId: function(){
		return this._elementId;
	},
	setElementId: function(id){
		if(!Bitrix.TypeUtility.isString(id)) throw "Bitrix.ComponentParameterSection.setElementId: id is not valid!";
		this._elementId = id;
	},	
	getExpanderElementId: function(){
		return this._expanderElementId;
	},
	setExpanderElementId: function(id){
		if(!Bitrix.TypeUtility.isString(id)) throw "Bitrix.ComponentParameterSection.setExpanderElementId: id is not valid!";
		this._expanderElementId = id;
	},	
	isExpanded: function(){
		if(this._expanded == null){
			var expanderElem = document.getElementById(this._expanderElementId);
			if(expanderElem)
				this._expanded = expanderElem.className == "bx-popup-sign bx-popup-minus";
			else
				this._expanded = false;
		}
		return this._expanded;
	},
	expand: function(expand){
		if(!Bitrix.TypeUtility.isBoolean(expand)) throw "Bitrix.ComponentParameterSection.expand: expand is not valid!";
		var elem = document.getElementById(this._elementId);
		if(!elem) return false;
		var paramContainer = elem.nextSibling;
		while(paramContainer){
			if(paramContainer.nodeType == 1){
				if(typeof(paramContainer.className) != "string" || !("className" in paramContainer) || paramContainer.className.indexOf(Bitrix.ComponentParameterSection._paramContainerClassName) < 0)
					break;
				var views = Bitrix.ComponentParametersEditor.getInstance().getViewsByViewContainerElementID(paramContainer.id);	
				if(views != null && views.length > 0){
					for(var i = 0; i < views.length; i++)
						views[i].setContainerVisibility(expand, 1, true);
				}
				else
					paramContainer.style.display = expand ? "" : "none";
				
				delete views;
			}
			paramContainer =  paramContainer.nextSibling;
		}
		
		var expanderElem = document.getElementById(this._expanderElementId);
		if(expanderElem)
			expanderElem.className = expand ? "bx-popup-sign bx-popup-minus": "bx-popup-sign bx-popup-plus";
		
		this._expanded = expand;
		return true;
	},
	refresh: function()
	{
		var elem = document.getElementById(this._elementId);
		if(!elem) return;
		
		if(!this.isExpanded())
			return;
		
		var aboutDisplay = false;
		var paramContainer = elem.nextSibling;
		while(paramContainer){
			if(paramContainer.nodeType == 1){
				if(typeof(paramContainer.className) != "string" || !("className" in paramContainer) || paramContainer.className.indexOf(Bitrix.ComponentParameterSection._paramContainerClassName) < 0)
					break;
				if(paramContainer.style.display != "none"){
					aboutDisplay = true;
					break;
				}
			}
			paramContainer =  paramContainer.nextSibling;
		}
		elem.style.display = aboutDisplay ? "" : "none";	
	},
	reset: function(){}
}

Bitrix.ComponentParameterSection.create = function(id, elementId, expanderElementId){
	var self = new Bitrix.ComponentParameterSection();
	self.initialize(id, elementId, expanderElementId);
	return self;
}

Bitrix.ComponentParameterSection._paramContainerClassName = "paramcontainer";
Bitrix.ComponentParameterSection._sectionClassName = "section";

Bitrix.ComponentParameterSection.getSectionElementParameterContainerElement = function(paramContainerEl){
	if(!paramContainerEl || !("tagName" in paramContainerEl)) throw "Bitrix.ComponentParameterSection.getElementSection: paramContainerEl is not valid!";
		var el = paramContainerEl.previousSibling;
		while(el){
			if(el.nodeType == 1 && "className" in el && el.className.indexOf(Bitrix.ComponentParameterSection._sectionClassName) >= 0)
				return el;
			el = el.previousSibling;
		}
		return null;
}

if (typeof (Bitrix.ComponentParameterSection.registerClass) == "function")
	Bitrix.ComponentParameterSection.registerClass("Bitrix.ComponentParameterSection");

Bitrix.ComponentParametersEditor = function() {
	if (typeof (Bitrix.ComponentParametersEditor.initializeBase) == "function")			
		Bitrix.ComponentParametersEditor.initializeBase(this);
	this._initialized = false;
	this._sectionsSettings = null;
	this._views = null;
	this._sections;
	//this._sectionClassNameExpanded = "bx-popup-sign bx-popup-plus";
	//this._sectionClassNameCollapsed = "bx-popup-sign bx-popup-minus";
	this._paramContainerClassName = "paramcontainer";
	this._viewContainerElementByViewIdFunction = null;
}

Bitrix.ComponentParametersEditor.prototype = {
	initialize: function(){
		this.reset();
		this._initialized = true;
	},
	_getSettings: function(sectionId, id, createIfNotExists){
		if(typeof(sectionId) != "string" || sectionId.length == 0)
			throw "Invalid argument: sectionId";
		if(typeof(id) != "string" || id.length == 0)
			throw "Invalid argument: id";
		if(typeof(this._sectionsSettings[sectionId]) == "undefined" || this._sectionsSettings[sectionId] == null || typeof(this._sectionsSettings[sectionId][id]) == "undefined" || this._sectionsSettings[sectionId][id] == null){
			if(createIfNotExists){
				if(typeof(this._sectionsSettings[sectionId]) == "undefined" || this._sectionsSettings[sectionId] == null)
					this._sectionsSettings[sectionId] = new Object();
				
				this._sectionsSettings[sectionId][id] = new Bitrix.ComponentParametersSectionSettings();
				this._sectionsSettings[sectionId][id].initialize();
				return this._sectionsSettings[sectionId][id];
			}
			else
				return null;
		}
		return this._sectionsSettings[sectionId][id];		
	},
	_clearSettings: function(sectionId, id){
		if(typeof(sectionId) != "string" || sectionId.length == 0)
			throw "Invalid argument: sectionId";
		if(typeof(id) != "string" || id.length == 0)
			throw "Invalid argument: id";
		if(typeof(this._sectionsSettings[sectionId]) == "undefined" || this._sectionsSettings[sectionId] == null || typeof(this._sectionsSettings[sectionId][id]) == "undefined" || this._sectionsSettings[sectionId][id] == null)
			return;
		this._sectionsSettings[sectionId][id] = null;	
	},
	expandSection: function(sectionId, expand){
		var section = this.getSection(sectionId);
		if(!section) return false;
		return section.expand(expand);
	},
	filterSection: function(el, className, handler){
		if(!el)
			throw "Is not defined: el!";
		if(typeof(className) != "string" || className.length == 0 )
			throw "Is not assigned: className!";
		var container = null;
		if(el.className && el.className.indexOf(className) >= 0)
			container = el;
		else{
			var elParentNode = el.parentNode;
			while(elParentNode){
				if(!elParentNode.className || elParentNode.className.indexOf(className) < 0){
					elParentNode = elParentNode.parentNode;
					continue;
				}
				container = elParentNode;
				break;
			}
		}
		if(!container) return;
		var firstSibling = container;
		var prevSibling = null;
		do{
			var prevSibling = firstSibling.previousSibling; 
			if(prevSibling){
				while(prevSibling && prevSibling.nodeType != 1){
					prevSibling = prevSibling.previousSibling;
				}
			}
			if(prevSibling){
				if(typeof(prevSibling.className) != "undefined" && prevSibling.className.indexOf(className) >= 0)
					firstSibling = prevSibling;
				else
					prevSibling = null;
			}
			  
		} while(prevSibling);                 

		var curSibling = firstSibling;
		do{
			if(curSibling.nodeType != 1){
				curSibling = curSibling.nextSibling;
				continue;
			}
			if(typeof(curSibling.className) == "undefined" || curSibling.className.indexOf(className) < 0)
				break;
			 handler(curSibling, el);
			curSibling = curSibling.nextSibling;
		} while(curSibling);
	},
	reset: function(){
		delete this._sectionsSettings;
		this._sectionsSettings = new Object();
		delete this._views;
		this._views = new Object();
		delete this._sections;
		this._sections = new Object;
	},
	setView: function(key, val){
		if(!this._initialized) throw "Is not initialized!";
		this._views[key] = val;
		val.setViewContainerElementByViewIdFunction(this._viewContainerElementByViewIdFunction);
	},
	getView: function(key){
		if(!this._initialized) throw "Is not initialized!";
		return typeof(this._views[key]) != "undefined" ? this._views[key] : null;
	},
	getViewsByParentObjectID: function(parentObjectID){
		if(!Bitrix.TypeUtility.isNotEmptyString(parentObjectID))
			return getAllViews();
		var r = new Array();
		for(var key in this._views){
			var view = this._views[key];
			if(view && view.getParentObjectID() == parentObjectID)
				r.push(view);
		}
		return r;
	},
	
	getViewsByComponentParameterID: function(componentParameterID){
		var r = new Array();
		for(var key in this._views){
			var view = this._views[key];
			if(view && view.getComponentParameterID() == componentParameterID)
				r.push(view);
		}
		return r;		
	},
	getViewsByViewContainerElementID: function(ID){
		if(!Bitrix.TypeUtility.isNotEmptyString(ID)) throw "ComponentParametersEditor.getViewsByViewContainerElementID: ID is not valid!";
		var r = new Array();
		for(var key in this._views){
			var view = this._views[key];
			if(!view || typeof(view.getViewContainerElement) != "function")
				continue;
			var el = view.getViewContainerElement();
			if(!el || el.id != ID)
				continue;
			r.push(view);
		}
		return r;
	},	
	getAllViews: function(){
		var r = new Array();
		for(var key in this._views)
			r.push(this._views[key]);
		return r;		
	},
	setSection: function(val){
		if(!(val instanceof Bitrix.ComponentParameterSection)) throw "Bitrix.ComponentParametersEditor.setView: val is not valid!";
		this._sections[val.getId()] = val;
	},
	getSection: function(key){
		if(!Bitrix.TypeUtility.isNotEmptyString(key)) throw "Bitrix.ComponentParametersEditor.key: key is not valid!";
		return typeof(this._sections) == "object" && key in this._sections ? this._sections[key] : null;
	},
	getViewContainerElementByViewIdFunction: function(){
		return this._viewContainerElementByViewIdFunction;
	},
	setViewContainerElementByViewIdFunction: function(func){
		this._viewContainerElementByViewIdFunction = Bitrix.TypeUtility.isFunction(func) ? func : null;
		if(!this._views) return;
		for(var key in this._views)
			this._views[key].setViewContainerElementByViewIdFunction(this._viewContainerElementByViewIdFunction);
	},		
	handleViewVisibilityChange: function(view){
		if(typeof(this._sections) != "object") return;
		var section = null;
		if(view && typeof(view.getViewContainerElement) == "function"){
			var el = view.getViewContainerElement();
			if(el){
				var sectionEl = Bitrix.ComponentParameterSection.getSectionElementParameterContainerElement(el);
				if(sectionEl)
					for(var key in this._sections){
						if(this._sections[key].getElementId() != sectionEl.id)
							continue;
						section = this._sections[key];
						break;
					}
			}
		}
		if(section)
			section.refresh();
		else{
			for(var key in this._sections)
				this._sections[key].refresh();
		}
	},
	evaluteVisibilityForParameterView: function(parameterID){
		return Bitrix.ParamClientSideActionManager.getInstance().evaluteVisibilityForParameterView(parameterID);
	}
}
Bitrix.ComponentParametersEditor._instance = null;
Bitrix.ComponentParametersEditor.getInstance = function(){
	if(this._instance == null){
        this._instance = new Bitrix.ComponentParametersEditor();
		this._instance.initialize();
	}
    return this._instance;
}

Bitrix.ComponentParametersEditor.getView = function(key){
	return this.getInstance().getView(key);
}

Bitrix.ComponentParametersEditor.setView = function(key, val){
	return this.getInstance().setView(key, val);
}

Bitrix.ComponentParametersEditor.getViewsByParentObjectID = function(parentObjectID){
	return this.getInstance().getViewsByParentObjectID(parentObjectID);
}

Bitrix.ComponentParametersEditor.getViewsByComponentParameterID = function(componentParameterID){
	return this.getInstance().getViewsByComponentParameterID(componentParameterID);
}

if (typeof (Bitrix.ComponentParametersEditor.registerClass) == "function")
	Bitrix.ComponentParametersEditor.registerClass("Bitrix.ComponentParametersEditor");

Bitrix.ComponentParameterModificationMode = function() {
	if (typeof (Bitrix.ComponentParameterModificationMode.initializeBase) == "function")				
		Bitrix.ComponentParameterModificationMode.initializeBase(this);
	this._initialized = false;
	this._currentID = null;
}

Bitrix.ComponentParameterModificationMode.prototype = {
	initialize: function(currentID){
		this.setCurrentID(currentID);
		this._initialized = true;
	},
	getCurrentID: function(){ return this._currentID; },
	getCurrentName: function(){
		return Bitrix.EnumHelper.getName(Bitrix.ComponentParameterModificationMode._values, this._currentID);
	},	
	setCurrentID: function(userValue){
		if(!Bitrix.EnumHelper.checkPresenceById(Bitrix.ComponentParameterModificationMode._values, userValue))		
			throw "Value '" + userValue + "' is unknown!";
		this._currentID = userValue;
	}
}

Bitrix.ComponentParameterModificationMode._values = { standard : 1, expression: 2};
Bitrix.ComponentParameterModificationMode.values = function(){ return this._values; }

Bitrix.ComponentParameterModificationMode.create = function(current){
    var self = new Bitrix.ComponentParameterModificationMode();
	self.initialize(current);
    return self;	
}

Bitrix.ComponentParameterModificationMode.createByName = function(name){
	var id = Bitrix.EnumHelper.getId(Bitrix.ComponentParameterModificationMode._values, name);
	if(!(typeof(id) == "number" || id instanceof Number)) throw "Bitrix.ComponentParameterModificationMode.createByName: id is not found!";
		
	var self = new Bitrix.ComponentParameterModificationMode();
	self.initialize(id);
    return self;	
}

if (typeof (Bitrix.ComponentParameterModificationMode.registerClass) == "function")
	Bitrix.ComponentParameterModificationMode.registerClass("Bitrix.ComponentParameterModificationMode");

Bitrix.DynamicExpression = function() {
	if (typeof (Bitrix.DynamicExpression.initializeBase) == "function")					
		Bitrix.DynamicExpression.initializeBase(this);
	this._initialized = false;
	this._prefix = "";
	this._content = "";
	this._enabled = false;
	this._sourceText = ""; //initial text
}

Bitrix.DynamicExpression.prototype = {
	initialize: function(){
		this._initialized = true;
	},
	getPrefix: function(){
		return this._prefix;
	},
	setPrefix: function(prefix){
		//if(!Bitrix.TypeUtility.isNotEmptyString(prefix))
		//	throw "Bitrix.DynamicExpression.setPrefix: prefix is not valid!";
		this._prefix = typeof(bxhtmlunspecialchars) == "function" ? bxhtmlunspecialchars(prefix) : prefix;
	},
	getContent: function(){
		return this._content;
	},
	setContent: function(content){
		//if(!Bitrix.TypeUtility.isNotEmptyString(content))
		//	throw "Bitrix.DynamicExpression.setPrefix: content is not valid!";
		this._content = typeof(bxhtmlunspecialchars) == "function" ? bxhtmlunspecialchars(content) : content;
	},
	getSourceText: function(){
		return this._sourceText;
	},
	isEmpty: function(){
		return this._prefix.length == 0 && this._content.length == 0;
	},	
	isValid: function(){
		return this._prefix.length > 0 && this._content.length > 0;
	},
	getEnabled: function(){
		return this._enabled;
	},
	setEnabled: function(enabled){
		this._enabled = enabled;
	},
	parse: function(sourceText){
		if(!this._initialized) throw "Bitrix.DynamicExpression.parse: Is not initialized!";
		if(!Bitrix.TypeUtility.isNotEmptyString(sourceText))
			sourceText = "";
			
		var prefix = "", content = "";
		var match = Bitrix.DynamicExpression._dynExpRegex.exec(sourceText);
		if(match != null){
			if(1 in match)
				prefix = match[1];
			if(2 in match)
				content = match[2];
		}
		this.setPrefix(prefix);
		this.setContent(content);
		this._sourceText = sourceText;	
	},
	toString: function(){
		var prefix = this._prefix;
		var content = this._content;
		if(typeof(bxhtmlspecialchars) == "function"){
			prefix = bxhtmlspecialchars(prefix);
			content = bxhtmlspecialchars(content);
		}
		return this.isValid() ? "<%$ " + prefix + ":" + content + "%>" : this._sourceText;
	}
}

Bitrix.DynamicExpression._entries = new Object();

Bitrix.DynamicExpression.getEntry = function(key){
	if(!Bitrix.TypeUtility.isNotEmptyString(key))
		return null;
	return typeof(this._entries[key]) != "undefined" ? this._entries[key] : null;
}

Bitrix.DynamicExpression.setEntry = function(key, entry){
	if(!Bitrix.TypeUtility.isNotEmptyString(key))
		throw "Bitrix.DynamicExpression.setEntry: key is not specified!";
	if(!entry)
		throw "Bitrix.DynamicExpression.setEntry: entry is not specified!";	
	this._entries[key] = entry;
}

Bitrix.DynamicExpression.create = function(key){
	var self = new Bitrix.DynamicExpression();
	self.initialize();
	if(Bitrix.TypeUtility.isNotEmptyString(key))
		this.setEntry(key, self);
	return self;
}

Bitrix.DynamicExpression.remove = function(key){
	if(!Bitrix.TypeUtility.isNotEmptyString(key))
		return;
	if(key in this._entries)
		delete this._entries[key];
}

Bitrix.DynamicExpression._dynExpRegex = new RegExp("^<%\\s*\\$\\s*(\\w+)\\s*\\:\\s*([^\\r\\n]+?)\\s*%>$");

Bitrix.DynamicExpression.parse = function(sourceText){
	var self = new Bitrix.DynamicExpression();
	self.initialize();
	self.parse(sourceText);
	return self;
}

Bitrix.DynamicExpression.isValidSourceText = function(sourceText){
	if(!Bitrix.TypeUtility.isNotEmptyString(sourceText))
		return false;
	return this._dynExpRegex.test(sourceText);
}

if (typeof (Bitrix.DynamicExpression.registerClass) == "function")
	Bitrix.DynamicExpression.registerClass("Bitrix.DynamicExpression");

Bitrix.ComponentParameterViewComboExpressionEditor = function() {
	if (typeof (Bitrix.ComponentParameterViewComboExpressionEditor.initializeBase) == "function")					
		Bitrix.ComponentParameterViewComboExpressionEditor.initializeBase(this);
	this._initialized = false;
	this._prefixCtrlID = null;
	this._expressionCtrlID = null; //container
	this._expressionPrefixCtrlID = null;
	this._expressionContentCtrlID = null;
	this._modificationModeCtrlID = null;
	this._modificationMode = null;
	this._switchModeHandler = BX.delegate(this._handleSwitchMode, this);
	this._parentObjectID = null;
	this._ID = null;
	this._componentParameterID = null;
	this._componentParameterViewCtrlID = null;
	this._changePrefixHandler = BX.delegate(this._handlePrefixChange, this);
	this._changeContentHandler = BX.delegate(this._handleContentChange, this);
	this._refreshTriggerHandler = BX.delegate(this._handleRefreshTrigger, this);
	this._refreshSourceHandler = BX.delegate(this._handleRefreshSource, this);
	this._refreshTriggeringScript = "";
	this._refreshTriggeringCtrlID = "";
	this._refreshTriggeringCtrlEventName = "";
	
	this._refreshTriggeringEventListeningEnabled = false;
	this._containerVisibility = null;
	this._viewContainerElementByViewIdFunction = null;
	this._viewContainerElementID = null;
}

Bitrix.ComponentParameterViewComboExpressionEditor.prototype = {
	initialize: function(ID, componentParameterID, componentParameterViewCtrlID, expressionCtrlID, modificationModeCtrlID){
		this._ID = ID;
		this._componentParameterID = componentParameterID;
		this._componentParameterViewCtrlID = componentParameterViewCtrlID;
		this._expressionCtrlID = expressionCtrlID;
		this._modificationModeCtrlID = modificationModeCtrlID;
	
		var modificationModeCtrl = document.getElementById(this._modificationModeCtrlID);
		
		if(modificationModeCtrl && modificationModeCtrl.value && modificationModeCtrl.value.length > 0)
			this._modificationMode = Bitrix.ComponentParameterModificationMode.createByName(modificationModeCtrl.value);
		else
			this._modificationMode = Bitrix.ComponentParameterModificationMode.create(Bitrix.ComponentParameterModificationMode.values().standard);
		this._initialized = true;
	},	
	getID: function(){
		return this._ID;
	},
	setID: function(ID){
		this._ID = ID;
	},
	getComponentParameterID: function(){
		return this._componentParameterID;
	},
	setComponentParameterID: function(ID){
		this._componentParameterID = ID;
	},
	enableChangingHandling: function(){
		var prefix = this.getExpressionPrefixCtrl();
		if(prefix)
			Bitrix.EventHandlingUtility.getInstance().addEventListener(prefix, "change", this._changePrefixHandler);
			
		var content = this.getExpressionContentCtrl();
		if(content)
			Bitrix.EventHandlingUtility.getInstance().addEventListener(content, "change", this._changeContentHandler);			
	},
	_handlePrefixChange: function(e){
		Bitrix.ComponentParameterViewDynamicExpressionsManager._handleExpressionViewChange(this);
	},
	_handleContentChange: function(e){
		Bitrix.ComponentParameterViewDynamicExpressionsManager._handleExpressionViewChange(this);
	},
	_handleRefreshTrigger: function(e){
		var refreshTriggeringCtrl = this.getRefreshTriggeringCtrl();
		if(!refreshTriggeringCtrl)
			throw "Bitrix.ComponentParameterViewComboExpressionEditor:_handleRefresh: refreshTriggeringCtrl is not found!";
		refreshTriggeringCtrl.src = Bitrix.ComponentParameterViewDynamicExpressionsManager.getInstance().getRefreshAnimatedImageUrl(); 	
	
		if(this._refreshTriggeringScript.length > 0)
			window.eval(this._refreshTriggeringScript);
	},
	_handleRefreshSource: function(){
		var refreshTriggeringCtrl = this.getRefreshTriggeringCtrl();
		if(!refreshTriggeringCtrl)
			throw "Bitrix.ComponentParameterViewComboExpressionEditor:_handleRefresh: refreshTriggeringCtrl is not found!";
		refreshTriggeringCtrl.src = Bitrix.ComponentParameterViewDynamicExpressionsManager.getInstance().getRefreshAnimatedImageUrl(); 			
	},
	getComponentParameterViewCtrlID: function(){
		return this._componentParameterViewCtrlID;
	},
	setComponentParameterViewCtrlID: function(val){
		this._componentParameterViewCtrlID = val;
	},	
	getExpressionCtrlID: function(){
		return this._expressionCtrlID;
	},
	setExpressionCtrlID: function(val){
		this._expressionCtrlID = val;
	},
	getExpressionPrefixCtrlID: function(){
		return this._expressionPrefixCtrlID;
	},
	getExpressionPrefixCtrl: function(){
		return document.getElementById(this._expressionPrefixCtrlID);
	},	
	setExpressionPrefixCtrlID: function(val){
		this._expressionPrefixCtrlID = val;
	},	
	getExpressionContentCtrlID: function(){
		return this._expressionContentCtrlID;
	},
	getExpressionContentCtrl: function(){
		return document.getElementById(this._expressionContentCtrlID);
	},		
	setExpressionContentCtrlID: function(val){
		this._expressionContentCtrlID = val;
	},	
	getModificationModeCtrlID: function(){
		return this._modificationModeCtrlID;
	},
	setModificationModeCtrlID: function(val){
		this._modificationModeCtrlID = val;
	},	
	getRefreshTriggeringCtrlID: function(){
		return this._refreshTriggeringCtrlID;
	},
	setRefreshTriggeringCtrlID: function(val){
		if(this._refreshTriggeringEventListeningEnabled)
			throw "Bitrix.ComponentParameterViewComboExpressionEditor.setRefreshTriggeringCtrlID: Event is already listened to!";
			
		if(!Bitrix.TypeUtility.isNotEmptyString(val))
			throw "Bitrix.ComponentParameterViewComboExpressionEditor.setRefreshTriggeringCtrlID: val is not valid!";
		this._refreshTriggeringCtrlID = val;
	},
	getRefreshTriggeringCtrl: function(){
		if(!Bitrix.TypeUtility.isNotEmptyString(this._refreshTriggeringCtrlID))
			throw "Bitrix.ComponentParameterViewComboExpressionEditor:getRefreshTriggeringCtrl: refreshTriggeringCtrlID is not defined!";
		return document.getElementById(this._refreshTriggeringCtrlID);
	},
	getRefreshTriggeringCtrlEventName: function(){
		return this._refreshTriggeringCtrlEventName;
	},
	setReshreshTriggeringCtrlEventName: function(val){
		if(this._refreshTriggeringEventListeningEnabled)
			throw "Bitrix.ComponentParameterViewComboExpressionEditor.setRefreshTriggeringCtrlID: Event is already listened to!";
			
		if(!Bitrix.TypeUtility.isNotEmptyString(val))
			throw "Bitrix.ComponentParameterViewComboExpressionEditor.setRefreshTriggeringCtrlID: val is not valid!";
		this._refreshTriggeringCtrlEventName = val;
	},
	getModificationMode: function(){
		return this._modificationMode;
	},
	getParentObjectID: function(){
		return this._parentObjectID;
	},
	setParentObjectID: function(parentObjectID){
		this._parentObjectID = parentObjectID;
	},
	getRefreshTriggeringScript: function(){
		return this._refreshTriggeringScript;
	},
	setRefreshTriggeringScript: function(refreshTriggeringScript){
		this._refreshTriggeringScript = Bitrix.TypeUtility.isNotEmptyString(refreshTriggeringScript) ? refreshTriggeringScript : "";
	},
	_adjustControls: function(){
		this._ensureInitialized();
		
		var componentParameterViewCtrl = document.getElementById(this._componentParameterViewCtrlID);
		if(!componentParameterViewCtrl) throw "Bitrix.ComponentParameterViewComboExpressionEditor._adjustControls: could not find component parameter view control with ID = '" + this._componentParameterViewCtrlID + "'!";
	
		var expressionCtrl = document.getElementById(this._expressionCtrlID);
		if(!expressionCtrl) throw "Bitrix.ComponentParameterViewComboExpressionEditor._adjustControls: could not find expression control with ID = '" + this._expressionCtrlID + "'!";	
		
		var modificationModeCtrl = document.getElementById(this._modificationModeCtrlID);
		if(!modificationModeCtrl) throw "Bitrix.ComponentParameterViewComboExpressionEditor._adjustControls: could not find nodification mode control with ID = '" + this._modificationModeCtrlID + "'!";		
			
		if(this._modificationMode.getCurrentID() == Bitrix.ComponentParameterModificationMode.values().standard){
			componentParameterViewCtrl.style.display = "";
			expressionCtrl.style.display = "none";
		}
		else if(this._modificationMode.getCurrentID() == Bitrix.ComponentParameterModificationMode.values().expression){
			componentParameterViewCtrl.style.display = "none";
			expressionCtrl.style.display = "";			
		}
		else throw "Bitrix.ComponentParameterViewComboExpressionEditor._adjustControls: modification mode '" + this._modificationMode.getCurrentName() + "' is not supported in current context!";	
		modificationModeCtrl.value = this._modificationMode.getCurrentName();
	},
	setModificationMode: function(val){
		this._ensureInitialized();
		if(val.getCurrentID() != this._modificationMode.getCurrentID())
			this._modificationMode = val;
		this._adjustControls();
	},
	switchModificationMode: function(){
		this._ensureInitialized();
		if(this._modificationMode.getCurrentID() == Bitrix.ComponentParameterModificationMode.values().standard)
			this.setModificationMode(Bitrix.ComponentParameterModificationMode.create(Bitrix.ComponentParameterModificationMode.values().expression));
		else if(this._modificationMode.getCurrentID() == Bitrix.ComponentParameterModificationMode.values().expression)
			this.setModificationMode(Bitrix.ComponentParameterModificationMode.create(Bitrix.ComponentParameterModificationMode.values().standard));
		else throw "Modification mode '" + this._modificationMode.getCurrentName() + "' is not supported in current context!";
	},
	_handleSwitchMode: function(e){
		this.switchModificationMode();
		Bitrix.ComponentParameterViewDynamicExpressionsManager._handleExpressionViewChange(this);
	},
	enableListeningToSwitchModeTriggerElement: function(el, eventName){
		this._ensureInitialized();
		if(!el) 
			throw "Bitrix.ComponentParameterViewComboExpressionEditor:enableListeningToSwitchModeTriggerElement: el is not defined!";
		if(!Bitrix.TypeUtility.isNotEmptyString(eventName))
			throw "Bitrix.ComponentParameterViewComboExpressionEditor:enableListeningToSwitchModeTriggerElement: eventName is not defined!";
		Bitrix.EventHandlingUtility.getInstance().addEventListener(el, eventName, this._switchModeHandler);	
	},
	enableListeningToRefreshTriggerElement: function(enable){
		this._ensureInitialized();
		if(enable){
			if(this._refreshTriggeringEventListeningEnabled)
				return;
	
			var refreshTriggeringCtrl = this.getRefreshTriggeringCtrl();
			if(!refreshTriggeringCtrl)
				throw "Bitrix.ComponentParameterViewComboExpressionEditor:enableListeningToRefreshTriggerElement: refreshTriggeringCtrl is not found!";
			
			Bitrix.EventHandlingUtility.getInstance().addEventListener(refreshTriggeringCtrl, Bitrix.TypeUtility.isNotEmptyString(this._refreshTriggeringCtrlEventName) ? this._refreshTriggeringCtrlEventName : "click", this._refreshTriggerHandler);
			this._refreshTriggeringEventListeningEnabled = true;
		}
		else{
			if(!this._refreshTriggeringEventListeningEnabled)
				return;

			var refreshTriggeringCtrl = this.getRefreshTriggeringCtrl();
			if(!refreshTriggeringCtrl)
				throw "ComponentParameterViewComboExpressionEditor:enableListeningToRefreshTriggerElement: refreshTriggeringCtrl is not found!";
				
			Bitrix.EventHandlingUtility.getInstance().removeEventListener(refreshTriggeringCtrl, this._refreshTriggeringCtrlEventName, this._refreshTriggerHandler);
			this._refreshTriggeringEventListeningEnabled = false;
		}
	},
	enableListeningToRefreshSourceElement: function(enable, el, eventName){
		this._ensureInitialized();
		if(!(typeof(el) == "object" && "tagName" in el))
			throw "ComponentParameterViewComboExpressionEditor.enableListeningToRefreshSourceElement: el is not valid";
		if(!Bitrix.TypeUtility.isNotEmptyString(eventName)) 
			eventName = "click";
		if(enable)
			Bitrix.EventHandlingUtility.getInstance().addEventListener(el, eventName, this._refreshSourceHandler);		
		else
			Bitrix.EventHandlingUtility.getInstance().removeEventListener(el, eventName, this._refreshSourceHandler);		
	},
	setPrefixValue: function(prefixValue){
		if(!Bitrix.TypeUtility.isString(prefixValue)) 
			prefixValue = "";
		var prefixCtrl = document.getElementById(this._expressionPrefixCtrlID);
		if(prefixCtrl) 
			prefixCtrl.value = prefixValue;		
	},
	setContentValue: function(contentValue){
		if(!Bitrix.TypeUtility.isString(contentValue)) 
			contentValue = "";			
		var contentCtrl = document.getElementById(this._expressionContentCtrlID);
		if(contentCtrl) 
			contentCtrl.value = contentValue;		
	},
	isValuesAvailable: function(){
		return document.getElementById(this._expressionPrefixCtrlID) != null && document.getElementById(this._expressionContentCtrlID) != null;
	},
	getPrefixValue: function(){
		var prefixCtrl = document.getElementById(this._expressionPrefixCtrlID); 
		return prefixCtrl ? prefixCtrl.value : null;		
	},
	getContentValue: function(){
		var contentCtrl = document.getElementById(this._expressionContentCtrlID); 
		return contentCtrl ? contentCtrl.value : null;		
	},
	getContainerVisibility: function(){
		if(this._containerVisibility == null){
			var containerElement = this.getViewContainerElement();
			if(containerElement && ("style" in containerElement))
				this._containerVisibility = containerElement.style.display != "none";
		}
		return this._containerVisibility;
	},
	setContainerVisibility: function(visibility, delay, check){
		if(!Bitrix.TypeUtility.isBoolean(visibility)) throw "ComponentParameterViewComboExpressionEditor.setContainerVisibility: visibility is not valid!";
		if(!Bitrix.TypeUtility.isBoolean(check)) throw "ComponentParameterViewComboExpressionEditor.setContainerVisibility: check is not valid!";
		if(isNaN(delay) || delay < 0) 
			delay = 1;		
			
		if(check){
			var allowedVisibility = Bitrix.ComponentParametersEditor.getInstance().evaluteVisibilityForParameterView(this._componentParameterID);
			if(visibility && !allowedVisibility)
				visibility  = allowedVisibility;
		}
		
		if(this._containerVisibility === visibility)
			return;		
		
		this._containerVisibility = visibility;
		var containerElement = this.getViewContainerElement();
		if(!containerElement || !("style" in containerElement)) 
			return;
			
		var thisView = this;
		window.setTimeout(function(){if(!visibility)containerElement.blur(); containerElement.style.display = visibility ? "" : "none"; Bitrix.ComponentParametersEditor.getInstance().handleViewVisibilityChange(thisView);}, delay, "JavaScript");				
	},
	getViewContainerElementByViewIdFunction: function(){
		return this._viewContainerElementByViewIdFunction;
	},
	setViewContainerElementByViewIdFunction: function(func){
		this._viewContainerElementByViewIdFunction = Bitrix.TypeUtility.isFunction(func) ? func : null;
		this._viewContainerElementID = null;
	},	
	getViewContainerElement: function(){
		var r = null;
		if(this._viewContainerElementID == null)
			r = this._viewContainerElementByViewIdFunction  != null ? this._viewContainerElementByViewIdFunction(this._componentParameterViewCtrlID) : document.getElementById(this._componentParameterViewCtrlID);
		else
			r = document.getElementById(this._viewContainerElementID);
		return r;
	},		
	_ensureInitialized: function(){
		if(!this._initialized) 
			throw "ComponentParameterViewComboExpressionEditor: Is not initialized!";
	}
}

Bitrix.ComponentParameterViewComboExpressionEditor.create = function(ID, componentParameterID, componentParameterViewCtrlID, expressionCtrlID, modificationModeCtrlID){
    var self = new Bitrix.ComponentParameterViewComboExpressionEditor();
	self.initialize(ID, componentParameterID, componentParameterViewCtrlID, expressionCtrlID, modificationModeCtrlID);
    return self;	
}

Bitrix.ComponentParameterViewComboExpressionEditor.createWhithControls = function(ID, componentParameterID, componentParameterViewCtrlID, controlSeedID, parentEl){
	if(!Bitrix.TypeUtility.isNotEmptyString(controlSeedID))
		throw "Bitrix.ComponentParameterViewComboExpressionEditor.createWhithControls: controlSeedID is not defined!";
	if(!parentEl)
		throw "Bitrix.ComponentParameterViewComboExpressionEditor.createWhithControls: parentEl is not defined!";
		
	var containerCtrl = document.createElement("SPAN");
	var containerCtrlID =  controlSeedID + "_ExpEd";
	containerCtrl.id = containerCtrlID;
	
	var prefixCtrl = document.createElement("INPUT");
	prefixCtrl.type = "text";
	var prefixCtrlID = controlSeedID + "_ExpEd_Pref";
	prefixCtrl.id = prefixCtrlID;
	prefixCtrl.className = "bx-expressionEditor-prefix";
	containerCtrl.appendChild(prefixCtrl);	
	
	var delimiterCtrl = document.createElement("LABEL");
	delimiterCtrl.className = "bx-expressionEditor-delimiter";
	delimiterCtrl.innerHTML = ":";
	containerCtrl.appendChild(delimiterCtrl);
	
	var contentCtrl = document.createElement("INPUT");
	contentCtrl.type = "text";
	var contentCtrlID = controlSeedID + "_ExpEd_Exp";
	contentCtrl.id = contentCtrlID;
	contentCtrl.className = "bx-expressionEditor-expression";		
	containerCtrl.appendChild(contentCtrl);	
	
	parentEl.appendChild(containerCtrl);
	
	var selectedModeCtrl = document.createElement("INPUT");
	selectedModeCtrl.type = "hidden";
	var selectedModeCtrlID = controlSeedID + "_SelectedMode";
	selectedModeCtrl.id = selectedModeCtrlID;
	
	parentEl.appendChild(selectedModeCtrl);		
	var view = this.create(ID, componentParameterID, componentParameterViewCtrlID, containerCtrlID, selectedModeCtrlID);
	view.setExpressionPrefixCtrlID(prefixCtrlID);
	view.setExpressionContentCtrlID(contentCtrlID);
	return view;
}

if (typeof (Bitrix.ComponentParameterViewComboExpressionEditor.registerClass) == "function")
	Bitrix.ComponentParameterViewComboExpressionEditor.registerClass("Bitrix.ComponentParameterViewComboExpressionEditor");

var r = { top: 0, left: 0, invert: function(){ this.top *= -1; this.left *= -1 } };
Bitrix.Point = function() {
	if (typeof (Bitrix.Point.initializeBase) == "function")	
		Bitrix.Point.initializeBase(this);
	this._top = 0;
	this._left = 0;
}

Bitrix.Point.prototype = {
	initialize: function(top, left){
		if(!isNaN(top)) this._top = top;
		if(!isNaN(left)) this._left = left;
		this._isInitialized = true;
	},
	
	getTop: function(){
		return this._top;
	},
	
	getLeft: function(){
		return this._left;
	},	
	
	setTop: function(val){
		if(isNaN(val)) throw "'" + val + "' is not a number!";
		this._top = val;
	},
	
	setLeft: function(val){
		if(isNaN(val)) throw "'" + val + "' is not a number!";
		this._left = val;
	},

	assign: function(src){
		if(typeof(src) != "object" || src == null) throw "Bitrix.Point.assign: src is not defined!";
		this._top = src._top;
		this._left = src._left;
	},
	
	equals: function(src){
		if(typeof(src) != "object" || src == null) throw "Bitrix.Point.equals: src is not defined!";
		return this._top == src._top && this._left == src._left;
	},
	
	invert: function(){ 
		this._top *= -1; 
		this._left *= -1;
	},
	
	add: function(src){
		if(typeof(src) != "object" || src == null) throw "Bitrix.Point.equals: src is not defined!";
		return Bitrix.Point.create(this._top + src._top, this._left + src._left);
	}
}

Bitrix.Point.create = function(top, left){
	var self = new Bitrix.Point();
	self.initialize(top, left);
	return self;
}

Bitrix.EventHandlingUtility = function() {
	if (typeof (Bitrix.EventHandlingUtility.initializeBase) == "function")
		Bitrix.EventHandlingUtility.initializeBase(this);
}	

Bitrix.EventHandlingUtility.prototype = {
	initialize: function(){
		this._isInitialized = true;
	},
    addEventListener: function(element, eventName, listener){
		if(typeof(element) != "object" || element == null) throw "element is not specified";
		
		if(typeof(element.attachEvent) != "undefined") // IE
			element.attachEvent("on" + eventName, listener);
		else if(typeof(element.addEventListener) != "undefined") //W3C
			element.addEventListener(eventName, listener, false);
		else
			element["on" + eventName] = listener;	    
	},
	removeEventListener: function(element, eventName, listener){
		if(typeof(element) != "object" || element == null) throw "element is not specified";
		
		if(typeof(element.detachEvent) != "undefined") // IE
			element.detachEvent("on" + eventName, listener);
		else if(typeof(element.addEventListener) != "undefined") //W3C
			element.removeEventListener(eventName, listener, false);
		else
			delete element["on" + eventName];			
	},
	stopEventBubbling: function(e){
		if(typeof(e) == "object" && e != null && typeof(e.stopPropagation) == "function") //W3C
			e.stopPropagation();
		else if(window.event)
			window.event.cancelBubble = true; //IE
	}
}

Bitrix.EventHandlingUtility._instance = null;
Bitrix.EventHandlingUtility.getInstance = function(){
	if(this._instance == null){
		this._instance = new Bitrix.EventHandlingUtility();
		this._instance.initialize();
	}
	return this._instance;
}

if (typeof (Bitrix.EventHandlingUtility.registerClass) == "function")
	Bitrix.EventHandlingUtility.registerClass("Bitrix.EventHandlingUtility");

Bitrix.ElementHighlighter = function() {
	if (typeof (Bitrix.ElementHighlighter.initializeBase) == "function")
		Bitrix.ElementHighlighter.initializeBase(this);
	this._elID = null;
	this._elementsCreated = false;
	this._borderTopElID = null;
	this._borderRightElID = null;
	this._borderBottomElID = null;
	this._borderLeftElID = null;
	this._isInitialized = false;	
	this._mouseOverHandler = BX.delegate(this._handleMouseOver, this);
	this._mouseOutHandler = BX.delegate(this._handleMouseOut, this);
	this._windowScrollHandler = BX.delegate(this._handleWindowScroll, this);
	this._useComplexLayout = false;
	this._visible = false;
}

Bitrix.ElementHighlighter.prototype = {
    initialize: function(elID, useComplexLayout){
		if(typeof(elID) != "string" || elID.length == 0) throw "el is not specified";
		this._elID = elID;        
		
		if(typeof(useComplexLayout) == "boolean")
			this._useComplexLayout = useComplexLayout;
		
		var el = this._getElement();		
		Bitrix.EventHandlingUtility.getInstance().addEventListener(el, "mouseover", this._mouseOverHandler);	
		Bitrix.EventHandlingUtility.getInstance().addEventListener(el, "mouseout", this._mouseOutHandler);	
		//this._addEventListener(document.body, "scroll", this._windowScrollHandler);	
		
		Bitrix.ElementHighlighter._entries[this.getID()] = this;
        this._isInitialized = true;
    },
	
	getID: function(){
		return Bitrix.ElementHighlighter.getEntryIdByElementId(this._elID);
	},
	
	_getBorderTopElementID: function(){
		return this._elID + "_HighlighterBorderTop";
	},
	
	_getBorderRightElementID: function(){
		return this._elID + "_HighlighterBorderRight";
	},
		
	_getBorderBottomElementID: function(){
		return this._elID + "_HighlighterBorderBottom";
	},
		
	_getBorderLeftElementID: function(){
		return this._elID + "_HighlighterBorderLeft";
	},
				
	_getElementById: function(id){
		this._ensureElementsCreated();
		var r = document.getElementById(id);
		if(r == null) throw "Highlighter: Could not find element '" + id + "'!";
		return r;
	},
	
	getUseComplexLayout: function(){
		return this._useComplexLayout;
	},
	
	setUseComplexLayout: function(val){
		if(typeof(val) != "boolean") throw "'" + val + "' is not valid boolean value!";
		this._useComplexLayout = val;
	},
	
	_ensureElementsCreated: function(){
		if(this._elementsCreated) return;
		
		
		var containerID  = this._elID + "_HighlighterContainer";
		var container = document.getElementById(containerID);
		if(container != null) throw "Highlighter: Can't create elements. Element '" + containerID + "' already exists!";
		container = document.createElement("DIV");
		document.body.appendChild(container);
		container.id = containerID;
		container.className = "highlightContainer";
		
		var borderTopID = this._getBorderTopElementID();
		var borderTop = document.getElementById(borderTopID);
		if(borderTop != null) throw "Highlighter: Can't create elements. Element '" + borderTopID + "' already exists!";
		borderTop = document.createElement("DIV");
		document.body.appendChild(borderTop);			
		borderTop.id = borderTopID;
		borderTop.className = this._useComplexLayout ? "highlight highlight-tb-complex" : "highlight highlight-tb";
		borderTop.innerHTML = "&nbsp";
		
		var borderRightID = this._getBorderRightElementID();
		var borderRight = document.getElementById(borderRightID);
		if(borderRight != null) throw "Highlighter: Can't create elements. Element '" + borderRightID + "' already exists!";
		borderRight = document.createElement("DIV");
		document.body.appendChild(borderRight);			
		borderRight.id = borderRightID;
		borderRight.className = this._useComplexLayout ? "highlight highlight-lr-complex" : "highlight highlight-lr";
		borderRight.innerHTML = "&nbsp";		
		
		var borderBottomID = this._getBorderBottomElementID();
		var borderBottom = document.getElementById(borderBottomID);
		if(borderBottom != null) throw "Highlighter: Can't create elements. Element '" + borderBottomID + "' already exists!";
		borderBottom = document.createElement("DIV");
		document.body.appendChild(borderBottom);			
		borderBottom.id = borderBottomID;
		borderBottom.className = this._useComplexLayout ? "highlight highlight-tb-complex" : "highlight highlight-tb";
		borderBottom.innerHTML = "&nbsp";		
		
		var borderLeftID = this._getBorderLeftElementID();
		var borderLeft = document.getElementById(borderLeftID);
		if(borderLeft != null) throw "Highlighter: Can't create elements. Element '" + borderLeftID + "' already exists!";
		borderLeft = document.createElement("DIV");
		document.body.appendChild(borderLeft);			
		borderLeft.id = borderLeftID;
		borderLeft.className = this._useComplexLayout ? "highlight highlight-lr-complex" : "highlight highlight-lr";
		borderLeft.innerHTML = "&nbsp";			
		
		this._elementsCreated = true;
	},
	
	_getElement: function(){
		var r = document.getElementById(this._elID);
		if(r == null) throw "Highlighter: Document element with ID = '" + this._elID + "' is not found!";		
		return r;
	},
	
	_getElementRect : function(){
		var el = this._getElement();
		return Bitrix.ElementPositioningUtility.getInstance().getElementRect(el);
	},	
	
	_getElementParentScroll: function(){
		var el = this._getElement();

		var r = { top: 0, left: 0 };
		if(typeof(el.parentNode) == "undefined") return r;
		
		var x=0, y=0;
		while((el = el.parentNode) != null){
			if(el.tagName && el.tagName.toUpperCase() == "BODY") break;
			var scrollTop = typeof(el.scrollTop) != "undefined" ? el.scrollTop : 0;
			if(!isNaN(scrollTop) && scrollTop > 0 )
			   y += scrollTop;			
			var scrollLeft = typeof(el.scrollLeft) != "undefined" ? el.scrollLeft : 0;
			if(!isNaN(scrollLeft) && scrollLeft > 0 )
			   x += scrollLeft;     
		}

		r.top = y;
		r.left = x;		
		return r;
	},
			
    _handleMouseOver: function(e){		
		e = e || window.event;	
		this.show();
		if(Bitrix.ElementHighlighter.getDebug()){
			if(typeof(console) != "undefined" && typeof(console.info) != "undefined")
				console.info("Highlighter._handleMouseOver: %s", this._elID);	
		}		
		Bitrix.EventHandlingUtility.getInstance().stopEventBubbling(e);
    },
    
    show: function(){		
		if(Bitrix.ElementHighlighter.getShowSingle())
			for(var entryID in Bitrix.ElementHighlighter._entries){
				if(entryID == this.getID()) continue;
				Bitrix.ElementHighlighter._entries[entryID].hide();
			}
		
		var clientRect = this._getElementRect();
		var scroll = this._getElementParentScroll();
		
		var top = this._getElementById(this._getBorderTopElementID());
		var right = this._getElementById(this._getBorderRightElementID());
		var bottom = this._getElementById(this._getBorderBottomElementID());
		var left = this._getElementById(this._getBorderLeftElementID());		
		
		//top.style.width = parseInt(clientRect.right - clientRect.left) + "px";
		top.style.width = parseInt(clientRect.width) + "px";
		top.style.left = parseInt(clientRect.left - scroll.left) + "px";
		top.style.top = parseInt(clientRect.top - scroll.top) + "px";
		//top.style.top ="0px";
		top.style.display = "block";
		
		right.style.height = parseInt(clientRect.bottom - clientRect.top) + "px";
		right.style.left = parseInt(clientRect.right - 1 - scroll.left) + "px";
		right.style.top = parseInt(clientRect.top - scroll.top) + "px";
		right.style.display = "block";	
		
		//bottom.style.width = parseInt(clientRect.right - clientRect.left) + "px";
		bottom.style.width = parseInt(clientRect.width) + "px";
		bottom.style.left = parseInt(clientRect.left - scroll.left) + "px";
		bottom.style.top = parseInt(clientRect.bottom - 1 - scroll.top) + "px";
		
		bottom.style.display = "block";
		
		//left.style.height = parseInt(clientRect.bottom - clientRect.top) + "px";
		left.style.height = parseInt(clientRect.height) + "px";
		left.style.left = parseInt(clientRect.left - scroll.left) + "px";
		left.style.top = parseInt(clientRect.top - scroll.top) + "px";
		
		left.style.display = "block";			
		
		if(!this._visible) 
			this._visible = true;    
    },
    
    _handleMouseOut: function(e){
		if(!this._visible) return;

		e = e || window.event;	
		this.hide();
		//this._stopEventBubbling(e);		
		if(Bitrix.ElementHighlighter.getDebug()){
			if(typeof(console) != "undefined" && typeof(console.info) != "undefined")
				console.info("Highlighter._handleMouseOut: %s", this._elID);	
		}
    },
    
    hide: function(){
		if(!this._visible) return;
		
		var top = this._getElementById(this._getBorderTopElementID());
		var right = this._getElementById(this._getBorderRightElementID());
		var bottom = this._getElementById(this._getBorderBottomElementID());
		var left = this._getElementById(this._getBorderLeftElementID());		
			
		top.style.display = "none";
		right.style.display = "none";
		bottom.style.display = "none";
		left.style.display = "none";
			
		this._visible = false;    
    },
    
    _handleWindowScroll: function(e){	
		if(!this._visible) return;
		e = e || window.event;	
		this.show();		
		//this._stopEventBubbling(e);
    }, 
    
    getVisible: function(){
		return this._visible;
    }       
}

Bitrix.ElementHighlighter._entries = new Object();
Bitrix.ElementHighlighter._showSingle = true;
Bitrix.ElementHighlighter.getShowSingle = function(){
	return Bitrix.ElementHighlighter._showSingle;
}
Bitrix.ElementHighlighter.setShowSingle = function(val){
	Bitrix.ElementHighlighter._showSingle = val;
}

Bitrix.ElementHighlighter._debug = false;
Bitrix.ElementHighlighter.getDebug = function(){
	return Bitrix.ElementHighlighter._debug;
}
Bitrix.ElementHighlighter.setDebug = function(val){
	Bitrix.ElementHighlighter._debug = val;
}

Bitrix.ElementHighlighter.getEntryIdByElementId = function(elID){
	return typeof(elID) == "string" && elID.length > 0 ? elID + "_Highlighter" : "";
}

Bitrix.ElementHighlighter.getEntryById = function(entryID){
	return (typeof(entryID) == "string" && entryID.length > 0 && typeof(this._entries[entryID]) == "object") ? this._entries[entryID] : null;
}

Bitrix.ElementHighlighter.create = function(elID, useComplexLayout){
	var self = new Bitrix.ElementHighlighter();
	try{
		self.initialize(elID, useComplexLayout);
	}
	catch(e){
		return null;
	}
	return self;
}

if (typeof (Bitrix.ElementHighlighter.registerClass) == "function")
	Bitrix.ElementHighlighter.registerClass("Bitrix.ElementHighlighter");


Bitrix.ComponentPanel = function() {
	if (typeof (Bitrix.ComponentPanel.initializeBase) == "function")
		Bitrix.ComponentPanel.initializeBase(this);
	this._panelElemID = null;
	this._componentElemID = null;
	this._mouseOverHandler = BX.delegate(this._handleMouseOver, this);
	this._mouseOutHandler = BX.delegate(this._handleMouseOut, this);
	this._panelElemPosition = Bitrix.Point.create(0, 0);
}
Bitrix.ComponentPanel.prototype = {
	initialize: function(panelElemID, componentElemID){
		if(typeof(panelElemID) != "string" || panelElemID.length == 0) throw "Bitrix.ComponentPanel.initialize: panel element is not specified!";
		if(typeof(componentElemID) != "string" || componentElemID.length == 0) throw "Bitrix.ComponentPanel.initialize: component element is not specified!";
				
		this._panelElemID = panelElemID;
		this._componentElemID = componentElemID;
		
		var panel = this._getPanelElement();		
		Bitrix.EventHandlingUtility.getInstance().addEventListener(panel, "mouseover", this._mouseOverHandler);	
		Bitrix.EventHandlingUtility.getInstance().addEventListener(panel, "mouseout", this._mouseOutHandler);			
		
		Bitrix.ComponentPanel._entries[this.getID()] = this;		
		this._isInitialized = true;
	},
	
	getID: function(){
		return this._panelElemID + "_ComponentPanel";
	},
		
	_getPanelElement: function(){
		var r = document.getElementById(this._panelElemID);
		if(r == null) throw "Bitrix.ComponentPanel._getPanelElement: document element with ID = '" + this._panelElemID + "' is not found!";		
		return r;
	},	
	
	_getPanelElementPosition: function(){
		return this._panelElemPosition
	},
	
	_getComponentElement: function(){
		var r = document.getElementById(this._componentElemID);
		if(r == null) throw "Bitrix.ComponentPanel._getComponentElement: document element with ID = '" + this._componentElemID + "' is not found!";		
		return r;
	},	
	
	show: function(offset){
		if(typeof(offset) != "object" || offset == null)
			offset = Bitrix.Point.create(0, 0);

		var panel = this._getPanelElement();
		var component = this._getComponentElement();
		
		var panelOffset = Bitrix.ElementPositioningUtility.getInstance().getOffset(panel, component);
		panelOffset.invert();		
		var position = panelOffset.add(offset);
		
		if(Bitrix.ComponentPanel.getPreventEntriesOverlay()){
			for(var curStep = 0; curStep < 9 && Bitrix.ComponentPanel.findEntryWithPosition(position) != null; curStep++)
				position = position.add(Bitrix.Point.create(20, 7));
		}
		panel.style.top = parseInt(position.getTop()) + "px";
		panel.style.left = parseInt(position.getLeft()) + "px";
		
		this._panelElemPosition = position;
		
		panel.style.display = "block";
	},
	
    _handleMouseOver: function(e){		
		e = e || window.event;	
		var hightlighter = Bitrix.ElementHighlighter.getEntryById(Bitrix.ElementHighlighter.getEntryIdByElementId(this._componentElemID));
		if(hightlighter)
			hightlighter.show();
		Bitrix.EventHandlingUtility.getInstance().stopEventBubbling(e);
    },	
    
    _handleMouseOut: function(e){
		var hightlighter = Bitrix.ElementHighlighter.getEntryById(Bitrix.ElementHighlighter.getEntryIdByElementId(this._componentElemID));
		if(hightlighter)
			hightlighter.hide();
    }    
}

Bitrix.ComponentPanel._entries = new Object();
Bitrix.ComponentPanel._panelElemSize = { width:50, height:27 };
Bitrix.ComponentPanel._preventEntriesOverlay = true;

Bitrix.ComponentPanel.getPreventEntriesOverlay = function(){
	return this._preventEntriesOverlay;
}
Bitrix.ComponentPanel.setPreventEntriesOverlay = function(val){
	this._preventEntriesOverlay = val;
}

Bitrix.ComponentPanel.findEntryWithPosition = function(pos){
	if(typeof(pos) != "object" || pos == null) throw "Bitrix.ComponentPanel.findEntryWithPosition: pos is not specified!";
	for(var entryID in this._entries){
		var entry = this._entries[entryID];
		entryPos = entry._getPanelElementPosition();
		if(pos.equals(entryPos))
			return entry;
	}
	return null;
}

Bitrix.ComponentPanel.create = function(panelElemID, componentElemID, initialOffset){
	var self = new Bitrix.ComponentPanel();
	try{
		self.initialize(panelElemID, componentElemID);
		
		if(typeof(initialOffset) != "object" || initialOffset == null)
			initialOffset = Bitrix.Point.create(-1 * this._panelElemSize.height, 0);
		self.show(initialOffset);
	}
	catch(e){
		//remove panel element
		var panel = null;
		if(typeof(panelElemID) == "string" && panelElemID.length > 0)
			panel = document.getElementById(panelElemID);
			
		var component = null;
		if(typeof(componentElemID) == "string" && componentElemID.length > 0)
			component = document.getElementById(componentElemID);

		if(panel != null && typeof(panel.parentNode) != "undefined" && panel.parentNode != null)
			panel.parentNode.removeChild(panel);					
		return null;
	}
	return self;
}

if (typeof (Bitrix.ComponentPanel.registerClass) == "function")
	Bitrix.ComponentPanel.registerClass("Bitrix.ComponentPanel");

Bitrix.ElementGroup = function() {
	if (typeof (Bitrix.ElementGroup.initializeBase) == "function")
		Bitrix.ElementGroup.initializeBase(this);
	this._name = null;
	this._visibility = true;
	this._itemIdArr = new Array();
	this._itemElementByIdFunction = null;
}

Bitrix.ElementGroup.prototype = {
	initialize: function(name, visibility){
		if(!Bitrix.TypeUtility.isNotEmptyString(name)) throw "Bitrix.ElementGroup.initialize: name is not valid!"; 
		this._name = name;
		if(Bitrix.TypeUtility.isBoolean(visibility))
			this._visibility = visibility;
		this._isInitialized = true;
	},
	getName: function(){
		return this._name;
	},
	_getItemElement: function(id){
		if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Bitrix.ElementGroup._getItemElement: id is not valid!";
		return this._itemElementByIdFunction  != null ? this._itemElementByIdFunction(id) : document.getElementById(id);
	},
	_adjustItemVisibility: function(id, delay){
		if(!delay || delay < 0)
			delay = 1;
		if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Bitrix.ElementGroup._adjustItemVisibility: id is not valid!";
		var el = this._getItemElement(id);
		if(!el) return;
		var elVisibility = this._visibility;
		window.setTimeout(function(){if(!elVisibility)el.blur(); el.style.display = elVisibility ? "" : "none";}, delay, "JavaScript");
		//el.style.display = this._visibility ? "" : "none";
	},
	
	_adjustItemsVisibility: function(){
		var showDelay = 100;
		if(this._visibility){
			for(var i = 0; i < this._itemIdArr.length; i++){
				this._adjustItemVisibility(this._itemIdArr[i], showDelay);	
				showDelay += 120;			
			}
		}
		else{
			for(var i = 0; i < this._itemIdArr.length; i++)
				this._adjustItemVisibility(this._itemIdArr[i]);
		}
	},	
	
	addItemId: function(id){
		if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Bitrix.ElementGroup.addItemId: id is not valid!";
		for(var i = 0; i < this._itemIdArr.length; i++){
			if(this._itemIdArr[i] == id) throw "Bitrix.ElementGroup.addItemId: itemId '" + id + "' already exists!";
		}		
		this._itemIdArr.push(id);
		this._adjustItemVisibility(id);
	},
	removeItemId: function(id){
		if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Bitrix.ElementGroup.removeItemId: id is not valid!";	
		for(var i = 0; i < this._itemIdArr.length; i++){
			if(this._itemIdArr[i] != id) continue;
			this._itemIdArr.splice(i, 1);
			break;
		}
	},
	ensureItemIdPresent: function(id){
		if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Bitrix.ElementGroup.ensureItemIdPresent: id is not valid!";
		for(var i = 0; i < this._itemIdArr.length; i++)
			if(this._itemIdArr[i] == id) return;		
		this._itemIdArr.push(id);
		this._adjustItemVisibility(id);
	},	
	
	setItemsVisibility: function(visibility){
		if(!Bitrix.TypeUtility.isBoolean(visibility)) throw "Bitrix.ElementGroup.setItemsVisibility: visibility is not valid!";	
		if(this._visibility == visibility) return;
		this._visibility = visibility;
		this._adjustItemsVisibility();
	},
	getItemElementByIdFunction: function(){
		return this._itemElementByIdFunction;
	},
	setItemElementByIdFunction: function(func){
		this._itemElementByIdFunction = Bitrix.TypeUtility.isFunction(func) ? func : null;
		this._adjustItemsVisibility();
	}	
}

Bitrix.ElementGroup.create = function(name, visibility){
	var self = new Bitrix.ElementGroup();
	self.initialize(name, visibility);
	return self;
}

if (typeof (Bitrix.ElementGroup.registerClass) == "function")
	Bitrix.ElementGroup.registerClass("Bitrix.ElementGroup");

Bitrix.ParamClientSideActionGroupViewMemberGroup = function() {
	if (typeof (Bitrix.ParamClientSideActionGroupViewMemberGroup.initializeBase) == "function")
		Bitrix.ParamClientSideActionGroupViewMemberGroup.initializeBase(this);
	this._initialized = false;
	this._name = null;
	this._visibility = true;	
	this._members = null;
}

Bitrix.ParamClientSideActionGroupViewMemberGroup.prototype = {
	initialize: function(name, visibility){
		if(this._initialized) return;
		if(!Bitrix.TypeUtility.isNotEmptyString(name)) throw "Bitrix.ParamClientSideActionGroupViewMemberGroup.initialize: name is not valid!"; 
		this._name = name;
		if(Bitrix.TypeUtility.isBoolean(visibility))
			this._visibility = visibility;
		this._initialized = true;
	},
	getName: function(){
		return this._name;
	},
	getVisibility: function(){
		return this._visibility;
	},
	setVisibility: function(visibility){
		if(!Bitrix.TypeUtility.isBoolean(visibility))
			throw "Bitrix.ParamClientSideActionGroupViewMemberGroup.setVisibility: visibility is not specified!";
		if(this._visibility == visibility) return;
		
		this._visibility = visibility;
		this._adjustMembersVisibility();
	},	
	findMemberIndex: function(member){
		if(!(member instanceof Bitrix.ParamClientSideActionGroupViewMember)) throw "Bitrix.ParamClientSideActionGroupViewMemberGroup.isMemberPresent: member is not specified!";
		
		if(this._members == null || this._members.length == 0)
			return -1;
			
		for(var i = 0; i < this._members.length; i++){
			if(this._members[i] !== member) continue;
			return i;
		}	
		return -1;
	},	
	addMember: function(member){
		if(!(member instanceof Bitrix.ParamClientSideActionGroupViewMember)) throw "Bitrix.ParamClientSideActionGroupViewMemberGroup.addMember: member is not specified!";
		
		var memberIndex = this.findMemberIndex(member);
		if(memberIndex >= 0) 
			return;
		
		if(this._members == null)
			this._members = new Array();
			
		this._members.push(member);	
		member.registerInGroup(this);
	},
	removeMember: function(member){
		if(!(member instanceof Bitrix.ParamClientSideActionGroupViewMember)) throw "Bitrix.ParamClientSideActionGroupViewMemberGroup.removeMember: member is not specified!";	
		var memberIndex = this.findMemberIndex(member);
		if(memberIndex < 0) 
			return;
			
		this._members.splice(memberIndex, 1);
		member.unregisterFromGroup(this);
	},
	_adjustMembersVisibility: function(){
		if(this._members == null || this._members.length == 0)
			return;
		var showDelay = 100;
		if(this._visibility){
			for(var i = 0; i < this._members.length; i++){
				if(this._members[i].adjustVisibility(showDelay))	
					showDelay += 120;			
			}
		}
		else{
			for(var i = 0; i < this._members.length; i++)
				this._members[i].adjustVisibility();
		}
	}
}

Bitrix.ParamClientSideActionGroupViewMemberGroup.create = function(name, visibility){
	var self = new Bitrix.ParamClientSideActionGroupViewMemberGroup();
	self.initialize(name, visibility);
	return self;
}

Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition = function() {
	if (typeof (Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition.initializeBase) == "function")	
		Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition.initializeBase(this);
	this._initialized = false;
	this._value = null;
}

Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition.prototype = {
	initialize: function(value){
		if(!Bitrix.EnumHelper.checkPresenceById(Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition._values, value))		
			throw "Value '" + value + "' is unknown!";
		this._value = value;
		
		this._initialized = true;
	},
	getValue: function(){ return this._value; },
	getName: function(){
		return Bitrix.EnumHelper.getName(Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition._values, this._value);
	}
}

Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition._values = { or : 1, and: 2};
Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition.values = function(){ return this._values; }

Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition._or = null;
Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition.or = function(){
	if(Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition._or == null){
		Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition._or = new Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition();
		Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition._or.initialize(Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition._values.or);
	}
	return Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition._or;
}

Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition._and = null;
Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition.and = function(){
	if(Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition._and == null){
		Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition._and = new Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition();
		Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition._and.initialize(Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition._values.and);
	}
	return Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition._and;
}


Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition.getByName = function(name){
	var value = Bitrix.EnumHelper.getId(Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition._values, name);
	if(!(typeof(value) == "number" || value instanceof Number)) throw "Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition.getByName: value is not found!";
	
	var or = Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition.or();
	if(value == or.getValue())
		return or;
		
	var and = Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition.and();
	if(value == and.getValue())
		return and;
		
	throw "Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition.getByName: value is not supported: " + value + " !";	
}

if (typeof (Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition.registerClass) == "function")
	Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition.registerClass("Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition");

Bitrix.ParamClientSideActionGroupViewMember = function() {
	if (typeof (Bitrix.ParamClientSideActionGroupViewMember.initializeBase) == "function")		
		Bitrix.ParamClientSideActionGroupViewMember.initializeBase(this);
	this._initialized = false;
	this._id = null;
	this._parameterId = null;
	this._controlId = null;
	this._visibility = null;
	this._groups = null;
	this._displayCondition = null;
	//this._elementByIdFunction = null;
}

Bitrix.ParamClientSideActionGroupViewMember.prototype = {
	initialize: function(id, parameterId, controlId, displayCondition){
		this.setId(id);
		this.setParameterId(parameterId);
		this.setControlId(controlId);
		if(!(displayCondition instanceof Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition))
			displayCondition = Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition.or();
		this.setDisplayCondition(displayCondition);
		this._initialized = true;
	},
	
	getId: function(){
		return this._id;
	},
	setId: function(id){
		if(!Bitrix.TypeUtility.isString(id)) throw "Bitrix.ParamClientSideActionGroupViewMember.setId: id is not valid!";
		this._id = id;
	},
	getParameterId: function(){
		return this._parameterId;
	},
	setParameterId: function(id){
		if(!Bitrix.TypeUtility.isString(id)) throw "Bitrix.ParamClientSideActionGroupViewMember.setParameterId: id is not valid!";
		this._parameterId = id;
	},		
	getControlId: function(){
		return this._controlId;
	},
	setControlId: function(id){
		if(!Bitrix.TypeUtility.isString(id)) throw "Bitrix.ParamClientSideActionGroupViewMember.setControlId: id is not valid!";
		this._controlId = id;
	},
	getVisibility: function(){
		if(this._visibility == null)
			this._visibility = this.evaluteVisibility();
		return this._visibility;
	},
	evaluteVisibility: function(){
		var groupCount = this._groups != null ? this._groups.length : 0;
		if(groupCount == 0)
			return true;
			
		if(this._displayCondition === Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition.or()){
			var visibility = false;
			for(var i = 0; i < groupCount; i++){
				if(!this._groups[i].getVisibility())
					continue;
				visibility = true;
				break;
			}
			return visibility;
		}
		else if(this._displayCondition === Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition.and()){
			var visibility = true;
			for(var i = 0; i < groupCount; i++){
				if(this._groups[i].getVisibility())
					continue;
				visibility = false;
				break;
			}
			return visibility;
		}
		throw "Bitrix.ParamClientSideActionGroupViewMember.adjustVisibility: unknown displayCondition value '"+ this._displayCondition.getValue() +"'!";	
	},
	
	getDisplayCondition: function(){
		return this._displayCondition;
	},
	setDisplayCondition: function(value){
		if(!(value instanceof Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition)) throw "Bitrix.ParamClientSideActionGroupViewMember.setDisplayCondition: value is not specified!";
		this._displayCondition = value;
	},
	/*
	getElementByIdFunction: function(){
	//	return this._elementByIdFunction;
	},
	setElementByIdFunction: function(func){
		this._elementByIdFunction = Bitrix.TypeUtility.isFunction(func) ? func : null;
		this.adjustVisibility();
	},	
	_getElement: function(){
		return this._elementByIdFunction  != null ? this._elementByIdFunction(this._controlId) : document.getElementById(this._controlId);
	},	
	*/
	adjustVisibility: function(delay){
		if(isNaN(delay) || delay < 0) 
			delay = 1;
		
		//var el = this._getElement();
		//if(!el) return false;
		var paramViews = null;
		if(Bitrix.TypeUtility.isNotEmptyString(this._parameterId))
			paramViews = Bitrix.ComponentParametersEditor.getInstance().getViewsByComponentParameterID(this._parameterId);
		if(paramViews == null || !(paramViews instanceof Array) || paramViews.length == 0)
			return false;
		var paramView = paramViews[0];
		if(!paramView)
			return false;
		
		var visibility = this.evaluteVisibility();		
		if(visibility === this._visibility)
			return false;
			
		this._visibility = visibility;
		
		paramView.setContainerVisibility(visibility, delay, false);
		return true;
	},	
	registerInGroup: function(group){
		if(!(group instanceof Bitrix.ParamClientSideActionGroupViewMemberGroup)) throw "Bitrix.ParamClientSideActionGroupViewMember.registerInGroup: group is not specified!";
		
		if(this._groups == null)
			this._groups = new Array();
			
		this._groups.push(group);
		this.adjustVisibility();
	},
	
	unregisterFromGroup: function(group){
		if(!(group instanceof Bitrix.ParamClientSideActionGroupViewMemberGroup)) throw "Bitrix.ParamClientSideActionGroupViewMember.unregisterFromGroup: group is not specified!";
		
		if(this._groups == null || this._groups.length == 0)
			return;
			
		for(var i = 0; i < this._groups.length; i++){
			if(this._groups[i] !== group) continue;
			this._groups.splice(i, 1);
			this.adjustVisibility();
			return;
		}
	}
}

Bitrix.ParamClientSideActionGroupViewMember.create = function(id, parameterId, controlId, displayCondition)
{
	var self = new Bitrix.ParamClientSideActionGroupViewMember();
	self.initialize(id, parameterId, controlId, displayCondition);
	return self;
}

if (typeof (Bitrix.ParamClientSideActionGroupViewMember.registerClass) == "function")
	Bitrix.ParamClientSideActionGroupViewMember.registerClass("Bitrix.ParamClientSideActionGroupViewMember");

Bitrix.ParamClientSideActionGroupView = function() {
	if (typeof (Bitrix.ParamClientSideActionGroupView.initializeBase) == "function")			
		Bitrix.ParamClientSideActionGroupView.initializeBase(this);
	this._id = "";
	this._groupArr = null;
	this._memberArr = null;
	this._singleGroupShow = false;
	this._itemElementByIdFunction = null;
}

Bitrix.ParamClientSideActionGroupView.prototype = {
	initialize: function(id){
		if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Bitrix.ParamClientSideActionGroupView.initialize: id is not valid!";	
		this._id = id;
		this._groupArr = new Array();
		this._memberArr = new Array();
		this._contextId = "";
		Bitrix.ParamClientSideActionGroupView._entries[this._id] = this;
		this._isInitialized = true;
	},
	getId: function(){
		return this._id;
	},
	getContextId: function(){
		return this._contextId;
	},
	setContextId: function(val){
		if(!Bitrix.TypeUtility.isNotEmptyString(val))
			throw "ParamClientSideActionGroupView.setContextId: value is not valid!";
		this._contextId = val;
	},
	getSingleGroupShow: function(){
		return this._singleGroupShow;
	},
	setSingleGroupShow: function(singleGroupShow){
		if(!Bitrix.TypeUtility.isBoolean(singleGroupShow)) throw "Bitrix.ParamClientSideActionGroupView.setSingleGroupShow: singleGroupShow is not valid!";	
		this._singleGroupShow = singleGroupShow;
	},
	/*
	getItemElementByIdFunction: function(){
		return this._itemElementByIdFunction;
	},
	setItemElementByIdFunction: function(func){
		this._itemElementByIdFunction = Bitrix.TypeUtility.isFunction(func) ? func : null;
		//for(var i = 0; i < this._groupArr.length; i++){
		//	this._groupArr[i].setItemElementByIdFunction(this._itemElementByIdFunction);
		//}	
		for(var i = 0; i < this._memberArr.length; i++){
			this._memberArr[i].setElementByIdFunction(this._itemElementByIdFunction);
		}					
	},
	setItemElementByIdFunctionIfNull: function(func){
		if(this._itemElementByIdFunction != null)
			return;
		this.setItemElementByIdFunction(func);
	},
	*/
	getGroup: function(groupName){
		if(!Bitrix.TypeUtility.isNotEmptyString(groupName)) throw "Bitrix.ParamClientSideActionGroupView.getGroup: groupName is not valid!";
		for(var i = 0; i < this._groupArr.length; i++){
			if(this._groupArr[i].getName() == groupName) return this._groupArr[i];
		}
		return null;
	},
	addGroup: function(groupName, visibility){
		if(!Bitrix.TypeUtility.isNotEmptyString(groupName)) throw "Bitrix.ParamClientSideActionGroupView.addGroup: groupName is not valid!";	
		if(this.getGroup(groupName) != null) throw "Bitrix.ParamClientSideActionGroupView.addGroup: group '" + groupName + "' already exists!";
		var group = Bitrix.ElementGroup.create(groupName, visibility); 
		//group.setItemElementByIdFunction(this._itemElementByIdFunction);
		this._groupArr.push(group);
	},
	removeGroup: function(groupName){
		if(!Bitrix.TypeUtility.isNotEmptyString(groupName)) throw "Bitrix.ParamClientSideActionGroupView.removeGroup: groupName is not valid!";		
		for(var i = 0; i < this._groupArr.length; i++){
			if(this._groupArr[i].getName() != groupName) continue;
			this._groupArr.splice(i, 1);
			break;
		}		
	},
	ensureGroupCreated: function(groupName, visibility){
		if(!Bitrix.TypeUtility.isNotEmptyString(groupName)) throw "Bitrix.ParamClientSideActionGroupView.ensureGroupCreated: groupName is not valid!";	
		var r = this.getGroup(groupName);
		if(r == null){
			r = Bitrix.ParamClientSideActionGroupViewMemberGroup.create(groupName, visibility);
			var length = this._groupArr.push(r);	
		}
		else if(Bitrix.TypeUtility.isBoolean(visibility))
			r.setVisibility(visibility);

		return r;
	},
	ensureGroupsCreated: function(groupNames, visibility){	
		if(!(groupNames instanceof Array)) throw "Bitrix.ParamClientSideActionGroupView.ensureGroupsCreated: groupNames is not specified!";
		for(var i = 0; i < groupNames.length; i++)
			this.ensureGroupCreated(groupNames[i], visibility);
	},
	getMember: function(id){
		if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Bitrix.ParamClientSideActionGroupView.getMember: id is not valid!";
		for(var i = 0; i < this._memberArr.length; i++){
			if(this._memberArr[i].getId() == id) return this._memberArr[i];
		}
		return null;
	},	
	getMembersByParameterId: function(id){
		if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Bitrix.ParamClientSideActionGroupView.getMembersByParameterId: id is not valid!";
		var r = new Array();
		if(this._memberArr != null)
			for(var i = 0; i < this._memberArr.length; i++){
				var member = this._memberArr[i];
				if(!(member instanceof Bitrix.ParamClientSideActionGroupViewMember) || member.getParameterId() != id)
					continue;
				r.push(member);
			}
		return r;
	},		
	ensureMemberCreated: function(id, parameterId, controlId, displayCondition, groupNames){
		if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Bitrix.ParamClientSideActionGroupView.ensureMemberCreated: id is not valid!";	
		if(!(displayCondition instanceof Bitrix.ParamClientSideActionGroupViewMemberDisplayCondition)) throw "Bitrix.ParamClientSideActionGroupView.ensureMemberCreated: displayCondition is not specified!";		
		if(!(groupNames instanceof Array)) throw "Bitrix.ParamClientSideActionGroupView.ensureMemberCreated: groupNames is not specified!";		
		
		var r = this.getMember(id);
		if(r == null){
			r = Bitrix.ParamClientSideActionGroupViewMember.create(id, parameterId, controlId, displayCondition);
			//r.setElementByIdFunction(this._itemElementByIdFunction);
			this._memberArr.push(r);			
		}
		else{
			r.setParameterId(parameterId);
			r.setControlId(controlId);
			r.setDisplayCondition(displayCondition);
		}
		
		for(var i = 0; i < groupNames.length; i++)
			this.ensureGroupCreated(groupNames[i]).addMember(r);

		return r;
	},
	clearMembers: function(){
		if(this._memberArr)
			delete this._memberArr;
		this._memberArr = new Array();		
	},
	showGroup: function(groupName){
		if(!Bitrix.TypeUtility.isNotEmptyString(groupName)) throw "Bitrix.ParamClientSideActionGroupView.showGroup: groupName is not valid!";
		var r = this.getGroup(groupName);
		if(r == null) return;
		
		if(this._singleGroupShow){
			for(var i = 0; i < this._groupArr.length; i++){
				if(this._groupArr[i].getName() == groupName) continue;
				//this._groupArr[i].setItemsVisibility(false);
				this._groupArr[i].setVisibility(false);
			}				
		}
		r.setVisibility(true);
	},
	hideGroup: function(groupName){
		if(!Bitrix.TypeUtility.isNotEmptyString(groupName)) throw "Bitrix.ParamClientSideActionGroupView.hideGroup: groupName is not valid!";
		var r = this.getGroup(groupName);
		if(r == null) return;
		//r.setItemsVisibility(false);
		r.setVisibility(false);
	},
	clearGroups: function(){
		if(this._groupArr)
			delete this._groupArr;
		this._groupArr = new Array();
	},
	clear: function(){
		this.clearGroups();
		this.clearMembers();
	}
}
Bitrix.ParamClientSideActionGroupView._entries = new Object();
Bitrix.ParamClientSideActionGroupView.ensureEntryCreated = function(id){
	if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Bitrix.ParamClientSideActionGroupView.ensureEntryCreated: id is not valid!";
	var r = typeof(this._entries[id]) != "undefined" ? this._entries[id] : null;
	if(r == null){
		r = new Bitrix.ParamClientSideActionGroupView();
		r.initialize(id);
	}
	return r;
}
Bitrix.ParamClientSideActionGroupView.getEntryById = function(id){
	if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Bitrix.ParamClientSideActionGroupView.getEntryById: id is not valid!";
	return typeof(this._entries[id]) != "undefined" ? this._entries[id] : null;
}

Bitrix.ParamClientSideActionGroupView.removeEntryById = function(id){
	if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Bitrix.ParamClientSideActionGroupView.removeEntryById: id is not valid!";
	if(id in this._entries)
		delete this._entries[id];
}

Bitrix.ParamClientSideActionGroupView.removeEntriesByContextId = function(contextId){
	if(!Bitrix.TypeUtility.isNotEmptyString(contextId)) throw "Bitrix.ParamClientSideActionGroupView.removeEntryById: contextId is not valid!";
	var aboutDie = new Array();
	for(var entryKey in this._entries){
		if(this._entries[entryKey].getContextId() == contextId)
			aboutDie.push(entryKey);
	}
	if(aboutDie.length > 0){
		for(var index = 0; index < aboutDie.length; index++)
			delete this._entries[aboutDie[index]];
	}
	delete aboutDie;
}
		//if(!Bitrix.TypeUtility.isNotEmptyString(val))
		//	throw "ParamClientSideActionGroupView.setContextId: value is not valid!";

if (typeof(Bitrix.ParamClientSideActionGroupView.registerClass) == "function")
	Bitrix.ParamClientSideActionGroupView.registerClass("Bitrix.ParamClientSideActionGroupView");

Bitrix.ParamClientSideActionGroupViewSelector = function() {
	if (typeof (Bitrix.ParamClientSideActionGroupViewSelector.initializeBase) == "function")
		Bitrix.ParamClientSideActionGroupViewSelector.initializeBase(this);
	this._id = "";
	this._viewId = "";
	this._selectorElementId = "";
	this._selectorChangeHandler = BX.delegate(this._handleSelectorChange, this);
}

Bitrix.ParamClientSideActionGroupViewSelector.prototype = {
	initialize: function(id, viewId, selectorElementId){
		if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Bitrix.ParamClientSideActionGroupViewSelector.initialize: id is not valid!"; 
		this._id = id;
		
		if(!Bitrix.TypeUtility.isNotEmptyString(viewId)) throw "Bitrix.ParamClientSideActionGroupViewSelector.initialize: viewId is not valid!"; 
		this._viewId = viewId;
		
		if(!Bitrix.TypeUtility.isNotEmptyString(selectorElementId)) throw "Bitrix.ParamClientSideActionGroupViewSelector.initialize: selectorElementId is not valid!"; 
		this._selectorElementId = selectorElementId;
		
		Bitrix.EventHandlingUtility.getInstance().addEventListener(this.getSelectorElement(), "change", this._selectorChangeHandler);
		this._isInitialized = true;
		Bitrix.ParamClientSideActionGroupViewSelector._entries[this._id] = this;
	},
	getId: function(){
		return this._id;
	},	
	getViewId: function(){
		return this._viewId;
	},	
	getSelectorElementId: function(){
		return this._selectorElementId;
	},
	getSelectorElement: function(){
		return document.getElementById(this._selectorElementId);
	},
	apply: function(){
		var selectorEl = this.getSelectorElement();
		if(selectorEl == null) return;
		var options = selectorEl.options;
		var optionsCount = options != null ? options.length : 0;
		if(optionsCount == 0) return;
		
		var view = Bitrix.ParamClientSideActionGroupView.getEntryById(this._viewId);
		if(view == null) return;
		
		for(var i = 0; i < optionsCount; i++){
			var option = options.item(i);
			option.selected ? view.showGroup(option.value) : view.hideGroup(option.value);		
		}
	},
	_handleSelectorChange: function(e){
		this.apply();
	}
}

Bitrix.ParamClientSideActionGroupViewSelector._entries = new Object();
Bitrix.ParamClientSideActionGroupViewSelector.create = function(id, viewId, selectorElementId){
	var self = new Bitrix.ParamClientSideActionGroupViewSelector();
	self.initialize(id, viewId, selectorElementId);
	return self;
}

Bitrix.ParamClientSideActionGroupViewSelector.getEntryById = function(id){
	return typeof(this._entries[id]) != "undefied" ? this._entries[id] : null;
}

if (typeof (Bitrix.ParamClientSideActionGroupViewSelector.registerClass) == "function")
	Bitrix.ParamClientSideActionGroupViewSelector.registerClass("Bitrix.ParamClientSideActionGroupViewSelector");

Bitrix.ParamClientSideActionGroupViewSwitch = function() {
	if (typeof (Bitrix.ParamClientSideActionGroupViewSwitch.initializeBase) == "function")
		Bitrix.ParamClientSideActionGroupViewSwitch.initializeBase(this);
	this._id = "";
	this._viewId = "";
	this._switchElementId = "";
	this._onGroupName = "";
	this._offGroupName = "";
	this._switchChangeHandler = BX.delegate(this._handleSwitchChange, this);
}

Bitrix.ParamClientSideActionGroupViewSwitch.prototype = {
	initialize: function(id, viewId, switchElementId, onGroupName, offGroupName){
		if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Bitrix.ParamClientSideActionGroupViewSwitch.initialize: id is not valid!"; 
		this._id = id;
		
		if(!Bitrix.TypeUtility.isNotEmptyString(viewId)) throw "Bitrix.ParamClientSideActionGroupViewSwitch.initialize: viewId is not valid!"; 
		this._viewId = viewId;
		
		if(!Bitrix.TypeUtility.isNotEmptyString(switchElementId)) throw "Bitrix.ParamClientSideActionGroupViewSwitch.initialize: switchElementId is not valid!"; 
		this._switchElementId = switchElementId;
		
		this._onGroupName = onGroupName;
		this._offGroupName = offGroupName;		
		
		Bitrix.EventHandlingUtility.getInstance().addEventListener(this.getSwitchElement(), "click", this._switchChangeHandler);
		this._isInitialized = true;
		Bitrix.ParamClientSideActionGroupViewSwitch._entries[this._id] = this;
	},
	getId: function(){
		return this._id;
	},	
	getViewId: function(){
		return this._viewId;
	},	
	getSwitchElementId: function(){
		return this._switchElementId;
	},
	getSwitchElement: function(){
		return document.getElementById(this._switchElementId);
	},
	getOnGroupName: function(){
		return this._onGroupName;	
	},
	setOnGroupName: function(val){
		this._onGroupName = val;
	},
	getOffGroupName: function(){
		return this._offGroupName;	
	},
	setOffGroupName: function(val){
		this._offGroupName = val;
	},	
	apply: function(){
		var switchEl = this.getSwitchElement();
		if(switchEl == null) return;
		var view = Bitrix.ParamClientSideActionGroupView.getEntryById(this._viewId);
		if(view == null) return;
		
		try{
			var aboutShowGroupName = this._onGroupName;
			var aboutHideGroupName = this._offGroupName;
			if(!switchEl.checked){	
				aboutShowGroupName = this._offGroupName;
				aboutHideGroupName = this._onGroupName;
			}
			if(Bitrix.TypeUtility.isNotEmptyString(aboutHideGroupName))
				view.hideGroup(aboutHideGroupName);
			if(Bitrix.TypeUtility.isNotEmptyString(aboutShowGroupName))
				view.showGroup(aboutShowGroupName);				
		}
		catch(e){}		
	},
	_handleSwitchChange: function(e){
		this.apply();
	}
}

Bitrix.ParamClientSideActionGroupViewSwitch._entries = new Object();
Bitrix.ParamClientSideActionGroupViewSwitch.create = function(id, viewId, switchElementId, onGroupName, offGroupName){
	var self = new Bitrix.ParamClientSideActionGroupViewSwitch();
	self.initialize(id, viewId, switchElementId, onGroupName, offGroupName);
	return self;
}

Bitrix.ParamClientSideActionGroupViewSwitch.getEntryById = function(id){
	return typeof(this._entries[id]) != "undefied" ? this._entries[id] : null;
}

if (typeof (Bitrix.ParamClientSideActionGroupViewSwitch.registerClass) == "function")
	Bitrix.ParamClientSideActionGroupViewSwitch.registerClass("Bitrix.ParamClientSideActionGroupViewSwitch");

Bitrix.ComponentParameterViewDynamicExpressionsManager = function() {
	if (typeof (Bitrix.ComponentParameterViewDynamicExpressionsManager.initializeBase) == "function")
		Bitrix.ComponentParameterViewDynamicExpressionsManager.initializeBase(this);
	this._expressionOffImageUrl = "";
	this._expressionOnImageUrl = "";
	this._isInitialized  = false;
	this._isProcessingStarted = false;
	this._dynExpRegex = null;
}

Bitrix.ComponentParameterViewDynamicExpressionsManager.prototype = {
	initialize: function(id, name){
		this._expressionOffImageUrl = Bitrix.WebAppHelper.getPath() + "/bitrix/themes/.default/images/components/exp_off.gif";
		this._expressionOnImageUrl = Bitrix.WebAppHelper.getPath() + "/bitrix/themes/.default/images/components/exp_on.gif";
		this._refreshNotAnimatedImageUrl = Bitrix.WebAppHelper.getPath() + "/bitrix/themes/.default/images/refresh_blue.gif";
		this._refreshAnimatedImageUrl = Bitrix.WebAppHelper.getPath() + "/bitrix/themes/.default/images/refresh_blue_anim.gif";
		//this._dynExpRegex = new RegExp("^<%\\s*\\$\\s*(\\w+)\\s*\\:\\s*([^\\r\\n]+?)\\s*%>$");
		this._isInitialized = true;
	},
	getExpressionOffImageUrl: function()
	{
		if(!this._isInitialized) throw "Bitrix.ComponentParameterViewDynamicExpressionsManager.getExpressionOffImageUrl: is not initialized!";
		return this._expressionOffImageUrl;
	},
	getExpressionOnImageUrl: function()
	{
		if(!this._isInitialized) throw "Bitrix.ComponentParameterViewDynamicExpressionsManager.getExpressionOffImageUrl: is not initialized!";
		return this._expressionOnImageUrl;
	},	
	getRefreshNotAnimatedImageUrl: function(){
		if(!this._isInitialized) throw "Bitrix.ComponentParameterViewDynamicExpressionsManager.getRefreshNotAnimatedImageUrl: is not initialized!";
		return this._refreshNotAnimatedImageUrl;		
	},	
	getRefreshAnimatedImageUrl: function(){
		if(!this._isInitialized) throw "Bitrix.ComponentParameterViewDynamicExpressionsManager.getRefreshAnimatedImageUrl: is not initialized!";
		return this._refreshAnimatedImageUrl;		
	},
	startProcessing: function(){
		this._isProcessingStarted = true;
	},
	finishProcessing: function(){
		this._isProcessingStarted = false;
	},	
	_getObjectParameterViewKey: function(objectID, parameterName){
		if(!this._isInitialized) throw "Bitrix.ComponentParameterViewDynamicExpressionsManager._getObjectParameterViewKey: is not initialized!";
		if(!Bitrix.TypeUtility.isNotEmptyString(objectID)) throw "Bitrix.ComponentParameterViewDynamicExpressionsManager._getObjectParameterViewKey: objectID is not valid!";
		if(!Bitrix.TypeUtility.isNotEmptyString(parameterName)) throw "Bitrix.ComponentParameterViewDynamicExpressionsManager._getObjectParameterViewKey: parameterName is not valid!";
		return objectID + "_" + parameterName.toLowerCase();
	},
	_handleExpressionViewChange: function(view){
		this.persistObjectParameterViewDataByView(view);
	},
	processObjectParameterViewValue: function(parameterValue){
		if(!Bitrix.TypeUtility.isNotEmptyString(parameterValue))
			return parameterValue;

		var r = Bitrix.DynamicExpression.isValidSourceText(parameterValue);			
		return r ? "" : parameterValue;
	},
	_setupElementsStyles: function(el){
		if(!(typeof(el) == "object" && "tagName" in el))
			return;
		if(el.tagName == "INPUT"){
			if("type" in el && el.type.toLowerCase() == "checkbox")
				el.className += " bx-input-checkbox-parameter";
			return;
		}
		else if(el.tagName == "SELECT"){
			el.className += "multiple" in el && el.multiple ? " bx-select-multiple-parameter" : " bx-select-parameter";
		}
		if(!("childNodes" in el && el.childNodes.length > 0))
			return;
			
		for(var i = 0; i < el.childNodes.length; i++){
			this._setupElementsStyles(el.childNodes[i]);
		}
	},
	
	processObjectParameterValue: function(objectID, parameterName, parameterValue){
		if(!this._isInitialized) throw "Bitrix.ComponentParameterViewDynamicExpressionsManager.processObjectParameterValue: is not initialized!";
		
		if(!Bitrix.TypeUtility.isNotEmptyString(parameterValue))
			parameterValue = "";
		
		var key = this._getObjectParameterViewKey(objectID, parameterName);
		var data = Bitrix.DynamicExpression.getEntry(key);
		
		if(data){
			data.parse(parameterValue);
			if(data.isValid())
				data.setEnabled(true);
			else
				Bitrix.DynamicExpression.remove(key);
		}
		else{
			data = Bitrix.DynamicExpression.parse(parameterValue);	
			if(data.isValid()){
				data.setEnabled(true);
				Bitrix.DynamicExpression.setEntry(key, data);
			}
		}
	},
	processObjectParameterView: function(objectID, parameterName, parameterValue, arElements, parentEl){
		if(!this._isInitialized) throw "Bitrix.ComponentParameterViewDynamicExpressionsManager.processObjectParameterView: is not initialized!";
		if(!arElements || typeof(arElements) != "object" || typeof(arElements.length) == "undefined" || arElements.length <= 0) throw "Bitrix.ComponentParameterViewDynamicExpressionsManager.processElement: elements array is not defined!";
		if(!parentEl) throw "Bitrix.ComponentParameterViewDynamicExpressionsManager.processObjectParameterView:  parentEl is not defined!";
		
		var key = this._getObjectParameterViewKey(objectID, parameterName);
		
		var data = Bitrix.DynamicExpression.getEntry(key);
		
		var expPrefix = "", expContent = "";
		var modificationMode = Bitrix.ComponentParameterModificationMode.create(Bitrix.ComponentParameterModificationMode.values().standard);
		
		if(data){
			expPrefix = data.getPrefix();
			expContent = data.getContent();
			if(data.getEnabled())
				modificationMode = Bitrix.ComponentParameterModificationMode.create(Bitrix.ComponentParameterModificationMode.values().expression);
		}
		else{
			if(!Bitrix.TypeUtility.isNotEmptyString(parameterValue))
				parameterValue = "";
			data = Bitrix.DynamicExpression.parse(parameterValue);	
			if(data.isValid()){
				data.setEnabled(true);
				Bitrix.DynamicExpression.setEntry(key, data);	
				expPrefix = data.getPrefix();
				expContent = data.getContent();				
				modificationMode = Bitrix.ComponentParameterModificationMode.create(Bitrix.ComponentParameterModificationMode.values().expression);				
			}	
		}

		var elWrapper = document.createElement("DIV");
		elWrapper.className = "parameter-data-conainer";
		parentEl.appendChild(elWrapper);
		for(var i = 0; i < arElements.length; i++)
		{
			this._setupElementsStyles(arElements[i]);
			elWrapper.appendChild(arElements[i]);
		}
		var parameterElement = arElements[0];
		if(!parameterElement) throw "Bitrix.ComponentParameterViewDynamicExpressionsManager.processObjectParameterView: could not find parameter element!";
		
		if(!Bitrix.TypeUtility.isNotEmptyString(parameterElement.id))
			parameterElement.id = key + "_ParamView";
			
		var view = Bitrix.ComponentParameterViewComboExpressionEditor.createWhithControls(key, parameterName, parameterElement.id, key, elWrapper);
		
		view.setPrefixValue(expPrefix);
		view.setContentValue(expContent);
		view.setModificationMode(modificationMode);
		//view.setID(key);
		view.setParentObjectID(objectID);
		view.enableChangingHandling();		
		Bitrix.ComponentParametersEditor.setView(key, view);
		
		var switchModeButtonContainerId = key + "_SwitchModeButtonContainer";
		var switchModeButtonContainer = document.createElement("DIV");
		switchModeButtonContainer.id = switchModeButtonContainerId;
		switchModeButtonContainer.className = "parameter-buttons-container";
		var switchModeButton = document.createElement("IMG");
		switchModeButton.id = key + "_ImgSwitchModificationMode";
		switchModeButton.className = "parameter-button mode";
		
		switchModeButton.src = modificationMode.getCurrentID() == Bitrix.ComponentParameterModificationMode.values().standard ? this._expressionOffImageUrl : this._expressionOnImageUrl;
		switchModeButton.onclick = Bitrix.ComponentParameterViewDynamicExpressionsManager._switchModeButtonOnClick;		
		switchModeButtonContainer.appendChild(switchModeButton);
		
		parentEl._viewId = key;
		parentEl._switchModeButtonContainerId = switchModeButtonContainerId;
		parentEl.onmouseover = Bitrix.ComponentParameterViewDynamicExpressionsManager._dataContainerOnMouseOver;
		parentEl.onmouseout = Bitrix.ComponentParameterViewDynamicExpressionsManager._dataContainerOnMouseOut;
		if(view.getModificationMode().getCurrentID() == Bitrix.ComponentParameterModificationMode.values().standard)
			switchModeButtonContainer.style.visibility = "hidden";
		parentEl.appendChild(switchModeButtonContainer);	
		
		view.enableListeningToSwitchModeTriggerElement(switchModeButton, "click");		
		return modificationMode.getCurrentID() == Bitrix.ComponentParameterModificationMode.values().standard ? parameterValue : "";
	},
	
	
	processObjectParameterViewRefreshTrigger: function(objectID, parameterName, el, eventName, parentEl){
		el.style.display = "none";
		parentEl.appendChild(el);
		
		var key = this._getObjectParameterViewKey(objectID, parameterName);
		var refreshButtonContainer = document.createElement("DIV");
		refreshButtonContainer.id = key + "_RefreshButtonContainer";
		refreshButtonContainer.className = "parameter-buttons-container";		
		var refreshButton = document.createElement("IMG");
		refreshButton.id = key + "_ImgRefresh";
		refreshButton.className = "parameter-button mode";		
		//refreshButton.onclick = Bitrix.ComponentParameterViewDynamicExpressionsManager._refreshButtonOnClick;
		refreshButton.src = this._refreshNotAnimatedImageUrl;
		refreshButtonContainer.appendChild(refreshButton);
		var switchModeContainer = document.getElementById(key + "_SwitchModeButtonContainer");
		//if(switchModeContainer && typeof(parentEl.contains) == "function" && parentEl.contains(switchModeContainer))
		if(switchModeContainer)
			parentEl.insertBefore(refreshButtonContainer, switchModeContainer);
		else
			parentEl.appendChild(refreshButtonContainer);
			
		var view = Bitrix.ComponentParametersEditor.getView(key);
		if(view){
			view.setRefreshTriggeringCtrlID(refreshButton.id);
			view.setReshreshTriggeringCtrlEventName("click");
			var refreshScript = "document.getElementById('" + el.id + "')." + eventName + "();";
			view.setRefreshTriggeringScript(refreshScript);
			view.enableListeningToRefreshTriggerElement(true);
		}			
	},
	processObjectParameterViewRefreshSource: function(objectID, parameterName, el, eventName){
		var key = this._getObjectParameterViewKey(objectID, parameterName);
		var view = Bitrix.ComponentParametersEditor.getView(key);
		if(!view)
			return;
		view.enableListeningToRefreshSourceElement(true, el, eventName);
	},
	tryGetObjectParameterValue: function(objectID, parameterName){
		var key = this._getObjectParameterViewKey(objectID, parameterName);
		var data = Bitrix.DynamicExpression.getEntry(key);
		if(data && data.getEnabled() && data.isValid())
			return data.toString();
		return null;
	},

	persistObjectParameterViewDataByView: function(view){		
		if(!view || !view.isValuesAvailable()) 
			return;
		
		var viewID = view.getID();
		if(!Bitrix.TypeUtility.isNotEmptyString(viewID))
			return;
		
		var prefix = view.getPrefixValue();
		var content = view.getContentValue();
		
		if(prefix.length > 0 || content.length > 0){
		
			var data = Bitrix.DynamicExpression.getEntry(viewID);
			if(!data)
				data = Bitrix.DynamicExpression.create(viewID);
			data.setPrefix(prefix);
			data.setContent(content);

			var viewMode = view.getModificationMode();
			data.setEnabled(viewMode && viewMode.getCurrentID() == Bitrix.ComponentParameterModificationMode.values().expression);			
		}
		else
			Bitrix.DynamicExpression.remove(viewID);		
	}
}

Bitrix.ComponentParameterViewDynamicExpressionsManager._switchModeButtonOnClick = function(){
		var decorator = Bitrix.ComponentParameterViewDynamicExpressionsManager.getInstance(); 
		this.src = this.src.indexOf(decorator.getExpressionOnImageUrl()) >= 0 ? decorator.getExpressionOffImageUrl() : decorator.getExpressionOnImageUrl(); 	
}
Bitrix.ComponentParameterViewDynamicExpressionsManager._refreshButtonOnClick = function(){
		var decorator = Bitrix.ComponentParameterViewDynamicExpressionsManager.getInstance(); 
		this.src = decorator.getRefreshAnimatedImageUrl(); 	
}

Bitrix.ComponentParameterViewDynamicExpressionsManager._dataContainerOnMouseOver = function(){
	if(!Bitrix.TypeUtility.isNotEmptyString(this._switchModeButtonContainerId))
		return;

	if(this._setVisibleTask){ 
		window.clearTimeout(this._setVisibleTask); 
		this._setVisibleTask = null; 
	} 
	if(this._setNotVisibleTask){
		window.clearTimeout(this._setNotVisibleTask); 
		this._setNotVisibleTask = null;
	} 
	var switchModeButtonContainerId = this._switchModeButtonContainerId;
	this._setVisibleTask = window.setTimeout(function(){var switchModeButtonContainer = document.getElementById(switchModeButtonContainerId); if(switchModeButtonContainer) document.getElementById(switchModeButtonContainerId).style.visibility = 'visible';}, 500);
}

Bitrix.ComponentParameterViewDynamicExpressionsManager._dataContainerOnMouseOut = function(){
	if(!Bitrix.TypeUtility.isNotEmptyString(this._switchModeButtonContainerId) || !Bitrix.TypeUtility.isNotEmptyString(this._viewId))
		return;
		
	if(this._setNotVisibleTask){ 
		window.clearTimeout(this._setNotVisibleTask); 
		this._setNotVisibleTask = null; 
	} 
	if(this._setVisibleTask){
		window.clearTimeout(this._setVisibleTask); 
		this._setVisibleTask = null;
	}
	var switchModeButtonContainerId = this._switchModeButtonContainerId;
	var viewId = this._viewId;
	this._setNotVisibleTask = window.setTimeout(function(){var view = Bitrix.ComponentParametersEditor.getInstance().getView(viewId); if(view.getModificationMode().getCurrentID() == Bitrix.ComponentParameterModificationMode.values().expression) return; var switchModeButtonContainer = document.getElementById(switchModeButtonContainerId); if(switchModeButtonContainer)document.getElementById(switchModeButtonContainerId).style.visibility = 'hidden';}, 75);
}


Bitrix.ComponentParameterViewDynamicExpressionsManager._instance = null;
Bitrix.ComponentParameterViewDynamicExpressionsManager.getInstance = function(){
	if(this._instance == null){
		this._instance = new Bitrix.ComponentParameterViewDynamicExpressionsManager();
		this._instance.initialize();
	}
	return this._instance;
}
Bitrix.ComponentParameterViewDynamicExpressionsManager.startProcessing = function(){
	this.getInstance().startProcessing();
}
Bitrix.ComponentParameterViewDynamicExpressionsManager.finishProcessing = function(){
	this.getInstance().finishProcessing();
}
Bitrix.ComponentParameterViewDynamicExpressionsManager.processObjectParameterView = function(objectID, parameterName, parameterValue, arElements, parentEl){
	this.getInstance().processObjectParameterView(objectID, parameterName, parameterValue, arElements, parentEl);
}

Bitrix.ComponentParameterViewDynamicExpressionsManager.processObjectParameterViewValue = function(parameterValue){
	return this.getInstance().processObjectParameterViewValue(parameterValue);
}

Bitrix.ComponentParameterViewDynamicExpressionsManager.tryGetObjectParameterValue = function(objectID, parameterName){
	return this.getInstance().tryGetObjectParameterValue(objectID, parameterName);
}
Bitrix.ComponentParameterViewDynamicExpressionsManager._handleExpressionViewChange = function(view){
	this.getInstance()._handleExpressionViewChange(view);
}
Bitrix.ComponentParameterViewDynamicExpressionsManager.processObjectParameterViewRefreshTrigger = function(objectID, parameterName, el, eventName, parentEl){
	this.getInstance().processObjectParameterViewRefreshTrigger(objectID, parameterName, el, eventName, parentEl)
}

Bitrix.ComponentParameterViewDynamicExpressionsManager.processObjectParameterViewRefreshSource = function(objectID, parameterName, el, eventName){
	this.getInstance().processObjectParameterViewRefreshSource(objectID, parameterName, el, eventName);
}

if (typeof (Bitrix.ComponentParameterViewDynamicExpressionsManager.registerClass) == "function")
	Bitrix.ComponentParameterViewDynamicExpressionsManager.registerClass("Bitrix.ComponentParameterViewDynamicExpressionsManager");

Bitrix.ParamClientSideActionManager = function(){
	this._initialized = false;
}

Bitrix.ParamClientSideActionManager.prototype = {
	initialize: function(){
		this._initialized = true;
	},
	startProcessing: function(objectID){
		Bitrix.ParamClientSideActionGroupView.removeEntriesByContextId(objectID);
	},
	processObjectParameterView: function(objectID, parameterName, el, clientSideAction, itemElementByIdFunc){
		if(!(typeof(clientSideAction) == "object" && "REGISTRATOR" in clientSideAction))
			return;
		var registratorStr = clientSideAction.REGISTRATOR;
		if(typeof(registratorStr) != "string" || registratorStr.length == 0)
			return;
		var registrator = eval(registratorStr);
		if(typeof(registrator) != "object" || typeof(registrator.register) != "function")
			return;
		registrator.register(el, itemElementByIdFunc, objectID);
		delete registrator;
	},
	evaluteVisibilityForParameterView: function(parameterID){
		var members = new Array();
		if(!Bitrix.TypeUtility.isNotEmptyString(parameterID)) throw "ParamClientSideActionManager.demandVisibilityForParameterView: parameterID is not valid!";
		for(var key in Bitrix.ParamClientSideActionGroupView._entries){
			var entry = Bitrix.ParamClientSideActionGroupView._entries[key];
			var curMembers = entry.getMembersByParameterId(parameterID);
			if(curMembers != null && curMembers.length > 0)
				for(var i = 0; i < curMembers.length; i++ )
					members.push(curMembers[i]);
			delete curMembers;
		}
		
		var r = true;
		if(members.length > 0){
			
			for(var i = 0; i < members.length && r; i++){
				r = members[i].evaluteVisibility();
			}
		}
		delete members;
		return r;
	}
}

Bitrix.ParamClientSideActionManager._instance = null;
Bitrix.ParamClientSideActionManager.getInstance = function(){
	if(this._instance == null){
		this._instance = new Bitrix.ParamClientSideActionManager();
		this._instance.initialize();
	}
	return this._instance;
}

Bitrix.ParamClientSideActionManager.startProcessing = function(objectID){
	this.getInstance().startProcessing(objectID);
}

Bitrix.ParamClientSideActionManager.processObjectParameterView = function(objectID, parameterName, el, clientSideAction, itemElementByIdFunc){
	this.getInstance().processObjectParameterView(objectID, parameterName, el, clientSideAction, itemElementByIdFunc);
}


if (typeof (Bitrix.ParamClientSideActionManager.registerClass) == "function")
	Bitrix.ParamClientSideActionManager.registerClass("Bitrix.ParamClientSideActionManager");

Bitrix.ComponentParameterInfo = function() {
	if (typeof (Bitrix.ComponentParameterInfo.initializeBase) == "function")
		Bitrix.ComponentParameterInfo.initializeBase(this);
	this._initialized = false;
	this._componentId = null;
	this._parameterName = null;
	this._elementsArray = null;
	this._view = null;
}

Bitrix.ComponentParameterInfo.prototype = {
	initialize: function(componentId, parameterName, elementsArray, view){
		this._componentId = componentId;
		this._parameterName = parameterName;
		this._elementsArray = elementsArray;
		this._view = view;
		
		this._initialized = true;
	},
	getComponentId: function(){ return this._componentId; },
	getParameterName: function(){ return this._parameterName; },
	getElementsArray: function(){ return this._elementsArray; },
	getView: function(){ return this._view; }	
}

Bitrix.ComponentParameterInfo.create = function(componentId, parameterName, elementsArray, view){
	var self = new Bitrix.ComponentParameterInfo();
	self.initialize(componentId, parameterName, elementsArray, view);
	return self;
}

Bitrix.ComponentParameterRegistrationOptions = function() {
	if (typeof (Bitrix.ComponentParameterRegistrationOptions.initializeBase) == "function")
		Bitrix.ComponentParameterRegistrationOptions.initializeBase(this);
	this._initialized = false;
	this._componentId = null;
	this._parameterName = null;	
	this._viewId = null;
	this._customData = null;
	this._containerElement = null;
	this._replaceContainer = false;
}

Bitrix.ComponentParameterRegistrationOptions.prototype = {
	initialize: function(componentId, parameterName, viewId, containerElement, replaceContainer, customData){
		this._componentId = componentId;
		this._parameterName = parameterName;
		this._viewId = viewId;
		this._customData = customData && typeof(customData) == "object" ? customData : new Object;
		this._containerElement = containerElement;
		this._replaceContainer = replaceContainer;
		this._initialized = true;
	},
	getComponentId: function(){ return this._componentId; },
	getParameterName: function(){ return this._parameterName; },
	getViewId: function(){ return this._viewId; },
	getContainerElement: function(){ return this._containerElement; },
	getReplaceContainer: function(){ return this._replaceContainer; },
	getCustomData: function(){ return this._customData; }
}

Bitrix.ComponentParameterRegistrationOptions.create = function(componentId, parameterName, viewId, containerElement, replaceContainer, customData){
	var self = new Bitrix.ComponentParameterRegistrationOptions();
	self.initialize(componentId, parameterName, viewId, containerElement, replaceContainer, customData);
	return self;
}

Bitrix.ComponentParameterViewDoubleList = function() {
	if (typeof (Bitrix.ComponentParameterViewDoubleList.initializeBase) == "function")
		Bitrix.ComponentParameterViewDoubleList.initializeBase(this);
	this._initialized = false;
	this._id = null;
	this._componentId = null;
	this._parameterName = null;
	this._items = null;
	this._selectedValues = null;
	this._availableSelectElId = null; 
	this._selectedSelectElId = null;
	this._selectedValuesInputElId = null;
	this._changeEvent = null;
	this._customData = null;
	this._customAddOptions = null;
	this._allowDuplicates = false;
	this._useCsv = false;
}

Bitrix.ComponentParameterViewDoubleList.prototype = {
	initialize: function(id, items, selectedValues, availableSelectElId, selectedSelectElId, selectedValuesInputElId, useCsv, customAddOptions, allowDuplicates){
		if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "id is not valid!";
		if(!Bitrix.TypeUtility.isNotEmptyString(availableSelectElId)) throw "availableSelectElId is not valid!";
		if(!Bitrix.TypeUtility.isNotEmptyString(selectedSelectElId)) throw "selectedSelectElId is not valid!";
		if(!Bitrix.TypeUtility.isNotEmptyString(selectedValuesInputElId)) throw "selectedValuesInputElId is not valid!";
		
		this._allowDuplicates = (allowDuplicates == true); 
		
		if (customAddOptions != null && customAddOptions.Allow)
		{
			if(!Bitrix.TypeUtility.isNotEmptyString(customAddOptions.Prefix)) throw "customAddOptions.Prefix is not valid!";
			if(!Bitrix.TypeUtility.isNotEmptyString(customAddOptions.Prompt)) 
				customAddOptions.Prompt =  "Enter custom item value";
			if (customAddOptions.DefaultValue == null)
				customAddOptions.DefaultValue = "New Item";
			if (customAddOptions.ValueFormat == null)
				customAddOptions.ValueFormat = "-- {0} --";
		}
		this._customAddOptions = customAddOptions ? customAddOptions : {};
		
		this._id = id;
		this._useCsv = (useCsv == true);
		
		if(items instanceof Array)
			this._items = items;
		else if(typeof(items) == "object"){
			this._items = new Array();
			for(var key in items){
				var item = items[key];
				var myItem = new Object();
				myItem["value"] = key;
				myItem["text"] = item.toString();
				this._items.push(myItem);
			}
		}
		else
			this._items = new Array();
		
		if(selectedValues instanceof Array)
			this._selectedValues = selectedValues;
		else if(Bitrix.TypeUtility.isNotEmptyString(selectedValues))
			this._selectedValues = this._useCsv ? Bitrix.TypeUtility.csvToArray(selectedValues) : selectedValues.split(";");
		else 
			this._selectedValues = new Array();
			
		this._availableSelectElId = availableSelectElId;
		this._selectedSelectElId = selectedSelectElId;	
		this._selectedValuesInputElId = selectedValuesInputElId;
		this.prepare();
		
		this._initialized = true;
	},
	getId: function(){ return this._id; },
	/*ParamView*/
	getComponentId: function(){ return this._componentId; },
	setComponentId: function(value){ this._componentId = value;},
	getParameterName: function(){ return this._parameterName; },
	setParameterName: function(value){ this._parameterName = value; },
	getValue: function(){
		return this._useCsv ? Bitrix.TypeUtility.arrayToCsv(this._selectedValues) : this._selectedValues.join(";");	
	},
	setValue: function(){ throw "Not supported!"; },
	prepare: function(){ 
		this._saveSelectedValues();
		this._fillAvailable();
		this._fillSelected();
	},
	addChangeListener: function(listener){
		if(this._changeEvent == null)
			this._changeEvent = new Bitrix.EventPublisher();
		this._changeEvent.addListener(listener);
	},	
	removeModelStateChangeListener: function(listener){
		if(this._changeEvent == null) return;
		this._changeEvent.removeListener(listener);
	},	
	getCustomData: function(){
		if(this._customData == null)
			this._customData = new Object();
		return this._customData;
	},
	setCustomData: function(value){
		this._customData = value;
	},
	/**/
	_getItemByValue: function(value){
		var index = this._getItemIndexByValue(value);
		return index >= 0 ? this._items[index] : null;
	},
	_getItemIndexByValue: function(value){
		if(this._items == null) return -1;
		for(var i = 0; i < this._items.length; i++){
			var item = this._items[i];
			if(item && "value" in item && item["value"] == value)
				return i;
		}
		return -1;
	},
	_getSelectedValueIndex: function(value){
		if(this._selectedValues == null) return -1;
		for(var i = 0; i < this._selectedValues.length; i++){
			if(this._selectedValues[i] == value)
				return i;
		}
		return -1;
	},
	_fillAvailable: function(){
		var availableSelectEl = document.getElementById(this._availableSelectElId);
		if(!availableSelectEl) return;
		while(availableSelectEl.length > 0)
			availableSelectEl.remove(0);

		for(var i = 0; i < this._items.length; i++){
			var item = this._items[i];
			if(!this._allowDuplicates && this._getSelectedValueIndex(item["value"]) >= 0)
				continue;
			var option = document.createElement("OPTION");
			option.value = item["value"];
			option.text = "text" in item ? item["text"] : item["value"];
			try{
				availableSelectEl.add(option, null);
			}
			catch(e){
				availableSelectEl.add(option);
			}
			
		}		
	},
	_selectAvailableItemsByValues: function(values){
		var availableSelectEl = document.getElementById(this._availableSelectElId);
		if(!availableSelectEl) return;
		for(var i = 0; i < values.length; i++){
			var value = values[i];
			for(var j = 0; j < availableSelectEl.options.length; j++){
				if(availableSelectEl.options[j].value != value) continue;
				availableSelectEl.options[j].selected = true;
				break;
			}			
		}		
	},
	_fillSelected: function(keepSelection){
		var selectedSelectEl = document.getElementById(this._selectedSelectElId);
		if(!selectedSelectEl) return;
		var selectedValues = new Array();
		while(selectedSelectEl.options.length > 0)
		{
			if(keepSelection === true && selectedSelectEl.options[0].selected)
				selectedValues.push(selectedSelectEl.options[0].value);
			selectedSelectEl.remove(0);
		}	
		for(var i = 0; i < this._selectedValues.length; i++){
			var option;
			var val = this._selectedValues[i];
			if(this._customAddOptions.Allow == true && val.substr(0, this._customAddOptions.Prefix.length) == this._customAddOptions.Prefix)
			{
				option = document.createElement("OPTION");
				option.value = val;
				option.text = this._customAddOptions.ValueFormat.replace(/\{0\}/g, val.substring(this._customAddOptions.Prefix.length));
				option.isCustom = true;
			}
			else
			{
				var index = this._getItemIndexByValue(val);
				if(index < 0) 
					continue;
				var item = this._items[index];
				option = document.createElement("OPTION");
				option.value = item["value"];
				option.text = "text" in item ? item["text"] : item["value"];
			}
			
			try{
				selectedSelectEl.add(option, null);
			}
			catch(e){
				selectedSelectEl.add(option);
			}
			var isSelected = false;
			for(var j = 0; j < selectedValues.length; j++){
				if(selectedValues[j] != option.value) continue;
				isSelected = true;
				break;
			}
			if(isSelected)
				selectedSelectEl.options[selectedSelectEl.options.length - 1].selected = true;
		}
		delete selectedValues;
	},
	_saveSelectedValues: function(){
		var selectedValuesInputEl = document.getElementById(this._selectedValuesInputElId);	
		if(!selectedValuesInputEl) return;
		selectedValuesInputEl.value = this._useCsv ? Bitrix.TypeUtility.arrayToCsv(this._selectedValues) : this._selectedValues.join(",");		
		if(this._changeEvent != null) this._changeEvent.fire(this);			
	},
	
	addToSelection: function(){
		var availableSelectEl = document.getElementById(this._availableSelectElId);
		var selectedSelectEl = document.getElementById(this._selectedSelectElId);
		var selectedValuesInputEl = document.getElementById(this._selectedValuesInputElId);
		if(!availableSelectEl || !selectedSelectEl) return;
		
		for(var j = 0; j < selectedSelectEl.options.length; j++)
			if(selectedSelectEl.options[j].selected)
				selectedSelectEl.options[j].selected = false;
		
		var isChanged = false;
		for(var i = 0; i < availableSelectEl.options.length; i++){
			var curOption = availableSelectEl.options[i];
			if(!curOption.selected) continue;
			var item = this._getItemByValue(curOption.value);
			if(item == null) continue;
			if(!isChanged) isChanged = true;
			this._selectedValues.push(item["value"]);
			var selectedOption = document.createElement("OPTION");
			selectedOption.value = item["value"];
			selectedOption.text = "text" in item ? item["text"] : item["value"];
			try{
				selectedSelectEl.add(selectedOption, null);
			}
			catch(e){
				selectedSelectEl.add(selectedOption);
			}
			selectedSelectEl.options[selectedSelectEl.options.length - 1].selected = true;
		}
		if(isChanged){
			this._saveSelectedValues();
			this._fillAvailable();
		}
	},
	removeFromSelection: function(){
		var selectedSelectEl = document.getElementById(this._selectedSelectElId);
		if(!selectedSelectEl) return;	
		var isChanged = false;
		var i = 0, lastSelectedIndex = -1;
		var removedValues = new Array();
		while(i < selectedSelectEl.options.length){
			var curOption = selectedSelectEl.options[i];
			if(curOption.selected){
				lastSelectedIndex = i;
				var selectedValueIndex = this._getSelectedValueIndex(curOption.value);
				if(selectedValueIndex >= 0)
					this._selectedValues.splice(selectedValueIndex, 1);
				if (!curOption.isCustom)
					removedValues.push(curOption.value);
				selectedSelectEl.remove(i);
				if(!isChanged) isChanged = true;
				continue;
			}
			i++;
		}
		if(isChanged){
			if(lastSelectedIndex >= 0 && selectedSelectEl.options.length > 0)
				selectedSelectEl.options[lastSelectedIndex <= selectedSelectEl.options.length - 1 ? lastSelectedIndex : selectedSelectEl.options.length - 1].selected = true;
			this._saveSelectedValues();
			this._fillAvailable();
			this._selectAvailableItemsByValues(removedValues);
		}
		delete removedValues;
	},
	addCustom: function(){
		var availableSelectEl = document.getElementById(this._availableSelectElId);
		var selectedSelectEl = document.getElementById(this._selectedSelectElId);
		var selectedValuesInputEl = document.getElementById(this._selectedValuesInputElId);
		if(!availableSelectEl || !selectedSelectEl) return;
		
		for(var j = 0; j < selectedSelectEl.options.length; j++)
			if(selectedSelectEl.options[j].selected)
				selectedSelectEl.options[j].selected = false;
		
		var newVal = window.prompt(this._customAddOptions.Prompt, this._customAddOptions.DefaultValue);
		if (newVal == null)
			return;
		
		var selectedOption = document.createElement("OPTION");
		selectedOption.value = this._customAddOptions.Prefix + newVal;
		this._selectedValues.push(selectedOption.value);
		selectedOption.text = this._customAddOptions.ValueFormat.replace(/\{0\}/g, newVal);
		selectedOption.isCustom = true;
		try
		{
			selectedSelectEl.add(selectedOption, null);
		}
		catch(e)
		{
			selectedSelectEl.add(selectedOption);
		}
		selectedSelectEl.options[selectedSelectEl.options.length - 1].selected = true;
		this._saveSelectedValues();
	},
	moveDown: function(){
		var selectedSelectEl = document.getElementById(this._selectedSelectElId);
		if(!selectedSelectEl) return;
		var movedValuesArr = new Array();
		for(var i = 0; i < selectedSelectEl.options.length; i++)
			if(selectedSelectEl.options[i].selected)
				movedValuesArr.push(selectedSelectEl.options[i].value);
		for(var j = movedValuesArr.length - 1; j >= 0; j--){
			var index = this._getSelectedValueIndex(movedValuesArr[j]);
			if(index < 0 || index >= this._selectedValues.length - 1) continue;
			var nextValue = this._selectedValues[index + 1];
			var isNextValueMoved = false;
			for(var k = 0; k < movedValuesArr.length; k++){
				if(nextValue != movedValuesArr[k])
					continue;
				isNextValueMoved = true;
				break;
			}
			if(isNextValueMoved) continue;
			this._selectedValues[index + 1] = this._selectedValues[index];
			this._selectedValues[index] = nextValue;			
		}
		delete movedValuesArr;
		this._saveSelectedValues();
		this._fillSelected(true);		
	},
	moveUp: function(){
		var selectedSelectEl = document.getElementById(this._selectedSelectElId);
		if(!selectedSelectEl) return;
		var movedValuesArr = new Array();
		for(var i = 0; i < selectedSelectEl.options.length; i++)
			if(selectedSelectEl.options[i].selected)
				movedValuesArr.push(selectedSelectEl.options[i].value);
		for(var j = 0; j < movedValuesArr.length; j++){
			var index = this._getSelectedValueIndex(movedValuesArr[j]);
			if(index <= 0) continue;
			var prevValue = this._selectedValues[index - 1];
			var isPrevValueMoved = false;
			for(var k = 0; k < movedValuesArr.length; k++){
				if(prevValue != movedValuesArr[k])
					continue;
				isPrevValueMoved = true;
				break;
			}
			if(isPrevValueMoved) continue;
			this._selectedValues[index - 1] = this._selectedValues[index];
			this._selectedValues[index] = prevValue;			
		}
		delete movedValuesArr;
		this._saveSelectedValues();
		this._fillSelected(true);
	}	
}

Bitrix.ComponentParameterViewDoubleList._entries = null;

Bitrix.ComponentParameterViewDoubleList.createEntry = function(id, items, selectedValues, availableSelectElId, selectedSelectElId, selectedValuesInputElId, useCsv, customAddOptions, allowDuplicates){
	var self = new Bitrix.ComponentParameterViewDoubleList();
	if(this._entries != null && id in this._entries) throw "'" + id + "' already exists!";
	self.initialize(id, items, selectedValues, availableSelectElId, selectedSelectElId, selectedValuesInputElId, useCsv, customAddOptions, allowDuplicates);
	if(this._entries == null)
		this._entries = new Object();
	this._entries[id] = self;
	return self;
}

Bitrix.ComponentParameterViewDoubleList.getEntry = function(id){
	if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Id is not valid!";
	return this._entries != null && id in this._entries ? this._entries[id] : null;
}

Bitrix.ComponentParameterViewDoubleList.addToSelection = function(id){
	var entry = this.getEntry(id);
	if(entry)
		entry.addToSelection();
}

Bitrix.ComponentParameterViewDoubleList._attrNameViewID = "BXComponentParameterViewDoubleListID";

Bitrix.ComponentParameterViewDoubleList._addEventListenerAddToSelection = function(el, id){
	el.setAttribute(Bitrix.ComponentParameterViewDoubleList._attrNameViewID, id);
	jsUtils.addEvent(el, "click", Bitrix.ComponentParameterViewDoubleList._handleAddToSelection);
}

Bitrix.ComponentParameterViewDoubleList._handleAddToSelection = function(){
	var sender = sender = /*@cc_on!@*/false ? (event ? event.srcElement : null) : this;
	if(!sender) return;
	var id = sender.getAttribute(Bitrix.ComponentParameterViewDoubleList._attrNameViewID);
	Bitrix.ComponentParameterViewDoubleList.addToSelection(id);
}

Bitrix.ComponentParameterViewDoubleList.removeFromSelection = function(id){
	var entry = this.getEntry(id);
	if(entry)
		entry.removeFromSelection();
}

Bitrix.ComponentParameterViewDoubleList._addEventListenerRemoveFromSelection = function(el, id){
	el.setAttribute(Bitrix.ComponentParameterViewDoubleList._attrNameViewID, id);
	jsUtils.addEvent(el, "click", Bitrix.ComponentParameterViewDoubleList._handleRemoveFromSelection);
}

Bitrix.ComponentParameterViewDoubleList._handleRemoveFromSelection = function(){
	var sender = sender = /*@cc_on!@*/false ? (event ? event.srcElement : null) : this;
	if(!sender) return;
	var id = sender.getAttribute(Bitrix.ComponentParameterViewDoubleList._attrNameViewID);
	Bitrix.ComponentParameterViewDoubleList.removeFromSelection(id);
}

Bitrix.ComponentParameterViewDoubleList.addCustom = function(id){
	var entry = this.getEntry(id);
	if(entry)
		entry.addCustom();
}

Bitrix.ComponentParameterViewDoubleList._addEventListenerAddCustom = function(el, id){
	el.setAttribute(Bitrix.ComponentParameterViewDoubleList._attrNameViewID, id);
	jsUtils.addEvent(el, "click", Bitrix.ComponentParameterViewDoubleList._handleAddCustom);
}

Bitrix.ComponentParameterViewDoubleList._handleAddCustom = function(){
	var sender = sender = /*@cc_on!@*/false ? (event ? event.srcElement : null) : this;
	if(!sender) return;
	var id = sender.getAttribute(Bitrix.ComponentParameterViewDoubleList._attrNameViewID);
	Bitrix.ComponentParameterViewDoubleList.addCustom(id);
}


Bitrix.ComponentParameterViewDoubleList.moveDown = function(id){
	var entry = this.getEntry(id);
	if(entry)
		entry.moveDown();
}

Bitrix.ComponentParameterViewDoubleList._addEventListenerMoveDown = function(el, id){
	el.setAttribute(Bitrix.ComponentParameterViewDoubleList._attrNameViewID, id);
	jsUtils.addEvent(el, "click", Bitrix.ComponentParameterViewDoubleList._handleMoveDown);
}

Bitrix.ComponentParameterViewDoubleList._handleMoveDown = function(){
	var sender = sender = /*@cc_on!@*/false ? (event ? event.srcElement : null) : this;
	if(!sender) return;
	var id = sender.getAttribute(Bitrix.ComponentParameterViewDoubleList._attrNameViewID);
	Bitrix.ComponentParameterViewDoubleList.moveDown(id);
}

Bitrix.ComponentParameterViewDoubleList.moveUp = function(id){
	var entry = this.getEntry(id);
	if(entry)
		entry.moveUp();
}

Bitrix.ComponentParameterViewDoubleList._addEventListenerMoveUp = function(el, id){
	el.setAttribute(Bitrix.ComponentParameterViewDoubleList._attrNameViewID, id);
	jsUtils.addEvent(el, "click", Bitrix.ComponentParameterViewDoubleList._handleMoveUp);
}

Bitrix.ComponentParameterViewDoubleList._handleMoveUp = function(){
	var sender = sender = /*@cc_on!@*/false ? (event ? event.srcElement : null) : this;
	if(!sender) return;
	var id = sender.getAttribute(Bitrix.ComponentParameterViewDoubleList._attrNameViewID);
	Bitrix.ComponentParameterViewDoubleList.moveUp(id);
}

Bitrix.ComponentParameterViewDoubleList.removeEntry = function(id){
	if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Id is not valid!";
	if(this._entries == null || !(id in this._entries)) return;
	delete this._entries[id];
}

Bitrix.ComponentParameterViewDoubleList.createHtml = function(ID, nameForValueInput, captions, containerEl, replaceContainer, addAllowed){
	var table = document.createElement("TABLE");
	table.id = ID;
	table.className = "bx-table-double-list-parameter";
	var tbody = document.createElement("TBODY");
	table.appendChild(tbody);
	
	var headerR = document.createElement("TR");
	tbody.appendChild(headerR);
	
	var headerC1 = document.createElement("TD");
	headerR.appendChild(headerC1);
	headerC1.appendChild(captions && "HeaderAvailable" in captions ? document.createTextNode(captions["HeaderAvailable"]) : "Available");
	
	var headerC2 = document.createElement("TD");
	headerR.appendChild(headerC2);
	
	var headerC3 = document.createElement("TD");
	headerR.appendChild(headerC3);
	headerC3.appendChild(captions && "HeaderSelected" in captions ? document.createTextNode(captions["HeaderSelected"]) : "Selected");	
	
	var contentR = document.createElement("TR");
	tbody.appendChild(contentR);

	var contentCAvl = document.createElement("TD");
	contentR.appendChild(contentCAvl);
	contentCAvl.className = "data";
	//contentCAvl.setAttribute("width", "50%");
	
	var selectAvl = document.createElement("SELECT");
	contentCAvl.appendChild(selectAvl);
	selectAvl.id = ID + "_" + "AvailableItems";
	selectAvl.className = "bx-select-double-list-parameter";
	selectAvl.setAttribute("multiple", "multiple");
	
	var contentCMiddleCmd = document.createElement("TD");
	contentR.appendChild(contentCMiddleCmd);
	
	var btnAdd = document.createElement("INPUT");
	btnAdd.setAttribute("type", "button");
	btnAdd.className = "bx-cmd-btn-add-double-list-parameter";
	btnAdd.value = ">";
	this._addEventListenerAddToSelection(btnAdd, ID);
	contentCMiddleCmd.appendChild(btnAdd);		
	
	var contentCSel = document.createElement("TD");
	contentR.appendChild(contentCSel);
	contentCSel.className = "data";
	//contentCSel.setAttribute("width", "50%");

	var selectSel = document.createElement("SELECT");
	contentCSel.appendChild(selectSel);
	selectSel.id = ID + "_" + "SelectedItems";
	selectSel.className = "bx-select-double-list-parameter";
	selectSel.setAttribute("multiple", "multiple");	
	
	var contentCRightCmd = document.createElement("TD");
	contentR.appendChild(contentCRightCmd);

	var btnUp = document.createElement("INPUT");
	btnUp.setAttribute("type", "button");
	btnUp.className = "bx-cmd-btn-double-list-parameter";
	btnUp.value = captions && "TitleUp" in captions ? captions["TitleUp"] : "Up";
	contentCRightCmd.appendChild(btnUp);
	contentCRightCmd.appendChild(document.createElement("BR"));
	this._addEventListenerMoveUp(btnUp, ID);		

	var btnDown = document.createElement("INPUT");
	btnDown.setAttribute("type", "button");
	btnDown.className = "bx-cmd-btn-double-list-parameter";
	btnDown.value = captions && "TitleDown" in captions ? captions["TitleDown"] : "Down";
	contentCRightCmd.appendChild(btnDown);
	contentCRightCmd.appendChild(document.createElement("BR"));	
	this._addEventListenerMoveDown(btnDown, ID);

	if (addAllowed)
	{
		var btnAdd = document.createElement("INPUT");
		btnAdd.setAttribute("type", "button");
		btnAdd.className = "bx-cmd-btn-double-list-parameter";
		btnAdd.value = captions && "TitleAddCustom" in captions ? captions["TitleAddCustom"] : "Add";
		contentCRightCmd.appendChild(btnAdd);
		contentCRightCmd.appendChild(document.createElement("BR"));
		this._addEventListenerAddCustom(btnAdd, ID);
	}

	var btnRem = document.createElement("INPUT");
	btnRem.setAttribute("type", "button");
	btnRem.className = "bx-cmd-btn-double-list-parameter";
	btnRem.value = captions && "TitleRemove" in captions ? captions["TitleRemove"] : "Remove";
	contentCRightCmd.appendChild(btnRem);
	contentCRightCmd.appendChild(document.createElement("BR"));
	this._addEventListenerRemoveFromSelection(btnRem, ID);
	
	var hdnValues = document.createElement("INPUT");	
	hdnValues.setAttribute("type", "hidden");
	hdnValues.id = ID + "_" + "SelectedValues";
	hdnValues.name = nameForValueInput;
	contentCRightCmd.appendChild(hdnValues);
	
	if(containerEl){
		if(replaceContainer === true){
			var parentNode = containerEl.parentNode;
			parentNode.replaceChild(table, containerEl);
		}
		else
			containerEl.appendChild(table);	
	}
	
	var result = new Array();
	result.push(table);
	return result;
}

Bitrix.ComponentParameterViewDoubleList.create = function(options){
	if(!(options instanceof Bitrix.ComponentParameterRegistrationOptions)) throw "Options is not valid!";
	var componentId = options.getComponentId();
	var parameterName = options.getParameterName();
	var ID = options.getViewId();
	if(!Bitrix.TypeUtility.isNotEmptyString(ID))
		ID = componentId + "_" + parameterName;
		
	var containerEl = options.getContainerElement();
	var replaceContainer = options.getReplaceContainer();
	var customData = options.getCustomData();
	var nameForValueInput = "NameForValueInput" in customData ? customData["NameForValueInput"] : ID;
	var items = "Items" in customData ? customData["Items"] : null;
	var selectedValues = "SelectedValues" in customData ? customData["SelectedValues"] : null;
	var captions = "Captions" in customData ? customData["Captions"] : null;
		
	var allowCustomAdd = (customData.CustomAddOptions != null) && (customData.CustomAddOptions.Allow == true);
	var elementsArray = this.createHtml(ID, nameForValueInput, captions, containerEl, replaceContainer, allowCustomAdd);
	this.removeEntry(ID);
	var view = this.createEntry(ID, items, selectedValues, ID + "_" + "AvailableItems", ID + "_" + "SelectedItems", ID + "_" + "SelectedValues", customData.UseCsv == true, customData.CustomAddOptions, customData.AllowDuplicates == true);
	view.setComponentId(componentId);
	view.setParameterName(parameterName);
	view.setCustomData(customData);
	var result = Bitrix.ComponentParameterInfo.create(componentId, parameterName, elementsArray, view);
	return result;
}

Bitrix.ComponentParameterViewStub = new Object();
Bitrix.ComponentParameterViewStub.create = function(options) {
    if(!(options instanceof Bitrix.ComponentParameterRegistrationOptions)) throw "Invalid options!"; 
    
    var containerEl = options.getContainerElement(); 
    var stubEl = document.createElement("SPAN"); 
    stubEl.innerHTML = options.getCustomData() && "Text" in options.getCustomData() ? options.getCustomData()["Text"] : "N/A";
    if(containerEl)
        containerEl.appendChild(stubEl);
    return Bitrix.ComponentParameterInfo.create(options.getComponentId(), options.getParameterName(), [stubEl], null); 
}

Bitrix.ComponentParameterViewGoogleMapsInitialState = function() {
	this._id = "";
	this._dataInputId = null;
	this._componentId = null;
	this._parameterName = null;
	this._changeEvent = null;
	this._customData = null;
	this._editDialogCloseHandler = Bitrix.TypeUtility.createDelegate(this, this._handleEditDialogClose);
	this._initialized = false;
}

Bitrix.ComponentParameterViewGoogleMapsInitialState.prototype = {
	initialize: function(options) {
		if(!options) throw "CVGoogleMapsInitialState: options is not defined!";
		var id = this._id = "id" in options ? options.id : null;
		if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "CVGoogleMapsInitialState: id is not found!";
		this._dataInputId = id + "_Data"
		this._initialized = true;
	},
	createHtml: function(ID, nameForValueInput, captions, containerEl, replaceContainer){
		var wrapper = document.createElement("DIV");
		wrapper.id = ID;
		var data = document.createElement("INPUT");
		data.type = "hidden";
		wrapper.appendChild(data);
		data.id = this._dataInputId;
		data.name = nameForValueInput;
		
		var customData = this.getCustomData();
		if("EditorDialogData" in customData)
			data.value = Bitrix.GoogleMapsData.fromObject({ initialState: customData.EditorDialogData }).toString();
	
		var btn = document.createElement("INPUT");
		btn.type = "button";
		wrapper.appendChild(btn);
		btn.value = captions && "EditButtonTitle" in captions ? captions.EditButtonTitle : "Edit...";
		btn.onclick = Bitrix.TypeUtility.createDelegate(this, this._handleEditButtonClick);
		
		if(containerEl) {
			if(!replaceContainer) 
				containerEl.appendChild(wrapper);
			else {
				var parentNode = containerEl.parentNode;
				parentNode.replaceChild(wrapper, containerEl);
			}	
		}
		return [wrapper];
	},
	_handleEditButtonClick: function() {
		var dlgId = this._id + "_Dialog"
		var d = Bitrix.Dialog.get(dlgId);
		var customData = this.getCustomData();
		if(!d) d = Bitrix.GoogleMapsInitSettingsEditDialog.create(dlgId, "ComponentParameterViewGoogleMapsInitialStateDialog", "", { zIndex: 1100, data: "EditorDialogData" in customData ? customData.EditorDialogData : {} });
		d.addCloseListener(this._editDialogCloseHandler);
		d.open();
	},
	_handleEditDialogClose: function(sender, args) {
		if(!sender) return;
		sender.removeCloseListener(this._editDialogCloseHandler);
		if(args["buttonId"] != Bitrix.Dialog.button.bOk) return;
		var dlgData = sender.getData();
		var customData = this.getCustomData();
		customData.EditorDialogData = dlgData.toObject();
		document.getElementById(this._dataInputId).value = dlgData.toString(); 
	},
	/*ParamView*/
	getComponentId: function(){ return this._componentId; },
	setComponentId: function(val){ this._componentId = val;},
	getParameterName: function(){ return this._parameterName; },
	setParameterName: function(val){ this._parameterName = val; },
	getValue: function(){ return ""; },
	setValue: function(){ throw "Not supported!"; },
	prepare: function(){},
	addChangeListener: function(listener){
		if(this._changeEvent == null)
			this._changeEvent = new Bitrix.EventPublisher();
		this._changeEvent.addListener(listener);
	},	
	removeModelStateChangeListener: function(listener){
		if(this._changeEvent == null) return;
		this._changeEvent.removeListener(listener);
	},	
	getCustomData: function(){
		if(this._customData == null)
			this._customData = new Object();
		return this._customData;
	},
	setCustomData: function(value){
		this._customData = value;
	}
	/**/	
}
Bitrix.ComponentParameterViewGoogleMapsInitialState._entries = null;
Bitrix.ComponentParameterViewGoogleMapsInitialState.createEntry = function(options){
	if(!options) throw "CVGoogleMapsInitialState: options is not defined!";
	var id = "id" in options ? options.id : null;
	if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "CVGoogleMapsInitialState: id is not found!";
	
	var self = new Bitrix.ComponentParameterViewGoogleMapsInitialState();
	if(this._entries != null && id in this._entries) throw "'" + id + "' already exists!";
	self.initialize(options);
	(this._entries ? this._entries : (this._entries = {}))[id] = self;
	return self;
}
Bitrix.ComponentParameterViewGoogleMapsInitialState.create = function(options) {
	if(!(options instanceof Bitrix.ComponentParameterRegistrationOptions)) throw "Options is not valid!";
	var componentId = options.getComponentId();
	var parameterName = options.getParameterName();
	var id = options.getViewId();
	if(!Bitrix.TypeUtility.isNotEmptyString(id))
		id = componentId + "_" + parameterName;
		
	var containerEl = options.getContainerElement();
	var replaceContainer = options.getReplaceContainer();
	var customData = options.getCustomData();
	var nameForValueInput = "NameForValueInput" in customData ? customData.NameForValueInput : id;
	var captions = "Captions" in customData ? customData.Captions : null;
	
	this.removeEntry(id);
	var view = this.createEntry({ id: id });
	view.setComponentId(componentId);
	view.setParameterName(parameterName);
	view.setCustomData(customData);
	var elements = view.createHtml(id, nameForValueInput, captions, containerEl, replaceContainer);
	var result = Bitrix.ComponentParameterInfo.create(componentId, parameterName, elements, view);
	return result;	
}
Bitrix.ComponentParameterViewGoogleMapsInitialState.removeEntry = function(id){
	if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Id is not valid!";
	if(this._entries == null || !(id in this._entries)) return;
	delete this._entries[id];
}

Bitrix.ComponentParameterViewYandexMapsInitialState = function() {
    this._id = "";
    this._dataInputId = null;
    this._componentId = null;
    this._parameterName = null;
    this._changeEvent = null;
    this._customData = null;
    this._editDialogCloseHandler = Bitrix.TypeUtility.createDelegate(this, this._handleEditDialogClose);
    this._initialized = false;
}

Bitrix.ComponentParameterViewYandexMapsInitialState.prototype = {
    initialize: function(options) {
        if (!options) throw "CVYMapsInitialState: options is not defined!";
        var id = this._id = "id" in options ? options.id : null;
        if (!Bitrix.TypeUtility.isNotEmptyString(id)) throw "CVYMapsInitialState: id is not found!";
        this._dataInputId = id + "_Data"
        this._initialized = true;
    },
    createHtml: function(ID, nameForValueInput, captions, containerEl, replaceContainer) {
        var wrapper = document.createElement("DIV");
        wrapper.id = ID;
        var data = document.createElement("INPUT");
        data.type = "hidden";
        wrapper.appendChild(data);
        data.id = this._dataInputId;
        data.name = nameForValueInput;

        var customData = this.getCustomData();
        if ("EditorDialogData" in customData)
            data.value = Bitrix.YandexMapData.fromObject({ initialState: customData.EditorDialogData }).toString();

        var btn = document.createElement("INPUT");
        btn.type = "button";
        wrapper.appendChild(btn);
        btn.value = captions && "EditButtonTitle" in captions ? captions.EditButtonTitle : "Edit...";
        btn.onclick = Bitrix.TypeUtility.createDelegate(this, this._handleEditButtonClick);

        if (containerEl) {
            if (!replaceContainer)
                containerEl.appendChild(wrapper);
            else {
                var parentNode = containerEl.parentNode;
                parentNode.replaceChild(wrapper, containerEl);
            }
        }
        return [wrapper];
    },
    _handleEditButtonClick: function() {
        var dlgId = this._id + "_Dialog"
        var d = Bitrix.Dialog.get(dlgId);
        var customData = this.getCustomData();
        if (!d) d = Bitrix.YandexMapInitSettingsEditDialog.create(dlgId, "CVYMapsInitialStateDialog", "", { zIndex: 1100, data: "EditorDialogData" in customData ? customData.EditorDialogData : {} });
        d.addCloseListener(this._editDialogCloseHandler);
        d.open();
    },
    _handleEditDialogClose: function(sender, args) {
        if (!sender) return;
        sender.removeCloseListener(this._editDialogCloseHandler);
        if (args["buttonId"] != Bitrix.Dialog.button.bOk) return;
        var dlgData = sender.getData();
        var customData = this.getCustomData();
        customData.EditorDialogData = dlgData.toObject();
        document.getElementById(this._dataInputId).value = dlgData.toString();
    },
    /*ParamView*/
    getComponentId: function() { return this._componentId; },
    setComponentId: function(val) { this._componentId = val; },
    getParameterName: function() { return this._parameterName; },
    setParameterName: function(val) { this._parameterName = val; },
    getValue: function() { return ""; },
    setValue: function() { throw "Not supported!"; },
    prepare: function() { },
    addChangeListener: function(listener) {
        if (this._changeEvent == null)
            this._changeEvent = new Bitrix.EventPublisher();
        this._changeEvent.addListener(listener);
    },
    removeModelStateChangeListener: function(listener) {
        if (this._changeEvent == null) return;
        this._changeEvent.removeListener(listener);
    },
    getCustomData: function() {
        if (this._customData == null)
            this._customData = new Object();
        return this._customData;
    },
    setCustomData: function(value) {
        this._customData = value;
    }
    /**/
}
Bitrix.ComponentParameterViewYandexMapsInitialState._entries = null;
Bitrix.ComponentParameterViewYandexMapsInitialState.createEntry = function(options) {
    if (!options) throw "CVGoogleMapsInitialState: options is not defined!";
    var id = "id" in options ? options.id : null;
    if (!Bitrix.TypeUtility.isNotEmptyString(id)) throw "CVGoogleMapsInitialState: id is not found!";

    var self = new Bitrix.ComponentParameterViewYandexMapsInitialState();
    if (this._entries != null && id in this._entries) throw "'" + id + "' already exists!";
    self.initialize(options);
    (this._entries ? this._entries : (this._entries = {}))[id] = self;
    return self;
}
Bitrix.ComponentParameterViewYandexMapsInitialState.create = function(options) {
	if(typeof(YMaps) == "undefined") return null;
    if (!(options instanceof Bitrix.ComponentParameterRegistrationOptions)) throw "Options is not valid!";
    var componentId = options.getComponentId();
    var parameterName = options.getParameterName();
    var id = options.getViewId();
    if (!Bitrix.TypeUtility.isNotEmptyString(id))
        id = componentId + "_" + parameterName;

    var containerEl = options.getContainerElement();
    var replaceContainer = options.getReplaceContainer();
    var customData = options.getCustomData();
    var nameForValueInput = "NameForValueInput" in customData ? customData.NameForValueInput : id;
    var captions = "Captions" in customData ? customData.Captions : null;

    this.removeEntry(id);
    var view = this.createEntry({ id: id });
    view.setComponentId(componentId);
    view.setParameterName(parameterName);
    view.setCustomData(customData);
    var elements = view.createHtml(id, nameForValueInput, captions, containerEl, replaceContainer);
    var result = Bitrix.ComponentParameterInfo.create(componentId, parameterName, elements, view);
    return result;
}
Bitrix.ComponentParameterViewYandexMapsInitialState.removeEntry = function(id) {
    if (!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Id is not valid!";
    if (this._entries == null || !(id in this._entries)) return;
    delete this._entries[id];
}


if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded(); 


