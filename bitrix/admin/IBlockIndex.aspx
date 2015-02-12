<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="IBlockIndex.aspx.cs" Inherits="bitrix_admin_IBlockIndex" Title="<%$ Loc:PageTitle.InformationalBlock %>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
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
			<div id="divBody" runat="server"></div>
		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>

