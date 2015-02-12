<%@ Reference Control="~/bitrix/components/bitrix/news.detail/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="NewsDetailTemplate" %>
<%@ Import namespace="System.Collections.Generic"%>
<%@ Import namespace="System.ComponentModel"%>
<%@ Import namespace="Bitrix.IBlock"%>

<%if (!String.IsNullOrEmpty(Component.errorMessage)){ %>
	<span class="errortext"><%=Component.errorMessage%></span>
<%}

else if(Component.Element != null)
{%>
	<div class="news-item news-detail">
	<%
		if (Component.DetailImage != null && Component.ShowDetailPicture)
		{
		%>
			<img class="detail_picture" border="0" src="<%= Component.DetailImage.FilePath %>" width="<%= Component.DetailImage.Width %>"
				height="<%= Component.DetailImage.Height %>" alt="<%= Component.DetailImage.Description %>" />
		<%
		}
		else if (Component.PreviewImage != null && Component.ShowPreviewPicture)
		{
		%>
			<img class="detail_picture" border="0" src="<%= Component.PreviewImage.FilePath %>" width="<%= Component.PreviewImage.Width %>"
				height="<%= Component.PreviewImage.Height %>" alt="<%= Component.PreviewImage.Description %>" />
		<%
		}
		if (Component.ShowDate)
		{%><div class="news-date"><%=Component.DisplayDate%></div>
		<%
		}
		if (Component.ShowTitle)
		{%>
			<h3><%= Component.Element.Name %></h3>
		<%
		}
		if (Component.ShowPreviewText && !String.IsNullOrEmpty(Component.Element.PreviewText))
		{
		%>
			<%= Component.Element.PreviewText %>
		<%}
			
		%>
		
		<%= !String.IsNullOrEmpty(Component.Element.DetailText) ? Component.Element.DetailText : Component.Element.PreviewText %>
		<div style="clear: both"></div>
		<%
		if (Component.Properties != null)
		{
			foreach (KeyValuePair<string, Dictionary<string, string>> kvp in Component.Properties)
			{
				if (kvp.Value.ContainsKey("Value") && !String.IsNullOrEmpty(kvp.Value["Value"]))
				{
					%><span class="news-element-property"><%= kvp.Value["Name"] + ":" %>&nbsp;<span><%= kvp.Value["Value"] %></span></span> <%
				}
			}
		}%>
		<% if (!String.IsNullOrEmpty(Component.IBlockUrl) && !String.IsNullOrEmpty(Component.IBlockUrlTitle)){%>
	<a class="news-detail-link" href="<%= Component.IBlockUrl %>">&larr; <%=GetMessage("BackToNews") %></a>
<%} %>
	</div>
<%} %>


