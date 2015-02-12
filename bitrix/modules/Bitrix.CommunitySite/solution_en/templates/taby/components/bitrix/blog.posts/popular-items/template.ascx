<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.posts/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogPostsTemplate" EnableViewState="false" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>
<%@ Import Namespace="System.Collections.Generic" %>

<%	if (Component.FatalError != BlogPostsComponent.ErrorCode.None) { %>
	<div class="errortext"><%= Component.GetErrorHtml(Component.FatalError) %></div>
<%
	return;
}
else if (Component.Posts.Count < 1)
{
	return;
} %>

<div class="rounded-block">
	<div class="corner left-top"></div><div class="corner right-top"></div>
	<div class="block-content">

		<h3><%= GetMessage("Title") %></h3>

		<ul class="last-items-list">
		<% 	for(int i = 0; i < Component.Posts.Count; i++)
		{
			var post = Component.Posts[i];%>
			<li><a class="item-author" href="<%=post.BlogHref%>">
			<%= post.AuthorNameHtml %>
			</a> <i>&gt;</i> 
			<a class="item-name" title="<%=post.TitleHtml%>"
			href="<%=post.PostViewHref%>"><%=post.TitleHtml%></a>
			</li>
			<%} %>
		</ul>
	</div>
	
	<div class="corner left-bottom"></div><div class="corner right-bottom"></div>
</div>	