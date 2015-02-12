<%@ Reference VirtualPath="~/bitrix/components/bitrix/search.page/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Search.Components.SearchPageTemplate" EnableViewState="false" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<%@ Import Namespace="Bitrix.Services" %>
<%@ Import Namespace="Bitrix.Search" %>
<%@ Import Namespace="Bitrix.Search.Components" %>
<script runat="server">
	protected void Search_Click(object sender, EventArgs args)
	{
		Component.DoSearch(query.Text, Component.ShowSearchFilter ? where.SelectedValue : null, string.Join(",", Component.Tags.ToArray()));
	}
	protected void Page_Init(object sender, EventArgs args)
	{
		if (Component.ShowSearchFilter)
		{
			where.Items.Clear();
			foreach (ListItem l in Component.SearchFilterItems)
				where.Items.Add(l);
		}
	}
	protected void Page_PreRender(object sender, EventArgs args)
	{
		if (!IsPostBack)
		{
			if (Component.ShowSearchFilter && !string.IsNullOrEmpty(Component.SearchFilter))
				try
				{
					where.SelectedValue = Component.SearchFilter;
				}
				catch
				{
				}
			query.Text = Component.Query;
		}
	}
</script>

<% 
	BXSearchResultCollection results = Component.SearchResults;
%>
<div class="search-page">

<table cellpadding="0" cellspacing="0" width="100%" border="0" onkeypress="return FireDefaultButton(event, '<%= search.ClientID %>')">
	<tr>
		<td style="width: 100%"><asp:TextBox ID="query" runat="server" CssClass="search-query" /></td>
		<td>&nbsp;</td>
		<td><asp:Button ID="search" runat="server" Text="<%$ LocRaw:ButtonText.Find %>" OnClick="Search_Click" CssClass="search-button" /></td>
		<% 
		if (Component.ShowSearchFilter)
		{
			%>
			<td>&nbsp;</td>
			<td><asp:DropDownList ID="where" runat="server" CssClass="search-filter" /></td>
			<%
		} 
		%>	
	</tr>
	<tr>
		<td colspan="<%= Component.ShowSearchFilter ? 5 : 3 %>">
		<% if (Component.Tags.Count != 0) { %>
			<span style="font-size: smaller" >
			<%= GetMessage("LimitByTags") %>:
			<% StringBuilder remove = new StringBuilder(); %>
			<% foreach(string tag in Component.Tags) { %>
			<%
				remove.Length = 0;
				if (Component.Tags.Count > 1)
				{
					foreach(string left in Component.Tags)
					{
						if (tag == left)
							continue;
						if (remove.Length != 0)
							remove.Append(',');
						remove.Append(UrlEncode(left));
					}
				}
				string url = Component.MakeSearchLinkTemplate(UrlEncode(Component.Query),  UrlEncode(Component.Where), remove.ToString(), null, null);
				if (url == null)
				{
					UriBuilder newUrl = new UriBuilder(BXSefUrlManager.CurrentUrl);
					NameValueCollection query = HttpUtility.ParseQueryString(newUrl.Query);
					query.Remove(Component.ParamTags);
					newUrl.Query = query.ToString();
					url = newUrl.Uri.PathAndQuery;
				}
			%>
			<span class="searchTag"><%= Encode(tag) %></span> [<a href="<%= Encode(url) %>">X</a>]
			<% } %>
		</span>
		<% } %>
		</td>
	</tr>
</table>

<%	
if (results == null)
{
	if (Component.SearchQuery != null || string.IsNullOrEmpty(Component.SearchQuery))
	{
		%><div class="search-result"><%= GetMessageRaw("EmptyQuery") %></div><%
	}
} 
else if(results.Count == 0)
{
	%><div class="search-result"><%= GetMessageRaw("NoResults") %></div><% 
}
else
{ 
	%><div class="search-result"><%= GetMessageRaw("Found") %>: <%= Component.TotalSearchResultsCount%></div>


	<%
	foreach (BXSearchResult result in results)
	{
		string preview = Bitrix.Services.Text.BXStringUtility.ProcessSegments(
			result.Preview,
			result.PreviewHighlights,
			x => "<b>" + Encode(x) + "</b>",
			x => Encode(x)
		);

		string title = Bitrix.Services.Text.BXStringUtility.ProcessSegments(
			result.Title,
			result.TitleHighlights,
			x => "<b>" + Encode(x) + "</b>",
			x => Encode(x)
		);
	%>
	<div class="search-item">
		<h4>
			<% 	if (result.Urls.Length > 0) { %>
			<a href="<%= Encode(result.Urls[0]) %>"><%= string.IsNullOrEmpty(title) ? GetMessageRaw("NoTitle") : title %></a>
			<% } else {	%>
			<span class="search-item-nourl"><%= Bitrix.Services.Text.BXStringUtility.IsNullOrTrimEmpty(title) ? GetMessageRaw("NoTitle") : title %></span>
			<% } %>
		</h4>
				
		<div class="search-preview">
			<%= Bitrix.Services.Text.BXStringUtility.IsNullOrTrimEmpty(preview) ? "..." : preview %>
		</div>
		
		<% if (result.Tags.Count != 0) { %>
		<div class="search-tags">
			<label><%= GetMessageRaw("Tags") %>:</label>
			<% foreach (string tag in result.Tags) { %>
				<% if (!string.IsNullOrEmpty(Component.ParamTags)) { %>
				<a href="<%= Encode(Component.MakeSearchLink(query.Text, Component.ShowSearchFilter ? where.SelectedValue : null, tag, null, null)) %>"><%= Encode(tag) %></a> 
				<% } else { %>
				<span><%= Encode(tag) %></span>
				<% } %>
			<% } %>
		</div>
		<% } %>
	</div>
	<% 
	} 
	%>

	<bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="bottom" />
<%
}
%>
</div>