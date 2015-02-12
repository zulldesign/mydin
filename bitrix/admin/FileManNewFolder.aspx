<%--<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" 
CodeFile="FileManNewFolder.aspx.cs" Inherits="bitrix_admin_FileManNewFolder" Title="<%$ Loc:PageTitle %>" 
StylesheetTheme="AdminTheme" Theme="AdminTheme" EnableViewState="false" %> zg--%>

<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" 
CodeFile="FileManNewFolder.aspx.cs" Inherits="bitrix_admin_FileManNewFolder" Title="<%$ Loc:PageTitle %>"  EnableViewState="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div runat="server" id="contentWrapper">
		<% BackButton.Href = BackUrl; %>
	    <bx:BXContextMenuToolbar id="BXContextMenuToolbar1" runat="server">
            <Items>
			    <bx:BXCmImageButton runat="server" ID="BackButton" CssClass="context-button icon btn_folder_up" CommandName="back"
				    Text="<%$ Loc:ActionText.GoBack %>" Title="<%$ Loc:ActionTitle.GoBack %>" />
		    </Items>
        </bx:BXContextMenuToolbar>
        <bx:BXMessage ID="resultMessage" runat="server" CssClass="Ok" IconClass="Ok" Visible="False" />
	    <bx:BXTabControl ID="mainTabControl" runat="server" 
	        ValidationGroup="folderName" OnCommand="mainTabControl_Command">
		    <bx:BXTabControlTab runat="server" Selected="True" Text="<%$ Loc:TabText.NewFolder %>" Title="<%$ Loc:TabTitle.NewFolder %>">
		    <div runat="server" id="editor_container">
		        <table class="edit-table" cellspacing="0" cellpadding="0" border="0">
		            <tr>
			            <td class="field-name"><asp:Literal ID="Literal1" runat="server" Text="<%$ Loc:FieldName.FolderName %>" />: </td>
			            <td><asp:TextBox ID="folderName" runat="server" Width="200px" /> <br/>
				            <asp:RegularExpressionValidator ID="folderNameValidator" runat="server" ControlToValidate="folderName"
					           EnableClientScript="true" Display="Dynamic" ValidationGroup="folderName"></asp:RegularExpressionValidator>
		                    <asp:RequiredFieldValidator runat="server" ID="folderNameRequired" 
		                        Display="Dynamic" Text="" EnableClientScript="true"
		                        ControlToValidate="folderName"
		                        ValidationGroup="folderName">
		                    </asp:RequiredFieldValidator>						            
			            </td>
		            </tr>
		            <tr>
			            <td class="field-name"><asp:Literal ID="Literal2" runat="server" Text="<%$ Loc:FieldName.SectionName %>" />: </td>
			            <td><asp:TextBox ID="sectionName" runat="server" Width="200px" /></td>
		            </tr>
		            <tr><td colspan="2" style="height: 27px">&nbsp;</td></tr>
		            <tr>
			            <td class="field-name"><asp:Literal ID="Literal6" runat="server" Text="<%$ Loc:FieldName.CreateMenuItem %>" />: </td>
			            <td><asp:CheckBox ID="CreateMenu" runat="server" Checked="false" onclick="fileMan_CheckCreateMenuItem(this);" /></td>
		            </tr>
		            <tr>
			            <td class="field-name"><asp:Literal ID="Literal7" runat="server" Text="<%$ Loc:FieldName.MenuType %>" />: </td>
			            <td><asp:DropDownList ID="MenuType" runat="server" /></td>
		            </tr>
		            <tr>
			            <td class="field-name"><asp:Literal ID="Literal8" runat="server" Text="<%$ Loc:FieldName.MenuItemName %>" />: </td>
			            <td><asp:TextBox ID="MenuItemName" runat="server" /></td>
		            </tr>
		            <tr><td colspan="2" style="height: 27px">&nbsp;</td></tr>
		            <tr>
			            <td class="field-name"><asp:Literal ID="Literal3" runat="server" Text="<%$ Loc:FieldName.CreateDefaultPage %>" />: </td>
			            <td><asp:CheckBox runat="server" ID="addDefault" onclick="fileMan_CheckDefaultPage(this);" /></td>
		            </tr>
		            <tr>
			            <td class="field-name"><asp:Literal ID="Literal4" runat="server" Text="<%$ Loc:FieldName.DefaultPageTemplate %>" />: </td>
			            <td><asp:DropDownList runat="server" ID="defaultTemplate" Width="200px" /></td>
		            </tr>
		            <tr>
			            <td class="field-name"><asp:Literal ID="Literal5" runat="server" Text="<%$ Loc:FieldName.EditDefaultPage %>" />: </td>
			            <td><asp:CheckBox runat="server" ID="editDefault" /></td>
		            </tr>
		            </table>
		    </div>
    <%--		<script type="text/javascript">
		    //<!--
			    document.getElementById('<% =MenuType.ClientID %>').disabled = document.getElementById('<% =MenuItemName.ClientID %>').disabled = <% =(!CreateMenu.Checked).ToString().ToLowerInvariant() %>;
			    document.getElementById('<% =editDefault.ClientID %>').disabled = document.getElementById('<% =defaultTemplate.ClientID %>').disabled = <% =(!addDefault.Checked).ToString().ToLowerInvariant() %>;
		    //-->
		    </script>--%>
		    </bx:BXTabControlTab>
	    </bx:BXTabControl>
	</div>
</asp:Content>

