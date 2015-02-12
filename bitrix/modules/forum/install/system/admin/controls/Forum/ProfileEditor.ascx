<%@ Control Language="C#" AutoEventWireup="false" CodeFile="ProfileEditor.ascx.cs"
	Inherits="bitrix_admin_controls_Forum_ProfileEditor" EnableViewState="false" %>
<table class="edit-table" cellspacing="0" cellpadding="0" border="0">
	<tr class="heading">
		<td align="center" width="0%" colspan="2"><%= GetMessage("Heading.Forum") %></td>
	</tr>
	<tr valign="top">
		<td class="field-name" width="40%"><%= GetMessage("Label.Posts") %>:</td>
		<td width="60%"><asp:Literal runat="server" ID="Posts" Mode="Encode" Text="0" /></td>
	</tr>
	<tr valign="top">
		<td class="field-name" width="40%"><%= GetMessage("Label.Signature") %>:</td>
		<td width="60%"><asp:TextBox  runat="server" ID="Signature" TextMode="MultiLine" Columns="50" Rows="5" /></td>
	</tr>
	<tr valign="top">
		<td class="field-name" width="40%"></td>
		<td width="60%"><asp:CheckBox ID="OwnPostNotification" runat="server"/><label for="<%= OwnPostNotification.ClientID%>"><%= Bitrix.Forum.BXForumModule.GetMessage("UserProfile", "OwnPostNotification", true) %></label></td>
	</tr>
</table>
