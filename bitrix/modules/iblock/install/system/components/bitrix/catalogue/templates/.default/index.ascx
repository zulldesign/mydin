<%@ Control Language="C#" Inherits="Bitrix.UI.BXComponentTemplate" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/catalogue.section.tree/component.ascx" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/catalogue.element.list/component.ascx" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/catalogue.compare.list/component.ascx" %>
<%@ Import Namespace="Bitrix.IBlock.Components" %>

<% 
	if (IsComponentDesignMode)
	{
		CatalogueSectionTreeComponent sectionTree = (CatalogueSectionTreeComponent)CatalogueSectionTree.Component;
		CatalogueElementListComponent elementList = (CatalogueElementListComponent)CatalogueElementList.Component;
	
		if (sectionTree.TreeItems == null || elementList.Items == null)
		{
			%><%= GetMessage("YouHaveToAdjustCatalogue") %><%
			return;
		}
	}
%>

<bx:IncludeComponent runat="server"
	ID="CatalogueSectionTree" 	 
	ComponentName="bitrix:catalogue.section.tree" 
	Template=".default" 
	
	SectionUrl="<%$ Results:SectionElementListUrl %>"
	
	CountSubElements="<%$ Parameters:ShowSubElements %>"
	AddAdminPanelButtons="N"
	
	SectionId="" 
	SectionCode="" 
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
	
	SectionId="" 
	SectionCode="" 
	
	SortBy="<%$ Parameters:ListSortBy%>" 
	SortOrder="<%$ Parameters:ListSortOrder%>" 
	
	ShowCatalogItemProperties="<%$ Parameters:ShowCatalogItemProperties %>" 		
	Properties="<%$ Parameters:ListProperties %>" 
	
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
