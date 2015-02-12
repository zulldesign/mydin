if(typeof(Bitrix) == "undefined"){
	var Bitrix = new Object();
}

if(typeof(Bitrix.TypeUtility) == "undefined"){
	Bitrix.TypeUtility = new Object();
	Bitrix.TypeUtility.isFunction = function(item){
		return typeof(item) == "function" || item instanceof Function;
	}
	Bitrix.TypeUtility.isNumber = function(item){
		return typeof(item) == "number" || item instanceof Number;
	}
	Bitrix.TypeUtility.isNotEmptyString = function(item){
		return typeof(item) == "string" || item instanceof String ? item.length > 0 : false;
	}
	Bitrix.TypeUtility.createDelegate = function(instance, method){
		return function() {
			return method.apply(instance, arguments);
		}
	}
	Bitrix.TypeUtility.returnFalse = function(){ return false; }
	Bitrix.TypeUtility.disableSelection = function(target){
		if(typeof(target.onselectstart) != "undefined"){//IE route
			target.onselectstart = Bitrix.TypeUtility.returnFalse;
		}
		if(typeof(target.ondrag) != "undefined"){//IE route
			target.ondrag = Bitrix.TypeUtility.returnFalse;
		}		
		else if(typeof target.style.MozUserSelect!="undefined"){//Firefox route
			target.style.MozUserSelect = "none";
		}
		else{//All other route (ie: Opera)
			target.onmousedown = Bitrix.TypeUtility.returnFalse;
		}
	}	
}

if(typeof(Bitrix.EventUtility) == "undefined"){
	Bitrix.EventUtility = new Object();
	Bitrix.EventUtility.addEventListener = (
		function(){
			if(document.addEventListener)   
				return function(element, name, handler, capture){ element.addEventListener(name, handler, capture); };
			if(document.attachEvent)
				return function(element, name, handler){ element.attachEvent("on" + name, handler); };
			return function(){ throw "Could not find appropriate function for event listening!"; };
		}
		)();
		
	Bitrix.EventUtility.stopEventPropagation = function(e){
		if(!e) e = window.event;
        if (e.stopPropagation) e.stopPropagation( );  // DOM Level 2
        else e.cancelBubble = true;                  // IE		
	}		
}

if(typeof(Bitrix.DomElementCache) == "undefined"){
	Bitrix.ElementCache = new Object();
	Bitrix.ElementCache._items = null;
	Bitrix.ElementCache.getById = function(id){
		var result = this._items != null ? this._items[id] : null;
		if(result)
			return result;
		result = document.getElementById(id);
		if(result){
			if(this._items == null) this._items = new Array();
			this._items[id] = result;
		}
		return result;
	}
}


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
}

if(typeof(Bitrix.StringUtility) == "undefined"){
	Bitrix.StringUtility = new Object();
	Bitrix.StringUtility.toString = function(value){
		if(typeof(value) == "object"){
			if(value == undefined) return "undefined";
			if(value == null) return "null";
			return value.toString();
		}
		return "?";
	}
}

Bitrix.WeekDay = {
	all: 		0,
	sunday: 	1,	//Su
	monday: 	2,	//Mo	
	tuesday: 	3,	//Tu
	wednesday: 	4,	//We
	thursday: 	5,	//Th
	friday: 	6,	//Fr
	saturday: 	7	//Sa	
}

Bitrix.WeekDay.containsValue = function(value){
	if(!(typeof(value) == "number" || value instanceof Number)) return false;
	return value >= 0 && value <= 7;
}

Bitrix.WeekDay.isDay = function(value){
	if(!(typeof(value) == "number" || value instanceof Number)) return false;
	return value >= 1 && value <= 7;
}

Bitrix.ClockType = {
	clock24h:	1,
	clock12h:	2
}

Bitrix.WeekScheduleEditor = function Bitrix$WeekScheduleEditor()
{
	this._initialized = false;
	this._constructed = false;
	this._id = "";
	this._dataContainerId = "";
	this._parentElementId = null;
	this._containerElementId = "";	
	this._elementId = "";	
	this._summaryElementId = "";
	this._summaryContentElementId = "";
	this._days = null;
	this._highlighter = null;
	this._highlightingChangeHandler = Bitrix.TypeUtility.createDelegate(this, this._handleHighlightingChange);
	this._highlightingFinishHandler = Bitrix.TypeUtility.createDelegate(this, this._handleHighlightingFinish);
	this._endOfRequestHandler = Bitrix.TypeUtility.createDelegate(this, this._handleEndOfRequest);
	this._refreshSummaryHandler = Bitrix.TypeUtility.createDelegate(this, this._refreshSummary);
	this._suppressHourChoiceChangeHandling = false;
	this._enableSummary = false;
	this._selectedPeriods = null;
	this._refreshSummaryTimerId = null;
}

Bitrix.WeekScheduleEditor.prototype = {
	initialize: function(id, dataContainerId, enableSummary) {
		if(!Bitrix.TypeUtility.isNotEmptyString(id)) throw "Id is not defined!";
		this._id = id;
		if(Bitrix.TypeUtility.isNotEmptyString(dataContainerId)) 
			this._dataContainerId = dataContainerId;
		this._highlighter = Bitrix.Highlighter.create();
		this._highlighter.addChangeHighlitingListener(this._highlightingChangeHandler);
		this._highlighter.addFinishMiltipleHighlitingListener(this._highlightingFinishHandler);
		
		if(typeof(enableSummary) != "undefined")
			this._enableSummary = enableSummary;
		this._initialized = true;
	},
	construct: function(parentElementId){
		if(!this._initialized) throw "Is not initailized!";
		if(this._constructed) throw "Already constructed!"
		this._parentElementId = parentElementId;	

		var parentElement = document.getElementById(this._parentElementId);
		if(!parentElement) throw "Parent element is not found!";		
		
		var containerDiv = document.createElement("DIV");
		containerDiv.id = this._containerElementId = this._getContainerElementId();
		parentElement.appendChild(containerDiv);
		
		
		if(this._enableSummary){
			var summaryDiv = document.createElement("DIV");
			summaryDiv.id = this._summaryElementId = this._getSummaryElementId();
			summaryDiv.className = "BXWeekScheduleSummary";
			containerDiv.appendChild(summaryDiv);
			var summaryLabel = document.createElement("SPAN");
			summaryLabel.className = "label";
			summaryLabel.innerHTML = Bitrix.WeekScheduleEditor.getSummaryText("title") + ":";
			summaryDiv.appendChild(summaryLabel);
			var summaryContent = document.createElement("SPAN");
			summaryContent.id = this._summaryContentElementId = this._getSummaryContentElementId();
			summaryContent.className = "content";
			summaryDiv.appendChild(summaryContent);			
		}		
		
		var tab = document.createElement("TABLE");
		tab.id = this._elementId = this._getElementId(); 
		tab.className = "BXWeekScheduleContent";
		tab.cellSpacing = "0px";
		tab.cellPadding = "0px";
		containerDiv.appendChild(tab);		
		
		var body = document.createElement("TBODY");
		tab.appendChild(body);
		
		var legendContainer = document.createElement("TABLE");
		legendContainer.className = "BXWeekScheduleLegendContainer";
		var legendContainerBody = document.createElement("TBODY");
		legendContainer.appendChild(legendContainerBody);
		
		var legendRow = document.createElement("TR");
		legendContainerBody.appendChild(legendRow);
		var legendChoiceIconTd = document.createElement("TD");
		legendRow.appendChild(legendChoiceIconTd);
		var legendChoiceIcon = document.createElement("DIV");
		legendChoiceIconTd.appendChild(legendChoiceIcon);
		legendChoiceIcon.className = "choice-icon";
		var legendChoiceText = document.createElement("TD");
		legendRow.appendChild(legendChoiceText);		
		legendChoiceText.innerHTML = "- " + Bitrix.WeekScheduleEditor.getLegendText("choiced");
		
		var legenSeparatorTd = document.createElement("TD");
		legendRow.appendChild(legenSeparatorTd);
		var legendSeparator = document.createElement("DIV");
		legenSeparatorTd.appendChild(legendSeparator);
		legendSeparator.className = "separator";
		
		var legendNotChoiceIconTd = document.createElement("TD");
		legendRow.appendChild(legendNotChoiceIconTd);
		var legendNotChoiceIcon = document.createElement("DIV");
		legendNotChoiceIconTd.appendChild(legendNotChoiceIcon);
		legendNotChoiceIcon.className = "not-choice-icon";
		var legendNotChoiceText = document.createElement("TD");
		legendRow.appendChild(legendNotChoiceText);		
		legendNotChoiceText.innerHTML = "- " + Bitrix.WeekScheduleEditor.getLegendText("notChoiced");		
		
		containerDiv.appendChild(legendContainer);
		
		this._days = new Array();
		var generalizedDay = null;
		for(var d = 0; d <= 7; d++){
			var dIndex = Bitrix.WeekScheduleEditor.getDayIndex(d);
			if(dIndex < 0) continue;
			
			var day = Bitrix.WeekScheduleDay.create(this, d, dIndex, generalizedDay);
			if(d == 0) generalizedDay = day;
			
			if(dIndex >= this._days.length)
				this._days.push(day);
			else
				this._days.splice(dIndex, 0, day);
		}
		
		var dIndex = 0;
		do{
			this._days[dIndex].construct();
			dIndex++;
		} while(dIndex < this._days.length);
		
		this._constructed = true;

		var dataContainer = this._getDataContainer();
		if(dataContainer){
			try { this.setSelectedPeriods(Bitrix.WeekPeriod.arrayFromJSONString(dataContainer.value)); }
			catch(e) {}
		}
		else{
			this._refreshSummary();
		}
		
	},
	_getContainerElementId: function(){
		return this._id + "_Container";
	},	
	_getElementId: function(){
		return this._id + "_Conent";
	},
	_getSummaryElementId: function(){
		return this._id + "_Summary";
	},
	_getSummaryContentElementId: function(){
		return this._id + "_SummaryContent";
	},	
	getContainerElement: function(){
		return this._containerElementId.length > 0 ? Bitrix.ElementCache.getById(this._containerElementId) : null;
	},
	getElement: function(){
		return this._elementId.length > 0 ? Bitrix.ElementCache.getById(this._elementId) : null;
	},
	getSummaryElement: function(){
		return this._summaryElementId.length > 0 ? Bitrix.ElementCache.getById(this._summaryElementId) : null;
	},	
	getSummaryContentElement: function(){
		return this._summaryContentElementId.length > 0 ? Bitrix.ElementCache.getById(this._summaryContentElementId) : null;
	},	
	_getDataContainer: function(){
		return this._dataContainerId.length > 0 ? Bitrix.ElementCache.getById(this._dataContainerId) : null;
	},
	getDayContainer: function(){
		var el = this.getElement();
		return el ? el.tBodies[0] : null;
	},
	getHighlighter: function(){
		return this._highlighter;
	},
	_handleHighlightingChange: function(highlighter, item, changeType){
		if(!this._constructed || !highlighter || highlighter != this._highlighter) return;
		if(changeType == Bitrix.HighlightingChangeType.itemOut) return;		
		
		var f = highlighter.getFirstHighlightedItem();
		var l = highlighter.getLastHighlightedItem();
		
		var fHour = f ? f.getClient() : null; 
		var lHour = l ? l.getClient() : null;
		
		if(!fHour || !lHour)
			return;
		
		var fDay = fHour.getParentDay();
		var fDayIndex = Bitrix.WeekScheduleEditor.getDayIndex(fDay.getDay());
		var fHourVal = fHour.getHour();
		var lDay = lHour.getParentDay();
		var lDayIndex = Bitrix.WeekScheduleEditor.getDayIndex(lDay.getDay());
		var lHourVal = lHour.getHour();
				
		var tlDayIndex = 0, brDayIndex = 0;
		if(fDayIndex <= lDayIndex ){
			tlDayIndex = fDayIndex;
			brDayIndex = lDayIndex;
		}
		else{
			tlDayIndex = lDayIndex;
			brDayIndex = fDayIndex;		
		}
		
		var tlHourVal = 0, brHourVal = 0;
		if(fHourVal <= lHourVal ){
			tlHourVal = fHourVal;
			brHourVal = lHourVal;
		}
		else{
			tlHourVal = lHourVal;
			brHourVal = fHourVal;	
		}
		
		var highlightedRange = new Array();
		for(var i = tlDayIndex; i <= brDayIndex; i++){
			var day = this._days[i];
			day.getHourHighlightingRange(tlHourVal, brHourVal - tlHourVal + 1, highlightedRange);
		}
		this._highlighter.setHighlighting(f, l, highlightedRange);
	},
	_handleHighlightingFinish: function(highlighter, item, changeType){
		if(!this._constructed || !highlighter || highlighter != this._highlighter) return;
		if(changeType == Bitrix.HighlightingChangeType.itemOut) return;
		
		var f = highlighter.getFirstHighlightedItem();
		var l = highlighter.getLastHighlightedItem();
		var fHour = f ? f.getClient() : null; 
		var lHour = l ? l.getClient() : null;
		
		if(!fHour || !lHour)
			return; 
		
		var fDay = fHour.getParentDay();
		var fDayIndex = Bitrix.WeekScheduleEditor.getDayIndex(fDay.getDay());
		var fHourVal = fHour.getHour();
		var lDay = lHour.getParentDay();
		var lDayIndex = Bitrix.WeekScheduleEditor.getDayIndex(lDay.getDay());
		var lHourVal = lHour.getHour();
		
		if(fDayIndex != lDayIndex || fHourVal != lHourVal){
			var highlightedItems = this._highlighter.getHighlightedItems();
			var choice = !fHour.getChoice();
			for(var key in highlightedItems){
				h = highlightedItems[key].getClient();
				h.getParentDay().setHourChoice(h.getHour(), choice);
			}		
		}
		this._highlighter.collapseHighlighting();
	},
	_handleEndOfRequest: function(){
		this._externalizeData();
		return true;
	},
	_getDayById: function(dayId){
		if(!this._constructed) return null;
		for(var i = 0; i < this._days.length; i++)
			if(this._days[i].getDay() == dayId) 
				return this._days[i];
		return null;
	},
	handleHourChoiceChange: function(hour){
		if(this._suppressHourChoiceChangeHandling) return;
		if(this._selectedPeriods != null) this._selectedPeriods = null;
		if(this._refreshSummaryTimerId != null){
			window.clearTimeout(this._refreshSummaryTimerId);
			this._refreshSummaryTimerId = null;
		}
		this._refreshSummaryTimerId = window.setTimeout(this._refreshSummaryHandler, 300);
	},
	_externalizeData: function(){
		var dataContainer = this._getDataContainer();
		if(!dataContainer) return;
		try {
			dataContainer.value  = Bitrix.WeekPeriod.arrayToJSONString(this.getSelectedPeriods());
		}
		catch(e) {}		
	},
	clearSelectedPeriods: function(){
		this._suppressHourChoiceChangeHandling = false;
		this._clearSelectedPeriods();
		this._suppressHourChoiceChangeHandling = false;
		if(this._selectedPeriods != null) this._selectedPeriods = null;
		this._externalizeData();	
		this._refreshSummary();
	},
	_clearSelectedPeriods: function(){
		if(!this._constructed) throw "Is not constructed!";
		for(var i = 0; i < this._days.length; i++)
			this._days[i].clearHourChoice();
	},	
	getSelectedPeriods: function(){
		if(!this._constructed) throw "Is not constructed!";
		//if(this._selectedPeriods != null) return this._selectedPeriods;
		this._selectedPeriods = new Array();
		var current = null;
		for(var i = 0; i < this._days.length; i++){
			var day = this._days[i];
			if(day.getDay() == Bitrix.WeekDay.all)
				continue;
			var hours = day.getHours();
			for(var j = 0; j < hours.length; j++){
				var hour = hours[j];
				if(!hour.getChoice()) continue;
				if(current != null && hour.getHourOfWeek() - current.getFinishHourOfWeek() == 1){
					current.setFinishDay(hour.getParentDay().getDay());
					current.setFinishHour(hour.getHour());
					continue;
				}
				current = Bitrix.WeekPeriod.create(hour.getParentDay().getDay(), hour.getHour() - 1, hour.getParentDay().getDay(), hour.getHour());
				this._selectedPeriods.push(current);
			}
		}
		return this._selectedPeriods;
	},
	setSelectedPeriods: function(periods){
		if(!this._constructed) throw "Is not constructed!";
		if(!periods || !(periods instanceof Array)){ 
			this._refreshSummary();
			return;
		}
		this._clearSelectedPeriods();
		var results = new Array();
		for(var i = 0; i < periods.length; i++){
			var period = periods[i];
			var startDay = this._getDayById(period.getStartDay());
			if(!startDay) continue;
			var finishDay = this._getDayById(period.getFinishDay());
			if(!finishDay) continue;
			var startHour = startDay.getHourByValue(period.getStartHour() + 1);
			if(!startHour) continue;
			var finishHour = finishDay.getHourByValue(period.getFinishHour());
			if(!finishHour) continue;
			var startHourOfWeek =  startHour.getHourOfWeek();
			var finishHourOfWeek = finishHour.getHourOfWeek();
			
			var result = null;
			if(startHourOfWeek <= finishHourOfWeek){
				result = new Object();
				result["start"] = startHourOfWeek;
				result["finish"] = finishHourOfWeek;
				results.push(result);
			}
			else{
				result = new Object();
				result["start"] = startHourOfWeek;
				result["finish"] = 168;
				results.push(result);
				
				result = new Object();
				result["start"] = 1;
				result["finish"] = finishHourOfWeek;
				results.push(result);				
			}
		}
		
		this._suppressHourChoiceChangeHandling = true;
		for(var j = 0; j < this._days.length; j++)
			this._days[j].setChoiceByRangeHourOfWeek(results);
		this._suppressHourChoiceChangeHandling = false;
		if(this._selectedPeriods != null) this._selectedPeriods = null;
		this._externalizeData();
		this._refreshSummary();
	},
	isSummaryEnabled: function(){
		return this._enableSummary;
	},
	enableSummary: function(enable){
		this._enableSummary = enable;
	},
	_refreshSummary: function(){
		var summaryEl = null;
		if(!this._enableSummary || !(summaryEl = this.getSummaryContentElement())) return;
		
		var nothingChoiced = true;
		var everythingChoiced = true;
		
		var choicedDays = new Array();
		for(var i = 0; i < this._days.length; i++){
			var day = this._days[i];
			if(day.getDay() == Bitrix.WeekDay.all)
				continue;
			var hours = day.getHours();
			var leastHourChoiced = false;
			for(var j = 0; j < hours.length; j++){
				var hour = hours[j];
				if(!hour.getChoice() && everythingChoiced)
					everythingChoiced = false;
				if(hour.getChoice() && nothingChoiced)
					nothingChoiced = false;				
				if(hour.getChoice() && !leastHourChoiced)
					leastHourChoiced = true;
				if(!everythingChoiced && !nothingChoiced && leastHourChoiced) break;
			}
			
			if(leastHourChoiced) 
				choicedDays.push(day.getDay());
		}
		
		var summaryText = "";
		if(choicedDays.length == 0)
			summaryText += Bitrix.WeekScheduleEditor.getSummaryText("displayNever");
		else if(choicedDays.length == 7)
			summaryText +=  Bitrix.WeekScheduleEditor.getSummaryText("displayEveryDay");
		else{
			for(var i = 0; i < choicedDays.length; i++){
				if(i > 0)
					summaryText += ", ";
				summaryText += Bitrix.WeekScheduleEditor.getAbbreviatedDayName(choicedDays[i]);
			}
		}
		summaryEl.innerHTML = summaryText;
	}
}

Bitrix.WeekScheduleEditor._items = null;
Bitrix.WeekScheduleEditor.create = function(id, dataContainerId, enableSummary){
    var self = new Bitrix.WeekScheduleEditor();
    self.initialize(id, dataContainerId, enableSummary);
    if(this._items == null)
        this._items = new Object();
    this._items[id] = self;
    return self;
}

Bitrix.WeekScheduleEditor.getItemById = function(id){
    return this._items != null && id in this._items ? this._items[id] : null;
}

Bitrix.WeekScheduleEditor.getDayName = function(day){
	switch(day){
		case Bitrix.WeekDay.all:
			return this.days["all"];
		case Bitrix.WeekDay.sunday:
			return this.days["sunday"];
		case Bitrix.WeekDay.monday:
			return this.days["monday"];
		case Bitrix.WeekDay.tuesday:
			return this.days["tuesday"];
		case Bitrix.WeekDay.wednesday:
			return this.days["wednesday"];			
		case Bitrix.WeekDay.thursday:
			return this.days["thursday"];				
		case Bitrix.WeekDay.friday:
			return this.days["friday"];			
		case Bitrix.WeekDay.saturday:
			return this.days["saturday"];				
	}
	throw "Unknown week day " + day.toString() + "!";
}

Bitrix.WeekScheduleEditor.getAbbreviatedDayName = function(day){
	switch(day){
		case Bitrix.WeekDay.sunday:
			return this.abbreviatedDays["sunday"];
		case Bitrix.WeekDay.monday:
			return this.abbreviatedDays["monday"];
		case Bitrix.WeekDay.tuesday:
			return this.abbreviatedDays["tuesday"];
		case Bitrix.WeekDay.wednesday:
			return this.abbreviatedDays["wednesday"];			
		case Bitrix.WeekDay.thursday:
			return this.abbreviatedDays["thursday"];				
		case Bitrix.WeekDay.friday:
			return this.abbreviatedDays["friday"];			
		case Bitrix.WeekDay.saturday:
			return this.abbreviatedDays["saturday"];				
	}
	throw "Unknown week day " + day.toString() + "!";
}

Bitrix.WeekScheduleEditor.getSummaryText = function(id){
	return id in Bitrix.WeekScheduleEditor.summaryText ? Bitrix.WeekScheduleEditor.summaryText[id] : "";
}

Bitrix.WeekScheduleEditor.getLegendText = function(id){
	return id in Bitrix.WeekScheduleEditor.legendText ? Bitrix.WeekScheduleEditor.legendText[id] : "";
}

Bitrix.WeekScheduleEditor.getDayJSONAlias = function(day){
	if(!Bitrix.WeekDay.containsValue(day)) throw "'"+ Bitrix.StringUtility.toString(day) +" ' is not a week day!";
	if(!("dayJSONAliases" in this)) 
		return day.toString();
	else if(day == Bitrix.WeekDay.sunday && "sunday" in this.dayJSONAliases)
		return this.dayJSONAliases["sunday"];
	else if(day == Bitrix.WeekDay.monday && "monday" in this.dayJSONAliases)
		return this.dayJSONAliases["monday"];		
	else if(day == Bitrix.WeekDay.tuesday && "tuesday" in this.dayJSONAliases)
		return this.dayJSONAliases["tuesday"];			
	else if(day == Bitrix.WeekDay.wednesday && "wednesday" in this.dayJSONAliases)
		return this.dayJSONAliases["wednesday"];			
	else if(day == Bitrix.WeekDay.thursday && "thursday" in this.dayJSONAliases)
		return this.dayJSONAliases["thursday"];				
	else if(day == Bitrix.WeekDay.friday && "friday" in this.dayJSONAliases)
		return this.dayJSONAliases["friday"];	
	else if(day == Bitrix.WeekDay.saturday && "saturday" in this.dayJSONAliases)
		return this.dayJSONAliases["saturday"];			
	else	
		return day.toString();
}

Bitrix.WeekScheduleEditor.getDayByJSONAlias = function(alias){
	for(var key in this.dayJSONAliases){
		if(this.dayJSONAliases[key] != alias) continue;
		return key in Bitrix.WeekDay ? Bitrix.WeekDay[key] : -1;
	}
}

Bitrix.WeekScheduleEditor.getDayIndex = function(day){
	if(day == Bitrix.WeekDay.all) return 0;
	var firstDayOfWeek = "firstDayOfWeek" in this ? this["firstDayOfWeek"] : Bitrix.WeekDay.sunday;
	if(day == firstDayOfWeek) return 1;
	var offset = 1 - firstDayOfWeek;
	var result = day + offset;
	if(result <= 0)
		result += 7;
	return result;
}

Bitrix.WeekScheduleEditor.getHourName = function(hour){
	if(this.clockType == Bitrix.ClockType.clock24h)
		return this.getHourName24(hour);
	if(this.clockType == Bitrix.ClockType.clock12h)
		return this.getHourName12(hour);	
	throw "'" + this.clockType.toString() + "' is unknown clock type!";
}

Bitrix.WeekScheduleEditor.enableGeneralizedDayHoursDisplay = true;

Bitrix.WeekScheduleEditor.getHourName24 = function(hour){
	if(hour < 0 || hour > 24) throw "Hour '" + hour.toString() + "' is not valid!";
	return (hour - 1).toString();
}

Bitrix.WeekScheduleEditor.getHourName12 = function(hour){
	if(hour < 0 || hour > 24) throw "Hour '" + hour.toString() + "' is not valid!";
	if(hour == 1) return "12";
	if(hour < 13) return (hour - 1).toString();
	if(hour == 13) return "12";
	return (hour - 13).toString()
}

Bitrix.WeekScheduleEditor.days = {
	all: 		"All",
	sunday: 	"Sunday",
	monday: 	"Monday",
	tuesday: 	"Tuesday",
	wednesday: 	"Wednesday",
	thursday: 	"Thursday",
	friday: 	"Friday",
	saturday: 	"Saturday"
};

Bitrix.WeekScheduleEditor.abbreviatedDays = {
	sunday: 	"Su",
	monday: 	"Mo",
	tuesday: 	"Tu",
	wednesday: 	"We",
	thursday: 	"Th",
	friday: 	"Fr",
	saturday: 	"Sa"
}

Bitrix.WeekScheduleEditor.summaryText = {
	title: "Display",
	displayEveryDay: "Every day",
	displayNever: "Never"
};

Bitrix.WeekScheduleEditor.legendText = {
	choiced: "choiced",
	notChoiced: "not choiced"
};

Bitrix.WeekScheduleEditor.dayJSONAliases = {
	sunday: 	"Su",
	monday: 	"Mo",
	tuesday: 	"Tu",
	wednesday: 	"We",
	thursday: 	"Th",
	friday: 	"Fr",
	saturday: 	"Sa"
};

Bitrix.WeekScheduleEditor.firstDayOfWeek = Bitrix.WeekDay.sunday;
Bitrix.WeekScheduleEditor.clockType = Bitrix.ClockType.clock24h;

Bitrix.WeekScheduleDay = function Bitrix$WeekScheduleDay(){
	this._elementId = null;
	this._dayCellElementId = null;
	this._dayCellElementClickHandler = Bitrix.TypeUtility.createDelegate(this, this._handleDayCellElementClick);
	this._dayCellElementMouseOverHandler = Bitrix.TypeUtility.createDelegate(this, this._handleDayCellElementMouseOver);
	this._dayCellElementMouseOutHandler = Bitrix.TypeUtility.createDelegate(this, this._handleDayCellElementMouseOut);	
	this._highlightingChangeHandler = Bitrix.TypeUtility.createDelegate(this, this._handleHighlightingChange);
	this._parentControl = null;
	this._emphasis = false;
	this._choice = false;
	this._day = null;
	this._hours = null;
	this._dayIndex = -1;
	this._days = null;
	this._genaralizedDay = null;
	this._suppressGenaralizedEventsHandling = false;
	this._initialized = false;
	this._constructed = false;
}

Bitrix.WeekScheduleDay.prototype = {
	initialize: function(parentControl, day, dayIndex, genaralizedDay){
		this._parentControl = parentControl;
		this._day = day;
		this._dayIndex = dayIndex;
		if(genaralizedDay){
			this._genaralizedDay = genaralizedDay;
			this._genaralizedDay.registerDay(this);
		}
		this._parentControl.getHighlighter().addChangeHighlitingListener(this._highlightingChangeHandler);
		this._initialized = true;
	},
	getDay: function(){
		return this._day;
	},
	getDayIndex: function(){
		return this._dayIndex;
	},
	isGeneralized: function(){
		return this._day == Bitrix.WeekDay.all;
	},
	registerDay: function(day){
		if(!this.isGeneralized()) throw "Is not generalized!";
		if(this._days == null) this._days = new Array();
		this._days.push(day);
	},
	_getElementId: function(){
		var r = "Day_" + this._day.toString();
		var parentEl = this._parentControl.getElement();
		if(!parentEl) throw "Could not find parent element!";
		return parentEl.id.length > 0 ? parentEl.id + "_" + r : r;
	},	
	getElement: function(){
		//return document.getElementById(this._elementId);
		return Bitrix.ElementCache.getById(this._elementId);
	},
	_getElementClassName: function(){
		return this.isGeneralized() ? "weekDayRowGeneralized" : "weekDayRow";
	},		
	getHourContainer: function(){
		return this.getElement();
	},
	_getDayCellElementId: function(){
		return this._getElementId() + "_Header";
	},	
	_getDayCellElementClassName: function(){
		return this._choice ? (this._emphasis ? "dayChoiceEmphasis" : "dayChoice") : (this._emphasis ? "dayEmphasis" : "day");
	},	
	_getDayCellElement: function(){
		//return document.getElementById(this._dayCellElementId);
		return Bitrix.ElementCache.getById(this._dayCellElementId);
	},
	getEmphasis: function(){
		return this._emphasis;
	},	
	getChoice: function(){
		return this._choice;
	},
	setChoice: function(choice){
		if(choice && this._choice) return;
		this._choice = choice;
		if(!this.isGeneralized() && this._hours != null)
			for(var i = 0; i < this._hours.length; i++)
				this._hours[i].setChoice(this._choice);	
		this._handleHourChange(null);
	},	
	getHourChoice: function(hour){
		if(hour < 0 || hour > 24 || this._hours == null) return;
		if(this._hours.length < 24) throw "Hour array is not valid!";
		return this._hours[hour - 1].getChoice();	
	},
	setHourChoice: function(hour, choice){
		if(hour < 0 || hour > 24 || this._hours == null) return;
		if(this._hours.length < 24) throw "Hour array is not valid!";
		var hourItem = this._hours[hour - 1];
		hourItem.setChoice(choice);
		this._handleHourChange(hourItem);
	},
	setChoiceByRangeHourOfWeek: function(hourOfWeekRangeArray){
		if(!this._constructed) throw "Is not constructed!";
		if(!hourOfWeekRangeArray) return;
		var dIndex = Bitrix.WeekScheduleEditor.getDayIndex(this._day);
		if(dIndex <= 0) return;
		var dOffset = (dIndex - 1) * 24;
		var dStartHour = dOffset + 1;
		var dFinishHour = dStartHour + 23;
		var processed = 0;
		for(var i = 0; i < hourOfWeekRangeArray.length; i++){
			var range = hourOfWeekRangeArray[i];
			if(dStartHour > range.finish || dFinishHour < range.start) continue;
			var startHourInd = (dStartHour >= range.start ? dStartHour : range.start) - dOffset - 1;
			var finishHourInd = (dFinishHour <= range.finish ? dFinishHour : range.finish) - dOffset - 1;
			for(var j = startHourInd; j <= finishHourInd; j++)
				this._hours[j].setChoice(true);	
			processed++;
		}
		if(processed > 0)
			this._handleHourChange(null);
	},
	getHours: function(){
		return this._hours;
	},
	_checkHourValue: function(value){
		return (typeof(value) == "number" || value instanceof Number) && value >= 1 && value <= 24;
	},	
	getHourByValue: function(hourValue){
		if(!this._constructed || !this._checkHourValue(hourValue)) return null;
		return this._hours[hourValue - 1];
	},
	getHourHighlightingRange: function(startHour, length, destinationRange){
		if(!startHour) startHour = 1;
		if(startHour < 0 || startHour > 24) throw "Is not valid start hour!";
		if(!length) length = 24;
		if(length < 0 || length > 24) throw "Is not valid length!";
		if(!this._constructed || length == 0) return null;
		var result = destinationRange instanceof Array ? destinationRange : new Array();
		var curLength = 0;
		for(var i = startHour - 1; i < this._hours.length && curLength < length; i++){
			var hl = this._hours[i].getHighlighting();
			if(hl) result.push(hl);
			curLength++;
		}
		return result;
	},
	getParentControl: function(){
		return this._parentControl;
	},
	refresh: function(skipHours){
		var dayCellElement = this._getDayCellElement();
		if(dayCellElement)
			dayCellElement.className = this._getDayCellElementClassName();
			
		if(skipHours || this._hours == null) return;
		for(var i = 0; i < this._hours.length; i++)
			this._hours[i].refresh();
	},	
	_handleDayCellElementClick: function(){
		this.setChoice(!this._choice);
		if(this.isGeneralized()){
			this._suppressGenaralizedEventsHandling = true;
			if(this._days != null)
				for(var i = 0; i < this._days.length; i++)
					this._days[i].setChoice(this._choice);
			if(this._hours != null)
				for(var j = 0; j < this._hours.length; j++)
					this._hours[j].setChoice(this._choice);	
			this._suppressGenaralizedEventsHandling = false;
			this.refresh(true);
		}			
	},	
	_handleDayCellElementMouseOver: function(e){
		if(this._emphasis) return;
		this._emphasis = true;
		this.refresh(true);
		Bitrix.EventUtility.stopEventPropagation(e);
	},
	_handleDayCellElementMouseOut: function(e){
		if(!this._emphasis) return;
		this._emphasis = false;
		this.refresh(true);	
		Bitrix.EventUtility.stopEventPropagation(e);
	},
	handleHourDoubleClick: function(hour){
		if(!hour || this._hours == null) return;
		if(this._hours.length < 24) throw "Hour array is not valid!";
		
		var h = hour.getHour();
		var s = !hour.getChoice();
		while(h > 0){
			var curHour = this._hours[h - 1];
			if(curHour.getChoice() == s) break;
			this.setHourChoice(curHour.getHour(), s);
			h--;
		}
	},	
	handleHourClick: function(hour){
		this._handleHourChange(hour);	
	},
	handleHourChangeGeneralized: function(day, hour){
		if(!this.isGeneralized()) throw "Is not generalized!";
		if(this._suppressGenaralizedEventsHandling || this._days == null) return;
		if(hour)
			this._handleHourChangeGeneralized(hour);
		else if(day){
			var dayHours = day.getHours();
			if(dayHours)
				for(var i = 0; i < dayHours.length; i++)
					this._handleHourChangeGeneralized(dayHours[i]);
		}
		
		var aboutDayChoice = true;
		var dayChoice = false;
		for(var j = 0; j < this._days.length && aboutDayChoice; j++){
			if(j == 0){
				dayChoice = this._days[j].getChoice();
				continue;
			}
			aboutDayChoice = this._days[j].getChoice() == dayChoice;		
		}
		this._choice = aboutDayChoice ? dayChoice : false;
		
		var dayCellElement = this._getDayCellElement();
		if(dayCellElement)
			dayCellElement.className = this._getDayCellElementClassName();			
	},
	_handleHourChangeGeneralized: function(hour){
		if(this._days == null) return;
		var h = hour.getHour();
		var s = hour.getChoice();
		var aboutHourChoice = true;
		for(var i = 0; i < this._days.length && aboutHourChoice; i++)
			aboutHourChoice = this._days[i].getHourChoice(h) == s;
			
		if(this._hours.length < 24) throw "Hour array is not valid!";
		this._hours[h - 1].setChoice(aboutHourChoice ? s : false);	
	},
	_handleHourChange: function(hour){	
		if(!this._constructed) return;
		if(!this.isGeneralized()){
			var s = (hour ? hour : this._hours[0]).getChoice();	
			var aboutDayChoice = true;
			for(var i = 0; i < 24 && aboutDayChoice; i++)
				aboutDayChoice = this._hours[i].getChoice() == s;
			this._choice = aboutDayChoice ? s : false;			
			if(this._genaralizedDay)
				this._genaralizedDay.handleHourChangeGeneralized(this, hour);
		}
		else{
			if(!hour || this._days == null) return;
			var s = hour.getChoice();
			for(var i = 0; i < this._days.length; i++)
				this._days[i].setHourChoice(hour.getHour(), s);			
		}
		var dayCellElement = this._getDayCellElement();
		if(dayCellElement)
			dayCellElement.className = this._getDayCellElementClassName();		
	},
	_handleHighlightingChange: function(highlighter, item, changeType){
		if(!highlighter || !this._constructed) return;
		var l = highlighter.getLastHighlightedItem();
		var highlightedHour = 0;
		var lHour = l ? l.getClient() : null;		
		if(this.isGeneralized()){
			if(lHour){
				if(lHour.getParentDay().getDay() == Bitrix.WeekDay.all)
					return;
				highlightedHour = lHour.getHour();
			}
			for(var i = 0; i < this._hours.length; i++){
				var h = this._hours[i];
				h.setEmphasis(highlightedHour > 0 ? h.getHour() == highlightedHour : false);
			}
		}
		else{
			var emphasis = lHour ? lHour.getParentDay().getDay() == this._day : false;
			if(this._emphasis == emphasis) return;
			this._emphasis = emphasis;
			this.refresh();			
		}
	},
	clearHourChoice: function(){
		if(!this._constructed) throw "Is not constructed!";
		var processed = 0;
		for(var i = 0; i < this._hours.length; i++){
			var h = this._hours[i];
			if(!h.getChoice()) continue;
			h.setChoice(false);
			processed++;
		}
		
		if(processed > 0)
			this._handleHourChange(null);
	},	
	isConstructed: function(){
		return this._constructed;
	},
	construct: function(){
		if(this._constructed) throw "Already constructed!";
		var el = document.createElement("TR");	
		el.id = this._elementId = this._getElementId();
		el.className = this._getElementClassName();

		var parentElement = this._parentControl.getDayContainer();
		if(!parentElement) throw "Parent element is not found!";			
		parentElement.appendChild(el);
		
		var elDayCell = document.createElement("TD");
		elDayCell.id = this._dayCellElementId = this._getDayCellElementId();
		elDayCell.className = this._getDayCellElementClassName();
		elDayCell.onclick = this._dayCellElementClickHandler;
		Bitrix.TypeUtility.disableSelection(elDayCell);
		var elDayCellContent = document.createElement("DIV");
		elDayCellContent.className = "dayLabel";
		elDayCellContent.disabled = true;
		Bitrix.TypeUtility.disableSelection(elDayCellContent);
		elDayCellContent.innerHTML = Bitrix.WeekScheduleEditor.getDayName(this._day).toString();
		elDayCell.appendChild(elDayCellContent);
		
		Bitrix.EventUtility.addEventListener(elDayCell, "mouseover", this._dayCellElementMouseOverHandler, false);	
		Bitrix.EventUtility.addEventListener(elDayCell, "mouseout", this._dayCellElementMouseOutHandler, false);	 
		
		el.appendChild(elDayCell);
		
		this._hours = new Array();
		//var enableChoice = !this.isGeneralized();
		for(var h = 1; h <= 24; h++){
			var hour = Bitrix.WeekScheduleHour.create(this._parentControl, this, h);
			hour.setChoice(this._choice);
			//hour.enableHourDisplay(this.isGeneralized() ? h % 2 == 0 : false);
			hour.enableHourDisplay(this.isGeneralized() && Bitrix.WeekScheduleEditor.enableGeneralizedDayHoursDisplay);
			//hour.enableHighlighting(!this.isGeneralized());
			hour.construct();
			this._hours.push(hour);			
		}
		this._constructed = true;
	}
}

Bitrix.WeekScheduleDay.create = function(parentControl, day, dayIndex, genaralizedDay){
	var self = new Bitrix.WeekScheduleDay();
	self.initialize(parentControl, day, dayIndex, genaralizedDay);
	return self;
}

Bitrix.WeekScheduleHour = function Bitrix$WeekScheduleHour(){
	this._parentControl = null;
	this._parentDay = null;
	this._hour = 0;
	this._hourOfWeek = -1;
	this._elementId = null;	
	this._elementClickHandler = Bitrix.TypeUtility.createDelegate(this, this._handleElementClick);	
	this._elementDoubleClickHandler = Bitrix.TypeUtility.createDelegate(this, this._handleElementDoubleClick);
	this._elementHighlightChangeHandler = Bitrix.TypeUtility.createDelegate(this, this._handleElementHighlightChange);		
	
	this._emphasis = false;
	this._choice = false;
	this._enableChoice = true;
	this._enableHourDisplay = true;
	this._enableHighlighting = true;
	this._highlighting = null;
	this._constructed = false;
	this._initialized = false;
}
Bitrix.WeekScheduleHour.prototype = {
	initialize: function(parentControl, parentDay, hour, enableChoice){
		this._parentControl = parentControl;
		this._parentDay = parentDay;
		this._hour = hour;
		if(this._parentDay.getDay() != Bitrix.WeekDay.all)
			this._hourOfWeek = ((Bitrix.WeekScheduleEditor.getDayIndex(this._parentDay.getDay()) - 1) * 24) + this._hour;
			
		if(enableChoice)
			this._enableChoice = enableChoice;
			
		this._initialized = true;
	},
	getHour: function(){
		return this._hour;
	},
	getHourOfWeek: function(){
		return this._hourOfWeek;
	},
	getParentControl: function(){
		return this._parentControl;
	},	
	getParentDay: function(){
		return this._parentDay;
	},
	_getElementId: function(){
		var r = "Hour_" + this._hour.toString();
		var parentEl = this._parentDay.getElement();
		if(!parentEl) throw "Could not find parent element!";
		return parentEl.id.length > 0 ? parentEl.id + "_" + r : r;
	},
	getElement: function(){
		//return document.getElementById(this._elementId);
		return Bitrix.ElementCache.getById(this._elementId);
	},
	getEmphasis: function(){
		return this._emphasis;
	},
	setEmphasis: function(emphasis){
		if(this._emphasis == emphasis) return;
		this._emphasis = !this._emphasis;
		this.refresh();
	},
	getChoice: function(){
		return this._choice;
	},
	setChoice: function(choice){
		if(this._choice == choice) return;
		this._choice = !this._choice;
		this._parentControl.handleHourChoiceChange(this);
		this.refresh();
	},	
	isChoiceEnabled: function(){
		return this._enableChoice;
	},
	enableChoice: function(enable){
		this._enableChoice = enable;
		this.setChoice(false);
	},
	isHourDisplayEnabled: function(){
		return this._enableHourDisplay;
	},
	enableHourDisplay: function(enable){
		this._enableHourDisplay = enable;
	},
	isHighlightingEnabled: function(){
		return this._enableHighlighting;
	},
	enableHighlighting: function(enable){
		this._enableHighlighting = enable;
	},
	getHighlighting: function(){
		return this._highlighting;
	},
	refresh: function(){
		var element = this.getElement();
		if(element)
			element.className = this._getElementClassName();	
	},
	_handleElementClick: function(){
		if(this._enableChoice)
			this.setChoice(!this._choice);
		if(this._parentDay)
			this._parentDay.handleHourClick(this);		
	},
	_handleElementDoubleClick: function(){
		if(this._parentDay)
			this._parentDay.handleHourDoubleClick(this);
	},	
	_handleElementHighlightChange: function(highlight){
		this.setEmphasis(highlight);
	},
	_getElementClassName: function(){
		return this._choice ? (this._emphasis ? "hourChoiceEmphasis" : "hourChoice") : (this._emphasis ? "hourEmphasis" : "hour");
	},	
	isConstructed: function(){
		return this._constructed;
	},
	construct: function(){	
		if(this._constructed) throw "Already constructed!";
		var el = document.createElement("TD");
		el.id = this._elementId = this._getElementId();
		el.className = this._getElementClassName();	
		el.onclick = this._elementClickHandler;
		el.ondblclick = this._elementDoubleClickHandler;
		Bitrix.TypeUtility.disableSelection(el);
		var parentElement = this._parentDay.getHourContainer();
		if(!parentElement) throw "Parent element is not found!";		
		parentElement.appendChild(el);		
		
		var elContent = document.createElement("DIV");
		elContent.className = "hourLabel";
		elContent.disabled = true;
		Bitrix.TypeUtility.disableSelection(elContent);
		if(this._enableHourDisplay)
			elContent.innerHTML = Bitrix.WeekScheduleEditor.getHourName(this._hour);
		el.appendChild(elContent);
	
		if(this._enableHighlighting)
			this._highlighting = this._parentDay.getParentControl().getHighlighter().registerClient(this, "Bitrix$WeekScheduleHour", "Bitrix$WeekScheduleDay" + this._parentDay.getDay() +"Hour" + this._hour, this._elementHighlightChangeHandler, [el]);	
		this._constructed = true;
	}
}
Bitrix.WeekScheduleHour.create = function(parentControl, parentDay, hour, enableChoice){
	var self = new Bitrix.WeekScheduleHour();
	self.initialize(parentControl, parentDay, hour, enableChoice);
	return self;
}

Bitrix.HighlightingChangeType = {
	itemIn:	 	1,
	itemOut:	2
}

Bitrix.Highlighter = function Bitrix$Highlighter(){
	this._items = null;
	this._highlightedItems = null;
	this._multiple = false;
	this._firstHighlighted = null;
	this._lastHighlighted = null;
	this._keepLastHighlighted = false;

	this._documentMouseUpHandler = Bitrix.TypeUtility.createDelegate(this, this._handleDocumentMouseUp);
	this._startMiltipleHighlitingEvent = null;
	this._changeHighlitingEvent = null;
	this._finishMiltipleHighlitingEvent = null;
	this._initialized = false;
}
Bitrix.Highlighter.prototype = {
	initialize: function(){
		Bitrix.EventUtility.addEventListener(document, "mouseup", this._documentMouseUpHandler, false);	
		this._initialized = true;
	},
	registerClient: function(client, clientTypeName, clientKey, clientRefreshCallBack,  watchedElements){
		if(this._items == null)
			this._items = new Object();
		if(clientKey in this._items) throw "Clent '"+ clientKey +"' is already registered!";
		var item = Bitrix.HighlighterItem.create(this, client, clientTypeName, clientKey, clientRefreshCallBack,  watchedElements);
		this._items[clientKey] = item;
		return item;
	},
	isMultiple: function(){ 
		return this._multiple; 
	},
	addStartMiltipleHighlitingListener: function(listener){
		if(this._startMiltipleHighlitingEvent == null)
			this._startMiltipleHighlitingEvent = new Bitrix.EventPublisher();
		this._startMiltipleHighlitingEvent.addListener(listener);
	},
	removeStartMiltipleHighlitingListener: function(listener){
		if(this._startMiltipleHighlitingEvent != null)
			this._startMiltipleHighlitingEvent.removeListener(listener);
	},	
	addChangeHighlitingListener: function(listener){
		if(this._changeHighlitingEvent == null)
			this._changeHighlitingEvent = new Bitrix.EventPublisher();
		this._changeHighlitingEvent.addListener(listener);
	},
	removeChangeHighlitingListener: function(listener){
		if(this._changeHighlitingEvent != null)
			this._changeHighlitingEvent.removeListener(listener);
	},		
	addFinishMiltipleHighlitingListener: function(listener){
		if(this._finishMiltipleHighlitingEvent == null)
			this._finishMiltipleHighlitingEvent = new Bitrix.EventPublisher();
		this._finishMiltipleHighlitingEvent.addListener(listener);
	},
	removeFinishMiltipleHighlitingListener: function(listener){
		if(this._finishMiltipleHighlitingEvent != null)
			this._finishMiltipleHighlitingEvent.removeListener(listener);
	},		
	getFirstHighlightedItem: function(){
		return this._firstHighlighted;
	}, 
	getLastHighlightedItem: function(){
		return this._lastHighlighted;
	},
	getHighlightedItems: function(){
		return this._highlightedItems;
	},
	setHighlighting: function(first, last, items){
		if(!(items instanceof Array)) throw "Is not defined!";
		this.resetHighlighting();
		this._firstHighlighted  = first;
		this._lastHighlighted = last;
		for(var i = 0; i < items.length; i++)
			this.addToHighlighting(items[i], true);
	},
	resetHighlighting: function(){
		if(this._highlightedItems == null) return;	
		for(var key in this._highlightedItems){
			var item = this._highlightedItems[key];
			delete this._highlightedItems[key];
			item.setHighlighted(false);
			if(this._firstHighlighted != null && this._firstHighlighted.getClientKey() == item.getClientKey())
				this._firstHighlighted = null;
			if(this._lastHighlighted != null && this._lastHighlighted.getClientKey() == item.getClientKey())
				this._lastHighlighted = null;				
		}
		this._keepLastHighlighted = false;
	},
	collapseHighlighting: function(){
		if(this._highlightedItems == null) return;	
		for(var key in this._highlightedItems){
			var item = this._highlightedItems[key];
			if(this._lastHighlighted != null && this._lastHighlighted.getClientKey() == item.getClientKey()) continue;
			delete this._highlightedItems[key];
			item.setHighlighted(false);
			if(this._firstHighlighted != null && this._firstHighlighted.getClientKey() == item.getClientKey())
				this._firstHighlighted = null;			
		}
		this._firstHighlighted = this._lastHighlighted;
	},
	addToHighlighting: function(item, skipBoundaryItemsAssign){
		if(item == null) return;
		if(this._highlightedItems == null) this._highlightedItems = new Object();
		if(item.getClientKey() in this._highlightedItems)
			item = this._highlightedItems[item.getClientKey()];
		else{
			this._highlightedItems[item.getClientKey()] = item;
			item.setHighlighted(true);
		}
		if(skipBoundaryItemsAssign) return;
		this._lastHighlighted = item;
		if(!this._firstHighlighted || !this._multiple) 
			this._firstHighlighted = item;
	},
	removeFromHighlighting: function(item){
		if(item == null || (this._highlightedItems != null && !(item.getClientKey() in this._highlightedItems))) return;
		if(this._keepLastHighlighted && this._lastHighlighted != null && this._lastHighlighted.getClientKey() == item.getClientKey())
			return;
		delete this._highlightedItems[item.getClientKey()];
		item.setHighlighted(false);
		if(this._firstHighlighted != null && this._firstHighlighted.getClientKey() == item.getClientKey())
			this._firstHighlighted = null;
		if(this._lastHighlighted != null && this._lastHighlighted.getClientKey() == item.getClientKey())
			this._lastHighlighted = null;	
	},
	handleItemMouseOver: function(item){
		if(!item) return;
		this.addToHighlighting(item);
		if(this._changeHighlitingEvent )
			this._changeHighlitingEvent.fire(this, item, Bitrix.HighlightingChangeType.itemIn);
	},
	handleItemMouseOut: function(item){
		if(!item) return;	
		if(this._changeHighlitingEvent )
			this._changeHighlitingEvent.fire(this, item, Bitrix.HighlightingChangeType.itemOut);		
		if(!this._multiple)
			this.removeFromHighlighting(item);
		this._keepLastHighlighted = false;		
	},
	handleItemMouseUp: function(item){
		if(!item) return;	
		this._multiple = false;
		this._keepLastHighlighted = true;
		if(this._finishMiltipleHighlitingEvent != null)
			this._finishMiltipleHighlitingEvent.fire(this, item);		
	},
	handleItemMouseDown: function(item){
		if(!item) return;
		this.collapseHighlighting();
		this._multiple = true;
		if(this._startMiltipleHighlitingEvent)
			this._startMiltipleHighlitingEvent.fire(this, item);
	},
	handleItemMouseClick: function(item){
		if(!item) return;
		this._multiple = false;
		this.collapseHighlighting();
	},
	_handleDocumentMouseUp: function(){
		if(!this._multiple) return;
		this._multiple = false;
		if(this._finishMiltipleHighlitingEvent != null)
			this._finishMiltipleHighlitingEvent.fire(this, this._lastHighlighted);			
	}
}

Bitrix.Highlighter.create = function(){
	var self = new Bitrix.Highlighter();
	self.initialize();
	return self;
}
Bitrix.HighlighterItem = function Bitrix$HighlighterItem(){
	this._highlighter = null
	this._client = null;
	this._clientTypeName = null;
	this._clientKey = null;
	this._clientRefreshCallBack = null;
	this._watchedElements = null;
	this._elementMouseUpHandler = Bitrix.TypeUtility.createDelegate(this, this._handleElementMouseUp);
	this._elementMouseDownHandler = Bitrix.TypeUtility.createDelegate(this, this._handleElementMouseDown);
	this._elementMouseClickHandler = Bitrix.TypeUtility.createDelegate(this, this._handleElementMouseClick);
	this._elementMouseOverHandler = Bitrix.TypeUtility.createDelegate(this, this._handleElementMouseOver);
	this._elementMouseOutHandler = Bitrix.TypeUtility.createDelegate(this, this._handleElementMouseOut);
	this._highlighted = false;
	this._initialized = false;
}
Bitrix.HighlighterItem.prototype = {
	initialize: function(highlighter, client, clientTypeName, clientKey, clientRefreshCallBack,  watchedElements){
		if(!highlighter) throw "Highlighter is not defined!";
		this._highlighter = highlighter;
		
		if(!client) throw "Client is not defined!";
		this._client = client;
		
		if(!clientTypeName) throw "Client type name key is not defined!";
		this._clientTypeName = clientTypeName;
		
		if(!clientKey) throw "Client key is not defined!";
		this._clientKey = clientKey;
		
		if(typeof(clientRefreshCallBack) != "function") throw "Client refresh callBack is not defined!";
		this._clientRefreshCallBack = clientRefreshCallBack;
		
		if(!watchedElements) throw "Watched elements is not defined!";
		this._watchedElements = watchedElements;
		
		for(var i = 0; i < this._watchedElements.length; i++)
			this._addEventLesteners(this._watchedElements[i]);
			
		this._initialized = true;
	},
	getClient: function(){
		return this._client;
	},	
	getClientTypeName: function(){
		return this._clientTypeName;
	},	
	getClientKey: function(){
		return this._clientKey;
	},
	getHighlighted: function(){
		return this._highlighted;
	},
	setHighlighted: function(highlighted){
		if(this._highlighted == highlighted)
			return;
		this._highlighted = highlighted;
		this._clientRefreshCallBack(highlighted);
	},
	_addEventLesteners: function(element){
		Bitrix.EventUtility.addEventListener(element, "mouseup", this._elementMouseUpHandler, false);
		Bitrix.EventUtility.addEventListener(element, "mousedown", this._elementMouseDownHandler, false);
		Bitrix.EventUtility.addEventListener(element, "click", this._elementMouseClickHandler, false);
		Bitrix.EventUtility.addEventListener(element, "mouseover", this._elementMouseOverHandler, false);
		Bitrix.EventUtility.addEventListener(element, "mouseout", this._elementMouseOutHandler, false);
	},
	_handleElementMouseUp: function(e){
		this._highlighter.handleItemMouseUp(this);
		//Bitrix.EventUtility.stopEventPropagation(e);
	},
	_handleElementMouseDown: function(e){
		this._highlighter.handleItemMouseDown(this);
		//Bitrix.EventUtility.stopEventPropagation(e);
	},
	_handleElementMouseClick: function(e){
		this._highlighter.handleItemMouseClick(this);
		//Bitrix.EventUtility.stopEventPropagation(e);
	},
	_handleElementMouseOver: function(e){
		this._highlighter.handleItemMouseOver(this);
		//Bitrix.EventUtility.stopEventPropagation(e);
	},
	_handleElementMouseOut: function(e){
		this._highlighter.handleItemMouseOut(this);
		//Bitrix.EventUtility.stopEventPropagation(e);
	}
}

Bitrix.HighlighterItem.create = function(highlighter, client, clientTypeName, clientKey, clientRefreshCallBack,  watchedElements){
	var self = new Bitrix.HighlighterItem();
	self.initialize(highlighter, client, clientTypeName, clientKey, clientRefreshCallBack,  watchedElements);
	return self;
}

Bitrix.WeekPeriod = function Bitrix$WeekPeriod(){
	this._startDay = null;
	this._startHour = null;
	this._finishDay = null;
	this._finishHour = null;
	this._initialized = false;
}

Bitrix.WeekPeriod.prototype = {
	initialize: function(startDay, startHour, finishDay, finishHour){
		if(!Bitrix.WeekDay.isDay(startDay)) throw "Start day '" + Bitrix.StringUtility.toString(startDay) + "' is not valid";
		if(!Bitrix.WeekDay.isDay(finishDay)) throw "Finish day '" + Bitrix.StringUtility.toString(finishDay) + "' is not valid";		
		//if(Bitrix.WeekScheduleEditor.getDayIndex(startDay) > Bitrix.WeekScheduleEditor.getDayIndex(finishDay)) throw "Start day value must be less than or equal to finish day value!";	
		if(!this._checkHour(startHour)) throw "Start hour '" + Bitrix.StringUtility.toString(startHour) + "' is not valid!";
		if(!this._checkHour(finishHour)) throw "Finish hour '" + Bitrix.StringUtility.toString(finishHour) + "' is not valid!";
		if(startDay == finishDay && startHour > finishHour) throw "Start hour value must be less than or equal to finish hour value!";
		this._startDay = startDay;
		this._startHour = startHour;
		this._finishDay = finishDay;
		this._finishHour = finishHour;		
		
		this._initialized = true;
	},
	initializeFromObject: function(object){
		if(!object || typeof(object) != "object") throw "Object is not defined or invalid";
		var startDay = -1, startHour = -1, finishDay = -1, finishHour = -1;
		if("startDay" in object) startDay = object["startDay"];
		else if("startDayName" in object) startDay = Bitrix.WeekScheduleEditor.getDayByJSONAlias(object["startDayName"]);
		if("startHour" in object) startHour = object["startHour"];
		if("finishDay" in object) finishDay = object["finishDay"];
		else if("finishDayName" in object) finishDay = Bitrix.WeekScheduleEditor.getDayByJSONAlias(object["finishDayName"]);	
		if("finishHour" in object) finishHour = object["finishHour"];
		this.initialize(startDay, startHour, finishDay, finishHour);
	},
	_checkHour: function(hour){
		return (typeof(hour) == "number" || hour instanceof Number) && hour >= 0 && hour <= 24;
	},
	getStartDay: function(){
		return this._startDay;
	},
	setStartDay: function(day){
		if(!Bitrix.WeekDay.isDay(day)) throw "'" + Bitrix.StringUtility.toString(day) + "' is not valid day";
		//if(Bitrix.WeekScheduleEditor.getDayIndex(day) > Bitrix.WeekScheduleEditor.getDayIndex(this._finishDay)) throw "Start day value must be less than or equal to finish day value!";
		this._startDay = day;
	},
	getStartHour: function(){
		return this._startHour;
	},
	getStartHourOfWeek: function(){
		return 24 * (Bitrix.WeekScheduleEditor.getDayIndex(this._startDay) - 1) + this._startHour;
	},		
	setStartHour: function(hour){
		if(!this._checkHour(hour)) throw  "'" + Bitrix.StringUtility.toString(hour) + "' is not valid hour!";
		if(this._startDay == this._finishDay && hour > this._finishHour) throw "Start hour value must be less than or equal to finish hour value!";
		this._startHour = hour;
	},
	getFinishDay: function(){
		return this._finishDay;
	},
	setFinishDay: function(day){
		if(!Bitrix.WeekDay.isDay(day)) throw "'" + Bitrix.StringUtility.toString(day) + "' is not valid day";
		//if(Bitrix.WeekScheduleEditor.getDayIndex(day) < Bitrix.WeekScheduleEditor.getDayIndex(this._startDay)) throw "Finish day value must be greater than or equal to start day value!";
		this._finishDay = day;
	},	
	getFinishHour: function(){
		return this._finishHour;
	},
	getFinishHourOfWeek: function(){
		return 24 * (Bitrix.WeekScheduleEditor.getDayIndex(this._finishDay) - 1) + this._finishHour;
	},	
	setFinishHour: function(hour){
		if(!this._checkHour(hour)) throw  "'" + Bitrix.StringUtility.toString(hour) + "' is not valid hour!";
		if(this._startDay == this._finishDay && hour < this._startHour) throw "Finish hour value must be greater than or equal to start hour value!";
		this._finishHour = hour;
	},
	toJSONString: function(){
		return '{ '.concat('"startDayName": "', Bitrix.WeekScheduleEditor.getDayJSONAlias(this._startDay), '", "startHour":', this._startHour.toString(), ', "finishDayName": "', Bitrix.WeekScheduleEditor.getDayJSONAlias(this._finishDay), '", "finishHour":', this._finishHour.toString(), ' }');
	}
}

Bitrix.WeekPeriod.arrayToJSONString = function(array){
	var result = '[';
	for(var i = 0; i < array.length; i++)
		result = i > 0 ? result.concat(', ', array[i].toJSONString()) : result.concat(array[i].toJSONString());
	return result.concat(']');
}

Bitrix.WeekPeriod.arrayFromJSONString = function(string){
	var result = null;
	var srcData = null;
	try{
		srcData = eval(string);
		if(srcData instanceof Array)
			for(var i = 0; i < srcData.length; i++){
				if(result == null) result = new Array();
				result.push(this.createFromObject(srcData[i]));
			}		
	}
	catch(e){
		return null;
	}
	return result;
}

Bitrix.WeekPeriod.create = function(startDay, startHour, finishDay, finishHour){
	var self = new Bitrix.WeekPeriod();
	self.initialize(startDay, startHour, finishDay, finishHour);
	return self;
}

Bitrix.WeekPeriod.createFromObject = function(object){
	var self = new Bitrix.WeekPeriod();
	self.initializeFromObject(object);
	return self;
}
