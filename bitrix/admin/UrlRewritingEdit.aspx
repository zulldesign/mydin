<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="UrlRewritingEdit.aspx.cs" Inherits="bitrix_admin_UrlRewritingEdit" Title="<%$ Loc:PageTitle.UrlProcessingRules %>" EnableViewState="false" %>

<asp:Content ID="mainContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<script type="text/javascript">
		function GoBack()
		{
			window.location.href = '<%= Bitrix.Services.Js.BXJSUtility.Encode(BackUrl) %>';
			return false;
		}
	</script>
	<bx:BXContextMenuToolbar ID="mainActionBar" runat="server">
		<Items>
			<bx:BXCmSeparator runat="server" SectionSeparator="true" />
			<bx:BXCmImageButton CssClass="context-button icon btn_list" CommandName="list"
				Text="<%$ Loc:ActionText.RuleList %>" Title="<%$ Loc:ActionTitle.GoBackToRuleList %>" Href="UrlRewriting.aspx" />
		</Items>
	</bx:BXContextMenuToolbar>
	<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="UrlRewritingEdit" />
	<bx:BXMessage ID="successMessage" runat="server" Content="<%$ Loc:Message.SavingHasBeenCompletedSuccessfully %>"
		CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False"
		Width="438px" />
	<bx:BXTabControl ID="mainTabControl" runat="server" OnCommand="mainTabControl_Command" ValidationGroup="UrlRewritingEdit">
		<bx:BXTabControlTab runat="server" Selected="True" Text="<%$ Loc:TabText.Rule %>" Title="<%$ Loc:TabTitle.RuleSettings %>" >
			<table class="edit-table" cellspacing="0" cellpadding="0" border="0">
			<%
	if (!string.IsNullOrEmpty(ComponentType))
	{
			%>
				<tr valign="top">
					<td class="field-name">
						<%= GetMessage("ComponentType") %></td>
					<td>
						<%= Encode(ComponentType) %>
					</td>
				</tr>
			<%
	}
	if (!string.IsNullOrEmpty(ContainerPage))
	{
			%>
				<tr valign="top">
					<td class="field-name">
						<%= GetMessage("PageRedirectTo") %></td>
					<td>
						<%= Encode(ContainerPage) %>
					</td>
				</tr>
			<%
	} 
			%>
				<tr valign="top">
					<td class="field-name">
						<%= GetMessage("Site") %></td>
					<td>
						<asp:DropDownList ID="Site" runat="server" /></td>
				</tr>
				<tr valign="top">
					<td class="field-name">
						<span class="required">*</span><%= GetMessage("MatchTemplate") %></td>
					<td>
						<asp:TextBox ID="MatchTemplate" runat="server" Width="250px" />
						<asp:RequiredFieldValidator runat="server" ValidationGroup="UrlRewritingEdit" ControlToValidate="MatchTemplate" ErrorMessage="<%$ Loc:Message.FillMatchTemplate %>" Display="Dynamic" >*</asp:RequiredFieldValidator>
						<asp:CustomValidator runat="server" ValidationGroup="UrlRewritingEdit" ControlToValidate="MatchTemplate" ErrorMessage="<%$ Loc:Message.MatchTemplateIsNotAValidRegex %>" OnServerValidate="RegexValidator_ServerValidate" Display="Dynamic" >*</asp:CustomValidator>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name">&nbsp;</td>
					<td>
						<asp:CheckBox ID="Ignore" runat="server"  Text="<%$ Loc:CheckBoxText.ExcludeRuleFromProcessing %>" />
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name">
						<% 
		if (string.IsNullOrEmpty(ContainerPage))
		{ 
						%><span class="required">*</span><%
		} 
						%><%= GetMessage("RedirectionTemplate") %>
					</td>
					<td>
						<asp:TextBox ID="ReplaceTemplate" runat="server" Width="250px" />
						<asp:RequiredFieldValidator runat="server" ID="ReplaceTemplateValidator" ValidationGroup="UrlRewritingEdit" ControlToValidate="ReplaceTemplate" ErrorMessage="<%$ Loc:Message.FillReplaceTemplate %>" Display="Dynamic" >*</asp:RequiredFieldValidator>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name"><%= GetMessage("SortIndex") %>:</td>
					<td>
						<asp:TextBox ID="Sort" runat="server" Width="100px" />
						<asp:CompareValidator runat="server" ID="SortValidator" ValidationGroup="UrlRewritingEdit" ControlToValidate="Sort" Operator="DataTypeCheck" Type="Integer" ErrorMessage="<%$ Loc:Message.SortIndexMustBeInteger %>" Display="Dynamic" >*</asp:CompareValidator>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name"><%= GetMessage("HelperId") %>:</td>
					<td>
						<asp:TextBox ID="HelperId" runat="server" Width="100px" />
					</td>
				</tr>
			</table>
		</bx:BXTabControlTab>
	</bx:BXTabControl>
</asp:Content>
