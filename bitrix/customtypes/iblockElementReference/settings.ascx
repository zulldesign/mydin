<%@ Control Language="C#" AutoEventWireup="true" CodeFile="settings.ascx.cs" Inherits="BXCustomTypeIBlockElementReferenceSettings" %>
<table cellspacing="0" cellpadding="0" border="0" class="edit-table" >
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
