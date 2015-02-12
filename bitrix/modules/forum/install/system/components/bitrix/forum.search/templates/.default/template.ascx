<%@ Reference Control="~/bitrix/components/bitrix/forum.search/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.Forum.Components.ForumSearchTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.Forum.Components" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="Bitrix.Security" %>
<script runat="server">
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		if (Component.PostList.Count == 0)
			Results["PagingTotalRecordCount"] = "1";
	}
    </script>

<% 

    if (Component.ComponentError != ForumSearchComponentError.None)
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
		return;
	}
%>

<div class="forum-content">
<div class="forum-navigation-box forum-navigation-top">
	<div class="forum-page-navigation">
		<bx:IncludeComponent runat="server" ID="HeaderPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="top" CssClassPrefix="forum-" />
	</div>
</div>

<% 
if (Component.PostList.Count > 0)
{
    int lastTopicId = -1;
    int k = 0;
    for (int i = 0; i < Component.PostList.Count; i++)
    {
        var post = Component.PostList[i];
		post.Processor.HighlightingStartHtml = @"<b class=""forum-search-highlight"">";
        post.Processor.HighlightingEndHtml = "</b>";
        Component.SearchExpression.FillHighlightSegments(post.Post.Post,post.Processor.HighlightSegments);      
        if (Component.SortBy !=
                         ForumSearchComponentSorting.Topic ||
                          (Component.SortBy == ForumSearchComponentSorting.Topic && post.Topic.Id != lastTopicId))
                      {
                          lastTopicId = post.Topic.Id; k = 0; 
        
          %>
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
								<span title="<%= Encode(post.Post.DateCreate.ToString()) %>"><%= Encode(post.Post.DateCreate.ToString("g"))%></span>
							</div>
							<div class="forum-post-entry">
								<div class="forum-post-text" id="forum_post_text_<%= post.Post.Id %>"><%= post.Processor.Process(post.Post.Post)%></div>
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
			</table>
			
		<% if (k == 0)
         {	%>	
		</div>
	</div>
</div><% 
		 }
   
	k++;
	} 
}
else
{ %>
    

<div class="forum-header-box">
	<div class="forum-header-title"><span><%=GetMessage("Description") %></span></div>
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

	<div class="forum-navigation-box forum-navigation-bottom">
	<div class="forum-page-navigation">
		<bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="bottom" CssClassPrefix="forum-" />
	</div>
</div>	

</div>
