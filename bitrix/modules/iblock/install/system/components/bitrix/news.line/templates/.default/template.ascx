<%@ Reference Control="~/bitrix/components/bitrix/news.line/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="NewsLineTemplate" %>

<div class="news-line">
<%
if (Component.Items != null)
{
	foreach (NewsLineItem item in Component.Items)
	{
		%><div class="news-line-item">
			<%if (!String.IsNullOrEmpty(item.DisplayDate))
				{
					%><span class="news-date-time"><%= item.DisplayDate %>&nbsp;</span><%
				}
			 %>
			<a href="<%= item.DetailUrl %>"><%= item.Name %></a>
		</div><%
	}
}
%>
</div>