<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.posts/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogPostsTemplate" EnableViewState="false" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>

<%	if (Component.FatalError != BlogPostsComponent.ErrorCode.None) { %>
<div class="error">
<%= Component.GetErrorHtml(Component.FatalError) %>
</div>
<%	return; %>
<% } %>

<% if (Component.Posts.Count > 0) { %>
	<dl class="block-list">
	<% foreach(var post in Component.Posts) { %>
		<dt><%= Encode(post.Post.DatePublished.ToString("g")) %></dt>	
		<dd><a href="<%= post.PostViewHref %>"><%= post.TitleHtml %></a></dd>
	<% } %>
	</dl>
<% } %>