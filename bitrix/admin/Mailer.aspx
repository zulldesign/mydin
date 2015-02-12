<%--<%@ Page Language="C#" MasterPageFile="AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="Mailer.aspx.cs" Inherits="bitrix_admin_Mailer" ValidateRequest="false"
	Theme="AdminTheme" StylesheetTheme="AdminTheme" Title="<%$ Loc:PageTitle %>" %> zg--%>
	
<%@ Page Language="C#" MasterPageFile="AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="Mailer.aspx.cs" Inherits="bitrix_admin_Mailer" ValidateRequest="false" Title="<%$ Loc:PageTitle %>" %> 	

<%@ Register Assembly="Main" Namespace="Bitrix.UI" TagPrefix="bx" %>
<%@ Import Namespace="Bitrix.Services" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
		<ContentTemplate>
			<bx:BXAdminFilter ID="Filter" runat="server" AssociatedGridView="MailerGridView" >
				<bx:BXDropDownKeyTextBoxFilter ID="BXDropDownKeyTextBoxFilter1" runat="server" Text="<%$ Loc:Kernel.Find %>" Visibility="AlwaysVisible" Operation="Like" >
					<asp:ListItem Value="Subject" Text="<%$ Loc:Option.Subject %>" />
					<asp:ListItem Value="EmailFrom" Text="<%$ Loc:Option.Sender %>" />
					<asp:ListItem Value="EmailTo" Text="<%$ Loc:Option.Addressee %>" />
					<asp:ListItem Value="Message" Text="<%$ Loc:Option.Message %>" />
				</bx:BXDropDownKeyTextBoxFilter>
				<bx:BXTextBoxFilter ID="BXTextBoxFilter1" runat="server" Key="Id" Text="ID" ValueType="Integer" />
				<bx:BXTextBoxAndDropDownFilter ID="templateFilter" runat="server" Key="Name"
					Text="<%$ Loc:FilterText.EventType %>">
					<asp:ListItem Value="" Text="<%$ Loc:Option.All %>" />
				</bx:BXTextBoxAndDropDownFilter>
				<bx:BXTimeIntervalFilter ID="BXTimeIntervalFilter1" runat="server" Key="LastUpdated" Text="<%$ Loc:FilterText.DateOfModification %>" />
				<bx:BXDropDownFilter ID="siteFilter" runat="server" Text="<%$ Loc:FilterText.Site %>" Key="Sites.SiteId">
					<asp:ListItem Value="" Text="<%$ Loc:Option.All %>" />
				</bx:BXDropDownFilter>
				<bx:BXDropDownFilter ID="BXDropDownFilter1" runat="server" Text="<%$ Loc:FilterText.Activity %>" Key="Active" ValueType="Boolean">
					<asp:ListItem Value="" Text="<%$ Loc:Option.All %>" />
					<asp:ListItem Value="true" Text="<%$ Loc:Kernel.Yes %>" />
					<asp:ListItem Value="false" Text="<%$ Loc:Kernel.No %>" />
				</bx:BXDropDownFilter>
				<bx:BXTextBoxFilter ID="BXTextBoxFilter2" runat="server" Key="EmailFrom" Text="<%$ Loc:FilterText.Sender %>" />
				<bx:BXTextBoxFilter ID="BXTextBoxFilter3" runat="server" Key="EmailTo" Text="<%$ Loc:FilterText.Addressee %>" />
				<bx:BXTextBoxFilter ID="BXTextBoxFilter4" runat="server" Key="Bcc" Text="<%$ Loc:FilterText.Bcc %>" />
				<bx:BXTextBoxFilter ID="BXTextBoxFilter5" runat="server" Key="Subject" Text="<%$ Loc:Option.Subject %>" />
				<bx:BXDropDownFilter ID="BXDropDownFilter2" runat="server" Text="<%$ Loc:FilterText.MessageType %>" Key="BodyType">
					<asp:ListItem Value="" Text="<%$ Loc:Option.All %>" />
					<asp:ListItem Value="html" Text="Html" />
					<asp:ListItem Value="text" Text="<%$ Loc:Option.Text %>" />
				</bx:BXDropDownFilter>
				<bx:BXTextBoxStringFilter ID="BXTextBoxStringFilter1" runat="server" Key="Message" Text="<%$ Loc:FilterText.Message %>" />
			</bx:BXAdminFilter>
			<bx:BXContextMenuToolbar ID="MailerContextMenuToolbar" runat="server">
				<Items>
					<bx:BXCmImageButton ID="AddButton" runat="server" CssClass="context-button icon btn_new" CommandName="add"
						Text="<%$ Loc:Kernel.Add %>" Title="<%$ Loc:ActionTitle.Add %>" Href="MailerEdit.aspx?action=add" />
				</Items>
			</bx:BXContextMenuToolbar>
			<br />
			<bx:BXGridView ID="MailerGridView" runat="server" ContentName="<%$ Loc:TableTitle %>"
				DataSourceID="MailerGridView" AllowSorting="True" DataKeyNames="Id"
				ContextMenuToolbarId="MailerMultiActionMenuToolbar" PopupCommandMenuId="MailerPopupPanel"
				AllowPaging="True" AjaxConfiguration-UpdatePanelId="UpdatePanel2" SettingsToolbarId="MailerContextMenuToolbar"
				OnDelete="MailerGridView_Delete" OnSelect="MailerGridView_Select" OnSelectCount="MailerGridView_SelectCount"
				OnPopupMenuClick="MailerGridView_PopupMenuClick" OnUpdate="MailerGridView_Update"
				OnRowDataBound="MailerGridView_RowDataBound"
                ForeColor="#333333"
                BorderWidth="0px"
                BorderColor="white"
                BorderStyle="none"
                ShowHeader = "true"
                CssClass="list"
                style="font-size: small; font-family: Arial; border-collapse: separate;" 				
				>
				<Columns>
					<asp:BoundField ReadOnly="True" DataField="Id" SortExpression="Id" HeaderText="<%$ Loc:IdColumnHeader %>">
					</asp:BoundField>
					<asp:BoundField ReadOnly="True" DataField="LastUpdated" SortExpression="LastUpdated"
						HeaderText="<%$ Loc:LastUpdatedColumnHeader %>"></asp:BoundField>
					<asp:TemplateField SortExpression="Active" HeaderText="<%$ Loc:ActiveColumnHeader %>">
						<itemtemplate>
<asp:Literal id="Literal1" runat="server" Text='<%# (bool)Eval("Active") ? GetMessage("Kernel.Yes") : GetMessage("Kernel.No") %>'></asp:Literal> 
</itemtemplate>
						<edititemtemplate>
							<asp:CheckBox ID="CheckBox1" runat="server" Checked='<%# Bind("Active") %>' />
						</edititemtemplate>
					</asp:TemplateField>
					<asp:BoundField ReadOnly="True" DataField="Name" SortExpression="Name" HeaderText="<%$ Loc:NameColumnHeader %>">
					</asp:BoundField>
					<asp:BoundField DataField="Subject" HeaderText="<%$ Loc:SubjectColumnHeader %>"></asp:BoundField>
					<asp:TemplateField HeaderText="<%$ Loc:SitesColumnHeader %>">
						<itemtemplate>
						<asp:Literal ID="Literal2" runat="server" Text='<%# GetSitesList(Container.DataItem) %>'/>				
</itemtemplate>
					</asp:TemplateField>
				</Columns>
				<AjaxConfiguration UpdatePanelId="UpdatePanel2" />
			</bx:BXGridView>
			<br />
			<bx:BXMultiActionMenuToolbar ID="MailerMultiActionMenuToolbar" runat="server">
				<Items>
					<bx:BXMamImageButton ID="BXMamImageButton1" runat="server" EnabledCssClass="context-button icon edit" DisabledCssClass="context-button icon edit-dis"
						CommandName="inline" Title="<%$ Loc:ActionTitle.Edit %>" DisableForAll="true"
						ShowConfirmBar="true" />
					<bx:BXMamImageButton ID="BXMamImageButton2" runat="server" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>" EnabledCssClass="context-button icon delete"
						DisabledCssClass="context-button icon delete-dis" CommandName="delete" Title="<%$ Loc:ActionTitle.Delete %>" />
				</Items>
			</bx:BXMultiActionMenuToolbar>
			<bx:BXPopupPanel ID="MailerPopupPanel" runat="server">
				<Commands>
					<bx:CommandItem Default="True" IconClass="edit" ItemText="<%$ Loc:Kernel.Edit %>"
						ItemTitle="<%$ Loc:PopupTitle.Edit %>" UserCommandId="edit" OnClickScript="location.href=UserData.EditUrl; return false" />
					<bx:CommandItem IconClass="copy" ItemText="<%$ Loc:PopupText.Copy %>" ItemTitle="<%$ Loc:PopupTitle.Copy %>"
						OnClickScript="" UserCommandId="copy" />
					<bx:CommandItem ItemCommandType="Separator" />
					<bx:CommandItem IconClass="delete" ItemText="<%$ Loc:Kernel.Delete %>" ItemTitle="<%$ Loc:PopupTitle.Delete %>"
						UserCommandId="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
				</Commands>
			</bx:BXPopupPanel>
			<bx:BXPopupPanel ID="PopupPanelView" runat="server">
				<Commands>
					<bx:CommandItem Default="True" IconClass="view" ItemText="<%$ Loc:Kernel.View %>" ItemTitle="<%$ LocRaw:PopupTitle.View %>"
						UserCommandId="edit" OnClickScript="location.href=UserData.EditUrl; return false" />
				</Commands>
			</bx:BXPopupPanel>
		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>


