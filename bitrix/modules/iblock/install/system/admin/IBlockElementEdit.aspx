<%--<%@ Page Language="C#" MasterPageFile="AdminMasterPage.master" AutoEventWireup="true"
    CodeFile="Language.aspx.cs" Inherits="bitrix_admin_Language" ValidateRequest="false"
    Theme="AdminTheme" StylesheetTheme="AdminTheme" %> zg--%>
    
<%@ Page Language="C#" MasterPageFile="AdminMasterPage.master" AutoEventWireup="true"
    CodeFile="IBlockElementEdit.aspx.cs" Inherits="bitrix_admin_IBlockElementEdit" ValidateRequest="false" %>    
<%@ Reference VirtualPath="~/bitrix/admin/controls/iblock/IBlockPublicEditorGroupRenderer.ascx" %>

<%@ Register Assembly="Main" Namespace="Bitrix.UI" TagPrefix="bx" %>
<%@ Import Namespace="Bitrix.Services" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
   			<bx:BXContextMenuToolbar ID="Toolbar" runat="server" OnCommandClick="Toolbar_CommandClick">
				<Items>			
				<bx:BXCmImageButton
					CssClass="context-button icon btn_list" ID="GoToListButton" CommandName="go2list"
					Text="" Title=""
				/>
				<bx:BXCmSeparator SectionSeparator="true" />
				<bx:BXCmImageButton ID="AddElementButton" CommandName="addElement" CssClass="context-button icon btn_new" Text="" Title="" />
					<bx:BXCmSeparator />
									<bx:BXCmImageButton ID="DeleteButton" 
						CssClass="context-button icon btn_delete" CommandName="delete"
						Text="" Title=""
						ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.DeleteElement %>"
					/>

					<bx:BXCmImageButton ID="EditButton" CssClass="context-button icon btn_settings" Text="<%$ Loc:Setup %>"
						CommandName="edit" Href="" OnClickScript="return openEditDialog();" />
				</Items>
			</bx:BXContextMenuToolbar>
            <bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>"/>
			<asp:PlaceHolder ID="TabControlPreInit" runat="server" OnInit="TabControlPreInit_Init" />
            <bx:BXTabControl ID="BXTabControl1" runat="server" OnCommand="BXTabControl1_Command">
            </bx:BXTabControl>        
	<script type="text/javascript">
		function openEditDialog() {
		    (new BX.CDialogNet({ 'content_url': '<%= SettingsPagePath %>?iblock_id=<%= IBlock.Id %>&clientType=WindowManager&ReturnUrl=<%= HttpUtility.UrlEncode(Bitrix.Services.BXSefUrlManager.CurrentUrl.ToString()) %>&lang=<%= HttpUtility.UrlEncode(Bitrix.Services.BXLoc.CurrentLocale) %>', 'width': '500', 'height': '400', 'resizable': false, 'adminStyle': true })).Show();
			return false;
		}
</script>


</asp:Content>

