<%@ Control Language="C#" ClassName="list" Inherits="Bitrix.UI.BXComponentTemplate" %>
<script runat="server">
</script>

<bx:IncludeComponent runat="server" ID="mediaGalleryElementList" 	 
	ComponentName="bitrix:media.gallery.element.list" Template=".default" 
	
	SectionId="<%$ Results:SectionId %>" 
	SectionCode="<%$ Results:SectionCode %>" 
	ElementDetailUrl ="<%$ Results:ElementDetailUrl %>"
    SectionElementListUrl="<%$ Results:SectionElementListUrl %>"	
	SortBy="<%$ Parameters:ListSortBy%>" 
	SortOrder="<%$ Parameters:ListSortOrder%>" 
	Properties="<%$ Parameters:ListProperties %>" 

	PagingIndexTemplate="<%$ Results:PagingIndexTemplate %>"
	PagingPageTemplate="<%$ Results:PagingPageTemplate %>" 
	PagingShowAllTemplate="<%$ Results:PagingShowAllTemplate %>" 
	PagingPageId="<%$ Results:PageId %>" 
	PagingShowAll="<%$ Results:ShowAll %>"     
/>
