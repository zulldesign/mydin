<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>
<%@ Import Namespace="Bitrix.IO" %>
<%@ Reference Control="~/bitrix/components/bitrix/blog.list/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.Blog.Components.BlogListTemplate" %>

<% if (Component.ComponentErrors != BlogListComponent.Error.None) { 
	return;
} %>

<div class="rounded-block">
	<div class="corner left-top"></div><div class="corner right-top"></div>
	<div class="block-content">

	<h3><%= GetMessage("Title") %></h3>

		<ul class="last-items-list">
		<%
			foreach (BlogListComponent.ListItem item in Component.Items)
			{
		%>
			<li><a class="item-author" href="<%= item.BlogOwnerProfileUrl %>"><%= item.OwnerDisplayName %></a><i>&gt;</i> 
			<a title="<%= item.BlogName %>" class="item-name" href="<%= item.BlogUrl %>"><%= item.BlogName %></a></li>
		<%} %>
		</ul>

	</div>
	
	<div class="corner left-bottom"></div><div class="corner right-bottom"></div>
	
</div>	

	
