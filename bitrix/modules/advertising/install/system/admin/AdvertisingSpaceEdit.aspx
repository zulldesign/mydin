<%@ Page Language="C#" 
    MasterPageFile="~/bitrix/admin/AdminMasterPage.master" 
	AutoEventWireup="false" 
	EnableViewState="false"
	CodeFile="AdvertisingSpaceEdit.aspx.cs" 
	Inherits="bitrix_admin_AdvertisingSpaceEdit" %>
		
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<bx:BXContextMenuToolbar ID="BXBlogEditToolbar" runat="server" OnCommandClick="OnToolBarButtonClick">
		<Items>
			<bx:BXCmSeparator ID="ListButton" runat="server" SectionSeparator="true" />
			<bx:BXCmImageButton CssClass="context-button icon btn_list" CommandName="list"
				Text="<%$ LocRaw:ActionText.GoBack %>" Title="<%$ LocRaw:ActionTitle.GoBack %>" Href="AdvertisingSpaceList.aspx" />
		
			<bx:BXCmSeparator SectionSeparator="true" />
			<bx:BXCmImageButton ID="AddButton"
				CommandName="add" Text="<%$ LocRaw:Kernel.Add %>" Title="<%$ LocRaw:ActionTitle.Add %>"
				CssClass="context-button icon btn_new" Href="AdvertisingSpaceEdit.aspx" />
				
			<bx:BXCmSeparator SectionSeparator="true" />
			<bx:BXCmImageButton ID="DeleteButton"
				CommandName="delete" Text="<%$ LocRaw:Kernel.Delete %>" Title="<%$ LocRaw:ActionTitle.Delete %>"
				CssClass="context-button icon btn_delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>"/>
		</Items>
	</bx:BXContextMenuToolbar>
	<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="AdvertisingSpaceEdit" />
	<bx:BXTabControl ID="TabControl" runat="server" OnCommand="OnAdvertisingSpaceEdit" ValidationGroup="AdvertisingSpaceEdit">
		<bx:BXTabControlTab ID="MainSettingsTab" Runat="server" Selected="True" Text="<%$ LocRaw:TabText.AdvertisingSpace %>" Title="<%$ LocRaw:TabTitle.AdvertisingSpace %>">
			<table class="edit-table" cellspacing="0" cellpadding="0" border="0">
				<% if (AdvertisingSpaceId > 0){
					%><tr valign="top"><td class="field-name">ID:</td><td><%= AdvertisingSpaceId %></td></tr><%
                }%>
				
				<tr valign="top" title="<%= GetMessage("FieldTooltip.AdvertisingSpaceCode") %>" >
					<td class="field-name"><span class="required">*</span><%= GetMessage("FieldLabel.AdvertisingSpaceCode")%>:</td>
					<td>
						<asp:TextBox ID="AdvertisingSpaceCode"  runat="server" Width="250px" />	
						<asp:RequiredFieldValidator ID="CodeRequiredValidator" runat="server" ValidationGroup="AdvertisingSpaceEdit" ControlToValidate="AdvertisingSpaceCode" ErrorMessage="<%$ Loc:Message.AdvertisingSpaceCodeIsNotSpecified %>" Display="Dynamic">*</asp:RequiredFieldValidator>											
					</td>
				</tr>					
						
				<tr valign="top" title="<%= GetMessage("FieldTooltip.Active") %>">
					<td class="field-name" width="40%"><%= GetMessage("FieldLabel.AdvertisingSpaceActive") %>:</td>
					<td width="60%"><asp:CheckBox ID="AdvertisingSpaceActive" runat="server" /></td>
				</tr>
						
				<tr valign="top" title="<%= GetMessage("FieldTooltip.AdvertisingSpaceName") %>">
					<td class="field-name"><span class="required">*</span><%= GetMessage("FieldLabel.AdvertisingSpaceName")%>:</td>
					<td>
						<asp:TextBox ID="AdvertisingSpaceName" runat="server" Width="350px" />
						<asp:RequiredFieldValidator ID="NameRequiredValidator" runat="server" ValidationGroup="AdvertisingSpaceEdit" ControlToValidate="AdvertisingSpaceName" ErrorMessage="<%$ Loc:Message.AdvertisingSpaceNameIsNotSpecified %>" Display="Dynamic">*</asp:RequiredFieldValidator>
					</td>
				</tr>				

				<tr valign="top" title="<%= GetMessage("FieldTooltip.AdvertisingSpaceDescription") %>">
					<td class="field-name"><%= GetMessage("FieldLabel.AdvertisingSpaceDescription")%>:</td>
					<td>
					    <asp:TextBox ID="AdvertisingSpaceDescription" runat="server" Width="350px" TextMode="MultiLine" Rows="5" />
					</td>
				</tr>
								
				<tr valign="top" title="<%= GetMessage("FieldTooltip.Sort") %>">
					<td class="field-name"><%= GetMessage("FieldLabel.Sort") %>:</td>
					<td>
					    <asp:TextBox ID="AdvertisingSpaceSort" runat="server" Width="50px" />
					    <asp:RegularExpressionValidator runat="server" ID="SortValidator" ValidationExpression="^[\d]*$" ControlToValidate="AdvertisingSpaceSort" ValidationGroup="AdvertisingSpaceEdit" ErrorMessage="<%$ LocRaw:Message.IncorrectSortIndex %>" Display="Dynamic">*</asp:RegularExpressionValidator>
					</td>
				</tr>
							
				<tr valign="top" title="<%= GetMessage("FieldTooltip.XmlId") %>" >
					<td class="field-name"><%= GetMessage("FieldLabel.XmlId") %>:</td>
					<td>
						<asp:TextBox ID="AdvertisingSpaceXmlId"  runat="server" Width="250px" />						
					</td>
				</tr>
			</table>
		</bx:BXTabControlTab>
	
	</bx:BXTabControl>
</asp:Content>

