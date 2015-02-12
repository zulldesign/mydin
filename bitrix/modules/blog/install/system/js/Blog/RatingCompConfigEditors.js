if(typeof(Bitrix) == "undefined"){
	var Bitrix = {};
}

Bitrix.BloggingActivityConfigEditor = function() {
	this._config = this._todayPostCoefValueInput = this._weekPostCoefValueInput = this._monthPostCoefValueInput = this._todayCommentCoefValueInput = this._weekCommentCoefValueInput = this._monthCommentCoefValueInput = null;
}
Bitrix.BloggingActivityConfigEditor.prototype = {
	initialize: function(config) {
		this._config = config;
	},
	_constructInputText: function(cfgKey, inputKey, container){
		var input = this[inputKey] = document.createElement("INPUT");
		input.type = "text";
		if(cfgKey in this._config) input.value = this._config[cfgKey];
		container.appendChild(input);
    },
    _parseFloat: function(str, defaultVal) {
        var r = parseFloat(str.replace(/[\,]/, '.').replace(/[^0-9,\.]/g, ''));
        return !isNaN(r) ? r : defaultVal;
    },	
	_numeric2Json: function(input, key, ary){
	    if (!(input != null && Bitrix.TypeUtility.isNotEmptyString(input.value))) return;
		    ary.push("\"" + key + "\":" + this._parseFloat(input.value, 1).toString());
	},
	getVersion: function() { return 1; },
	getParamCount: function() { return 6; },	
	getParamTitle: function(index) {
		switch(index) {
			case 0: return this.getMsg("DayPostsCoef.Label");
			case 1: return this.getMsg("WeekPostsCoef.Label");
			case 2: return this.getMsg("MonthPostsCoef.Label");
			case 3: return this.getMsg("DayCommentsCoef.Label");
			case 4: return this.getMsg("WeekCommentsCoef.Label");
			case 5: return this.getMsg("MonthCommentsCoef.Label");
		}
		return null;
	},
	constructParamControls: function(index, container) {
		switch(index) {
			case 0: 
				this._constructInputText("todayPostCoef", "_todayPostCoefValueInput", container); 
				break;
			case 1: 
				this._constructInputText("weekPostCoef", "_weekPostCoefValueInput", container);
				break;
			case 2: 
				this._constructInputText("monthPostCoef", "_monthPostCoefValueInput", container);
				break;
			case 3: 
				this._constructInputText("todayCommentCoef", "_todayCommentCoefValueInput", container);
				break;
			case 4: 
				this._constructInputText("weekCommentCoef", "_weekCommentCoefValueInput", container);
				break;
			case 5: 
				this._constructInputText("monthCommentCoef", "_monthCommentCoefValueInput", container);
				break;
		}		
	},
	getConfigJson: function() {
		var ary = [];
		this._numeric2Json(this._todayPostCoefValueInput, "todayPostCoef", ary);
		this._numeric2Json(this._weekPostCoefValueInput, "weekPostCoef", ary);
		this._numeric2Json(this._monthPostCoefValueInput, "monthPostCoef", ary);
		this._numeric2Json(this._todayCommentCoefValueInput, "todayCommentCoef", ary);
		this._numeric2Json(this._weekCommentCoefValueInput, "weekCommentCoef", ary);
		this._numeric2Json(this._monthCommentCoefValueInput, "monthCommentCoef", ary);
		var r = ary.join(", ");
		return r.length > 0 ? "{" + r + "}" : "";
	},
	getMsg: function(key) {
	    var c = window.BLOG_RATING_COMP_CFG_ED_MSG;
	    return typeof(c) != "undefined" && key in c ? c[key] : key; 
	}	
}

Bitrix.BloggingActivityConfigEditor.create = function(config) {
	var self = new Bitrix.BloggingActivityConfigEditor();
	self.initialize(config);
	return self;
}

Bitrix.BlogPostingActivityConfigEditor = function() {
	this._config = this._todayPostCoefValueInput = this._weekPostCoefValueInput = this._monthPostCoefValueInput = null;
}
Bitrix.BlogPostingActivityConfigEditor.prototype = {
	initialize: function(config) { this._config = config; },
	_constructInputText: function(cfgKey, inputKey, container){
		var input = this[inputKey] = document.createElement("INPUT");
		input.type = "text";
		if(cfgKey in this._config) input.value = this._config[cfgKey];
		container.appendChild(input);
    },
    _parseFloat: function(str, defaultVal) {
        var r = parseFloat(str.replace(/[\,]/, '.').replace(/[^0-9,\.]/g, ''));
        return !isNaN(r) ? r : defaultVal;
    },	
	_numeric2Json: function(input, key, ary){
	    if (!(input != null && Bitrix.TypeUtility.isNotEmptyString(input.value))) return;
		    ary.push("\"" + key + "\":" + this._parseFloat(input.value, 1).toString());
	},
	getVersion: function() { return 1; },
	getParamCount: function() { return 3; },	
	getParamTitle: function(index) {
		switch(index) {
			case 0: return this.getMsg("DayPostsCoef.Label");
			case 1: return this.getMsg("WeekPostsCoef.Label");
			case 2: return this.getMsg("MonthPostsCoef.Label");
		}
		return null;
	},
	constructParamControls: function(index, container) {
		switch(index) {
			case 0: 
				this._constructInputText("todayPostCoef", "_todayPostCoefValueInput", container); 
				break;
			case 1: 
				this._constructInputText("weekPostCoef", "_weekPostCoefValueInput", container);
				break;
			case 2: 
				this._constructInputText("monthPostCoef", "_monthPostCoefValueInput", container);
				break;
		}		
	},
	getConfigJson: function() {
		var ary = [];
		this._numeric2Json(this._todayPostCoefValueInput, "todayPostCoef", ary);
		this._numeric2Json(this._weekPostCoefValueInput, "weekPostCoef", ary);
		this._numeric2Json(this._monthPostCoefValueInput, "monthPostCoef", ary);
		var r = ary.join(", ");
		return r.length > 0 ? "{" + r + "}" : "";
	},
	getMsg: function(key) {
	    var c = window.BLOG_RATING_COMP_CFG_ED_MSG;
	    return typeof(c) != "undefined" && key in c ? c[key] : key; 
	}	
}

Bitrix.BlogPostingActivityConfigEditor.create = function(config) {
	var self = new Bitrix.BlogPostingActivityConfigEditor();
	self.initialize(config);
	return self;
}