<%@ Control Language="C#" AutoEventWireup="true" CodeFile="advanced_settings.ascx.cs"
    Inherits="BXCustomTypeListAdvancedSettings" %>
<bx:InlineScript runat="server" AsyncMode="ScriptBlock">
<script type="text/javascript">
	function customTypeListAdvancedSettings_validateXmlId(source, args)
	{
		var name = document.getElementById(source.controltovalidate.replace(/XMLID_([0-9]+)$/, 'VALUE_$1'));
		args.IsValid = (args.Value != '' || name.value == '');
	}
	
	function customTypeListAdvancedSettings_validateValue(source, args)
	{
		var xmlId = document.getElementById(source.controltovalidate.replace(/VALUE_([0-9]+)$/, 'XMLID_$1'));
		args.IsValid = (args.Value != '' || xmlId.value == '');
	}
</script>
</bx:InlineScript>
<table cellspacing="0" cellpadding="0" border="0" id="edit2_edit_table" class="edit-table">
    <tbody>
        <tr valign="top">
            <td class="field-name">
                <%= GetMessage("Legend.ListValues") + ":" %></td>
            <td>
                <table cellspacing="0" cellpadding="0" border="0" id="ListValue" class="internal" runat="server">
                    <tbody>
                        <tr class="heading">
                            <td>
                                <%= GetMessage("ID") %></td>
                            <td>
                                <%= GetMessage("XMLID") %><span class="required">*</span></td>
                            <td>
                                <%= GetMessage("Value") %><span class="required">*</span></td>
                            <td>
                                <%= GetMessage("Sort") %></td>
                            <td>
                                <%= GetMessage("Default") %></td>
                            <td>
                                <%= GetMessage("Delete") %></td>
                        </tr>
                        <tr>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                                <%= GetMessage("None") %></td>
                            <td>
                            </td>
                            <td>
                                <%--<asp:RadioButton runat="server" ID="rdNone" Checked="true" GroupName="DefaultGroup" />--%> </td>
                            <td>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </td>
        </tr>
        <tr>
            <td align="right" colspan="2">
                <asp:Button runat="server" ID="AddMore" Text="<%$ Loc:More %>" OnClick="Button1_Click" CausesValidation="false" /></td>
        </tr>
    </tbody>
</table>
<asp:HiddenField runat="server" ID="Q" />