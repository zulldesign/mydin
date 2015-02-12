<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="IBlockSectionEdit.aspx.cs" Inherits="bitrix_admin_IBlockSectionEdit" Title="<%$ Loc:PageTitleNewSection %>" ValidateRequest="false" %>
<%@ Register Src="~/bitrix/admin/controls/Main/AdminImageField.ascx" TagName="AdminImageField" TagPrefix="uc1" %>
<%@ Register Src="~/bitrix/controls/Main/CustomFieldList.ascx" TagName="CustomFieldList" TagPrefix="uc1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
		<bx:InlineScript runat="server" ID="Script" AsyncMode="ScriptBlock">
		<script type="text/javascript">
		function iblockSection_ListAdmin()
		{
			window.location.href = 'IBlockListAdmin.aspx?iblock_id=<%= iblockId %>&type_id=<%= typeId %><%= redirectSectionId >= 0 ? "&filter_sectionid=" + redirectSectionId : "" %>';
		}
		function iblockSection_Add()
		{
			window.location.href = 'IBlockSectionEdit.aspx?iblock_id=<%= iblockId %>&type_id=<%= typeId %><%= redirectSectionId >= 0 ? "&section_id=" + redirectSectionId : "" %>';
		}
		</script>
		</bx:InlineScript>
			<%
				ListButton.Href = "IBlockListAdmin.aspx?iblock_id=" + iblockId +"&type_id=" + typeId + (redirectSectionId >= 0 ? "&filter_sectionid=" + redirectSectionId : "");
				AddButton.Href = "IBlockSectionEdit.aspx?iblock_id=" + iblockId + "&type_id=" + typeId + (redirectSectionId >= 0 ? "&section_id=" + redirectSectionId : "");
			%>
			<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" OnCommandClick="BXContextMenuToolbar1_CommandClick" runat="server">
				<Items>
					<bx:BXCmSeparator SectionSeparator="true" />
					<bx:BXCmImageButton ID="ListButton"
						CssClass="context-button icon btn_list" CommandName="go2list"
						Text="" Title=""
					/>
					<bx:BXCmSeparator />
					<bx:BXCmImageButton ID="AddButton"
						CssClass="context-button icon btn_new" CommandName="AddNewSection"
						Text="" Title="" 
					/>
					<bx:BXCmSeparator />
					<bx:BXCmImageButton ID="DeleteButton"
						CssClass="context-button icon btn_delete" CommandName="DeleteSection"
						Text="" Title=""
						ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText. %>"
					/>
				</Items>
			</bx:BXContextMenuToolbar>

			<bx:BXValidationSummary ID="errorMassage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="SectionEdit"/>
			<bx:BXMessage ID="successMessage" runat="server" Content="<%$ Loc:Message.RecordHasBeenModifiedSuccessfully %>"
				CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False" Width="438px" />

			<asp:HiddenField ID="hfIBlockId" runat="server" />
			<asp:HiddenField ID="hfTypeId" runat="server" />
			<asp:HiddenField ID="hfSectionId" runat="server" />

			<bx:BXTabControl ID="BXTabControl1" runat="server" OnCommand="BXTabControl1_Command" ValidationGroup="SectionEdit">
				<bx:BXTabControlTab ID="BXTabControlTab1" runat="server" Selected="True" Text="<%$ Loc:TabText.Groups %>" Title="<%$ Loc:TabText.Groups %>">
					<table id="tblSection" border="0" cellpadding="0" cellspacing="0" class="edit-table">
						<tr valign="top" id="trID" runat="server">
							<td class="field-name" width="40%" runat="server">
								ID:</td>
							<td width="60%" runat="server">
								<asp:Label ID="lbID" runat="server"></asp:Label>
							</td>
						</tr>
						<tr valign="top" id="trUpdateDate" runat="server">
							<td class="field-name" width="40%" runat="server">
								<%= GetMessage("Legend.DateOfModification") %></td>
							<td width="60%" runat="server">
								<asp:Label ID="lbUpdateDate" runat="server"></asp:Label>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<%= GetMessage("Legend.IsActive") %></td>
							<td width="60%">
								<asp:CheckBox ID="cbActive" runat="server" />
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<%= GetMessage("Legend.ParentSection") %></td>
							<td width="60%">
								<asp:DropDownList ID="ddlParentSection" runat="server">
								</asp:DropDownList>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<span class="required">*</span><%= GetMessage("Legend.Title") %></td>
							<td width="60%">
								<asp:TextBox ID="tbName" Width="350px" runat="server"></asp:TextBox>
								<asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="tbName" ErrorMessage="<%$ Loc:ErrorMessage.TitleIsRequired %>" ValidationGroup="SectionEdit">*</asp:RequiredFieldValidator>
							</td>
						</tr>
						
						<%
					if (CustomFieldList1.HasItems)
					{
						%>
						<tr class="heading">
							<td colspan="2"><%= GetMessage("Legend.Properties") %>:</td>
						</tr>
						<tr valign="top">
							<td colspan="2" align="center">
								<asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
						        <ContentTemplate>
			                        <uc1:CustomFieldList ID="CustomFieldList1" runat="server" EditMode="false" ValidationGroup="SectionEdit" />
								</ContentTemplate>
								</asp:UpdatePanel>
							</td>
						</tr>
						<%
					}
						%>
						<tr class="heading">
							<td colspan="2">
								<%= GetMessage("Legend.Description") %>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<%= GetMessage("Legend.Image") %></td>
							<td width="60%">
								&nbsp;<uc1:AdminImageField ID="Img" runat="server" MaxImageHeight="400" />
							</td>
						</tr>
						<tr valign="top">
							<td colspan="2" align="center">
								<asp:RadioButtonList ID="rblDescriptionType" runat="server" RepeatDirection="Horizontal">
									<asp:ListItem Selected="True" Value="text" Text="<%$ Loc:Legend.Text %>"></asp:ListItem>
									<asp:ListItem Value="html" Text="Html"></asp:ListItem>
								</asp:RadioButtonList>
								<asp:TextBox ID="tbDescription" runat="server" Columns="60" Rows="15" TextMode="MultiLine" Width="100%" ></asp:TextBox>
								<bx:BXWebEditor ID="bxweDescription" runat="server" 
									Width="100%" Height="400px"
									StartModeSelector="True"
									StartMode="HTMLVisual"
									UseHTMLEditor="False" AutoLoadContent="True" ContentType="Text" FullScreen="False" LimitCodeAccess="False" TemplateId="" UseOnlyDefinedStyles="False" WithoutCode="False">
								<Taskbars>
						            <bx:BXWebEditorBar Name="BXPropertiesTaskbar" />
                                </Taskbars>			
							    </bx:BXWebEditor>
							</td>
						</tr>
					</table>
				</bx:BXTabControlTab>
				<bx:BXTabControlTab ID="BXTabControlTab2" runat="server" Text="<%$ Loc:TabText.AdditionalParams %>" Title="<%$ Loc:TabTitle.AdditionalParams %>">
					<table id="tblAdditional" border="0" cellpadding="0" cellspacing="0" class="edit-table">
						<tr valign="top">
							<td class="field-name" width="40%">
								<%= GetMessage("Legend.SortIndex") %></td>
							<td width="60%">
								<asp:TextBox ID="tbSort" runat="server"></asp:TextBox>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<%= GetMessage("Legend.Code") %></td>
							<td width="60%">
								<asp:TextBox ID="tbCode" runat="server"></asp:TextBox>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<%= GetMessage("Legend.ExternalKey") %></td>
							<td width="60%">
								<asp:TextBox ID="tbXmlId" runat="server"></asp:TextBox>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<%= GetMessage("Legend.DetailImage") %></td>
							<td width="60%">
								<uc1:AdminImageField ID="DetailImg" runat="server" MaxImageHeight="400" />
							</td>
						</tr>
					</table>
				</bx:BXTabControlTab>
			</bx:BXTabControl>
</asp:Content>
