<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.post.list/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogPostListTemplate" EnableViewState="false" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>
<%@ Import Namespace="Bitrix.Security" %>
<%@ Import Namespace="System.Collections.Generic" %>

<%	if (Component.FatalError != BlogPostListComponent.ErrorCode.None) { %>
<div class="blog-content">
	<div class="blog-note-box blog-note-error">
		<div class="blog-note-box-text">
			<%= Component.GetErrorHtml(Component.FatalError) %>
		</div>
	</div>
</div><%
	return;
}
else if (Component.Posts.Count < 1)
{
%>
	<div class="blog-content">
		<%= (Component.IsFilterSet ? GetMessage("PostsNotFoundByFilter") : Component.PostPublishMode == BlogPostListComponent.PublishMode.Draft ? GetMessage("DraftsNotFound") : GetMessage("PostsNotFound"))%>
	</div>
<%
	return;
} %>

<div class="blog-content">

	<div class="blog-posts blog-post-list">
		<div id="blog-posts-content">
			<% if (Component.Paging.IsTopPosition) { %>
			<div class="blog-navigation-box blog-navigation-top">
				<div class="blog-page-navigation">
					<bx:IncludeComponent runat="server" ID="HeaderPager" ComponentName="bitrix:system.pager"  Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="top" CssClassPrefix="blog-" />
				</div>
			</div>
			<% } %>
			<%
				BlogPostListComponent.PostInfo post = null;
				int cutNum = 0;	
				Component.RenderHideCut += delegate(object sender, BXBlogCutTagEventArgs e)
				{
					cutNum++;
					string title = !Bitrix.Services.Text.BXStringUtility.IsNullOrTrimEmpty(e.Option) ? e.Option.Trim() : GetMessage("CutDefaultTitle");
					e.Writer.Write(String.Concat(@"<a class=""blog-cut-link"" href=""", post.PostViewHref, "#cut", cutNum, @""">", Encode(title), "</a>"));
					
				};			
				StringBuilder postClasses = new StringBuilder();		
				for(int i = 0; i < Component.Posts.Count; i++)
				{
					cutNum = 0;
					post = Component.Posts[i];
					
					postClasses.Length = 0;
					if (i % 2 == 1)
						postClasses.Append(" blog-post-alt");
					if ((post.Post.Flags & BXBlogPostFlags.FromSyndication) != BXBlogPostFlags.None)
						postClasses.Append(" blog-post-from-syndication");
					DateTime date = post.Post.DatePublished;
					if (!post.Post.IsPublished || date > DateTime.Now)
						postClasses.Append(" blog-post-draft");
					postClasses.Append(" blog-post-year-");
					postClasses.Append(date.Year);
					postClasses.Append(" blog-post-month-");
					postClasses.Append(date.Month);
					postClasses.Append(" blog-post-day-");
					postClasses.Append(date.Day);
					if (i == 0)
						postClasses.Append(" blog-post-first");
					if (i == Component.Posts.Count - 1)
						postClasses.Append(" blog-post-last");
			%>
			<div class="blog-post<%= postClasses %>">
				<h2 class="blog-post-title"><a href="<%= post.PostViewHref %>" title="<%= post.Post.Title %>"><%= post.TitleHtml %></a></h2>
				<div class="blog-post-info-back blog-post-info-top">
					<div class="blog-post-info">
						<div class="blog-author">
							<a class="blog-author-icon" href="<%= post.UserProfileHref %>"></a><a class="" href="<%= post.BlogHref %>"><%= post.AuthorNameHtml %></a>
						</div>
					    <div class="blog-post-date" title="<%= Encode(post.Post.DatePublished.ToString("g")) %>">
							<span class="blog-post-day"><%= Encode(post.Post.DatePublished.ToString("d")) %></span> <span class="blog-post-time"><%= Encode(post.Post.DatePublished.ToString("t")) %></span>
						</div>
						<%--<% if (Component.SortByVotingTotals) {%>
						<div style="display:inline; line-height:1.4em; margin:0.1em 0; padding:0 0 0 0.3em;">Voting total: <%= post.VotingTotalValue.ToString("N0")%></div>
						<%} %>--%>

					</div>
				</div>
				<div class="blog-post-content">
					<% if (post.Post.Author != null && post.Post.Author.User != null && post.Post.Author.User.Image != null) { %>
					<div class="blog-post-avatar"><img alt="" width="<%= post.Post.Author.User.Image.Width %>" height="<%= post.Post.Author.User.Image.Height %>" src="<%= post.Post.Author.User.Image.FilePath %>" /></div>
					<% } %>
					<%= post.GetContentHtml() %>
					<div class="blog-clear-float"></div>
				</div>

				<div class="blog-post-meta">
					<div class="blog-post-info-back blog-post-info-bottom">
						<div class="blog-post-info">
							<div class="blog-author">
								<a class="blog-author-icon" href="<%= post.UserProfileHref %>"></a><a class="" href="<%= post.BlogHref %>"><%= post.AuthorNameHtml %></a>
							</div>
							<div class="blog-post-date" title="<%= Encode(post.Post.DatePublished.ToString("g")) %>">
								<span class="blog-post-day"><%= Encode(post.Post.DatePublished.ToString("d")) %></span> <span class="blog-post-time"><%= Encode(post.Post.DatePublished.ToString("t")) %></span>
							</div>
						</div>
					</div>

					<div class="blog-post-meta-util">
					    <% if (post.Post.EnableComments) { %>
						    <span class="blog-post-comments-link"><a href="<%= post.PostViewHref %>#comments" title="<%= GetMessage("CommentsLinkTitle") %>"><span class="blog-post-link-caption"><%= GetMessage("CommentsLinkTitle") %>:</span><span class="blog-post-link-counter"><%= post.Post.CommentCount %></span></a></span>
						    <%--<a class="blog-post-rss-link" href="<%= post.PostRssHref %>">RSS</a>--%>
					    <% } %>
						<span class="blog-post-views-link"><a href="<%= post.PostViewHref %>" title="<%= GetMessage("ViewsLinkTitle")%>"><span class="blog-post-link-caption"><%= GetMessage("ViewsLinkTitle")%>:</span><span class="blog-post-link-counter"><%= post.Post.ViewCount %></span></a></span>
						<% if (!String.IsNullOrEmpty(post.PostEditHref)) { %>	
						<span class="blog-post-edit-link"><a href="<%= post.PostEditHref %>" title="<%= GetMessage("EditLinkTitle")%>"><span class="blog-post-link-caption"><%= GetMessage("EditLinkTitle")%></span></a></span>
						<% } %>
						<% if (post.Auth.CanDeleteThisPost) { %>
						<% string delLinkClientID = Component.ClientID + ClientIDSeparator + "PostDel" + post.Post.Id; %>
						<span class="blog-post-delete-link"><a id="<%= delLinkClientID %>" href="<%= Component.GetOperationHref("delete", post.Post.Id, false) %>" onclick="return confirm('<%= Encode(JSEncode(GetMessage("DeleteConfirmMessage"))) %>')" rel="nofollow" title="<%= GetMessage("DeleteLinkTitle")%>"><span class="blog-post-link-caption"><%= GetMessage("DeleteLinkTitle")%></span></a></span>
						<script type="text/javascript">
							(function() { document.getElementById("<%= delLinkClientID %>").href += "&" + dotNetVars.securityTokenPair; })();
						</script>
						<% } %>
					</div>
					
					<% if (post.Tags.Count != 0) { %>
					<div class="blog-post-tag">
						<span><%= GetMessage("Tags") %>: </span>
						<% bool first = true; %>
						<% 
						foreach(BlogPostListComponent.TagInfo tag in post.Tags) 
						{ 
							if (!first) { %>, <% } else first = false;
							if (!string.IsNullOrEmpty(tag.TagHref)) { 
								%><a href="<%= tag.TagHref %>"><%= tag.TagHtml %></a><% 
							} else { 
								%><%= tag.TagHtml %><% 
							} 
						} 
						%>
					</div>
					<% } %>
					<%if (Component.EnableVotingForPost) { %>
						<div id="<%= Component.ClientID + ClientIDSeparator + "PostVoting" + post.Post.Id %>" class="blog-post-voting"></div>	
					<%} %>						
				</div>
			</div>
			<%	} %>
			<%if (Component.Paging.IsBottomPosition) {%>
			<div class="blog-navigation-box blog-navigation-bottom">
				<div class="blog-page-navigation">
					<bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="bottom" CssClassPrefix="blog-" />
				</div>
			</div>
			<%} %>
		</div>
	</div>
	<div class="blog-clear-float">
	</div>
</div>

<script	runat="server">
    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
		
		RequiresEpilogue = Component.EnableVotingForPost;
    }    
</script>
