<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" 
	AutoEventWireup="false" CodeFile="SearchContentTagList.aspx.cs" Inherits="bitrix_admin_SearchContentTagList" Title="<%$ LocRaw:PageTitle %>" %>
	
<%@ Import Namespace="Bitrix.DataLayer" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<asp:UpdatePanel ID="UpdatePanel" runat="server">
		<ContentTemplate>
       		<bx:BXAdminFilter ID="ItemFilter" runat="server" AssociatedGridView="ItemGrid">
				<bx:BXTextBoxFilter Key="Tag.Name" Text="<%$ LocRaw:FilterText.Tag %>" Operation="Like" Visibility="AlwaysVisible" />
				<bx:BXTextBoxFilter Key="Tag.Id" Text="ID" ValueType="Integer" />
				<bx:BXDropDownFilter Key="Tag.Status" Text="<%$ LocRaw:FilterText.Status %>" ValueType="Integer">
					<asp:ListItem Text="<%$ LocRaw:Kernel.Any %>" Value="" />
					<asp:ListItem Text="<%$ LocRaw:TagStatus.Unknown %>" Value="0" />
					<asp:ListItem Text="<%$ LocRaw:TagStatus.Approved %>" Value="1" />					
					<asp:ListItem Text="<%$ LocRaw:TagStatus.Rejected %>" Value="-1" />					
				</bx:BXDropDownFilter>
				<bx:BXBetweenFilter Key="Tag.TagCount" Text="<%$ LocRaw:FilterText.TagCount %>" ValueType="Integer" />
				<bx:BXTimeIntervalFilter Key="Tag.LastUpdate" Text="<%$ LocRaw:FilterText.LastUpdate %>" />
			</bx:BXAdminFilter>

			<bx:BXContextMenuToolbar ID="ItemListToolbar" runat="server">
				<Items>
					<bx:BXCmSeparator SectionSeparator="true" />
					<bx:BXCmImageButton ID="AddButton" Text="<%$ LocRaw:ActionText.Add %>"
						Title="<%$ LocRaw:ActionTitle.Add %>" CssClass="context-button icon btn_new"
						Href="SearchContentTagEdit.aspx" />
				</Items>
				
			</bx:BXContextMenuToolbar>
			
			<bx:BXPopupPanel ID="PopupPanelView" runat="server">
				<Commands>
					<bx:CommandItem UserCommandId="view" Default="True" IconClass="view" 
						ItemText="<%$ LocRaw:PopupText.View %>" ItemTitle="<%$ LocRaw:PopupTitle.View %>" OnClickScript="window.location.href='SearchContentTagEdit.aspx?id=' + UserData['Id']; return false;"  />
					<bx:CommandItem UserCommandId="edit" Default="True" IconClass="edit" 
						ItemText="<%$ LocRaw:Kernel.Edit %>" ItemTitle="<%$ LocRaw:PopupTitle.Edit %>" OnClickScript="window.location.href='SearchContentTagEdit.aspx?id=' + UserData['Id']; return false;"  />
					<bx:CommandItem UserCommandId="delete" Default="True" IconClass="delete" ItemText="<%$ LocRaw:Kernel.Delete %>"
						ItemTitle="<%$ LocRaw:PopupTitle.Delete %>" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
				</Commands>
			</bx:BXPopupPanel>
			
			<bx:BXValidationSummary ID="ErrorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="GridView"/>
			
			<br />
			
			<bx:BXGridView ID="ItemGrid" runat="server"
				ContentName="<%$ LocRaw:TableTitle %>" AllowSorting="True" AllowPaging="True" DataKeyNames="Id"  
				
				SettingsToolbarId="ItemListToolbar"
				PopupCommandMenuId="PopupPanelView"
				ContextMenuToolbarId="MultiActionMenuToolbar"
				
				DataSourceID="ItemGrid" 
				OnSelect="ItemGrid_Select" 
				OnSelectCount="ItemGrid_SelectCount"
				OnDelete="ItemGrid_Delete"
				OnUpdate="ItemGrid_Update"
				OnRowDataBound = "ItemGrid_RowDataBound"
				>
					<Columns>
						<asp:BoundField DataField="Id" HeaderText="ID" SortExpression="Id" ReadOnly="True"/>
						<asp:BoundField DataField="Name" HeaderText="<%$ LocRaw:ColumnHeaderText.Tag %>" SortExpression="Name" ReadOnly="True"/>
						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Status %>" SortExpression="Status">
							<itemtemplate>
								<%# GetMessage("TagStatus." + Eval("Status")) %>
							</itemtemplate>
							<edititemtemplate>
								<asp:DropDownList id="statusEdit" runat="server" SelectedValue='<%# Bind("Status") %>' >
									<asp:ListItem Text="<%$ LocRaw:TagStatus.Unknown %>" Value="Unknown" />
									<asp:ListItem Text="<%$ LocRaw:TagStatus.Approved %>" Value="Approved" />					
									<asp:ListItem Text="<%$ LocRaw:TagStatus.Rejected %>" Value="Rejected" />					
								</asp:DropDownList> 
							</edititemtemplate>
						</asp:TemplateField>	
						<asp:BoundField DataField="TagCount" HeaderText="<%$ LocRaw:ColumnHeaderText.TagCount %>" SortExpression="TagCount" ReadOnly="True"/>				
						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.LastUpdate %>" SortExpression="LastUpdate">
							<itemtemplate>
								<%# ((DateTime)Eval("LastUpdate")) != DateTime.MinValue ? ((DateTime)Eval("LastUpdate")).ToString("g") : GetMessage("Undefined") %>
							</itemtemplate>
						</asp:TemplateField>
					</Columns>
					<AjaxConfiguration UpdatePanelId="UpdatePanel" />
			</bx:BXGridView>
			
			<bx:BXMultiActionMenuToolbar ID="MultiActionMenuToolbar" runat="server" ValidationGroup="GridView">
				<Items>
					<bx:BXMamImageButton CommandName="inline" ShowConfirmBar="true" DisableForAll="true"
						EnabledCssClass="context-button icon edit" DisabledCssClass="context-button icon edit-dis"
						Title="<%$ LocRaw:ActionTitle.Edit %>" />
				    <bx:BXMamImageButton CommandName="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>" EnabledCssClass="context-button icon delete"
						DisabledCssClass="context-button icon delete-dis" Title="<%$ LocRaw:ActionTitle.Delete %>" />
				</Items>
			</bx:BXMultiActionMenuToolbar>			

		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>