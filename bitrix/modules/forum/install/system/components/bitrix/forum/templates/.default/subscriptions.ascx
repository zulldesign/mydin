<%@ Reference VirtualPath="~/bitrix/components/bitrix/forum/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Forum.Components.ForumTemplate" %>

<bx:IncludeComponent 
	ID="menu" 
	runat="server" 
	Visible="<%$ Results:ShowMenu %>"
	
	ComponentName="bitrix:forum.menu" 
	Template=".default"
	ThemeCssFilePath="" 
	ColorCssFilePath="" 
	SearchUrl="<%$ Results:SearchUrlTemplate %>" 
	ForumIndexUrl="<%$ Results:IndexUrlTemplate %>"
	UserProfileUrlTemplate="<%$ Results:UserProfileUrlTemplate %>" 
	ForumRulesUrl="<%$ Results:ForumRulesUrl %>" 
	ForumHelpUrl="<%$ Results:ForumHelpUrl %>"
	ActiveTopicsUrl="<%$ Results:ActiveTopicsUrlTemplate %>"  
	UnAnsweredTopicsUrl="<%$ Results:UnAnsweredTopicsUrlTemplate %>" 
	UserSubscriptionsUrlTemplate="<%$ Results:UserSubscriptionsUrlTemplate %>"
/>

<%@ Reference VirtualPath="~/bitrix/components/bitrix/forum.subscription/component.ascx" %>
<%@ Import Namespace="Bitrix.Forum.Components" %>
<%@ Register TagName="ForumBreadcrumb" TagPrefix="bx" Src="breadcrumb.ascx" %>
<%
	if (Component.ShowNavigation && ((ForumSubscriptionComponent)subscriptions.Component).FatalError == ForumSubscriptionComponent.ErrorCode.None)
	{
		topbreadcrumb.Component = bottombreadcrumb.Component = Component;

		Bitrix.Forum.Components.ForumSubscriptionComponent postForm = (Bitrix.Forum.Components.ForumSubscriptionComponent)subscriptions.Component;
		topbreadcrumb.CustomCrumbTitle = bottombreadcrumb.CustomCrumbTitle = GetMessage("Breadcrumb.Subscription.Name");
	}
%>
<bx:ForumBreadcrumb 
	runat="server" 
	ID="topbreadcrumb" 
	Visible="<%$ Results:ShowNavigation %>"
	
	CssPostfix="top" 
	MaxWordLength="<%$ Results:MaxWordLength %>"
	RootTitle="<%$ Parameters:ForumListTitle %>" 
	RootUrlTemplate="<%$ Results:IndexUrlTemplate %>" 
	ForumUrlTemplate="<%$ Results:TopicListUrlTemplate %>" 
	TopicUrlTemplate="<%$ Results:TopicUrlTemplate %>" 
/>

<bx:IncludeComponent 
	id="subscriptions" 
	runat="server" 
	componentname="bitrix:forum.subscription" 
	Template=".default" 
	ThemeCssFilePath=""
	ColorCssFilePath="" 
	CacheMode="None" 
	CacheDuration="30"
	
	PagingPageID="<%$ Results:PageId %>" 
	PagingIndexTemplate="<%$ Results:UserSubscriptionsUrlTemplate %>" 
	PagingPageTemplate="<%$ Results:UserSubscriptionsPageUrlTemplate %>"  
	 
	UserId="" 
	
	ForumUrlTemplate="<%$ Results:TopicListUrlTemplate %>" 
	TopicUrlTemplate="<%$ Results:TopicUrlTemplate %>" 
	PostReadUrlTemplate="<%$ Results:PostReadUrlTemplate %>" />
	
<bx:ForumBreadcrumb 
	runat="server" 
	ID="bottombreadcrumb" 
	Visible="<%$ Results:ShowNavigation %>"
	
	CssPostfix="bottom"
	MaxWordLength="<%$ Results:MaxWordLength %>"
	RootTitle="<%$ Parameters:ForumListTitle %>" 
	RootUrlTemplate="<%$ Results:IndexUrlTemplate %>" 
	ForumUrlTemplate="<%$ Results:TopicListUrlTemplate %>" 
	TopicUrlTemplate="<%$ Results:TopicUrlTemplate %>" 
/>

<% 
	if (((ForumSubscriptionComponent)subscriptions.Component).FatalError == ForumSubscriptionComponent.ErrorCode.None)
	{ 
%>
<bx:IncludeComponent 
	ID="forums" 
	runat="server" 
	ComponentName="bitrix:forum.list" 
	Template="dropdown" 
	CurrentForumId="<%$ Results:ForumId %>"
	ThemeCssFilePath="" 
	ColorCssFilePath="" 
	PostReadUrlTemplate="<%$ Results:PostReadUrlTemplate %>" 
	TopicListUrlTemplate="<%$ Results:TopicListUrlTemplate %>" 
/>
<% } %>