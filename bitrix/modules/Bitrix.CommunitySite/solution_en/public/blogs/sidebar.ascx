<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.UI.BXControl" %>
			
<bx:IncludeComponent 
	id="PopularPosts" 
	runat="server" 
	componentname="bitrix:blog.posts" 
	template="popular-items" 
	ThemeCssFilePath="" 
	ColorCssFilePath="" 
	SortBy="ByViews" 
	PeriodDays="30" 
	Tags="" 
	FilterByPostCustomProperty="False" 
	MaxWordLength="15" 
	SetPageTitle="False" 
	BlogUrlTemplate="<%$ Options:Bitrix.CommunitySite:BlogUrlTemplate %>" 
	UserProfileUrlTemplate="<%$ Options:Bitrix.CommunitySite:UserProfileUrlTemplate %>" 
	PostViewUrlTemplate="<%$ Options:Bitrix.CommunitySite:PostViewUrlTemplate %>" 
	SearchTagsUrlTemplate="<%$ Options:Bitrix.CommunitySite:SearchTagsUrlTemplate %>" 
	PagingAllow="False"
	CacheMode="Auto" 
	CacheDuration="7200" 
/> 

					
<bx:IncludeComponent 
	id="NewBlogs" 
	runat="server" 
	componentname="bitrix:blog.list" 
	template="new-blogs" 
	ThemeCssFilePath="" 
	ColorCssFilePath="" 
	CategoryId="" 
	SortByFirst="ID" 
	SortOrderFirst="Desc" 
	SortBySecond="Sort" 
	SortOrderSecond="Desc" 
	MaxWordLength="15" 
	SetPageTitle="False" 
	BlogPageUrlTemplate="<%$ Options:Bitrix.CommunitySite:BlogUrlTemplate %>" 
	BlogOwnerProfilePageUrlTemplate="<%$ Options:Bitrix.CommunitySite:UserProfileUrlTemplate%>" 
	PagingAllow="False" 
	CacheMode="Auto" 
	CacheDuration="7200" />
