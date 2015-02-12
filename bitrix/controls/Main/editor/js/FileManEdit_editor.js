window.BXOnSaveAs = function(bDialog, bApply, editor)
{
	var Save = function(obj)
	{
		var but = BX(bApply ? window.apply_but_id : window.save_but_id);
		but.parentNode.appendChild(BX.create("INPUT", {props: {type: 'hidden', name: but.name, value: but.value}}));
		
		try
		{
			if (obj && obj.onSubmit != null)
				obj.onSubmit();
			return but.form.submit();
		}
		catch(e)
		{
			return false;
		}
		return true;
	};

	if (!bDialog)
		return Save(editor);

	var
		JSName = window[window._fileDialog.JSName],
		ExtraJSName = window[window._fileDialog.ExtraJSName],
		pSaveAs = BX(BX_DOTNET_ID.SaveAs),
		_def_path = _curDir + '\\' + pSaveAs.value;

	ExtraJSName.Properties = {ShowFiles: true, ItemsToSelect: 2, ExtensionsList: '' /* 'aspx,html'*/, DefaultPath: _def_path};
	ExtraJSName.SaveEvent = function()
	{
		var
			pSaveAsPath = BX(BX_DOTNET_ID.SaveAsPath),
			path = ExtraJSName.GetTargetValue(),
			_path = path.replace(/\\/g, '/'),
			ind = _path.lastIndexOf('/'),
			filename = _path.substr(ind + 1),
			form = BX('aspnetForm');

		form.action = form.action.replace(/path=[^&]*/, "path="+jsUtils.urlencode(path));
		pSaveAs.value = filename;
		pSaveAsPath.value = path;
		Save(editor);
	};

	JSName.ShowPopupDialog();
	ExtraJSName.Reset();
	ExtraJSName.CollapseToRoot();
}


arButtons['new']	= ['BXButton',
	{
		id : 'new',
		iconkit : '_global_iconkit.gif',
		codeEditorMode : true,
		name : BX_MESS.TBNewPage,
		hideCondition: function(){return !window.bx_new_file_command_path;},
		handler : function ()
		{
			if (window.bx_new_file_command_path.length <= 0)
				return;
			try
			{
				window.location.href = APPPath + '/bitrix/admin/' + window.bx_new_file_command_path;
			}
			catch(e)
			{
			}
		}
	}
];

arButtons['save_and_exit'] = ['BXButton',
	{
		id : 'save_and_exit',
		iconkit : '_global_iconkit.gif',
		codeEditorMode : true,
		name : BX_MESS.TBSaveExit,
		title : BX_MESS.TBSaveExit,
		show_name : true,
		handler : function (editor){BXOnSaveAs(!_bEdit, false, editor);}
	}
];

arButtons['exit'] = ['BXButton',
	{
		id : 'exit',
		iconkit : '_global_iconkit.gif',
		codeEditorMode : true,
		name : BX_MESS.TBExit,
		handler : function ()
		{
			this.pMainObj.OnEvent("OnSelectionChange");
			var need_to_ask = (pBXEventDispatcher.arEditors[0].IsChanged() && !pBXEventDispatcher.arEditors[0].isSubmited);

			if(need_to_ask)
			{
				this.bNotFocus = true;
				this.pMainObj.OpenEditorDialog("asksave", false, 600, {window: window, savetype: _bEdit ? 'save' : 'saveas'}, true);
			}
			else
			{
				// Emulate cancel button click
				var but = BX(window.cancel_but_id);
				but.parentNode.appendChild(BX.create("INPUT", {props: {type: 'hidden', name: but.name, value: but.value}}));
				try
				{
					but.form.submit();
				}
				catch(e)
				{
				}
			}
		}
	}
];

arButtons['saveas'] = ['BXButton',
	{
		id : 'saveas',
		iconkit : '_global_iconkit.gif',
		codeEditorMode : true,
		name : BX_MESS.TBSaveAs,
		handler : function (){BXOnSaveAs(true, true);}
	}
];


arButtons['save'] = ['BXButton',
	{
		id : 'save',
		iconkit : '_global_iconkit.gif',
		codeEditorMode : true,
		name : BX_MESS.TBSave,
		handler : function (){BXOnSaveAs(!_bEdit, true);}
	}
];

arButtons['pageprops'] = ['BXButton',
	{
		id : 'pageprops',
		iconkit : '_global_iconkit.gif',
		codeEditorMode : true,
		name : BX_MESS.TBProps,
		handler : function ()
		{
			this.bNotFocus = true;
			this.pMainObj.OpenEditorDialog("pageprops", false, 600, {window: window, document: document});
		}
	}
];

arToolbars['manage'] = [FE_MESS.FILEMAN_HTMLED_MANAGE_TB, [arButtons['save_and_exit'], arButtons['exit'], arButtons['new'], arButtons['save'], arButtons['saveas'], arButtons['pageprops']]];

arDefaultTBPositions['manage'] = [0, 0, 2];

window.onbeforeunload = function(e)
{
	var pMainObj = pBXEventDispatcher.arEditors[0];
	if (pMainObj.IsChanged() && !pMainObj.isSubmited)
		return BX_MESS.ExitConfirm;
}

// function InitializeCPHToolbar(pMainObj)
// {
	// if (!pMainObj)
		// throw 'System level exception. pMainObj == null';
	// if (!pMainObj.cphID)
		// throw 'this.pMainObj.cphID can`t have value not presented in contents list.';
	// if (!pMainObj.arCPH)
		// throw 'Toolbar should not be initialized when no content areas exists.';

	// var cphtblist = oBXEditorUtils.createToolbar("ContentPlaceHolderTB", BX_MESS.ASPXP_TB_CPHAREAS, [], {show: true, docked: true, position: [0, 0, 2]});

	// oBXEditorUtils.addToolbar(cphtblist);

	// var ContentPlaceHolderList = ['BXEdList',
	// {
		// width: 160,
		// field_size: 100,
		// title: '(template)',
		// bSetGlobalStyles: true,
		// disableOnCodeView: false,
		// values: pMainObj.arCPH,
		// OnCreate: function()
		// {
			// this.pMainObj.pCPHListbox = this;
			// this.refreshList = function()
			// {
				// var arSC = this.pMainObj.pASPXParser.arShadowedControls;
				// var cph = this.pMainObj.arTemplateParams.CPH;
				// var _cph = {};
				// // Refresh CPH list
				// this.pMainObj.arCPH = [];
				// var id, ocphid;
				// for (var i = 0, l = cph.length; i < l; i++)
				// {
					// ocphid = cph[i];
					// id = cph[i].toLowerCase();
					// _cph[id] = true;
					// this.pMainObj.arCPH.push({name:cph[i], value: id});
					// if (arSC[id])
						// continue;
					// arSC[id] = { // Add new CPH to arShadowedControls
						// id: id,
						// ocphid: ocphid,
						// attributes: {ID: id, runat: 'server'},
						// prefix: 'asp',
						// control: 'ContentPlaceHolder',
						// close: '>',
						// tagend: ['</asp:ContentPlaceHolder>']
					// };
				// }
				// this.SetValues(this.pMainObj.arCPH);

				// // Find first nonempty CPH and select it
				// for (var id in arSC)
				// {
					// if (arSC[id].contentVal && _cph[id] && arSC[id].contentVal.length > 0)
					// {
						// this.SelectByVal(id);
						// this.OnChange({value:id});
						// break;
					// }
				// }
			// };
			// this.pMainObj.AddEventHandler("OnTemplateChanged", this.refreshList, this);
		// },
		// OnInit: function()
		// {

			// this.SelectByVal(this.pMainObj.cphID);
			// var arSC = this.pMainObj.pASPXParser.arShadowedControls;
			// var cph = this.pMainObj.arTemplateParams.CPH;
			// var _cph = {};
			// for (var i = 0, l = cph.length; i < l; i++)
				// _cph[cph[i].toLowerCase()] = true;

			// var selId = false;

			// for (var id in arSC)
			// {
				// if (id.toLowerCase() == this.pMainObj.arConfig.defaultCPH.toLowerCase())
				// {
					// selId = id;
					// break;
				// }
			// }

			// if (!selId)
				// for (var id in arSC)
				// {
					// if (!selId) selId = id;
					// if (arSC[id].contentVal && _cph[id] && arSC[id].contentVal.length > 0)
					// {
						// selId = id;
						// break;
					// }
				// }

			// this.SelectByVal(selId);
			// this.pMainObj.pASPXParser.SetCurrentCPH(selId, true);
		// },
		// OnChange: function(selected)
		// {
			// this.pMainObj.pASPXParser.SetCurrentCPH(selected.value);
		// }
	// }];

	// oBXEditorUtils.appendButton('ContentPlaceHolderList', ContentPlaceHolderList, 'ContentPlaceHolderTB');
// }

// function RefreshValuesCPHToolbar(pMainObj)
// {
	// if (pMainObj && pMainObj.pCPHListbox && pMainObj.arCPH)
		// pMainObj.pCPHListbox.SetValues(pMainObj.arCPH);
// }


arEditorFastDialogs['asksave'] = function(pObj)
{
	return {
		title: BX_MESS.EDITOR,
		innerHTML : "<div style='padding: 5px;'>" + BX_MESS.DIALOG_EXIT_ACHTUNG + "</div>",
		OnLoad: function()
		{
			window.oBXEditorDialog.SetButtons([
				new BX.CWindowButton(
				{
					title: BX_MESS.TBSaveExit,
					action: function()
					{
						pObj.pMainObj.isSubmited = true;
						pObj.params.window.BXOnSaveAs((pObj.params.savetype == 'saveas'), false);
						window.oBXEditorDialog.Close();
					}
				}),
				new BX.CWindowButton(
				{
					title: BX_MESS.DIALOG_EXIT_BUT,
					action: function()
					{
						pObj.pMainObj.isSubmited = true;
						// Emulate cancel button click
						var but = BX(window.cancel_but_id);
						but.parentNode.appendChild(BX.create("INPUT", {props: {type: 'hidden', name: but.name, value: but.value}}));
						try
						{
							but.form.submit();
						}
						catch(e)
						{
						}
				

						window.oBXEditorDialog.Close();
					}
				}),
				window.oBXEditorDialog.btnCancel
			]);

			BX.addClass(window.oBXEditorDialog.PARTS.CONTENT, "bxed-dialog");
			window.oBXEditorDialog.adjustSizeEx();
		}
	};
}

arEditorFastDialogs['pageprops'] = function(pObj)
{
	var pSettingsTab = BX(BX_DOTNET_ID.settingsTab);
	var cn = pSettingsTab.childNodes, pSettingsTable, i, l = cn.length;
	for (i = 0; i < l; i++)
	{
		if (cn[i].nodeType != 1)
			continue;
		if (cn[i].nodeName.toUpperCase() == 'TABLE' && cn[i].className.toUpperCase().indexOf('EDIT-TABLE') != -1)
		{
			pSettingsTable = cn[i];
			break;
		}
	}
	if (!pSettingsTable)
		return false;

	var
		arInputs = pSettingsTable.getElementsByTagName('INPUT'),
		oInputs = [], id,
		innerHTML = pSettingsTab.innerHTML;

	innerHTML = innerHTML.replace(/\s*<table[\S|\s]*?<\/table>\s*/, '');
	for (i = 0, l = arInputs.length; i < l; i++)
	{
		id = BX_DOTNET_ID.settingsTab + "_KeywordsTable_k" + i + "_Value";
		innerHTML = innerHTML.replace(id, "bx_editor_pageprops_" + i);
		oInputs.push({input: arInputs[i]});
	}
	arInputs = null;

	return {
		title: BX_MESS.TBProps,
		innerHTML : innerHTML,
		OnLoad: function()
		{
			window.oBXEditorDialog.SetButtons([
				new BX.CWindowButton(
				{
					title: BX_MESS.TBSave,
					action: function()
					{
						for (var i = 0, l = oInputs.length; i < l; i++)
							oInputs[i].input.value = oInputs[i].new_input.value;
						window.oBXEditorDialog.Close();
					}
				}),
				window.oBXEditorDialog.btnCancel
			]);
			BX.addClass(window.oBXEditorDialog.PARTS.CONTENT, "bxed-dialog-2");

			window.oBXEditorDialog.adjustSizeEx();
			for (var i = 0, l = oInputs.length; i < l; i++)
			{
				oInputs[i].new_input = BX("bx_editor_pageprops_" + i);
				oInputs[i].new_input.value = oInputs[i].input.value;
			}
		}
	};
}