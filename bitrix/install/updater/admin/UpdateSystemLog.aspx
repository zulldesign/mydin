<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="UpdateSystemLog.aspx.cs" Inherits="bitrix_admin_UpdateSystemLog" Title="Untitled Page" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

<br />
<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" runat="server">
	<Items>
		<bx:BXCmSeparator ID="BXCmSeparator1" runat="server">
		</bx:BXCmSeparator>
		<bx:BXCmImageButton ID="BXCmImageButton2" runat="server" CssClass="context-button icon"
			CommandName="ViewUpdates" Text="<%$ Loc:ActionText.Go2UpdateSystem %>" OnClickScript="window.location.href=&quot;UpdateSystem.aspx&quot;;return false;" Href="UpdateSystem.aspx" >
		</bx:BXCmImageButton>
		<bx:BXCmSeparator ID="BXCmSeparator3" runat="server">
		</bx:BXCmSeparator>
		<bx:BXCmImageButton ID="BXCmImageButton1" runat="server" CssClass="context-button icon"
			CommandName="ChangeSettings" Text="<%$ Loc:ActionText.ChangeSettings %>" OnClickScript="window.location.href=&quot;UpdateSystemSettings.aspx&quot;;return false;" Href="UpdateSystemSettings.aspx" >
		</bx:BXCmImageButton>
	</Items>
</bx:BXContextMenuToolbar>
<br />

</asp:Content>

