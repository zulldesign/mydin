<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CustomFieldSetUp.ascx.cs"
    Inherits="bitrix_ui_CustomFieldSetUp" %>
<%@ Register Src="CustomFieldEdit.ascx" TagName="CustomFieldEdit" TagPrefix="uc1" %>
<asp:UpdatePanel runat="server" ID="Fields" ChildrenAsTriggers="false" UpdateMode="Conditional" RenderMode="Inline" >
<ContentTemplate>
<style type="text/css" >
	table.bx-custom-field-setup td
	{
		white-space: nowrap;
	}
</style>
<table cellspacing="0" cellpadding="0" border="0" class="edit-table bx-custom-field-setup" runat="server"
    id="CustomFields">
    <tbody>
        <tr class="heading">
            <td>
                <%= GetMessage("ID") %></td>
            <td>
                <%= GetMessage("Code") %><span class="required">*</span></td>
            <td>
                <%= GetMessage("FieldTitle") %><span class="required">*</span></td>
            <td>
                <%= GetMessage("Active") %></td>
            <td>
                <%= GetMessage("Type") %></td>
            <td>
                <%= GetMessage("Multiple") %></td>
            <td>
                <%= GetMessage("Sort") %></td>
            <td>
                <%= GetMessage("Change") %></td>
            <td runat="server" id="DeleteHeader" >
                <%= GetMessage("Delete") %></td>
        </tr>
    </tbody>
</table>
</ContentTemplate>
</asp:UpdatePanel>
<asp:Placeholder runat="server" ID="DP" />
<bx:InlineScript runat="server" AsyncMode="ScriptBlock" >
<script type="text/javascript">
	function customFieldSetUp_validateCode(source, args)
	{
		var codeId = source.controltovalidate;
		var nameId = codeId.replace(/_CODES_/, '_NAME_');
		var name = document.getElementById(nameId);
		args.IsValid = name.value == '' || args.Value != '';
	}
	function customFieldSetUp_validateName(source, args)
	{
		var nameId = source.controltovalidate;
		var codeId = nameId.replace(/_NAME_/, '_CODES_');
		var code = document.getElementById(codeId);
		args.IsValid = (code && code.value == '') || args.Value != '';
	}
</script>
</bx:InlineScript>
