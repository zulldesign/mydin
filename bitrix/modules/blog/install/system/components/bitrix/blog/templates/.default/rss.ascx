<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogTemplate" %>

<bx:IncludeComponent 
	ID="blogpost" 
	runat="server" 
	ComponentName="bitrix:blog.rss" 
	Template=".default"
	
	FeedTitle=""
	FeedDescription=""
	StuffType="<%$ Results:RssStuffType %>"
	FiltrationType="<%$ Results:RssFiltrationType %>"
	CategoryId="<%$ Parameters:CategoryId %>"
	BlogSlug="<%$ Results:BlogSlug %>"
	PostId="<%$ Results:PostId %>"
	EnablePostCut="False"
	ItemQuantity="10"
	FeedUrlTemplate="<%$ Results:RssBlogPostsUrlTemplate %>"
	FeedItemUrlTemplate="<%$ Results:RssItemUrlTemplate %>"
/>

