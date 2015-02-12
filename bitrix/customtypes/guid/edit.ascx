<%@ Control Language="C#" AutoEventWireup="true" CodeFile="edit.ascx.cs" Inherits="BXCustomTypeGuidEdit" %>
<asp:TextBox ID="ValueTextBox" runat="server" Columns="38" MaxLength="38" />
<asp:RequiredFieldValidator 
	ID="ValueRequired" 
	runat="server" 
	ValidationGroup="<%# ValidationGroup %>" 
	Display="Dynamic"	
	ControlToValidate="ValueTextBox" 
>*</asp:RequiredFieldValidator><asp:RegularExpressionValidator 
	ID="ValueMask" 
	runat="server" 
	ValidationExpression="^\s*{?[0-9a-fA-F]{8}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{12}}?\s*$"
	ValidationGroup="<%# ValidationGroup %>" 
	Display="Dynamic" 
	ControlToValidate="ValueTextBox" 
>*</asp:RegularExpressionValidator>