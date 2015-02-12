<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="false"
    CodeFile="PromoRuleList.aspx.cs" Inherits="bitrix_admin_PromoRuleList" Title="<%$ LocRaw:PageTitle %>" %>
    
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:UpdatePanel ID="UpdatePanel" runat="server">
        <ContentTemplate>
            <bx:BXAdminFilter ID="ItemFilter" runat="server" AssociatedGridView="ItemGrid">
                <bx:BXTextBoxStringFilter Key="Name" Text="<%$ LocRaw:FilterText.Name %>" Visibility="AlwaysVisible" />
                <bx:BXDropDownFilter ID="ActivityFilter" Key="Active" Text="<%$ LocRaw:FilterText.Active %>">
                    <asp:ListItem Text="<%$ LocRaw:Kernel.All %>" Value="" />
                    <asp:ListItem Text="<%$ LocRaw:Option.Active %>" Value="true" />
                    <asp:ListItem Text="<%$ LocRaw:Option.Inactive %>" Value="false" />
                </bx:BXDropDownFilter>
                <bx:BXTextBoxFilter Key="ID" Text="ID" ValueType="Integer" />
                <bx:BXTextBoxStringFilter Key="BoundEntityTypeId" Text="<%$ LocRaw:FilterText.BoundEntityTypeId %>" />
            </bx:BXAdminFilter>
            <bx:BXContextMenuToolbar ID="ItemListToolbar" runat="server">
                <Items>
                    <bx:BXCmSeparator SectionSeparator="true" />
                    <bx:BXCmImageButton ID="AddButton" Text="<%$ LocRaw:ActionText.Add %>" Title="<%$ LocRaw:ActionTitle.Add %>"
                        CssClass="context-button icon btn_new" Href="PromoRuleEdit.aspx" />
                </Items>
            </bx:BXContextMenuToolbar>
            <bx:BXPopupPanel ID="PopupPanelView" runat="server">
                <Commands>
                    <bx:CommandItem UserCommandId="edit" Default="True" IconClass="edit" ItemText="<%$ LocRaw:PopupText.Edit %>"
                        ItemTitle="<%$ LocRaw:PopupTitle.Edit %>" OnClickScript="window.location.href = 'PromoRuleEdit.aspx?id=' + UserData['ID']; return false;" />
                    <bx:CommandItem UserCommandId="recalculate" Default="False" IconClass="edit" ItemText="<%$ LocRaw:PopupText.Apply %>"
                        ItemTitle="<%$ LocRaw:PopupTitle.Apply %>" OnClickScript="window.location.href = 'PromoRuleList.aspx?action=apply&id=' + UserData['ID']; return false;" />                        
                    <bx:CommandItem UserCommandId="delete" Default="False" IconClass="delete" ItemText="<%$ LocRaw:PopupText.Delete %>"
                        ItemTitle="<%$ LocRaw:PopupTitle.Delete %>" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
                </Commands>
            </bx:BXPopupPanel>  
            <bx:BXValidationSummary ID="ErrorMessage" runat="server" CssClass="errorSummary"
                HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="GridView" />
            <br />
            <bx:BXGridView ID="ItemGrid" runat="server" ContentName="<%$ LocRaw:TableTitle %>"
                AllowSorting="True" AllowPaging="True" DataKeyNames="ID" SettingsToolbarId="ItemListToolbar"
                PopupCommandMenuId="PopupPanelView" ContextMenuToolbarId="MultiActionMenuToolbar"
                DataSourceID="ItemGrid" OnSelect="ItemGrid_Select" OnSelectCount="ItemGrid_SelectCount"
                OnDelete="ItemGrid_Delete" OnRowDataBound="ItemGrid_RowDataBound">
                <Columns> 
                    <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" ReadOnly="True" />
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Name %>" SortExpression="Name">
                        <ItemTemplate>
                            <%# HttpUtility.HtmlEncode(((PromoRuleWrapper)Container.DataItem).Name) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Created %>" SortExpression="Created">
                        <ItemTemplate>
                            <%# HttpUtility.HtmlEncode(((PromoRuleWrapper)Container.DataItem).Created) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.LastModified %>" SortExpression="LastModified">
                        <ItemTemplate>
                            <%# HttpUtility.HtmlEncode(((PromoRuleWrapper)Container.DataItem).LastModified) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.LastApplied %>" SortExpression="LastApplied">
                        <ItemTemplate>
                            <%# HttpUtility.HtmlEncode(((PromoRuleWrapper)Container.DataItem).LastApplied) %>
                        </ItemTemplate>
                    </asp:TemplateField>                    
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Active %>" SortExpression="Active">
                        <ItemTemplate>
                            <%# HttpUtility.HtmlEncode(((PromoRuleWrapper)Container.DataItem).Active) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Status %>" SortExpression="Status">
                        <ItemTemplate>
                            <%# HttpUtility.HtmlEncode(((PromoRuleWrapper)Container.DataItem).Status) %>
                        </ItemTemplate>
                    </asp:TemplateField>                     
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.BoundEntityTypeId %>" SortExpression="BoundEntityTypeId">
                        <ItemTemplate>
                            <%# HttpUtility.HtmlEncode(((PromoRuleWrapper)Container.DataItem).BoundEntityTypeId) %>
                        </ItemTemplate>
                    </asp:TemplateField>                                                                                                 
                </Columns>
                <AjaxConfiguration UpdatePanelId="UpdatePanel" />
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
</asp:Content>    