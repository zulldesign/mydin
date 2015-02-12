<%@ Reference VirtualPath="~/bitrix/components/bitrix/media.player/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.Main.Components.MediaPlayerComponentTemplate" %>
<%@ Import  Namespace="Bitrix.UI"%>
<%@ Import  Namespace="Bitrix.Main.Components"%>
<script type="text/javascript" src="<%= VirtualPathUtility.ToAbsolute("~/bitrix/js/Main/utils_net.js") %>"></script>	
<script type="text/javascript" src="<%= VirtualPathUtility.ToAbsolute("~/bitrix/controls/main/media.player/wmv/silverlight.js") %>"></script>	
<script type="text/javascript" src="<%= VirtualPathUtility.ToAbsolute("~/bitrix/controls/main/media.player/wmv/wmvplayer.js") %>"></script>	
<script type="text/javascript" src="<%= VirtualPathUtility.ToAbsolute("~/bitrix/controls/main/media.player/js/player.js") + "?t=" + HttpUtility.UrlEncode(System.IO.File.GetLastWriteTimeUtc(Request.MapPath("~/bitrix/controls/main/media.player/js/player.js")).Ticks.ToString()) %>"></script>	

<%	if (!Component.Parameters.ContainsKey("abouttext"))
		Component.Parameters["abouttext"] = Component.AboutText; 

	if (!Component.Parameters.ContainsKey("aboutlink"))
		Component.Parameters["aboutlink"] = Component.AboutLink;

   System.Web.Script.Serialization.JavaScriptSerializer jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
   StringBuilder sb = new StringBuilder();
   jsSerializer.Serialize(Parameters, sb);%>

<script type="text/javascript">
   window.setTimeout(
		function(){			
			Bitrix.MediaPlayer.messages = {
				installFlashPlayer: '<%= GetMessageJS("InstallFlashPlayer")  %>',
				couldntCreatePlayer: '<%= GetMessageJS("CouldntCreatePlayerErrorMsg") %>',
				couldntCreateFlashPlayer: '<%= GetMessageJS("CouldntCreateFlashPlayerErrorMsg") %>',
				couldntCreateSilverlightPlayer: '<%= GetMessageJS("CouldntCreateSilverlightPlayerErrorMsg") %>'
			};			
			Bitrix.MediaPlayer.create('<%= Component.ClientID %>', 
				'<%= string.Concat(Component.ClientID, "_Container") %>', 
				Bitrix.MediaPlayerData.create(<%= sb.ToString() %>)).activate(); 
		}, 100); 
</script>   

<div id="<%= HttpUtility.HtmlAttributeEncode(string.Concat(Component.ClientID, "_Container")) %>"></div>

