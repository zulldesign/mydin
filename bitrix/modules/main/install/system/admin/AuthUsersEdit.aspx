<%@ Import Namespace="Bitrix.Configuration" %>
<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="AuthUsersEdit.aspx.cs" Inherits="bitrix_admin_AuthUsersEdit" Title="<%$ Loc:PageTitle.CreationOfUser %>"
	ValidateRequest="false" %> 

<%@ Register Src="~/bitrix/controls/Main/CustomFieldList.ascx" TagName="CustomFieldList"
	TagPrefix="uc1" %>
<%@ Register Src="~/bitrix/controls/Main/Calendar.ascx" TagName="Calendar" TagPrefix="bx" %>
<%@ Register Src="~/bitrix/admin/controls/Main/AdminImageField.ascx" TagName="AdminImageField"
	TagPrefix="bx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
			<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" runat="server" OnCommandClick="BXContextMenuToolbar1_CommandClick">
				<Items>
					<bx:BXCmSeparator runat="server" SectionSeparator="True">
					</bx:BXCmSeparator>
					<bx:BXCmImageButton runat="server" Title="<%$ Loc:ActionTitle.Go2UserList %>" Href="AuthUsersList.aspx"
						CssClass="context-button icon btn_list" CommandName="go2list" Text="<%$ Loc:ActionText.UserList %>">
					</bx:BXCmImageButton>
					<bx:BXCmSeparator runat="server" SectionSeparator="True" ID="AddUserSeparator">
					</bx:BXCmSeparator>
					<bx:BXCmImageButton runat="server" ID="AddUserButton" Title="<%$ Loc:ActionTitle.AddNewUser %>"
						Href="AuthUsersEdit.aspx" CssClass="context-button icon btn_new" CommandName="AddNewUser"
						Text="<%$ Loc:ActionText.AddNewUser %>">
					</bx:BXCmImageButton>
					<bx:BXCmSeparator runat="server" ID="DeleteUserSeparator">
					</bx:BXCmSeparator>
					<bx:BXCmImageButton runat="server" ID="DeleteUserButton" Title="<%$ Loc:ActionTitle.DeleteCurrentUser %>"
						CssClass="context-button icon btn_delete" CommandName="DeleteUser" ShowConfirmDialog="true"
						ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" Text="<%$ Loc:ActionText.DeleteCurrentUser %>">
					</bx:BXCmImageButton>
				</Items>
			</bx:BXContextMenuToolbar>
			<bx:BXValidationSummary ID="userValidationSummary" runat="server" CssClass="errorSummary"
				ValidationGroup="vgInnerForm" HeaderText="<%$ Loc:Kernel.Error %>"></bx:BXValidationSummary>
			<bx:BXMessage ID="successMessage" Title="<%$ Loc:Kernel.Information %>" runat="server"
				CssClass="ok" Visible="False" IconClass="ok" Content="<%$ Loc:Message.RecordHasBeenModifiedSuccessfully %>">
			</bx:BXMessage>
			<asp:HiddenField ID="hfUserId" runat="server"></asp:HiddenField>
			<bx:BXTabControl ID="BXTabControl1" runat="server" ValidationGroup="vgInnerForm"
				CanExpand="True" OnCommand="BXTabControl1_Command">
				<bx:BXTabControlTab runat="server" Selected="True" ID="MainTab" Title="<%$ Loc:TabTitle.RegistrationInformation %>"
					Text="<%$ Loc:TabText.User %>">
					<table id="Table1" border="0" cellpadding="0" cellspacing="0" class="edit-table">
						<tr runat="server" id="trCreationDate" valign="top">
							<td runat="server" class="field-name" width="40%">
								<%= GetMessage("LegendText.DateOfRegistration") %>
							</td>
							<td runat="server" width="60%">
								<asp:Label runat="server" ID="lbCreationDate"></asp:Label>
							</td>
						</tr>
						<tr runat="server" id="trLastLoginDate" valign="top">
							<td runat="server" class="field-name" width="40%">
								<%= GetMessage("LegendText.DateOfLastLogin") %>
							</td>
							<td runat="server" width="60%">
								<asp:Label runat="server" ID="lbLastLoginDate"></asp:Label>
							</td>
						</tr>
						<tr runat="server" id="trLastActivityDate" valign="top">
							<td runat="server" class="field-name" width="40%">
								<%= GetMessage("LegendText.DateOfLastActivity") %>
							</td>
							<td runat="server" width="60%">
								<asp:Label runat="server" ID="lbLastActivityDate"></asp:Label>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<%= GetMessage("LegendText.Approved") %>
							</td>
							<td width="60%">
								<asp:CheckBox runat="server" ValidationGroup="vgInnerForm" ID="cbIsApproved"></asp:CheckBox>
							</td>
						</tr>
						<tr runat="server" id="trIsLockedOut" valign="top">
							<td runat="server" class="field-name" width="40%">
								<%= GetMessage("LegendText.Lockout") %>
							</td>
							<td runat="server" width="60%">
								<asp:CheckBox runat="server" ValidationGroup="vgInnerForm" ID="cbIsLockedOut"></asp:CheckBox>
								<asp:HiddenField runat="server" ID="hfIsLockedOut"></asp:HiddenField>
							</td>
						</tr>
						<tr runat="server" id="trLastLockoutDate" valign="top">
							<td runat="server" class="field-name" width="40%">
								<%= GetMessage("LegendText.DateOfLastLockout") %>
							</td>
							<td runat="server" width="60%">
								<asp:Label runat="server" ID="lbLastLockoutDate"></asp:Label>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<span class="required">*</span><%= GetMessage("LegendText.AuthorizationProvider") %></td>
							<td width="60%">
								<asp:Label runat="server" ID="lbProviderName"></asp:Label>
								<asp:DropDownList runat="server" ValidationGroup="vgInnerForm" ID="ddProviderName">
								</asp:DropDownList>
								<asp:RequiredFieldValidator runat="server" ErrorMessage="<%$ Loc:ErrorMessage.AuthorizationProviderIsRequired %>"
									ControlToValidate="ddProviderName" ValidationGroup="vgInnerForm" ID="rfvProviderName"
									Display="Dynamic">*</asp:RequiredFieldValidator>
								<asp:CustomValidator runat="server" ClientValidationFunction="CheckProviderName"
									ErrorMessage="<%$ Loc:ErrorMessage.AuthorizationProviderIsNotFound %>" ControlToValidate="ddProviderName"
									ValidationGroup="vgInnerForm" ID="cvProviderName" Display="Dynamic" OnServerValidate="cvProviderName_ServerValidate">*</asp:CustomValidator>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<span class="required">*</span><%= GetMessage("LegendText.Login") %></td>
							<td width="60%">
								<asp:Label runat="server" ID="lbUserName"></asp:Label>
								<asp:TextBox runat="server" ValidationGroup="vgInnerForm" ID="tbUserName"></asp:TextBox>
								<asp:RequiredFieldValidator runat="server" ErrorMessage="<%$ Loc:ErrorMessage.LoginIsRequired %>"
									ControlToValidate="tbUserName" ValidationGroup="vgInnerForm" ID="rfvUserName"
									Display="Dynamic">*</asp:RequiredFieldValidator>
							</td>
						</tr>
						<tr runat="server" id="trPassword" valign="top">
							<td runat="server" class="field-name" width="40%">
								<%= GetMessage("LegendText.CurrentPassword") %>
							</td>
							<td runat="server" width="60%">
								<asp:TextBox runat="server" ValidationGroup="vgInnerForm" ID="tbPassword" AutoComplete="off" TextMode="Password"></asp:TextBox>
								<asp:CustomValidator runat="server" ValidateEmptyText="True" ClientValidationFunction="CheckPassword"
									ErrorMessage="<%$ Loc:ErrorMessage.PasswordIsRequiredWhenChangePassword %>" ControlToValidate="tbPassword"
									ValidationGroup="vgInnerForm" ID="cvPassword" OnServerValidate="cvPassword_ServerValidate">*</asp:CustomValidator>
							</td>
						</tr>
						<tr runat="server" valign="top" id="trNewPassword">
							<td class="field-name" width="40%">
								<% if (user == null) { %><span class="required">*</span><% } %><%= GetMessage(user == null ? "LegendText.Password" : "LegendText.NewPassword") %><br />
							</td>
							<td width="60%">
								<asp:TextBox runat="server" ValidationGroup="vgInnerForm" ID="tbNewPassword" TextMode="Password"></asp:TextBox>
								<asp:RequiredFieldValidator runat="server" ErrorMessage="<%$ Loc:ErrorMessage.PasswordIsRequired %>"
									ControlToValidate="tbNewPassword" ValidationGroup="vgInnerForm" ID="rfvNewPassword">*</asp:RequiredFieldValidator>
								<asp:Label ID="lbPasswordHint" runat="server" Text="" />
							</td>
						</tr>
						<tr runat="server" valign="top" id="trNewPasswordConf">
							<td class="field-name" width="40%">
								<% if (user == null) { %><span class="required">*</span><% } %><%= GetMessage("LegendText.PasswordConfirmation") %></td>
							<td width="60%">
								<asp:TextBox runat="server" ValidationGroup="vgInnerForm" ID="tbNewPasswordConf"
									TextMode="Password"></asp:TextBox>
								<asp:CompareValidator runat="server" ErrorMessage="<%$ Loc:ErrorMessage.PasswordAndItsConfirmationDontMatch %>"
									ControlToValidate="tbNewPasswordConf" CultureInvariantValues="True" ValidationGroup="vgInnerForm"
									ControlToCompare="tbNewPassword" ID="cvNewPasswordConf" Display="Dynamic">*</asp:CompareValidator><asp:RequiredFieldValidator
										runat="server" ErrorMessage="<%$ Loc:ErrorMessage.PasswordConfirmationIsRequired %>"
										ControlToValidate="tbNewPasswordConf" ValidationGroup="vgInnerForm" ID="rfvNewPasswordConf"
										Display="Dynamic">*</asp:RequiredFieldValidator><asp:CustomValidator runat="server"
											ValidateEmptyText="True" ClientValidationFunction="CheckNewPasswordConf" ErrorMessage="<%$ Loc:ErrorMessage.PasswordConfirmationIsRequiredForPasswordChange %>"
											ControlToValidate="tbNewPasswordConf" ValidationGroup="vgInnerForm" ID="cfNewPasswordConf"
											Display="Dynamic" OnServerValidate="cfNewPasswordConf_ServerValidate">*</asp:CustomValidator>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<span class="required">*</span>E-Mail:</td>
							<td width="60%">
								<asp:TextBox runat="server" ValidationGroup="vgInnerForm" ID="tbEmail"></asp:TextBox>
								<asp:RequiredFieldValidator runat="server" ErrorMessage="<%$ Loc:ErrorMessage.EmailIsRequired %>"
									ControlToValidate="tbEmail" ValidationGroup="vgInnerForm" ID="RequiredFieldValidator3"
									Display="Dynamic">*</asp:RequiredFieldValidator><asp:RegularExpressionValidator runat="server"
										ErrorMessage="<%$ Loc:ErrorMessage.EmailIsInvalid %>" ControlToValidate="tbEmail"
										ValidationGroup="vgInnerForm" ID="RegularExpressionValidator1" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
										Display="Dynamic">*</asp:RegularExpressionValidator>
							</td>
						</tr>
						<tr runat="server" id="trPasswordQuestion" valign="top">
							<td runat="server" class="field-name" width="40%">
								<%= GetMessage("LegendText.SecretQuestion") %>
							</td>
							<td runat="server" width="60%">
								<asp:TextBox runat="server" ValidationGroup="vgInnerForm" ID="tbPasswordQuestion"></asp:TextBox>
							</td>
						</tr>
						<tr runat="server" id="trPasswordAnswer" valign="top">
							<td runat="server" class="field-name" width="40%">
								<%= GetMessage("LegendText.AnswerToSecretQuestion") %>
							</td>
							<td runat="server" width="60%">
								<asp:TextBox runat="server" ValidationGroup="vgInnerForm" ID="tbAnswer"></asp:TextBox>
							</td>
						</tr>
						<tr runat="server" id="trLastPasswordChangedDate" valign="top">
							<td runat="server" class="field-name" width="40%">
								<%= GetMessage("DateOfLastPasswordChange") %>
							</td>
							<td runat="server" width="60%">
								<asp:Label runat="server" ID="lbLastPasswordChangedDate"></asp:Label>
							</td>
						</tr>
					</table>
				</bx:BXTabControlTab>
				<bx:BXTabControlTab runat="server" ID="RolesTab" Title="<%$ Loc:TabTitle.Roles %>"
					Text="<%$ Loc:TabTitle.Roles %>">
					<table runat="server" id="tblRoles" class="internal" cellspacing="0" cellpadding="0"
						align="center" border="0">
						<tr runat="server" class="heading">
							<td runat="server" align="center" width="0%" colspan="2">
								&nbsp;</td>
							<td runat="server" style="padding-left: 10px" width="0%" colspan="2">
								<%= GetMessage("LegendText.PreiodOfActivity") %>
							</td>
						</tr>
					</table>
				</bx:BXTabControlTab>
				<bx:BXTabControlTab runat="server" ID="PersonalInfoTab" Title="<%$ Loc:TabTitle.PersonalInformation %>"
					Text="<%$ Loc:TabTitle.PersonalInformation %>">
					<table class="edit-table" cellspacing="0" cellpadding="0" border="0">
						<tbody>
							<tr valign="top">
								<td class="field-name" width="40%">
									<%= GetMessage("LegendText.UserImage") %>:</td>
								<td width="60%">
									<bx:AdminImageField ID="aifImage" runat="server" ShowDescription="false" />
								</td>
							</tr>
							<tr valign="top">
								<td class="field-name" width="40%">
									<%= GetMessage("LegendText.DisplayName") %>:</td>
								<td width="60%">
									<asp:TextBox runat="server" ID="tbDisplayName" /></td>
							</tr>
							<tr valign="top">
								<td class="field-name" width="40%">
									<%= GetMessage("LegendText.FirstName") %>
								</td>
								<td width="60%">
									<asp:TextBox runat="server" ValidationGroup="vgInnerForm" ID="tbFirstName"></asp:TextBox>
								</td>
							</tr>
							<tr valign="top">
								<td class="field-name" width="40%">
									<%= GetMessage("LegendText.SecondName") %>
								</td>
								<td width="60%">
									<asp:TextBox runat="server" ValidationGroup="vgInnerForm" ID="tbSecondName"></asp:TextBox>
								</td>
							</tr>
							<tr valign="top">
								<td class="field-name" width="40%">
									<%= GetMessage("LegendText.LastName") %>
								</td>
								<td width="60%">
									<asp:TextBox runat="server" ValidationGroup="vgInnerForm" ID="tbLastName"></asp:TextBox>
								</td>
							</tr>
							<tr valign="top">
								<td class="field-name" width="40%">
									<%= GetMessage("LegendText.Gender") %>:
								</td>
								<td width="60%">
									<asp:DropDownList runat="server" ValidationGroup="vgInnerForm" ID="ddlGender">
										<asp:ListItem Selected="True" Text="<%$ LocRaw:Kernel.Unknown %>" Value="Unknown" />
										<asp:ListItem Text="<%$ LocRaw:Option.GenderMale %>" Value="Male" />
										<asp:ListItem Text="<%$ LocRaw:Option.GenderFemale %>" Value="Female" />
									</asp:DropDownList>
								</td>
							</tr>
							<tr valign="top">
								<td class="field-name" width="40%">
									<%= GetMessage("LegendText.Birthdate") %>
								</td>
								<td width="60%">
									<asp:TextBox runat="server" ValidationGroup="vgInnerForm" ID="tbBirthdayDate"></asp:TextBox><bx:Calendar
										ID="Calendar1" runat="server" TextBoxId="tbBirthdayDate" />
								</td>
							</tr>
							<tr valign="top">
								<td class="field-name" width="40%">
									<%= GetMessage("LegendText.DefaultSite") %>
								</td>
								<td width="60%">
									<asp:DropDownList runat="server" ValidationGroup="vgInnerForm" ID="ddlSite">
									</asp:DropDownList>
								</td>
							</tr>
						</tbody>
					</table>
				</bx:BXTabControlTab>
				<bx:BXTabControlTab runat="server" ID="NotesTab" Title="<%$ Loc:TabTitle.Notes %>"
					Text="<%$ Loc:TabTitle.Notes %>">
					<asp:TextBox runat="server" Width="460px" Height="110px" ValidationGroup="vgInnerForm"
						ID="tbComment" TextMode="MultiLine"></asp:TextBox>
				</bx:BXTabControlTab>
				<bx:BXTabControlTab runat="server" ID="ExtensionTab" Title="<%$ LocRaw:TabTitle.AdditionalProperties %>"
					Text="<%$ LocRaw:TabText.AdditionalProperties %>" Visible="false" >
				</bx:BXTabControlTab>
				<bx:BXTabControlTab runat="server" ID="CustomFieldsTab" Text="<%$ LocRaw:TabText.CustomProperties %>"
					Title="<%$ LocRaw:TabTitle.CustomProperties %>">
					<asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
						<ContentTemplate>
							<uc1:CustomFieldList ID="CustomFieldList1" runat="server" ValidationGroup="vgInnerForm"
								EntityId="USER" EditMode="true"></uc1:CustomFieldList>
						</ContentTemplate>
					</asp:UpdatePanel>
				</bx:BXTabControlTab>
			</bx:BXTabControl>
</asp:Content>
