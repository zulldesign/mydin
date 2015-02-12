<%@ Control Language="C#" AutoEventWireup="true" CodeFile="advanced_settings.ascx.cs"
    Inherits="BXCustomTypeEnumerationAdvancedSettings" %>
<%@ Import Namespace="Bitrix.Services.Js" %>
<bx:InlineScript ID="Script1" runat="server" AsyncMode="ScriptBlock" SyncMode="ScriptBlock">
<script type="text/javascript">
	function <%= ClientID %>_Validate(sender, args)
	{
		args.IsValid = true;
		
		var table = document.getElementById('<%= ClientID %>_Values');
		var start = <%= MultipleBehavior ? 1 : 2 %>;
		
		
		for (var i = start; i < table.rows.length; i++)
		{
			var row = table.rows[i];
			var id = row.cells[0].getElementsByTagName('INPUT')[0].value;
			var isNew = id.match(/^@\d+$/);
			
			var title = row.cells[1].getElementsByTagName('INPUT')[0].value;
			var xmlId = row.cells[2].getElementsByTagName('INPUT')[0].value;
			var checks = row.cells[5].getElementsByTagName('INPUT');
			var asterisk = row.cells[1].getElementsByTagName('SPAN')[0];
			
			var valid = true;
			
			if (!isNew && title == '' && !checks[0].checked)
				valid = false;
			
			if (isNew && title == '' && xmlId != '')
				valid = false;
				
			asterisk.style.display = valid ? 'none' : '';
			
			if (!valid)
				args.IsValid = false;	
		}
	}
</script>
</bx:InlineScript>
<table cellspacing="0" cellpadding="0" border="0" class="edit-table">
    <tr valign="top">
        <td class="field-name"><%= GetMessage("Legend.ListValues") + ":" %></td>
        <td>
			<% OptimizeState(true); %>
            <table id="<%= ClientID %>_Values" cellspacing="0" cellpadding="0" border="0" class="internal" >
                <tr class="heading">
                    <td>ID</td>
                    <td><%= GetMessage("ColumnText.Title") %><span class="required">*</span></td>
                    <td>XML ID</td>
                    <td><%= GetMessage("ColumnText.Sort") %></td>
                    <td><%= GetMessage("ColumnText.Default") %></td>
                    <td><%= GetMessage("ColumnText.Delete") %></td>
                </tr>
                <% if (!MultipleBehavior) { %>
                <tr>
                    <td>&nbsp;</td>
                    <td><%= GetMessage("None") %></td>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                    <td align="center" ><input type="radio" name="<%= Encode(DefaultFormName) %>" value="" <%= noDefault ? @"checked=""checked""" : string.Empty %> /></td>
                    <td>&nbsp;</td>
                </tr>
                <% } %>
                <% foreach(EnumerationItem item in state) { %>
                <tr>
                    <td><%= item.IsNew ? string.Empty : item.Id.ToString() %><input type="hidden" name="<%= Encode(IdFormName) %>" value="<%= Encode(item.IdValue) %>" /></td>
                    <td><input type="text" name="<%= Encode(TitleFormPrefix + item.IdValue) %>" value="<%= Encode(item.Title) %>" /><span class="required" style="<%= item.IsValid ? "display: none" : string.Empty %>">*</span></td>
                    <td><input type="text" name="<%= Encode(XmlIdFormPrefix + item.IdValue) %>" value="<%= Encode(item.XmlId) %>" /></td>
                    <td><input type="text" size="10" name="<%= Encode(SortFormPrefix + item.IdValue) %>" value="<%= item.Sort %>" /></td>
                    <td align="center" ><input type="<%= MultipleBehavior ? "checkbox" : "radio" %>" name="<%= Encode(DefaultFormName) %>" value="<%= item.IdValue %>" <%= item.IsDefault ? @"checked=""checked""" : string.Empty %> /></td>
                    <td <%= item.IsNew ? string.Empty : "align=\"center\"" %> ><% if (!item.IsNew) { %><input type="checkbox" name="<%= Encode(DeleteFormName) %>" value="<%= item.IdValue %>" /><% } else { %>&nbsp;<% } %></td>
                </tr>
                <% } %>
			</table>
		</td>
	</tr>
    <tr>
		<td align="right" colspan="2"><input type="button" id="<%= ClientID %>_More" value="<%= GetMessage("More") %>" /></td>
	</tr>
</table>
<asp:CustomValidator ID="Validator" runat="server" OnServerValidate="Validator_ServerValidate" ErrorMessage="<%$ Loc:Message.ItemNameRequired %>" Display="None" /> 
<bx:InlineScript ID="Script2" runat="server" AsyncMode="Startup" SyncMode="Startup">
<script type="text/javascript">
	document.getElementById('<%= ClientID %>_More').onclick = function()
	{
		var table = document.getElementById('<%= ClientID %>_Values');
		
		var lastId = table.rows[table.rows.length - 1].cells[0].getElementsByTagName('INPUT')[0].value;
		var id = '@' + ((lastId.match(/^@\d+$/)) ? (parseInt(lastId.substring(1)) + 1) : 0);
		
		var row = table.insertRow(-1);
		var cell;
		
		row.insertCell(-1).innerHTML = '<input type="hidden" name="<%= BXJSUtility.JSHEncode(IdFormName) %>" value="' + id + '" />';
		
		row.insertCell(-1).innerHTML = '<input type="text" name="<%= BXJSUtility.JSHEncode(TitleFormPrefix) %>' + id + '" value="" /><span class="required" style="display: none">*</span>';
		
		row.insertCell(-1).innerHTML = '<input type="text" name="<%= BXJSUtility.JSHEncode(XmlIdFormPrefix) %>' + id + '" value="" />';
        
        row.insertCell(-1).innerHTML = '<input type="text" size="10" name="<%= BXJSUtility.JSHEncode(SortFormPrefix) %>' + id + '" value="100" />';
                    
        cell = row.insertCell(-1);          
        cell.align = 'center';
        cell.innerHTML = '<input type="<%= MultipleBehavior ? "checkbox" : "radio" %>" name="<%= BXJSUtility.JSHEncode(DefaultFormName) %>" value="' + id + '" />';
        
        row.insertCell(-1);          
	}
</script>
</bx:InlineScript>

                
            