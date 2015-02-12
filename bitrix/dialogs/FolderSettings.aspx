<%@ Page Language="C#" AutoEventWireup="true" CodeFile="FolderSettings.aspx.cs" Inherits="bitrix_dialogs_FolderSettings" 
 ValidateRequest="false" EnableEventValidation="true" Title="<%$ Loc:TITLE %>"  %>
<html>
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <bx:BXPageAsDialogBehaviour runat="server" ID="Behaviour" UseStandardStyles="true" OnSave="Behaviour_Save" />
            <asp:Repeater ID="tblFolderProp" runat="server">
            <HeaderTemplate>
                <table id="tblFolderProp" class="bx-width100" ><tbody>	                   
                    <%--<tr class="section">
                        <td colspan="2">
                            <table cellspacing="0"><tbody>
			                    <tr>
				                    <td><%= GetMessage("Title.Title") %></td>
                                </tr>
		                    </tbody></table>                           
                        </td>
                    </tr>
                    <tr>
                        <td class="bx-popup-label bx-width30"><%= GetMessage("Legend.Title") %></td>
                        <td><input type="text" value='<%= ClientFolderTitle %>' name="folderTitle" id="folderTitle" style="width:90%;"/></td>                        
                    </tr>--%>
                    <tr class="empty">
                        <td colspan="2"><div class="empty"></div></td>
                    </tr>	                           	                                    				                    
                    <tr class="section">
                        <td colspan="2">
                            <table cellspacing="0"><tbody>
			                    <tr>
				                    <td><%= GetMessage("TITLE") %></td>
				                    <td>&nbsp;</td>
				                    <td id="bx_folder_prop_name"></td>
                                </tr>
		                    </tbody></table>                            
                        </td>
                    </tr>           
            </HeaderTemplate>
            <ItemTemplate>
	            <tr style="height:30px;">
	                <td class="bx-popup-label bx-width30"><%# HttpUtility.HtmlEncode((string)Eval("Value"))+ ":" %></td>
	                <td>
                        <div 
                            style="<%# string.Format("border:1px solid white; padding:2px 12px 2px 2px; overflow:hidden; width:90%; cursor:text; -moz-box-sizing:border-box; background-color:transparent; background-position:right center; background-repeat:no-repeat; {0}", IsKeywordValueChanged((string)Eval("Key")) ? "display:none;" : "display:block;") %>"
                            id="<%# string.Format("bx_view_property_{0}", ((RepeaterItem)Container).ItemIndex) %>" class="edit-field" 
                            onmouseout="this.style.borderColor = 'white';" 
                            onmouseover="this.style.borderColor = '#434B50 #ADC0CF #ADC0CF #434B50';"
                            title="<%# IsKeywordValueChanged((string)Eval("Key")) ? "" : GetMessage("InheritedFolderProperty.Modify") %>" 
                            onclick="<%# string.Format("BXEditProperty({0})", ((RepeaterItem)Container).ItemIndex) %>" ><%# GetKeywordValue((string)Eval("Key"), true)%></div>	
                        <div 
                            style="<%# IsKeywordValueChanged((string)Eval("Key")) ? "display:block;" : "display:none;" %>" 
                            id="<%# string.Format("bx_edit_property_{0}", ((RepeaterItem)Container).ItemIndex) %>">
                            <input type="text" 
                                name="<%# string.Format("PROPERTY[{0}][VALUE]", ((RepeaterItem)Container).ItemIndex) %>" 
                                style="padding: 2px; width: 90%;" 
                                id="<%# string.Format("bx_property_input_{0}", ((RepeaterItem)Container).ItemIndex) %>" 
                                value="<%# GetKeywordValue((string)Eval("Key"), true)%>"
                                onblur="<%# IsKeywordValueChanged((string)Eval("Key")) ? string.Empty : string.Format("window.BXBlurProperty(this,{0})", ((RepeaterItem)Container).ItemIndex ) %>"/>                        
                        </div>                                                                       
                        <input type="hidden" 
                            value="<%# HttpUtility.HtmlEncode((string)Eval("Key")) %>" 
                            name="<%# string.Format("PROPERTY[{0}][CODE]", ((RepeaterItem)Container).ItemIndex) %>"/>
	                </td>
	            </tr>            
            </ItemTemplate>
            <FooterTemplate>
                </tbody></table>
            </FooterTemplate>
        </asp:Repeater>				
    </div>
    </form>
    <script type="text/javascript">
    	window.BXEditProperty = function(propertyIndex) {
    		var editProperty = document.getElementById("bx_edit_property_" + propertyIndex);
    		var viewProperty = document.getElementById("bx_view_property_" + propertyIndex);

    		var input = document.getElementById("bx_property_input_" + propertyIndex);
    		if (!input) {
    			input = document.createElement("INPUT");

    			input.type = "text";
    			input.name = "PROPERTY[" + propertyIndex + "][VALUE]";
    			input.style.width = "90%";
    			input.style.padding = "2px";
    			input.id = "bx_property_input_" + propertyIndex;
    			input.onblur = function() { BXBlurProperty(input, propertyIndex); };
    			input.value = viewProperty.innerHTML;

    			editProperty.appendChild(input);
    		}

    		viewProperty.style.display = "none";
    		editProperty.style.display = "block";

    		input.focus();
    		input.select();
    	};
    	
    	window.BXBlurProperty = function(element, propertyIndex) {
    		var viewProperty = document.getElementById("bx_view_property_" + propertyIndex);
    		if (element.value == "" || element.value == viewProperty.innerHTML) {
    			var editProperty = document.getElementById("bx_edit_property_" + propertyIndex);

    			viewProperty.style.display = "block";
    			editProperty.style.display = "none";

    			while (editProperty.firstChild)
    				editProperty.removeChild(editProperty.firstChild);
    		}
    	};
        
        (function()
        {
            var td = document.getElementById("bx_folder_prop_name");
            if(td == null) return;
            var hint = new BXHint("<%= GetMessageJS("SectionPropsHint") %>");
            td.appendChild(hint.oIcon);        	
        })();
        
    	(function() {
    		var pageTitleInput = document.getElementById("pageTitle");
    		if (pageTitleInput != null) pageTitleInput.focus();
    	})();                  
    </script>    
</body>
</html>
