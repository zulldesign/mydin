<%--<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="FileManView.aspx.cs" Inherits="bitrix_admin_FileManView" Title="<%$ Loc:PageTitle %>"
	Theme="AdminTheme" StylesheetTheme="AdminTheme" EnableViewState="false" ValidateRequest="false" %> zg--%>
	
<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="FileManView.aspx.cs" Inherits="bitrix_admin_FileManView" Title="<%$ Loc:PageTitle %>" EnableViewState="false" ValidateRequest="false" %> 	

<%@ Import Namespace="Bitrix.Services" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<% ButtonBack.Href = BackUrl; %>
	<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" runat="server">
		<Items>
			<bx:BXCmImageButton runat="server" ID="ButtonBack" CssClass="context-button icon btn_folder_up" CommandName="back"
				Text="<%$ Loc:ActionText.GoBack %>" Title="<%$ Loc:ActionTitle.GoBack %>"
				/>
		</Items>
	</bx:BXContextMenuToolbar>
	<bx:BXTabControl ID="BXTabControl1" runat="server" ButtonsMode="Hidden">
		<bx:BXTabControlTab runat="server" ID="mainTab" Selected="True">
			<asp:MultiView ID="fileViews" runat="server">
				<asp:View ID="imageView" runat="server">
					<div style="border: solid 1px black; margin-top: 5px; margin-bottom: 5px; padding: 5px; text-align: center; ">
						<asp:Image ID="imageContent" runat="server" />
					</div>
				</asp:View>
				<asp:View ID="textView" runat="server">
					<asp:Literal ID="Literal1" runat="server" Text="<%$ Loc:FileEncoding %>"></asp:Literal>
					<asp:DropDownList ID="textViewEncodings" runat="server" Width="200px" AutoPostBack="True" EnableViewState="False" />
					<asp:Button ID="textViewEdit" runat="server" Text="<%$ Loc:Kernel.Edit %>" UseSubmitBehavior="False" />
					<table class="edit-table" style="border: solid 1px black; margin-top: 5px; margin-bottom: 5px; padding: 5px;">
						<tr><td><asp:Literal ID="textContent" runat="server"></asp:Literal></td></tr>
					</table>
				</asp:View>
				<asp:View ID="defaultView" runat="server">
					<div style="border: solid 1px black; margin-top: 5px; margin-bottom: 5px; padding: 5px; overflow: auto">
						<asp:Literal ID="defaultContent" runat="server" />
					</div>
				</asp:View>
			</asp:MultiView>
		</bx:BXTabControlTab>
	</bx:BXTabControl>
	<br />
</asp:Content>
