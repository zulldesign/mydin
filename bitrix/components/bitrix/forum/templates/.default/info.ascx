<%@ Reference VirtualPath="~/bitrix/components/bitrix/forum/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Forum.Components.ForumTemplate" %>
<script runat="server">
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		if (Component.ShouldSetPageTitle)
			Component.SetPageTitle(Component.PageTitle);
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
	if (Component.ShowNavigation)
	{
		topbreadcrumb.Component = bottombreadcrumb.Component = Component;

		//ForumPostListComponent postForm = (ForumPostListComponent)ActiveTopics.Component;
		topbreadcrumb.CustomCrumbTitle = bottombreadcrumb.CustomCrumbTitle = Component.PageTitle;
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
	id="include" 
	runat="server" 
	ComponentName="bitrix:includeArea" 
	Template=".default" 
	Mode="File" 
	RecursiveFileSearch="False" 
	FilePath="<%$ Results:InfoFilePath %>"
	AllowEditing="<%$ Results:InfoAllowEditing %>"
	Title="<%$ Results:PageTitle %>"
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