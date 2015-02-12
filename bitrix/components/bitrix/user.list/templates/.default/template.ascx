<%@ Import Namespace="Bitrix.Main.Components" %>
<%@ Reference Control="~/bitrix/components/bitrix/user.list/component.ascx" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.Main.Components.UserListTemplate" %>

<% if (Component.ComponentError != UserListComponentError.None) { %>
	<div class="user-list-container">
		<div class="user-list-note-box user-note-error">
		<% foreach (string errorMsg in GetErrorMessages()){%>
		    <div class="user-list-note-box-text"><%= errorMsg%></div>
		<%} %>
		</div>
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
<div class="user-list-container">
    <table>
        <tbody>
        <% foreach (UserWrapper user in Component.UserList) {%>
            <tr>
                <td valign="top">
                    <a class="user-profile-img-link" href="<%= HttpUtility.HtmlAttributeEncode(user.UserProfileUrl) %>">
                        <img alt="<%= HttpUtility.HtmlAttributeEncode(user.NameToShowUp) %>" src="<%= HttpUtility.HtmlAttributeEncode(!string.IsNullOrEmpty(user.ImageFileUrl) ? user.ImageFileUrl : ResolveUrl("./images/nophoto.gif")) %>"  width="<%= user.ImageWidth.ToString() %>px" height="<%= user.ImageHeight.ToString() %>px" />
                    </a>
                </td>
                <td valign="top">
                    <div class="user-info1-container">
                        <a class="user-profile-text-link" href="<%= HttpUtility.HtmlAttributeEncode(user.UserProfileUrl) %>"><%= HttpUtility.HtmlEncode(user.NameToShowUp) %></a>
                    </div>
                </td>
                <td valign="top">
                    <div class="user-info2-container">
                        <%= GetMessage("Message.UserRegisteredAt")%> <%= user.DateOfRegistration.ToShortDateString() %>
                    </div>
                </td>
            </tr>
        <%} %>
        </tbody>
    </table>
</div>
<%if (Component.Paging.IsBottomPosition) {%>
    <div class="user-list-navigation-box user-navigation-bottom">
	    <div class="user-list-page-navigation">
		    <bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="bottom" />
	    </div>
    </div>
<%} %>
