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
						window.setTimeout(function(){BXVisualPageEditorDialogManager.getInstance().save();}, 100);
						window.oBXEditorDialog.Close();
					}
				}),
				new BX.CWindowButton(
				{
					title: BX_MESS.DIALOG_EXIT_BUT,
					action: function()
					{
						if(typeof(jsPopup) == "undefined")
							throw "Could not find jsPopup!";
						jsPopup.AllowClose();

						window.setTimeout(function(){BXVisualPageEditorDialogManager.getInstance().exitWithDisclaimingChanges();}, 100);
						window.oBXEditorDialog.Close();
					}
				}),
				new BX.CWindowButton(
				{
					title: BX_MESS.DIALOG_EDIT_BUT,
					action: function()
					{
						window.oBXEditorDialog.Close();
					}
				})
			]);

			BX.addClass(window.oBXEditorDialog.PARTS.CONTENT, "bxed-dialog");
			window.oBXEditorDialog.adjustSizeEx();
		}
	};
}

//---BXVisualPageEditorDialogManager--//
BXVisualPageEditorDialogManager = function()
{
    this._initialized = false;
    this._chargeID = null;
    this._charge = null;
    this._redirectAfterExit = false;
    this._redirectionUrl = null;
}

BXVisualPageEditorDialogManager.prototype =
{
	initialize: function()
	{
		if(this._initialized)
			throw "Already initialized!";
		this._initialized = true;
	},

	start: function(chargeName)
	{
		this._chargeID  = chargeName;
		jsUtils.addCustomEvent("EditorLoadFinish_" + this._chargeID, BXVisualPageEditorDialogManager.onEditorLoad);
	},

	save: function()
	{
		if (typeof (jsPopup) == "undefined")
			throw "Could not find jsPopup!";
		jsPopup.AllowClose();

		this._ensureChargeIsAssigned();
		this.set_charge(null);
		if(typeof(window["BX_DOTNET_WEB_EDITOR_CONTEXT"]) == "undefined")
			throw "Could not find editor context";

		var buttonId = BX_DOTNET_WEB_EDITOR_CONTEXT.ButtonIdSave;
		var button = typeof(buttonId) == "string" ? BX(buttonId) : null;

		if(!button)
			throw "Could not find save button";

		button.onclick();
	},

	exitWithDisclaimingChanges: function()
	{
		this._charge.oPublicDialog.Close(true);
		this.set_charge(null);
		window.setTimeout(function(){BXVisualPageEditorDialogManager.getInstance().redirectIfNeed();}, 500);
	},

	get_charge: function()
	{
	        return this._charge;
	},

	set_charge: function(pMainObj)
	{
		if(typeof(jsUtils) == "undefined")
			throw "Could not find jsUtils!";
		this._charge = pMainObj;
	},

	_ensureChargeIsAssigned: function()
	{
		if(this._charge == null)
			throw "BXVisualPageEditorDialogManager: charge is not assigned!";
	},

	get_redirectAfterExit: function()
	{
		return this._redirectAfterExit;
	},

	set_redirectAfterExit: function(flag)
	{
		this._redirectAfterExit = flag;
	},

	get_redirectionUrl: function()
	{
		return this._redirectionUrl;
	},

	set_redirectionUrl: function(url)
	{
		this._redirectionUrl = url;
	},

	redirectIfNeed: function()
	{
		if(this._redirectAfterExit && this._redirectionUrl && typeof(this._redirectionUrl) == "string")
		{
			if(typeof(window["jsUtils"]) == "undefined")
				throw "Could not find jsUtils!";
			jsUtils.Redirect(arguments, this._redirectionUrl);
		}
	}
}

BXVisualPageEditorDialogManager._instance = null;
BXVisualPageEditorDialogManager.getInstance = function(){
	if(this._instance == null){
        this._instance = new BXVisualPageEditorDialogManager();
        this._instance.initialize();
	}
    return this._instance;
}

BXVisualPageEditorDialogManager.onEditorLoad = function()
{
	var self = BXVisualPageEditorDialogManager.getInstance();
	var pMainObj = GLOBAL_pMainObj[self._chargeID];
	pMainObj.oPublicDialog = BX.WindowManager.Get();
	BX.addClass(pMainObj.oPublicDialog.PARTS.CONTENT, "bx-editor-dialog-cont");
	pMainObj.oPublicDialog.AllowClose();

	if (BX.browser.IsIE())
	{
		pMainObj.pWnd.firstChild.rows[0].style.height = '1px';
		var sftbl;
		if (sftbl = BX.findChild(pMainObj.oPublicDialog.PARTS.CONTENT, {tagName: "TABLE"}))
		{
			sftbl.cellSpacing = 0;
			sftbl.cellPadding = 0;
		}
	}

	var onWinResizeExt = function(Params)
	{
		var
			h = parseInt(Params.height) - 2,
			w = parseInt(Params.width) - 10;

		pMainObj.pWnd.style.height = h + "px";
		pMainObj.pWnd.style.width = w + "px";

		BX.findParent(pMainObj.cEditor, {tagName: "TABLE"}).style.height = (h - 86) + "px";
		pMainObj.arTaskbarSet[2]._SetTmpClass(true);
		pMainObj.arTaskbarSet[2].Resize(false, false, false);
		pMainObj.arTaskbarSet[3].Resize(false, false, false);

		if (window._SetTmpClassInterval)
			clearInterval(window._SetTmpClassInterval);
		window._SetTmpClassInterval = setTimeout(function()
		{
			pMainObj.arTaskbarSet[2]._SetTmpClass(false);
			pMainObj.SetCursorFF();
		}, 300);
	}

	onWinResizeExt(pMainObj.oPublicDialog.GetInnerPos());
	BX.addCustomEvent(pMainObj.oPublicDialog, 'onWindowResizeExt', onWinResizeExt);
	BX.addCustomEvent(pMainObj.oPublicDialog, 'onBeforeWindowClose', function()
	{
		// We need to ask user
		if (pMainObj.IsChanged() && !pMainObj.isSubmited)
		{
			pMainObj.oPublicDialog.DenyClose();
			pMainObj.OpenEditorDialog("asksave", false, 600, {window: window, savetype: 'save', popupMode: true}, true);
		}
	});
	self.set_charge(pMainObj);
	self._charge.AddEventHandler("OnFullscreen", jsPopup.StretchDialogContentOnFullScreen, jsPopup);
}