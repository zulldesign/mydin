<%@ Control Language="C#" ClassName="list" Inherits="Bitrix.UI.BXComponentTemplate" %>
<script runat="server"></script>

<bx:IncludeComponent runat="server" ID="mediaGalleryElementList" 	 
	ComponentName="bitrix:media.gallery.element.list" Template=".default" 
	SectionId="" 
	SectionCode="" 
    ElementDetailUrl ="<%$ Results:ElementDetailUrl %>"	
	SortBy="<%$ Parameters:TopSortBy%>" 
	SortOrder="<%$ Parameters:TopSortOrder%>" 
	Properties="<%$ Parameters:TopProperties %>" 
	SectionElementListUrl="<%$ Results:SectionElementListUrl %>"
	PagingAllow="False"
	PagingRecordsPerPage = "<%$ Parameters:TopElementCount %>"    
/>