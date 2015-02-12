<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="IBlockListAdmin.aspx.cs" Inherits="bitrix_admin_IBlockListAdmin" Title="Untitled Page" %>
<%@ Register Assembly="System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" Namespace="System.Web.UI" TagPrefix="cc1" %>
<%@ Register Src="~/bitrix/controls/Main/Calendar.ascx" TagName="Calendar" TagPrefix="bx" %>
<%@ Register Src="~/bitrix/admin/controls/Main/AdminImageField.ascx" TagName="AdminImageField" TagPrefix="bx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<asp:UpdatePanel ID="UpdatePanel1" runat="server">
		<ContentTemplate>
			<bx:BXAdminFilter ID="BXAdminFilter1" runat="server" AssociatedGridView="GridView1">
				<bx:BXTextBoxStringFilter ID="BXTextBoxFilter1" runat="server" Key="Name" Text="<%$ Loc:FilterText.Title %>" Visibility="AlwaysVisible" />
				<bx:BXDropDownFilter ID="filterSectionId" runat="server" Key="SectionId" Text="<%$ Loc:FilterText.Chapter %>" ValueType="Integer" />
				<bx:BXBetweenFilter ID="filterId" runat="server" Key="ID" Text="<%$ Loc:FilterText.ID %>" ValueType="Integer" />
				<bx:BXTimeIntervalFilter ID="BXTimeIntervalFilter1" runat="server" Key="UpdateDate" Text="<%$ Loc:FilterText.DateOfModification %>" />
				<bx:BXTextBoxStringFilter ID="BXTextBoxStringFilter1" runat="server" Key="Code" Text="<%$ Loc:FilterText.Code %>" />
				<bx:BXTextBoxStringFilter ID="BXTextBoxStringFilter2" runat="server" Key="XmlId" Text="<%$ Loc:FilterText.ExternalCode %>" />
				<bx:BXAutoCompleteFilter 
				    ID="modifiedByAutocomplete" 
				    runat="server" 
				    Key="ModifiedBy" 
				    Text="<%$ LocRaw:FilterText.ModifiedBy %>"
				    Url="~/bitrix/handlers/Main/UsersHandler.ashx" 
				    TextBoxWidth="460px"
				/>
				<bx:BXTimeIntervalFilter ID="BXTimeIntervalFilter2" runat="server" Key="CreateDate" Text="<%$ Loc:FilterText.CreationDate %>" />
				<bx:BXDropDownFilter ID="BXDropDownFilter1" runat="server" Key="Active" Text="<%$ Loc:FilterText.IsActive %>">
					<asp:ListItem Value="" Text="<%$ Loc:Any %>"></asp:ListItem>
					<asp:ListItem Value="Y" Text="<%$ Loc:Kernel.Yes %>" ></asp:ListItem>
					<asp:ListItem Value="N" Text="<%$ Loc:Kernel.No %>" ></asp:ListItem>
				</bx:BXDropDownFilter>
				<bx:BXTextBoxStringFilter ID="BXTextBoxStringFilter3" runat="server" Key="PreviewText" Text="<%$ Loc:FilterText.Description %>" />
				<bx:BXTextBoxStringFilter ID="BXTextBoxStringFilter4" runat="server" Key="Tags" Text="<%$ LocRaw:FilterText.Tags %>" />
				<bx:BXAutoCompleteFilter 
				    ID="createByAutocomplete" 
				    runat="server" 
				    Key="CreatedBy" 
				    Text="<%$ LocRaw:FilterText.CreatedBy %>"
				    Url="~/bitrix/handlers/Main/UsersHandler.ashx" 
				    TextBoxWidth="460px"
				    />
			</bx:BXAdminFilter>
			
			<%
			int sectionId = 0;
				
			foreach (Bitrix.DataLayer.BXFormFilterItem fi in BXAdminFilter1.CurrentFilter)
			{
				if (fi.filterName.Equals("SectionId", StringComparison.InvariantCultureIgnoreCase))
				{
					int.TryParse(fi.filterValue.ToString(), out sectionId);
					break;
				}
			}

			AddSectionButton.Href = String.Format("IBlockSectionEdit.aspx?type_id={1}&iblock_id={0}&section_id={2}", iblockId, typeId, sectionId);		
			AddElementButton.Href = String.Format("IBlockElementEdit.aspx?type_id={1}&iblock_id={0}&section_id={2}", iblockId, typeId, sectionId);
			IBlockPropertiesButton.Href = String.Format(
				"IBlockEdit.aspx?type_id={1}&id={0}&{2}={3}", 
				iblockId, 
				typeId,
				Bitrix.Configuration.BXConfigurationUtility.Constants.BackUrl,
				UrlEncode(Request.Url.PathAndQuery)
			);
			%>
			
			<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" runat="server" OnCommandClick="BXContextMenuToolbar1_CommandClick">
				<Items>
					<bx:BXCmSeparator SectionSeparator="True" />
					<bx:BXCmImageButton ID="AddSectionButton" CssClass="context-button icon btn_new" CommandName="addSection"
						Text="" Title="" />
					<bx:BXCmSeparator />
					<bx:BXCmImageButton ID="AddElementButton" CommandName="addElement" CssClass="context-button icon btn_new" Text="" Title="" />
					<bx:BXCmSeparator />
					<bx:BXCmImageButton ID="IBlockPropertiesButton" CommandName="iblock" CssClass="context-button icon btn_iblock_settings"
						Text="<%$ Loc:ActionText.InformationalBlockProperties %>" />
				</Items>
			</bx:BXContextMenuToolbar>
			<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="grid" />
			<bx:BXMessage ID="successMessage" runat="server" Content="<%$ Loc:Message.RecordsHaveBeenDeletedSuccessfully %>"
				CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False" />

			<asp:HiddenField ID="hfTypeId" runat="server" />
			<asp:HiddenField ID="hfIBlockId" runat="server" />

			<bx:BXPopupPanel ID="PopupPanel1" runat="server">
				<Commands>
					<bx:CommandItem Default="True" IconClass="edit" ItemText="<%$Loc:PopupText.Change %>" ItemTitle="<%$ Loc:PopupText.Change %>"
						UserCommandId="edit" />
					<bx:CommandItem Default="True" IconClass="view" ItemText="<%$Loc:Kernel.View  %>" ItemTitle="<%$ LocRaw:PopupTitle.View %>"
						UserCommandId="view" />
					<bx:CommandItem IconClass="delete" ItemText="<%$Loc:PopupText.Delete%>" ItemTitle="<%$ Loc:PopupText.Delete %>"
						UserCommandId="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
				</Commands>
			</bx:BXPopupPanel>
			<bx:BXGridView ID="GridView1" runat="server" 
				ContextMenuToolbarId="MultiActionMenuToolbar1" 
				PopupCommandMenuId="PopupPanel1" ContentName="" DataSourceID="GridView1" 
				AllowPaging="True" 
				AjaxConfiguration-UpdatePanelId="UpdatePanel1" 
				SettingsToolbarId="BXContextMenuToolbar1" DataKeyNames="ID,CommonElementType"
				AllowSorting="True"
				OnSelect="GridView1_Select" 
				OnPopupMenuClick="GridView1_PopupMenuClick" 
				OnSelectCount="GridView1_SelectCount" 
				OnDelete="GridView1_Delete"
				OnRowDataBound="GridView1_RowDataBound"
				OnUpdate = "GridView1_Update"
				OnRowUpdating="GridView1_RowUpdating"
				OnGetSettingsQueryString="GridView1_GetSettingsQueryString"
								
                ForeColor="#333333"
                BorderWidth="0px"
                BorderColor="white"
                BorderStyle="none"
                ShowHeader = "true"
                CssClass="list"
                style="font-size: small; font-family: Arial; border-collapse: separate;"   				
				>
				
                <pagersettings position="TopAndBottom" pagebuttoncount="3"/>
                <RowStyle BackColor="#FFFFFF"/>
                <AlternatingRowStyle BackColor="#FAFAFA" /> 
                <FooterStyle BackColor="#EAEDF7"/>				
				<AjaxConfiguration UpdatePanelId="UpdatePanel1" />
				<Columns>
					
					<asp:TemplateField HeaderText="<%$ Loc:FilterText.Title %>" SortExpression="Name">
						<itemtemplate>
							<%# GetElementName(Container.DataItem) %>
						</itemtemplate>
						<edititemtemplate>
							<asp:TextBox id="Name" runat="server" Text='<%# Bind("Name") %>' Width="100%" Enabled='<%# ((string)Eval("CommonElementType") == "E" && CanModifyElements) || ((string)Eval("CommonElementType") == "S" && CanModifySections) %>'></asp:TextBox>
							<asp:RequiredFieldValidator ID="NameValidator" runat="server" ControlToValidate="Name"
									ErrorMessage="<%$ Loc:Message.ElementNameIsRequired %>" ValidationGroup="grid" >*</asp:RequiredFieldValidator>
						</edititemtemplate>
					</asp:TemplateField>
				
					<asp:TemplateField HeaderText="<%$ Loc:ColumnHeaderText.Active %>" SortExpression="Active">
						<itemtemplate>
							<%# ((bool)Eval("Active")) ? GetMessageRaw("Kernel.Yes") : GetMessageRaw("Kernel.No") %>
						</itemtemplate>
						<edititemtemplate>
							<asp:CheckBox ID="Active" runat="server" Checked='<%# Bind("Active") %>' Enabled='<%# ((string)Eval("CommonElementType") == "E" && CanModifyElements) || ((string)Eval("CommonElementType") == "S" && CanModifySections) %>' />
						</edititemtemplate>
					</asp:TemplateField>
					
					<asp:TemplateField HeaderText="<%$ Loc:ColumnHeaderText.ActiveFormDate %>" SortExpression="ActiveFromDate" Visible="False" ItemStyle-Wrap="False">
						<itemtemplate>
							<%# Eval("ActiveFromDate")%>&nbsp;
						</itemtemplate>
						<edititemtemplate>
							<asp:TextBox ID="ActiveFromDate" Visible='<%# (string)Eval("CommonElementType") == "E" %>' runat="server" Text='<%# Bind("ActiveFromDate") %>' Enabled='<%# ((string)Eval("CommonElementType") == "E" && CanModifyElements) %>'></asp:TextBox>&nbsp;<bx:Calendar ID="ActiveFromDateCalendar" Visible='<%# (string)Eval("CommonElementType") == "E" %>' runat="server" TextBoxId="ActiveFromDate" />
						</edititemtemplate>
					</asp:TemplateField>
					
					<asp:TemplateField HeaderText="<%$ Loc:ColumnHeaderText.ActiveUntilDate %>" SortExpression="ActiveToDate" Visible="False" ItemStyle-Wrap="False">
						<itemtemplate>
							<%# Eval("ActiveToDate")%>&nbsp;
						</itemtemplate>
						<edititemtemplate>
							<asp:TextBox Visible='<%# (string)Eval("CommonElementType") == "E" %>' ID="ActiveToDate" runat="server" Text='<%# Bind("ActiveToDate") %>' Enabled='<%# ((string)Eval("CommonElementType") == "E" && CanModifyElements) %>'></asp:TextBox>&nbsp;<bx:Calendar ID="ActiveToDateCalendar" Visible='<%# (string)Eval("CommonElementType") == "E" %>' runat="server" TextBoxId="ActiveToDate" />
						</edititemtemplate>
					</asp:TemplateField>
					
					<asp:TemplateField HeaderText="<%$ Loc:ColumnHeaderText.AnnouncementDescription %>" Visible="False">
						<itemtemplate>
							<%# GetElementText(Container.DataItem, "PreviewText")%>
						</itemtemplate>
						<edititemtemplate>
							<asp:RadioButtonList ID="PreviewTextType" runat="server" RepeatDirection="Horizontal" SelectedValue='<%# Bind("PreviewTextType") %>' Visible='<%# (string)Eval("CommonElementType") == "E" %>' Enabled='<%# ((string)Eval("CommonElementType") == "E" && CanModifyElements)  %>'>
								<asp:ListItem Value="T" Text="<%$ Loc:Option.TextType %>" />
								<asp:ListItem Value="H" Text="HTML" />
							</asp:RadioButtonList>
							<asp:TextBox ID="PreviewText" runat="server" Columns="50" Rows="10" TextMode="MultiLine" Text='<%# Bind("PreviewText") %>' Visible='<%# (string)Eval("CommonElementType") == "E" %>' Enabled='<%# ((string)Eval("CommonElementType") == "E" && CanModifyElements)  %>'></asp:TextBox>&nbsp;
						</edititemtemplate>
					</asp:TemplateField>
					
					<asp:TemplateField HeaderText="<%$ Loc:ColumnHeaderText.DetailText %>" Visible="False">
						<itemtemplate>
							<%# GetElementText(Container.DataItem, "DetailText")%>
						</itemtemplate>
						<edititemtemplate>
							<asp:RadioButtonList ID="DetailTextType" runat="server" RepeatDirection="Horizontal" SelectedValue='<%# Bind("DetailTextType") %>' Enabled='<%# ((string)Eval("CommonElementType") == "E" && CanModifyElements) || ((string)Eval("CommonElementType") == "S" && CanModifySections) %>'>
								<asp:ListItem Value="T" Text="<%$ Loc:Option.TextType %>" />
								<asp:ListItem Value="H" Text="HTML" />
							</asp:RadioButtonList>
							<asp:TextBox ID="DetailText" runat="server" Columns="50" Rows="10" TextMode="MultiLine" Text='<%# Bind("DetailText") %>' Enabled='<%# ((string)Eval("CommonElementType") == "E" && CanModifyElements) || ((string)Eval("CommonElementType") == "S" && CanModifySections) %>'></asp:TextBox>
						</edititemtemplate>
					</asp:TemplateField>
					
					<asp:TemplateField HeaderText="<%$ Loc:ColumnHeaderText.Sort %>" SortExpression="Sort" Visible="False">
						<itemtemplate>
							<%# Eval("Sort")%>&nbsp;
						</itemtemplate>
						<edititemtemplate>
							<asp:TextBox id="Sort" size="5" runat="server" Text='<%# Bind("Sort") %>' Enabled='<%# ((string)Eval("CommonElementType") == "E" && CanModifyElements) || ((string)Eval("CommonElementType") == "S" && CanModifySections) %>'></asp:TextBox>
						</edititemtemplate>
					</asp:TemplateField>

					<asp:TemplateField HeaderText="<%$ Loc:FilterText.Code %>" SortExpression="Code" Visible="False">
						<itemtemplate>
							<%# Encode(Eval("Code").ToString())%>&nbsp;
						</itemtemplate>
						<edititemtemplate>
							<asp:TextBox id="Code" size="10" runat="server" Text='<%# Bind("Code") %>' Enabled='<%# ((string)Eval("CommonElementType") == "E" && CanModifyElements) || ((string)Eval("CommonElementType") == "S" && CanModifySections) %>'></asp:TextBox>
						</edititemtemplate>
					</asp:TemplateField>

					<asp:TemplateField HeaderText="<%$ Loc:ColumnHeaderText.ExternalKey %>" SortExpression="XmlId" Visible="False">
						<itemtemplate>
							<%# Encode(Eval("XmlId").ToString())%>&nbsp;
						</itemtemplate>
						<edititemtemplate>
							<asp:TextBox id="XmlId" size="10" runat="server" Text='<%# Bind("XmlId") %>' Enabled='<%# ((string)Eval("CommonElementType") == "E" && CanModifyElements) || ((string)Eval("CommonElementType") == "S" && CanModifySections) %>'></asp:TextBox>
						</edititemtemplate>
					</asp:TemplateField>					
					
					<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Tags %>" Visible="False">
						<itemtemplate>
							<%# Encode(Eval("Tags").ToString())%>&nbsp;
						</itemtemplate>
						<edititemtemplate>
							<asp:TextBox id="Tags" size="20" runat="server" Text='<%# Bind("Tags") %>' Enabled='<%# (string)Eval("CommonElementType") == "E" && CanModifyElements %>'></asp:TextBox>
						</edititemtemplate>
					</asp:TemplateField>	
					
					<asp:TemplateField HeaderText="<%$ Loc:ColumnHeaderText.DetailPicture %>" Visible="False">
						<itemtemplate>
							<bx:AdminImageField ID="DetailPictureView" Editable="False" ShowDescription="False" runat="server" MaxImageWidth="250" ImageId='<%# Eval("DetailPictureId") %>' />&nbsp;
						</itemtemplate>
						<edititemtemplate>
							<bx:AdminImageField ID="DetailPicture" ShowDescription="True" runat="server" MaxImageWidth="250" ImageId='<%# Bind("DetailPictureId") %>' Editable='<%# ((string)Eval("CommonElementType") == "E" && CanModifyElements) || ((string)Eval("CommonElementType") == "S" && CanModifySections) %>'/>&nbsp;
						</edititemtemplate>
					</asp:TemplateField>
					
					<asp:TemplateField HeaderText="<%$ Loc:ColumnHeaderText.PreviewPicture %>" Visible="False">
						<itemtemplate>
							<bx:AdminImageField ID="PreviewPictureView" Editable="False" ShowDescription="False" runat="server" MaxImageWidth="250" ImageId='<%# Eval("PreviewPictureId") %>' />&nbsp;
						</itemtemplate>
						<edititemtemplate>
							<bx:AdminImageField ID="PreviewPicture" ShowDescription="True" runat="server" MaxImageWidth="250" ImageId='<%# Bind("PreviewPictureId") %>' Editable='<%# ((string)Eval("CommonElementType") == "E" && CanModifyElements) || ((string)Eval("CommonElementType") == "S" && CanModifySections) %>'/>&nbsp;
						</edititemtemplate>
					</asp:TemplateField>
	
					<asp:BoundField DataField="ElementsCount" HeaderText="<%$ Loc:ColumnHeaderText.QuantityOfElements %>" Visible="False" ReadOnly="True" HtmlEncode="True" />
					<asp:BoundField DataField="SectionsCount" HeaderText="<%$ Loc:ColumnHeaderText.QuantityOfChapters %>" Visible="False" ReadOnly="True" HtmlEncode="True" />
					<asp:BoundField DataField="UpdateDate" HeaderText="<%$ Loc:ColumnHeaderText.DateOfModification %>" SortExpression="UpdateDate" ReadOnly="True" HtmlEncode="True"/>
					<asp:BoundField DataField="ModifiedBy" HeaderText="<%$ Loc:ColumnHeaderText.ModifiedBy %>" Visible="False" ReadOnly="True" HtmlEncode="False"/>
					<asp:BoundField DataField="CreateDate" HeaderText="<%$ Loc:ColumnHeaderText.CreationDate %>" Visible="False" ReadOnly="True" SortExpression="CreateDate" HtmlEncode="True"/>
					<asp:BoundField DataField="CreatedBy" HeaderText="<%$ Loc:ColumnHeaderText.CreatedBy %>" Visible="False" ReadOnly="True" HtmlEncode="False" />
					<asp:BoundField AccessibleHeaderText="ID" DataField="ID" HeaderText="ID" SortExpression="ID" ReadOnly="True" HtmlEncode="True" />
				</Columns>
			</bx:BXGridView>

			<bx:BXMultiActionMenuToolbar ID="MultiActionMenuToolbar1" runat="server" ValidationGroup="grid"  >
				<Items>
					<bx:BXMamImageButton  CommandName="inline" ShowConfirmBar="true" DisableForAll="true"
						EnabledCssClass="context-button icon edit" DisabledCssClass="context-button icon edit-dis"
						Title="<%$ Loc:ActionTitle.EditElements %>" />
						
					<bx:BXMamImageButton ID="BXMamImageButton1" runat="server" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>" EnabledCssClass="context-button icon delete"
						DisabledCssClass="context-button icon delete-dis" CommandName="delete"
					/>
				</Items>
			</bx:BXMultiActionMenuToolbar>
		</ContentTemplate>
		<Triggers>
			<asp:PostBackTrigger ControlID="MultiActionMenuToolbar1" />
		</Triggers>
		</asp:UpdatePanel>
</asp:Content>