<%@ Control Language="C#" AutoEventWireup="true" CodeFile="settings.ascx.cs" Inherits="BXCustomTypeStringSettings" %>
<bx:InlineScript ID="InlineScript1" runat="server" AsyncMode="ScriptBlock">
<script type="text/javascript">
function <%= DefaultValueConditions.ClientValidationFunction %>(source, args)
{
	args.IsValid = true;
	var i = args.Value.length;
	if (i == 0)
		return;
		
	var tb = document.getElementById('<%= MinLength.ClientID %>');
	if (tb && !isNaN(tb.value))
	{
		var j = parseInt(tb.value);
		if (j != 0 && i < j)
			return (args.IsValid = false);
	}

	var tb = document.getElementById('<%= MaxLength.ClientID %>');
	if(tb && !isNaN(tb.value))
	{
		var j = parseInt(tb.value);
		if (j != 0 && i > j)
			return (args.IsValid = false);
	}
	
	args.IsValid=true;
}
function <%= HandleTypeChangeFunctionName %>()
{
    var patternBtnEl = document.getElementById("<%= PatternButton.ClientID %>");
    var patternEl = document.getElementById("<%= PatternContainer.ClientID %>");
    if(patternEl) patternEl.style.display = patternBtnEl && patternBtnEl.checked ? "" : "none";
    
}
 function <%= PutMacroParamInPatternFunctionName %>(macroParam) 
 {
    if(!macroParam) return;
    var patternEl = document.getElementById("<%= Pattern.ClientID %>");
    if(patternEl) patternEl.value += macroParam; 
 }
 
 if ( window.attachEvent )
    window.attachEvent("onload",<% =HandleTypeChangeFunctionName %>);
 else if ( window.addEventListener ) 
    window.addEventListener("load",<%=HandleTypeChangeFunctionName%>,false)
</script>
</bx:InlineScript>
<table cellspacing="0" cellpadding="0" border="0" class="edit-table" >
	<tr valign="top">
		<td class="field-name" width="40%"><%= GetMessage("Type") %>:</td>
		<td width="60%">
			<asp:RadioButton runat="server" ID="TextButton" Text="<%$ Loc:RadioButtonText.Text %>" GroupName="TextType" Checked="true"/>&nbsp;
			<asp:RadioButton runat="server" ID="HtmlButton" Text="HTML" GroupName="TextType" />&nbsp;
			<asp:RadioButton runat="server" ID="PatternButton" Text="<%$ Loc:PatternButtonText.Text %>" GroupName="TextType" />
		</td>
	</tr>
	
	<tr valign="top" runat="server" id="PatternContainer" style="display:none;">
		<td class="field-name" width="40%">
			<% =GetMessage("Pattern")%>:</td>
		<td  width="60%">
			<asp:TextBox ID="Pattern" runat="server" TextMode="MultiLine" Width="100%" Height="90px" />
			<br/>
			<div>
			    <b><%= GetMessage("MacroParameterLegend.Title")%>:</b><br/> 
			    <a href="javascript:<%= PutMacroParamInPatternFunctionName %>('#Value#')">#Value#</a> - <%= GetMessage("MacroParameterLegend.Value")%><br/>
			    <a href="javascript:<%= PutMacroParamInPatternFunctionName %>('#HtmlValue#')">#HtmlValue#</a> - <%= GetMessage("MacroParameterLegend.HtmlValue")%><br/>
			    <a href="javascript:<%= PutMacroParamInPatternFunctionName %>('#UrlValue#')">#UrlValue#</a> - <%= GetMessage("MacroParameterLegend.UrlValue")%><br/>
		        <a href="javascript:<%= PutMacroParamInPatternFunctionName %>('#UrlHtmlValue#')">#UrlHtmlValue#</a> - <%= GetMessage("MacroParameterLegend.UrlHtmlValue")%>
			</div>
		</td>
	</tr>	
	
	<tr valign="top">
		<td class="field-name" width="40%">
			<% =GetMessage("DefaultValue")%>:</td>
		<td  width="60%">
			<asp:TextBox ID="DefaultValue" runat="server" Columns="20" MaxLength="255" />
			<asp:CustomValidator ID="DefaultValueConditions" runat="server" 
				ControlToValidate="DefaultValue" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic"
				ErrorMessage='<%# GetMessageFormat("Error.DefaultRangeInvalid", GetMessage("DefaultValue")) %>' 
				OnServerValidate="DefaultValueConditions_ServerValidate" >*</asp:CustomValidator>
		</td>
	</tr>
	<tr valign="top">
		<td class="field-name">
			<% =GetMessage("TextBoxSize")%>:</td>
		<td>
			<asp:TextBox ID="TextBoxSize" runat="server" Columns="20" MaxLength="255" Text="20" />
			<asp:RangeValidator ID="TextBoxSizeValidator" runat="server" 
				ControlToValidate="TextBoxSize" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic"
				ErrorMessage='<%# GetMessageFormat("Error.FieldRangeInvalid", GetMessage("TextBoxSize"), 1, 255) %>' 
				Type="Integer" MinimumValue="1" MaximumValue="255">*</asp:RangeValidator>
		</td>
	</tr>
	<tr valign="top">
		<td class="field-name">
			<% =GetMessage("RowsCount")%>:</td>
		<td>
			<asp:TextBox ID="RowsCount" runat="server" Columns="20" MaxLength="255" Text="1" />
			<asp:RangeValidator ID="RowsCountValidator" runat="server" 
				ControlToValidate="RowsCount" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic"
				ErrorMessage='<%# GetMessageFormat("Error.FieldRangeInvalid", GetMessage("RowsCount"), 1, 255) %>' 
				Type="Integer" MinimumValue="1" MaximumValue="255">*</asp:RangeValidator>
		</td>
	</tr>
	<tr valign="top">
		<td class="field-name">
			<% =GetMessage("MinLength")%>:</td>
		<td>
			<asp:TextBox ID="MinLength" runat="server" Columns="20" MaxLength="255" Text="0" />
			<asp:RangeValidator ID="MinLengthValidator" runat="server" 
				ControlToValidate="MinLength" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic"
				ErrorMessage='<%# GetMessageFormat("Error.FieldRangeInvalid", GetMessage("MinLength"), 0, 255) %>' 
				Type="Integer" MinimumValue="0" MaximumValue="255">*</asp:RangeValidator>
		</td>
	</tr>
	<tr valign="top">
		<td class="field-name">
			<% =GetMessage("MaxLength")%>:</td>
		<td>
			<asp:TextBox ID="MaxLength" runat="server" Columns="20" MaxLength="255" Text="0" />
			<asp:RangeValidator ID="MaxLengthValidator" runat="server" 
				ControlToValidate="MaxLength" ValidationGroup="<%# ValidationGroup %>" Display="Dynamic"
				ErrorMessage='<%# GetMessageFormat("Error.FieldRangeInvalid", GetMessage("MaxLength"), 0, 255) %>' 
				Type="Integer" MinimumValue="0" MaximumValue="255">*</asp:RangeValidator>
		</td>
	</tr>
	<tr valign="top">
		<td class="field-name" style="height: 24px">
			<% =GetMessage("ValidationRegex")%>:</td>
		<td style="height: 24px">
			<asp:TextBox ID="ValidationRegex" runat="server" Columns="20" MaxLength="255" />
		</td>
	</tr>
</table>
