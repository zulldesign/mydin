<%@ Control Language="C#" AutoEventWireup="true" CodeFile="settings.ascx.cs" Inherits="BXCustomTypeDateSettings" %>
<%@ Register Src="~/bitrix/controls/Main/Calendar.ascx" TagName="Calendar"
	TagPrefix="bx" %>
<table cellspacing="0" cellpadding="0" border="0" class="edit-table">
	<tr valign="top">
		<td class="field-name">
			<% =GetMessage("DefaultValue") + ":" %>
		</td>
		<td>
			<asp:RadioButton runat="server" ID="rbNone" GroupName="DefaultDateTime" Text="<%$ Loc:No %>"
				Checked="True" /><br />
			<asp:RadioButton runat="server" ID="rbCurrent" GroupName="DefaultDateTime" Text="<%$ Loc:CurrentTime %>" /><br />
			<asp:RadioButton runat="server" ID="rbCustom" GroupName="DefaultDateTime" />
			<asp:TextBox ID="txtDate" runat="server">
			</asp:TextBox>
			<bx:Calendar ID="Calendar1" runat="server" TextBoxId="txtDate" />
		</td>
	</tr>
	
	<tr valign="top">
		<td class="field-name">
			<%= GetMessage("ShowTime") %>
		</td>
		<td>
			<asp:CheckBox runat="server" ID="showTime" />
		</td>
	</tr>
</table>
