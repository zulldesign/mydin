<%@ Control Language="C#" AutoEventWireup="true" CodeFile="edit.ascx.cs" Inherits="BXCustomTypeDoubleEdit" %>
<asp:TextBox ID="ValueTextBox" runat="server" />
<asp:RequiredFieldValidator ID="ValueRequired" runat="server" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic" ControlToValidate="ValueTextBox" >*</asp:RequiredFieldValidator>
<asp:CompareValidator ID="ValueType" runat="server" Type="Double" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic" ControlToValidate="ValueTextBox" Operator="DataTypeCheck" >*</asp:CompareValidator>
<asp:CompareValidator ID="ValueMin" runat="server" Type="Double" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic" ControlToValidate="ValueTextBox" Operator="GreaterThanEqual" >*</asp:CompareValidator>
<asp:CompareValidator ID="ValueMax" runat="server" Type="Double" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic" ControlToValidate="ValueTextBox" Operator="LessThanEqual" >*</asp:CompareValidator>
