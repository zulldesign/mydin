<%@ Import Namespace="Bitrix.Main.Components" %>
<%@ Reference Control="~/bitrix/components/bitrix/user.list/component.ascx" %>
<%@ Reference Control="~/bitrix/modules/Bitrix.CommunitySite/solution_en/tools/Utils.ascx" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.Main.Components.UserListTemplate" %>


<% if (Component.ComponentError != UserListComponentError.None) { %>
		<div class="user-list-note-box user-note-error">
		<% foreach (string errorMsg in GetErrorMessages()){%>
		    <div class="user-list-note-box-text"><%= errorMsg%></div>
		<%} %>
		</div>

	<% return; %>
<% } %>

<% if (Component.Paging.IsTopPosition) {%>
    <div class="user-list-navigation-box user-navigation-top">
	    <div class="user-list-page-navigation">
		    <bx:IncludeComponent runat="server" ID="HeaderPager" ComponentName="bitrix:system.pager"  Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="top" />
	    </div>
    </div>
<%} %>

<div class="content-list user-list">
<% 
	foreach (UserWrapper user in Component.UserList) 
    {
        %><div class="content-item">
			<div class="content-sidebar">
				<div class="content-date"><%= string.Format(GetMessageRaw("TimeOnSite"), Bitrix.CommunitySite.Utils.GetTimePeriod(DateTime.Now - user.DateOfRegistration)) %></div>
				<% if (user.User.CustomPublicValues["REGION"] != null) { %>
				<div class="content-city"><%= user.User.CustomPublicValues.GetHtml("REGION")%></div>
				<%} %>
			</div>
			<% if (user.User!=null && user.User.Image!=null ) { %>
			<div class="content-avatar">
				<a href="<%= HttpUtility.HtmlAttributeEncode(user.UserProfileUrl) %>" style="background: transparent url('<%=HttpUtility.HtmlAttributeEncode(user.ImageFileUrl)%>') no-repeat center center;"></a>
			</div>
			<%} else { %>
			<div class="content-avatar">
				<a href="<%= HttpUtility.HtmlAttributeEncode(user.UserProfileUrl) %>"></a>
			</div>
			<%} %>
			<div class="content-info">
				<div class="content-title"><a href="<%= HttpUtility.HtmlAttributeEncode(user.UserProfileUrl) %>"><%= HttpUtility.HtmlEncode(user.NameToShowUp) %></a> </div>
				
				<% if (user.User.CustomPublicValues["OCCUPATION"] != null) { %>
				<div class="content-signature"><%= user.User.CustomPublicValues.GetHtml("OCCUPATION")%></div>
				<%} %>
				
				<div class="content-rating" title="<%= GetMessageRaw("Rating") %>"><%= user.User.CustomPublicValues["RATING"] != null ? user.User.CustomPublicValues.GetHtml("RATING") : "0,00"%></div>
				
			</div>
        </div>
        <div class="hr"></div>
        <%
    }
%>
</div>

<%if (Component.Paging.IsBottomPosition) {%>
    <div class="user-list-navigation-box user-navigation-bottom">
	    <div class="user-list-page-navigation">
		    <bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="bottom" />
	    </div>
    </div>
<%} %>
