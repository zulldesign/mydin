<%--
<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="AuthTasksEdit.aspx.cs" Inherits="bitrix_admin_AuthTasksEdit" Title="Создание задачи" Theme="AdminTheme" StylesheetTheme="AdminTheme"%>
zg
--%>
<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="AuthTasksEdit.aspx.cs" Inherits="bitrix_admin_AuthTasksEdit" Title="<%$ Loc:PageTitle.CreationOfTask %>"%>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<asp:UpdatePanel ID="UpdatePanel1" runat="server">
		<ContentTemplate>
			<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" runat="server" OnCommandClick="BXContextMenuToolbar1_CommandClick">
				<Items>
					<bx:BXCmSeparator SectionSeparator="true" />
					<bx:BXCmImageButton CssClass="context-button icon btn_list" CommandName="go2list"
						Text="<%$ Loc:ActionText.TaskList %>" Title="<%$ Loc:ActionTitle.Go2TaskList %>"
						Href="AuthTasksList.aspx" 
					/>
					<bx:BXCmSeparator SectionSeparator="true" />
					<bx:BXCmImageButton ID="AddTaskButton" CssClass="context-button icon btn_new" CommandName="AddNewTask"
						Text="<%$ Loc:ActionText.NewTask %>" Title="<%$ Loc:ActionTitle.AddNewTask %>"
						Href="AuthTasksEdit.aspx" />
					<bx:BXCmSeparator />
					<bx:BXCmImageButton ID="DeleteTaskButton" CssClass="context-button icon btn_delete" CommandName="DeleteTask"
						Text="<%$ Loc:ActionText.DeleteTask %>" Title="<%$ Loc:ActionTitle.DeleteCurrentTask %>" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>"
					/>
				</Items>
			</bx:BXContextMenuToolbar>
			<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>"/>
			<bx:BXMessage ID="successMessage" runat="server" Content="<%$ Loc:Message.RecordHasBeenModifiedSuccessfully %>"
				CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False" Width="438px" />
			<asp:HiddenField ID="hfTaskId" runat="server" />
			<bx:BXTabControl ID="BXTabControl1" runat="server" OnCommand="BXTabControl1_Command">
				<bx:BXTabControlTab ID="BXTabControlTab1" runat="server" Selected="True" Text="<%$ Loc:TabText.ParametersOfTask %>" Title="<%$ Loc:TabText.ParametersOfTask %>">
					<table id="Table1" border="0" cellpadding="0" cellspacing="0" class="edit-table">
						<tr valign="top" id="trTasksCount" runat="server">
							<td class="field-name" width="40%">
								<%= GetMessage("LegendText.QuantityOfSubtasks") %></td>
							<td width="60%">
								&nbsp;&nbsp;
								<asp:Label ID="lbTasksCount" runat="server" Text=""></asp:Label>
							</td>
						</tr>
						<tr valign="top" id="trOperationsCount" runat="server">
							<td class="field-name" width="40%">
								<%= GetMessage("LegendText.QuantityOfOperations") %></td>
							<td width="60%">
								&nbsp;&nbsp;
								<asp:Label ID="lbOperationsCount" runat="server" Text=""></asp:Label>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<span class="required">*</span><%= GetMessage("LegendText.Code") %></td>
							<td width="60%">
								&nbsp;<asp:TextBox ID="tbTaskName" runat="server" Width="150"></asp:TextBox>
								<asp:RequiredFieldValidator ID="rfvTaskName" runat="server" ControlToValidate="tbTaskName"
									ErrorMessage="<%$ Loc:ErrorMessage.TaskNameIsRequired %>">*</asp:RequiredFieldValidator>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								<%= GetMessage("LegendText.Title") %></td>
							<td width="60%">
								&nbsp;<asp:TextBox ID="tbTaskTitle" runat="server" Width="300"></asp:TextBox>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name" width="40%">
								&nbsp;<%= GetMessage("LegendText.Comment") %></td>
							<td width="60%">
								&nbsp;<asp:TextBox ID="tbComment" runat="server" Width="300" Height="250" TextMode="MultiLine"></asp:TextBox>
							</td>
						</tr>
					</table>
	
				</bx:BXTabControlTab>
				<bx:BXTabControlTab ID="BXTabControlTab4" runat="server" Text="<%$ Loc:TabText.Subtasks %>" Title="<%$ Loc:TabTitle.ExecutionOfSubtasks %>">
					<table id="Table4" border="0" cellpadding="0" cellspacing="0" class="edit-table">
						<tr valign="top">
							<td class="field-name" width="40%">
								<%= GetMessage("LegendText.ExecutionOfSubtasks") %></td>
							<td width="60%">
								<asp:ListBox ID="lbTasks" runat="server" Rows="20" SelectionMode="Multiple"></asp:ListBox>
							</td>
						</tr>
					</table>
				</bx:BXTabControlTab>
				<bx:BXTabControlTab ID="tctOperations" runat="server" Text="<%$ Loc:TabText.Operations %>" Title="<%$ Loc:TabTitle.ExecutionOfOperations %>">
					<table id="tblOperations" runat="server" border="0" cellpadding="0" cellspacing="0" class="edit-table">
					</table>
				</bx:BXTabControlTab>
			</bx:BXTabControl>
			<bx:BXAdminNote runat="server" ID="Remarks">
				<p>
				<span class="required" style="vertical-align:super" id="remark1" >1</span>
				<%= GetMessageRaw("Note1") %>
				</p>
			</bx:BXAdminNote>
		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>

