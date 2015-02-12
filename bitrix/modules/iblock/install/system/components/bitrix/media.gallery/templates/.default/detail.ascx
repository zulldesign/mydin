<%@ Control Language="C#" ClassName="list" Inherits="Bitrix.UI.BXComponentTemplate" %>
<script runat="server">
</script>

<bx:IncludeComponent runat="server" ID="mediaGalleryElementList" 	 
	ComponentName="bitrix:media.gallery.element.detail" Template=".default" 
	ElementId="<%$ Results:ElementId %>" 
	ElementCode="<%$ Results:ElementCode %>"
	Properties="<%$ Parameters:DetailProperties %>"
/>
