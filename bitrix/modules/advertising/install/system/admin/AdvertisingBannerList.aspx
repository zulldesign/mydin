<%@ Page Language="C#"
    MasterPageFile="~/bitrix/admin/AdminMasterPage.master" 
	AutoEventWireup="false" 
	CodeFile="AdvertisingBannerList.aspx.cs" 
	Inherits="bitrix_admin_AdvertisingBannerList" 
	Title="<%$ LocRaw:PageTitle %>" %>


<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<script type="text/javascript" src="/bitrix/js/main/silverlight.debug.js"></script>
	<asp:UpdatePanel ID="UpdatePanel" runat="server">
		<ContentTemplate>
       
			<bx:BXAdminFilter ID="ItemFilter" runat="server" AssociatedGridView="ItemGrid">
				<bx:BXTextBoxStringFilter Key="Name" Text="<%$ LocRaw:FilterText.Name %>" Visibility="AlwaysVisible" />
				<bx:BXTextBoxFilter Key="ID" Text="ID" ValueType="Integer" />
				<bx:BXTextBoxStringFilter Key="Code" Text="<%$ LocRaw:FilterText.Code %>" />
				<bx:BXDropDownFilter runat="server" Text="<%$ LocRaw:FilterText.IsInRotation %>" Key="IsInRotation" ValueType="Boolean">
					<asp:ListItem Value="" Text="<%$ LocRaw:Option.All %>" />
					<asp:ListItem Value="true" Text="<%$ LocRaw:Option.InRotation %>" />
					<asp:ListItem Value="false" Text="<%$ LocRaw:Option.NotInRotation %>" />
				</bx:BXDropDownFilter>
				<bx:BXDropDownFilter ID="abFilterActive" Key="Active" Text="<%$ LocRaw:FilterText.Active %>"></bx:BXDropDownFilter>			
				<bx:BXDropDownFilter ID="DropDownFilterSpaces" runat="server" Text="<%$ LocRaw:FilterText.Space %>" Key="SpaceId" ValueType="Integer">
                </bx:BXDropDownFilter>
				<bx:BXListBoxFilter ID="ListBoxFilterSites" Key="Sites.SiteId" Text="<%$ LocRaw:FilterText.Sites %>" ></bx:BXListBoxFilter>
				<bx:BXTextBoxFilter Key="DisplayCount" Text="<%$ LocRaw:FilterText.DisplayCount %>" ValueType="Integer" />
				<bx:BXTextBoxFilter Key="VisitorCount" Text="<%$ LocRaw:FilterText.VisitorCount %>" ValueType="Integer" />
                <bx:BXTextBoxFilter Key="RedirectionCount" Text="<%$ LocRaw:FilterText.RedirectionCount %>" ValueType="Integer" />				
				<bx:BXTextBoxStringFilter Key="XmlId" Text="<%$ LocRaw:FilterText.XmlId %>" />
			</bx:BXAdminFilter>
			<bx:BXContextMenuToolbar ID="ItemListToolbar" runat="server">
				<Items>
					<bx:BXCmSeparator SectionSeparator="true" />
					<bx:BXCmImageButton ID="AddButton" Text="<%$ LocRaw:ActionText.Add %>"
						Title="<%$ LocRaw:ActionTitle.Add %>" CssClass="context-button icon btn_new"
						Href="AdvertisingBannerEdit.aspx" />
				</Items>
			</bx:BXContextMenuToolbar>
			<bx:BXPopupPanel ID="PopupPanelView" runat="server">
				<Commands>
					<bx:CommandItem UserCommandId="edit" Default="True" IconClass="edit" 
						ItemText="<%$ LocRaw:PopupText.Edit %>" ItemTitle="<%$ LocRaw:PopupTitle.Edit %>" OnClickScript="window.location.href = 'AdvertisingBannerEdit.aspx?id=' + UserData['ID']; return false;"  />
					<bx:CommandItem UserCommandId="delete" Default="True" IconClass="delete" ItemText="<%$ LocRaw:PopupText.Delete %>"
						ItemTitle="<%$ LocRaw:PopupTitle.Delete %>" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
				</Commands>
			</bx:BXPopupPanel>
			<bx:BXValidationSummary ID="ErrorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="GridView"/>
			
			<br />
			
			<bx:BXGridView ID="ItemGrid" runat="server"
				ContentName="<%$ LocRaw:TableTitle %>" AllowSorting="True" AllowPaging="True" DataKeyNames="ID"
				SettingsToolbarId="ItemListToolbar"
				PopupCommandMenuId="PopupPanelView"
				ContextMenuToolbarId="MultiActionMenuToolbar"
				
				DataSourceID="ItemGrid" 
				OnSelect="ItemGrid_Select" 
				OnSelectCount="ItemGrid_SelectCount"
				OnDelete="ItemGrid_Delete"
				OnUpdate="ItemGrid_Update"
				OnRowUpdating="ItemGrid_RowUpdating"
				OnRowDataBound = "ItemGrid_RowDataBound">
					<Columns>
						<asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" ReadOnly="True" Visible="False"/>
						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.IsInRotation %>" SortExpression="IsInRotation">
						    <itemtemplate>
						        <center>
						            <div class="<%# ((AdvertisingBannerWrapper)Container.DataItem).IsInRotation ? "lamp-green" : "lamp-red"%>" title="<%# ((AdvertisingBannerWrapper)Container.DataItem).RotationLegend %>" ></div>
						        </center>
						    </itemtemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Content %>" SortExpression="" >
						    <itemtemplate>
						        <div style=" margin-bottom:4px;">
						            <%# ((AdvertisingBannerWrapper)Container.DataItem).GetContentHTML(600, 400)%>
						        </div>
						    </itemtemplate>							
						</asp:TemplateField>					
						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Name %>" SortExpression="Name">
							<itemtemplate>
								<a href="<%# string.Concat(VirtualPathUtility.ToAbsolute("~/bitrix/admin/AdvertisingBannerEdit.aspx"), "?id=", ((AdvertisingBannerWrapper)Container.DataItem).ID.ToString()) %>" target="_self"><%# Encode(((AdvertisingBannerWrapper)Container.DataItem).Name)%></a>
							</itemtemplate>
							<edititemtemplate>
							<asp:TextBox id="Name" runat="server" Text='<%#  Bind("Name") %>' Width="100%"></asp:TextBox>
							<asp:RequiredFieldValidator ID="NameValidator" runat="server" ControlToValidate="Name"
									ErrorMessage="<%$ Loc:Message.ElementNameIsRequired %>" ValidationGroup="grid" >*</asp:RequiredFieldValidator>
						    </edititemtemplate>
						</asp:TemplateField>																	
						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Active %>" SortExpression="Active" Visible="False">
							<itemtemplate>
								<%# Encode(((AdvertisingBannerWrapper)Container.DataItem).Active)%>
							</itemtemplate>
							<edititemtemplate>
							<asp:CheckBox ID="Active" runat="server" Checked='<%# Bind("isActive") %>' />
						    </edititemtemplate>
						</asp:TemplateField>	
						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Weight %>" SortExpression="Weight" Visible="False">
							<itemtemplate>
								<%# Encode(((AdvertisingBannerWrapper)Container.DataItem).Weight)%>
							</itemtemplate>
							<edititemtemplate>
							<asp:TextBox id="Weight" runat="server" Text='<%#  Bind("Weight") %>' Width="100%"></asp:TextBox>
						    </edititemtemplate>
						</asp:TemplateField>							
						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.SpaceName %>" SortExpression="Space.Name" Visible="False">
							<itemtemplate>
								<%# Encode(((AdvertisingBannerWrapper)Container.DataItem).SpaceName)%>
							</itemtemplate>
							<edititemtemplate>
								<asp:DropDownList id="spaceEdit" runat="server" DataValueField="ID" SelectedValue='<%# Bind("SpaceId") %>' DataTextField="Name" DataSource="<%# AdvSpaces %>">
                                </asp:DropDownList> 
							</edititemtemplate>
						</asp:TemplateField>						
						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.DateOfLastDisplay %>" SortExpression="DateOfLastDisplay" Visible="False">
							<itemtemplate>
								<%# Encode(((AdvertisingBannerWrapper)Container.DataItem).DateOfLastDisplay)%>
							</itemtemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.DateOfFirstDisplay %>" SortExpression="DateOfFirstDisplay" Visible="False">
							<itemtemplate>
								<%# Encode(((AdvertisingBannerWrapper)Container.DataItem).DateOfFirstDisplay)%>
							</itemtemplate>
						</asp:TemplateField>						
						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.EnableUniformRotationVelocity %>" SortExpression="EnableUniformRotationVelocity" Visible="False">
							<itemtemplate>
								<%# Encode(((AdvertisingBannerWrapper)Container.DataItem).EnableUniformRotationVelocity)%>
							</itemtemplate>
						</asp:TemplateField>	
						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.DisplayCount %>" SortExpression="DisplayCount" Visible="True">
							<itemtemplate>
								<%# Encode(((AdvertisingBannerWrapper)Container.DataItem).DisplayCount)%>
							</itemtemplate>						    
						</asp:TemplateField>
						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.RedirectionCount %>" SortExpression="RedirectionCount" Visible="False">
							<itemtemplate>
								<%# Encode(((AdvertisingBannerWrapper)Container.DataItem).RedirectionCount)%>
							</itemtemplate>						    
						</asp:TemplateField>
						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.VisitorCount %>" SortExpression="VisitorCount" Visible="False">
							<itemtemplate>
								<%# Encode(((AdvertisingBannerWrapper)Container.DataItem).VisitorCount)%>
							</itemtemplate>						    
						</asp:TemplateField>
						<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.CTR %>" SortExpression="ClickThroughRation" Visible="False">
							<itemtemplate>
								<%# Encode(((AdvertisingBannerWrapper)Container.DataItem).ClickThroughRation)%>
							</itemtemplate>						    
						</asp:TemplateField>																																																				
					</Columns>
					<AjaxConfiguration UpdatePanelId="UpdatePanel" />
			</bx:BXGridView>
			
			<bx:BXMultiActionMenuToolbar ID="MultiActionMenuToolbar" runat="server" ValidationGroup="GridView">
				<Items>
                    <%--					
                    <bx:BXMamImageButton CommandName="inline" ShowConfirmBar="true" DisableForAll="true"
						EnabledCssClass="context-button icon edit" DisabledCssClass="context-button icon edit-dis"
						Title="������������� ��������� ��������� �������" />
				    --%>
				    <bx:BXMamImageButton  CommandName="inline" ShowConfirmBar="true" DisableForAll="true"
						EnabledCssClass="context-button icon edit" DisabledCssClass="context-button icon edit-dis"
						Title="<%$ Loc:ActionTitle.EditElements %>" />
					<bx:BXMamImageButton CommandName="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>" EnabledCssClass="context-button icon delete"
						DisabledCssClass="context-button icon delete-dis" Title="<%$ LocRaw:ActionTitle.Delete %>" />
				</Items>
			</bx:BXMultiActionMenuToolbar>			

		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>