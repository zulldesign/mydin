<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" 
	AutoEventWireup="false" CodeFile="BlogCategoryEdit.aspx.cs" Inherits="bitrix_admin_BlogCategoryEdit" EnableViewState="false"%>
	
<%@ Register Src="~/bitrix/controls/Main/OperationsEdit.ascx" TagName="OperationsEdit"
	TagPrefix="bx" %>
	
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	
	<bx:BXContextMenuToolbar ID="BXBlogEditToolbar" runat="server" OnCommandClick="OnToolBarButtonClick">
		<Items>
			<bx:BXCmSeparator ID="ListButton" runat="server" SectionSeparator="true" />
			<bx:BXCmImageButton CssClass="context-button icon btn_list" CommandName="list"
				Text="<%$ LocRaw:ActionText.GoBack %>" Title="<%$ LocRaw:ActionTitle.GoBack %>" Href="BlogCategoryList.aspx" />
		
			<bx:BXCmSeparator SectionSeparator="true" />
			<bx:BXCmImageButton ID="AddButton"
				CommandName="add" Text="<%$ LocRaw:Kernel.Add %>" Title="<%$ LocRaw:ActionTitle.Add %>"
				CssClass="context-button icon btn_new" Href="BlogCategoryEdit.aspx" />
				
			<bx:BXCmSeparator SectionSeparator="true" />
			<bx:BXCmImageButton ID="DeleteButton"
				CommandName="delete" Text="<%$ LocRaw:Kernel.Delete %>" Title="<%$ LocRaw:ActionTitle.Delete %>"
				CssClass="context-button icon btn_delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>"/>
		</Items>
	</bx:BXContextMenuToolbar>
	<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="BlogCategoryEdit" />
	<bx:BXTabControl ID="TabControl" runat="server" OnCommand="OnBlogCategoryEdit" ValidationGroup="BlogCategoryEdit">
		<bx:BXTabControlTab ID="MainSettingsTab" Runat="server" Selected="True" Text="<%$ LocRaw:TabText.Category %>" Title="<%$ LocRaw:TabTitle.Category %>">
			<table class="edit-table" cellspacing="0" cellpadding="0" border="0">
				<% 
				if (BlogCategoryId > 0)
				{
					%><tr valign="top"><td class="field-name">ID:</td><td><%= BlogCategoryId %></td></tr><%
				}
				%>
		
				<tr valign="top" title="<%= GetMessage("FieldTooltip.CategoryName") %>">
					<td class="field-name"><span class="required">*</span><%= GetMessage("FieldLabel.CategoryName") %>:</td>
					<td>
						<asp:TextBox ID="BlogCategoryName" runat="server" Width="350px" />
						<asp:RequiredFieldValidator ID="NameRequiredValidator" runat="server" ValidationGroup="BlogCategoryEdit" ControlToValidate="BlogCategoryName" ErrorMessage="<%$ Loc:Message.CategoryNameIsNotSpecified %>" Display="Dynamic">*</asp:RequiredFieldValidator>
					</td>
				</tr>				
				<tr valign="top" title="<%= GetMessage("FieldTooltip.CategorySites") %>">
                    <td class="field-name">
                        <%  if (Bitrix.Modules.BXModuleManager.IsModuleInstalled("search")) { %><a href="#remark1" style="vertical-align:super; text-decoration:none"><span class="required">1</span></a><% } %><%= GetMessage("FieldLabel.CategorySites") %>:
					</td>
                    <td>
                        <asp:ListBox runat="server" ID="BlogCategorySites" SelectionMode="Multiple" Width="350px" Height="175px">
                        </asp:ListBox>
                    </td>
				</tr>	
								
				<tr valign="top" title="<%= GetMessage("FieldTooltip.Sort") %>">
					<td class="field-name"><%= GetMessage("FieldLabel.Sort") %>:</td>
					<td>
					    <asp:TextBox ID="BlogCategorySort" runat="server" Width="50px" />
					    <asp:RegularExpressionValidator runat="server" ID="SortValidator" ValidationExpression="^[\d]*$" ControlToValidate="BlogCategorySort" ValidationGroup="BlogCategoryEdit" ErrorMessage="<%$ LocRaw:Message.IncorrectSortIndex %>" Display="Dynamic">*</asp:RegularExpressionValidator>
					</td>
				</tr>
				
				<tr valign="top" title="<%= GetMessage("FieldTooltip.XmlId") %>" >
					<td class="field-name"><%= GetMessage("FieldLabel.XmlId") %>:</td>
					<td>
						<asp:TextBox Rows="7" ID="BlogCategoryXmlId"  runat="server" Width="350px" />						
					</td>
				</tr>
			</table>
		</bx:BXTabControlTab>
	
	</bx:BXTabControl>
	
<% if (Bitrix.Modules.BXModuleManager.IsModuleInstalled("search")) { %>
<bx:BXAdminNote ID="BXAdminNote1" runat="server">
    <span class="required" style="vertical-align:super" id="remark1" >1</span><%= string.Format(GetMessageRaw("Remark.ReindexRequired"), "SearchReindex.aspx") %>
</bx:BXAdminNote>
<% } %>
</asp:Content>

