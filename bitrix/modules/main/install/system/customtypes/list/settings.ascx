<%@ Control Language="C#" AutoEventWireup="true" CodeFile="settings.ascx.cs" Inherits="BXCustomTypeListSettings" %>
<table cellspacing="0" cellpadding="0" border="0" class="edit-table">
	<tr valign="top">
		<td class="field-name" width="40%">
			<% =GetMessage("View") + ":" %></td>
		<td  width="60%">
		    <asp:RadioButton runat="server" ID="rbList" Text="<%$ Loc:ListView %>" GroupName="DefaultGroup" Checked="true"/><br />
		    <asp:RadioButton runat="server" ID="rbFlags" Text="<%$ Loc:RadioButtonView %>" GroupName="DefaultGroup" />
		</td>
	</tr>
	<tr valign="top">
		<td class="field-name">
			<% =GetMessage("ListSize") + ":" %></td>
		<td>
			<asp:TextBox ID="ListSize" runat="server" Columns="10" MaxLength="10" Text="5"/>
		</td>
	</tr>
</table>
