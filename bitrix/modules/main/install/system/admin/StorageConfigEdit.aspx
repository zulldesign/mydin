<%@ Page Language="C#" AutoEventWireup="false" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" CodeFile="StorageConfigEdit.aspx.cs" Inherits="bitrix_admin_StorageConfigEdit" %>
<%@ Import Namespace="Bitrix.UI" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="ContentPlaceHolder1">
	<bx:BXContextMenuToolbar ID="BXBlogEditToolbar" runat="server" OnCommandClick="OnToolBarButtonClick">
		<Items>
			<bx:BXCmSeparator ID="ListButton" runat="server" SectionSeparator="true" />
			<bx:BXCmImageButton CssClass="context-button icon btn_list" CommandName="list"
				Text="<%$ LocRaw:ActionText.GoBack %>" Title="<%$ LocRaw:ActionTitle.GoBack %>" Href="StorageConfigList.aspx" />
		
			<bx:BXCmSeparator SectionSeparator="true" />
			<bx:BXCmImageButton ID="AddButton"
				CommandName="add" Text="<%$ LocRaw:Kernel.Add %>" Title="<%$ LocRaw:ActionTitle.Add %>"
				CssClass="context-button icon btn_new" Href="StorageConfigEdit.aspx" />
				
			<bx:BXCmSeparator SectionSeparator="true" />
			<bx:BXCmImageButton ID="DeleteButton"
				CommandName="delete" Text="<%$ LocRaw:Kernel.Delete %>" Title="<%$ LocRaw:ActionTitle.Delete %>"
				CssClass="context-button icon btn_delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>"/>
		</Items>
	</bx:BXContextMenuToolbar>
	<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="StorageConfigEdit" />
	<bx:BXTabControl ID="TabControl" runat="server" OnCommand="OnEntityEdit" ValidationGroup="StorageConfigEdit">
		<bx:BXTabControlTab ID="MainSettingsTab" runat="server" Selected="True" Text="<%$ LocRaw:TabText.StorageConfig %>" Title="<%$ LocRaw:TabTitle.StorageConfig %>">
			<table class="edit-table" cellspacing="0" cellpadding="0" border="0">
				<% if (EntityId > 0){ %>
				<tr valign="top"><td class="field-name">ID:</td><td><%= EntityId.ToString() %></td></tr>
				<% } %>

				<tr valign="top" title="<%= GetMessage("FieldTooltip.Active") %>">
					<td class="field-name" width="40%"><%= GetMessage("FieldLabel.Active") %>:</td>
					<td width="60%"><asp:CheckBox ID="ActiveChkBx" runat="server" /></td>
				</tr>	

				<tr valign="top" title="<%= GetMessage("FieldTooltip.Sort") %>">
					<td class="field-name" width="40%"><%= GetMessage("FieldLabel.Sort") %>:</td>
					<td width="60%"><asp:TextBox ID="SortTbx" runat="server" Width="35px" /></td>
				</tr>
				
				<tr valign="top" title="<%= GetMessage("FieldTooltip.Provider") %>">
					<td class="field-name" width="40%"><%= GetMessage("FieldLabel.Provider") %>:</td>
					<td width="60%">
						<asp:DropDownList ID="ProviderDdl" runat="server" Width="350px"></asp:DropDownList> 
						<asp:HiddenField ID="SettingsEditorDataFld" runat="server" />
					</td>
				</tr>				
				<tr <% if (ProviderDdl.Items.Count == 0) { %>style="display:none"<% } %>>
					<td width="40%">
					</td>
					<td width="60%">
						<div class="notes">
							<table class="notes" cellspacing="0" cellpadding="0" border="0">
								<tbody>
									<tr class="top">
										<td class="left">
											<div class="empty"></div>
										</td> 
										<td>
											<div class="empty"></div>
										</td>
										<td class="right">
											<div class="empty"></div>
										</td> 
									</tr>
									<tr>
										<td class="left">
											<div class="empty"></div>
										</td> 
										<td class="content">
											<div id="ProviderInfoContainer">
											</div>
										</td>
										<td class="right">
											<div class="empty"></div>
										</td> 
									</tr>
									<tr class="bottom">
										<td class="left">
											<div class="empty"></div>
										</td> 
										<td>
											<div class="empty"></div>
										</td>
										<td class="right">
											<div class="empty"></div>
										</td> 
									</tr>
								</tbody>
							</table>
						</div>
					</td>
				</tr>              	
			</table>            
			<table id="SettingsTable" class="edit-table" cellspacing="0" cellpadding="0" border="0">
			</table>
		</bx:BXTabControlTab>
		<bx:BXTabControlTab ID="BindingTab" runat="server" Text="<%$ LocRaw:TabText.Bindings %>" Title="<%$ LocRaw:TabTitle.Bindings %>">
			<table class="edit-table" cellspacing="0" cellpadding="0" border="0">
				<tr>
					<td align="center">
						<asp:HiddenField ID="BindingDataFld" runat="server" />
						<div id="BindingEditorContainer">
						</div>
					</td>
				</tr>
				<tr>
					<td>
						<bx:BXAdminNote runat="server" ID="Notes">
							<%= GetMessageRaw("Note") %>							
						</bx:BXAdminNote>				
					</td>
				</tr>
			</table>
		</bx:BXTabControlTab>
	</bx:BXTabControl>
<script  type="text/javascript" language="javascript">
	if(typeof(Bitrix) == "undefined"){
		var Bitrix = {};
	}

	Bitrix.StorageController = function Bitrix$StorageController() {
		this._providerSelectId = this._providerDescrId = this._settingsTableId = this._settingsEditorDataFldId = this._bindingContainerId = this._bindingDataFieldId = "";
		this._providerType = "";
		this._bindingEditors = [];
		this._settingsEditors = [];
		this._settingsEditor = null;
		this._bindingTable = null;
	}

	Bitrix.StorageController.prototype = {
		initialize: function(providerSelectId, providerDescrId, settingsTableId, settingsEditorDataFldId, bindingContainerId, bindingDataFieldId) {
			this._providerSelectId = providerSelectId;
			this._providerDescrId = providerDescrId;
			this._settingsTableId = settingsTableId;
			this._settingsEditorDataFldId = settingsEditorDataFldId;
			this._bindingContainerId = bindingContainerId;
			this._bindingDataFieldId = bindingDataFieldId;
		},
		_evalJson: function(json) {
			try {
				return eval("(" + json + ")");
			}
			catch(e) {
			}
			return null;         
		},
		construct: function() {
			this._settingsEditors = this._evalJson(document.getElementById(this._settingsEditorDataFldId).value);
			if(!this._settingsEditors){
				this._settingsEditors = [];
			}

			var providerSel = document.getElementById(this._providerSelectId);
			this._constructSettingsEditor(providerSel.value);
			Bitrix.EventUtility.addEventListener(providerSel, "change", Bitrix.TypeUtility.createDelegate(this, this._handleProviderChange));

			var bindingContainer = document.getElementById(this._bindingContainerId);
			
			var table = this._bindingTable = document.createElement("TABLE");
			table.className = "internal";
			table.border = 0;
			table.cellPadding = 0;
			table.cellSpacing = 0;
			bindingContainer.appendChild(table);

			var headR = table.insertRow(-1);
			headR.className = "heading";
			
			var moduleC = headR.insertCell(-1);
			moduleC.innerHTML = "<%= GetMessageJS("ModuleList") %>" + "<sup><span class='required'>1</span></sup>";

			var extC = headR.insertCell(-1);
			extC.innerHTML = "<%= GetMessageJS("ExtensionList") %>" + "<sup><span class='required'>2</span></sup>";

			var sizeC = headR.insertCell(-1);
			sizeC.innerHTML = "<%= GetMessageJS("SizeList") %>" + "<sup><span class='required'>3</span></sup>";

			headR.insertCell(-1); //delete button
			
			var addBtn = document.createElement("SPAN");
			addBtn.className = "bx-action";
			addBtn.innerHTML = "<%= GetMessageJS("AddNewRule") %>";
			Bitrix.EventUtility.addEventListener(addBtn, "click", Bitrix.TypeUtility.createDelegate(this, this._handleAddBtnClick));
			bindingContainer.appendChild(addBtn);
			
			var bindingData = this._evalJson(document.getElementById(this._bindingDataFieldId).value);
			if(!bindingData){
				bindingData = [];
			}
			if(bindingData.length > 0) {
				for(var i = 0; i < bindingData.length; i++) {
					this.addBinding(bindingData[i]);
				}
			}
			else {
				this.addBinding(null);
			}
			
			this._handleProviderChange();
		},
		_handleProviderChange: function() {
			var providerSel = document.getElementById(this._providerSelectId);
			this._constructSettingsEditor(providerSel.value);
		},
		_constructSettingsEditor: function(name) {
			var table = document.getElementById(this._settingsTableId);
			while(table.rows.length > 0) {
				table.deleteRow(0);
			}

			this._providerType = name;
			this._settingsEditor = null;

			var descr = document.getElementById(this._providerDescrId);
			descr.innerHTML = "";

			if(!Bitrix.TypeUtility.isNotEmptyString(name)) {
				return;
			}

			for(var i = 0; i < this._settingsEditors.length; i++) {
				var cur = this._settingsEditors[i];
				if(cur.typeName == name) {
					var editor = this._settingsEditor = this._evalJson(cur.constructorJson)(cur.settings);
					var paramCount = this._settingsEditor.getParamCount();
					for(var j = 0; j < paramCount; j++) {
						var r = table.insertRow(-1);
						r.valign = "top";
						var ttl = r.insertCell(-1);
						ttl.className = "field-name";
						ttl.setAttribute("width", "40%");
						ttl.innerHTML = editor.getParamTitle(j) + ":";
						var cnt = r.insertCell(-1);
						cnt.setAttribute("width", "60%");
						editor.constructParamControls(j, cnt);

						descr.innerHTML = editor.getDescriptionHtml();
					}
					break;
				}
			}

		},
		_handleAddBtnClick: function() {
			this.addBinding(null);
		},
		addBinding: function(config) {
			var binding = Bitrix.StorageBindingEditor.create(config, this);
			this._bindingEditors.push(binding);
			var controls = binding.construct();
			
			var bindingR = this._bindingTable.insertRow(-1);
			
			var modulesC = bindingR.insertCell(-1);
			modulesC.appendChild(controls.inputs.modules);

			var extensionsC = bindingR.insertCell(-1);
			extensionsC.appendChild(controls.inputs.extensions);

			var sizesC = bindingR.insertCell(-1);
			sizesC.appendChild(controls.inputs.sizes);

			var delC = bindingR.insertCell(-1);
			delC.appendChild(controls.buttons.del);	
			
			return binding;
		},
		deleteBinding: function(binding) {
			for(var i = 0; i < this._bindingEditors.length; i++) {
				if(binding == this._bindingEditors[i]) {
					this._bindingEditors.splice(i, 1);
					this._bindingTable.deleteRow(i + 1);
					break;
				}
			}
		},
		saveConfig: function() {
			var editorJson = "";
			editorJson += "typeName:\"" + this._providerType + "\"";
			editorJson += ", settings:" + (this._settingsEditor != null ? this._settingsEditor.getSettingsJson() : "");

			document.getElementById(this._settingsEditorDataFldId).value = "{" + editorJson + "}";
			var bindingJson = "";
			for(var i = 0; i < this._bindingEditors.length; i++) {
				var c = this._bindingEditors[i].getConfig();
				if(bindingJson.length > 0) bindingJson += ",";
				bindingJson += "{";
				bindingJson += "modules:\"" + c.modules + "\"";
				bindingJson += ", extensions:\"" + c.extensions + "\"";
				bindingJson += ", sizes:\"" + c.sizes + "\"";
				bindingJson += "}";
			}

			document.getElementById(this._bindingDataFieldId).value = "[" + bindingJson + "]";
		}
	}
	
	Bitrix.StorageController.instance = null;
	Bitrix.StorageController.getInstance = function() {
		if(this.instance) return this.instance;
		this.instance = new Bitrix.StorageController();
		this.instance.initialize("<%= ProviderDdl.ClientID %>", "ProviderInfoContainer", "SettingsTable", "<%= SettingsEditorDataFld.ClientID %>", "BindingEditorContainer", "<%= BindingDataFld.ClientID %>");
		return this.instance;
	}
	
	Bitrix.StorageBindingEditor = function Bitrix$StorageBindingEntryEditor() {
		this._modulesInput = this._extInput = this._sizeInput = this._delBtn = null;
		this._config = {};
		this._controller = null;
	}
	
	Bitrix.StorageBindingEditor.prototype = {
		initialize: function(config, controller) {
			this._config = config ? config : {};
			this._controller = controller;
		},
		getConfig: function() { 
			var m = this._modulesInput;
			if(m) this._config.modules = m.value;
			
			var e = this._extInput;
			if(e) this._config.extensions = e.value;

			var s = this._sizeInput;
			if(s) this._config.sizes = s.value;
			
			return this._config; 
		},
		getSetting: function(name, defVal) {
			return typeof(this._config[name]) != "undefined" ? this._config[name] : defVal;
		},
		construct: function() {
			var m = this._modulesInput = document.createElement("INPUT");
			m.type = "text";
			m.value = this.getSetting("modules", "");
			
			var e = this._extInput = document.createElement("INPUT");
			e.type = "text";
			e.value = this.getSetting("extensions", "");
			
			var s = this._sizeInput = document.createElement("INPUT");
			s.type = "text";
			s.value = this.getSetting("sizes", "");
			
			var d = this._delBtn = document.createElement("SPAN");
			d.style.cursor = "pointer";
			var img = document.createElement("IMG");
			img.src = "<%= VirtualPathUtility.ToAbsolute(BXThemeHelper.SimpleCombineWithCurrentThemePath("images/actions/delete_button.gif")) %>"
			d.appendChild(img); 
			Bitrix.EventUtility.addEventListener(d, "click", Bitrix.TypeUtility.createDelegate(this, this._handleDelBtnClick));

			return { "inputs": { "modules": m, "extensions": e, "sizes": s },  "buttons": {"del": d } };
		},
		_handleDelBtnClick: function() {
			this._controller.deleteBinding(this);
		}
	}
	
	Bitrix.StorageBindingEditor.create = function(config, controller) {
		var self = new Bitrix.StorageBindingEditor();
		self.initialize(config, controller);
		return self;
	}
	
	Bitrix.StorageController.getInstance().construct(); 
	
</script>
</asp:Content>