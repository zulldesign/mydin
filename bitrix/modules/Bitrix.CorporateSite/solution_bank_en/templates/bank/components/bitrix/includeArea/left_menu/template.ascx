<%@ Reference VirtualPath="~/bitrix/components/bitrix/includeArea/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="IncludeAreaComponentTemplate" %>
<%@ Import Namespace="Bitrix.UI" %>
<script runat="server">
	protected override void OnLoad(EventArgs e)
	{
		Control content = GetContentControl();
		if (content != null)
		{
			Custom.Controls.Add(content);
			Controls.Remove(DefaultMenu);
		}
		
		base.OnLoad(e);
	}
</script>
<bx:IncludeComponent 
	ID="DefaultMenu" 
	runat="server" 
	componentname="bitrix:system.PublicMenu"
	template="vertical" 
	Depth="3" 
	MenuName="top" 
	SubMenuName="left" 
/>
<asp:PlaceHolder ID="Custom" runat="server" />
						