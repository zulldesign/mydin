<%@ Reference VirtualPath="~/bitrix/components/bitrix/photogallery/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.IBlock.Components.PhotoGalleryTemplate" %>

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
	PagingPageId="<%$ Results:PageId %>"
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
	CommentAddUrlTemplate="<%$Results: CommentAddUrlTemplate %>"
	RecordsPerPage="4"
 />