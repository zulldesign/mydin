<%@ Import Namespace="Bitrix.Main.Components" %>
<%@ Reference Control="~/bitrix/components/bitrix/user.list/component.ascx" %>
<%@ Reference Control="~/bitrix/modules/Bitrix.CommunitySite/solution_en/tools/Utils.ascx" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.Main.Components.UserListTemplate" %>

<% if (Component.ComponentError != UserListComponentError.None | Component.UserList.Count < 0)
	return; 
%>

<div class="rounded-block">
	<div class="corner left-top"></div><div class="corner right-top"></div>
	<div class="block-content">

	<h3><%= GetMessageRaw("Title") %></h3>

	<table cellspacing="0" class="user-stat-list">
	<% foreach (UserWrapper user in Component.UserList) {%>

		<tr>
			<td class="user-name"><a href="<%= HttpUtility.HtmlAttributeEncode(user.UserProfileUrl) %>"><%= HttpUtility.HtmlEncode(user.NameToShowUp) %></a></td>
			<td class="user-stat"><span><%= Bitrix.CommunitySite.Utils.GetTimePeriod(DateTime.Now - user.DateOfRegistration)%></span></td>
		</tr>
	<%
	}
	%>
	</table>

	</div>
	
	<div class="corner left-bottom"></div><div class="corner right-bottom"></div>
	
</div>	
