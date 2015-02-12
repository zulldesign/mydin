<%@ Control Language="C#" AutoEventWireup="true" CodeFile="InprocSessionConfigurator.ascx.cs" Inherits="Bitrix.Main.UI.InprocSessionConfigurator" %>
<table border="0" cellpadding="0" cellspacing="0" style="width:100%" class="edit-table">
	<% if (isSet) { %>
	<tr valign="top">
		<td class="field-name" width="40%">&nbsp;</td>
		<td width="60%"><b><%= GetMessageRaw("IsActive") %></b></td>
	</tr>
	<% } %>
	<tr valign="top">
		<td class="field-name" width="40%">&nbsp;</td>
		<td width="60%">
			<% Install.Text = !isSet ? GetMessageRaw("Button.Activate") : GetMessageRaw("Button.Reactivate"); %>
			<asp:Button runat="server" ID="Install" Text="" 
				onclick="Install_Click" />			
		</td>
	</tr>
</table>