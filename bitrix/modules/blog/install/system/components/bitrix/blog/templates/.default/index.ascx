<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog/component.ascx" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.menu/component.ascx" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.posts/component.ascx" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.list/component.ascx" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.comments/component.ascx" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<%@ Import Namespace="Bitrix.Services" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogTemplate" %>

<script runat="server">

	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);

		if (!Bitrix.Security.BXPrincipal.Current.IsCanOperate(BXBlog.Operations.BlogPublicView))
		{
			Bitrix.Security.BXAuthentication.AuthenticationRequired();
			return;
		}
        
        if (Bitrix.Modules.BXModuleManager.IsModuleInstalled("Search")) 
        {
            TagCloud.Controls.Add(LoadControl("index-tagcloud.ascx"));  
        } 
	}

    private string rssAllPostsTemplate = null;
    protected string RssAllPostsTemplate
    {
        get
        {
            if (rssAllPostsTemplate != null)
                return rssAllPostsTemplate;

            rssAllPostsTemplate = string.Empty;
            object o;
            if (Component.ComponentCache.TryGetValue("RssAllPostsTemplate", out o))
                rssAllPostsTemplate = (string)o;

            return rssAllPostsTemplate;                            
        } 
    }

    protected string ResolveTemplateUrl(string template, BXParamsBag<object> parameters)
    {
        string r = (parameters != null) ? BXSefUrlUtility.MakeLink(template, parameters, null) : template;
        if (r.StartsWith("~/"))
            r = ResolveUrl(r);
        return r;
    }    
    
    private string rssAllPostsUrl = null;
    protected string RssAllPostsUrl
    {
        get 
        {
            return rssAllPostsUrl ?? (rssAllPostsUrl = ResolveTemplateUrl(RssAllPostsTemplate, null));
        } 
    }
    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);
        if (!string.IsNullOrEmpty(RssAllPostsTemplate))
            BXPage.RegisterLink(
                "alternate",
                "application/rss+xml",
                RssAllPostsUrl,
                new KeyValuePair<string, string>("title", GetMessageRaw("HeadTitle.RssAllPosts"))
            );
    }
		
</script>
<div class="blog-content">
<div class="blog-mainpage">

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


		<% if (((BlogMenuComponent)menu.Component).FatalError != BlogMenuComponent.ErrorCode.None) { %>
		<div class="blog-mainpage-create-blog">
			<a href="<%= Results["NewBlogUrlTemplate"] %>" class="blog-author-icon"></a>&nbsp;<a href="<%= Results["NewBlogUrlTemplate"] %>"><%= GetMessage("CreateNewBlog") %></a>
		</div>
		<%} %>
<%} %>
<% if (((BlogListComponent)newblogs.Component).HasItems)
   { %>

		<script type="text/javascript">
		<!--
		function BXBlogTabShow(id, type)
		{
			if(type == 'post')
			{
				
				document.getElementById('new-posts').style.display = 'inline';
				document.getElementById('popular-posts').style.display = 'inline';
				document.getElementById('commented-posts').style.display = 'inline';
				
				document.getElementById('new-posts-title').style.display = 'none';
				document.getElementById('popular-posts-title').style.display = 'none';
				document.getElementById('commented-posts-title').style.display = 'none';
				
				document.getElementById('new-posts-content').style.display = 'none';
				document.getElementById('popular-posts-content').style.display = 'none';
				document.getElementById('commented-posts-content').style.display = 'none';

				document.getElementById(id).style.display = 'none';
				document.getElementById(id+'-title').style.display = 'inline';
				document.getElementById(id+'-content').style.display = 'block';
			}
			else if(type == 'blog')
			{
				document.getElementById('new-blogs').style.display = 'inline-block';
				document.getElementById('popular-blogs').style.display = 'inline-block';
				
				document.getElementById('new-blogs-title').style.display = 'none';
				document.getElementById('popular-blogs-title').style.display = 'none';
				
				document.getElementById('new-blogs-content').style.display = 'none';
				document.getElementById('popular-blogs-content').style.display = 'none';

				document.getElementById(id).style.display = 'none';
				document.getElementById(id+'-title').style.display = 'inline-block';
				document.getElementById(id+'-content').style.display = 'block';
			}
			
			return false;
		}
		//-->
		</script>
		<div class="blog-mainpage-side-left">
			<div class="blog-tab-container">
				<div class="blog-tab-left"></div>
				<div class="blog-tab-right"></div>
				<div class="blog-tab">
					<div class="blog-tab-title">
						<span id="new-posts-title"><%= GetMessage("NewPostsTitleFull")%></span>
						<span id="commented-posts-title" style="display:none;"><%= GetMessage("CommentedPostsTitleFull")%></span>
						<span id="popular-posts-title" style="display:none;"><%= GetMessage("PopularPostsTitleFull")%></span>
					</div>		
					<div class="blog-tab-items">
						<span id="new-posts" style="display:none;"><a href="<%= Results["NewPostListUrlTemplate"] %>" onclick="return BXBlogTabShow('new-posts', 'post');"><%= GetMessage("NewPostsTitle")%></a></span>
						<span id="commented-posts"><a href="<%= Results["DiscussPostListUrlTemplate"] %>" onclick="return BXBlogTabShow('commented-posts', 'post');"><%= GetMessage("CommentedPostsTitle")%></a></span>
						<span id="popular-posts"><a href="<%= Results["PopularPostListUrlTemplate"] %>" onclick="return BXBlogTabShow('popular-posts', 'post');"><%= GetMessage("PopularPostsTitle") %></a></span>
					</div>
				</div>	
			</div>
			<div class="blog-clear-float"></div>
			<div class="blog-tab-content">
				<div id="new-posts-content" style="display:block;">

					<bx:IncludeComponent
						ID="newposts" 
						runat="server" 
						ComponentName="bitrix:blog.posts" 
						Template="main.page"
						ThemeCssFilePath="" 
						ColorCssFilePath=""
						CategoryId="<%$Parameters:CategoryId %>"  
						Sortby="ByDate" 
						BlogUrlTemplate="<%$ Results:PostListUrlTemplate %>" 
						UserProfileurltemplate="<%$ Results:UserProfileUrlTemplate %>" 
						PostViewUrlTemplate="<%$ Results:PostUrlTemplate %>" 
						PostRssUrlTemplate="<%$ Results:RssBlogPostCommentsUrlTemplate %>"
						Pagingallow="False" 
						PagingRecordsPerPage="<%$ Results:MainPagePostCount %>"
						SetPageTitle="False"
					/>
					<% if (((BlogPostsComponent)newposts.Component).HasPosts) { %>
					<a href="<%= Results["NewPostListUrlTemplate"] %>"><%= GetMessage("AllLastPosts") %></a>
					<%} %>					
				</div>
				<div id="commented-posts-content" style="display:none;">
					<bx:IncludeComponent
						ID="commentedposts" 
						runat="server" 
						ComponentName="bitrix:blog.posts" 
						Template="main.page"
						ThemeCssFilePath="" 
						ColorCssFilePath=""
						CategoryId="<%$Parameters:CategoryId %>"   
						Sortby="ByComments" 
						BlogUrlTemplate="<%$ Results:PostListUrlTemplate %>" 
						UserProfileurltemplate="<%$ Results:UserProfileUrlTemplate %>" 
						PostViewUrlTemplate="<%$ Results:PostUrlTemplate %>" 
						PostRssUrlTemplate="<%$ Results:RssBlogPostCommentsUrlTemplate %>"
						Pagingallow="False" 
						PagingRecordsPerPage="<%$ Results:MainPagePostCount %>" 	
						SetPageTitle="False"
					/>
					<% if (((BlogPostsComponent)commentedposts.Component).HasPosts) { %>
					<a href="<%= Results["DiscussPostListUrlTemplate"] %>"><%= GetMessage("AllDiscussedPosts") %></a>
					<%} %>					
				</div>
				<div id="popular-posts-content" style="display:none;">
					<bx:IncludeComponent
						ID="popularposts" 
						runat="server" 
						ComponentName="bitrix:blog.posts" 
						Template="main.page"
						ThemeCssFilePath="" 
						ColorCssFilePath=""
						CategoryId="<%$Parameters:CategoryId %>"   
						Sortby="<%$ Results:PopularPostsSortBy %>" 
						BlogUrlTemplate="<%$ Results:PostListUrlTemplate %>" 
						UserProfileurltemplate="<%$ Results:UserProfileUrlTemplate %>" 
						PostViewUrlTemplate="<%$ Results:PostUrlTemplate %>" 
						PostRssUrlTemplate="<%$ Results:RssBlogPostCommentsUrlTemplate %>"
						Pagingallow="False" 
						PagingRecordsPerPage="<%$ Results:MainPagePostCount %>" 
						SetPageTitle="False"
					/>
					<% if (((BlogPostsComponent)popularposts.Component).HasPosts) { %>
					<a href="<%= Results["PopularPostListUrlTemplate"] %>"><%= GetMessage("AllPopularPosts") %></a>
					<%} %>				
				</div>
			</div>

		</div>
		<div class="blog-mainpage-side-right">
		    <% if (Bitrix.Modules.BXModuleManager.IsModuleInstalled("Search")) { %>
		    <div class="blog-tab-container">
				<div class="blog-tab-left"></div>
				<div class="blog-tab-right"></div>
				<div class="blog-tab"><span class="blog-tab-title"><%= GetMessage("TagsCloudTitleFull") %></span></div>	
			</div>
			<div class="blog-tab-content">
				<asp:PlaceHolder runat="server" ID="TagCloud" />
			</div>
			<% } %>
			<% if (((BlogCommentsComponent)lastcomments.Component).HasComments) { %>
			<div class="blog-tab-container">
				<div class="blog-tab-left"></div>
				<div class="blog-tab-right"></div>
				<div class="blog-tab"><span class="blog-tab-title"><%= GetMessage("NewCommentsTitle") %></span></div>	
			</div>
			<div class="blog-tab-content">
				<bx:IncludeComponent 
					ID="lastcomments" 
					runat="server" 
					ComponentName="bitrix:blog.comments" 
					Template="main.page"
					ThemeCssFilePath="" 
					ColorCssFilePath=""
					CategoryId="<%$Parameters:CategoryId %>" 
					BlogUrltemplate="<%$ Results:PostListUrlTemplate %>" 
					UserProfileUrlTemplate="<%$ Results:UserProfileUrlTemplate %>" 
					PostViewUrlTemplate="<%$ Results:PostUrlTemplate %>" 
					PagingAllow="False" 
					PagingRecordsPerPage="<%$ Results:MainPagePostCount %>" 
					CommentUrlTemplate="<%$ Results:CommentReadUrlTemplate %>"
					SetPageTitle="False"
				/> 
			
			</div>
			<%} %>
			<div class="blog-tab-container">
				<div class="blog-tab-left"></div>
				<div class="blog-tab-right"></div>
				<div class="blog-tab">
					<span class="blog-tab-items">
						<span id="new-blogs" style="display:none;"><a href="#" onclick="return BXBlogTabShow('new-blogs', 'blog');"><%= GetMessage("NewBlogsTitle") %></a></span>
						<span id="popular-blogs"><a href="#" onclick="return BXBlogTabShow('popular-blogs', 'blog');"><%= GetMessage("CommentedBlogsTitle")%></a></span>
					</span>
					<span class="blog-tab-title">
						<span id="new-blogs-title"><%= GetMessage("NewBlogsTitleFull")%></span>
						<span id="popular-blogs-title" style="display:none;"><%= GetMessage("CommentedBlogsTitleFull")%></span>
					</span>
				</div>	
			</div>
			<div class="blog-tab-content">
				<div id="new-blogs-content">
					<bx:IncludeComponent 
						ID="newblogs" 
						runat="server" 
						ComponentName="bitrix:blog.list" 
						Template="main.page"
						ThemeCssFilePath="" 
						ColorCssFilePath="" 
						CategoryId="<%$Parameters:CategoryId %>" 
						SortByFirst="DateCreated" 
						SortOrderFirst="Desc" 
						SortBySecond="Name" 
						SortOrderSecond="Asc" 
						BlogPageUrlTemplate="<%$ Results:PostListUrlTemplate %>" 
						BlogOwnerProfilePageUrlTemplate="<%$ Results:UserProfileUrlTemplate %>"
						Pagingallow="False" 
						PagingRecordsPerPage="<%$ Results:MainPageBlogCount %>"
						SetPageTitle="False" 
					/>				
				</div>
				<div id="popular-blogs-content" style="display:none;">
					<bx:IncludeComponent 
						ID="commentedblogs" 
						runat="server" 
						ComponentName="bitrix:blog.list" 
						Template="main.page"
						ThemeCssFilePath="" 
						ColorCssFilePath="" 
						CategoryId="<%$Parameters:CategoryId %>" 
						SortByFirst="PostCount" 
						SortOrderFirst="Desc" 
						SortBySecond="Name" 
						SortOrderSecond="Asc" 
						Pagingallow="False" 
						PagingRecordsPerPage="<%$ Results:MainPageBlogCount %>" 
						BlogPageUrlTemplate="<%$ Results:PostListUrlTemplate %>" 
						BlogOwnerProfilePageUrlTemplate="<%$ Results:UserProfileUrlTemplate %>"
						SetPageTitle="False" 
					/>
				</div>
			</div>
			<div class="blog-tab-container">
                <div class="blog-tab-left"></div>
				<div class="blog-tab-right"></div>
				<div class="blog-tab blog-rss-subscribe-tab">
				    <span class="blog-tab-items">
				        <span id="blogs-rss">
				            <a class="blog-rss-icon" href="<%= RssAllPostsUrl %>" title="<%= HttpUtility.HtmlAttributeEncode(GetMessageRaw("Hint.RssAllPosts")) %>"></a>
				        </span>
				    </span>
				    <span class="blog-tab-title"><a href="<%= RssAllPostsUrl %>" title="<%= HttpUtility.HtmlAttributeEncode(GetMessageRaw("Hint.RssAllPosts")) %>"><%= HttpUtility.HtmlEncode(GetMessageRaw("Title.RssAllPosts")) %></a></span>
				</div>
			</div>
			<div class="blog-tab-content">
			</div>
		</div>
		<div class="blog-clear-float"></div>
		<div class="blog-tab-container">
				<div class="blog-tab-left"></div>
				<div class="blog-tab-right"></div>
				<div class="blog-tab"><span class="blog-tab-title"><%= GetMessage("CategoriesTitle") %></span></div>	
			</div>
		<bx:IncludeComponent 
					ID="categories" 
					runat="server" 
					ComponentName="bitrix:blog.category.list" 
					Template=".default"
					ThemeCssFilePath="" 
					ColorCssFilePath=""
					SortByFirst="Sort" 
					SortOrderFirst="Asc" 
					SortBySecond="Name" 
					SortOrderSecond="Asc" 
					DisplayLimit="0" 
					ColumnCount="2" 
					BlogCategoryPageUrlTemplate="<%$ Results:CategoryBlogListUrlTemplate %>"
					SetPageTitle="False" 
				/>
		<a href="<%= Results["BlogListUrlTemplate"] %>"><%= GetMessage("AllBlogs") %></a>
	<%} %>
	</div>
</div>
