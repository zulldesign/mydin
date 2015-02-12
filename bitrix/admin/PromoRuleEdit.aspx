<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" 
	AutoEventWireup="false" CodeFile="PromoRuleEdit.aspx.cs" Inherits="bitrix_admin_PromoRuleEdit" %>
	
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<bx:BXContextMenuToolbar ID="EditToolbar" runat="server" OnCommandClick="OnToolBarButtonClick">
		<Items>
			<bx:BXCmSeparator ID="ListButton" runat="server" SectionSeparator="true" />
			<bx:BXCmImageButton CssClass="context-button icon btn_list" CommandName="list"
				Text="<%$ LocRaw:ActionText.GoBack %>" Title="<%$ LocRaw:ActionTitle.GoBack %>" Href="PromoRuleList.aspx" />
			<bx:BXCmSeparator SectionSeparator="true" />
			<bx:BXCmImageButton ID="AddButton" CssClass="context-button icon btn_new" CommandName="add"
				Text="<%$ LocRaw:Kernel.Add %>" Title="<%$ LocRaw:ActionTitle.Add %>" Href="PromoRuleEdit.aspx" />
			<bx:BXCmSeparator SectionSeparator="true" />
			<bx:BXCmImageButton ID="DeleteButton" CssClass="context-button icon btn_delete" CommandName="delete"
				Text="<%$ LocRaw:Kernel.Delete %>" Title="<%$ LocRaw:ActionTitle.Delete %>" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>"/>
		</Items>
	</bx:BXContextMenuToolbar>
	<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="Main" />
	<bx:BXTabControl ID="TabControl" runat="server" OnCommand="OnTabControlCommand" ValidationGroup="Main">
		<bx:BXTabControlTab ID="MainSettingsTab" Runat="server" Selected="True" Text="<%$ LocRaw:TabText.MainSettings %>" Title="<%$ LocRaw:TabTitle.MainSettings %>">
			<table class="edit-table" cellspacing="0" cellpadding="0" border="0">
				<% if (EntityId > 0){ %>
		        <tr valign="top"><td class="field-name" width="25%">ID:</td><td width="30%"><%= EntityId.ToString() %></td></tr>
				<% } %>
				<tr valign="top" title="<%= GetMessage("FieldTooltip.Active") %>">
					<td class="field-name"><%= GetMessage("FieldLabel.Active") %>:</td>
					<td><asp:CheckBox ID="Active" runat="server" /></td>
					<td></td>
				</tr>				

				<tr valign="top" title="<%= GetMessage("FieldTooltip.Name") %>">
					<td class="field-name"><span class="required">*</span><%= GetMessage("FieldLabel.Name") %>:</td>
					<td>
						<asp:TextBox ID="Name" runat="server" Width="350px" />
						<asp:RequiredFieldValidator ID="NameRequiredValidator" runat="server" ValidationGroup="Main" ControlToValidate="Name" ErrorMessage="<%$ Loc:Message.NameNotSpecified %>" Display="Dynamic">*</asp:RequiredFieldValidator>
					</td>
					<td></td>
				</tr>				
		
                <tr valign="top" title="<%= GetMessage("FieldTooltip.BoundEntityType") %>">
					<td class="field-name"><span class="required">*</span><%= GetMessage("FieldLabel.BoundEntityType")%>:</td>
					<td>
                        <asp:DropDownList ID="BoundEntityType" runat="server" >
                        </asp:DropDownList>
                        <asp:CustomValidator ID="BoundEntityTypeValidator" runat="server" 
                            ValidationGroup="Main" ControlToValidate="BoundEntityType" 
                            ErrorMessage="<%$ Loc:Message.BoundEntityType %>"  Display="Dynamic" ValidateEmptyText="true"
                            ClientValidationFunction="Bitrix.PromoRuleEditor.validateDropDownList" 
                            OnServerValidate="ValidateDropDownList">*</asp:CustomValidator>
					</td>
					<td></td>
				</tr>				
												
				<tr valign="top" title="<%= GetMessage("FieldTooltip.XmlId") %>" >
					<td class="field-name"><%= GetMessage("FieldLabel.XmlId") %>:</td>
					<td>
						<asp:TextBox Rows="7" ID="XmlId"  runat="server" Width="350px" />						
					</td>
					<td></td>
				</tr>			
			</table>
			<table class="edit-table" cellspacing="0" cellpadding="0" border="0">
				<tr class="heading">
				    <td colspan="3"><%= GetMessage("WhatIsCondition")%></td>
				</tr>		
                <tr valign="top" title="<%= GetMessage("FieldTooltip.ConditionType") %>">
					<td class="field-name" width="25%"><span class="required">*</span><%= GetMessage("FieldLabel.ConditionType")%>:</td>
					<td width="30%">
                        <asp:DropDownList ID="ConditionType" runat="server"></asp:DropDownList>
                        <asp:CustomValidator ID="ConditionTypeValidator" runat="server" 
                            ValidationGroup="Main" ControlToValidate="ConditionType" 
                            ErrorMessage="<%$ Loc:Message.ConditionType %>"  Display="Dynamic" ValidateEmptyText="true"
                            ClientValidationFunction="Bitrix.PromoRuleEditor.validateDropDownList" 
                            OnServerValidate="ValidateDropDownList">*</asp:CustomValidator>
					    <asp:HiddenField ID="InitialConditionData" runat="server"/>
					    <asp:HiddenField ID="ClientConditionData" runat="server"/>	                            
					</td>
					<td></td>
				</tr>							
			</table>		
			<table id="ConditionTable" class="edit-table" cellspacing="0" cellpadding="0" border="0">		
			</table>
			<table class="edit-table" cellspacing="0" cellpadding="0" border="0">
				<tr class="heading">
				    <td colspan="3"><%= GetMessage("WhatIsAction")%></td>
				</tr>
                <tr valign="top" title="<%= GetMessage("FieldTooltip.ActionType") %>">
                    <td class="field-name" width="25%"><span class="required">*</span><%= GetMessage("FieldLabel.ActionType")%>:</td>   
                    <td width="30%">
                        <asp:DropDownList ID="ActionType" runat="server"></asp:DropDownList>
                        <asp:CustomValidator ID="ActionTypeValidator" runat="server" 
                            ValidationGroup="Main" ControlToValidate="ActionType" 
                            ErrorMessage="<%$ Loc:Message.ActionType %>"  Display="Dynamic" ValidateEmptyText="true"
                            ClientValidationFunction="Bitrix.PromoRuleEditor.validateDropDownList" 
                            OnServerValidate="ValidateDropDownList">*</asp:CustomValidator>                        
					    <asp:HiddenField ID="InitialActionData" runat="server"/>
					    <asp:HiddenField ID="ClientActionData" runat="server"/>	                        
                    </td> 
                    <td></td>            				
                </tr>
		    </table>
			<table id="ActionTable" class="edit-table" cellspacing="0" cellpadding="0" border="0">		
			</table>						
		</bx:BXTabControlTab>
	</bx:BXTabControl>
	
<script  type="text/javascript" language="javascript">
if(typeof(Bitrix) == "undefined"){
	var Bitrix = new Object();
}

Bitrix.PromoRuleEditor = function Bitrix$PromoRuleEditor() {
    this._boundEntityTypeElId = "<%= BoundEntityType.ClientID %>"; 
    
    this._initialConditionDataElId = "<%= InitialConditionData.ClientID %>"; 
    this._clientConditionDataElId = "<%= ClientConditionData.ClientID %>"; 
    this._conditionInfos = <%= ConditionInfosJson %>;    
    this._conditionTypeElId = "<%= ConditionType.ClientID %>"; 
    this._conditionContainerElId = "ConditionTable"; 
    this._conditionEditor = null;
    this._conditionNotSelectedStr = "<%= GetMessage("NotSelectedNeuter") %>";
    
    this._initialActionDataElId = "<%= InitialActionData.ClientID %>"; 
    this._clientActionDataElId = "<%= ClientActionData.ClientID %>"; 
    this._actionInfos = <%= ActionInfosJson %>;    
    this._actionTypeElId = "<%= ActionType.ClientID %>"; 
    this._actionContainerElId = "ActionTable"; 
    this._actionEditor = null;
    this._actionNotSelectedStr = "<%= GetMessage("NotSelectedNeuter") %>";    
}

Bitrix.PromoRuleEditor.prototype = {
    getBoundEntityTypeEl: function() { return document.getElementById(this._boundEntityTypeElId); },
    getBoundEntityType: function() {
        var el = this.getBoundEntityTypeEl();
        return el != null ? el.value : "";
    },
    getInitialConditionDataEl: function() { return document.getElementById(this._initialConditionDataElId); },
    getClientConditionDataEl: function() { return document.getElementById(this._clientConditionDataElId); },
    getConditionContainerEl: function() { return document.getElementById(this._conditionContainerElId); },
    getConditionTypeEl: function() { return document.getElementById(this._conditionTypeElId); },
    getConditionConfigEditor: function(typeName) {
        var editors = this.getInitialConditionData();
        if(!editors) return null;
        for(var i = 0; i < editors.length; i++)
            if(editors[i].typeName == typeName)
                return editors[i];
        return null;
    },
    getInitialConditionData: function() {
        var r = [];
        try{
            var el = this.getInitialConditionDataEl();
            if(el) {
                r = eval("(" + el.value + ")");
                if(!(r instanceof Array)) r = []; 
            }
        }
        catch(e){
        }
        return r;         
    },  
    getInitialActionDataEl: function() { return document.getElementById(this._initialActionDataElId); },
    getClientActionDataEl: function() { return document.getElementById(this._clientActionDataElId); },
    getActionContainerEl: function() { return document.getElementById(this._actionContainerElId); },
    getActionTypeEl: function() { return document.getElementById(this._actionTypeElId); },
    getActionConfigEditor: function(typeName) {
        var editors = this.getInitialActionData();
        if(!editors) return null;
        for(var i = 0; i < editors.length; i++)
            if(editors[i].typeName == typeName)
                return editors[i];
        return null;
    },
    getInitialActionData: function() {
        var r = [];
        try{
            var el = this.getInitialActionDataEl();
            if(el) {
                r = eval("(" + el.value + ")");
                if(!(r instanceof Array)) r = []; 
            }
        }
        catch(e){
        }
        return r;         
    },     
    layout: function() { 
        var boundTypeEl = this.getBoundEntityTypeEl();
        if(boundTypeEl)
            Bitrix.EventUtility.addEventListener(boundTypeEl, "change", Bitrix.TypeUtility.createDelegate(this, this.handleBoundEntityTypeChange));  
            
        var conditionTypeEl = this.getConditionTypeEl();
        if(conditionTypeEl)
            Bitrix.EventUtility.addEventListener(conditionTypeEl, "change", Bitrix.TypeUtility.createDelegate(this, this.handleConditionTypeChange));  
          
        var actionTypeEl = this.getActionTypeEl();
        if(actionTypeEl)
            Bitrix.EventUtility.addEventListener(actionTypeEl, "change", Bitrix.TypeUtility.createDelegate(this, this.handleActionTypeChange));  
                    
        //this._prepareConditions(); 
        this._prepareConditionEditor();        
        //this._prepareActions();  
        this._prepareActionEditor();          
    },
    _prepareConditions: function() {
        var typeEl = this.getConditionTypeEl();
        if(!typeEl) return;
        Bitrix.DomHelper.removeAllFromSelect(typeEl);
		var opts = [];		    
		opts.push( { 'text': this._conditionNotSelectedStr, 'value': "" } );        
		var type = this.getBoundEntityType();
        if(type.length > 0)
            for(var key in this._conditionInfos) {
			    var info = this._conditionInfos[key];
			    if(!Bitrix.PromoRuleEditor.isTypeSupported(info, type)) continue;
			    opts.push({ 'text': info.typeDescr, 'value': info.typeName });
		    }
        Bitrix.DomHelper.addToSelect(typeEl, opts);
        typeEl.selectedIndex = typeEl.options.length > 1 ? 1 : 0;
    },
    _prepareConditionEditor: function() {           
        var tbl = this.getConditionContainerEl();
        if(!tbl) return;
        
        while(tbl.rows.length > 0) tbl.deleteRow(0);       
        
        var conditionTypeEl = this.getConditionTypeEl();
        var conditionType = conditionTypeEl ? conditionTypeEl.value : "";
        if(conditionType.length > 0) {
            var info = null;
            for(var i in this._conditionInfos) {            
                if(this._conditionInfos[i].typeName != conditionType) continue;
                info = this._conditionInfos[i];
                break;					
            }
            if(info) {
                var ed = this._conditionEditor = Bitrix.PromoRuleNestConfigEditor.create(info, "CONDITION", this);                                                             
                ed.construct(tbl);               
            }             
        }                           
    },    
    _prepareActions: function() {	
        var typeEl = this.getActionTypeEl();
        if(!typeEl) return;
        Bitrix.DomHelper.removeAllFromSelect(typeEl);
		var opts = [];		    
		opts.push( { 'text': this._actionNotSelectedStr, 'value': "" } );        
		var type = this.getBoundEntityType();
        if(type.length > 0)
            for(var key in this._actionInfos) {
			    var info = this._actionInfos[key];
			    if(!Bitrix.PromoRuleEditor.isTypeSupported(info, type)) continue;
			    opts.push({ 'text': info.typeDescr, 'value': info.typeName });
		    }
        Bitrix.DomHelper.addToSelect(typeEl, opts);
        typeEl.selectedIndex = typeEl.options.length > 1 ? 1 : 0;
    },
    _prepareActionEditor: function() {           
        var tbl = this.getActionContainerEl();
        if(!tbl) return;
        
        while(tbl.rows.length > 0) tbl.deleteRow(0);       
        
        var typeEl = this.getActionTypeEl();
        var type = typeEl ? typeEl.value : "";
        if(type.length > 0) {
            var info = null;
            for(var i in this._actionInfos) {            
                if(this._actionInfos[i].typeName != type) continue;
                info = this._actionInfos[i];
                break;					
            }
            if(info) {
                var ed = this._actionEditor = Bitrix.PromoRuleNestConfigEditor.create(info, "ACTION", this);                                                             
                ed.construct(tbl);               
            }             
        }                           
    },        
    handleBoundEntityTypeChange: function() { 
        this._prepareConditions(); 
        this._prepareConditionEditor();        
        this._prepareActions();  
        this._prepareActionEditor(); 
    },
    handleConditionTypeChange: function() { this._prepareConditionEditor(); },
    handleActionTypeChange: function() { this._prepareActionEditor(); },    
    
    saveSettings: function() {
        var el = this.getClientConditionDataEl();
        if(el) {
            var ed = this._conditionEditor;
            if(!ed) el.value = "";
            else {
                var r = "";
                if(r.length > 0) r += ", "
                r += "{";
                r += "\"typeName\":\"" + ed.getNestTypeName() + "\"";
                //r += ", \"isSelected\":" + ed.isSelected().toString();
                var configJson = ed.getConfigJson();
                if(Bitrix.TypeUtility.isNotEmptyString(configJson))
                    r += ", \"config\":" + configJson;
                r += "}";
                el.value = r; 
            }
        }
        el = this.getClientActionDataEl();
        if(el) {
            var ed = this._actionEditor;
            if(!ed) el.value = "";
            else {
                var r = "";
                if(r.length > 0) r += ", "
                r += "{";
                r += "\"typeName\":\"" + ed.getNestTypeName() + "\"";
                //r += ", \"isSelected\":" + ed.isSelected().toString();
                var configJson = ed.getConfigJson();
                if(Bitrix.TypeUtility.isNotEmptyString(configJson))
                    r += ", \"config\":" + configJson;
                r += "}";
                el.value = r; 
            }
        }        
    }   
}
Bitrix.PromoRuleEditor.validateDropDownList = function(source, arguments) { arguments.IsValid = Bitrix.TypeUtility.isNotEmptyString(arguments.Value); }
Bitrix.PromoRuleEditor.isTypeSupported = function(info, type) {
    var typeArr = info["supportedTypes"];
    if(!typeArr || typeArr.length == 0) return true;
    for(var i = 0; i < typeArr.length; i++) if(typeArr[i] == type) return true;
    return false;     
}
Bitrix.PromoRuleEditor._instance = null;
Bitrix.PromoRuleEditor.getInstance = function() { return this._instance != null ? this._instance : (this._instance = new Bitrix.PromoRuleEditor()); }

Bitrix.PromoRuleNestConfigEditor = function Bitrix$PromoRuleNestConfigEditor(){
    this._parent = this._info = this._lbl = this._cfgVsbSwitch = this._descr = this._paramRows = this._configEd = this._config = null;
    this._selected = false;
    this._target = "CONDITION";
    
}
Bitrix.PromoRuleNestConfigEditor.prototype = {
    initialize: function(info, target, parent) {
        if(!info) throw "PromoRuleNestConfigEditor: info is not defined!";
        this._info = info;
        this._target = target;
        if(!(parent instanceof Bitrix.PromoRuleEditor)) throw "PromoRuleNestConfigEditor: parent is not PromoRuleEditor!";
        this._parent = parent;  
        var edData = this.getEditorData(); 
        if(edData) {
            var cfg = this._config = edData.config;
            this._selected = edData.isSelected;  
        }
      },
     isSelected: function() { return this._selected; },
     getInfo: function() { return this._info; },
     getConfigJson: function() { 
        var cfgEd = this._configEd;
        return cfgEd && typeof(cfgEd.getConfigJson) == "function" ? cfgEd.getConfigJson() : "";
     },
     isTypeSupported: function(type) {
        var typeArr = this._info["supportedTypes"];
        if(!typeArr || typeArr.length == 0) return true;
        for(var i = 0; i < typeArr.length; i++) if(typeArr[i] == type) return true;
        return false;     
     },
     getEditorData: function() {
         return this._target == "ACTION" ? this._parent.getActionConfigEditor(this.getNestTypeName()) : this._parent.getConditionConfigEditor(this.getNestTypeName()); 
     },
     construct: function(tbl) {
        if(!tbl) return;	
		
        var edData = this.getEditorData(); 	
        if(edData && "constructor" in edData) {
            var constructor = null;
            try {
                constructor = eval("(" + edData.constructor + ")");
            }
            catch(e){}
            if(constructor != null) { 
                var ed = this._configEd = constructor.construct(this._config, this._parent, tbl);
				this._paramRows = [];
				if(typeof(ed.getVersion) == "function" && ed.getVersion() == 1) {
					for(var i = 0; i < ed.getParamCount(); i++) {
						var r = tbl.insertRow(-1);
						this._paramRows.push(r);
						r.vAlign = "top";
						var ttlC = r.insertCell(-1);
						ttlC.className = "field-name";
						ttlC.width = "25%";
						var lbl = document.createElement("LABEL");
						ttlC.appendChild(lbl);
						lbl.innerHTML = ed.getParamTitle(i) + ":";
						var dataC = r.insertCell(-1);
						dataC.width = "30%";
						ed.constructParamControls(i, dataC);
						if(i == 0) {
							var descrC = r.insertCell(-1);
							descrC.className = "promo-rule-nest-descr";
							//descrC.width="50%";
							descrC.innerHTML = this._info["descrHtml"];							
							descrC.rowSpan = ed.getParamCount();							
						}						
					}
		            var gapC = tbl.insertRow(-1).insertCell(-1);
		            gapC.colSpan = 3;					
		            gapC.className = "promo-rule-nest-gap";		            
				}
            }
        }
    }, 
    getNestTypeName: function() { return this._info["typeName"]; }
}
Bitrix.PromoRuleNestConfigEditor.create = function(info, target, parent){
    var self = new Bitrix.PromoRuleNestConfigEditor();
    self.initialize(info, target, parent);    
    return self;
}
Bitrix.PromoRuleEditor.getInstance().layout();
</script>	
</asp:Content>