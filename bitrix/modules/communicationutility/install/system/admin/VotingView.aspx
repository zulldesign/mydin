<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" 
	AutoEventWireup="false" CodeFile="VotingView.aspx.cs" Inherits="bitrix_admin_VotingView" EnableViewState="false"%>
	
<%@ Register Src="~/bitrix/controls/Main/OperationsEdit.ascx" TagName="OperationsEdit"
	TagPrefix="bx" %>
	
<%@ Register Src="~/bitrix/admin/controls/CommunicationUtility/VotingSubjectLabel.ascx" TagName="VotingSubjectLabel" TagPrefix="bx" %>	
	
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	
	<bx:BXContextMenuToolbar ID="BXVoteEditToolbar" runat="server" OnCommandClick="OnToolBarButtonClick">
		<Items>
			<bx:BXCmSeparator ID="ListButton" runat="server" SectionSeparator="true" />
			<bx:BXCmImageButton CssClass="context-button icon btn_list" CommandName="list"
				Text="<%$ LocRaw:ActionText.GoBack %>" Title="<%$ LocRaw:ActionTitle.GoBack %>" Href="VotingList.aspx" />
		
			<bx:BXCmSeparator SectionSeparator="true" />
			<bx:BXCmImageButton ID="DeleteButton"
				CommandName="delete" Text="<%$ LocRaw:Kernel.Delete %>" Title="<%$ LocRaw:ActionTitle.Delete %>"
				CssClass="context-button icon btn_delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>"/>

			<bx:BXCmSeparator SectionSeparator="true" />
			<bx:BXCmImageButton ID="RecalculateButton" 
			    CommandName="recalculate" Text="<%$ Loc:ActionTitle.Recount %>" Title="<%$ Loc:ActionTitle.Recount %>" />
		</Items>
	</bx:BXContextMenuToolbar>
	<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="VoteEdit" />
	<% if (!errorOnPage){ %>
	<bx:BXTabControl ID="TabControl" runat="server">
		<bx:BXTabControlTab ID="MainSettingsTab" Runat="server" Selected="True" Text="<%$ LocRaw:TabText.Category %>" Title="<%$ LocRaw:TabTitle.Category %>">
			
			<table class="edit-table" cellspacing="0" cellpadding="0" border="0">
				<tr align="left">
				    <td class="field-name">ID : </td>
				    <td><%=Id%></td>
				</tr>
		        

		        <tr valign="top">
		            <td class="field-name"><%=GetMessage("FieldLabel.TypeName")%> : </td>
		            <td>
		               <asp:Label ID="VotingTypeName" runat="server"/>
		            </td>
		        </tr>

		        <tr valign="top">
		            <td class="field-name"><%=GetMessage("FieldLabel.VotingSubject")%> : </td>
		            <td>
		               <bx:VotingSubjectLabel ID="VotingSubject" runat="server" />
		            </td>
		        </tr>
		        
		        <tr valign="top">
		            <td class="field-name"><%=GetMessage("FieldLabel.TotalNegativeVotes")%> : </td>
		            <td>
		               <asp:Label ID="VotingTotalNegativeVotes" runat="server" />
		            </td>
		        </tr>

		        <tr valign="top">
		            <td class="field-name"><%=GetMessage("FieldLabel.TotalPositiveVotes")%> : </td>
		            <td>
		               <asp:Label ID="VotingTotalPositiveVotes" runat="server" />
		            </td>
		        </tr>

		        <tr valign="top">
		            <td class="field-name"><%=GetMessage("FieldLabel.TotalVotes")%> : </td>
		            <td>
		               <asp:Label ID="VotingTotalVotes" runat="server" />
		            </td>
		        </tr>

		        <tr valign="top">
		            <td class="field-name"><%=GetMessage("FieldLabel.TotalValue")%> : </td>
		            <td>
		               <asp:Label ID="VotingTotalValue" runat="server" />
		            </td>
		        </tr>
		        
		        
		        <tr valign="top">
		            <td class="field-name"><%=GetMessage("FieldLabel.CreatedUtc")%> : </td>
		            <td>
		               <asp:Label ID="VotingCreatedUtc" runat="server" />
		            </td>
		        </tr>		        

		        <tr valign="top">
		            <td class="field-name"><%=GetMessage("FieldLabel.LastCalculatedUtc")%> : </td>
		            <td>
		               <asp:Label ID="VotingLastCalculatedUtc" runat="server" />
		            </td>
		        </tr>		        
			</table>

		</bx:BXTabControlTab>
	
	</bx:BXTabControl>
	<% } %>
</asp:Content>