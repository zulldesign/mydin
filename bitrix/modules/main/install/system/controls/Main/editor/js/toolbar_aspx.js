function InitializeCPHToolbar(pMainObj)
{
	if (!pMainObj)
		throw 'System level exception. pMainObj == null';
	if (!pMainObj.cphID)
		throw 'this.pMainObj.cphID can`t have value not presented in contents list.';
	if (!pMainObj.arCPH)
		throw 'Toolbar should not be initialized when no content areas exists.';

	if (!lightMode)
		oBXEditorUtils.addToolbar(oBXEditorUtils.createToolbar("ContentPlaceHolderTB", BX_MESS.ASPXP_TB_CPHAREAS, [], {show: true, docked: true, position: [0, 0, 2]}));

	var ContentPlaceHolderList = ['BXEdList',
	{
		isCPHList:true,
		width: 160,
		field_size: 100,
		title: '(template)',
		bSetGlobalStyles: true,
		disableOnCodeView: false,
		values: pMainObj.arCPH,
		OnCreate: function()
		{
			this.pMainObj.pCPHListbox = this;
			this.refreshList = function()
			{
				var arSC = this.pMainObj.pASPXParser.arShadowedControls;
				var cph = this.pMainObj.arTemplateParams.CPH || [];
				var _cph = {};
				// Refresh CPH list
				this.pMainObj.arCPH = [];
				var id, ocphid;
				for (var i = 0, l = cph.length; i < l; i++)
				{
					ocphid = cph[i];
					id = cph[i].toLowerCase();
					_cph[id] = true;
					this.pMainObj.arCPH.push({name:cph[i], value: id});
					if (arSC[id])
						continue;
					arSC[id] = { // Add new CPH to arShadowedControls
						id: id,
						ocphid: ocphid,
						attributes: {ID: id, runat: 'server'},
						prefix: 'asp',
						control: 'ContentPlaceHolder',
						close: '>',
						tagend: ['</asp:ContentPlaceHolder>']
					};
				}
				this.SetValues(this.pMainObj.arCPH);

				// Find first nonempty CPH and select it
				for (var id in arSC)
				{
					if (arSC[id].contentVal && _cph[id] && arSC[id].contentVal.length > 0)
					{
						this.SelectByVal(id);
						this.OnChange({value:id});
						break;
					}
				}
			};
			this.pMainObj.AddEventHandler("OnTemplateChanged", this.refreshList, this);
		},
		OnInit: function()
		{

			this.SelectByVal(this.pMainObj.cphID);
			var arSC = this.pMainObj.pASPXParser.arShadowedControls;
			var cph = this.pMainObj.arTemplateParams.CPH || [];
			var _cph = {};
			for (var i = 0, l = cph.length; i < l; i++)
				_cph[cph[i].toLowerCase()] = true;

			var selId = false;

			for (var id in arSC)
			{
				if (id.toLowerCase() == this.pMainObj.arConfig.defaultCPH.toLowerCase())
				{
					selId = id;
					break;
				}
			}

			if (!selId)
				for (var id in arSC)
				{
					if (!selId) selId = id;
					if (arSC[id].contentVal && _cph[id] && arSC[id].contentVal.length > 0)
					{
						selId = id;
						break;
					}
				}

			this.SelectByVal(selId);
			this.pMainObj.pASPXParser.SetCurrentCPH(selId, true);
		},
		OnChange: function(selected)
		{
			this.pMainObj.pASPXParser.SetCurrentCPH(selected.value);
		}
	}];

	if (lightMode)
	{
		//remove old toolbar
		for (var i = window.arGlobalToolbar.length - 1; i >= 0; i--)
		{
			var t = window.arGlobalToolbar[i];
			if (t[0] == "BXEdList" && t[1] && t[1].isCPHList)
				window.arGlobalToolbar.splice(i, 1);
		}
		
		window.arGlobalToolbar.push(ContentPlaceHolderList);
	}
	else
		oBXEditorUtils.appendButton('ContentPlaceHolderList', ContentPlaceHolderList, 'ContentPlaceHolderTB');
}

function RefreshValuesCPHToolbar(pMainObj)
{
	if (pMainObj && pMainObj.pCPHListbox && pMainObj.arCPH)
		pMainObj.pCPHListbox.SetValues(pMainObj.arCPH);
}