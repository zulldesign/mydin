<%--<%@ Page Language="C#" MasterPageFile="AdminMasterPage.master" AutoEventWireup="true"
    CodeFile="Language.aspx.cs" Inherits="bitrix_admin_Language" ValidateRequest="false"
    Theme="AdminTheme" StylesheetTheme="AdminTheme" %> zg--%>
    
<%@ Page Language="C#" MasterPageFile="AdminMasterPage.master" AutoEventWireup="true"
    CodeFile="Language.aspx.cs" Inherits="bitrix_admin_Language" ValidateRequest="false" %>    


<%@ Register Assembly="Main" Namespace="Bitrix.UI" TagPrefix="bx" %>
<%@ Import Namespace="Bitrix.Services" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
		<ContentTemplate>
			<bx:BXPopupPanel ID="PopupPanel" runat="server">
				<Commands>
					<bx:CommandItem UserCommandId="edit" ItemTitle="<%$ Loc:EditDesc %>" Default="True"
						ItemText="<%$ Loc:Edit %>" OnClickScript="" IconClass="edit"></bx:CommandItem>
					<bx:CommandItem UserCommandId="copy" ItemTitle="<%$ Loc:CopyDesc %>" ItemText="<%$ Loc:Copy %>"
						OnClickScript="" IconClass="copy"></bx:CommandItem>
					<bx:CommandItem ItemCommandType="Separator" UserCommandId="" ItemTitle="" ItemText=""
						OnClickScript="" IconClass=""></bx:CommandItem>
					<bx:CommandItem UserCommandId="delete" ItemTitle="<%$ Loc:DeleteDesc %>" ItemText="<%$ Loc:Delete %>"
						OnClickScript="" IconClass="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
				</Commands>
			</bx:BXPopupPanel>
			<bx:BXPopupPanel ID="PopupPanelView" runat="server">
				<Commands>
					<bx:CommandItem Default="True" IconClass="view" ItemText="<%$ Loc:Kernel.View %>" ItemTitle="<%$ LocRaw:PopupTitle.View %>"
						UserCommandId="edit" />
				</Commands>
			</bx:BXPopupPanel>
			<bx:BXContextMenuToolbar ID="Toolbar" runat="server" OnCommandClick="Toolbar_CommandClick">
				<Items>
					<bx:BXCmImageButton ID="AddButton" CssClass="context-button icon btn_new" Text="<%$ Loc:Kernel.Add %>"
						CommandName="add" Href="LanguageEdit.aspx" />
				</Items>
			</bx:BXContextMenuToolbar>
			<br />
			<bx:BXValidationSummary ID="Summary" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" />
			<bx:BXGridView ID="GridView" runat="server" ContextMenuToolbarId="MultiActionMenuToolbar"
				SettingsToolbarId="Toolbar" AjaxConfiguration-UpdatePanelId="UpdatePanel2" AllowPaging="True"
				PopupCommandMenuId="PopupPanel" OnPopupMenuClick="GridView_PopupMenuClick" DataKeyNames="Id"
				AllowSorting="True" ContentName="<%$ Loc:TableTitle.MailTemplates %>" DataSourceID="GridView"
				OnSelect="GridView_Select" OnSelectCount="GridView_SelectCount" OnDelete="GridView_Delete">
				<Columns>
					<asp:TemplateField SortExpression="ID" HeaderText="<%$ Loc:ColId %>">
						<itemtemplate>
                    <a href="LanguageEdit.aspx?id=<%# Encode((string)Eval("ID")) %>"><%# Encode((string)Eval("ID")) %></a>
                    
</itemtemplate>
					</asp:TemplateField>
					<asp:TemplateField SortExpression="Active" HeaderText="<%$ Loc:ColActive %>">
						<itemtemplate>
					<asp:Literal ID="ltActive" runat="server" Text='<%# (bool)Eval("Active") ? GetMessage("Kernel.Yes") : GetMessage("Kernel.No") %>'></asp:Literal>
                    
</itemtemplate>
					</asp:TemplateField>
					<asp:BoundField ReadOnly="True" DataField="Sort" SortExpression="Sort" HeaderText="<%$ Loc:ColSort %>">
					</asp:BoundField>
					<asp:BoundField ReadOnly="True" DataField="Name" SortExpression="Name" HeaderText="<%$ Loc:ColName %>">
					</asp:BoundField>
					<asp:TemplateField SortExpression="Default" HeaderText="<%$ Loc:ColDefault %>">
						<itemtemplate>
					<asp:Literal ID="ltDefault" runat="server" Text='<%# (bool)Eval("Default") ? GetMessage("Kernel.Yes") : GetMessage("Kernel.No") %>'></asp:Literal>
				
                    
</itemtemplate>
					</asp:TemplateField>
					<asp:TemplateField SortExpression="Culture" HeaderText="<%$ Loc:ColumnHeaderText.Culture %>">
						<itemtemplate>
<%# Encode(((Bitrix.BXLanguage)Container.DataItem).GetCultureInfo().DisplayName) %>
</itemtemplate>
					</asp:TemplateField>
				</Columns>
				<AjaxConfiguration UpdatePanelId="UpdatePanel2"></AjaxConfiguration>
			</bx:BXGridView>
			<br />
			<bx:BXMultiActionMenuToolbar ID="MultiActionMenuToolbar" runat="server"
				ToolBarType="View">
				<Items>
					<bx:BXMamImageButton runat="server" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>" EnabledCssClass="context-button icon delete"
						DisabledCssClass="context-button icon delete-dis" CommandName="delete" />
				</Items>
			</bx:BXMultiActionMenuToolbar>
			<br />
			<br />
		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>
