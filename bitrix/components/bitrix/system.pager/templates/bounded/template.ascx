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

<center>
	<div class="boundedPager">
		<% 
		if (Results.Get("PrevPage") != null) 
		{ 
			%><a class="prevLink" href="<%= Encode(Results.Get<string>("PrevPage")) %>" enableAjax="true" noAjaxStyle="true" ><% 
		} else 
		{
			%><span class="prev" ><%
		} 
		%><%= GetMessage("Prev") %><% 
		if (Results.Get("PrevPage") == null)	
		{ 
			%></span><% 
		} 
		else 
		{ 
			%></a><% 
		}
		
		int prevIndex = 0;
		int i = 0;
		int[] pageIndexes = Results.Get("BoundedRange", new int[0]);		
		while (i < pageIndexes.Length)
		{
			int k = pageIndexes[i];
			if (i == 0 || k == prevIndex + 1)
			{
				if (pages[k].Get<bool>("selected"))
				{
					%><span class="selected"><%
				}
				else
				{
					%><a href="<%= Encode(pages[k].Get<string>("url")) %>" enableAjax="true" noAjaxStyle="true" ><%
				}
				%><%= k %><%
				if (!pages[k].Get<bool>("selected"))
				{
					%></a><%
				}
				else
				{
					%></span><%
				}
				prevIndex = k;
				i++;
			}
			else
			{
				%><span class="break">...</span><%
				prevIndex = k - 1;
			}
		}

		if (Results.Get("NextPage") != null) 
		{ 
			%><a class="nextLink" href="<%= Encode(Results.Get<string>("NextPage")) %>" enableAjax="true" noAjaxStyle="true" ><% 
		} 
		else 
		{
			%><span class="next" ><%
		} 
		%><%= GetMessage("Next") %><% 
		if (Results.Get("NextPage") == null)	
		{ 
			%></span><% 
		} 
		else 
		{ 
			%></a><% 
		} 
		%>
	</div>
	<% if (Results.Get<bool>("EnableShowAll", true)) 
	{
		 %><div class="boundedPagerShowAll" >(<a href="<%= Encode(Results.Get<bool>("ShowAll", false) ? Results.Get<string>("DefaultPageUrl") : Results.Get<string>("ShowAllUrl"))  %>" enableAjax="true" noAjaxStyle="true" ><%= Results.Get<bool>("ShowAll", false) ? GetMessage("ShowPages") : GetMessage("ShowAll")%></a>)</div><%
	} %>
</center>
