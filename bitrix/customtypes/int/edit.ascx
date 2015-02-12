<%@ Control Language="C#" AutoEventWireup="true" CodeFile="edit.ascx.cs" Inherits="BXCustomTypeIntEdit" %>
<asp:TextBox ID="ValueTextBox" runat="server" />
<asp:RequiredFieldValidator ID="ValueRequired" runat="server" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic" ControlToValidate="ValueTextBox" >*</asp:RequiredFieldValidator>
<asp:RangeValidator ID="ValueRange" runat="server" Type="Integer" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic" ControlToValidate="ValueTextBox" >*</asp:RangeValidator>