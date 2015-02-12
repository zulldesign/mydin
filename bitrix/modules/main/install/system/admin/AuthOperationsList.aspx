<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="AuthOperationsList.aspx.cs" Inherits="bitrix_admin_AuthOperationsList" Title="<%$ Loc:PageTitle %>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<asp:UpdatePanel ID="UpdatePanel1" runat="server">
		<ContentTemplate>
			<bx:BXAdminFilter ID="BXAdminFilter1" runat="server" AssociatedGridView="AuthOperationsGridView">
				<bx:BXTextBoxStringFilter ID="BXTextBoxStringFilter1" runat="server" Key="OperationName" Text="<%$ Loc:FilterText.Code %>" Visibility="AlwaysVisible" />
				<bx:BXDropDownFilter ID="BXDropDownFilter1" runat="server" Key="ModuleId" Text="<%$ Loc:FilterText.Module %>">
				</bx:BXDropDownFilter>
			</bx:BXAdminFilter>
			<bx:BXContextMenuToolbar id="OperationsContextMenuToolbar" runat="server">
				<Items>
					<bx:BXCmImageButton runat="server" CssClass="context-button icon btn_new" CommandName="add" ID="AddButton"
						Text="<%$ Loc:ActionText.CreateOperation %>" Href="AuthOperationsEdit.aspx" />
				</Items>
			</bx:BXContextMenuToolbar>
			<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" />
			<bx:BXMessage ID="successMessage" runat="server" Content="<%$ Loc:Message.RecordsHasBeenSuccessfullyDeleted %>"
				CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False" />

			<bx:BXPopupPanel ID="OperationsPopupPanel" runat="server">
				<Commands>
					<bx:CommandItem Default="True" IconClass="edit" ItemText="<%$ Loc:Kernel.Edit %>" ItemTitle="<%$ Loc:PopupTitle.ModifyOperation %>"
						OnClickScript="" UserCommandId="edit" />
					<bx:CommandItem IconClass="" ItemCommandType="Separator" ItemText="" ItemTitle=""
						OnClickScript="" UserCommandId="" />
					<bx:CommandItem IconClass="delete" ItemText="<%$ Loc:Kernel.Delete %>" ItemTitle="<%$ Loc:PopupTitle.DeleteOperation %>"
						OnClickScript="" UserCommandId="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
				</Commands>
			</bx:BXPopupPanel>
			<bx:BXPopupPanel ID="PopupPanelView" runat="server">
				<Commands>
					<bx:CommandItem Default="True" IconClass="view" ItemText="<%$ Loc:Kernel.View %>" ItemTitle="<%$ LocRaw:PopupTitle.View %>"
						UserCommandId="edit" />
				</Commands>
			</bx:BXPopupPanel>

			<bx:bxgridview id="AuthOperationsGridView" runat="server"
				ContextMenuToolbarId="OperationsMultiActionMenuToolbar"
				PopupCommandMenuId="OperationsPopupPanel" contentname="" AllowSorting="True"
				datasourceid="AuthOperationsGridView"
				allowpaging="True" AjaxConfiguration-UpdatePanelId="UpdatePanel1" 
				SettingsToolbarId="OperationsContextMenuToolbar" OnSelect="AuthOperationsGridView_Select"
				OnSelectCount="AuthOperationsGridView_SelectCount"
				OnPopupMenuClick="AuthOperationsGridView_PopupMenuClick" OnDelete="AuthOperationsGridView_Delete"
				DataKeyNames="OperationId" 
				
                ForeColor="#333333" BorderWidth="0px" BorderColor="white" BorderStyle="none"
                ShowHeader = "true" CssClass="list"
                style="font-size: small; font-family: Arial; border-collapse: separate;" 
                >
				<Columns>
					<asp:BoundField DataField="OperationId" SortExpression="ID" HeaderText="ID" />
					<asp:BoundField DataField="OperationName" SortExpression="OperationName" HeaderText="<%$ Loc:FilterText.Caption %>" />
					<asp:BoundField DataField="OperationType" SortExpression="OperationType" HeaderText="<%$ Loc:ColumnHeaderText.OperationType %>" />
					<asp:BoundField DataField="ModuleName" SortExpression="ModuleID" HeaderText="<%$ Loc:FilterText.Module %>" />
					<asp:BoundField DataField="Comment" HeaderText="<%$ Loc:ColumnHeaderText.Comment %>" />
				</Columns>
				<AjaxConfiguration UpdatePanelId="UpdatePanel1" />
				<pagersettings position="TopAndBottom" pagebuttoncount="3"/>
                <RowStyle BackColor="#FFFFFF"/>
                <AlternatingRowStyle BackColor="#FAFAFA" /> 
                <FooterStyle BackColor="#EAEDF7"/>
				
			</bx:bxgridview>
			<bx:BXMultiActionMenuToolbar ID="OperationsMultiActionMenuToolbar" runat="server" ToolBarType="View">
				<Items>
					<bx:BXMamImageButton runat="server" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>" EnabledCssClass="context-button icon delete"
						DisabledCssClass="context-button icon delete-dis" CommandName="delete"
					/>
				</Items>
			</bx:BXMultiActionMenuToolbar>
			<br />
		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>