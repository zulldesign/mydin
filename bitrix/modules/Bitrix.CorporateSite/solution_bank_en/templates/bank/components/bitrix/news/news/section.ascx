<%@ Control Language="C#" Inherits="Bitrix.UI.BXComponentTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<script runat="server">
	string RssPath = "";
	protected void Page_Load(object sender, EventArgs e)
	{
		if (Parameters.Get<bool>("UseRss"))
		{
			RssPath = ((string)Results["UrlTemplatesSectionRss"]).Replace("#SectionId#", Results.Get("SectionId", "0"));
			BXPage.RegisterLink(
				"alternate",
				"application/rss+xml",
				RssPath,
				new KeyValuePair<string, string>("title", RssPath)
			);
		}
	}
</script>

<%if (Parameters["UseRss"] == "True")
  {%>
<a href="<%= RssPath %>" title="rss" target="_self" class="rss"></a>
<%}%>

<bx:IncludeComponent runat="server" 
	ID="IncludeComponent1" 
	ComponentName="bitrix:news.list"
	PageId="<%$ Results:PageId %>" 
	PageShowAll="<%$ Results:PageShowAll %>" 
	UrlTemplatesNews="<%$ Results:UrlTemplatesNews %>"
	UrlTemplatesNewsPage="<%$ Results:UrlTemplatesNewsPage %>" 
	UrlTemplatesNewsShowAll="<%$ Results:UrlTemplatesNewsShowAll %>"
	ParentSectionId="<%$ Results:SectionId %>" 
	DetailUrl="<%$ Results:UrlTemplatesDetail %>"
	SectionUrl="<%$ Results:UrlTemplatesSection %>" 	
	ParamSection = "<%$ Parameters:ParamSection %>"
	ShowPreviewText="<%$ Parameters:ListShowPreviewText%>"
	ShowTitle="<%$ Parameters:ListShowTitle%>"
	ShowDetailText="<%$ Parameters:ListShowDetailText%>"
	ShowPreviewPicture="<%$ Parameters:ListShowPreviewPicture%>"
	SetTitle="<%$ Parameters:ListSetTitle%>"
	ShowDate="<%$ Parameters:ListShowDate%>"
	ActiveDateFormat="<%$ Parameters:ActiveDateFormat%>" />
