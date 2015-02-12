<%@ Control Language="C#" ClassName="photo" Inherits="Bitrix.UI.BXComponentTemplate" %>
               
<bx:IncludeComponent 
    runat="server" 
    ID="PhotoViewer" 
    ComponentName="bitrix:photogallery.photo" 
    Template="new" 
    CacheMode="<%$ Parameters:CacheMode %>" 
    CacheDuration="<%$ Parameters:CacheDuration %>"
    EnableAjax="<%$ Parameters:EnableAjax %>"
    EnableSEF="<%$ Parameters:EnableSEF %>" 
	SEFFolder="<%$ Parameters:SEFFolder %>"
	 
    GalleryRoot="<%$ Results:GalleryRoot %>"
	UrlTemplateAlbum="<%$ Results:UrlTemplateAlbum %>"
	UrlTemplateAlbumEdit="<%$ Results:UrlTemplateAlbumEdit %>"
	UrlTemplatePhoto="<%$ Results:UrlTemplatePhoto %>"
	UrlTemplatePhotoEdit="<%$ Results:UrlTemplatePhotoEdit %>"
	UrlTemplateUpload="<%$ Results:UrlTemplateUpload %>"  
     />