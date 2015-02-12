<%--
<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="AuthUsersList.aspx.cs" Inherits="bitrix_admin_UsersList" Title="Список пользователей" ValidateRequest="false"
	Theme="AdminTheme" StylesheetTheme="AdminTheme"%>
	zg, 25.04.2008
--%>

<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="AuthUsersList.aspx.cs" Inherits="bitrix_admin_UsersList" Title="<%$ Loc:PageTitle.UserList %>" ValidateRequest="false"%>
	
<%@ Register Assembly="Main" Namespace="Bitrix.UI" TagPrefix="bx" %>
<%@ Register Src="~/bitrix/controls/Main/Calendar.ascx" TagName="Calendar" TagPrefix="bx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
		<ContentTemplate>
			<bx:BXAdminFilter ID="BXAdminFilter1" runat="server" AssociatedGridView="AuthUserGridView">
				<bx:BXTextBoxStringFilter ID="BXTextBoxStringFilter1" runat="server" Key="UserName" Text="<%$ Loc:FilterText.Login %>" Visibility="AlwaysVisible"/>
				<bx:BXTextBoxStringFilter ID="BXTextBoxStringFilter2" runat="server" Key="LastName" Text="<%$ Loc:FilterText.LastName %>"/>
				<bx:BXTextBoxStringFilter ID="BXTextBoxStringFilter3" runat="server" Key="Email" Text="EMail"/>
				<bx:BXBetweenFilter runat="server" Key="UserId" Text="ID" ValueType="Integer" />
				<bx:BXTimeIntervalFilter ID="BXTimeIntervalFilter1" runat="server" Key="UpdateDate" Text="<%$ Loc:FilterText.DateOfModification %>" />
				<bx:BXTimeIntervalFilter ID="BXTimeIntervalFilter2" runat="server" Key="LastLoginDate" Text="<%$ Loc:FilterText.DateOfLastAuthorization %>" />
				<bx:BXDropDownFilter ID="BXDropDownFilter1" runat="server" Key="IsApproved" Text="<%$ Loc:FilterText.IsApproved %>" ValueType="Boolean" >
					<asp:ListItem Value="" Text="<%$ Loc:ListItemText.Any %>"></asp:ListItem>
					<asp:ListItem Value="True" Text="<%$ Loc:Kernel.Yes %>"></asp:ListItem>
					<asp:ListItem Value="False" Text="<%$ Loc:Kernel.No %>"></asp:ListItem>
				</bx:BXDropDownFilter>
				<bx:BXDropDownFilter ID="filterSite" runat="server" Key="SiteId" Text="<%$ Loc:FilterText.Site %>"></bx:BXDropDownFilter>
				<bx:BXCustomAdminFilter ID="filterRoles" runat="server" Text="<%$ LocRaw:FilterText.Roles %>" OnOnBuildFilter="Roles_BuildFilter" OnOnLoadState="Roles_LoadState" OnOnReset="Roles_Reset" OnOnSaveState="Roles_SaveState" OnOnInit="Roles_Init" >
					<% MoveSelectedUp(RolesList.Items); %>
					<asp:ListBox runat="server" ID="RolesList" SelectionMode="Multiple" Rows="10" /><br />
					<asp:CheckBox runat="server" ID="IncludeSubRoles" Text="<%$ LocRaw:CheckBoxText.IncludeSubRoles %>" />
				</bx:BXCustomAdminFilter>
			</bx:BXAdminFilter>

			<bx:BXContextMenuToolbar id="AuthUserContextMenuToolbar" runat="server">
				<Items>
					<bx:BXCmImageButton ID="AddButton" CssClass="context-button icon btn_new" CommandName="add"
						Text="<%$ Loc:ActionText.addUser %>" Href="AuthUsersEdit.aspx" />
				</Items>
			</bx:BXContextMenuToolbar>

			<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" />
			<bx:BXMessage ID="successMessage" runat="server" Content="<%$ Loc:Message.RecordsHaveBeenDeletedSuccessfully %>"
				CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False" />

			<bx:BXPopupPanel ID="AuthUserPopupPanel" runat="server">
				<Commands>
					<bx:CommandItem Default="True" IconClass="edit" ItemText="<%$ Loc:Kernel.Edit %>" ItemTitle="<%$ Loc:PopupTitle.ModificationOfUser %>"
						OnClickScript="" UserCommandId="edit" />
					<bx:CommandItem Default="True" IconClass="view" ItemText="<%$ Loc:Kernel.View %>" ItemTitle="<%$ LocRaw:PopupTitle.View %>"
						UserCommandId="view" />
					<bx:CommandItem IconClass="" ItemCommandType="Separator" ItemText="" ItemTitle=""
						OnClickScript="" UserCommandId="" />
					<bx:CommandItem IconClass="delete" ItemText="<%$ Loc:Kernel.Delete %>" ItemTitle="<%$ Loc:PopupTitle.DeleteUser %>"
						OnClickScript="" UserCommandId="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
					<bx:CommandItem IconClass="delete" ItemText="<%$ Loc:PopupText.RemoveUserFromProvider %>" ItemTitle="<%$ Loc:PopupTitle.RemoveUserFromProvider %>"
						OnClickScript="" UserCommandId="deleteProvider" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.DeleteFromProvider %>" />
				</Commands>
			</bx:BXPopupPanel>

			<bx:BXGridView ID="AuthUserGridView" runat="server" 
				ContextMenuToolbarId="AuthUserMultiActionMenuToolbar" 
				PopupCommandMenuId="AuthUserPopupPanel" ContentName="" 
				DataSourceID="AuthUserGridView" 
				AllowPaging="True" AjaxConfiguration-UpdatePanelId="UpdatePanel1" 
				SettingsToolbarId="AuthUserContextMenuToolbar" 
				OnSelect="AuthUserGridView_Select"
				AllowSorting="True" 
				OnSelectCount="AuthUserGridView_SelectCount"
				OnPopupMenuClick="AuthUserGridView_PopupMenuClick" 
				DataKeyNames="UserId"
				OnRowDataBound="AuthUserGridView_RowDataBound"
				OnMultiOperationActionRun="AuthUserGridView_MultiOperationActionRun"
				
                ForeColor="#333333"
                BorderWidth="0px"
                BorderColor="white"
                BorderStyle="none"
                ShowHeader = "true"
                CssClass="list"
                style="font-size: small; font-family: Arial; border-collapse: separate;" 				
				>
				<AjaxConfiguration UpdatePanelId="UpdatePanel1" />
				<Columns>
					<asp:BoundField AccessibleHeaderText="<%$ Loc:ColumnAccessibleHeaderText.Code %>" SortExpression="UserId" DataField="UserId" HeaderText="ID" />
					<asp:BoundField DataField="UserName" SortExpression="UserName" HeaderText="<%$ Loc:FilterText.Login %>" />
					<asp:BoundField DataField="Email" SortExpression="Email" HeaderText="E-Mail" />
					<asp:BoundField DataField="FirstName" SortExpression="FirstName" HeaderText="<%$ Loc:ColumnHeaderText.FirstName %>" />
					<asp:BoundField DataField="SecondName" SortExpression="SecondName" HeaderText="<%$ Loc:ColumnHeaderText.SecondName %>" />
					<asp:BoundField DataField="LastName" SortExpression="LastName" HeaderText="<%$ Loc:FilterText.LastName %>" />
					<asp:BoundField DataField="SiteId" HeaderText="<%$ Loc:FilterText.Site %>" />
					
					<asp:TemplateField HeaderText="<%$ Loc:ColumnHeaderText.Birthdate %>" SortExpression="BirthdayDate">
						<itemtemplate>
							<%# ((DateTime)Eval("BirthdayDate")) != DateTime.MinValue ? ((DateTime)Eval("BirthdayDate")).ToString("d") : ""%>
						</itemtemplate>
						<edititemtemplate>
							<asp:TextBox ID="BirthdayDate" runat="server" Text='<%# Bind("BirthdayDate") %>'></asp:TextBox>&nbsp;<bx:Calendar ID="BirthdayDateCalendar" runat="server" TextBoxId="BirthdayDate" />
						</edititemtemplate>
					</asp:TemplateField>
					
					<asp:TemplateField HeaderText="<%$ Loc:FilterText.IsApproved %>" SortExpression="IsApproved">
						<itemtemplate>
							<%# ((bool)Eval("IsApproved")) ? GetMessageRaw("Kernel.Yes") : GetMessageRaw("Kernel.No")%>
						</itemtemplate>
						<edititemtemplate>
							<asp:CheckBox ID="IsApproved" runat="server" Checked='<%# Bind("IsApproved") %>' />
						</edititemtemplate>
					</asp:TemplateField>
					
					<asp:TemplateField HeaderText="<%$ Loc:ColumnHeaderText.IsLockedout %>" SortExpression="IsLockedOut">
						<itemtemplate>
							<%# ((bool)Eval("IsLockedOut")) ? GetMessageRaw("Kernel.Yes") : GetMessageRaw("Kernel.No")%>
						</itemtemplate>
						<edititemtemplate>
							<asp:CheckBox ID="IsLockedOut" runat="server" Checked='<%# Bind("IsLockedOut") %>' />
						</edititemtemplate>
					</asp:TemplateField>
					
					<asp:BoundField DataField="LastActivityDate" SortExpression="LastActivityDate" HeaderText="<%$ Loc:ColumnHeaderText.DateOfLastVisit %>" />
					<asp:BoundField DataField="CreationDate" SortExpression="CreationDate" HeaderText="<%$ Loc:ColumnHeaderText.DateOfRegistration %>" />
				</Columns>
				
                <pagersettings position="TopAndBottom" pagebuttoncount="3"/>
                <RowStyle BackColor="#FFFFFF"/>
                <AlternatingRowStyle BackColor="#FAFAFA" /> 
                <FooterStyle BackColor="#EAEDF7"/>				
			</bx:BXGridView>

			<bx:BXMultiActionMenuToolbar ID="AuthUserMultiActionMenuToolbar" runat="server" >
				<Items>
					<bx:BXMamImageButton runat="server" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>"  ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>" EnabledCssClass="context-button icon delete"
						DisabledCssClass="context-button icon delete-dis" CommandName="delete"
					/>
					<bx:BXMamListItem ID="BXMamListItem2" runat="server" CommandName="deleteProvider"
						Text="<%$ Loc:PopupText.RemoveUserFromProvider %>" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.DeleteProvider %>"  ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.DeleteProvider %>" />
				</Items>
			</bx:BXMultiActionMenuToolbar>
		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>

