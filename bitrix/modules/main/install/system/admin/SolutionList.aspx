<%@ Page Language="C#" MasterPageFile="AdminMasterPage.master" AutoEventWireup="true" CodeFile="SolutionList.aspx.cs" Inherits="Bitrix.Main.AdminPages.SolutionList" ValidateRequest="false" %>
<%@ Import Namespace="Bitrix.Services" %>
<%@ Import Namespace="Bitrix.Configuration" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<script type="text/javascript">
		function Solution_Settings(id)
		{
			location.href = '<%= JSEncode(WizardUrl) %>?site=' + encodeURIComponent(id) + '&act=setup&<%= BXConfigurationUtility.Constants.BackUrl %>=<%= JSEncode(UrlEncode(Request.Url.AbsoluteUri)) %>';
			return false;
		}
		function Solution_SystemSettings(id)
		{
			location.href = '<%= JSEncode(new Uri(Request.Url, VirtualPathUtility.ToAbsolute("~/bitrix/admin/SolutionEdit.aspx")).AbsoluteUri) %>?site=' + encodeURIComponent(id) + '&<%= BXConfigurationUtility.Constants.BackUrl %>=<%= JSEncode(UrlEncode(Request.Url.AbsoluteUri)) %>';
			return false;
		}
		function Solution_Install(id)
		{
			location.href = '<%= JSEncode(WizardUrl) %>?site=' + encodeURIComponent(id);
			return false;
		}
	</script>

	<bx:BXPopupPanel id="PopupPanel" runat="server">
		<Commands>
			<bx:CommandItem UserCommandId="settings" ItemTitle="<%$ LocRaw:PopupTitle.Settings %>" Default="true" ItemText="<%$ LocRaw:PopupText.Settings %>" OnClickScript="return Solution_Settings(UserData['id']);" />
			<bx:CommandItem UserCommandId="system" ItemTitle="<%$ LocRaw:PopupTitle.SystemSettings %>" ItemText="<%$ LocRaw:PopupText.SystemSettings %>" OnClickScript="return Solution_SystemSettings(UserData['id']);" />
			<bx:CommandItem UserCommandId="install" ItemTitle="<%$ LocRaw:PopupTitle.Install %>" Default="true" ItemText="<%$ LocRaw:PopupText.Install %>" OnClickScript="return Solution_Install(UserData['id']);" />					
			<bx:CommandItem UserCommandId="nop" ItemTitle="<%$ LocRaw:PopupTitle.NoOperations %>" ItemText="<%$ LocRaw:PopupTitle.NoOperations %>" OnClickScript="return false;" />					
		</Commands>
	</bx:BXPopupPanel>
	
	<bx:BXContextMenuToolbar id="Toolbar" runat="server">
		<Items>
			<bx:BXCmImageButton ID="AddButton" CssClass="context-button icon btn_new" Text="<%$ LocRaw:ActionText.InstallNew %>" Href="InstallSolutionWizard.aspx" />
		</Items>
	</bx:BXContextMenuToolbar>
	
	<bx:BXGridView 
		id="GridView" 
		runat="server" 
		AllowPaging="True" 
		PopupCommandMenuId="PopupPanel" 
		AllowSorting="True"  
		ContentName="<%$ LocRaw:TableTitle %>" 
		OnSelect="GridView_Select" 
		OnSelectCount="GridView_SelectCount" 
		DataSourceID="GridView"
		ForeColor="#333333"
		BorderWidth="0px"
		BorderColor="white"
		BorderStyle="none"
		ShowHeader="true"
		OnRowDataBound="GridView_RowDataBound"
	>
	<Columns>	
		<asp:TemplateField HeaderText="ID">
		<ItemTemplate>
            [<a href="SiteEdit.aspx?id=<%# Encode(UrlEncode((string)Eval("Site.ID"))) %>"><%# Encode((string)Eval("Site.ID")) %></a>] <%# Encode((string)Eval("Site.Name")) %>
		</ItemTemplate>
		</asp:TemplateField>
		<asp:BoundField HeaderText="<%$ LocRaw:ColumnHeaderText.Solution %>" DataField="TitleHtml" HtmlEncode="false" ReadOnly="true" />		
	</Columns>
	</bx:BXGridView>

	<bx:BXMultiActionMenuToolbar id="MultiActionMenuToolbar" runat="server" Visible="false"/>
</asp:Content>
