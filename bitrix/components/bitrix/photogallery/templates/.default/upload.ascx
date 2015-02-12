<%@ Reference VirtualPath="~/bitrix/components/bitrix/photogallery/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.IBlock.Components.PhotoGalleryTemplate" %>

<bx:IncludeComponent 
    runat="server" 
    ID="Uploader" 
    ComponentName="bitrix:photogallery.upload" 
    Template="new" 
    CacheMode="None" 
    CacheDuration="0"
    EnableAjax="<%$ Parameters:EnableAjax %>"
    EnableSEF="<%$ Parameters:EnableSEF %>" 
	SEFFolder="<%$ Parameters:SEFFolder %>"
    
    AlbumId="<%$ Results:AlbumId %>"
    UrlTemplateUpload="<%$ Results:UrlTemplateUpload %>"
    UrlTemplateAlbum="<%$ Results:UrlTemplateAlbum %>"
    />