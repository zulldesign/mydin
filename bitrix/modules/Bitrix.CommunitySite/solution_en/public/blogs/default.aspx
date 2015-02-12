<%@ Page Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.BXPublicPage, Main" Title="Blog Feed" %>
<script runat="server">

	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		Bitrix.UI.BXPage.RegisterLink(
			"wlwmanifest",
			"application/wlwmanifest+xml",
			Bitrix.BXSite.Current.DirectoryAbsolutePath + "wlwmanifest.xml"
		);
	}
</script>
<asp:Content ID="Content1" ContentPlaceHolderID="bxcontent" runat="server" > 

<bx:IncludeComponent 
	id="BlogList" 
	runat="server" 
	componentname="bitrix:blog.list" 
	template=".default" 
	ThemeCssFilePath="~/bitrix/components/bitrix/blog/templates/.default/style.css" 
	ColorCssFilePath="<%$ Options:Bitrix.CommunitySite:BlogColorScheme %>" 
	CategoryId="" 
	SortOrderFirst="Desc" 
	SortBySecond="Sort" 
	SortOrderSecond="Desc" 
	MaxWordLength="40" 
	SetPageTitle="True" 
	BlogPageUrlTemplate="<%$ Options:Bitrix.CommunitySite:BlogUrlTemplate %>" 
	BlogOwnerProfilePageUrlTemplate="<%$ Options:Bitrix.CommunitySite:UserProfileUrlTemplate %>" 
	PagingAllow="True" 
	PagingMode="direct" 
	PagingTemplate=".default" 
	PagingShowOne="False" 
	PagingRecordsPerPage="10" 
	PagingTitle="Pages" 
	PagingPosition="bottom" 
	PagingMaxPages="10" 
	PagingMinRecordsInverse="1" 
	PagingPageID="<%$ Request:page %>" 
	PagingIndexTemplate="<%$ Options:Bitrix.CommunitySite:BlogSefFolder:~{0} %>" 
	PagingPageTemplate="?page=#pageid#" 
	CacheMode="None" 
	CacheDuration="30" 
	SortByFirst="ID" 
/>
 </asp:Content>

