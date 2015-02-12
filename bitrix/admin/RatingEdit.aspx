<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="false" CodeFile="RatingEdit.aspx.cs" Inherits="bitrix_admin_RatingEdit" %>
	
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<bx:BXContextMenuToolbar ID="EditToolbar" runat="server" OnCommandClick="OnToolBarButtonClick">
		<Items>
			<bx:BXCmSeparator ID="ListButton" runat="server" SectionSeparator="true" />
			<bx:BXCmImageButton CssClass="context-button icon btn_list" CommandName="list"
				Text="<%$ LocRaw:ActionText.GoBack %>" Title="<%$ LocRaw:ActionTitle.GoBack %>" Href="RatingList.aspx" />
			<bx:BXCmSeparator SectionSeparator="true" />
			<bx:BXCmImageButton ID="AddButton" CssClass="context-button icon btn_new" CommandName="add"
				Text="<%$ LocRaw:Kernel.Add %>" Title="<%$ LocRaw:ActionTitle.Add %>" Href="RatingEdit.aspx" />
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
		        <tr valign="top"><td class="field-name">ID:</td><td><%= EntityId.ToString() %></td></tr>
				<% } %>
				<tr valign="top" title="<%= GetMessage("FieldTooltip.Active") %>">
					<td class="field-name" width="40%"><%= GetMessage("FieldLabel.Active") %>:</td>
					<td width="60%"><asp:CheckBox ID="Active" runat="server" /></td>
				</tr>				

				<tr valign="top" title="<%= GetMessage("FieldTooltip.Name") %>">
					<td class="field-name"><span class="required">*</span><%= GetMessage("FieldLabel.Name") %>:</td>
					<td>
						<asp:TextBox ID="Name" runat="server" Width="350px" />
						<asp:RequiredFieldValidator ID="NameRequiredValidator" runat="server" ValidationGroup="Main" ControlToValidate="Name" ErrorMessage="<%$ Loc:Message.NameNotSpecified %>" Display="Dynamic">*</asp:RequiredFieldValidator>
					</td>
				</tr>				
		
                <tr valign="top" title="<%= GetMessage("FieldTooltip.BoundEntityType") %>">
					<td class="field-name"><span class="required">*</span><%= GetMessage("FieldLabel.BoundEntityType")%>:</td>
					<td>
                        <asp:DropDownList ID="BoundEntityType" runat="server" >
                        </asp:DropDownList>
                        <asp:CustomValidator ID="BoundEntityTypeValidator" runat="server" 
                            ValidationGroup="Main" ControlToValidate="BoundEntityType" 
                            ErrorMessage="<%$ Loc:Message.BoundEntityType %>"  Display="Dynamic" ValidateEmptyText="true"
                            ClientValidationFunction="Bitrix.RatingEditor.validateBoundEntityType" 
                            OnServerValidate="ValidateBoundEntityType">*</asp:CustomValidator>
					</td>
				</tr>				
				
				<tr valign="top" title="<%= GetMessage("FieldTooltip.CurValCustomFieldName") %>">
                    <td class="field-name"><span class="required">*</span><%= GetMessage("FieldLabel.CurValCustomFieldName")%>:</td>
				    <td>
				        <asp:TextBox ID="CurValCustomFieldName" runat="server" Width="350px" />
                        <asp:RequiredFieldValidator ID="CurValCustomFieldNameRequiredValidator" runat="server" ValidationGroup="Main" ControlToValidate="CurValCustomFieldName" ErrorMessage="<%$ Loc:Message.CurValCustomFieldNameNotSpecified %>" Display="Dynamic">*</asp:RequiredFieldValidator>
				    </td>
				</tr>

				<tr valign="top" title="<%= GetMessage("FieldTooltip.PrevValCustomFieldName") %>">
				    <td class="field-name"><span class="required">*</span><%= GetMessage("FieldLabel.PrevValCustomFieldName")%>:</td>
				    <td>
				        <asp:TextBox ID="PrevValCustomFieldName" runat="server" Width="350px" />
						<asp:RequiredFieldValidator ID="PrevValCustomFieldNameRequiredValidator" runat="server" ValidationGroup="Main" ControlToValidate="PrevValCustomFieldName" ErrorMessage="<%$ Loc:Message.PrevValCustomFieldNameNotSpecified %>" Display="Dynamic">*</asp:RequiredFieldValidator>				        
				    </td>
				</tr>
								
                <%--<tr valign="top" title="<%= GetMessage("FieldTooltip.RefreshMethod") %>">
					<td class="field-name"><%= GetMessage("FieldLabel.RefreshMethod")%>:</td>
					<td>
                        <asp:DropDownList ID="RefreshMethod" runat="server" >
                        </asp:DropDownList>
					</td>
				</tr>--%>					
								
				<tr valign="top" title="<%= GetMessage("FieldTooltip.XmlId") %>" >
					<td class="field-name"><%= GetMessage("FieldLabel.XmlId") %>:</td>
					<td>
						<asp:TextBox Rows="7" ID="XmlId"  runat="server" Width="350px" />						
					</td>
				</tr>			
			</table>
		</bx:BXTabControlTab>
		<bx:BXTabControlTab ID="CalculationSettingsTab" Runat="server" Text="<%$ LocRaw:TabText.CalculationSettings %>" Title="<%$ LocRaw:TabTitle.CalculationSettings %>">
			<table class="edit-table" cellspacing="0" cellpadding="0" border="0">
				<tr class="heading">
				    <td colspan="2"><%= GetMessage("HowIsCalculated")%></td>
				</tr>			
                <tr valign="top" title="<%= GetMessage("FieldTooltip.CalculationMethod") %>">
					<td class="field-name"><%= GetMessage("FieldLabel.CalculationMethod")%>:</td>
					<td width="70%">
                        <asp:DropDownList ID="CalculationMethod" runat="server" >
                        </asp:DropDownList>
					</td>
				</tr>
                <tr valign="top">
					<td colspan="2">
					    <asp:HiddenField ID="InitialComponentData" runat="server" />
					    <asp:HiddenField ID="ClientComponentDataField" runat="server" />
					    <div id="ComponentContainter"></div>
                        <asp:CustomValidator ID="ComponentTypeValidator" runat="server" 
                            ValidationGroup="Main" 
                            ErrorMessage="<%$ Loc:Message.Components %>" Display="Dynamic" ValidateEmptyText="true"
                            ClientValidationFunction="Bitrix.RatingEditor.validateSelectedComponents" 
                            OnServerValidate="ValidateSelectedComponents">*</asp:CustomValidator>					    
					</td>
				</tr>							
			</table>		
			<table id="ComponentTable" class="edit-table" cellspacing="0" cellpadding="0" border="0">		
			</table>
	    </bx:BXTabControlTab>		
	</bx:BXTabControl>
	
<script  type="text/javascript" language="javascript">
if(typeof(Bitrix) == "undefined"){
	var Bitrix = new Object();
}

Bitrix.RatingEditor = function Bitrix$RatingEditor(){
    this._initialComponentDataElId = "<%= InitialComponentData.ClientID %>"; 
    this._clientComponentDataElId = "<%= ClientComponentDataField.ClientID %>"; 
    this._componentInfos = <%= ComponentInfosJson %>;
    this._boundEntityTypeElId = "<%= BoundEntityType.ClientID %>"; 
    this._headingText = "<%= GetMessage("WhatIsCalculated")%>";
    this._componentContainerElId = "ComponentTable"; 
    this._componentEditors = [];
}

Bitrix.RatingEditor.prototype = {
    getInitialComponentDataEl: function() { return document.getElementById(this._initialComponentDataElId); },
    getClientComponentDataEl: function() { return document.getElementById(this._clientComponentDataElId); },
    getComponentContainerEl: function() { return document.getElementById(this._componentContainerElId); },
    getBoundEntityTypeEl: function() { return document.getElementById(this._boundEntityTypeElId); },
    getBoundEntityType: function() {
        var el = this.getBoundEntityTypeEl();
        return el != null ? el.value : "";
    },
    getComponentConfigEditor: function(typeName) {
        var editors = this.getInitialComponentData();
        if(!editors) return null;
        for(var i = 0; i < editors.length; i++)
            if(editors[i].typeName == typeName)
                return editors[i];
        return null;
    },
    layout: function() { 
        var boundTypeEl = this.getBoundEntityTypeEl();
        if(boundTypeEl)
            Bitrix.EventUtility.addEventListener(boundTypeEl, "change", Bitrix.TypeUtility.createDelegate(this, this.handleBoundEntityTypeChange));  
        this._prepareEditors(); 
    },
    _prepareEditors: function() {
        var editors = this._componentEditors;
        if(editors.length > 0) editors.splice(0, editors.length);
        var tbl = this.getComponentContainerEl();
        if(!tbl) return;
        
        while(tbl.rows.length > 0)
            tbl.deleteRow(0);
        
        var headingR = tbl.insertRow(-1);
        headingR.className = "heading";
        
        var headingC = headingR.insertCell(-1);
        headingC.colSpan = 3;
        headingC.innerHTML = this._headingText;
        
        var gapC = tbl.insertRow(-1).insertCell(-1);
        gapC.colSpan = 3;					
        gapC.className = "rating-component-gap";        
        
        var type = this.getBoundEntityType();        
        if(type.length > 0)
            for(var i in this._componentInfos) {            
                var ed = Bitrix.RatingComponentEditor.create(this._componentInfos[i], this); 
                if(!ed.isTypeSupported(type)) continue;
                editors.push(ed);                                                              
                ed.construct(tbl);  
					
            }                            
    },
    getInitialComponentData: function() {
        var r = [];
        try{
            var el = this.getInitialComponentDataEl();
            if(el) {
                r = eval("(" + el.value + ")");
                if(!(r instanceof Array)) r = []; 
            }
        }
        catch(e){
        }
        return r;         
    },
    hasSelectedComponentTypes: function() {
        var editors = this._componentEditors;
        for(var i = 0; i < editors.length; i++) if(editors[i].isSelected()) return true;
        return false;
    },
    handleBoundEntityTypeChange: function() { this._prepareEditors(); },
    saveSettings: function() {
        var el = this.getClientComponentDataEl();
        if(!el) return;
        var editors = this._componentEditors;
        var r = "";
        for(var i = 0; i < editors.length; i++) {
            var ed = editors[i];
            if(r.length > 0) r += ", "
            r += "{";
            r += "\"typeName\":\"" + ed.getComponentTypeName() + "\"";
            r += ", \"isSelected\":" + ed.isSelected().toString();
            var configJson = ed.getConfigJson();
            if(Bitrix.TypeUtility.isNotEmptyString(configJson))
                r += ", \"config\":" + configJson;
            r += "}";
        }
        el.value = "[" + r + "]";        
    }   
}
Bitrix.RatingEditor.validateBoundEntityType = function(source, arguments) { arguments.IsValid = Bitrix.TypeUtility.isNotEmptyString(arguments.Value); }
Bitrix.RatingEditor.validateSelectedComponents = function(source, arguments) { arguments.IsValid = this.getInstance().hasSelectedComponentTypes(); }
Bitrix.RatingEditor._instance = null;
Bitrix.RatingEditor.getInstance = function() { return this._instance != null ? this._instance : (this._instance = new Bitrix.RatingEditor()); }

Bitrix.RatingComponentEditor = function Bitrix$RatingComponentEditor(){
    this._parent = this._info = this._cbx = this._lbl = this._cfgVsbSwitch = this._descr = this._paramRows = this._configEd = this._config = null;
    this._selected = false;
}
Bitrix.RatingComponentEditor.prototype = {
    initialize: function(info, parent){
        if(!info) throw "RatingComponentEditor: info is not defined!";
        this._info = info;
        if(!(parent instanceof Bitrix.RatingEditor)) throw "RatingComponentEditor: parent is not RatingEditor!";
        this._parent = parent;   
        var cfgEd = parent.getComponentConfigEditor(this.getComponentTypeName()); 
        var cfg = this._config = cfgEd.config;
        this._selected = cfgEd.isSelected;  
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
     construct: function(tbl) {
        if(!tbl) return;
		
		var rH = tbl.insertRow(-1);
		rH.className = "heading-left";
		var cH = rH.insertCell(-1);
		cH.colSpan = 3;
		
		var cbx = this._cbx = document.createElement("INPUT");
		cbx.type = "checkbox";
		cH.appendChild(cbx);                
        cbx.id = this._info["typeName"];
        cbx.checked = this._selected;
        cbx.onclick = Bitrix.TypeUtility.createDelegate(this, this._handleSelectionChange);    		
		
		var lbl = this._lbl = document.createElement("LABEL");
		cH.appendChild(lbl);
        lbl.htmlFor = cbx.id;
        lbl.innerHTML = this._info["typeDescr"]; 	
		
        var cfgEd = this._parent.getComponentConfigEditor(this.getComponentTypeName())
        if(cfgEd && "constructor" in cfgEd) {
            var constructor = null;
            try {
                constructor = eval("(" + cfgEd.constructor + ")");
            }
            catch(e){}
            if(constructor != null) {
                var ed = this._configEd = constructor.construct(this._config, tbl);
				this._paramRows = [];
				if(typeof(ed.getVersion) == "function" && ed.getVersion() == 1) {
					for(var i = 0; i < ed.getParamCount(); i++) {
						var r = tbl.insertRow(-1);
						this._paramRows.push(r);
						r.vAlign = "top";
						var ttlC = r.insertCell(-1);
						ttlC.className = "field-name";
						var lbl = document.createElement("LABEL");
						ttlC.appendChild(lbl);
						lbl.innerHTML = ed.getParamTitle(i) + ":";
						var dataC = r.insertCell(-1);
						dataC.width = "20%";
						ed.constructParamControls(i, dataC);
						if(i == 0) {
							var descrC = r.insertCell(-1);
							descrC.className = "rating-component-descr";
							descrC.width="50%";
							descrC.innerHTML = this._info["descrHtml"];							
							descrC.rowSpan = ed.getParamCount();							
						}						
					}
		            var gapC = tbl.insertRow(-1).insertCell(-1);
		            gapC.colSpan = 3;					
		            gapC.className = "rating-component-gap";
					this.layout();
				}
            }
        }
     }, 
     getComponentTypeName: function() { return this._info["typeName"]; },
    _handleSelectionChange: function() { 
		this._selected = this._cbx.checked;
        this.layout();
    },
	layout: function() {
		var rows = this._paramRows;
		if(!rows) return;
		var val = this._selected ? "" : "none";
		for(var i = 0; i < rows.length; i++)
			rows[i].style.display = val;  
	}
}
Bitrix.RatingComponentEditor.create = function(info, parent){
    var self = new Bitrix.RatingComponentEditor();
    self.initialize(info, parent);    
    return self;
}
Bitrix.RatingEditor.getInstance().layout();
</script>	
</asp:Content>