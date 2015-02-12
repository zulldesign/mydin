<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="IBlockTypeList.aspx.cs" Inherits="bitrix_admin_IBlockTypeList" Title="<%$ Loc:PageTitle.InformationalBlockTypes %>" %>
<%@ Import Namespace="Bitrix.IO" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<bx:InlineScript ID="Script" runat="server">
	<script type="text/javascript">
		function GoToIBlockList(id)
		{
			window.location.href = '<%= JSEncode(BXPath.ToVirtualAbsolutePath("~/bitrix/admin/IBlockAdmin.aspx") + "?type_id=") %>' + id;
			return false;
		}
	</script>
	</bx:InlineScript>
	<asp:UpdatePanel ID="UpdatePanel1" runat="server">
		<ContentTemplate>
			<bx:BXAdminFilter ID="BXAdminFilter1" runat="server" AssociatedGridView="GridView1">
				<bx:BXBetweenFilter ID="BXBetweenFilter1" runat="server" Key="ID" Text="ID" Visibility="AlwaysVisible" ValueType="Integer" />
				<bx:BXDropDownFilter ID="BXDropDownFilter2" runat="server" Key="HaveSections" Text="<%$ Loc:FilterText.HasSections %>">
					<asp:ListItem Value="" Text="<%$ Loc:Any %>"></asp:ListItem>
					<asp:ListItem Value="Y" Text="<%$ Loc:Kernel.Yes %>"></asp:ListItem>
					<asp:ListItem Value="N" Text="<%$ Loc:Kernel.No %>"></asp:ListItem>
				</bx:BXDropDownFilter>
			</bx:BXAdminFilter>
			<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" runat="server">
				<Items>
					<bx:BXCmSeparator SectionSeparator="True"/>
					<bx:BXCmImageButton ID="AddButton" CssClass="context-button icon btn_new"
						CommandName="addType" Text="<%$ Loc:ActionText.AddNewType %>" Href="IBlockTypeEdit.aspx" />
				</Items>
			</bx:BXContextMenuToolbar>
			<br />
			<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" />
			<bx:BXMessage ID="successMessage" runat="server" Content=""
				CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False" />
			<bx:BXPopupPanel ID="PopupPanel1" runat="server">
				<Commands>
					<bx:CommandItem IconClass="" ItemText="<%$ Loc:PopupText.InformationalBlocks %>" ItemTitle="<%$ Loc:PopupText.InformationalBlocks %>"
						OnClickScript="return GoToIBlockList(UserData.Id);" UserCommandId="iblocks" />
					<bx:CommandItem IconClass="" ItemCommandType="Separator" ItemText="" ItemTitle=""
						OnClickScript="" UserCommandId="" />
					<bx:CommandItem Default="True" IconClass="edit" ItemText="<%$ Loc:PopupText.ChangeType %>" ItemTitle="<%$ Loc:PopupTitle.ChangeType %>"
						OnClickScript="" UserCommandId="edit" />
					<bx:CommandItem IconClass="delete" ItemText="<%$ Loc:Kernel.Delete %>" ItemTitle="<%$ Loc:PopupTitle.DeleteType %>" OnClickScript=""
						UserCommandId="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
				</Commands>
			</bx:BXPopupPanel>
			<bx:BXPopupPanel ID="PopupPanelView" runat="server">
				<Commands>
					<bx:CommandItem IconClass="" ItemText="<%$ Loc:PopupText.InformationalBlocks %>" ItemTitle="<%$ Loc:PopupText.InformationalBlocks %>"
						OnClickScript="return GoToIBlockList(UserData.Id);" UserCommandId="iblocks" />
					<bx:CommandItem IconClass="" ItemCommandType="Separator" ItemText="" ItemTitle=""
						OnClickScript="" UserCommandId="" />
					<bx:CommandItem Default="True" IconClass="view" ItemText="<%$ Loc:Kernel.View %>" ItemTitle="<%$ LocRaw:PopupTitle.View %>"
						UserCommandId="edit" />
				</Commands>
			</bx:BXPopupPanel>
				
			<bx:BXGridView ID="GridView1" runat="server" 
				ContextMenuToolbarId="MultiActionMenuToolbar1"
				PopupCommandMenuId="PopupPanel1" ContentName="" DataSourceID="GridView1" 
				AjaxConfiguration-UpdatePanelId="UpdatePanel1" 
				SettingsToolbarId="BXContextMenuToolbar1"
				AllowSorting="True"
				DataKeyNames="TypeId" 
				OnSelect="GridView1_Select" OnPopupMenuClick="GridView1_PopupMenuClick"
				OnRowDataBound="GridView1_RowDataBound"
				OnDelete="GridView1_Delete"
			>
				<AjaxConfiguration UpdatePanelId="UpdatePanel1" />
				<Columns>
					<asp:BoundField AccessibleHeaderText="ID" DataField="TypeId" HeaderText="ID" SortExpression="ID" />
					<asp:BoundField DataField="Name" HeaderText="<%$ Loc:ColumnHeaderText.Title %>" />
					<asp:BoundField DataField="Sort" HeaderText="<%$ Loc:ColumnHeaderText.Sort %>" SortExpression="Sort" />
					<asp:BoundField DataField="HaveSections" HeaderText="<%$ Loc:ColumnHeaderText.HasSections %>" SortExpression="HaveSections" />
				</Columns>
			</bx:BXGridView>
			<br />
			<bx:BXMultiActionMenuToolbar ID="MultiActionMenuToolbar1" runat="server">
				<Items>
					<bx:BXMamImageButton ID="BXMamImageButton1" runat="server" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>"
						EnabledCssClass="context-button icon delete" DisabledCssClass="context-button icon delete-dis"
						CommandName="delete" />
				</Items>
			</bx:BXMultiActionMenuToolbar>
		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>
