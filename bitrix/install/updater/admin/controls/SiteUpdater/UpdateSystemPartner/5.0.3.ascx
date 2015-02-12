<%@ Control Language="C#" AutoEventWireup="true" CodeFile="5.0.3.ascx.cs" Inherits="bitrix_admin_controls_SiteUpdater_UpdateSystemPartner_5_0_3" %>
<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" runat="server">
	<Items>
		<bx:BXCmSeparator />
		<bx:BXCmImageButton ID="BXCmImageButton1" runat="server"
			Text="<%$ Loc:ActionText.CheckForUpdates %>" 
			OnClickScript="location.reload(true); return false;" 
			Href="javascript: void(0)"
		/>
		<bx:BXCmSeparator />
		<bx:BXCmImageButton ID="BXCmImageButton2" runat="server"
			Text="<%$ Loc:ActionText.ChangeSetings %>" 
			Href="UpdateSystemSettings.aspx?back_url=UpdateSystemPartner.aspx" 
		/>
	</Items>
</bx:BXContextMenuToolbar>
<br />

<script type="text/javascript">
function ViewListSelectAllRows(checkbox)
{
	var tbl = checkbox.parentNode.parentNode.parentNode.parentNode;
	var bChecked = checkbox.checked;
	var i;
	var n = tbl.rows.length;
	for (i = 1; i < n; i++)
	{
		var box = tbl.rows[i].cells[0].childNodes[0];
		if (box && box.tagName && box.tagName.toUpperCase() == 'INPUT' && box.type.toUpperCase() == 'CHECKBOX')
		{
			if (box.checked != bChecked && !box.disabled)
			{
				arTmp = box.id.split('_');
				if (arTmp[arTmp.length - 2] == 'cb')
					__ViewListClick('M', bChecked, arTmp[arTmp.length - 1]);
				else if (arTmp[arTmp.length - 2] == 'cbl')
					__ViewListClick('L', bChecked, arTmp[arTmp.length - 1]);
				box.checked = bChecked;
			}
		}
	}
	var installUpdatesSelButton = document.getElementById("<%= ViewListInstallButton.ClientID %>");
	installUpdatesSelButton.disabled = !bChecked;
}

function ViewListClick(t, checkbox, module)
{
	__ViewListClick(t, checkbox.checked, module);
	var tbl = checkbox.parentNode.parentNode.parentNode.parentNode;
	var bChecked = false;
	var i;
	var n = tbl.rows.length;
	for (i = 1; i < n; i++)
	{
		var box = tbl.rows[i].cells[0].childNodes[0];
		if (box && box.tagName && box.tagName.toUpperCase() == 'INPUT' && box.type.toUpperCase() == 'CHECKBOX')
		{
			if (box.checked && !box.disabled)
			{
				bChecked = true;
				break;
			}
		}
	}
	var installUpdatesSelButton = document.getElementById("<%= ViewListInstallButton.ClientID %>");
	installUpdatesSelButton.disabled = !bChecked;
	document.<%= Page.Form.ClientID %>.<%= ViewListAllCheckbox.ClientID %>.checked = bChecked;
}

function __ViewListClick(t, flag, module)
{
	var hf;
	if (t == 'M')
		hf = document.<%= Page.Form.ClientID %>.<%= hfViewListModules.ClientID %>;
	else if (t == 'L')
		hf = document.<%= Page.Form.ClientID %>.<%= hfViewListLangs.ClientID %>;
	var str = hf.value;
	var result = '';
	if (str.length > 0)
	{
		var arStr = str.split(',');
		for (var i = 0; i < arStr.length; i++)
		{
			if (arStr[i].length > 0 && arStr[i] != module)
			{
				if (result.length > 0)
					result += ',';
				result += arStr[i];
			}
		}
	}
	if (flag)
	{
		if (result.length > 0)
			result += ',';
		result += module;
	}
	hf.value = result;
}

var arModuleUpdatesDescr = {};

function ShowDescription(module)
{
	if (document.getElementById('updates_float_div'))
		CloseDescription();

	var div = document.body.appendChild(document.createElement('DIV'));
	div.id = 'updates_float_div';
	div.className = 'settings-float-form';
	div.style.position = 'absolute';
	div.innerHTML = arModuleUpdatesDescr[module];
	var left = parseInt(document.body.scrollLeft + document.body.clientWidth/2 - div.offsetWidth/2);
	var top = parseInt(document.body.scrollTop + document.body.clientHeight/2 - div.offsetHeight/2);
	jsFloatDiv.Show(div, left, top);
	jsUtils.addEvent(document, 'keypress', DescriptionOnKeyPress);
}

function DescriptionOnKeyPress(e)
{
	if (!e)
		e = window.event;
	if (!e)
		return;
	if (e.keyCode == 27)
		CloseDescription();
}

function CloseDescription()
{
	jsUtils.removeEvent(document, 'keypress', DescriptionOnKeyPress);
	var div = document.getElementById('updates_float_div');
	jsFloatDiv.Close(div);
	div.parentNode.removeChild(div);
}
</script>

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
				<bx:InlineScript runat="server" ID="Script" AsyncMode="ScriptBlock" >
				<script type="text/javascript">
					arModuleUpdatesDescr = <%= BuildUpdatesDescription() %>;
				</script>
				</bx:InlineScript>
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
						<td class="ErrorTopU" colspan="3">&nbsp;<%= GetMessage("UpdateError") %></td>
					</tr>
					<tr>
						<td style="width: 40px" class="ErrorBody">&nbsp;</td>
						<td width="100%" class="ErrorBody">
							<%= displayMessage %>
							<br />
							<br />
							<br />
							<%= string.Format(GetMessage("FormatUpdateSystemSetupComment"), "<a href=\"UpdateSystemSettings.aspx\">", "</a>") %>
						</td>
						<td width="30" class="ErrorBody">&nbsp;&nbsp;&nbsp;&nbsp;</td>
						<td width="0%" align="right" valign="top" class="ErrorBody"><br /><br /><br /></td>
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
		<td nowrap="nowrap" width="0%"><b><%= GetMessage("Legend.LicenseKeyHash") %>:</b></td>
		<td>&nbsp;&nbsp;</td>
		<td width="100%"><b><%= Encode(siteUpdater.Config.KeyMD5) %></b></td>
	</tr>
	<tr>
		<td nowrap="nowrap" width="0%"><%= GetMessage("Legend.LastUpdateWasPerformed") %></td>
		<td>&nbsp;&nbsp;</td>
		<td><%= Encode(siteUpdater.Config.LastUpdateCheck.ToString()) %></td>
	</tr>
	<tr>
		<td nowrap="nowrap"><%= GetMessage("Legend.UpdatesWereInstalled") %></td>
		<td>&nbsp;&nbsp;</td>
		<td><%= Encode(siteUpdater.Config.LastUpdateInstall.ToString()) %></td>
	</tr>
	<% if (siteUpdater.ServerManifest != null && siteUpdater.ServerManifest.Client != null) { %>
	<tr>
		<td nowrap="nowrap"><%= GetMessage("Legend.RegistredBy") %></td>
		<td>&nbsp;&nbsp;</td>
		<td><%= Encode(siteUpdater.ServerManifest.Client.name) %></td>
	</tr>
	<tr>
		<td nowrap="nowrap"><%= GetMessage("Legend.ProductEdition") %></td>
		<td>&nbsp;&nbsp;</td>
		<td><%= Encode(siteUpdater.ServerManifest.Client.license) %></td>
	</tr>
	<tr>
		<td nowrap="nowrap"><%= GetMessage("Legend.QuantityOfSites") %></td>
		<td>&nbsp;&nbsp;</td>
		<td><%= (siteUpdater.ServerManifest.Client.maxSites > 0) ? siteUpdater.ServerManifest.Client.maxSites.ToString() : GetMessage("WithoutLimits") %></td>
	</tr>
	<%
		string activity;
		if (siteUpdater.ServerManifest.Client.dateFrom != DateTime.MinValue && siteUpdater.ServerManifest.Client.dateTo != DateTime.MinValue)
			activity = GetMessageFormat("FormatFromTo", siteUpdater.ServerManifest.Client.dateFrom, siteUpdater.ServerManifest.Client.dateTo);
		else if (siteUpdater.ServerManifest.Client.dateFrom != DateTime.MinValue && siteUpdater.ServerManifest.Client.dateTo == DateTime.MinValue)
			activity = GetMessageFormat("FormatFrom", siteUpdater.ServerManifest.Client.dateFrom);
		else if (siteUpdater.ServerManifest.Client.dateFrom == DateTime.MinValue && siteUpdater.ServerManifest.Client.dateTo != DateTime.MinValue)
			activity = GetMessageFormat("FormatTo", siteUpdater.ServerManifest.Client.dateTo);
		else
			activity = GetMessage("WithoutLimits");
	%>
	<tr>
		<td nowrap="nowrap"><%= GetMessage("Legend.UpdatesAreAvailable") %></td>
		<td>&nbsp;&nbsp;</td>
		<td><%= activity %></td>
	</tr>
	<tr>
		<td nowrap="nowrap"><%= GetMessage("Legend.ServerOfUpdates") %></td>
		<td>&nbsp;&nbsp;</td>
		<td><%= Encode(siteUpdater.ServerManifest.Client.httpHost) %></td>
	</tr>
	<% } %>
</table>

<bx:BXAdminNote runat="server" ID="Note">
<%= GetMessageRaw("Note.Disclaimer") %>
</bx:BXAdminNote>