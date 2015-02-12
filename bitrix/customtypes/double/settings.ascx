<%@ Control Language="C#" AutoEventWireup="true" CodeFile="settings.ascx.cs" Inherits="BXCustomTypeDoubleSettings" %>
<%@ Import Namespace="System.Globalization" %>
<%--<bx:InlineScript ID="InlineScript1" runat="server" AsyncMode="ScriptBlock">
<script type="text/javascript">
function <%= DefaultValueConditions.ClientValidationFunction %>(source, args)
{
	args.IsValid=true;
	alert(args.Value);
	
	if(isNaN(args.Value))
		return;
	var i = parseFloat(args.Value);
		
	var tb = document.getElementById('<%= MinValue.ClientID %>');
	alert(tb.value);
	if(tb && !isNaN(tb.value))
	{
		var j = parseFloat(tb.value);
		 alert(j);
		if (i < j)
			return (args.IsValid = false);
	}

	var tb = document.getElementById('<%= MaxValue.ClientID %>');
	if(tb && !isNaN(tb.value))
	{
		var j = parseFloat(tb.value);
		alert(tb.value + ' - ' + j);
		if (i > j)
			return (args.IsValid = false);
	}
}
</script>
</bx:InlineScript>--%>
<table cellspacing="0" cellpadding="0" border="0" class="edit-table" >
    <tr valign="top">
		<td class="field-name" width="40%">
			<% =GetMessage("Precision")+ ":" %></td>
		<td>
			<asp:TextBox ID="Precision" runat="server" Columns="20" MaxLength="255"  Text="4" />
		</td>
	</tr>
	<tr valign="top">
		<td class="field-name" width="40%">
			<% =GetMessage("DefaultValue") + ":" %></td>
		<td style="width: 176px">
			<asp:TextBox ID="DefaultValue" runat="server" Columns="20" MaxLength="255" />
			<asp:CompareValidator ID="DefaultValueValidator" runat="server" 
				Operator="DataTypeCheck" Type="Double" ControlToValidate="DefaultValue" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic"
				ErrorMessage='<%# GetMessageFormat("Error.FieldIsNotReal", GetMessage("DefaultValue")) %>' 
			>*</asp:CompareValidator>
			<%--<asp:CustomValidator ID="DefaultValueConditions" runat="server" 
				ControlToValidate="DefaultValue" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic"
				ErrorMessage='<%# GetMessageFormat("Error.DefaultRangeInvalid", GetMessage("DefaultValue")) %>' 
				OnServerValidate="DefaultValueConditions_ServerValidate" ValidateEmptyText="True" >*</asp:CustomValidator>--%>
			<asp:CompareValidator ID="CompareValidator1" runat="server" 
				Operator="GreaterThanEqual" Type="Double" ControlToValidate="DefaultValue" ControlToCompare="MinValue"
				ValidationGroup="<%# ValidationGroup %>" Display="Dynamic"
				ErrorMessage='<%# GetMessageFormat("Error.DefaultRangeInvalid", GetMessage("DefaultValue")) %>' 
			 >*</asp:CompareValidator>
			 <asp:CompareValidator ID="CompareValidator2" runat="server" 
				Operator="LessThanEqual" Type="Double" ControlToValidate="DefaultValue" ControlToCompare="MaxValue"
				ValidationGroup="<%# ValidationGroup %>" Display="Dynamic"
				ErrorMessage='<%# GetMessageFormat("Error.DefaultRangeInvalid", GetMessage("DefaultValue")) %>' 
			 >*</asp:CompareValidator>
			
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
			<asp:CompareValidator ID="MinValueValidator" runat="server" 
				ControlToValidate="MinValue" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic"
				ErrorMessage='<%# GetMessageFormat("Error.FieldIsNotReal", GetMessage("MinValue")) %>' 
				Type="Double" Operator="DataTypeCheck" >*</asp:CompareValidator>
		</td>
	</tr>
	<tr valign="top">
		<td class="field-name">
			<% =GetMessage("MaxValue") + ":" %></td>
		<td style="width: 176px">
			<asp:TextBox ID="MaxValue" runat="server" Columns="20" MaxLength="255" />
			<asp:CompareValidator ID="MaxValueValidator" runat="server" 
				ControlToValidate="MaxValue" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic"
				ErrorMessage='<%# GetMessageFormat("Error.FieldIsNotReal", GetMessage("MaxValue")) %>' 
				Type="Double" Operator="DataTypeCheck" >*</asp:CompareValidator>
		</td>
	</tr>
</table>
