<%@ Page Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.BXPublicPage, Main" %>
<asp:Content ID="Content1" ContentPlaceHolderID="bxcontent" runat="server" > 
  
<bx:IncludeComponent 
	id="newslist1" 
	runat="server" 
	componentname="bitrix:news.list" 
	template="news" 
	IBlockTypeId="" 
	IBlockId="<%$ Options:Bitrix.BankSite:BankNewsIBlockId,int %>" 
	PagingRecordsPerPage="5" 
	ShowPreviewText="True" 
	ShowDetailText="False" 
	ShowPreviewPicture="True" 
	PreviewTruncateLen="" 
	ShowTitle="True" 
	SetTitle="False" 
	ShowDate="True" 
	ActiveDateFormat="MM.dd.yyyy" 
	HideLinkWhenNoDetail="False" 
	PropertyCode="" 
	SortBy1="ActiveFromDate" 
	SortBy2="Sort" 
	SortOrder1="Desc" 
	SortOrder2="Desc" 
	DisplayPanel="False" 
	UsePermissions="False" 
	GroupPermissions="" 
	ShowActiveElements="True" 
	ParentSectionId="0" 
	IncludeSubsections="True" 
	DetailUrl="<%$ Options:Bitrix.BankSite:NewsDetailUrl %>" 
	PagingAllow="False" 
	PagingMode="direct" 
	PagingTemplate="catalog" 
	PagingShowOne="False" 
	PagingAllowAll="False" 
	PagingTitle="Pages" 
	PagingPosition="bottom" 
	PagingMaxPages="10" 
	PagingMinRecordsInverse="1" 
	CacheMode="None" 
	CacheDuration="30" 
/>  
 </asp:Content>




