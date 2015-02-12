<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" 
	AutoEventWireup="true" CodeFile="ForumSubscriptionsList.aspx.cs" Inherits="BXForumAdminPageForumSubscriptionsList" Title="<%$ LocRaw:PageTitle %>" %>
	
<%@ Import Namespace="Bitrix.Forum" %>	
<%@ Import Namespace="Bitrix.DataLayer" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<asp:UpdatePanel ID="UpdatePanel" runat="server">
		<ContentTemplate>
			<bx:BXAdminFilter ID="BXUsersFilter" runat="server" AssociatedGridView="BXUsersGrid">
				
				<bx:BXTextBoxStringFilter Key="User.UserName" Text="<%$ LocRaw:FilterText.UserName %>" Visibility="AlwaysVisible"/>
				<bx:BXTextBoxStringFilter Key="User.LastName" Text="<%$ LocRaw:FilterText.LastName %>" Visibility="StartHidden"/>
				<bx:BXTextBoxStringFilter Key="User.FirstName" Text="<%$ LocRaw:FilterText.FirstName %>" Visibility="StartHidden"/>
				
			</bx:BXAdminFilter>
			<bx:BXContextMenuToolbar ID="BXUsersListToolbar" runat="server">
				<Items>
				</Items>
			</bx:BXContextMenuToolbar>
			<bx:BXPopupPanel ID="PopupPanelView" runat="server">
				<Commands>
					<bx:CommandItem UserCommandId="detail" Default="True" IconClass="edit" 
						ItemText="<%$ LocRaw:PopupText.ViewDetail %>" ItemTitle="<%$ LocRaw:PopupText.ViewDetail %>" OnClickScript="window.location.href = 'ForumSubscriptionsDetail.aspx?id=' + UserData['UserID']; return false;"  />
					<bx:CommandItem UserCommandId="delete" Default="True" IconClass="delete" ItemText="<%$ LocRaw:PopupText.DeleteText %>"
						ItemTitle="<%$ LocRaw:PopupText.DeleteText %>" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
				</Commands>
			</bx:BXPopupPanel>
			
			<bx:BXValidationSummary ID="ErrorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="GridView"/>
			<br />
			<bx:BXGridView ID="BXUsersGrid" runat="server"
				ContentName="<%$ LocRaw:DataGridTitle %>" AllowSorting="True" AllowPaging="True" DataKeyNames="UserID"  
				
				SettingsToolbarId="BXUsersListToolbar"
				PopupCommandMenuId="PopupPanelView"
				ContextMenuToolbarId="BXMultiActionMenuToolbar"
				
				DataSourceID="BXUsersGrid" 
				OnSelect="BXUsersGrid_Select" 
				OnSelectCount="BXUsersGrid_SelectCount"
				OnRowDataBound="BXUsersGrid_RowDataBound"
				OnDelete="BXUsersGrid_Delete">
					<Columns>
						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.UserName %>" SortExpression="User.UserName">
							<itemtemplate>
								<%# Encode(Eval("UserName").ToString())%>
							</itemtemplate>
							<edititemtemplate>
								<asp:TextBox ID="Name" runat="server" Text='<%# Bind("UserName") %>' Width="90%"></asp:TextBox>
								<asp:RequiredFieldValidator ID="NameValidator" runat="server" ControlToValidate="Name"
									ErrorMessage="<%$ Loc:Error.EmptyForumName %>" ValidationGroup="GridView" >*</asp:RequiredFieldValidator>
							</edititemtemplate>
						</asp:TemplateField>
						<asp:BoundField DataField="DisplayNameWithEditLink" HeaderText="<%$ LocRaw:ColumnHeaderText.DisplayName %>" SortExpression="User.DisplayName" HtmlEncode="false"/>
						<asp:BoundField DataField="FirstName" HeaderText="<%$ LocRaw:ColumnHeaderText.FirstName %>" SortExpression="User.FirstName" Visible="false"/>
						<asp:BoundField DataField="LastName" HeaderText="<%$ LocRaw:ColumnHeaderText.LastName %>" SortExpression="User.LastName" ReadOnly="True" Visible="false"/>
						<asp:BoundField DataField="Email" HeaderText="<%$ LocRaw:ColumnHeaderText.Email %>" SortExpression="User.Email" ReadOnly="True"/>
						<asp:BoundField DataField="SubscriptionsCount" HeaderText="<%$ LocRaw:ColumnHeaderText.SubscriptionsCount %>" ReadOnly="True"/>
					</Columns>
					<AjaxConfiguration UpdatePanelId="UpdatePanel" />
			</bx:BXGridView>
			
			<bx:BXMultiActionMenuToolbar ID="BXMultiActionMenuToolbar" runat="server" ValidationGroup="GridView">
				<Items>
					<bx:BXMamImageButton CommandName="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>" EnabledCssClass="context-button icon delete"
						DisabledCssClass="context-button icon delete-dis" Title="<%$ LocRaw:PopupText.DeleteText %>" />
				</Items>
			</bx:BXMultiActionMenuToolbar>			

		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>