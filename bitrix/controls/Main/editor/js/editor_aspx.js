var editor_path = APPPath + "/bitrix/controls/Main/editor";
var one_gif_src = editor_path + "/js/images/htmledit2/1.gif";
var image_path = editor_path + "/js/images/htmledit2";
var c2wait_path = image_path + '/c2waiter.gif';
var global_iconkit_path = image_path + '/_global_iconkit.gif';
var settings_page_path = editor_path + '/settings.aspx?set=0';
var editor_dialog_path = editor_path + '/editor_dialog.aspx';
var dxShadowImgPath = APPPath + '/bitrix/themes/.default/Images/shadow.png';
var flash_preview_path = editor_path + '/flash_preview.aspx';
var to_template_path = false;
var editor_action_path = editor_path + '/editor_action.aspx?a=0';

var bitrixWebAppPath = APPPath;
if (typeof(Bitrix) == 'undefined') {
	var Bitrix = {};
}

Bitrix.__CPM = Bitrix.ComponentParameterViewDynamicExpressionsManager;

BXHTMLEditor.prototype.OnLoad_ex = function() {
	this.pASPXParser = new ASPXParser(this);

	this.bUseAAP = true; // AAP - Advanced ASPX Parser
	this.AAPConfig =
	{
		arTags_before : ['tbody','thead','tfoot','tr','td','th'],
		arTags_after : ['tbody','thead','tfoot','tr','td','th'],
		arTags :
		{
			'a' : ['href','title','class','style'],
			'img' : ['src','alt','class','style']
		}
	};
	//this.AddEventHandler('OnSubmit', this.OnSubmit_ex, this);
};

BXHTMLEditor.prototype.OnSubmit_ex = function() {
	if(!this.bDotNet || this.sEditorMode == 'code')
		return;
	this.SaveContent();
	this.SetCodeEditorContent(this.GetContent());
	this.sEditorMode = 'code';
};

BXHTMLEditor.prototype.CheckCPHBeforeSave = function(){};

BXHTMLEditor.prototype.SelectElementControl = function (pElement) {
	if(this.pEditorWindow.getSelection)
	{
		var oSel = this.pEditorWindow.getSelection();
		oSel.selectAllChildren(pElement);
		oRange = oSel.getRangeAt(0);
	}
	else
	{
		this.pEditorDocument.selection.empty();
		try
		{
			var oRange = this.pEditorDocument.body.createControlRange();
			oRange.add(pElement);
			oRange.select();
		}
		catch (e) { _alert(e); }
	}
	return oRange;
};

BXHTMLEditor.prototype.SetCodeEditorContent_ex = function(sContent) {
	//if (this.fullEditMode)
		return sContent;
	return this._head + this._body + sContent + this._footer;
};

BXHTMLEditor.prototype.SetTemplate_ex = function() {
	var arTemplateParams = this.arTemplateParams;
	if (this.cphID && arTemplateParams.CPH)
	{
		var test = false;
		for (var i = 0;i < arTemplateParams.CPH.length; i++)
			if (arTemplateParams.CPH[i].toLowerCase() == this.cphID.toLowerCase()) {test = true; break; }
		if (!test && this.pASPXParser)
		{
			this.cphID = TrySetInvariantKey(arTemplateParams.CPH[0], this.pASPXParser.arShadowedControls);
			if (this.pCPHListbox)
				this.pCPHListbox.SelectByVal(this.cphID);
		}
	}
	this.OnEvent('ClearResourcesBeforeChangeView');
};


BXHTMLEditor.prototype.OnEvent_ex = function (eventName, arParams) {
	if(!this.arEventHandlers[eventName]) {
		if (arParams && arParams.length == 1)
			arParams = arParams[0];
		return arParams;
	}

	var res = arParams;
	for(var i=0; i < this.arEventHandlers[eventName].length; i++)
	{
		if(!res) res = [];
		if(this.arEventHandlers[eventName][i][1])
			res = this.arEventHandlers[eventName][i][0].apply(this.arEventHandlers[eventName][i][1], res);
		else
			res = this.arEventHandlers[eventName][i][0](res);
	}
	if (res && res.length == 1) res = res[0];
	return res;
};

BXHTMLEditor.prototype.LoadASPXComponents = function(oCallBack) {
	var callback = function(oCallBack)
	{
		if (!oCallBack.params)
			oCallBack.func.apply(oCallBack.obj);
		else
			oCallBack.func.apply(oCallBack.obj, oCallBack.params);
	};

	if (window.arASPXCompElements) // Components already loaded
		return callback(oCallBack);

	var count = 0;
	CHttpRequest.Action = function(){
		var interval = setInterval(function()
			{
				if (window.arASPXCompElements)
				{
					clearInterval(interval);
					window.as_arASPXCompElements = {};
					for (var i=0, l = window.arASPXCompElements.length; i < l; i++)
						window.as_arASPXCompElements[window.arASPXCompElements[i].name] = window.arASPXCompElements[i];
					callback(oCallBack);
					return;
				}

				if (count > 20)
				{
					clearInterval(interval);
					alert('Error: Cant load aspx visual components data');
					callback(oCallBack);
				}
				count++;
			}
		, 5);
	};
	CHttpRequest.Send(editor_path + '/load_aspxcomponents.aspx?lang='+BXLang+'&site='+BXSite);
};

// Advanced ASPX parser - AAP :)
BXHTMLEditor.prototype.AAP_Parse = function(sContent) {
	if (!this.bUseAAP)
		return sContent;

	this.arAAPFragments = [];
	sContent = this.AAP_ParseBetweenTableTags(sContent);
	sContent = this.AAP_ParseInAttributes(sContent);
	return sContent;
};

BXHTMLEditor.prototype.AAP_ParseBetweenTableTags = function(str) {
	var
		_this = this,
		replace_before = function(str, b1, b2, b3, b4)
		{
			_this.arAAPFragments.push(JS_addslashes(b1));
			return b2 + b3 + ' __bx_aspx_before=\"*AAP' + (_this.arAAPFragments.length - 1) + '*\" ' + b4;
		},
		replace_after = function(str, b1, b2, b3, b4)
		{
			_this.arAAPFragments.push(JS_addslashes(b4));
			return b1 + '>' + b3 + '<' + b2 + ' style="display:none;"__bx_aspx_after=\"*AAP' + (_this.arAAPFragments.length - 1) + '*\"></' + b2 + '>';
		},
		arTags_before = _this.AAPConfig.arTags_before,
		arTags_after = _this.AAPConfig.arTags_after,
		l1 = arTags_before.length,
		l2 = arTags_after.length,
		tagName, re, i;

	// ASPX fragments before tags
	for (i = 0; i < l1; i++)
	{
		tagName = arTags_before[i];
		re = new RegExp('<%(.*?)%>(\\s*)(<'+tagName+'[^>]*?)(>)',"ig");
		str = str.replace(re, replace_before);
	}

	// ASPX fragments after tags
	for (i = 0; i < l2; i++)
	{
		tagName = arTags_after[i];
		re = new RegExp('(</('+tagName+')[^>]*?)>(\\s*)<%(.*?)%>',"ig");
		str = str.replace(re, replace_after);
	}
	return str;
};

BXHTMLEditor.prototype.AAP_ParseInAttributes = function(str) {
	var
		_this = this,
		replace_inAtr = function(str, b1, b2, b3, b4, b5, b6)
		{
			_this.arAAPFragments.push(JS_addslashes(b5));
			return b1 + b2 + b3 + '""' + ' __bx_ex_' + b2 + b3 + '\"*AAP' + (_this.arAAPFragments.length - 1) + '*\"' + b6;
		},
		arTags = _this.AAPConfig.arTags,
		tagName, atrName, atr, i, re, cnt;

	for (tagName in arTags)
	{
		for (i = 0, cnt = arTags[tagName].length; i < cnt; i++)
		{
			atrName = arTags[tagName][i];
			re = new RegExp('(<' + tagName + '(?:[^>](?:\\?>)*?)*?)('+atrName+')(\\s*=\\s*)((?:"|\')?)<%(.*?)%>\\4((?:[^>](?:\\?>)*?)*?>)',"ig");
			str = str.replace(re, replace_inAtr);
		}
	}
	return str;
};


BXHTMLEditor.prototype.AAP_Unparse = function(sContent) {
	if (!this.bUseAAP)
		return sContent;

	sContent = this.AAP_UnparseBetweenTableTags(sContent);
	sContent = this.AAP_UnparseInAttributes(sContent);
	return sContent;
};

BXHTMLEditor.prototype.AAP_UnparseBetweenTableTags = function(str) {
	var 
		_this = this,
		unreplace_before = function(str, b1, b2, b3)
		{
			return '<%'+JS_stripslashes(_this.arAAPFragments[parseInt(b2)])+'%>'+b1+b3;
		},
		unreplace_after = function(str, b1, b2)
		{
			return b1+'<%'+JS_stripslashes(_this.arAAPFragments[parseInt(b2)])+'%>';
		},
		arTags_before = _this.AAPConfig.arTags_before,
		arTags_after = _this.AAPConfig.arTags_after,
		l1 = arTags_before.length,
		l2 = arTags_after.length,
		tagName, re, i;

	// ASPX fragments before tags
	for (i = 0; i < l1; i++)
	{
		tagName = arTags_before[i];
		re = new RegExp('(<'+tagName+'[^>]*?)__bx_aspx_before="\\*AAP(\\d+)\\*"([^>]*?>)',"ig");
		str = str.replace(re, unreplace_before);
	}
	// ASPX fragments after tags
	for (i = 0; i < l2; i++)
	{
		tagName = arTags_after[i];
		re = new RegExp('(</'+tagName+'[^>]*?>\\s*)<'+tagName+'[^>]*?__bx_aspx_after="\\*AAP(\\d+)\\*"[^>]*?>(?:.|\\s)*?</'+tagName+'>',"ig");
		str = str.replace(re, unreplace_after);
	}
	return str;
};

BXHTMLEditor.prototype.AAP_UnparseInAttributes = function(str) {
	var
		_this = this,
		un_replace_inAtr = function(str, b1, b2, b3, b4, b5, b6, b7){return b1+'"<%'+JS_stripslashes(_this.arAAPFragments[parseInt(b6)])+'%>" '+b3+b7;},
		un_replace_inAtr2 = function(str,b1,b2,b3,b4,b5,b6){return b1+b4+'"<%'+JS_stripslashes(_this.arAAPFragments[parseInt(b3)])+'%>" '+b6;},
		arTags = _this.AAPConfig.arTags,
		tagName, atrName, atr, i, re, re2, cnt;

	for (tagName in arTags)
	{
		for (i = 0, cnt = arTags[tagName].length; i < cnt; i++)
		{
			atrName = arTags[tagName][i];
			re = new RegExp('(<'+tagName+'(?:[^>](?:\\?>)*?)*?'+atrName+'\\s*=\\s*)("|\')[^>]*?\\2((?:[^>](?:\\?>)*?)*?)(__bx_ex_'+atrName+')(?:\\s*=\\s*)("|\')\\*AAP(\\d+)\\*\\5((?:[^>](?:\\?>)*?)*?>)',"ig");
			re2 = new RegExp('(<'+tagName+'(?:[^>](?:\\?>)*?)*?)__bx_ex_' + atrName + '\\s*=\\s*("|\')\\*AAP(\\d+)\\*\\2((?:[^>](?:\\?>)*?)*?'+atrName+'\\s*=\\s*)("|\').*?\\5((?:[^>](?:\\?>)*?)*?>)',"ig");
			str = str.replace(re, un_replace_inAtr);
			str = str.replace(re2, un_replace_inAtr2);
		}
	}
	return str;
};

function _GAttrEx(pEl, atrName, atrNameEX, pTaskbar) {
	var 
		returnASPX_atr = function(str, b1){return '<%' + JS_stripslashes(pTaskbar.pMainObj.arAAPFragments[parseInt(b1)])+'%>';},
		v = GAttr(pEl, atrNameEX);

	if (v.length > 0 && pTaskbar.pMainObj.bUseAAP)
		return  v.replace(/\*AAP(\d+)\*/ig, returnASPX_atr);

	return GAttr(pEl, atrName);
}

function _SAttrEx(pEl, atrName, atrNameEX, val, pTaskbar) {
	if (pTaskbar.pMainObj.bUseAAP && val.substr(0, 2) == '<%' && val.substr(val.length-2,2) == '%>')
	{
		var rep = function(str, b1)
		{
			var v = GAttr(pEl, atrNameEX), i;
			if (v.length > 0)
				i = parseInt(v.slice(4,-1));
			else
			{
				pTaskbar.pMainObj.arAAPFragments.push(JS_addslashes(b1));
				i = pTaskbar.pMainObj.arAAPFragments.length - 1;
				SAttr(pEl, atrNameEX, '*AAP' + i + '*');
				SAttr(pEl, atrName, " ");
			}
			pTaskbar.pMainObj.arAAPFragments[i] = JS_addslashes(b1);
		};
		val.replace(/<%((?:.|\s)*?)\%>/ig, rep);
	}
	else
	{
		pEl.removeAttribute(atrNameEX);
		SAttr(pEl, atrName, val);
	}
}