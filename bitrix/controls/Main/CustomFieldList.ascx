<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CustomFieldList.ascx.cs"
    Inherits="bitrix_ui_CustomFieldList" %>
<table cellspacing="0" cellpadding="0" border="0" class="edit-table" runat="server"
    id="CustomFields">
    <tbody>
        <tr colspan="2">
            <td align="left">
                <a href="CustomFieldEdit.aspx?EntityId=<%= this.EntityId %>&<% =Bitrix.Configuration.BXConfigurationUtility.Constants.BackUrl + "=" + HttpUtility.UrlEncode(Request.RawUrl)%>">
                    <%= GetMessage("AddCustomField") %>
                </a>
            </td>
        </tr>
    </tbody>
</table>
