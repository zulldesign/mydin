<%@ Control Language="C#" Inherits="Bitrix.UI.BXComponentTemplate" %>

<bx:IncludeComponent runat="server"
	Id="CatalogueElementList"
	ComponentName="bitrix:catalogue.element.list"
	Template=".default" 
	
	SectionId="" 
	SectionCode="" 
	
	SortBy="<%$ Parameters:TopSortBy%>" 
	SortOrder="<%$ Parameters:TopSortOrder%>" 
	
	properties="<%$ Parameters:TopProperties %>" 
	
	sectionelementlisturl="<%$ Results:SectionElementListUrl %>"
	elementdetailurl="<%$ Results:ElementDetailUrl %>" 

	Pagingallow="False"
	PagingRecordsPerPage = "<%$ Parameters:TopElementCount %>"
/>
