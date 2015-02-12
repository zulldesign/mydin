<%@ Control Language="C#" AutoEventWireup="true" CodeFile="settings.ascx.cs" Inherits="BXCustomTypeBooleanSettings" %>
<table cellspacing="0" cellpadding="0" border="0" class="edit-table">
	<tr valign="top">
		<td class="field-name" width="40%">
			<% =GetMessage("DefaultValue") + ":" %></td>
		<td  width="60%">
			<asp:DropDownList runat="server" ID="DefaultValue">
			    <asp:ListItem Selected="True" Text="<%$ Loc:Yes %>"></asp:ListItem>
			    <asp:ListItem Text="<%$ Loc:No %>"></asp:ListItem>
			</asp:DropDownList>
		</td>
	</tr>
	<tr valign="top">
	    <td class="field-name" width="40%">
			<% =GetMessage("View") + ":" %></td>
		<td>
            <asp:RadioButton runat="server" ID="rbCheckbox" GroupName="DefaultDateTime" Text="<%$ Loc:CheckBox %>"
                Checked="True" /><br />
            <asp:RadioButton runat="server" ID="rbRadiobuttons" GroupName="DefaultDateTime" Text="<%$ Loc:Radiobuttons %>" /><br />
            <asp:RadioButton runat="server" ID="rbDropDown" GroupName="DefaultDateTime" Text="<%$ Loc:DropDown %>" />
        </td>
	</tr>
</table>
