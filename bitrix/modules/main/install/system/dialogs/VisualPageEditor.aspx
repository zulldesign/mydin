<%@ Page Language="C#" AutoEventWireup="true" CodeFile="VisualPageEditor.aspx.cs" Inherits="bitrix_dialogs_VisualPageEditor" 
EnableViewState="false" ValidateRequest="false"%>

<html>
<head runat="server">
    <title></title> 
</head>
<body>
    <form id="form1" runat="server">
        <bx:BXPageAsDialogBehaviour runat="server" ID="Behaviour" OnSave="Behaviour_Save" />
        <asp:MultiView runat="server" ID="ViewContainer">
            <asp:View runat="server" ID="VisualEditorView">
                <bx:BXWebEditor ID="VisualEditor" runat="server"
                    RunningUnderModalDialog="true" EnableChildDialogsSelfdestrurctionOnModalDialogClose="true"
                    AutoLoadContent="true" ContentType="Text" FullScreen="False" LimitCodeAccess="false" 
                    OnIncludeWebEditorScript="VisualEditor_IncludeWebEditorScript" 
                    StartMode="HTMLVisual" StartModeSelector="False" TemplateId="" 
                    UseHTMLEditor="true" UseOnlyDefinedStyles="False" WithoutCode="false" >
                    <Taskbars>
			            <bx:BXWebEditorBar Name="BXPropertiesTaskbar" />
				        <bx:BXWebEditorBar Name="ASPXComponentsTaskbar" />
                    </Taskbars>
			        <Toolbars>
			            <bx:BXWebEditorBar Name="manage" />
				        <bx:BXWebEditorBar Name="standart" />
				        <bx:BXWebEditorBar Name="formating" />
				        <bx:BXWebEditorBar Name="source" />
				        <bx:BXWebEditorBar Name="table" />
                        <bx:BXWebEditorBar Name="template" />
				        <bx:BXWebEditorBar Name="style" />
                    </Toolbars>
                </bx:BXWebEditor>               
            </asp:View>
            <asp:View runat="server" ID="TextEditorView">
				<asp:TextBox runat="server" ID="TextEditor" TextMode="MultiLine" />
            </asp:View>
        </asp:MultiView>     
    </form>

<%--
    <script type="text/javascript">         
        window.BXVisualPageEditorBefofeCloseDialog = function()
        {
            if(typeof(window["jsUtils"]) == "undefined") throw "Could not find jsUtils!";
            jsUtils.removeCustomEvent("OnBeforeCloseDialog", BXVisualPageEditorBefofeCloseDialog);
            window.setTimeout(function(){ <%= string.Format("jsUtils.Redirect(arguments, \"{0}\");", ClientPath) %>}, 1000);
        }         
        if(typeof(window["jsUtils"]) != "undefined"){
            jsUtils.addCustomEvent('OnBeforeCloseDialog', BXVisualPageEditorBefofeCloseDialog, [], window);
        }    
    </script>
        <script type="text/javascript">
        BXVisualPageEditorDialogManager.getInstance().start("<%= VisualEditor.ClientID %>");
    </script>
--%>

<% if(Behaviour.ClientType == BXPageAsDialogClientType.WindowManager9 && EditorModeMode == VisualPageEditorMode.PlainText) {%>
<script type="text/javascript" bxrunfirst="true">
	var border = null, ta = null, wnd = BX.WindowManager.Get();
	function TAResize(data) {
		if (null == ta) ta = BX('<%= TextEditor.ClientID %>');
		if (null == border) border = parseInt(BX.style(ta, 'border-left-width')) + parseInt(BX.style(ta, 'border-right-width'));
		if (isNaN(border)) border = 0;

		var add = BX('bx_additional_params');

		if (data.height) ta.style.height = (data.height - border - wnd.PARTS.HEAD.offsetHeight - (add ? add.offsetHeight : 0) - 35) + 'px';
		if (data.width) ta.style.width = (data.width - border - 10) + 'px';

		//console.info("width: %s, height: %s", data.width, data.height);
	}
	BX.addCustomEvent(wnd, 'onWindowResizeExt', TAResize);
	TAResize(wnd.GetInnerPos());
</script>
<%} %>
</body>
</html>
