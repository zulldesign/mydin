<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.comments/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogCommentsTemplate" EnableViewState="false" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>

<%	if (Component.FatalError != BlogCommentsComponent.ErrorCode.None || Component.Comments.Count < 1) 
		return;
%>

<%
	for (int i = 0; i < Component.Comments.Count; i++)
	{
		BlogCommentsComponent.CommentInfo comment = Component.Comments[i];
%>
		<%if (i > 0) {%>
		<div class="blog-line"></div>
		<%} %>
		
		<div class="blog-mainpage-item">
		
		<div class="blog-author">
			<% if (!String.IsNullOrEmpty(comment.AuthorBlogHref)) { %>
				<a class="blog-author-icon" href="<%= comment.UserProfileHref %>"></a><a href="<%= comment.AuthorBlogHref %>"><%= comment.AuthorNameHtml%></a>
			<% } else { %>   
				<span class="blog-author-icon"></span><%= comment.AuthorNameHtml%>
			<% } %>
			
			<span class="blog-comment-date" title="<%= comment.Comment.DateCreated %>"><%= comment.Comment.DateCreated.ToString("g") %></span> <a href="<%= comment.CommentHref %>" title="<%= GetMessage("CommentLinkTitle") %>">#</a>
		</div>

		<div class="blog-clear-float"></div>
		<div class="blog-mainpage-content">
			<%= comment.GetPreviewHtml(100)%>
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