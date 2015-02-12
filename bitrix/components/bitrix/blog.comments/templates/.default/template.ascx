<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.comments/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogCommentsTemplate" EnableViewState="false" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>
<%@ Import Namespace="System.Collections.Generic" %>

<%	if (Component.FatalError != BlogCommentsComponent.ErrorCode.None) { %>
<div class="blog-content">
	<div class="blog-note-box blog-note-error">
		<div class="blog-note-box-text">
			<%= Component.GetErrorHtml(Component.FatalError) %>
		</div>
	</div>
</div><%
	return;
}
else if (Component.Comments.Count < 1)
{
%>
	<div class="blog-content">
	<div class="blog-note-box blog-note-success">
		<div class="blog-note-box-text"><%= GetMessage("PostsNotFound") %></div>
	</div>
</div>
<%
	return;
} %>

<div class="blog-content">

	<div class="blog-posts">
		<div id="blog-posts-content">
			<%	if (Component.Paging.IsTopPosition) 
				{ 
			%>
			<div class="blog-navigation-box blog-navigation-top">
				<div class="blog-page-navigation">
					<bx:IncludeComponent runat="server" ID="HeaderPager" ComponentName="bitrix:system.pager"  Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="top" CssClassPrefix="blog-" />
				</div>
			</div>
			<%
				}	
				for (int i = 0; i < Component.Comments.Count; i++)
				{
					BlogCommentsComponent.CommentInfo comment = Component.Comments[i];
			%>
			<div class="blog-post">
				<h2 class="blog-post-title"><a href="<%= comment.PostViewHref %>"><%= comment.TitleHtml%></a></h2>
				<div class="blog-post-info-back">
					<div class="blog-post-info">
						<div class="blog-author">
							<a class="blog-author-icon" href="<%= comment.UserProfileHref %>"></a><a href="<%= comment.AuthorBlogHref %>"><%= comment.AuthorNameHtml%></a>
						</div>
						<div class="blog-post-date" title="<%= comment.Post.DatePublished.ToString("g")%>"><%= comment.Post.DatePublished%></div>
					</div>
				</div>
				<div class="blog-post-content">
					<% if (comment.Author != null && comment.Author.User != null && comment.Author.User.Image != null){ %>
					<div class="blog-post-avatar"><img alt="" width="<%= comment.Author.User.Image.Width %>" height="<%= comment.Author.User.Image.Height %>" src="<%= comment.Author.User.Image.FilePath %>" /></div>
					<% } %>
					<%= comment.ContentHtml%>
				</div>
			</div>
			<%	} 
				if (Component.Paging.IsBottomPosition) 
				{		
			%>
			<div class="blog-navigation-box blog-navigation-bottom">
				<div class="blog-page-navigation">
					<bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="bottom" CssClassPrefix="blog-" />
				</div>
			</div>
			<%	} %>
		</div>
	</div>
	<div class="blog-clear-float"></div>
</div>