<%@ Reference Control="~/bitrix/components/bitrix/forum.topic.read/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.Forum.Components.ForumTopicReadTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.Forum" %>
<%@ Import Namespace="Bitrix.Forum.Components" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="Bitrix.Security" %>

<% 
	if (Component.FatalError != ForumTopicReadComponent.ErrorCode.None)
	{ 
		%>
		<div class="forum-content">
		<div class="forum-note-box forum-note-error">
			<div class="forum-note-box-text"><%= Component.GetErrorHtml(Component.FatalError) %></div>
		</div>	
		</div>
		<% 
		return;
	}
%>

<bx:InlineScript runat="server" ID="Script">
<script type="text/javascript">

function <%=ClientID %>_SelectAllPosts(link,select)
{
	if ( !link ) return;
	var label = select ? '<%=GetMessageJS("Title.DeSelectAll") %>' : '<%=GetMessageJS("Title.SelectAll") %>';
	
	if ( link.textContent )
		link.textContent = label;
	else 
		link.innerHtml = label;
		
	var posts = document.getElementsByName("<%=UniqueID %>$post");
	if ( !posts ) return;
	for ( var i = 0; i< posts.length; i++ ){
		var table = posts[i].parentNode.parentNode.parentNode.parentNode.parentNode.parentNode;
		if ( select )
			posts[i].checked = "checked";
		else 
			posts[i].checked = "";

		if ( table )
		 if(!table.className.match(/forum-post-selected/) && select)
			table.className += ' forum-post-selected';
		else if ( !select ) 
			table.className = table.className.replace(/\s*forum-post-selected/i, '');
	}
	link.setAttribute("onclick","<%=ClientID %>_SelectAllPosts(this,"+ (select ? "false":"true")+");return false;");
}

function <%= ClientID %>_SelectPost(table)
{
	
	if (table == null)
		return;

	if(table.className.match(/forum-post-selected/))
		table.className = table.className.replace(/\s*forum-post-selected/i, '');
	else
		table.className += ' forum-post-selected';
}

function <%= ClientID %>_ConfirmTopic()
{
	return window.confirm('<%= GetMessageJS("Confirmation.DeleteTopic") %>');
}

function <%= ClientID %>_ConfirmPost()
{
	return window.confirm('<%= GetMessageJS("Component.DeletePost") %>');
}

function <%= ClientID %>_TopicOKClick()
{
	var ddl = document.getElementById('<%= ClientID %>_TopicOp');
	if (ddl == null)
		return true;
	if (ddl.value == '')
		return false;
	if (ddl.value == '<%= ForumTopicReadComponent.TopicOperation.Delete %>')
		return window.confirm('<%= GetMessageJS("Confirmation.DeleteTopic") %>');
	return true;
}

function <%= ClientID %>_Reply2Author(authorName, enableBBCode, parentPostId)
{
	if (typeof window.GetForumPostTextArea == 'function')
	{
		var textarea = window.GetForumPostTextArea();
		
		if (textarea != null)
		{
			if (enableBBCode)
				textarea.value += "[b]"+authorName+"[/b]"+", ";
			else
				textarea.value += authorName + ", ";
				
			textarea.focus();
			
			return false;
		}
	}
	return true;
}

function <%= ClientID %>_PostOKClick()
{
	var ddl = document.getElementById('<%= ClientID %>_PostOp');
	if (ddl == null)
		return true;
	if (ddl.value == '')
		return false;
	if (ddl.value == '<%= ForumTopicReadComponent.PostOperation.Delete %>')
		return window.confirm('<%= GetMessageJS("Confirmation.DeletePosts") %>');
	return true;
}

function <%= ClientID %>_PrepareCodeBlockForQuote(str, codeBlock){
    if(typeof(Bitrix.ForumQuotationProcessors) == "undefined") return "[code]\r\n" + codeBlock + "[/code]";
    var r = null;
    for(var p in Bitrix.ForumQuotationProcessors)
        if((r = Bitrix.ForumQuotationProcessors[p](codeBlock))!= null) 
            break; 
             
    return "[code]\r\n" + (r ? r : codeBlock) + "[/code]";
}

function <%= ClientID %>_Quote(authorInfo, containerId, enableBBCode)
{
	var dstTextArea = null;
	if (typeof window.GetForumPostTextArea != 'function' || !(dstTextArea = window.GetForumPostTextArea()))
	    return true;

    var text = enableBBCode ? "[quote]" : "";
    
    if(authorInfo)
        text += authorInfo.toString() + ":\n";
        
    if(!enableBBCode)
		text += "===========================\n";
    
    var selectedText = Bitrix.DocumentSelection.create().getSelectedText();
    if(selectedText.length == 0){
        var messageContainer = null;
        var messageId = parseInt(containerId.replace(/forum_post_text_/gi, ""));
        if(messageId > 0 && (messageContainer = document.getElementById(containerId))){
            var messageContainerHtml = messageContainer.innerHTML; 
			messageContainerHtml = messageContainerHtml.replace(/\<br(\s)*(\/)*\>[\r\n]*/gi, "\r\n");
			
			messageContainerHtml = messageContainerHtml.replace(/\<(\/?)(b|i|s|u)\>/gi, "[$1$2]");
			
			messageContainerHtml = messageContainerHtml.replace(/\<script[^\>]*>[\r\n]*/gi, "\001").replace(/\<\/script[^\>]*>[\r\n]*/gi, "\002").replace(/\001([^\002]*)\002/gi, "");
			messageContainerHtml = messageContainerHtml.replace(/\<noscript[^\>]*>[\r\n]*/gi, "\001").replace(/\<\/noscript[^\>]*>[\r\n]*/gi, "\002").replace(/\001([^\002]*)\002/gi, "");
			// Quote & Code
			messageContainerHtml = messageContainerHtml.replace(/\<table\s*class\s*\=\s*(\"|\')?forum-quote(\"|\')?([^>]*)\>\s*\<tbody\>\s*(\<tr\>\s*\<th\>\s*([^<]*)\s*\<\/th\>\s*\<\/tr\>\s*)?\s*\<tr\>\s*\<td\>/gi, "\001");
			messageContainerHtml = messageContainerHtml.replace(/\<\/td\>\<\/tr\>\<\/tbody\>\<\/table\>/gi, "\003");                  
			var i = 0;
			while(i < 50 && (messageContainerHtml.search(/\002([^\002\003]*)\003/gi) >= 0 || messageContainerHtml.search(/\001([^\001\003]*)\003/gi) >= 0))
			{
				i++;
				messageContainerHtml = messageContainerHtml.replace(/\001([^\001\003]*)\003/gi, "[quote]$1[/quote]");				
			}
			messageContainerHtml = messageContainerHtml.replace(/[\001\002\003]/gi, "");
			messageContainerHtml = messageContainerHtml.replace(/\<div[^\>]*class\s*=\s*(?:\"|\')?forum-code-box(?:\"|\')?[^\>]*\>([\w\W]*?)\<\/div\><!--ForumCodeBoxEnd-->/ig, <%= ClientID %>_PrepareCodeBlockForQuote);				
			
			// Hrefs 
			messageContainerHtml = messageContainerHtml.replace(/\<a[^>]+href=[\"]([^\"]+)\"[^>]+\>([^<]+)\<\/a\>/gi, "[url=$1]$2[/url]");
			messageContainerHtml = messageContainerHtml.replace(/\<a[^>]+href=[\']([^\']+)\'[^>]+\>([^<]+)\<\/a\>/gi, "[url=$1]$2[/url]");
			
            messageContainerHtml = messageContainerHtml.replace(/\<[^\>]+\>/gi, "");        
            selectedText = Bitrix.HttpUtility.htmlDecode(messageContainerHtml);
        }
    }
    
    if(selectedText.length == 0)
        return true;
        
    text += selectedText;
    text += enableBBCode ? "[/quote]" : "\n===========================\n";
    
    if(dstTextArea.value.length > 0)
        dstTextArea.value += "\n";
	dstTextArea.value += text;
	dstTextArea.focus();
	return false;    
}
</script>
</bx:InlineScript>

<div class="forum-content">

<div class="forum-navigation-box forum-navigation-top">
	<div class="forum-page-navigation">
		<bx:IncludeComponent runat="server" ID="HeaderPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="top" CssClassPrefix="forum-" />
	</div>
	<% if (Component.Auth.CanTopicReply && (!Component.Topic.Closed || Component.Auth.CanOpenCloseThisTopic))
	{ %>
	<div class="forum-new-post">
		<a href="<%= Component.TopicReplyHref %>" onclick="return <%= ClientID %>_Reply2Author('<%= JSEncode(Component.Topic.AuthorName) %>', <%=(Component.Forum.AllowBBCode ? "true" : "false") %>,<%= Component.Topic.FirstPostId %>)"><span><%= GetMessage("UrlTitle.Reply") %></span></a>
	</div>
	<% } %>
	<div class="forum-clear-float"></div>
</div>

<div class="forum-header-box">
	<div class="forum-header-options">
	<% 
	for (int i = 0; i < Component.HeaderLinks.Count; i++) 
	{ 
		ForumTopicReadComponent.LinkInfo link = Component.HeaderLinks[i];
		%><% if (i != 0) { %>&nbsp;&nbsp;<% } %><span class="<%= link.CssClass ?? ("forum-option-" + i) %>"><a href="<%= link.Href %>"<%= link.CustomAttrs %>><%= link.Title %></a></span><%
	}	
	%>
	</div>
	<div class="forum-header-title">
		<% if (Component.Topic.Closed) { %>
		<span class="forum-header-title-closed">[ <span><%= GetMessage("Status.Closed") %></span> ]</span>
		<% } %>
		<span><%= Component.TopicTitleHtml %><%= !string.IsNullOrEmpty(Component.TopicDescriptionHtml) ? (", " + Component.TopicDescriptionHtml) : "" %></span>
	</div>
</div>

<div class="forum-block-container">
	<div class="forum-block-outer">
		<div class="forum-block-inner">
			<% 
            IncludeComponent voting = null;    	    
			for(int i = 0; i < Component.Posts.Count; i++)
			{ 
				ForumTopicReadComponent.PostInfo post = Component.Posts[i];
			%>
			<table class="forum-post-table <%= (i == 0) ? "forum-post-first " : "" %><%= (i == Component.Posts.Count - 1) ? "forum-post-last " : "" %><%= (i % 2 == 0) ? "forum-post-odd" : "forum-post-even" %><%= !post.Post.Approved ? " forum-post-hidden" : "" %>" cellspacing="0" id="post<%= post.Post.Id %>" >
				<tbody>
					<tr>
						<td class="forum-cell-user">
							<div class="forum-user-info">
								<div class="forum-user-name">
								<% if (!string.IsNullOrEmpty(post.UserProfileHref)) { %>
								<a href="<%= post.UserProfileHref %>"><span><%= post.AuthorNameHtml %></span></a>
								<% } else { %>
								<span><%= post.AuthorNameHtml %></span>
								<% } %>
								</div>
								
								<% if (post.Author == null || post.Author.User == null) { %>
								<div class="forum-user-guest-avatar"></div>
								<div class="forum-user-status"><span><%= GetMessage("Guest") %></span></div>
								<% } else if (post.Author.User.Image == null) { %>
								<div class="forum-user-register-avatar"></div>
								<% } else { %>
								<div class="forum-user-avatar"><img src="<%= post.Author.User.Image.FilePath %>" alt="<%= post.Author.User.Image.Description %>" width="<%= post.Author.User.Image.Width %>" height="<%= post.Author.User.Image.Height %>" /></div>
								<% } %>
								
								<div class="forum-user-additional">
									<% if (post.Author != null && post.Author.User != null) { %>
									
									<% if (post.UserPostsReadHref != String.Empty)
                                    { %>
									
									<span><%= GetMessage("Posts")%>: <a href="<%=post.UserPostsReadHref %>" title="<%=GetMessage("ViewPostsByAuthor") %>"><%= post.Author.Posts%></a></span>
									<%}
                                     else
                                    { %>
                                    <span><%= GetMessage("Posts")%>: <span><%= post.Author.Posts%></span></span>
									<%} %>
									<span><%= GetMessage("RegistrationDate") %>: <span><%= post.Author.User.CreationDate.ToString("d") %></span></span>
									<% } %>
									<% if (Component.Auth.CanViewIP && !BXStringUtility.IsNullOrTrimEmpty(post.Post.AuthorIP)) { %>
									<span>IP: <span><a href="http://whois.domaintools.com/<%= Encode(UrlEncode(post.Post.TextEncoder.Decode(post.Post.AuthorIP))) %>"><%= post.Post.AuthorIP %></a></span></span>
									<% } %>
								</div>
							</div>
						</td>
						<td class="forum-cell-post">
							<div class="forum-post-date">
								
								<div class="forum-post-number">
									<a href="<%= post.ThisPostHref %>" onclick="prompt('<%= JSEncode(GetMessageRaw("Dialog.CopyThisLinkToClipboard")) %>', this.href); return false;" title="<%= GetMessage("ToolTip.PermanentLink") %>" rel="nofollow" >#<%= post.Num %></a><% 
									if (Component.CanModeratePosts) 
									{ 
										%>&nbsp;<input type="checkbox" onclick="<%= ClientID %>_SelectPost(this.parentNode.parentNode.parentNode.parentNode.parentNode.parentNode)" name="<%= UniqueID %>$post" value="<%= post.Post.Id %>" /><% 
									} 
									%>
								</div>
								
								<% if (post.Post.Id == Component.Topic.FirstPostId) { %>
									<div class="forum-post-rating"><bx:IncludeComponent 
										id="topicVoting" 
										runat="server" 
										componentname="bitrix:rating.vote" 
										Template=".default" 
										BoundEntityTypeId="ForumTopic" 
										BoundEntityId="<%$ Parameters:TopicId %>" 
										CustomPropertyEntityId = "ForumTopic"
										RolesAuthorizedToVote="User" 
										BannedUsers = ""
									 /></div>
								<%}else if (Component.EnableVotingForPost && (voting = GetVotingComponent(post.Post.Id)) != null) {%>
									<div class="forum-post-rating"><% voting.RenderControl(CurrentWriter); %></div>
								<%} %>

								<span title="<%= Encode(post.Post.DateCreate.ToString()) %>"><%= Encode(post.Post.DateCreate.ToString("g")) %></span>
							</div>
							<div class="forum-post-entry">
								<div class="forum-post-text" id="forum_post_text_<%= post.Post.Id %>"><%= post.ContentHtml %></div>
								<% if (post.Author != null && !BXStringUtility.IsNullOrTrimEmpty(post.Author.Signature)) { %>
								<div class="forum-user-signature">
									<div class="forum-signature-line"></div>
									<span class="forum-signature-content"><%= post.AuthorSignatureHtml %></span>
								</div>
								<% } %>
							</div>
						</td>
					</tr>
					
					<tr>
						<td class="forum-cell-contact">
							<div class="forum-contact-links">&nbsp;</div>
						</td>
						
						<td class="forum-cell-actions">
							<div class="forum-action-links">
								&nbsp;
								<% if (post.Post.Id ==  Component.Topic.FirstPostId) { %>
									<% if (Component.Auth.CanApprove) { %>
										<% if (Component.Topic.Approved) { %>
										&nbsp;&nbsp;<span class="forum-action-hide"><a href="<%= GetTopicOperationHref(ForumTopicReadComponent.TopicOperation.Hide) %>" rel="nofollow"><%= GetMessage("UrlTitle.HideTopic") %></a></span>
										<% } else { %>
										&nbsp;&nbsp;<span class="forum-action-approve"><a href="<%= GetTopicOperationHref(ForumTopicReadComponent.TopicOperation.Approve) %>" rel="nofollow"><%= GetMessage("UrlTitle.ApproveTopic") %></a></span>
										<% } %>
									<% } %>
									<% if (Component.Auth.CanDeleteThisTopic) { %>
									&nbsp;&nbsp;<span class="forum-action-delete"><a href="<%= GetTopicOperationHref(ForumTopicReadComponent.TopicOperation.Delete) %>" onclick="if (!<%= ClientID %>_ConfirmTopic()) return false;" rel="nofollow"><%= GetMessage("UrlTitle.DeleteTopic") %></a></span>
									<% } %>
									<% if (Component.Auth.CanEditThisTopic) { %>
									&nbsp;&nbsp;<span class="forum-action-edit"><a href="<%= Component.TopicEditHref %>" rel="nofollow"><%= GetMessage("UrlTitle.EditTopic") %></a></span>
									<% } %>
									<% if (Component.Auth.CanOpenCloseThisTopic) { %>
										<% if (Component.Topic.Closed) { %>
										&nbsp;&nbsp;<span class="forum-action-open"><a href="<%= GetTopicOperationHref(ForumTopicReadComponent.TopicOperation.Open) %>" rel="nofollow"><%= GetMessage("UrlTitle.OpenTopic") %></a></span>
										<% } else { %>
										&nbsp;&nbsp;<span class="forum-action-close"><a href="<%= GetTopicOperationHref(ForumTopicReadComponent.TopicOperation.Close) %>" rel="nofollow"><%= GetMessage("UrlTitle.CloseTopic") %></a></span>
										<% } %>
									<% } %>
								<% } else { %>
									<% if (post.Auth.CanApprove) { %>
										<% if (post.Post.Approved) { %>
										&nbsp;&nbsp;<span class="forum-action-hide"><a href="<%= GetPostOperationHref(ForumTopicReadComponent.PostOperation.Hide, post.Post.Id) %>" rel="nofollow"><%= GetMessage("UrlTitle.HidePost") %></a></span>
										<% } else { %>
										&nbsp;&nbsp;<span class="forum-action-approve"><a href="<%= GetPostOperationHref(ForumTopicReadComponent.PostOperation.Approve, post.Post.Id) %>" rel="nofollow"><%= GetMessage("UrlTitle.ApprovePost") %></a></span>
										<% } %>
									<% } %>
									<% if (post.Auth.CanDeleteThisPost) { %>
									&nbsp;&nbsp;<span class="forum-action-delete"><a href="<%= GetPostOperationHref(ForumTopicReadComponent.PostOperation.Delete, post.Post.Id) %>" onclick="if (!<%= ClientID %>_ConfirmPost()) return false;" rel="nofollow"><%= GetMessage("UrlTitle.DeletePost") %></a></span>
									<% } %>
									<% if (post.Auth.CanEditThisPost) { %>
									&nbsp;&nbsp;<span class="forum-action-edit"><a href="<%= post.PostEditHref %>" rel="nofollow"><%= GetMessage("UrlTitle.EditPost") %></a></span>
									<% } %>
								<% } %>
								<% if (Component.Auth.CanTopicReply) { %>
								&nbsp;&nbsp;<span class="forum-action-quote"><a href="<%= post.PostQuoteHref %>" rel="nofollow"  onclick="return <%= ClientID %>_Quote('<%= JSEncode(post.Post.AuthorName) %>', 'forum_post_text_<%= post.Post.Id %>', <%=(Component.Forum.AllowBBCode ? "true" : "false") %>);"><%= GetMessage("UrlTitle.QuotePost") %></a></span>
								&nbsp;&nbsp;<span class="forum-action-reply"><a href="#postform" rel="nofollow" onclick="return <%= ClientID %>_Reply2Author('<%= JSEncode(post.Post.AuthorName) %>', <%=(Component.Forum.AllowBBCode ? "true" : "false") %>,<%= post.Post.Id %>)" title="<%= GetMessage("UrlTitle.Reply2Author.Title")%>"><%= GetMessage("UrlTitle.Reply2Author")%></a></span>
								<% } %>
							</div>
						</td>
					</tr>
					</tbody>
					<% if (i == Component.Posts.Count - 1) { %>
					<tfoot>
					<tr>
						<td colspan="2" class="forum-column-footer">
							<div class="forum-footer-inner">
								<% if (!Component.CanModeratePosts && !Component.CanModerateTopic) { %>&nbsp;<% } %>
								<% if (Component.CanModeratePosts) { %>
								<div class="forum-post-moderate">
									<select id="<%= ClientID %>_PostOp" name="<%= UniqueID %>$postop">
										<option value=""><%= GetMessage("Option.ManagePosts") %></option>
										<% if (Component.Auth.CanApprove) { %>
										<option value="<%= ForumTopicReadComponent.PostOperation.Hide %>"><%= GetMessage("Option.HidePosts") %></option>
										<option value="<%= ForumTopicReadComponent.PostOperation.Approve %>"><%= GetMessage("Option.ApprovePosts") %></option>
										<% } %>
										<% if (Component.Auth.CanPostDelete) { %>
										<option value="<%= ForumTopicReadComponent.PostOperation.Delete %>"><%= GetMessage("Option.DeletePosts") %></option>
										<% } %>
									</select>&nbsp;<asp:Button runat="server" ID="PostOK" Text="OK" OnClick="PostOKClick" />
									<span class="forum-footer-option forum-footer-selectall forum-footer-option-first"><a rel="nofollow" href="#" onclick="<%=ClientID %>_SelectAllPosts(this,true);return false;"><%=GetMessage("Title.SelectAll") %></a></span>
								</div>
								<% } %>
								<% if (Component.CanModerateTopic) { %>
								<div class="forum-topic-moderate">
									<select id="<%= ClientID %>_TopicOp" name="<%= UniqueID %>$topicop">
										<option value=""><%= GetMessage("Option.ManageTopic") %></option>
										<% if (Component.Auth.CanOpenCloseThisTopic) { %>
											
											<% if (Component.Topic.Closed) {%>
											<option value="<%= ForumTopicReadComponent.TopicOperation.Open %>"><%= GetMessage("Option.OpenTopic") %></option>
											<% } else  {%>								 
											<option value="<%= ForumTopicReadComponent.TopicOperation.Close %>"><%= GetMessage("Option.CloseTopic") %></option>
											<% } %>
											
										<% } %>
										<% if (Component.Auth.CanApprove) { %>
										
											<% if (Component.Topic.Approved) {%>
											<option value="<%= ForumTopicReadComponent.TopicOperation.Hide %>"><%= GetMessage("Option.HideTopic") %></option>
											<% } else  {%>
											<option value="<%= ForumTopicReadComponent.TopicOperation.Approve %>"><%= GetMessage("Option.ApproveTopic") %></option>
											<% } %>
										<% } %>
										<% if (Component.Auth.CanTopicStick) { %>
											<% if (Component.Topic.StickyIndex > 0) {%>
											<option value="<%= ForumTopicReadComponent.TopicOperation.Unstick %>"><%= GetMessage("Option.UnstickTopic") %></option>
											<% } else  {%>
											<option value="<%= ForumTopicReadComponent.TopicOperation.Stick %>"><%= GetMessage("Option.StickTopic") %></option>
											<% } %>
										<% } %>
										<% if (Component.Auth.CanMoveThisTopic) { %>
										<option value="<%= ForumTopicReadComponent.TopicOperation.Move %>"><%= GetMessage("Option.MoveTopic") %></option>
										<% } %>
										<% if (Component.Auth.CanDeleteThisTopic) { %>
										<option value="<%= ForumTopicReadComponent.TopicOperation.Delete %>"><%= GetMessage("Option.DeleteTopic") %></option>
										<% } %>
									</select>&nbsp;<asp:Button runat="server" ID="TopicOK" Text="OK" OnClick="TopicOKClick" />
								</div>
								<% } %>
							</div>

						</td>
					</tr>
				</tfoot>
				<% } %>
			</table>
			<%
			} 
			%>
		</div>
	</div>
</div>

<div class="forum-navigation-box forum-navigation-bottom">
	<div class="forum-page-navigation">
		<bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="bottom" CssClassPrefix="forum-" />
	</div>
	<% if (Component.Auth.CanTopicReply && (!Component.Topic.Closed || Component.Auth.CanOpenCloseThisTopic))
	{ %>
	<div class="forum-new-post">
		<a href="<%= Component.TopicReplyHref %>" onclick="return <%= ClientID %>_Reply2Author('<%= JSEncode(Component.Topic.AuthorName) %>', <%=(Component.Forum.AllowBBCode ? "true" : "false") %>,<%= Component.Topic.FirstPostId %>)"><span><%= GetMessage("UrlTitle.Reply") %></span></a>
	</div>
	<% } %>
	<div class="forum-clear-float"></div>
</div>

</div>
<% repeater.Visible = false; %>
<asp:Repeater runat="server" ID="repeater" OnItemDataBound="OnPostDataBound">
    <ItemTemplate>
       <bx:IncludeComponent 
            id="postVoting" 
            runat="server" 
            componentname="bitrix:rating.vote" 
            Template=".default" />			           
    </ItemTemplate>
</asp:Repeater>



<script runat="server">
	private const string TargetParameter = "_target";
	private const string OperationParameter = "_action";
	private const string PostIdParameter = "_id";
	
	private string GetTopicOperationHref(ForumTopicReadComponent.TopicOperation operation)
	{
		NameValueCollection query = HttpUtility.ParseQueryString(Bitrix.Services.BXSefUrlManager.CurrentUrl.Query);
		query.Set(TargetParameter, "topic");
		query.Set(OperationParameter, operation.ToString().ToLower());
		BXCsrfToken.SetToken(query);
		
		UriBuilder uri = new UriBuilder(Bitrix.Services.BXSefUrlManager.CurrentUrl);
		uri.Query = query.ToString();
		return Encode(uri.Uri.ToString());
	}

	private string GetPostOperationHref(ForumTopicReadComponent.PostOperation operation, long id)
	{
		NameValueCollection query = HttpUtility.ParseQueryString(Bitrix.Services.BXSefUrlManager.CurrentUrl.Query);
		query.Set(TargetParameter, "post");
		query.Set(OperationParameter, operation.ToString().ToLower());
		query.Set(PostIdParameter, id.ToString());
		BXCsrfToken.SetToken(query);

		UriBuilder uri = new UriBuilder(Bitrix.Services.BXSefUrlManager.CurrentUrl);
		uri.Query = query.ToString();
		return Encode(uri.Uri.ToString());
	}
	
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		if (Component.FatalError != ForumTopicReadComponent.ErrorCode.None)
			return;
		TopicOK.OnClientClick = string.Format("if (!{0}_TopicOKClick()) return false;", ClientID);
		PostOK.OnClientClick = string.Format("if (!{0}_PostOKClick()) return false;", ClientID);

        topicVoting.Visible = Component.EnableVotingForTopic;
        if (Component.Auth.UserId == Component.Topic.AuthorId || !Component.Auth.CanVoteForThisTopic)
            topicVoting.Component.Parameters["BannedUsers"] = Component.Auth.UserId.ToString();          

        
        if (Component.EnableVotingForPost)
        {
            repeater.DataSource = Component.Posts;
            repeater.DataBind();        
        }
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		
		if (Component.FatalError != ForumTopicReadComponent.ErrorCode.None)
			return;
        
		AddSubscriptionLink();
        
		if (Request == null)
			return;
		string target = Request.QueryString[TargetParameter];
		if (target != "topic" && target != "post")
			return;
		if (Request.QueryString[OperationParameter] == null)
			return;
		if (!BXCsrfToken.CheckTokenFromRequest(Request.QueryString))
			return;

		if (target == "topic")
			OperateTopic();
		else
			OperatePost();      
	}
	
	private void AddSubscriptionLink()
	{
		if (!Component.Auth.CanSubscribe)
			return;
		
		List<ForumTopicReadComponent.LinkInfo> links = Component.HeaderLinks;
		if (links == null)
			links = new List<ForumTopicReadComponent.LinkInfo>();

		if (Component.UserSubscription == null)
		{
			links.Add(
				new ForumTopicReadComponent.LinkInfo(
					HttpUtility.HtmlDecode(GetTopicOperationHref(ForumTopicReadComponent.TopicOperation.Subscribe)),
					GetMessage("Option.SubscribeForum"),
					"forum-option-subscribe",
					@" title=""" + GetMessage("Subscribe.Link.Title") + @""""
				)
			);
		}
		else if (Component.UserSubscription.TopicId > 0)
		{
			links.Add(
				new ForumTopicReadComponent.LinkInfo(
					HttpUtility.HtmlDecode(GetTopicOperationHref(ForumTopicReadComponent.TopicOperation.Unsubscribe)),
					GetMessage("Option.UnSubscribeForum"),
					"forum-option-unsubscribe",
					@" title=""" + GetMessage("UnSubscribe.Link.Title") + @""""
				)
			);
		}
	}
	
	private void OperateTopic()
	{
		string operation = Request.QueryString[OperationParameter];
		ForumTopicReadComponent.TopicOperation op;
		try
		{
			op = (ForumTopicReadComponent.TopicOperation)Enum.Parse(typeof(ForumTopicReadComponent.TopicOperation), operation, true);
		}
		catch
		{
			return;
		}
		Component.DoTopicOperation(op);
	}

	private void OperatePost()
	{
		string operation = Request.QueryString[OperationParameter];
		ForumTopicReadComponent.PostOperation op;
		try
		{
			op = (ForumTopicReadComponent.PostOperation)Enum.Parse(typeof(ForumTopicReadComponent.PostOperation), operation, true);
		}
		catch
		{
			return;
		}

		int id;
		if (!int.TryParse(Request.QueryString[PostIdParameter], out id))
			return;

		Component.DoPostOperation(op, new int[] { id });
	}

	private void TopicOKClick(object sender, EventArgs e)
	{
		if (!Component.CanModerateTopic)
			return;

		string operationString = Request.Form[UniqueID + "$topicop"];
		if (string.IsNullOrEmpty(operationString) || !Enum.IsDefined(typeof(ForumTopicReadComponent.TopicOperation), operationString))
			return;

		ForumTopicReadComponent.TopicOperation operation;
		try
		{
			operation = (ForumTopicReadComponent.TopicOperation)Enum.Parse(typeof(ForumTopicReadComponent.TopicOperation), operationString);
		}
		catch
		{
			return;
		}

		Component.DoTopicOperation(operation);
	}

	private void PostOKClick(object sender, EventArgs e)
	{
		if (!Component.CanModeratePosts)
			return;

		string operationString = Request.Form[UniqueID + "$postop"];
		if (string.IsNullOrEmpty(operationString) || !Enum.IsDefined(typeof(ForumTopicReadComponent.PostOperation), operationString))
			return;
		ForumTopicReadComponent.PostOperation operation;
		try
		{
			operation = (ForumTopicReadComponent.PostOperation)Enum.Parse(typeof(ForumTopicReadComponent.PostOperation), operationString);
		}
		catch
		{
			return;
		}
		

		string[] idStrings = Request.Form.GetValues(UniqueID + "$post");
		if (idStrings == null || idStrings.Length == 0)
			return;
		List<int> ids = new List<int>();

		foreach (string s in idStrings)
		{
			int id;
			if (!int.TryParse(s, out id) || id <= 0)
				continue;
			int i = ids.BinarySearch(id);
			if (i >= 0)
				continue;
			ids.Insert(~i, id);
		}

		if (ids.Count == 0)
			return;

		Component.DoPostOperation(operation, ids);
	}

    private Dictionary<long, IncludeComponent> votingDic = null;
    private void OnPostDataBound(Object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            return;

        ForumTopicReadComponent.PostInfo info = (ForumTopicReadComponent.PostInfo)e.Item.DataItem;
        IncludeComponent c = (IncludeComponent)e.Item.FindControl("postVoting");

        if (c.Component == null)
            return;

		c.Component.Parameters["BoundEntityTypeId"] = "ForumPost";
		c.Component.Parameters["BoundEntityId"] = info.Post.Id.ToString();
		c.Component.Parameters["CustomPropertyEntityId"] = BXForumModuleConfiguration.PostCustomFieldEntityId;
        c.Component.Parameters["RolesAuthorizedToVote"] = "User";

        if (Component.Auth.UserId == info.Post.AuthorId || !Component.Auth.CanVoteForThisPost)
            c.Component.Parameters["BannedUsers"] = Component.Auth.UserId.ToString();

        if (votingDic == null)
            votingDic = new Dictionary<long, IncludeComponent>();
        votingDic.Add(info.Post.Id, c);
    }

    private IncludeComponent GetVotingComponent(long postId)
    {
        IncludeComponent r;
        return votingDic != null && votingDic.TryGetValue(postId, out r) ? r : null;
    } 
	
	
	   
</script>
