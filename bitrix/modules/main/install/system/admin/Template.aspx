<%--<%@ Page Language="C#" MasterPageFile="AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="Template.aspx.cs" Inherits="bitrix_admin_Template" ValidateRequest="false"
	Theme="AdminTheme" StylesheetTheme="AdminTheme" Title="<%$ Loc:PageTitle %>" %>--%>
	
<%@ Page Language="C#" MasterPageFile="AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="Template.aspx.cs" Inherits="bitrix_admin_Template" ValidateRequest="false"
	Title="<%$ Loc:PageTitle %>" %>
	
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
						OnClickScript="" IconClass="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>"></bx:CommandItem>
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
					<bx:BXCmImageButton ID="AddButton" Href="TemplateEdit.aspx" CssClass="context-button icon btn_new" Text="<%$ Loc:Kernel.Add %>" CommandName="add" />
				</Items>
			</bx:BXContextMenuToolbar>
			<bx:BXGridView ID="GridView" runat="server" ContentName="<%$ Loc:TableTitle.SiteTemplates %>"
				AllowSorting="True" DataKeyNames="ID"
				OnPopupMenuClick="GridView_PopupMenuClick" PopupCommandMenuId="PopupPanel" AllowPaging="True"
				AjaxConfiguration-UpdatePanelId="UpdatePanel2" SettingsToolbarId="Toolbar" ContextMenuToolbarId="MultiActionMenuToolbar"
				OnDelete="GridView_Delete" OnSelect="GridView_Select" OnSelectCount="GridView_SelectCount" DataSourceID="GridView"
			>
                <pagersettings position="TopAndBottom" pagebuttoncount="3"/>
                <RowStyle BackColor="White"/>
                <AlternatingRowStyle BackColor="#FAFAFA" /> 
                <FooterStyle BackColor="#EAEDF7"/>				
				<Columns>
					<asp:TemplateField SortExpression="ID" HeaderText="<%$ Loc:ColId %>">
						<itemtemplate>
                    <a href="TemplateEdit.aspx?id=<%# Eval("ID") %>"><%# Eval("ID") %></a>
                    <%# GetPreviewHtml(Eval("PreviewSmall") as string, Eval("PreviewBig") as string, Eval("PreviewBigPath") as string)%>
</itemtemplate>
					</asp:TemplateField>
					<asp:BoundField ReadOnly="True" DataField="Name" HeaderText="<%$ Loc:ColName %>"></asp:BoundField>
					<asp:BoundField ReadOnly="True" DataField="Description" HeaderText="<%$ Loc:ColDescription %>" HtmlEncode="false">
					</asp:BoundField>
				</Columns>
				<AjaxConfiguration UpdatePanelId="UpdatePanel2"></AjaxConfiguration>
			</bx:BXGridView>
			<bx:BXMultiActionMenuToolbar ID="MultiActionMenuToolbar" runat="server" >
				<Items>
                <bx:BXMamImageButton runat="server" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>"
					EnabledCssClass="context-button icon delete" DisabledCssClass="context-button icon delete-dis"
					CommandName="delete" />
				</Items>
			</bx:BXMultiActionMenuToolbar>
			<br />
			<br />
		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>
