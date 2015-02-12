<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" 
	AutoEventWireup="true" CodeFile="ForumList.aspx.cs" Inherits="BXForumAdminPageForumList" Title="<%$ LocRaw:PageTitle %>" %>
	
<%@ Import Namespace="Bitrix.Forum" %>	
<%@ Import Namespace="Bitrix.DataLayer" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<asp:UpdatePanel ID="UpdatePanel" runat="server">
		<ContentTemplate>
			<bx:BXAdminFilter ID="BXForumFilter" runat="server" AssociatedGridView="BXForumGrid">
			
				<bx:BXTextBoxStringFilter Key="Name" Text="<%$ LocRaw:FilterText.Name %>" Visibility="AlwaysVisible" />
			
				<bx:BXDropDownFilter ID="siteIdFilter" Key="Sites.SiteId" Text="<%$ LocRaw:FilterText.Site %>" Visibility="AlwaysVisible">
					<asp:ListItem Text="<%$ LocRaw:Option.SelectSite %>" Value="" />
				</bx:BXDropDownFilter>
				
				<bx:BXDropDownFilter ID="categoryIdFilter" Key="CategoryId" Text="<%$ LocRaw:FilterText.Category %>" ValueType="Integer">
					<asp:ListItem Text="<%$ LocRaw:Option.SelectCategory %>" Value="" />
					<asp:ListItem Text="<%$ LocRaw:Option.EmptyCategory %>" Value="0" />
				</bx:BXDropDownFilter>
				
				<bx:BXTextBoxFilter Key="ID" Text="ID" ValueType="Integer" />
				<bx:BXTextBoxFilter Key="Sort" Text="<%$ LocRaw:FilterText.Sort %>" ValueType="Integer" />
				<bx:BXTextBoxStringFilter Key="Code" Text="<%$ LocRaw:FilterText.Code %>" />
				<bx:BXTextBoxStringFilter Key="XmlId" Text="<%$ LocRaw:FilterText.XmlId %>" />
				
			</bx:BXAdminFilter>
			<bx:BXContextMenuToolbar ID="BXForumListToolbar" runat="server">
				<Items>
					<bx:BXCmSeparator SectionSeparator="true" />
					<bx:BXCmImageButton ID="AddButton" Text="<%$ LocRaw:NewButtonText %>"
						Title="<%$ LocRaw:NewForumButtonTitle %>" CssClass="context-button icon btn_new"
						Href="ForumEdit.aspx" />
				</Items>
			</bx:BXContextMenuToolbar>
			<bx:BXPopupPanel ID="PopupPanelView" runat="server">
				<Commands>
					<bx:CommandItem UserCommandId="edit" Default="True" IconClass="edit" 
						ItemText="<%$ LocRaw:PopupText.EditText %>" ItemTitle="<%$ LocRaw:PopupTitle.EditTitle %>" OnClickScript="window.location.href = 'ForumEdit.aspx?id=' + UserData['ID']; return false;"  />
					<bx:CommandItem UserCommandId="delete" Default="True" IconClass="delete" ItemText="<%$ LocRaw:PopupText.DeleteText %>"
						ItemTitle="<%$ LocRaw:PopupText.DeleteText %>" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
				</Commands>
			</bx:BXPopupPanel>
			
			<bx:BXValidationSummary ID="ErrorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="GridView"/>
			<br />
			<bx:BXGridView ID="BXForumGrid" runat="server"
				ContentName="<%$ LocRaw:DataGridTitle %>" AllowSorting="True" AllowPaging="True" DataKeyNames="ID"  
				
				SettingsToolbarId="BXForumListToolbar"
				PopupCommandMenuId="PopupPanelView"
				ContextMenuToolbarId="BXMultiActionMenuToolbar"
				
				DataSourceID="BXForumGrid" 
				OnSelect="BXForumGrid_Select" 
				OnSelectCount="BXForumGrid_SelectCount"
				OnRowDataBound="BXForumGrid_RowDataBound"
				OnRowUpdating="BXForumGrid_RowUpdating"
				OnUpdate="BXForumGrid_Update"
				OnDelete="BXForumGrid_Delete">
					<Columns>
						<asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" ReadOnly="True"/>

						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Name %>" SortExpression="Name">
							<itemtemplate>
								<%# Encode(Eval("Name").ToString())%>
							</itemtemplate>
							<edititemtemplate>
								<asp:TextBox ID="Name" runat="server" Text='<%# Bind("Name") %>' Width="90%"></asp:TextBox>
								<asp:RequiredFieldValidator ID="NameValidator" runat="server" ControlToValidate="Name"
									ErrorMessage="<%$ Loc:Error.EmptyForumName %>" ValidationGroup="GridView" >*</asp:RequiredFieldValidator>
							</edititemtemplate>
						</asp:TemplateField>

						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Category %>" SortExpression="CategoryId">
							<edititemtemplate>
								<asp:DropDownList id="categoryIdEdit" runat="server" DataValueField="ID" DataTextField="Name" DataSource="<%# Categories %>" ></asp:DropDownList> 
							</edititemtemplate>
							<itemtemplate>
								<asp:Literal id="categoryId" runat="server" Text='<%# GetCategoryName(Container.DataItem) %>'></asp:Literal> 
							</itemtemplate>
						</asp:TemplateField>

						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Active %>" SortExpression="Active">
							<itemtemplate>
								<%# ((bool)Eval("Active")) ? GetMessageRaw("Kernel.Yes") : GetMessageRaw("Kernel.No") %>
							</itemtemplate>
							<edititemtemplate>
								<asp:CheckBox ID="Active" runat="server" Checked='<%# Bind("Active") %>' />
							</edititemtemplate>
						</asp:TemplateField>
						
						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Site %>" SortExpression="SiteId">
							<itemtemplate>
								<asp:Literal id="siteId" runat="server" Text='<%# GetSites(Container.DataItem)%>'></asp:Literal> 
							</itemtemplate>
						</asp:TemplateField>
						
						<asp:BoundField DataField="Sort" HeaderText="<%$ LocRaw:ColumnHeaderText.Sort %>" SortExpression="Sort" />
						<asp:BoundField DataField="Topics" HeaderText="<%$ LocRaw:ColumnHeaderText.Topics %>" SortExpression="Topics" ReadOnly="True"/>
						<asp:BoundField DataField="Replies" HeaderText="<%$ LocRaw:ColumnHeaderText.Replies %>" SortExpression="Replies" ReadOnly="True"/>
						<asp:BoundField DataField="DateCreate" HeaderText="<%$ LocRaw:ColumnHeaderText.DateCreate %>" SortExpression="DateCreate" ReadOnly="True"/>
						
						<asp:TemplateField HeaderText="BBCode" SortExpression="AllowBBCode" Visible="false">
							<itemtemplate>
								<%# ((bool)Eval("AllowBBCode")) ? GetMessageRaw("Kernel.Yes") : GetMessageRaw("Kernel.No")%>
							</itemtemplate>
							<edititemtemplate>
								<asp:CheckBox ID="AllowBBCode" runat="server" Checked='<%# Bind("AllowBBCode") %>' />
							</edititemtemplate>
						</asp:TemplateField>

						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.AllowSmiles %>" SortExpression="AllowSmiles" Visible="false">
							<itemtemplate>
								<%# ((bool)Eval("AllowSmiles")) ? GetMessageRaw("Kernel.Yes") : GetMessageRaw("Kernel.No")%>
							</itemtemplate>
							<edititemtemplate>
								<asp:CheckBox ID="AllowSmiles" runat="server" Checked='<%# Bind("AllowSmiles") %>' />
							</edititemtemplate>
						</asp:TemplateField>
						
						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.IndexContent %>" SortExpression="IndexContent" Visible="false">
							<itemtemplate>
								<%# ((bool)Eval("IndexContent")) ? GetMessageRaw("Kernel.Yes") : GetMessageRaw("Kernel.No")%>
							</itemtemplate>
							<edititemtemplate>
								<asp:CheckBox ID="IndexContent" runat="server" Checked='<%# Bind("IndexContent") %>' />
							</edititemtemplate>
						</asp:TemplateField>
						
						<asp:BoundField DataField="Code" HeaderText="<%$ LocRaw:ColumnHeaderText.Code %>" SortExpression="Code" Visible="false"/>
						<asp:BoundField DataField="XmlId" HeaderText="<%$ LocRaw:ColumnHeaderText.XmlId %>" SortExpression="XmlId" Visible="false"/>
					</Columns>
					<AjaxConfiguration UpdatePanelId="UpdatePanel" />
			</bx:BXGridView>
			
			<bx:BXMultiActionMenuToolbar ID="BXMultiActionMenuToolbar" runat="server" ValidationGroup="GridView">
				<Items>
					<bx:BXMamImageButton CommandName="inline" ShowConfirmBar="true" DisableForAll="true"
						EnabledCssClass="context-button icon edit" DisabledCssClass="context-button icon edit-dis"
						Title="<%$ LocRaw:ActionTitle.EditSelected %>" />
					<bx:BXMamImageButton CommandName="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>" EnabledCssClass="context-button icon delete"
						DisabledCssClass="context-button icon delete-dis" Title="<%$ LocRaw:PopupText.DeleteText %>" />
				</Items>
			</bx:BXMultiActionMenuToolbar>			

		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>