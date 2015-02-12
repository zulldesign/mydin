<%@ Reference Control="~/bitrix/components/bitrix/media.gallery.element.detail/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.IBlock.Components.MediaGalleryElementDetailTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.UI" %>
<%@ Import Namespace="Bitrix.Services.Js" %>
<%@ Import Namespace="Bitrix.IBlock.Components" %>
<script runat="server">
    protected string GetPlayerID()
    {
        return string.Concat(Component.ClientID, "_Player");
    }
    protected string GetPlayerContainerID() 
	{
        return Component.ClientID + ClientIDSeparator + "PlayerContainer";
    }

    protected string GetPlayerContainerStyle()
    {
        string screenColor = Component.PlayerScreenColor;
        return string.Format(
            "width:{0}; height:{1}; background-color:#{2};",
            Component.PlayerWidth,
            Component.PlayerHeight,
            !string.IsNullOrEmpty(screenColor) ? screenColor : "000000"
            );
    }            
</script>

<%
if (Component.ComponentErrors != MediaGalleryElementDetailComponent.ComponentError.ErrNone)
{ 
    string[] errorNames = Component.ComponentErrors.ToString().Split(',');
    int errorNamesCount = errorNames != null ? errorNames.Length : 0;
    for (int i = 0; i < errorNamesCount; i++)
    {
        %><span class="errortext"><%= GetMessage(string.Concat("MediaGalleryElementDetailComponent", errorNames[i]))%></span><br /><%
    }
}
else if (Component.Element != null)
{%>
	<script type="text/javascript" src="<%= VirtualPathUtility.ToAbsolute("~/bitrix/js/Main/utils_net.js") %>"></script>	
	<script type="text/javascript" src="<%= VirtualPathUtility.ToAbsolute("~/bitrix/controls/main/media.player/wmv/silverlight.js") %>"></script>	
	<script type="text/javascript" src="<%= VirtualPathUtility.ToAbsolute("~/bitrix/controls/main/media.player/wmv/wmvplayer.js") %>"></script>	
	<script type="text/javascript" src="<%= VirtualPathUtility.ToAbsolute("~/bitrix/controls/main/media.player/js/player.js") + "?t=" + HttpUtility.UrlEncode(System.IO.File.GetLastWriteTimeUtc(Request.MapPath("~/bitrix/controls/main/media.player/js/player.js")).Ticks.ToString()) %>"></script>	

	<div class="catalog-element">
	
	<table width="100%" cellspacing="0" cellpadding="2" border="0">
	<tr>
	    <td width="100%" valign="top" colspan="2" align="left">
	        <div id="<%= GetPlayerContainerID() %>"  style="<%= GetPlayerContainerStyle() %>"></div>
	    </td>
    </tr>	
		<%
        foreach (MediaGalleryElementDetailComponent.ElementDetailProperty property in Component.Properties)
		{
			if (!String.IsNullOrEmpty(property.DisplayValue) && Component.ShowProperties.Contains(property.Code))
			{
				%><tr width="100%"><td width="0%" valign="top"><%=property.Name%>:</td><td width="100%" valign="top"><b><%=property.DisplayValue%></b></td></tr><%
			}
		}
		%>
	</table>
	<br />
	<%
	if (Component.Element.DetailText.Length > 0)
	{
		%><%= Component.Element.DetailText%><br /><%
	}
	else if (Component.Element.PreviewText.Length > 0)
	{
		%><%= Component.Element.PreviewText%><br /><%
	}
	
	%></div>
	
	<% string playbackUrl = Component.ElementFileUrl;
       string downloadingUrl = string.Empty;
       if (Component.PlayerEnableDownloading) 
            downloadingUrl = Component.ElementPropertyForDownloadingFilePath;%>
	
	<script type="text/javascript">
		Bitrix.MediaPlayer.messages = {
			installFlashPlayer: '<%= GetMessageJS("InstallFlashPlayer")  %>',
			couldntCreatePlayer: '<%= GetMessageJS("CouldntCreatePlayerErrorMsg") %>',
			couldntCreateFlashPlayer: '<%= GetMessageJS("CouldntCreateFlashPlayerErrorMsg") %>',
			couldntCreateSilverlightPlayer: '<%= GetMessageJS("CouldntCreateSilverlightPlayerErrorMsg") %>'
		};
		
        Bitrix.MediaPlayer.create('<%= GetPlayerID() %>', 
			'<%= GetPlayerContainerID() %>', 
			Bitrix.MediaPlayerData.create({ 
				file:'<%= BXJSUtility.JSHEncode(playbackUrl) %>', 
				image:'<%= BXJSUtility.JSHEncode(Component.ElementPlayerPreviewImageFileUrl) %>',  
				width:'<%= Component.PlayerWidth %>', 
				height:'<%= Component.PlayerHeight %>', 
				stretching:'<%= Component.PlayerStretching %>', 
				bufferlength:<%= Component.PlayerBufferLengthInSeconds %>, 
				controlpanelbackgroundcolor:'<%= Component.PlayerControlPanelBackgroundColor %>', 
				controlscolor:'<%= Component.PlayerControlsColor %>', 
				controlsovercolor:'<%= Component.PlayerControlsOverColor %>', 
				screencolor:'<%= Component.PlayerScreenColor %>', 
				link:'<%= string.IsNullOrEmpty(downloadingUrl) ? string.Empty : BXJSUtility.JSHEncode(downloadingUrl) %>', 
				linktarget:'<%= string.IsNullOrEmpty(downloadingUrl) ? string.Empty : Component.PlayerDownloadingLinkTargetWindow %>', 
				showcontrolpanel: <%= Component.PlayerShowControlPanel.ToString().ToLowerInvariant() %>, 
				enabledownloading: <%= Component.PlayerEnableDownloading.ToString().ToLowerInvariant() %>, 
				fullscreen:<%= Component.PlayerEnableFullScreenModeSwitch.ToString().ToLowerInvariant() %>, 
				volume:<%= Component.PlayerVolumeLevelInPercents.ToString() %>, 
				repeat:<%= Component.PlayerEnableRepeatMode.ToString().ToLowerInvariant() %>, 
				autostart:<%= Component.PlayerEnableAutoStart.ToString().ToLowerInvariant() %>,
				flvwmode:'opaque', 
				wmvwmode:'windowless' })).activate();
	</script>
	
<% } %>

<% if (!String.IsNullOrEmpty(Component.IBlockUrl) && !String.IsNullOrEmpty(Component.IBlockUrlTitle)){%>
	<p><a enableajax="true" href="<%= Component.IBlockUrl %>"><%= Component.IBlockUrlTitle %></a></p>
<%} %>