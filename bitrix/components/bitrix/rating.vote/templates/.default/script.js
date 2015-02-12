if(typeof(Bitrix) == "undefined") {
	var Bitrix = new Object();
}

Bitrix.RatingVoting = function Bitrix$RatingVoting() {
	this._id = "";
	this._options = {};
}

Bitrix.RatingVoting.prototype = {
    initialize: function (id, options) {
        this._id = id;
        this._options = options;
    },
    setTotals: function (totals) {
        if (!this._options) this._options = new Object();
        this._options["totals"] = totals;
    },
    getOption: function (name, defVal) { return this._options && name in this._options ? this._options[name] : defVal; },
    _getContainer: function (name) {
        var id = this.getOption(name);
        return id ? document.getElementById(id) : null;
    },
    prepare: function () {
        var totalsEl = this._getContainer("totalsContainer");
        var plusContainerEl = this._getContainer("ratingContainer");

        var totals = this.getOption("totals");
        if (totals && totalsEl) {
            var val = "value" in totals ? totals.value : 0;
            totalsEl.innerHTML = val.toString();

            var votes = "votes" in totals ? totals.votes : 0;
            if (votes > 0) {
                var ttl = this.getOption("titleTemplate");
                if (ttl) {
                    ttl = ttl.replace(/#votes#/g, votes);
                    ttl = ttl.replace(/#posVotes#/g, "posVotes" in totals ? totals.posVotes : "n/a");
                    ttl = ttl.replace(/#negVotes#/g, "negVotes" in totals ? totals.negVotes : "n/a");
                    totalsEl.setAttribute("title", ttl);
                }
            }
            else
                totalsEl.setAttribute("title", this.getOption("noVotesMsg", ""));

            //var plus = this._getContainer("plusLink");
            //if (plus)
            //	plus.onclick = function() { return false; };

            //var minus = this._getContainer("minusLink");
            //if (minus)
            //	minus.onclick = function() { return false; };

            if (val != 0)
                totalsEl.className += val > 0 ? " rating-vote-result-plus" : " rating-vote-result-minus";

            if (plusContainerEl && !(this.getOption("isAllowed", false) && !this.tryGetBoolean(totals, "hasVoted", false)))
                plusContainerEl.className += " rating-vote-disabled";
        }
    },
    tryGetBoolean: function (object, name, def) {
        var v = object[name];
        if (typeof (v) == "boolean" || v instanceof Boolean) return v;
        else if (typeof (v) == "string" || v instanceof String) return v.toUpperCase() == "TRUE";
        else return def;
    },
    vote: function (sign) {
        var totals = this.getOption("totals");
        if (!(totals && this.getOption("isAllowed", false) && !this.tryGetBoolean(totals, "hasVoted", false))) return;

        var request = window.ActiveXObject ? new ActiveXObject("Microsoft.XMLHTTP") : new XMLHttpRequest();

        var url = this.getOption("votingActionUrl");
        url = url.replace(/#value#/g, sign ? this.getOption("positiveVoteValue") : this.getOption("negativeVoteValue"));
        url = url.replace(/#securityToken#/g, dotNetVars.securityTokenPair);

        request.open("GET", url, true);


        request.onreadystatechange = function () {
            if (request.readyState != 4) return;
            try {
                Bitrix.RatingVoting.receiveServerData(request.responseText);
            }
            catch (e) { }
        };
        request.send();
    }
}
Bitrix.RatingVoting._items = null;
Bitrix.RatingVoting.create = function(id, options) {
	if(this._items != null && (id in this._items))
		return this._items[id];
	
	var self = (this._items ? this._items : (this._items = new Object()))[id] = new Bitrix.RatingVoting();
	self.initialize(id, options);
	return self;
}
Bitrix.RatingVoting.get = function(id) {
	return this._items != null && (id in this._items) ? this._items[id] : null;
}

Bitrix.RatingVoting.vote = function(id, sign) {
	var item = this.get(id);
	if(item) item.vote(sign);
}
Bitrix.RatingVoting.receiveServerData = function(data) {
	try {
		var r = eval("(" + data + ")");
		if ("error" in r) {
			//Bitrix.RatingVoting.showServerError(r.error);
			return;
		}
		var item = "id" in r ? Bitrix.RatingVoting.get(r.id) : null;
		if (!item) return;
		if ("totals") {
			item.setTotals(r.totals);
			item.prepare();
		}
	}
	catch (e) { }
}