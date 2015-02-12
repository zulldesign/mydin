<%@ Control Language="C#" ClassName="template" Inherits="Bitrix.UI.BXComponentTemplate" %>
<%@ Import Namespace="Bitrix" %>

<select name="site" onchange="location.href=this.value">
<%
	foreach (BXSite site in (BXSiteCollection)Results["sites"])
	{
		string href;
		BXSiteDomainCollection c = (BXSiteDomainCollection)Results["_"+site.Id];
		if (c.Count > 0)
		{
			href = "http://" + c[0].Domain;
			if (c[0].Domain.Equals(Request.Url.Host, StringComparison.InvariantCultureIgnoreCase))
				href += Component.Root;
		}
		else
			href = Component.Root;
		href += site.Directory;
		bool selected = site.Id.Equals(BXSite.Current.Id, StringComparison.InvariantCultureIgnoreCase);
%>
	<option value="<%= HttpUtility.HtmlEncode(href) %>"<% if (selected) {%> selected="true" <% } %> ><%= HttpUtility.HtmlEncode(site.SiteName) %></option>
<%
	}
%>
</select>

