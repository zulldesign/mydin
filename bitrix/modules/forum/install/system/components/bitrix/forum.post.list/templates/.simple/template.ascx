<%@ Import Namespace="Bitrix.Forum.Components" %>
<%@ Reference Control="~/bitrix/components/bitrix/forum.post.list/component.ascx" %>
<%@ Control Language="C#" EnableViewState="false" AutoEventWireup="false" ClassName="template" Inherits="Bitrix.Forum.Components.ForumPostListTemplate" %>

<% if (Component.ComponentError != ForumPostListComponentError.None) { %>
<div class="forum-post-list-container">
	<div class="forum-post-list-note-box forum-post-list-note-error">
	<% foreach (string errorMsg in GetErrorMessages()){%>
	    <div class="forum-post-list-note-box-text"><%= errorMsg%></div>
	<%} %>
	</div>
</div>
	<% return; %>
<% } %>

<% if (Component.Paging.IsTopPosition) {%>
<div class="forum-post-list-navigation-box forum-post-list-navigation-top">
    <div class="forum-post-list-page-navigation">
	    <bx:IncludeComponent runat="server" ID="HeaderPager" ComponentName="bitrix:system.pager"  Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="top" />
    </div>
</div>
<%} %>

<div class="forum-post-list-container">
<% foreach (var post in Component.PostList) {%>
    <div class="forum-post-list-post">
        <div class="forum-post-list-post-header">
            <a class="forum-post-list-post-forum-link" href="<%= post.ForumReadUrl %>"><%= post.ForumName %></a> / <a class="forum-post-list-post-topic-link" href="<%= post.TopicReadUrl %>"><%= post.TopicName %></a> (<a class="forum-post-list-post-author-link" href="<%= post.AuthorProfileUrl %>"><%= post.AuthorName%></a>, <a class="forum-post-list-post-date-link" href="<%= post.PostReadUrl %>"><%= post.DateOfCreation.ToShortDateString() %></a>) 
        </div>
        <div class="forum-post-list-post-preview">
            <%= post.GetContentPreview(256) %><a href="<%= post.PostReadUrl %>">...</a>
        </div>
        <div class="forum-post-list-post-gotopost">
            <a class="forum-post-list-post-gotopost-link" href="<%= post.PostReadUrl %>"><%= GetMessage("GotoPostLinkText")%>...</a>
        </div>
    </div>
<%} %>
</div>

<%if (Component.Paging.IsBottomPosition) {%>
<div class="forum-post-list-navigation-box forum-post-list-navigation-bottom">
    <div class="forum-post-list-page-navigation">
	    <bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="bottom" />
    </div>
</div>
<%} %>