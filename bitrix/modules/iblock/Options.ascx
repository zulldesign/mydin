<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Options.ascx.cs" Inherits="bitrix_modules_iblock_Options" %>

<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" ValidationGroup="vgInnerForm" HeaderText="<%$ Loc:Kernel.Error %>" />
<bx:BXMessage ID="successMessage" runat="server" CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>"
	Visible="False" Content="<%$ Loc:Message.SettingsHasBeenSavedSuccessfully %>" />

<bx:BXTabControl ID="BXTabControl1" ValidationGroup="vgInnerForm" runat="server" OnCommand="BXTabControl1_Command">
	<bx:BXTabControlTab runat="server" Selected="True" Text="<%$ Loc:TabText.AdjustmentOfModuleParameters %>"
		Title="<%$ Loc:Kernel.TopPanel.Settings %>">
		<table border="0" cellpadding="0" cellspacing="0" class="edit-table">
		    <tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Legend.UseVisualEditor") %>
				</td>
				<td width="60%">
					<asp:CheckBox ID="cbUserVisualEditor" ValidationGroup="vgInnerForm" runat="server" />
				</td>
			</tr>
		    <%--<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Legend.CombinedReviewOfSectionsAndElements") %>
				</td>
				<td width="60%">
					<asp:CheckBox ID="cbCombinedListMode" ValidationGroup="vgInnerForm" runat="server" />
				</td>
			</tr>--%>
		    <tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Legend.MaxDepthOfSectionMenus") %>
				</td>
				<td width="60%">
					<asp:TextBox ID="tbMenuMaxDepth" ValidationGroup="vgInnerForm" runat="server"></asp:TextBox>
				</td>
			</tr>
		</table>
	</bx:BXTabControlTab>
<%--	<bx:BXTabControlTab runat="server" Text="Регистрация и авторизация" Title="Авторизация">
		<table border="0" cellpadding="0" cellspacing="0" class="edit-table">
			<tr valign="top">
				<td class="field-name" width="40%">
					Свойство X:
				</td>
				<td width="60%">
					<asp:TextBox ValidationGroup="vgInnerForm" ID="TextBox2" runat="server"></asp:TextBox>
				</td>
			</tr>
		</table>
	</bx:BXTabControlTab>
--%>
</bx:BXTabControl>
