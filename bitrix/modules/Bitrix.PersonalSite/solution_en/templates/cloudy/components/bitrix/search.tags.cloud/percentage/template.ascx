<%@ Reference Control="~/bitrix/components/bitrix/search.tags.cloud/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Search.Components.SearchTagsCloudTemplate" %>
<%@ Import Namespace="Bitrix.Search.Components" %>
<%@ Import Namespace="System.Drawing" %>
<noindex>
<% foreach(SearchTagsCloudComponent.TagInfo tag in Component.Tags) { %>
	<a rel="nofollow" href="<%= tag.Href %>" style="<% if (tag.FontSize > 0) { %> font-size: <%= tag.FontSize %>%;<% }; if (tag.Color != Color.Empty) { %> color: <%= ColorTranslator.ToHtml(tag.Color) %>;<% } %>"><%= tag.Tag.Name %></a>
<% } %>
</noindex>