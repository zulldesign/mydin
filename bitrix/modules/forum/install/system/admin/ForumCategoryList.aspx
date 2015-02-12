<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="ForumCategoryList.aspx.cs" Inherits="BXForumAdminPageCategoryList" Title="<%$ LocRaw:PageTitle %>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<asp:UpdatePanel ID="UpdatePanel" runat="server">
		<ContentTemplate>
			<bx:BXAdminFilter ID="BXForumCategoryFilter" runat="server" AssociatedGridView="BXForumCategoryGrid">
				<bx:BXTextBoxStringFilter Key="Name" Text="<%$ LocRaw:FilterText.Name %>" Visibility="AlwaysVisible" />
				<bx:BXTextBoxFilter Key="ID" Text="ID" ValueType="Integer" />
				<bx:BXTextBoxFilter Key="Sort" Text="<%$ LocRaw:FilterText.Sort %>" ValueType="Integer" />
				<bx:BXTextBoxFilter Key="XmlId" Text="<%$ LocRaw:FilterText.XmlId %>" />
			</bx:BXAdminFilter>
			<bx:BXContextMenuToolbar ID="BXForumCategoryListToolbar" runat="server">
				<Items>
					<bx:BXCmSeparator SectionSeparator="true" />
					<bx:BXCmImageButton ID="AddButton" Text="<%$ LocRaw:NewButtonText %>"
						Title="<%$ LocRaw:NewButtonTitle %>" CssClass="context-button icon btn_new"
						Href="ForumCategoryEdit.aspx" />
				</Items>
			</bx:BXContextMenuToolbar>
			<bx:BXPopupPanel ID="PopupPanelView" runat="server">
				<Commands>
					<bx:CommandItem UserCommandId="edit" Default="True" IconClass="edit" 
						ItemText="<%$ LocRaw:PopupText.Edit %>" ItemTitle="<%$ LocRaw:PopupText.Edit %>" OnClickScript="window.location.href = 'ForumCategoryEdit.aspx?id=' + UserData['ID']; return false;"  />
					<bx:CommandItem UserCommandId="delete" Default="True" IconClass="delete" ItemText="<%$ LocRaw:PopupText.Delete %>"
						ItemTitle="<%$ LocRaw:PopupText.Delete %>" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
				</Commands>
			</bx:BXPopupPanel>
			
			<bx:BXValidationSummary ID="ErrorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="GridView"/>
			<br />
			<bx:BXGridView ID="BXForumCategoryGrid" runat="server"
				ContentName="<%$ LocRaw:DataGridTitle %>" AllowSorting="True" AllowPaging="True" DataKeyNames="ID"  
				
				SettingsToolbarId="BXForumCategoryListToolbar"
				PopupCommandMenuId="PopupPanelView"
				ContextMenuToolbarId="BXMultiActionMenuToolbar"
				
				DataSourceID="BXForumCategoryGrid"
				OnSelect="BXForumCategoryGrid_Select" 
				OnSelectCount="BXForumCategoryGrid_SelectCount"
				OnRowDataBound="BXForumCategoryGrid_RowDataBound"
				OnUpdate="BXForumCategoryGrid_Update"
				OnDelete="BXForumCategoryGrid_Delete">
					<Columns>
						<asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" ReadOnly="True"/>
						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Name %>" SortExpression="Name">
							<itemtemplate>
								<%# Encode(Eval("Name").ToString())%>
							</itemtemplate>
							<edititemtemplate>
								<asp:TextBox ID="Name" runat="server" Text='<%# Bind("Name") %>' Width="90%"></asp:TextBox>
								<asp:RequiredFieldValidator ID="NameValidator" runat="server" ControlToValidate="Name"
									ErrorMessage="<%$ Loc:Error.EmptyCategoryName %>" ValidationGroup="GridView" >*</asp:RequiredFieldValidator>
							</edititemtemplate>
						</asp:TemplateField>
						<asp:BoundField DataField="Sort" HeaderText="<%$ LocRaw:FilterText.Sort %>" SortExpression="Sort" />
						<asp:BoundField DataField="XmlId" HeaderText="<%$ LocRaw:FilterText.XmlId %>" SortExpression="XmlId" Visible="false"/>
					</Columns>
					<AjaxConfiguration UpdatePanelId="UpdatePanel" />
			</bx:BXGridView>
			
			<bx:BXMultiActionMenuToolbar ID="BXMultiActionMenuToolbar" runat="server" ValidationGroup="GridView">
				<Items>
					<bx:BXMamImageButton CommandName="inline" ShowConfirmBar="true" DisableForAll="true"
						EnabledCssClass="context-button icon edit" DisabledCssClass="context-button icon edit-dis"
						Title="<%$ LocRaw:ActionTitle.EditSelectedCategories %>" />
					<bx:BXMamImageButton CommandName="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>" EnabledCssClass="context-button icon delete"
						DisabledCssClass="context-button icon delete-dis" Title="<%$ LocRaw:PopupText.Delete %>" />
				</Items>
			</bx:BXMultiActionMenuToolbar>
			
		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>