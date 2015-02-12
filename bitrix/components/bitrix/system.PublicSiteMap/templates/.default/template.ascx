<%@ Reference VirtualPath="~/bitrix/components/bitrix/system.PublicSiteMap/component.ascx" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.Main.Components.SystemPublicSiteMapTemplate" %>
<%@ Import Namespace="Bitrix" %>
<% 
	if (Component.RootNode != null)
	{
		int allNum = Component.RootNode.Count;
		int colNum = (int)Math.Ceiling(((double)allNum / Component.NumberOfColumns));

		int counter = 0;
		%>
<table class="map-columns">
<tr>
	<td>
		<ul class="map-level-0">
	<%
	HashSet<BXPublicMenuItem> processed = new HashSet<BXPublicMenuItem>();
	foreach (Bitrix.BXPublicMenuItem item in Component.RootNode)
	{
		if (processed.Contains(item))
			continue;
		processed.Add(item);
		if (counter >= colNum)
		{
			counter = 0;
			%></ul></td><td><ul class="map-level-0"><%
		}

		if (item.IsAccessible)
		{
			string ss = RecurseTree(item.Children, 1, processed);
			%>
			<li><a href="<%= Encode(item.Href) %>"><%= Encode(item.Title)%></a></li>
			<%= ss %>
			<%
			counter++;
		}
	}
	%>
		</ul>
	</td>
</tr>
</table>
<% } %>

<script runat="server" type="text/C#">
	protected string RecurseTree(BXPublicMenuItemCollection collection, int level, HashSet<BXPublicMenuItem> processed)
	{
		if (Component.Depth > 0 && level >= Component.Depth)
			return "";
		
		StringBuilder sb = new StringBuilder();
		if (collection != null)
		{
			sb.AppendFormat("<ul class=\"map-level-{0}\">", level);
			foreach (Bitrix.BXPublicMenuItem item in collection)
			{
				if (processed.Contains(item))
					continue;
				processed.Add(item);
				
				if (item.IsAccessible)
				{
					sb.AppendFormat("<li><a href=\"{0}\">{1}</a></li>", Encode(item.Href), Encode(item.Title));
					sb.Append(RecurseTree(item.Children, level + 1, processed));
				}
			}
			sb.Append("</ul>");
		}
		return sb.ToString();
	}
	
	protected override void PrepareDesignMode()
	{
		MinimalWidth = "175";
		MinimalHeight = "45";
		StartHeight = "170px";

		//Добавляем данные, которые будут отображаться в редакторе
		if (Results["root"] == null)
		{
			BXPublicMenuItemCollection menu = new BXPublicMenuItemCollection();

			for (int i = 0; i < 5; i++)
			{
				BXPublicMenuItem item = new BXPublicMenuItem();
				item.Link = "~/bitrix";
				item.Title = string.Format("Bitrix {0}", i + 1);
				menu.Add(item);
			}

			Results["root"] = menu;
		}
	}
</script>

