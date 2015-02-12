<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" CodeFile="Options.ascx.cs" Inherits="bitrix_modules_Blog_Options" %>
<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" ValidationGroup="vgInnerForm" HeaderText="<%$ Loc:Kernel.Error %>" />
<bx:BXMessage ID="successMessage" runat="server" CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>"
	Visible="False" Content="<%$ Loc:Message.SettingsHasBeenSavedSuccessfully %>" />
	
<bx:BXTabControl ID="tabContainer" ValidationGroup="vgInnerForm" runat="server" OnCommand="OnTabCommand">
	<bx:BXTabControlTab ID="tabMain" runat="server" Selected="True" Text="<%$ Loc:TabText.AdjustmentOfModuleParameters %>"
		Title="<%$ Loc:Kernel.TopPanel.Settings %>">
		<table border="0" cellpadding="0" cellspacing="0" class="edit-table">
		    <tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Legend.BlogSyndicationRefreshIntervalInMin") %>
				</td>
				<td width="60%">
					<asp:TextBox ID="tbxBlogSyndicationRefreshIntervalInMin" ValidationGroup="vgInnerForm" runat="server"></asp:TextBox>
					<asp:RegularExpressionValidator ID="vcBlogSyndicationRefreshIntervalInMin" runat="server" ValidationGroup="vgInnerForm"  ControlToValidate="tbxBlogSyndicationRefreshIntervalInMin" ValidationExpression="^[1-9][0-9]*$" ErrorMessage="<%$ Loc:Message.BlogSyndicationRefreshIntervalInMinMustBeIntegerGreaterThanZero %>" Display="Dynamic">*</asp:RegularExpressionValidator>	
				</td>
			</tr>
		</table>
	</bx:BXTabControlTab>
</bx:BXTabControl>	