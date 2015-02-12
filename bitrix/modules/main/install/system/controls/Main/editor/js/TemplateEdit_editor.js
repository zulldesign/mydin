var edit_hbf = [ //hbf - head, body, footer
	'BXButton',
	{
		id : 'edit_hbf',
		iconkit : '_global_iconkit.gif',
		name : TE_MESS.FILEMAN_EDIT_HBF,
		handler : function ()
		{
			this.bNotFocus = true;
			this.pMainObj.OpenEditorDialog("edit_hbf", false, 700, {bUseTabControl: true});
		}
	}
];

var insert_cph = [
	'BXButton',
	{
		id : 'insert_wa',
		iconkit : '_global_iconkit.gif',
		name : TE_MESS.INSERT_CPH,
		handler : function ()
		{
			this.bNotFocus = true;
			this.pMainObj.OpenEditorDialog("insert_cph", false, 500, {});
		}
	}
];

var preview_tmpl = [
	'BXButton',
	{
		id : 'preview_tmpl',
		iconkit : '_global_iconkit.gif',
		name : TE_MESS.FILEMAN_PREVIEW_TEMPLATE,
		title : TE_MESS.FILEMAN_PREVIEW_TEMPLATE_TITLE,
		handler : function () {preview_template(__ID);}
	}
];

delete arToolbars['template'];

var edit_template = oBXEditorUtils.createToolbar("edit_template", TE_MESS.templateToolbar, [edit_hbf, insert_cph]);
oBXEditorUtils.addToolbar(edit_template);

function TEContentParser(str, pMainObj)
{
	str = str.replace(/<asp:\s*contentplaceholder\s*id\s*=\s*("|')(\w*)\1[^>]*?(?:>\s*?<\/asp\:contentplaceholder)?(?:\/?)?>/ig,
	function(str, s1, val)
	{
		return "<IMG src='" + image_path + "/cph.gif' id=\"" + pMainObj.SetBxTag(false, {tag: "cph", params: {value: val}}) + "\"/>";
	});
	return str;
}
oBXEditorUtils.addContentParser(TEContentParser);

function TEUnparser(node)
{
	var oTag = node.pParser.pMainObj.GetBxTag(node.arAttributes['id']);
	if (oTag.tag == 'cph' && oTag.params && oTag.params.value)
		return '<asp:ContentPlaceHolder ID="' + (oTag.params.value  || 'BXContentPlaceHolder') + '" runat="server" />';

	return false;
}
oBXEditorUtils.addUnParser(TEUnparser);

pPropertybarHandlers['cph'] = function(bNew, pTaskbar, pElement)
{
	pTaskbar.pHtmlElement = pElement;
	
	if(bNew)
	{
		pTaskbar.arElements = [];
		var tProp;
		var arBarHandlersCache = pTaskbar.pMainObj.arBarHandlersCache;
		if(arBarHandlersCache['cph'])
		{
			tProp = arBarHandlersCache['cph'][0];
			pTaskbar.arElements = arBarHandlersCache['cph'][1];
		}
		else
		{
			tProp = pTaskbar.pMainObj.CreateElement("TABLE", {className: "bxtaskbarprops", cellSpacing: 0, cellPadding: 1}, {width: "100%"});
			var row = tProp.insertRow(-1);
			var cell = row.insertCell(-1); cell.align = 'right'; cell.width="40%";
			cell.appendChild(pTaskbar.pMainObj.CreateElement("SPAN", {innerHTML: TE_MESS.CPH_ID + ':&nbsp;'}));

			cell = row.insertCell(-1); cell.width="60%";
			pTaskbar.arElements.id = cell.appendChild(pTaskbar.pMainObj.CreateElement("INPUT", {size: '40', title: TE_MESS.CPH_ID,  type: 'text'}));
			arBarHandlersCache['cph'] = [tProp, pTaskbar.arElements];
		}
		pTaskbar.pCellProps.appendChild(tProp);
	}
	
	var bxTag = pTaskbar.pMainObj.GetBxTag(pElement);
	pTaskbar.arElements.id.value = bxTag.params.value;

	pTaskbar.arElements.id.onchange = function ()
	{
		bxTag.params.value = pTaskbar.arElements.id.value;
		pTaskbar.pMainObj.SetBxTag(false, bxTag);
	};
};


arEditorFastDialogs['insert_cph'] = function(pObj)
{
	return {
		title: TE_MESS.INSERT_CPH,
		innerHTML : '<div class="inner_content" style="font-size: 11px;">' + TE_MESS.CPH_ID + ': <input type="text" size="30" value="" id="bx_cph_id" style="width: 180px;" /></div>',
		OnLoad: function()
		{
			window.oBXEditorDialog.SetButtons([
				new BX.CWindowButton(
				{
					title: BX_MESS.TBSaveExit,
					action: function()
					{
						var id = BX("bx_cph_id").value;
						if (id.length <= 0)
							return;

						var elId = pObj.pMainObj.SetBxTag(false, {tag: "cph", params: {value: id}});
						pObj.pMainObj.insertHTML("<IMG src='" + image_path + "/cph.gif' id=\"" + elId + "\"/>");
						var pComponent = pObj.pMainObj.pEditorDocument.getElementById(elId);
						if(pObj.pMainObj.pEditorWindow.getSelection)
							pObj.pMainObj.pEditorWindow.getSelection().selectAllChildren(pComponent);
						window.oBXEditorDialog.Close();
					}
				}),
				window.oBXEditorDialog.btnCancel
			]);

			window.oBXEditorDialog.adjustSizeEx();
		}
	};
}