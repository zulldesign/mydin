<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Options.ascx.cs" Inherits="bitrix_modules_bitrix_corporatesite_Options" EnableViewState="false" %>

<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" ValidationGroup="vgInnerForm_Bank" HeaderText="<%$ Loc:Kernel.Error %>" />
<% if (Request["status"] == "ok") { %> 
<bx:BXMessage ID="successMessage" runat="server" CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Content="Parameters have been saved successfully." />
<% } %>	
<bx:BXTabControl ID="TabControl_Bank" ValidationGroup="vgInnerForm_Bank" runat="server" OnCommand="TabControl_Command">
	<bx:BXTabControlTab runat="server" ID="Tab_Bank" Selected="True" Title="Component parameters (Service company's website)" Text="Components">
		<table border="0" cellpadding="0" cellspacing="0" class="edit-table">
			<tr class="heading">
				<td colspan="2">News</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">SEF Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Bank_NewsSefFolder" /></td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">News Page Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Bank_NewsDetailUrl" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Information Block:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Bank_BankNewsIBlockId_int" /></td>
			</tr>
			
			<tr class="heading">
				<td colspan="2">Jobs</td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%"> SEF Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Bank_VacanciesSefFolder" /></td>
			</tr>	
			<tr valign="top">
				<td class="field-name" width="40%">Information Block:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Bank_VacanciesIblockId_int" /></td>
			</tr>
			
			<tr class="heading">
				<td colspan="2">Management Board</td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">Management Board Page Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Bank_ManagementDetailUrl" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Information Block:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Bank_ManagementIBlockId_int" /></td>
			</tr>
			
			<tr class="heading">
				<td colspan="2">Feedback</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Information Block:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Bank_FeedBackIBlockId_int" /></td>
			</tr>
			
			<tr class="heading">
				<td colspan="2">Site Template</td>
			</tr>
			
			<tr valign="top">
				<td class="field-name" width="40%">Company Slogan Include Area:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Bank_TemplateSloganIncludeArea" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Website Logo Include Area:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Bank_TemplateLogoIncludeArea" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Website Footer Include Area:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Bank_TemplateCopyrightIncludeArea" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Banner Include Area:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Bank_BannerIncludeArea" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Banner Text Include Area:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Bank_BannerTextIncludeArea" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Company Phone Number Include Area:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Bank_TemplatePhoneIncludeArea" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Business Hours Include Area:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Bank_TemplateScheduleIncludeArea" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Sidebar Include Area:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Bank_TemplateSidebarIncludeArea" /></td>
			</tr>			
			<tr class="heading">
				<td colspan="2">Other</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Search Pahe Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Bank_SearchUrl" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Authorization Page SEF Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Bank_LoginSefFolder" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Title Page Menu Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Bank_DefaultPageMenuPath" /></td>
			</tr>
		</table>
	</bx:BXTabControlTab>
</bx:BXTabControl>
