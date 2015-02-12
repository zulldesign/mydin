<%@ Page Language="C#" AutoEventWireup="false" CodeFile="StylesheetEditor.aspx.cs" Inherits="bitrix_dialogs_StylesheetEditor" %>

<html>
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server" class="stylesheet-editor-form">
        <bx:BXPageAsDialogBehaviour runat="server" ID="behaviour" Enabled ="true" 
            ButonSetLayout="SaveCancel"  OnSave="behaviour_Save" />     
        <div class="stylesheet-editor-container" id="stylesheet_editor_container">
            <asp:TextBox ID="textEditor" runat="server" TextMode="MultiLine" />
        </div>
    </form>
<% if(Behaviour.ClientType == BXPageAsDialogClientType.WindowManager9) {%>
<script type="text/javascript">
	var border = null, ta = null, wnd = BX.WindowManager.Get();
	function TAResize(data) {
		if (null == ta) ta = BX('textEditor');
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
