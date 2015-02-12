<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" 
	AutoEventWireup="true" CodeFile="ForumEdit.aspx.cs" Inherits="BXForumAdminPageForumEdit"%>
	
<%@ Register Src="~/bitrix/controls/Main/OperationsEdit.ascx" TagName="OperationsEdit"
	TagPrefix="bx" %>
	
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	
	<bx:BXContextMenuToolbar ID="BXForumEditToolbar" runat="server" OnCommandClick="OnToolBarButtonClick">
		<Items>
			<bx:BXCmSeparator ID="ListButton" runat="server" SectionSeparator="true" />
			<bx:BXCmImageButton CssClass="context-button icon btn_list" CommandName="list"
				Text="<%$ LocRaw:BackButtonText %>" Title="<%$ LocRaw: BackButtonTitle %>" Href="ForumList.aspx" />
		
			<bx:BXCmSeparator SectionSeparator="true" />
			<bx:BXCmImageButton ID="AddButton"
				CommandName="add" Text="<%$ LocRaw:Kernel.Add %>" Title="<%$ LocRaw:NewButtonTitle %>"
				CssClass="context-button icon btn_new" Href="ForumEdit.aspx" />
				
			<bx:BXCmSeparator SectionSeparator="true" />
			<bx:BXCmImageButton ID="DeleteButton"
				CommandName="delete" Text="<%$ LocRaw:Kernel.Delete %>" Title="<%$ LocRaw:DeleteButtonTitle %>"
				CssClass="context-button icon btn_delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:DeleteButtonConfirmText %>"/>
		</Items>
	</bx:BXContextMenuToolbar>
	<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="ForumEdit" />
	<bx:BXTabControl ID="TabControl" runat="server" OnCommand="OnForumEdit" ValidationGroup="ForumEdit">
		<bx:BXTabControlTab ID="MainSettingsTab" Runat="server" Selected="True" Text="<%$ LocRaw:BaseForumSettingsTabText %>" Title="<%$ LocRaw: BaseForumSettingsTabTitle %>">
			<table class="edit-table" cellspacing="0" cellpadding="0" border="0">
				<% 
				if (Id > 0)
				{
					%><tr valign="top"><td class="field-name">ID:</td><td><%=Id %></td></tr><%
				}
				%>
				
				<tr valign="top">
					<td class="field-name" width="40%"><%= GetMessage("Forum.Active") %>:</td>
					<td width="60%"><asp:CheckBox ID="ForumActive" runat="server" /></td>
				</tr>

				<tr valign="top">
					<td class="field-name"><span class="required">*</span><%= GetMessage("Forum.Name") %>:</td>
					<td>
						<asp:TextBox ID="ForumName" runat="server" Width="350px" />
						<asp:RequiredFieldValidator ID="NameRequiredValidator" runat="server" ValidationGroup="ForumEdit" ControlToValidate="ForumName" ErrorMessage="<%$ Loc:Error.EmptyForumName %>" Display="Dynamic">*</asp:RequiredFieldValidator>
					</td>
				</tr>				

				<tr valign="top">
					<td class="field-name" width="40%"><span class="required">*</span><%= GetMessage("Forum.Sites") %>:</td>
					<td width="60%">
						<table>
							<tr>
								<td valign="top"><asp:CheckBoxList ID="ForumSites" runat="server"></asp:CheckBoxList></td>
								<td valign="top">
									<asp:CustomValidator ID="ForumSitesValidator" runat="server" ValidationGroup="ForumEdit" ErrorMessage="<%$ Loc:Error.EmptyForumSites %>"
										ClientValidationFunction="CheckSites" OnServerValidate="ForumSites_ServerValidate"
										Display="Dynamic">*</asp:CustomValidator>
								</td>
							</tr>
						</table>
					</td>
				</tr>
				
				<tr valign="top">
					<td class="field-name"><%= GetMessage("Forum.Category") %>:</td>
					<td>
						<asp:DropDownList ID="ForumCategory" runat="server" Width="250px">
							<asp:ListItem Value="0" Text="<%$ LocRaw:Option.SelectCategory %>" />
						</asp:DropDownList>&nbsp;&nbsp;<a id="ForumCategoryNewLink" runat="server" href="ForumCategoryEdit.aspx"><%= GetMessage("CreateNewCategory") %></a>
					</td>
				</tr>

				<tr valign="top">
					<td class="field-name"><%= GetMessage("Forum.Description") %>:<br /><label>(<%= GetMessage("AllowHtmlInDescription") %>)</label></td>
					<td>
						<asp:TextBox Rows="7" ID="ForumDescription"  TextMode="MultiLine" runat="server" Width="350px" />						
					</td>
				</tr>
				
				<tr valign="top">
					<td class="field-name"><%= GetMessage("Forum.Sort") %>:</td>
					<td><asp:TextBox ID="ForumSort" runat="server" Width="50px" /></td>
				</tr>

			</table>
		</bx:BXTabControlTab>

		<bx:BXTabControlTab ID="AdditionalSettingsTab" Runat="server" Text="<%$ Loc:Kernel.TopPanel.Settings %>" Title="<%$ Loc:AdditionalSettingsTabTitle %>">
			<table class="edit-table" cellspacing="0" cellpadding="0" border="0">
			
				<tr valign="top">
					<td class="field-name" width="40%"></td>
					<td width="60%"><asp:CheckBox ID="ForumBBCode" runat="server" />&nbsp;<label for="<%= ForumBBCode.ClientID %>"><%= GetMessage("Forum.AllowBBCode") %></label></td>
				</tr>

				<tr valign="top">
					<td class="field-name" width="40%"></td>
					<td width="60%"><asp:CheckBox ID="ForumSmiles" runat="server" />&nbsp;<label for="<%= ForumSmiles.ClientID %>"><%= GetMessage("Forum.AllowSmiles") %></label></td>
				</tr>

				<tr valign="top">
					<td class="field-name" width="40%"></td>
					<td width="60%"><asp:CheckBox ID="ForumVotingForTopic" runat="server" />&nbsp;<label for="<%= ForumVotingForTopic.ClientID %>"><%= GetMessage("Forum.AllowVotingForTopic")%></label></td>
				</tr>

				<tr valign="top">
					<td class="field-name" width="40%"></td>
					<td width="60%"><asp:CheckBox ID="ForumVotingForPost" runat="server" />&nbsp;<label for="<%= ForumVotingForPost.ClientID %>"><%= GetMessage("Forum.AllowVotingForPost")%></label></td>
				</tr>
				
				<tr valign="top">
					<td class="field-name" width="40%"></td>
					<td width="60%"><asp:CheckBox ID="ForumIndexContent" runat="server" />&nbsp;<label for="<%= ForumIndexContent.ClientID %>"><%= GetMessage("Forum.IndexContent") %></label></td>
				</tr>
				<% if (IsSearchModuleInstalled){ %>
				<tr valign="top">
					<td class="field-name" width="40%"></td>
					<td width="60%"><asp:CheckBox ID="ForumRebuildSearchIndex" runat="server" />&nbsp;<label for="<%= ForumRebuildSearchIndex.ClientID %>"><%= GetMessage("Forum.RebuildSearchIndex")%></label></td>
				</tr>
				<%} %>			
				<tr valign="top">
					<td class="field-name" width="40%"><%= GetMessage("Forum.Code") %>:</td>
					<td width="60%"><asp:TextBox ID="ForumCode" runat="server" Width="250px"></asp:TextBox></td>
				</tr>
			
				<tr valign="top">
					<td class="field-name"><%= GetMessage("Forum.XmlId") %>:</td>
					<td><asp:TextBox ID="ForumXmlId" runat="server" Width="250px" /></td>
				</tr>
			</table>
		</bx:BXTabControlTab>

		<bx:BXTabControlTab ID="PermissionsTab" Runat="server" Text="<%$ LocRaw:PermissionSettingsTabText %>" Title="<%$ LocRaw:PermissionSettingsTabTitle %>">
			<bx:OperationsEdit runat="server" 
				ID="AccessEdit" 
				AllowedStates="AllButDenied"
				ShowLegend="true"
				LegendText-Allow="<%$ LocRaw:AllowForCurrentForum %>" 
				LegendText-InheritAllow="<%$ LocRaw:AllowForAllForums %>"
				LegendText-InheritDeny="<%$ LocRaw:NotSet %>" />
		</bx:BXTabControlTab>
	</bx:BXTabControl>
</asp:Content>

