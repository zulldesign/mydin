<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DaysOfWeek.ascx.cs" Inherits="DaysOfWeek" %>
<table cellspacing="1" cellpadding="0" border="0" class="internal">
    <tbody>
        <tr class="heading">
			<% if (Bitrix.Services.BXLoc.CurrentLocale == "en") { %>
				<td><%= GetMessage(DaysOfWeekEnum.Sunday.ToString())%></td>
			<% } %>
            <td><%= GetMessage(DaysOfWeekEnum.Monday.ToString())%></td>
            <td><%= GetMessage(DaysOfWeekEnum.Tuesday.ToString())%></td>
			<td><%= GetMessage(DaysOfWeekEnum.Wednesday.ToString())%></td>
			<td><%= GetMessage(DaysOfWeekEnum.Thursday.ToString())%></td>
			<td><%= GetMessage(DaysOfWeekEnum.Friday.ToString())%></td>
			<td><%= GetMessage(DaysOfWeekEnum.Saturday.ToString())%></td>
			<% if (Bitrix.Services.BXLoc.CurrentLocale != "en") { %>
				<td><%= GetMessage(DaysOfWeekEnum.Sunday.ToString())%></td>
			<% } %>
        </tr>
		<tr>
			<% if (Bitrix.Services.BXLoc.CurrentLocale == "en") { %>
				<td><input type="checkbox" value="0" name="<%= UniqueID %>$Days" <% if (Days != null && Days.Contains(0)) { %>checked="checked"<% } %> /></td>
			<% } %>
			<td><input type="checkbox" value="1" name="<%= UniqueID %>$Days" <% if (Days != null && Days.Contains(1)) { %>checked="checked"<% } %> /></td>
			<td><input type="checkbox" value="2" name="<%= UniqueID %>$Days" <% if (Days != null && Days.Contains(2)) { %>checked="checked"<% } %> /></td>
			<td><input type="checkbox" value="3" name="<%= UniqueID %>$Days" <% if (Days != null && Days.Contains(3)) { %>checked="checked"<% } %> /></td>
			<td><input type="checkbox" value="4" name="<%= UniqueID %>$Days" <% if (Days != null && Days.Contains(4)) { %>checked="checked"<% } %> /></td>
			<td><input type="checkbox" value="5" name="<%= UniqueID %>$Days" <% if (Days != null && Days.Contains(5)) { %>checked="checked"<% } %> /></td>
			<td><input type="checkbox" value="6" name="<%= UniqueID %>$Days" <% if (Days != null && Days.Contains(6)) { %>checked="checked"<% } %> /></td>
			<% if (Bitrix.Services.BXLoc.CurrentLocale != "en") { %>
				<td><input type="checkbox" value="0" name="<%= UniqueID %>$Days" <% if (Days != null && Days.Contains(0)) { %>checked="checked"<% } %> /></td>
			<% } %>
        </tr>
    </tbody>
</table>

<script type="text/javascript" language="javascript">
</script>
