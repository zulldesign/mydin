<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AdminBreadCrumb.ascx.cs" Inherits="bitrix_kernel_AdminBreadCrumb" %>
<%@ Reference VirtualPath="~/bitrix/admin/controls/Main/AdminMenu.ascx" %>
<% if (DesignMode)
   {
	    %>crumb > crumb > crumb<%
		return;
   }
%>

<% for(int i = 0; i < crumbs.Count; i++)
   {
	   if (i != 0)
	   {
		   %><img class="arrow" alt="" src="<%= FromAdminTheme("images/chain_arrow.gif") %>"/><%
	   }
	   %><a href="<%= Encode(crumbs[i].Url) %>" ><% 
	   if (i == 0)
	   { 
		   %><img class="home" alt="" style="border:none 0px" src="<%= FromAdminTheme("images/home.gif") %>" /><% 
	   }
	   %><%= crumbs[i].Text %></a><%
   } 
%>