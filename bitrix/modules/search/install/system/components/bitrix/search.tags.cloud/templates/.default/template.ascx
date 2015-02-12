<%@ Reference Control="~/bitrix/components/bitrix/search.tags.cloud/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Search.Components.SearchTagsCloudTemplate" %>
<%@ Import Namespace="Bitrix.Search.Components" %>
<%@ Import Namespace="System.Drawing" %>
<div>
<noindex>
<% foreach(SearchTagsCloudComponent.TagInfo tag in Component.Tags) { %>
	<% if (!string.IsNullOrEmpty(tag.Href))	{ %>
	<a 
		onmouseover="this.style.borderBottom='solid 1px'"
		onmouseout="this.style.borderBottom=''"
		href="<%= tag.Href %>" 
		style="text-decoration: none; margin-right: 5px;<% if (tag.FontSize > 0) { %> font-size: <%= tag.FontSize %>px;<% }; if (tag.Color != Color.Empty) { %> color: <%= ColorTranslator.ToHtml(tag.Color) %>;<% } %>"
		rel="nofollow"
	><%= tag.Tag.Name %></a>
	<% } else { %>
	<span style="margin-right: 5px;<% if (tag.FontSize > 0) { %> font-size: <%= tag.FontSize %>px;<% }; if (tag.Color != Color.Empty) { %> color: <%= ColorTranslator.ToHtml(tag.Color) %>;<% } %>"><%= tag.Tag.Name %></span>
	<% } %>
<% } %>
</noindex>
</div>