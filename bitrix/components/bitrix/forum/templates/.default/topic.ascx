<%@ Reference VirtualPath="~/bitrix/components/bitrix/forum/component.ascx" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/forum.topic.read/component.ascx" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/forum.post.form/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Forum.Components.ForumTemplate" %>
<%@ Import Namespace="Bitrix.Forum.Components" %>
<%@ Import Namespace="Bitrix.Forum"%>
<script runat="server">
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		int postId = Component.ComponentCache.GetInt("PostId");
		if (postId != 0 && Component.ComponentCache.GetInt("TopicId") == 0)
		{
			BXForumPost post = BXForumPost.GetById(postId);
			if (post != null)
					form.Component.Parameters["TopicId"] = post.TopicId.ToString();
		}
	}

	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);
		if (((ForumTopicReadComponent)posts.Component).FatalError != ForumTopicReadComponent.ErrorCode.None)
		{
			ForumPostFormComponent component = (ForumPostFormComponent)form.Component;
			component.FatalError = ForumPostFormComponent.ErrorCode.FatalPostNotFound;
		}
	}
	
</script>
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
<%
	if (Component.ShowNavigation && ((ForumTopicReadComponent)posts.Component).FatalError == ForumTopicReadComponent.ErrorCode.None)
	{
		topbreadcrumb.Component = bottombreadcrumb.Component = Component;

		Bitrix.Forum.Components.ForumTopicReadComponent topicRead = (Bitrix.Forum.Components.ForumTopicReadComponent)posts.Component;
		topbreadcrumb.Forum = bottombreadcrumb.Forum = topicRead.Forum;
		topbreadcrumb.Topic = bottombreadcrumb.Topic = topicRead.Topic;
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
/>

<bx:IncludeComponent 
	ID="posts"
	runat="server" 
	ComponentName="bitrix:forum.topic.read" 
	Template=".default" 
	ThemeCssFilePath=""
	ColorCssFilePath="" 
	CacheMode="None" 
	CacheDuration="30"
	PagingPageID="<%$ Results:PageId %>" 
	PagingIndexTemplate="<%$ Results:TopicUrlTemplate %>" 
	PagingPageTemplate="<%$ Results:TopicPageUrlTemplate %>" 
	TopicId="<%$ Results:TopicId %>"
	PostId="<%$ Results:PostId %>" 
	TopicUrlTemplate="<%$ Results:TopicUrlTemplate %>" 
	TopicListUrlTemplate="<%$ Results:TopicListUrlTemplate %>" 
	TopicReplyUrlTemplate="<%$ Results:TopicReplyUrlTemplate %>" 
	TopicMoveUrlTemplate="<%$ Results:TopicMoveUrlTemplate %>" 
	TopicEditUrlTemplate="<%$ Results:TopicEditUrlTemplate %>" 
	PostReadUrlTemplate="<%$ Results:PostReadUrlTemplate %>" 
	PostQuoteUrlTemplate="<%$ Results:PostQuoteUrlTemplate %>" 
	PostEditUrlTemplate="<%$ Results:PostEditUrlTemplate %>" 
	UserProfileUrlTemplate="<%$ Results:UserProfileUrlTemplate %>"
	UserPostsReadUrlTemplate="<%$ Results:UserPostsReadUrlTemplate %>"  
	HeaderLinks="<%$ Results:TopicHeaderLinks %>"
/>


<%
	ForumPostFormComponent formComponent = (ForumPostFormComponent)form.Component;
	ForumPostFormTemplate formTemplate = (ForumPostFormTemplate)formComponent.ComponentTemplate;
	
	//formTemplate.PostTextareaName
	
	if (((ForumTopicReadComponent)posts.Component).FatalError == ForumTopicReadComponent.ErrorCode.None
		&& formComponent.FatalError == ForumPostFormComponent.ErrorCode.None) 
	{ 
%>

<script type="text/javascript">
	function GetForumPostTextArea()
	{
		return document.getElementById('<%=formTemplate.PostTextareaClientID %>');
	}
</script>

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
	PostId="" 
	ParentPostId=""
	SetPageTitle="False" 
	Mode="add" 
	RedirectUrlTemplate="<%$ Results:PostReadUrlTemplate %>"
	PostReadUrlTemplate="<%$ Results:PostReadUrlTemplate %>" 
	HeaderLinks="<%$ Results:FormHeaderLinks %>" 
/>
<%
	} 
%>

<bx:ForumBreadcrumb 
	runat="server" 
	ID="bottombreadcrumb" 
	Visible="<%$ Results:ShowNavigation %>"
	
	CssPostfix="bottom" 
	MaxWordLength="<%$ Results:MaxWordLength %>"
	RootTitle="<%$ Parameters:ForumListTitle %>"  
	RootUrlTemplate="<%$ Results:IndexUrlTemplate %>" 
	ForumUrlTemplate="<%$ Results:TopicListUrlTemplate %>" 
/>

<% 
	if (((ForumTopicReadComponent)posts.Component).FatalError == ForumTopicReadComponent.ErrorCode.None)
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