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

<%@ Register TagName="ForumBreadcrumb" TagPrefix="bx" Src="breadcrumb.ascx" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/forum.topic.list/component.ascx" %>
<%@ Import Namespace="Bitrix.Forum.Components" %>
<%
	if (Component.ShowNavigation && ((ForumTopicListComponent)topics.Component).FatalError == ForumTopicListComponent.ErrorCode.None)
	{
		topbreadcrumb.Component = bottombreadcrumb.Component = Component;
	
		Bitrix.Forum.Components.ForumTopicListComponent topicList = (Bitrix.Forum.Components.ForumTopicListComponent)topics.Component;
		topbreadcrumb.Forum = bottombreadcrumb.Forum = topicList.Forum;
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
/>

<bx:IncludeComponent 
	ID="topics" 
	runat="server" 
	ComponentName="bitrix:forum.topic.list" 
	Template=".default" 
	ThemeCssFilePath="" 
	ColorCssFilePath=""
	ForumId="<%$ Results:ForumId %>" 
	PagingPageID="<%$ Results:PageId %>" 
	PagingIndexTemplate="<%$ Results:TopicListUrlTemplate %>" 
	PagingPageTemplate="<%$ Results:TopicListPageUrlTemplate %>" 
	TopicUrlTemplate="<%$ Results:TopicUrlTemplate %>" 
	PostReadUrlTemplate="<%$ Results:PostReadUrlTemplate %>" 
	NewTopicUrlTemplate="<%$ Results:NewTopicUrlTemplate %>" 
	MoveTopicsUrlTemplate="<%$ Results:MoveTopicsUrlTemplate %>" 
	HeaderLinks="<%$ Results:ForumHeaderLinks %>" 
	SortByVotingTotals="<%$ Parameters:SortTopicsByVotingTotals %>"
/>

<bx:ForumBreadcrumb 
	runat="server" 
	ID="bottombreadcrumb" 
	Visible="<%$ Results:ShowNavigation %>"
	
	CssPostfix="bottom" 
	MaxWordLength="<%$ Results:MaxWordLength %>"
	RootTitle="<%$ Parameters:ForumListTitle %>" 
	RootUrlTemplate="<%$ Results:IndexUrlTemplate %>" 
/>


<% if (((ForumTopicListComponent)topics.Component).FatalError == ForumTopicListComponent.ErrorCode.None) { %>
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