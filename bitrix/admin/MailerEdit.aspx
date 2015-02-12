<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="MailerEdit.aspx.cs" Inherits="bitrix_admin_MailerEdit" Title="<%$ Loc:PageTitle %>"
	Trace="false" ValidateRequest="false" %>	

<%@ Import Namespace="Bitrix.Services" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<asp:UpdatePanel ID="UpdatePanel1" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
		<ContentTemplate>
			<% CopyButton.Href = String.Concat("MailerEdit.aspx?action=new&id=", templateId); %>
			<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" runat="server" OnCommandClick="BXContextMenuToolbar1_CommandClick">
				<Items>
					<bx:BXCmSeparator runat="server" SectionSeparator="True" />
					<bx:BXCmImageButton runat="server" CssClass="context-button icon btn_list" CommandName="getlist"
						Text="<%$ Loc:ActionText.Templates %>" Title="<%$ Loc:ActionTitle.Templates %>"
						Href="Mailer.aspx" />
					<bx:BXCmSeparator ID="AddSeparator" SectionSeparator="True" />
					<bx:BXCmImageButton ID="AddButton" CssClass="context-button icon btn_new" CommandName="new"
						Text="<%$ Loc:Kernel.Add %>" Title="<%$ Loc:ActionTitle.Add %>" Href="MailerEdit.aspx?action=add"
					/>
					<bx:BXCmSeparator ID="CopySeparator" />
					<bx:BXCmImageButton ID="CopyButton" CssClass="context-button icon btn_copy"
						CommandName="copy" Text="<%$ Loc:Kernel.Copy %>" Title="<%$ Loc:ActionTitle.Copy %>"
					/>
					<bx:BXCmSeparator ID="DeleteSeparator" />
					<bx:BXCmImageButton ID="DeleteButton" CssClass="context-button icon btn_delete"
						CommandName="delete" Text="<%$ Loc:Kernel.Delete %>" Title="<%$ Loc:ActionTitle.Delete %>"
						ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" />
				</Items>
			</bx:BXContextMenuToolbar>
			<asp:HiddenField ID="IdHiddenField" runat="server" />
			<bx:BXMessage ID="successMessage" runat="server" Content="<%$ Loc:Message.OperationSuccessful %>"
				CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False" Width="438px" EnableViewState="false" />
			<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>"/>
			<bx:BXTabControl ID="BXTabControl1" runat="server" OnCommand="BXTabControl1_Command">
				<bx:BXTabControlTab ID="edittab" runat="server" Selected="True" Text="<%$ Loc:TabText.Template %>"
					Title="<%$ Loc:TabTitle.Template %>">
					<table id="Table1" border="0" cellpadding="0" cellspacing="0" class="edit-table">
						<% 
							if (editTemplate)
							{
						%>
						<tr valign="top">
							<td class="field-name" >
								<asp:Literal ID="LabelLU" runat="server" EnableViewState="False" Text="<%$ Loc:EdUpdated %>" />:
							</td>
							<td style="width: 60%">
								<asp:Literal ID="lastUpdated" runat="server" EnableViewState="False" />
							</td>
						</tr>
						<%
							}
						%>
						<tr valign="top">
							<td class="field-name">
								<asp:Literal ID="LabelActive" runat="server" EnableViewState="False" Text="<%$ Loc:EdActive %>"/>: </td>
							<td style="width: 60%">
								<asp:CheckBox ID="ActiveCheckBox" runat="server" Checked="True" /></td>
						</tr>
						<tr valign="top">
							<td class="field-name">
								<span class="required">*</span><asp:Literal ID="LiteralSite" runat="server" EnableViewState="False"
									Text="<%$ Loc:SiteLabel %>"/>: </td>
							<td>
								<table border="0" cellspacing="0" cellpadding="0">
								<tr><td>
								<asp:CheckBoxList ID="siteList" runat="server"/>
								</td><td>&nbsp;</td><td style="vertical-align: middle">
								<span runat="server" id="siteStar" class="required" visible="false" >*</span>
								</td></tr>
								</table>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name">
								<span class="required">*</span>
								<asp:Literal ID="Literal4" runat="server" EnableViewState="False" Text="<%$ Loc:MailEventType %>" />:
							</td>
							<td>
								<asp:Label ID="typeIdLabel" runat="server" EnableViewState="False" Visible="False"></asp:Label>
								<asp:DropDownList ID="mailEventTypes" runat="server" Width="90%" AutoPostBack="True"
									EnableViewState="False">
								</asp:DropDownList>
								<span runat="server" id="nameStar" class="required" visible="false" >*</span>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name">
								<span class="required">*</span><asp:Literal ID="Label2" runat="server" EnableViewState="False"
									Text="<%$ Loc:FromLabel %>" />: </td>
							<td>
								<asp:TextBox ID="FromTextBox" runat="server" Width="200px" Text="#DEFAULT_EMAIL_FROM#"></asp:TextBox>
								<span runat="server" id="fromStar" class="required" visible="false" >*</span>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name">
								<span class="required">*</span><asp:Literal ID="Label3" runat="server" EnableViewState="False"
									Text="<%$ Loc:ToLabel %>"/>: </td>
							<td>
								<asp:TextBox ID="ToTextBox" runat="server" Width="200px"></asp:TextBox>
								<span runat="server" id="toStar" class="required" visible="false" >*</span>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name">
								<asp:Literal ID="Label6" runat="server" EnableViewState="False" Text="<%$ Loc:BccLabel %>"/>: </td>
							<td>
								<asp:TextBox ID="BccTextBox" runat="server" Width="200px"></asp:TextBox></td>
						</tr>
						<tr valign="top">
							<td class="field-name">
								<asp:Literal ID="Label4" runat="server" EnableViewState="False" Text="<%$ Loc:SubjectLabel %>"/>: </td>
							<td>
								<asp:TextBox ID="SubjectTextBox" runat="server" Width="200px"></asp:TextBox>
							</td>
						</tr>
						<tr valign="top">
							<td class="field-name">
								<asp:Literal ID="Label5" runat="server" EnableViewState="False" Text="<%$ Loc:BodyTypeLabel %>"/>: </td>
							<td>
								<asp:RadioButton ID="RadioText" runat="server" Text="<%$ Loc:TextLabel %>" GroupName="ContentType" />
								<asp:RadioButton ID="RadioHtml" runat="server" Text="<%$ Loc:HtmlLabel %>" GroupName="ContentType"
									Checked="True" />
							</td>
						</tr>
						<tr class="heading">
							<td colspan="2">
								<asp:Literal ID="Literal1" runat="server" EnableViewState="False" Text="<%$ Loc:Body %>" />: </td>
						</tr>
						<tr valign="top">
							<td align="center" colspan="2">
								<asp:TextBox ID="MessageTextBox" runat="server" Height="274px" TextMode="MultiLine"
									Width="100%"></asp:TextBox></td>
						</tr>
						<tr>
							<td colspan="2">
								<b>
									<asp:Literal ID="Literal2" runat="server" EnableViewState="False" Text="<%$ Loc:AvailableFields %>" />: </b>
								<br />
								<asp:Literal ID="availableFields" runat="server" EnableViewState="False" /></td>
						</tr>
						<tr valign="top">
							<td align="left" colspan="2">
							</td>
						</tr>
					</table>
				</bx:BXTabControlTab>
			</bx:BXTabControl>
		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>
