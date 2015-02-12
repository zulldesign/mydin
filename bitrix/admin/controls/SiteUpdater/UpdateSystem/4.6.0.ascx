<%@ Control Language="C#" AutoEventWireup="true" CodeFile="4.6.0.ascx.cs" Inherits="bitrix_admin_controls_SiteUpdater_UpdateSystem_4_6_0" %>
<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" runat="server">
	<Items>
		<bx:BXCmSeparator ID="BXCmSeparator1" runat="server">
		</bx:BXCmSeparator>
		<bx:BXCmImageButton ID="BXCmImageButton1" runat="server"
			CommandName="Refresh" Text="<%$ Loc:ActionText.CheckForUpdates %>" OnClickScript="location.reload(true); return false;" Href="javascript: void(0)"/>
		<bx:BXCmSeparator ID="BXCmSeparator2" runat="server">
		</bx:BXCmSeparator>
		<bx:BXCmImageButton ID="BXCmImageButton2" runat="server"
			CommandName="ChangeSettings" Text="<%$ Loc:ActionText.ChangeSetings %>" Href="UpdateSystemSettings.aspx" >
		</bx:BXCmImageButton>
	</Items>
</bx:BXContextMenuToolbar>
<br />

<asp:Panel ID="RegisterPanel" runat="server" Width="100%">
	<asp:UpdatePanel ID="UpdatePanel3" runat="server">
		<ContentTemplate>
			<table class="BXUpdateSystem" width="100%" cellpadding="3" cellspacing="0">
				<tr>
					<td class="HasUpdatesTop"><div class="icon icon-update">&nbsp;</div></td>
					<td colspan="3" class="HasUpdatesTop">
						&nbsp;<%= GetMessage("RegisterYourCopy") %></td>
				</tr>
				<tr>
					<td width="30" class="HasUpdatesBody">
					</td>
					<td width="100%" class="HasUpdatesBody">
						<asp:Label ID="lbRegisterError" runat="server" CssClass="BXUpdateSystemError" Visible="False"></asp:Label>
						<strong><%= GetMessage("RegisterYourCopyUntilExpirationOfTrailPeriod") %></strong>
						<br /><br /><%= GetMessage("InstallLatestProductUpdates") %></td>
					<td width="30" class="HasUpdatesBody">&nbsp;&nbsp;&nbsp;&nbsp;</td>
					<td width="0%" align="right" valign="top" class="HasUpdatesBody">
						<br />
						<asp:Button ID="btnRegister" ValidationGroup="vgRegister" runat="server" Text="<%$ Loc:ButtonText.Register %>" OnClick="btnRegister_Click" /><br />
						<asp:UpdateProgress ID="UpdateProgress4" runat="server" AssociatedUpdatePanelID="UpdatePanel3">
							<ProgressTemplate>
								<div class="BXUpdateSystem" style="position:relative; left:-20px; top:-70px; z-index: 1000; cursor: wait;">
									<br />
									<nobr>&nbsp;&nbsp;<img src="../../bitrix/images/update_progressbar.gif" alt="<%= GetMessage("ImgAlt.PleaseWaitForCompletionOfRegistration") %>" />&nbsp;&nbsp;</nobr><br />
									<nobr>&nbsp;&nbsp;<%= GetMessage("PleaseWaitForCompletionOfOperation") %>&nbsp;&nbsp;</nobr>
									<br />
								</div>
							</ProgressTemplate>
						</asp:UpdateProgress>
					</td>
				</tr>
			</table>
		</ContentTemplate>
	</asp:UpdatePanel>
	<br /><br />
</asp:Panel>

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
	<ContentTemplate>

		<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">

			<asp:View ID="ViewMain" runat="server">
				<table class="BXUpdateSystem" width="100%" cellpadding="3" cellspacing="0">
					<tr>
						<td class="HasUpdatesTop"><div class="icon icon-update">&nbsp;</div></td>
						<td colspan="3" class="HasUpdatesTop">
							&nbsp;<%= GetMessage("InstallUpdatesForYourSystem") %></td>
					</tr>
					<tr>
						<td width="30" class="HasUpdatesBody" style="height: 163px">
						</td>
						<td width="100%" class="HasUpdatesBody" style="height: 163px">
							<strong><%= GetMessage("Total") %> 
							<asp:Label ID="lbImportantUpdates" runat="server"></asp:Label></strong><br />
								<asp:Label ID="lbOptionalUpdates" runat="server"></asp:Label><br />
							<br />
							<%= GetMessage("InstallLatestProductUpdates") %></td>
						<td width="30" class="HasUpdatesBody" style="height: 163px">&nbsp;&nbsp;&nbsp;&nbsp;</td>
						<td width="0%" align="right" valign="top" class="HasUpdatesBody" style="height: 163px">
							<br />
							<asp:Button ID="btnInstallUpdates" ValidationGroup="vgInstallUpdates" runat="server" Text="<%$ Loc:ButtonText.InstallUpdates %>" OnClick="btnInstallUpdates_Click" /><br />
							<asp:LinkButton ID="btnViewUpdates" runat="server" ValidationGroup="vgInstallUpdates" Font-Size="X-Small" OnClick="btnViewUpdates_Click"><%= GetMessage("ViewAvailableUpdates") %></asp:LinkButton>
							<asp:UpdateProgress ID="UpdateProgress2" runat="server" AssociatedUpdatePanelID="UpdatePanel1">
								<ProgressTemplate>
									<div class="BXUpdateSystem" style="position:relative; left:-20px; top:-70px; z-index: 1000; cursor: wait;">
										<br />
										<nobr>&nbsp;&nbsp;<img src="../../bitrix/images/update_progressbar.gif" alt="<%= GetMessage("PleaseWaitForCompletionOfUpdatesSetup") %>" />&nbsp;&nbsp;</nobr><br />
										<nobr>&nbsp;&nbsp;<%= GetMessage("PleaseWaitForCompletionOfOperation") %>&nbsp;&nbsp;</nobr>
										<br />
									</div>
								</ProgressTemplate>
							</asp:UpdateProgress>
						</td>
					</tr>
				</table>
			</asp:View>

			<asp:View ID="ViewNoUpdates" runat="server">
				<table class="BXUpdateSystem" width="100%" cellpadding="3" cellspacing="0">
					<tr>
						<td class="NoUpdatesTop"><div class="icon icon-update">&nbsp;</div></td>
						<td colspan="3" class="NoUpdatesTop">
							&nbsp;<%= GetMessage("YourProductIsUpToDate") %></td>
					</tr>
					<tr>
						<td width="30" class="NoUpdatesBody">
						</td>
						<td width="100%" class="NoUpdatesBody">
							<asp:Label ID="lbOptionalNoUpdates" runat="server"></asp:Label><br />
							<br />
							<%= GetMessage("ThereAreNoAvailableUpdatesForYourProduct") %></td>
						<td width="30" class="NoUpdatesBody">&nbsp;&nbsp;&nbsp;&nbsp;</td>
						<td width="0%" align="right" valign="top" class="NoUpdatesBody">
							<br />
					</tr>
				</table>
			</asp:View>

			<asp:View ID="ViewDownload" runat="server">
				<table class="BXUpdateSystem" width="100%" cellpadding="3" cellspacing="0">
					<tr>
						<td class="HasUpdatesTop"><div class="icon icon-update">&nbsp;</div></td>
						<td colspan="3" class="HasUpdatesTop">
							&nbsp;<%= GetMessage("PleaseLoadUpdatesForYourProduct") %></td>
					</tr>
					<tr>
						<td width="30" class="HasUpdatesBody">
						</td>
						<td width="100%" class="HasUpdatesBody">
							<strong><%= GetMessage("Total") %> 
							<asp:Label ID="lbImportantUpdatesDld" runat="server"></asp:Label></strong><br />
								<asp:Label ID="lbOtherUpdatesDld" runat="server"></asp:Label><br />
							<br />
							<%= GetMessage("InstallLatestProductUpdates") %></td>
						<td width="30" class="HasUpdatesBody">&nbsp;&nbsp;&nbsp;&nbsp;</td>
						<td width="0%" align="right" valign="top" class="HasUpdatesBody">
							<asp:Button ID="btnDownload" runat="server" ValidationGroup="vgDownloadUpdates" Text="<%$ Loc:ButtonText.LoadUpdates %>" OnClick="btnDownload_Click" />
							<asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="UpdatePanel1">
								<ProgressTemplate>
									<div class="BXUpdateSystem" style="position:relative; left:-20px; top:-50px; z-index: 1000; cursor: wait;">
										<br />
										<nobr>&nbsp;&nbsp;<img src="../../bitrix/images/update_progressbar.gif" alt="<%= GetMessage("ImgAlt.PleaseWaitForCompletionOfLoadindUpdates") %>" />&nbsp;&nbsp;</nobr><br />
										<nobr>&nbsp;&nbsp;<%= GetMessage("PleaseWaitForCompletionOfOperation") %>&nbsp;&nbsp;</nobr>
										<br />
									</div>
								</ProgressTemplate>
							</asp:UpdateProgress>
							<br />
							<br />
					</tr>
				</table>
			</asp:View>

			<asp:View ID="ViewList" runat="server">
				<asp:HiddenField ID="hfViewListModules" runat="server" />
				<asp:HiddenField ID="hfViewListLangs" runat="server" />
				<table class="BXUpdateSystem" id="ViewListTable" runat="server" width="100%" cellpadding="3" cellspacing="0">
					<tr>
						<td class="ListTop">
							<input id="ViewListAllCheckbox" runat="server" checked="checked" type="checkbox" title="<%$ Loc:CheckBoxTitle.SelectAll %>" onclick="ViewListSelectAllRows(this);"/>
						</td>
						<td class="ListTop">
							<%= GetMessage("Title") %>
						</td>
						<td class="ListTop">
							<%= GetMessage("Type") %>
						</td>
						<td class="ListTop">
							<%= GetMessage("Version") %>
						</td>
						<td class="ListTopRight">
							<%= GetMessage("Description") %>
						</td>
					</tr>
				</table>
				<br /><br />
				<asp:Button ID="ViewListInstallButton" ValidationGroup="vgInstallSelectUpdates" runat="server" Text="<%$ Loc:ButtonText.InstallUpdates %>" OnClick="ViewListInstallButton_Click" />
				&nbsp;&nbsp;&nbsp;
				<asp:Button ID="ViewListCancelButton" runat="server" Text="<%$ Loc:ButtonText.Cancel %>" ValidationGroup="vgInstallSelectUpdates" OnClick="ViewListCancelButton_Click" /></asp:View>

			<asp:View ID="ViewFinish" runat="server">
				<table class="BXUpdateSystem" width="100%" cellpadding="3" cellspacing="0">
					<tr>
						<td class="FinishTop"><div class="icon icon-update">&nbsp;</div></td>
						<td class="FinishTop" colspan="3">
							&nbsp;<%= GetMessage("UpdatesHaveBeenInstalledSuccessfully") %></td>
					</tr>
					<tr>
						<td style="width: 30px" class="FinishBody">
						</td>
						<td width="100%" class="FinishBody">
							<%= GetMessage("Legend.Updated") %><br/>
							<asp:Label ID="lbInstalledUpdates" runat="server"></asp:Label></td>
						<td width="30" class="FinishBody">&nbsp;&nbsp;&nbsp;&nbsp;</td>
						<td width="0%" align="right" valign="top" class="FinishBody">
							<br />
							<br /><br /></td>
					</tr>
				</table>
			</asp:View>

			<asp:View ID="ViewError" runat="server">
				<table class="BXUpdateSystem" width="100%" cellpadding="3" cellspacing="0">
					<tr>
						<td class="ErrorTopU"><div class="icon icon-update">&nbsp;</div></td>
						<td class="ErrorTopU" colspan="3">
							&nbsp;<%= GetMessage("UpdateError") %></td>
					</tr>
					<tr>
						<td style="width: 40px" class="ErrorBody">
						</td>
						<td width="100%" class="ErrorBody">
							<asp:Label ID="lbUpdateError" runat="server"></asp:Label><br />
							<br />
							<br />
							<%= string.Format(
							    GetMessage("FormatUpdateSystemSetupComment"),
							    "<a href=\"UpdateSystemSettings.aspx\">", "</a>"
							    )%>
						</td>
						<td width="30" class="ErrorBody">&nbsp;&nbsp;&nbsp;&nbsp;</td>
						<td width="0%" align="right" valign="top" class="ErrorBody">
							<br />
							<br /><br /></td>
					</tr>
				</table>
			</asp:View>

			<asp:View ID="ViewSelfUpdate" runat="server">
				<table class="BXUpdateSystem" width="100%" cellpadding="3" cellspacing="0">
					<tr>
						<td class="HasUpdatesTop"><div class="icon icon-update">&nbsp;</div></td>
						<td colspan="3" class="HasUpdatesTop">
							&nbsp;<%= GetMessage("PleaseUpdateYourUpdateSystem") %></td>
					</tr>
					<tr>
						<td width="30" class="HasUpdatesBody">
						</td>
						<td width="100%" class="HasUpdatesBody">
							<strong><%= GetMessage("BeforeUsingOfUpdateSystemYouHaveToInstallItLatestVersion") %></strong>
							<br /><br /><%= GetMessage("InstallLatestProductUpdates") %></td>
						<td width="30" class="HasUpdatesBody">&nbsp;&nbsp;&nbsp;&nbsp;</td>
						<td width="0%" align="right" valign="top" class="HasUpdatesBody" style="height: 163px">
							<br />
							<asp:Button ID="btnUpdateUpdate" ValidationGroup="vgUpdateUpdates" runat="server" Text="<%$ Loc:ButtonText.InstallUpdates %>" OnClick="btnUpdateUpdate_Click" /><br />
							<asp:UpdateProgress ID="UpdateProgress3" runat="server" AssociatedUpdatePanelID="UpdatePanel1">
								<ProgressTemplate>
									<div class="BXUpdateSystem" style="position:relative; left:-20px; top:-70px; z-index: 1000; cursor: wait;">
										<br />
										<nobr>&nbsp;&nbsp;<img src="../../bitrix/images/update_progressbar.gif" alt="<%= GetMessage("PleaseWaitForCompletionOfUpdatesSetup") %>" />&nbsp;&nbsp;</nobr><br />
										<nobr>&nbsp;&nbsp;<%= GetMessage("PleaseWaitForCompletionOfOperation") %>&nbsp;&nbsp;</nobr>
										<br />
									</div>
								</ProgressTemplate>
							</asp:UpdateProgress>
						</td>
					</tr>
				</table>
			</asp:View>

			<asp:View ID="ViewLicense" runat="server">
				<table class="BXUpdateSystem" width="100%" cellpadding="3" cellspacing="0">
					<tr>
						<td class="HasUpdatesTop"><div class="icon icon-update">&nbsp;</div></td>
						<td class="HasUpdatesTop">
							&nbsp;<%= GetMessage("NewLicenceAgreement") %></td>
					</tr>
					<tr>
						<td colspan="2" class="HasUpdatesBody">
							<strong><%= GetMessage("BeforeUsingYouMustAgreeTheTermsOfLicenceAgreement") %></strong>
							<br /><br />
							<iframe id="iframeLicenseAgreement" runat="server" src="" width="100%" />
							<br /><br />
							<asp:HiddenField ID="hfAgreeLicenceVersion" runat="server" />
							<asp:Button ID="btnAgreeLicence" ValidationGroup="vgAgreeLicense" runat="server" Text="<%$ Loc:ButtonText.IAgreeTheTermsOfLicenceAgreement %>" OnClick="btnAgreeLicence_Click"/>
						</td>
					</tr>
				</table>
			</asp:View>

			<asp:View ID="ViewActivate" runat="server">
				<table class="BXUpdateSystem" width="100%" cellpadding="3" cellspacing="0">
					<tr>
						<td class="HasUpdatesTop"><div class="icon icon-update">&nbsp;</div></td>
						<td width="100%" class="HasUpdatesTop">
							&nbsp;<%= GetMessage("ActivationOfLicenceKey") %></td>
					</tr>
					<tr>
						<td colspan="2" class="HasUpdatesBody">
							<strong><%= GetMessage("BeforeUsingOfUpdateSystemYouMustActivateLicenceKey") %></strong>
							<br /><br />

							<table cellspacing="3" cellpadding="3">
								<tr class="heading"><td colspan="2"><b>
									<br />
									<%= GetMessage("RegistrationInfo") %></b></td></tr>
								<tr>
									<td valign="top">
										<span class="required">*</span>
										<%= GetMessage("OwnerOfThisProductCopy") %>
									</td>
									<td valign="top">
										<asp:TextBox ID="tbActOwnerName" runat="server" 
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox><br/>
										<asp:RequiredFieldValidator ID="rfvActOwnerName" runat="server" 
											ControlToValidate="tbActOwnerName" Display="Dynamic" 
											ValidationGroup="vgKeyActivation"
											ErrorMessage="<%$ Loc:Message.InformationAboutOwnerIsNotSpecified %>"
											></asp:RequiredFieldValidator>
									</td>
									<td valign="top">
										<small><%= GetMessage("OrganizationOrPersonNameThatOwnsThisProductCopy") %></small>
									</td>
								</tr>
								<tr>
									<td valign="top"><span class="required">*</span><%= GetMessage("Legend.SiteAddress") %></td>
									<td valign="top">
										<asp:TextBox ID="tbActSiteUrl" runat="server" 
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox><br/>
										<asp:RequiredFieldValidator ID="rfvActSiteUrl" runat="server" ControlToValidate="tbActSiteUrl"
											ValidationGroup="vgKeyActivation"
											Display="Dynamic" ErrorMessage="<%$ Loc:Message.SiteAddressIsNotSpecified %>"></asp:RequiredFieldValidator>
									</td>
									<td valign="top">
										<small><%= GetMessage("Hint.SiteAddress") %></small>
									</td>
								</tr>
								<tr>
									<td valign="top"><span class="required">*</span><%= GetMessage("Legend.OwnerPhone") %></td>
									<td valign="top">
										<asp:TextBox ID="tbActOwnerPhone" runat="server" 
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox><br/>
										<asp:RequiredFieldValidator ID="rfvActOwnerPhone" runat="server" Display="Dynamic"
											ControlToValidate="tbActOwnerPhone" ValidationGroup="vgKeyActivation"
											ErrorMessage="<%$ Loc:Message.OwnerPhoneIsNotSpefied %>"
											></asp:RequiredFieldValidator>
									</td>
									<td valign="top">
										<small><%= GetMessage("Hint.OwnerPhone") %></small>
									</td>
								</tr>
								<tr>
									<td valign="top">
										<span class="required">*</span><%= GetMessage("Legend.Email") %>
									</td>
									<td valign="top">
										<asp:TextBox ID="tbActOwnerEMail" runat="server" 
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox><br/>
										<asp:RegularExpressionValidator ID="revActOwnerEMail" runat="server" 
											ControlToValidate="tbActOwnerEMail"
											Display="Dynamic" ErrorMessage="<%$ Loc:Message.SpecifiedEmailIsIncorrect %>" 
											ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
											ValidationGroup="vgKeyActivation"></asp:RegularExpressionValidator>
										<asp:RequiredFieldValidator ID="rfvActOwnerEMail" runat="server" Display="Dynamic"
											ControlToValidate="tbActOwnerEMail" ValidationGroup="vgKeyActivation"
											ErrorMessage="<%$ Loc:Message.EmailIsNotSpecified %>"></asp:RequiredFieldValidator>
									</td>
									<td valign="top">
										<small><%= GetMessage("Hint.Email") %></small>
									</td>
								</tr>
								<tr>
									<td valign="top">
										<span class="required">*</span><%= GetMessage("Legend.ContactPerson") %>
									</td>
									<td valign="top">
										<asp:TextBox ID="tbActContactPerson" runat="server" 
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox><br/>
										<asp:RequiredFieldValidator ID="rfvActContactPerson" runat="server" Display="Dynamic"
											ControlToValidate="tbActContactPerson" ValidationGroup="vgKeyActivation"
											ErrorMessage="<%$ Loc:Message.ContaclPersonIsNotSpecified %>"></asp:RequiredFieldValidator>
									</td>
									<td valign="top">
										<small><%= GetMessage("Hint.ContactPerson") %></small>
									</td>
								</tr>
								<tr>
									<td valign="top"><span class="required">*</span><%= GetMessage("Legend.ContactPersonEmail") %></td>
									<td valign="top">
										<asp:TextBox ID="tbActContactEMail" runat="server" 
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox><br/>
										<asp:RequiredFieldValidator ID="rfvActContactEMail" runat="server" Display="Dynamic"
											ControlToValidate="tbActContactEMail" ValidationGroup="vgKeyActivation"
											ErrorMessage="<%$ Loc:Message.EmailIsNotSpecified %>"></asp:RequiredFieldValidator>
										<asp:RegularExpressionValidator ID="revActContactEMail" runat="server" 
											ControlToValidate="tbActContactEMail" ValidationGroup="vgKeyActivation"
											ErrorMessage="<%$ Loc:Message.SpecifiedEmailIsIncorrect %>" Display="Dynamic"
											ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
											></asp:RegularExpressionValidator>
									</td>
									<td valign="top">
										<small><%= GetMessage("Hint.ContactPersonEmail") %></small>
									</td>
								</tr>
								<tr>
									<td valign="top"><span class="required">*</span><%= GetMessage("Legend.ContactPersonPhone") %></td>
									<td valign="top">
										<asp:TextBox ID="tbActContactPhone" runat="server" 
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox><br/>
										<asp:RequiredFieldValidator ID="rfvActContactPhone" runat="server" Display="Dynamic"
											ControlToValidate="tbActContactPhone" ValidationGroup="vgKeyActivation"
											ErrorMessage="<%$ Loc:Message.ContactPersonPhoneIsNotSpecified %>"></asp:RequiredFieldValidator>
									</td>
									<td valign="top">
										<small><%= GetMessage("Hint.ContactPersonPhone") %></small>
									</td>
								</tr>
								<tr>
									<td valign="top"><%= GetMessage("Legend.OwnerContactInfo") %></td>
									<td valign="top">
										<asp:TextBox ID="tbActContactInfo" runat="server" TextMode="MultiLine" 
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox><br/>
									</td>
									<td valign="top">
										<small><%= GetMessage("Hint.OwnerContactInfo") %></small>
									</td>
								</tr>
							</table>

							<table cellspacing="3" cellpadding="3">
								<tr class="heading"><td colspan="2"><b>
									<br />
									<%= GetMessage("Legend.UserOnProductSite") %></b></td></tr>
								<tr>
									<td colspan="2">
									<%= GetMessageRaw("FormatProductSiteRegistrationCommentFull") %>
									</td>
								</tr>
								<tr>
									<td colspan="2" align="center">
										<asp:CheckBox ID="cbActGenerateUser" ValidationGroup="vgKeyActivation" 
											runat="server" Checked="true"
											Text="<%$ Loc:CheckBoxText.CreateUserOnProductSite %>" />
									</td>
								</tr>
								<tr>
									<td valign="top"><span class="required">*</span><%= GetMessage("Legend.YourFirstName") %></td>
									<td valign="top">
										<asp:TextBox ID="tbActUserName" runat="server" 
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox>
										<asp:RequiredFieldValidator ID="rfvActUserName" runat="server" Display="Dynamic"
											ControlToValidate="tbActUserName" ValidationGroup="vgKeyActivation"
											ErrorMessage="<%$ Loc:Message.YourFirstNameIsNotSpecified %>"></asp:RequiredFieldValidator>
									</td>
								</tr>
								<tr>
									<td valign="top"><span class="required">*</span><%= GetMessage("Legend.YourLastName") %></td>
									<td valign="top">
										<asp:TextBox ID="tbActUserLastName" runat="server" 
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox>
										<asp:RequiredFieldValidator ID="rfvActUserLastName" runat="server" Display="Dynamic"
											ControlToValidate="tbActUserLastName" ValidationGroup="vgKeyActivation"
											ErrorMessage="<%$ Loc:Message.YourLastNameIsNotSpecified %>"></asp:RequiredFieldValidator>
									</td>
								</tr>
								<tr>
									<td valign="top"><span class="required">*</span><%= GetMessage("Legend.Login") %></td>
									<td valign="top">
										<asp:TextBox ID="tbActUserLogin" runat="server"
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox>
										<asp:RequiredFieldValidator ID="rfvActUserLogin" runat="server" Display="Dynamic"
											ControlToValidate="tbActUserLogin" ValidationGroup="vgKeyActivation"
											ErrorMessage="<%$ Loc:Message.LoginIsNotSpecified %>"></asp:RequiredFieldValidator>
									</td>
								</tr>
								<tr>
									<td valign="top"><span class="required">*</span><%= GetMessage("Legend.Password") %></td>
									<td valign="top">
										<asp:TextBox ID="tbActUserPassword" runat="server" TextMode="Password"
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox>
										<asp:RequiredFieldValidator ID="rfvActUserPassword" runat="server" Display="Dynamic"
											ControlToValidate="tbActUserPassword" ValidationGroup="vgKeyActivation"
											ErrorMessage="<%$ Loc:Message.PasswordIsNotSpecified %>"></asp:RequiredFieldValidator>
									</td>
								</tr>
								<tr>
									<td valign="top"><span class="required">*</span><%= GetMessage("Legend.PasswordConfirmation") %></td>
									<td valign="top">
										<asp:TextBox ID="tbActUserPasswordConf" runat="server" TextMode="Password"
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox>
										<asp:RequiredFieldValidator ID="rfvActUserPasswordConf" runat="server" Display="Dynamic"
											ControlToValidate="tbActUserPasswordConf" ValidationGroup="vgKeyActivation"
											ErrorMessage="<%$ Loc:Message.PasswordConfirmationIsNotSpecified %>"></asp:RequiredFieldValidator>
										<asp:CompareValidator ID="cvActUserPasswordConf" runat="server" Display="Dynamic"
											ControlToCompare="tbActUserPassword" ValidationGroup="vgKeyActivation"
											ControlToValidate="tbActUserPasswordConf" 
											ErrorMessage="<%$ Loc:Message.PasswordAndItsConfirmationDontMatch %>"></asp:CompareValidator>
									</td>
								</tr>
							</table>

							<br /><br />
							<asp:Button ID="btnActivate" ValidationGroup="vgKeyActivation" runat="server" 
								Text="<%$ Loc:ButtonText.ActivateLicenceKey %>" OnClick="btnActivate_Click"/>

							<asp:UpdateProgress ID="UpdateProgress5" runat="server" AssociatedUpdatePanelID="UpdatePanel1">
								<ProgressTemplate>
									<div class="BXUpdateSystem" style="position:relative; left:0px; top:-30px; z-index: 1000; cursor: wait;">
										<br />
										<nobr>&nbsp;&nbsp;<img src="../../bitrix/images/update_progressbar.gif" alt="<%= GetMessage("PleaseWaitForCompletionOfOperation") %>" />&nbsp;&nbsp;</nobr><br />
										<nobr>&nbsp;&nbsp;<%= GetMessage("PleaseWaitForCompletionOfOperation") %>&nbsp;&nbsp;</nobr>
										<br />
									</div>
								</ProgressTemplate>
							</asp:UpdateProgress>
						</td>
					</tr>
				</table>
			</asp:View>

		</asp:MultiView>
	</ContentTemplate>
</asp:UpdatePanel>

<br />
<br />

<asp:Panel ID="CouponPanel" runat="server" Width="100%">
	<asp:UpdatePanel ID="CouponUpdatePanel" runat="server">
		<ContentTemplate>
			<table class="BXUpdateSystem" width="100%" cellpadding="3" cellspacing="0">
				<tr>
					<td class="HasUpdatesTop"><div class="icon icon-update">&nbsp;</div></td>
					<td colspan="3" class="HasUpdatesTop"><%= GetMessage("ActivateCouponHeader") %></td>
				</tr>
				<tr>
					<td width="30" class="HasUpdatesBody">
					</td>
					<td width="100%" class="HasUpdatesBody">
						<%= GetMessage("ActivateCouponText") %>
						<br /><br />	
						<%= GetMessage("ActivateCouponTextBoxLabel") %>:<br />
						<asp:TextBox ID="CouponKey" runat="server" Width="230px" />
						<asp:RequiredFieldValidator runat="server" ID="CouponKeyRequired" ValidationGroup="vgCoupon" ControlToValidate="CouponKey" ErrorMessage="<%$ LocRaw:ActivateCouponValidatorMessage %>">*</asp:RequiredFieldValidator>
						<asp:Label ID="CouponError" runat="server" CssClass="BXUpdateSystemError" Visible="False" EnableViewState="false" />
						<asp:Label ID="CouponSuccess" runat="server" Font-Bold="true" Visible="False" EnableViewState="false"><br /><br /><%= GetMessage("ActivateCouponSuccessMessage") %></asp:Label>
					</td>
					<td width="30" class="HasUpdatesBody">&nbsp;&nbsp;&nbsp;&nbsp;</td>
					<td width="0%" align="right" valign="top" class="HasUpdatesBody">
						<br />
						<asp:Button ID="CouponSend" ValidationGroup="vgCoupon" runat="server" Text="<%$ LocRaw:ActivateCouponButtonTitle %>" OnClick="btnCoupon_Click" /><br />
						<asp:UpdateProgress ID="CouponProgress" runat="server" AssociatedUpdatePanelID="CouponUpdatePanel">
							<ProgressTemplate>
								<div class="BXUpdateSystem" style="position:relative; left:-20px; top:-70px; z-index: 1000; cursor: wait;">
									<br />
									<nobr>&nbsp;&nbsp;<img src="../../bitrix/images/update_progressbar.gif" alt="<%= GetMessage("PleaseWaitForCompletionOfOperation") %>" />&nbsp;&nbsp;</nobr><br />
									<nobr>&nbsp;&nbsp;<%= GetMessage("PleaseWaitForCompletionOfOperation") %>&nbsp;&nbsp;</nobr>
									<br />
								</div>
							</ProgressTemplate>
						</asp:UpdateProgress>
					</td>
				</tr>
			</table>
		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Panel>

<br />
<br />
<table width="100%" class="BXUpdateSystemNotes">
	<tr>
		<td nowrap="nowrap" width="0%">
			<%= GetMessage("Legend.LastUpdateWasPerformed") %>&nbsp;&nbsp;
		</td>
		<td width="100%">
			<asp:Label ID="lbLastCheckDate" runat="server"></asp:Label></td>
	</tr>
	<tr>
		<td nowrap="nowrap">
			<%= GetMessage("Legend.UpdatesWereInstalled") %>&nbsp;&nbsp;
		</td>
		<td>
			<asp:Label ID="lbLastUpdateDate" runat="server"></asp:Label>
			&nbsp;&nbsp;&nbsp;
		</td>
	</tr>
	<tr>
		<td nowrap="nowrap">
			<%= GetMessage("Legend.RegistredBy") %>&nbsp;&nbsp;
		</td>
		<td>
			<asp:Label ID="lbClientName" runat="server"></asp:Label></td>
	</tr>
	<tr>
		<td nowrap="nowrap">
			<%= GetMessage("Legend.ProductEdition") %>&nbsp;&nbsp;
		</td>
		<td>
			<asp:Label ID="lbEditionName" runat="server"></asp:Label></td>
	</tr>
	<tr>
		<td nowrap="nowrap">
			<%= GetMessage("Legend.QuantityOfSites") %>&nbsp;&nbsp;
		</td>
		<td>
			<asp:Label ID="lbSitesCount" runat="server"></asp:Label></td>
	</tr>
	<tr>
		<td nowrap="nowrap">
			<%= GetMessage("Legend.UpdatesAreAvailable") %>&nbsp;&nbsp;
		</td>
		<td>
			<asp:Label ID="lbUpdatesActivity" runat="server"></asp:Label></td>
	</tr>
	<tr>
		<td nowrap="nowrap">
			<%= GetMessage("Legend.ServerOfUpdates") %>&nbsp;&nbsp;
		</td>
		<td>
			<asp:Label ID="lbUpdatesServer" runat="server"></asp:Label></td>
	</tr>
</table>