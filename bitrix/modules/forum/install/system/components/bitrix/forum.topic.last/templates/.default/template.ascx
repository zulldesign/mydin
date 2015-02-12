<%@ Import Namespace="Bitrix.Forum.Components" %>
<%@ Reference Control="~/bitrix/components/bitrix/forum.topic.last/component.ascx" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.Forum.Components.ForumTopicLastTemplate" %>

<% if (Component.ComponentError != ForumTopicLastComponentError.None) { %>
<div class="forum-topic-last-container">
	<div class="forum-topic-last-note-box forum-topic-note-error">
	<% foreach (string errorMsg in GetErrorMessages()){%>
	    <div class="forum-topic-last-note-box-text"><%= errorMsg%></div>
	<%} %>
	</div>
</div>
	<% return; %>
<% } %>

<% if (Component.Paging.IsTopPosition) {%>
<div class="forum-topic-last-navigation-box forum-topic-navigation-top">
    <div class="forum-topic-last-page-navigation">
	    <bx:IncludeComponent runat="server" ID="HeaderPager" ComponentName="bitrix:system.pager"  Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="top" />
    </div>
</div>
<%} %>
<div class="forum-topic-last-container">
    <table cellpadding="0px" cellspacing="0px" class="forum-topic-last-table">
        <thead>
            <tr>
                <th class="forum-topic-column forum-topic-column-first forum-topic-column-title"><%= GetMessage("Topic") %></th>
                <th class="forum-topic-column forum-topic-column-forum"><%= GetMessage("Forum") %></th>
                <th class="forum-topic-column forum-topic-column-last forum-topic-column-author"><%= GetMessage("Author")%></th>
            </tr>
        </thead>
        <tbody>
        <% foreach (ForumTopicWrapper topic in Component.TopicList) {%>
            <tr class="forum-topic-row">
                <td class="forum-topic-column forum-topic-column-first forum-topic-column-title">
                    <div class="forum-topic-name"><a href="<%= topic.TopicReadUrl %>"><%= topic.Name %></a></div>
                </td>
                <td class="forum-topic-column forum-topic-column-forum">
                    <div class="forum-name"><a href="<%= topic.ForumReadUrl %>"><%= topic.ForumName %></a></div>
                </td>
                <td class="forum-topic-column forum-topic-column-last forum-topic-column-author">
                    <div class="author-name"><a href="<%= topic.AuthorProfileUrl %>"><%= topic.AuthorName %></a></div>
                </td>                
            </tr>            
        <% }%>
        </tbody>
    </table>
</div>


<%if (Component.Paging.IsBottomPosition) {%>
<div class="forum-topic-last-navigation-box forum-topic-navigation-bottom">
    <div class="forum-topic-last-page-navigation">
	    <bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="bottom" />
    </div>
</div>
<%} %>