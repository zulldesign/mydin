<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.post/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogPostTemplate" %>
<%@ Import Namespace="Bitrix.Services"%>
<%@ Import Namespace="Bitrix.Security"%>
<%@ Import Namespace="Bitrix.Blog"%>
<%@ Import Namespace="Bitrix.UI"%>
<%@ Import Namespace="Bitrix.Blog.Components"%>
<% if (Component.FatalError != BlogPostComponent.ErrorCode.None) { %>
	<div class="blog-content">
		<div class="blog-note-box blog-note-error">
			<div class="blog-note-box-text"><%= Component.GetErrorHtml(Component.FatalError) %></div>
		</div>
	</div>
	<% return; %>
<% } %>
<%
	StringBuilder postClasses = new StringBuilder();
	if ((Component.Post.Post.Flags & BXBlogPostFlags.FromSyndication) != BXBlogPostFlags.None)
		postClasses.Append(" blog-post-from-syndication");
	DateTime date = Component.Post.Post.DatePublished;
	if (!Component.Post.Post.IsPublished || date > DateTime.Now)
		postClasses.Append(" blog-post-draft");
	postClasses.Append(" blog-post-year-");
	postClasses.Append(date.Year);
	postClasses.Append(" blog-post-month-");
	postClasses.Append(date.Month);
	postClasses.Append(" blog-post-day-");
	postClasses.Append(date.Day);
%>
<div class="blog-content">
<div class="blog-post blog-post-detail<%= postClasses %>">
	<h2 class="blog-post-title"><span><%= Component.Post.TitleHtml %></span></h2>
	<div class="blog-post-info-back blog-post-info-top">
		<div class="blog-post-info">
			<div class="blog-author">
				<a class="blog-author-icon" href="<%= Component.Post.UserProfileHref %>"></a><a href="<%= Component.Post.BlogHref %>"><%= Component.Post.AuthorNameHtml %></a></div>
			<div class="blog-post-date" title="<%= Encode(Component.Post.Post.DatePublished.ToString("g")) %>">
				<span class="blog-post-day"><%= Encode(Component.Post.Post.DatePublished.ToString("d")) %></span> <span class="blog-post-time"><%= Encode(Component.Post.Post.DatePublished.ToString("t")) %></span>
			</div>
		</div>
	</div>
	<div class="blog-post-content">
		<% if (Component.Post.Post.Author != null && Component.Post.Post.Author.User != null && Component.Post.Post.Author.User.Image != null) { %>
		<div class="blog-post-avatar"><img alt="" width="<%= Component.Post.Post.Author.User.Image.Width %>px" height="<%= Component.Post.Post.Author.User.Image.Height %>px" src="<%= Component.Post.Post.Author.User.Image.FilePath %>" /></div>
		<% } %>
		<%
	int i = 1;	
	Component.RenderBeginCut += delegate(object sender, BXBlogCutTagEventArgs e)
	{
		e.Writer.Write(@"<a class=""blog-cut-anchor"" name=""cut");
		e.Writer.Write(i++);
		e.Writer.WriteLine(@"""></a>");
	};	
	Component.Post.RenderPostContent(CurrentWriter); 
		%>
		<div class="blog-clear-float"></div>
	</div>
	

	
	<div class="blog-post-meta">

		<div class="blog-post-info-back blog-post-info-bottom">
			<div class="blog-post-info">
				<div class="blog-author">
					<a class="blog-author-icon" href="<%= Component.Post.UserProfileHref %>"></a><a href="<%= Component.Post.BlogHref %>"><%= Component.Post.AuthorNameHtml %></a></div>
				<div class="blog-post-date" title="<%= Encode(Component.Post.Post.DatePublished.ToString("g")) %>">
					<span class="blog-post-day"><%= Encode(Component.Post.Post.DatePublished.ToString("d")) %></span> <span class="blog-post-time"><%= Encode(Component.Post.Post.DatePublished.ToString("t")) %></span>
				</div>
			</div>
		</div>

		<div class="blog-post-meta-util">
		    <% if (Component.Post.Post.EnableComments) { %>
			<span class="blog-post-comments-link"><a href="<%= Component.Post.PostViewHref %>#comments" title="<%= GetMessage("Comments") %>"><span class="blog-post-link-caption"><%= GetMessage("Comments") %>:</span><span class="blog-post-link-counter"><%= Component.Post.Post.CommentCount %></span></a></span>
			<span class="blog-post-rss-link"><a href="<%= Component.Post.RssHref %>" title="<%= GetMessage("CommentsRss") %>">RSS</a></span>
		    <%} %>
			<span class="blog-post-views-link"><a href="<%= Component.Post.PostViewHref %>" title="<%= GetMessage("Views") %>"><span class="blog-post-link-caption"><%= GetMessage("Views") %>:</span><span class="blog-post-link-counter"><%= Component.Post.Post.ViewCount %></span></a></span>
			<% if (Component.Auth.CanEditThisPost) { %>
			<span class="blog-post-edit-link"><a href="<%= Component.Post.PostEditHref %>" rel="nofollow" title="<%= GetMessage("Kernel.Edit") %>"><span class="blog-post-link-caption"><%= GetMessage("Kernel.Edit") %></span></a></span>
			<% } %>
			<% if (Component.Auth.CanDeleteThisPost) { %>
			<span class="blog-post-delete-link"><a href="<%= GetOperationHref(DeleteOperation) %>" onclick="return confirm('<%= Encode(JSEncode(GetMessageRaw("Confirm.Delete"))) %>');" rel="nofollow" title="<%= GetMessage("Kernel.Delete") %>"><span class="blog-post-link-caption"><%= GetMessage("Kernel.Delete") %></span></a></span>
			<% } %>
		</div>
		
		<% if (Component.Post.Tags.Count != 0) { %>
		<div class="blog-post-tag">
			<span><%= GetMessage("Tags") %>: </span> 
			<% 
			bool first = true;
			foreach(BlogPostComponent.TagInfo tag in Component.Post.Tags) 
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
		<% if (Component.EnableVotingForPost) {%>
		<div class="blog-post-voting">
            <bx:IncludeComponent 
	            id="voting" 
	            runat="server" 
	            componentname="bitrix:rating.vote" 
	            Template=".default" 
	            BoundEntityTypeId="BlogPost" 
	            BoundEntityId="<%$ Parameters:PostId %>" 
	            CustomPropertyEntityId = "BlogPost"
	            RolesAuthorizedToVote="<%$ Parameters:RolesAuthorizedToVote %>"
	            BannedUsers = "<%$ Results:UsersBannedToVote %>" />		    
	    </div>
	    <%} %>
	</div>
</div>
</div>
<script runat="server">
	private const string OperationParameter = "_action";
	private const string SourceParameter = "_source";
	private const string DeleteOperation = "delete-post";
	
	string GetOperationHref(string operation)
	{
		NameValueCollection query = HttpUtility.ParseQueryString(BXSefUrlManager.CurrentUrl.Query);
		query.Set(SourceParameter, ClientID.GetHashCode().ToString());
		query.Set(OperationParameter, operation);
		BXCsrfToken.SetToken(query);

		UriBuilder uri = new UriBuilder(BXSefUrlManager.CurrentUrl);
		uri.Query = query.ToString();
		return Encode(uri.Uri.ToString());
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		voting.Visible = Component.EnableVotingForPost;
		if (Request == null)
			return;
		if (Request.QueryString[OperationParameter] != DeleteOperation)
			return;
		if (Request.QueryString[SourceParameter] != ClientID.GetHashCode().ToString())
			return;
		if (!BXCsrfToken.CheckTokenFromRequest(Request.QueryString))
			return;

		Component.Delete();
	}
</script>
