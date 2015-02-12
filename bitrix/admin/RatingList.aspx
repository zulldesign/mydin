<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="false"
    CodeFile="RatingList.aspx.cs" Inherits="bitrix_admin_RatingList" Title="<%$ LocRaw:PageTitle %>" %>
    
<%@ Import Namespace="Bitrix.Services.Text" %>
  
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
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
                <%--<bx:BXTextBoxStringFilter Key="XmlId" Text="<%$ LocRaw:FilterText.XmlId %>" />--%>
            </bx:BXAdminFilter>
            <bx:BXContextMenuToolbar ID="ItemListToolbar" runat="server">
                <Items>
                    <bx:BXCmSeparator SectionSeparator="true" />
                    <bx:BXCmImageButton ID="AddButton" Text="<%$ LocRaw:ActionText.Add %>" Title="<%$ LocRaw:ActionTitle.Add %>"
                        CssClass="context-button icon btn_new" Href="RatingEdit.aspx" />
                </Items>
            </bx:BXContextMenuToolbar>
            <bx:BXPopupPanel ID="PopupPanelView" runat="server">
                <Commands>
                    <bx:CommandItem UserCommandId="edit" Default="True" IconClass="edit" ItemText="<%$ LocRaw:PopupText.Edit %>"
                        ItemTitle="<%$ LocRaw:PopupTitle.Edit %>" OnClickScript="window.location.href = 'RatingEdit.aspx?id=' + UserData['ID']; return false;" />
                    <bx:CommandItem UserCommandId="recalculate" Default="False" IconClass="edit" ItemText="<%$ LocRaw:PopupText.Recalculate %>"
                        ItemTitle="<%$ LocRaw:PopupTitle.Recalculate %>" />                        
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
                            <%# BXStringUtility.Break(((RatingWrapper)Container.DataItem).Name, 30, true)%>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Created %>" SortExpression="Created">
                        <ItemTemplate>
                            <%# BXStringUtility.Break(((RatingWrapper)Container.DataItem).Created, 30, true)%>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.LastModified %>" SortExpression="LastModified">
                        <ItemTemplate>
                            <%# BXStringUtility.Break(((RatingWrapper)Container.DataItem).LastModified, 30, true)%>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.LastCalculated %>" SortExpression="LastCalculated">
                        <ItemTemplate>
                            <%# BXStringUtility.Break(((RatingWrapper)Container.DataItem).LastCalculated, 30, true)%>
                        </ItemTemplate>
                    </asp:TemplateField>                    
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Active %>" SortExpression="Active">
                        <ItemTemplate>
                            <%# BXStringUtility.Break(((RatingWrapper)Container.DataItem).Active, 30, true)%>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Status %>" SortExpression="Status">
                        <ItemTemplate>
                            <%# BXStringUtility.Break(((RatingWrapper)Container.DataItem).Status, 30, true)%>
                        </ItemTemplate>
                    </asp:TemplateField>                     
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.BoundEntityTypeId %>" SortExpression="BoundEntityTypeId">
                        <ItemTemplate>
                            <%# BXStringUtility.Break(((RatingWrapper)Container.DataItem).BoundEntityTypeId, 30, true)%>
                        </ItemTemplate>
                    </asp:TemplateField>                 
                    <%--<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.CalculationMethod %>" SortExpression="CalculationMethod">
                        <ItemTemplate>
                            <%# BXWordBreakingProcessor.Break(((RatingWrapper)Container.DataItem).CalculationMethod, 30, true)%>
                        </ItemTemplate>
                    </asp:TemplateField>--%>
                    <%--<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.RefreshMethod %>" SortExpression="RefreshMethod">
                        <ItemTemplate>
                            <%# BXWordBreakingProcessor.Break(((RatingWrapper)Container.DataItem).RefreshMethod, 30, true)%>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.XmlId %>" SortExpression="XmlId">
                        <ItemTemplate>
                            <%# BXWordBreakingProcessor.Break(((RatingWrapper)Container.DataItem).XmlId, 30, true)%>
                        </ItemTemplate>
                    </asp:TemplateField>--%>                                                                                 
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