<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>
<%@ Reference Control="~/bitrix/components/bitrix/blog.category.list/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.Blog.Components.BlogCategoryListTemplate" %>

<% 
    IList<ColumnData> columns = Columns;
    int columnCount = columns.Count;
    int maxColumnItemCount = GetMaxColumnItemsCount();
%>

<table class="blog-groups" width="100%" cellspacing="0" cellpadding="4" border="0">
    <tbody><% 
    for (int i = 0; i < maxColumnItemCount; i++) { 
        %><tr><% 
        for (int j = 0; j < columnCount; j++) {
            %><td valign="top" width="<%= string.Format(System.Globalization.NumberFormatInfo.InvariantInfo, "{0:.##}", 100.0/columnCount) %>%"><%
            ColumnData column = columns[j];
            int itemCount = column.Items.Count;
            BlogCategoryListComponent.ListItem item = column.Items.Count - 1 >= i ? column.Items[i] : null;
            if (item != null) {
                %><a class="blog-group-icon" href="<%= item.BlogCategoryUrl %>"></a>
                <a href="<%= item.BlogCategoryUrl %>"><%= item.CategoryName %></a><%                
            } 
            %></td><% 
        } 
        %></tr><%
    } 
    %></tbody>
</table>
