<%@ Reference VirtualPath="~/bitrix/components/bitrix/system.PublicBreadCrumb/component.ascx" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.Main.Components.SystemPublicBreadCrumbTemplate" %>
<%@ Import Namespace="Bitrix" %>
<% 
if (Component.BreadCrumb != null)
{ 
	%><div id="breadcrumb"><b class="r0 top"></b><p><% 
	
	for (int i = 0; i < Component.BreadCrumb.Length; i++)
	{
		BXPublicMenuItem item = Component.BreadCrumb[i];
		if (!item.IsAccessible)
			continue;

		if (i > 0)
		{
			%>&nbsp;&mdash;&nbsp;<%
		}
        if (i != Component.BreadCrumb.Length - 1)
        {    
		%><a href="<%= Encode(item.Href) %>" title="<%= Encode(item.Title)%>"><% } if (i == 0)
     { %><img src="<% = VirtualPathUtility.ToAbsolute("~/")%>bitrix/templates/<%=BXSite.CurrentTemplate %>/images/home.gif" height="8"/><%}
     else
     { %>
   <% if (i == Component.BreadCrumb.Length - 1)
      {  %><span><%} %><%= Encode(item.Title)%><% if (i == Component.BreadCrumb.Length - 1){ %></span><%} %>
     <%} %>
     <%  if (i != Component.BreadCrumb.Length - 1)
         { %></a><%	}	
	}
	%></p><b class="r0 bottom"></b></div><%
}
%>

<script runat="server" type="text/C#">
	protected override void PrepareDesignMode()
	{
		MinimalWidth = "175";
		MinimalHeight = "45";
		StartHeight = "170px";

		//Добавляем данные, которые будут отображаться в редакторе
		if (!Results.ContainsKey("menu") || Results["menu"] == null)
		{
			System.Collections.Generic.List<BXPublicMenuItem> menu = new System.Collections.Generic.List<BXPublicMenuItem>();
			for (int i = 0; i < 5; i++)
			{
				BXPublicMenuItem item = new BXPublicMenuItem();
				item.Link = "~/bitrix";
				item.Title = string.Format("Bitrix {0}", i + 1);
				menu.Add(item);
			}

			Results["menu"] = menu.ToArray();
		}
	}
</script>

