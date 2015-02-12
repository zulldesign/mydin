<%@ Reference VirtualPath="~/bitrix/components/bitrix/catalogue.compare.list/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.UI.BXComponentTemplate" %>

<bx:IncludeComponent runat="server"
	ID="CatalogueSectionTree" 	 
	ComponentName="bitrix:catalogue.section.tree" 
	Template=".default" 
	
	sectionurl="<%$ Results:SectionElementListUrl %>" 
	
	CountSubElements="<%$ Parameters:ShowSubElements %>"
	AddAdminPanelButtons="N"
	
	SectionId="<%$ Results:SectionId %>" 
	SectionCode="<%$ Results:SectionCode %>" 
	
	Template_RootTitle="<%$ Results:RootSectionTitle %>"
	Template_RootUrl="<%$ Results:IndexTemplate %>"
/>
<br />
<%if(Parameters.GetBool("AllowComparison", false)) {%>
<bx:IncludeComponent 
	id="CatalogueCompareList" 
	runat="server" 
	componentname="bitrix:catalogue.compare.list" 
	template=".default" 
	IBlockId="<%$ Parameters:IBlockId %>" 
	ElementUrlTemplate="<%$ Results:ElementDetailUrl %>" 
	CompareResultUrlTemplate="<%$ Results:CompareResultUrl %>" 
/>
<br />
<%} %>
<bx:IncludeComponent runat="server"
	Id="CatalogueElementList"
	ComponentName="bitrix:catalogue.element.list"
	Template=".default" 
	
	SectionId="<%$ Results:SectionId %>" 
	SectionCode="<%$ Results:SectionCode %>" 
	
	SortBy="<%$ Parameters:ListSortBy%>" 
	SortOrder="<%$ Parameters:ListSortOrder%>" 
	
	Properties = "<%$ Parameters:ListProperties %>"
	ShowCatalogItemProperties="<%$ Parameters:ShowCatalogItemProperties %>" 
	
	sectionelementlisturl="<%$ Results:SectionElementListUrl %>"
	elementdetailurl="<%$ Results:ElementDetailUrl %>" 

	pagingindextemplate="<%$ Results:PagingIndexTemplate %>"
	pagingpagetemplate="<%$ Results:PagingPageTemplate %>" 
	pagingshowalltemplate="<%$ Results:PagingShowAllTemplate %>" 
	pagingpageid="<%$ Results:PageId %>" 
	pagingshowall="<%$ Results:ShowAll %>" 
	ComparerUrlTemplate = ""
    FilterByElementCustomProperty = "<%$ Parameters:ListFilterByCustomProperty %>"
    ElementCustomPropertyFilterSettings = "<%$ Parameters:ListCustomPropertyFilterSettings %>"
/>
