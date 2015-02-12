<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="FileManSecurity.aspx.cs" Inherits="bitrix_admin_FileManSecurity" Title="<%$ Loc:PageTitle %>"
	EnableViewState="false" %>

<%@ Register Src="~/bitrix/controls/Main/OperationsEdit.ascx" TagName="OperationsEdit"
	TagPrefix="uc1" %>
<%@ Import Namespace="Bitrix.IO" %>
<%@ Import Namespace="Bitrix.UI" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<%@ Import Namespace="Bitrix.Security" %>
<%@ Import Namespace="System.Collections.Generic" %>
<asp:Content ID="mainContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<% BXCmImageButton1.Href = String.Concat("FileMan.aspx?path=", UrlEncode(curDir)); %>
	<bx:BXContextMenuToolbar ID="MainActionBar" runat="server">
		<Items>
			<bx:BXCmSeparator ID="BXCmSeparator1" runat="server" SectionSeparator="true" />
			<bx:BXCmImageButton ID="BXCmImageButton1" runat="server" CssClass="context-button icon btn_folder_up"
				Text="<%$ Loc:ActionText.GoBack %>" Title="<%$ Loc:ActionTitle.GoBack %>" OnClickScript="return fileManSecurity_GoBackToList();" />
		</Items>
	</bx:BXContextMenuToolbar>
	<bx:BXValidationSummary ID="ErrorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" />
	<bx:BXMessage ID="SuccessMessage" runat="server" Content="<%$ Loc:Message.Success %>"
		CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False"
		Width="438px" />
	<bx:BXTabControl ID="MainTabControl" runat="server" OnCommand="mainTabControl_Command">
		<bx:BXTabControlTab runat="server" Selected="True" Text="<%$ Loc:TabText.Security %>"
			Title="<%$ Loc:TabTitle.Security %>" ShowTitle="True">
			<table width="100%" cellspacing="0" cellpadding="0" border="0" class="edit-table">
				<tr>
					<td>
						<asp:Literal ID="SecurityLiteralTop" runat="server" EnableViewState="false" /></td>
				</tr>
				<tr>
					<td style="vertical-align: top">
						<uc1:OperationsEdit ID="OperationsEdit" runat="server" ShowLegend="true" LegendText-Allow="<%$ LocRaw:SecurityLegend.Allow %>"
							LegendText-Deny="<%$ LocRaw:SecurityLegend.Deny %>" LegendText-InheritAllow="<%$ LocRaw:SecurityLegend.InheritAllow %>"
							LegendText-InheritDeny="<%$ LocRaw:SecurityLegend.InheritDeny %>"
							LegendText-DontModify="<%$ LocRaw:SecurityLegend.DontModify %>" />
					</td>
				</tr>
			</table>
		</bx:BXTabControlTab>
	</bx:BXTabControl>
</asp:Content>
