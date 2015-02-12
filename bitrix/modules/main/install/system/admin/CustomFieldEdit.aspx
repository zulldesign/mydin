<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="CustomFieldEdit.aspx.cs" Inherits="bitrix_admin_CustomFieldEdit" Title="<%$ Loc:PageTitle %>" EnableViewState="false" ValidateRequest="false" %>
<asp:Content ID="mainContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <script type="text/javascript">
        function validateName(oSrc, args) {
            if(args.Value)
            {
                var trimed = args.Value.replace(/^\s+|\s+$/g,"").toUpperCase();
                args.IsValid = (trimed!="VALUE") && (trimed!="VALUEID") && (trimed!="VALUEINT") && (trimed!="VALUEDOUBLE") && (trimed!="VALUEDATE");
            }
         }
    </script>
    <% BackAction.Href = Request[Bitrix.Configuration.BXConfigurationUtility.Constants.BackUrl] ?? "CustomField.aspx"; %>
	<bx:BXContextMenuToolbar ID="mainActionBar" runat="server">
		<Items>
			<bx:BXCmSeparator runat="server" SectionSeparator="true" />
			<bx:BXCmImageButton ID="BackAction" runat="server" CssClass="context-button" CommandName="back" Text="<%$ Loc:Kernel.Back %>" />
		</Items>
	</bx:BXContextMenuToolbar>
	<bx:BXMessage ID="SuccessMessage" runat="server" CssClass="Ok" IconClass="Ok" Visible="False"
		Title="<%$ Loc:Kernel.Information %>" Content="<%$ Loc:Message.OperationSuccessful %>" />
	<bx:BXValidationSummary ID="ValidationSummary" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>"
		ValidationGroup="edit" />
	<bx:BXTabControl ID="mainTabControl" runat="server" OnCommand="mainTabControl_Command"
		ValidationGroup="edit">
		<bx:BXTabControlTab runat="server" Selected="True" Text="<%$ Loc:TabText.Main %>"
			Title="<%$ Loc:TabTitle.Main %>">
			<table cellspacing="0" cellpadding="0" border="0" class="edit-table">
				<tr id="IdRow" runat="server">
					<td class="field-name" width="40%" runat="server">
						ID:</td>
					<td width="60%" runat="server">
						<asp:Literal ID="IdLiteral" runat="server" />
					</td>
				</tr>
				<tr>
					<td class="field-name" width="40%">
						<span class="required">*</span><asp:Literal ID="Literal2" runat="server" Text="<%$ Loc:UserTypeId %>" />:</td>
					<td width="60%">
						<asp:Literal ID="UserTypeIdLiteral" runat="server" />
						<asp:DropDownList ID="UserTypeIdList" runat="server" AutoPostBack="True"  /> 
						<asp:RequiredFieldValidator ID="UserTypeIdRequired" runat="server" ControlToValidate="UserTypeIdList"
							Display="Dynamic" ErrorMessage="<%$ Loc:Error.UserTypeIdRequired %>" ValidationGroup="edit" >*</asp:RequiredFieldValidator>
						<asp:CustomValidator ID="UserTypeIdValidator" runat="server" ControlToValidate="UserTypeIdList"
							Display="Dynamic" ErrorMessage="<%$ Loc:Error.UserTypeIdIllegal %>" OnServerValidate="UserTypeIdValidator_ServerValidate"
							ValidationGroup="edit">*</asp:CustomValidator>
						<%
						if (string.Equals("Opera", Request.Browser.Browser, StringComparison.InvariantCultureIgnoreCase) && UserTypeIdList.Visible)
						{
						%>
						<asp:Button runat="server" Text="<%$ LocRaw:Kernel.Ok %>" />
						<%
						}
						%>
					</td>
				</tr>
				<tr>
					<td class="field-name">
						<span class="required">*</span><asp:Literal ID="Literal1" runat="server" Text="<%$ Loc:EntityId %>" />:</td>
					<td>
						<asp:Literal ID="EntityIdLiteral" runat="server" />
						<asp:TextBox ID="EntityIdTextBox" runat="server" />
						<asp:RequiredFieldValidator ID="EntityIdRequired" runat="server" ControlToValidate="EntityIdTextBox"
							Display="Dynamic" ErrorMessage="<%$ Loc:Error.EntityIdRequired %>" ValidationGroup="edit">*</asp:RequiredFieldValidator>
						<asp:RegularExpressionValidator ID="EntityIdValidator" runat="server" ControlToValidate="EntityIdTextBox"
							Display="Dynamic" ErrorMessage="<%$ Loc:Error.EntityIdIllegal %>" ValidationExpression="[a-zA-Z0-9_]+"
							ValidationGroup="edit">*</asp:RegularExpressionValidator>
					</td>
				</tr>
				<tr>
					<td class="field-name">
						<span class="required">*</span><asp:Literal ID="Literal3" runat="server" Text="<%$ Loc:FieldName %>" />:</td>
					<td>
						<asp:Literal ID="FieldNameLiteral" runat="server" />
						<asp:TextBox ID="FieldNameTextBox" runat="server" MaxLength="20" />
						<asp:RequiredFieldValidator ID="FieldNameRequired" runat="server" ControlToValidate="FieldNameTextBox"
							Display="Dynamic" ErrorMessage="<%$ Loc:Error.FieldNameRequired %>" ValidationGroup="edit">*</asp:RequiredFieldValidator>
						<asp:CustomValidator runat="server" ID="cvFieldName"  ValidationGroup="edit" ErrorMessage="<%$ Loc:Error.FieldNameIllegalWords %>" ControlToValidate="FieldNameTextBox" ClientValidationFunction="validateName">*</asp:CustomValidator>
						<asp:RegularExpressionValidator ID="FieldNameValidator" runat="server" ControlToValidate="FieldNameTextBox"
							Display="Dynamic" ErrorMessage="<%$ Loc:Error.FieldNameIllegal %>" ValidationExpression="[a-zA-Z0-9_]+"
							ValidationGroup="edit">*</asp:RegularExpressionValidator>
					</td>
				</tr>
				<tr>
					<td class="field-name">
						<asp:Literal ID="Literal4" runat="server" Text="<%$ Loc:XmlId %>" />:</td>
					<td>
						<asp:TextBox ID="XmlId" runat="server" MaxLength="255" />
					</td>
				</tr>
				<tr>
					<td class="field-name">
						<asp:Literal ID="Literal5" runat="server" Text="<%$ Loc:Sort %>" />:</td>
					<td>
						<asp:TextBox ID="Sort" runat="server" /></td>
				</tr>
				<tr>
					<td class="field-name">
						<asp:Literal ID="Literal6" runat="server" Text="<%$ Loc:Multiple %>" />:</td>
					<td>
						<asp:Literal ID="MultipleLiteral" runat="server" />
						<asp:CheckBox ID="MultipleCheckBox" runat="server" />
					</td>
				</tr>
				<tr>
					<td class="field-name">
						<asp:Literal ID="Literal7" runat="server" Text="<%$ Loc:Mandatory %>" />:</td>
					<td>
						<asp:CheckBox ID="Mandatory" runat="server" /></td>
				</tr>
				<tr>
					<td class="field-name">
						<asp:Literal ID="Literal8" runat="server" Text="<%$ Loc:ShowInFilter %>" />:</td>
					<td>
						<asp:DropDownList ID="ShowFilter" runat="server" />
					</td>
				</tr>
				<tr>
					<td class="field-name">
						<asp:Literal ID="Literal9" runat="server" Text="<%$ Loc:DontShowInList %>" />:</td>
					<td>
						<asp:CheckBox ID="DontShowInList" runat="server" />
					</td>
				</tr>
				<tr>
					<td class="field-name">
						<asp:Literal ID="Literal10" runat="server" Text="<%$ Loc:DontEditInList %>" />:</td>
					<td>
						<asp:CheckBox ID="DontEditInList" runat="server" /></td>
				</tr>
				<tr>
					<td class="field-name">
						<asp:Literal ID="Literal11" runat="server" Text="<%$ Loc:IsSearchable %>" />:</td>
					<td>
						<asp:CheckBox ID="IsSearchable" runat="server" /></td>
				</tr>
				<tr class="heading" id="SettingsHeader" runat="server">
					<td colspan="2" runat="server">
						<asp:Literal ID="Literal12" runat="server" Text="<%$ Loc:Settings %>" />
					</td>
				</tr>
				<tr>
				<td colspan="2" id="SettingsHolder" runat="server"></td>
				</tr>
				<tr class="heading">
					<td colspan="2">
						<asp:Literal ID="Literal13" runat="server" Text="<%$ Loc:Localization %>" />
					</td>
				</tr>
				<tr>
					<td colspan="2" align="center">
						<asp:Repeater ID="Localization" runat="server" OnItemDataBound="Localization_ItemDataBound">
							<HeaderTemplate>
								<table border="0" cellspacing="6" width="100%">
									<tr>
										<td style="width: 9%; text-align: right;" >
											<asp:Literal ID="Literal14" runat="server" Text="<%$ Loc:Column.Language %>" />&nbsp;&nbsp;&nbsp;</td>
										<td style="width: 18%; text-align: center;" >
											<asp:Literal ID="Literal15" runat="server" Text="<%$ Loc:Column.EditFormLabel %>" /></td>
										<td style="width: 18%; text-align: center;" >
											<asp:Literal ID="Literal16" runat="server" Text="<%$ Loc:Column.ListColumnLabel %>" /></td>
										<td style="width: 18%; text-align: center;" >
											<asp:Literal ID="Literal17" runat="server" Text="<%$ Loc:Column.ListFilterLabel %>" /></td>
										<td style="width: 18%; text-align: center;" >
											<asp:Literal ID="Literal18" runat="server" Text="<%$ Loc:Column.ErrorMessage %>" /></td>
										<td style="width: 18%; text-align: center;" >
											<asp:Literal ID="Literal19" runat="server" Text="<%$ Loc:Column.HelpMessage %>" /></td>
									</tr>
							</HeaderTemplate>
							<ItemTemplate>
								<tr>
									<td style="white-space: nowrap; text-align: right;">
										<%# Eval("Name") + ":" %>&nbsp;&nbsp;&nbsp;</td>
									<td style="white-space: nowrap; text-align: center;">
										<asp:TextBox ID="EditFormLabel" runat="server" MaxLength="255" Width="100%" /></td>
									<td style="white-space: nowrap; text-align: center;">
										<asp:TextBox ID="ListColumnLabel" runat="server" MaxLength="255" Width="100%" /></td>
									<td style="white-space: nowrap; text-align: center;">
										<asp:TextBox ID="ListFilterLabel" runat="server" MaxLength="255" Width="100%" /></td>
									<td style="white-space: nowrap; text-align: center;">
										<asp:TextBox ID="ErrorMessage" runat="server" MaxLength="255" Width="100%" /></td>
									<td style="white-space: nowrap; text-align: center;">
										<asp:TextBox ID="HelpMessage" runat="server" MaxLength="255" Width="100%" /></td>
								</tr>
							</ItemTemplate>
							<FooterTemplate>
								</table>
							</FooterTemplate>
						</asp:Repeater>
					</td>
				</tr>
			</table>
		</bx:BXTabControlTab>
		<bx:BXTabControlTab ID="ExtraTab" Visible="False" runat="server" Text="<%$ Loc:TabText.Extra %>"
			Title="<%$ Loc:TabTitle.Extra %>">
			<asp:PlaceHolder runat="server" ID="ExtraSettingsPlaceholder"></asp:PlaceHolder>
			</bx:BXTabControlTab>
	</bx:BXTabControl>
</asp:Content>
