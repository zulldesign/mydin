<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="Modules.aspx.cs" Inherits="bitrix_admin_Modules" Title="Untitled Page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<bx:BXGridView ID="GridView" runat="server" ContentName="<%$ Loc:TableTitleModules %>"
		DataKeyNames="Id" PopupCommandMenuId="PopupMenu" OnRowDataBound="GridView_RowDataBound"
		AutoSelectField="false" OnPopupMenuClick="GridView_PopupMenuClick" ForeColor="#333333"
		BorderWidth="0px" BorderColor="white" BorderStyle="none" ShowHeader="true" CssClass="list"
		Style="font-size: small; font-family: Arial; border-collapse: separate;" >
		<PagerSettings Position="TopAndBottom" PageButtonCount="3" />
		<RowStyle BackColor="#FFFFFF" />
		<AlternatingRowStyle BackColor="#FAFAFA" />
		<FooterStyle BackColor="#EAEDF7" />
		<Columns>
			<asp:BoundField ReadOnly="True" DataField="NameHtml" HeaderText="<%$ Loc:ColumnHeaderText.ModuleName %>" HtmlEncode="false" />
			<asp:BoundField ReadOnly="True" DataField="Version" HeaderText="<%$ Loc:ColumnHeaderText.Version %>" />
			<asp:BoundField ReadOnly="True" DataField="Description" HeaderText="<%$ Loc:ColumnHeaderText.Description %>" />
			<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Status %>" ItemStyle-Wrap="False" >
				<itemtemplate>
					<%# (bool)Eval("IsInstalled") ? GetMessageRaw("Status.Installed") : string.Format("<font style=\"color: red;\">{0}</font>", GetMessageRaw("Status.NotInstalled"))  %>
			    </itemtemplate>
			</asp:TemplateField>
		</Columns>
	</bx:BXGridView>
	<bx:BXPopupPanel ID="PopupMenu" runat="server">
		<Commands>
			<bx:CommandItem UserCommandId="nop" ItemText="<%$ LocRaw:PopupText.NoOperations %>" ItemTitle="<%$ LocRaw:PopupTitle.NoOperations %>"
				OnClickScript="return false" />
			<bx:CommandItem Default="True" IconClass="new" UserCommandId="install" ItemText="<%$ Loc:PopupText.Install %>"
				ItemTitle="<%$ Loc:PopupText.Install %>" />
			<bx:CommandItem Default="True" IconClass="delete" UserCommandId="uninstall" ItemText="<%$ Loc:PopupText.Uninstall %>"
				ItemTitle="<%$ Loc:PopupText.Uninstall %>" />
		</Commands>
	</bx:BXPopupPanel>
</asp:Content>
