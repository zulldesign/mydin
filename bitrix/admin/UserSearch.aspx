<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminSimpleMasterPage.master"
	AutoEventWireup="true" CodeFile="UserSearch.aspx.cs" Inherits="bitrix_admin_UserSearch"
	Title="<%$ LocRaw:lsUserSearch %>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<asp:UpdatePanel runat="server" ID="UP" >
		<ContentTemplate>
			<bx:BXAdminFilter ID="BXAdminFilter1" runat="server" AssociatedGridView="GridView1">
				<bx:BXBetweenFilter ID="BXBetweenFilter1" runat="server" Key="UserId" Text="ID" ValueType="Integer" />
                <bx:BXDropDownFilter ID="BXDropDownFilter1" runat="server" Key="IsApproved" Text="<%$ LocRaw:lsActive %>" ValueType="Boolean" >
					<asp:ListItem Value="" Text="<%$ LocRaw:lsAny %>"></asp:ListItem>
					<asp:ListItem Value="True" Text="<%$ LocRaw:Kernel.Yes %>"></asp:ListItem>
					<asp:ListItem Value="False" Text="<%$ LocRaw:Kernel.No %>"></asp:ListItem>
				</bx:BXDropDownFilter>
                <bx:BXTextBoxStringFilter ID="BXTextBoxStringFilter1" runat="server" Key="UserName" Text="<%$ LocRaw:lsLogin %>" Visibility="AlwaysVisible"/>
                <bx:BXTextBoxStringFilter ID="BXTextBoxStringFilter4" runat="server" Key="FirstName" Text="<%$ LocRaw:lsFirstName %>"/>
				<bx:BXTextBoxStringFilter ID="BXTextBoxStringFilter2" runat="server" Key="LastName" Text="<%$ LocRaw:lsLastName %>"/>
				<bx:BXTextBoxStringFilter ID="BXTextBoxStringFilter3" runat="server" Key="Email" Text="EMail"/>
				<bx:BXTimeIntervalFilter ID="BXTimeIntervalFilter2" runat="server" Key="LastLoginDate" Text="<%$ LocRaw:lsDateAuthorization %>" />
			</bx:BXAdminFilter>
			
			<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" runat="server" />
			<br />
            <asp:HiddenField ID="hfUserId" runat="server" />
			<asp:HiddenField ID="hfCallback" runat="server" />
			<bx:BXGridView ID="GridView1" runat="server" ContextMenuToolbarId="MultiActionMenuToolbar1"
				PopupCommandMenuId="PopupPanel1" ContentName="" DataSourceID="GridView1" AllowPaging="True"
				AjaxConfiguration-UpdatePanelId="UpdatePanel1" SettingsToolbarId="BXContextMenuToolbar1"
				DataKeyNames="ID" OnSelect="GridView1_Select" AllowSorting="True" OnSelectCount="GridView1_SelectCount"
				OnRowDataBound="GridView1_RowDataBound" AutoSelectField="False">
				<AjaxConfiguration UpdatePanelId="UP" />
				<Columns>
					<asp:BoundField DataField="ID" SortExpression="ID" HeaderText="ID" HtmlEncode="False" />
					<asp:BoundField DataField="Active" HeaderText="<%$ LocRaw:lsActive %>" SortExpression="Active" />
					<asp:BoundField DataField="Login" HeaderText="<%$ LocRaw:lsLogin %>" SortExpression="Login" />
					<asp:BoundField DataField="FirstName" HeaderText="<%$ LocRaw:lsFirstName %>" SortExpression="FirstName" />
					<asp:BoundField DataField="LastName" HeaderText="<%$ LocRaw:lsLastName %>" SortExpression="LastName" />
					<asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
					<asp:BoundField DataField="DateActive" HeaderText="<%$ LocRaw:lsLastAuthorization %>" SortExpression="DateActive" />
				</Columns>
			</bx:BXGridView>
			<bx:BXMultiActionMenuToolbar ID="MultiActionMenuToolbar1" runat="server" Visible="false" />
			<bx:BXPopupPanel ID="PopupPanel1" runat="server">
				<Commands>
					<bx:CommandItem Default="True" IconClass="select" ItemText="<%$ LocRaw:lsSelect %>"
						ItemTitle="<%$ LocRaw:lsSelect %>" UserCommandId="edit" OnClickScript="SelEl(UserData); return false;" />
				</Commands>
			</bx:BXPopupPanel>
		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>
