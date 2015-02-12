<%@ Control Language="C#" AutoEventWireup="true" CodeFile="settings.ascx.cs" Inherits="BXCustomTypeIBlockSectionReferenceSettings" %>
<table cellspacing="0" cellpadding="0" border="0" class="edit-table" >
	<tr valign="top">
		<td width="40%" class="field-name">
			<% =GetMessage("TextBoxSize") + ":" %></td>
		<td width="60%">
			<asp:TextBox ID="TextBoxSize" runat="server" Columns="20" MaxLength="255" Text="20" />
			<asp:RangeValidator ID="TextBoxSizeValidator" runat="server" 
				ControlToValidate="TextBoxSize" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic"
				ErrorMessage='<%# GetMessageFormat("Error.FieldRangeInvalid", GetMessage("TextBoxSize"), 1, 255) %>' 
				Type="Integer" MinimumValue="1" MaximumValue="255">*</asp:RangeValidator>
		</td>
	</tr>
	<tr valign="top">
		<td class="field-name">
			<% =GetMessage("IBlockId") + ":" %></td>
		<td>
			<asp:DropDownList ID="ddlIBlock" runat="server">
			</asp:DropDownList>
			<asp:RequiredFieldValidator ID="rfbIBlock" ControlToValidate="ddlIBlock" runat="server" ErrorMessage='<%# GetMessageFormat("Error.Required", GetMessage("IBlockId")) %>'>*</asp:RequiredFieldValidator>
		</td>
	</tr>
</table>
