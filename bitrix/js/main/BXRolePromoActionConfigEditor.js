if(typeof(Bitrix) == "undefined"){
	var Bitrix = new Object();
}

if (typeof(Bitrix.PromoActionPolicy) == "undefined")
{
	Bitrix.PromoActionPolicy ={
		promotion: 	1,
		demotion: 	2
	}
	Bitrix.PromoActionPolicy.fromString = function(str){
		return Bitrix.EnumHelper.fromString(str, this);
	}
	Bitrix.PromoActionPolicy.toString = function(src){
		return Bitrix.EnumHelper.toString(src, this);
	}
	Bitrix.PromoActionPolicy.tryParse = function(src){
		return Bitrix.EnumHelper.tryParse(src, this);
	}
	Bitrix.PromoActionPolicy.getId = function(name){
		return Bitrix.EnumHelper.getId(this, name, true);
	}
}


Bitrix.RolePromoActionConfigEditor = function() {
    this._config = this._roleInput = this._policyInput = null;
}
Bitrix.RolePromoActionConfigEditor.prototype = {
    initialize: function(config) {
        this._config = config;
    },
    getVersion: function() { return 1; },
    getParamCount: function() { return 2; },
    getParamTitle: function(index) {
        switch (index) {
            case 0: return this.getMsg("Role.Label");
            case 1: return this.getMsg("Policy.Label");
        }
        return null;
    },
    constructParamControls: function(index, container) {
		var cfg = this._config;
        switch (index) {
            case 0: {
                    var input = this._roleInput = document.createElement("SELECT");
                    if ("roles" in cfg) {
						var opts = [];
                        for(var key in cfg.roles) {
							var role = cfg.roles[key];
							opts.push( { 'text': role.title, 'value': role.name } );
						}
						Bitrix.DomHelper.addToSelect(input, opts);
						if("roleName" in cfg)
							Bitrix.DomHelper.selectOption(input, cfg.roleName);
                    }
					container.appendChild(input);
            }
            break;
			case 1: {
                    var input = this._policyInput = document.createElement("SELECT");
					input.multiple = true;
                    if ("policies" in cfg) {
						var opts = [];
                        for(var key in cfg.policies) {
							var policy = cfg.policies[key];
							opts.push( { 'text': policy.title, 'value': Bitrix.PromoActionPolicy.getId(policy.name) } );
						}
						Bitrix.DomHelper.addToSelect(input, opts);
						for(var i = 0; i < input.options.length; i++)
							input.options[i].selected = true;
							
						if("policy" in cfg)
							Bitrix.DomHelper.selectOptions(input, Bitrix.PromoActionPolicy.fromString(cfg.policy));							
                    }
					container.appendChild(input);
			}
			break;
        }
    },
    getConfigJson: function() {
        var r = "";
        var roleNameInput = this._roleInput;
        if (roleNameInput != null && Bitrix.TypeUtility.isNotEmptyString(roleNameInput.value))
            r += "\"roleName\":\"" + roleNameInput.value + "\"";
			
        var policyInput = this._policyInput;
        if (policyInput != null && Bitrix.TypeUtility.isNotEmptyString(policyInput.value)) {
			if(r.length > 0) r+= ", ";
			var polAry = [];
			for(var i = 0; i < policyInput.options.length; i++)
				if(policyInput.options[i].selected)
					polAry.push(parseInt(policyInput.options[i].value));
            r += "\"policy\":\"" + Bitrix.PromoActionPolicy.toString(polAry) + "\"";
		}
			
        return r.length > 0 ? "{" + r + "}" : "";
    },
    getMsg: function(key) {
        var c = window.MAIN_ROLE_PROMO_ACTION_CFG_ED_MSG;
        return typeof (c) != "undefined" && key in c ? c[key] : key;
    }
}

Bitrix.RolePromoActionConfigEditor.create = function(config) {
	var self = new Bitrix.RolePromoActionConfigEditor();
	self.initialize(config);
	return self;
}