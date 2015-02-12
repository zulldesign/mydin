<%@ Page Language="C#" AutoEventWireup="false" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" CodeFile="UserSettings.aspx.cs" Inherits="bitrix_admin_UserSettings" EnableViewState="false"%>
<asp:Content runat="server" ID="Content" ContentPlaceHolderID="ContentPlaceHolder1">
	<bx:BXValidationSummary ID="errorSummary" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="Main" />
	<bx:BXTabControl runat="server" ID="TabControl" OnCommand="OnCommand">
		<bx:BXTabControlTab runat="server" ID="MainSettingsTab" Selected="True" Text="<%$ LocRaw:TabText.MainTab %>" Title="<%$ LocRaw:TabTitle.MainTab %>">
			<table class="edit-table" cellspacing="0" cellpadding="0" border="0">
				<tr class="heading">
					<td colspan="2">
						<%= GetMessage("TabTitle.Toolbar")%>
					</td>
				</tr>			
				<tr align="top">
				    <td class="field-name" width="40%"><%= GetMessage("FieldLabel.DisplayPageEditorToolbar") %></td>
				    <td>
						<asp:CheckBox runat="server" ID="displayPageEditorToolbarChbx" />
				    </td>
				</tr>
				<tr class="heading">
					<td colspan="2">
						<%= GetMessage("TabTitle.CommonSettings")%>
					</td>
				</tr>
				<tr align="top">
				    <td class="field-name"><%= GetMessage("FieldLabel.UseByDefault") %></td>
				    <td>
						<asp:CheckBox runat="server" ID="useByDefault" />
				    </td>
				</tr>									
			</table>
		</bx:BXTabControlTab>
	</bx:BXTabControl>	
</asp:Content>
