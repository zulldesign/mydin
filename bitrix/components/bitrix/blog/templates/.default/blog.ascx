<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogTemplate" %>

<% if (Component.DisplayMenu) {%>
<bx:IncludeComponent 
	ID="menu" 
	runat="server" 
	ComponentName="bitrix:blog.menu" 
	Template=".default"
	ThemeCssFilePath="" 
	ColorCssFilePath="" 
	CategoryId="<%$Parameters:CategoryId %>" 		
	NewPostUrlTemplate="<%$ Results:NewPostUrlTemplate %>" 
	UserBlogUrlTemplate="<%$ Results:PostListUrlTemplate %>" 
	UserBlogSettingsUrlTemplate="<%$ Results:BlogEditUrlTemplate %>" 
	BlogIndexUrl="<%$ Results:IndexUrlTemplate %>" 
	UserProfileUrlTemplate="<%$ Results:UserProfileUrlTemplate %>"
	DraftPostListUrlTemplate="<%$ Results:DraftPostListUrlTemplate%>"
	Visible="<%$ Results:DisplayMenu%>"
	NewBlogUrlTemplate="<%$ Results:NewBlogUrlTemplate %>"
/>
<%} %>
<div class="blog-post-list-container">
	<% if (Component.DisplaySidebar && SideBar.Visible && ((BlogPostListComponent)blog.Component).FatalError == BlogPostListComponent.ErrorCode.None) { %>
	<div class="blog-sidebar">
		<%@ Register Src="blog-sidebar.ascx" TagPrefix="bx" TagName="SideBar" %>
		<bx:SideBar
			runat="server" 
			ID="SideBar"
			BlogSlug="<%$ Results:BlogSlug %>" 
			Visible="<%$ Results:DisplaySidebar %>"
			DraftPostListUrlTemplate="<%$ Results:DraftPostListUrlTemplate%>"
			IndexUrlTemplate="<%$ Results:IndexUrlTemplate %>"
			NewPostUrlTemplate="<%$ Results:NewPostUrlTemplate %>"
			RssBlogPostsTemplate="<%$ Results:RssBlogPostsUrlTemplate %>"
			BlogEditUrlTemplate="<%$ Results:BlogEditUrlTemplate %>"
			UserProfileUrlTemplate="<%$ Results:UserProfileUrlTemplate %>"
			RssBlogPostsUrlTemplate="<%$ Results:RssBlogPostsUrlTemplate %>"
			MetaWeblogUrlTemplate="<%$ Results:BlogMetaWeblogApiUrlTemplate %>"
		/>
	</div>
	
	<div class="blog-posts-column">
	<% } %>

		<bx:IncludeComponent 
			ID="blog"
			runat="server" 
			ComponentName="bitrix:blog.post.list" 
			Template=".default"
			ThemeCssFilePath="" 
			ColorCssFilePath="" 
			CategoryId="<%$Parameters:CategoryId %>" 
			Publishmode="<%$ Results:PublishMode %>" 
			BlogUrlTemplate="<%$ Results:PostListUrlTemplate %>" 
			UserProfileUrlTemplate="<%$ Results:UserProfileUrlTemplate %>" 
			PostViewUrlTemplate="<%$ Results:PostUrlTemplate %>" 
			PostRssUrlTemplate="<%$ Results:RssBlogPostCommentsUrlTemplate %>"
			PostEditUrlTemplate="<%$ Results:PostEditUrlTemplate %>"
			SearchTagsUrlTemplate="<%$ Results:SearchTagsUrlTemplate %>" 
			PagingRecordsPerPage="<%$ Results:PostsPerPage %>" 
			PagingPageID="<%$ Results:PageId %>" 
			PagingIndexTemplate="<%$ Results:PagingIndexTemplate %>" 
			PagingPageTemplate="<%$ Results:PagingPageTemplate %>" 
			BlogSlug="<%$ Results:BlogSlug %>" 
			SortByVotingTotals = "<%$ Parameters:SortBlogPostsByVotingTotals %>"
		/>
		
        <%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.post.list/component.ascx" %>
        <%@ Import Namespace="Bitrix.Blog.Components" %>
        <%@ Import Namespace="Bitrix.DataTypes" %>
        <% 
	        if (blogPostList.FatalError == BlogPostListComponent.ErrorCode.None 
				&& blogPostList.Posts.Count < 1 
				&& blogPostList.PostPublishMode == BlogPostListComponent.PublishMode.Published 
				&& !blogPostList.IsFilterSet 
				&& blogPostList.IsOwner)
	        { 
		        BXParamsBag<object> replace = new BXParamsBag<object>();
		        replace["BlogSlug"] = Results["BlogSlug"];
	        %>
	        <br /><a href="<%= Encode(blogPostList.ResolveTemplateUrl((string)Results["NewPostUrlTemplate"], replace)) %>"><%= GetMessage("CreateFirstPost") %></a>.
        <%
	        } 
        %>
        
    <% if (Component.DisplaySidebar && SideBar.Visible && blogPostList.FatalError == BlogPostListComponent.ErrorCode.None) { %>
	</div>
	<%} %>

	<div class="blog-clear-float"></div>
</div>

<script runat="server">
	BlogPostListComponent blogPostList;
	
	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);
		
		blogPostList = (BlogPostListComponent)blog.Component;
		if (blogPostList.FatalError != BlogPostListComponent.ErrorCode.None)
			return;
		
		var url = Component.ComponentCache.GetString("BlogMetaWeblogManifestUrlTemplate");
		if (!string.IsNullOrEmpty(url))
		{
			var replace = new BXParamsBag<object>
			{
				{ "BlogSlug", blogPostList.BlogSlug }
			};
			
			Bitrix.UI.BXPage.RegisterLink(
				"wlwmanifest",
				"application/wlwmanifest+xml",
				blogPostList.ResolveTemplateUrl(url, replace)
			);
		}
	}
</script>