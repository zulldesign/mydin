<%@ Control Language="C#" ClassName="list" Inherits="Bitrix.UI.BXComponentTemplate" %>
<script runat="server">
</script>

<bx:IncludeComponent runat="server" ID="myElementList"
	ComponentName="bitrix:iblock.myElement.list"
	Template=".default" 
	ElementCreationUrl="<%$ Results:ElementCreationUrl %>"
	ElementModificationUrl="<%$ Results:ElementModificationUrl %>"
	ElementCreationUrlTitle="<%$ Parameters:CreateButtonTitle %>"
	PagingIndexTemplate="<%$ Results:PagingIndexTemplate %>"
	PagingPageTemplate="<%$ Results:PagingPageTemplate %>" 
	PagingShowAllTemplate="<%$ Results:PagingShowAllTemplate %>" 
	PagingPageID="<%$ Results:PageID %>" 
	PagingShowAll="<%$ Results:ShowAll %>" 
/>
