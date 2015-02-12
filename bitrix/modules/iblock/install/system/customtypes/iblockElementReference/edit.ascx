<%@ Control Language="C#" AutoEventWireup="true" CodeFile="edit.ascx.cs" Inherits="BXCustomTypeIBlockElementReferenceEdit" %>

<asp:TextBox ID="tbValue" Width="60" runat="server"></asp:TextBox>
<input id="bSearch" runat="server" type="button" value="..." onclick="" />
<asp:Label ID="lbName" runat="server" Text=""></asp:Label>
<asp:RequiredFieldValidator ID="ValueRequired" runat="server" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic" ControlToValidate="tbValue" >*</asp:RequiredFieldValidator>
