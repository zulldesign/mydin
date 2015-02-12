<%@ Page Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.BXPublicPage, Main"  Title="Search" %>

<asp:Content ID="Content" ContentPlaceHolderID="BXContent" runat="server">
<bx:IncludeComponent 
	runat="server" 
	ID="SearchPage" 
	ComponentName="bitrix:search.page" 
	Template=".default" 
	SearchFilter="" 
	MaxChars="500" 
	HighlightDiameter="200" 
	ShowFilter="False" 
	ShowFilterItems="" 
	ShowTags="NotRejected" 
	ParamSearch="q" 
	ParamPage="page" 
	ParamWhere="where" 
	ParamTags="tags" 
	PagingAllow="True" 
	PagingMode="direct" 
	PagingTemplate=".default" 
	PagingShowOne="False" 
	PagingRecordsPerPage="10" 
	PagingTitle="Pages" 
	PagingPosition="bottom" 
	PagingMaxPages="10" 
	PagingMinRecordsInverse="1" 
/>

</asp:Content>