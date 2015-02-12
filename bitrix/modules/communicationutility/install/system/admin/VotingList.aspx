<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="false"
    CodeFile="VotingList.aspx.cs" Inherits="bitrix_admin_VotingList" Title="<%$ LocRaw:PageTitle %>" %>
<%@ Register Assembly="System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" Namespace="System.Web.UI" TagPrefix="cc1" %>
<%@ Register Src="~/bitrix/controls/Main/Calendar.ascx" TagName="Calendar" TagPrefix="bx" %>
<%@ Register Src="~/bitrix/admin/controls/Main/AdminImageField.ascx" TagName="AdminImageField" TagPrefix="bx" %>
<%@ Register Src="~/bitrix/admin/controls/CommunicationUtility/VotingSubjectLabel.ascx" TagName="VotingSubjectLabel" TagPrefix="bx" %>
<%@ Import Namespace="Bitrix.CommunicationUtility" %>
<%@ Import Namespace="Bitrix.Modules" %>
<%@ Import Namespace="Bitrix.UI" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <asp:UpdatePanel ID="UpdatePanel" runat="server">
        <ContentTemplate>
            <bx:BXAdminFilter ID="ItemFilter" runat="server" AssociatedGridView="ItemGrid">
                <bx:BXTextBoxFilter Key="ID" Text="ID" ValueType="Integer" Visibility="AlwaysVisible"/> 				
				<bx:BXTextBoxFilter ID="BoundEntityId" Key="BoundEntityId" Text="<%$ Loc:ColumnHeaderText.BoundEntityId %>" ValueType="String" runat="server" />
				<bx:BXDropDownFilter ID="Active" runat="server" Key="Active" ValueType="Boolean" Text="<%$ Loc:FilterText.Active %>">
					<asp:ListItem Value="true" Text="<%$ Loc:Kernel.Yes %>" ></asp:ListItem>
					<asp:ListItem Value="false" Text="<%$ Loc:Kernel.No %>" ></asp:ListItem>
				</bx:BXDropDownFilter>
				
                <bx:BXDropDownFilter ID="BoundEntityTypeId" runat="server" Key="BoundEntityTypeId" ValueType="String" Text="<%$ Loc:Filter.TypeName %>" Visibility="AlwaysVisible">
                    <asp:ListItem Value="" Text="<%$ Loc:NotSelectedMasculine %>"></asp:ListItem>
                    <asp:ListItem Value="USER" Text="<%$ Loc:VotingEntityType.USER %>"></asp:ListItem>
                    <asp:ListItem Value="FORUMPOST" Text="<%$ Loc:VotingEntityType.FORUMPOST %>"></asp:ListItem>
                    <asp:ListItem Value="FORUMTOPIC" Text="<%$ Loc:VotingEntityType.FORUMTOPIC %>"></asp:ListItem>
                    <asp:ListItem Value="IBLOCKELEMENT" Text="<%$ Loc:VotingEntityType.IBLOCKELEMENT %>"></asp:ListItem>
                    <asp:ListItem Value="BLOGPOST" Text="<%$ Loc:VotingEntityType.BLOGPOST %>"></asp:ListItem>
                    <asp:ListItem Value="BLOGCOMMENT" Text="<%$ Loc:VotingEntityType.BLOGCOMMENT %>"></asp:ListItem>
				</bx:BXDropDownFilter>
								
				<bx:BXTextBoxFilter ID="VotingSubjectContains" Key="VotingSubjectContains" runat="server" Text="<%$ Loc:Filter.VotingSubject %>" Visibility="AlwaysVisible"/>
				<bx:BXTimeIntervalFilter ID="CreatedUtc" runat="server" Key="CreatedUtc" Text="<%$ LocRaw:Filter.CreatedUtc %>" />
                <bx:BXTimeIntervalFilter ID="LastCalculatedUtc" runat="server" Key="LastCalculatedUtc" Text="<%$ LocRaw:Filter.LastCalculatedUtc %>" />
                <bx:BXBetweenFilter ID="TotalPositiveVotes" runat="server" Key="TotalPositiveVotes" ValueType="Integer" Text="<%$ LocRaw:Filter.TotalPositiveVotes %>" />
                <bx:BXBetweenFilter ID="TotalNegativeVotes" runat="server" Key="TotalNegativeVotes" ValueType="Integer" Text="<%$ LocRaw:Filter.TotalNegativeVotes %>" />
                <bx:BXBetweenFilter ID="TotalVotes" runat="server" Key="TotalVotes" ValueType="Integer" Text="<%$ LocRaw:Filter.TotalVotes %>" />
                <bx:BXBetweenFilter ID="TotalValue" runat="server" Key="TotalValue" ValueType="Integer" Text="<%$ LocRaw:Filter.TotalValue %>" />
                <%--<bx:BXTextBoxStringFilter Key="XmlId" Text="<%$ LocRaw:FilterText.XmlId %>" />--%>
            </bx:BXAdminFilter>
            <bx:BXContextMenuToolbar ID="ItemListToolbar" runat="server">
                <Items>
                </Items>
            </bx:BXContextMenuToolbar>
            <bx:BXPopupPanel ID="PopupPanelView" runat="server">
                <Commands>
                    <bx:CommandItem UserCommandId="view" Default="True" IconClass="view" ItemText="<%$ LocRaw:PopupText.View %>"
                        ItemTitle="<%$ LocRaw:PopupTitle.View %>" OnClickScript="window.location.href = 'VotingView.aspx?id=' + UserData['ID']; return false;" />
                    <bx:CommandItem UserCommandId="recalculate" Default="False" ItemText="<%$ LocRaw:PopupText.Recalculate %>" ItemTitle="<%$ LocRaw:PopupTitle.Recalculate %>" OnClickScript="window.location.href = 'VotingList.aspx?recalculate=Y&RecalculateId=' + UserData['ID']; return false;" />     
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
                OnDelete="ItemGrid_Delete" OnRowDataBound="ItemGrid_RowDataBound"
                AjaxConfiguration-UpdatePanelId="UpdatePanel1"
                OnUpdate = "GridView1_Update">
                <Columns> 
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Id %>" SortExpression="ID" Visible="false" >
                        <ItemTemplate>
                            <%# BXWordBreakingProcessor.Break(((RatingVotingWrapper)Container.DataItem).ID, 30, true)%>
                        </ItemTemplate>
                    </asp:TemplateField>
                                       
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.VotingSubject %>">
                        <ItemTemplate>
                            <bx:VotingSubjectLabel runat="server" ItemId="<%# (((RatingVotingWrapper)Container.DataItem).RatedItemId) %>" 
                            TypeName="<%# ((RatingVotingWrapper)Container.DataItem).TypeName %>" DisplayTypeName="true" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <%--<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.TypeName %>" SortExpression="BoundEntityTypeId">
                        <ItemTemplate>
                            <%# GetMessage("VotingEntityType."+((RatingVotingWrapper)Container.DataItem).TypeName)%>
                        </ItemTemplate>
                    </asp:TemplateField>--%>                    
                                                          
                    
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.TotalPositiveVotes %>" SortExpression="TotalPositiveVotes">
                        <ItemTemplate>
                            <%# ((RatingVotingWrapper)Container.DataItem).TotalPositiveVotes %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.TotalNegativeVotes %>" SortExpression="TotalNegativeVotes">
                        <ItemTemplate>
                            <%# ((RatingVotingWrapper)Container.DataItem).TotalNegativeVotes %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.TotalVotes %>" SortExpression="TotalVotes">
                        <ItemTemplate>
                            <%# ((RatingVotingWrapper)Container.DataItem).TotalVotes %>
                        </ItemTemplate>
                    </asp:TemplateField>                    
                    
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.TotalValue %>" SortExpression="TotalValue">
                        <ItemTemplate>
                            <%# ((RatingVotingWrapper)Container.DataItem).TotalValue %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Created %>" SortExpression="CreatedUtc">
                        <ItemTemplate>
                            <%# ((RatingVotingWrapper)Container.DataItem).Created %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.LastCalculated %>" SortExpression="LastCalculatedUtc">
                        <ItemTemplate>
                            <%# ((RatingVotingWrapper)Container.DataItem).LastCalculated %>
                        </ItemTemplate>
                    </asp:TemplateField>                                          
                       
                    <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Active %>" SortExpression="Active">
                        <ItemTemplate>
                            <%# ((RatingVotingWrapper)Container.DataItem).Active %>
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
        Bitrix.VotingListGridLegend = [];
        Bitrix.VotingListGridLegend._handleClick = function(e) { Bitrix.EventUtility.stopEventPropagation(e); }
        Bitrix.VotingListGridLegend.prepare = function() {
            var legendAry = document.getElementsByName("Go2Legend");
            for (var i = 0; i < legendAry.length; i++)
                Bitrix.EventUtility.addEventListener(legendAry[i], "click", Bitrix.TypeUtility.createDelegate(this, this._handleClick));
        }
    </script>
</asp:Content>