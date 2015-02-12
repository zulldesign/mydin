<%--<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="AuthRolesSync.aspx.cs" Inherits="bitrix_admin_AuthRolesSync" Title="Синхронизация ролей" StylesheetTheme="AdminTheme" %>
zg --%>
<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="AuthRolesSync.aspx.cs" Inherits="bitrix_admin_AuthRolesSync" Title="<%$ Loc:PageTitle.SynchronizationOfRoleList %>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<asp:UpdatePanel ID="UpdatePanel1" runat="server">
		<ContentTemplate>
			<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" runat="server">
				<Items>
					<bx:BXCmSeparator ID="BXCmSeparator1" runat="server" SectionSeparator="true" />
					<bx:BXCmImageButton ID="BXCmImageButton1" runat="server" CssClass="context-button icon btn_list" CommandName="go2list"
						Text="<%$ Loc:ActionText.RolesList %>" Title="<%$ Loc:ActionTitle.Go2RoleList %>"
						Href="AuthRolesList.aspx" />
				</Items>
			</bx:BXContextMenuToolbar>
			<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>"/>
			<bx:BXMessage ID="successMessage" runat="server" Content="<%$ Loc:Message.RoleListHasBeenSynchronizedSuccessfully %>"
				CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False" Width="438px" />
			<bx:BXTabControl ID="BXTabControl1" runat="server" OnCommand="BXTabControl1_Command">
				<bx:BXTabControlTab ID="BXTabControlTab1" runat="server" Selected="True" Text="<%$ Loc:TabText.Synchronization %>" Title="<%$ Loc:TabText.Synchronization %>">
					<table id="Table1" border="0" cellpadding="0" cellspacing="0" class="edit-table">
						<tr valign="top" id="trUsersCount" runat="server">
							<td class="field-name" width="40%">
								<%= GetMessage("LegendText.SynchronizationProviders") %></td>
							<td width="60%">
								<asp:ListBox ID="lbSyncRoles" SelectionMode="Multiple" runat="server"></asp:ListBox>
								<asp:Label ID="lblSyncRolesStub" runat="server" Visible="false"><%= GetMessage("Message.AuthorizationProvidersAreNotDefined") %></asp:Label>
							</td>
						</tr>
					</table>
				</bx:BXTabControlTab>
			</bx:BXTabControl>
		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>

