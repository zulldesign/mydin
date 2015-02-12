<%@ Control Language="C#" AutoEventWireup="true" CodeFile="album.ascx.cs" Inherits="bitrix_components_bitrix_photogallery_templates_album" %>
<bx:IncludeComponent 
    runat="server" 
    id="Albums" 
    ComponentName="bitrix:photogallery.album" 
    Template="new" 
    CacheMode="<%$ Parameters:CacheMode %>" 
    CacheDuration="<%$ Parameters:CacheDuration %>" 
    EnableSef="<%$ Parameters:EnableSef %>"
	SefFolder="<%$ Parameters:SefFolder %>" 
	EnableAjax="<%$ Parameters:EnableAjax %>"
	
	AlbumId="<%$ Results:AlbumId %>"
	PageId="<%$ Results:PageId %>"
	PageShowAll="<%$ Results:PageShowAll %>"
	
	GalleryRoot="<%$ Results:GalleryRoot %>"
	
	UrlTemplateAlbum="<%$ Results:UrlTemplateAlbum %>"
	UrlTemplateAlbumPage="<%$ Results:UrlTemplateAlbumPage %>"
	UrlTemplateAlbumShowAll="<%$ Results:UrlTemplateAlbumshowAll %>"
	UrlTemplateAlbumEdit="<%$ Results:UrlTemplateAlbumEdit %>"
	UrlTemplateAlbumAdd="<%$ Results:UrlTemplateAlbumAdd %>"
	UrlTemplatePhoto="<%$ Results:UrlTemplatePhoto %>"
	UrlTemplatePhotoEdit="<%$ Results:UrlTemplatePhotoEdit %>"
	UrlTemplateUpload="<%$ Results:UrlTemplateUpload %>"
	
	RecordsPerPage="4"
 />