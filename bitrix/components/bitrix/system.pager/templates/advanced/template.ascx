<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.UI.BXComponentTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.UI" %>

<script runat="server">

	public override string Title
	{
		get
		{
			return GetMessageRaw("Title");
		}
	}
	
	//Firefox load page in "next" attribute
	//protected override void OnPreRender(EventArgs e)
	//{
	//    base.OnPreRender(e);
		
	//    string prev = Results.GetString("PrevPage");
	//    string next = Results.GetString("NextPage");
	//    BXPage page = this.Page as BXPage;
		
	//    if (page != null)
	//    {
	//        if (prev != null)
	//            page.HeadItems.Add(this.AppRelativeVirtualPath, "link", new KeyValuePair<string, string>("rel", "previous"), new KeyValuePair<string, string>("href", prev));		
		
	//        if (next != null)
	//            page.HeadItems.Add(this.AppRelativeVirtualPath, "link", new KeyValuePair<string, string>("rel", "next"), new KeyValuePair<string, string>("href", next));			
	//    }
	//}

</script>

<% List<BXParamsBag<object>> pages = Results.Get("Pages", new List<BXParamsBag<object>>()); %>

<div class="navigation-yandex-style">

	<%
	if (!String.IsNullOrEmpty(Results.GetString("Title"))) 
	{ 
		%><b><%= Encode(Results.GetString("Title"))%></b>&nbsp;<% 
	}
                                                                                                    	  		                                               	                                                   	  	  	                                              	
	if (Results.GetString("PrevPage") != null) 
	{ 
		%><span class="arrow">←</span><span class="ctrl"><script type="text/javascript">document.write(" Ctrl");</script></span><a href="<%= Encode(Results.GetString("PrevPage")) %>" enableAjax="true" id="bx_next_page"><%=GetMessage("Prev")%></a>&nbsp;<%
	}
	else
	{ 
		%><span class="disabled"><span class="arrow">←</span><span class="ctrl"><script type="text/javascript">document.write(" Ctrl");</script></span>&nbsp;<%=GetMessage("Prev")%></span> <% 
	}
	 
	if (Results.GetString("NextPage") != null) 
	{ 
		%><a href="<%= Encode(Results.GetString("NextPage")) %>" enableAjax="true" id="bx_previous_page"><%=GetMessage("Next")%></a>&nbsp;<span class="ctrl"><script type="text/javascript">document.write(" Ctrl");</script></span><span class="arrow">→</span>&nbsp;<% 
	} 
	else
	{
		%><span class="disabled"><%=GetMessage("Next")%>&nbsp;<span class="ctrl"><script type="text/javascript">document.write("Ctrl ");</script></span><span class="arrow">→</span></span> <%
	}
	%>
	<br /> 
	<%		                                                                                                                                                                                	
	int[] pageNums = Results.Get("MiddleRange", new int[0]);
	if (pageNums.Length > 0)
	{
		foreach (int i in pageNums)
		{
			if (pages[i].GetBool("selected"))
			{
				%><span class="nav-current-page"><%=i %></span> <% 
			} 
			else
			{ 
				%><a href="<%= Encode(pages[i].GetString("url")) %>" enableAjax="true"><%=i %></a> <% 				
			}
		}
	}
	%>		                                                                                                       	
</div>
