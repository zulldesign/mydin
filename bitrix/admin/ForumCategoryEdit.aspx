<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" 
	AutoEventWireup="true" CodeFile="ForumCategoryEdit.aspx.cs" Inherits="BXForumAdminPageCategoryEdit"%>
	
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	
	<bx:BXContextMenuToolbar ID="BXForumCategoryEditToolbar" runat="server" OnCommandClick="OnToolBarButtonClick">
		<Items>
			<bx:BXCmSeparator ID="ListButton" runat="server" SectionSeparator="true" />
			<bx:BXCmImageButton CssClass="context-button icon btn_list" CommandName="list"
				Text="<%$ LocRaw:BackButtonText %>" Title="<%$ LocRaw:BackButtonTitle %>" Href="ForumCategoryList.aspx" />
		
			<bx:BXCmSeparator SectionSeparator="true" />
			<bx:BXCmImageButton ID="AddButton"
				CommandName="add" Text="<%$ LocRaw:Kernel.Add %>" Title="<%$ LocRaw:NewButtonTitle %>"
				CssClass="context-button icon btn_new" Href="ForumCategoryEdit.aspx" />
				
			<bx:BXCmSeparator SectionSeparator="true" />
			<bx:BXCmImageButton ID="DeleteButton"
				CommandName="delete" Text="<%$ LocRaw:Kernel.Delete %>" Title="<%$ LocRaw:DeleteButtonTitle %>"
				CssClass="context-button icon btn_delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:DeleteConfirmText %>"/>
		</Items>
	</bx:BXContextMenuToolbar>
	<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="ForumCategoryEdit" />
	<bx:BXTabControl ID="mainTabControl" runat="server" OnCommand="OnForumCategoryEdit" ValidationGroup="ForumCategoryEdit">
		<bx:BXTabControlTab ID="BXTabControlTab1" Runat="server" Selected="True" Text="<%$ LocRaw:BaseSettingsTabText %>" Title="<%$ LocRaw:BaseSettingsTabTitle %>">
			<table class="edit-table" cellspacing="0" cellpadding="0" border="0">
				<% 
				if (Id > 0)
				{
					%><tr valign="top"><td class="field-name">ID:</td><td><%=Id %></td></tr><%
				}
				%>
				<tr valign="top">
					<td class="field-name"><span class="required">*</span><%= GetMessage("Category.Name") %>:</td>
					<td>
						<asp:TextBox ID="CategoryName" runat="server" Width="250px" />
						<asp:RequiredFieldValidator ID="NameRequiredValidator" runat="server" ValidationGroup="ForumCategoryEdit" ControlToValidate="CategoryName" ErrorMessage="<%$ Loc:Error.EmptyCategoryName %>" Display="Dynamic">*</asp:RequiredFieldValidator>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name"><%= GetMessage("Category.Sort") %>:</td>
					<td><asp:TextBox ID="CategorySort" runat="server" Width="50px" /></td>
				</tr>
				<tr valign="top">
					<td class="field-name"><%= GetMessage("Category.XmlId") %>:</td>
					<td><asp:TextBox ID="CategoryXmlId" runat="server" Width="250px" /></td>
				</tr>
			</table>
		</bx:BXTabControlTab>
	</bx:BXTabControl>
</asp:Content>

