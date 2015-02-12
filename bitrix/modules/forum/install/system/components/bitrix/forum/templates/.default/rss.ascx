<%@ Control Language="C#" ClassName="rss" %>

<bx:IncludeComponent 
	ID="forumRss"
	runat="server" 
	ComponentName="bitrix:forum.rss" 
	Template=".default"
	FeedTitle="<%$ Results:ForumRssFeedTitle %>" 
	FeedDescription="<%$ Results:ForumRssFeedDescription %>" 
	StuffType="<%$ Results:ForumRssStuffType %>"
	FiltrationType="<%$ Results:ForumRssFiltrationType %>" 
    CategoryId="0" 
    ForumId="<%$ Results:ForumRssForumId %>" 
    TopicId="<%$ Results:ForumRssTopicId %>" 
    FeedUrlTemplate="<%$ Results:ForumRssFeedUrlTemplate %>" 
    FeedItemUrlTemplate="<%$ Results:ForumRssFeedItemUrlTemplate %>" 	
    ItemQuantity="<%$ Results:ForumRssFeedItemQuantity %>"
    FeedTtl="<%$ Results:ForumRssFeedTtl %>"
	CacheMode="<%$ Results:ForumRssFeedCacheMode %>" 
	CacheDuration="<%$ Results:ForumRssFeedCacheDuration %>"
/>
