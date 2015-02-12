<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.UI.BXComponentTemplate" %>

<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<%@ Import Namespace="Bitrix" %>

<script runat="server">
	public override string Title
	{
		get
		{
			return GetMessageRaw("Title");
		}
	}
</script>

<% List<BXParamsBag<object>> pages = Results.Get("Pages", new List<BXParamsBag<object>>()); %>
<% string cssPrefix = Parameters.GetString("CssClassPrefix"); %>

<% 
	if (!string.IsNullOrEmpty(Results.GetString("Title"))) 
	{ 
%>
	<span class="<%= cssPrefix %>page-title"><%= Encode(Results.GetString("Title")) %></span>
<% 
	}
	
	if (Results.GetString("PrevPage") != null)
	{ 
%>
	<a class="<%= cssPrefix %>page-previous" href="<%= Encode(Results.GetString("PrevPage")) %>"><%= GetMessage("Prev")%></a>
<% 
	}

	int prevIndex = 0;
	int i = 0;
	int[] pageIndexes = Results.Get("BoundedRange", new int[0]);		
	while (i < pageIndexes.Length)
	{
		int k = pageIndexes[i];
		if (i == 0 || k == prevIndex + 1)
		{
			if (pages[k].GetBool("Selected"))
			{
				%>
				<span class="<%= cssPrefix %>page-current<%= i == 0 ? (" " + cssPrefix + "page-first") : string.Empty %>"><%= k %></span>
				<%
			}
			else
			{
				%>
				<a href="<%= Encode(pages[k].GetString("Url")) %>"<%= i == 0 ? (" class=\"" + cssPrefix + "page-first\"") : string.Empty %>><%= k %></a>
				<%
			}
			prevIndex = k;
			i++;
		}
		else
		{
			%>
			<span class="<%= cssPrefix %>page-dots">...</span>
			<%
			prevIndex = k - 1;
		}
	}
		
	if (Results.GetString("NextPage") != null)
	{ 
%>
	<a class="<%= cssPrefix %>page-next" href="<%= Encode(Results.GetString("NextPage")) %>"><%= GetMessage("Next")%></a>
<% 
	}
%>