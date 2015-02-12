<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MenuEdit.aspx.cs" Inherits="bitrix_dialogs_MenuEdit"
	Title="<%$ Loc:TITLE %>" %>

<%@ Register Src="~/bitrix/controls/Main/DirectoryBrowser.ascx" TagName="DirectoryBrowser"
	TagPrefix="uc1" %>
<html>
<head runat="server">
	<title></title>
	<%--<script type="text/javascript" src='<%= VirtualPathUtility.ToAbsolute("~/bitrix/js/main/dd.js") %>'></script>--%>
</head>
<body>
	<form id="form1" runat="server">
		<bx:BXPageAsDialogBehaviour runat="server" ID="Behaviour" OnSave="Behaviour_Save" />
		<uc1:DirectoryBrowser ID="FileDialog" runat="server" OKClientScript="return MenuEdit_FileDialogOK()"
			CancelClientScript="return MenuEdit_FileDialogCancel()" ShowExtras="false" ExtensionsList=""
			AppendSlashToFolders="true" WindowTitle="<%$ Loc:DlgTitle.SelecPageOfFolder %>" />
<script type="text/jscript">	
	var currentLink = -1;
    var currentRow = null;

    var GLOBAL_bDisableActions = false;
    var GLOBAL_bDisableDD = false;

	window.setTimeout(function()
	{ 
		//alert('bring me to life');
		///debugger;
		var oNode = document.getElementById('<%= FileDialog.ContainerDialog.ClientID %>'); 
		oNode.parentNode.removeChild(oNode); 
		var oForm = document.body;//getElementById('<%= Form.ClientID + ClientIDSeparator + Form.ClientID %>'); 
		if (oForm.firstChild == null) 
			oForm.appendChild(oForm); 
		else 
			oForm.insertBefore(oNode, oForm.firstChild);
	}, 1);

    function MenuEdit_OnBeforeCloseDialog()
    {
	    //debugger;
	    try
	    {
		    jsUtils.removeCustomEvent('OnBeforeCloseDialog', MenuEdit_OnBeforeCloseDialog);
	    }
	    catch(err)
	    {
    		
	    }
	    <%= FileDialog.ContainerDialog.GetJSObjectName() %>.ClosePopupDialog();
	    var node = document.getElementById('<%= FileDialog.ContainerDialog.ClientID %>');
	    if (node)
		    node.parentNode.removeChild(node);
    }

    function MenuEdit_FileDialogOK()
    {
	    setLink(<%= FileDialog.JavaScriptObject %>.GetTargetValue());
	    <%= FileDialog.ContainerDialog.GetJSObjectName() %>.ClosePopupDialog();
	    return false;
    }
    function MenuEdit_FileDialogCancel()
    {
	    <%= FileDialog.ContainerDialog.GetJSObjectName() %>.ClosePopupDialog();
	    return false;
    }
		
		
    jsUtils.addCustomEvent('OnBeforeCloseDialog', MenuEdit_OnBeforeCloseDialog);
	

    function MenuEdit_OpenFileBrowserWindFile()
    {
	    //debugger;
	    var url = /*document.forms[jsPopup.form_name]*/theForm['link_' + currentLink].value;
	    <%= FileDialog.JavaScriptObject %>.Properties.DefaultPath = jsUtils.Path.IsFull(url) ? "~" : url;
	    <%= FileDialog.JavaScriptObject %>.Reset();
	    <%= FileDialog.ContainerDialog.GetJSObjectName() %>.ShowPopupDialog(false, true);
    }

    function setLink(path)
    {
	    //document.forms.component_settings['link_' + currentLink].value = (path == '' ? '/' : path + '/') + filename;
	    //document.forms[jsPopup.form_name]['link_' + currentLink].value = path;//(path == '' ? '/' : path + '/') + filename;
	    theForm['link_' + currentLink].value = path;
	    editArea('link_' + currentLink, true);
	    viewArea('link_' + currentLink, true);
    }

    var jsMenuMess = {
	    noname: '<%= GetMessageJS("JS_NONAME") %>'
    }
//findFirstChildNodeByTagName(document.getElementById('bx_menu_layout'), "DIV")

function menuCheckIcons()
{	
	var ary = BX.findChild(BX('bx_menu_layout'), { 'class': 'bx-menu-placement' }, true, true);
	for (var i = 0; i < ary.length; i++)
	{	
		var row = BX.findChild(BX.findChild(ary[i], { 'class': 'bx-edit-menu-item' }), { 'tag': 'table' }).tBodies[0].rows[0];
				
		findFirstChild(row.cells[4]).style.visibility = (i == 0 ? 'hidden' : 'visible');
		findFirstChild(row.cells[5]).style.visibility = (i == (ary.length - 1) ? 'hidden' : 'visible');
		findFirstChild(row.cells[0]).value = 10 * (i + 1);
	}
}

function menuMoveUp(i)
{
    //debugger;
	if (GLOBAL_bDisableActions)
		return;

	var obRow = document.getElementById('bx_menu_row_' + i);
	var obPlacement = obRow.parentNode;
	
	var index = obPlacement.id.substring(18);
	if (1 >= index)
		return;
	
	var obNewPlacement = findPreviousSibling(obPlacement);
	var obSwap = findFirstChild(obNewPlacement);
	
	obPlacement.removeChild(obRow);
	obNewPlacement.removeChild(obSwap);
	obPlacement.appendChild(obSwap);
	obNewPlacement.appendChild(obRow);
	
	setCurrentRow(obRow);
	menuCheckIcons();
}

function menuMoveDown(i)
{
	if (GLOBAL_bDisableActions)
		return;

	var obRow = document.getElementById('bx_menu_row_' + i);
	var obPlacement = obRow.parentNode;
	var obNewPlacement = findNextSibling(obPlacement);
	if (null == obNewPlacement)
		return;
	
	var obSwap = findFirstChild(obNewPlacement);
	
	obPlacement.removeChild(obRow);
	obNewPlacement.removeChild(obSwap);
	obPlacement.appendChild(obSwap);
	obNewPlacement.appendChild(obRow);
	
	setCurrentRow(obRow);
	menuCheckIcons();
}

function menuDelete(i)
{
	if (GLOBAL_bDisableActions)
		return;

	var obInput = document.getElementsByName('del_' + i)[0];
	var obPlacement = document.getElementById('bx_menu_row_' + i).parentNode;
	
	obInput.value = 'Y';

	if (findFirstChild(obPlacement) == currentRow) currentRow = null;
	
	obPlacement.parentNode.removeChild(obPlacement);
	menuCheckIcons();
}

function menuAdd()
{
	var obCounter = document.getElementsByName("itemcnt")[0];
	var nums = parseInt(obCounter.value);
	obCounter.value = ++nums;
	
	var obPlacement = document.createElement('DIV');
	obPlacement.className = 'bx-menu-placement';
	obPlacement.id = 'bx_menu_placement_' + nums;
	
	document.getElementById('bx_menu_layout').appendChild(obPlacement);
	
	
	var obRow = document.createElement('DIV');
	obRow.className = 'bx-edit-menu-item';
	obRow.id = 'bx_menu_row_' + nums;
	obPlacement.appendChild(obRow);
	
	var arCellsHTML = [
		'<span class="rowcontrol drag" title="<%= JSEncode(GetMessage("TOOLTIP_DRAG")) %>"></span>',
		getAreaHTML('text_' + nums, '', '<%= JSEncode(GetMessage("TOOLTIP_TEXT_EDIT")) %>'),
		getAreaHTML('link_' + nums, '', '<%= JSEncode(GetMessage("TOOLTIP_LINK_EDIT")) %>'),
		'<span onclick="if(!GLOBAL_bDisableActions) { currentLink = \'' + nums + '\'; MenuEdit_OpenFileBrowserWindFile();}" class="rowcontrol folder" title="<%= JSEncode(GetMessage("TOOLTIP_FD")) %>"></span>',
		'<span onclick="menuMoveUp(' + nums + ')" class="rowcontrol up" style="visibility: ' + (nums == 1 ? 'hidden' : 'visible') + '" title="<%= JSEncode(GetMessage("MENU_EDIT_TOOLTIP_UP")) %>"></span>',
		'<span onclick="menuMoveDown(' + nums + ')" class="rowcontrol down" style="visibility: hidden" title="<%= JSEncode(GetMessage("TOOLTIP_DOWN")) %>"></span>',
		'<span onclick="menuDelete(' + nums + ')" class="rowcontrol delete" title="<%= JSEncode(GetMessage("TOOLTIP_DELETE")) %>"></span>'
	];
	
	var row_content = '<table border="0" cellpadding="2" cellspacing="0" class="bx-width100 internal" class="menu-table"><tbody><tr>';
	
	for (var i = 0; i < arCellsHTML.length; i++)
	{
		//var obCell = obRow.insertCell(-1);
		//obCell.innerHTML = arCellsHTML[i];
		row_content += '<td>' + arCellsHTML[i] + '</td>';
	}
	
	row_content += '</tr></tbody></table>';
	
	obRow.innerHTML = row_content;

	var arInputs = [
		['ids[]', nums],
		['del_' + nums, 'N'],
		['sort_' + nums, 2 * nums * 10]
	];



	for (var i = 0; i<arInputs.length; i++)
	{
		var obInput = BX.create('INPUT', {
			props: {type: 'hidden', name: arInputs[i][0], value: arInputs[i][1]}
		});
		
		var obFirstCell = obRow.firstChild.tBodies[0].rows[0].cells[0];
		obFirstCell.insertBefore(obInput, obFirstCell.firstChild);
	}

	jsDD.registerDest(obPlacement);
	
	obRow.onbxdragstart = BXDD_DragStart;
	obRow.onbxdragstop = BXDD_DragStop;
	obRow.onbxdraghover = BXDD_DragHover;

	jsDD.registerObject(obRow);

	setCurrentRow(nums);
	menuCheckIcons();
}

function getAreaHTML(area, value, title)
{
	if (null == value) value = '';

	return '<div onmouseout="rowMouseOut(this)" onmouseover="rowMouseOver(this)" class="edit-field view-area" style="width: 220px; padding: 2px; display: block; border: 1px solid white; cursor: text; -moz-box-sizing: border-box; background-position: right center; background-repeat: no-repeat;" id="view_area_' + area + '" onclick="editArea(\'' + area + '\')" title="' + title + '">' + (value ? value : jsMenuMess.noname) + '</div>' +
			'<div class="edit-area" id="edit_area_' + area + '" style="display: none;"><input type="text" style="width: 220px;" name="' + area + '" value="' + value + '" onblur="viewArea(\'' + area + '\')" /></div>';
}

var currentEditingRow = null;

function editArea(area, bSilent)
{
	if (GLOBAL_bDisableActions)
		return;

	jsDD.Disable();
	GLOBAL_bDisableDD = true;

	jsDD.allowSelection();
	l = BX('bx_menu_layout');
	l.ondrag = l.onselectstart = null;
	l.style.MozUserSelect = '';

	if (null == bSilent) bSilent = false;

	var obEditArea = BX('edit_area_' + area);
	var obViewArea = BX('view_area_' + area);

	obEditArea.style.display = 'block';
	obViewArea.style.display = 'none';

	if (!bSilent)
	{
		obEditArea.firstChild.focus();

		if (BX.browser.IsIE())
			setTimeout(function () {setCurrentRow(obViewArea.parentNode.parentNode.parentNode.parentNode.parentNode)}, 30);
		else
			setCurrentRow(obViewArea.parentNode.parentNode.parentNode.parentNode.parentNode);
	}

	return obEditArea;
}


function viewArea(area, bSilent)
{
	if (GLOBAL_bDisableActions)
		return;

	jsDD.Enable();
	GLOBAL_bDisableDD = false;

	l = BX('bx_menu_layout');
	l.ondrag = l.onselectstart = BX.False;
	l.style.MozUserSelect = 'none';

	if (null == bSilent) bSilent = false;

	var obEditArea = BX('edit_area_' + area);
	var obViewArea = BX('view_area_' + area);

	obEditArea.firstChild.value = BX.util.trim(obEditArea.firstChild.value);

	obViewArea.innerHTML = '';
	BX.adjust(obViewArea, {text:obEditArea.firstChild.value.length > 0 ? obEditArea.firstChild.value : jsMenuMess.noname})

	obEditArea.style.display = 'none';
	obViewArea.style.display = 'block';

	currentEditingRow = null;
	setCurrentRow(obViewArea.parentNode.parentNode.parentNode.parentNode.parentNode);

	return obViewArea;
}

function setCurrentRow(i)
{
	i = BX(i);

	if (null != currentRow) BX.removeClass(currentRow, 'bx-menu-current-row')

	BX.addClass(i, 'bx-menu-current-row');
	currentRow = i;
}

function rowMouseOut (obArea)
{
	obArea.className = 'edit-field view-area';
	obArea.style.backgroundColor = '';
}

function rowMouseOver (obArea)
{
	if (GLOBAL_bDisableActions || jsDD.bPreStarted)
		return;

	obArea.className = 'edit-field-active view-area';
	obArea.style.backgroundColor = 'white';
}
</script>
<div class="bx-public-dialog-content">
    <input type="hidden" name="save" value="Y" />
	<table border="0" cellpadding="2" cellspacing="0" class="bx-width100 internal" id="menu_table">
	    <thead>
		    <tr class="heading">
			    <td width="0"></td>
			    <td width="50%"><b><%= GetMessage("NAME") %></b></td>
			    <td width="50%"><b><%= GetMessage("LINK") %></b></td>
			    <td width="0px"></td>
			    <td width="0px"></td>
			    <td width="0px"></td>
			    <td width="0px"></td>
		    </tr>
	    </thead>
    </table>
    <div id="bx_menu_layout" class="bx-menu-layout">
				<%int itemCount = 0;
				    for (int i = 1; i <= MenuItemCollection.Count; i++){
						itemCount++;
						Bitrix.BXPublicMenuItem menuItem = MenuItemCollection[i - 1];//	aMenuLinksItem = new string[] { null, null };//$aMenuLinksTmp[$i-1];%>
                <div class="bx-menu-placement" id="bx_menu_placement_<%= i %>">
                    <div class="bx-edit-menu-item" id="bx_menu_row_<%= i %>">
                        <table border="0" cellpadding="2" cellspacing="0" class="bx-width100 internal menu-table"><tbody>
                            <tr>
					            <td>
						            <input type="hidden" name="sort_<%= i %>" value="<%= i*10 %>" />
						            <input type="hidden" name="ids[]" value="<%= i %>" />
						            <input type="hidden" name="del_<%= i %>" value="N" />
						            <input type="hidden" name="additional_params_<%= i %>" value="<%= Encode(Serialize(menuItem)) %>" />
						            <span class="rowcontrol drag" title="<%= GetMessage("TOOLTIP_DRAG") %>"></span>
					            </td>
					            <td>
						            <div onmouseout="rowMouseOut(this)" onmouseover="rowMouseOver(this)" class="edit-field view-area" id="view_area_text_<%= i %>" style="width: 220px; padding: 2px; display: block; border: 1px solid white; cursor: text; -moz-box-sizing: border-box; background-position: right center; background-repeat: no-repeat;" onclick="editArea('text_<%= i %>')" title="<%= GetMessage("TOOLTIP_TEXT_EDIT") %>"><%= !string.IsNullOrEmpty(menuItem.Title) ? Encode(menuItem.Title) : GetMessage("JS_NONAME") %></div>
						            <div class="edit-area" id="edit_area_text_<%= i %>" style="display: none;"><input type="text" style="width: 220px;" name="text_<%= i %>" value="<%= Encode(menuItem.Title) %>" onblur="viewArea('text_<%= i %>')" /></div>
					            </td>
					            <td>
						            <div onmouseout="rowMouseOut(this)" onmouseover="rowMouseOver(this)" class="edit-field view-area" id="view_area_link_<%= i %>" style="width: 220px; padding: 2px; display: block; border: 1px solid white; cursor: text; -moz-box-sizing: border-box; background-position: right center; background-repeat: no-repeat;" onclick="editArea('link_<%= i %>')" title="<%= GetMessage("TOOLTIP_LINK_EDIT") %>"><%= !string.IsNullOrEmpty(menuItem.Link) ? Encode(menuItem.Link) : GetMessage("JS_NONAME")%></div>
						            <div class="edit-area" id="edit_area_link_<%= i %>" style="display: none;"><input type="text" style="width: 220px;" name="link_<%= i %>" value="<%= Encode(menuItem.Link) %>" onblur="viewArea('link_<%= i %>')" /></div>
					            </td>
					            <td>
						            <span onclick="if (!GLOBAL_bDisableActions){currentLink = '<%= i %>'; MenuEdit_OpenFileBrowserWindFile();}" class="rowcontrol folder" title="<%= GetMessage("TOOLTIP_FD") %>"></span>
					            </td>
					            <td>
						            <span onclick="menuMoveUp(<%= i %>)" class="rowcontrol up" style="visibility: <%= (i == 1 ? "hidden" : "visible") %>" title="<%= GetMessage("TOOLTIP_UP") %>"></span>
					            </td>
					            <td>
						            <span onclick="menuMoveDown(<%= i %>)" class="rowcontrol down" style="visibility: <%= (i == MenuItemCollection.Count ? "hidden" : "visible") %>" title="<%= GetMessage("TOOLTIP_DOWN") %>"></span>
					            </td>
					            <td>
						            <span onclick="menuDelete(<%= i %>)" class="rowcontrol delete" title="<%= GetMessage("TOOLTIP_DELETE") %>"></span>
					            </td>
				            </tr>                            	
                            </tbody></table>
                    </div>
                </div>	                
                <%}%>
    </div>				
			<%bool onlyEdit = false;
				if (!onlyEdit){%><input type="button" onclick="menuAdd()" value="<%= GetMessage("ADD_ITEM") %>" /><%}%>
			<input type="hidden" name="itemcnt" value="<%= itemCount %>" />
</div>
<script type="text/javascript">
function BXDD_DragStart()
{
	if (GLOBAL_bDisableDD)
		return false;
	
	//this.className = 'bx-edit-menu-item bx-edit-menu-drag';
	this.BXOldPlacement = this.parentNode;

	var id = this.id.substring(12);
	rowMouseOut(viewArea('link_' + id));
	rowMouseOut(viewArea('text_' + id));
	
	GLOBAL_bDisableActions = true;
	
	return true;
}

function BXDD_DragStop()
{
	//this.className = 'bx-edit-menu-item';
	this.BXOldPlacement = false;
	
	setTimeout('GLOBAL_bDisableActions = false', 50);
	
	return true;
}

function BXDD_DragHover(obPlacement, x, y)
{
	if (GLOBAL_bDisableDD)
		return false;

	if (obPlacement == this.BXOldPlacement)
		return false;

	var obSwap = findFirstChild(obPlacement);

	this.BXOldPlacement.removeChild(this);
	obPlacement.removeChild(obSwap);
	this.BXOldPlacement.appendChild(obSwap);
	obPlacement.appendChild(this);

	this.BXOldPlacement = obPlacement;

	menuCheckIcons();

	return true;
}

BX.ready(function() {
	try {
		jsDD.Reset();
		jsDD.registerContainer(BX.WindowManager.Get().GetContent());
	}
	catch (e) { }
	l = BX('bx_menu_layout');
	l.ondrag = l.onselectstart = BX.False;
	l.style.MozUserSelect = 'none';

	var ary = BX.findChild(l, { 'class': 'bx-menu-placement' }, true, true);
	if (ary) for (var i = 0; i < ary.length; i++) {
		jsDD.registerDest(ary[i]);
		var item = BX.findChild(ary[i], { 'class': 'bx-edit-menu-item' });
		item.onbxdragstart = BXDD_DragStart;
		item.onbxdragstop = BXDD_DragStop;
		item.onbxdraghover = BXDD_DragHover;
		jsDD.registerObject(item);
	}
});
</script>
</form>
</body>
</html>
