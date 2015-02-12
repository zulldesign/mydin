<%@ Control Language="C#" Inherits="Bitrix.UI.BXComponentTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<script runat="server">
string RssPath = "";
protected void Page_Load(object sender, EventArgs e)
{
	if (Parameters.Get<bool>("UseRss"))
	{
		RssPath = (string)Results["UrlTemplatesRss"];
		BXPage.RegisterLink(
			"alternate", 
			"application/rss+xml", 
			RssPath, 
			new KeyValuePair<string, string>("title", RssPath)
		);
	}
}
</script>

<%
if (Parameters.Get<bool>("UseRss"))
{
	  %><a href="<%= RssPath %>" title="rss" target="_self" class="rss"></a><%
}
%>

<bx:IncludeComponent 
	runat="server"
	ID="NewsList" 
	ComponentName="bitrix:news.list"	
	PageId="<%$ Results:PageId %>"
	ParamSection = "<%$ Parameters:ParamSection %>"
	PageShowAll="<%$ Results:PageShowAll %>"
	UrlTemplatesNews="<%$ Results:UrlTemplatesNews %>"
	UrlTemplatesNewsPage="<%$ Results:UrlTemplatesNewsPage %>"
	UrlTemplatesNewsShowAll="<%$ Results:UrlTemplatesNewsShowAll %>"
	DetailUrl="<%$ Results:UrlTemplatesDetail %>"
	ShowPreviewText="<%$ Parameters:ListShowPreviewText%>"
	ShowTitle="<%$ Parameters:ListShowTitle%>"
	ShowDetailText="<%$ Parameters:ListShowDetailText%>"
	ShowPreviewPicture="<%$ Parameters:ListShowPreviewPicture%>"
	SetTitle="<%$ Parameters:ListSetTitle%>"
	ShowDate="<%$ Parameters:ListShowDate%>"
	ActiveDateFormat="<%$ Parameters:ActiveDateFormat%>"
	
/>