<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Options.ascx.cs" Inherits="bitrix_modules_bitrix_corporatesite_Options" EnableViewState="false" %>

<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" ValidationGroup="vgInnerForm_Bank" HeaderText="<%$ Loc:Kernel.Error %>" />
<% if (Request["status"] == "ok") { %> 
<bx:BXMessage ID="successMessage" runat="server" CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Content="Parameters have been saved successfully." />
<% } %>
<bx:BXTabControl ID="TabControl_Furniture" ValidationGroup="vgInnerForm_Furn" runat="server" OnCommand="TabControl_Command">
	<bx:BXTabControlTab runat="server" ID="Tab_Furniture" Selected="True" Title="Component parameters (Manufacturer’s website)" Text="Components">
		<table border="0" cellpadding="0" cellspacing="0" class="edit-table">
		    <tr class="heading">
				<td colspan="2">News</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">SEF Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Furniture_NewsSEFFolder" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">News Page Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Furniture_NewsDetailUrl" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Information Block:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Furniture_NewsIblockId_int" /></td>
			</tr>
			<tr class="heading">
				<td colspan="2">Services</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%"> SEF Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Furniture_ServicesSEFFolder" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%"> Information Block:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Furniture_ServicesIblockId_int" /></td>
			</tr>
			<tr class="heading">
				<td colspan="2">Jobs</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%"> SEF Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Furniture_VacanciesSEFFolder" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%"> Information Block:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Furniture_VacanciesIblockId_int"/></td>
			</tr>
			<tr class="heading">
				<td colspan="2">Products</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%"> SEF Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Furniture_ProductsSEFFolder" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Product Page Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Furniture_ProductsDetailUrl" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%"> Information Block:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Furniture_CatalogIblockId_int" /></td>
			</tr>
			
			<tr class="heading">
				<td colspan="2">Site Template</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Company Slogan Include Area:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Furniture_TemplateSloganIncludeArea" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Website Logo Include Area:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Furniture_TemplateLogoIncludeArea" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Website Footer Include Area:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Furniture_TemplateCopyrightIncludeArea" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Banner Include Area:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Furniture_TemplateBannerIncludeArea" /></td>
			</tr>
			<tr class="heading">
				<td colspan="2">Other</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Authorization Page SEF Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Furniture_LoginSEFFolder" /></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">Search Pahe Path:</td>
				<td width="60%"><asp:TextBox runat="server" ID="Furniture_SearchUrl" /></td>
			</tr>
		</table>
	</bx:BXTabControlTab>
</bx:BXTabControl>
