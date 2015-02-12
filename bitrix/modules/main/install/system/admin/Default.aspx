<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="bitrix_admin_Default" Title="<%$ Loc:Kernel.TopPanel.ControlPanel %>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

<div class="notes">
<table cellspacing="0" cellpadding="0" border="0" class="notes" width="100%">
	<tr class="top">
		<td class="left"><div class="empty"></div></td>
		<td><div class="empty"></div></td>
		<td class="right"><div class="empty"></div></td>
	</tr>
	<tr>
		<td class="left"><div class="empty"></div></td>
		<td class="content"><%= GetMessage("Text.Welcome") %>&nbsp;&quot;<b><span id="spanSiteName" runat="server"/></b>&quot;.<br/>
<div class="empty" style="height:4px"></div><%= GetMessage("Text.PoweredBy") %>&nbsp;&quot;<%= GetMessage("Text.BitrixCMS")%>&nbsp;<span id="spanVersion" runat="server"/>&quot;.<br/>
		</td>
		<td class="right"><div class="empty"></div></td>
	</tr>
	<tr class="bottom">
		<td class="left"><div class="empty"></div></td>
		<td><div class="empty"></div></td>
		<td class="right"><div class="empty"></div></td>
	</tr>
</table>
</div>

	<asp:UpdatePanel ID="UpdatePanel1" runat="server">
		<ContentTemplate>
			<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" OnCommandClick="BXContextMenuToolbar1_CommandClick" runat="server">
				<Items>
					<bx:BXCmSeparator ID="BXCmSeparator1" runat="server" SectionSeparator="true" />
					<bx:BXCmImageButton ID="BXCmImageButton1" runat="server" 
						CssClass="context-button icon btn_list" CommandName="icon"
						Text="<%$ Loc:ActionText.Icons %>" Title="<%$ Loc:ActionText.Icons %>" 
					/>
					<bx:BXCmSeparator ID="BXCmSeparator2" runat="server" SectionSeparator="true" />
					<bx:BXCmImageButton ID="BXCmImageButton2" runat="server" 
						CssClass="context-button icon btn_new" CommandName="list"
						Text="<%$ Loc:ActionText.List %>" Title="<%$ Loc:ActionText.List %>"
					/>
				</Items>
			</bx:BXContextMenuToolbar>
			<br /><br />
			<table id="mainPageTable" runat="server" cellpadding="0" cellspacing="0" border="0" width="100%">
			</table>
		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>
