<%@ Import Namespace="Bitrix.Forum.Components" %>
<%@ Reference Control="~/bitrix/components/bitrix/forum.topic.last/component.ascx" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.Forum.Components.ForumTopicLastTemplate" %>

<% if (Component.ComponentError != ForumTopicLastComponentError.None || Component.TopicList.Count < 1)
	   return;
%>

<div class="rounded-block">
	<div class="corner left-top"></div><div class="corner right-top"></div>
	<div class="block-content">
		<h3><%= GetMessage("Title") %></h3>
			<ul class="last-items-list">

			<% foreach (ForumTopicWrapper topic in Component.TopicList) {%>
				<li><<%= topic.Topic.AuthorId > 0 ? "a href=\"" + topic.AuthorProfileUrl + "\"" : "span" %> class="item-author"><%=topic.AuthorName %></<%= topic.Topic.AuthorId > 0 ? "a" : "span" %>> <i>&gt;</i> <a href="<%= topic.ForumReadUrl %>" class="item-category"><%= topic.ForumName %></a><i>&gt;</i> <a href="<%= topic.TopicReadUrl %>" class="item-name"><%= topic.Name %></a></li>          
			<% }%>

			</ul>
		</div>
	<div class="corner left-bottom"></div><div class="corner right-bottom"></div>
</div>	