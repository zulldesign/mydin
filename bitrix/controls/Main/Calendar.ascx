<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Calendar.ascx.cs" Inherits="bitrix_ui_Calendar" %>
<%@ Import Namespace="Bitrix.UI" %>
<%--<a title="Календарь" href="javascript:void(0);"><img onmouseout="this.className = this.className.replace(/\s*calendar-icon-hover/ig, '');" onmouseover="this.className+=' calendar-icon-hover';" runat="server" id="imgBtn" class="calendar-icon" alt="Календарь" src="../../App_Themes/AdminTheme/images/calendar/icon.gif"/></a>--%>
<%
	
	string onClick = string.Empty;
	if (!string.IsNullOrEmpty(TextBoxId))
	{
		Control control = Bitrix.UI.BXControlUtility.FindControl(this, TextBoxId);
		if (control != null)
			onClick = "jsCalendar.Show(this, '" + control.ClientID + "', '', '', false, " + UnixTimeStamp() + ");";
	}
%>
<a title="<%= GetMessage("LinkTitle.Calendar") %>" href="javascript:void(0);"><img onmouseout="this.className = this.className.replace(/\s*calendar-icon-hover/ig, '');" onmouseover="this.className+=' calendar-icon-hover';" onclick="<%= Encode(onClick) %>" src="<%= Encode(BXThemeHelper.AddAbsoluteThemePath("images/calendar/icon.gif")) %>" class="calendar-icon" style="border:0 none;" alt="<%= GetMessage("ImageAlt.Calendar") %>" /></a>
