<%@ Reference Control="~/bitrix/components/bitrix/forum.list/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.Forum.Components.ForumListTemplate" %>
<%@ Import Namespace="Bitrix.Forum.Components" %>

<div class="forum-content">

<div class="forum-header-box">
	<div class="forum-header-title"><span><%= Component.ForumListTitle %></span></div>
</div>

<div class="forum-block-container">
	<div class="forum-block-outer">
		<div class="forum-block-inner">
			<table cellspacing="0" class="forum-table forum-forum-list">
	<%
	int itemIdex = 0;
	bool firstCategory = true;
	int? categoryId = null;

	for (int i = 0; i < Component.ForumList.Count; i++)
	{
		ForumListComponent.ForumListItem item = Component.ForumList[i];
		
		if (categoryId.HasValue && categoryId != item.Forum.CategoryId)
		{
			itemIdex = 0;
			firstCategory = false;
		}

		itemIdex++;
		categoryId = item.Forum.CategoryId;
				
		StringBuilder rowClass = new StringBuilder();
		if (itemIdex == 1)
			rowClass.Append("forum-row-first");

		if (i == Component.ForumList.Count-1 || (i + 1 < Component.ForumList.Count && Component.ForumList[i + 1].Forum.CategoryId != categoryId))
		{
			if (rowClass.Length > 0)
				rowClass.Append(" ");	
			rowClass.Append("forum-row-last");
		}

		if (rowClass.Length > 0)
			rowClass.Append(" ");
								
		if ( itemIdex % 2 == 0)
			rowClass.Append("forum-row-even");
		else
			rowClass.Append("forum-row-odd");
				
		string iconClass = item.IsNewPostExists ? "forum-icon-newposts" : "forum-icon-default";

		if (itemIdex == 1) { 
			
			if (!firstCategory) {%>
			</tbody>
			<tbody class="forum-category-separator">
				<tr>
					<td class="forum-category-separator" colspan="5"></td>
				</tr>
			</tbody>
			<%}%>
			<thead>
				<tr>
					<th class="forum-column-title" colspan="2"><div class="forum-head-title"><span><%= (item.Category == null || String.IsNullOrEmpty(item.Category.Name) ? GetMessage("DefaultCategoryTitle") : item.Category.Name)%></span></div></th>
					<th class="forum-column-topics"><span><%= GetMessage("TopicsColumnTitle")%></span></th>
					<th class="forum-column-replies"><span><%= GetMessage("RepliesColumnTitle")%></span></th>
					<th class="forum-column-lastpost"><span><%= GetMessage("LastPostColumnTitle")%></span></th>
				</tr>
			</thead>
			<tbody>
		<%}%>
 				<tr class="<%= rowClass.ToString() %>">
					<td class="forum-column-icon">
						<div class="forum-icon-container">
							<div class="forum-icon <%= iconClass %>"><!-- ie --></div>
						</div>
					</td>
					<td class="forum-column-title">
						<div class="forum-item-info">
							<div class="forum-item-name"><span class="forum-item-title"><a href="<%= item.TopicListHref %>"><%= item.Forum.Name %></a></span></div>
						<% if (!String.IsNullOrEmpty(item.Forum.Description)) { %>
							<span class="forum-item-desc"><%= item.Forum.Description %></span>
						<% } %>
							
						<% if (item.Forum.QueuedTopics > 0 || item.Forum.QueuedReplies > 0) { %>
							<div class="forum-moderator-stat"><%= GetMessage("QueuedTopics")%>:&nbsp;<span><%= item.Forum.QueuedTopics %></span>, <%= GetMessage("QueuedPosts")%>:&nbsp;<span><%= item.Forum.QueuedReplies %></span></div>
						<% } %>
						</div>
					</td>
					<td class="forum-column-topics"><span><%= item.Forum.Topics.ToString("#,0")%></span></td>
					<td class="forum-column-replies"><span><%= item.Forum.Replies.ToString("#,0")%></span></td>
					<td class="forum-column-lastpost">
						<% if (item.Forum.LastPostId > 0) { %>
						<div class="forum-lastpost-box">
							<span class="forum-lastpost-title"><a href="<%= item.LastPostHref %>" title="<%= item.LastTopicTitleFull %>"><%= item.LastTopicTitleHtml %> <span class="forum-lastpost-author">(<%= item.LastTopicAuthorHtml %>)</span></a></span>
							<span class="forum-lastpost-date" title="<%= item.Forum.LastPostDate %>"><%= item.Forum.LastPostDate.ToString("g") %></span>
						</div>
						<%} %>
						&nbsp;
					</td>
				</tr>
		<% } 
		
		if (Component.ForumList.Count == 0) {%>
			<tbody>
				<tr class="forum-row-first forum-row-last forum-row-odd">
					<td class="forum-column-alone">
						<div class="forum-empty-message"><%= GetMessage("BoardIsEmpty")%></div>
					</td>
				</tr>
		<%} %>
			</tbody>
			<tfoot>
				<tr>
					<td colspan="5" class="forum-column-footer">
						<div class="forum-footer-inner">
	<% 
        if (Component.FooterLinks.Count == 0)
        {
            %>&nbsp;<% 
        }   
	for (int i = 0; i < Component.FooterLinks.Count; i++) 
	{ 
		ForumListComponent.LinkInfo link = Component.FooterLinks[i];
		if (i != 0) 
        { 
            %>&nbsp;&nbsp;<% 
        } 
        %><span class="<%= string.Concat(link.CssClass ?? (string.Concat("forum-option-", i)), i == 0 ? " forum-footer-option-first" : string.Empty, i == Component.FooterLinks.Count - 1 ? " forum-footer-option-last" : string.Empty)  %>">
            <a href="<%= link.Href %>"><%= link.Title %></a>
          </span><%
    } %>						
						</div>
					</td>
				</tr>
			</tfoot>
			</table>
		</div>
	</div>
</div>

</div>