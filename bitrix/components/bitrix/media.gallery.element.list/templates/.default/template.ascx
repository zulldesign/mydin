<%@ Reference Control="~/bitrix/components/bitrix/media.gallery.element.list/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.IBlock.Components.MediaGalleryElementListTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.Services.Js" %>
<%@ Import Namespace="Bitrix.IBlock.Components" %>

<%
if (Component.ComponentErrors != MediaGalleryElementListComponent.Error.ErrNone)
{
    string[] errorNames = Component.ComponentErrors.ToString().Split(',');
    int errorNamesCount = errorNames != null ? errorNames.Length : 0;
    for (int i = 0; i < errorNamesCount; i++)
    {
        %><span class="errortext"><%= GetMessage(string.Concat("MediaGalleryElementListComponent", errorNames[i]))%></span><br /><%
    }
	return;
}
else if (Component.Items == null)
   return;
%>

<div class="news-list-pager">
<bx:IncludeComponent runat="server" ID="HeaderPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="top" />
</div>
<div class="bx-media-gallery-element-list">
<%  IList<MediaGalleryElementListComponent.ElementListItem> items = Component.Items;
    for (int i = 0; i < items.Count; i++ )
    {
        MediaGalleryElementListComponent.ElementListItem item = items[i];
        string fileUrl = item.ElementFileUrl;
        string playlistPreviewImageFileUrl = item.ElementPlaylistPreviewImageFileUrl;
        string playerPreviewImageFileUrl = item.ElementPlayerPreviewImageFileUrl;
        string conainerID = GetItemContainerClientID(item.Element.Id);
        %> 
    <div class="bx-media-gallery-element-container" id="<%= conainerID %>">
        <div class="bx-media-gallery-element-normal" style="padding: 10px 0px;" onmouseover="if(this.className=='bx-media-gallery-element-normal') this.className='bx-media-gallery-element-hover'" onmouseout="if(this.className=='bx-media-gallery-element-hover') this.className='bx-media-gallery-element-normal'">
            <div class="bx-media-gallery-element-preview-conainer" >
                <a href="<%= HttpUtility.HtmlAttributeEncode(item.ElementDetailUrl) %>" enableajax="true" >
                    <img src="<%= !string.IsNullOrEmpty(playlistPreviewImageFileUrl) ? HttpUtility.HtmlAttributeEncode(VirtualPathUtility.ToAbsolute(playlistPreviewImageFileUrl)) : HttpUtility.HtmlAttributeEncode(ResolveUrl("./images/preview_img_na.gif")) %>" alt="media gallery element preview" style="height:48px;width:64px;" />
                </a>
            </div>
            <div class="bx-media-gallery-element-description-container">
                <a class="bx-media-gallery-element-description" href="<%= HttpUtility.HtmlAttributeEncode(item.ElementDetailUrl) %>" >
                    <%= HttpUtility.HtmlEncode(item.Element.Name) %>
                </a>
                <% if (!string.IsNullOrEmpty(item.ElementPreviewText))
                   {%>
                    <p class="announcement">
                        <%= item.ElementPreviewText %>
                    </p>                                   
                <%} %>
                <div class="bx-media-gallery-element-description-param-line-bottom">
				    <% bool isFirstParam = true;
                    foreach (MediaGalleryElementListComponent.ElementListItemProperty property in item.Properties)
				    {
					    if (!String.IsNullOrEmpty(property.DisplayValue) && Component.ShowProperties.Contains(property.Code))
					    {
                            if (!isFirstParam)
                            {%>
                                <div class="bx-media-gallery-element-description-param-delimiter"></div> 
                            <%}
                              else
                                  isFirstParam = false;%>
                            <div class="bx-media-gallery-element-description-param-line">
						        <%= property.DisplayValue %>
						    </div>
					    <%}
				    }%>
				    <div style="clear: both;"></div>
                </div>
            </div>
            <div style="clear: both;"></div>
        </div>
        <div class="bx-media-gallery-element-delimiter-gray-mono-grad2"></div>
    </div> 
	<% RenderElementToolbar(item.Element, conainerID); %>	     
<% }%>
</div>


<div class="news-list-pager">
<bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager" CurrentPosition="bottom" Template="<%$ Parameters:PagingTemplate %>"/>
</div>