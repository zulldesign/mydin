if(typeof(Bitrix) == "undefined"){
	var Bitrix = new Object();
}

Bitrix.RatingPromoConditionEditor = function() {
    this._config = this._editor = this._ratingIdInput = this._thresholdInput = null;
}
Bitrix.RatingPromoConditionEditor.prototype = {
    initialize: function(config, editor) {
        this._config = config;
		this._editor = editor;
    },
    getVersion: function() { return 1; },
    getParamCount: function() { return 2; },
    getParamTitle: function(index) {
        switch (index) {
            case 0: return this.getMsg("RatingId.Label");
            case 1: return this.getMsg("Threshold.Label");
        }
        return null;
    },
    constructParamControls: function(index, container) {
		var cfg = this._config;
        switch (index) {
            case 0: {
					var boundEnityType = this._editor ? this._editor.getBoundEntityType() : "";
                    var input = this._ratingIdInput = document.createElement("SELECT");
                    if ("ratings" in cfg) {
						var opts = [];
                        for(var key in cfg.ratings) {
							var rating = cfg.ratings[key];
							if(boundEnityType.length > 0 && rating.boundEntityTypeId != boundEnityType) continue;
							opts.push( { 'text': rating.name, 'value': rating.id } );
						}
						Bitrix.DomHelper.addToSelect(input, opts);	
						if("ratingId" in cfg)
							Bitrix.DomHelper.selectOption(input, cfg.ratingId.toString());
                    }
					container.appendChild(input);
            }
            break;
			case 1: {
				 var input = this._thresholdInput = document.createElement("INPUT");
				 input.type = "text";
				 if ("threshold" in cfg)
					input.value = cfg.threshold;
				 container.appendChild(input);
			}
			break;
        }
    },
    _parseInt: function(str, defaultVal) {
        var r = parseInt(str.replace(/[^0-9]/g, ''));
        return !isNaN(r) ? r : defaultVal;
    },
    _parseFloat: function(str, defaultVal) {
        var r = parseFloat(str.replace(/[\,]/, '.').replace(/[^0-9,\.]/g, ''));
        return !isNaN(r) ? r : defaultVal;
    },	
    getConfigJson: function() {
        var r = "";
        var ratingIdInput = this._ratingIdInput;
        if (ratingIdInput != null && Bitrix.TypeUtility.isNotEmptyString(ratingIdInput.value))
            r += "\"ratingId\":" + this._parseInt(ratingIdInput.value, 0).toString();
		var thresholdInput = this._thresholdInput;
        if (thresholdInput != null && Bitrix.TypeUtility.isNotEmptyString(thresholdInput.value)) {
			if(r.length > 0) r += ", ";
            r += "\"threshold\":" + this._parseFloat(thresholdInput.value, 50).toString();
		}			
        return r.length > 0 ? "{" + r + "}" : "";
    },
    getMsg: function(key) {
        var c = window.MAIN_RATING_PROMO_CONDITION_CFG_ED_MSG;
        return typeof (c) != "undefined" && key in c ? c[key] : key;
    }
}

Bitrix.RatingPromoConditionEditor.create = function(config, editor) {
	var self = new Bitrix.RatingPromoConditionEditor();
	self.initialize(config, editor);
	return self;
}