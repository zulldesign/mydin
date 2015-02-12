<%@ Control Language="C#" AutoEventWireup="true" CodeFile="album.edit.ascx.cs" Inherits="bitrix_components_bitrix_photogallery_templates_album_edit" %>
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
    />