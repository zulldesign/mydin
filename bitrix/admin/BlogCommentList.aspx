<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" 
	AutoEventWireup="false" CodeFile="BlogCommentList.aspx.cs" Inherits="bitrix_admin_BlogCommentList" Title="<%$ LocRaw:PageTitle %>" %>
	
<%@ Import Namespace="Bitrix.Blog" %>	
<%@ Import Namespace="Bitrix.DataLayer" %>
<%@ Import Namespace="Bitrix.CommunicationUtility" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	<asp:UpdatePanel ID="UpdatePanel" runat="server">
		<ContentTemplate>
       
			<bx:BXAdminFilter runat="server" ID="ItemFilter" AssociatedGridView="ItemGrid">				
				<bx:BXTextBoxFilter Text="ID" Key="ID" ValueType="Integer" Visibility="AlwaysVisible" />
				<bx:BXTimeIntervalFilter Key="DateCreated" Text="<%$ Loc:FilterText.DateCreated %>" />
				<bx:BXAutoCompleteFilter runat="server"
				    ID="AuthorFilterItem" 
				    ValueType="Integer" 
				    Key="AuthorId" 
				    Text="<%$ LocRaw:FilterText.Author %>"
				    Url="~/bitrix/handlers/Main/UsersHandler.ashx" 
				    TextBoxWidth="460px" />
				<bx:BXTextBoxStringFilter Key="AuthorName" Text="<%$ LocRaw:FilterText.AuthorName %>" Visibility="AlwaysVisible" />				
				<bx:BXTextBoxStringFilter Key="AuthorEmail" Text="<%$ LocRaw:FilterText.AuthorEmail %>" />				
				<%--<bx:BXDropDownFilter ID="Anonymous" Key="Active" Text="<%$ Loc:FilterText.Anonymous %>">
					<asp:ListItem Value="" Text="<%$ Loc:Any %>"></asp:ListItem>
					<asp:ListItem Value="Y" Text="<%$ Loc:Kernel.Yes %>" ></asp:ListItem>
					<asp:ListItem Value="N" Text="<%$ Loc:Kernel.No %>" ></asp:ListItem>
				</bx:BXDropDownFilter>--%>
				<bx:BXTextBoxStringFilter Key="Content" Text="<%$ LocRaw:FilterText.Content %>" />											
				<bx:BXTextBoxStringFilter Key="AuthorIP" Text="<%$ LocRaw:FilterText.AuthorIP %>" />											
				<bx:BXDropDownFilter ID="IsApproved" ValueType="Boolean" Key="IsApproved" Text="<%$ Loc:FilterText.IsApproved %>">
					<asp:ListItem Value="" Text="<%$ Loc:Any %>"></asp:ListItem>
					<asp:ListItem Value="true" Text="<%$ Loc:Kernel.Yes %>" ></asp:ListItem>
					<asp:ListItem Value="false" Text="<%$ Loc:Kernel.No %>" ></asp:ListItem>
				</bx:BXDropDownFilter>				
				<bx:BXDropDownFilter runat="server" ID="BlogIdFilterItem" Key="BlogId" ValueType="Integer" Text="<%$ Loc:FilterText.Blog %>" Visibility="AlwaysVisible">
					<asp:ListItem Value="" Text="<%$ Loc:Any %>"></asp:ListItem>
				</bx:BXDropDownFilter>
				<bx:BXTextBoxFilter Key="PostId" Text="<%$ LocRaw:FilterText.PostId %>" ValueType="Integer" />											
				<bx:BXAutoCompleteFilter 
				    Key="Blog.OwnerId" 
				    ValueType="Integer"
				    Text="<%$ LocRaw:FilterText.BlogOwner %>"
				    Url="~/bitrix/handlers/Main/UsersHandler.ashx" 
				    TextBoxWidth="460px"/>								
				<bx:BXDropDownFilter runat="server" ID="CategoryIdFilterItem" Key="Blog.Categories.CategoryId" ValueType="Integer" Text="<%$ Loc:FilterText.Category %>">
					<asp:ListItem Value="" Text="<%$ Loc:Any %>"></asp:ListItem>
				</bx:BXDropDownFilter>				    
				<bx:BXDropDownFilter runat="server" ID="SiteFilterItem" Key="Blog.Categories.Category.Sites.SiteId" ValueType="String" Text="<%$ Loc:FilterText.Site %>">
					<asp:ListItem Value="" Text="<%$ Loc:Any %>"></asp:ListItem>
				</bx:BXDropDownFilter>		
			</bx:BXAdminFilter>

			<bx:BXContextMenuToolbar ID="ItemListToolbar" runat="server">
				<Items></Items>
			</bx:BXContextMenuToolbar>
			
			<bx:BXPopupPanel ID="PopupPanelView" runat="server" OnCommandClick="PopupPanelView_CommandClick">
				<Commands>
					<bx:CommandItem UserCommandId="approve" Default="True" IconClass="" 
						ItemText="<%$ LocRaw:PopupText.Approve %>" ItemTitle="<%$ LocRaw:PopupTitle.Approve %>" />
						
					<bx:CommandItem UserCommandId="disapprove" Default="True" IconClass="" 
						ItemText="<%$ LocRaw:PopupText.Disapprove %>" ItemTitle="<%$ LocRaw:PopupTitle.Disapprove %>" />
												
					<bx:CommandItem UserCommandId="delete" Default="True" IconClass="delete" ItemText="<%$ LocRaw:PopupText.Delete %>"
						ItemTitle="<%$ LocRaw:PopupTitle.Delete %>" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
				</Commands>
			</bx:BXPopupPanel>
			
			<bx:BXValidationSummary ID="ErrorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="GridView"/>
			
			<br />
			
			<bx:BXGridView ID="ItemGrid" runat="server" PageSize="20"
				ContentName="<%$ LocRaw:TableTitle %>" AllowSorting="True" AllowPaging="True" DataKeyNames="ID"  
				SettingsToolbarId="ItemListToolbar" PopupCommandMenuId="PopupPanelView" ContextMenuToolbarId="MultiActionMenuToolbar" DataSourceID="ItemGrid" 
				OnSelect="ItemGrid_Select" OnSelectCount="ItemGrid_SelectCount" 
				OnDelete="ItemGrid_Delete" OnRowDataBound = "ItemGrid_RowDataBound"
				OnMultiOperationActionRun="ItemGrid_MultiOperationActionRun">
					<Columns>
						<asp:BoundField DataField="ID"  HeaderText="ID" SortExpression="ID" ItemStyle-Width="30px" ReadOnly="True"/>
						<asp:BoundField DataField="IsApproved"  HeaderText="<%$ LocRaw:ColumnHeaderText.IsApproved %>" SortExpression="IsApproved" ItemStyle-Width="100px" ReadOnly="True" />
						<asp:BoundField DataField="DateCreated"  HeaderText="<%$ LocRaw:ColumnHeaderText.DateCreated %>" SortExpression="DateCreated" ItemStyle-Width="100px" ReadOnly="True" />
						<asp:BoundField DataField="AuthorHtml" HtmlEncode="false"   HeaderText="<%$ LocRaw:ColumnHeaderText.Author %>" SortExpression="AuthorName" ReadOnly="True" />					
						<asp:BoundField DataField="Text" HtmlEncode="false"  HeaderText="<%$ LocRaw:ColumnHeaderText.Text %>" SortExpression="Content" ReadOnly="True" />
						<asp:BoundField DataField="PostTitleHtml" HtmlEncode="false"  HeaderText="<%$ LocRaw:ColumnHeaderText.PostTitle %>" SortExpression="Post.Title" ReadOnly="True" />
						<asp:BoundField DataField="BlogHtml" HtmlEncode="false"  HeaderText="<%$ LocRaw:ColumnHeaderText.BlogName %>" SortExpression="Blog.Name" ReadOnly="True" />
						<asp:BoundField DataField="AuthorIpHtm"  HtmlEncode="false" HeaderText="<%$ LocRaw:ColumnHeaderText.AuthorIP %>" SortExpression="AuthorIP" ReadOnly="True" />						
					</Columns>
					<AjaxConfiguration UpdatePanelId="UpdatePanel" />
			</bx:BXGridView>
			
			<bx:BXMultiActionMenuToolbar ID="MultiActionMenuToolbar" runat="server" ValidationGroup="GridView">
				<Items>
					<bx:BXMamImageButton CommandName="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>" EnabledCssClass="context-button icon delete"
						DisabledCssClass="context-button icon delete-dis" Title="<%$ LocRaw:ActionTitle.Delete %>" />
					<bx:BXMamListItem runat="server" CommandName="approve" Text="<%$ Loc:ActionText.Approve %>" />
					<bx:BXMamListItem runat="server" CommandName="disapprove" Text="<%$ Loc:ActionText.Disapprove %>" />
				</Items>				
			</bx:BXMultiActionMenuToolbar>			

		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>