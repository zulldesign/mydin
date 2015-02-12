<%@ Reference VirtualPath="~/bitrix/components/bitrix/system.PublicMenu/component.ascx" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.Main.Components.SystemPublicMenuTemplate" %>
<%@ Import Namespace="Bitrix" %>

<% if (Results.ContainsKey("menu") && Results["menu"] != null)
   { %>
   <!--div class="footer-box"-->
   <% foreach (Bitrix.BXPublicMenuItem item in (Bitrix.BXPublicMenuItemCollection)Results["menu"])
      { %>
        <% if (item.IsAccessible){ %>
            <% if (item.IsSelected)
            { %>
                <span><%= Encode(item.Title) %></span>&nbsp;&nbsp;
            <%}
            else
            { %>
                <a href="<%= Encode(BXUri.ToRelativeUri(item.Link)) %>"><%= Encode(item.Title) %></a>&nbsp;&nbsp;
            <%} %>
       <% } %>
   <% } %>
   <!--/div-->
<% } %>

<script runat="server" type="text/C#">
    public override string Title
	{
		get
		{
			return GetMessageRaw("Title");
		}
	}
</script>