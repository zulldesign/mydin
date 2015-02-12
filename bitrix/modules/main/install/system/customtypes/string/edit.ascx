<%@ Control Language="C#" AutoEventWireup="true" CodeFile="edit.ascx.cs" Inherits="BXCustomTypeStringEdit" %>
<asp:TextBox ID="ValueTextBox" runat="server" />
<asp:RequiredFieldValidator ID="ValueRequired" runat="server" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic" ControlToValidate="ValueTextBox" >*</asp:RequiredFieldValidator>
<asp:RegularExpressionValidator ID="ValueRegex" runat="server" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic" ControlToValidate="ValueTextBox" >*</asp:RegularExpressionValidator>
<asp:CustomValidator ID="ValueMinLength" runat="server"  ValidationGroup="<%# ValidationGroup %>" Display="Dynamic" ControlToValidate="ValueTextBox" OnServerValidate="ValueMinLength_ServerValidate" >*</asp:CustomValidator>
<asp:CustomValidator ID="ValueMaxLength" runat="server"  ValidationGroup="<%# ValidationGroup %>" Display="Dynamic" ControlToValidate="ValueTextBox" OnServerValidate="ValueMaxLength_ServerValidate" >*</asp:CustomValidator>