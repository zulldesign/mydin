<%--<%@ Page Language="C#" MasterPageFile="AdminMasterPage.master" AutoEventWireup="true"
    CodeFile="SiteAdmin.aspx.cs" Inherits="bitrix_admin_Site" ValidateRequest="false"
    Theme="AdminTheme" StylesheetTheme="AdminTheme" %>
    zg--%>
<%@ Page Language="C#" MasterPageFile="AdminMasterPage.master" AutoEventWireup="true"
    CodeFile="SiteAdmin.aspx.cs" Inherits="bitrix_admin_Site" ValidateRequest="false" %>
<%@ Register Assembly="Main" Namespace="Bitrix.UI" TagPrefix="bx" %>

<%@ Import Namespace="Bitrix.Services" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <contenttemplate>

			<bx:BXAdminFilter ID="BXAdminFilter1" runat="server" AssociatedGridView="GridView">
				<bx:BXTextBoxStringFilter ID="filterName" runat="server" Key="Name" Text="<%$ Loc:FilterText.Title %>" Visibility="AlwaysVisible" />
				<bx:BXDropDownFilter ID="filterLanguageId" runat="server" Key="LanguageId" Text="<%$ Loc:FilterText.Languages %>">
				</bx:BXDropDownFilter>
				<bx:BXDropDownFilter ID="BXDropDownFilter1" runat="server" Key="Active" Text="<%$ Loc:FilterText.IsActive %>" ValueType="Boolean" >
					<asp:ListItem Value="" Text="<%$ Loc:Any %>"></asp:ListItem>
					<asp:ListItem Value="True" Text="<%$ Loc:Kernel.Yes %>"></asp:ListItem>
					<asp:ListItem Value="False" Text="<%$ Loc:Kernel.No %>"></asp:ListItem>
				</bx:BXDropDownFilter>
				<bx:BXTextBoxFilter ID="BXTextBoxFilter1" runat="server" Key="ID" Text="ID" />
			</bx:BXAdminFilter>
			
			<bx:BXPopupPanel id="PopupPanel" runat="server">
				<Commands>
					<bx:CommandItem UserCommandId="edit" ItemTitle="<%$ Loc:PopupTitle.Change %>" Default="True" ItemText="<%$ Loc:PopupTitle.Change %>" OnClickScript="" IconClass="edit"></bx:CommandItem>
					<bx:CommandItem UserCommandId="copy" ItemTitle="<%$ Loc:Kernel.Copy %>" ItemText="<%$ Loc:Kernel.Copy %>" OnClickScript="" IconClass="copy"></bx:CommandItem>
					<bx:CommandItem ItemCommandType="Separator" UserCommandId="" ItemTitle="" ItemText="" OnClickScript="" IconClass=""></bx:CommandItem>
					<bx:CommandItem UserCommandId="delete" ItemTitle="<%$ Loc:Kernel.Delete %>" ItemText="<%$ Loc:Kernel.Delete %>" OnClickScript="" IconClass="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>"></bx:CommandItem>
				</Commands>
			</bx:BXPopupPanel> 
			<bx:BXPopupPanel ID="PopupPanelView" runat="server">
				<Commands>
					<bx:CommandItem Default="True" IconClass="view" ItemText="<%$ Loc:Kernel.View %>" ItemTitle="<%$ LocRaw:PopupTitle.View %>"
						UserCommandId="edit" />
				</Commands>
			</bx:BXPopupPanel>
			<bx:BXContextMenuToolbar id="Toolbar" runat="server" OnCommandClick="Toolbar_CommandClick">
				<Items>
					<bx:BXCmImageButton ID="AddButton" CssClass="context-button icon btn_new" Text="<%$ Loc:Kernel.Add %>" CommandName="add" />
				</Items>
			</bx:BXContextMenuToolbar>
			
			<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" />
			<bx:BXMessage ID="successMessage" runat="server" Content="<%$ Loc:Message.RecordsHaveBeenDeletedSuccessfully %>"
				CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False" />
			
			<bx:BXGridView id="GridView" runat="server" ContextMenuToolbarId="MultiActionMenuToolbar" SettingsToolbarId="Toolbar" AjaxConfiguration-UpdatePanelId="UpdatePanel2" AllowPaging="True" PopupCommandMenuId="PopupPanel" OnPopupMenuClick="GridView_PopupMenuClick" DataKeyNames="ID" AllowSorting="True"  ContentName="<%$ Loc:TableTitle.sites %>" OnSelect="GridView_Select" OnDelete="GridView_Delete" OnSelectCount="GridView_SelectCount" DataSourceID="GridView"
            ForeColor="#333333"
            BorderWidth="0px"
            BorderColor="white"
            BorderStyle="none"
            ShowHeader = "true"
            CssClass="list"
            style="font-size: small; font-family: Arial; border-collapse: separate;" 			
			>
			
			<pagersettings position="TopAndBottom" pagebuttoncount="3"/>
            <RowStyle BackColor="#FFFFFF"/>
            <AlternatingRowStyle BackColor="#FAFAFA" /> 
            <FooterStyle BackColor="#EAEDF7"/>
			<Columns>
				<asp:TemplateField SortExpression="ID" HeaderText="ID"><ItemTemplate>
                    <a href="SiteEdit.aspx?id=<%# Encode(UrlEncode((string)Eval("ID"))) %>"><%# Encode((string)Eval("ID")) %></a>
				</ItemTemplate>
				</asp:TemplateField>
				<asp:TemplateField SortExpression="Active" HeaderText="<%$ Loc:ColumnHeaderText.IsActive %>"><ItemTemplate>
					<asp:Literal ID="ltActive" runat="server" Text='<%# (bool)Eval("Active") ? GetMessage("Kernel.Yes") : GetMessage("Kernel.No") %>'></asp:Literal>
                    
</ItemTemplate>
</asp:TemplateField>
<asp:BoundField ReadOnly="True" DataField="Sort" SortExpression="Sort" HeaderText="<%$ Loc:ColumnHeaderText.Sort %>"></asp:BoundField>
<asp:BoundField ReadOnly="True" DataField="Name" SortExpression="Name" HeaderText="<%$ Loc:FilterText.Title %>"></asp:BoundField>
<asp:BoundField ReadOnly="True" DataField="Directory" SortExpression="Directory" HeaderText="<%$ Loc:ColumnHeaderText.Folder %>"></asp:BoundField>
<asp:TemplateField SortExpression="Default" HeaderText="<%$ Loc:ColumnHeaderText.Default %>"><ItemTemplate>
					<asp:Literal ID="ltDefault" runat="server" Text='<%# (bool)Eval("Default") ? GetMessage("Kernel.Yes") : GetMessage("Kernel.No") %>'></asp:Literal>
				
                    
</ItemTemplate>
</asp:TemplateField>
</Columns>

<AjaxConfiguration UpdatePanelId="UpdatePanel2"></AjaxConfiguration>
</bx:BXGridView>

<bx:BXMultiActionMenuToolbar id="MultiActionMenuToolbar" runat="server">
    <Items>
		<bx:BXMamImageButton runat="server" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>" 
			EnabledCssClass="context-button icon delete" DisabledCssClass="context-button icon delete-dis"
			CommandName="delete" />
	</Items>
</bx:BXMultiActionMenuToolbar>
<br/>
<br/>
</contenttemplate>
    </asp:UpdatePanel>
</asp:Content>
