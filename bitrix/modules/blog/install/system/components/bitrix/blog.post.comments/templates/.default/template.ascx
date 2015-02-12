<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.post.comments/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogPostCommentsTemplate" EnableViewState="false" %>
<%@ Register TagPrefix="bx" TagName="CommentForm" Src="~/bitrix/controls/Blog/CommentForm.ascx" %>
<%@ Register TagPrefix="bx" Assembly="CommunicationUtility"  Namespace="Bitrix.CommunicationUtility" %>
<%@ Import Namespace="Bitrix.Services" %>
<%@ Import Namespace="Bitrix.Security" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.UI" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>
<%@ Import Namespace="System.Collections.Generic" %>

<%@ Import Namespace="Bitrix.Blog.Components" %>
<% if (Component.FatalError != BlogPostCommentsComponent.ErrorCode.None) { %>
	<div class="blog-content">
		<div class="blog-note-box blog-note-error">
			<div class="blog-note-box-text">
				<%= Component.GetErrorHtml(Component.FatalError) %>
			</div>
		</div>
	</div>
	<% return; %>
<% } %>


<% if (Component.EnableComments) { %>
<script type="text/javascript">
	function <%= ClientID %>_ShowForm(div, commentId)
	{
		var d = div.nextSibling;
		while (d != null)
		{
			if (d.tagName == "DIV" && d.className == "blog-comment-form")
			{
				d.style.display = "block";
				<%= Comment.GetAllocateScript("d", "commentId", true) %>
				<%= Comment.GetFocusScript() %>
				break;
			}
			d = d.nextSibling;
		}
	}
</script>
<%} %>
<div class="blog-content">
<div class="blog-comments" id="comments">
	<%if (Component.Paging.IsTopPosition) {%>
	<div class="blog-navigation-box blog-navigation-top">
		<div class="blog-page-navigation">
			<bx:IncludeComponent runat="server" ID="HeaderPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="top" CssClassPrefix="blog-" />
		</div>
	</div>
	<%} %>
	
	<% if (!Component.EnableComments && Component.IsSyndicatedPost) { %>
	    	<div class="blog-add-comment blog-add-comment-top"><a target="_blank" href="<%= Component.SyndicatedPostCommentsUrl %>"><b><%= GetMessage("GoToOriginalPostComments") %></b></a></div>
	<%} %>
	
	<% if (Component.EnableComments && Component.Auth.CanCommentThisPost) { %>
	    <div class="blog-add-comment blog-add-comment-top"><a onclick="<%= ClientID %>_ShowForm(this.parentNode, 0);" href="javascript:void(0)"><b><%= GetMessage("AddComment") %></b></a></div>
	    <div class="blog-comment-form"></div>
	<% } %>
	
	<% int hiddenCommentId =  Component.HiddenCommentId; %>
	<% if(hiddenCommentId > 0) { %> 
	<div id="comment<%= hiddenCommentId.ToString() %>" class="blog-comment-should-be-approved">
		<div class="blog-comment-should-be-approved-cont">
			<%= GetMessage("CommentShouldBeApproved") %>
		</div>
	</div>					
	<% Component.HiddenCommentId = 0; %>
	<%} %>	
	
	<%
	float padding = 2.5f;
	bool hasComments = false;
    BlogPostCommentsComponent.CommentInfo previousRootComment = null;
    IncludeComponent voting = null;     
    foreach (BlogPostCommentsComponent.CommentInfo comment in Component.Comments) 
    { 
		if (!comment.BranchIsDeleted)
		{
			hasComments = true;
	%>
		<div id="comment<%= comment.Comment.Id %>" class="blog-comment" style="padding-left: <%= ((comment.Comment.DepthLevel - 1) * padding).ToString(".##", System.Globalization.NumberFormatInfo.InvariantInfo) %>em;">
	<%
			if (!comment.Comment.MarkedForDelete && (comment.Comment.IsApproved || comment.Auth.CanApproveThisComment))
			{
	%>
			<div class= "blog-comment-cont<%= !comment.Comment.IsApproved ? " blog-comment-hidden" : ""%>">
				<div class="blog-comment-cont-white">
					<div class="blog-comment-info">
					
						<% if (comment.Comment.Author != null && comment.Comment.Author.User != null && comment.Comment.Author.User.Image != null) { %>
						<div class="blog-comment-avatar" width="<%= comment.Comment.Author.User.Image.Width %>" height="<%= comment.Comment.Author.User.Image.Height %>" style="background:url('<%= HttpUtility.HtmlAttributeEncode(comment.Comment.Author.User.Image.FilePath) %>') no-repeat center center;"></div>
						<% } else if (comment.Comment.Author != null) {%>
						<div class="blog-comment-register-avatar"></div>
						<% } else { %>
						<div class="blog-comment-guest-avatar"></div>
						<%} %>
						
						<div class="blog-author">
							<% if (comment.UserProfileHref != null) { %>
							<a class="blog-author-icon" href="<%= comment.UserProfileHref %>"></a><a href="<%= comment.BlogHref ?? comment.UserProfileHref %>"><%= comment.AuthorNameHtml %></a>
							<% } else { %>
							<span class="blog-author-icon" ></span><%= comment.AuthorNameHtml %>
							<% } %>
						</div>
						<div class="blog-comment-date" title="<%= comment.Comment.DateCreated %>"><%= comment.Comment.DateCreated.ToString("g") %></div>

						
						<%if (Component.EnableVotingForComment && (voting = GetVotingComponent(comment.Comment.Id)) != null) { %>
							<div class="blog-comment-voting">
							<% voting.RenderControl(CurrentWriter); %>
							</div>	
						<%} %>	
						
						<% if (Component.Auth.CanViewIP) { %>
						<div class="blog-comment-author-ip">
							<a href="http://whois.domaintools.com/<%= Encode(UrlEncode(comment.Comment.AuthorIP)) %>" rel="nofollow"><%= Encode(comment.Comment.AuthorIP)%></a>
						</div>
						<% } %>
						
					</div>
					<div class="blog-comment-content">
						<div class="blog-comment-avatar">
							<% if (comment.Comment.Author != null && comment.Comment.Author.User != null && comment.Comment.Author.User.Image != null) { %>
							<img alt="" src="<%= comment.Comment.Author.User.Image.FilePath %>"  border="0" width="<%= comment.Comment.Author.User.Image.Width %>px"  height="<%= comment.Comment.Author.User.Image.Height %>px" />
							<% } %>
						</div>
						<%= comment.GetPostContent() %>
					</div>
					<div class="blog-comment-meta">
						<% if (Component.EnableComments && Component.Auth.CanCommentThisPost) { %>
						<span class="blog-comment-answer"><a href="javascript:void(0)" onclick="<%= ClientID %>_ShowForm(window.document.getElementById('comment<%= comment.Comment.Id %>'), <%= comment.Comment.Id %>);" rel="nofollow"><%= GetMessage("Reply") %></a></span>
						<span class="blog-vert-separator">|</span> 
						<% } %>
						<% if (comment.Parent != null) { %>
						<span class="blog-comment-parent"><a href="#comment<%= comment.Parent.Comment.Id %>" rel="nofollow"><%= GetMessage("Parent") %></a></span> 
						<span class="blog-vert-separator">|</span>
						<% } %>
						<span class="blog-comment-link"><a href="<%= comment.CommentHref %>" onclick="prompt('<%= JSEncode(GetMessageRaw("Confirm.Link")) %>', this.href); return false;" rel="nofollow" ><%= GetMessage("Link") %></a></span>
						<% if (comment.Auth.CanDeleteThisComment) { %>
						<span class="blog-vert-separator">|</span>
						<span class="blog-comment-delete"><a href="<%= GetDeleteOperationHref(comment.Comment.Id, GetPostDeletionFocusedId(comment, previousRootComment)) %>" onclick="return confirm('<%= Encode(JSEncode(GetMessageRaw("Confirm.Delete"))) %>')" rel="nofollow"><%= GetMessage("Kernel.Delete") %></a></span>
						<% } %>
						<% if (comment.Auth.CanApproveThisComment) { %>
						<span class="blog-vert-separator">|</span>
							<% if(comment.Comment.IsApproved) {%>
						<span class="blog-comment-approve"><a href="<%= GetOperationHref(DisapproveOperation, comment.Comment.Id) %>" rel="nofollow"><%= GetMessage("Disapprove") %></a></span>
							<%} else {%>
						<span class="blog-comment-disapprove"><a href="<%= GetOperationHref(ApproveOperation, comment.Comment.Id) %>" rel="nofollow"><%= GetMessage("Approve") %></a></span>						
							<%} %>
						<%} %>						
					</div>
								
				</div>
			</div>		
	<%
            if (comment.Parent == null)
                previousRootComment = comment;
			} 
			else if(comment.Comment.MarkedForDelete)
			{
	%>		<div class="blog-comment-cont">
				<div class="blog-comment-cont-white">
					<div class="blog-comment-content">
					<%= GetMessage("CommentDeleted") %>
					</div>
				</div>
			</div>
	<%
			}
	%>
			<div class="blog-clear-float">
			</div>
		</div>
		<% if (preview && Comment.ParentCommentId == comment.Comment.Id) { %>
		<div class="blog-comment-preview">
			<div id="commentPreview" class="blog-comment" style="padding-left: <%= (comment.Comment.DepthLevel * padding).ToString(".##", System.Globalization.NumberFormatInfo.InvariantInfo) %>em;">
				<div class="blog-comment-cont">
					<div class="blog-comment-cont-white">
						<%--<div class="blog-comment-info">
							<div class="blog-author"><div class="blog-author-icon"></div><%= Encode(GetDisplayName()) %></div>
							<div class="blog-comment-date" title="<%= Encode(DateTime.Now.ToString()) %>"><%= Encode(DateTime.Now.ToString("g")) %></div>
						</div>--%>
						<div class="blog-comment-content">
							<% Component.Preview(Comment.Text, CurrentWriter); %>
						</div>
					</div>
				</div>
				<div class="blog-clear-float"></div>
			</div>
		</div>
		<% } %>
		<% //if (!comment.Comment.MarkedForDelete && Component.Auth.CanCommentThisPost) { %>
		<div class="blog-comment-form"></div>
		<% //} %>
	<%
		}
	}
	%> 
	
	<% if (Component.EnableComments && Component.Auth.CanCommentThisPost) { %>
	
		<% if (hasComments) {%>
		<div class="blog-add-comment blog-add-comment-bottom">
			<a onclick="<%= ClientID %>_ShowForm(this.parentNode, 0);" href="javascript:void(0)"><b><%= GetMessage("AddComment") %></b></a>
		</div>
		<%} %>
		
		<% if (preview && Comment.ParentCommentId == 0) { %>
		<div class="blog-comment-preview">
			<div id="commentPreview" class="blog-comment" style="padding-left: <%= (0 * padding).ToString(".##", System.Globalization.NumberFormatInfo.InvariantInfo) %>em;">
				<div class="blog-comment-cont">
					<div class="blog-comment-cont-white">
						<%--<div class="blog-comment-info">
							<div class="blog-author"><div class="blog-author-icon"></div><%= Encode(GetDisplayName()) %></div>
							<div class="blog-comment-date" title="<%= Encode(DateTime.Now.ToString()) %>"><%= Encode(DateTime.Now.ToString("g")) %></div>
						</div>--%>
						<div class="blog-comment-content">
							<% Component.Preview(Comment.Text, CurrentWriter); %>
						</div>
					</div>
				</div>
				<div class="blog-clear-float" ></div>
			</div>
		</div>
		<% } %>
	<div class="blog-comment-form"></div>
	<% } %>
	
	<% repeater.Visible = false; %>
	<asp:Repeater runat="server" ID="repeater" OnItemDataBound="OnCommentDataBound">
	    <ItemTemplate>
           <bx:IncludeComponent 
                id="voting" 
                runat="server" 
                componentname="bitrix:rating.vote" 
                Template=".default" />			           
	    </ItemTemplate>
	</asp:Repeater>
	
	<%if (Component.Paging.IsBottomPosition) { %>
	<div class="blog-navigation-box blog-navigation-bottom">
		<div class="blog-page-navigation">
			<bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="bottom" CssClassPrefix="blog-" />
		</div>
	</div>
	<%} %>
</div>
	<bx:CommentForm runat="server" ID="Comment" Hidden="true" OnSubmit="Comment_Submit" CssClass="blog-comment-form-box" OnPreview="Comment_Preview" />
</div>

<script runat="server">
	private const string OperationParameter = "_action";
	private const string SourceParameter = "_source";
	private const string IdParameter = "_id";
    private const string FocusedIdParameter = "_focusedId";
	private const string DeleteOperation = "delete-comment";
	protected readonly string ApproveOperation = "approve-comment";
	protected readonly string DisapproveOperation = "disapprove-comment";
	
	private bool preview;
	
    
	string GetOperationHref(string operation, int id)
	{
		NameValueCollection query = HttpUtility.ParseQueryString(BXSefUrlManager.CurrentUrl.Query);
		query.Set(SourceParameter, ClientID.GetHashCode().ToString());
		query.Set(OperationParameter, operation);
		query.Set(IdParameter, id.ToString());
		BXCsrfToken.SetToken(query);
		

		UriBuilder uri = new UriBuilder(BXSefUrlManager.CurrentUrl);
		uri.Query = query.ToString();
		return Encode(uri.Uri.ToString());
	}
    string GetDeleteOperationHref(int deletedId, int focusedId)
    {
        NameValueCollection query = HttpUtility.ParseQueryString(BXSefUrlManager.CurrentUrl.Query);
        query.Set(SourceParameter, ClientID.GetHashCode().ToString());
        query.Set(OperationParameter, DeleteOperation);
        query.Set(IdParameter, deletedId.ToString());
        if (focusedId > 0)
            query.Set(FocusedIdParameter, focusedId.ToString());
        BXCsrfToken.SetToken(query);


        UriBuilder uri = new UriBuilder(BXSefUrlManager.CurrentUrl);
        uri.Query = query.ToString();
        return Encode(uri.Uri.ToString());
    }
		
    /// <summary>
    /// Ид элемента, которой поличит "фокус" после удаления текущего элемента
    /// </summary>
    int GetPostDeletionFocusedId(BlogPostCommentsComponent.CommentInfo current, BlogPostCommentsComponent.CommentInfo previousSibling)
    {
        //если нет "родителя", то страница спозиционируется на предыдущем "соседе" 
        if (current == null || current.Parent == null)
            return previousSibling != null ? previousSibling.Comment.Id : 0;
        //если есть "родитель" и он не удалён, то на "родителе"
        if (!current.Parent.Comment.MarkedForDelete)
            return current.Parent.Comment.Id;
        //если у "родителя" есть ещё хоть один неудалённый "ребёнок" (кроме текущего), то на "родителе"
        if (current.Parent.NotDeletedChildCount > 1)
            return current.Parent.Comment.Id;
        //продолжаем обработку
        return GetPostDeletionFocusedId(current.Parent, previousSibling);          
    }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        if(Component.EnableVotingForComment)
        {
            repeater.DataSource = Component.Comments;
            repeater.DataBind();        
        }
    }


    private Dictionary<int, IncludeComponent> votingDic = null;
    private void OnCommentDataBound(Object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            return;
        
        BlogPostCommentsComponent.CommentInfo info = (BlogPostCommentsComponent.CommentInfo)e.Item.DataItem;
        IncludeComponent c = (IncludeComponent)e.Item.FindControl("voting");

        if (c.Component == null)
            return;
        
        c.Component.Parameters["RolesAuthorizedToVote"] = Component.Parameters.Get("RolesAuthorizedToVote", string.Empty);
        c.Component.Parameters["BoundEntityTypeId"] = "BlogComment";        
        c.Component.Parameters["BoundEntityId"] = info.Comment.Id.ToString();
        c.Component.Parameters["CustomPropertyEntityId"] = BXBlogComment.GetCustomFieldsKey();
        c.Component.Parameters["BannedUsers"] = info.Comment.AuthorId.ToString();
        
        if (votingDic == null)
            votingDic = new Dictionary<int, IncludeComponent>();
        votingDic.Add(info.Comment.Id, c);
    }

    private IncludeComponent GetVotingComponent(int commentId)
    {
        IncludeComponent r;
        return votingDic != null && votingDic.TryGetValue(commentId, out r) ? r : null;
    }
    
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
        
		if (Component.FatalError != BlogPostCommentsComponent.ErrorCode.None)
		{
            Comment.Visible = false;
			Comment.Enabled = false; 
			return;
		}

        Comment.Visible = Comment.Enabled = Component.EnableComments && Component.Auth.CanCommentThisPost;
		if (Comment.Enabled && Component.IsGuest)
		{
			Comment.RequireUserName = CommentForm.FieldMode.Require;
			Comment.RequireUserEmail = Component.ShowGuestEmail ? (Component.RequireGuestEmail ? CommentForm.FieldMode.Require : CommentForm.FieldMode.Show) : CommentForm.FieldMode.Hide;
			Comment.RequireCaptcha = Component.RequireGuestCaptcha;
		}

		if (Request == null)
			return;
		
		string operationName = Request.QueryString[OperationParameter];
		
		if (operationName != DeleteOperation && operationName != ApproveOperation && operationName != DisapproveOperation)
			return;
		
		if (Request.QueryString[SourceParameter] != ClientID.GetHashCode().ToString())
			return;
		
		if (!BXCsrfToken.CheckTokenFromRequest(Request.QueryString))
			return;

        int id, focusedId;
		if (!int.TryParse(Request.QueryString[IdParameter], out id) || id <= 0)
			return;
        int.TryParse(Request.QueryString[FocusedIdParameter], out focusedId);

		if(operationName == DeleteOperation)
			Component.Hide(id, focusedId);
		else if(operationName == ApproveOperation)
			Component.Approve(id, true);
		else if(operationName == DisapproveOperation)
			Component.Approve(id, false);		
	}
	private void Comment_Submit(object sender, CommentForm.SubmitEventArgs e)
	{
		if (!e.Success)
		{
			Comment.Hidden = false;
				
			if (Comment.ParentCommentId != 0)
			{
				ScriptManager.RegisterStartupScript(
					this, 
					GetType(), 
					"CommentError",
					ClientID + "_ShowForm(document.getElementById('comment" + Comment.ParentCommentId + "'), " + Comment.ParentCommentId + ");",
					true
				);
			}
			return;
		}
		
		BlogPostCommentsComponent.CommentData data = new BlogPostCommentsComponent.CommentData();
		if (Comment.RequireUserName != CommentForm.FieldMode.Hide)
			data.UserName = Comment.UserName;
		if (Comment.RequireUserEmail != CommentForm.FieldMode.Hide)
			data.UserEmail = Comment.UserEmail;
		
		Component.AddComment(Comment.ParentCommentId, Comment.Text, data);
	}
	private void Comment_Preview(object sender, EventArgs e)
	{
		Comment.Hidden = false;
		string script = 
			Comment.ParentCommentId != 0
			? ClientID + "_ShowForm(document.getElementById('comment" + Comment.ParentCommentId + "'), " + Comment.ParentCommentId + ");"
			: ClientID + "_ShowForm(document.getElementById('commentPreview').parentNode, 0);";
		
		ScriptManager.RegisterStartupScript(
			this,
			GetType(),
			"CommentPreview",
			script,
			true
		);
		preview = true;
	}
	private string GetDisplayName()
	{
		if (Comment.RequireUserName != CommentForm.FieldMode.Hide)
			return Comment.UserName;
		return BXIdentity.Current.User.GetDisplayName();
	}
</script>