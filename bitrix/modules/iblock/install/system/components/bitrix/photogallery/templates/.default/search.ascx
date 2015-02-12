<%@ Reference VirtualPath="~/bitrix/components/bitrix/photogallery/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.IBlock.Components.PhotoGalleryTemplate" %>
<bx:IncludeComponent 
    runat="server" 
    ID="PhotoViewer" 
    ComponentName="bitrix:photogallery.search" 
    Template="new" 
    CacheMode="<%$ Parameters:CacheMode %>" 
    CacheDuration="<%$ Parameters:CacheDuration %>" 
    EnableAjax="<%$ Parameters:EnableAjax %>"
    EnableSEF="<%$ Parameters:EnableSEF %>" 
	SEFFolder="<%$ Parameters:SEFFolder %>"
    />