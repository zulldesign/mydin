<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>
<%@ Reference Control="~/bitrix/components/bitrix/blog.list/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.Blog.Components.BlogListTemplate" %>

<% if (Component.ComponentErrors != BlogListComponent.Error.None)
	   return; 
%>

<%
    int categoryId = Component.CategoryId;
    BXBlogCategory category = categoryId > 0 ? Component.Category : null;
    if (categoryId > 0 && category == null)
    { 
        %><%= string.Format(GetMessage("CategoryIsNotFound"), categoryId)%><%
        return;
    }  
                                                                               
    IList<BlogListComponent.ListItem> items = Component.Items;
    if (items.Count ==0)
    {
        %><%=  GetMessage(category != null ? "NoItemsInCategory" : "NoItems") %><%
        return;
    }
%>

<div class="blog-mainpage-blogs">  
<%	foreach (BlogListComponent.ListItem item in items)
    {
        %><div class="blog-mainpage-item">
            <div class="blog-author">
                <a class="blog-author-icon" href="<%= item.BlogOwnerProfileUrl %>"></a>
                <a href="<%= item.BlogUrl %>"><%= item.OwnerDisplayName %></a>
            </div>
            <div class="blog-clear-float"></div>
            <div class="blog-mainpage-title">
               <a href="<%= item.BlogUrl %>"><%= item.BlogName %></a> 
            </div>
            <div class="blog-mainpage-content"><%= item.GetBlogDescription(128)%></div>
            <div class="blog-clear-float"></div>
        </div><%
    }
%>
</div>
