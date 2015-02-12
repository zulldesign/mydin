<%@ Control Language="C#" AutoEventWireup="true" CodeFile="edit.ascx.cs" Inherits="BXCustomTypeDateEdit" %>
<%@ Register Src="~/bitrix/controls/Main/Calendar.ascx" TagName="Calendar" TagPrefix="bx" %>
<asp:TextBox ID="txtDate" runat="server">
</asp:TextBox>
<asp:RequiredFieldValidator runat="server" ID="valDate" ErrorMessage="<%$ Loc:Error %>" Display="Dynamic"  ControlToValidate="txtDate">*</asp:RequiredFieldValidator>
<bx:Calendar ID="Calendar1" runat="server" TextBoxId="txtDate"/>