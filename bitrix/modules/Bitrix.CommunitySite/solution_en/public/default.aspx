<%@ Page Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.BXPublicPage, Main"  Title="" %>


<asp:Content ID="Content1" ContentPlaceHolderID="bxcontent" runat="server" > 
 
<a href="rss/" class="page-feed-icon" runat="server"></a>
 
<bx:IncludeComponent 
	id="PostList" 
	runat="server" 
	componentname="bitrix:blog.posts" 
	template=".default" 
	ThemeCssFilePath="~/bitrix/components/bitrix/blog/templates/.default/style.css" 
	ColorCssFilePath="<%$ Options:Bitrix.CommunitySite:BlogColorScheme %>" 
	SortBy="ByDate" 
	PeriodDays="0" 
	MaxWordLength="40" 
	SetPageTitle="False" 
	BlogUrlTemplate="<%$ Options:Bitrix.CommunitySite:BlogUrlTemplate %>" 
	UserProfileUrlTemplate="<%$ Options:Bitrix.CommunitySite:UserProfileUrlTemplate %>" 
	PostViewUrlTemplate="<%$ Options:Bitrix.CommunitySite:PostViewUrlTemplate %>" 
	PagingAllow="True" 
	PagingMode="inverse" 
	PagingTemplate=".default" 
	PagingShowOne="False" 
	PagingRecordsPerPage="10" 
	PagingTitle="Pages" 
	PagingPosition="bottom" 
	PagingMaxPages="10" 
	PagingMinRecordsInverse="10" 
	PagingPageID="<%$ Request:page %>" 
	PagingIndexTemplate="<%$ Options:Bitrix.CommunitySite:SiteFolder %>" 
	PagingPageTemplate="?page=#pageid#" 
	Tags="" 
	SearchTagsUrlTemplate="<%$ Options:Bitrix.CommunitySite:SearchTagsUrlTemplate %>" 
	FilterByPostCustomProperty="False" 
	PostCustomPropertyFilterSettings="p:o:0:{};" 
	CacheMode="False" 
	CacheDuration="60" 
	BlogSlug="" 
	PostRssUrlTemplate="<%$ Options:Bitrix.CommunitySite:PostRssUrlTemplate %>" 
	PostEditUrlTemplate="<%$ Options:Bitrix.CommunitySite:PostEditUrlTemplate %>" 
	SetMasterTitle="False" 
/>

</asp:Content>