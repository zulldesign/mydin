<%@ Page Language="C#" AutoEventWireup="false" CodeFile="DebugMenu.aspx.cs" Inherits="bitrix_dialogs_DebugMenu" Title="<%$ Loc:Title %>" %>
<html>
<head runat="server">
</head>
<body>
    <form id="form1" class="debugmenu-form" runat="server">
    <bx:BXPageAsDialogBehaviour runat="server" ID="Behaviour" />
    <div id="debugmenu_container" class="debugmenu-container" runat="server">
        <textarea name="debugMenuContent" id="debugMenuContent" readonly="readonly" runat="server" style="border:solid 1px #CCCCCC; width:100%; padding:0px; margin:0px; height:99%; overflow-y:auto; wrap:soft; display:block;"></textarea>
    </div>
    </form>
<% if(Behaviour.ClientType == BXPageAsDialogClientType.WindowManager9) {%>
<script type="text/javascript">
	var border = null, ta = null, wnd = BX.WindowManager.Get();
	function TAResize(data) {
		if (null == ta) ta = BX('debugMenuContent');
		if (null == border) border = parseInt(BX.style(ta, 'border-left-width')) + parseInt(BX.style(ta, 'border-right-width'));
		if (isNaN(border)) border = 0;

		var add = BX('bx_additional_params');

		if (data.height) ta.style.height = (data.height - border - wnd.PARTS.HEAD.offsetHeight - (add ? add.offsetHeight : 0) - 35) + 'px';
		if (data.width) ta.style.width = (data.width - border - 10) + 'px';
	}
	BX.addCustomEvent(wnd, 'onWindowResizeExt', TAResize);
	TAResize(wnd.GetInnerPos());
</script>
<%} %>
</body> 
</html>
