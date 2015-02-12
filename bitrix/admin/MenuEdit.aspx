<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="MenuEdit.aspx.cs" Inherits="bitrix_admin_MenuEdit" Title="<%$ Loc:PageTitle %>" EnableViewState="false" %> 
	
<%@ Register Src="~/bitrix/controls/Main/DirectoryBrowser.ascx" TagName="DirectoryBrowser" TagPrefix="uc1" %>
<asp:Content ID="mainContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<script>
				function ChType(ob)
				{
					window.location.href = 'MenuEdit.aspx?path=<%= JSEncode(UrlEncode(CurDir.TrimEnd('/'))) %>/' + ob[ob.selectedIndex].value + '.menu';
				}
				function SendToFileMan()
				{
					window.location.href = 'FileManEdit.aspx?path=<%= JSEncode(UrlEncode(Path)) %>'
						+ '&<%= Bitrix.Configuration.BXConfigurationUtility.Constants.BackUrl %>=<%= JSEncode(UrlEncode(Request.QueryString[Bitrix.Configuration.BXConfigurationUtility.Constants.BackUrl] ?? Request.RawUrl)) %>' 
						+ '<%= Bitrix.IO.BXSecureIO.FileExists(Path) ? string.Empty : "&new=" %>';
					return false;
			    }
			    function SendToAdvancedMode()
				{
					window.location.href = 'MenuEditEx.aspx?path=<%= JSEncode(UrlEncode(Path)) %>'
						+ '&<%= Bitrix.Configuration.BXConfigurationUtility.Constants.BackUrl %>=<%= JSEncode(UrlEncode(Request.QueryString[Bitrix.Configuration.BXConfigurationUtility.Constants.BackUrl] ?? Request.RawUrl)) %>';
					return false;
			    }
			    function SelectDirClick(target)
				{
					<%= SelectDirDialog.JavaScriptObject %>.MenuLinkTarget = document.getElementById(target);
					<%= SelectDirDialog.ContainerDialog.GetJSObjectName() %>.ShowPopupDialog(false, true);
					return false;
				}
				function SelectDirOK()
				{
					<%= SelectDirDialog.JavaScriptObject %>.MenuLinkTarget.value = <%= SelectDirDialog.JavaScriptObject %>.GetTargetValue();
					<%= SelectDirDialog.ContainerDialog.GetJSObjectName() %>.ClosePopupDialog();
					return false;
				}
				function SelectDirCancel()
				{
					<%= SelectDirDialog.ContainerDialog.GetJSObjectName() %>.ClosePopupDialog();
					return false;
				}
			            
	</script>
	
	<% SendToFileManButton.Href = String.Concat(
							"FileManEdit.aspx?path=", UrlEncode(Path), "&", 
							Bitrix.Configuration.BXConfigurationUtility.Constants.BackUrl, "=", UrlEncode(Request.QueryString[Bitrix.Configuration.BXConfigurationUtility.Constants.BackUrl] ?? Request.RawUrl), 
							Bitrix.IO.BXSecureIO.FileExists(Path) ? String.Empty : "&new="
						);

	SendToAdvancedModeButton.Href = String.Concat("MenuEditEx.aspx?path=", UrlEncode(Path), "&", 
							Bitrix.Configuration.BXConfigurationUtility.Constants.BackUrl, "=", UrlEncode(Request.QueryString[Bitrix.Configuration.BXConfigurationUtility.Constants.BackUrl] ?? Request.RawUrl)
						);


	BackButton.Href = BackUrl;
	%>				
	<bx:BXContextMenuToolbar ID="mainActionBar" runat="server" OnCommandClick="mainActionBar_CommandClick">
		<Items>
			<bx:BXCmSeparator runat="server" SectionSeparator="true" />
			<bx:BXCmImageButton runat="server" CssClass="context-button" ID="BackButton" CommandName="back" Text="<%$ Loc:Back %>"
				Title="<%$ Loc:Back %>" />
			<bx:BXCmSeparator runat="server" SectionSeparator="true" />
			<bx:BXCmImageButton runat="server" CssClass="context-button" CommandName="advanced_mode"
				Text="<%$ Loc:AdvancedMode %>" Title="<%$ Loc:AdvancedMode %>" ID="SendToAdvancedModeButton" />
			<bx:BXCmSeparator runat="server" SectionSeparator="true" />
			<bx:BXCmImageButton runat="server" CssClass="context-button" CommandName="edit_as_file"
				Text="<%$ Loc:EditAsFile %>" Title="<%$ Loc:EditAsFile %>" ID="SendToFileManButton" />
			<bx:BXCmSeparator SectionSeparator="true"  Visible="false" />
			<bx:BXCmImageButton runat="server" CssClass="context-button" CommandName="delete"  Visible="false"
				Text="<%$ Loc:DeleteMenu %>" Title="<%$ Loc:DeleteMenu %>" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" />
		</Items>
	</bx:BXContextMenuToolbar>
	<bx:BXMessage ID="resultMessage" runat="server" CssClass="Ok" IconClass="Ok" Visible="False" />
	<bx:BXTabControl ID="mainTabControl" runat="server" OnCommand="mainTabControl_Command">
		<bx:BXTabControlTab runat="server" Selected="True" Text="<%$ Loc:TabText %>" Title="<%$ Loc:TabTitle %>">
			<table cellspacing="0" class="edit-table">
				<tr>
					<td valign="top" class="field-name">
						<asp:Literal runat="server" ID="Literal4" Text="<%$ Loc:MenuType %>"></asp:Literal>:</td>
					<td valign="top">
						<asp:DropDownList runat="server" ID="ddlMenu" onchange="ChType(this);">
						</asp:DropDownList>
					</td>
				</tr>
			</table>
			<table cellspacing="0" class="edit-table">
				<tbody>
					<tr>
						<td>
							<table cellspacing="0" class="internal" width="100%" runat="server" id="tblItems">
								<tr class="heading" runat="server">
									<td align="center" runat="server">
										<asp:Literal runat="server" ID="Literal11" Text="<%$ Loc:ColTitle %>"></asp:Literal></td>
									<td align="center" runat="server">
										<asp:Literal ID="Literal1" runat="server" Text="<%$ Loc:ColLink %>"></asp:Literal></td>
									<td align="center" runat="server">
										<asp:Literal ID="Literal2" runat="server" Text="<%$ Loc:ColSort %>"></asp:Literal></td>
									<td align="center" runat="server">
										<asp:Literal ID="Literal3" runat="server" Text="<%$ Loc:ColDelete %>"></asp:Literal></td>
								</tr>
							</table>
						</td>
					</tr>
				</tbody>
			</table>
		</bx:BXTabControlTab>
	</bx:BXTabControl>
	<uc1:DirectoryBrowser 
		ID="SelectDirDialog" 
		runat="server" 
		ShowFiles="true" 
		ExtensionsList="aspx"
		ItemsToSelect="FilesAndFolders"
		AppendSlashToFolders="true"
		EnableExtras="false" 
		ShowDescription="true"
		DescriptionText="<%$ Loc:SelectDirDialog.Text.SelectPage %>" 
		WindowTitle="<%$ Loc:SelectDirDialog.Title.SelectPage %>" 
		OKClientScript="return SelectDirOK();"
		CancelClientScript="return SelectDirCancel();"
	/>
</asp:Content>
