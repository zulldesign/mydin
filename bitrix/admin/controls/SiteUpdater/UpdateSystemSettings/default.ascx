<%@ Control Language="C#" AutoEventWireup="true" CodeFile="default.ascx.cs" Inherits="bitrix_admin_controls_UpdateSystemSettings_default" %>
<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" runat="server">
	<Items>
		<bx:BXCmSeparator ID="BXCmSeparator1" runat="server">
		</bx:BXCmSeparator>
		<bx:BXCmImageButton ID="BXCmImageButton2" runat="server" 
			CommandName="ViewUpdates" Text="<%$ Loc:ActionText.Go2UpdateSystem %>" OnClickScript="window.location.href=&quot;UpdateSystem.aspx&quot;;return false;" Href="UpdateSystem.aspx" >
		</bx:BXCmImageButton>
	</Items>
</bx:BXContextMenuToolbar>

<bx:BXValidationSummary ID="errorMassage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>"/>
<bx:BXMessage ID="successMessage" runat="server" Content="<%$ Loc:Message.RecordHasBeenModifiedSuccessfully %>"
	CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False" Width="438px" />

<bx:BXTabControl ID="BXTabControl1" runat="server" OnCommand="BXTabControl1_Command">
	<bx:BXTabControlTab ID="BXTabControlTab1" runat="server" Text="<%$ Loc:TabText.UpdateSystemSettings %>" Title="<%$ Loc:TabText.UpdateSystemSettings %>" Selected="True">
		<table border="0" cellpadding="0" cellspacing="0" class="edit-table">
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Legend.UpdateSystemAddress")%></td>
				<td width="60%">
					<asp:TextBox ID="tbUpdateUrl" Width="350px" runat="server"></asp:TextBox>
					<asp:RequiredFieldValidator ID="rfvUpdateUrl" runat="server" ControlToValidate="tbUpdateUrl"
						ErrorMessage="<%$ Loc:Message.UpdateSystemAddressIsRequired %>">*</asp:RequiredFieldValidator>
					<asp:RegularExpressionValidator ID="revUpdateUrl" runat="server" ControlToValidate="tbUpdateUrl"
						ErrorMessage="<%$ Loc:Message.UpdateSystemAddressIsInvalid %>" ValidationExpression="http(s)?://([\w-]+\.)+[\w-]+(:[\d]+)?(/[\w- ./?%&amp;=]*)?">*</asp:RegularExpressionValidator>
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Legend.LicenceKey") %></td>
				<td width="60%">
					<script type="text/javascript">
					function ActivateCheckbox()
					{
						if (window.RequestActivationCheckBoxActivated)
							return;
						document.getElementById("<%= RequestActivation.ClientID %>").checked = true;
						window.RequestActivationCheckBoxActivated = true;
					}
					</script>
					<asp:TextBox ID="tbKey" Width="350px" runat="server" onkeypress="ActivateCheckbox();"></asp:TextBox>
					<asp:RequiredFieldValidator ID="rfvKey" runat="server" ControlToValidate="tbKey"
						ErrorMessage="<%$ Loc:Message.LicenceKeyIsRequired %>">*</asp:RequiredFieldValidator>
					<br /><asp:CheckBox runat="server" ID="RequestActivation" Text="<%$ LocRaw:CheckBoxText.RefreshLicenseData %>" />
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Legend.InstallStableVersionsOnly") %></td>
				<td width="60%">
					<asp:CheckBox Checked="True" ID="cbStableVersionsOnly" runat="server" />
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Legend.SafeUpdating") %></td>
				<td width="60%">
					<asp:CheckBox ID="cbSafeUpdating" Checked="True" runat="server" />
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Legend.DefaultLanguageForUpdateSystem") %></td>
				<td width="60%">
					<asp:DropDownList ID="ddlLanguage" runat="server">
						<asp:ListItem Selected="True" Value="en">English</asp:ListItem>
						<asp:ListItem Value="ru" Text="<%$ Loc:Russian %>"></asp:ListItem>
					</asp:DropDownList>
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("DbConnectionStringForUpdateSystem") %></td>
				<td width="60%">
					<asp:TextBox ID="tbConnectionString" Width="350px" runat="server"></asp:TextBox>
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("DbOwner") %></td>
				<td width="60%">
					<asp:TextBox ID="tbDatabaseOwner" Text="dbo." Width="131px" runat="server"></asp:TextBox>
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("EmailForNotificationAboutUpdates") %></td>
				<td width="60%">
					<asp:TextBox ID="tbNotificationEmail" Text="" Width="350px" runat="server"></asp:TextBox>
				</td>
			</tr>
		</table>
	</bx:BXTabControlTab>
	<bx:BXTabControlTab ID="BXTabControlTab3" runat="server" Text="<%$ Loc:Tab.Automation %>" Title="<%$ Loc:Tab.Automation %>">
		<table border="0" cellpadding="0" cellspacing="0" class="edit-table">
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Legend.DownloadUpdatesAutomatically") %></td>
				<td width="60%">
					<asp:CheckBox ID="cbAutoStart" Checked="True" runat="server" />
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Legend.BeforeFirstUpdateTestDelay") %></td>
				<td width="60%">
					<asp:TextBox ID="tbInitialPollInterval" Text="60" Width="50px" runat="server"></asp:TextBox> <%= GetMessage("Seconds") %>
					<asp:RangeValidator ID="rvInitialPollInterval" runat="server" ControlToValidate="tbInitialPollInterval"
						ErrorMessage="<%$ Loc:Message.BeforeFirstUpdateTestDelayIsInvalid %>" MaximumValue="3600" MinimumValue="10" Type="Integer">*</asp:RangeValidator>
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Legend.UpdateTestFrequency") %></td>
				<td width="60%">
					<asp:TextBox ID="tbPollInterval" Text="3600" Width="50px" runat="server"></asp:TextBox> <%= GetMessage("Seconds") %>
					<asp:RangeValidator ID="rvPollInterval" runat="server" ControlToValidate="tbPollInterval"
						ErrorMessage="<%$ Loc:Message.UpdateTestFrequencyIsRequired %>" MaximumValue="86400" MinimumValue="600" Type="Integer"></asp:RangeValidator>
				</td>
			</tr>
		</table>
	</bx:BXTabControlTab>
	<bx:BXTabControlTab ID="BXTabControlTab2" runat="server" Text="<%$ Loc:TabText.ProxySetup %>" Title="<%$ Loc:TabText.ProxySetup %>">
		<table border="0" cellpadding="0" cellspacing="0" class="edit-table">
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Legend.UserProxy") %></td>
				<td width="60%">
					<asp:CheckBox ID="cbUseProxy" runat="server" />
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Legend.ProxyAddress") %></td>
				<td width="60%">
					<asp:TextBox ID="tbProxyAddress" Width="350px" runat="server"></asp:TextBox>
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Legend.UserName") %></td>
				<td width="60%">
					<asp:TextBox ID="tbProxyUsername" Width="350px" runat="server"></asp:TextBox>
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Legend.Password") %></td>
				<td width="60%">
					<asp:TextBox ID="tbProxyPassword" TextMode="Password" Width="350px" runat="server"></asp:TextBox>
				</td>
			</tr>
			<tr valign="top">
				<td class="field-name" width="40%">
					<%= GetMessage("Legend.PasswordConfirmation") %></td>
				<td width="60%">
					<asp:TextBox ID="tbProxyPasswordConfirm" TextMode="Password" Width="350px" runat="server"></asp:TextBox>
					<asp:CompareValidator runat="server" ErrorMessage="<%$ Loc:Message.PasswordAndItsConfirmationDontMatch %>"
						ControlToValidate="tbProxyPasswordConfirm" CultureInvariantValues="True" 
						ControlToCompare="tbProxyPassword" ID="cvProxyPasswordConfirm">*</asp:CompareValidator>
				</td>
			</tr>
		</table>
	</bx:BXTabControlTab>
</bx:BXTabControl>