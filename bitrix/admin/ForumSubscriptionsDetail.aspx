<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" 
	AutoEventWireup="true" CodeFile="ForumSubscriptionsDetail.aspx.cs" Inherits="BXForumAdminPageForumSubscriptionsDetail" Title="<%$ LocRaw:PageTitle %>" %>
	
<%@ Import Namespace="Bitrix.Forum" %>	
<%@ Import Namespace="Bitrix.DataLayer" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<asp:UpdatePanel ID="UpdatePanel" runat="server">
		<ContentTemplate>
			<bx:BXAdminFilter ID="BXSubscriptionsFilter" runat="server" AssociatedGridView="BXSubscriptionsGrid">
				<bx:BXTextBoxStringFilter Key="Forum.Name" Text="<%$ LocRaw:FilterText.ForumName %>" Visibility="AlwaysVisible" />
				<bx:BXTextBoxStringFilter Key="Topic.Name" Text="<%$ LocRaw:FilterText.TopicName %>" Visibility="AlwaysVisible" />
				<bx:BXDropDownFilter ID="siteIdFilter" Key="SiteId" Text="<%$ LocRaw:FilterText.Site %>" Visibility="AlwaysVisible">
					<asp:ListItem Text="<%$ LocRaw:Option.SelectSite %>" Value="" />
				</bx:BXDropDownFilter>
				
			</bx:BXAdminFilter>
			<bx:BXContextMenuToolbar ID="BXSubscriptionsToolbar" runat="server">
				<Items>
				</Items>
			</bx:BXContextMenuToolbar>
			<bx:BXPopupPanel ID="PopupPanelView" runat="server">
				<Commands>
					<bx:CommandItem UserCommandId="delete" Default="True" IconClass="delete" ItemText="<%$ LocRaw:PopupText.DeleteText %>"
						ItemTitle="<%$ LocRaw:PopupText.DeleteText %>" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
				</Commands>
			</bx:BXPopupPanel>
			
			<bx:BXValidationSummary ID="ErrorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="GridView"/>
			<br />
			<bx:BXGridView ID="BXSubscriptionsGrid" runat="server"
				ContentName="<%$ LocRaw:DataGridTitle %>" AllowSorting="True" AllowPaging="True" DataKeyNames="ID"  
				
				SettingsToolbarId="BXSubscriptionsToolbar"
				PopupCommandMenuId="PopupPanelView"
				ContextMenuToolbarId="BXMultiActionMenuToolbar"
				
				DataSourceID="BXSubscriptionsGrid" 
				OnSelect="BXSubscriptionsGrid_Select" 
				OnSelectCount="BXSubscriptionsGrid_SelectCount"
				OnRowDataBound="BXSubscriptionsGrid_RowDataBound"
				OnDelete="BXSubscriptionsGrid_Delete">
					<Columns>
						<asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" ReadOnly="True"/>
						
						<asp:BoundField DataField="ForumName" HeaderText="<%$ LocRaw:ColumnHeaderText.ForumName %>" SortExpression="Forum.Name" ReadOnly="True"/>
						<asp:BoundField DataField="TopicName" HeaderText="<%$ LocRaw:ColumnHeaderText.TopicName %>" SortExpression="Topic.Name" ReadOnly="True"/>
						<asp:BoundField DataField="SubscriptionTypeMessage" HeaderText="<%$ LocRaw:ColumnHeaderText.OnlyTopic %>" SortExpression="OnlyTopic" ReadOnly="True"/>
						<asp:BoundField DataField="SiteId" HeaderText="<%$ LocRaw:ColumnHeaderText.SiteId %>" SortExpression="SiteId" ReadOnly="True"/>
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