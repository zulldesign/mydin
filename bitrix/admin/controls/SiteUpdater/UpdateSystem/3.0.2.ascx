<%@ Control Language="C#" AutoEventWireup="true" CodeFile="3.0.2.ascx.cs" Inherits="bitrix_admin_controls_SiteUpdater_UpdateSystem_3_0_2" %>
<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" runat="server">
	<Items>
		<bx:BXCmSeparator ID="BXCmSeparator1" runat="server">
		</bx:BXCmSeparator>
		<bx:BXCmImageButton ID="BXCmImageButton1" runat="server" CssClass="context-button icon" 
			CommandName="Refresh" Text="Проверить обновления" OnClickScript="location.reload(true);" />
		<bx:BXCmSeparator ID="BXCmSeparator2" runat="server">
		</bx:BXCmSeparator>
		<bx:BXCmImageButton ID="BXCmImageButton2" runat="server" CssClass="context-button icon"
			CommandName="ChangeSettings" Text="Изменить настройки" OnClickScript="window.location=&quot;UpdateSystemSettings.aspx&quot;">
		</bx:BXCmImageButton>
		<%--<bx:BXCmSeparator ID="BXCmSeparator3" runat="server">
		</bx:BXCmSeparator>
		<bx:BXCmImageButton ID="BXCmImageButton3" runat="server" CssClass="context-button icon"
			CommandName="ViewHistory" Text="Посмотреть историю обновлений" OnClickScript="window.location=&quot;UpdateSystemLog.aspx&quot;">
		</bx:BXCmImageButton>--%>
	</Items>
</bx:BXContextMenuToolbar>
<br />

<asp:Panel ID="RegisterPanel" runat="server" Width="100%">
	<asp:UpdatePanel ID="UpdatePanel3" runat="server">
		<ContentTemplate>
			<table class="BXUpdateSystem" width="100%" cellpadding="3" cellspacing="0">
				<tr>
					<td width="30" class="HasUpdatesTop">
						<img src="../images/updates_important.jpg" alt="Зарегистрируйте вашу копию" style="float: left" /></td>
					<td colspan="3" class="HasUpdatesTop">
						&nbsp;Зарегистрируйте вашу копию</td>
				</tr>
				<tr>
					<td width="30" class="HasUpdatesBody">
					</td>
					<td width="100%" class="HasUpdatesBody">
						<asp:Label ID="lbRegisterError" runat="server" CssClass="BXUpdateSystemError" Visible="False"></asp:Label>
						<strong>Зарегистрируйте вашу копию до окончания периода работы пробной версии продукта.</strong>
						<br /><br />
						Всегда устанавливайте последние обновления системы, чтобы получить новый функционал,
						усилить безопасность и оптимизировать скорость работы</td>
					<td width="30" class="HasUpdatesBody">&nbsp;&nbsp;&nbsp;&nbsp;</td>
					<td width="0%" align="right" valign="top" class="HasUpdatesBody">
						<br />
						<asp:Button ID="btnRegister" ValidationGroup="vgRegister" runat="server" Text="Зарегистрировать" OnClick="btnRegister_Click" /><br />
						<asp:UpdateProgress ID="UpdateProgress4" runat="server" AssociatedUpdatePanelID="UpdatePanel3">
							<ProgressTemplate>
								<div class="BXUpdateSystem" style="position:relative; left:-20px; top:-70px; z-index: 1000; cursor: wait;">
									<br />
									<nobr>&nbsp;&nbsp;<img src="../../bitrix/images/update_progressbar.gif" alt="Дождитесь окончания регистрации..." />&nbsp;&nbsp;</nobr><br />
									<nobr>&nbsp;&nbsp;Дождитесь окончания операции...&nbsp;&nbsp;</nobr>
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
						<td width="30" class="HasUpdatesTop">
							<img src="../images/updates_important.jpg" alt="Установите обновления для вашей системы" style="float: left" /></td>
						<td colspan="3" class="HasUpdatesTop">
							&nbsp;Установите обновления для вашей системы</td>
					</tr>
					<tr>
						<td width="30" class="HasUpdatesBody" style="height: 163px">
						</td>
						<td width="100%" class="HasUpdatesBody" style="height: 163px">
							<strong>Всего: 
							<asp:Label ID="lbImportantUpdates" runat="server"></asp:Label></strong><br />
								<asp:Label ID="lbOptionalUpdates" runat="server"></asp:Label><br />
							<br />
							Всегда устанавливайте последние обновления системы, чтобы получить новый функционал,
							усилить безопасность и оптимизировать скорость работы</td>
						<td width="30" class="HasUpdatesBody" style="height: 163px">&nbsp;&nbsp;&nbsp;&nbsp;</td>
						<td width="0%" align="right" valign="top" class="HasUpdatesBody" style="height: 163px">
							<br />
							<asp:Button ID="btnInstallUpdates" ValidationGroup="vgInstallUpdates" runat="server" Text="Установить обновления" OnClick="btnInstallUpdates_Click" /><br />
							<asp:LinkButton ID="btnViewUpdates" runat="server" ValidationGroup="vgInstallUpdates" Font-Size="X-Small" OnClick="btnViewUpdates_Click">Посмотреть доступные обновления</asp:LinkButton>
							<asp:UpdateProgress ID="UpdateProgress2" runat="server" AssociatedUpdatePanelID="UpdatePanel1">
								<ProgressTemplate>
									<div class="BXUpdateSystem" style="position:relative; left:-20px; top:-70px; z-index: 1000; cursor: wait;">
										<br />
										<nobr>&nbsp;&nbsp;<img src="../../bitrix/images/update_progressbar.gif" alt="Дождитесь окончания установки обновлений..." />&nbsp;&nbsp;</nobr><br />
										<nobr>&nbsp;&nbsp;Дождитесь окончания операции...&nbsp;&nbsp;</nobr>
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
						<td width="30" class="NoUpdatesTop">
							<img src="../images/updates_finish.jpg" style="float: left" /></td>
						<td colspan="3" class="NoUpdatesTop">
							&nbsp;Ваша система имеет последнюю версию</td>
					</tr>
					<tr>
						<td width="30" class="NoUpdatesBody">
						</td>
						<td width="100%" class="NoUpdatesBody">
							<asp:Label ID="lbOptionalNoUpdates" runat="server"></asp:Label><br />
							<br />
							Для вашей системы нет доступных важных обновлений</td>
						<td width="30" class="NoUpdatesBody">&nbsp;&nbsp;&nbsp;&nbsp;</td>
						<td width="0%" align="right" valign="top" class="NoUpdatesBody">
							<br />
							<%--<nobr><asp:LinkButton id="btnViewNoUpdates" runat="server" Font-Size="X-Small" __designer:wfdid="w42">Посмотреть доступные обновления</asp:LinkButton></nobr>--%></td>
					</tr>
				</table>
			</asp:View>

			<asp:View ID="ViewDownload" runat="server">
				<table class="BXUpdateSystem" width="100%" cellpadding="3" cellspacing="0">
					<tr>
						<td width="30" class="HasUpdatesTop">
							<img src="../images/updates_important.jpg" style="float: left" /></td>
						<td colspan="3" class="HasUpdatesTop">
							&nbsp;Загрузите обновления для вашей системы</td>
					</tr>
					<tr>
						<td width="30" class="HasUpdatesBody">
						</td>
						<td width="100%" class="HasUpdatesBody">
							<strong>Всего: 
							<asp:Label ID="lbImportantUpdatesDld" runat="server"></asp:Label></strong><br />
								<asp:Label ID="lbOtherUpdatesDld" runat="server"></asp:Label><br />
							<br />
							Всегда устанавливайте последние обновления системы, чтобы получить новый функционал,
							усилить безопасность и оптимизировать скорость работы</td>
						<td width="30" class="HasUpdatesBody">&nbsp;&nbsp;&nbsp;&nbsp;</td>
						<td width="0%" align="right" valign="top" class="HasUpdatesBody">
							<asp:Button ID="btnDownload" runat="server" ValidationGroup="vgDownloadUpdates" Text="Загрузить обновления" OnClick="btnDownload_Click" />
							<asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="UpdatePanel1">
								<ProgressTemplate>
									<div class="BXUpdateSystem" style="position:relative; left:-20px; top:-50px; z-index: 1000; cursor: wait;">
										<br />
										<nobr>&nbsp;&nbsp;<img src="../../bitrix/images/update_progressbar.gif" alt="Дождитесь окончания загрузки обновлений..." />&nbsp;&nbsp;</nobr><br />
										<nobr>&nbsp;&nbsp;Дождитесь окончания операции...&nbsp;&nbsp;</nobr>
										<br />
									</div>
								</ProgressTemplate>
							</asp:UpdateProgress>
							<br />
							<br />
							<%--<asp:LinkButton ID="lbtnViewUpdates" runat="server" Font-Size="X-Small">Посмотреть доступные обновления</asp:LinkButton>--%></td>
					</tr>
				</table>
			</asp:View>

			<asp:View ID="ViewList" runat="server">
				<asp:HiddenField ID="hfViewListModules" runat="server" />
				<asp:HiddenField ID="hfViewListLangs" runat="server" />
				<table class="BXUpdateSystem" id="ViewListTable" runat="server" width="100%" cellpadding="3" cellspacing="0">
					<tr>
						<td class="ListTop">
							<input id="ViewListAllCheckbox" runat="server" checked="checked" type="checkbox" title="Выделить все" onclick="ViewListSelectAllRows(this);"/>
						</td>
						<td class="ListTop">
							Название
						</td>
						<td class="ListTop">
							Тип
						</td>
						<td class="ListTop">
							Версия
						</td>
						<td class="ListTopRight">
							Описание
						</td>
					</tr>
				</table>
				<br /><br />
				<asp:Button ID="ViewListInstallButton" ValidationGroup="vgInstallSelectUpdates" runat="server" Text="Установить обновления" OnClick="ViewListInstallButton_Click" />
				&nbsp;&nbsp;&nbsp;
				<asp:Button ID="ViewListCancelButton" runat="server" Text="Отмена" ValidationGroup="vgInstallSelectUpdates" OnClick="ViewListCancelButton_Click" /></asp:View>

			<asp:View ID="ViewFinish" runat="server">
				<table class="BXUpdateSystem" width="100%" cellpadding="3" cellspacing="0">
					<tr>
						<td class="FinishTop">
							<img src="../images/updates_finish.jpg" style="float: left" /></td>
						<td class="FinishTop" colspan="3">
							&nbsp;Обновления были успешно установлены</td>
					</tr>
					<tr>
						<td style="width: 30px" class="FinishBody">
						</td>
						<td width="100%" class="FinishBody">
							Обновлены:<br/>
							<asp:Label ID="lbInstalledUpdates" runat="server"></asp:Label></td>
						<td width="30" class="FinishBody">&nbsp;&nbsp;&nbsp;&nbsp;</td>
						<td width="0%" align="right" valign="top" class="FinishBody">
							<br />
							<%--<nobr><a href="">История обновлений</a></nobr>--%><br /><br /></td>
					</tr>
				</table>
			</asp:View>

			<asp:View ID="ViewError" runat="server">
				<table class="BXUpdateSystem" width="100%" cellpadding="3" cellspacing="0">
					<tr>
						<td class="ErrorTopU" style="width: 40px"><img src="../images/updates_important.jpg" alt="Обновите систему обновлений" style="float: left" /></td>
						<td class="ErrorTopU" colspan="3">
							&nbsp;Ошибка обновления</td>
					</tr>
					<tr>
						<td style="width: 40px" class="ErrorBody">
						</td>
						<td width="100%" class="ErrorBody">
							<asp:Label ID="lbUpdateError" runat="server"></asp:Label></td>
						<td width="30" class="ErrorBody">&nbsp;&nbsp;&nbsp;&nbsp;</td>
						<td width="0%" align="right" valign="top" class="ErrorBody">
							<br />
							<%--<nobr><a href="">История обновлений</a></nobr>--%><br /><br /></td>
					</tr>
				</table>
			</asp:View>

			<asp:View ID="ViewSelfUpdate" runat="server">
				<table class="BXUpdateSystem" width="100%" cellpadding="3" cellspacing="0">
					<tr>
						<td width="30" class="HasUpdatesTop">
							<img src="../images/updates_important.jpg" alt="Обновите систему обновлений" style="float: left" /></td>
						<td colspan="3" class="HasUpdatesTop">
							&nbsp;Обновите систему обновлений</td>
					</tr>
					<tr>
						<td width="30" class="HasUpdatesBody">
						</td>
						<td width="100%" class="HasUpdatesBody">
							<strong>Перед использованием системы обновлений вам необходимо установить ее последнюю версию.</strong>
							<br /><br />
							Всегда устанавливайте последние обновления системы, чтобы получить новый функционал,
							усилить безопасность и оптимизировать скорость работы</td>
						<td width="30" class="HasUpdatesBody">&nbsp;&nbsp;&nbsp;&nbsp;</td>
						<td width="0%" align="right" valign="top" class="HasUpdatesBody" style="height: 163px">
							<br />
							<asp:Button ID="btnUpdateUpdate" ValidationGroup="vgUpdateUpdates" runat="server" Text="Установить обновления" OnClick="btnUpdateUpdate_Click" /><br />
							<asp:UpdateProgress ID="UpdateProgress3" runat="server" AssociatedUpdatePanelID="UpdatePanel1">
								<ProgressTemplate>
									<div class="BXUpdateSystem" style="position:relative; left:-20px; top:-70px; z-index: 1000; cursor: wait;">
										<br />
										<nobr>&nbsp;&nbsp;<img src="../../bitrix/images/update_progressbar.gif" alt="Дождитесь окончания установки обновлений..." />&nbsp;&nbsp;</nobr><br />
										<nobr>&nbsp;&nbsp;Дождитесь окончания операции...&nbsp;&nbsp;</nobr>
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
						<td width="30" class="HasUpdatesTop">
							<img src="../images/updates_important.jpg" alt="Новое лицензионное соглашение" style="float: left" /></td>
						<td class="HasUpdatesTop">
							&nbsp;Новое лицензионное соглашение</td>
					</tr>
					<tr>
						<td colspan="2" class="HasUpdatesBody">
							<strong>Перед использованием системы обновлений вам необходимо принять лицензионное соглашение.</strong>
							<br /><br />
							<iframe id="iframeLicenseAgreement" runat="server" src="" width="100%" />
							<br /><br />
							<asp:HiddenField ID="hfAgreeLicenceVersion" runat="server" />
							<asp:Button ID="btnAgreeLicence" ValidationGroup="vgAgreeLicense" runat="server" Text="Я принимаю лицензионное соглашение" OnClick="btnAgreeLicence_Click"/>
						</td>
					</tr>
				</table>
			</asp:View>

			<asp:View ID="ViewActivate" runat="server">
				<table class="BXUpdateSystem" width="100%" cellpadding="3" cellspacing="0">
					<tr>
						<td width="30" class="HasUpdatesTop">
							<img src="../images/updates_important.jpg" alt="Активация лицензионного ключа" style="float: left" /></td>
						<td width="100%" class="HasUpdatesTop">
							&nbsp;Активация лицензионного ключа</td>
					</tr>
					<tr>
						<td colspan="2" class="HasUpdatesBody">
							<strong>Перед использованием системы обновлений вам необходимо активировать лицензионный ключ.</strong>
							<br /><br />

							<table cellspacing="3" cellpadding="3">
								<tr class="heading"><td colspan="2"><b>
									<br />
									Регистрационная информация</b></td></tr>
								<tr>
									<td valign="top">
										<span class="required">*</span>
										Владелец данной копии:
									</td>
									<td valign="top">
										<asp:TextBox ID="tbActOwnerName" runat="server" 
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox><br/>
										<asp:RequiredFieldValidator ID="rfvActOwnerName" runat="server" 
											ControlToValidate="tbActOwnerName" Display="Dynamic" 
											ValidationGroup="vgKeyActivation"
											ErrorMessage="Не введено название владельца данной копии продукта"
											></asp:RequiredFieldValidator>
									</td>
									<td valign="top">
										<small>Название организации или имя и фамилия частного лица, владельца данной копии продукта</small>
									</td>
								</tr>
								<tr>
									<td valign="top"><span class="required">*</span>Адрес сайта:</td>
									<td valign="top">
										<asp:TextBox ID="tbActSiteUrl" runat="server" 
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox><br/>
										<asp:RequiredFieldValidator ID="rfvActSiteUrl" runat="server" ControlToValidate="tbActSiteUrl"
											ValidationGroup="vgKeyActivation"
											Display="Dynamic" ErrorMessage="Не введен адрес сайта"></asp:RequiredFieldValidator>
									</td>
									<td valign="top">
										<small>Все домены, которые будут использоваться для сайта, включая тестовые</small>
									</td>
								</tr>
								<tr>
									<td valign="top"><span class="required">*</span>Телефон владельца данной копии:</td>
									<td valign="top">
										<asp:TextBox ID="tbActOwnerPhone" runat="server" 
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox><br/>
										<asp:RequiredFieldValidator ID="rfvActOwnerPhone" runat="server" Display="Dynamic"
											ControlToValidate="tbActOwnerPhone" ValidationGroup="vgKeyActivation"
											ErrorMessage="Не введен телефон владельца данной копии продукта"
											></asp:RequiredFieldValidator>
									</td>
									<td valign="top">
										<small>Телефон владельца данной копии (организации или частного лица)</small>
									</td>
								</tr>
								<tr>
									<td valign="top">
										<span class="required">*</span>E-mail для связи по вопросам лицензирования и использования:
									</td>
									<td valign="top">
										<asp:TextBox ID="tbActOwnerEMail" runat="server" 
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox><br/>
										<asp:RegularExpressionValidator ID="revActOwnerEMail" runat="server" 
											ControlToValidate="tbActOwnerEMail"
											Display="Dynamic" ErrorMessage="Введен некорректный E-Mail адрес" 
											ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
											ValidationGroup="vgKeyActivation"></asp:RegularExpressionValidator>
										<asp:RequiredFieldValidator ID="rfvActOwnerEMail" runat="server" Display="Dynamic"
											ControlToValidate="tbActOwnerEMail" ValidationGroup="vgKeyActivation"
											ErrorMessage="Не введен E-Mail адрес"></asp:RequiredFieldValidator>
									</td>
									<td valign="top">
										<small>E-mail организации или частного лица для связи по вопросам лицензирования и использования копии продукта</small>
									</td>
								</tr>
								<tr>
									<td valign="top">
										<span class="required">*</span>Контактное лицо, ответственное за использование данной копии:
									</td>
									<td valign="top">
										<asp:TextBox ID="tbActContactPerson" runat="server" 
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox><br/>
										<asp:RequiredFieldValidator ID="rfvActContactPerson" runat="server" Display="Dynamic"
											ControlToValidate="tbActContactPerson" ValidationGroup="vgKeyActivation"
											ErrorMessage="Не указано контактное лицо"></asp:RequiredFieldValidator>
									</td>
									<td valign="top">
										<small>Имя и фамилия сотрудника, ответственного за использование данной копии продукта</small>
									</td>
								</tr>
								<tr>
									<td valign="top"><span class="required">*</span>E-mail контактного лица:</td>
									<td valign="top">
										<asp:TextBox ID="tbActContactEMail" runat="server" 
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox><br/>
										<asp:RequiredFieldValidator ID="rfvActContactEMail" runat="server" Display="Dynamic"
											ControlToValidate="tbActContactEMail" ValidationGroup="vgKeyActivation"
											ErrorMessage="Не введен E-Mail адрес"></asp:RequiredFieldValidator>
										<asp:RegularExpressionValidator ID="revActContactEMail" runat="server" 
											ControlToValidate="tbActContactEMail" ValidationGroup="vgKeyActivation"
											ErrorMessage="Введен некорректный E-Mail адрес" Display="Dynamic"
											ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
											></asp:RegularExpressionValidator>
									</td>
									<td valign="top">
										<small>E-mail контактного лица для связи по техническим вопросам, для отправки регистрационной информации и напоминаний о продлении техподдержки</small>
									</td>
								</tr>
								<tr>
									<td valign="top"><span class="required">*</span>Телефон контактного лица:</td>
									<td valign="top">
										<asp:TextBox ID="tbActContactPhone" runat="server" 
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox><br/>
										<asp:RequiredFieldValidator ID="rfvActContactPhone" runat="server" Display="Dynamic"
											ControlToValidate="tbActContactPhone" ValidationGroup="vgKeyActivation"
											ErrorMessage="Не введен телефон контактного лица"></asp:RequiredFieldValidator>
									</td>
									<td valign="top">
										<small>Телефон контактного лица</small>
									</td>
								</tr>
								<tr>
									<td valign="top">Контактная информация:</td>
									<td valign="top">
										<asp:TextBox ID="tbActContactInfo" runat="server" TextMode="MultiLine" 
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox><br/>
									</td>
									<td valign="top">
										<small>Адрес и прочие контактные данные владельца данной копии продукта</small>
									</td>
								</tr>
							</table>

							<table cellspacing="3" cellpadding="3">
								<tr class="heading"><td colspan="2"><b>
									<br />
									Пользователь на сайте продукта</b></td></tr>
								<tr>
									<td colspan="2">
										Если вы еще не зарегистрированы на сайте <a href="http://www.bitrixsoft.ru" target="_blank">www.bitrixsoft.ru</a>, убедитесь, что флаг "Создать пользователя" установлен, и введите свои данные (имя, фамилию, логин и пароль) в соответствующие поля формы. Регистрация на сайте www.bitrixsoft.ru даст вам возможность пользоваться автоматизированной службой <a href="http://www.bitrixsoft.ru/support/" target="_blank">технической поддержки</a> и <a href="http://www.bitrixsoft.ru/support/forum/" target="_blank">закрытым форумом</a> для решения возникающих вопросов.
									</td>
								</tr>
								<tr>
									<td colspan="2" align="center">
										<asp:CheckBox ID="cbActGenerateUser" ValidationGroup="vgKeyActivation" 
											runat="server" Checked="true"
											Text="Создать пользователя на сайте www.bitrixsoft.ru" />
									</td>
								</tr>
								<tr>
									<td valign="top"><span class="required">*</span>Ваше имя:</td>
									<td valign="top">
										<asp:TextBox ID="tbActUserName" runat="server" 
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox>
										<asp:RequiredFieldValidator ID="rfvActUserName" runat="server" Display="Dynamic"
											ControlToValidate="tbActUserName" ValidationGroup="vgKeyActivation"
											ErrorMessage="Не введено имя"></asp:RequiredFieldValidator>
									</td>
								</tr>
								<tr>
									<td valign="top"><span class="required">*</span>Ваша фамилия:</td>
									<td valign="top">
										<asp:TextBox ID="tbActUserLastName" runat="server" 
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox>
										<asp:RequiredFieldValidator ID="rfvActUserLastName" runat="server" Display="Dynamic"
											ControlToValidate="tbActUserLastName" ValidationGroup="vgKeyActivation"
											ErrorMessage="Не введена фамилия"></asp:RequiredFieldValidator>
									</td>
								</tr>
								<tr>
									<td valign="top"><span class="required">*</span>Логин (не менее 3 символов):</td>
									<td valign="top">
										<asp:TextBox ID="tbActUserLogin" runat="server"
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox>
										<asp:RequiredFieldValidator ID="rfvActUserLogin" runat="server" Display="Dynamic"
											ControlToValidate="tbActUserLogin" ValidationGroup="vgKeyActivation"
											ErrorMessage="Не введен логин"></asp:RequiredFieldValidator>
									</td>
								</tr>
								<tr>
									<td valign="top"><span class="required">*</span>Пароль:</td>
									<td valign="top">
										<asp:TextBox ID="tbActUserPassword" runat="server" TextMode="Password"
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox>
										<asp:RequiredFieldValidator ID="rfvActUserPassword" runat="server" Display="Dynamic"
											ControlToValidate="tbActUserPassword" ValidationGroup="vgKeyActivation"
											ErrorMessage="Не введен пароль"></asp:RequiredFieldValidator>
									</td>
								</tr>
								<tr>
									<td valign="top"><span class="required">*</span>Подтверждения пароля:</td>
									<td valign="top">
										<asp:TextBox ID="tbActUserPasswordConf" runat="server" TextMode="Password"
											ValidationGroup="vgKeyActivation" Width="230px"></asp:TextBox>
										<asp:RequiredFieldValidator ID="rfvActUserPasswordConf" runat="server" Display="Dynamic"
											ControlToValidate="tbActUserPasswordConf" ValidationGroup="vgKeyActivation"
											ErrorMessage="Не введено подтверждение пароля"></asp:RequiredFieldValidator>
										<asp:CompareValidator ID="cvActUserPasswordConf" runat="server" Display="Dynamic"
											ControlToCompare="tbActUserPassword" ValidationGroup="vgKeyActivation"
											ControlToValidate="tbActUserPasswordConf" 
											ErrorMessage="Пароль и его подтверждение не совпадают"></asp:CompareValidator>
									</td>
								</tr>
							</table>

							<br /><br />
							<asp:Button ID="btnActivate" ValidationGroup="vgKeyActivation" runat="server" 
								Text="Активировать лицензионый ключ" OnClick="btnActivate_Click"/>

							<asp:UpdateProgress ID="UpdateProgress5" runat="server" AssociatedUpdatePanelID="UpdatePanel1">
								<ProgressTemplate>
									<div class="BXUpdateSystem" style="position:relative; left:0px; top:-30px; z-index: 1000; cursor: wait;">
										<br />
										<nobr>&nbsp;&nbsp;<img src="../../bitrix/images/update_progressbar.gif" alt="Дождитесь окончания операции..." />&nbsp;&nbsp;</nobr><br />
										<nobr>&nbsp;&nbsp;Дождитесь окончания операции...&nbsp;&nbsp;</nobr>
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
<table width="100%" class="BXUpdateSystemNotes">
	<tr>
		<td nowrap="nowrap" width="0%">
			Последняя проверка обновлений произведена:&nbsp;&nbsp;
		</td>
		<td width="100%">
			<asp:Label ID="lbLastCheckDate" runat="server"></asp:Label></td>
	</tr>
	<tr>
		<td nowrap="nowrap">
			Обновления были установлены:&nbsp;&nbsp;
		</td>
		<td>
			<asp:Label ID="lbLastUpdateDate" runat="server"></asp:Label>
			&nbsp;&nbsp;&nbsp;<%--<a href="">История обновлений</a>--%>
		</td>
	</tr>
	<tr>
		<td nowrap="nowrap">
			Зарегистрировано на имя:&nbsp;&nbsp;
		</td>
		<td>
			<asp:Label ID="lbClientName" runat="server"></asp:Label></td>
	</tr>
	<tr>
		<td nowrap="nowrap">
			Редакция продукта:&nbsp;&nbsp;
		</td>
		<td>
			<asp:Label ID="lbEditionName" runat="server"></asp:Label></td>
	</tr>
	<tr>
		<td nowrap="nowrap">
			Количество сайтов:&nbsp;&nbsp;
		</td>
		<td>
			<asp:Label ID="lbSitesCount" runat="server"></asp:Label></td>
	</tr>
	<tr>
		<td nowrap="nowrap">
			Обновления доступны:&nbsp;&nbsp;
		</td>
		<td>
			<asp:Label ID="lbUpdatesActivity" runat="server"></asp:Label></td>
	</tr>
	<tr>
		<td nowrap="nowrap">
			Сервер обновлений:&nbsp;&nbsp;
		</td>
		<td>
			<asp:Label ID="lbUpdatesServer" runat="server"></asp:Label></td>
	</tr>
</table>