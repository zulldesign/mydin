<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.UI.BXComponentTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.UI" %>

<% %>

<div class="navigation">
	<div class="navigation-arrows">		
	<%                                                       	                                                                                                	  		                                               	                                                   	  	  	                                              	
	if (Results.GetString("PrevPage") != null) 
	{ 
		%><span class="arrow">←</span><span class="ctrl"><script type="text/javascript">document.write(" ctrl");</script></span><a href="<%= Encode(Results.GetString("PrevPage")) %>" id="<%= ClientID %>_next_page"><%=GetMessage("Prev")%></a>&nbsp;<%
	}
	else
	{ 
		%><span class="disabled"><span class="arrow">←</span><span class="ctrl"><script type="text/javascript">document.write(" ctrl");</script></span>&nbsp;<%=GetMessage("Prev")%></span> <% 
	}
	 
	if (Results.GetString("NextPage") != null) 
	{ 
		%><a href="<%= Encode(Results.GetString("NextPage")) %>" id="<%= ClientID %>_previous_page"><%= GetMessage("Next")%></a>&nbsp;<span class="ctrl"><script type="text/javascript">document.write(" ctrl");</script></span><span class="arrow">→</span>&nbsp;<% 
	} 
	else
	{
		%><span class="disabled"><%=GetMessage("Next")%>&nbsp;<span class="ctrl"><script type="text/javascript">document.write("ctrl ");</script></span><span class="arrow">→</span></span> <%
	}
	%>
	</div>
	<div class="navigation-pages">
	<%
	if (!String.IsNullOrEmpty(Results.GetString("Title"))) 
	{ 
		%><span class="navigation-title"><%= Encode(Results.GetString("Title"))%></span><% 
	}
	var pages = Results.Get("Pages", new List<BXParamsBag<object>>()); 
		                                                                                	
	int prevIndex = 0;
	int i = 0;
	var pageIndexes = Results.Get<int[]>("BoundedRange");		
	
	if (pageIndexes != null)
	while (i < pageIndexes.Length)
	{
		int k = pageIndexes[i];
		if (i == 0 || k == prevIndex + 1)
		{
			if (pages[k].Get<bool>("selected"))
			{
				%><span class="nav-current-page"><%= k %></span> <%
			}
			else
			{
				%><a href="<%= Encode(pages[k].Get<string>("url")) %>"><%= k %></a> <%
			}
			prevIndex = k;
			i++;
		}
		else
		{
			%><span>...</span> <%
			prevIndex = k - 1;
		}
	}
	%>	
	</div>	                                                                                                       	
</div>
<script type="text/javascript">
document.onkeydown = function(event)
{
   if (!document.getElementById)
      return;

   if (window.event)
      event = window.event;

   if (event.ctrlKey)
   {
      var key = (event.keyCode ? event.keyCode : (event.which ? event.which : null) );
      if (!key)
         return;

      var link = null;
      if (key == 37)
         link = document.getElementById('<%= ClientID %>_next_page');
      else if (key == 39)
         link = document.getElementById('<%= ClientID %>_previous_page');

      if (link && link.href)
         document.location = link.href;
   }
}
</script>