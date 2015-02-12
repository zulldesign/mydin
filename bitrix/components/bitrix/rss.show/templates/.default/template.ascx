<%@ Control Language="C#" AutoEventWireup="true" CodeFile="template.ascx.cs" Inherits="ComponentRssShowTemplate" %>
<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.IBlock" %>
<%@ Import Namespace="System.Collections.Generic" %>

<%@ Reference Control="~/bitrix/components/bitrix/rss.show/component.ascx"%>
<div class="rss-show">
<%= Results.Get<string>("msg", string.Empty)%>
<h2><%= Component.Name %></h2>
<%foreach(BXTemplateRssItem item in Component.Items){%>
	<%if (item.Image != null){%>
	  <img src="<%= item.Image.url%>" alt="<%= item.Name%>" /><br />
	<%}%>

	<%if(item.DisplayDate.Length>0){%>
		<p><%= item.DisplayDate%></p>
	<%} %>
	<%if (item.DetailUrl.Length > 0)
    {%>
		<a href="<%=item.DetailUrl %>"><%= item.Name %></a>
	<%}
    else
    {%>
		<%= item.Name %>
	<%} %>
	<p>
	<%= item.Description %>
	</p>
	<hr />
<%}%>
</div>
