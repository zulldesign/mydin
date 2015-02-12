<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" Async="true" CodeFile="MarketplaceDetail.aspx.cs" Inherits="bitrix_admin_MarketplaceDetail" Title="Marketplace" EnableViewState="false" %>
<asp:Content ID="Content" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	<bx:BXContextMenuToolbar id="ActionBar" runat="server">
        <Items>
			<bx:BXCmSeparator runat="server" SectionSeparator="true" />
			<bx:BXCmImageButton runat="server" CssClass="context-button icon btn_list" Text="<%$ LocRaw:ActionText.GoBack %>" Title="<%$ LocRaw:ActionTitle.GoBack %>" Href="Marketplace.aspx" />
		</Items>
    </bx:BXContextMenuToolbar>
    
    <bx:BXValidationSummary ID="ErrorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="TabControl" />
   
    <% if (!HasError) { %>     
    <% 
		MainTab.Title = string.Format(GetMessageRaw("TabTitle.Main"), Data.Title); 
		DownloadAndInstall.Enabled = !Data.Downloaded;
	%>
	<bx:BXTabControl ID="TabControl" runat="server" ButtonsMode="Hidden" ValidationGroup="TabControl" >
		<Tabs>
			<bx:BXTabControlTab runat="server" ID="MainTab" Selected="True" Text="<%$ LocRaw:TabText.Main %>" >
				<table cellspacing="0" cellpadding="0" border="0" class="edit-table">
					<tr>
						<td width="40%" valign="top" align="right" class="field-name"><%= GetMessage("Label.Id") %>:</td>
						<td width="60%" valign="top"><%= Encode(Data.Id) %></td>
					</tr>
					<tr>
						<td width="40%" valign="top" align="right" class="field-name"><%= GetMessage("Label.Downloaded") %>:</td>
						<td width="60%" valign="top"><%= Data.Downloaded ? GetMessage("Kernel.Yes") : GetMessage("Kernel.No") %></td>
					</tr>
					<tr>
						<td width="40%" valign="top" align="right" class="field-name"><%= GetMessage("Label.Title") %>:</td>
						<td width="60%" valign="top"><%= Encode(Data.Title) %></td>
					</tr>
					<tr>
						<td width="40%" valign="top" align="right" class="field-name"><%= GetMessage("Label.Description") %>:</td>
						<td width="60%" valign="top"><%= Bitrix.Services.Text.BXStringUtility.HtmlEncodeEx(Data.Description) %></td>
					</tr>
					<% if (!string.IsNullOrEmpty(Data.ImageUrl)) { %>
					<tr>
						<td width="40%" valign="top" align="right" class="field-name"><%= GetMessage("Label.Image") %>:</td>
						<td width="60%" valign="top">
							<img 
								src="<%= HttpUtility.HtmlAttributeEncode(Data.ImageUrl) %>"
								<% if (Data.ImageWidth > 0) { %>width="<%= Data.ImageWidth %>"<% } %>
								<% if (Data.ImageHeight > 0) { %>height="<%= Data.ImageHeight %>"<% } %>
								alt=""
							/>
						</td>
					</tr>
					<% } %>
					<tr>
						<td width="40%" valign="top" align="right" class="field-name"><%= GetMessage("Label.Partner") %>:</td>
						<td width="60%" valign="top"><%= Encode(Data.Partner) %></td>
					</tr>
					<% if (Data.UpdateDate != DateTime.MinValue) { %>
					<tr>
						<td width="40%" valign="top" align="right" class="field-name"><%= GetMessage("Label.UpdateDate") %>:</td>
						<td width="60%" valign="top"><%= Encode(Data.UpdateDate.ToString("g")) %></td>
					</tr>
					<% } %>
					<% if (Data.CreateDate != DateTime.MinValue) { %>
					<tr>
						<td width="40%" valign="top" align="right" class="field-name"><%= GetMessage("Label.CreateDate") %>:</td>
						<td width="60%" valign="top"><%= Encode(Data.CreateDate.ToString("g")) %></td>
					</tr>
					<% } %>
					<% if (!Bitrix.Services.Text.BXStringUtility.IsNullOrTrimEmpty(Data.Category)) { %>
					<tr>
						<td width="40%" valign="top" align="right" class="field-name"><%= GetMessage("Label.Category") %>:</td>
						<td width="60%" valign="top"><%= Encode(Data.Category) %></td>
					</tr>
					<% } %>
					<% if (!Bitrix.Services.Text.BXStringUtility.IsNullOrTrimEmpty(Data.Type)) { %>
					<tr>
						<td width="40%" valign="top" align="right" class="field-name"><%= GetMessage("Label.Type") %>:</td>
						<td width="60%" valign="top"><%= Encode(Data.Type) %></td>
					</tr>
					<% } %>
				</table>
			</bx:BXTabControlTab>
		</Tabs>
		<ButtonsBar>
			<asp:Button 
				ID="DownloadAndInstall" 
				runat="server" 
				Text="<%$ LocRaw:ButtonText.DownloadAndInstall %>" 
				ValidationGroup="TabControl" 
				OnClick="DownloadAndInstall_Click" 
			/>&nbsp;<asp:Button 
				ID="Cancel" 
				runat="server" 
				Text="<%$ LocRaw:ButtonText.Cancel %>" 
				CausesValidation="false" 
				OnClick="Cancel_Click" 
			/>
		</ButtonsBar>
	</bx:BXTabControl>
	<% } %>
</asp:Content>

