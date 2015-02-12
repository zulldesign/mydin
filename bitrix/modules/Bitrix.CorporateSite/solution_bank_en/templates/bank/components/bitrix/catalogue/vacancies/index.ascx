<%@ Control Language="C#" Inherits="Bitrix.UI.BXComponentTemplate" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/catalogue.section.tree/component.ascx" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/catalogue.element.list/component.ascx" %>
<%@ Import Namespace="Bitrix.IBlock.Components" %>

<% 
	if (IsComponentDesignMode)
	{
		CatalogueElementListComponent elementList = (CatalogueElementListComponent)CatalogueElementList.Component;
	}
%>

<bx:IncludeComponent runat="server"
	Id="CatalogueElementList"
	ComponentName="bitrix:catalogue.element.list"
	Template=".default" 
	
	SectionId="" 
	SectionCode="" 
	
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
