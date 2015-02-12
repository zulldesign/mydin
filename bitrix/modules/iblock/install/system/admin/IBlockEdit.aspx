<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="IBlockEdit.aspx.cs" Inherits="bitrix_admin_IBlockEdit" Title="<%$ LocRaw:PageTitle %>"
	ValidateRequest="false" %>

<%@ Register Src="~/bitrix/admin/controls/Main/AdminImageField.ascx" TagName="AdminImageField"
	TagPrefix="bx" %>
<%@ Register Src="~/bitrix/controls/Main/CustomFieldSetUp.ascx" TagName="CustomFieldSetUp"
	TagPrefix="bx" %>
<%@ Register Src="~/bitrix/controls/Main/OperationsEdit.ascx" TagName="OperationsEdit"
	TagPrefix="bx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<asp:UpdatePanel runat="server" ID="DUP">
	<ContentTemplate><asp:PlaceHolder runat="server" ID="Dialogs" /></ContentTemplate>
	</asp:UpdatePanel>

	<%--<asp:UpdatePanel ID="UpdatePanel1" runat="server">
		<ContentTemplate>--%>
	<%
		ListButton.Href = "IBlockAdmin.aspx?type_id=" + UrlEncode(Request.QueryString["type_id"]);
		AddButton.Href = "IBlockEdit.aspx?type_id=" + UrlEncode(Request.QueryString["type_id"]);
	%>
	<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" OnCommandClick="BXContextMenuToolbar1_CommandClick"
		runat="server">
		<Items>
			<bx:BXCmSeparator SectionSeparator="true" />
			<bx:BXCmImageButton CssClass="context-button icon btn_list" ID="ListButton"
				CommandName="go2list" Text="<%$ LocRaw:ActionText.GoToList %>" Title="<%$ LocRaw:ActionTitle.GoToList %>"
			/>
			<bx:BXCmSeparator />
			<bx:BXCmImageButton ID="AddButton" CssClass="context-button icon btn_new"
				CommandName="AddNewIBlock" Text="<%$ LocRaw:ActionText.NewIBlock %>" Title="<%$ LocRaw:ActionTitle.NewIBlock %>"
			/>
			<bx:BXCmSeparator />
			<bx:BXCmImageButton ID="DeleteButton" CssClass="context-button icon btn_delete"
				CommandName="DeleteIBlock" Text="<%$ LocRaw:ActionText.DeleteIBlock %>" Title="<%$ LocRaw:ActionTitle.DeleteIBlock %>"
				ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.DeleteIBlock %>" />
		</Items>
	</bx:BXContextMenuToolbar>
	<bx:BXValidationSummary ID="errorMassage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>"
		ValidationGroup="IBlockEdit" />
	<bx:BXMessage ID="successMessage" runat="server" Content="<%$ Loc:Message.Success %>"
		CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False"
		Width="438px" />
	<asp:HiddenField ID="hfIBlockId" runat="server" />
	<asp:HiddenField ID="hfTypeId" runat="server" />
	<bx:BXTabControl ID="BXTabControl1" runat="server" OnCommand="BXTabControl1_Command"
		ValidationGroup="IBlockEdit">
		<bx:BXTabControlTab ID="mainTab" runat="server" Selected="True" Text="<%$ LocRaw:TabText.Main %>"
			Title="<%$ LocRaw:TabTitle.Main %>">
			<table id="tblIBlock" border="0" cellpadding="0" cellspacing="0" class="edit-table">
				<tr valign="top" id="trID" runat="server">
					<td class="field-name" width="40%" runat="server">
						ID:</td>
					<td width="60%" runat="server">
						<asp:Label ID="lbID" runat="server"></asp:Label>
					</td>
				</tr>
				<tr valign="top" id="trUpdateDate" runat="server">
					<td class="field-name" width="40%" runat="server">
						<%= GetMessage("Label.Modified") + ":" %>
					</td>
					<td width="60%" runat="server">
						<asp:Label ID="lbUpdateDate" runat="server"></asp:Label>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="40%">
						<%= GetMessage("Label.Active") + ":" %>
					</td>
					<td width="60%">
						<asp:CheckBox ID="cbActive" runat="server" />
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="40%">
						<%= GetMessage("Label.MnemonicCode") + ":" %>
					</td>
					<td width="60%">
						<asp:TextBox ID="tbCode" runat="server"></asp:TextBox>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="40%">
						<span class="required">*</span><%= GetMessage("Label.Sites") + ":" %></td>
					<td width="60%">
						<asp:CheckBoxList ID="cblSites" runat="server">
						</asp:CheckBoxList>
						<asp:CustomValidator ID="cvSites" runat="server" ValidationGroup="IBlockEdit" ErrorMessage="<%$ Loc:Message.ChooseAtLeastOneSite %>"
							ClientValidationFunction="CheckSites" OnServerValidate="cvSites_ServerValidate"
							Display="Dynamic">*</asp:CustomValidator>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="40%">
						<span class="required">*</span><%= GetMessage("Label.Name") + ":" %></td>
					<td width="60%">
						<asp:TextBox ID="tbName" runat="server"></asp:TextBox>
						<asp:RequiredFieldValidator ID="rfvName" runat="server" ValidationGroup="IBlockEdit"
							ControlToValidate="tbName" ErrorMessage="<%$ LocRaw:Message.IBlockNameRequired %>">*</asp:RequiredFieldValidator>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="40%">
						<%= GetMessage("Label.SortIndex") + ":" %>
					</td>
					<td width="60%">
						<asp:TextBox ID="tbSort" runat="server"></asp:TextBox>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="40%">
						<%= GetMessage("Label.ExternalCode") + ":" %>
					</td>
					<td width="60%">
						<asp:TextBox ID="tbXmlId" runat="server"></asp:TextBox>
					</td>
				</tr>
				<% 
					if (Bitrix.Modules.BXModuleManager.IsModuleInstalled("search"))
					{	 
				%>
				<tr valign="top">
					<td class="field-name" width="40%">
						<a href="#remark1" style="vertical-align:super; text-decoration:none"><span class="required">1</span></a><%= GetMessage("Label.IndexForSearch") %>:
					</td>
					<td width="60%">
						<asp:CheckBox ID="cbIndexContent" runat="server" />
					</td>
				</tr>
				<% if (IsSearchModuleInstalled){ %>
				<tr valign="top">
					<td class="field-name" width="40%">
						<%= GetMessage("Label.RebuildSearchIndex")%>:
					</td>
					<td width="60%">
						<span id="rebuildSearchIndex"><asp:CheckBox ID="cbRebuildSearchIndex" runat="server" /></span>
					</td>
				</tr>
				<% } %>				
				<%
					} 
				%>
				<tr class="heading">
					<td colspan="2">
						<%= GetMessage("Section.Description") + ":" %>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="40%">
						<%= GetMessage("Label.Image") + ":" %>
					</td>
					<td width="60%">
						<bx:AdminImageField ID="Img" runat="server" MaxImageWidth="640" />
					</td>
				</tr>
				<tr valign="top">
					<td colspan="2" align="center">
						<asp:RadioButtonList ID="rblDescriptionType" runat="server" RepeatDirection="Horizontal">
							<asp:ListItem Selected="True" Value="text" Text="<%$ Loc:Option.Text %>" />
							<asp:ListItem Value="html" Text="Html" />
						</asp:RadioButtonList>
						<asp:TextBox ID="tbDescription" runat="server" Columns="60" Rows="15" TextMode="MultiLine"
							Width="100%"></asp:TextBox>
						<bx:BXWebEditor ID="bxweDescription" runat="server" Width="100%" Height="400px" StartModeSelector="True"
							StartMode="HTMLVisual" UseHTMLEditor="False" AutoLoadContent="True" ContentType="Text"
							FullScreen="False" LimitCodeAccess="False" TemplateId="" UseOnlyDefinedStyles="False"
							WithoutCode="False">
							<Taskbars>
								<bx:BXWebEditorBar Name="BXPropertiesTaskbar" />
							</Taskbars>
						</bx:BXWebEditor>
					</td>
				</tr>
			</table>
		</bx:BXTabControlTab>
		<bx:BXTabControlTab ID="BXTabControlTab2" runat="server" Text="<%$ LocRaw:TabText.Properties %>"
			Title="<%$ LocRaw:TabTitle.Properties %>">
			<bx:CustomFieldSetUp runat="server" ID="cfl" ValidationGroup="IBlockEdit" />
		</bx:BXTabControlTab>
		<bx:BXTabControlTab ID="BXTabControlTab1" runat="server" Text="<%$ LocRaw:TabText.SectionProperties %>"
			Title="<%$ LocRaw:TabTitle.SectionProperties %>">
			<bx:CustomFieldSetUp runat="server" ID="cfls" ValidationGroup="IBlockEdit" />
		</bx:BXTabControlTab>
		<bx:BXTabControlTab ID="BXTabControlTab3" runat="server" Text="<%$ LocRaw:TabText.Access %>"
			Title="<%$ LocRaw:TabTitle.Access %>">
			<bx:OperationsEdit runat="server" ID="AccessEdit" AllowedStates="AllButDenied" ShowLegend="true"
				LegendText-Allow="<%$ LocRaw:SecurityLegend.Allowed %>" LegendText-InheritAllow="<%$ LocRaw:SecurityLegend.InheritAllowed %>"
				LegendText-InheritDeny="<%$ LocRaw:SecurityLegend.InheritDenied %>" />
			<%--<table id="tblPermissions" runat="server" border="0" cellpadding="0" cellspacing="0"
				class="edit-table">
			</table>--%>
		</bx:BXTabControlTab>
		<bx:BXTabControlTab ID="BXTabControlTab4" runat="server" Text="<%$ LocRaw:TabText.Captions %>"
			Title="<%$ LocRaw:TabTitle.Captions %>">
			<table id="tblTitles" border="0" cellpadding="0" cellspacing="0" class="edit-table">
				<tr class="heading">
					<td colspan="2">
						<%= GetMessage("Label.SectionCaptions")%>
					</td>
				</tr>
				<tr valign="top">

					<td class="field-name" width="45%">
						<%= GetMessage("Label.Sections") + ":" %>
					</td>
					<td width="55%">
						<asp:TextBox ID="tbSectionsName" runat="server"></asp:TextBox>
					</td>
				</tr>
					
				<tr valign="top">
					<td class="field-name" width="45%">
						<%= GetMessage("Label.Section") + ":" %>
					</td>
					<td width="55%">
						<asp:TextBox ID="tbSectionName" runat="server"></asp:TextBox>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="45%">
						<%= GetMessage("Label.AddSection") + ":" %>
					</td>
					<td width="55%">
						<asp:TextBox ID="tbAddSection" runat="server"></asp:TextBox>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="45%">
						<%= GetMessage("Label.ChangeSection") + ":" %>
					</td>
					<td width="55%">
						<asp:TextBox ID="tbChangeSection" runat="server"></asp:TextBox>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="45%">
						<%= GetMessage("Label.DeleteSection") + ":" %>
					</td>
					<td width="55%">
						<asp:TextBox ID="tbDeleteSection" runat="server"></asp:TextBox>
					</td>
				</tr>
								<tr valign="top">
					<td class="field-name" width="45%">
						<%= GetMessage("Label.NewSection") + ":" %>
					</td>
					<td width="55%">
						<asp:TextBox ID="tbNewSection" runat="server"></asp:TextBox>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="45%">
						<%= GetMessage("Label.SectionList") + ":" %>
					</td>
					<td width="55%">
						<asp:TextBox ID="tbSectionList" runat="server"></asp:TextBox>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="45%">
						<%= GetMessage("Label.ModifyingSection") + ":" %>
					</td>
					<td width="55%">
						<asp:TextBox ID="tbModifyingSection" runat="server"></asp:TextBox>
					</td>
				</tr>
				<tr class="heading">
					<td colspan="2">
						<%= GetMessage("Label.ElementCaptions")%>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="45%">
						<%= GetMessage("Label.Elements") + ":" %>
					</td>
					<td width="55%">
						<asp:TextBox ID="tbElementsName" runat="server"></asp:TextBox>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="45%">
						<%= GetMessage("Label.Element") + ":" %>
					</td>
					<td width="55%">
						<asp:TextBox ID="tbElementName" runat="server"></asp:TextBox>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="45%">
						<%= GetMessage("Label.AddElement") + ":" %>
					</td>
					<td width="55%">
						<asp:TextBox ID="tbAddElement" runat="server"></asp:TextBox>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="45%">
						<%= GetMessage("Label.ChangeElement") + ":" %>
					</td>
					<td width="55%">
						<asp:TextBox ID="tbChangeElement" runat="server"></asp:TextBox>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="45%">
						<%= GetMessage("Label.DeleteElement") + ":" %>
					</td>
					<td width="55%">
				        <asp:TextBox ID="tbDeleteElement" runat="server"></asp:TextBox>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="45%">
						<%= GetMessage("Label.NewElement") + ":" %>
					</td>
					<td width="55%">
						<asp:TextBox ID="tbNewElement" runat="server"></asp:TextBox>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="45%">
						<%= GetMessage("Label.ElementList") + ":" %>
					</td>
					<td width="55%">
						<asp:TextBox ID="tbElementList" runat="server"></asp:TextBox>
					</td>
				</tr>
				<tr valign="top">
					<td class="field-name" width="45%">
						<%= GetMessage("Label.ModifyingElement") + ":" %>
					</td>
					<td width="55%">
						<asp:TextBox ID="tbModifyingElement" runat="server"></asp:TextBox>
					</td>
				</tr>
			</table>
		</bx:BXTabControlTab>
	</bx:BXTabControl>

<% if (IsSearchModuleInstalled){ %>
<bx:BXAdminNote ID="BXAdminNote1" runat="server">
    <span class="required" style="vertical-align:super" id="remark1" >1</span><%= string.Format(GetMessageRaw("Note.Reindex"), "SearchReindex.aspx", "#rebuildSearchIndex", string.Format("document.getElementById('{0}').checked = true;", cbRebuildSearchIndex.ClientID))%>
</bx:BXAdminNote>
<bx:BXAdminNote ID="BXAdminNote2" runat="server">
    <span class="required" style="vertical-align:super" id="remark2" >2</span>
    <%=GetMessageRaw("Note.WhereCaptionsAppear") %>
</bx:BXAdminNote>
<% } %>
	
	<%--</ContentTemplate>
		<Triggers>
			<asp:PostBackTrigger ControlID="BXTabControl1" />
		</Triggers>
	</asp:UpdatePanel>--%>
</asp:Content>
