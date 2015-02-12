if(typeof(Bitrix) == "undefined"){
	var Bitrix = new Object();
}

Bitrix.ForumActivityConfigEditor = function() {
	this._config = this._todayTopicCoefValueInput = this._weekTopicCoefValueInput = this._monthTopicCoefValueInput = this._todayPostCoefValueInput = this._weekPostCoefValueInput = this._monthPostCoefValueInput = null;
}
Bitrix.ForumActivityConfigEditor.prototype = {
	initialize: function(config) {
		this._config = config;
	},
	_constructInputText: function(cfgKey, inputKey, container){
		var input = this[inputKey] = document.createElement("INPUT");
		input.type = "text";
		if(cfgKey in this._config) input.value = this._config[cfgKey];
		container.appendChild(input);
	},
	getVersion: function() { return 1; },
	getParamCount: function() { return 6; },	
	getParamTitle: function(index) {
		switch(index) {
			case 0: return this.getMsg("DayTopicsCoef.Label");
			case 1: return this.getMsg("WeekTopicsCoef.Label");
			case 2: return this.getMsg("MonthTopicsCoef.Label");
			case 3: return this.getMsg("DayPostsCoef.Label");
			case 4: return this.getMsg("WeekPostsCoef.Label");
			case 5: return this.getMsg("MonthPostsCoef.Label");
		}
		return null;
	},
	constructParamControls: function(index, container) {
		switch(index) {
			case 0: 
				this._constructInputText("todayTopicCoef", "_todayTopicCoefValueInput", container); 
				break;
			case 1: 
				this._constructInputText("weekTopicCoef", "_weekTopicCoefValueInput", container);
				break;
			case 2: 
				this._constructInputText("monthTopicCoef", "_monthTopicCoefValueInput", container);
				break;
			case 3: 
				this._constructInputText("todayPostCoef", "_todayPostCoefValueInput", container);
				break;
			case 4: 
				this._constructInputText("weekPostCoef", "_weekPostCoefValueInput", container);
				break;
			case 5: 
				this._constructInputText("monthPostCoef", "_monthPostCoefValueInput", container);
				break;
		}
    },
    _parseFloat: function(str, defaultVal) {
        var r = parseFloat(str.replace(/[\,]/, '.').replace(/[^0-9,\.]/g, ''));
        return !isNaN(r) ? r : defaultVal;
    },	
	_numeric2Json: function(input, key, arr){
	    if (!(input != null && Bitrix.TypeUtility.isNotEmptyString(input.value))) return;
		    arr.push("\"" + key + "\":" + this._parseFloat(input.value, 1).toString());
	},
	getConfigJson: function() {
		var arr = [];
		this._numeric2Json(this._todayTopicCoefValueInput, "todayTopicCoef", arr);
		this._numeric2Json(this._weekTopicCoefValueInput, "weekTopicCoef", arr);
		this._numeric2Json(this._monthTopicCoefValueInput, "monthTopicCoef", arr);		
		this._numeric2Json(this._todayPostCoefValueInput, "todayPostCoef", arr);
		this._numeric2Json(this._weekPostCoefValueInput, "weekPostCoef", arr);
		this._numeric2Json(this._monthPostCoefValueInput, "monthPostCoef", arr);
		var r = arr.join(", ");
		return r.length > 0 ? "{" + r + "}" : "";
	},
	getMsg: function(key) {
	    var c = window.FORUM_RATING_COMP_CFG_ED_MSG;
	    return typeof(c) != "undefined" && key in c ? c[key] : key; 
	}	
}

Bitrix.ForumActivityConfigEditor.create = function(config) {
	var self = new Bitrix.ForumActivityConfigEditor();
	self.initialize(config);
	return self;
}