<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
    CodeFile="CustomField.aspx.cs" Inherits="bitrix_CustomField" Title="<%$ Loc:Title %>" %>    

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<asp:UpdatePanel ID="UpdatePanel1" runat="server">
		<ContentTemplate>
			<bx:BXAdminFilter ID="BXAdminFilter1" runat="server" AssociatedGridView="BXGridView1">
				<bx:BXDropDownKeyTextBoxFilter ID="BXDropDownKeyTextBoxFilter1" runat="server" Text="<%$ Loc:Kernel.Find %>" Visibility="AlwaysVisible" OnCustomBuildFilter="BXDropDownKeyTextBoxFilter1_CustomBuildFilter" >
					<asp:ListItem Value="Id" Text="ID" />
					<asp:ListItem Value="OwnerEntityId" Text="<%$ Loc:ColumnEntity %>" />
				</bx:BXDropDownKeyTextBoxFilter>
				<bx:BXBetweenFilter ID="BXTextBoxFilter1" runat="server" Key="ID" Text="<%$ Loc:ColumnId %>" ValueType="Integer" Visibility="StartVisible" />
				<bx:BXTextBoxFilter ID="BXTextBoxFilter2" runat="server" Key="OwnerEntityId" Text="<%$ Loc:ColumnEntity %>"  Visibility="StartVisible" />
				<bx:BXTextBoxStringFilter ID="BXTextBoxStringFilter1" runat="server" Key="Name" Text="<%$ Loc:ColumnFieldName %>" />
				<bx:BXDropDownFilter ID="CustomTypeFilter" runat="server" Key="CustomTypeId" Text="<%$ Loc:ColumnType %>">
					<asp:ListItem Value="" Text="<%$ Loc:OptionText.All %>" />
				</bx:BXDropDownFilter>
				<bx:BXTextBoxFilter ID="BXTextBoxFilter3" runat="server" Key="XmlId" Text="XML_ID" />
				<bx:BXDropDownFilter ID="BXDropDownFilter1" runat="server" Key="Multiple" Text="<%$ Loc:FilterText.Multiple %>" ValueType="Boolean">
					<asp:ListItem Value="" Text="<%$ Loc:OptionText.All %>" />
					<asp:ListItem Value="true" Text="<%$ Loc:Kernel.Yes %>" />
					<asp:ListItem Value="false" Text="<%$ Loc:Kernel.No %>" />
				</bx:BXDropDownFilter>
				<bx:BXDropDownFilter ID="BXDropDownFilter2" runat="server" Key="Mandatory" Text="<%$ Loc:FilterText.Required %>" ValueType="Boolean">
					<asp:ListItem Value="" Text="<%$ Loc:OptionText.All %>" />
					<asp:ListItem Value="true" Text="<%$ Loc:Kernel.Yes %>" />
					<asp:ListItem Value="false" Text="<%$ Loc:Kernel.No %>" />
				</bx:BXDropDownFilter>
				<bx:BXDropDownFilter ID="ShowInFilter" runat="server" Key="ShowInFilter" Text="<%$ Loc:FilterText.Filter %>">
					<asp:ListItem Value="" Text="<%$ Loc:OptionText.Any %>" />
				</bx:BXDropDownFilter>
				<bx:BXDropDownFilter ID="BXDropDownFilter3" runat="server" Key="ShowInList" Text="<%$ Loc:FilterText.DisplayInList %>" ValueType="Boolean">
					<asp:ListItem Value="" Text="<%$ Loc:OptionText.All %>" />
					<asp:ListItem Value="true" Text="<%$ Loc:Kernel.Yes %>" />
					<asp:ListItem Value="false" Text="<%$ Loc:Kernel.No %>" />
				</bx:BXDropDownFilter>
				<bx:BXDropDownFilter ID="BXDropDownFilter4" runat="server" Key="EditInList" Text="<%$ Loc:FilterText.AllowModification %>"
					ValueType="Boolean">
					<asp:ListItem Value="" Text="<%$ Loc:OptionText.All %>" />
					<asp:ListItem Value="true" Text="<%$ Loc:Kernel.Yes %>" />
					<asp:ListItem Value="false" Text="<%$ Loc:Kernel.No %>" />
				</bx:BXDropDownFilter>
				<bx:BXDropDownFilter ID="BXDropDownFilter5" runat="server" Key="IsSearchable" Text="<%$ Loc:FilterText.TakePartInSearch %>"
					ValueType="Boolean">
					<asp:ListItem Value="" Text="<%$ Loc:OptionText.All %>" />
					<asp:ListItem Value="true" Text="<%$ Loc:Kernel.Yes %>" />
					<asp:ListItem Value="false" Text="<%$ Loc:Kernel.No %>" />
				</bx:BXDropDownFilter>
			</bx:BXAdminFilter>
			
			<% AddButton.Href = String.Format("CustomFieldEdit.aspx?{0}={1}", Bitrix.Configuration.BXConfigurationUtility.Constants.BackUrl, Request.RawUrl); %>
			<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" runat="server" OnCommandClick="BXContextMenuToolbar1_CommandClick">
				<Items>
					<bx:BXCmImageButton ID="AddButton" runat="server" CssClass="context-button icon btn_new" CommandName="add"
						Text="<%$ Loc:Kernel.Add %>" Title="<%$ Loc:Kernel.Add %>" />
				</Items>
			</bx:BXContextMenuToolbar>
			<br />
			<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" />
			<bx:BXGridView ID="BXGridView1" DataSourceID="BXGridView1" runat="server" OnDelete="BXGridView1_Delete"
				OnSelect="BXGridView1_Select" OnSelectCount="BXGridView1_SelectCount"
				AllowPaging="True" AllowSorting="True" Width="100%" ContextMenuToolbarId="BXMultiActionMenuToolbar1"
				SettingsToolbarId="BXContextMenuToolbar1" DataKeyNames="Id" PopupCommandMenuId="BXPopupPanel1"
				OnPopupMenuClick="BXGridView1_PopupMenuClick1"
                ForeColor="#333333"
                BorderWidth="0px"
                BorderColor="white"
                BorderStyle="none"
                ShowHeader = "true"
                CssClass="list"
                style="font-size: small; font-family: Arial; border-collapse: separate;"      				
				>
				<Columns>
					<asp:BoundField HeaderText="<%$ Loc:ColumnId %>" SortExpression="Id" DataField="Id"
						ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle" />
					<asp:TemplateField HeaderText="<%$ Loc:ColumnEntity %>" SortExpression="OwnerEntityId"
						ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle" >
						<ItemTemplate><%# Encode(Eval("OwnerEntityId").ToString().ToUpper()) %></ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="<%$ Loc:ColumnFieldName %>" SortExpression="Name"
						ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle" >
						<itemtemplate><%# Encode(Eval("Name").ToString().ToUpper())%></itemtemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="<%$ Loc:ColumnType %>" 
						ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle" >
						<itemtemplate><%# Encode(GetTypeName((string)Eval("CustomTypeId")))%></itemtemplate>
					</asp:TemplateField>
					<asp:BoundField HeaderText="<%$ Loc:ColumnSort %>" SortExpression="Sort" DataField="Sort"
						ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle" />
					<asp:BoundField Visible="False" HeaderText="XML_ID" SortExpression="XmlId" DataField="XmlId"
						ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle" />
					<asp:TemplateField Visible="False" SortExpression="Multiple" HeaderText="<%$ Loc:FilterText.Multiple %>"
						ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle" >
						<itemtemplate><%# (bool)Eval("Multiple") ? GetMessage("Kernel.Yes") : GetMessage("Kernel.No")%></itemtemplate>	
					</asp:TemplateField>
					<asp:TemplateField Visible="False" SortExpression="Mandatory" HeaderText="<%$ Loc:FilterText.Required %>"
						ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle" >
						<itemtemplate><%# (bool)Eval("Mandatory") ? GetMessage("Kernel.Yes") : GetMessage("Kernel.No")%></itemtemplate>	
					</asp:TemplateField>
					<asp:TemplateField Visible="False" SortExpression="ShowInFilter" HeaderText="<%$ Loc:FilterText.Filter %>"
						ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle" >
						<itemtemplate><%# Encode(GetFilterVisibility((Bitrix.BXCustomFieldFilterVisibility)Eval("ShowInFilter"))) %></itemtemplate>	
					</asp:TemplateField>
					<asp:TemplateField Visible="False" SortExpression="ShowInList" HeaderText="<%$ Loc:FilterText.DisplayInList %>"
						ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle" >
						<itemtemplate><%# (bool)Eval("ShowInList") ? GetMessage("Kernel.Yes") : GetMessage("Kernel.No")%></itemtemplate>	
					</asp:TemplateField>
					<asp:TemplateField Visible="False" SortExpression="EditInList" HeaderText="<%$ Loc:FilterText.AllowModification %>"
						ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle" >
						<itemtemplate><%# (bool)Eval("EditInList") ? GetMessage("Kernel.Yes") : GetMessage("Kernel.No")%></itemtemplate>	
					</asp:TemplateField>
					<asp:TemplateField Visible="False" SortExpression="IsSearchable" HeaderText="<%$ Loc:FilterText.TakePartInSearch %>"
						ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle" >
						<itemtemplate><%# (bool)Eval("IsSearchable") ? GetMessage("Kernel.Yes") : GetMessage("Kernel.No")%></itemtemplate>	
					</asp:TemplateField>
				</Columns>
			</bx:BXGridView>
			<br />
			<bx:BXMultiActionMenuToolbar ID="BXMultiActionMenuToolbar1" runat="server">
				<Items>
					<bx:BXMamImageButton ID="BXMamImageButton1" runat="server" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>" EnabledCssClass="context-button icon delete"
						DisabledCssClass="context-button icon delete-dis" CommandName="delete" Title="<%$ Loc:Kernel.Delete %>" />
				</Items>
			</bx:BXMultiActionMenuToolbar>
			<bx:BXPopupPanel ID="BXPopupPanel1" runat="server">
				<Commands>
					<bx:CommandItem Default="True" IconClass="edit" UserCommandId="edit" ItemText="<%$ Loc:Kernel.Edit %>"
						ItemTitle="<%$ Loc:Kernel.Edit %>" />
					<bx:CommandItem IconClass="delete" UserCommandId="delete" ItemText="<%$ Loc:Kernel.Delete %>"
						ItemTitle="<%$ Loc:Kernel.Delete %>" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
				</Commands>
			</bx:BXPopupPanel>
			<bx:BXPopupPanel ID="PopupPanelView" runat="server">
				<Commands>
					<bx:CommandItem Default="True" IconClass="view" ItemText="<%$ Loc:Kernel.View %>" ItemTitle="<%$ LocRaw:PopupTitle.View %>"
						UserCommandId="edit" />
				</Commands>
			</bx:BXPopupPanel>
		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>


