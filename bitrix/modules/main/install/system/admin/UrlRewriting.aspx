<%--<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="UrlRewriting.aspx.cs" Inherits="bitrix_admin_UrlRewriting" Title="<%$ Loc:PageTitle %>" StylesheetTheme="AdminTheme" Theme="AdminTheme" %>
zg --%>

<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="UrlRewriting.aspx.cs" Inherits="bitrix_admin_UrlRewriting" Title="<%$ Loc:PageTitle %>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<asp:UpdatePanel ID="UpdatePanel1" runat="server">
		<ContentTemplate>
			<bx:BXAdminFilter ID="BXAdminFilter1" runat="server" AssociatedGridView="BXGridView1">
				<bx:BXDropDownFilter ID="siteIdFilter" Key="SiteId" Text="<%$ LocRaw:Column.SiteId %>"
					Visibility="AlwaysVisible">
					<asp:ListItem Text="<%$ LocRaw:SelectSite %>" Value="" />
				</bx:BXDropDownFilter>
				<bx:BXTextBoxStringFilter Key="MatchExpression" Text="<%$ LocRaw:Column.MatchExpression %>" />
				<bx:BXTextBoxStringFilter Key="ReplaceExpression" Text="<%$ LocRaw:Column.ReplaceExpression %>" />
				<bx:BXTextBoxStringFilter Key="Path" Text="<%$ LocRaw:Column.Path %>" />
				<bx:BXTextBoxStringFilter Key="ComponentName" Text="<%$ LocRaw:Column.ComponentName %>" />
				<bx:BXTextBoxStringFilter Key="HelperId" Text="<%$ LocRaw:Column.HelperId %>" />
			</bx:BXAdminFilter>
			<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>"
				Visible="True" />
			<bx:BXMessage ID="successMessage" runat="server" Content="<%$ Loc:Message.OperationHasBeenCompletedSuccessfully %>"
				CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False"
				Width="438px" />
			<bx:BXContextMenuToolbar ID="BXContextMenuToolbar1" runat="server" OnCommandClick="BXContextMenuToolbar1_CommandClick">
				<Items>
					<bx:BXCmSeparator SectionSeparator="true" />
					<bx:BXCmImageButton ID="AddButton"
						CommandName="add" Text="<%$ Loc:ActionText.Add %>" Title="<%$ Loc:ActionTitle.Add %>"
						CssClass="context-button icon btn_new" Href="UrlRewritingEdit.aspx" />
					<bx:BXCmSeparator ID="AddSeparator" />
					<bx:BXCmImageButton CommandName="reload" Text="<%$ Loc:ActionText.Reload %>" Title="<%$ Loc:ActionTitle.Reload %>" />
					<bx:BXCmSeparator ID="RefreshSefSeparator" />
					<bx:BXCmImageButton ID="RefreshSefButton"
						CommandName="refreshsef" Text="<%$ LocRaw:ActionText.RefreshSef %>" Title="<%$ LocRaw:ActionTitle.RefreshSef %>" />
				</Items>
			</bx:BXContextMenuToolbar>
			<br />
			<bx:BXGridView ID="BXGridView1" runat="server" AllowSorting="True" PopupCommandMenuId="BXPopupPanel1"
				ContentName="<%$ LocRaw:TableTitle %>" SettingsToolbarId="BXContextMenuToolbar1"
				DataSourceID="BXGridView1" OnSelect="BXGridView1_Select" OnSelectCount="BXGridView1_SelectCount"
				AllowPaging="True" ContextMenuToolbarId="BXMultiActionMenuToolbar1" DataKeyNames="Id"
				OnDelete="BXGridView1_Delete" OnUpdate="BXGridView1_Update" OnRowDataBound="BXGridView1_RowDataBound"
				OnRowUpdating="BXGridView1_RowUpdating" ForeColor="#333333" BorderWidth="0px"
				BorderColor="white" BorderStyle="none" ShowHeader="true" CssClass="list" Style="font-size: small;
				font-family: Arial; border-collapse: separate;">
				<Columns>
					<asp:TemplateField HeaderText="<%$ LocRaw:Column.MatchExpression %>" SortExpression="MatchExpression">
						<edititemtemplate>
							<asp:TextBox id="matchExpressionEdit" runat="server" Text='<%# Bind("MatchExpression") %>' />
						</edititemtemplate>
						<itemtemplate>
							<%--<img alt="<%# (bool)Eval("Ignore") ? "ignore" : "use" %>" src="<%# VirtualPathUtility.ToAbsolute("~/bitrix/admin/img/" + ((bool)Eval("Ignore") ? "minus.gif" : "plus.gif")) %>" style="vertical-align: middle" />
							&nbsp;&nbsp;--%>
							<asp:Label id="matchExpression" runat="server" Text='<%# Encode((string)Eval("MatchExpression")) %>' ForeColor='<%# (bool)Eval("Ignore") ? System.Drawing.Color.FromArgb(255, 200, 0, 0) : System.Drawing.Color.Empty %>' ></asp:Label> 
						</itemtemplate>
					</asp:TemplateField>
					<asp:BoundField DataField="ReplaceExpression" HeaderText="<%$ LocRaw:Column.ReplaceExpression %>"
						SortExpression="ReplaceExpression" />
					<asp:BoundField DataField="ComponentName" HeaderText="<%$ LocRaw:Column.ComponentName %>"
						SortExpression="ComponentName" ReadOnly="True" />
					<asp:BoundField DataField="Path" HeaderText="<%$ LocRaw:Column.Path %>" SortExpression="Path"
						ReadOnly="True" />
					<asp:BoundField DataField="HelperId" HeaderText="<%$ LocRaw:Column.HelperId %>" SortExpression="HelperId" />
					<asp:TemplateField HeaderText="<%$ LocRaw:Column.SiteId %>" SortExpression="SiteId">
						<edititemtemplate>
			<asp:DropDownList id="siteIdEdit" runat="server" DataValueField="ID" DataTextField="Name" DataSource="<%# Bitrix.BXSite.GetList(null, null) %>" ></asp:DropDownList> 
		</edititemtemplate>
						<itemtemplate>
			<asp:Literal id="siteId" runat="server" Text='<%# (Bitrix.BXSite.GetById((string)Eval("SiteId")) ?? new Bitrix.BXSite()).Name %>' Mode="Encode" ></asp:Literal> 
		</itemtemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Sort %>" SortExpression="Sort">
						<edititemtemplate>
							<asp:TextBox id="sortEdit" runat="server" Text='<%# Bind("Sort") %>' />
							<asp:CompareValidator runat="server" ID="sortEditValidator" ControlToValidate="sortEdit" Operator="DataTypeCheck" Type="Integer" ErrorMessage="<%$ LocRaw:Message.SortMustBeInteger %>" ValidationGroup="GridView" >*</asp:CompareValidator>
						</edititemtemplate>
						<itemtemplate>
							<asp:Literal id="sort" runat="server" Text='<%# Eval("Sort") %>' Mode="Encode" ></asp:Literal> 
						</itemtemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.ExcludeRule %>" SortExpression="Ignore">
						<edititemtemplate>
							<asp:CheckBox id="ignoreEdit" runat="server" Checked='<%# Bind("Ignore") %>' />
						</edititemtemplate>
						<itemtemplate>
							<asp:Literal id="ignore" runat="server" Text='<%# (bool)Eval("Ignore") ? GetMessage("Kernel.Yes") : GetMessage("Kernel.No") %>' ></asp:Literal> 
						</itemtemplate>
					</asp:TemplateField>
				</Columns>
			</bx:BXGridView>
			
			<bx:BXMultiActionMenuToolbar ID="BXMultiActionMenuToolbar1" runat="server" ValidationGroup="GridView">
				<Items>
					<bx:BXMamImageButton CommandName="inline" ShowConfirmBar="true" DisableForAll="true"
						EnabledCssClass="context-button icon edit" DisabledCssClass="context-button icon edit-dis"
						Title="<%$ LocRaw:ActionTitle.Inline %>" />
					<bx:BXMamImageButton CommandName="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>" EnabledCssClass="context-button icon delete"
						DisabledCssClass="context-button icon delete-dis" Title="<%$ LocRaw:ActionTitle.Delete %>" />
				</Items>
			</bx:BXMultiActionMenuToolbar>
			<bx:BXPopupPanel ID="BXPopupPanel1" runat="server">
				<Commands>
					<bx:CommandItem Default="True" IconClass="view" UserCommandId="edit" ItemText="<%$ Loc:PopupText.Edit %>"
						ItemTitle="<%$ Loc:PopupTitle.Edit %>" OnClickScript="window.location.href = 'UrlRewritingEdit.aspx?edit=&id=' + UserData['id']; return false;" />
					<bx:CommandItem Default="True" IconClass="delete" UserCommandId="delete" ItemText="<%$ Loc:Kernel.Delete %>"
						ItemTitle="<%$ Loc:PopupTitle.DeleteSelectedRule %>" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
				</Commands>
			</bx:BXPopupPanel>
			<bx:BXPopupPanel ID="PopupPanelView" runat="server">
				<Commands>
					<bx:CommandItem Default="True" IconClass="view" ItemText="<%$ Loc:Kernel.View %>" ItemTitle="<%$ LocRaw:PopupTitle.View %>"
						UserCommandId="edit" OnClickScript="window.location.href = 'UrlRewritingEdit.aspx?edit=&id=' + UserData['id']; return false;" />
				</Commands>
			</bx:BXPopupPanel>
		</ContentTemplate>
	</asp:UpdatePanel>
</asp:Content>
