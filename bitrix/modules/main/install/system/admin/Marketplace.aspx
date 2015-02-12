<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="Marketplace.aspx.cs" Inherits="bitrix_admin_Marketplace" Title="Marketplace" Async="true" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
	<bx:BXAdminFilter runat="server" ID="AdminFilter" AssociatedGridView="GridView">
		<bx:BXTextBoxFilter Text="<%$ LocRaw:FilterText.Name %>" Key="Title" Visibility="StartVisible" ValueType="String" />
		<bx:BXDropDownFilter runat="server" ID="CategoryFilter" Text="<%$ LocRaw:FilterText.Category %>" Key="Category" ValueType="Integer" />
		<bx:BXDropDownFilter runat="server" ID="TypeFilter" Text="<%$ LocRaw:FilterText.Type %>" Key="Type" ValueType="Integer" />
	</bx:BXAdminFilter>
	<bx:BXValidationSummary ID="ErrorMessage" runat="server" CssClass="errorSummary" HeaderText="<%$ Loc:Kernel.Error %>" ValidationGroup="GridView" />
	<bx:BXContextMenuToolbar ID="ContextMenuToolbar" runat="server" />
	<br />
	<bx:BXGridView ID="GridView" runat="server" 
		AllowPaging="True" AllowSorting="True"
		AutoCommandField="True"  AutoSelectField="False"
		ContentName="<%$ LocRaw:TableTitle.Modules %>"
        PopupCommandMenuId="PopupMenu" SettingsToolbarId="ContextMenuToolbar"
        DataSourceID="GridView" DataKeyNames="Id"
		OnSelect="GridView_Select" 
		OnRowDataBound="GridView_RowDataBound"
		OnPopupMenuClick="GridView_PopupMenuClick"
     >
		<Columns>
			<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Image %>">
			<ItemTemplate>
				<%# 
					!BXStringUtility.IsNullOrTrimEmpty((string)Eval("ImageUrl"))
					? string.Format(
						@"<img src=""{0}""{1}{2} alt="""" />",
						HttpUtility.HtmlAttributeEncode((string)Eval("ImageUrl")),
						(int)Eval("ImageWidth") > 0 ? string.Concat(@" width=""", Eval("ImageWidth").ToString(), @"""") : "",
						(int)Eval("ImageHeight") > 0 ? string.Concat(@" height=""", Eval("ImageHeight").ToString(), @"""") : ""			
					)
					: "&nbsp;"
				%>
			</ItemTemplate>
			</asp:TemplateField>
			
			<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Id %>" SortExpression="Id">
			<ItemTemplate>
				<%# 
					string.Format(
						@"<a href=""MarketplaceDetail.aspx?module={0}"" />{1}</a>",
						HttpUtility.HtmlAttributeEncode(HttpUtility.UrlEncode((string)Eval("Id"))),
						HttpUtility.HtmlEncode((string)Eval("Id"))
					)
				%>
			</ItemTemplate>
			</asp:TemplateField>
			
			<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Title %>" SortExpression="Title">
			<ItemTemplate>
				<%# HttpUtility.HtmlEncode((string)Eval("Title")) %>
			</ItemTemplate>
			</asp:TemplateField>
			
			<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Description %>">
			<ItemTemplate>
				<%# BXStringUtility.HtmlEncodeEx((string)Eval("Description")) %>
			</ItemTemplate>
			</asp:TemplateField>
			
			<asp:BoundField DataField="Partner" HeaderText="<%$ LocRaw:ColumnHeaderText.Partner %>" ReadOnly="True" HtmlEncode="true" />
			<asp:BoundField DataField="UpdateDate" SortExpression="UpdateDate" HeaderText="<%$ LocRaw:ColumnHeaderText.UpdateDate %>" HtmlEncode="true" DataFormatString="{0:g}" ReadOnly="True" />
			
			<asp:TemplateField HeaderText="<%$ LocRaw:ColumnHeaderText.Downloaded %>">
			<ItemTemplate>
				<%# (bool)Eval("Downloaded") ? GetMessage("Kernel.Yes") : GetMessage("Kernel.No") %>
			</ItemTemplate>
			</asp:TemplateField>
		</Columns>
	</bx:BXGridView>
	<bx:BXPopupPanel ID="PopupMenu" runat="server">
		<Commands>
			<bx:CommandItem UserCommandId="view" IconClass="view" ItemText="<%$ LocRaw:Kernel.View %>" ItemTitle="<%$ LocRaw:PopupTitle.View %>" Default="true" OnClickScript="location.href='MarketplaceDetail.aspx?module=' + encodeURIComponent(UserData.Id); return false;" />
			<bx:CommandItem UserCommandId="install" ItemText="<%$ LocRaw:PopupText.Install %>" ItemTitle="<%$ LocRaw:PopupTitle.Install %>" OnClickScript="return false" />
		</Commands>
	</bx:BXPopupPanel>
</asp:Content>

