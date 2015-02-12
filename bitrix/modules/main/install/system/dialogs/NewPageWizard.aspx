<%@ Page Language="C#" AutoEventWireup="false" CodeFile="NewPageWizard.aspx.cs" Inherits="bitrix_dialogs_NewPageWizard" 
  ValidateRequest="false" EnableEventValidation="true" EnableViewState="false" Title="<%$ Loc:TITLE %>" %>

<html>
<head runat="server">
    <title></title>
</head>
<body>
    <%if (IsAnyMenuDefined) { %>
    <bx:BXPageAsDialogWizardBehaviour runat="server" ID="newPageWizardLong" Name="bxNewPageWizard" 
        Enabled="true" UseStandardStyles="true" >
        <Steps>
            <bx:BXPageAsDialogWizardStep ID="newPageWizardLongCommon" OnShowClientScript="Bitrix.NewPageWizard.showFirstStep"
                LinkedContentContainerID="tblNewPageCommon" >
                <NavigationCommands>
                    <bx:BXPageAsDialogWizardNavigationCommand  ID="newPageWizardLongCommonNext" CommandType="GoToNext" 
                        TargetStepID="newPageWizardLongMenu" OnCallClientScript="Bitrix.NewPageWizard.onFirstStepNext"/>
                    <bx:BXPageAsDialogWizardNavigationCommand ID="newPageWizardLongCommonDone" CommandType="Complete" 
                        OnCallClientScript="Bitrix.NewPageWizard.onBeforeSave" />
                </NavigationCommands>
            </bx:BXPageAsDialogWizardStep>
            
            <bx:BXPageAsDialogWizardStep ID="newPageWizardLongMenu" OnShowClientScript="Bitrix.NewPageWizard.showSecondStep"
                LinkedContentContainerID="tblNewPageMenu">
                <NavigationCommands>
                    <bx:BXPageAsDialogWizardNavigationCommand  ID="newPageWizardLongMenuPrevious" CommandType="GoToPrevious" TargetStepID="newPageWizardLongCommon"/>
                    <bx:BXPageAsDialogWizardNavigationCommand  ID="newPageWizardLongMenuNext" CommandType="GoToNext" TargetStepID="newPageWizardLongProp"/>
                    <bx:BXPageAsDialogWizardNavigationCommand ID="newPageWizardLongMenuDone" CommandType="Complete" OnCallClientScript="Bitrix.NewPageWizard.onBeforeSave" />
                </NavigationCommands>                
            </bx:BXPageAsDialogWizardStep>
            
            <bx:BXPageAsDialogWizardStep ID="newPageWizardLongProp"
                LinkedContentContainerID="tblNewPageProp">                            
                <NavigationCommands>
                    <bx:BXPageAsDialogWizardNavigationCommand  ID="newPageWizardLongPropPrevious" CommandType="GoToPrevious" TargetStepID="newPageWizardLongMenu" 
                        OnCallClientScript="Bitrix.NewPageWizard.onThirdStepPrev"/>
                    <bx:BXPageAsDialogWizardNavigationCommand ID="newPageWizardLongPropDone" CommandType="Complete" OnCallClientScript="Bitrix.NewPageWizard.onBeforeSave" />
                </NavigationCommands>                   
            </bx:BXPageAsDialogWizardStep>
        </Steps>
    </bx:BXPageAsDialogWizardBehaviour>   
    <% } else { %>
    <bx:BXPageAsDialogWizardBehaviour runat="server" ID="newPageWizardShort" Name="bxNewPageWizard" 
        Enabled="true" UseStandardStyles="true">
        <Steps>
            <bx:BXPageAsDialogWizardStep ID="newPageWizardShortCommon" OnShowClientScript="Bitrix.NewPageWizard.showFirstStep"
                LinkedContentContainerID="tblNewPageCommon" >
                <NavigationCommands>
                    <bx:BXPageAsDialogWizardNavigationCommand  ID="newPageWizardShortCommonNex" CommandType="GoToNext" TargetStepID="newPageWizardShortProp"/>
                    <bx:BXPageAsDialogWizardNavigationCommand ID="newPageWizardShortCommonComplete" CommandType="Complete" OnCallClientScript="Bitrix.NewPageWizard.onBeforeSave" />
                </NavigationCommands>
            </bx:BXPageAsDialogWizardStep>        
            <bx:BXPageAsDialogWizardStep ID="newPageWizardShortProp"
                LinkedContentContainerID="tblNewPageProp">                            
                <NavigationCommands>
                    <bx:BXPageAsDialogWizardNavigationCommand  ID="newPageWizardShortPropPrevious" CommandType="GoToPrevious" TargetStepID="newPageWizardShortCommon" 
                        OnCallClientScript="Bitrix.NewPageWizard.onThirdStepPrev"/>
                    <bx:BXPageAsDialogWizardNavigationCommand ID="newPageWizardShortPropComplete" CommandType="Complete" 
                        OnCallClientScript="Bitrix.NewPageWizard.onBeforeSave" />
                </NavigationCommands>                   
            </bx:BXPageAsDialogWizardStep>
        </Steps>
    </bx:BXPageAsDialogWizardBehaviour>              
    <% } %> 
    <form id="form1" runat="server">
        <table id="tblNewPageCommon" runat="server" class="bx-width100" style="display:auto;">
            <tbody>
                <tr>
		            <td class="bx-popup-label bx-width30"><%= GetMessage("Legend.FileName") %></td>
		            <td>
		                <asp:TextBox ID="tbxPageName" runat="server"></asp:TextBox>
		                <br />
		                <asp:RegularExpressionValidator runat="server" ID="vlrPageName" Display="Dynamic" 
		                    Text="" EnableClientScript="true"
		                    ControlToValidate="tbxPageName" SetFocusOnError="true">
		                </asp:RegularExpressionValidator>
		                <asp:RequiredFieldValidator runat="server" ID="vlrPageNameRequired" Display="Dynamic"
		                    Text="" EnableClientScript="true"
		                    ControlToValidate="tbxPageName">
		                </asp:RequiredFieldValidator>
		            </td>
	            </tr>
	            <tr>
		                <td class="bx-popup-label bx-width30"><%= GetMessage("LegendPageTitle") %></td>
		                <td>
		                    <asp:TextBox ID="tbxTitle" runat="server"></asp:TextBox>
		                </td>
	                </tr>
		        <tr>
		                <td class="bx-popup-label bx-width30"></td>
		                <td>
		                    <asp:CheckBox ID="chbxEditAfterSave" runat="server" Checked="true" Text="<%$ Loc:CheckBoxText.Go2ModificationOfPage %>" />
		                </td>
	                </tr>            	
		        <tr>
		                <td class="bx-popup-label bx-width30"></td>
		                <td>
							<asp:CheckBox ID="chbxAddToMenu" runat="server" Checked="true" Text="<%$ Loc:CheckBoxText.AddToMenu %>" />
		                </td>
	                </tr>     
            </tbody>
        </table>
        <% 
        	if (!_menuItemsVisible) tblNewPageMenuItems.Style["display"] = "none";
			if (!_createMenuVisible) tblNewPageMenuCheck.Style["display"] = "none";
        %>
        <table id="tblNewPageMenu" runat="server" class="bx-width100" style=" display:none;">
	        <tbody>
	            <tr>
		            <td class="bx-popup-label bx-width30"><%= GetMessage("Legend.MenuItemName") %></td>
		            <td>
		                <asp:TextBox ID="tbxMenuName" runat="server" EnableViewState="false"></asp:TextBox>
		            </td>
	            </tr>
	            <tr>
		            <td class="bx-popup-label bx-width30"><%= GetMessage("Legend.MenuType") %></td>
		            <td>
						<asp:DropDownList ID="ddlMenuTypes" runat="server" EnableViewState="false"></asp:DropDownList>
		            </td>
	            </tr>
	            
	            <tr id="tblNewPageMenuItems">
		            <td  valign="top" class="bx-popup-label bx-width30"><%= GetMessage("Legend.InsertIntoMenuBefore") %></td>
		            <td>
				        <asp:DropDownList ID="ddlMenuItems" runat="server" EnableViewState="false"></asp:DropDownList>
				        <asp:HiddenField ID="hfMenuPosition" runat="server" Value="-1" />
		            </td>
	            </tr>
	            <tr id="tblNewPageMenuCheck">
		            <td valign="top" class="bx-popup-label bx-width30">&nbsp;</td>
		            <td>
						<input type="checkbox" id="<%= ClientID %><%= ClientIDSeparator %>cbCreateInSection" name="<%= UniqueID %><%= IdSeparator %>cbCreateInSection" value="Y" <% if (_createMenuVisible && !_menuItemsVisible) { %> checked="checked" disabled="disabled" <% } else if (_createMenuChecked) { %> checked="checked" <% } %> />
				        <label for="<%= ClientID %><%= ClientIDSeparator %>cbCreateInSection"><%= GetMessage("Legend.CreateMenu") %></label>
		            </td>
	            </tr>
            </tbody>
        </table>
        <asp:Repeater ID="tblNewPageProp" runat="server">
            <HeaderTemplate>
                <table id="tblNewPageProp" class="bx-width100" style="display:none;">
                    <tbody>
                        <tr class="section">
		                    <td colspan="2">
			                    <table cellspacing="0">
				                    <tbody>
				                        <tr>
					                        <td><%= GetMessage("Legend.PageProperties") %></td>
					                        <td id="bx_page_prop_name"> 
					                        </td>
				                        </tr>
			                        </tbody>
			                    </table>
		                    </td>
	                    </tr>             
            </HeaderTemplate>
            <ItemTemplate>
	            <tr style="height:30px;">
	                <td class="bx-popup-label bx-width30"><%# HttpUtility.HtmlEncode((string)Eval("Value")) + ":"%></td>
	                <td>
                        <div title="<%# IsKeywordValueChanged((string)Eval("Key")) ? "" : GetMessage("InheritedPageProperty.Modify")%>" style="<%# string.Format("border:1px solid white; padding:2px 12px 2px 2px; overflow:hidden; width:90%; cursor:text; -moz-box-sizing:border-box; background-color:transparent; background-position:right center; background-repeat:no-repeat; {0}", IsKeywordValueChanged((string)Eval("Key")) ? "display:none;" : "display:block;") %>" id="<%# string.Format("bx_view_property_{0}", ((RepeaterItem)Container).ItemIndex) %>" class="edit-field" 
                            onmouseout="this.style.borderColor = 'white'" 
                            onmouseover="this.style.borderColor = '#434B50 #ADC0CF #ADC0CF #434B50';" 
                            onclick="<%# string.Format("Bitrix.NewPageWizard.editProperty({0})", ((RepeaterItem)Container).ItemIndex) %>" ><%# HttpUtility.HtmlEncode(GetKeywordValue((string)Eval("Key")))%></div>	
                        <div style="<%# IsKeywordValueChanged((string)Eval("Key")) ? "display:block;" : "display:none;" %>" id="<%# string.Format("bx_edit_property_{0}", ((RepeaterItem)Container).ItemIndex) %>">
                            <input type="text" name="<%# string.Format("PROPERTY[{0}][VALUE]", ((RepeaterItem)Container).ItemIndex) %>" 
                                style="padding: 2px; width: 90%;" id="<%# string.Format("bx_property_input_{0}", ((RepeaterItem)Container).ItemIndex) %>" 
                                value="<%# HttpUtility.HtmlAttributeEncode(GetKeywordValue((string)Eval("Key")))%>"
                                onblur="<%# IsKeywordValueChanged((string)Eval("Key")) ? string.Empty : string.Format("Bitrix.NewPageWizard.blurProperty(this,{0})", ((RepeaterItem)Container).ItemIndex ) %>"/>                        
                        </div>                                  
                        <input type="hidden" value="<%# HttpUtility.HtmlAttributeEncode((string)Eval("Key")) %>" name="<%# string.Format("PROPERTY[{0}][CODE]", ((RepeaterItem)Container).ItemIndex) %>"/>
	                </td>
	            </tr>            
            </ItemTemplate>
            <FooterTemplate>
                    </tbody>
                </table>
            </FooterTemplate>
        </asp:Repeater>
<script type="text/javascript">                                                                                    
    if(typeof(Bitrix) == "undefined"){ var Bitrix = {}; }
    
    Bitrix.NewPageWizard = function Bitrix$NewPageWizard() {
        this._initialized = false;
        this._nameElID = "<%= tbxPageName.ClientID %>";
        this._addToMenuElID = "<%= chbxAddToMenu.ClientID %>";
        this._nameContentVlrElID = "<%= vlrPageName.ClientID %>";
        this._nameRequiredVlrElID = "<%= vlrPageNameRequired.ClientID %>"; 
        this._menuNameElID = "<%= tbxMenuName.ClientID %>";   
        this._ttlElID = "<%= tbxTitle.ClientID %>"; 
		this._menuElID = "<%= ddlMenuItems.ClientID %>";		
    }
    
    Bitrix.NewPageWizard.prototype = {
        initialize: function(){
            if(typeof(BXHint) != 'undefined')
	            this._getElById("bx_page_prop_name").appendChild((new BXHint("<%= GetMessageJS("PagePropsHint") %>")).oIcon);        
            this._initialized = true;
        },
        showFirstStep: function(wizard) {
            var addToMenu = this._getElById(this._addToMenuElID);
	        if (addToMenu && addToMenu.checked) {
                wizard.SetButtonDisabled("next", false);
		        wizard.SetButtonDisabled("finish", true);
	        }
	        else if (!wizard.IsStepExists("<%= DialogWizard.GetStepClientID(IsAnyMenuDefined ? "newPageWizardLongProp" : "newPageWizardShortProp") %>"))
                wizard.SetButtonDisabled("next", true);
	          
	        this._select(this._nameElID);    
        },
        onFirstStepNext: function(wizard)
        {
            if(!this._isFileNameValid()) {
                this._select(this._nameElID);
                return false;
            }
        
            var addToMenu = this._getElById(this._addToMenuElID)
            if (!(addToMenu && addToMenu.checked))
                wizard.SetCurrentStep("<%= DialogWizard.GetStepClientID(IsAnyMenuDefined ? "newPageWizardLongProp" : "newPageWizardShortProp") %>");
            else {
                var menuName = this._getElById(this._menuNameElID);
                if (menuName.value == "" || menuName.disabled)
                    menuName.value = this._getElById(this._ttlElID).value;
            }
            return true;
        },
        showSecondStep: function(wizard) { 
            this._select(this._menuNameElID); 
        },
        onThirdStepPrev: function(wizard)
        {
            var addToMenu = this._getElById(this._addToMenuElID)
            if (!(addToMenu && addToMenu.checked)) {
                var curStep = "<%= IsAnyMenuDefined ? DialogWizard.GetStepClientID("newPageWizardLongMenu") : "" %>";
                if(curStep.length > 0)
	                wizard.SetCurrentStep(curStep);            
            }                    
        },
        onBeforeSave: function(wizard) {                          
            if(this._isFileNameValid())
                return true;
                
            wizard.SetCurrentStep("<%= DialogWizard.GetStepClientID(IsAnyMenuDefined ? "newPageWizardLongCommon" : "newPageWizardShortCommon") %>");
            return false;
        },        
        editProperty: function(propertyIndex) {
            var viewProp = this._getElById("bx_view_property_" + propertyIndex);
            viewProp.style.display = "none";
            var editProp = this._getElById("bx_edit_property_" + propertyIndex);
            editProp.style.display = "block";	
                        
            var input = this._getElById("bx_property_input_" + propertyIndex);           
            if(!input){
                input = document.createElement("INPUT");
                input.type = "text";
                input.name = "PROPERTY["+propertyIndex+"][VALUE]";
                input.style.width = "90%";
                input.style.padding = "2px";
                input.id = "bx_property_input_" + propertyIndex;
                
                var self = this;
                input.onblur = function(){ self.blurProperty(input,propertyIndex); };
                
                input.value = viewProp.innerHTML;
                editProp.appendChild(input);	            
            }            
            input.focus();
            input.select();
        },
        blurProperty: function(element, propertyIndex) {
            var viewProp = this._getElById("bx_view_property_" + propertyIndex);
            if (element.value == "" || element.value == viewProp.innerHTML)
            {
	            var editProp = this._getElById("bx_edit_property_" + propertyIndex);

	            viewProp.style.display = "block";
	            editProp.style.display = "none";

	            while (editProp.firstChild)
		            editProp.removeChild(editProp.firstChild);
            }
        },  
		onChangeMenuType: function(menuType, onChange) {              
			var menuItems = this._getElById(this._menuElID);
			if (!menuItems)
				return;

			menuItems.options.length = 0;

			//Create options list
			var selectDocument = menuItems.ownerDocument;
			if (!selectDocument)
				selectDocument = menuItems.document;
							
			var hasItems = bxMenuType[menuType] && bxMenuType[menuType]["ITEMS"];
			selectDocument.getElementById("<%= tblNewPageMenuItems.ClientID %>").style.display = hasItems ? '' : 'none';	
			selectDocument.getElementById("<%= tblNewPageMenuCheck.ClientID %>").style.display = bxMenuType[menuType]["SECTION"] ? '' : 'none';	
							
			var cbCreate = selectDocument.getElementById("<%= ClientID %><%= ClientIDSeparator %>cbCreateInSection")
			cbCreate.disabled = !hasItems;
			cbCreate.checked = !hasItems;
			
			if (hasItems)
			{
				var length = bxMenuType[menuType]["ITEMS"].length;
				
				for (var itemPosition = 0; itemPosition < length; itemPosition++)
				{
					var option = selectDocument.createElement("OPTION");
					option.text = bxMenuType[menuType]["ITEMS"][itemPosition];
					option.value = itemPosition;
					menuItems.options.add(option);
				}

				var option = selectDocument.createElement("OPTION");
				option.text = "<%= GetMessage("LastMenuItem") %>";
				option.value = itemPosition;
				menuItems.options.add(option);

				var menuItemPosition = document.getElementById("<%= hfMenuPosition.ClientID %>");
				if ( (onChange && onChange == true) || (menuItemPosition && menuItemPosition.value < 0) )
					menuItems.selectedIndex = menuItems.options.length - 1;
				else if (menuItemPosition)
					menuItems.selectedIndex = menuItemPosition.value;
			}
		},
		onChangeAddToMenu: function(checked) {
			if (window.bxNewPageWizard)
				window.bxNewPageWizard.SetButtonDisabled("finish", checked);         
		},		
        _getElById: function(id) { return document.getElementById(id); },      
        _select: function(id) {
            var el = this._getElById(id);
            if (!el) return;
            el.focus();
            el.select();              
        },
        _isFileNameValid: function() {
            var contentVl = this._getElById(this._nameContentVlrElID), requiredVl = this._getElById(this._nameRequiredVlrElID);             
            return (!contentVl || contentVl.style.visibility == "hidden" || contentVl.style.display == "none") && (!requiredVl || requiredVl.style.visibility == "hidden" || requiredVl.style.display == "none");
        }                
    }
    
    Bitrix.NewPageWizard._instance = null;
    Bitrix.NewPageWizard.getInstance = function() {
        if(this._instance) return this._instance;
        var self = this._instance = new Bitrix.NewPageWizard();
        self.initialize();
        return self;
    }
    Bitrix.NewPageWizard.onPageLoad = function(){ this.getInstance(); }
    Bitrix.NewPageWizard.showFirstStep = function(wizard) { this.getInstance().showFirstStep(wizard); }
    Bitrix.NewPageWizard.showSecondStep = function(wizard) { this.getInstance().showSecondStep(wizard); }
    Bitrix.NewPageWizard.onFirstStepNext = function(wizard) { return this.getInstance().onFirstStepNext(wizard); }
    Bitrix.NewPageWizard.onThirdStepPrev = function(wizard) { return this.getInstance().onThirdStepPrev(wizard); }
    Bitrix.NewPageWizard.editProperty = function(propInd) { this.getInstance().editProperty(propInd); }   
    Bitrix.NewPageWizard.blurProperty = function(el, propInd) { this.getInstance().blurProperty(el, propInd); }
    Bitrix.NewPageWizard.onBeforeSave = function(wizard) { return this.getInstance().onBeforeSave(wizard); } 
	Bitrix.NewPageWizard.onChangeMenuType = function(menuType, onChange) { return this.getInstance().onChangeMenuType(menuType, onChange); }
	Bitrix.NewPageWizard.onChangeAddToMenu = function(checked) { return this.getInstance().onChangeAddToMenu(checked); }                                                                                            
</script>        
    </form>
</body>
</html>
