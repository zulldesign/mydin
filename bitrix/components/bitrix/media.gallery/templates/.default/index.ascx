<%@ Control Language="C#" ClassName="list" Inherits="Bitrix.UI.BXComponentTemplate" %>
<script runat="server"></script>

<bx:IncludeComponent runat="server" ID="mediaGalleryElementList" 	 
	ComponentName="bitrix:media.gallery.element.list" Template=".default" 
	SectionId="" 
	SectionCode="" 
    ElementDetailUrl="<%$ Results:ElementDetailUrl %>"	
    SectionElementListUrl="<%$ Results:SectionElementListUrl %>"
	SortBy="<%$ Parameters:ListSortBy%>" 
	SortOrder="<%$ Parameters:ListSortOrder%>" 
	Properties="<%$ Parameters:ListProperties %>"   
	PagingIndexTemplate="<%$ Results:PagingIndexTemplate %>"
	PagingPageTemplate="<%$ Results:PagingPageTemplate %>" 
	PagingShowAllTemplate="<%$ Results:PagingShowAllTemplate %>" 
	PagingPageID="<%$ Results:PageId %>"
	PagingShowAll="<%$ Results:ShowAll %>" 	  
/>