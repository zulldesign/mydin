<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="false"
    CodeFile="VoteList.aspx.cs" Inherits="bitrix_admin_VoteList" Title="<%$ LocRaw:PageTitle %>" %>
<%@ Register Assembly="System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" Namespace="System.Web.UI" TagPrefix="cc1" %>
<%@ Register Src="~/bitrix/controls/Main/Calendar.ascx" TagName="Calendar" TagPrefix="bx" %>
<%@ Register Src="~/bitrix/admin/controls/CommunicationUtility/VotingSubjectLabel.ascx" TagName="VotingSubjectLabel" TagPrefix="bx" %>
<%@ Register Src="~/bitrix/admin/controls/Main/AdminUserLink.ascx" TagName="AdminUserLink" TagPrefix="bx" %>
<%@ Register Src="~/bitrix/controls/Main/OperationsEdit.ascx" TagName="OperationsEdit" TagPrefix="bx" %>
<%@ Import Namespace="Bitrix.CommunicationUtility" %>
<%@ Import Namespace="Bitrix.UI" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <asp:UpdatePanel ID="UpdatePanel" runat="server">
        <ContentTemplate>
            <bx:BXAdminFilter ID="ItemFilter" runat="server" AssociatedGridView="ItemGrid">
                <bx:BXTextBoxFilter Key="ID" Text="ID" ValueType="Integer" Visibility="AlwaysVisible"/>
                <bx:BXTextBoxFilter ID="RatingVotingId" Key="RatingVotingId" Text="<%$ Loc:FilterText.VotingId %>" ValueType="Integer"  Visibility="AlwaysVisible" runat="server"/>                
			    <bx:BXAutoCompleteFilter 
			        ID="User" 
			        runat="server" 
			        Key="UserId" 
			        Text="<%$ Loc:FilterText.User %>"
			        Url="~/bitrix/handlers/Main/UsersHandler.ashx" 
			        Visibility="AlwaysVisible"
			        TextBoxWidth="300px"
			    />                
				<bx:BXTimeIntervalFilter ID="CreatedUtc" runat="server" Key="CreatedUtc" Text="<%$ Loc:FilterText.CreatedUtc %>" />
                <bx:BXBetweenFilter ID="Value" runat="server" Key="Value" ValueType="Integer" Text="<%$ Loc:FilterText.VoteValue %>" />
            </bx:BXAdminFilter>

            <bx:BXContextMenuToolbar ID="ItemListToolbar" runat="server">
                <Items>
                    <%--<bx:BXCmImageButton ID="AddButton" Text="<%$ LocRaw:ActionText.Add %>" Title="<%$ LocRaw:ActionTitle.Add %>"
                        CssClass="context-button icon btn_new" Href="VoteEdit.aspx" />--%>
                    <bx:BXCmImageButton ID="Go2YListButton" Text="<%$ LocRaw:MenuToolbarText.Go2YList %>" Title="<%$ LocRaw:MenuToolbarTitle.Go2YList %>"
                        CssClass="context-button icon btn_list" Href="" /> 
                    <bx:BXCmImageButton ID="Go2NListButton" Text="<%$ LocRaw:MenuToolbarText.Go2NList %>" Title="<%$ LocRaw:MenuToolbarTitle.Go2NList %>"
                        CssClass="context-button icon btn_list" Href="" />
                    <bx:BXCmImageButton ID="Go2List" Text="<%$ LocRaw:MenuToolbarText.Go2List %>" Title="<%$ LocRaw:MenuToolbarTitle.Go2List %>"
                        CssClass="context-button icon btn_list" Href="" />
                    <bx:BXCmImageButton ID="Go2ListAll" Text="<%$ LocRaw:MenuToolbarText.Go2ListAll %>" Title="<%$ LocRaw:MenuToolbarTitle.Go2ListAll %>"
                        CssClass="context-button icon btn_list" Href="" />                                                                                                                        
                </Items>
            </bx:BXContextMenuToolbar>
            <bx:BXPopupPanel ID="PopupPanelView" runat="server">
                <Commands>
                    <bx:CommandItem UserCommandId="edit" Default="True" IconClass="edit" ItemText="<%$ LocRaw:PopupText.Edit %>"
                        ItemTitle="<%$ LocRaw:PopupTitle.Edit %>" OnClickScript="window.location.href = 'VoteEdit.aspx?id=' + UserData['ID']; return false;" />
                    <bx:CommandItem UserCommandId="delete" Default="False" IconClass="delete" ItemText="<%$ LocRaw:PopupText.Delete %>"
                        ItemTitle="<%$ LocRaw:PopupTitle.Delete %>" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
                </Commands>
            </bx:BXPopupPanel>
            <bx:BXValidationSummary ID="ErrorMessage" runat="server" CssClass="errorSummary"
                HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="GridView" />
            <bx:BXAdminNote ID="BXAdminNote2" runat="server">
                <%= GetMessageRaw("FieldHint")%>
            </bx:BXAdminNote>  
            <bx:BXGridView ID="ItemGrid" runat="server" ContentName="<%$ LocRaw:TableTitle %>"
                AllowSorting="True" AllowPaging="True" DataKeyNames="ID" SettingsToolbarId="ItemListToolbar"
                PopupCommandMenuId="PopupPanelView" ContextMenuToolbarId="MultiActionMenuToolbar"
                DataSourceID="ItemGrid" OnSelect="ItemGrid_Select" OnSelectCount="ItemGrid_SelectCount"
                OnDelete="ItemGrid_Delete" OnRowDataBound="ItemGrid_RowDataBound">
                <Columns> 
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Id %>" SortExpression="ID">
                        <ItemTemplate>
                            <%# BXWordBreakingProcessor.Break(((RatingVoteWrapper)Container.DataItem).ID, 3, true)%>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.VotingId %>" SortExpression="RatingVotingId" Visible="false">
                        <ItemTemplate>
                            <%# ((RatingVoteWrapper)Container.DataItem).VotingId %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.VotingSubject %>" Visible="true" >
                        <ItemTemplate>
                            <bx:VotingSubjectLabel ID="VotingSuject" 
                                DisplayTypeName = "true"
                                ItemId="<%# ((RatingVoteWrapper)Container.DataItem).Voting.RatedItem.Identity %>" 
                                TypeName="<%# ((RatingVoteWrapper)Container.DataItem).Voting.RatedItem.TypeName %>"
                            runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                                        
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.VoteValue %>" SortExpression="Value">
                        <ItemTemplate>
                            <%# BXWordBreakingProcessor.Break(((RatingVoteWrapper)Container.DataItem).VoteValue, 3, true)%>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.User %>" SortExpression="UserId" Visible="true">
                        <ItemTemplate>
                            <bx:AdminUserLink UserId="<%# ((RatingVoteWrapper)Container.DataItem).UserId %>" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.CreatedUtc %>" SortExpression="CreatedUtc">
                        <ItemTemplate>
                            <%# BXWordBreakingProcessor.Break(((RatingVoteWrapper)Container.DataItem).Created, 3, true)%>
                        </ItemTemplate>
                    </asp:TemplateField>
                                        
                </Columns>
                <AjaxConfiguration UpdatePanelId="UpdatePanel" />
            </bx:BXGridView>
            <bx:BXMultiActionMenuToolbar ID="MultiActionMenuToolbar" runat="server" ValidationGroup="GridView">
                <Items>
                    <bx:BXMamImageButton CommandName="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>"
                        ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>" EnabledCssClass="context-button icon delete"
                        DisabledCssClass="context-button icon delete-dis" Title="<%$ LocRaw:ActionTitle.Delete %>" />
                </Items>
            </bx:BXMultiActionMenuToolbar>
            <br />
            <bx:BXAdminNote ID="BXAdminNote1" runat="server">
                <%= GetMessageRaw("FieldLegend")%>
            </bx:BXAdminNote>
            <br />            
        </ContentTemplate>
    </asp:UpdatePanel>
    <script type="text/javascript">
        if (typeof (Bitrix) == "undefined") {
            var Bitrix = [];
        }
        Bitrix.VoteListGridLegend = [];
        Bitrix.VoteListGridLegend._handleClick = function(e) { Bitrix.EventUtility.stopEventPropagation(e); }
        Bitrix.VoteListGridLegend.prepare = function() {
            var legendAry = document.getElementsByName("Go2Legend");
            for (var i = 0; i < legendAry.length; i++)
                Bitrix.EventUtility.addEventListener(legendAry[i], "click", Bitrix.TypeUtility.createDelegate(this, this._handleClick));
        }
    </script>    
</asp:Content>