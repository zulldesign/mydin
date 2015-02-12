<%@ Control Language="C#" AutoEventWireup="true" CodeFile="upload.ascx.cs" Inherits="bitrix_components_bitrix_photogallery_templates__default_upload" %>
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