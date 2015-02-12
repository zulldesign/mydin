<%@ Reference VirtualPath="~/bitrix/components/bitrix/search.page/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Search.Components.SearchPageTemplate"
	EnableViewState="false" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Drawing" %>
<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.Components" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<%@ Import Namespace="Bitrix.DataLayer" %>
<%@ Import Namespace="Bitrix.Services" %>
<%@ Import Namespace="Bitrix.Search" %>
<%@ Import Namespace="Bitrix.Search.Components" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/search.tags.cloud/component.ascx" %>

<script runat="server">
	private string PrepareTags()
	{
		StringBuilder tags = new StringBuilder();
		foreach(string tag in Component.Tags)
		{
			if (tags.Length != 0)
				tags.Append(',');
			tags.Append(UrlEncode(tag));
		}
		return tags.ToString();		
	}
	
	protected void Search_Click(object sender, EventArgs args)
	{
		Component.DoSearch(query.Text, Component.ShowSearchFilter ? where.SelectedValue : null, string.Join(",", Component.Tags.ToArray()));
	}
	protected void Page_Init(object sender, EventArgs args)
	{
		IncludeComponent ic = new IncludeComponent("bitrix:search.tags.cloud", ".default");

		string linkTemplate;
		if (!string.IsNullOrEmpty(Component.ParamTags))
		{
			linkTemplate = Component.MakeSearchLinkTemplate(
				UrlEncode(Component.Query), 
				UrlEncode(Component.Where), 
				Parameters.GetBool("Template_NarrowSearch") ? PrepareTags() : null, 
				null, 
				Component.ParamTags + "=#SearchTags#"
			);
		}
		else
			linkTemplate = "";
		
		ic.Attributes["TagLinkTemplate"] = linkTemplate;
		ic.Attributes["PagingAllow"] = "false";
		ic.Attributes["SelectionSort"] = BXContentTagField.TagCount.ToString();
		ic.Attributes["SelectionOrder"] = BXOrderByDirection.Desc.ToString();
		ic.Attributes["DisplaySort"] = BXContentTagField.Name.ToString();
		ic.Attributes["DisplayOrder"] = BXOrderByDirection.Asc.ToString();

		switch (Component.ShowTags)
		{
			case BXSearchQuerySelectTagsMode.All:
				ic.Attributes["Moderation"] = SearchTagsCloudComponent.ModerationMode.All.ToString();
				break;
			case BXSearchQuerySelectTagsMode.Approved:
				ic.Attributes["Moderation"] = SearchTagsCloudComponent.ModerationMode.Approved.ToString();
				break;
			case BXSearchQuerySelectTagsMode.NotRejected:
				ic.Attributes["Moderation"] = SearchTagsCloudComponent.ModerationMode.NotRejected.ToString();
				break;
		}

		foreach (KeyValuePair<string, string> kv in Parameters)
		{
			if (kv.Key.StartsWith("Template_", StringComparison.OrdinalIgnoreCase))
				ic.Attributes[kv.Key.Substring("Template_".Length)] = kv.Value;
		}

		Cloud.Controls.Add(ic);
		
		((SearchTagsCloudComponent)ic.Component).ContentQuery = Component.GetSearchQuery();
		

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

	protected override void PreLoadTemplateDefinition(BXParametersDefinition def)
	{
		BXCategory tagCloudCategory = new BXCategory(GetMessageRaw("Category.TagCloud"), "tagcloud", 1000);

		def["Template_NarrowSearch"] = new BXParamYesNo(GetMessageRaw("Param.NarrowSearch"), true, tagCloudCategory);	
		
		def["Template_PagingRecordsPerPage"] = new BXParamText(GetMessageRaw("Param.PagingRecordsPerPage"), "10", tagCloudCategory);

		def["Template_SizeDistribution"] = new BXParamSingleSelection(GetMessageRaw("Param.SizeDistribution"), SearchTagsCloudComponent.SizeInterpolationMode.Exponential.ToString(), tagCloudCategory);
		def["Template_SizeMin"] = new BXParamText(GetMessageRaw("Param.SizeMin"), "10", tagCloudCategory);
		def["Template_SizeMax"] = new BXParamText(GetMessageRaw("Param.SizeMax"), "50", tagCloudCategory);

		def["Template_ColorDistribution"] = new BXParamSingleSelection(GetMessageRaw("Param.ColorDistribution"), SearchTagsCloudComponent.ColorInterpolationMode.None.ToString(), tagCloudCategory);
		def["Template_ColorMin"] = new BXParamColor(GetMessageRaw("Param.ColorMin"), Color.Empty, tagCloudCategory);
		def["Template_ColorMax"] = new BXParamColor(GetMessageRaw("Param.ColorMax"), Color.Empty, tagCloudCategory);
	}
	protected override void LoadTemplateDefinition(BXParametersDefinition def)
	{
		List<BXParamValue> sizeDistr = new List<BXParamValue>();
		sizeDistr.Add(new BXParamValue(GetMessageRaw("Option.DontUse"), SearchTagsCloudComponent.SizeInterpolationMode.None.ToString()));
		sizeDistr.Add(new BXParamValue(GetMessageRaw("Option.Linear"), SearchTagsCloudComponent.SizeInterpolationMode.Linear.ToString()));
		sizeDistr.Add(new BXParamValue(GetMessageRaw("Option.Exponential"), SearchTagsCloudComponent.SizeInterpolationMode.Exponential.ToString()));
		def["Template_SizeDistribution"].Values = sizeDistr;

		List<BXParamValue> colorDistr = new List<BXParamValue>();
		colorDistr.Add(new BXParamValue(GetMessageRaw("Option.DontUse"), SearchTagsCloudComponent.ColorInterpolationMode.None.ToString()));
		colorDistr.Add(new BXParamValue(GetMessageRaw("Option.Linear"), SearchTagsCloudComponent.ColorInterpolationMode.Linear.ToString()));
		colorDistr.Add(new BXParamValue(GetMessageRaw("Option.Logarithmic"), SearchTagsCloudComponent.ColorInterpolationMode.Logarithmic.ToString()));
		def["Template_ColorDistribution"].Values = colorDistr;
	}
</script>


<div class="searchTagsCloud">
	<asp:PlaceHolder runat="server" ID="Cloud" />
</div>
<asp:Panel ID="Container" runat="server" DefaultButton="search">
	<table cellpadding="0" cellspacing="0" width="100%" border="0">
		<tr>
			<td style="width: 100%"><asp:TextBox ID="query" runat="server" Width="99%" /></td>
			<td>&nbsp;</td>
			<td><asp:Button ID="search" runat="server" Text="<%$ LocRaw:Kernel.Find %>" OnClick="Search_Click" /></td>
			<% 	if (Component.ShowSearchFilter)	{ %>
			<td>&nbsp;</td>
			<td><asp:DropDownList ID="where" runat="server" /></td>
			<% } %>
		</tr>
	</table>
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
</asp:Panel>
<br />
<%	
	BXSearchResultCollection results = Component.SearchResults;
	if (results == null)
	{
		if (Component.SearchQuery != null && string.IsNullOrEmpty(Component.SearchQuery))
		{
%><span class="searchEmptyQuery"><%= GetMessage("EmptyQuery") %></span><%
																		   }
	}
	else if (results.Count == 0)
	{
%><%= GetMessage("NothingFound")%><% 
									  }
	else
	{ 
%><%= GetMessage("ItemsFound")+ ":" %>
<%= Component.TotalSearchResultsCount%><br />
<%
	if (Component.PagingShow && Component.PagingPosition != "bottom")
	{
%>
<bx:IncludeComponent runat="server" ID="HeaderPager" ComponentName="bitrix:system.pager"
	Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="top" />
<%
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
			&nbsp;
		</td>
	</tr>
	<tr>
		<td class="searchResultsTitle">
			<% 
				if (result.Urls.Length > 0)
				{ %><a href="<%= Encode(result.Urls[0]) %>"><% }
			else
			{	%><span
				class="searchResultsTitleNoUrl"><% }
				%><%= string.IsNullOrEmpty(title) ? GetMessage("NoTitle") : title%><% 
																					   if (result.Urls.Length == 0)
																					   { %></span><% }
			else
			{ %></a><% }
			%>
		</td>
	</tr>
	<tr>
		<td class="searchResultsBody">
			<%= string.IsNullOrEmpty(preview) ? "..." : preview%>
		</td>
	</tr>
	<% if (result.Tags.Count != 0)
	{ %>
	<tr>
		<td class="searchResultsTags">
			<%= GetMessage("Tags") %>:
			<% foreach (string tag in result.Tags) { %>
				<% if (!string.IsNullOrEmpty(Component.ParamTags)) { %>
				<a class="searchTag" href="<%= Encode(Component.MakeSearchLink(query.Text, Component.ShowSearchFilter ? where.SelectedValue : null, tag, null, null)) %>"><%= Encode(tag) %></a>
				<% } else { %>
				<span style="searchTag"><%= Encode(tag) %></span>
				<% } %>
			<% } %>
		</td>
	</tr>
	<% } %>
	<%	
		} 
	%>
</table>
<% 
	if (Component.PagingShow && Component.PagingPosition != "top")
	{
%><br />
<bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager"
	Template="<%$ Parameters:PagingTemplate %>"  CurrentPosition="bottom"/>
<%
	}
	}
%>
