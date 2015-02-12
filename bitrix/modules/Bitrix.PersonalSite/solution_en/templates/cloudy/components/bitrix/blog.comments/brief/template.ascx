<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.comments/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogCommentsTemplate" EnableViewState="false" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>


<%	if (Component.FatalError != BlogCommentsComponent.ErrorCode.None) { %>
<div class="error">
<%= Component.GetErrorHtml(Component.FatalError) %>
</div>
<%	return; %>
<% } %>

<% if (Component.Comments.Count > 0) { %>
	<dl class="block-list">
	<% foreach(var comment in Component.Comments) { %>
		<dt><%= Encode(comment.Comment.DateCreated.ToString("g")) %></dt>	
		<dd><a href="<%= comment.CommentHref %>"><%= comment.GetPreviewHtml(100)%></a></dd>
	<% } %>
	</dl>
<% } %>