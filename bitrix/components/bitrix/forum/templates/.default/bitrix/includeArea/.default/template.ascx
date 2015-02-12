<%@ Reference VirtualPath="~/bitrix/components/bitrix/includeArea/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="IncludeAreaComponentTemplate" %>
<script runat="server">
	protected override void OnLoad(EventArgs e)
	{
		Control content = GetContentControl();
		if (content != null)
			Content.Controls.Add(content);
		base.OnLoad(e);
	}
</script>
<div class="forum-content">

<div class="forum-header-box"> 
<div class="forum-header-title"><span><%= Encode(Parameters.GetString("Title", GetMessageRaw("Kernel.Information"))) %></span></div> 
</div> 
<div class="forum-info-box forum-rules"> 
	<div class="forum-info-box-inner"><asp:PlaceHolder ID="Content" runat="server" /></div> 
</div>

</div>