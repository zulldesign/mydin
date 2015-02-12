<%@ Page Language="C#" AutoEventWireup="false" CodeFile="NewFolderWizard.aspx.cs" Inherits="bitrix_dialogs_NewFolderWizard" 
    ValidateRequest="false" EnableEventValidation="true" EnableViewState="false" Title="<%$ Loc:TITLE %>" %> %>

<html>
<head runat="server"><title></title></head>
<body>
    <bx:BXPageAsDialogWizardBehaviour runat="server" ID="newFolderWizard" Name="bxNewFolderWizard" 
            Enabled="true" UseStandardStyles="true" >
            <Steps>
                <bx:BXPageAsDialogWizardStep ID="newFolderWizardCommon" OnShowClientScript="BXFirstStepShow"
                    LinkedContentContainerID="tblNewFolderCommon" >
                    <NavigationCommands>
                        <bx:BXPageAsDialogWizardNavigationCommand  ID="newFolderWizardCommonNext" CommandType="GoToNext" 
                            TargetStepID="newFolderWizardMenu" OnCallClientScript="BXFirstStepNext"/>
                        <bx:BXPageAsDialogWizardNavigationCommand ID="newFolderWizardCommonDone" CommandType="Complete" 
                            OnCallClientScript="BXBeforeNewFolderSave" />
                    </NavigationCommands>
                </bx:BXPageAsDialogWizardStep>
                
                <bx:BXPageAsDialogWizardStep ID="newFolderWizardMenu" OnShowClientScript="BXSecondStepShow"
                    LinkedContentContainerID="tblNewFolderMenu">
                    <NavigationCommands>
                        <bx:BXPageAsDialogWizardNavigationCommand  ID="newFolderWizardMenuPrevious"
                            CommandType="GoToPrevious" TargetStepID="newFolderWizardCommon"/>
                        <bx:BXPageAsDialogWizardNavigationCommand  ID="newFolderWizardMenuNext" 
                            CommandType="GoToNext" TargetStepID="newFolderWizardProp"/>
                        <bx:BXPageAsDialogWizardNavigationCommand ID="newFolderWizardMenuDone" 
                            CommandType="Complete" OnCallClientScript="BXBeforeNewFolderSave" />
                    </NavigationCommands>                
                </bx:BXPageAsDialogWizardStep>
   
                <bx:BXPageAsDialogWizardStep ID="newFolderWizardProp"
                    LinkedContentContainerID="tblNewFolderProp">                            
                    <NavigationCommands>
                        <bx:BXPageAsDialogWizardNavigationCommand  ID="newFolderWizardPropPrevious" 
                            CommandType="GoToPrevious" TargetStepID="newFolderWizardMenu" 
                            OnCallClientScript="BXThirdStepPrev"/>
                        <bx:BXPageAsDialogWizardNavigationCommand ID="newPageWizardLongPropDone" 
                            CommandType="Complete" OnCallClientScript="BXBeforeNewFolderSave" />
                    </NavigationCommands>                   
                </bx:BXPageAsDialogWizardStep>
            </Steps>
        </bx:BXPageAsDialogWizardBehaviour> 
    <form id="form1" runat="server">
    <div> 
        <table id="tblNewFolderCommon" runat="server" class="bx-width100" style="display:auto;">
            <tbody>
                <tr>
		            <td class="bx-popup-label bx-width30"><%= GetMessage("Legend.FolderName") %></td>
		            <td>
		                <asp:TextBox ID="tbxFolderName" runat="server"></asp:TextBox>
		                <br />
		                <asp:RegularExpressionValidator runat="server" ID="vlrFolderName" 
		                    Display="Dynamic" Text="" EnableClientScript="true"
		                    ControlToValidate="tbxFolderName" 
		                    SetFocusOnError="true">
		                </asp:RegularExpressionValidator>
		                <asp:RequiredFieldValidator runat="server" ID="vlrFolderNameRequired" 
		                    Display="Dynamic" Text="" EnableClientScript="true"
		                    ControlToValidate="tbxFolderName">
		                </asp:RequiredFieldValidator>
		            </td>
	            </tr>
	            <tr>
		                <td class="bx-popup-label bx-width30"><%= GetMessage("Legend.SectionTitle") %></td>
		                <td>
		                    <asp:TextBox ID="tbxTitle" runat="server"></asp:TextBox>
		                </td>
	                </tr>
		        <tr>
		                <td class="bx-popup-label bx-width30"></td>
		                <td>
		                    <asp:CheckBox ID="chbxEditAfterSave" runat="server" Checked="true" Text="<%$ Loc:CheckBoxText.Go2ModificationOfIndexPage %>" />
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
        <table id="tblNewFolderMenu" runat="server" class="bx-width100" style=" display:none;">
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
		            <td valign="top" class="bx-popup-label bx-width30"><%= GetMessage("Legend.InsertIntoMenuBeforeItem") %></td>
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
        <asp:Repeater ID="tblNewFolderProp" runat="server">
            <HeaderTemplate>
                <table id="tblNewFolderProp" class="bx-width100" style="display:none;">
                    <tbody>
                        <tr class="section">
		                    <td colspan="2">
			                    <table cellspacing="0">
				                    <tbody>
				                        <tr>
					                        <td><%= GetMessage("Legend.FolderProperties")%></td>
					                        <td id="bx_folder_prop_name"> 
					                        </td>
				                        </tr>
			                        </tbody>
			                    </table>
		                    </td>
	                    </tr>             
            </HeaderTemplate>
            <ItemTemplate>
	            <tr style="height:30px;">
	                <td class="bx-popup-label bx-width30"><%# HttpUtility.HtmlEncode((string)Eval("Value"))+ ":" %></td>
	                <td>
                        <div title="<%# AboutKeywordViewMode((string)Eval("Key")) ? GetMessage("InheritedFolderProperty.Modify") : ""  %>" style="<%# string.Format("border:1px solid white; padding:2px 12px 2px 2px; overflow:hidden; width:90%; cursor:text; -moz-box-sizing:border-box; background-color:transparent; background-position:right center; background-repeat:no-repeat; {0}", AboutKeywordViewMode((string)Eval("Key")) ? "display:block;" : "display:none;") %>" id="<%# string.Format("bx_view_property_{0}", ((RepeaterItem)Container).ItemIndex) %>" class="edit-field" 
                            onmouseout="this.style.borderColor = 'white'" 
                            onmouseover="this.style.borderColor = '#434B50 #ADC0CF #ADC0CF #434B50';" 
                            onclick="<%# string.Format("BXEditProperty({0})", ((RepeaterItem)Container).ItemIndex) %>" ><%# HttpUtility.HtmlEncode(GetKeywordValue((string)Eval("Key")))%></div>	
                        <div style="<%# AboutKeywordViewMode((string)Eval("Key")) ? "display:none;" : "display:block;" %>" id="<%# string.Format("bx_edit_property_{0}", ((RepeaterItem)Container).ItemIndex) %>">
                            <input type="text" name="<%# string.Format("PROPERTY[{0}][VALUE]", ((RepeaterItem)Container).ItemIndex) %>" 
                                style="padding: 2px; width: 90%;" id="<%# string.Format("bx_property_input_{0}", ((RepeaterItem)Container).ItemIndex) %>" 
                                value="<%# HttpUtility.HtmlAttributeEncode(GetKeywordValue((string)Eval("Key")))%>"
                                onblur="<%# AboutKeywordViewMode((string)Eval("Key")) ? string.Format("window.BXBlurProperty(this,{0})", ((RepeaterItem)Container).ItemIndex ) : string.Empty %>"/>                        
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
    </div> 
<script type="text/javascript">
            window.BXChangeMenuType = function(menuType, onChange)
            {              
	            var menuItems = document.getElementById("<%= ddlMenuItems.ClientID %>");
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
					option.text = "<%= GetMessage("LAST_MENU_ITEM") %>";
					option.value = itemPosition;
					menuItems.options.add(option);

					var menuItemPosition = document.getElementById("<%= hfMenuPosition.ClientID %>");
					if ( (onChange && onChange == true) || (menuItemPosition && menuItemPosition.value < 0) )
						menuItems.selectedIndex = menuItems.options.length - 1;
					else if (menuItemPosition)
						menuItems.selectedIndex = menuItemPosition.value;
				}
            }
            
            window.BXChangeAddToMenu = function(checked)
            {
		        if (!window.bxNewFolderWizard)
		            return;
		        
		        //window.bxNewFolderWizard.SetButtonDisabled("next", !checked);
		                         
			    window.bxNewFolderWizard.SetButtonDisabled("finish", checked);         
            }            
            
            window.BXFirstStepShow = function(wizard)
            {
	            var addToMenu = document.getElementById("<%= chbxAddToMenu.ClientID %>");

	            if (addToMenu && addToMenu.checked)
	            {
		            wizard.SetButtonDisabled("next", false);
		            wizard.SetButtonDisabled("finish", true);
	            }
	            else
	            {
		            if (!wizard.IsStepExists("<%= DialogWizard.GetStepClientID("newFolderWizardProp") %>"))
			            wizard.SetButtonDisabled("next", true);
	            }
	            
	            BXFolderNameSelect();	            
            }             
            
            window.BXFirstStepNext = function(wizard)
            {
                var isFolderNameValid = BXIsFolderNameValid();
                if(!isFolderNameValid){
                    BXFolderNameSelect();
                    return false;
                }
            
	            var addToMenu = document.getElementById("<%= chbxAddToMenu.ClientID %>");

	            if (!addToMenu || !addToMenu.checked)
	            {
                    wizard.SetCurrentStep("<%= DialogWizard.GetStepClientID("newFolderWizardProp") %>");
		            return true;
	            }

	            //Set item name equal title
	            var menuName = document.getElementById("<%= tbxMenuName.ClientID %>");
	            var folderTitle = document.getElementById("<%= tbxTitle.ClientID %>");

	            if (menuName && folderTitle && (menuName.value == "" || menuName.disabled))
		            menuName.value = folderTitle.value;
		        return true;		            
            }  
             
            window.BXSecondStepShow = function()
            {
                BXMenuNameSelect();
            }
                                 
            //Save
            window.BXBeforeNewFolderSave = function(wizard)
            {
                var isValid = BXIsFolderNameValid(); 
                
                if(isValid)
                    return true;
                    
                wizard.SetCurrentStep("<%= DialogWizard.GetStepClientID("newFolderWizardCommon") %>");
                BXFolderNameSelect();
                return false;
            }

            window.BXIsFolderNameValid = function(){
                var folderNameValidatorContent = document.getElementById("<%= vlrFolderName.ClientID %>");
                var folderNameValidatorRequired = document.getElementById("<%= vlrFolderNameRequired.ClientID %>");
                
                if(!folderNameValidatorContent && !folderNameValidatorRequired)
                    return true;
                                
                var isValid = true;
                if(folderNameValidatorContent != null)
                    isValid = folderNameValidatorContent.style.visibility == "hidden" || folderNameValidatorContent.style.display == "none";
                if(folderNameValidatorRequired != null && isValid)
                    isValid = folderNameValidatorRequired.style.visibility == "hidden" || folderNameValidatorRequired.style.display == "none";
                    
                return isValid;            
            }
            
            window.BXFolderNameSelect = function()
            {
                BXSelectInput("<%= tbxFolderName.ClientID %>");
            }
            
            window.BXMenuNameSelect = function(){
                BXSelectInput("<%= tbxMenuName.ClientID %>");             
            } 
            
            window.BXSelectInput = function(id){
	            var input = document.getElementById(id);
	            if (input)
	            {
	                try
	                {
		                input.focus();
		                input.select();
		            }
		            catch(e){}
	            }                
            }
            
            window.BXThirdStepPrev = function(wizard)
            {
	            var addToMenu = document.getElementById("<%= chbxAddToMenu.ClientID %>");

	            if (!addToMenu || !addToMenu.checked)
	            {
	                var curStep = "<%= IsAnyMenuDefined ? DialogWizard.GetStepClientID("newFolderWizardMenu") : "" %>";
	                if(curStep.length == 0)
	                    return;
		            wizard.SetCurrentStep(curStep);
	            }
            } 
            
            window.BXEditProperty = function(propertyIndex)
            {
	            var editProperty = document.getElementById("bx_edit_property_" + propertyIndex);
	            var viewProperty = document.getElementById("bx_view_property_" + propertyIndex);

	            var input = document.getElementById("bx_property_input_" + propertyIndex);
	            if(!input){
	                input = document.createElement("INPUT");

	                input.type = "text";
	                input.name = "PROPERTY["+propertyIndex+"][VALUE]";
	                input.style.width = "90%";
	                input.style.padding = "2px";
	                input.id = "bx_property_input_" + propertyIndex;
	                input.onblur = function(){BXBlurProperty(input,propertyIndex);};
	                input.value = viewProperty.innerHTML;

	                editProperty.appendChild(input);	            
	            }
	            
	            viewProperty.style.display = "none";
	            editProperty.style.display = "block";	            
	            
	            input.focus();
	            input.select();
            }  
            
            window.BXBlurProperty = function(element, propertyIndex)
            {
	            var viewProperty = document.getElementById("bx_view_property_" + propertyIndex);
	            if (element.value == "" || element.value == viewProperty.innerHTML)
	            {
		            var editProperty = document.getElementById("bx_edit_property_" + propertyIndex);

		            viewProperty.style.display = "block";
		            editProperty.style.display = "none";

		            while (editProperty.firstChild)
			            editProperty.removeChild(editProperty.firstChild);
	            }
            } 
            
            window.BXFolderEditHint = function()
            {
                if(typeof(BXHint) == 'undefined')
                    return;
	            var td = document.getElementById("bx_folder_prop_name");
	            if (td)
	            {
		            oBXHint = new BXHint("<%= GetMessageJS("SectionPropsHint") %>");
		            td.appendChild(oBXHint.oIcon);
	            }          	
            }
            window.BXFolderEditHint();             
                                                                                                                                   
        </script>       
    </form>       
</body>
</html>
