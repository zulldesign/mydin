<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Options.ascx.cs" Inherits="bitrix_modules_Search_Options" %>
<%@ Import Namespace="Bitrix" %>	
<%@ Import Namespace="Bitrix.Components" %>	
<%@ Import Namespace="Bitrix.Search" %>	
<%@ Import Namespace="System.Collections.Generic" %>
	
<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" ValidationGroup="vgInnerForm" HeaderText="<%$ Loc:Kernel.Error %>" />
<bx:BXMessage ID="successMessage" runat="server" CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>"
	Visible="False" Content="<%$ Loc:Message.SaveSettingsSuccess %>" />

<bx:BXTabControl ID="BXTabControl1" ValidationGroup="vgInnerForm" runat="server" OnCommand="BXTabControl1_Command" >
	<bx:BXTabControlTab ID="BXTabControlTab1" runat="server" Selected="True" Text="<%$ LocRaw:TabText.Main %>"
		Title="<%$ LocRaw:TabTitle.Main %>">
		<table border="0" cellpadding="0" cellspacing="0" class="edit-table">
		   <tr class="heading">
				<td colspan="2">
					<%= GetMessage("Heading.StaticFiles") %>
				</td>
			</tr>
		    <tr valign="top">
				<td class="field-name" width="40%"><%= GetMessage("Label.MaxIndexingDocumentFileSize") + ":" %></td>
				<td width="60%">
					<asp:TextBox ID="MaxFileSize" ValidationGroup="vgInnerForm" runat="server"></asp:TextBox>
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%"><%= GetMessage("Label.InclusionMask") %>:</td>
				<td width="60%">
					<%= GetMessage("Label.Files") %><br />
					<asp:TextBox ID="IncludeMask" ValidationGroup="vgInnerForm" runat="server" Width="400px" ></asp:TextBox><br />
					<%= GetMessage("Label.Folders") %><br />
					<asp:TextBox ID="FolderIncludeMask" ValidationGroup="vgInnerForm" runat="server" Width="400px" ></asp:TextBox>
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%"><%= GetMessage("Label.ExclusionMask") %>:</td>
				<td width="60%">
					<%= GetMessage("Label.Files") %><br />
					<asp:TextBox ID="ExcludeMask" ValidationGroup="vgInnerForm" runat="server" Width="400px" ></asp:TextBox><br />
					<%= GetMessage("Label.Folders") %><br />
					<asp:TextBox ID="FolderExcludeMask" ValidationGroup="vgInnerForm" runat="server" Width="400px" ></asp:TextBox>
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%"><%= GetMessage("Label.Tags") %>:</td>
				<td width="60%">
					<table style="border: 0px; width: 100%" cellpadding="0" cellspacing="2" class="edit-table">
					<tr class="heading">
						<td><%= GetMessage("TableHeader.Site") %></td>
						<td><%= GetMessage("TableHeader.PageKeyword") %></td>
					</tr>
					<% foreach(BXSite site in BXSite.GetList(null, null, null, null, Bitrix.Services.Text.BXTextEncoder.EmptyTextEncoder)) { %>
					<tr>
						<td><%= Encode(site.Name) %></td>
						<td>
							<select name="<%= ClientID %>_PageTagsKeywords_<%= Encode(site.Id) %>" >
								<% string selected = BXSearchModule.Options.PageTagsKeyword[site.Id]; %>
								<option value="" <% if (string.IsNullOrEmpty(selected)) { %>selected="selected"<% } %>><%= GetMessage("Option.DontIndex") %></option>
								<% foreach(KeyValuePair<string, string> kw in BXPageManager.GetKeywords(site.Id)) { %>
								<option value="<%= Encode(kw.Key) %>" <% if (kw.Key == selected) { %>selected="selected"<% } %>><%= Encode(kw.Value) %></option>
								<% } %>
							</select>
						</td>
					</tr>
					<% } %>
					</table>
				</td>
			</tr>
			 <tr class="heading">
				<td colspan="2">
					<%= GetMessage("Heading.Stemming") %>
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
				</td>
				<td width="60%">
					<asp:CheckBox runat="server" ID="UseStemming" Text="<%$ Loc:Label.UseStemming %>" />
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%"><%= GetMessage("Label.StemmingExtraSymbols") + ":" %></td>
				<td width="60%">
					<asp:TextBox ID="StemmingWordChars" ValidationGroup="vgInnerForm" runat="server" Width="200px" ></asp:TextBox>
				</td>
			</tr>
			 <tr class="heading">
				<td colspan="2"><%= GetMessage("Heading.SystemProperties") %></td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%"><%= GetMessage("Label.MaxSearchResults") %>:</td>
				<td width="60%">
					<asp:TextBox ID="MaxSearchResults" ValidationGroup="vgInnerForm" runat="server" Width="200px" ></asp:TextBox>
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
				</td>
				<td width="60%">
					<asp:CheckBox runat="server" ID="UseSimplifiedRanking" Text="<%$ Loc:CheckBoxText.UseSimplifiedRanking %>" />
				</td>
			</tr>
		</table>
	</bx:BXTabControlTab>
</bx:BXTabControl>

