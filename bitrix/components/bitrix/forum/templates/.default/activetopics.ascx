<%@ Reference VirtualPath="~/bitrix/components/bitrix/forum/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Forum.Components.ForumTemplate" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/forum.post.list/component.ascx" %>

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
	UserSubscriptionsUrlTemplate="<%$ Results:UserSubscriptionsUrlTemplate %>"
    ActiveTopicsUrl="<%$ Results:ActiveTopicsUrlTemplate %>"  
	UnAnsweredTopicsUrl="<%$ Results:UnAnsweredTopicsUrlTemplate %>"

/>


<%@ Reference VirtualPath="~/bitrix/components/bitrix/forum.post.list/component.ascx" %>
<%@ Import Namespace="Bitrix.Forum.Components" %>
<%@ Register TagName="ForumBreadcrumb" TagPrefix="bx" Src="breadcrumb.ascx" %>
<%
	if (Component.ShowNavigation && ((ForumPostListComponent)ActiveTopics.Component).ComponentError == ForumPostListComponentError.None)
	{
		topbreadcrumb.Component = bottombreadcrumb.Component = Component;

		ForumPostListComponent postForm = (ForumPostListComponent)ActiveTopics.Component;
		topbreadcrumb.CustomCrumbTitle = bottombreadcrumb.CustomCrumbTitle = GetMessage("Breadcrumb.ActiveTopics.Name");
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
 id="ActiveTopics"
 runat="server"
 componentname="bitrix:forum.post.list"
 template=".topics"
 AuthorId=""
 DisplayMode="ActiveTopics"
 GroupingOption="None"
 DateCreateFrom=""
 DateCreateTo=""
 Forums="<%$Parameters:AvailableForums%>"
 ThemeCssFilePath=""
 ColorCssFilePath=""
 SortBy="ID"
 SortDirection="Desc"
 ForumReadUrlTemplate="<%$Results:TopicListUrlTemplate %>"
 TopicReadUrlTemplate="<%$Results:TopicUrlTemplate%>"
 PostReadUrlTemplate="<%$Results:PostReadUrlTemplate%>"
 AuthorProfileUrlTemplate="<%$ Results:UserProfileUrlTemplate%>"
 UserPostsTemplate="<%$ Results:UserPostsTemplate%>"
 
 PagingIndexTemplate="<%$ Results:ActiveTopicsUrlTemplate %>" 
 PagingPageTemplate="<%$ Results:ActiveTopicsPageUrlTemplate %>"  
 MaxWordLength="<%$Parameters:MaxWordLength %>"
 PagingAllow="<%$Parameters:PagingAllow %>"
 PagingMode="<%$Parameters:PagingMode %>"
 PagingTemplate="<%$Parameters:PagingTemplate%>"
 PagingShowOne="<%$Parameters:PagingShowOne %>"
 PagingRecordsPerPage="<%$Parameters:PagingRecordsPerPage %>"
 PagingTitle="<%$Parameters:PagingTitle %>"
 PagingPosition="<%$Parameters:PagingPosition %>"
 PagingMaxPages="<%$Parameters:PagingMaxPages %>"
 PagingMinRecordsInverse="<%$Parameters:PagingMinRecordsInverse %>"
 PagingPageID="<%$ Request:page%>"
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
	if (((ForumPostListComponent)ActiveTopics.Component).ComponentError == ForumPostListComponentError.None)
	{ 
%>
<bx:IncludeComponent 
	ID="forums" 
	runat="server" 
	ComponentName="bitrix:forum.list" 
	Template="dropdown" 
	ThemeCssFilePath="" 
	ColorCssFilePath="" 
	PostReadUrlTemplate="<%$ Results:PostReadUrlTemplate %>" 
	TopicListUrlTemplate="<%$ Results:TopicListUrlTemplate %>" 
/>
<% } %>