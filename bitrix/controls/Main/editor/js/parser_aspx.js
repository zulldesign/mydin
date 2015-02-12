// Main for ASPX

function ASPXComponentsTaskbar()
{
	var oTaskbar = this;

	ASPXComponentsTaskbar.prototype.OnTaskbarCreate = function ()
	{
		oTaskbar.bVisualRender = this.pMainObj.bRenderComponents;
		this.pHeaderTable.setAttribute("__bxtagname", "_taskbar_cached"); // need for correct context menu for taskbar title

		this.icon_class = 'tb_icon_components2';
		this.iconDiv.className = 'tb_icon ' + this.icon_class;
		this.ASPXParser = this.pMainObj.pASPXParser;
		oTaskbar.pCellComp = oTaskbar.CreateScrollableArea(oTaskbar.pWnd);
		oTaskbar.pCellComp.style.width = "100%";
		oTaskbar.pCellComp.style.height = "100%";
		this.pMainObj.pASPXComponentTaskbar = this;
		this.FetchArray(false);
		oBXEditorUtils.addPropertyBarHandler('aspxcomponent', oTaskbar.ShowProps);
		this.pMainObj.AddEventHandler("OnDblClick", this.ASPXParser.COnDblClick, this.ASPXParser);

		emptyRow = null;
		table = null;
		if (!this.pMainObj.templateID)
			this.pMainObj.templateID = '.default';

		if (oTaskbar.bVisualRender)
			oTaskbar.ASPXParser.InitRenderingSystem();
	}

	ASPXComponentsTaskbar.prototype.FetchArray = function (clearCache)
	{
	    var self = this;
		var loadASPXComp = function()
		{
			BX.cleanNode(oTaskbar.pCellComp);
			oTaskbar.BuildList(window.arASPXCompElements);
			window.as_arASPXCompElements = [];
			var i, l = window.arASPXCompElements.length;
			for (i = 0; i < l; i++)
				window.as_arASPXCompElements[window.arASPXCompElements[i].name] = window.arASPXCompElements[i];

			BX.closeWait();
		};

		BX.cleanNode(oTaskbar.pCellComp);

		if(!window.arASPXCompElements || clearCache)
		{
		    CHttpRequest.Action = function(result)
		    {
			    try
			    {
				    setTimeout(function (){loadASPXComp();}, 10);
			    }
			    catch(e)
			    {
			        _alert('Error: Cant load aspx visual components data');
			    }
		    };

			var url = editor_path + '/load_aspxcomponents.aspx?lang='+BXLang+'&site='+BXSite;
			if(clearCache === true)
			    url += '&clearCache=Y'

			BX.showWait();
			CHttpRequest.Send(url);
	    }
	    else
	        loadASPXComp();
	}

	ASPXComponentsTaskbar.prototype.BuildList = function (arElements)
	{
		for (var i = 0, len = arElements.length; i < len; i++)
		{
			arElements[i].tagname = 'aspxcomponent';
			arElements[i].childElements = [];
			arElements[i].params.name = arElements[i].name;

			if (arElements[i].isGroup && !arElements[i].path)
				oTaskbar.AddElement(arElements[i], oTaskbar.pCellComp, arElements[i].path);
		}
		this.fullyLoaded = false;
	}

	ASPXComponentsTaskbar.prototype.PreBuildList = function ()
	{
		var
			arElements = window.arASPXCompElements,
			i, len = arElements.length;

		for (i = 0; i < len; i++)
			if (!arElements[i].isGroup || arElements[i].path)
				oTaskbar.AddElement(arElements[i], oTaskbar.pCellComp, arElements[i].path);

		this.fullyLoaded = true;
	}

    ASPXComponentsTaskbar.prototype.IsCachingSupported = function ()
    {
        return true;
    }

    ASPXComponentsTaskbar.prototype.ClearCache = function ()
    {
        this.FetchArray(true);
    }

	ASPXComponentsTaskbar.prototype.OnElementDragEnd = function(oEl)
	{
		if (!oEl)
			return;

		var oTag = oTaskbar.pMainObj.GetBxTag(oEl);
		if (oTag.tag != 'aspxcomponent')
			return;

		// Run it only when dropped into editor doc
		if (oEl.ownerDocument != oTaskbar.pMainObj.pEditorDocument)
			return oTaskbar.OnElementDragEnd(oTaskbar.pMainObj.pEditorDocument.body.appendChild(oEl.cloneNode(false)));

		oTag.id = null;
		delete oTag.id;
		oEl.id = '';
		oEl.removeAttribute('id');

		var draggedElId = oTaskbar.pMainObj.SetBxTag(oEl, copyObj(oTag));

		if (oTaskbar.pMainObj.bRenderComponents)
		{
			var otherComp = BXFindParentByTagName(oEl, 'DIV', 'bxc2-block');
			if (otherComp) // Component dragget into another
				otherComp.parentNode.insertBefore(oEl, otherComp); // Put element before parent component
		}

		var id = oTaskbar.InitializeNewControl(oEl);

		if (oTaskbar.pMainObj.bRenderComponents)
		{
			oTaskbar.ASPXParser.StartWaiting(oEl); // Show waiting icon instead of native component's icon
			oTaskbar.ASPXParser.__bPreventComponentDeselect = true;
			oTaskbar.ASPXParser.GetRenderedContent({id: id, bSelect: true});
		}

		this.nLastDragNDropElement = null;
		this.nLastDragNDropElementFire = false;
	}

	ASPXComponentsTaskbar.prototype.InitializeNewControl = function(oEl)
	{
		var aspx = oTaskbar.ASPXParser;

		var oTag = oTaskbar.pMainObj.GetBxTag(oEl);
		if (oTag.tag != 'aspxcomponent' || !oTag.params || !oTag.params.name)
			return;

		var element = window.as_arASPXCompElements[oTag.params.name];
		if (!element)
			return;

		// Get Component name
		var componentName = element.name.replace('.', '');
		i = componentName.indexOf(':');
		if (i >= 0)
			componentName = componentName.substring(i + 1, componentName.length);
		if (componentName.length <= 0)
			return null;

		// Get New Control Id
		var generateCID;
		do
		{
			generateCID = (componentName + oTaskbar.ASPXParser.GetFreeId(componentName)).replace(/[^a-zA-Z0-9_]/g, "_").toLowerCase();
		}
		while (aspx.arComponents[generateCID]);

		oTag.params.generateCID = generateCID;
		oTaskbar.pMainObj.SetBxTag(false, oTag.params);
		//oEl.id = generateCID;

		// Create attributes set
		var attrs = {name: oTag.params.name, id: generateCID, runat: 'server', componentname: element.name, template: '.default'};
		var appvp = oTaskbar.pMainObj.arConfig['path'];
		if (appvp)
			attrs.srcappvp = appvp;

		// Create new control information entry
		var ctrl = {id: generateCID, attributes: attrs, prefix: 'bx', control: 'IncludeComponent', ownerName: element.name, state:'new', close: '/>', elementId: oEl.id};

		// Clear all stuff
		aspx.RemoveComponent(generateCID);

		// Save control shadow
		aspx.arShadowedControls[generateCID] = ctrl;
		aspx.arComponents[generateCID] = ctrl;

		// Return html preview
		element = componentName = attrs = ctrl = null;
		return generateCID;
	}

	ASPXComponentsTaskbar.prototype.ShowProps = function (_bNew, _pTaskbar, _pElement, bReloadProps)
	{
		var oTag = oTaskbar.pMainObj.GetBxTag(_pElement);
		if (oTag.tag != 'aspxcomponent' || !oTag.params || !oTag.params.generateCID)
			return;

		var
			ctrl = oTaskbar.ASPXParser.GetCompParams(oTag.params.generateCID),
			__arProps = ctrl.attributes,
			postData = oBXEditorUtils.ConvertArray2Post(__arProps || {}, 'ctrl');

		if (!__arProps)
			return;

		var appvp = oTaskbar.pMainObj.arConfig['path'];
		if (appvp)
			__arProps.srcappvp = appvp;

		if (bReloadProps === true)
		{
			oTaskbar.LoadASPXCompParams(__arProps.name, oTaskbar.BXShowASPXComponentPanel,oTaskbar,[_bNew, _pTaskbar,_pElement],'POST',postData);
			oTaskbar.ASPXParser.ReRenderComponent(__arProps.id);
		}
		else if (window._bx_reload_template_props)
		{
			oTaskbar.deleteTemplateParams(_pElement);
			oTaskbar.setCompTemplate(_pElement, this);
		}
		else if (window.as_arASPXCompParams[__arProps.name])
		{
			oTaskbar.BXShowASPXComponentPanel(_bNew, _pTaskbar,_pElement);
		}
		else
		{
			oTaskbar.LoadASPXCompParams(__arProps.name,oTaskbar.BXShowASPXComponentPanel, oTaskbar, [_bNew, _pTaskbar,_pElement], 'POST', postData);
		}
	}

	ASPXComponentsTaskbar.prototype.LoadASPXCompParams = function (elementName, calbackFunc, calbackObj, calbackParams, method, data)
	{
		var loadHelp = (this.pMainObj.showTooltips4Components) ? "Y" : "N";
		arASPXCompTooltips[elementName] = [];

		CHttpRequest2.QueueRequest(function(result)
			{
				try{
					CHttpRequest2.CallHandler('def.load.scripts', result);
					setTimeout(function ()
						{
							window.as_arASPXCompParams[elementName] = window.arASPXCompProps;
							window.as_arASPXCompTemplates[elementName] = window.arASPXCompTemplates;

							if(calbackObj && calbackFunc)
								calbackFunc.apply(calbackObj,(calbackParams) ? calbackParams : []);
							else if(calbackFunc)
								calbackFunc();
						}, 10
					);
				}catch(e){alert('Error: LoadASPXCompParams');}
			});

		if (method == 'POST' && data)
			CHttpRequest2.Post(editor_path + '/work_aspxcomp.aspx?task=loadparams&lang=' + BXLang + '&site=' + BXSite + '&templateID=' + this.pMainObj.templateID  + "&loadhelp=" + loadHelp, data);
		else
			CHttpRequest2.Send(editor_path + '/work_aspxcomp.aspx?task=loadparams&lang=' + BXLang + '&site=' + BXSite + '&templateID=' + this.pMainObj.templateID + "&loadhelp=" + loadHelp);
	}

	ASPXComponentsTaskbar.prototype.OnElementClick = function (oEl,arEl)
	{
		if (!this.pMainObj.oPropertiesTaskbar)
			return;

		if (!arEl.screenshots)
			arEl.screenshots = [];

		_pTaskbar = this.pMainObj.oPropertiesTaskbar;
		BX.cleanNode(_pTaskbar.pCellProps);

		//****** DISPLAY TITLE *******
		var compName = arEl.name;
		var compTitle = arEl.title;
		var compDesc = arEl.desc;
		var bComplex = isTrue(arEl.complex);

		var tCompTitle = document.createElement("TABLE");
		tCompTitle.className = "componentTitle";
		var row = tCompTitle.insertRow(-1);
		var cell = row.insertCell(-1);
		cell.innerHTML = "<SPAN class='title'>"+compTitle+"  ("+compName+")</SPAN><BR /><SPAN class='description'>"+(bComplex ? BX_MESS.COMPLEX_COMPONENT : "")+compDesc+"</SPAN>";
		cell.className = "titlecell";
		cell.width = "100%";
		var _helpCell = row.insertCell(-1);
		_helpCell.className = "helpicon";

		_pTaskbar.pCellProps.appendChild(tCompTitle);
		var oDivSS;
		for (var i=0; i<arEl.screenshots.length; i++)
		{
			oDivSS = document.createElement("DIV");
			oDivSS.className = "scrshot";
			var imgSS = oTaskbar.pMainObj.CreateElement("IMG", {"src": arEl.screenshots[i],"title": compTitle, "alt": compTitle});
			oDivSS.appendChild(imgSS);
			_pTaskbar.pCellProps.appendChild(oDivSS);
			oDivSS = null;
		}

		oDivSS = null;
		_helpCell = null;
		_helpicon = null;
		tCompTitle = null;
	}

	ASPXComponentsTaskbar._paramGetCont = function(elId)
	{
		if(!((typeof(elId) == "string" || elId instanceof String) && elId.length > 0))
			throw "ASPXComponentsTaskbar._paramGetCont: elId is not valid!";

		var el = document.getElementById(elId);
		if(!el)
			return null;

		while(el){
			if(!("tagName" in el) || el.tagName == "TABLE")
				break;
			if(el.tagName == "TR" && el.className == "bxtaskbarpropscomp")
				return el;
			el = "parentNode" in el ? el.parentNode : null;
		}
		return null;
	}

	ASPXComponentsTaskbar._handleParameterViewChange = function(sender)
	{
		if(!(sender && typeof(sender) == "object"))
			return;
		if(typeof(sender["getComponentId"]) != "function" || typeof(sender["getComponentId"]) != "function" || typeof(sender["getParameterName"]) != "function" ||  typeof(sender["getCustomData"]) != "function")
			return;

		var
			senderCustomData = sender.getCustomData(),
			_pElement = senderCustomData.Element || false;

		if(!_pElement || !_pElement.id)
			return;

		var oTag = oTaskbar.pMainObj.GetBxTag(_pElement);
		if (!oTag.tag || !oTag.params || !oTag.params.generateCID)
			return;

		var ctrl = oTaskbar.ASPXParser.GetCompParams(oTag.params.generateCID);
		if (!ctrl)
			return;
		var arAllProps = ctrl.attributes;

		var
			value = sender.getValue(),
			componentId = sender.getComponentId(),
			parameterName = sender.getParameterName(),
			key = TryInvariantKey(parameterName, arAllProps);

		arAllProps[key || parameterName] = value;
		oTaskbar.ASPXParser.SetCompParams(oTag.params.generateCID, arAllProps);
	}

	ASPXComponentsTaskbar.prototype.BXShowASPXComponentPanel = function(_bNew, _pTaskbar, _pElement)
	{
		var oTag = oTaskbar.pMainObj.GetBxTag(_pElement);
		if (oTag.tag != 'aspxcomponent' || !oTag.params || !oTag.params.generateCID)
			return;

		var
			id = _pElement.id,
			ctrl = oTaskbar.ASPXParser.GetCompParams(oTag.params.generateCID),
			arAllProps = ctrl.attributes;

		if (!arAllProps)
			return;

		var arProps = window.as_arASPXCompParams[arAllProps.name];
		if (!arProps || typeof arProps != 'object')
			return;

		_pTaskbar.arElements = Array();
		var arCurrentTooltips = arASPXCompTooltips[arAllProps.name];

		//var fChange_SEF_MODE;
		BX.cleanNode(_pTaskbar.pCellProps);
		__arProps = {};

		var nodeIdKey = TryInvariantKey("id", arAllProps);
		var nodeId = nodeIdKey != undefined ? arAllProps[nodeIdKey] : "";

		if (typeof (Bitrix) != "undefined")
		{
			Bitrix.__CPM.startProcessing();
			Bitrix.ParamClientSideActionManager.startProcessing(nodeId);
		}

	    // **** Function handle changes in properties ****
	    var fChange = function(e)
	    {
	        var
				arAllFields = [], __k,
				ctrl = oTaskbar.ASPXParser.GetCompParams(oTag.params.generateCID),
				arAllPropsLoc = ctrl.attributes;

	        function addEl(arEls)
		{
	            var el, n, i, l = arEls.length;
	            for (i = 0; i < l; i++) {
	                el = arEls[i];
	                if (!el || !el["__exp"] || !isTrue(el["__exp"]))
	                    continue;
	                if (el["name"].substr(el["name"].length - 2, 2) == '[]') {
	                    n = el["name"].substr(0, el["name"].length - 2);
	                    if (!arAllFields[n])
	                        arAllFields[n] = [];
	                    arAllFields[n].push(el);
	                }
	                else
	                    arAllFields[el["name"]] = el;
	            }
	        }

	        var propID, i, j, val, propIDi;

	        addEl(_pTaskbar.pCellProps.getElementsByTagName("SELECT"));
	        addEl(_pTaskbar.pCellProps.getElementsByTagName("INPUT"));
	        addEl(_pTaskbar.pCellProps.getElementsByTagName("TEXTAREA"));

	        __arProps = {};
	        for (i = 0; i < _pTaskbar.arElements.length; i++) {
	            propID = _pTaskbar.arElements[i];
	            val = arAllFields[propID];

	            if (arAllFields[propID + '_alt'] && val.selectedIndex == 0)
	                val = arAllFields[propID + '_alt'];

	            propIDi = TryInvariantKey(propID, arAllPropsLoc);

	            if (propIDi == undefined)
	                propIDi = propID;

	            if (!val) {
	                if (arAllPropsLoc[propIDi])
	                    __arProps[propIDi] = arAllPropsLoc[propIDi]; // ???
	                continue;
	            }

	            if (val.tagName) // one element
	            {
	                if (val.tagName.toUpperCase() == "SELECT") {
	                    for (j = 0; j < val.length; j++) {
	                        //if(val[j].selected && val[j].value!='')
	                        if (val[j].selected)
	                            __arProps[propIDi] = val[j].value;
	                    }
	                }
	                else {
	                    __arProps[propIDi] = val.value;
	                }
	            }
	            else {
	                var __arProps_temp = [];
	                for (k = 0; k < val.length; k++) {
	                    if (val[k].tagName.toUpperCase() == "SELECT")
	                        for (j = 0; j < val[k].length; j++) {
	                        //if(val[k][j].selected && val[k][j].value!='')
	                        if (val[k][j].selected)
	                            __arProps_temp.push(val[k][j].value);
	                    }
	                    else
	                        __arProps_temp.push(val[k].value);
	                }
	                __arProps[propIDi] = _BXArr2Str(__arProps_temp);
	                __arProps_temp = null;
	            }
	        }

	        // Update store collection
	        for (var key in __arProps) {
	            var ki = TryInvariantKey(key, arAllPropsLoc);
	            if (ki == undefined)
	                ki = key;
	            arAllPropsLoc[ki] = __arProps[key];
	        }

	        oTaskbar.ASPXParser.SetCompParams(oTag.params.generateCID, arAllPropsLoc);
	        oTaskbar.ASPXParser.ReRenderComponent(oTag.params.generateCID);
	    }

	    // **** function display parametr ****
	    var fDisplay = function(arProp, __tProp, oCont)
	    {
	        propertyID = arProp.param_name;
	        _pTaskbar.arElements.push(propertyID);
	        var
				refreshByOnclick = false,
				propertyIdKey = "",
				paramValueString = "", //parameter values as string
				oOption;

	        res = '';
	        arUsedValues = [];
	        arPropertyParams = arProp;

	        // Load param value
	        if (arPropertyParams["DEFAULT"] != undefined)
	            arValues = paramValueString = arPropertyParams["DEFAULT"];
	        else
	            arValues = paramValueString = "";


	        if (arAllProps) {
	            propertyIdKey = TryInvariantKey(propertyID, arAllProps);
	            if (propertyIdKey != undefined)
	                arValues = paramValueString = arAllProps[propertyIdKey];
	            else
	                arAllProps[propertyID] = arValues;
	        }
	        else {
	            arAllProps = {};
	            arAllProps[propertyID] = arValues;
	        }

	        arValues = Bitrix.__CPM.processObjectParameterViewValue(arValues);
	        if ((typeof (arValues) != "string" && !(arValues instanceof String)) || arValues.length == 0) {
	            if ("DEFAULT" in arPropertyParams)
	                arValues = arPropertyParams["DEFAULT"];
	        }

	        arAllProps[propertyIdKey || propertyID] = arValues;

	        if (!arPropertyParams.MULTIPLE || !isTrue(arPropertyParams.MULTIPLE))
	            arPropertyParams.MULTIPLE = "False";
	        if (!arPropertyParams.TYPE)
	            arPropertyParams.TYPE = "STRING";
	        if (!arPropertyParams.CNT)
	            arPropertyParams.CNT = 0;
	        if (!arPropertyParams.SIZE)
	            arPropertyParams.SIZE = 0;
	        if (!arPropertyParams.ADDITIONAL_VALUES)
	            arPropertyParams.ADDITIONAL_VALUES = 'False';
	        if (!arPropertyParams.ROWS)
	            arPropertyParams.ROWS = 0;
	        if (!arPropertyParams.COLS || arPropertyParams.COLS < 1)
	            arPropertyParams.COLS = '30';

	        if (arPropertyParams.MULTIPLE && isTrue(arPropertyParams.MULTIPLE) && typeof (arValues) != 'object') {
	            if (!arValues)
	                arValues = Array();
	        }
	        else if (arPropertyParams.TYPE && arPropertyParams.TYPE == "LIST" && typeof (arValues) != 'object')
	            arValues = Array(arValues);

	        if (arPropertyParams.MULTIPLE && isTrue(arPropertyParams.MULTIPLE)) {
	            arPropertyParams.CNT = parseInt(arPropertyParams.CNT);
	            if (arPropertyParams.CNT < 1)
	                arPropertyParams.CNT = 1;
	        }

	        if (!arPropertyParams.PARENT) {
	            arPropertyParams.PARENT = '__bx_additional_group';
	            arPropertyParams.group_title = BX_MESS.ADD_INSERT;
	        }
	        // If it's group property
	        if (arPropertyParams.PARENT) {
	            if (!arGroups[arPropertyParams.PARENT]) {
	                arGroups[arPropertyParams.PARENT] =
						{
						    title: arPropertyParams.group_title,
						    datacell: oTaskbar.GetPropGroupDataCell(arPropertyParams.PARENT, arPropertyParams.group_title, oCont, [arAllProps.name])
						};
	                _dataCell = arGroups[arPropertyParams.PARENT].datacell;
	            }
	            else
	                _dataCell = arGroups[arPropertyParams.PARENT].datacell;

	            if (_dataCell.getElementsByTagName("TABLE").length > 0)
	                tProp = _dataCell.getElementsByTagName("TABLE")[0];
	            else {
	                var tProp = document.createElement("TABLE");
	                tProp.className = "bxtaskbarprops";
	                tProp.style.width = "100%";
	                tProp.cellSpacing = 0;
	                tProp.cellPadding = 1;
	                _dataCell.appendChild(tProp);
	            }
	        }
	        else {
	            tProp = __tProp;
	            oCont.appendChild(tProp);
	        }

	        row = tProp.insertRow(-1);
	        row.className = "bxtaskbarpropscomp";
	        cell = row.insertCell(-1);
	        cell.width = "50%";
	        cell.align = "right";
	        cell.vAlign = "top";
	        var oSpan = _pTaskbar.pMainObj.CreateElement("SPAN", { 'innerHTML': oTaskbar.Remove__script__(arPropertyParams.NAME) + ':' });
	        cell.appendChild(oSpan);
	        oSpan = null;
	        cell = row.insertCell(-1);
	        cell.width = "50%";

	        arPropertyParams.TYPE = arPropertyParams.TYPE.toUpperCase();

	        //****** Displaying data ******
	        //array of elements to add in data cell
	        var arDataControls = [];
	        switch (arPropertyParams.TYPE) {
	            case "LIST":
	                arPropertyParams.SIZE = (isTrue(arPropertyParams.MULTIPLE) && (parseInt(arPropertyParams.SIZE) <= 1 || isNaN(parseInt(arPropertyParams.SIZE))) ? '5' : arPropertyParams.SIZE);
	                if (parseInt(arPropertyParams.SIZE) <= 0 || isNaN(parseInt(arPropertyParams.SIZE)))
	                    arPropertyParams.SIZE = 1;

	                pSelect = _pTaskbar.pMainObj.CreateElement("SELECT", { size: arPropertyParams.SIZE, name: propertyID + (arPropertyParams.MULTIPLE && isTrue(arPropertyParams.MULTIPLE) ? '[]' : ''), '__exp': 'True', 'onchange': fChange, "multiple": (isTrue(arPropertyParams.MULTIPLE)) });
	                arDataControls.push(pSelect);

	                if (!arPropertyParams.VALUES)
	                    arPropertyParams.VALUES = [];

	                var _arValues = [];
	                if (typeof arValues == 'string')
	                    _arValues = _BXStr2Arr(arValues);

	                bFound = false;
	                for (opt_val in arPropertyParams.VALUES) {
	                    bSel = false;
	                    oOption = new Option(JS_stripslashes(arPropertyParams.VALUES[opt_val]), opt_val, false, false);
	                    pSelect.options.add(oOption);

	                    key = BXSearchInd(arValues, opt_val);

	                    if (key < 0 && _arValues.length > 0)
	                        key = BXSearchInd(_arValues, opt_val);

	                    if (key >= 0) {
	                        bFound = true;
	                        arUsedValues[key] = true;
	                        bSel = true;
	                        setTimeout(__BXSetOptionSelected(oOption, true), 1);
	                    }
	                }
	                if (pSelect.options.length == 1)
	                    setTimeout(__BXSetOptionSelected(pSelect.options[0], false), 1);

	                if (arPropertyParams.ADDITIONAL_VALUES.toLowerCase() != 'false') {
	                    oOption = document.createElement("OPTION");
	                    oOption.value = '';
	                    oOption.selected = !bFound;
	                    oOption.text = (isTrue(arPropertyParams.MULTIPLE) ? BX_MESS.TPropCompNS : BX_MESS.TPropCompOth) + ' ->';
	                    pSelect.options.add(oOption, 0);
	                    oOption = null;

	                    if (isTrue(arPropertyParams.MULTIPLE)) {
	                        if (typeof (arValues) == 'string')
	                            arValues = _arValues;

	                        for (k = 0; k < arValues.length; k++) {
	                            if (arUsedValues[k])
	                                continue;

	                            val = JS_stripslashes(arValues[k]);
	                            arDataControls.push(_pTaskbar.pMainObj.CreateElement("BR"));
	                            if (arPropertyParams.ROWS > 1) {
	                                var oTextarea = _pTaskbar.pMainObj.CreateElement("TEXTAREA", { 'cols': (isNaN(arPropertyParams.COLS) ? '20' : arPropertyParams.COLS), 'value': val, 'name': propertyID + '[]', '__exp': 'True', 'onchange': fChange });
	                                arDataControls.push(oTextarea);
	                                oTextarea = null;
	                            }
	                            else {
	                                var oInput = _pTaskbar.pMainObj.CreateElement("INPUT", { 'type': 'text', 'size': (isNaN(arPropertyParams.COLS) ? '20' : arPropertyParams.COLS), 'value': val, 'name': propertyID + '[]', '__exp': 'True', 'onchange': fChange });
	                                arDataControls.push(oInput);
	                                oInput = null;
	                            }
	                        }

	                        for (k = 0; k < arPropertyParams.CNT; k++) {
	                            arDataControls.push(_pTaskbar.pMainObj.CreateElement("BR"));
	                            if (arPropertyParams.ROWS > 1) {
	                                var oTextarea = _pTaskbar.pMainObj.CreateElement("TEXTAREA", { 'cols': (isNaN(arPropertyParams.COLS) ? '20' : arPropertyParams.COLS), 'value': '', 'name': propertyID + '[]', '__exp': 'True', 'onchange': fChange });
	                                arDataControls.push(oTextarea);
	                                oTextarea = null;
	                            }
	                            else {
	                                var oInput = _pTaskbar.pMainObj.CreateElement("INPUT", { 'type': 'text', 'size': (isNaN(arPropertyParams.COLS) ? '20' : arPropertyParams.COLS), 'value': '', 'name': propertyID + '[]', '__exp': 'True', 'onchange': fChange });
	                                arDataControls.push(oInput);
	                                oInput = null;
	                            }
	                        }
	                        arDataControls.push(_pTaskbar.pMainObj.CreateElement("BR"));
	                        var oInput = _pTaskbar.pMainObj.CreateElement("INPUT", { 'type': 'button', 'value': '+', 'pMainObj': _pTaskbar.pMainObj, 'arPropertyParams': arPropertyParams });
	                        oInput.propertyID = propertyID;
	                        oInput.fChange = fChange;
	                        oInput.onclick = function() {
	                            if (this.arPropertyParams['ROWS'] && this.arPropertyParams['ROWS'] > 1) {
	                                var oTextarea = this.pMainObj.CreateElement("TEXTAREA", { 'cols': (!this.arPropertyParams['COLS'] || isNaN(this.arPropertyParams['COLS']) ? '20' : this.arPropertyParams['COLS']), 'value': '', 'name': this.propertyID + '[]', '__exp': 'True', 'onchange': this.fChange });
	                                this.parentNode.insertBefore(oTextarea, this);
	                                oTextarea = null;
	                            }
	                            else {
	                                var oInput = this.pMainObj.CreateElement("INPUT", { 'type': 'text', 'size': (!this.arPropertyParams['COLS'] || isNaN(this.arPropertyParams['COLS']) ? '20' : this.arPropertyParams['COLS']), 'value': '', 'name': this.propertyID + '[]', '__exp': 'True', 'onchange': this.fChange });
	                                this.parentNode.insertBefore(oInput, this);
	                                oInput = null;
	                            }
	                            this.parentNode.insertBefore(this.pMainObj.CreateElement("BR"), this);
	                        }
	                        arDataControls.push(oInput);
	                        oInput = null;
	                        var oBR = _pTaskbar.pMainObj.CreateElement("BR");
	                        arDataControls.push(oBR);
	                        oBR = null;
	                    }
	                    else {
	                        val = '';
	                        for (k = 0; k < arValues.length; k++) {
	                            if (arUsedValues[k])
	                                continue;
	                            val = arValues[k];
	                            break;
	                        }

	                        if (arPropertyParams['ROWS'] && arPropertyParams['ROWS'] > 1)
	                            alt = _pTaskbar.pMainObj.CreateElement("TEXTAREA", { 'cols': (!arPropertyParams['COLS'] || isNaN(arPropertyParams['COLS']) ? '20' : arPropertyParams['COLS']), 'value': val, 'disabled': bFound, 'name': propertyID + '_alt', '__exp': 'True', 'onchange': fChange });
	                        else
	                            alt = _pTaskbar.pMainObj.CreateElement("INPUT", { 'type': 'text', 'size': (!arPropertyParams['COLS'] || isNaN(arPropertyParams['COLS']) ? '20' : arPropertyParams['COLS']), 'value': val, 'disabled': bFound, 'name': propertyID + '_alt', '__exp': 'True', 'onchange': fChange });

	                        arDataControls.push(alt);
	                        pSelect.pAlt = alt;

	                        if (!arPropertyParams.REFRESH || !isTrue(arPropertyParams.REFRESH)) {
	                            pSelect.onchange = function(e) { this.pAlt.disabled = (this.selectedIndex != 0); fChange(e); };
	                            Bitrix.__CPM.processObjectParameterViewRefreshSource(nodeId, propertyID, alt, "change");
	                        }
	                    }
	                }

	                if (arPropertyParams.REFRESH && isTrue(arPropertyParams.REFRESH)) {
	                    pSelect.onchange = function(e) {
	                        fChange(e);
	                        _this.ShowProps(_bNew, _pTaskbar, _pElement, true);
	                    };
	                    Bitrix.__CPM.processObjectParameterViewRefreshSource(nodeId, propertyID, pSelect, "change");
	                }

	                if (arDataControls.length > 1) {
	                    var controlsContainer = document.createElement("DIV");
	                    for (var arDataControlsInd = 0; arDataControlsInd < arDataControls.length; arDataControlsInd++)
	                        controlsContainer.appendChild(arDataControls[arDataControlsInd]);
	                    arDataControls.splice(0, arDataControls.length);
	                    arDataControls.push(controlsContainer);
	                }

	                Bitrix.__CPM.processObjectParameterView(nodeId, propertyID, paramValueString, arDataControls, cell);
	                if (typeof (arPropertyParams.CLIENT_SIDE_ACTION) == "object")
	                    Bitrix.ParamClientSideActionManager.processObjectParameterView(nodeId, propertyID, pSelect, arPropertyParams.CLIENT_SIDE_ACTION, ASPXComponentsTaskbar._paramGetCont);
	                break;
	            case "CHECKBOX":
	                pCheckbox = _pTaskbar.pMainObj.CreateElement("INPUT", { 'type': 'checkbox', 'name': propertyID, '__exp': 'True' });

	                if (arValues)
	                    oBXEditorUtils.setCheckbox(pCheckbox, arValues && isTrue(arValues), false);
	                else if (arPropertyParams.DEFAULT != undefined)
	                    oBXEditorUtils.setCheckbox(pCheckbox, arPropertyParams.DEFAULT && isTrue(arPropertyParams.DEFAULT), false);
	                else
	                    oBXEditorUtils.setCheckbox(pCheckbox, false, false);

	                if (arPropertyParams.REFRESH && isTrue(arPropertyParams.REFRESH)) {
	                    pCheckbox.onclick = function(e) {
	                        oBXEditorUtils.setCheckbox(this, this.checked, false);
	                        fChange(e);
	                        _this.ShowProps(_bNew, _pTaskbar, _pElement, true);
	                    }
	                    refreshByOnclick = true;
	                    Bitrix.__CPM.processObjectParameterViewRefreshSource(nodeId, propertyID, pCheckBox, "click");
	                }
	                else {
	                    pCheckbox.onclick = function(e) {
	                        oBXEditorUtils.setCheckbox(this, this.checked, false);
	                        fChange(e);
	                    }
	                }

	                arDataControls = [];
	                arDataControls.push(pCheckbox);

	                arAllProps[propertyIdKey || propertyID] = pCheckbox.value;
	                //arAllProps[TrySetInvariantKey(propertyID, arAllProps)] = pCheckbox.value;
	                Bitrix.__CPM.processObjectParameterView(nodeId, propertyID, paramValueString, arDataControls, cell);
	                if (typeof (arPropertyParams.CLIENT_SIDE_ACTION) == "object")
	                    Bitrix.ParamClientSideActionManager.processObjectParameterView(nodeId, propertyID, pCheckbox, arPropertyParams.CLIENT_SIDE_ACTION, ASPXComponentsTaskbar._paramGetCont);
	                break;
	            case "CUSTOM":
	                var registrator = typeof (arPropertyParams.CUSTOM_REGISTRATOR) == "object" ? arPropertyParams.CUSTOM_REGISTRATOR : null;
	                if (registrator && typeof (registrator["register"]) == "function") {
	                    var customData = new Object();
	                    customData["Element"] = _pElement;
	                    if (arAllProps) {
	                        var key = TryInvariantKey(propertyID, arAllProps);
	                        customData["SelectedValues"] = key ? arAllProps[key] : "";
	                    }
	                    if (arPropertyParams && typeof (arPropertyParams) == "object" && typeof (arPropertyParams["VALUES"]) == "object") {
	                        customData["Items"] = arPropertyParams["VALUES"];
	                    }
	                    var registationOptions = Bitrix.ComponentParameterRegistrationOptions.create(nodeId, propertyID, null, null, null, customData);
	                    var info;
	                    try {
	                        info = registrator.register(registationOptions);
	                    } catch (e) { }
	                    if (info && typeof (info) == "object") {
	                        if (typeof (info["getElementsArray"]) == "function") {
	                            arDataControls = info.getElementsArray();
	                            Bitrix.__CPM.processObjectParameterView(nodeId, propertyID, paramValueString, arDataControls, cell);
	                        }
	                        if (typeof (info["getView"]) == "function") {
	                            var view = info.getView();
	                            if (view && typeof (view) == "object") {
	                                if (typeof (view["prepare"]) == "function")
	                                    view.prepare();
	                                if (typeof (view["addChangeListener"]) == "function")
	                                    view.addChangeListener(ASPXComponentsTaskbar._handleParameterViewChange);
	                            }
	                        }
	                    }
	                }
	                if (typeof (arPropertyParams.CLIENT_SIDE_ACTION) == "object")
	                    Bitrix.ParamClientSideActionManager.processObjectParameterView(nodeId, propertyID, pCheckbox, arPropertyParams.CLIENT_SIDE_ACTION, ASPXComponentsTaskbar._paramGetCont);
	                break;
	            default:
	                if (isTrue(arPropertyParams.MULTIPLE)) {
	                    arDataControls = [];
	                    bBr = false;
	                    for (val in arValues) {
	                        if (bBr)
	                            arDataControls.push(_pTaskbar.pMainObj.CreateElement("BR"));
	                        else
	                            bBr = true;

	                        if (arPropertyParams.ROWS > 1)
	                            arDataControls.push(_pTaskbar.pMainObj.CreateElement("TEXTAREA", { 'cols': (isNaN(arPropertyParams['COLS']) ? '20' : arPropertyParams['COLS']), 'value': JS_stripslashes(val), 'name': propertyID + '[]', '__exp': 'True', 'onchange': fChange }));
	                        else
	                            arDataControls.push(_pTaskbar.pMainObj.CreateElement("INPUT", { 'type': 'text', 'size': (isNaN(arPropertyParams['COLS']) ? '20' : arPropertyParams['COLS']), 'value': JS_stripslashes(val), 'name': propertyID + '[]', '__exp': 'True', 'onchange': fChange }));
	                    }

	                    for (k = 0; k < arPropertyParams.CNT; k++) {
	                        if (bBr)
	                            arDataControls.push(_pTaskbar.pMainObj.CreateElement("BR"));
	                        else
	                            bBr = true;

	                        if (arPropertyParams.ROWS > 1)
	                            arDataControls.push(_pTaskbar.pMainObj.CreateElement("TEXTAREA", { 'cols': (isNaN(arPropertyParams['COLS']) ? '20' : arPropertyParams['COLS']), 'value': '', 'name': propertyID + '[]', '__exp': 'True', 'onchange': fChange }));
	                        else
	                            arDataControls.push(_pTaskbar.pMainObj.CreateElement("INPUT", { 'type': 'text', 'size': (isNaN(arPropertyParams['COLS']) ? '20' : arPropertyParams['COLS']), 'value': '', 'name': propertyID + '[]', '__exp': 'True', 'onchange': fChange }));

	                    }
	                    arDataControls.push(_pTaskbar.pMainObj.CreateElement("BR"));
	                    var addBtn = _pTaskbar.pMainObj.CreateElement("INPUT", { 'type': 'button', 'value': '+', 'pMainObj': _pTaskbar.pMainObj, 'arPropertyParams': arPropertyParams });
	                    addBtn.propertyID = propertyID;
	                    addBtn.fChange = fChange;
	                    addBtn.onclick = function() {
	                        this.parentNode.insertBefore(this.pMainObj.CreateElement("BR"), this);
	                        if (this.arPropertyParams['ROWS'] && this.arPropertyParams['ROWS'] > 1)
	                            this.parentNode.insertBefore(this.pMainObj.CreateElement("TEXTAREA", { 'cols': (!this.arPropertyParams['COLS'] || isNaN(this.arPropertyParams['COLS']) ? '20' : this.arPropertyParams['COLS']), 'value': '', 'name': this.propertyID + '[]', '__exp': 'True', 'onchange': this.fChange }), this);
	                        else
	                            this.parentNode.insertBefore(this.pMainObj.CreateElement("INPUT", { 'type': 'text', 'size': (!this.arPropertyParams['COLS'] || isNaN(this.arPropertyParams['COLS']) ? '20' : this.arPropertyParams['COLS']), 'value': '', 'name': this.propertyID + '[]', '__exp': 'True', 'onchange': this.fChange }), this);
	                    }
	                    arDataControls.push(addBtn);
	                    arDataControls.push(_pTaskbar.pMainObj.CreateElement("BR"));

	                    if (arDataControls.length > 1) {
	                        var controlsContainer = document.createElement("DIV");
	                        for (var arDataControlsInd = 0; arDataControlsInd < arDataControls.length; arDataControlsInd++)
	                            controlsContainer.appendChild(arDataControls[arDataControlsInd]);
	                        arDataControls.splice(0, arDataControls.length);
	                        arDataControls.push(controlsContainer);
	                    }
	                    Bitrix.__CPM.processObjectParameterView(nodeId, propertyID, paramValueString, arDataControls, cell);
	                    if (typeof (arPropertyParams.CLIENT_SIDE_ACTION) == "object" && arDataControls.length > 0)
	                        Bitrix.ParamClientSideActionManager.processObjectParameterView(nodeId, propertyID, arDataControls[0], arPropertyParams.CLIENT_SIDE_ACTION, ASPXComponentsTaskbar._paramGetCont);
	                }
	                else {
	                    val = arValues;
	                    var pEl = (arPropertyParams.ROWS && arPropertyParams.ROWS) > 1 ?
						_pTaskbar.pMainObj.CreateElement("TEXTAREA", { 'cols': (!arPropertyParams.COLS || isNaN(arPropertyParams.COLS) ? '20' : arPropertyParams.COLS), 'value': JS_stripslashes(val), 'name': propertyID, '__exp': 'True', 'onchange': fChange })
						: _pTaskbar.pMainObj.CreateElement("INPUT", { 'type': 'text', 'size': (!arPropertyParams.COLS || isNaN(arPropertyParams.COLS) ? '20' : arPropertyParams.COLS), 'value': JS_stripslashes(val), 'name': propertyID, '__exp': 'True', 'onchange': fChange });

	                    arDataControls = []; // ????
	                    arDataControls.push(pEl);
	                    Bitrix.__CPM.processObjectParameterView(nodeId, propertyID, paramValueString, arDataControls, cell);
	                    if (typeof (arPropertyParams.CLIENT_SIDE_ACTION) == "object")
	                        Bitrix.ParamClientSideActionManager.processObjectParameterView(nodeId, propertyID, pEl, arPropertyParams.CLIENT_SIDE_ACTION, ASPXComponentsTaskbar._paramGetCont);

	                }
	                break;
	        }

	        if (arPropertyParams.REFRESH && isTrue(arPropertyParams.REFRESH) && !refreshByOnclick) {
	            var refreshButtonId = nodeId + "_" + propertyID + "_RefreshButton";
	            xCell = _pTaskbar.pMainObj.CreateElement("INPUT", { 'type': 'button', 'value': 'ok', 'pMainObj': _pTaskbar.pMainObj, 'arPropertyParams': arPropertyParams });
	            xCell.id = refreshButtonId;
	            xCell.onclick = function(e) {
	                fChange(e);
	                _this.ShowProps(_bNew, _pTaskbar, _pElement, true);
	            };
	            Bitrix.__CPM.processObjectParameterViewRefreshTrigger(nodeId, propertyID, xCell, "click", cell);
	        }

	        // #   TOOLTIP
	        if (arCurrentTooltips[propertyID] && _pTaskbar.pMainObj.showTooltips4Components) {
	            oBXHint = new BXHint(arCurrentTooltips[propertyID], 0, 0, 0, "image");
	            cell.appendChild(oBXHint.oIcon);
	            oBXHint.oIcon.style.marginLeft = "5px";
	        }
	    }

	    Bitrix.ComponentParametersEditor.getInstance().reset();
	    Bitrix.ComponentParametersEditor.getInstance().setViewContainerElementByViewIdFunction(ASPXComponentsTaskbar._paramGetCont);
	    //****** DISPLAY TITLE *******
	    var compTitle, compDesc, bComplex;
	    if (window.as_arASPXCompElements[arAllProps.name]) {
	        compTitle = window.as_arASPXCompElements[arAllProps.name].title;
	        compDesc = window.as_arASPXCompElements[arAllProps.name].desc;
	        bComplex = (window.as_arASPXCompElements[arAllProps.name].complex == 'Y') ? true : false;
	    }

	    var tCompTitle = document.createElement("TABLE");
	    tCompTitle.className = "componentTitle";
	    var row = tCompTitle.insertRow(-1);
	    var cell = row.insertCell(-1);
	    cell.innerHTML = "<SPAN class='title'>" + compTitle + "  (" + arAllProps.name + ")</SPAN><BR /><SPAN class='description'>" + (bComplex ? BX_MESS.COMPLEX_COMPONENT : "") + compDesc + "</SPAN>";
	    cell.className = "titlecell";
	    cell.width = "100%";
	    var _helpCell = row.insertCell(-1);
	    _helpCell.className = "helpicon";
	    _pTaskbar.pCellProps.appendChild(tCompTitle);
	    tCompTitle = null;

	    var arGroups = [];
	    //***********************************************
	    //DISPLAY COMPONENT TEMPLATE PARAMETERS
	    //***********************************************
	    var templateKey = TryInvariantKey("Template", arAllProps) || "Template";

	    if (arAllProps[templateKey] == undefined)
	        arAllProps[templateKey] = "";

	    var
			oOption, _el, k, j, site_template,
			arTemplates = as_arASPXCompTemplates[arAllProps.name],
			_len = arTemplates.length;

	    if (_len > 0)
	    {
	        var templList = document.createElement("SELECT");
	        templList.id = '__bx_comp2templ_select';
	        templList.onchange = function(e) {
	            oTaskbar.deleteTemplateParams(_pElement);
	            var postData = oBXEditorUtils.ConvertArray2Post(arAllProps.paramvals, 'curval');
	            oTaskbar.setCompTemplate(_pElement, this);

			var oTag = oTaskbar.pMainObj.GetBxTag(_pElement);
			if (!oTag.tag || !oTag.params || !oTag.params.generateCID)
				return;

			var ctrl = oTaskbar.ASPXParser.GetCompParams(oTag.params.generateCID);
			arAllProps = ctrl ? ctrl.attributes : {};

	            fChange(e);
	            _this.ShowProps(_bNew, _pTaskbar, _pElement, true);
	        };
	        Bitrix.__CPM.processObjectParameterViewRefreshSource(nodeId, "Template", templList, "change");


	        _T_datacell = oTaskbar.GetPropGroupDataCell('templateParams', BX_MESS.COMPONENT_TEMPLATE, _pTaskbar.pCellProps, [arAllProps.name]);
	        var tTProp = _pTaskbar.pMainObj.CreateElement("TABLE");
	        tTProp.className = "bxtaskbarprops";
	        tTProp.style.width = "100%";
	        tTProp.cellSpacing = 0;
	        tTProp.id = '__bx_tProp';
	        tTProp.cellPadding = 1;

	        row = tTProp.insertRow(-1);
	        row.className = "bxtaskbarpropscomp";
	        cell = row.insertCell(-1);
	        cell.width = "50%";
	        cell.align = "right";
	        cell.vAlign = "top";
	        var oSpan = _pTaskbar.pMainObj.CreateElement("SPAN", { 'innerHTML': BX_MESS.COMPONENT_TEMPLATE + ':' });
	        cell.appendChild(oSpan);
	        oSpan = null;
	        cell = row.insertCell(-1);
	        cell.width = "50%";
	        _T_datacell.appendChild(tTProp);
	        //cell.appendChild(templList);

	        Bitrix.__CPM.processObjectParameterView(nodeId, "template", arAllProps[templateKey], [templList], cell);

	        xCell = _pTaskbar.pMainObj.CreateElement("INPUT", { 'type': 'button', 'value': 'ok', 'pMainObj': _pTaskbar.pMainObj, 'arPropertyParams': arPropertyParams });
	        xCell.id = nodeId + "_Template_RefreshButton";
	        xCell.onclick = function(e) {
	            fChange(e);
	            _this.ShowProps(_bNew, _pTaskbar, _pElement, true);
	        };
	        Bitrix.__CPM.processObjectParameterViewRefreshTrigger(nodeId, "Template", xCell, "click", cell);

	        //Displaying component template list
	        for (j = 0; j < _len; j++) {
	            _el = arTemplates[j];
	            oOption = document.createElement("OPTION");
	            site_template = '';
	            if (_el.template != '')
	                for (k in arBXTemplates) {
	                if (arBXTemplates[k].value == _el.template) {
	                    site_template = ' (' + arBXTemplates[k].name + ')';
	                    break;
	                }
	            }
	            else
	                site_template = ' (' + BX_MESS.BUILD_IN_TEMPLTE + ')';

	            oOption.value = _el.name;
	            oOption.innerHTML = ((_el.title) ? _el.title : _el.name) + site_template;
	            oOption.selected = (arAllProps[templateKey] == undefined && (_el.name == ".default" || _el.name == "")) ||
							(arAllProps[templateKey] == "" && _el.name == ".default") ||
							(arAllProps[templateKey] == _el.name);
	            templList.appendChild(oOption);
	        }
	        //**** Displaying component's template parameters ****
	        var __len = window.arASPXCompTemplateProps.length;
	        __paramvals = [];
	        for (var __i = 0; __i < __len; __i++)
	            fDisplay(window.arASPXCompTemplateProps[__i], tTProp, _T_datacell);
	    }
	    templList = oOption = null;


	    //****************************************
	    //Displaying components params
	    //****************************************
	    var
			_this = this,
			oDiv = _pTaskbar.pMainObj.CreateElement('DIV', {}, { width: '100%', height: '0%' }),
			oCont = _pTaskbar.pCellProps,
			tProp = null,
			templateID = _pTaskbar.pMainObj.templateID,
			__tProp = _pTaskbar.pMainObj.CreateElement("TABLE", { id: '__bx_tProp', className: "bxtaskbarprops", cellSpacing: 0, cellPadding: 1 }, { width: '100%' }),
			row, cell, arPropertyParams, bSel, arValues, res, pSelect, arUsedValues, bFound, key, oOption, val, xCell, opt_val, bBr, i, k, alt,
			propertyID, _dataCell, k,
			arSorted = [], sorted,
			parInd = 0;

	    for (k in arProps)
	        arSorted[parInd++] = arProps[k];

	    arSorted.sort(ASPXComponentsTaskbar._compareComponentParametersByGroupSort);

	    for (parInd = 0; parInd < arSorted.length; parInd++) {
	        sorted = arSorted[parInd];
	        if (sorted.param_name == "SEFFolder" && !sorted.DEFAULT)
	            sorted.DEFAULT = (relPath != "/" ? relPath : "") + "/";
	        fDisplay(sorted, __tProp, oCont);
	    }

	    //oBXEditorUtils.setCustomNodeParams(_pElement,arAllProps);
	    __tProp = tProp = row = cell = arPropertyParams = pSelect = oOption = null;
	}

	ASPXComponentsTaskbar._compareComponentParametersByGroupSort = function(a, b)
	{
		if(("group_sort" in a) && ("group_sort" in b)){
			var aSort = parseInt(a.group_sort);
			var bSort = parseInt(b.group_sort);
			return (!isNaN(aSort) ? aSort : 65536) - (!isNaN(bSort) ? bSort : 65536);
		}
		if("group_sort" in a && !isNaN(parseInt(a.group_sort)))
			return -1;
		if("group_sort" in b && !isNaN(parseInt(b.group_sort)))
			return 1;
		return 0;
	}

	ASPXComponentsTaskbar.prototype.GetPropGroupDataCell = function (name, title, oCont,arParams)
	{
		var _oTable = document.createElement('TABLE');
		_oTable.cellPadding = 0;
		_oTable.cellSpacing = 0;
		_oTable.width = '100%';
		_oTable.className = 'bxpropgroup';
		_oTable.setAttribute('__bxpropgroup','__'+name);

		var rowTitle = _oTable.insertRow(-1);
		var c = rowTitle.insertCell(-1);
		c.style.width = '0%';
		c.appendChild(this.pMainObj.CreateElement("IMG", {src: one_gif_src, className: 'tskbr_common bx_btn_tabs_plus_big'}));
		c = rowTitle.insertCell(-1);
		c.style.width = '100%';
		c.innerHTML = (title) ? BXReplaceSpaceByNbsp(title) : "";

		var rowData = _oTable.insertRow(-1);
		c = rowData.insertCell(-1);
		c.colSpan = 2;
		c.id = '__bxpropgroup_dc_'+name;

		var compName = arParams[0];
		var _this = this;
		rowTitle.__bxhidden = false;
		rowTitle.id = '__bxpropgroup_tr_'+name;
		rowTitle.className = "bxtskbrprp_title_d";
		rowTitle.onclick = function(){_this.HidePropGroup(name,!this.__bxhidden,[compName]);};
		oCont.appendChild(_oTable);

		if (!arASPXCompPropGroups[compName])
		{
			arASPXCompPropGroups[compName] = {};
			arASPXCompPropGroups[compName][name] = false;
			oTaskbar.HidePropGroup(name,false,arParams);
		}
		else
			oTaskbar.HidePropGroup(name,((arASPXCompPropGroups[compName][name]===false) ? true : false),arParams);

		return c;
	}

	ASPXComponentsTaskbar.prototype.HidePropGroup = function (groupName,bHide,arParams)
	{
		var compName = arParams[0];
		arASPXCompPropGroups[compName][groupName] = !bHide;

		if (!arParams)
			arParams = [];

		var titleRow = document.getElementById('__bxpropgroup_tr_'+groupName);
		var dataCell = document.getElementById('__bxpropgroup_dc_'+groupName);

		if (bHide)
		{
			dataCell.style.display = GetDisplStr(0);
			titleRow.__bxhidden = true;
			titleRow.className = "bxtskbrprp_title_d";
			titleRow.cells[0].firstChild.className = 'tskbr_common bx_btn_tabs_plus_big';
		}
		else
		{
			dataCell.style.display = GetDisplStr(1);
			titleRow.__bxhidden = false;
			titleRow.className = "bxtskbrprp_title_a";
			titleRow.cells[0].firstChild.className = 'tskbr_common bx_btn_tabs_minus_big';
		}
	}

	ASPXComponentsTaskbar.prototype.Remove__script__ = function (str)
	{
		str = str.replace(/<\/__s__c__r__i__p__t__/ig, "</_script");
		str = str.replace(/\\n/ig, "\n");
		return str;
	}

	ASPXComponentsTaskbar.prototype.deleteTemplateParams = function(_pElement)
	{
		var oTag = oTaskbar.pMainObj.GetBxTag(_pElement);
		if (!oTag.tag || !oTag.params || !oTag.params.generateCID)
			return;

		var ctrl = oTaskbar.ASPXParser.GetCompParams(oTag.params.generateCID);
		var arAllProps = ctrl ? ctrl.attributes : {};

		var
			l = window.arASPXCompTemplateProps.length,
			n, i;

		for (i = 0; i < l; i++)
		{
			n = TryInvariantKey(window.arASPXCompTemplateProps[i].param_name);
			if (arAllProps[n]!=undefined)
				delete arAllProps[n];
		}
		// Save template
		oTaskbar.ASPXParser.SetCompParams(oTag.params.generateCID, arAllProps);
		window.arASPXCompTemplateProps = [];
	}

	ASPXComponentsTaskbar.prototype.setCompTemplate = function(_pElement, val)
	{
		var oTag = oTaskbar.pMainObj.GetBxTag(_pElement);
		if (!oTag.params || !oTag.params.generateCID)
			return;

		var ctrl = oTaskbar.ASPXParser.GetCompParams(oTag.params.generateCID);
		if (!ctrl)
			return;
		var arAllProps = ctrl.attributes;

		var templateKey = TryInvariantKey("Template", arAllProps) || "Template";
		if (val)
		{
			for(var j = 0, l = val.length; j < l; j++)
			{
				if(val[j].selected)
				{
					arAllProps[templateKey] = val[j].value;
					break;
				}
			}
		}
		else
		{
			arAllProps[templateKey] = '.default';
		}
		// Save template
		oTaskbar.ASPXParser.SetCompParams(oTag.params.generateCID, arAllProps);
		// Rerender component with new template
		oTaskbar.ASPXParser.ReRenderComponent(oTag.params.generateCID);
	}
}

function ASPXParser(pMainObj)
{
	this.pMainObj = pMainObj;
	var obj = this;

	this.freeIndexes = {};
	this.arExpressions = [];
	this.arShadowedControls = {};
	this.arComponents = {};
	this.arComponentsCSS = {};
	this.arComponentsSource = {};
	this.sCSS = '';

	// Content serialization direction
	// 0 - Default
	// 1 - ASPX Page Alone mode (usual, master page)
	// 2 - ASPX Content Page based on certain master page
	this.contentSerType = 0;
	// Rendering system
	//this.scr = new SysCompRender(pMainObj); // !!!!
	oBXEditorUtils.addContentParser(_SystemContentParse);
	oBXEditorUtils.addUnParser(obj.DropSpiritShadow);
	pMainObj.AddEventHandler('OnChangeView', this.OnChangeView, this);
	pMainObj.AddEventHandler('SetContentAfter', this.SetContent, this);

	pMainObj.AddEventHandler('ClearResourcesBeforeChangeView', this.ClearResourcesBeforeChangeView, this);
	pMainObj.AddEventHandler('LoadContentBefore', this.OnLoadFetchCPH, this);

	//if (!pMainObj.pCPHListbox){
	//pMainObj.AddEventHandler('OnSubmit', this.OnSaveContent, this);
    //}
}

ASPXParser.prototype.OnLoadFetchCPH = function()
{
	var obj = this;
	if(this.pMainObj.sFirstContent !== null)
		return;

	var str = this.pMainObj.GetContent();
	var nstr = '';
	var aspx = obj.pMainObj.pASPXParser;
	str = aspx.PreparseHeaders(str);
	if (aspx.contentSerType == 0)
		nstr = str;

	if (aspx.contentSerType == 1)
		nstr = aspx.ParseASPXComponents(str);

	if (typeof Bitrix.__CPM == 'undefined')
		Bitrix.__CPM = Bitrix.ComponentParameterViewDynamicExpressionsManager;

	if (aspx.contentSerType == 2)
	{
		// Content mode 2. Loading content in master page.
		//is not required - already called in _SystemContentParse
		//str = str.replace(/<%--([\s\S]*?)--%>/gi, this.ParseAspxComment); //Replace comments
		// Slice content page here
		var mpc = str, i = -1, key3;
		// Load cph signes
		var tag, tagregex = new RegExp('<(\s*[\\w\\d]+):Content[\\x20]+([^>]*)>', "ig");
		var attr, attrregex = new RegExp('([\\w_\\-]+)[\\x20]?=[\\x20]?"([^"]*)"', "ig");
		//var attrs, id, cphid, ocphid, runat, ctrl, i1, i2;
		var attrs, id, cphid, ocphid, runat, ctrl;
		while (tag = tagregex.exec(mpc))
		{
			attrs = {};
			id = cphid = runat = null;
			if (tag[2])
				while (attr = attrregex.exec(tag[2]))
					if (attr[1]) {
						attrs[attr[1]] = attr[2];
						if (attr[1].toLowerCase() == 'id')
							id = TrySetInvariantKey(attr[2], aspx.arShadowedControls);
						if (attr[1].toLowerCase() == 'contentplaceholderid')
							cphid = TrySetInvariantKey(attr[2], aspx.arShadowedControls);
						if (attr[1].toLowerCase() == 'runat')
							runat = attr[2];
					}

			id = "#bx#" + cphid + "#";

			// Check ContentPlaceHolder id existing
			ocphid = cphid;
			cphid = cphid.toLowerCase();
			if (!cphid)
				continue;
			if (!aspx.arShadowedControls[cphid])
			{
				aspx.arShadowedControls[cphid] = {
					id: cphid,
					attributes: {ID:cphid, runat:'server'},
					prefix: 'asp',
					control: 'ContentPlaceHolder',
					close: '>',
					tagend: ['</asp:ContentPlaceHolder>']
				};
			}

			aspx.arShadowedControls[cphid].cph = {
				id: id,
				attributes: attrs,
				prefix: tag[1],
				control: 'Content',
				cphid: cphid
			};
			aspx.arShadowedControls[cphid].ocphid = ocphid;

			for(var attrName in attrs)
				Bitrix.__CPM.getInstance().processObjectParameterValue(id, attrName, attrs[attrName]);

			//i1 = tag.index + tag[0].length;
			//i2 = mpc.indexOf('</asp:Content>', i1);
			var contentStr = mpc.substring(tag.index + tag[0].length);
			if(contentStr.length > 0)
			{
				var contentEndRx = new RegExp("<\\/\\s*" + tag[1] + "\\:Content\\s*", "i");
				var contentEndMatch = contentEndRx.exec(contentStr);
				if(contentEndMatch != null)
					contentStr = contentStr.substring(0, contentEndMatch.index);
			}

			aspx.arShadowedControls[cphid].contentVal = contentStr;
			//aspx.arShadowedControls[cphid].contentVal = '';
			//if (i1 > -1 && i2 > -1 && i2 >= i1)
			//	aspx.arShadowedControls[cphid].contentVal = mpc.substring(i1, i2);
		}
		//tagregex = tag = attr = attrregex = attrs = id = cphid = runat = ctrl = i1 = i2 = null;
		tagregex = tag = attr = attrregex = attrs = id = cphid = runat = ctrl = null;

		// Prepare ui system to startup
		obj.pMainObj.arCPH = [];

		// Check template data
		var tp = obj.pMainObj.arTemplateParams;
		if (tp && tp.CPH)
		{
			if (!obj.pMainObj.cphID && tp.CPH.length > 0)
				obj.pMainObj.cphID = tp.CPH[0].toLowerCase();
			for (var cphInd = 0; cphInd < tp.CPH.length; cphInd++)
			{
				id = tp.CPH[cphInd];
				key3 = TrySetInvariantKey(id, aspx.arShadowedControls);
				obj.pMainObj.arCPH.push({name:id, value:key3.toLowerCase()});
				key3 = TryInvariantKey(id, aspx.arShadowedControls);
				ocphid = id;
				id = id.toLowerCase();
				if (key3 == undefined)
				{
					aspx.arShadowedControls[id] = {
						id: id,
						ocphid: ocphid,
						attributes: {ID: id, runat: 'server'},
						prefix: 'asp',
						control: 'ContentPlaceHolder',
						close: '>',
						tagend: ['</asp:ContentPlaceHolder>']
					};
				}
			}
		}

		if (obj.pMainObj.arCPH && obj.pMainObj.cphID && window.InitializeCPHToolbar)
			InitializeCPHToolbar(obj.pMainObj);
		obj.pMainObj.AddEventHandler('OnSubmit', this.OnSaveContent, this);
	}
	aspx = null;
};

function _SystemContentParse(sContent, pMainObj)
{
	var ASPXParser = pMainObj.pASPXParser;
	return ASPXParser.SystemContentParse.apply(ASPXParser, [sContent]);
}

ASPXParser.prototype.OnLoadSystem = function()
{
	this.SetupSerializationMode();

	this.pMainObj.cphID = null;
	this.pMainObj.fullEdit = true;
	this.pMainObj.DotNetTemplate = window.DotNetTemplate || false; // master page

	this.pMainObj.pParser.isPhpAttribute = this.isAspxAttribute;
	this.pMainObj.pParser.GetAttributesList = this.GetAttributesList;
	//this.scr.editControllerStrongLevel = this.contentSerType;
}

ASPXParser.prototype.SetCompParams = function(id, params)
{
	if (this.arComponents[id])
		this.arComponents[id].attributes = params;
}

ASPXParser.prototype.GetAttributesList = function(str)
{
	str = " " + str + " ";
	var arParams = {}, arASPX = [], _this = this;
	// 1. Replace Aspx by #BXASPX#
	str = str.replace(/<%.*?%>/ig, function(s)
	{
		arASPX.push(s);
		return "#BXASPX" + (arASPX.length - 1) + "#";
	});

	// 2.0 Parse params - without quotes
	str = str.replace(/([^\w]??\s)(\w+?)=([^\s\'"]+?)\s/ig, function(s, b0, b1, b2)
	{
		b2 = b2.replace(/#BXASPX(\d+)#/ig, function(s, num){return arASPX[num] || s;});
		arParams[b1.toLowerCase()] = _this.isPhpAttribute(b2) ? b2 : BX.util.htmlspecialcharsback(b2);
		return b0;
	});

	// 2.1 Parse params
	str = str.replace(/([^\w]??\s)(\w+?)\s*=\s*("|\')([^\3]*?)\3/ig, function(s, b0, b1, b2, b3)
	{
		// 3. Replace Aspx back
		b3 = b3.replace(/#BXASPX(\d+)#/ig, function(s, num){return arASPX[num] || s;});
		arParams[b1.toLowerCase()] = _this.isPhpAttribute(b3) ? b3 : BX.util.htmlspecialcharsback(b3);
		return b0;
	});

	return arParams;
};

ASPXParser.prototype.isAspxAttribute = function(str)
{
	if (typeof str == 'number' || typeof str == 'object')
		return false;

	return str.indexOf("<%") != -1 || str.indexOf("%>") != -1;
};

ASPXParser.prototype.GetCompParams = function(id)
{
	return this.arComponents[id] || {};oTaskbar.ASPXParser.ReRenderComponent(id);
}

ASPXParser.prototype.SetupSerializationMode = function()
{
	// 0 - Default
	this.contentSerType = 0;

	if (BXContentType == 'AspxAlone' || BXContentType == 'MasterPage' || BXContentType == 'Control')
		this.contentSerType = 1;

	if (BXContentType == 'MasterContent')
		this.contentSerType = 2;
}

ASPXParser.prototype.GetFreeId = function(name)
{
	if (this.freeIndexes[name] > 0)
	{
		this.freeIndexes[name]++;
		return this.freeIndexes[name]-1;
	}

	this.freeIndexes[name] = 2;
	return 1;
}

ASPXParser.prototype.ReserveId = function(id, control)
{
	var gid = new RegExp('([^\\d]*)([\\d]*)$', 'i'), test_cid, cid;
	test_cid = gid.exec(id);
	if (!this.freeIndexes[control]) this.freeIndexes[control] = 1;
	if (test_cid)
	{
		if (test_cid[1] && !this.freeIndexes[test_cid[1]]) this.freeIndexes[test_cid[1]] = 1;

		cid = parseInt(test_cid[2], 10);
		if (cid>=this.freeIndexes[control]) this.freeIndexes[control]=cid+1;
		if (test_cid[1] && test_cid[1] != control &&
			cid>=this.freeIndexes[test_cid[1]]) this.freeIndexes[test_cid[1]]=cid+1;
	}
}

ASPXParser.prototype.OnChangeView = function(ViewState, IsSplit)
{
	if (this.contentSerType == 1 || this.contentSerType == 2)
		this.OnChangeView_CPHmode(ViewState, IsSplit);
}

ASPXParser.prototype.OnChangeView_CPHmode = function(ViewState, IsSplit)
{
	if (ViewState == 'code')
		this.ClearAllResources();
}

ASPXParser.prototype.ClearResourcesBeforeChangeView = function()
{
	//this.ClearAllResources();

	window.as_arASPXCompParams = {};
	window.arASPXCompProps = [];

	if (this.pMainObj.oPropertiesTaskbar)
		BX.cleanNode(this.pMainObj.oPropertiesTaskbar.pCellProps);
}

ASPXParser.prototype.ClearAllResources = function()
{
	var cphs = {};
	for (var i in this.arShadowedControls)
	{
		if (this.arShadowedControls[i].cph)
			cphs[i] = this.arShadowedControls[i];
		else
			this.RemoveComponent(i);
	}

	//DisposeGroupEvents('movdivs');
	//DisposeGroupEvents('mdLinks');
	//DisposeGroupEvents('dds');

	for (var i in this.arExpressions)
		this.arExpressions[i] = null;

	//this.freeIndexes = {};
	this.arShadowedControls = cphs;
	this.arExpressions = [];
}

ASPXParser.prototype.RemoveComponent = function(id)
{
	return;
	if (this.arShadowedControls[id])
	{
		if (this.arShadowedControls[id]['attributes'])
			this.arShadowedControls[id]['attributes'] = null;
		if (this.arShadowedControls[id]['tagend'])
			this.arShadowedControls[id]['tagend'] = null;
		this.arShadowedControls[id].contentVal = null;
		this.arShadowedControls[id] = null;
	}
}

/*
ASPXParser.prototype.getDefaultNodeParams = function(id)
{
	var omcnp = this.scr.getDefaultNodeParams(id);
	if (this.arShadowedControls[id])
	{
		if (this.arShadowedControls[id].attributes)
		{
			if (this.arShadowedControls[id].attributes['componentname'])
				this.arShadowedControls[id]['ownerName'] = this.arShadowedControls[id].attributes['componentname'];
		}

		if (this.arShadowedControls[id].ownerName)
			omcnp.name = this.arShadowedControls[id].ownerName;
		else if (this.arShadowedControls[id].control)
			omcnp.name = this.arShadowedControls[id].control;

		omcnp.componentname = omcnp.name;
		omcnp.template = '.default';

		var y, ky;
		if (omcnp.name && window.as_arASPXCompParams[omcnp.name])
			for (var key in window.as_arASPXCompParams[omcnp.name])
			{
				y = window.as_arASPXCompParams[omcnp.name][key];
				if (!y) continue;
				if (y.DEFAULT && y.param_name)
				{
					ky = TrySetInvariantKey(y.param_name, omcnp);
					omcnp[ky] = y.DEFAULT;
				}
			}
	}
	return omcnp;
}
*/

ASPXParser.prototype.SetContent = function(sContent)
{
	// If we came from html to code
	if (this.pMainObj.sEditorMode == 'html' ||
	(this.pMainObj.sEditorMode == 'split' && this.pMainObj.sEditorSplitMode != 'code'))
	{

		// Cut content if we are in 2 serialization mode
		if (this.contentSerType == 2)
		{
			// Save current cph, all others saved already or unchanged. Saving on cph change by listbox.
			var curId = this.pMainObj.cphID;
			if (curId && this.arShadowedControls[curId] && this.arShadowedControls[curId].cph)
				this.arShadowedControls[curId].contentVal = sContent;

			var str = '', i = 0, j = 0, ocphid, cphVal, id;

			var arTP = this.pMainObj.arTemplateParams;
			// Try to save using Template definition information
			if (arTP && arTP.ID == this.pMainObj.templateID && arTP.CPH)
			{
				for (j = 0; j < arTP.CPH.length; j++)
				{
					ocphid = arTP.CPH[j];
					id = TrySetInvariantKey(arTP.CPH[j], this.arShadowedControls);
					id = id.toLowerCase();
					if (id !== curId)
						continue;
					if (this.arShadowedControls[id] && this.arShadowedControls[id].cph)
					{
						cphVal = this.arShadowedControls[id].contentVal;
						if (id != this.pMainObj.cphID)
							cphVal = this.pMainObj.pParser.SystemUnParse(cphVal);
						str += (i++ > 0 ? '\n' : '') + cphVal;
						//str += (i++ > 0 ? '\n' : '') + this.HtmlCPHContent(id, cphVal);
					}
					else if (this.arShadowedControls[id])
					{
						var html_id = 'BXCPH' + j;
						this.arShadowedControls[id].cph = {
							id: html_id,
							cphid: id,
							attributes: {ID: html_id, runat: 'server'},
							prefix: 'asp',
							control: 'Content',
							tagend: '</asp:Content>',
							close: '>'
						};
						this.arShadowedControls[id].ocphid = ocphid;
						//str += (i > 0 ? '\n' : '') + this.HtmlCPHContent(id, '');
						i++;
					}
					else
					{
						_alert('Error during page serialization: needed cph does not exists = ' + id);
					}
				}
			}
			else
			{
				if (arTP.ID != this.pMainObj.templateID)
					_alert('Error during page serialization: current template definition is not relevant, ID = ' + arTP.ID + ', requiredID = ' + this.pMainObj.templateID);
				else
					_alert('Error during page serialization: invalid template definition');

				for (var id in this.arShadowedControls)
				{
					if (this.arShadowedControls[id] && this.arShadowedControls[id].cph)
					{
						if (i > 0)
							str += '\n';
						str += this.HtmlCPHContent(id, this.arShadowedControls[id].contentVal);
						i++;
					}
				}
			}
			this.pMainObj.value = str;
		}
		if (this.contentSerType == 1 || this.contentSerType == 2)
			this.pMainObj.value = this.TryHtmlExpressionsToCode(this.pMainObj.value);
	}
	return true;
}


ASPXParser.prototype.OnSaveContent = function()
{
	var aspx = this.pMainObj.pASPXParser;
	if (aspx.contentSerType != 2)
		return;

	var str = '', i = 0;
	for (var id in this.arShadowedControls)
	{
		if (this.pMainObj.cphID == id)
		{
		    if (this.arShadowedControls[id] && this.arShadowedControls[id].cph)
		    {
			    var bCode = this.pMainObj.sEditorMode == 'code' ||
			    (this.pMainObj.sEditorMode == 'split' && this.pMainObj.sEditorSplitMode == 'code');
			    // Save current CHP
			    var curCPHVal = (bCode) ? this.pMainObj.GetCodeEditorContent() : this.pMainObj.GetEditorContent(true, true);
			    this.arShadowedControls[id].contentVal = curCPHVal;
			}
		}
		str += (i > 0 ? '\n' : '')+ this.HtmlCPHContent(id, this.arShadowedControls[id].contentVal);
		i++;
	}

	str = this.pMainObj.pParser.AppendHBF(str, true);
	this.pMainObj.SetContent(str);
	this.pMainObj.SaveContent();
	this.pMainObj.SetCodeEditorContent(str);
	this.pMainObj.sEditorMode = 'code';
}


ASPXParser.prototype.HtmlCPHContent = function(id, sContent)
{
	var ctrl = this.arShadowedControls[id];

	if (!ctrl || !ctrl.cph)
		return '';
	var cphId = "contentplaceholderid";
	var res = '<' + ctrl.prefix + ':Content ';
	if(ctrl.cph.attributes)
	{
		var isContentPlaceHolderIdFound = false;
		for(var key in ctrl.cph.attributes)
		{
			if(key.toLowerCase() != cphId)
				continue;
			isContentPlaceHolderIdFound = true;
			break;
		}

		if (!isContentPlaceHolderIdFound)
			res += 'ContentPlaceHolderID="' + ctrl.ocphid + '" ';
		res += this.WriteAttributes(ctrl.cph.attributes);
	}
	else
		res += 'ContentPlaceHolderID="' + ctrl.ocphid + '" ';
	res += '>' + sContent + '</asp:Content>';
	return res;
}

ASPXParser.prototype.WriteAttributes = function(attrs)
{
	var str = '';
	if (!attrs) return str;
	for (var id in attrs)
		if (attrs[id])
			str += id+'="'+attrs[id]+'" ';
	return str;
}

ASPXParser.prototype.ExistId = function(id)
{
	return (this.arShadowedControls[id])?true:false;
}

ASPXParser.prototype.DropSpiritShadow = function(node)
{
	var oTag = node.pParser.pMainObj.GetBxTag(node.arAttributes['id']);
	var aspx = node.pParser.pMainObj.pASPXParser;

	if (oTag.tag == 'aspxcomponent')
		return aspx.UnparseToCode(node, oTag);

	if ((oTag.tag == 'aspx_comment' || oTag.tag == 'code' || oTag.tag == '.net component') && oTag.params)
		return oTag.params.value || "";

	// if (oTag.tag == 'code' && oTag.params)
	// {
		// //var r = oTag.params.value || "";
		// // var rx = /<%\s*(\=\s*[\s\S]*?)(\;+\s*)+%>/gi;
		// // var m = rx.exec("<%" + r + "%>");
		// // if(m != null)
			// // r = m[1];
		// //return '<%' + r + '%>';
		// return oTag.params.value || "";
	// }

	// if (oTag.tag == '.net component' && oTag.params)
		// return oTag.params.value || "";

	return false;
}

ASPXParser.prototype.ParseExtractExpressions = function(str)
{
	var expr, ei = this.arExpressions.length, i, j;

	// Cut comments first
	while ((i = str.indexOf('<!--')) > -1)
	{
		j = str.indexOf('-->', i);
		if (j > -1)
		{
			this.arExpressions[ei] = {
				'expression': str.substring(i+'<!--'.length, j-1),
				'type':'c'
				};
			j += '-->'.length;
			str = str.substring(0, i) + '$'+ei+'$' + str.substring(j, str.length);
			ei++;
		}
	}

	// Cut expressions second
	var expbldrx = new RegExp('<%([$@#%?=]?)([^>]*)%>', 'i');
	while (expr = expbldrx.exec(str)) {
		str=str.replace(expr[0], '$'+ei+'$');
		this.arExpressions[ei] = {'expression':expr[2], 'type':expr[1]};
		ei++;
	}

	return str;
}

ASPXParser.prototype.TryExpressionsBack = function(str)
{
	var s;
	var expr, exprex = new RegExp('\\$(\\d+)\\$','i');
	while(expr = exprex.exec(str)){
		matchVal = expr[1];
		if(!(matchVal in this.arExpressions))
			continue;

		if (this.arExpressions[matchVal]['type'] == 'c')
			s = '<!--' + this.arExpressions[matchVal]['expression'] + '-->';
		else
			s = '<%' + this.arExpressions[matchVal]['type'] + this.arExpressions[matchVal]['expression'] + '%>';
		str = str.replace(exprex, s);
	}
	return str;
}

ASPXParser.prototype.InjectExpressionsBack = function(str, encode)
{
	var s;
	var expr, exprex = new RegExp('\\$(\\d+)\\$','i');
	while (expr = exprex.exec(str))
	if (expr && this.arExpressions[expr[1]])
	{
		if (this.arExpressions[expr[1]]['type'] == 'c')
			s = '<!--' + this.arExpressions[expr[1]]['expression'] + '-->';
		else
			s = '<%' + this.arExpressions[expr[1]]['type'] +
					  this.arExpressions[expr[1]]['expression'] +
				'%>';
		if (encode) eval('s='+encode+'(s);');
		str = str.replace('$'+expr[1]+'$', s);
	} else {
		str = str.replace('$'+expr[1]+'$', '');
	}
	return str;
}

ASPXParser.prototype.TryHtmlExpressionsToCode = function(str)
{
	var expr;
	while (expr = str.match(/&lt;[!%][$@#%?=-]?.*[%-]&gt;/i))
		str = str.substring(0, expr.index) + XmlDecode(expr[0]) +
			str.substring(expr.index + expr[0].length, str.length);
	return str;
}

ASPXParser.prototype.UnparseToCode = function(node, oTag)
{
	var
		code, param,
		generateCID = oTag.params.generateCID,
		ctrl = this.GetCompParams(generateCID);

	if (!ctrl.ownerName)
		return '';

	var params = ctrl.attributes || false;

	// compile control. make head;
	code = '\r\n<';
	if (ctrl.prefix)
		code += ctrl.prefix + ':';
	code += ctrl.control;

	// collect attributes of control;
	if (params)
	{
		for (var key in params)
		{
			if (ctrl.control == 'IncludeComponent')
			{
			    var kl = key.toLowerCase();
			    if (kl == 'name' || kl == "srcappvp")
				    continue;
			}
			param = Bitrix.__CPM.getInstance().tryGetObjectParameterValue(generateCID, key);
			if(!param)
				param = bxhtmlspecialchars(params[key]);
			// try to inject this expression by $1$ regexp ($1$'s should be left by bxhtmlspecialchars)
			param = this.TryExpressionsBack(param);

			code += "\n " + key + '="' + param + '"';
		}
		code += "\n ";
	}

	// close tag
	code += ctrl.close;
	// write body
	if (ctrl.close == '>' && ctrl.tagend)
	{
		// insert content, without children controls
		if (ctrl.contentVal)
			code+=ctrl.contentVal;
		code+=ctrl.tagend;
	}

	return code;
}

ASPXParser.prototype.PreparseHeaders = function(sContent)
{
	if (window.fullEditMode) // Template Editing
		return sContent;

	this.pMainObj._head = '';
	this.pMainObj._body = '';
	this.pMainObj._footer = '';

	this.arExpressions = [];

	if (this.contentSerType == 0)
		return sContent;

	// Parse doctype
	var doctype = sContent.match(/<!(DOCTYPE[\x20]+)([^>]*)>/ig);
	if (doctype)
	{
		this.pMainObj._head += doctype[0];
		this.arExpressions['DOCTYPE'] = doctype[0].substring(10, doctype[0].length - 1);
		sContent = sContent.substring(doctype[0].length, sContent.length);
	}

	// Parse all <@ ASPX Directives
	var dir;
	var DirectiveRX = new RegExp('<%@[\\x20]*([\\w]+)[\\x20]*([^>]*)%>', 'i');
	var AttrRX = new RegExp('([\\w_\\-]+)=\"([^"]*)"', 'ig');
	while ((dir = DirectiveRX.exec(sContent)) != null)
	{
		this.pMainObj._head += dir[0];
		sContent = sContent.substring(0, dir.index) + sContent.substring(dir.index+dir[0].length, sContent.length);

		var item = {'tag': dir[1], 'param': {}};
		while ((attr = AttrRX.exec(dir[2])) != null)
		{
			item.param[attr[1]] = attr[2];
		}

		if (dir[1].toLowerCase() == 'register' || dir[1].toLowerCase() == 'import')
		{
			if (!this.arExpressions[dir[1]]) this.arExpressions[dir[1]] = [];
			this.arExpressions[dir[1]].push(item.param);
		}
		else
			this.arExpressions[dir[1]] = item;

		item = null;
	}
	dir = null;

	// Test our selves
	var catm = sContent.match(/<[\w\d_]+:Content[^>]+ContentPlaceHolderID/i);

	if (catm && this.contentSerType != 2)
	{
		alert('Error detection content type: Server thinks content type is '+this.contentSerType+' instead of 2.');
		this.contentSerType = 2;
	}

	if (this.contentSerType == 2)
	{
		var i = -1;
		// Load head
		if (catm)
			i = catm.index;
		if (i > -1)
		{
			this.pMainObj._head += sContent.substring(0, i);
			sContent = sContent.substring(i, sContent.length);
		}
		return sContent;
	}

	var frx = sContent.match(/<form[^>]+runat="server"[^>]*>/i);
	if (frx)
	{
		this.pMainObj._head += sContent.substring(0, frx.index+frx[0].length);

		var i = 0, oi = 0;
		while ((i = sContent.indexOf('</form>', oi+1))>-1) oi = i;
		if (oi > -1)
			this.pMainObj._footer = sContent.substring(oi, sContent.length);
		return sContent.substring(this.pMainObj._head.length + 1, oi);
	}

	var brx = sContent.match(/<body[^>]*>/i);
	if (brx)
	{
		this.pMainObj._head += sContent.substring(0, brx.index + brx[0].length);
		sContent = sContent.substring(brx.index + brx[0].length, sContent.length);
	}

	var brx = sContent.match(/<\/body>/i);
	if (brx)
	{
		this.pMainObj._footer = sContent.substring(brx.index, sContent.length);
		sContent = sContent.substring(0, brx.index);
	}

	return sContent;
}

ASPXParser.prototype.SystemContentParse = function(str)
{
	var _this = this;
	if (this.contentSerType == 1 || this.contentSerType == 2)
	{
		//Replace comments
		str = str.replace(/<%--[\s\S]*?--%>/gi, function(str)
		{
			return '<img id="' + _this.pMainObj.SetBxTag(false, {tag: "aspx_comment", params: {value: str}}) + '" src="' + image_path + '/aspx_comment.gif" />';
		});

		//Replace code
		str = str.replace(/<%\s*[^\$\#][\s\S]*?%>/gi, function(str)
		{
			return '<img id="' + _this.pMainObj.SetBxTag(false, {tag: "code", params: {value: str}}) + '" src="' + image_path + '/code.gif" />';
		});

		str = this.ParseASPXComponents(str);
	}
	return str;
}

ASPXParser.prototype.ParseDotNetComponents = function(code)
{
	return '<img id="' + this.pMainObj.SetBxTag(false, {tag: ".net component", params: {value: code}}) + '" src="' + image_path + '/dotnet.gif" />';
};

ASPXParser.prototype.GetCurrentCPHContent = function()
{
	if (this.pMainObj.cphID && this.arShadowedControls[this.pMainObj.cphID])
		return this.arShadowedControls[this.pMainObj.cphID].contentVal;
	return '';
};

ASPXParser.prototype.SetCurrentCPH = function(id, bInit)
{
	var oid = this.pMainObj.cphID;
	if (!bInit && (id == oid || !id))
		return;
	if (!this.arShadowedControls[id])
	{
		alert('Error: You are trying to switch to unknown cph.');
		return;
	}

	var bCode = this.pMainObj.sEditorMode == 'code' || (this.pMainObj.sEditorMode == 'split' && this.pMainObj.sEditorSplitMode == 'code');
	// Save current CHP
	if (!bInit)
	{
		var curCPHVal = (bCode) ? this.pMainObj.GetCodeEditorContent() : this.pMainObj.GetEditorContent(true, true);
		this.arShadowedControls[this.pMainObj.cphID].contentVal = curCPHVal;
	}

	this.pMainObj.cphID = id;
	// Get new CPH content
	var sCPHContent = this.arShadowedControls[id].contentVal || '';
	this.pMainObj.SetContent(sCPHContent);
	if (bInit)
		return;
	this.pMainObj.SetEditorContent(sCPHContent);
	if (bCode)
	{
		var newCPHVal = this.pMainObj.GetEditorContent(true, true);
		this.pMainObj.SetCodeEditorContent(newCPHVal);
	}

	this.pMainObj.OnEvent("OnChangeContentPlaceHolder");
}

ASPXParser.prototype.ParseASPXComponents = function(str)
{
	var
		gid = new RegExp('([\\d]+)$', 'i'),
		tagregex = new RegExp('<([\\w\\d]+):([\\w\\d]+\\b)\\s*([^>]*)([^/]>|/>)', "i"),
		newTagRegex = new RegExp("<([\\w\\d]+)\\:([\\w\\d]+\\b)(\\s+(\\w[-\\w:]*)(\\s*=\\s*\"([^\"]*)\"|\\s*=\\s*'([^']*)'|\\s*=\\s*(<\\%[\\#\\$].*?\\%>)|\\s*=\\s*([^\\s=/>]*)|(\\s*?)))*\\s*(/)?>", "i"),
		attrregex = new RegExp('([\\w_\\-]+)[\\x20]?=[\\x20]?"([^"]*)"',"ig"),
		tagerx = RegExp(''),
		tag, attr, attrs, id, runat, body, tagend, ctrl, ownerName, closeTag, ind, controlCode, closeInd;

	str = this.ParseExtractExpressions(str);

	while(str.search(tagregex) > -1)
	{
		tag = tagregex.exec(str);

		if (!tag)
		{
			alert('Error of parsing at '+str.search(tagregex));
			continue;
		}
		if (tag[4].charAt(0) != '/')
		{
			tag[3]+=tag[4].charAt(0);
			tag[4]='>';
		}
		attrs = {};
		id = runat = ownerName = null;

		if (tag[3])
			tag[3] = this.TryExpressionsBack(tag[3]);

		if (tag[3])
		{
			while (attr = attrregex.exec(tag[3]))
			{
				if (attr[2] && attr[2][attr[2].length-1] == '\\')
				{
					var search_i = attr.index + attr[0].length;
					while (
							tag[3][search_i] != '"' ||
							(tag[3][search_i] == '"'	&&
								(search_i > 0 && tag[3][search_i-1] == '\\'))
						   )
					{
						search_i++;
						if (search_i >= tag[3].length)
							break;
					}
					if (search_i >= tag[3].length)
					{
						alert('Error of parsing attributes: '+tag[3]);
					}
					else
					{
						attr[0] = tag[3].substring(attr.index, search_i + 1);
						var si = attr[0].indexOf('"');
						attr[2] = attr[0].substring(si + 1, search_i - attr.index);
						attr[0] = attr[0];
					}
				}
				if (attr[1])
				{
					attrs[attr[1]] = bxhtmlunspecialchars(attr[2]);
					if (attr[1].toLowerCase() == 'id')
						id = attrs[attr[1]];
					if (attr[1].toLowerCase() == 'runat')
					{
						attrs[attr[1]] = attr[2].toLowerCase();
						runat = attrs[attr[1]];
					}
					if (attr[1].toLowerCase() == 'componentname')
						ownerName = attrs[attr[1]];
				}
			}
		}

		tagend = null;
		if (runat != 'server')
		{
			alert("Runat attribute is not found or has invalid value: " + tag[0]);
			// Cut incorrect component
			str = str.substr(0, tag.index) + str.substr(tag.index + tag[0].length, str.length);
			continue;
		}

		if (tag[2].toLowerCase() != 'includecomponent')
		{
			closeTag = '</asp:' + tag[2] + '>';
			ind = tag.index + tag[0].length;

			closeInd = str.toLowerCase().indexOf(closeTag.toLowerCase(), ind);
			if (closeInd != -1)
				ind = closeInd + closeTag.length;

			//if (str.substr(tag.index + tag[0].length, closeTag.length).toLowerCase() == closeTag.toLowerCase())
			//	ind += closeTag.length;

			controlCode = str.substr(tag.index, ind - tag.index);
			controlCode = this.TryExpressionsBack(controlCode); //return expessions to control

			str = str.substr(0, tag.index) + this.ParseDotNetComponents(controlCode) + str.substr(ind);
			continue;
		}

		if(!id) //id is required for includecomponent
		{
			alert("Id is required for includecomponent: " + tag[0]);
			// Cut incorrect component
			str = str.substr(0, tag.index) + str.substr(tag.index + tag[0].length, str.length);
			continue;
		}

		if (!attrs.name)
			attrs.name = attrs.componentname;
		try //processing dynexpression only for includecomponent
		{
			for(var attrName in attrs)
				Bitrix.__CPM.getInstance().processObjectParameterValue(id, attrName, attrs[attrName]);
		}catch(e){}

		ctrl = {'id':id, 'attributes':attrs, 'prefix':tag[1], 'control':tag[2], 'i1':tag.index, 'i2':tag.index+tag[0].length, 'close':tag[4]};
		if (ownerName)
			ctrl.ownerName = ownerName;

		if (tag[4] == '>')
		{
			tagerxs = '</';
			if (tag[1]) tagerxs+= tag[1]+':';
			tagerxs += tag[2]+'[\\x20]*>';
			tagerx.compile(tagerxs, 'ig');
			tagend = tagerx.exec(str.substr(tag.index+tag[0].length));
			if (tagend) tagend.index+=tag.index+tag[0].length;
			ctrl['tagend'] = tagend;
			ctrl['i2'] = tagend.index+tagend[0].length;
			ctrl.contentVal = str.substring(ctrl.i1+tag[0].length, ctrl.i2-tagend[0].length);
		}

		// Save free index
		this.ReserveId(ctrl.id, ctrl.control);

		var icon;
		if (window.as_arASPXCompElements[ctrl.ownerName] && window.as_arASPXCompElements[ctrl.ownerName].icon)
			icon = window.as_arASPXCompElements[ctrl.ownerName].icon;
		else
			icon = image_path + '/component.gif';


		var bTagParams = {
			generateCID: id,
			name: attrs.name
		};

		if (this.pMainObj.bRenderComponents)
		{
			bTagParams._src = icon;
			icon = c2wait_path;
		}

		var elId = this.pMainObj.SetBxTag(false, {tag: "aspxcomponent", params: bTagParams});
		ctrl.elementId = elId;

		// Save control shadow
		this.arShadowedControls[id] = ctrl;
		this.arComponents[id] = ctrl;

		var code =  '<img id="' + elId + '" src="' + icon + '" />';
		str = str.substr(0, ctrl.i1) + code + str.substr(ctrl.i2, str.length - ctrl.i2);
	}

	// Inject back encoded expressions
	str = this.InjectExpressionsBack(str, 'XmlEncode');
	return str;
}

ASPXParser.prototype.InitRenderingSystem = function()
{
	this.sCSS = "\n"+
	".bxc2-block{border: 1px dotted #E4E4E4 !important;}\n" +
	".bxc2-block-selected{border: 1px solid #000 !important;}\n" +
	".bxc2-block *{-moz-user-select:none; cursor: default !important;}\n" +
	".bxc2-block-icon{}\n" +
	".bxc2-cont-block{padding: 4px;}\n" +
	".bxc2-del{width: 21px; height: 18px; cursor: pointer !important; background: url(" + image_path + "/c2del.gif);}\n" +
	".bxc2-move{width: 12px; height: 18px; cursor: move !important; background: url(" + image_path + "/c2move.gif) 0 1px;}\n" +
	".bxc2-block-tbl{width: 100%; height: 18px; background-color: #E4E4E4; border-collapse: collapse;}\n" +
	".bxc2-block-tbl td{padding: 0 0 0 0px; font-size: 13px; color: #404040; border-width: 0px !important; white-space: nowrap !important;}\n" +
	".bx-bogus-inp{width: 5px; position: absolute;}\n" +
	".bxc2-block-selected .bxc2-block-tbl td{font-weight: bold; color: #000; background-color: #C0C0C0;}\n";

	this.pMainObj.AddEventHandler("OnChangeView", this.COnChangeView, this);
	this.pMainObj.AddEventHandler("OnSelectionChange", this.COnSelectionChange, this);
	this.pMainObj.AddEventHandler("OnChangeContentPlaceHolder", this.COnChangeContentPlaceHolder, this);

	if (this.pMainObj.sEditorMode == 'html')
		this.COnChangeView();
}

ASPXParser.prototype.COnChangeView = function()
{
	if (this.pMainObj.sEditorMode == 'html' || (this.pMainObj.sEditorMode == 'split' && this.pMainObj.sEditorSplitMode != 'code'))
	{
		var _this = this;
		window['COnKeyDown' + this.pMainObj.name] = function(e){_this.COnKeyDown(e);};
		window['COnMouseDown' + this.pMainObj.name] = function(e){_this.COnMouseDown(e);};
		window['COnDragEnd' + this.pMainObj.name] = function(e){_this.COnDragEnd(e);};

		if (!BX.browser.IsIE())
			addAdvEvent(this.pMainObj.pEditorDocument, 'dragdrop', window['COnDragEnd' + this.pMainObj.name]);

		addAdvEvent(this.pMainObj.pEditorDocument, 'keydown', window['COnKeyDown' + this.pMainObj.name]);
		addAdvEvent(this.pMainObj.pEditorDocument, 'mousedown', window['COnMouseDown' + this.pMainObj.name]);

		this.GetRenderedContent({bAllComponents: true});
		setTimeout(function(){_this.AppendCSS(_this.sCSS);}, 5);
	}
	else
	{
		this.DeSelectComponent(false, false);
		if (this.pMainObj.oPropertiesTaskbar)
		{
			BX.cleanNode(this.pMainObj.oPropertiesTaskbar.pCellProps);
			this.pMainObj.oPropertiesTaskbar.OnSelectionChange('always', this.pMainObj.pEditorDocument.body);
		}
	}
}

ASPXParser.prototype.COnChangeContentPlaceHolder = function()
{
	if (this.pMainObj.sEditorMode == 'html' || (this.pMainObj.sEditorMode == 'split' && this.pMainObj.sEditorSplitMode != 'code'))
	{
		var _this = this;
		this.GetRenderedContent({bAllComponents: true});
		setTimeout(function(){_this.AppendCSS(_this.sCSS);}, 5);
	}
	else
	{
		this.DeSelectComponent(false, false);
		if (this.pMainObj.oPropertiesTaskbar)
		{
			BX.cleanNode(this.pMainObj.oPropertiesTaskbar.pCellProps);
			this.pMainObj.oPropertiesTaskbar.OnSelectionChange('always', this.pMainObj.pEditorDocument.body);
		}
	}
}

ASPXParser.prototype.COnMouseDown = function(e)
{
	this.__bMouseDownComp = false;
	this.pMainObj.__bMouseDownComp = false;

	var
		bDel = false,
		bMove = false,
		pElement, pElementTemp, tagName, cn;
	if (!e)
		e = this.pMainObj.pEditorWindow.event;
	if (e.target)
		pElement = e.target;
	else if (e.srcElement)
		pElement = e.srcElement;
	if (pElement.nodeType == 3) // defeat Safari bug
		pElement = pElement.parentNode;

	while(pElement && (pElementTemp = pElement.parentNode) != null)
	{
		if(pElementTemp.nodeType!=1 || !pElement.tagName)
		{
			pElement = pElementTemp;
			continue;
		}
		tagName = pElement.tagName.toLowerCase();
		cn = pElement.className;
		if (tagName == 'img' && cn.indexOf('bxc2-block-icon') != -1)
		{
			bDel = cn.indexOf('bxc2-del') != -1; // Delete
			bMove = cn.indexOf('bxc2-move') != -1; // Start drag
		}
		if(tagName == 'div' && cn.indexOf('bxc2-block') != -1)
		{
			if (bDel)
				this.DeleteComponent(pElement);
			if (bMove)
			{
				this.pDraggedElementId = pElement.id;
				break;
			}
			this.__bMouseDownComp = true;
			this.pMainObj.__bMouseDownComp = true;
			if (BX.browser.IsIE())
				this._IEClearStupidSelection(pElement);

			// Select component;
			this.SelectComponent(pElement);
			return jsUtils.PreventDefault(e);
		}
		pElement = pElementTemp;
	}
	return true;
}

ASPXParser.prototype._IEClearStupidSelection = function(pEl)
{
	try{
	//if (!BX.browser.IsIE())
	//	return;
	var
		_this = this,
		pWin = this.pMainObj.pEditorWindow,
		pDoc = this.pMainObj.pEditorDocument;

	var dd = pDoc.getElementById('dd_toggle_' + pEl.id);
	if (dd && dd.parentNode)
	{
		var inp = this.pMainObj.CreateEditorElement("INPUT", {className: 'bx-bogus-inp'});
		dd.parentNode.insertBefore(inp, dd);
		setTimeout(function(){if (inp && inp.parentNode){inp.focus(); inp.parentNode.removeChild(inp);}}, 50);
	}
	}catch(e){};
}

ASPXParser.prototype.COnDragEnd = function(e)
{
	if (!this.pDraggedElementId)
		return;

	var
		o, par, bInside, pDel,
		id = this.pDraggedElementId,
		ddid = 'dd_toggle_' + id,
		_this = this;

	// Rerender component into the dragged toggle position
	setTimeout(function()
	{
		var
			arImgs = _this.pMainObj.pEditorDocument.getElementsByTagName('IMG'),
			el, i, l = arImgs.length;

		for (i = 0; i < l; i++)
		{
			el = arImgs[i];
			if (el && el.id == ddid)
			{
				// Check parent
				o = el;
				bInside = false;
				while(par = o.parentNode)
				{
					if(par.tagName && par.tagName.toUpperCase() == 'DIV' && par.className && par.className.indexOf('bxc2-block') != -1)
					{
						if (par.id != id) // Element inside other component
							pDel = el;
						bInside = true;
						break;
					}
					o = par;
				}

				if (!bInside)
					_this.MoveRenderedComponent(el, id);
			}
		}

		if (pDel)
			pDel.parentNode.removeChild(pDel);
	}, 5);
}

ASPXParser.prototype.COnKeyDown = function(e)
{
	var pElement, pElementTemp, tn, cn, _this = this;
	if (!e)
		e = this.pMainObj.pEditorWindow.event;
	if (e.target)
		pElement = e.target;
	else if (e.srcElement)
		pElement = e.srcElement;
	if (pElement.nodeType == 3)
		pElement = pElement.parentNode;

	if (this.lastSelectedComponent)
	{
		if (e.keyCode == 27) // Esc - deselect component
		{
			_this.DeSelectComponent(el);
		}
		else if (e.keyCode == 37) // left arrow
		{
			// Deselect and focus before
		}
		else if (e.keyCode == 39) // right arrow
		{
			// Deselect and focus after
		}
		else
		{
			var el = this.lastSelectedComponent;
			try{
				this.pMainObj.SelectElement(el);
			}catch(e){}
			setTimeout(function(){
				if (el && el.parentNode)
				{
					var tbl = el.getElementsByTagName('TABLE');
					if (tbl.length <= 0)
					{
						var innerHTML = el.innerHTML.toString();
						if (innerHTML.indexOf('>') == -1) // text content
						{
							var txt = _this.pMainObj.pEditorDocument.createTextNode(innerHTML);
							el.parentNode.insertBefore(txt, el);
						}
						_this.DeleteComponent(el);
					}
				}
			}, 1);
		}
	}

	while(pElement && (pElementTemp = pElement.parentNode) != null)
	{
		if(pElementTemp.nodeType!=1 || !pElement.tagName)
		{
			pElement = pElementTemp;
			continue;
		}
		tn = pElement.tagName.toLowerCase();
		cn = pElement.className;
		if(tn == 'div' && cn.indexOf('bxc2-block') != -1)
		{
			return jsUtils.PreventDefault(e);
		}
		pElement = pElementTemp;
	}
}

ASPXParser.prototype.COnSelectionChange = function(sReloadControl)
{
	if (this.__bMouseDownComp || this.__bPreventComponentDeselect)
		return false;
	this.DeSelectComponent();
}


ASPXParser.prototype.COnDblClick = function (e)
{
	if (!this.pMainObj.bRenderComponents)
		return true;

	var pEl;
	if (!e)
		e = this.pMainObj.pEditorWindow.event;
	if (e.target)
		pEl = e.target;
	else if (e.srcElement)
		pEl = e.srcElement;
	if (pEl.nodeType == 3)
		pEl = pEl.parentNode;

	if (pEl.nodeName.toLowerCase() == 'body' && pEl.lastChild && pEl.lastChild.getAttribute)
		pEl = pEl.lastChild;

	var pBlock = BXFindParentByTagName(pEl, 'DIV', 'bxc2-block');
	if (pBlock && pBlock.getAttribute("__bxtagname") == 'aspxcomponent')
	{
		this.SelectComponent(pBlock);
		return jsUtils.PreventDefault(e);
	}
	return true;
}


ASPXParser.prototype.GetRenderedContent = function(P)
{
	if (!this.pMainObj.bRenderComponents)
		return;
	var id, ctrl, pEl;
	if (!P.bReRender)
		P.bReRender = false;

	if (P.bAllComponents) // send all editor content with all components
	{
		for (id in this.arComponents)
		{
			ctrl = this.arComponents[id];
			if (typeof ctrl != 'object')
				continue;
			this.RequestRenderedContent(id, ctrl.attributes, P.bReRender);
		}
	}
	else if (P.id) // get rendered content of the some component
	{
		ctrl = this.arComponents[P.id];
		if (ctrl && typeof ctrl == 'object')
			this.RequestRenderedContent(P.id, ctrl.attributes, P.bReRender, P.bSelect);
	}
}

ASPXParser.prototype.RequestRenderedContent = function(id, arProps, bReRender, bSelect)
{
	if (!arProps) 
		return;
	var _this = this, pEl;
	if (this.pMainObj.arConfig['path'])
		arProps.srcappvp = this.pMainObj.arConfig['path'];

	BX.ajax.post(editor_path + '/work_aspxcomp.aspx?task=render&lang='+BXLang+'&site='+BXSite+'&templateID='+((this.pMainObj.templateID) ? this.pMainObj.templateID : ''), {ctrl : arProps}, function(result)
	{
		setTimeout(function()
		{
			pEl = _this.RenderComponent(id, result, bReRender);
			if (bSelect && pEl)
			{
				// Only for FF: Clear white markers after component's icon replacing
				if (!BX.browser.IsIE())
					_this.pMainObj.pEditorDocument.execCommand('RemoveFormat', false, null);
				_this.SelectComponent(pEl);
			}
		}, 10);
	})

}

ASPXParser.prototype.RenderComponent = function(id, source, bReRender, repEl)
{
	source = source.replace(/<%--(\s|\S)*?--%>/ig, '');

	this.arComponentsSource[id] = source;
	var
		_this = this,
		pContentBlock = false, pHeader,
		ctrl = this.arComponents[id],
		title = BX_MESS.DefComp2Title,
		oEl = repEl || this.pMainObj.pEditorDocument.getElementById(ctrl.elementId);

	if (typeof source != 'string' || source.trim().length <= 0) // Component return empty result
		return this.StopWaiting(ctrl.elementId, oEl);

	if (!oEl)
		return;

	if (bReRender)
	{
		var
			arDivs = oEl.getElementsByTagName('DIV'),
			pBlock = oEl,
			i, l = arDivs.length,
			arCh = pBlock.childNodes,
			node, l2 = arCh.length;

		pBlock.style.width = null;

		for (i = 0; i < l; i++)
		{
			if (arDivs[i].className == 'bxc2-cont-block')
			{
				pContentBlock = arDivs[i];
				break;
			}
		}

		for (i = 0; i < l2; i++)
		{
			node = arCh[i];
			if (node.nodeType == 1 && node.nodeName.toUpperCase() == 'TABLE' && node.className == 'bxc2-block-tbl')
			{
				pHeader = node;
				break;
			}
		}

		if (!pContentBlock)
			return this.RenderComponent(id, source, false);
	}
	else
	{
		if (ctrl.ownerName && window.as_arASPXCompElements[ctrl.ownerName])
			title = window.as_arASPXCompElements[ctrl.ownerName].title;

		var pBlock = BX.create("DIV", {props: {id: ctrl.elementId, className: 'bxc2-block'}});
		pHeader = this.pMainObj.CreateEditorElement("TABLE", {id: id, className: 'bxc2-block-tbl'});

		var r = pHeader.insertRow(-1);
		var c0 = r.insertCell(-1); // move
		c0.style.width = '18px';
		var pMoveIcon = c0.appendChild(this.pMainObj.CreateEditorElement("IMG", {id: 'dd_toggle_' + id, src: one_gif_src, className: 'bxc2-block-icon bxc2-move'}));
		pMoveIcon.title = BX_MESS.MoveComponent;

		var c1 = r.insertCell(-1); // title
		c1.className = 'bxc2-block-title';
		c1.innerHTML = BX_MESS.Comp2Name + ': ' + title;

		var c2 = r.insertCell(-1); // Buttons block
		c2.style.textAlign = 'right';
		var pDelIcon = c2.appendChild(this.pMainObj.CreateEditorElement("IMG", {src: one_gif_src, className: 'bxc2-block-icon bxc2-del'}));
		pDelIcon.title = BX_MESS.DelComponent;

		pBlock.appendChild(pHeader);
		pContentBlock = pBlock.appendChild(this.pMainObj.CreateEditorElement("DIV", {className: 'bxc2-cont-block'}));

		oEl.parentNode.insertBefore(pBlock, oEl); // Insert rendered block
		oEl.parentNode.removeChild(oEl); // Remove yelow pill

		this.pMainObj.nLastDragNDropElement = false;
		pBlock.style.MozUserSelect = 'none'; // For mozilla

		if (BX.browser.IsIE())
		{
			pMoveIcon.ondragend = window['COnDragEnd' + this.pMainObj.name];
			pBlock.ondragend = function(){_this._IEpBlockOnDragEnd(id);};
		}
	}

	try
	{
		pContentBlock.innerHTML = source;
	}
	catch(e) //IE BUG WORKAROUND:  "Unknown runtime error" when using innerHTML
	{
		var _p = this.pMainObj.CreateEditorElement("DIV", {className: 'bxc2-cont-block'});
		_p.innerHTML = source;
		pContentBlock.parentNode.insertBefore(_p, pContentBlock);
		pContentBlock.parentNode.removeChild(pContentBlock); //
		pContentBlock = _p;
	}
	this.arComponentsSource[id] = source;

	// Set width correct width depending on content
	this.ResizeAfterRendering(pBlock, pContentBlock, pHeader);
	return pBlock;
}

ASPXParser.prototype._IEpBlockOnDragEnd = function(id)
{
	var _this = this;
	setTimeout(function()
	{
		var
			oEl = _this.pMainObj.pEditorDocument.getElementById(id),
			otherComp = BXFindParentByTagName(oEl, 'DIV', 'bxc2-block');

		if (otherComp) // Component dragget into another
		{
			var tmpIcon = _this.pMainObj.CreateEditorElement("IMG", {src: one_gif_src}); // Create simple image
			otherComp.parentNode.insertBefore(tmpIcon, otherComp); // Put element before parent component
			_this.MoveRenderedComponent(tmpIcon, id);
		}
		else //Rerender content after system dragging
		{
			_this.RenderComponent(id, _this.arComponentsSource[id], oEl);
		}
	}, 100);

}

ASPXParser.prototype.StartWaiting = function(pIcon)
{
	if (src = pIcon.getAttribute('src'))
		this.pMainObj.Add2BxTag(pIcon, {'_src': src});
	pIcon.src = c2wait_path;
}

ASPXParser.prototype.StopWaiting = function(id, repEl)
{
	var pEl = repEl || this.pMainObj.pEditorDocument.getElementById(id);
	if (pEl && pEl.nodeName.toLowerCase() == 'img')
	{
		var oTag = this.pMainObj.GetBxTag(pEl);
		if (oTag.params && oTag.params._src)
			pEl.src = oTag.params._src;
	}
	return pEl;
}

ASPXParser.prototype.ReRenderComponent = function(id)
{
	if (this.pMainObj.bRenderComponents)
		this.GetRenderedContent({id: id, bReRender: true});
}

ASPXParser.prototype.ResizeAfterRendering = function(pBlock, pContentBlock, pHeader)
{
	setTimeout(function()
	{
		var
			blockWidth = parseInt(pBlock.offsetWidth),
			arCh = pContentBlock.childNodes,
			maxWidth = 0,
			node, w, i, l = arCh.length;

		for (i = 0; i < l; i++) // For each child in content block
		{
			node = arCh[i];
			if (node && node.nodeType == 1) // If it's html element
			{
				w = parseInt(node.offsetWidth);
				if (!isNaN(w) && w > maxWidth)
					maxWidth = w;
			}
		}

		if (maxWidth > 0 && (blockWidth - maxWidth) > 20)
			pBlock.style.width = (maxWidth + 20) + 'px';
		if (maxWidth > 8)
			pContentBlock.style.width = (maxWidth - 8) + 'px';

		setTimeout(function()
		{
			if (pHeader)
			{
				var headerWidth = parseInt(pHeader.offsetWidth);
				if (!isNaN(headerWidth) && headerWidth > maxWidth)
					pBlock.style.width = (headerWidth + 20) + 'px';
				if (headerWidth > 8)
					pContentBlock.style.width = (headerWidth - 8) + 'px';
			}
		}, 20);

	}, 300);
}

ASPXParser.prototype.MoveRenderedComponent = function(el, id)
{
	var pEl = this.pMainObj.pEditorDocument.getElementById(id);
	pEl.parentNode.removeChild(pEl);
	this.RenderComponent(id, this.arComponentsSource[id], false, el);
}

ASPXParser.prototype.SelectComponent = function(pEl)
{
	if (this.lastSelectedComponent)
	{
		if (pEl.id == this.lastSelectedComponent.id) // already selected
			return;
		this.DeSelectComponent(false, false);
	}

	if (pEl.nodeName.toLowerCase() != 'img')
		pEl.className = 'bxc2-block bxc2-block-selected';
	this.lastSelectedComponent = pEl;
	this.pMainObj.SetFocus();

	var _this = this;
	setTimeout(function()
	{
		_this.pMainObj.oPropertiesTaskbar.OnSelectionChange('always', pEl);
		_this.__bPreventComponentDeselect = false;
	}, 250);
	setTimeout(function(){_this.__bMouseDownComp = false;}, 500);
	setTimeout(function(){_this.pMainObj.__bMouseDownComp = false;}, 500);
}

ASPXParser.prototype.DeSelectComponent = function(pEl, bCleanPropTaskbar)
{
	try{ // For IE permission denied stupid errors
	if (!pEl)
		pEl = this.lastSelectedComponent;
	if (!pEl || !pEl.nodeName)
		return true;
	if (pEl.nodeName.toLowerCase() != 'img')
		pEl.className = 'bxc2-block';
	this.lastSelectedComponent = false;
	if (bCleanPropTaskbar !== false)
		this.pMainObj.oPropertiesTaskbar.OnSelectionChange('always');
	}catch(e){}
}

ASPXParser.prototype.DeleteComponent = function(pEl)
{
	// TODO: Del from array
	var id = pEl.id;
	//this.arComponents[id] = null;
	//this.arShadowedControls[id] = null;
	pEl.parentNode.removeChild(pEl);
	this.lastSelectedComponent = false;
}

ASPXParser.prototype.LoadComponentCSS = function(name, params)
{
	//load CSS
	if (params['usecss'] && !this.arComponentsCSS[params['usecss']])
	{
		this.arComponentsCSS[params['usecss']] = params['usecss'];
		var
			_this = this,
			cssReq = new JCHttpRequest();
		cssReq.Action = function(styles){setTimeout(function(){_this.AddCSSToEditorFrame(_this.ExpandCssUrls(params['usecss'], styles));}, 10);};
		cssReq.Send(params['usecss'] + '?v=s' + parseInt(Math.random() * 100000)); // Request css file
	}
	return true;
}

ASPXParser.prototype.ExpandCssUrls = function(origin, styles)
{
	var i = origin.toString().lastIndexOf('/');
	if (i < 0)
		return styles;
	if (i + 1 < origin.length)
		origin = origin.slice(0, i + 1);

	var prefixes = ["http://", "https://", "file://", "ftp://"];

	return styles.replace(
		/url\(\s*'([^']+)'\s*\)|url\(\s*"([^"]+)"\s*\)|url\((\s*[^)]+\s*)\)/g,
		function(match, g0, g1, g2, offset, original)
		{
			var url = jsUtils.trim(g0 || g1 || g2);

			if (url.length == 0)
				return match;

			for(var i = prefixes.length - 1; i >= 0; i--)
			{
				if (url.length >= prefixes[i].length && url.substring(0, prefixes[i].length) == prefixes[i])
					return match;
			}

			if (url.charAt(0) == '/')
				return match;

			if (g0)
				return "url('" + origin + url + "')";
			else if (g1)
				return "url(\"" + origin + url + "\")";
			else
				return "url(" + origin + url + ")";
		}
	);
}

ASPXParser.prototype.AddCSSToEditorFrame = function(styles)
{
	if (styles.toLowerCase().indexOf('</html>') != -1) // Return if it's html page
		return false;
	this.sCSS += styles + "\n";
	this.AppendCSS(styles);
}

ASPXParser.prototype.AppendCSS = function(styles)
{
	styles = styles.trim();
	if (styles.length <= 0)
		return false;

	var
		pDoc = this.pMainObj.pEditorDocument,
		pHeads = pDoc.getElementsByTagName("HEAD");

	if(pHeads.length != 1)
		return false;

	if(BX.browser.IsIE())
	{
		setTimeout(function(){pDoc.styleSheets[0].cssText += styles;}, 5);
	}
	else
	{
		var xStyle = pDoc.createElement("STYLE");
		pHeads[0].appendChild(xStyle);
		xStyle.appendChild(pDoc.createTextNode(styles));
	}
	return true;
}

function XmlEncode(s)
{
	return s; // ???
	return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').
		replace(/"/g, '&quot;').replace(/\'/g, '&apos;');
}

function XmlDecode(s)
{
	return s; // ???
	return s.replace(/&lt;/g, '<').replace(/&gt;/g, '>').replace(/&quot;/g, '"').
		replace(/&apos;/g, '\'').replace(/&amp;/g, '&');
}

// System of previews component rendering in DOM for FX&IE
// Replacer - pair of "aspxscpstub" and "aspxscpplacer"

function isTrue(val)
{
	return val.toLowerCase() == "true";
}

function TryInvariantKey(key, params)
{
    if(typeof(key) == 'undefined') throw 'TrySetInvariantKey: key is not defined!';
    if(typeof(key) == 'function') throw 'TrySetInvariantKey: function is not valid key!';

	for (var i in params)
		if (i.toLowerCase() == key.toLowerCase())
			return (params[i]==undefined)?undefined:i;
	return undefined;
}

function TrySetInvariantKey(key, params)
{
    if(typeof(key) == 'undefined') throw 'TrySetInvariantKey: key is not defined!';
    if(typeof(key) == 'function') throw 'TrySetInvariantKey: function is not valid key!';
	for (var i in params)
		if (i.toLowerCase() == key.toLowerCase())
			return i.toLowerCase();
	return key.toLowerCase();
}

// Closure Collectors System
var closureCollector = [];

function _BXArr2Str(arObj)
{
	var l = arObj.length;
	if (!l) return '';
	var result = '';
	for(var i = 0; i < l; i++)
	{
		var s = arObj[i].toString();
		if (s == '' || jsUtils.trim(s).length != s.length || s.search(/[';]/))
			s = '\'' + s.replace(/'/g, '\'\'') + '\'';
		if (result.length != 0)
			result += ';'
		result += s;
	}
	return result;
}

function _BXStr2Arr(str)
{
	var
		result = [],
		myregexp = /'(?:''|[^'\r\n])*'|[^;\r\n]*/;
	if (str == '')
		return result;
	var m = str.match(myregexp);
	while (m != null)
	{
		var v = m[0];
		if (v.length >= 2 && v.charAt([0]) == '\'' && v.charAt(v.length - 1) == '\'')
			v = v.slice(1, -1).replace(/''/g, '\'');
		result.push(v);
		if (m.index + m[0].length < str.length)
			str = str.substring(m.index + m[0].length + 1);
		else
			break;
		m = str.match(myregexp);
	}
	return result;
}

oBXEditorUtils.addTaskBar('ASPXComponentsTaskbar', 2, BX_MESS.CompTBTitle, {bWithoutPHP: false}, 5);

window.arASPXCompTemplates = [];
window.arASPXCompProps = [];
window.arASPXCompTemplateProps = [];
window.as_arASPXCompParams = {};
window.as_arASPXCompTemplates = {};
window.as_arASPXCompElements = [];
window.arASPXCompTooltips = {};
window.arASPXCompPropGroups = {};
window.arASPXCompParamsGroups = {};
