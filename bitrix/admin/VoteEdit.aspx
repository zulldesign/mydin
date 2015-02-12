<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" 
	AutoEventWireup="false" CodeFile="VoteEdit.aspx.cs" Inherits="bitrix_admin_VoteEdit" EnableViewState="false"%>
<%@ Register Src="~/bitrix/admin/controls/Main/AdminUserLink.ascx" TagName="AdminUserLink" TagPrefix="bx" %>
<%@ Register Src="~/bitrix/controls/Main/OperationsEdit.ascx" TagName="OperationsEdit"
	TagPrefix="bx" %>
	
<%@ Register Src="~/bitrix/admin/controls/CommunicationUtility/VotingSubjectLabel.ascx" TagName="VotingSubjectLabel" TagPrefix="bx" %>	
	
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	
	<bx:BXContextMenuToolbar ID="BXVoteEditToolbar" runat="server" OnCommandClick="OnToolBarButtonClick">
		<Items>
			<bx:BXCmSeparator ID="ListButton" runat="server" SectionSeparator="true" />
			<bx:BXCmImageButton CssClass="context-button icon btn_list" CommandName="list"
				Text="<%$ LocRaw:ActionText.GoBack %>" Title="<%$ LocRaw:ActionTitle.GoBack %>" Href="VoteList.aspx" />
		
			<bx:BXCmSeparator SectionSeparator="true" />
			<bx:BXCmImageButton ID="DeleteButton"
				CommandName="delete" Text="<%$ LocRaw:Kernel.Delete %>" Title="<%$ LocRaw:ActionTitle.Delete %>"
				CssClass="context-button icon btn_delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>"/>
		</Items>
	</bx:BXContextMenuToolbar>
	<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="VoteEdit" />
	<% if (!ErrorsOnPage) { %>
	<bx:BXTabControl ID="TabControl" runat="server" OnCommand="OnVoteEdit">
		<bx:BXTabControlTab ID="MainSettingsTab" Runat="server" Selected="True" Text="<%$ LocRaw:TabText.VoteTabTitle %>" Title="<%$ LocRaw:TabText.VoteTabTitle %>">
			<table class="edit-table" cellspacing="0" cellpadding="0" border="0">
				<tr align="top">
				    <td class="field-name">ID : </td>
				    <td><%=Id %></td>
				</tr>
		        
				<tr align="top">
				    <td class="field-name"><%=GetMessage("FieldLabel.VotingId")%> : </td>
				    <td><asp:Label ID="VoteRatingVotingId" runat="server"></asp:Label></td>
				</tr>

				<tr align="top">
				    <td class="field-name" valign=top><%=GetMessage("FieldLabel.VotingSubject")%> : </td>
				    <td><bx:VotingSubjectLabel runat="server" ID="VotingSubject" /></td>
				</tr>
		        
		        <tr valign="top">
		            <td class="field-name"><%=GetMessage("FieldLabel.CreatedUtc") %> : </td>
		            <td>
		               <asp:Label ID="VoteCreatedUtc" runat="server" />
		            </td>
		        </tr>		        

		        <tr valign="top">
		            <td class="field-name"><%=GetMessage("FieldLabel.VoteValue") %> : </td>
		            <td>
		               <asp:TextBox ID="VoteValue" runat="server" Columns=3 />
		            </td>
		        </tr>		        

                <tr valign="top">
                    <td class="field-name">
                        <%=GetMessage("FieldLabel.User") %> : 
                    </td>
                    <td>
                        <bx:AdminUserLink ID="VoteUser" runat="server" />
                    </td>
                </tr>		        

			</table>
		</bx:BXTabControlTab>
	
	</bx:BXTabControl>
	<% } %>
</asp:Content>