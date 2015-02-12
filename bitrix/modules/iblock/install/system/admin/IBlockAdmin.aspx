<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="IBlockAdmin.aspx.cs" Inherits="bitrix_admin_IBlockAdmin" Title="<%$ Loc:PageTitle.InformationBlocks %>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<asp:UpdatePanel ID="UpdatePanel1" runat="server">
		<ContentTemplate>
			<bx:BXAdminFilter ID="BXAdminFilter1" runat="server" AssociatedGridView="GridView1">
				<bx:BXTextBoxStringFilter runat="server" Key="Name" Text="<%$ Loc:FilterText.Title %>" Visibility="AlwaysVisible" />
				<bx:BXDropDownFilter ID="filterSite" runat="server" Key="Site.ID" Text="<%$ Loc:FilterText.Sites %>" />
				<bx:BXDropDownFilter runat="server" Key="Active" Text="<%$ Loc:FilterText.IsActive %>">
					<asp:ListItem Value="" Text="<%$ Loc:Kernel.Any %>"></asp:ListItem>
					<asp:ListItem Value="Y" Text="<%$ Loc:Kernel.Yes %>"></asp:ListItem>
					<asp:ListItem Value="N" Text="<%$ Loc:Kernel.No %>"></asp:ListItem>
				</bx:BXDropDownFilter>
				<bx:BXTextBoxFilter runat="server" Key="ID" Text="ID" ValueType="Integer" />
				<bx:BXTextBoxStringFilter runat="server" Key="Code" Text="<%$ Loc:FilterText.MnemonicCode %>" />
			</bx:BXAdminFilter>
			<% AddButton.Href = "IBlockEdit.aspx?type_id=" + typeId; %>
			<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" runat="server">
				<Items>
					<bx:BXCmImageButton ID="AddButton" runat="server" CssClass="context-button icon btn_new" CommandName="add"
						Text="<%$ Loc:ActionText.AddBlock %>" />
				</Items>
			</bx:BXContextMenuToolbar>

			<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" />
			<bx:BXMessage ID="successMessage" runat="server" Content="<%$ Loc:Message.RecordsHaveBeenDeletedSuccessfully %>"
				CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False" />

			<asp:HiddenField ID="hfTypeId" runat="server" />

			<bx:BXPopupPanel ID="PopupPanel1" runat="server">
				<Commands>
					<bx:CommandItem IconClass="" ItemText="<%$ Loc:PopupText.Chapters %>" ItemTitle=""
						OnClickScript="window.location.href = UserData.SectionPage + '?iblock_id=' + UserData.IBlockId + '&type_id=' + UserData.TypeId; return false;" 
						UserCommandId="sections" />
					<bx:CommandItem IconClass="" ItemText="<%$ Loc:PopupText.Articles %>" ItemTitle=""
						OnClickScript="window.location.href = UserData.ElementPage + '?iblock_id=' + UserData.IBlockId + '&type_id=' + UserData.TypeId; return false;"
						UserCommandId="elements" />
					<bx:CommandItem Default="True" IconClass="edit" ItemText="<%$ Loc:PopupText.ChangeBlock %>" ItemTitle="<%$ Loc:PopupTitle.ChangeBlock %>"
						OnClickScript="window.location.href = 'IBlockEdit.aspx?id=' + UserData.IBlockId + '&type_id=' + UserData.TypeId; return false;" UserCommandId="edit" />
					<bx:CommandItem Default="True" IconClass="view" ItemText="<%$ Loc:Kernel.View %>" ItemTitle="<%$ LocRaw:PopupTitle.View %>"
						OnClickScript="window.location.href = 'IBlockEdit.aspx?id=' + UserData.IBlockId + '&type_id=' + UserData.TypeId;  return false;" UserCommandId="view" />
					<bx:CommandItem IconClass="delete" ItemText="<%$ Loc:Kernel.Delete %>" ItemTitle="<%$ Loc:PopupTitle.DeleteBlock %>"
						OnClickScript="" UserCommandId="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
				</Commands>
			</bx:BXPopupPanel>
			
			<bx:BXGridView ID="GridView1" runat="server"
				ContextMenuToolbarId="MultiActionMenuToolbar1" 
				PopupCommandMenuId="PopupPanel1" ContentName="" DataSourceID="GridView1" 
				AllowPaging="True" 
				AjaxConfiguration-UpdatePanelId="UpdatePanel1" 
				SettingsToolbarId="BXContextMenuToolbar1" DataKeyNames="IBlockId" 
				OnSelect="GridView1_Select"
				AllowSorting="True" OnSelectCount="GridView1_SelectCount" OnDelete="GridView1_Delete"
				OnRowDataBound="GridView1_RowDataBound"
				ForeColor="#333333"
                BorderWidth="0px"
                BorderColor="white"
                BorderStyle="none"
                ShowHeader = "true"
                CssClass="list"
                style="font-size: small; font-family: Arial; border-collapse: separate;"   
				>
				<AjaxConfiguration UpdatePanelId="UpdatePanel1" />
				
                <pagersettings position="TopAndBottom" pagebuttoncount="3"/>
                <RowStyle BackColor="#FFFFFF"/>
                <AlternatingRowStyle BackColor="#FAFAFA" />
                <FooterStyle BackColor="#EAEDF7"/>
				<Columns>
					<asp:BoundField AccessibleHeaderText="ID" DataField="IBlockId" HeaderText="ID" SortExpression="ID" />
					<asp:BoundField DataField="Name" HeaderText="<%$ Loc:FilterText.Title %>" SortExpression="Name" />
					<asp:BoundField DataField="Sort" HeaderText="<%$ Loc:ColumnHeaderText.Sort %>" SortExpression="Sort"/>
					<asp:BoundField DataField="Active" HeaderText="<%$ Loc:ColumnHeaderText.IsActive %>" SortExpression="Active"/>
					<asp:BoundField DataField="Code" HeaderText="<%$ Loc:ColumnHeaderText.Code %>" SortExpression="Code"/>
					<asp:BoundField DataField="ElementsCount" HeaderText="<%$ Loc:ColumnHeaderText.QuantityOfElements %>" />
					<asp:BoundField DataField="SectionsCount" HeaderText="<%$ Loc:ColumnHeaderText.QuantityOfChapters %>" />
					<asp:BoundField DataField="Sites" HeaderText="<%$ Loc:ColumnHeaderText.Site %>" />
					<asp:BoundField DataField="UpdateDate" HeaderText="<%$ Loc:ColumnHeaderText.DateOfModification %>" SortExpression="UpdateDate"/>
					<asp:BoundField DataField="IndexContent" HeaderText="<%$ Loc:ColumnHeaderText.ApplyIndexationToTheElements %>" SortExpression="IndexContent" Visible="False" />
				</Columns>
			</bx:BXGridView>
			&nbsp;

			<bx:BXMultiActionMenuToolbar ID="MultiActionMenuToolbar1" runat="server" >
				<Items>
					<bx:BXMamImageButton ID="BXMamImageButton1" runat="server" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>" EnabledCssClass="context-button icon delete"
						DisabledCssClass="context-button icon delete-dis" CommandName="delete"
					/>
				</Items>
			</bx:BXMultiActionMenuToolbar>

		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>