if(typeof(Bitrix) == "undefined"){
	var Bitrix = new Object();
}

Bitrix.RatingVotingConfigEditor = function() {
	this._config = this._coefValueInput = null;
}
Bitrix.RatingVotingConfigEditor.prototype = {
    initialize: function(config) {
        this._config = config;
    },
    getVersion: function() { return 1; },
    getParamCount: function() { return 1; },
    getParamTitle: function(index) {
        switch (index) {
            case 0: return this.getMsg("ConversionFactor.Label");
        }
        return null;
    },
    constructParamControls: function(index, container) {
        switch (index) {
            case 0:
                {
                    var input = this._coefValueInput = document.createElement("INPUT");
                    input.type = "text";
                    if ("coefficient" in this._config) input.value = this._config.coefficient;
                    container.appendChild(input);
                }
                break;
        }
    },
    _parseFloat: function(str, defaultVal) {
        var r = parseFloat(str.replace(/[\,]/, '.').replace(/[^0-9,\.]/g, ''));
        return !isNaN(r) ? r : defaultVal;
    },
    getConfigJson: function() {
        var r = "";
        var input = this._coefValueInput;
        if (input != null && Bitrix.TypeUtility.isNotEmptyString(input.value))
            r += "\"coefficient\":" + this._parseFloat(input.value, 1).toString();
        return r.length > 0 ? "{" + r + "}" : "";
    },
    getMsg: function(key) {
        var c = window.COMMUNICATION_UTILITY_RATINGVOTING_CFG_ED_MSG;
        return typeof (c) != "undefined" && key in c ? c[key] : key;
    }
}

Bitrix.RatingVotingConfigEditor.create = function(config) {
	var self = new Bitrix.RatingVotingConfigEditor();
	self.initialize(config);
	return self;
}