<%@ Reference VirtualPath="~/bitrix/components/bitrix/photogallery/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.IBlock.Components.PhotoGalleryTemplate" %>
<bx:IncludeComponent 
    runat="server" 
    id="AlbumEdit" 
    ComponentName="bitrix:photogallery.album.edit" 
    Template="new" 
    CacheMode="<%$ Parameters:CacheMode %>" 
    CacheDuration="<%$ Parameters:CacheDuration %>"  
    EnableSef="<%$ Parameters:EnableSef %>"
	SefFolder="<%$ Parameters:SefFolder %>" 
	EnableAjax="<%$ Parameters:EnableAjax %>"
    AlbumParentID="<%$ Results:AlbumParentID %>"
    AlbumID="<%$ Results:AlbumID %>"
    Action="<%$Results:Action %>"
    />