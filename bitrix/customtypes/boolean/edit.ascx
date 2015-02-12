<%@ Control Language="C#" AutoEventWireup="true" CodeFile="edit.ascx.cs" Inherits="BXCustomTypeBooleanEdit" %>
<asp:MultiView ID="MultiView1" runat="server">
	<asp:View ID="View1" runat="server">
		<asp:CheckBox runat="server" ID="chValue" /></asp:View>
	<asp:View ID="View2" runat="server">
		<asp:RadioButton runat="server" ID="Yes" Text="<%$ Loc:Yes %>" GroupName="DefaultGroup">
		</asp:RadioButton><br />
		<asp:RadioButton runat="server" ID="No" Text="<%$ Loc:No %>" GroupName="DefaultGroup">
		</asp:RadioButton></asp:View>
	<asp:View ID="View3" runat="server">
		<asp:DropDownList ID="ddValue" runat="server">
			<asp:ListItem Text="<%$ Loc:Yes %>"></asp:ListItem>
			<asp:ListItem Text="<%$ Loc:No %>"></asp:ListItem>
		</asp:DropDownList></asp:View>
</asp:MultiView>