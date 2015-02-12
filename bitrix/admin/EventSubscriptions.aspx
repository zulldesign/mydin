<%--<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="EventSubscriptions.aspx.cs" Inherits="bitrix_admin_EventSubscriptions" Title='<%$ Loc:PageTitle %>' StylesheetTheme="AdminTheme" Theme="AdminTheme" %>--%>
<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="EventSubscriptions.aspx.cs" Inherits="bitrix_admin_EventSubscriptions" Title='<%$ Loc:PageTitle %>' %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<%--	<bx:BXGridView ID="BXGridView1" runat="server" AllowPaging="True" AllowSorting="True"
		AutoCommandField="False"  AutoSelectField="False"
		ContentName="<%$ Loc:TableTitle %>" DataSourceID="EventSubscriptionsData" SkinID="BXGridViewSkin">
        zg, 25.04.2008
--%>
		
	<bx:BXGridView ID="BXGridView1" runat="server" AllowPaging="True" AllowSorting="True"
		AutoCommandField="False"  AutoSelectField="False"
		ContentName="<%$ Loc:TableTitle %>" DataSourceID="EventSubscriptionsData" 
        ForeColor="#333333" BorderWidth="0px" BorderColor="white" BorderStyle="none"
        ShowHeader = "true" CssClass="list" style="font-size: small; font-family: Arial; border-collapse: separate;">	
        <pagersettings position="TopAndBottom" pagebuttoncount="3"/>
        <RowStyle BackColor="#FFFFFF"/>
        <AlternatingRowStyle BackColor="#FAFAFA" /> 
        <FooterStyle BackColor="#EAEDF7"/>        	
		<Columns>
			<asp:BoundField DataField="num" HeaderText="#" ReadOnly="True" />
			<asp:BoundField DataField="request" HeaderText="<%$ Loc:ColumnTemplate %>" ReadOnly="True" SortExpression="request" />
			<asp:TemplateField HeaderText="<%$ Loc:ColumnListener %>" SortExpression="classname" >
				<ItemTemplate>
					<%# Encode((string)Eval("classname")) + (!Convert.IsDBNull(Eval("note")) && !string.IsNullOrEmpty((string)Eval("note")) ? " (" + Encode((string)Eval("note")) + ")" : "")%>
				</ItemTemplate>
			</asp:TemplateField>
			<asp:BoundField DataField="assemblyname" HeaderText="<%$ Loc:ColumnAssembly %>" ReadOnly="True" SortExpression="assemblyname" />
		</Columns>
	</bx:BXGridView>
	<asp:ObjectDataSource ID="EventSubscriptionsData" runat="server" EnablePaging="True"
		EnableViewState="False" OldValuesParameterFormatString="original_{0}" SelectCountMethod="SelectCount"
		SelectMethod="Select" SortParameterName="sortExpression" TypeName="Bitrix.Modules.BXModuleEventDataObject">
	</asp:ObjectDataSource>
</asp:Content>

