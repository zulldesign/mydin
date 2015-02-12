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
<bx:IncludeComponent 
	ID="move"
	runat="server" 
	ComponentName="bitrix:forum.topic.move" 
	Template=".default" 
	ThemeCssFilePath=""
	ColorCssFilePath="" 
	CacheMode="None" 
	CacheDuration="30"
	TopicsToMove="<%$ Results:MoveTopicsIds %>" 
	TopicUrlTemplate="<%$ Results:TopicUrlTemplate %>" 
	RedirectUrlTemplate="<%$ Results:RedirectUrlTemplate %>"	
/>

<%@ Reference VirtualPath="~/bitrix/components/bitrix/forum.topic.move/component.ascx" %>
<%@ Import Namespace="Bitrix.Forum.Components" %>
<% 
	if (((ForumTopicMoveComponent)move.Component).FatalError == ForumTopicMoveComponent.ErrorCode.None)
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
<%} %>