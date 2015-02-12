<%@ Reference VirtualPath="~/bitrix/components/bitrix/photogallery/component.ascx" %>
<%@ Control Language="C#" ClassName="photo" Inherits="Bitrix.IBlock.Components.PhotoGalleryTemplate" %>
         
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
	CommentId = "<%$ Results:CommentId %>"
    GalleryRoot="<%$ Results:GalleryRoot %>"
	UrlTemplateAlbum="<%$ Results:UrlTemplateAlbum %>"
	UrlTemplateAlbumPage="<%$ Results:UrlTemplateAlbumPage %>"
	UrlTemplateAlbumEdit="<%$ Results:UrlTemplateAlbumEdit %>"
	UrlTemplatePhoto="<%$ Results:UrlTemplatePhoto %>"
	UrlTemplatePhotoEdit="<%$ Results:UrlTemplatePhotoEdit %>"
	UrlTemplateUpload="<%$ Results:UrlTemplateUpload %>" 
	PageId = "<%$ Results:PageId %>"
	CommentReadPageUrlTemplate="<%$ Results:CommentReadPageUrlTemplate %>" 
	CommentReadUrlTemplate="<%$ Results:CommentReadUrlTemplate %>"
	CommentOperationUrlTemplate="<%$ Results:CommentOperationUrlTemplate %>"
	CommentOperation="<%$ Results:Operation %>"
     />
     
