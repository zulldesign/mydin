<%--<%@ Page Language="C#" MasterPageFile="AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="MailerTypes.aspx.cs" Inherits="bitrix_admin_MailerTypes" Title="<%$ Loc:EventTypes.PageTitle %>"
	StylesheetTheme="AdminTheme" %> zg --%>
	
	<%@ Page Language="C#" MasterPageFile="AdminMasterPage.master" AutoEventWireup="true"
	CodeFile="MailerTypes.aspx.cs" Inherits="bitrix_admin_MailerTypes" Title="<%$ Loc:EventTypes.PageTitle %>" %>

<%@ Import Namespace="Bitrix.Services" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<bx:BXGridView ID="mailEventTypes" runat="server" AllowPaging="True" AllowSorting="True"
		 AutoSelectField="False" ContentName="<%$ Loc:EventTypes.TableTitle %>"
		DataSourceID="mailEventTypesData" AutoCommandField="False" DataKeyNames="name"
        ForeColor="#333333"
        BorderWidth="0px"
        BorderColor="white"
        BorderStyle="none"
        ShowHeader = "true"
        CssClass="list"
        style="font-size: small; font-family: Arial; border-collapse: separate;"			
		>
		<Columns>
			<asp:BoundField DataField="num" HeaderText="#" ReadOnly="True" />
			<asp:BoundField DataField="id" HeaderText="<%$ Loc:EventTypes.ColumnId %>" ReadOnly="True" SortExpression="id" />
			<asp:BoundField DataField="name" HeaderText="<%$ Loc:EventTypes.ColumnName %>" ReadOnly="True" SortExpression="name" />
		</Columns>
	</bx:BXGridView>
	<asp:ObjectDataSource ID="mailEventTypesData" runat="server" EnablePaging="True" OldValuesParameterFormatString="original_{0}"
		SelectCountMethod="SelectCount" SelectMethod="Select" SortParameterName="sortExpression"
		TypeName="Bitrix.Services.BXMailEventTypesDataObject"></asp:ObjectDataSource>
</asp:Content>


