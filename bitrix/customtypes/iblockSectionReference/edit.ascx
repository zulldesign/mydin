<%@ Control Language="C#" AutoEventWireup="true" CodeFile="edit.ascx.cs" Inherits="BXCustomTypeIBlockSectionReferenceEdit" %>
<asp:ListBox ID="ValueList" runat="server"></asp:ListBox>
<asp:RequiredFieldValidator ID="ValueRequired" runat="server" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic" ControlToValidate="ValueList" >*</asp:RequiredFieldValidator>
