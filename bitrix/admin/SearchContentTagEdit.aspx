<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" 
	AutoEventWireup="true" CodeFile="SearchContentTagEdit.aspx.cs" Inherits="bitrix_admin_SearchContentTagEdit" %>
	
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<bx:BXContextMenuToolbar ID="BXContentTagEditToolbar" runat="server" OnCommandClick="OnToolBarButtonClick">
		<Items>
			<bx:BXCmSeparator ID="ListButton" runat="server" SectionSeparator="true" />
			<bx:BXCmImageButton CssClass="context-button icon btn_list" CommandName="list"
				Text="<%$ LocRaw:ActionText.List %>" Title="<%$ LocRaw:ActionTitle.List %>" Href="SearchContentTagList.aspx" />
		
			<bx:BXCmSeparator SectionSeparator="true" />
			<bx:BXCmImageButton ID="AddButton"
				CommandName="add" Text="<%$ LocRaw:Kernel.Add %>" Title="<%$ LocRaw:ActionTitle.Add %>"
				CssClass="context-button icon btn_new" Href="SearchContentTagEdit.aspx" />
				
			<bx:BXCmSeparator SectionSeparator="true" />
			<bx:BXCmImageButton ID="DeleteButton"
				CommandName="delete" Text="<%$ LocRaw:Kernel.Delete %>" Title="<%$ LocRaw:ActionTitle.Delete %>"
				CssClass="context-button icon btn_delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>"/>
		</Items>
	</bx:BXContextMenuToolbar>
	<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="TagEdit" />
	<bx:BXTabControl ID="TabControl" runat="server" OnCommand="OnTagEdit" ValidationGroup="TagEdit">
		<bx:BXTabControlTab ID="MainSettingsTab" Runat="server" Selected="True" Text="<%$ LocRaw:TabText.MainSettings %>" Title="<%$ LocRaw:TabTitle.MainSettings %>">
			<table class="edit-table" cellspacing="0" cellpadding="0" border="0">
				<% if (TagId > 0) { %>
				<tr valign="top"><td class="field-name">ID:</td><td><%= TagId %></td></tr>
				<% } %>
				
				<tr valign="top">
					<td class="field-name" width="40%"><%= GetMessage("Label.Name") %>:</td>
					<td width="60%"><% if (TagId <= 0) { %><asp:TextBox runat="server" ID="Name" /><% } else { %><b><%= Tag.Name %></b><% } %></td>
				</tr>

				<tr valign="top">
					<td class="field-name" width="40%"><%= GetMessage("Label.Status") %>:</td>
					<td width="60%">
						<asp:DropDownList runat="server" ID="Status" >
							<asp:ListItem Value="Unknown" Text="<%$ LocRaw:Status.Unknown %>" />
							<asp:ListItem Value="Approved" Text="<%$ LocRaw:Status.Approved %>" />
							<asp:ListItem Value="Rejected" Text="<%$ LocRaw:Status.Rejected %>" />
						</asp:DropDownList>
					</td>
				</tr>
				
				<% if (TagId > 0) { %>
				<tr valign="top">
					<td class="field-name" width="40%"><%= GetMessage("Label.TagCount") %>:</td>
					<td width="60%"><%= Tag.TagCount %></td>
				</tr>
				
				<tr valign="top">
					<td class="field-name" width="40%"><%= GetMessage("Label.LastUpdate") %>:</td>
					<td width="60%"><%= Tag.LastUpdate != DateTime.MinValue ? Tag.LastUpdate.ToString("g") : GetMessage("Undefined") %></td>
				</tr>
				<% } %>
			</table>
		</bx:BXTabControlTab>
	</bx:BXTabControl>
</asp:Content>

