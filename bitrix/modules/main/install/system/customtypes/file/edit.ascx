<%@ Control Language="C#" AutoEventWireup="true" CodeFile="edit.ascx.cs" Inherits="BXCustomTypeFileEdit" %>
<%--
    <table id="<% =ClientID + "_Loading" %>" border="0" cellpadding="0" cellspacing="0" style="display: none; visibility: hidden" ><tr><td><img src="<% =Bitrix.Common.BXConfigurationUtility.Constants.AdminThemeRoot + "images/wait.gif" %>" alt="" /></td><td>&nbsp;</td><td><% =GetMessage("Loading") %></td><td>&nbsp;</td><td id="<% =ClientID + "_ButtonPlaceholder" %>"></td></tr></table>
    zg, 25.04.2008
--%>
<table id="<% =ClientID + "_Loading" %>" border="0" cellpadding="0" cellspacing="0" style="display: none; visibility: hidden" >
<tr>
	<td>
		<img src="<% =Bitrix.UI.BXThemeHelper.AddAbsoluteThemePath("images/wait.gif") %>" alt="" />
	</td>
	<td>&nbsp;</td>
	<td><% =GetMessage("Loading") %></td>
	<td>&nbsp;</td>
	<td id="<% =ClientID + "_ButtonPlaceholder" %>"></td>
</tr>
</table>
<span id="SavedName" runat="server" />
<span id="<% =ClientID + "_ValuePlaceholder" %>" >
<input id="<% =ClientID + "_ValueUpload" %>" name="<% =ClientID + "_ValueUpload" %>" type="file" 
	onchange="CustomTypeFile_UploadFile(this, '<% =ClientID  %>');" 
	style="<% =UploadBoxStyle %>"
/>
</span>
<div id="DescriptionBlock" runat="server">
	<%= GetMessage("Label.Description") %>:<br />
	<asp:TextBox ID="Description" runat="server" />
</div>
<input id="<% =ClientID + "_ValueClear" %>" name="<% =ClientID + "_ValueClear" %>" type="button"
	onclick="CustomTypeFile_ClearFile(this, '<% =ClientID  %>');" style="<% =ClearButtonStyle %>" value="<% =GetMessage("Clear") %>"
/>
<asp:CustomValidator ID="SizeValidator" runat="server" ValidationGroup="<%# ValidationGroup %>" OnServerValidate="SizeValidator_ServerValidate" ValidateEmptyText="True" Display="Dynamic" >*</asp:CustomValidator>
<asp:CustomValidator ID="ExtensionValidator" runat="server" ValidationGroup="<%# ValidationGroup %>" ValidateEmptyText="True" Display="Dynamic" OnServerValidate="ExtensionValidator_ServerValidate" >*</asp:CustomValidator>
<asp:CustomValidator ID="RequiredValidator" runat="server" ValidationGroup="<%# ValidationGroup %>" ValidateEmptyText="True" Display="Dynamic" OnServerValidate="RequiredValidator_ServerValidate" >*</asp:CustomValidator>

<asp:HiddenField ID="CachedId" runat="server" />
<asp:HiddenField ID="StoredId" runat="server" />