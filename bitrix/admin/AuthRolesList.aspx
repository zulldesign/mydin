<%--
<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="AuthRolesList.aspx.cs" Inherits="bitrix_admin_RolesList" Title="Список ролей" Theme="AdminTheme" StylesheetTheme="AdminTheme" %>
zg, 25.04.2008
--%>
<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="AuthRolesList.aspx.cs" Inherits="bitrix_admin_RolesList" Title="<%$ Loc:PageTitle %>"%>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<asp:UpdatePanel ID="UpdatePanel1" runat="server">
		<ContentTemplate>
			<bx:BXAdminFilter ID="BXAdminFilter1" runat="server" AssociatedGridView="AuthRolesGridView">
				<bx:BXTextBoxStringFilter ID="BXTextBoxStringFilter1" runat="server" Key="RoleName" Text="<%$ Loc:FilterText.Caption %>" Visibility="AlwaysVisible"/>
				<bx:BXDropDownFilter ID="BXDropDownFilter1" runat="server" Key="Active" Text="<%$ Loc:FilterText.Activity %>">
					<asp:ListItem Value="" Text="<%$ Loc:FilterValue.Any %>"></asp:ListItem>
					<asp:ListItem Value="True" Text="<%$ Loc:Kernel.Yes %>"></asp:ListItem>
					<asp:ListItem Value="False" Text="<%$ Loc:Kernel.No %>"></asp:ListItem>
				</bx:BXDropDownFilter>
			</bx:BXAdminFilter>

			<bx:BXContextMenuToolbar id="RolesContextMenuToolbar" runat="server">
				<Items>
					<bx:BXCmImageButton ID="AddButton" runat="server" CssClass="context-button icon btn_new" CommandName="add"
						Text="<%$ Loc:ActionText.AddRole %>" Href="AuthRolesEdit.aspx" />
					<bx:BXCmSeparator ID="BXCmSeparator1" runat="server" SectionSeparator="true" />
					<bx:BXCmImageButton ID="SynchronizeButton" runat="server" CommandName="sync"
						Text="<%$ Loc:ActionText.Synchronize %>" Href="AuthRolesSync.aspx" />
				</Items>
			</bx:BXContextMenuToolbar>

			<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" />
			<bx:BXMessage ID="successMessage" runat="server" Content="<%$ Loc:Message.RecordsHaveBeenDeletedSuccefully %>"
				CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False" />

			<bx:BXPopupPanel ID="RolesPopupPanel" runat="server">
				<Commands>
					<bx:CommandItem Default="True" IconClass="edit" ItemText="<%$ Loc:Kernel.Edit %>" ItemTitle="<%$ Loc:PopupTitle.ModifyRole %>"
						OnClickScript="" UserCommandId="edit" />
					<bx:CommandItem IconClass="" ItemCommandType="Separator" ItemText="" ItemTitle=""
						OnClickScript="" UserCommandId="" />
					<bx:CommandItem IconClass="delete" ItemText="<%$ Loc:Kernel.Delete %>" ItemTitle="<%$ Loc:PopupTitle.DeleteRole %>"
						OnClickScript="" UserCommandId="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
					<bx:CommandItem IconClass="delete" ItemText="<%$ Loc:PopupText.RemoveFromProvider %>" ItemTitle="<%$ Loc:PopupTitle.RemoveRoleFromProvider %>"
						OnClickScript="" UserCommandId="deleteProvider" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.DeleteFromProvider %>" />
				</Commands>
			</bx:BXPopupPanel>
			<bx:BXPopupPanel ID="PopupPanelView" runat="server">
				<Commands>
					<bx:CommandItem Default="True" IconClass="view" ItemText="<%$ Loc:Kernel.View %>" ItemTitle="<%$ LocRaw:PopupTitle.View %>"
						UserCommandId="edit" />
				</Commands>
			</bx:BXPopupPanel>

			<bx:BXGridView id="AuthRolesGridView" runat="server"
				ContextMenuToolbarId="RolesMultiActionMenuToolbar"
				PopupCommandMenuId="RolesPopupPanel" contentname="" AllowSorting="True"
				datasourceid="AuthRolesGridView"
				allowpaging="True" AjaxConfiguration-UpdatePanelId="UpdatePanel1"
				SettingsToolbarId="RolesContextMenuToolbar" OnSelect="AuthRolesGridView_Select"
				OnSelectCount="AuthRolesGridView_SelectCount"
				OnPopupMenuClick="AuthRolesGridView_PopupMenuClick" DataKeyNames="RoleId" 
				OnMultiOperationActionRun="AuthRolesGridView_MultiOperationActionRun" 
								
				ForeColor="#333333"
                BorderWidth="0px"
                BorderColor="white"
                BorderStyle="none"
                ShowHeader = "true"
                CssClass="list"
                style="font-size: small; font-family: Arial; border-collapse: separate;"    				
				>
				<Columns>
					<asp:BoundField DataField="RoleId" SortExpression="ID" HeaderText="ID" />
					<asp:BoundField DataField="RoleName" SortExpression="RoleName" HeaderText="<%$ Loc:FilterText.Caption %>" />
					<asp:BoundField DataField="Active" SortExpression="Active" HeaderText="<%$ Loc:FilterText.Activity %>" />
					<asp:BoundField DataField="Comment" HeaderText="<%$ Loc:ColumnHeaderText.Comment %>" Visible="False" />
					<asp:BoundField DataField="Policy" HeaderText="<%$ Loc:ColumnHeaderText.Policy %>" Visible="False" />
					<asp:BoundField DataField="EffectivePolicy" HeaderText="<%$ Loc:ColumnHeaderText.EffectivePolicy %>" Visible="False" />
					<asp:TemplateField SortExpression="UsersCount" HeaderText="<%$ Loc:ColumnHeaderText.QuantityOfUsers %>">
					<ItemTemplate >
						<a href="AuthUsersList.aspx?filter_roles=<%# Eval("RoleId") %>"><%# Eval("UsersCount") %></a> 
					</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField DataField="RolesCount" SortExpression="RolesCount" HeaderText="<%$ Loc:ColumnHeaderText.QuantityOfGroups %>" />
				</Columns>
				<AjaxConfiguration UpdatePanelId="UpdatePanel1" />
                <pagersettings position="TopAndBottom" pagebuttoncount="3"/>
                <RowStyle BackColor="#FFFFFF"/>
                <AlternatingRowStyle BackColor="#FAFAFA" /> 
                <FooterStyle BackColor="#EAEDF7"/>				
			</bx:BXGridView>

			<bx:BXMultiActionMenuToolbar ID="RolesMultiActionMenuToolbar" runat="server" ToolBarType="View">
				<Items>
					<bx:BXMamImageButton runat="server" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>" EnabledCssClass="context-button icon delete"
						DisabledCssClass="context-button icon delete-dis" CommandName="delete"
					/>
					<bx:BXMamListItem ID="BXMamListItem2" runat="server" CommandName="deleteProvider" Text="<%$ Loc:PopupText.RemoveFromProvider %>" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.DeleteProvider %>" ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.DeleteProvider %>" />
				</Items>
			</bx:BXMultiActionMenuToolbar>
			<br />
		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>

