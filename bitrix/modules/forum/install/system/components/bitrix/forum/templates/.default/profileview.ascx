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
<bx:IncludeComponent 
	ID="profile" 
	runat="server" 
	ComponentName="bitrix:user.profile.view" 
	Template=".default" 
	UserId="<%$ Results:UserId %>" 
	Fields="<%$ Results:ProfileFields %>"
	EditProfileTitle="<%$ LocRaw:UrlTitle.Edit %>" 
	EditProfileUrlTemplate="<%$ Results:UserProfileEditUrlTemplate %>"
/>