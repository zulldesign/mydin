<%@ Control Language="C#" Inherits="Bitrix.UI.BXComponentTemplate" %>

<bx:IncludeComponent runat="server"
	ID="CatalogueSectionTree" 	 
	ComponentName="bitrix:catalogue.section.tree" 
	Template=".default" 
	
	sectionurl="<%$ Results:SectionElementListUrl %>"
	
	CountSubElements="<%$ Parameters:ShowSubElements %>"
	AddAdminPanelButtons="N"
	
	SectionId="" 
	SectionCode="" 
/>

<br />

<bx:IncludeComponent runat="server"
	Id="CatalogueElementList"
	ComponentName="bitrix:catalogue.element.list"
	Template=".default" 
	
	SectionId="" 
	SectionCode="" 
	
	SortBy="<%$ Parameters:TopSortBy%>" 
	SortOrder="<%$ Parameters:TopSortOrder%>" 
	
	Properties="<%$ Parameters:TopProperties %>" 
	ShowCatalogItemProperties="<%$ Parameters:ShowCatalogItemProperties %>" 
	
	sectionelementlisturl="<%$ Results:SectionElementListUrl %>"
	elementdetailurl="<%$ Results:ElementDetailUrl %>" 

	Pagingallow="False"
	PagingRecordsPerPage = "<%$ Parameters:TopElementCount %>"
    FilterByElementCustomProperty = "<%$ Parameters:ListFilterByCustomProperty %>"
    ElementCustomPropertyFilterSettings = "<%$ Parameters:ListCustomPropertyFilterSettings %>"
/>
