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

<%@ Reference VirtualPath="~/bitrix/components/bitrix/forum.post.form/component.ascx" %>
<%@ Import Namespace="Bitrix.Forum.Components" %>
<%@ Register TagName="ForumBreadcrumb" TagPrefix="bx" Src="breadcrumb.ascx" %>
<%
	if (Component.ShowNavigation && ((ForumPostFormComponent)form.Component).FatalError == ForumPostFormComponent.ErrorCode.None)
	{
		topbreadcrumb.Component = bottombreadcrumb.Component = Component;

		Bitrix.Forum.Components.ForumPostFormComponent postForm = (Bitrix.Forum.Components.ForumPostFormComponent)form.Component;
		topbreadcrumb.Forum = bottombreadcrumb.Forum = postForm.Forum;
		topbreadcrumb.Topic = bottombreadcrumb.Topic = postForm.Topic;
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
	ID="form"
	runat="server" 
	ComponentName="bitrix:forum.post.form" 
	Template=".default" 
	ThemeCssFilePath=""
	ColorCssFilePath="" 
	CacheMode="None" 
	CacheDuration="30"
	ForumId="<%$ Results:ForumId %>" 
	TopicId="<%$ Results:TopicId %>" 
	PostId="<%$ Results:PostId %>" 
	ParentPostId="<%$ Results:ParentPostId %>" 
	Mode="<%$ Results:FormMode %>" 
	RedirectUrlTemplate="<%$ Results:RedirectUrlTemplate %>" 
	PostReadUrlTemplate="<%$ Results:PostReadUrlTemplate %>"
	HeaderLinks="<%$ Results:FormHeaderLinks %>" 
/>

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
	if (((ForumPostFormComponent)form.Component).FatalError == ForumPostFormComponent.ErrorCode.None)
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