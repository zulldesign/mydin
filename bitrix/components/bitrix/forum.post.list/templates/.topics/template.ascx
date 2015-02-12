<%@ Reference Control="~/bitrix/components/bitrix/forum.post.list/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.Forum.Components.ForumPostListTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.Forum.Components" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="Bitrix.Security" %>
<%@ Import Namespace="Bitrix.Services" %>

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
		return;
	}
%>

<div class="forum-content">

<div class="forum-navigation-box forum-navigation-top">
	<div class="forum-page-navigation">
		<bx:IncludeComponent runat="server" ID="HeaderPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="top" CssClassPrefix="forum-" />
	</div>
	<div class="forum-new-post">
	</div>
	<div class="forum-clear-float"></div>
</div>

<div class="forum-header-box">
	<div class="forum-header-title"><span><%= GetMessage("HeaderText."+Component.ComponentDisplayMode.ToString()) %></span></div>
</div>

<div class="forum-block-container">
	<div class="forum-block-outer">
		<div class="forum-block-inner">
			<table cellspacing="0" class="forum-table forum-topic-list">
<% if (Component.PostList.Count > 0) { %>
			<thead>
				<tr>
					<th class="forum-column-title" colspan="2"><div class="forum-head-title"><span><%= GetMessage("Topics") %></span></div></th>
					<th class="forum-column-replies"><span><%= GetMessage("Replies") %></span></th>
					<th class="forum-column-views"><span><%= GetMessage("Views") %></span></th>
					<th class="forum-column-lastpost"><span><%= GetMessage("LastPost") %></span></th>
				</tr>
			</thead>
			<tbody>
<%
	StringBuilder cssClass = new StringBuilder();
	StringBuilder statusHtml = new StringBuilder();
	string statusFormat = @"<span class=""{0}"">{1}</span>";
	for (int i = 0; i < Component.PostList.Count; i++)
	{
		var info = Component.PostList[i];
		cssClass.Length = 0;
		if (i == 0)
			cssClass.Append("forum-row-first ");
		if (i == Component.PostList.Count - 1)
			cssClass.Append("forum-row-last ");
		cssClass.Append(i % 2 == 0 ? "forum-row-odd" : "forum-row-even"); //because of zero-based index
			
		
		statusHtml.Length = 0;
		if (info.Topic.StickyIndex > 0)
		{
			cssClass.Append(" forum-row-sticky");
			statusHtml.AppendFormat(statusFormat, "forum-status-sticky", GetMessageRaw("Status.Sticky"));
		}
		if (info.Topic.Closed)
		{
			cssClass.Append(" forum-row-closed");
			if (statusHtml.Length > 0) 
				statusHtml.Append(", ");
			statusHtml.AppendFormat(statusFormat, "forum-status-closed", GetMessageRaw("Status.Closed"));
		}
		if (info.Topic.MovedTo > 0)
		{
			
			cssClass.Append(" forum-row-moved");
			if (statusHtml.Length > 0)
				statusHtml.Append(", ");
			statusHtml.AppendFormat(statusFormat, "forum-status-moved", GetMessageRaw("Status.Moved"));
		}
		if (!info.Topic.Approved)
		{
			cssClass.Append(" forum-row-hidden");
		}

		string iconCss;
		string iconTitle;
		if (info.Topic.MovedTo > 0)
		{
			iconCss = "forum-icon-sticky";
			iconTitle = GetMessageRaw("Title.Moved");
		}
		else if (info.Topic.Closed)
		{
			if (info.Topic.StickyIndex > 0)
			{
				iconCss = "forum-icon-sticky-closed" + (info.TopicHasNewPosts ? "-newposts" : string.Empty);
				iconTitle = GetMessageRaw("Title.StickyClosed") + (info.TopicHasNewPosts ? (" (" + GetMessageRaw("Title.HasNewMessages") + ")") : string.Empty);
			}
			else
			{
				iconCss = "forum-icon-closed" + (info.TopicHasNewPosts ? "-newposts" : string.Empty);
				iconTitle = GetMessageRaw("Title.Closed") + (info.TopicHasNewPosts ? (" (" + GetMessageRaw("Title.HasNewMessages") + ")") : string.Empty);
			}
		}
		else if (info.Topic.StickyIndex > 0)
		{
			iconCss = "forum-icon-sticky" + (info.TopicHasNewPosts ? "-newposts" : string.Empty);
			iconTitle = GetMessageRaw("Title.Sticky") + (info.TopicHasNewPosts ? (" (" + GetMessageRaw("Title.HasNewMessages") + ")") : string.Empty);
		}
		else
		{
			iconCss = "forum-icon" + (info.TopicHasNewPosts ? "-newposts" : "-default");
			iconTitle = String.Empty;
		}

		
		if (statusHtml.Length > 0)
			statusHtml.Append(":&nbsp;");
%>
 				<tr class="<%= cssClass %>">
					<td class="forum-column-icon">
						<div class="forum-icon-container">
							<div class="forum-icon <%= iconCss %>" title="<%= iconTitle %>"><!-- ie --></div>
						</div>
					</td>
					<td class="forum-column-title">
						<div class="forum-item-info">
							<div class="forum-item-name"><%= statusHtml %><span class="forum-item-title"><a href="<%= info.TopicReadUrl %>"><%= info.TopicTitleHtml %></a></span></div>
							<% if (!string.IsNullOrEmpty(info.TopicDescriptionHtml)) { %>
							<span class="forum-item-desc"><%= info.TopicDescriptionHtml%></span><span class="forum-item-desc-sep">&nbsp;&middot; </span>
							<% } %>
							<span class="forum-item-author"><span><%= GetMessage("Author") %>:</span>&nbsp;<%= info.Topic.AuthorId == 0 ? GetMessageRaw("Guest") : string.Empty %><%= info.AuthorName %></span>
						    <span class="forum-item-forumname">
						        <span><%=GetMessage("InForum") %></span>&nbsp;<a href="<%=info.ForumReadUrl %>"><%=info.ForumName%></a>
						    </span>
						</div>
					</td>
				<% if (info.Topic.MovedTo == 0) { %>
					<td class="forum-column-replies"><span><%= info.TopicReplies.ToString("#,0") %></span></td>
					<td class="forum-column-views"><span><%= info.Topic.Views.ToString("#,0") %></span></td>
				<% } else { %>
					<td class="forum-column-replies"><span>&nbsp;</span></td>
					<td class="forum-column-views"><span>&nbsp;</span></td>
				<% } %>
					<td class="forum-column-lastpost">
					<% if (info.Topic.MovedTo == 0) { %>
						<div class="forum-lastpost-box">
							<span class="forum-lastpost-date"><a href="<%= info.TopicLastPostHref %>"><%= info.Topic.LastPostDate.ToString("g") %></a></span>
							<span class="forum-lastpost-title"><span class="forum-lastpost-author">
							<%= info.Topic.LastPosterId == 0 ? GetMessageRaw("Guest") : string.Empty %><%= info.TopicLastPosterNameHtml %></span></span>
						</div>
					<% } %>
					</td>
				</tr>
<%
	}
%>
			</tbody>
			<tfoot>
				<tr>
					<td colspan="5" class="forum-column-footer">
						<div class="forum-footer-inner">&nbsp;</div>
					</td>
				</tr>
			</tfoot>
<% } else { %>
			<tbody>
 				<tr class="forum-row-first forum-row-last forum-row-odd">
					<td class="forum-column-alone">
						<div class="forum-empty-message">
						<%=GetMessage("EmptyMessage")%>
						</div>
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
<% } %>
			</table>
		</div>
	</div>
</div>
<div class="forum-navigation-box forum-navigation-bottom">
	<div class="forum-page-navigation">
		<bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager"  Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="bottom" CssClassPrefix="forum-" />
	</div>

	<div class="forum-new-post">
	</div>

	<div class="forum-clear-float"></div>
</div>

</div>

<script runat="server">
	
	private const string TargetParameter = "_target";
	private const string OperationParameter = "_action";
	private const string PostIdParameter = "_id";

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);



		if (Component.ComponentError != ForumPostListComponentError.None)
			return;

		if (Request == null)
			return;

        if (Component.PostList.Count == 0)
            Results["PagingTotalRecordCount"] = "1";
		
		string target = Request.QueryString[TargetParameter];
		if (target != "forum")
			return;
		
		if (Request.QueryString[OperationParameter] == null)
			return;
		
		if (!BXCsrfToken.CheckTokenFromRequest(Request.QueryString))
			return;



	}


	private string GetRedirectUrl()
	{
		NameValueCollection query = HttpUtility.ParseQueryString(BXSefUrlManager.CurrentUrl.Query);
		query.Remove(TargetParameter);
		query.Remove(OperationParameter);
		query.Remove(BXCsrfToken.TokenKey);
		
		UriBuilder uri = new UriBuilder(BXSefUrlManager.CurrentUrl);
		if (query.Count > 0)
			uri.Query = query.ToString();
		
		return uri.Uri.ToString();
	}
	
</script>
