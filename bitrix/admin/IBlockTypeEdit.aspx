<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="IBlockTypeEdit.aspx.cs" Inherits="bitrix_admin_IBlockTypeEdit" Title="<%$ Loc:PageTitle.NewType %>" ValidateRequest="false" %>


<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<asp:UpdatePanel ID="UpdatePanel1" runat="server">
		<ContentTemplate>

			<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" OnCommandClick="BXContextMenuToolbar1_CommandClick" runat="server">
				<Items>
					<bx:BXCmSeparator SectionSeparator="true" />
					<bx:BXCmImageButton
						CssClass="context-button icon btn_list" CommandName="go2list"
						Text="<%$ Loc:ActionText.TypeList %>" Title="<%$ Loc:ActionTitle.Go2TypeList %>"
						Href="IBlockTypeList.aspx"
					/>
					<bx:BXCmSeparator />
					<bx:BXCmImageButton ID="AddButton"
						CssClass="context-button icon btn_new" CommandName="AddNewType"
						Text="<%$ Loc:PageTitle.NewType %>" Title="<%$ Loc:ActionTitle.AddNewType %>" 
						Href="IBlockTypeEdit.aspx" />
					<bx:BXCmSeparator />
					<bx:BXCmImageButton ID="DeleteButton"
						CssClass="context-button icon btn_delete" CommandName="DeleteType"
						Text="<%$ Loc:ActionText.DeleteCurrentType %>" Title="<%$ Loc:ActionTitle.DeleteCurrentType %>"
						ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>"
					/>
				</Items>
			</bx:BXContextMenuToolbar>

			<bx:BXValidationSummary ID="errorMassage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>"/>
			<bx:BXMessage ID="successMessage" runat="server" Content="<%$ Loc:Message.RecordHasBeenModifiedSuccessfully %>"
				CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False" Width="438px" />

			<asp:HiddenField ID="hfTypeId" runat="server" />

			<bx:BXTabControl ID="BXTabControl1" runat="server" OnCommand="BXTabControl1_Command">
				<bx:BXTabControlTab ID="BXTabControlTab1" runat="server" Selected="True" Text="<%$ Loc:TabText.Main %>" Title="<%$ Loc:TabTitle.TypeSettings %>">
					<table border="0" cellpadding="0" cellspacing="0" class="edit-table">
						<tr valign="top" id="trID" runat="server">
							<td class="field-name" width="40%">
								ID:</td>
							<td width="60%">
								<asp:Label ID="lbID" runat="server"></asp:Label>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<%= GetMessage("SortIndex") %></td>
							<td width="60%">
								<asp:TextBox ID="tbSort" runat="server"></asp:TextBox>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<%= GetMessage("Legend.UseTreeViewClassifier") %></td>
							<td width="60%">
								<asp:CheckBox ID="cbHaveSections" runat="server" />
							</td>
						</tr>
						<tr class="heading">
							<td colspan="2">
								<%= GetMessage("Legend.LanguageDependedObjectTitles") %>
							</td>
						</tr>
						<tr>
							<td colspan="2" align="center">
								<table id="tblLangTranslaters" runat="server" border="0" cellspacing="6">
									<tr>
										<td align="center"><%= GetMessage("Tang") %></td>
										<td align="center"><span class="required">*</span><%= GetMessage("Title") %></td>
									</tr>
								</table>
							</td>
						</tr>
					</table>
				</bx:BXTabControlTab>
<%--				<bx:BXTabControlTab ID="BXTabControlTab2" runat="server" Text="<%$ Loc:TabText.AdditionalTypeParams %>" Title="<%$ Loc:TabTitle.AdditionalTypeParams %>">
					<table border="0" cellpadding="0" cellspacing="0" class="edit-table">
						
					</table>
				</bx:BXTabControlTab>--%>
			</bx:BXTabControl>
		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>
