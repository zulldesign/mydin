<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.posts/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogPostsTemplate" EnableViewState="false" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>
<%@ Import Namespace="System.Collections.Generic" %>

<%	if (Component.FatalError != BlogPostsComponent.ErrorCode.None) { %>
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
	<%= GetMessage("PostsNotFound") %>
<%
	return;
} %>

<%
	for(int i = 0; i < Component.Posts.Count; i++)
	{
		BlogPostsComponent.PostInfo post = Component.Posts[i];
%>
		<%if (i > 0) {%>
		<div class="blog-line"></div>
		<%} %>
		
		<div class="blog-mainpage-item">
			<div class="blog-author"><a class="blog-author-icon" href="<%= post.UserProfileHref %>" title=""></a><a href="<%= post.BlogHref %>"><%= post.AuthorNameHtml %></a></div>
			<div class="blog-clear-float"></div>
			<div class="blog-mainpage-title"><a href="<%= post.PostViewHref %>" title="<%= post.Post.Title %>"><%= post.TitleHtml %></a></div>
			<div class="blog-mainpage-content"><%= post.GetPreviewHtml(100)%></div>
			<div class="blog-mainpage-meta">
			<a href="<%= post.PostViewHref %>" title="<%= GetMessage("DatePublishedTitle") %>"><%= post.Post.DatePublished.ToString("g") %></a>
			<% if (post.Post.ViewCount > 0) { %>
			<span class="blog-vert-separator">|</span> <a href="<%= post.PostViewHref %>" title="<%= GetMessage("ViewsLinkTitle")%>"><%= GetMessage("ViewsLinkTitle")%>:&nbsp;<%= post.Post.ViewCount %></a>
			<% } %>
			<% if (post.Post.CommentCount > 0) { %>
			<span class="blog-vert-separator">|</span> <a href="<%= post.PostViewHref %>#comments" title="<%= GetMessage("CommentsLinkTitle")%>"><%= GetMessage("CommentsLinkTitle")%>:&nbsp;<%= post.Post.CommentCount %></a>
			<%} %>
			</div>
			<div class="blog-clear-float"></div>
		</div>
<%	} %>


<script runat="server">

	public override string Title
	{
		get
		{
			return GetMessageRaw("Title");
		}
	}

</script>