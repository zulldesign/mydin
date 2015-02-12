<%@ Page Language="C#" EnableEventValidation="false" EnableViewStateMac="false" Inherits="Bitrix.UI.BXJavaScriptLocalizationPage" %>
<%@ OutputCache Duration="2592000" Location="ServerAndClient" VaryByParam="lang;t" %>
<script runat="server" type="text/C#">
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		LanguageId = Request.QueryString["lang"];
        JavaScriptVariable = "window.BLOG_GROUP_PROMO_ACTION_CFG_ED_MSG";
	}
	protected override void Render(HtmlTextWriter writer)
	{
		base.Render(writer);
		Script.RenderControl(writer);
	}
</script>

<asp:PlaceHolder runat="server" ID="Script">
//<script type="text/javascript">

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

Bitrix.BlogGroupPromoActionConfigEditor = function() 
{
    this._config =  null;
    this._blogInput = null;
    this._groupInput = null;
    this._policyInput = null;
    
}
Bitrix.BlogGroupPromoActionConfigEditor.prototype = 
{
    initialize: function(config) {
        this._config = config;
    },
    getVersion: function() { return 1; },
    getParamCount: function() { return 2; },
    getParamTitle: function(index) 
    {
        switch (index) 
        {
            case 0: return this.getMsg("BlogGroup.Label");
            case 1: return this.getMsg("Policy.Label");
        }
        return null;
    },
    constructParamControls: function(index, container) 
    {
		var cfg = this._config;
        switch (index) 
        {
            case 0: 
            {
                var span = document.createElement("SPAN");
                span.style.whiteSpace = "nowrap";
                
                var input = this._blogInput = document.createElement("SELECT");
                var opts = [ { text: this.getMsg("BlogGroup.ChooseBlog"), value: "" }];
                if ("blogs" in cfg) 
                {
				    for(var i = 0; i < cfg.blogs.length; i++) 
                    {
						var blog = cfg.blogs[i];
						opts.push( { text: blog.title, value: blog.id } );
					}						
                }
                if (Bitrix.TypeUtility.isNotEmptyString(cfg.groupRequestUrl))
					BX.bind(input, "change", BX.delegate(this.changeBlog, this));
		        Bitrix.DomHelper.addToSelect(input, opts);
				if ("blog" in cfg)
					Bitrix.DomHelper.selectOption(input, cfg.blog);
				span.appendChild(input);
				
				span.appendChild(document.createTextNode(this.getMsg("BlogGroup.InGroup")));
				
				var input = this._groupInput = document.createElement("SELECT");
				var opts = [ { text: this.getMsg("BlogGroup.ChooseGroup"), value: "" }];
                if ("blogGroups" in cfg) 
                {						
                     for(var i = 0; i < cfg.blogGroups.length; i++) 
                    {
						var group = cfg.blogGroups[i];
						opts.push( { text: group.title, value: group.id } );
					}
                }
                Bitrix.DomHelper.addToSelect(input, opts);
                if ("blogGroup" in cfg)
					Bitrix.DomHelper.selectOption(input, cfg.blogGroup);
				span.appendChild(input);
				
				container.appendChild(span);
            }
            break;
			case 1: 
			{
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
    changeBlog: function() 
	{	
		var blog = this._blogInput;
		var group = this._groupInput;
		if (blog.bx_xhr && blog.bx_xhr.abort)
		{
			try { blog.bx_xhr.abort(); } catch(e) {}			
		}
		
		if (blog.selectedIndex <= 0)
		{
			blog.bx_xhr = null;			
			while (group.options.length > 1)
				group.remove(group.options.length - 1);
			return;
		}
		
		var v = "";
		var s = blog.selectedIndex;
		if (s >= 0)
			v = blog.options[s].value;

		blog.bx_xhr = BX.ajax.loadJSON(
			this._config.groupRequestUrl + 'id=' + encodeURIComponent(v),
			function(data)
			{
				blog.bx_xhr = null;
				
				var v = "";
				var s = group.selectedIndex;
				if (s >= 0)
					v = group.options[s].value;
								
				while (group.options.length > 1)
					group.remove(group.options.length - 1);
					
				for(var i = 0; i < data.length; i++)
				{
					var opt = new Option(data[i].title, data[i].value);
					try
					{
						group.add(opt, null);		
					}
					catch(e)
					{
						group.add(opt);		
					}
				}
					
				for(var i = group.options.length - 1; i >= 0; i--)
				{
					if (group.options[i].value == v)
					{
						group.selectedIndex = i;
						break;
					}
				}
			}
		);
	},
    getConfigJson: function() 
    {
        var json = "";
        
        var input = this._groupInput;
        if (input != null && Bitrix.TypeUtility.isNotEmptyString(input.value))
            json += "blogGroup:'" + input.value + "'";
			
		input = this._policyInput;
        if (input != null && Bitrix.TypeUtility.isNotEmptyString(input.value)) 
        {
			if(json.length > 0) 
				json += ",";
			
			var policies = [];
			for(var i = 0; i < input.options.length; i++)
				if(input[i].selected)
					policies.push(parseInt(input.options[i].value));
            json += "policy:'" + Bitrix.PromoActionPolicy.toString(policies) + "'";
		}
			
        return json.length > 0 ? "{" + json + "}" : "";
    },
    getMsg: function(key) 
    {
        var c = window.BLOG_GROUP_PROMO_ACTION_CFG_ED_MSG;
        return typeof (c) != "undefined" && key in c ? c[key] : key;
    }
}

Bitrix.BlogGroupPromoActionConfigEditor.create = function(config) 
{
	var self = new Bitrix.BlogGroupPromoActionConfigEditor();
	self.initialize(config);
	return self;
}

//</script>
</asp:PlaceHolder>