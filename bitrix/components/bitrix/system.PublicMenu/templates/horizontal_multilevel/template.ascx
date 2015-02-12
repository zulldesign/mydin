<%@ Reference VirtualPath="~/bitrix/components/bitrix/system.PublicMenu/component.ascx" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.Main.Components.SystemPublicMenuTemplate" %>
<%@ Import Namespace="Bitrix" %>

<script runat="server">
	public string RenderSubmenu(Bitrix.BXPublicMenuItem item)
	{
		StringBuilder sb = new StringBuilder();

		if (item.Children != null && item.Children.Count > 0)
		{
			sb.Append("<ul>");

			foreach (Bitrix.BXPublicMenuItem mi in item.Children)
			{
				if (!mi.IsAccessible)
					continue;

				if (mi.IsSelected)
					sb.Append("<li class='item-selected'>");
				else
					sb.Append("<li>");

				if (mi.Children != null)
					sb.AppendFormat("<a class='parent' href=\"{0}\">{1}</a>", Encode(mi.Href), Encode(mi.Title));
				else
					sb.AppendFormat("<a href=\"{0}\">{1}</a>", Encode(mi.Href), Encode(mi.Title));

				sb.Append(RenderSubmenu(mi));
				sb.Append("</li>");
			}

			sb.Append("</ul>");
		}

		return sb.ToString();
	}


	public override string Title
	{
		get
		{
			return GetMessageRaw("Title");
		}
	}
</script>

<% 
	if (Results.ContainsKey("menu"))
	{
		int prevLevel = 0;
%>
<ul id="horizontal-multilevel-menu">
	<% 
        if (Component.Menu != null)
        {
            foreach (Bitrix.BXPublicMenuItem item in Component.Menu)
            {
                if (!item.IsAccessible)
                    continue; 
	%>
	<li><a href="<%= Encode(item.Href) %>" class="<%= item.IsSelected ? "root-item-selected" : "root-item" %>">
		<%= Encode(item.Title)%>
	</a>
		<%= RenderSubmenu(item)%>
	</li>
	<%
        }
        }
	%>
</ul>
<div class="menu-clear-left"></div>
<%
	} 
%>
