<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CustomFieldEdit.ascx.cs" Inherits="CustomFieldEdit" EnableViewState="false" %>
<bx:InlineScript runat="server" ID="Script">
<script type="text/javascript">
        function validateName(oSrc, args) {
            if(args.Value)
            {
                var trimed = args.Value.replace(/^\s+|\s+$/g,"").toUpperCase();
                args.IsValid = (trimed!="VALUE") && (trimed!="VALUEID") && (trimed!="VALUEINT") && (trimed!="VALUEDOUBLE") && (trimed!="VALUEDATE");
            }
         }
</script>
</bx:InlineScript>
<asp:UpdatePanel ID="U" runat="server" UpdateMode="Conditional">
	<ContentTemplate>
		<asp:HiddenField runat="server" ID="StoredUserTypeId" />
		<asp:HiddenField runat="server" ID="StoredFieldName" />
		<asp:HiddenField runat="server" ID="StoredMultiple" />
		<asp:HiddenField runat="server" ID="StoredSender" />
		<bx:BXPopupDialog ID="D" runat="server" Width="910px"
		StartHidden="true" HighlightContent="false" WindowTitle="<%$ Loc:WindowTitle %>"
		ShowCancelButton="False" ShowResetButton="False" ShowSaveButton="False">
			<ContentPanel>
				<asp:ValidationSummary ID="VS" runat="server" DisplayMode="BulletList" ValidationGroup="<%# ValidationGroup %>" />
				<bx:BXTabControl ID="T" runat="server" ValidationGroup="<%# ValidationGroup %>" ShowApplyButton="False" OnCommand="mainTabControl_Command" >
					<bx:BXTabControlTab ID="M" runat="server" Selected="True" Text="<%$ Loc:TabText.Main %>"
						Title="<%$ Loc:TabTitle.Main %>">
						<table cellspacing="0" cellpadding="0" border="0" class="edit-table" >
							<tr>
								<td class="field-name" width="40%">
									<span class="required">*</span><asp:Literal ID="Literal2" runat="server" Text="<%$ Loc:UserTypeId %>" />:</td>
								<td width="60%">
									<asp:Literal ID="UserTypeIdLiteral" runat="server" />
									<asp:DropDownList ID="UserTypeIdTextBox" runat="server" AutoPostBack="True" />
									<asp:RequiredFieldValidator ID="UserTypeIdRequired" runat="server"  ControlToValidate="UserTypeIdTextBox"
										Display="Dynamic" ErrorMessage="<%$ Loc:Error.UserTypeIdRequired %>" ValidationGroup="<%# ValidationGroup %>">*</asp:RequiredFieldValidator>
									<asp:CustomValidator ID="UserTypeIdValidator" runat="server"  ControlToValidate="UserTypeIdTextBox"
										Display="Dynamic" ErrorMessage="<%$ Loc:Error.UserTypeIdIllegal %>" OnServerValidate="UserTypeIdValidator_ServerValidate"
										ValidationGroup="<%# ValidationGroup %>">*</asp:CustomValidator>
									<%
										if (string.Equals(Request.Browser.Browser, "Opera", StringComparison.OrdinalIgnoreCase) && UserTypeIdTextBox.Visible)
										{
									%>
									<asp:Button ID="DropDownList1" runat="server" Text="<%$ LocRaw:Kernel.Ok %>" />
									<%
										}
									%>
								</td>
							</tr>
							<tr>
								<td class="field-name" >
									<span class="required">*</span><asp:Literal ID="Literal3" runat="server" Text="<%$ Loc:FieldName %>" />:</td>
								<td >
									<asp:Literal ID="FieldNameLiteral" runat="server" />
									<asp:TextBox ID="FieldNameTextBox" runat="server" MaxLength="20" />
									<asp:RequiredFieldValidator ID="FieldNameRequired" runat="server"  ControlToValidate="FieldNameTextBox"
										Display="Dynamic" ErrorMessage="<%$ Loc:Error.FieldNameRequired %>" ValidationGroup="<%# ValidationGroup %>">*</asp:RequiredFieldValidator>
									<asp:CustomValidator runat="server" ID="cvFieldName"  ValidationGroup="<%# ValidationGroup %>"
										ErrorMessage="<%$ Loc:Error.FieldNameIllegalWords %>" ControlToValidate="FieldNameTextBox"
										ClientValidationFunction="validateName">*</asp:CustomValidator>
									<asp:RegularExpressionValidator ID="FieldNameValidator" runat="server"  ControlToValidate="FieldNameTextBox"
										Display="Dynamic" ErrorMessage="<%$ Loc:Error.FieldNameIllegal %>" ValidationExpression="[a-zA-Z0-9_]+"
										ValidationGroup="<%# ValidationGroup %>">*</asp:RegularExpressionValidator>
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
						</table>
					</bx:BXTabControlTab>
					<bx:BXTabControlTab ID="BXTabControlTab2" runat="server" Text="<%$ Loc:TabText.Advanced %>"
						Title="<%$ Loc:TabTitle.Advanced %>">
						<table cellspacing="0" cellpadding="0" border="0" class="edit-table">
							<%--<tr class="heading" id="SettingsHeader" runat="server">
								<td id="Td3" colspan="2" runat="server">
									<asp:Literal ID="Literal12" runat="server" Text="<%$ Loc:Settings %>" />
								</td>
							</tr>--%>
							<tr>
								<td colspan="2" id="SettingsHolder" runat="server">
								</td>
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
											<table border="0" cellspacing="0" width="100%">
												<tr>
													<td align="center" style="width: 10%">
														<asp:Literal ID="Literal14" runat="server" Text="<%$ Loc:Column.Language %>" /></td>
													<td align="center" style="width: 18%">
														<asp:Literal ID="Literal15" runat="server" Text="<%$ Loc:Column.EditFormLabel %>" /></td>
													<td align="center" style="width: 18%">
														<asp:Literal ID="Literal16" runat="server" Text="<%$ Loc:Column.ListColumnLabel %>" /></td>
													<td align="center" style="width: 18%">
														<asp:Literal ID="Literal17" runat="server" Text="<%$ Loc:Column.ListFilterLabel %>" /></td>
													<td align="center" style="width: 18%">
														<asp:Literal ID="Literal18" runat="server" Text="<%$ Loc:Column.ErrorMessage %>" /></td>
													<td align="center" style="width: 18%">
														<asp:Literal ID="Literal19" runat="server" Text="<%$ Loc:Column.HelpMessage %>" /></td>
												</tr>
										</HeaderTemplate>
										<ItemTemplate>
											<tr>
												<td style="white-space: nowrap">
													<%# Eval("Name") + ":" %>
												</td>
												<td style="white-space: nowrap">
													<asp:TextBox ID="EditFormLabel" runat="server" MaxLength="255" /></td>
												<td style="white-space: nowrap">
													<asp:TextBox ID="ListColumnLabel" runat="server" MaxLength="255" /></td>
												<td style="white-space: nowrap">
													<asp:TextBox ID="ListFilterLabel" runat="server" MaxLength="255" /></td>
												<td style="white-space: nowrap">
													<asp:TextBox ID="ErrorMessage" runat="server" MaxLength="255" /></td>
												<td style="white-space: nowrap">
													<asp:TextBox ID="HelpMessage" runat="server" MaxLength="255" /></td>
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
					<bx:BXTabControlTab ID="E" Visible="False" runat="server" Text="<%$ Loc:TabText.Extra %>"
						Title="<%$ Loc:TabTitle.Extra %>">
						<asp:PlaceHolder runat="server" ID="ExtraSettingsPlaceholder"></asp:PlaceHolder>
					</bx:BXTabControlTab>
				</bx:BXTabControl>
			</ContentPanel>
		</bx:BXPopupDialog>
	</ContentTemplate>
</asp:UpdatePanel>
