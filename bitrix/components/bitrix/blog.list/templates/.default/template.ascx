<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>
<%@ Import Namespace="Bitrix.IO" %>
<%@ Reference Control="~/bitrix/components/bitrix/blog.list/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.Blog.Components.BlogListTemplate" %>

<% if (Component.ComponentErrors != BlogListComponent.Error.None) { %>
	<div class="blog-content">
		<div class="blog-note-box blog-note-error">
			<div class="blog-note-box-text"><%= Component.GetErrorMessageHtml() %></div>
		</div>
	</div>
	<% return; %>
<% } %>

<div class="blog-content">
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

	if (Component.Paging.IsTopPosition)
	{                                                                             	
%>
    <div class="blog-navigation-box blog-navigation-top">
	    <div class="blog-page-navigation">
		    <bx:IncludeComponent runat="server" ID="HeaderPager" ComponentName="bitrix:system.pager"  Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="top" CssClassPrefix="blog-" />
	    </div>
    </div>
<%
	}
	%>
	<div class="blog-list">
	<%
    foreach (BlogListComponent.ListItem item in items)
    {
        %><div class="blog-list-item">
			<% if (item.Blog.Owner != null && item.Blog.Owner.User != null && item.Blog.Owner.User.Image != null) {
				BXFile image = item.Blog.Owner.User.Image;
			%>
			<div class="blog-author-avatar" width="<%= image.Width %>" height="<%= image.Height %>" style="background: url('<%= HttpUtility.HtmlAttributeEncode(image.FilePath) %>') no-repeat center center;"></div>
			<%} else { %>
			<div class="blog-author-avatar"></div>
			<%} %>
			
            <div class="blog-author">
                <a class="blog-author-icon" href="<%= item.BlogOwnerProfileUrl %>"></a>
                <a href="<%= item.BlogUrl %>"><%= item.OwnerDisplayName %></a>
            </div>
            <div class="blog-clear-float"></div>
            <div class="blog-list-title">
               <a href="<%= item.BlogUrl %>"><%= item.BlogName %></a> 
            </div>
            <div class="blog-list-content"><%= item.BlogDescription %></div>
            <div class="blog-clear-float"></div>
            
            <div class="blog-register-date"><label><%= GetMessage("BlogCreated") %> </label><span><%= item.Blog.DateCreated.ToString("d") %></span></div>
        </div><%
    }

	%></div><%
	
	if (Component.Paging.IsBottomPosition)
	{
%>
    <div class="blog-navigation-box blog-navigation-bottom">
	    <div class="blog-page-navigation">
		    <bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="bottom" CssClassPrefix="blog-" />
	    </div>
    </div>
<%
	} 
%>
</div>
