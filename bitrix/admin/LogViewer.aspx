<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true"
    CodeFile="LogViewer.aspx.cs" Inherits="bitrix_admin_LogViewer" Title="<%$ Loc:PageTitle.EventLog %>" %>    

<%@ Import Namespace="Bitrix.Services" %>
<%@ Register Assembly="Main" Namespace="Bitrix.UI" TagPrefix="bx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <bx:BXAdminFilter ID="Filter" runat="server" AssociatedGridView="Grid">
        <bx:BXDropDownFilter ID="type" runat="server" Key="type" Text="<%$ LocRaw:Column.Type %>" Visibility="AlwaysVisible">
			<asp:ListItem Text="<%$ LocRaw:Kernel.Any %>" Value="" />
        </bx:BXDropDownFilter>        
        <bx:BXTextBoxFilter ID="code" runat="server" Text="<%$ LocRaw:Column.Code %>" Key="code" ValueType="Integer"  />
        <bx:BXBetweenFilter ID="id" runat="server" Text="ID" Key="id" ValueType="Integer" />
        <bx:BXTextBoxStringFilter ID="source" runat="server" Text="<%$ LocRaw:Column.Source %>" Key="source" />
        <bx:BXTextBoxStringFilter ID="title" runat="server" Text="<%$ LocRaw:Column.Title %>" Key="title" />
        <bx:BXTextBoxStringFilter ID="message" runat="server" Text="<%$ LocRaw:Column.Message %>" Key="message" />
        <bx:BXTimeIntervalFilter ID="occured" runat="server" Text="<%$ LocRaw:Collumn.DateOccured %>" Key="occured" />        
    </bx:BXAdminFilter>
    <bx:BXGridView ID="Grid" runat="server" ContentName=""
        SelectedColumnColor="White" AllowPaging="True" AllowSorting="True" PageSize="100"
        Width="100%" AutoCommandField="False" AutoSelectField="False" DataSourceID="Grid" OnSelect="Grid_Select" OnSelectCount="Grid_SelectCount"   
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
        
        <Columns>
			<asp:BoundField DataField="num" HeaderText="#" ReadOnly="True">
				<ItemStyle font-bold="True" horizontalalign="Center" />
			</asp:BoundField>
			<asp:BoundField DataField="id" HeaderText="ID" ReadOnly="True" SortExpression="id">
				<itemstyle horizontalalign="Center" />
			</asp:BoundField>
			<asp:ImageField DataAlternateTextField="type" DataImageUrlField="type" DataImageUrlFormatString="img/logstatus{0}.png"
				HeaderText="<%$ LocRaw:Column.Type %>" ReadOnly="True" SortExpression="type">
				<ItemStyle horizontalalign="Center" verticalalign="Middle" />
			</asp:ImageField>
			<asp:BoundField DataField="source" HeaderText="<%$ LocRaw:Column.Source %>" HtmlEncode="False" ReadOnly="True" SortExpression="source" >
				<ItemStyle horizontalalign="Center" />
			</asp:BoundField>
			<asp:BoundField DataField="code" HeaderText="<%$ LocRaw:Column.Code %>" ReadOnly="True" SortExpression="code" Visible="false" >
				<ItemStyle horizontalalign="Center" />
			</asp:BoundField>
			<asp:TemplateField HeaderText="<%$ LocRaw:Column.Message %>">
				<ItemTemplate>
					<asp:Label ID="Title" runat="server" Font-Bold="True" ForeColor="#FF8000" Text='<%# Eval("title") %>' /><br />
					<asp:Literal ID="Message" runat="server" Text='<%# Eval("message") %>' />
			</ItemTemplate>
			</asp:TemplateField>
			<asp:BoundField DataField="occured" HeaderText="<%$ LocRaw:Collumn.DateOccured %>" ReadOnly="True" SortExpression="occured" >
				<ItemStyle horizontalalign="Center" />
			</asp:BoundField>
		</Columns>
    </bx:BXGridView>
</asp:Content>
