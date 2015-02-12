<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Options.ascx.cs" Inherits="bitrix_modules_main_Options" EnableViewState="false" %>

<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" ValidationGroup="vgInnerForm" HeaderText="<%$ Loc:Kernel.Error %>" />
<% if (Request["status"] == "ok") { %> 
<bx:BXMessage ID="successMessage" runat="server" CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Content="Parameters have been saved successfully." />
<% } %>	


<bx:BXTabControl ID="TabControl" ValidationGroup="vgInnerForm" runat="server" OnCommand="TabControl_Command">
	<bx:BXTabControlTab runat="server" ID="Tab" Selected="True" Title="Component parameters" Text="Components">
		<table border="0" cellpadding="0" cellspacing="0" class="edit-table">
		    <tr class="heading">
				<td colspan="2">Blog</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Blog URL:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Blog" /></td>
			</tr>
		    <tr valign="top">
				<td class="field-name" width="40%">Blog Theme CSS File Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="BlogColorScheme" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">SEF Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="BlogSefFolder" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Blog Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="BlogUrl" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Blog Post Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="PostUrl" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Blog Post RSS Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="PostRssUrl" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Blog Post Edit Page:</td>
				<td width="60%"><asp:TextBox runat="server" ID="PostEditUrl" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Tags Page Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="BlogTagsUrl" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Comment Page Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="CommentUrl" /></td>
			</tr>
			<tr class="heading">
				<td colspan="2">Photogallery</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Information Block:</td>
				<td width="60%"><asp:TextBox runat="server" ID="PhotosIBlockId_int" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">SEF Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="PhotosSefFolder" /></td>
			</tr>
			<tr class="heading">
				<td colspan="2">Feedback</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Information Block:</td>
				<td width="60%"><asp:TextBox runat="server" ID="ContactsFeedbackIBlockId_int" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Email:</td>
				<td width="60%"><asp:TextBox runat="server" ID="ContactsEmail" /></td>
			</tr>
			<tr class="heading">
				<td colspan="2">Site Template</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Website Header Include Area:</td>
				<td width="60%"><asp:TextBox runat="server" ID="TemplateTitleIncludeArea" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Website Footer Include Area:</td>
				<td width="60%"><asp:TextBox runat="server" ID="TemplateSignatureIncludeArea" /></td>
			</tr>
			<tr class="heading">
				<td colspan="2">Other</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">User Profile Page Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="ProfileUrl" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Authorization Page SEF Path :</td>
				<td width="60%"><asp:TextBox runat="server" ID="LoginSefFolder" /></td>
			</tr>
		</table>
	</bx:BXTabControlTab>
</bx:BXTabControl>
