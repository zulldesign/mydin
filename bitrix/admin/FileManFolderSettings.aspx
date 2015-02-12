<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="FileManFolderSettings.aspx.cs" Inherits="bitrix_admin_FileManFolderSettings"
	Title="<%$ LocRaw:PageTitle %>" EnableViewState="false" %>

<%@ Register Src="../controls/Main/OperationsEdit.ascx" TagName="OperationsEdit"
	TagPrefix="uc1" %>
<%@ Import Namespace="Bitrix.IO" %>
<%@ Import Namespace="Bitrix.UI" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<%@ Import Namespace="Bitrix.Security" %>
<%@ Import Namespace="System.Collections.Generic" %>
<asp:Content ID="mainContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<% BXCmImageButton1.Href = String.Concat("FileMan.aspx?path=",UrlEncode(curPath)); %>
	<bx:BXContextMenuToolbar ID="MainActionBar" runat="server">
		<Items>
			<bx:BXCmSeparator ID="BXCmSeparator1" runat="server" SectionSeparator="true" />
			<bx:BXCmImageButton ID="BXCmImageButton1" runat="server" CssClass="context-button icon btn_folder_up"
				Text="<%$ Loc:ActionText.GoBack %>" Title="<%$ Loc:ActionTitle.GoBack %>" />
		</Items>
	</bx:BXContextMenuToolbar>
	<bx:BXValidationSummary ID="ErrorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" />
	<bx:BXMessage ID="SuccessMessage" runat="server" Content="<%$ Loc:Message.Success %>"
		CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False"
		Width="438px" />
	<bx:BXTabControl ID="MainTabControl" runat="server" OnCommand="mainTabControl_Command">
		<bx:BXTabControlTab runat="server" Selected="True" Text="<%$ LocRaw:TabText.Properties %>"
			Title="<%$ LocRaw:TabTitle.Properties %>" ShowTitle="True">
			<table runat="server" cellspacing="0" cellpadding="0" border="0" id="EditTable" class="edit-table">
				<tr>
					<td width="40%" class="field-name">
						<%= GetMessage("Label.SectionName") + ":" %>
					</td>
					<td width="60%">
						<asp:TextBox runat="server" ID="Name" MaxLength="255" Columns="50" />
					</td>
				</tr>
				<tr class="heading">
					<td colspan="2">
						<%= GetMessage("Heading.FolderProperties") %></td>
				</tr>
				<tr>
					<td align="center" colspan="2">
						<table cellspacing="1" cellpadding="3" border="0" class="internal">
							<tr class="heading">
								<td align="center">
									<%= GetMessage("Label.KeywordCode") %></td>
								<td align="center">
									<%= GetMessage("Label.KeywordValue") %></td>
							</tr>
							<%
								foreach (Keyword k in state)
								{
							%>
							<tr>
								<td>
									<%
										if (!string.IsNullOrEmpty(k.Name) && keywords.ContainsKey(k.Name))
										{
									%>
									<input type="hidden" value="<%= Encode(k.Name) %>" name="<%= EditTable.UniqueID + "$CODE" %>" />
									<input type="text" style="background-color: rgb(241, 241, 241);" readonly="" size="30"
										value="<%= Encode(keywords[k.Name]) %>" />
									<%
										}
										else
										{
									%>
									<input type="text" size="30" value="<%= Encode(k.Name) %>" name="<%= EditTable.UniqueID + "$CODE" %>" />
									<%
										}
									%>
								</td>
								<td>
									<input type="text" size="60" value="<%= Encode(k.Value) %>" name="<%= EditTable.UniqueID + "$VALUE" %>" />
									<% 
										if (string.IsNullOrEmpty(k.Value) && !string.IsNullOrEmpty(k.Inherited))
										{
									%>
									<br /><b><%= GetMessage("Label.CurrentValue") + ":" %> </b>
									<%= Encode(k.Inherited)%>
									<%
										}
									%>
								</td>
							</tr>
							<%
							}
							%>
							<tr>
								<td align="right" colspan="2">
									<asp:Button runat="server" ID="More" Text="<%$ LocRaw:ButtonText.More %>" OnClick="More_Click" />
								</td>
							</tr>
						</table>
					</td>
				</tr>
			</table>
		</bx:BXTabControlTab>
	</bx:BXTabControl>
</asp:Content>
