<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.UI.BXComponentTemplate" %>

<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<%@ Import Namespace="Bitrix" %>


<% List<BXParamsBag<object>> pages = Results.Get("Pages", new List<BXParamsBag<object>>()); %>

<span class="text">
	<% if (!string.IsNullOrEmpty(Results.Get<string>("Title"))) 
	{ 
		%><%= Encode(Results.Get<string>("Title"))%>&nbsp;<% 
	}
	if (Results.Get("RecordsCount", -1) >= 0) { 
		%><%= Results.Get("FirstRecordShown", "???") %><%
		if (Results.Get<int>("LastRecordShown") > Results.Get<int>("FirstRecordShown"))
		{ 
			%>&nbsp;-&nbsp;<%= Results.Get("LastRecordShown", "???")%><% 
		}
		%>&nbsp;<%= GetMessage("Of") %>&nbsp;<%= Results.Get("RecordsCount") %><br /><% 
	} %>
</span>
<span class="text">
	<% if (Results.Get("FirstPage") != null) { %><a href="<%= Encode(Results.Get<string>("FirstPage")) %>" enableAjax="true" ><% } 
	%><%= GetMessage("First") %><% 
	if (Results.Get("FirstPage") != null) { %></a><% } 
	%> | <% 
	if (Results.Get("PrevPage") != null) { %><a href="<%= Encode(Results.Get<string>("PrevPage")) %>" enableAjax="true" ><% } 
	%><%= GetMessage("Prev") %><% 
	if (Results.Get("PrevPage") != null) 
	{ 
		%></a><% 
	} 
	int[] pageNums = Results.Get("MiddleRange", new int[0]);
	if (pageNums.Length > 0)
	{
		%> | <%
		foreach (int i in Results.Get("MiddleRange", new int[0]))
		{
			%>&nbsp;<%
			if (!pages[i].Get<bool>("selected"))
			{ 
				%><a href="<%= Encode(pages[i].Get<string>("url")) %>" enableAjax="true" ><% 
			} 
			%><%= i%><% 
			if (!pages[i].Get<bool>("selected"))
			{ 
				%></a><% 
			}
			%>&nbsp;<%
		}
	}
	%> | <%
	if (Results.Get("NextPage") != null) { %><a href="<%= Encode(Results.Get<string>("NextPage")) %>" enableAjax="true" ><% } 
	%><%= GetMessage("Next") %><% 
	if (Results.Get("NextPage") != null) { %></a><% } 
	%> | <% 
	if (Results.Get("LastPage") != null) { %><a href="<%= Encode(Results.Get<string>("LastPage")) %>" enableAjax="true" ><% } 
	%><%= GetMessage("Last") %><% 
	if (Results.Get("LastPage") != null) { %></a><% }
	if (Results.Get<bool>("EnableShowAll", true))
	{
		%> | <a href="<%= Encode(Results.Get<bool>("ShowAll", false) ? Results.Get<string>("DefaultPageUrl") : Results.Get<string>("ShowAllUrl"))  %>" enableAjax="true" ><%= Results.Get<bool>("ShowAll", false) ? GetMessage("ShowPages") : GetMessage("ShowAll")%></a><%
	} %>
</span>