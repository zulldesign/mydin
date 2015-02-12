<%@ Control Language="C#" Inherits="Bitrix.UI.BXComponentTemplate" %>
<bx:IncludeComponent runat="server"
	Id="CatalogueElementList"
	ComponentName="bitrix:catalogue.element.list"
	Template=".default" 
	
	SectionId="<%$ Results:SectionId %>" 
	SectionCode="<%$ Results:SectionCode %>" 
	
	SortBy="<%$ Parameters:ListSortBy%>" 
	SortOrder="<%$ Parameters:ListSortOrder%>" 
	
	properties="<%$ Parameters:ListProperties %>" 
	
	sectionelementlisturl="<%$ Results:SectionElementListUrl %>"
	elementdetailurl="<%$ Results:ElementDetailUrl %>" 

	pagingindextemplate="<%$ Results:PagingIndexTemplate %>"
	pagingpagetemplate="<%$ Results:PagingPageTemplate %>" 
	pagingshowalltemplate="<%$ Results:PagingShowAllTemplate %>" 
	pagingpageid="<%$ Results:PageId %>" 
	pagingshowall="<%$ Results:ShowAll %>" 
/>
