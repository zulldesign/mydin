<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminSimpleMasterPage.master"
	AutoEventWireup="true" CodeFile="IBlockElementSearch.aspx.cs" Inherits="bitrix_admin_IBlockElementSearch"
	Title="Untitled Page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<asp:UpdatePanel runat="server" ID="UP" >
		<ContentTemplate>
			<bx:BXAdminFilter ID="BXAdminFilter1" runat="server" AssociatedGridView="GridView1">
				<bx:BXTextBoxStringFilter ID="BXTextBoxFilter1" runat="server" Key="Name" Text="<%$ Loc:FilterText.Title %>"
					Visibility="AlwaysVisible" />
				<bx:BXDropDownFilter ID="filterSectionId" runat="server" Key="Section.Id" Text="<%$ Loc:FilterText.Chapter %>" />
				<bx:BXBetweenFilter ID="filterId" runat="server" Key="ID" Text="<%$ Loc:FilterText.ID %>"
					ValueType="Integer" />
				<bx:BXTimeIntervalFilter ID="BXTimeIntervalFilter1" runat="server" Key="UpdateDate"
					Text="<%$ Loc:FilterText.DateOfModification %>" />
				<bx:BXTextBoxStringFilter ID="BXTextBoxStringFilter1" runat="server" Key="Code" Text="<%$ Loc:FilterText.Code %>" />
				<bx:BXTextBoxStringFilter ID="BXTextBoxStringFilter2" runat="server" Key="XmlId"
					Text="<%$ Loc:FilterText.ExternalCode %>" />
				<bx:BXTextBoxAndDropDownFilter ID="filterModifiedBy" runat="server" Key="ModifiedBy"
					Text="<%$ Loc:FilterText.ModifiedBy %>" OnCustomBuildFilter="filterUser_CustomBuildFilter">
				</bx:BXTextBoxAndDropDownFilter>
				<bx:BXTimeIntervalFilter ID="BXTimeIntervalFilter2" runat="server" Key="CreateDate"
					Text="<%$ Loc:FilterText.CreationDate %>" />
				<bx:BXTextBoxAndDropDownFilter ID="filterCreatedBy" runat="server" Key="CreatedBy"
					Text="<%$ Loc:FilterText.CreatedBy %>" OnCustomBuildFilter="filterUser_CustomBuildFilter">
				</bx:BXTextBoxAndDropDownFilter>
				<bx:BXDropDownFilter ID="BXDropDownFilter1" runat="server" Key="Active" Text="<%$ Loc:FilterText.IsActive %>">
					<asp:ListItem Value="" Text="<%$ Loc:Any %>">
					</asp:ListItem>
					<asp:ListItem Value="Y" Text="<%$ Loc:Kernel.Yes %>">
					</asp:ListItem>
					<asp:ListItem Value="N" Text="<%$ Loc:Kernel.No %>">
					</asp:ListItem>
				</bx:BXDropDownFilter>
				<bx:BXTextBoxStringFilter ID="BXTextBoxStringFilter3" runat="server" Key="PreviewText"
					Text="<%$ Loc:FilterText.Description %>" />
			</bx:BXAdminFilter>
			
			<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" runat="server" />
			<br />
			<asp:HiddenField ID="hfTypeId" runat="server" />
			<asp:HiddenField ID="hfIBlockId" runat="server" />
			<asp:HiddenField ID="hfFieldId" runat="server" />
			<asp:HiddenField ID="hfFieldName" runat="server" />
			<asp:HiddenField ID="hfCallback" runat="server" />
			<bx:BXGridView ID="GridView1" runat="server" ContextMenuToolbarId="MultiActionMenuToolbar1"
				PopupCommandMenuId="PopupPanel1" ContentName="" DataSourceID="GridView1" AllowPaging="True"
				AjaxConfiguration-UpdatePanelId="UpdatePanel1" SettingsToolbarId="BXContextMenuToolbar1"
				DataKeyNames="ID" OnSelect="GridView1_Select" AllowSorting="True" OnSelectCount="GridView1_SelectCount"
				OnRowDataBound="GridView1_RowDataBound" AutoSelectField="False">
				<AjaxConfiguration UpdatePanelId="UP" />
				<Columns>
					<asp:BoundField DataField="Name" SortExpression="Name" HeaderText="<%$ Loc:FilterText.Title %>"
						HtmlEncode="False" />
					<asp:BoundField DataField="Sort" HeaderText="<%$ Loc:ColumnHeaderText.Sort %>" SortExpression="Sort" />
					<asp:BoundField DataField="Active" HeaderText="<%$ Loc:ColumnHeaderText.IsActive %>"
						SortExpression="Active" />
					<asp:BoundField DataField="Code" HeaderText="<%$ Loc:FilterText.Code %>" Visible="False"
						SortExpression="Code" />
					<asp:BoundField DataField="XmlId" HeaderText="<%$ Loc:ColumnHeaderText.ExternalKey %>"
						Visible="False" SortExpression="XmlId" />
					<asp:BoundField DataField="UpdateDate" HeaderText="<%$ Loc:ColumnHeaderText.DateOfModification %>"
						SortExpression="UpdateDate" />
					<asp:BoundField DataField="ActiveFromDate" HeaderText="<%$ Loc:ColumnHeaderText.ActiveFromDate %>"
						Visible="False" SortExpression="ActiveFromDate" />
					<asp:BoundField DataField="ActiveToDate" HeaderText="<%$ Loc:ColumnHeaderText.ActiveUntilDate %>"
						Visible="False" SortExpression="ActiveToDate" />
					<asp:BoundField DataField="ModifiedBy" HeaderText="<%$ Loc:ColumnHeaderText.ModifiedBy %>"
						Visible="False" />
					<asp:BoundField DataField="CreateDate" HeaderText="<%$ Loc:ColumnHeaderText.CreationData %>"
						Visible="False" SortExpression="CreateDate" />
					<asp:BoundField DataField="CreatedBy" HeaderText="<%$ Loc:ColumnHeaderText.CreatedBy %>"
						Visible="False" />
					<asp:BoundField DataField="PreviewText" HeaderText="<%$ Loc:ColumnHeaderText.AnonceDescription %>"
						Visible="False" />
					<asp:BoundField AccessibleHeaderText="ID" DataField="ID" HeaderText="ID" SortExpression="ID" />
				</Columns>
			</bx:BXGridView>
			<bx:BXMultiActionMenuToolbar ID="MultiActionMenuToolbar1" runat="server" Visible="false" />
			<bx:BXPopupPanel ID="PopupPanel1" runat="server">
				<Commands>
					<bx:CommandItem Default="True" IconClass="select" ItemText="<%$ Loc:PopupText.SelectElement %>"
						ItemTitle="<%$ Loc:PopupTitle.SelectElement %>" UserCommandId="edit" OnClickScript="SelEl(UserData); return false;" />
				</Commands>
			</bx:BXPopupPanel>
		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>
