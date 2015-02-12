<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="FileMan.aspx.cs" Inherits="bitrix_admin_FileMan" Title="<%$ Loc:PageTitle %>"
	ValidateRequest="false" EnableViewState="false" %>

<%@ Import Namespace="System.Collections.Generic" %>
		
<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.IO" %>
<%@ Import Namespace="Bitrix.Services" %>
<%@ Import Namespace="Bitrix.Services.Js" %>
<%@ Import Namespace="Bitrix.Main" %>

<%@ Register Src="~/bitrix/controls/Main/DirectoryBrowser.ascx" TagName="DirectoryBrowser" TagPrefix="bx" %>


<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<asp:UpdatePanel runat="server" ID="UpdatePanel1">
		<ContentTemplate>
			<bx:InlineScript runat="server" ID="Script" AsyncMode="ScriptBlock" >
			<script type="text/javascript">
				function fileMan_PathAction(page)
				{
					location.href = page + '<%= BXJSUtility.Encode("?path=" + HttpUtility.UrlEncode(curPath)) %>';
				}
				function fileMan_NewFileAction(page)
				{
					location.href = '<%= BXJSUtility.Encode("FileManEdit.aspx?path=" + HttpUtility.UrlEncode(curPath)) %>&new=';
				}
				function fileMan_GotoPathAction()
				{
					var tb = document.getElementById('<%= GotoPath.ClientID %>')
					location.href =
						'FileMan.aspx?path=' 
						+ encodeURIComponent(tb.value) 
						+ '&role='
						+ '<%= BXJSUtility.Encode(HttpUtility.UrlEncode(rolesList.SelectedValue ?? string.Empty)) %>';
					return false;
				}
				function fileMan_AddMenuAction()
				{
					<% 
					BXSite site = BXSite.GetCurrentSite(BXPath.ToVirtualRelativePath(curPath), Request.Url.Host);
					List<string> menuTypes = new List<string>(BXPublicMenu.GetMenuTypes(site.Id).Keys);
					if (menuTypes.Count == 0)
					{
						%>
					if (confirm('<%= BXJSUtility.Encode(GetMessageRaw("SetMenuTypes")) %>'))
						location.href='<%= BXJSUtility.Encode("Settings.aspx?module_id=" + HttpUtility.UrlEncode(typeof(BXMain).FullName) + "&tabindex=1") %>';
						<%
					}
					else
					{
						%>
					location.href='<%= BXJSUtility.Encode("MenuEdit.aspx?path=" + HttpUtility.UrlEncode(BXPath.Combine(curPath, menuTypes[0] + ".menu"))) %>';
						<%
					}
					%>
					return false;
				}
				function fileMan_SelectDirClick()
				{
					//<%= selectDirDialog.JavaScriptObject %>.Reset();//CollapseToRoot();
					//<%= selectDirDialog.JavaScriptObject %>.ExpandPath('<%= BXJSUtility.Encode(curPath) %>');
					<%= selectDirDialog.ContainerDialog.GetJSObjectName() %>.ShowPopupDialog(false, true);
					return false;
				}
				function fileMan_SelectDirOK()
				{
					document.getElementById('<%= targetFolder.ClientID %>').value = <%= selectDirDialog.JavaScriptObject %>.GetTargetValue();
					<%= selectDirDialog.ContainerDialog.GetJSObjectName() %>.ClosePopupDialog();
					return false;
				}
				function fileMan_SelectDirCancel()
				{
					<%= selectDirDialog.ContainerDialog.GetJSObjectName() %>.ClosePopupDialog();
					return false;
				}
			</script>
			</bx:InlineScript>
			<bx:BXMessage ID="resultMessage" runat="server" CssClass="Error" IconClass="Error" Visible="False" />
			<bx:BXAdminFilter ID="Filter" runat="server" AssociatedGridView="fileManGrid">
				<bx:BXTextBoxFilter runat="server" Text="<%$ Loc:ColumnName %>" Visibility="AlwaysVisible" Key="name" Operation="Like" />
				<bx:BXTextBoxFilter runat="server" Key="ext" Text="<%$ Loc:FilterText.Extension %>" />
				<bx:BXTimeIntervalFilter runat="server" Text="<%$ Loc:FilterText.IsChanged %>" Key="edited" />
				<bx:BXDropDownFilter runat="server" Text="<%$ Loc:ColumnType %>" Key="type">
					<asp:ListItem Value="" Text="<%$ Loc:Option.All %>" />
					<asp:ListItem Value="f" Text="<%$ Loc:Option.Files %>" />
					<asp:ListItem Value="d" Text="<%$ Loc:Option.Folders %>" />
				</bx:BXDropDownFilter>
			</bx:BXAdminFilter>
			
			<%
				NewFolderButton.Href = String.Concat("FileManNewFolder.aspx?path=", HttpUtility.UrlEncode(curPath));
				NewFileButton.Href = String.Concat("FileManEdit.aspx?path=", HttpUtility.UrlEncode(curPath), "&new=");
				UploadButton.Href = String.Concat("FileManUpload.aspx?path=", HttpUtility.UrlEncode(curPath));
		
				BXSite site = BXSite.GetCurrentSite(BXPath.ToVirtualRelativePath(curPath), Request.Url.Host);
				List<string> menuTypes = new List<string>(BXPublicMenu.GetMenuTypes(site.Id).Keys);
				if (menuTypes.Count == 0)
				{
					AddMenuButton.OnClickScript = String.Concat(
						"if (confirm('", BXJSUtility.Encode(GetMessageRaw("SetMenuTypes")), 
						"'))location.href='",
						BXJSUtility.Encode("Settings.aspx?module_id=" + HttpUtility.UrlEncode(typeof(BXMain).FullName) + "&tabindex=1"),
						"'; return false;"
					);
				}
				else
				{
					AddMenuButton.Href = String.Concat("MenuEdit.aspx?path=", HttpUtility.UrlEncode(BXPath.Combine(curPath, menuTypes[0] + ".menu")));
				}
								
				FolderSettingsButton.Href = String.Concat("FileManFolderSettings.aspx?path=", HttpUtility.UrlEncode(curPath));
				RootSecurityButton.Href = String.Concat("FileManSecurity.aspx?path=", HttpUtility.UrlEncode(curPath));						
			%>
			
			<bx:BXContextMenuToolbar ID="Toolbar1" runat="server" >
				<Items>
					<bx:BXCmSeparator SectionSeparator="True"/>
					<bx:BXCmImageButton ID="NewFolderButton" LinkButtonClientID="btn_new_folder" CssClass="context-button icon" CommandName="newfolder" Text="<%$ Loc:ActionText.NewFolder %>" Title="<%$ Loc:ActionTitle.NewFolder %>" />
					<bx:BXCmSeparator />
					<bx:BXCmImageButton ID="NewFileButton" LinkButtonClientID="btn_new_file" CssClass="context-button icon" CommandName="newfile" Text="<%$ Loc:ActionText.NewFile %>" Title="<%$ Loc:ActionTitle.NewFile %>" />
					<bx:BXCmSeparator />
					<bx:BXCmImageButton ID="UploadButton" LinkButtonClientID="btn_upload" CssClass="context-button icon" CommandName="upload" Text="<%$ Loc:ActionText.UploadFile %>" Title="<%$ Loc:ActionTitle.UploadFile %>" />
					<bx:BXCmSeparator />
					<bx:BXCmImageButton ID="AddMenuButton"  CssClass="context-button icon btn_menu" CommandName="addmenu" Text="<%$ Loc:ActionText.AddMenu %>" Title="<%$ Loc:ActionText.AddMenu %>" />
				</Items>
			</bx:BXContextMenuToolbar>
			<bx:BXContextMenuToolbar ID="Toolbar2" runat="server" ShowCustomControls="True">
				<CustomControls>
					<asp:Literal ID="Literal1" runat="server" Text="<%$ Loc:ShowRights %>" />
					<asp:DropDownList ID="rolesList" runat="server" Width="231px" EnableViewState="False" AutoPostBack="True" OnSelectedIndexChanged="rolesList_SelectedIndexChanged" />
				</CustomControls>
				<Items>
					<bx:BXCmSeparator SectionSeparator="True" />
					<bx:BXCmImageButton LinkButtonClientID="btn_folder_prop" CssClass="context-button icon" CommandName="folder_settings" Title="<%$ LocRaw:ActionTitle.FolderProperties %>" Text="<%$ LocRaw:ActionText.FolderProperties %>" ID="FolderSettingsButton" />
					<bx:BXCmSeparator />
					<bx:BXCmImageButton LinkButtonClientID="btn_access" CssClass="context-button icon" CommandName="security" Title="<%$ Loc:ActionTitle.CurrentSecurity %>" Text="<%$ Loc:ActionText.CurrentSecurity %>"  ID="RootSecurityButton" />
					<bx:BXCmSeparator />
				</Items>
			</bx:BXContextMenuToolbar>
			<bx:BXContextMenuToolbar ID="Toolbar3" runat="server" ShowCustomControls="True">
				<CustomControls>
					<asp:Panel ID="G" runat="server" DefaultButton="Goto">
					<%= GetMessage("LegendText.Path") %>
					<asp:TextBox ID="GotoPath" runat="server" Width="344px"></asp:TextBox>
					<asp:Button ID="Goto" runat="server" Text="OK" ToolTip="<%$ Loc:ButtonToolTip.ChangeFolder %>" OnClientClick="return fileMan_GotoPathAction();" />
					</asp:Panel>
				</CustomControls>
				<Items>
					<bx:BXCmSeparator SectionSeparator="True" />
				</Items>
			</bx:BXContextMenuToolbar>
			
			<bx:BXGridView ID="fileManGrid" runat="server" ContentName="<%$ Loc:TableTitle %>"
				DataSourceID="fileManGrid"
				AllowPaging="True" AllowSorting="True" Width="100%"
				PopupCommandMenuId="fileManPopupMenu" OnPopupMenuClick="fileManGrid_PopupMenuClick"
				ContextMenuToolbarId="fileManActionBar" OnMultiOperationActionRun="fileManGrid_MultiOperationActionRun"
				OnRowDataBound="fileManGrid_RowDataBound" DataKeyNames="path,name,type" SettingsToolbarId="Toolbar1" 
				OnSelect="fileManGrid_Select" OnUpdate="fileManGrid_Update" OnSelectCount="fileManGrid_SelectCount"
              >
				
                <pagersettings position="TopAndBottom" pagebuttoncount="3"/>
                <RowStyle BackColor="#FFFFFF"/>
                <AlternatingRowStyle BackColor="#FAFAFA" /> 
                <FooterStyle BackColor="#EAEDF7"/>				
				<Columns>
					<asp:TemplateField HeaderText="<%$ Loc:ColumnName %>" SortExpression="name">
						<edititemtemplate>
							<asp:TextBox id="nameEdit" runat="server" Text='<%# Bind("name") %>' ></asp:TextBox>
						
</edititemtemplate>
						<itemtemplate>
							<table cellpadding="0" cellspacing="0" border="0">
							<tr>
							<td style="padding-right:5px"><a href='<%# GetActionLink(Container.DataItem) %>' title='<%# GetImageTitle(Container.DataItem) %>' ><img src='<%# GetImageUrl(Container.DataItem) %>' alt='type' border='0' width='16' height='16' /></a></td>
							<%# GetDownloadTD(Container.DataItem) %>
							<td><a href="<%# GetActionLink(Container.DataItem) %>"><%# GetDisplayText(Container.DataItem) %></a></td>
							<%# GetBrowseTD(Container.DataItem) %>
							</tr>
							</table>
						
</itemtemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="<%$ Loc:ColumnSize %>" SortExpression="size">
						<ItemTemplate>
							<%# (((string)Eval("type")) == "f") ? Encode(Bitrix.Services.Text.BXStringUtility.BytesToString((long)Eval("size"))) : "&nbsp;" %>
					
</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField DataField="edited" HeaderText="<%$ Loc:ColumnEdited %>" ReadOnly="True"
						SortExpression="edited">
						<itemstyle horizontalalign="Left" />
					</asp:BoundField>
					<asp:BoundField Visible="False" DataField="access" HeaderText="<%$ Loc:ColumnAccess %>" ReadOnly="True">
						<itemstyle horizontalalign="Left" />
					</asp:BoundField>
					<asp:TemplateField HeaderText="<%$ Loc:ColumnType %>">
						<ItemTemplate>
							<%# Encode((((string)Eval("type")) == "f") ? GetMessage("File") : GetMessage("Folder"))%>
						
</ItemTemplate>
						<itemstyle horizontalalign="Left" />
					</asp:TemplateField>
				</Columns>
			</bx:BXGridView>			
			<bx:BXMultiActionMenuToolbar ID="fileManActionBar" runat="server" ShowCustomControls="True">
				<Items>
					<bx:BXMamImageButton runat="server"  EnabledCssClass="context-button icon access" DisabledCssClass="context-button icon access-dis"
						CommandName="security" Title="<%$ Loc:ActionTitle.ManageSecurity %>" />
					<bx:BXMamSeparator runat="server" />
					<bx:BXMamImageButton runat="server"  CommandName="inline" ShowConfirmBar="True" DisableForAll="True"
						EnabledCssClass="context-button icon edit" DisabledCssClass="context-button icon edit-dis"
						Title="<%$ Loc:ActionTitle.Rename %>" />
					<bx:BXMamImageButton runat="server" CommandName="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>"
						EnabledCssClass="context-button icon delete" DisabledCssClass="context-button icon delete-dis"
						Title="<%$ Loc:ActionTitle.Delete %>" />
					<bx:BXMamListItem runat="server" CommandName="copy" Text="<%$ Loc:ActionText.Copy %>" />
					<bx:BXMamListItem runat="server" CommandName="move" Text="<%$ Loc:ActionText.Move %>" />
				</Items>
				<CustomControls>
					<asp:TextBox ID="targetFolder" runat="server" Width="212px" />
					<asp:Button ID="selectDirButton" runat="server" Text="..." UseSubmitBehavior="False" OnClientClick="return fileMan_SelectDirClick();" />
				</CustomControls>
			</bx:BXMultiActionMenuToolbar>
			<br />
			<bx:BXPopupPanel ID="fileManPopupMenu" runat="server">
				<Commands>
					<bx:CommandItem Default="True" IconClass="view" UserCommandId="view" ItemText="<%$ Loc:PopupText.View %>"
						ItemTitle="<%$ Loc:PopupTitle.View %>" />
					<bx:CommandItem Default="True" IconClass="edit" UserCommandId="edit" ItemText="<%$ Loc:PopupText.Edit %>"
						ItemTitle="<%$ Loc:PopupTitle.Edit %>" />
					<bx:CommandItem Default="True" IconClass="edit" UserCommandId="editmenu" ItemText="<%$ Loc:PopupText.EditMenu %>"
						ItemTitle="<%$ Loc:PopupTitle.EditMenu %>" />
					<bx:CommandItem Default="True" IconClass="edit" UserCommandId="editastext" ItemText="<%$ Loc:PopupText.EditAsText %>"
						ItemTitle="<%$ Loc:PopupTitle.EditAsText %>" />
					<bx:CommandItem Default="True" IconClass="edit" UserCommandId="editraw" ItemText="<%$ Loc:PopupText.EditSource %>"
						ItemTitle="<%$ Loc:PopupTitle.EditSource %>" />
					<bx:CommandItem Default="True" IconClass="view" UserCommandId="browse" ItemText="<%$ Loc:PopupText.Browse %>"
						ItemTitle="<%$ Loc:PopupTitle.Browse %>" />
					<bx:CommandItem UserCommandId="editsep" ItemCommandType="Separator" />
					<bx:CommandItem IconClass="access" UserCommandId="security" ItemText="<%$ Loc:PopupText.ManageSecurity %>"
						ItemTitle="<%$ Loc:PopupTitle.ManageSecurity %>" />
					<bx:CommandItem IconClass="edit" UserCommandId="inline" ItemText="<%$ Loc:PopupText.Rename %>"
						ItemTitle="<%$ Loc:PopupTitle.Rename %>" />
					<bx:CommandItem IconClass="delete" UserCommandId="delete" ItemText="<%$ Loc:PopupText.Delete %>"
						ItemTitle="<%$ Loc:PopupTitle.Delete %>" ShowConfirmDialog="true"  ConfirmDialogText="<%$ Loc:Popup.DeleteConfirmation %>"  />
					<bx:CommandItem UserCommandId="nop" ItemText="<%$ Loc:PopupText.NoOperations %>"
						ItemTitle="<%$ Loc:PopupTitle.NoOperations %>" OnClickScript="return false;" />
				</Commands>
			</bx:BXPopupPanel>
			<bx:DirectoryBrowser ID="selectDirDialog" runat="server" ShowFiles="false" ItemsToSelect="Folders" EnableExtras="false" ShowDescription="true"
				DescriptionText="<%$ Loc:SelectDirectory.DescriptionText %>" DescriptionTitle="<%$ Loc:SelectDirectory.DescriptionTitle %>" 
				WindowTitle="<%$ Loc:SelectDirectory.Title %>" 
				OKClientScript="return fileMan_SelectDirOK();"
				CancelClientScript="return fileMan_SelectDirCancel();"
			/>
		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>
