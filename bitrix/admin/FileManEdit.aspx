<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
    CodeFile="FileManEdit.aspx.cs" Inherits="bitrix_admin_FileManEdit" Title="<%$ Loc:PageTitle %>"
    EnableViewState="false" ValidateRequest="false" EnableViewStateMac="false" %>

<%@ Import Namespace="Bitrix.Services" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <%
        if (!fatalError)
        {
    %>
    <bx:InlineScript runat="server" ID="Script" AsyncMode="ScriptBlock">

        <script type="text/javascript" language="javascript">
    fileMan = <%= BuildScriptParameters() %>;
   
    window.fileMan_GoBack = function()
    {	
        return confirm('<%= GetMessageJS("ExitConfirm") %>');
    }

    window.fileMan_ConfirmFileNotPostBack = function(apply, validationGroup)
    {
        if (validationGroup && typeof(validationGroup) == "string" && validationGroup.length > 0 && typeof(Page_ClientValidate) == 'function' && !Page_ClientValidate(validationGroup))
            return false; 
        
        var text = document.getElementById(fileMan.saveAsId);
        var saveAsPath = document.getElementById(fileMan.saveAsPathId);
        var initialFile = fileMan.isNew ? '' : fileMan.curPath;
        var saveDir = '';
        if (saveAsPath.value != '')
            saveDir = jsUtils.Path.GetDirectory(saveAsPath.value);
        var saveFile = jsUtils.Path.Combine(saveDir == '' ? fileMan.curDir : saveDir, text.value);
        
        if (initialFile.toLowerCase() != saveFile.toLowerCase())
        {
            var url = jsUtils.Path.ToAbsolute('~/bitrix/admin/FileManEdit.aspx') 
                + '?path=' + encodeURIComponent(saveFile) 
                + '&check='
                + '&<%= JSEncode(Bitrix.Security.BXCsrfToken.BuildQueryStringPair()) %>';
        
            var request = null;
            if(window.XMLHttpRequest)
            {
                try {request = new XMLHttpRequest();} catch(e){}
            }
            else if(window.ActiveXObject)
            {
                try {request = new ActiveXObject("Microsoft.XMLHTTP");} catch(e){}
                if(!request)
                    try {request = new ActiveXObject("Msxml2.XMLHTTP");} catch (e){}
            }
            
            var isSuccessful = true;
            try{ 
                request.open("GET", url, false);
                request.send(null); 
            } 
            catch(e){
                isSuccessful = false;       
            }
            
            if(isSuccessful) 
                isSuccessful = request.status == 200;
            
            if(!isSuccessful){
                window.alert(fileMan.errorRequestFailedMessage);
                return false;
            }
            if (request.responseText == 'false')
                return true;
            var conf = confirm(fileMan.confirmMessage);
            if (conf == false && saveAsPath.value != '')
            {
                saveAsPath.value = fileMan.saveAsPath;
                text.value = fileMan.curFileName;
            }
            return conf;
        }
        else
            return true;
    }

        </script>

    </bx:InlineScript>
    <%
        }
        BXCmImageButton1.Href = BackUrl;
    %>
    <bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" runat="server">
        <Items>
            <bx:BXCmImageButton ID="BXCmImageButton1" runat="server" CssClass="context-button icon btn_folder_up"
                CommandName="back" Text="<%$ Loc:ActionText.GoBack %>" Title="<%$ Loc:ActionTitle.GoBack %>" />
        </Items>
    </bx:BXContextMenuToolbar>
    <bx:BXMessage ID="resultMessage" runat="server" CssClass="Ok" IconClass="Ok" Visible="False" />
    <bx:BXValidationSummary ID="ErrorMessage" runat="server" CssClass="errorSummary"
        HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="MainValidationGroup" ShowSummary="true" />
    <bx:BXTabControl ID="BXTabControl1" runat="server" ValidationGroup="MainValidationGroup" ButtonsMode="Hidden">
        <Tabs>
        <bx:BXTabControlTab runat="server" ID="mainTab" Selected="True" Text="<%$ Loc:TabText.Edit %>">
        
            <table cellpadding="0" cellspacing="0" border="0" class="edit-table" width="100%">
                <tr>
                    <td width="50%" class="field-name">
                        <asp:Literal ID="Literal7" runat="server" Text="<%$ Loc:SaveAs %>" />:
                    </td>
                    <td width="50%">
                        <asp:HiddenField ID="SaveAsPath" runat="server" />
                        <asp:TextBox ID="SaveAs" runat="server" Width="150px" />
                        <asp:RegularExpressionValidator runat="server" ID="vlrSaveAs" Display="Dynamic" EnableClientScript="true"
                            ControlToValidate="SaveAs" SetFocusOnError="false" ValidationGroup="MainValidationGroup">*</asp:RegularExpressionValidator>
                        <asp:RequiredFieldValidator runat="server" ID="vlrSaveAsRequired" Display="Dynamic"
                            EnableClientScript="true" ControlToValidate="SaveAs" SetFocusOnError="false"
                            ValidationGroup="MainValidationGroup">*</asp:RequiredFieldValidator>
                    </td>
                </tr>
                <tr id="EditPageTitle" runat="server">
                    <td width="50%" class="field-name" runat="server">
                        <asp:Literal ID="Literal3" runat="server" Text="<%$ Loc:EditPageTitle %>" />:
                    </td>
                    <td width="50%" runat="server">
                        <asp:TextBox ID="PageTitle" runat="server" Width="300px" /></td>
                </tr>
            </table>
            <asp:MultiView ID="Content" runat="server">
                <asp:View ID="TextView" runat="server">
                    <asp:TextBox ID="TextEditor" runat="server" TextMode="MultiLine" Width="100%" Height="450px" />
                </asp:View>
                <asp:View ID="VisualView" runat="server">
                    <bx:BXWebEditor ID="VisualEditor" runat="server" AutoLoadContent="False" Width="100%"
                        Height="650px" ContentType="Text" FullScreen="False" LimitCodeAccess="False"
                        OnIncludeWebEditorScript="VisualEditor_IncludeWebEditorScript" StartMode="HTMLVisual"
                        StartModeSelector="False" TemplateId="" UseHTMLEditor="True" UseOnlyDefinedStyles="False"
                        WithoutCode="False">
                        <Taskbars>
                            <bx:BXWebEditorBar Name="BXPropertiesTaskbar" />
                            <bx:BXWebEditorBar Name="ASPXComponentsTaskbar" />
                        </Taskbars>
                        <Toolbars>
                            <bx:BXWebEditorBar Name="manage" />
                            <bx:BXWebEditorBar Name="standart" />
                            <bx:BXWebEditorBar Name="style" />
                            <bx:BXWebEditorBar Name="formating" />
                            <bx:BXWebEditorBar Name="source" />
                            <bx:BXWebEditorBar Name="template" />
                            <bx:BXWebEditorBar Name="table" />
                        </Toolbars>
                    </bx:BXWebEditor>
                </asp:View>
            </asp:MultiView>
        </bx:BXTabControlTab>
        <bx:BXTabControlTab runat="server" ID="settingsTab" Text="<%$ Loc:TabText.Settings %>"
            Title="<%$ Loc:TabTitle.Settings %>">
            <asp:Repeater runat="server" ID="KeywordsTable" OnItemDataBound="KeywordsTable_ItemDataBound">
                <HeaderTemplate>
                    <table cellpadding="3" cellspacing="1" border="0" class="edit-table internal" width="100%">
                        <tr class="heading">
                            <td style="width: 40%">
                                <asp:Literal ID="Literal1" runat="server" Text="<%$ Loc:Column.Property %>" /></td>
                            <td style="width: 60%">
                                <asp:Literal ID="Literal2" runat="server" Text="<%$ Loc:Column.Value %>" /></td>
                        </tr>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td>
                            <%# HttpUtility.HtmlEncode((string)Eval("Value")) + ":" %>
                        </td>
                        <td>
                            <asp:TextBox runat="server" ID="Value" Width="100%" /><asp:Literal runat="server"
                                ID="Inherited" /></td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </table></FooterTemplate>
            </asp:Repeater>
        </bx:BXTabControlTab>
        <bx:BXTabControlTab ID="encodingOptions" runat="server" Text="<%$ Loc:TabText.Encodings %>"
            Title="<%$ Loc:TabTitle.Encodings %>">
            <table cellpadding="0" cellspacing="0" border="0" class="edit-table" width="100%">
                <tr>
                    <td width="50%" class="field-name">
                        <%= GetMessage("CurrentEncoding")+ ":" %>
                    </td>
                    <td width="50%">
                        <i>
                            <%= readEncoding.EncodingName %>
                        </i>
                    </td>
                </tr>
                <tr id="ReloadControls" runat="server">
                    <td class="field-name" runat="server">
                        <%= GetMessage("ReloadEncoding") + ":" %>
                    </td>
                    <td runat="server">
                        <asp:DropDownList ID="ChangeEncoding" runat="server" EnableViewState="False" Width="200px" />
                        <input type="button" value="<%= GetMessage("ButtonReload") %>" onclick="window.location.href = '<%= JSEncode(MakeChangeEncodingFileUrl()) %>&encoding=' + encodeURIComponent(document.getElementById('<%= ChangeEncoding.ClientID %>').value);" />
                    </td>
                </tr>
            </table>
        </bx:BXTabControlTab>
        <bx:BXTabControlTab ID="newPageOptions" runat="server" Text="<%$ Loc:TabText.NewPageOptions %>"
            Title="<%$ Loc:TabTitle.NewPageOptions %>">
            <table cellpadding="0" cellspacing="0" border="0" class="edit-table" width="100%">
                <tr class="heading">
                    <td width="100%">
                        <asp:CheckBox ID="AddMenu" runat="server" Text="<%$ Loc:AddMenuItem %>" EnableViewState="False" /></td>
                </tr>
                <tr id="MenuOptions" runat="server">
                    <td runat="server">
                        <table cellpadding="0" cellspacing="0" border="0" width="100%" class="edit-table">
                            <tr>
                                <td class="field-name" width="50%">
                                    <asp:Literal ID="Literal10" runat="server" Text="<%$ Loc:MenuType %>" />:
                                </td>
                                <td width="50%">
                                    <asp:DropDownList ID="MenuType" runat="server" /></td>
                            </tr>
                            <tr>
                                <td id="Td1" colspan="2" runat="server">
                                    <asp:Repeater ID="MenuTypeOptions" runat="server" OnItemDataBound="MenuTypeOptions_ItemDataBound">
                                        <ItemTemplate>
                                            <table id="Where" runat="server" cellpadding="0" cellspacing="0" border="0" width="100%">
                                                <tr>
                                                    <td class="field-name" width="50%">
                                                        <asp:Literal ID="Literal10" runat="server" Text="<%$ Loc:MenuOperation %>" />:
                                                    </td>
                                                    <td width="50%">
                                                        <asp:RadioButtonList ID="New" runat="server">
                                                            <asp:ListItem Value="new" Selected="True" Text="<%$ Loc:MenuOperation.New %>" />
                                                            <asp:ListItem Value="exists" Text="<%$ Loc:MenuOperation.Exists %>" />
                                                        </asp:RadioButtonList></td>
                                                </tr>
                                                <tr>
                                                    <td colspan="2">
                                                        <table id="NewOptions" runat="server" cellpadding="0" cellspacing="0" border="0"
                                                            width="100%">
                                                            <tr>
                                                                <td class="field-name" width="50%">
                                                                    <asp:Literal ID="Literal12" runat="server" Text="<%$ Loc:MenuNew.ItemName %>" />:
                                                                </td>
                                                                <td width="50%">
                                                                    <asp:TextBox ID="Name" runat="server" Width="200" /></td>
                                                            </tr>
                                                            <tr>
                                                                <td class="field-name">
                                                                    <asp:Literal ID="Literal13" runat="server" Text="<%$ Loc:MenuNew.InsertBefore %>" />:
                                                                </td>
                                                                <td>
                                                                    <asp:DropDownList ID="Before" runat="server" Width="200" /></td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colspan="2">
                                                        <table id="ExistsOptions" runat="server" cellpadding="0" cellspacing="0" border="0"
                                                            width="100%">
                                                            <tr>
                                                                <td class="field-name" width="50%">
                                                                    <asp:Literal ID="Literal14" runat="server" Text="<%$ Loc:MenuExists.ItemToAdd %>" />:
                                                                </td>
                                                                <td>
                                                                    <asp:DropDownList ID="AddTo" runat="server" Width="200" /></td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </table>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </bx:BXTabControlTab>
        </Tabs>
        <ButtonsBar>
            <asp:Button runat="server" ID="Save" Text="<%$ LocRaw:Kernel.Save %>" OnClick="SaveClick" ValidationGroup="MainValidationGroup" OnClientClick="if(!fileMan_ConfirmFileNotPostBack(false, 'MainValidationGroup')) return false;" />&nbsp;
            <asp:Button runat="server" ID="Apply" Text="<%$ LocRaw:Kernel.Apply %>" OnClick="ApplyClick" ValidationGroup="MainValidationGroup" OnClientClick="if(!fileMan_ConfirmFileNotPostBack(true, 'MainValidationGroup')) return false;" />&nbsp;
            <asp:Button runat="server" ID="Cancel" Text="<%$ LocRaw:Kernel.Cancel %>" OnClick="CancelClick" CausesValidation="false" OnClientClick="if (!fileMan_GoBack()) return false;" />
        </ButtonsBar>
    </bx:BXTabControl>
</asp:Content>
