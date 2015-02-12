<%@ Control Language="C#" Inherits="Bitrix.UI.BXComponentTemplate" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/catalogue.compare.list/component.ascx" %>
<%@ Import Namespace="Bitrix.IBlock.Components" %>
<bx:IncludeComponent 
	id="CatalogueCompareResult" 
	runat="server" 
	componentname="bitrix:catalogue.compare.result" 
	template=".default" 
	SelectedFields="<%$Parameters:ComparisonSelectedFields %>" 
	ElementUrlTemplate="<%$ Results:ElementDetailUrl %>" 
	ElementListUrlTemplate="<%$ Results:IndexTemplate %>" 
/>