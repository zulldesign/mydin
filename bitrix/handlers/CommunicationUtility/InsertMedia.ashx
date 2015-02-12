<%@ WebHandler Language="C#" Class="InsertMedia" %>

using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Web.Hosting;
using System.Web.SessionState;
using Bitrix.Main;
using Bitrix.Services.Js;

public class InsertMedia : IHttpHandler, IRequiresSessionState
{
    internal static long ticker = 0;
	
    public void ProcessRequest (HttpContext context) 
	{
		HttpRequest request = context.Request;
		HttpResponse response = context.Response;    
		
		response.ContentType = "text/html";

		Dictionary<string, object> parameters = new Dictionary<string, object>();
		string url = request.QueryString["url"];
		if (string.IsNullOrEmpty(url))
		{
			response.StatusCode = 404;
			response.End();
		}
		parameters["url"] = url;
		if(request.QueryString["audioOnly"] != null)
			parameters["audioOnly"] = true;
		parameters["flvskinfolderpath"] = "~/bitrix/controls/main/media.player/flv/skins";
		parameters["flvskinname"] = "bitrix.swf";
		parameters["abouttext"] = "1C-Bitrix Media Player";
        parameters["flvwmode"] = "transparent";
        parameters["wmvwmode"] = "windowless";
        
		int i;
		if (int.TryParse(request.QueryString["width"], out i) && i > 0)
			parameters["width"] = i + "px";
		if (int.TryParse(request.QueryString["height"], out i) && i > 0)
			parameters["height"] = i + "px";
		if (request.QueryString["nofullscreen"] != null)
			parameters["fullscreen"] = false;
		
		response.StatusCode = 200;
	
		string jsPathString = HttpRuntime.AppDomainAppVirtualPath;
		if (jsPathString == "/")
			jsPathString = string.Empty;
		jsPathString = BXJSUtility.Encode(jsPathString);

		response.Write(
@"<html>
<head>
	<script type=""text/javascript"">
		var bitrixWebAppPath = '" + jsPathString + @"'; 
		var APPPath = '" + jsPathString + @"';
		function ResizeIFrame()
		{
			if (!window.frameElement)			
				return;			
			window.frameElement.style.width = document.body.scrollWidth + 'px';			
			window.frameElement.style.height = document.body.scrollHeight + 'px';
		}				
	</script>
	<style type=""text/css"">
	body { margin: 0px; padding: 0px; border: 0px none; }
	div.bx-audio-player-container 
	{
		height: auto;
		margin: 0;
		overflow: hidden;
		padding: 0;
		position: relative;
		width: auto;
	}
	div.bx-audio-player-screen-stub
	{
		background-color: #FFFFFF;
		border: 0 none;
		height: 5px;
		left: 0;
		margin: 0;
		overflow: hidden;
		padding: 0;
		position: absolute;
		top: 0;
		width: 405px;	
	}
	div.bx-audio-player-inner-container 
	{
	}
	</style>	
"
		);
		RenderScript(response, "~/bitrix/js/Main/utils.js");
		RenderScript(response, "~/bitrix/js/Main/utils_net.js");
		RenderScript(response, "~/bitrix/controls/main/media.player/wmv/silverlight.js");
		RenderScript(response, "~/bitrix/controls/main/media.player/wmv/wmvplayer.js");
		RenderScript(response, "~/bitrix/controls/main/media.player/js/player.js");
        
        response.Write(
            string.Format("<script type=\"text/javascript\"> Bitrix.MediaPlayer.messages = {{ installFlashPlayer:'{0}', couldntCreatePlayer:'{1}', couldntCreateFlashPlayer:'{2}', couldntCreateSilverlightPlayer:'{3}' }};</script>",
                BXJSUtility.Encode(BXMain.GetModuleMessage("MediaPlayer", "InstallFlashPlayer", false)),
                BXJSUtility.Encode(BXMain.GetModuleMessage("MediaPlayer", "CouldntCreatePlayerErrorMsg", false)),
                BXJSUtility.Encode(BXMain.GetModuleMessage("MediaPlayer", "CouldntCreateFlashPlayerErrorMsg", false)),
                BXJSUtility.Encode(BXMain.GetModuleMessage("MediaPlayer", "CouldntCreateSilverlightPlayerErrorMsg", false))
                )
            );        
        
		response.Write(
@"</head>
<body>
	<div id=""player""></div>
	<script type=""text/javascript"">
		window.setTimeout(function()
		{
			var player = Bitrix.MediaPlayer.create(
				'player" + System.Threading.Interlocked.Increment(ref ticker) + @"',
				'player',
				Bitrix.MediaPlayerData.create("
		);
		response.Write(BXJSUtility.BuildJSON(parameters));
		response.Write(
@")
			);
			player.addActivationChangeListener(function(a, b)
			{		
				window.setTimeout(ResizeIFrame, 100);
			});
			player.activate();
		}, 100);
	</script>		
</body>
</html>"
		);
    }
	
	static void RenderScript(HttpResponse response, string virtualPath)
	{
		response.Write("\t<script src=\"");
        
		response.Write(
            string.Concat(
                HttpUtility.HtmlAttributeEncode(VirtualPathUtility.ToAbsolute(virtualPath)),
                "?t=",
                HttpUtility.UrlEncode(File.GetLastWriteTimeUtc(HostingEnvironment.MapPath(virtualPath)).Ticks.ToString())
                )
            );
        
        
		response.Write("\" type=\"text/javascript\"></script>\r\n");
	}
	
    public bool IsReusable 
	{
        get 
		{
            return true;
        }
    }

}