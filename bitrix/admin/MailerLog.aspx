<%@ Page Language="C#" MasterPageFile="AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="MailerLog.aspx.cs" Inherits="bitrix_admin_MailerLog" ValidateRequest="false" Title="<%$ LocRaw:PageTitle %>" %> 	

<%@ Register Assembly="Main" Namespace="Bitrix.UI" TagPrefix="bx" %>
<%@ Import Namespace="Bitrix.Services" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <asp:UpdatePanel ID="UpdatePanel" runat="server">

		<ContentTemplate>
		<bx:BXValidationSummary ID="ErrorMessage" runat="server" CssClass="errorSummary"
				HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="GridView" />
			<br />
		<% if (!HasFatalErrors) { %>
			<bx:BXAdminFilter ID="ItemFilter" runat="server" AssociatedGridView="ItemGrid">
				<bx:BXDropDownFilter ID="BXDropDownFilterStatus" runat="server" Key="Status" Text="<%$ LocRaw:ColumnHeaderText.Status %>" ValueType="String" Visibility="AlwaysVisible">
					<asp:ListItem Value="" Text="<%$ LocRaw:Option.All %>" Selected="True"></asp:ListItem>
					<asp:ListItem Value="Y" Text="<%$ LocRaw:Option.AllSent %>"></asp:ListItem>
					<asp:ListItem Value="N" Text="<%$ LocRaw:N %>"></asp:ListItem>
					<asp:ListItem Value="F" Text="<%$ LocRaw:F %>"></asp:ListItem>
					<asp:ListItem Value="P" Text="<%$ LocRaw:Option.SomeSent %>"></asp:ListItem>
					<asp:ListItem Value="O" Text="<%$ LocRaw:Option.NoTemplate %>"></asp:ListItem>
				</bx:BXDropDownFilter>
				<bx:BXTextBoxFilter Key="Id" Text="Id" ValueType="Integer" />
				<bx:BXTextBoxStringFilter Key="Template" Text="<%$ LocRaw:ColumnHeaderText.Template %>"/>
			</bx:BXAdminFilter>
            <bx:BXContextMenuToolbar ID="ItemListToolbar" runat="server">
			</bx:BXContextMenuToolbar>
			<br />
            <bx:BXGridView ID="ItemGrid" runat="server"
				ContentName="<%$ LocRaw:TableTitle.Send %>" AllowSorting="True" AllowPaging="True" DataKeyNames="ID"  
				
				SettingsToolbarId="ItemListToolbar"
				ContextMenuToolbarId="MultiActionMenuToolbar"
				
				DataSourceID="ItemGrid" 
				OnSelect="ItemGrid_Select" 
				OnSelectCount="ItemGrid_SelectCount"
				OnDelete="ItemGrid_Delete"
				OnRowDataBound = "ItemGrid_RowDataBound">
					<Columns>
						<asp:BoundField DataField="Id" HeaderText="ID" SortExpression="Id" ReadOnly="True"/>

						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Status %>" SortExpression="Status">
							<itemtemplate>								
								<center>
									
						            <div class="<%# ((LogWrapper)Container.DataItem).StatusId == "Y" ? "lamp-green" : ((LogWrapper)Container.DataItem).StatusId == "N" ? "lamp-grey" : ((LogWrapper)Container.DataItem).StatusId == "P" ? "lamp-yellow" : ((LogWrapper)Container.DataItem).StatusId == "O" ? "lamp-blue" : ((LogWrapper)Container.DataItem).StatusId == "F" ? "lamp-red" : "" %>"
										title="<%# ((LogWrapper)Container.DataItem).Status %>" ></div>
						        </center>
							</itemtemplate>
						</asp:TemplateField>

						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Update %>" SortExpression="LastUpdated">
							<itemtemplate>
								<%# ((LogWrapper)Container.DataItem).DateUpdate %>
							</itemtemplate>
						</asp:TemplateField>																	
						
                        <asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Template %>" SortExpression="Template">
							<itemtemplate>
								<%# ((LogWrapper)Container.DataItem).TemplateName %>
							</itemtemplate>
						</asp:TemplateField>	

						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Duplicate %>" SortExpression="Duplicate">
							<itemtemplate>
								<%# ((LogWrapper)Container.DataItem).Duplicate %>
							</itemtemplate>
						</asp:TemplateField>																	
						
						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Subject %>">
							<itemtemplate>
								<%# ((LogWrapper)Container.DataItem).Subject %>
							</itemtemplate>
						</asp:TemplateField>

						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.To %>">
							<itemtemplate>
								<%# ((LogWrapper)Container.DataItem).EmailTo %>
							</itemtemplate>
						</asp:TemplateField>

					</Columns>
					<AjaxConfiguration UpdatePanelId="UpdatePanel" />
			</bx:BXGridView>
            <bx:BXMultiActionMenuToolbar ID="MultiActionMenuToolbar" runat="server" ValidationGroup="GridView">
                <Items>
                    <bx:BXMamListItem
                        runat="server"
                        ID="btSend"
                        Text="<%$ LocRaw:ActionText.Send %>"
                        CommandName="SendSelectItems"
                        ShowConfirmDialog="true"
                        ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Confirm %>">
                    </bx:BXMamListItem>
					<bx:BXMamListItem
                        runat="server"
                        ID="btDelete"
                        Text="<%$ LocRaw:Kernel.Delete %>"
                        CommandName="DeleteSelectItems"
                        ShowConfirmDialog="true"
                        ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Confirm %>">
                    </bx:BXMamListItem>
				</Items>
			</bx:BXMultiActionMenuToolbar>  
            <%} %>
        </ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>