<%@ Page Language="C#" EnableEventValidation="false" EnableViewState="false" EnableViewStateMac="false" Inherits="Bitrix.UI.BXPage" ValidateRequest="False"%>
<script runat="server" type="text/C#">
	string FlashPath, FlashWidth, FlashHeight;
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		FlashPath = Bitrix.Services.Text.BXTextEncoder.HtmlTextEncoder.Encode(Request.QueryString["path"]);

		if (!Bitrix.IO.BXSecureIO.CheckRead(Bitrix.Components.BXUrlPath.ToVirtual(FlashPath)))
			Response.End();
		
		FlashWidth = Bitrix.Services.Text.BXTextEncoder.HtmlTextEncoder.Encode(Request.QueryString["width"]);
		FlashHeight = Bitrix.Services.Text.BXTextEncoder.HtmlTextEncoder.Encode(Request.QueryString["height"]);
		FlashWidth = (FlashWidth.Length > 0) ? "width=\"" + FlashWidth + "\"" : "";
		FlashHeight = (FlashHeight.Length > 0) ? "height=\"" + FlashHeight + "\"" : "";
	}
</script>

<HTML>
<HEAD></HEAD>
<BODY>
<embed
id="flash_preview"
pluginspage="http://www.macromedia.com/go/getflashplayer"
type="application/x-shockwave-flash"
name="preview_flash"
quality="high"
<%= FlashWidth %>
<%= FlashHeight %>
src="<%= FlashPath %>"
/>
</BODY>
</HTML>