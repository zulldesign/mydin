<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>
<%@ Import Namespace="Bitrix.IO" %>
<%@ Reference Control="~/bitrix/components/bitrix/blog.list/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.Blog.Components.BlogListTemplate" %>

<% if (Component.ComponentErrors != BlogListComponent.Error.None) { %>
	<div class="content-list blog-list">
		<div class="blog-note-box blog-note-error">
			<div class="blog-note-box-text"><%= Component.GetErrorMessageHtml() %></div>
		</div>
	</div>
	<% return; %>
<% } %>


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
<div class="content-list blog-list">
<%
foreach (BlogListComponent.ListItem item in items)
{
    %>
    
    <div class="content-item">
		<div class="content-sidebar">
			<div class="content-date"><%= GetMessage("BlogCreated") %> <%= item.Blog.DateCreated.ToString("d") %></div>
		</div>
		
		<% if (item.Blog.Owner != null && item.Blog.Owner.User != null && item.Blog.Owner.User.Image != null) {
			BXFile image = item.Blog.Owner.User.Image;
		%>
		<div class="content-avatar">
			<a href="<%=item.BlogOwnerProfileUrl %>" style="background: transparent url('<%=HttpUtility.HtmlAttributeEncode(image.FilePath)%>') no-repeat center center;"></a>
		</div>
		<%} else { %>
		<div class="content-avatar">
			<a href="<%=item.BlogOwnerProfileUrl %>"></a>
		</div>
		<%} %>
		<div class="content-info">
			<div class="content-author"><a href="<%= item.BlogOwnerProfileUrl %>"><%=item.OwnerDisplayName %></a></div>
			<div class="content-title"><a href="<%= item.BlogUrl %>"><%= item.BlogName %></a> </div>
			<div class="content-description"><%= item.BlogDescription %></div>
			
			<div class="content-rating" title="<%= GetMessage("Rating") %>"><%= item.Blog.CustomPublicValues["RATING"] != null ? item.Blog.CustomPublicValues.GetHtml("RATING") : "0,00"%></div>			
		</div>
    </div>
    <div class="hr"></div>
    
    <%
}
%>
</div>
	
<%if (Component.Paging.IsBottomPosition) {%>
    <div class="blog-navigation-box blog-navigation-bottom">
	    <div class="blog-page-navigation">
		    <bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="bottom" CssClassPrefix="blog-" />
	    </div>
    </div>
<% } %>

