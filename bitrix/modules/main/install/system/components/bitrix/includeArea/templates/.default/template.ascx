<%@ Reference VirtualPath="~/bitrix/components/bitrix/includeArea/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="IncludeAreaComponentTemplate" %>
<script runat="server">
	protected override void OnLoad(EventArgs e)
	{
		Control content = GetContentControl();
		if (content != null)
			Controls.Add(content);
		base.OnLoad(e);
	}
</script>

