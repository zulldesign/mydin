<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="false"
    CodeFile="BlogList.aspx.cs" Inherits="bitrix_admin_BlogList" Title="<%$ LocRaw:PageTitle %>" %>

<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.DataLayer" %>
<%@ Import Namespace="Bitrix.CommunicationUtility" %>

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
                <bx:BXTextBoxFilter Key="Sort" Text="<%$ LocRaw:FilterText.Sort %>" ValueType="Integer" />
                <bx:BXTextBoxStringFilter Key="Slug" Text="<%$ LocRaw:FilterText.Slug %>" />
                <bx:BXTextBoxStringFilter Key="XmlId" Text="<%$ LocRaw:FilterText.XmlId %>" />
                <bx:BXDropDownFilter Key="IndexContent" Text="<%$ LocRaw:FilterText.Index %>" ValueType="Integer">
                    <asp:ListItem Text="<%$ LocRaw:Kernel.Any %>" Value="" />
                    <asp:ListItem Text="<%$ LocRaw:IndexContent.Nothing %>" Value="0" />
                    <asp:ListItem Text="<%$ LocRaw:IndexContent.All %>" Value="1" />
                    <asp:ListItem Text="<%$ LocRaw:IndexContent.TagsOnly %>" Value="2" />
                </bx:BXDropDownFilter>
                <bx:BXDropDownFilter Key="IsTeam" Text="<%$ LocRaw:FilterText.IsTeam %>">
                    <asp:ListItem Text="<%$ LocRaw:Kernel.Any %>" Value="" />
                    <asp:ListItem Text="<%$ LocRaw:Kernel.Yes %>" Value="true" />
                    <asp:ListItem Text="<%$ LocRaw:Kernel.No %>" Value="false" />
                </bx:BXDropDownFilter>
            </bx:BXAdminFilter>
            <bx:BXContextMenuToolbar ID="ItemListToolbar" runat="server">
                <Items>
                    <bx:BXCmSeparator SectionSeparator="true" />
                    <bx:BXCmImageButton ID="AddButton" Text="<%$ LocRaw:ActionText.Add %>" Title="<%$ LocRaw:ActionTitle.Add %>"
                        CssClass="context-button icon btn_new" Href="BlogEdit.aspx" />
                </Items>
            </bx:BXContextMenuToolbar>
            <bx:BXPopupPanel ID="PopupPanelView" runat="server">
                <Commands>
                    <bx:CommandItem UserCommandId="edit" Default="True" IconClass="edit" ItemText="<%$ LocRaw:PopupText.Edit %>"
                        ItemTitle="<%$ LocRaw:PopupTitle.Edit %>" OnClickScript="window.location.href = 'BlogEdit.aspx?id=' + UserData['ID']; return false;" />
                    <bx:CommandItem UserCommandId="delete" Default="True" IconClass="delete" ItemText="<%$ LocRaw:PopupText.Delete %>"
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
                            <%# BXWordBreakingProcessor.Break(((BlogWrapper)Container.DataItem).Name, 30, true) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.DateCreated %>" SortExpression="DateCreated">
                        <ItemTemplate>
                            <%# BXWordBreakingProcessor.Break(((BlogWrapper)Container.DataItem).DateCreated, 30, true) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.DateLastPosted %>" SortExpression="DateLastPosted">
                        <ItemTemplate>
                            <%# BXWordBreakingProcessor.Break(((BlogWrapper)Container.DataItem).DateLastPosted, 30, true) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Active %>" SortExpression="Active">
                        <ItemTemplate>
                            <%# BXWordBreakingProcessor.Break(((BlogWrapper)Container.DataItem).Active, 30, true) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Owner %>">
                        <ItemTemplate>
                            <%# BXWordBreakingProcessor.Break(((BlogWrapper)Container.DataItem).Owner, 30, true) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Slug %>" SortExpression="Slug">
                        <ItemTemplate>
                            <%# BXWordBreakingProcessor.Break(((BlogWrapper)Container.DataItem).Slug, 30, true) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Categories %>">
                        <ItemTemplate>
                            <%# BXWordBreakingProcessor.Break(((BlogWrapper)Container.DataItem).Categories, 30, true)%>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField HeaderText="<%$ LocRaw:ColumnHeaderText.Index %>" DataField="IndexContent" SortExpression="IndexContent" ReadOnly="true" />
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.IsTeam %>" SortExpression="IndexContent">
                        <ItemTemplate>
                            <%# ((BlogWrapper)Container.DataItem).Charge.IsTeam ? GetMessage("Kernel.Yes") : GetMessage("Kernel.No") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <AjaxConfiguration UpdatePanelId="UpdatePanel" />
            </bx:BXGridView>
            <bx:BXMultiActionMenuToolbar ID="MultiActionMenuToolbar" runat="server" ValidationGroup="GridView">
                <Items>
                    <%--					
                    <bx:BXMamImageButton CommandName="inline" ShowConfirmBar="true" DisableForAll="true"
						EnabledCssClass="context-button icon edit" DisabledCssClass="context-button icon edit-dis"
						Title="Редактировать выбранные блоги" />
				    --%>
                    <bx:BXMamImageButton CommandName="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>"
                        ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>" EnabledCssClass="context-button icon delete"
                        DisabledCssClass="context-button icon delete-dis" Title="<%$ LocRaw:ActionTitle.Delete %>" />
                </Items>
            </bx:BXMultiActionMenuToolbar>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
