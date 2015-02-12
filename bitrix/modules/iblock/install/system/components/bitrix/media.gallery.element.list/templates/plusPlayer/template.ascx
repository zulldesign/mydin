<%@ Reference Control="~/bitrix/components/bitrix/media.gallery.element.list/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.IBlock.Components.MediaGalleryElementListTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.IBlock.Components" %>
<%@ Import Namespace="Bitrix.Services.Js" %>
<%@ Import Namespace="Bitrix.UI" %>

<script runat="server">

    public override string Title
    {
        get { return GetMessageRaw("TemplateTitle"); }
    }
    
    protected string GetPlayerID() {
        return string.Concat(Component.ClientID, "_Player");
    }
    protected string GetPlayerContainerID() {
        return Component.ClientID + ClientIDSeparator + "PlayerContainer";
    }

    protected string GetPlayerWidth()
    {
        return "100%";
    }

    protected string GetPlayerHeight()
    {
        return "420px";
    }

    protected string GetPlayerControlPanelBackgroundColor()
    {
        return "FFFFFF";
    }

    protected string GetPlayerControlsColor()
    {
        return "000000";
    }

    protected string GetPlayerControlsOverColor()
    {
        return "000000";
    }

    protected string GetPlayerScreencolor()
    {
        return "000000";
    }

    protected bool GetPlayerAllowDownloading()
    {
        return false;
    }

    protected bool GetPlayerEnableFullscreen()
    {
        return true; 
    }

    protected int GetPlayerVolume() 
    {
        return 90;
    }
        
    protected MediaGalleryElementListComponent.MediaPlayerStretchingMode GetPlayerStretchingMode()
    {
        return MediaGalleryElementListComponent.MediaPlayerStretchingMode.Proportionally;
    }
</script>

<% if (Component.ComponentErrors != MediaGalleryElementListComponent.Error.ErrNone)
{
    string[] errorNames = Component.ComponentErrors.ToString().Split(',');
    int errorNamesCount = errorNames != null ? errorNames.Length : 0;
    for (int i = 0; i < errorNamesCount; i++) { %>
		<span class="errortext"><%= GetMessage(string.Concat("MediaGalleryElementListComponent", errorNames[i]))%></span><br />
	<% }
	return;
}
else if (Component.Items == null)
   return; %>
   
<script type="text/javascript" src="<%= VirtualPathUtility.ToAbsolute("~/bitrix/js/Main/utils_net.js") %>"></script>	
<script type="text/javascript" src="<%= VirtualPathUtility.ToAbsolute("~/bitrix/controls/main/media.player/wmv/silverlight.js") %>"></script>	
<script type="text/javascript" src="<%= VirtualPathUtility.ToAbsolute("~/bitrix/controls/main/media.player/wmv/wmvplayer.js") %>"></script>	
<script type="text/javascript" src="<%= VirtualPathUtility.ToAbsolute("~/bitrix/controls/main/media.player/js/player.js") + "?t=" + HttpUtility.UrlEncode(System.IO.File.GetLastWriteTimeUtc(Request.MapPath("~/bitrix/controls/main/media.player/js/player.js")).Ticks.ToString()) %>"></script>	
<script type="text/javascript" src="<%= ResolveUrl("./list.js") %>"></script>	


<div id="<%= GetPlayerContainerID() %>" style="width:<%= GetPlayerWidth() %>; height:<%= GetPlayerHeight() %>; background-color:#<%= GetPlayerScreencolor() %>; margin:0px 0px 5px 0px; padding:0px;"></div>

<div class="bx-media-gallery-element-list" style="width:<%= GetPlayerWidth() %>;">
<%  IList<MediaGalleryElementListComponent.ElementListItem> items = Component.Items;	
    for (int i = 0; i < items.Count; i++ ) {
        MediaGalleryElementListComponent.ElementListItem item = items[i];
        string fileUrl = item.ElementFileUrl;
        string playlistPreviewImageFileUrl = item.ElementPlaylistPreviewImageFileUrl;
        string playerPreviewImageFileUrl = item.ElementPlayerPreviewImageFileUrl;	
        string conainerID = GetItemContainerClientID(item.Element.Id); %> 
        
    <div class="bx-media-gallery-element-container" id="<%= conainerID %>">
        <div class="bx-media-gallery-element-normal" style="padding: 10px 0px;" onmouseover="if(this.className=='bx-media-gallery-element-normal') this.className='bx-media-gallery-element-hover'" onmouseout="if(this.className=='bx-media-gallery-element-hover') this.className='bx-media-gallery-element-normal'">
            <div class="bx-media-gallery-element-preview-conainer" onclick="<%= string.Format("Bitrix.MediaGalleryElementListTemplDefault.getEntryById('{0}').play('{1}', {{file:'{2}', image:'{3}'}})", Component.ClientID, conainerID, BXJSUtility.JSHEncode(fileUrl), BXJSUtility.JSHEncode(playerPreviewImageFileUrl)) %>">
                <img src="<%= !string.IsNullOrEmpty(playlistPreviewImageFileUrl) ? HttpUtility.HtmlAttributeEncode(VirtualPathUtility.ToAbsolute(playlistPreviewImageFileUrl)) : HttpUtility.HtmlAttributeEncode(ResolveUrl("./images/preview_img_na.gif")) %>" alt="media gallery element preview" style="height:48px;width:64px;" />
            </div>
            <div class="bx-media-gallery-element-description-container">
                <a class="bx-media-gallery-element-description" onclick="<%= string.Format("Bitrix.MediaGalleryElementListTemplDefault.getEntryById('{0}').play('{1}', {{file:'{2}', image:'{3}'}})", Component.ClientID, conainerID, BXJSUtility.JSHEncode(fileUrl), BXJSUtility.JSHEncode(playerPreviewImageFileUrl)) %>"><%= HttpUtility.HtmlEncode(item.Element.Name) %></a>              
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
				    <div class="bx-media-gallery-element-announcement" style="clear: both;">
                        <% if (!string.IsNullOrEmpty(item.ElementPreviewText))
                           {%>
                            <p class="announcement">
                                <%= item.ElementPreviewText %>
                            </p>                                   
                        <%} %>  				    
				    </div>				    
                </div>
            </div>
            <div style="clear: both;"></div>
        </div>
        <div class="bx-media-gallery-element-delimiter-gray-mono-grad2"></div>
    </div> 
    <% RenderElementToolbar(item.Element, conainerID); %> 
<% }%>
    <script type="text/javascript">
    	window.setTimeout(function() {
    		var player = Bitrix.MediaGalleryElementListTemplDefault.create('<%= Component.ClientID %>', '<%= GetPlayerID() %>');
    		<% if(items.Count > 0) {%>
    		player.ensureItemCreated("<%= GetItemContainerClientID(items[0].Element.Id) %>", 
    			{ 
    				file:"<%= BXJSUtility.JSHEncode(items[0].ElementFileUrl) %>", 
    				image:"<%= BXJSUtility.JSHEncode(items[0].ElementPlayerPreviewImageFileUrl) %>" 
    			});
			<%} %>
            Bitrix.MediaPlayer.create('<%= GetPlayerID() %>', '<%= GetPlayerContainerID() %>', 
				Bitrix.MediaPlayerData.create(
					{ 
						file:'<%= items.Count > 0 ? BXJSUtility.JSHEncode(items[0].ElementFileUrl) : string.Empty %>', 
						image:'<%= items.Count > 0 ? BXJSUtility.JSHEncode(items[0].ElementPlayerPreviewImageFileUrl) : string.Empty %>', 
						width:'<%= GetPlayerWidth() %>', 
						height:'<%= GetPlayerHeight() %>', 
						stretching:'<%= GetPlayerStretchingMode().ToString() %>', 
						controlpanelbackgroundcolor:'<%= GetPlayerControlPanelBackgroundColor() %>', 
						controlscolor:'<%= GetPlayerControlsColor() %>',
						controlsovercolor:'<%= GetPlayerControlsOverColor() %>', 
						screencolor:'<%= GetPlayerScreencolor() %>', 
						enabledownloading:<%= GetPlayerAllowDownloading().ToString().ToLowerInvariant() %>, 
						fullscreen:<%= GetPlayerEnableFullscreen().ToString().ToLowerInvariant() %>, 
						volume:<%= GetPlayerVolume().ToString() %>,
						autostart:false, flvwmode:'opaque', wmvwmode:'windowless', wmvshowstopbutton:true, bufferlength:10, repeat:false 
					})).activate();                  		
		}, 100);
    </script>
</div>