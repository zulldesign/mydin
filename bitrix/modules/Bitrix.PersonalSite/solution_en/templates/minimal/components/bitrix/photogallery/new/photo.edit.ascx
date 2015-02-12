<%@ Control Language="C#" ClassName="photo" Inherits="Bitrix.UI.BXComponentTemplate" %>
<bx:IncludeComponent 
    runat="server" 
    ID="PhotoViewer" 
    ComponentName="bitrix:photogallery.photo.edit" 
    Template="new" 
    CacheMode="<%$ Parameters:CacheMode %>" 
    CacheDuration="<%$ Parameters:CacheDuration %>" 
    EnableAjax="<%$ Parameters:EnableAjax %>"
    EnableSEF="<%$ Parameters:EnableSEF %>" 
	SEFFolder="<%$ Parameters:SEFFolder %>"
    
    PhotoId="<%$ Results:PhotoId %>"
    GalleryRoot="<%$ Results:GalleryRoot %>"
	UrlTemplatePhoto="<%$ Results:UrlTemplatePhoto %>"
	UrlTemplatePhotoEdit="<%$ Results:UrlTemplatePhotoEdit %>"
    />