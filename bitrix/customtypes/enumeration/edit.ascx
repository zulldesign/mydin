<%@ Control Language="C#" AutoEventWireup="true" CodeFile="edit.ascx.cs" Inherits="BXCustomTypeEnumerationEdit" EnableViewState="false" %>
<asp:MultiView ID="View" runat="server">
    <asp:View ID="View1" runat="server">
        <asp:ListBox runat="server" ID="List" DataTextField="Text" DataValueField="Value" SelectionMode="Multiple" />
        <asp:RequiredFieldValidator runat="server" ID="valList" ErrorMessage="<%$ Loc:RequiredError %>" ControlToValidate="List" InitialValue="" >*</asp:RequiredFieldValidator>
    </asp:View>
    <asp:View ID="View2" runat="server">
        <asp:RadioButtonList   cellpadding="0" cellspacing="0" style="display:inline"  runat="server" ID="Flag" DataTextField="Text" DataValueField="Value" ></asp:RadioButtonList>
        <asp:RequiredFieldValidator runat="server" ID="valFlag" ErrorMessage="<%$ Loc:RequiredError %>" ControlToValidate="Flag" >*</asp:RequiredFieldValidator>
    </asp:View>
    <asp:View ID="View3" runat="server">
		<asp:CheckBoxList ID="ChBox" cellpadding="0" cellspacing="0" style="display:inline"  runat="server" DataTextField="Text" DataValueField="Value"></asp:CheckBoxList>
		<asp:CustomValidator ErrorMessage="<%$ Loc:RequiredError %>" runat="server" ID="valCheckbox" OnServerValidate="ChBox_ServerValidate">*</asp:CustomValidator>
    </asp:View>
    <asp:View ID="View4" runat="server">
		<asp:DropDownList runat="server" ID="DDList" DataTextField="Text" DataValueField="Value"/>
		<asp:RequiredFieldValidator runat="server" ID="valDDList" ErrorMessage="<%$ Loc:RequiredError %>" ControlToValidate="DDList" InitialValue="" >*</asp:RequiredFieldValidator>
    </asp:View>
</asp:MultiView>