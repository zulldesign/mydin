<%@ Reference Control="~/bitrix/components/bitrix/forum.post.list/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.Forum.Components.ForumPostListTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.Forum.Components" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="Bitrix.Security" %>
<script runat="server">
		
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);

		PostOK.OnClientClick = string.Format("if (!{0}OKClick()) return false;", ClientID);
	}
	void OKClick(object sender, EventArgs e)
	{
		string operationString = Request.Form[UniqueID + "$postop"];
		if (string.IsNullOrEmpty(operationString) || !Enum.IsDefined(typeof(ForumPostListComponent.PostOperation), operationString))
			return;

		ForumPostListComponent.PostOperation operation;
		try
		{
			operation = (ForumPostListComponent.PostOperation)Enum.Parse(typeof(ForumPostListComponent.PostOperation), operationString);
		}
		catch
		{
			return;
		}
		switch (operation)
		{
			case ForumPostListComponent.PostOperation.Delete:
				List<long> postIds = new List<long>();
				string[] posts = Request.Form.GetValues(UniqueID+"$post");
				if (posts == null || posts.Length == 0)
					return;			
				long i=0;
				foreach (string s in posts)
					if (Int64.TryParse(s, out i))
						postIds.Add(i);
				if (postIds.Count == 0) return;
				Bitrix.DataTypes.BXParamsBag<object> bag = new Bitrix.DataTypes.BXParamsBag<object>();
				bag["posts"] = postIds;
				List<string> errors = new List<string>();
				Component.ProcessCommand("delete", bag, errors);

				break;
		}
	}


</script>
<% 
    if ((Component.ComponentError != ForumPostListComponentError.None))
	{ 
		%>
		<div class="forum-content">
		<div class="forum-note-box forum-note-error">
	    <% foreach (string errorMsg in GetErrorMessages()){%>
	        <div class="forum-note-box-text"><%= errorMsg%></div>
	    <%} %>
		</div>	
		</div>
		<% 
		if ( Component.ComponentError != ForumPostListComponentError.OperationError )
		return;
	}
%>

<div class="forum-content">

<% 
    if (Component.PostList.Count > 0)
    { 
		%>
<div class="forum-navigation-box forum-navigation-top">
	<div class="forum-page-navigation">
		<bx:IncludeComponent runat="server" ID="HeaderPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="top" CssClassPrefix="forum-" />
	</div>
</div>


			<% 
    int lastTopicId = -1;
    int k = 0;
	bool canDeleteSome = false;
    for (int i = 0; i < Component.PostList.Count; i++)
    {
        var post = Component.PostList[i];
                %>
			<% if (Component.ComponentGroupingOption !=
         Bitrix.Forum.Components.ForumPostListGroupingOption.GroupByTopic ||
          (Component.ComponentGroupingOption == Bitrix.Forum.Components.ForumPostListGroupingOption.GroupByTopic && post.Topic.Id != lastTopicId))
      {
          lastTopicId = post.Topic.Id; k = 0; %>
                <div class="forum-header-box">
                    <div class="forum-header-options"></div>
                    <div class="forum-header-title">
                        <span><%=post.TopicTitleHtml%></span>
                    </div>
                </div>
<div class="forum-block-container">
	<div class="forum-block-outer">
		<div class="forum-block-inner">

				<% } %> 

			<table class="forum-post-table <%= (k == 0) ? "forum-post-first " : "" %><%= (i == Component.PostList.Count - 1 || ( k<Component.PostList.Count-1  && Component.PostList[k+1].Topic.Id!=post.Topic.Id)) ? "forum-post-last " : "" %><%= (k % 2 == 0) ? "forum-post-odd" : "forum-post-even" %><%= !post.Post.Approved ? " forum-post-hidden" : "" %>" cellspacing="0" id="post<%= post.Post.Id %>" >
				<tbody>

					<tr>
						<td class="forum-cell-user">
							<div class="forum-user-info">
								<div class="forum-user-name">
								<% if (post.Author == null || post.Author.User != null)
           { %>
								<a href="<%= post.AuthorProfileUrl %>"><span><%= post.AuthorName%></span></a>
								<% }
           else
           { %>
								<span><%= post.AuthorName%></span>
								<div class="forum-user-status"><span><%= GetMessage("Guest")%></span></div>
								<% } %>
								</div>
								
                                <% if (post.Author == null || post.Author.User == null || post.Author.User.Image == null)
                                   { %>
								<div class="forum-user-register-avatar"></div>
								<% }
                                   else
                                   { %>
								<div class="forum-user-avatar"><img src="<%= post.Author.User.Image.FilePath %>" alt="<%= post.Author.User.Image.Description %>" width="<%= post.Author.User.Image.Width %>" height="<%= post.Author.User.Image.Height %>" /></div>
								<% } %>
								
							</div>
						</td>
						<td class="forum-cell-post">
							<div class="forum-post-date">
							<% if ( post.CanDeletePost && (post.Post.Id!= post.Post.Topic.FirstPostId || (post.Post.Id== post.Post.Topic.FirstPostId && post.CanDeleteTopic)) ){
								canDeleteSome = true;
							%>
							<div class="forum-post-number">
								<input type="checkbox" id = "<%=ClientID %>post_<%=post.Post.Id %>" onclick="<%=ClientID %>selectPost(this);" value="<%= post.Post.Id %>" name = "<%=UniqueID %>$post" />
							</div>
							<%} %>
								<span title="<%= Encode(post.Post.DateCreate.ToString()) %>"><%= Encode(post.Post.DateCreate.ToString("g"))%></span>
							</div>
							<div class="forum-post-entry">
								<div class="forum-post-text" id="forum_post_text_<%= post.Post.Id %>"><%= post.ContentHtml%></div>
								<% if (post.Author != null && !BXStringUtility.IsNullOrTrimEmpty(post.Author.Signature))
								{ %>
								<div class="forum-user-signature">
									<div class="forum-signature-line"></div>
									<span class="forum-signature-content"><%= post.AuthorSignatureHtml%></span>
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
							<span class="forum-action-goto">
                                <noindex>
                                    <a href="<%=post.TopicReadUrl %>" rel="nofollow"><%=GetMessage("UrlTitle.GoToTopic")%></a>
                                </noindex>
                            </span>	
                            &nbsp;
							<span class="forum-action-goto">
                                <noindex>
                                    <a href="<%=post.PostReadUrl %>" rel="nofollow"><%=GetMessage("UrlTitle.GoTo")%></a>
                                </noindex>
                            </span>	
						    </div>
					    </td>
					</tr>
					</tbody>
					<% if (i == Component.PostList.Count - 1){ %>
					<tfoot>
					<tr>
						<td colspan="2" class="forum-column-footer">
							<div class="forum-footer-inner">
								<% if (canDeleteSome){ %>
								<div class="forum-post-moderate">
									<select id="<%= ClientID %>_PostOp" name="<%= UniqueID %>$postop">
										<option value="<%=ForumPostListComponent.PostOperation.Delete %>"><%= GetMessage("Option.DeletePosts")%></option>
									</select>&nbsp;<asp:Button runat="server" ID="PostOK" Text="OK" OnClick="OKClick" />
									<span class="forum-footer-option forum-footer-selectall forum-footer-option-first"><a rel="nofollow" href="#" onclick="<%=ClientID %>_SelectAllPosts(this,true);return false;"><%=GetMessage("Title.SelectAll") %></a></span>
								</div>
								<% } %>
							</div>
						</td>
					</tr>
					<% } %>
			</table>
			<% if (k == 0)
      {	%>	
		</div>
	</div>
</div>
<%} %>
			<%
    k++;
    } 
			%>
			
	<div class="forum-navigation-box forum-navigation-bottom">
	<div class="forum-page-navigation">
		<bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="bottom" CssClassPrefix="forum-" />
	</div>
</div>		
	
<%}
    else
    { %>
    



<div class="forum-header-box">
	<div class="forum-header-title"><span><%=GetMessage("Message.PostsOfUser") %></span></div>
</div>

<div class="forum-block-container">
	<div class="forum-block-outer">
		<div class="forum-block-inner">
			<table cellspacing="0" class="forum-table forum-topic-list">

			<tbody>
				<tr class="forum-row-first forum-row-last forum-row-odd">

					<td class="forum-column-alone">
						<div class="forum-empty-message">
						<%=Component.EmptyMessage %></div>
					</td>
				</tr>
			</tbody>
			<tfoot>
				<tr>
					<td class="forum-column-footer">
						<div class="forum-footer-inner">&nbsp;</div>
					</td>
				</tr>
			</tfoot>

			</table>
		</div>
	</div>

</div>

<% } %>

</div>

<script type="text/javascript">

function <%=ClientID %>OKClick()
{
	var select = document.getElementById('<%= ClientID %>_PostOp');
	if ( !select ) return true;
	if ( select.value=='' ) return false;
	if(select.value=='<%= ForumPostListComponent.PostOperation.Delete %>')
		return window.confirm('<%=GetMessageJS("DoYouWantToDeletePosts") %>');
	return true;
}

function <%=ClientID %>selectPost(element)
{	
	var table = element.parentNode.parentNode.parentNode.parentNode.parentNode.parentNode;
	if (!table)
		return;

	if(table.className.match(/forum-post-selected/))
		table.className = table.className.replace(/\s*forum-post-selected/i, '');
	else
		table.className += ' forum-post-selected';
}

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

</script>
