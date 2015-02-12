<%@ Reference Control="~/bitrix/components/bitrix/news.detail/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="NewsDetailTemplate" %>
<%@ Import namespace="System.Collections.Generic"%>
<%@ Import namespace="System.ComponentModel"%>
<%@ Import namespace="Bitrix.IBlock"%>

<%if (!String.IsNullOrEmpty(Component.errorMessage)){ %>
	<span class="errortext"><%=Component.errorMessage%></span>
<%}

else if(Component.Element != null)
{
	    if (Component.ShowTitle)
		{%>
			<h1><%= Component.Element.Name %></h1>
		<%
		}
    %>
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
		{%>
			<div class="news-date"><%= Component.DisplayDate%></div>
		<%
		}
        %>
        <div class="news-detail">  <%
		if (Component.ShowPreviewText && !String.IsNullOrEmpty(Component.Element.PreviewText))
		{
		%>
			<p><%= Component.Element.PreviewText %></p>
		<%}
		    	
		%>
		
		<%= !String.IsNullOrEmpty(Component.Element.DetailText) ? Component.Element.DetailText : Component.Element.PreviewText %>
				<% if (!String.IsNullOrEmpty(Component.IBlockUrl) && !String.IsNullOrEmpty(Component.IBlockUrlTitle)){%>
	                <a enableajax="true" class="news-detail-link" href="<%= Component.IBlockUrl %>">← <%= Component.IBlockUrlTitle %></a>
                <%} %>
		</div> 

	</div>
<%} %>


