<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.personal/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogPersonalTemplate" %>

<bx:IncludeComponent 
	ID="Rss" 
	runat="server" 
	ComponentName="bitrix:blog.rss" 
	
	BlogSlug="<%$ Results:BlogSlug %>"
	PostId="<%$ Results:PostId %>"
	FeedTitle=""
	FeedDescription=""
	StuffType="<%$ Results:RssStuffType %>"
	FiltrationType="<%$ Results:RssFiltrationType %>"
	CategoryId=""
	EnablePostCut="False"
	ItemQuantity="10"
	FeedUrlTemplate="<%$ Results:RssFeedUrlTemplate %>"
	FeedItemUrlTemplate="<%$ Results:RssItemUrlTemplate %>"
/>

