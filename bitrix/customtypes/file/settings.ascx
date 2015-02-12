<%@ Control Language="C#" AutoEventWireup="true" CodeFile="settings.ascx.cs" Inherits="BXCustomTypeFileSettings" %>
<table cellspacing="0" cellpadding="0" border="0" class="edit-table">
	<tr valign="top">
		<td class="field-name" width="40%">
			<% =GetMessage("TextBoxSize") + ":" %></td>
		<td width="60%">
			<asp:TextBox ID="TextBoxSize" runat="server" Columns="20" MaxLength="255" >20</asp:TextBox>
			<asp:RangeValidator ID="TextBoxSizeValidator" runat="server" 
				ControlToValidate="TextBoxSize" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic"
				ErrorMessage='<%# GetMessageFormat("Error.FieldRangeInvalid", GetMessage("TextBoxSize"), 1, 255) %>' 
				Type="Integer" MinimumValue="1" MaximumValue="255">*</asp:RangeValidator>
		</td>
	</tr>
	<tr valign="top">
		<td class="field-name">
			<% =GetMessage("MaxSize") + ":" %></td>
		<td>
			<asp:TextBox ID="MaxSize" runat="server" Columns="20" MaxLength="255" />
			<asp:RangeValidator ID="MaxSizeValidator" runat="server" 
				ControlToValidate="MaxSize" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic"
				ErrorMessage='<%# GetMessageFormat("Error.SizeInvalid", GetMessage("MaxSize")) %>' 
				Type="Integer" MinimumValue="1" MaximumValue="<%# int.MaxValue.ToString() %>">*</asp:RangeValidator>
		</td>
	</tr>
	<tr valign="top">
		<td class="field-name">
			<% =GetMessage("AllowedExtensions") + ":" %></td>
		<td>
			<asp:TextBox ID="AllowedExtensions" runat="server" Columns="20" MaxLength="255" />
		</td>
	</tr>
	<tr valign="top">
		<td class="field-name"><%= GetMessage("AddDescription") %>:</td>
		<td>
			<asp:CheckBox ID="AddDescription" runat="server" />
		</td>
	</tr>
</table>
