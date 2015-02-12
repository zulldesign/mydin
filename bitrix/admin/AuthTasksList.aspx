<%--<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="AuthTasksList.aspx.cs" Inherits="bitrix_admin_AuthTasksList" Title="Список задач" Theme="AdminTheme" StylesheetTheme="AdminTheme"%>
zg --%>

<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="AuthTasksList.aspx.cs" Inherits="bitrix_admin_AuthTasksList" Title="<%$ Loc:PageTitle.TaskList %>"%>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<asp:UpdatePanel ID="UpdatePanel1" runat="server">
		<ContentTemplate>
			<bx:BXAdminFilter ID="BXAdminFilter1" runat="server" AssociatedGridView="TasksGridView">
				<bx:BXTextBoxStringFilter ID="BXTextBoxStringFilter1" runat="server" Key="TaskName" Text="<%$ Loc:FilterText.Cation %>" Visibility="AlwaysVisible"/>
			</bx:BXAdminFilter>
			
			<bx:BXContextMenuToolbar id="TasksContextMenuToolbar" runat="server">
				<Items>
					<bx:BXCmImageButton ID="AddButton" CssClass="context-button icon btn_new" CommandName="add"
						Text="<%$ Loc:ActionText.AddTask %>" Href="AuthTasksEdit.aspx" />
				</Items>
			</bx:BXContextMenuToolbar>
			<br />
			<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" />
			<bx:BXMessage ID="successMessage" runat="server" Content="<%$ Loc:Message.RecordsHaveBeenDeletedSuccessfully %>"
				CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False" />
			<bx:BXPopupPanel ID="TasksPopupPanel" runat="server">
				<Commands>
					<bx:CommandItem Default="True" IconClass="edit" ItemText="<%$ Loc:Kernel.Edit %>" ItemTitle="<%$ Loc:PopupTitle.ModifyTask %>"
						OnClickScript="" UserCommandId="edit" />
					<bx:CommandItem IconClass="" ItemCommandType="Separator" ItemText="" ItemTitle=""
						OnClickScript="" UserCommandId="" />
					<bx:CommandItem IconClass="delete" ItemText="<%$ Loc:Kernel.Delete %>" ItemTitle="<%$ Loc:PopupTitle.DeleteTask %>"
						OnClickScript="" UserCommandId="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
				</Commands>
			</bx:BXPopupPanel>
			<bx:BXPopupPanel ID="PopupPanelView" runat="server">
				<Commands>
					<bx:CommandItem Default="True" IconClass="view" ItemText="<%$ Loc:Kernel.View %>" ItemTitle="<%$ LocRaw:PopupTitle.View %>"
						UserCommandId="edit" />
				</Commands>
			</bx:BXPopupPanel>

			<bx:BXGridView id="TasksGridView" runat="server"
				ContextMenuToolbarId="TasksMultiActionMenuToolbar"
				PopupCommandMenuId="TasksPopupPanel" contentname="" AllowSorting="True"
				datasourceid="TasksGridView"
				allowpaging="True" AjaxConfiguration-UpdatePanelId="UpdatePanel1"
				SettingsToolbarId="TasksContextMenuToolbar" OnSelect="TasksGridView_Select"
				OnSelectCount="TasksGridView_SelectCount"
				OnDelete="TasksGridView_Delete"
				OnPopupMenuClick="AuthTasksGridView_PopupMenuClick" DataKeyNames="TaskId" >
				<Columns>
					<asp:BoundField DataField="TaskId" SortExpression="ID" HeaderText="ID" />
					<asp:BoundField DataField="TaskName" SortExpression="TaskName" HeaderText="<%$ Loc:FilterText.Cation %>" />
					<asp:BoundField DataField="Comment" HeaderText="<%$ Loc:ColumnHeaderText.Comment %>" />
					<asp:BoundField DataField="TasksCount" SortExpression="TasksCount" HeaderText="<%$ Loc:ColumnHeaderText.QuantityOfSubtasks %>" />
					<asp:BoundField DataField="OperationsCount" SortExpression="OperationsCount" HeaderText="<%$ Loc:ColumnHeaderText.QuantityOfOperations %>" />
				</Columns>
				<AjaxConfiguration UpdatePanelId="UpdatePanel1" />
			</bx:BXGridView>

			<bx:BXMultiActionMenuToolbar ID="TasksMultiActionMenuToolbar" runat="server" >
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
