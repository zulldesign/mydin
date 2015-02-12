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
<asp:Panel ID="Container" runat="server" DefaultButton="search">
	<table cellpadding="0" cellspacing="0" width="100%" border="0">
		<tr>
			<td style="width: 100%">
				<asp:TextBox ID="query" runat="server" Width="99%" /></td>
			<td>&nbsp;</td>
			<td>
				<asp:Button ID="search" runat="server" Text="<%$ LocRaw:Kernel.Find %>" OnClick="Search_Click" />
			</td>
<% 
	if (Component.ShowSearchFilter)
	{
		%><td>&nbsp;</td><td><asp:DropDownList ID="where" runat="server" /></td><%
	} 
%>	
		</tr>
	</table>
</asp:Panel>
<%	
	if (results == null)
	{
		if (Component.SearchQuery != null && string.IsNullOrEmpty(Component.SearchQuery))
		{
			%><span class="searchEmptyQuery"><%= GetMessage("EmptyQuery") %></span><%
		}
	} 
	else if(results.Count == 0)
	{
		%><%= GetMessage("NothingFound")%><% 
	}
	else
	{ 
		%><%= GetMessage("ItemsFound")+ ":" %> <%= Component.TotalSearchResultsCount%><br /><%
		if (Component.PagingShow && Component.PagingPosition != "bottom")
		{
			%>
<bx:IncludeComponent 
    runat="server" 
    ID="HeaderPager" 
    ComponentName="bitrix:system.pager"
    Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="top" /><%
		} 
	%>
<table class="searchResultsTable" style="border: none 0px; padding: 0px; border-spacing: 0px;
	border-collapse: collapse; width: 100%">
	<%
		foreach (BXSearchResult result in results)
		{
			string preview = Bitrix.Services.Text.BXStringUtility.ProcessSegments(
				result.Preview,
				result.PreviewHighlights,
				delegate(string input)
				{
					return "<b>" + Encode(input) + "</b>";
				},
				delegate(string input)
				{
					return Encode(input);
				}
			);
			preview.Trim();

			string title = Bitrix.Services.Text.BXStringUtility.ProcessSegments(
				result.Title,
				result.TitleHighlights,
				delegate(string input)
				{
					return "<b>" + Encode(input) + "</b>";
				},
				delegate(string input)
				{
					return Encode(input);
				}
			);
			title.Trim();
	%>
	<tr>
		<td>
			&nbsp;</td>
	</tr>
	<tr>
		<td class="searchResultsTitle">
			<% 
			if (result.Urls.Length > 0) { %><a href="<%= Encode(result.Urls[0]) %>"><% } else {	%><span class="searchResultsTitleNoUrl" ><% }
			%><%= string.IsNullOrEmpty(title) ? GetMessage("NoTitle") : title%><% 
			if (result.Urls.Length == 0) { %></span><% } else { %></a><% }
			%>
		</td>
	</tr>
	<tr>
		<td class="searchResultsBody">
			<%= string.IsNullOrEmpty(preview) ? "..." : preview%>
		</td>
	</tr>
	<% if (result.Tags.Count != 0) { %>
	<tr>
		<td class="searchResultsTags">
			<%= GetMessage("Tags") %>:
			<% foreach (string tag in result.Tags) { %>
				<% if (!string.IsNullOrEmpty(Component.ParamTags)) { %>
				<a href="<%= Encode(Component.MakeSearchLink(query.Text, Component.ShowSearchFilter ? where.SelectedValue : null, tag, null, null)) %>"><%= Encode(tag) %></a> 
				<% } else { %>
				<span style="border-bottom: 1px dashed"><%= Encode(tag) %></span>
				<% } %>
			<% } %>
		</td>
	</tr>
	<% } %>
	<%	
		} 
	%>
</table><% 
		if (Component.PagingShow && Component.PagingPosition != "top")
		{
			%><br />
<bx:IncludeComponent 
    runat="server" 
    ID="FooterPager" 
    ComponentName="bitrix:system.pager" 
    Template="<%$ Parameters:PagingTemplate %>"
    CurrentPosition="bottom"
     />
<%
		}
	}
%>
