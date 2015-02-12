<%@ Control Language="C#" AutoEventWireup="true" CodeFile="settings.ascx.cs" Inherits="BXCustomTypeIntSettings" %>
<bx:InlineScript ID="InlineScript1" runat="server" AsyncMode="ScriptBlock">
<script type="text/javascript">
function <%= DefaultValueConditions.ClientValidationFunction %>(source, args)
{
	args.IsValid=true;
	
	if(isNaN(args.Value))
		return;
	var i = parseInt(args.Value);
		
	var tb = document.getElementById('<%= MinValue.ClientID %>');
	if(tb && !isNaN(tb.value))
	{
		var j = parseInt(tb.value);
		if (i < j)
			return (args.IsValid = false);
	}

	var tb = document.getElementById('<%= MaxValue.ClientID %>');
	if(tb && !isNaN(tb.value))
	{
		var j = parseInt(tb.value);
		if (i > j)
			return (args.IsValid = false);
	}
}
</script>
</bx:InlineScript>
<table cellspacing="0" cellpadding="0" border="0" class="edit-table" >
	<tr valign="top">
		<td class="field-name" width="40%">
			<% =GetMessage("DefaultValue") + ":" %></td>
		<td style="width: 176px">
			<asp:TextBox ID="DefaultValue" runat="server" Columns="20" MaxLength="255" />
			<asp:RangeValidator ID="DefaultValueValidator" runat="server" 
				ControlToValidate="DefaultValue" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic"
				ErrorMessage='<%# GetMessageFormat("Error.FieldIsNotInt", GetMessage("DefaultValue")) %>' 
				Type="Integer" MinimumValue="<%# int.MinValue %>" MaximumValue="<%# int.MaxValue %>" >*</asp:RangeValidator>
			<asp:CustomValidator ID="DefaultValueConditions" runat="server" 
				ControlToValidate="DefaultValue" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic"
				ErrorMessage='<%# GetMessageFormat("Error.DefaultRangeInvalid", GetMessage("DefaultValue")) %>' 
				OnServerValidate="DefaultValueConditions_ServerValidate" ValidateEmptyText="True" >*</asp:CustomValidator>
		</td>
	</tr>
	<tr valign="top">
		<td class="field-name">
			<% =GetMessage("TextBoxSize") + ":" %></td>
		<td style="width: 176px">
			<asp:TextBox ID="TextBoxSize" runat="server" Columns="20" MaxLength="255" Text="20" />
			<asp:RangeValidator ID="TextBoxSizeValidator" runat="server" 
				ControlToValidate="TextBoxSize" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic"
				ErrorMessage='<%# GetMessageFormat("Error.FieldRangeInvalid", GetMessage("TextBoxSize"), 1, 255) %>' 
				Type="Integer" MinimumValue="1" MaximumValue="255">*</asp:RangeValidator>
		</td>
	</tr>
	<tr valign="top">
		<td class="field-name">
			<% =GetMessage("MinValue") + ":" %></td>
		<td style="width: 176px">
			<asp:TextBox ID="MinValue" runat="server" Columns="20" MaxLength="255" />
			<asp:RangeValidator ID="MinValueValidator" runat="server" 
				ControlToValidate="MinValue" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic"
				ErrorMessage='<%# GetMessageFormat("Error.FieldIsNotInt", GetMessage("MinValue")) %>' 
				Type="Integer" MinimumValue="<%# int.MinValue %>" MaximumValue="<%# int.MaxValue %>" >*</asp:RangeValidator>
		</td>
	</tr>
	<tr valign="top">
		<td class="field-name">
			<% =GetMessage("MaxValue") + ":" %></td>
		<td style="width: 176px">
			<asp:TextBox ID="MaxValue" runat="server" Columns="20" MaxLength="255" />
			<asp:RangeValidator ID="MaxValueValidator" runat="server" 
				ControlToValidate="MaxValue" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic"
				ErrorMessage='<%# GetMessageFormat("Error.FieldIsNotInt", GetMessage("MaxValue")) %>' 
				Type="Integer" MinimumValue="<%# int.MinValue %>" MaximumValue="<%# int.MaxValue %>" >*</asp:RangeValidator>
		</td>
	</tr>
</table>
