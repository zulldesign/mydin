<%--<%@ Page Language="C#" MasterPageFile="AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="Scheduler.aspx.cs" Inherits="bitrix_admin_Scheduler" Title="<%$ Loc:PageTitle %>"
	StylesheetTheme="AdminTheme" %> zg--%>
<%--

--%>
<%@ Page Language="C#" MasterPageFile="AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="Scheduler.aspx.cs" Inherits="bitrix_admin_Scheduler" Title="<%$ Loc:PageTitle %>" %>

<%@ Import Namespace="Bitrix.Services" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<bx:BXAdminFilter ID="Filter" runat="server" AssociatedGridView="Grid">
		<bx:BXBetweenFilter Text="ID" Key="Id" ValueType="Integer" Visibility="AlwaysVisible" />
		<bx:BXTextBoxFilter Text="<%$ Loc:ColumnName %>" Key="Name" Operation="Like" />
		<bx:BXDropDownFilter Text="<%$ Loc:ColumnActive %>" Key="Active" ValueType="Boolean" >
			<asp:ListItem Value="" Text="<%$ LocRaw:Kernel.All %>" />
			<asp:ListItem Value="True" Text="<%$ LocRaw:Kernel.Yes %>" />
			<asp:ListItem Value="False" Text="<%$ LocRaw:Kernel.No %>" />
		</bx:BXDropDownFilter>
		<bx:BXTextBoxStringFilter Text="<%$ Loc:ColumnClassName %>" Key="ClassName" />
		<bx:BXTextBoxStringFilter Text="<%$ Loc:ColumnAssembly %>" Key="Assembly" />
		<bx:BXTimeIntervalFilter Text="<%$ Loc:ColumnStartTime %>" Key="StartTime" />
		<bx:BXDropDownFilter Text="<%$ Loc:ColumnPeriodic %>" Key="Periodic" ValueType="Boolean" >
			<asp:ListItem Value="" Text="<%$ LocRaw:Kernel.All %>" />
			<asp:ListItem Value="True" Text="<%$ LocRaw:Kernel.Yes %>" />
			<asp:ListItem Value="False" Text="<%$ LocRaw:Kernel.No %>" />
		</bx:BXDropDownFilter>
	</bx:BXAdminFilter>
	<bx:BXValidationSummary ID="errorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>"
		Visible="True" />
	<bx:BXMessage ID="successMessage" runat="server" Content="<%$ Loc:Message.OperationHasBeenCompletedSuccessfully %>"
		CssClass="ok" IconClass="ok" Title="<%$ Loc:Kernel.Information %>" Visible="False"
		Width="438px" />
	<bx:BXGridView ID="Grid" runat="server" 
	    AllowPaging="True" AllowSorting="True" AutoSelectField="False"
		ContentName="<%$ Loc:TableTitle %>" 
		PopupCommandMenuId="BXPopupPanel1" DataSourceID="Grid" DataKeyNames="Id"
		OnPopupMenuClick="Grid_PopupMenuClick" OnSelect="Grid_Select" OnSelectCount="Grid_SelectCount" OnDelete="Grid_Delete"
        ForeColor="#333333"
        BorderWidth="0px"
        BorderColor="white"
        BorderStyle="none"
        ShowHeader = "true"
        CssClass="list"
        style="font-size: small; font-family: Arial; border-collapse: separate;"  			
		>

		<Columns>
			<asp:BoundField DataField="Id" HeaderText="ID" ReadOnly="True" SortExpression="Id" />
			<asp:BoundField DataField="Name" HeaderText="<%$ Loc:ColumnName %>" ReadOnly="True" SortExpression="Name" />
			<asp:TemplateField HeaderText="<%$ Loc:ColumnActive %>" SortExpression="Active">
				<itemtemplate><%# (bool)Eval("Active") ? GetMessage("Kernel.Yes") : GetMessage("Kernel.No") %></itemtemplate>
			</asp:TemplateField>
			<asp:BoundField DataField="ClassName" HeaderText="<%$ Loc:ColumnClassName %>" ReadOnly="True"
				SortExpression="ClassName" />
			<asp:BoundField DataField="Assembly" HeaderText="<%$ Loc:ColumnAssembly %>"
				ReadOnly="True" SortExpression="Assembly" />
			<asp:TemplateField HeaderText="<%$ Loc:ColumnParameters %>" >
				<itemtemplate>
				<%# 
					string.Join(
						"<br/>",
	              		((BXParamsBag<object>)Eval("Parameters")).ToList<string>(delegate(string key, object value)
						{
                            return string.Format("<i>{0}:</i> {1}", Encode(key), value != null ? Encode(value.ToString()) : "<i>Null</i>");
						}).ToArray()
					)
				%>
				</itemtemplate>
			</asp:TemplateField>
			<asp:BoundField DataField="StartTime" HeaderText="<%$ Loc:ColumnStartTime %>" ReadOnly="True"
				SortExpression="StartTime" />
			<asp:TemplateField HeaderText="<%$ Loc:ColumnPeriodic %>" SortExpression="Periodic">
				<itemtemplate><%# (bool)Eval("Periodic") ? GetMessage("Kernel.Yes") : GetMessage("Kernel.No") %></itemtemplate>
			</asp:TemplateField>
			<asp:BoundField DataField="Period" HeaderText="<%$ Loc:ColumnPeriod %>" ReadOnly="True"
				SortExpression="Period" />
		</Columns>
	</bx:BXGridView>
	<bx:BXPopupPanel ID="BXPopupPanel1" runat="server">
		<Commands>
			<bx:CommandItem IconClass="settings" Default="true" ItemText="<%$ Loc:PopupStartStopText %>"
				ItemTitle="<%$ Loc:PopupStartStopTitle %>" OnClickScript="" UserCommandId="active" />
			<bx:CommandItem IconClass="delete" ItemText="<%$ Loc:PopupDeleteText %>" ItemTitle="<%$ Loc:PopupDeleteTitle %>"
				OnClickScript="" UserCommandId="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
		</Commands>
	</bx:BXPopupPanel>



</asp:Content>