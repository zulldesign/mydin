<%@ Reference Control="~/bitrix/components/bitrix/system.siteselector/component.ascx" %>
<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.Main.Components.SystemSiteSelectorTemplate" %>
<%@ Import Namespace="Bitrix" %>

<% 
	foreach (SiteSelector.SiteInfo site in Sites) 
	if (site.IsSelected) 
	{
		%><span title="<%= Encode(site.Name) %>"><%= Encode(site.Name) %></span>&nbsp;<%
	} 
	else 
	{
		
		BXUri uri;
		if (site.Domains.Count > 0)
		{
			string domain = site.Domains[0];
			uri = new BXUri(
				"http://", 
				domain, 
				string.Equals(domain, Request.Url.Host, StringComparison.InvariantCultureIgnoreCase) ? Component.Root : string.Empty,
				site.Directory
			);
		}
		else
			uri = new BXUri(
				Component.Root,
				site.Directory
			);
		%><a href="<%= Encode(uri.AbsoluteUri) %>" title="<%= Encode(site.Name) %>"><%= Encode(site.Name) %></a>&nbsp;<%
	}
%>

