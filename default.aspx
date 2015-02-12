<%@ Page Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.BXPublicPage, Main"  Title="Furniture Company" %>

<script runat="server" id="@__bx_pagekeywords">
	public override void SetPageKeywords(System.Collections.Generic.IDictionary<string, string> keywords)
	{
		keywords[@"guga"]=@"";
	}
</script>


<asp:Content ID="Content1" ContentPlaceHolderID="BXContent" runat="server" > 
 
<p>Furniture Company was founded in 2001 under the idea of designing & manufacturing low-cost, solid-wood furniture.
</p>
<p>Our ethos is based entirely on customer service and we listen closely to our client’s criteria in all aspects of design, budget and timescale fulfilling  them in every way possible.</p>
 
<div class="hr"></div>
 
<bx:IncludeComponent 
	id="cataloguesection_tree1" 
	runat="server" 
	componentname="bitrix:catalogue.section.tree" 
	template="main_links" 
	IBlockTypeId="" 
	IBlockId="<%$ Options:Bitrix.CorporateSite:CatalogIblockId,int %>" 
	SectionId="" 
	SectionCode="" 
	DepthLevel="1" 
	ShowCounters="False" 
	CountSubElements="True" 
	IncludeParentSections="True" 
	AddAdminPanelButtons="False" 
	Template_RootTitle="" 
	Template_RootUrl="" 
	SectionUrl="products/#SECTION_ID#/" 
	CacheMode="None" 
	CacheDuration="30" 
	FilterBySectionCustomProperty="True" 
	SectionCustomPropertyFilterSettings="p:o:2:{s:&quot;TEST&quot;;b:1;s:&quot;SHOWONMAIN&quot;;b:1;};" 
/> 
<bx:IncludeComponent 
	id="catalogueelement_list1" 
	runat="server" 
	componentname="bitrix:catalogue.element.list" 
	template="main_links" 
	IBlockTypeId="" 
	IBlockId="<%$ Options:Bitrix.CorporateSite:ServicesIblockId,int %>" 
	SectionId="0" 
	SectionCode="" 
	SortBy="ID" 
	SortOrder="Desc" 
	ShowSubElements="True" 
	ShowAllElementsOnIndex="True" 
	ActiveFilter="Active" 
	ActiveDateFilter="All" 
	Properties="-" 
	AddAdminPanelButtons="False" 
	SetPageTitle="True" 
	SectionElementListUrl="services/#SECTION_ID#/" 
	ElementDetailUrl="services/item-#ELEMENT_ID#/" 
	PagingAllow="True" 
	PagingMode="direct" 
	PagingTemplate=".default" 
	PagingShowOne="False" 
	PagingAllowAll="False" 
	PagingRecordsPerPage="10" 
	PagingTitle="Pages" 
	PagingPosition="both" 
	PagingMaxPages="10" 
	PagingMinRecordsInverse="1" 
	PagingPageID="<%$ Request:page %>" 
	PagingShowAll="<%$ Request:showall %>" 
	PagingIndexTemplate="" 
	PagingPageTemplate="?page=#pageid#" 
	PagingShowAllTemplate="?showall=y" 
	CacheMode="None" 
	CacheDuration="30" 
	FilterByElementCustomProperty="False" 
	ElementCustomPropertyFilterSettings="p:o:0:{};" 
/> </asp:Content>

