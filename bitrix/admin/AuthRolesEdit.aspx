<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="AuthRolesEdit.aspx.cs" Inherits="bitrix_admin_AuthRolesEdit" Title="<%$ Loc:PageTitle %>" %>
<%@ Register Src="~/bitrix/controls/Main/OperationsEdit.ascx" TagName="OperationsEdit" TagPrefix="bx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<asp:UpdatePanel ID="UpdatePanel1" runat="server">
		<ContentTemplate>
			<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" runat="server" OnCommandClick="BXContextMenuToolbar1_CommandClick">
				<Items>
					<bx:BXCmSeparator runat="server" SectionSeparator="true" />
					<bx:BXCmImageButton runat="server" CssClass="context-button icon btn_list" CommandName="go2list"
						Text="<%$ Loc:ActionText.GoToList %>" Title="<%$ Loc:ActionTitle.GoToList %>"
						Href="AuthRolesList.aspx" />
					<bx:BXCmSeparator ID="AddRoleSeparator" SectionSeparator="true" />
					<bx:BXCmImageButton ID="AddRoleButton" CssClass="context-button icon btn_new" CommandName="AddNewRole"
						Text="<%$ Loc:ActionText.AddNewRole %>" Title="<%$ Loc:ActionTitle.AddNewRole %>"
						Href="AuthRolesEdit.aspx" />
					<bx:BXCmSeparator ID="DeleteRoleSeparator" />
					<bx:BXCmImageButton ID="DeleteRoleButton" CssClass="context-button icon btn_delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" CommandName="DeleteRole"
						Text="<%$ Loc:ActionText.DeleteRole %>" Title="<%$ Loc:ActionTitle.DeleteRole %>" />
				</Items>
			</bx:BXContextMenuToolbar>
			<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>"/>
			<% if (!IsPostBack && Request.QueryString["ok"] != null) successMessage.Visible = true; %>
			<bx:BXMessage ID="successMessage" runat="server" Content="<%$ Loc:Message.Success %>"
				CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False" Width="438px" />
			<asp:HiddenField ID="hfRoleId" runat="server" />
			<bx:BXTabControl ID="BXTabControl1" runat="server" OnCommand="BXTabControl1_Command">
				<bx:BXTabControlTab runat="server" Selected="True" Text="<%$ Loc:TabText.Parameters %>" Title="<%$ Loc:TabTitle.Parameters %>">
					<table id="Table1" border="0" cellpadding="0" cellspacing="0" class="edit-table">
						<tr valign="top" id="trUsersCount" runat="server">
							<td class="field-name" width="40%">
								<%= GetMessage("UserCount") %></td>
							<td width="60%">
								&nbsp;&nbsp;
								<asp:Label ID="lbUsersCount" runat="server" Text=""></asp:Label>
							</td>
						</tr>
						<tr valign="top" id="trRolesCount" runat="server">
							<td class="field-name" width="40%">
								<%= GetMessage("SubRoleCount") %></td>
							<td width="60%">
								&nbsp;&nbsp;
								<asp:Label ID="lbRolesCount" runat="server" Text=""></asp:Label>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<%= GetMessage("Provider") %></td>
							<td width="60%">
								<asp:Label runat="server" ID="lbProviderName"></asp:Label>
								<asp:DropDownList runat="server" ID="ddProviderName">
								</asp:DropDownList>
								<asp:CustomValidator runat="server" 
									ErrorMessage="<%$ Loc:Message.ProviderNotFound %>" ControlToValidate="ddProviderName"
									ID="cvProviderName" Display="Dynamic" OnServerValidate="cvProviderName_ServerValidate">*</asp:CustomValidator>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<span class="required">*</span><%= GetMessage("Code") %></td>
							<td width="60%">
								<asp:TextBox ID="tbRoleName" runat="server" Width="200"></asp:TextBox>
								<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="tbRoleName"
									ErrorMessage="<%$ Loc:Message.NameRequired %>">*</asp:RequiredFieldValidator>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<%= GetMessage("Name") %></td>
							<td width="60%">
								<asp:TextBox ID="tbRoleTitle" Width="300" runat="server"></asp:TextBox>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<%= GetMessage("Active") %></td>
							<td width="60%">
								&nbsp;<asp:CheckBox ID="cbActive" runat="server" />
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								&nbsp;<%= GetMessage("Comment") %></td>
							<td width="60%">
								&nbsp;<asp:TextBox ID="tbComment" runat="server" Width="300" Height="200" TextMode="MultiLine"></asp:TextBox>
							</td>
						</tr>
					</table>
				</bx:BXTabControlTab>
				<bx:BXTabControlTab runat="server" Text="<%$ Loc:TabText.SubRoles %>" Title="<%$ Loc:TabTitle.SubRoles %>">
					<table id="Table3" border="0" cellpadding="0" cellspacing="0" class="edit-table">
						<tr valign="top" runat="server">
							<td class="field-name" width="40%">
								<%= GetMessage("IncludesRoles") %></td>
							<td width="60%">
								<% MoveSelectedUp(lbSubRoles.Items); %>
								<asp:ListBox ID="lbSubRoles" runat="server" Rows="10" SelectionMode="Multiple"></asp:ListBox>
							</td>
						</tr>
					</table>
				</bx:BXTabControlTab>
				<bx:BXTabControlTab runat="server" Text="<%$ Loc:TabText.SecurityPolicy %>" Title="<%$ Loc:TabTitle.SecurityPolicy %>">
					<table id="Table2" border="0" cellpadding="0" cellspacing="0" class="edit-table">
						<tr valign="top" runat="server">
							<td class="field-name" width="40%">
								<%= GetMessage("PredefinedSecuritySettings") %></td>
							<td width="60%">
								&nbsp;<asp:DropDownList ID="DropDownList1" runat="server">
									<asp:ListItem Selected="True" Text="<%$ LocRaw:Option.SelectLevel %>" ></asp:ListItem>
									<asp:ListItem Value="parent" Text="<%$ LocRaw:Option.Inherit %>" ></asp:ListItem>
									<asp:ListItem Value="low" Text="<%$ LocRaw:Option.Low %>" ></asp:ListItem>
									<asp:ListItem Value="middle" Text="<%$ LocRaw:Option.Normal %>" ></asp:ListItem>
									<asp:ListItem Value="high" Text="<%$ LocRaw:Option.High %>" ></asp:ListItem>
								</asp:DropDownList>
							</td>
						</tr>
						<tr valign="top" runat="server">
							<td class="field-name" width="40%">
								<%= GetMessage("SessionLifetime") %></td>
							<td width="60%">
								<asp:CheckBox ID="cbSessionTimeoutParent" runat="server" Text="<%$ Loc:CheckBoxText.Inherit %>" />
								<br />
								<asp:TextBox ID="tbSessionTimeout" runat="server"></asp:TextBox>
								<asp:CustomValidator ID="cvSessionTimeout" runat="server" 
									ControlToValidate="tbSessionTimeout"
									ErrorMessage="<%$ Loc:Message.SessionLifetimeOrInheritRequired %>"
									OnServerValidate="cvSessionTimeout_ServerValidate" ValidateEmptyText="True"
									ClientValidationFunction="cvSessionTimeout">*</asp:CustomValidator>
							</td>
						</tr>
						<tr valign="top" runat="server">
							<td class="field-name" width="40%">
								<%= GetMessage("SessionNetworkMask") %></td>
							<td width="60%">
								<asp:CheckBox ID="cbSessionIPMaskParent" runat="server" Text="<%$ Loc:CheckBoxText.Inherit %>" />
								<br />
								<asp:TextBox ID="tbSessionIPMask" runat="server"></asp:TextBox>
								<asp:CustomValidator ID="cvSessionIPMask" runat="server" 
									ControlToValidate="tbSessionIPMask"
									ErrorMessage="<%$ Loc:Message.SessionNetworkMaskOrInheritRequired %>"
									OnServerValidate="cvSessionIPMask_ServerValidate" ValidateEmptyText="True"
									ClientValidationFunction="cvSessionIPMask">*</asp:CustomValidator>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<%= GetMessage("MaxComputers") %></td>
							<td width="60%">
								<asp:CheckBox ID="cbMaxStoreNumParent" runat="server" Text="<%$ Loc:CheckBoxText.Inherit %>" />
								<br />
								<asp:TextBox ID="tbMaxStoreNum" runat="server"></asp:TextBox>
								<asp:CustomValidator ID="cvMaxStoreNum" runat="server" 
									ControlToValidate="tbMaxStoreNum"
									ErrorMessage="<%$ Loc:Message.MaxComputersOrInheritRequired %>"
									OnServerValidate="cvMaxStoreNum_ServerValidate" ValidateEmptyText="True"
									ClientValidationFunction="cvMaxStoreNum">*</asp:CustomValidator>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<%= GetMessage("SavedAuthorizationNetworkMask") %></td>
							<td width="60%">
								<asp:CheckBox ID="cbStoreIPMaskParent" runat="server" Text="<%$ Loc:CheckBoxText.Inherit %>" />
								<br />
								<asp:TextBox ID="tbStoreIPMask" runat="server"></asp:TextBox>
								<asp:CustomValidator ID="cvStoreIPMask" runat="server" 
									ControlToValidate="tbStoreIPMask"
									ErrorMessage="<%$ Loc:Message.SavedAuthorizationNetwotkMaskOrInheritRequired %>"
									OnServerValidate="cvStoreIPMask_ServerValidate" ValidateEmptyText="True"
									ClientValidationFunction="cvStoreIPMask">*</asp:CustomValidator>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<%= GetMessage("StoredAuthorizationLifetime") %></td>
							<td width="60%">
								<asp:CheckBox ID="cbStoreTimeoutParent" runat="server" Text="<%$ Loc:CheckBoxText.Inherit %>" />
								<br />
								<asp:TextBox ID="tbStoreTimeout" runat="server"></asp:TextBox>
								<asp:CustomValidator ID="cvStoreTimeout" runat="server" 
									ControlToValidate="tbStoreTimeout"
									ErrorMessage="<%$ Loc:Message.StoredAuthorizationLifetimeOrInheritRequired %>"
									OnServerValidate="cvStoreTimeout_ServerValidate" ValidateEmptyText="True"
									ClientValidationFunction="cvStoreTimeout">*</asp:CustomValidator>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<%= GetMessage("CheckwordLifetime") %></td>
							<td width="60%">
								<asp:CheckBox ID="cbCheckwordTimeoutParent" runat="server" Text="<%$ Loc:CheckBoxText.Inherit %>" />
								<br />
								<asp:TextBox ID="tbCheckwordTimeout" runat="server"></asp:TextBox>
								<asp:CustomValidator ID="cvCheckwordTimeout" runat="server" 
									ControlToValidate="tbCheckwordTimeout"
									ErrorMessage="<%$ Loc:Message.CheckwordLifetimeOrInheritRequired %>"
									OnServerValidate="cvCheckwordTimeout_ServerValidate" ValidateEmptyText="True"
									ClientValidationFunction="cvCheckwordTimeout">*</asp:CustomValidator>
							</td>
						</tr>
					</table>
				</bx:BXTabControlTab>
				<bx:BXTabControlTab runat="server" Text="<%$ Loc:TabText.Tasks %>" Title="<%$ Loc:TabTitle.Tasks %>">
					<table id="Table4" border="0" cellpadding="0" cellspacing="0" class="edit-table">
						<tr valign="top">
							<td class="field-name" width="40%">
								<%= GetMessage("Tasks") %></td>
							<td width="60%">
								<asp:ListBox ID="lbTasks" runat="server" Rows="20" SelectionMode="Multiple"></asp:ListBox>
							</td>
						</tr>
					</table>
				</bx:BXTabControlTab>
				<bx:BXTabControlTab ID="tctOperations" runat="server" Text="<%$ Loc:TabText.Operations %>" Title="<%$ Loc:TabTitle.Operations %>">
					<table id="tblOperations" runat="server" border="0" cellpadding="0" cellspacing="0" class="edit-table">
					</table>
				</bx:BXTabControlTab>
				<bx:BXTabControlTab ID="tctUserOperations" runat="server" Text="<%$ LocRaw:TabText.UserOperations %>" Title="<%$ LocRaw:TabTitle.UserOperations %>">
					<bx:InlineScript runat="server" ID="UserOperationsScript" SyncMode="Inline" AsyncMode="ScriptBlock">
						<script type="text/javascript">
							UserOperations_Data = [
								<% foreach (RepeaterItem item in UserOperationsContainer.Items) { %>
								{
									operation: '<%= item.FindControl("Operation").ClientID %>',
									limitPanel: '<%= item.FindControl("LimitPanel").ClientID %>',
									limit: '<%= item.FindControl("Limit").ClientID %>',
									rolesPanel: '<%= item.FindControl("RolesPanel").ClientID %>'
								},
								<% } %>
								null //dummy at the end
							];
						</script>
					</bx:InlineScript>
					<script type="text/javascript">
						function UserOperations_RefreshGUI() 
						{
							var separate = document.getElementById('<%= UserOperationsSeparate.ClientID %>').checked;
						
							for (var i = UserOperations_Data.length - 2 /*skip the dummy*/; i >= 0; i--)
							{
								var item = UserOperations_Data[i];
								
								document.getElementById(item.limitPanel).style.display = (separate && document.getElementById(item.operation).checked) ? '' : 'none';
								document.getElementById(item.rolesPanel).style.display = (separate && document.getElementById(item.limit).checked) ? '' : 'none';
							}
												
							document.getElementById('<%= UserOperationsLimitPanel.ClientID %>').style.display = !separate ? '': 'none';
							document.getElementById('<%= UserOperationsRolesPanel.ClientID %>').style.display = (!separate && document.getElementById('<%= UserOperationsLimit.ClientID %>').checked) ? '' : 'none';
						}
					</script>
					<table cellpadding="0" cellspacing="0" border="0" class="edit-table">
						<tr><td>
							<% 
								var separate = UserOperationsSeparate.Checked;					
								foreach (RepeaterItem item in UserOperationsContainer.Items)
								{
									var name = userOperationNames[item];
									var operation = ((CheckBox)item.FindControl("Operation"));
									operation.Text = Encode(string.Format("{0} ({1})", Bitrix.Services.BXLoc.GetModuleMessage("main", "Auth.Operations." + name), name));
									operation.Attributes["onclick"] = "UserOperations_RefreshGUI();";
									
									var limitPanel = ((Panel)item.FindControl("LimitPanel"));
									limitPanel.Style[HtmlTextWriterStyle.MarginLeft] = "20px";
									limitPanel.Style[HtmlTextWriterStyle.Display] = (separate && operation.Checked) ? "" : "none";
									
									var limit = ((CheckBox)item.FindControl("Limit"));
									limit.Attributes["onclick"] = "UserOperations_RefreshGUI();";
									limit.Text += @"<a href=""#remark1"" style=""vertical-align:super; text-decoration:none""><span class=""required"">1</span></a>";
									
									var rolesPanel = ((Panel)item.FindControl("RolesPanel"));
									rolesPanel.Style[HtmlTextWriterStyle.Display] = (separate && limit.Checked) ? "" : "none";
									
									MoveSelectedUp(((ListControl)item.FindControl("Roles")).Items);
								}
			  				%>
							<asp:Repeater runat="server" ID="UserOperationsContainer" OnItemDataBound="UserOperations_ItemDataBound">
								<ItemTemplate>
									<asp:Panel runat="server" ID="Container">
										<asp:CheckBox runat="server" ID="Operation" />
										<asp:Panel runat="server" ID="LimitPanel">
											<asp:CheckBox runat="server" ID="Limit" Text="<%$ LocRaw:CheckBoxText.LimitOperations %>" />
											<asp:Panel runat="server" ID="RolesPanel">
												<asp:ListBox runat="server" ID="Roles" Rows="10" SelectionMode="Multiple" />
												<a href="#remark2" style="vertical-align:super; text-decoration:none"><span class="required">2</span></a>
											</asp:Panel>
										</asp:Panel>
									</asp:Panel>
								</ItemTemplate>
							</asp:Repeater>
							<hr />
							<%
								UserOperationsLimit.Attributes["onclick"] = "UserOperations_RefreshGUI();";
																													
								UserOperationsLimitPanel.Style[HtmlTextWriterStyle.Display] = !separate ? "" : "none";
								UserOperationsRolesPanel.Style[HtmlTextWriterStyle.Display] = (!separate && UserOperationsLimit.Checked) ? "" : "none";					
			
								MoveSelectedUp(UserOperationsRoles.Items);		
			
								UserOperationsLimit.Text += @"<a href=""#remark1"" style=""vertical-align:super; text-decoration:none""><span class=""required"">1</span></a>";																		
							%>
							<asp:Panel runat="server" ID="UserOperationsLimitPanel">
								<asp:CheckBox runat="server" ID="UserOperationsLimit" Text="<%$ LocRaw:CheckBoxText.LimitOperation %>" />
								<asp:Panel runat="server" ID="UserOperationsRolesPanel">
									<asp:ListBox runat="server" ID="UserOperationsRoles" Rows="10" SelectionMode="Multiple" />
									<a href="#remark2" style="vertical-align:super; text-decoration:none"><span class="required">2</span></a>
								</asp:Panel>
							</asp:Panel>
							
							<%
								UserOperationsSeparate.Attributes["onclick"] = "UserOperations_RefreshGUI();";
							%>
							<asp:CheckBox runat="server" ID="UserOperationsSeparate" Text="<%$ LocRaw:CheckBoxText.SeparateSettings %>"/>
						</td></tr>
					</table>
				</bx:BXTabControlTab>
			</bx:BXTabControl>
			<bx:BXAdminNote runat="server" ID="Remarks">
				<p>
				<span class="required" style="vertical-align:super" id="remark1" >1</span>
				<%= GetMessageRaw("Note1") %>
				</p>
				<p>
				<span class="required" style="vertical-align:super" id="remark2" >2</span>
				<%= GetMessageRaw("Note2") %>
				</p>
			</bx:BXAdminNote>
		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>

