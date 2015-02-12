<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="false"
    CodeFile="StorageConfigList.aspx.cs" Inherits="bitrix_admin_StorageList" Title="<%$ LocRaw:PageTitle %>" %>

<%@ Import Namespace="Bitrix.DataLayer" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:UpdatePanel ID="UpdatePanel" runat="server">
        <ContentTemplate>
            <bx:BXContextMenuToolbar ID="ItemListToolbar" runat="server">
                <Items>
                    <bx:BXCmSeparator SectionSeparator="true" />
                    <bx:BXCmImageButton ID="AddButton" Text="<%$ LocRaw:ActionText.Add %>" Title="<%$ LocRaw:ActionTitle.Add %>"
                        CssClass="context-button icon btn_new" Href="StorageConfigEdit.aspx" />
                </Items>
            </bx:BXContextMenuToolbar>
            <bx:BXPopupPanel ID="PopupPanelView" runat="server">
                <Commands>
                    <bx:CommandItem UserCommandId="edit" Default="True" IconClass="edit" ItemText="<%$ LocRaw:PopupText.Edit %>"
                        ItemTitle="<%$ LocRaw:PopupTitle.Edit %>" OnClickScript="window.location.href = 'StorageConfigEdit.aspx?id=' + UserData['ID']; return false;" />

                    <bx:CommandItem UserCommandId="load" Default="False" IconClass="edit" ItemText="<%$ LocRaw:PopupText.Load %>"
                        ItemTitle="<%$ LocRaw:PopupTitle.Load %>" />

                    <bx:CommandItem UserCommandId="unload" Default="False" IconClass="edit" ItemText="<%$ LocRaw:PopupText.Unload %>"
                        ItemTitle="<%$ LocRaw:PopupTitle.Unload %>" />

                    <bx:CommandItem UserCommandId="delete" Default="False" IconClass="delete" ItemText="<%$ LocRaw:PopupText.Delete %>"
                        ItemTitle="<%$ LocRaw:PopupTitle.Delete %>" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
                </Commands>
            </bx:BXPopupPanel>
            <bx:BXValidationSummary ID="ErrorMessage" runat="server" CssClass="errorSummary"
                HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="GridView" />
            <br />
            <div class="message" id="FileTransferStatusContainer" style="display:none;">
            </div>
            <bx:BXGridView ID="ItemGrid" runat="server" ContentName="<%$ LocRaw:TableTitle %>"
                AllowSorting="True" AllowPaging="True" DataKeyNames="ID" SettingsToolbarId="ItemListToolbar"
                PopupCommandMenuId="PopupPanelView" ContextMenuToolbarId="MultiActionMenuToolbar"
                DataSourceID="ItemGrid" OnSelect="ItemGrid_Select" OnSelectCount="ItemGrid_SelectCount"
                OnDelete="ItemGrid_Delete" OnRowDataBound="ItemGrid_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" ReadOnly="True" HeaderStyle-Width="50px" />
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.ProviderName %>">
                        <ItemTemplate>
                            <%# ((StorageConfigWrapper)Container.DataItem).ProviderName %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.IsActive %>" SortExpression="IsActive" HeaderStyle-Width="50px">
                        <ItemTemplate>
                            <%# ((StorageConfigWrapper)Container.DataItem).IsActive %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Sort %>" SortExpression="Sort" HeaderStyle-Width="200px">
                        <ItemTemplate>
                            <%# ((StorageConfigWrapper)Container.DataItem).Sort %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.CreatedUtc %>" SortExpression="CreatedUtc" HeaderStyle-Width="200px">
                        <ItemTemplate>
                            <%# ((StorageConfigWrapper)Container.DataItem).CreatedUtc %>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </bx:BXGridView>
            <bx:BXMultiActionMenuToolbar ID="MultiActionMenuToolbar" runat="server" ValidationGroup="GridView">
                <Items>
                    <bx:BXMamImageButton CommandName="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>"
                        ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>" EnabledCssClass="context-button icon delete"
                        DisabledCssClass="context-button icon delete-dis" Title="<%$ LocRaw:ActionTitle.Delete %>" />
                </Items>
            </bx:BXMultiActionMenuToolbar>
        </ContentTemplate>
    </asp:UpdatePanel>
<script type="text/javascript" language="javascript">
    if (typeof(Bitrix) == "undefined") {
        var Bitrix = {};
    }

    Bitrix.FileTransferStatus = function Bitrix$FileTransferStatus() {
        this._intervalId = null;
        this._isConstructed = false;
        this._containerId = "FileTransferStatusContainer";
        this._table = this._title = this._content = this._icon = null;
    }
    Bitrix.FileTransferStatus.prototype = {
        initialize: function () {
        },
        construct: function() {
            if(this._isConstructed) {
                return;
            }

            var containerDiv = document.getElementById(this._containerId);
            var tab = this._table = document.createElement("TABLE");

		    tab.className = "message";
		    tab.cellSpacing = "0";
		    tab.cellPadding = "0";
            tab.border = "0";
		    containerDiv.appendChild(tab);	

            var c = tab.insertRow(-1).insertCell(-1);

            var innerTab = document.createElement("TABLE");
		    innerTab.className = "content";
		    innerTab.cellSpacing = "0";
		    innerTab.cellPadding = "0";
            innerTab.border = "0";
		    c.appendChild(innerTab);	

            var r = innerTab.insertRow(-1);

            var c = r.insertCell(-1);
            c.valign = "top";
            var icon = this._icon = document.createElement("DIV");
            c.appendChild(icon);

            c = r.insertCell(-1);
            var ttl = this._title = document.createElement("SPAN");
            ttl.className = "message-title"; 
            c.appendChild(ttl);

            var del = document.createElement("DIV");
            del.className = "empty";
            del.style.height = "0.5em";
            c.appendChild(del);

            var cnt = this._content = document.createElement("DIV");
            c.appendChild(cnt);

            containerDiv.style.display = "";

            this._isConstructed = true;
        },
        start: function() {
            window.setTimeout(Bitrix.TypeUtility.createDelegate(this, this.callback), 0);

        },
        stop: function() {
            if(this._intervalId) {
                window.clearInterval(this._intervalId);
            }
        },
        callback: function () {
            var arg = "";
            <%= Page.ClientScript.GetCallbackEventReference(this, "arg", "Bitrix.TypeUtility.createDelegate(this, this.receiveServerData)", "") %>
        },
        receiveServerData: function(arg, context) {
            if(!Bitrix.TypeUtility.isNotEmptyString(arg)){

                if(this._intervalId){
                    window.clearInterval(this._intervalId);
                }
                return;
            }

            var data = eval("(" + arg + ")");

            this.construct();

            if(!data.isCompleted) {
                this._title.innerHTML = data.mode == "Loading" ? "<%= GetMessageRaw("SynchronizerStatus.Title.Loading") %>" : "<%= GetMessageRaw("SynchronizerStatus.Title.Unloading") %>";        
            }
            else {
                this._title.innerHTML = data.mode == "Loading" ? "<%= GetMessageRaw("SynchronizerStatus.Title.LoadingCompleted") %>" : "<%= GetMessageRaw("SynchronizerStatus.Title.UnloadingCompleted") %>";        

                if(this._intervalId){
                    window.clearInterval(this._intervalId);
                }
            }

            if(Bitrix.TypeUtility.isNotEmptyString(data.error)) {
                this._table.className = "message message-error";
                this._content.innerHTML = data.error;
                this._icon.className = "icon-error";
            }
            else {
                this._table.className = "message message-ok";
                this._content.innerHTML = "<%= GetMessageRaw("SynchronizerStatus.Label.FilesProcessed") %>: " + data.filesProcessed.toString() + "<br/>" + "<%= GetMessageRaw("SynchronizerStatus.Label.FilesTransfered") %>: " + data.filesTransfered.toString();
                this._icon.className = "icon-ok";
            }

            if(!data.isCompleted && !this._intervalId)
            {
                this._intervalId = window.setInterval(function(){ Bitrix.FileTransferStatus.getInstance().callback() }, 3000);
            }
        }
    }

    Bitrix.FileTransferStatus._instance = null;
    Bitrix.FileTransferStatus.getInstance = function () {
        if (!this._instance) {
            this._instance = new Bitrix.FileTransferStatus();
            this._instance.initialize();
        }
        return this._instance;
    }

</script>
</asp:Content>