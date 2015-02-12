<%--<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="AuthOperationsEdit.aspx.cs" Inherits="bitrix_admin_AuthOperationsEdit" Title="Создание операции" Theme="AdminTheme" StylesheetTheme="AdminTheme"%>
zg --%>

<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="AuthOperationsEdit.aspx.cs" Inherits="bitrix_admin_AuthOperationsEdit" Title="<%$ Loc:PageTitle %>"%>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<asp:UpdatePanel ID="UpdatePanel1" runat="server">
		<ContentTemplate>
			<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" runat="server" OnCommandClick="BXContextMenuToolbar1_CommandClick">
				<Items>
					<bx:BXCmSeparator runat="server" SectionSeparator="true" />
					<bx:BXCmImageButton runat="server" CssClass="context-button icon btn_list" CommandName="go2list"
						Text="<%$ Loc:ActionText.Go2List %>" Title="<%$ Loc:ActionTitle.Go2List %>"
						Href="AuthOperationsList.aspx" />
					<bx:BXCmSeparator ID="AddOperationSeparator" SectionSeparator="true" />
					<bx:BXCmImageButton ID="AddOperationButton" CssClass="context-button icon btn_new" CommandName="AddNewOperation"
						Text="<%$ Loc:ActionText.AddNewOperation %>" Title="<%$ Loc:ActionTitle.AddNewOperation %>"
						Href="AuthOperationsEdit.aspx" />
					<bx:BXCmSeparator ID="DeleteOperationSeparator" />
					<bx:BXCmImageButton ID="DeleteOperationButton" CssClass="context-button icon btn_delete" CommandName="DeleteOperation"
						Text="<%$ Loc:ActionText.DeleteOperation %>" Title="<%$ Loc:ActionTitle.DeleteOperation %>"  ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" />
				</Items>
			</bx:BXContextMenuToolbar>
			<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>"/>
			<bx:BXMessage ID="successMessage" runat="server" Content="<%$ Loc:Message.RecordHasBeenSuccessfullyChanged %>"
				CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False" Width="438px" />
			<asp:HiddenField ID="hfOperationId" runat="server" />
			<bx:BXTabControl ID="BXTabControl1" runat="server" OnCommand="BXTabControl1_Command">
				<bx:BXTabControlTab ID="BXTabControlTab1" runat="server" Selected="True" Text="<%$ Loc:TabText.OperationParams %>" Title="<%$ Loc:TabText.OperationParams %>">
					<table id="Table1" border="0" cellpadding="0" cellspacing="0" class="edit-table">
						<tr valign="top">
							<td class="field-name" width="40%">
								<%= GetMessage("LegendText.Caption") %></td>
							<td width="60%">
								<asp:TextBox ID="tbOperationName" runat="server"></asp:TextBox>
								<asp:RequiredFieldValidator ID="rfvOperationName" runat="server" ControlToValidate="tbOperationName" Display="Static"
									ErrorMessage="<%$ Loc:RequiredFieldValidatorErrorMessage.OperationName %>">*</asp:RequiredFieldValidator>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								&nbsp;<%= GetMessage("LegendText.Comment") %></td>
							<td width="60%">
								<asp:TextBox ID="tbComment" runat="server" Rows="3" TextMode="MultiLine"></asp:TextBox>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<%= GetMessage("LegendText.OperationType") %></td>
							<td width="60%">
								<asp:TextBox ID="tbOperationType" runat="server"></asp:TextBox>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<%= GetMessage("LegendText.Module") %></td>
							<td width="60%">
								<asp:DropDownList ID="ddlModuleId" runat="server">
								</asp:DropDownList>
							</td>
						</tr>
					</table>
				</bx:BXTabControlTab>
			</bx:BXTabControl>
		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>

