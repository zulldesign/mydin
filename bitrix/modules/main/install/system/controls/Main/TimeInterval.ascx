<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TimeInterval.ascx.cs" Inherits="bitrix_ui_TimeInterval" %>
<%@ Import Namespace="Bitrix.UI" %>
<asp:TextBox ID="txtStartDate" runat="server" />
<%--
<a title="Календарь" href="javascript:void(0);"><img onmouseout="this.className = this.className.replace(/\s*calendar-icon-hover/ig, '');" onmouseover="this.className+=' calendar-icon-hover';" onclick="jsCalendar.Show(this, '<%= txtStartDate.ClientID  %>', '', '', false, <%= UnixTimeStamp() %>);" class="calendar-icon" alt="Календарь" src="../../App_Themes/AdminTheme/images/calendar/icon.gif"/></a>
zg, 25.04.2008
--%>
<a title="<%= GetMessage("LinkTitle.Calendar") %>" href="javascript:void(0);"><img onmouseout="this.className = this.className.replace(/\s*calendar-icon-hover/ig, '');" onmouseover="this.className+=' calendar-icon-hover';" onclick="jsCalendar.Show(this, '<%= txtStartDate.ClientID  %>', '', '', false, <%= UnixTimeStamp() %>);" class="calendar-icon" style="border:0 none;" alt="<%= GetMessage("ImageAlt.Calendar") %>" src='<%= Encode(BXThemeHelper.AddAbsoluteThemePath("images/calendar/icon.gif")) %>'/></a>

- 
<asp:TextBox ID="txtEndDate" runat="server" />
<%--<a title="Календарь" href="javascript:void(0);"><img onmouseout="this.className = this.className.replace(/\s*calendar-icon-hover/ig, '');" onmouseover="this.className+=' calendar-icon-hover';" onclick="jsCalendar.Show(this, '<%= txtEndDate.ClientID  %>', '', '', false, <%= UnixTimeStamp() %>);" class="calendar-icon" alt="Календарь" src="../../App_Themes/AdminTheme/images/calendar/icon.gif"/></a>--%>
<a title="<%= GetMessage("LinkTitle.Calendar") %>" href="javascript:void(0);"><img onmouseout="this.className = this.className.replace(/\s*calendar-icon-hover/ig, '');" onmouseover="this.className+=' calendar-icon-hover';" onclick="jsCalendar.Show(this, '<%= txtEndDate.ClientID  %>', '', '', false, <%= UnixTimeStamp() %>);" class="calendar-icon" style="border:0 none;" alt="<%= GetMessage("ImageAlt.Calendar") %>" src='<%= Encode(BXThemeHelper.AddAbsoluteThemePath("images/calendar/icon.gif")) %>' /></a>