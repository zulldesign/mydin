function ImportCss(sourceUrl)
{
  var linkElem = document.createElement('link');
  linkElem.rel='stylesheet';
  linkElem.type = 'text/css';
  linkElem.href = sourceUrl;
  document.getElementsByTagName('head')[0].appendChild(linkElem);
}

function calculateElementContentHeight(elem){
	var elmPaddingTop, elmPaddingBottom, elmBorderTopWidth, elmBorderBottomWidth, height;
	if(jsUtils.IsIE())
	{ // IE
		var elmStyle = elem.currentStyle;
		elmPaddingTop = parseInt(elmStyle.getAttribute('paddingTop'));
		elmPaddingBottom = parseInt(elmStyle.getAttribute('paddingBottom'));
	    elmBorderTopWidth = parseInt(elmStyle.getAttribute('borderTopWidth'));
	    elmBorderBottomWidth = parseInt(elmStyle.getAttribute('borderBottomWidth'));
	}
	else{
	    //W3
		var elmCompStyle = document.defaultView.getComputedStyle(elem, '');
		elmPaddingTop = parseInt(elmCompStyle.getPropertyValue('paddingTop'));
		elmPaddingBottom = parseInt(elmCompStyle.getPropertyValue('paddingBottom'));
	    elmBorderTopWidth = parseInt(elmCompStyle.getPropertyValue('borderTopWidth'));
	    elmBorderBottomWidth = parseInt(elmCompStyle.getPropertyValue('borderBottomWidth'));
	}
	var height = elem.offsetHeight - (!isNaN(elmPaddingTop) ? elmPaddingTop : 0)  - (!isNaN(elmPaddingBottom) ? elmPaddingBottom : 0) - (!isNaN(elmBorderTopWidth) ? elmBorderTopWidth : 0) - (!isNaN(elmBorderBottomWidth) ? elmBorderBottomWidth : 0);
	return height;
}

function calculateElementContentHeightById(elemId){
    var elem = document.getElementById(elemId);
    if(!elem) return 0;
    return calculateElementContentHeight(elem);
}

function setElementHeight(elem, height){
	if(jsUtils.IsIE())
	{ // IE
		//Strict Mode
		if(!(document.documentElement.clientWidth == 0)){
			var elmStyle = elem.currentStyle;
			var elmPaddingTop = parseInt(elmStyle.getAttribute('paddingTop'));
			var elmPaddingBottom = parseInt(elmStyle.getAttribute('paddingBottom'));
			var elmBorderTopWidth = parseInt(elmStyle.getAttribute('borderTopWidth'));
			var elmBorderBottomWidth = parseInt(elmStyle.getAttribute('borderBottomWidth'));
			elem.style.height = (height - elmPaddingTop - elmPaddingBottom - elmBorderTopWidth - elmBorderBottomWidth) + "px";
		}
		//Quirks Mode
		else{
			elem.style.height = height + "px";
		}
	}
	else{
		elem.style.height = height + "px";
	}
}

function trim(source){
    if(source == undefined || source == null || source.length == 0)
        return "";

	var spaceCount = 0, i = 0, result = source;
	while(i < result.length && (result.charCodeAt(i) == 9 || result.charCodeAt(i) == 10 || result.charCodeAt(i) == 13)){
		spaceCount++;
		i++;
	}
	if(spaceCount > 0)
		result = (spaceCount < result.length)? result.substring(spaceCount, result.length - 1) : "";

    if(result == null || result.length == 0)
        return "";
    spaceCount = 0;
    i = source.length - 1;
	while(i >= 0 && (result.charCodeAt(i) == 9 || result.charCodeAt(i) == 10 || result.charCodeAt(i) == 13)){
		spaceCount++;
		i--;
	}
	if(spaceCount > 0)
		result = (spaceCount < result.length)? result.substring(0, result.length - 1 - speceCount) : "";

	return result;
}


function findFirstChildNodeByTagName(source, tagName){
    if(!source || typeof(source.childNodes) != "object" || !tagName || typeof(tagName) != "string") return null;
    for(var i = 0; i < source.childNodes.length; i++){
        if(typeof(source.childNodes[i].tagName) == "undefined") continue;
        if(source.childNodes[i].tagName == tagName) return source.childNodes[i];
    }
    return null;
}

function findFirstChild(source){
    if(!source || typeof(source.childNodes) != "object") return null;
    for(var i = 0; i < source.childNodes.length; i++){
        if(typeof(source.childNodes[i].tagName) != "undefined") return source.childNodes[i];
    }
    return null;
}

function findPreviousSibling(source){
    if(!source || typeof(source.previousSibling) == "undefined") return null;
    var result = source.previousSibling;
    while(result){
        if(typeof(result.tagName) != "undefined") break;
        result = result.previousSibling;
    }
    return result;
}

function findNextSibling(source){
    if(!source || typeof(source.previousSibling) == "undefined") return null;
    var result = source.nextSibling;
    while(result){
        if(typeof(result.tagName) != "undefined") break;
        result = result.nextSibling;
    }
    return result;
}

var jsPopup = {
    container_div_id: 'bx_popup_form_div',
    title_div_id: 'bx_popup_title',
    title_div_class_name: 'bx-popup-title',
    description_container_div_id: 'bx_popup_description_container',
    description_container_div_class_name: 'bx-popup-description-container',
    description_div_id: 'bx_popup_description',
    description_div_class_name: 'bx-popup-description',
    content_wrapper_div_id: 'bx_popup_content',
    content_wrapper_div_class_name: 'bx-popup-content-alt',
    content_wrapper_div_class_name_std: 'bx-popup-content',
    _content_wrapper_div_class_name_custom: null,
    content_div_id: 'bx_popup_content_container',
    content_div_class_name: 'bx-popup-content-container-alt',
    content_div_class_name_std: 'bx-popup-content-container',
    overlay_id: 'bx_popup_overlay',
    form_name: 'bx-popup-form',
    class_name: 'bx-popup-form',
    close_button_title: 'Закрыть окно',
    buttons_div_id: "bx_popup_buttons",
    buttons_div_class_name: "bx-popup-buttons",


    loaded: false,
    url: '',
    arParams: null,

    bDenyClose: false,
    _bDenyShow: false,
    _useStandardStyles: false,
    __arRuntimeResize: {},

    bodyOverflow: "",
    currentScroll: 0,

    div: null,
    div_inner: null,

    x: 0,
    y: 0,

    error_dy: null,

    normalLayoutSettings: {
        "contentWrapper": {
            "style": {
                "width": null,
                "height": null,
                "margin": null,
                "zIndex": null,
                "position": null
            }
        },
        "container": {
            "style": {
                "left": 0,
                "top": 0
            }
        },
        "title": {
            "style": { "display": "" }
        },
        "description": {
            "style": { "display": "" }
        },
        "buttons": {
            "style": { "display": "" }
        }
    },

    _isFullSceen: false,

    UseStandardStyles: function(flag) {
        this._useStandardStyles = flag;
    },

    UseCustomContentWrapperStyle: function(className) {
        this._content_wrapper_div_class_name_custom = className;
    },

    DenyClose: function() {
        this.bDenyClose = true;
    },

    AllowClose: function() {
        this.bDenyClose = false;
    },

    DenyShowDialog: function() {
        this._bDenyShow = true;
    },

    AllowShowDialog: function() {
        this._bDenyShow = false;
    },

    __OnKeyPress: function(e) // do not use 'this': will be called in other context
    {
        if (!e) e = window.event
        if (!e) return;
        if (jsPopup.bDenyClose) return;
        if (e.keyCode == 27) {
            jsUtils.removeEvent(document, "keypress", jsPopup.__OnKeyPress);
            jsPopup.CloseDialog();
        }
    },

    _getElementHeightWithMargins: function(el) {
        if (!el)
            return 0;
        var rect = Bitrix.ElementPositioningUtility.getElementRect(el);
        var margins = Bitrix.ElementPositioningUtility.getElementMargins(el);
        return rect.height + margins.top + margins.bottom;
    },

    _getCloseButtonId: function() {
        return this.title_div_id + '_close_btn';
    },

    __AjaxAction: function(result) // do not use 'this': will be called in other context
    {
        CloseWaitWindow();
        //if(result == undefined || result == null)
        //	throw "Invalid result";

        if (jsPopup._bDenyShow) {
            jsPopup.CloseDialog();
            return;
        }

        if (typeof (window['bitrixDialogData']) == "undefined") {
            jsPopup.CloseDialog();
            window.alert("Dialog data is not defined!");
            throw "Dialog data is not defined!";
        }

        if (bitrixDialogData == null) {
            jsPopup.CloseDialog();
            window.alert("Dialog data is not assigned!");
            throw "Dialog data is not assigned!";
        }

        if (typeof (bitrixDialogData.sections) == "undefined") {
            jsPopup.CloseDialog();
            window.alert("Dialog sections is not defined!");
            throw "Dialog sections is not defined";
        }

        if (bitrixDialogData.sections == null) {
            jsPopup.CloseDialog();
            window.alert("Dialog sections is not assigned!");
            throw "Dialog sections is not assigned!";
        }

        var dlgTitle = null;
        if (typeof (bitrixDialogData.sections.title) != "undefined" && typeof (bitrixDialogData.sections.title.innerHTML) != "undefined")
            dlgTitle = bitrixDialogData.sections.title.innerHTML;


        var dlgDescription = null;
        if (typeof (bitrixDialogData.sections.description) != "undefined" && typeof (bitrixDialogData.sections.description.innerHTML) != "undefined")
            dlgDescription = bitrixDialogData.sections.description.innerHTML;

        dlgDescription = trim(dlgDescription);

        var dlgContent = null;
        if (typeof (bitrixDialogData.sections.content) != "undefined" && typeof (bitrixDialogData.sections.content.innerHTML) != "undefined") {
            dlgContent = bitrixDialogData.sections.content.innerHTML;
            //cleanup
            dlgContent = dlgContent.replace(/\s*<title[^>]*>[\s\S]*<\/title>/gi, "");
        }

        var dlgFrames = null;
        if (typeof (bitrixDialogData.sections.frame) != "undefined" && typeof (bitrixDialogData.sections.frame.innerHTML) != "undefined")
            dlgFrames = bitrixDialogData.sections.frame.innerHTML;

        var dlgButtons = null;
        if (typeof (bitrixDialogData.sections.buttonPanel) != "undefined" && typeof (bitrixDialogData.sections.buttonPanel.innerHTML) != "undefined")
            dlgButtons = bitrixDialogData.sections.buttonPanel.innerHTML;

        var dlgFormName = typeof (dlgContent) == "string" && dlgContent.length > 0 ? dlgContent.match(/<form[^>]*name\s*=\s*"(\w+)"[^>]*>/i) : null;
        //if(dlgFormName == null || dlgFormName.length < 2)
        //    throw "Could not find form name!";
        jsPopup.form_name = dlgFormName != null && dlgFormName.length > 1 ? dlgFormName[1] : null;


        var container_div = document.getElementById(jsPopup.container_div_id);

        if (container_div == null) {
            container_div = document.body.appendChild(document.createElement("DIV"));
            container_div.id = jsPopup.container_div_id;
            container_div.className = jsPopup.class_name;
            container_div.style.position = 'absolute';
            container_div.style.zIndex = 1020;
        }

        var frame_div = document.getElementById("bx_popup_frame");
        if (frame_div == null) {
            frame_div = document.body.appendChild(document.createElement("DIV"));
            frame_div.id = "bx_popup_frame";
            frame_div.style.display = "none";
            frame_div.style.width = "0pt";
            frame_div.style.height = "0pt";
        }

        frame_div.innerHTML = dlgFrames;

        var title_tab_row_td_pane_id = jsPopup.title_div_id + '_pane';

        var title_div = document.getElementById(jsPopup.title_div_id);
        if (title_div == null) {
            title_div = container_div.appendChild(document.createElement("DIV"));
            title_div.id = jsPopup.title_div_id;
            title_div.className = jsPopup.title_div_class_name;
            var title_tab = title_div.appendChild(document.createElement("TABLE"));
            title_tab.className = 'bx-width100';
            title_tab.cellspacing = 0;
            //title_tab.style.backgroundColor = "black";
            //title_tab.id = "popup_ttl_tab";

            var title_tab_body = title_tab.appendChild(document.createElement("TBODY"));

            var title_tab_row = title_tab_body.appendChild(document.createElement("TR"));
            var title_tab_row_td_pane = title_tab_row.appendChild(document.createElement("TD"));
            title_tab_row_td_pane.id = title_tab_row_td_pane_id;
            title_tab_row_td_pane.className = 'bx-width100 bx-title-text';
            title_tab_row_td_pane.innerHTML = dlgTitle;
            //document.getElementById(title_tab_row_td_pane_id).setAttribute('onMouseDown', 'jsFloatDiv.StartDrag(arguments[0], document.getElementById(\'' + jsPopup.container_div_id + '\'));');
            jsUtils.addEvent(title_tab_row_td_pane, "mousedown", function() { jsFloatDiv.StartDrag(arguments[0], document.getElementById(jsPopup.container_div_id)); }, null);

            var title_tab_row_td_button = title_tab_row.appendChild(document.createElement("TD"));
            title_tab_row_td_button.className = 'bx-width0';

            var title_tab_row_td_button_close = title_tab_row_td_button.appendChild(document.createElement("A"));
            var title_tab_row_td_button_close_id = jsPopup._getCloseButtonId();
            title_tab_row_td_button_close.id = title_tab_row_td_button_close_id;
            title_tab_row_td_button_close.className = 'bx-popup-close';
            title_tab_row_td_button_close.title = jsPopup.close_button_title;
            title_tab_row_td_button_close.href = 'javascript:void(0)';

            jsUtils.addEvent(title_tab_row_td_button_close, "click", function() { jsPopup.CloseDialog(); }, null);
            //title_tab_row_td_button_close.setAttribute('onClick', 'return jsPopup.CloseDialog();');
        }
        else {
            var title_tab_row_td_pane = document.getElementById(title_tab_row_td_pane_id);
            if (title_tab_row_td_pane == null)
                throw "Could not find title pane!";
            title_tab_row_td_pane.innerHTML = dlgTitle;
        }

        var description_container_div = document.getElementById(jsPopup.description_container_div_id);
        if (description_container_div == null) {
            description_container_div = container_div.appendChild(document.createElement("DIV"));
            description_container_div.id = jsPopup.description_container_div_id;
            description_container_div.className = jsPopup.description_container_div_class_name;

            var description_div = description_container_div.appendChild(document.createElement("DIV"));
            description_div.id = jsPopup.description_div_id;
            description_div.className = jsPopup.description_div_class_name;
            description_div.innerHTML = dlgDescription;
        }
        else {
            var description_div = document.getElementById(jsPopup.description_div_id);
            if (description_div == null)
                throw "Could not find description div!";
            description_div.innerHTML = dlgDescription;
        }


        //show || hide emty description
        description_container_div.style.display = dlgDescription != null && dlgDescription.length > 0 ? "block" : "none";

        var content_wrapper_div = document.getElementById(jsPopup.content_wrapper_div_id);
        if (content_wrapper_div == null) {
            content_wrapper_div = container_div.appendChild(document.createElement("DIV"));
            content_wrapper_div.id = jsPopup.content_wrapper_div_id;
            content_wrapper_div.style.overflowX = "hidden";
            content_wrapper_div.style.overflowY = "auto";
        }


        var content_div = document.getElementById(jsPopup.content_div_id);
        if (content_div == null) {
            content_div = content_wrapper_div.appendChild(document.createElement("DIV"));
            content_div.id = jsPopup.content_div_id;
            content_div.className = jsPopup._useStandardStyles ? jsPopup.content_div_class_name_std : jsPopup.content_div_class_name;
            content_div.innerHTML = dlgContent;
        }

        var buttons_div = document.getElementById(jsPopup.buttons_div_id);
        if (buttons_div == null) {
            buttons_div = container_div.appendChild(document.createElement("DIV"));
            buttons_div.id = jsPopup.buttons_div_id;
            buttons_div.className = jsPopup.buttons_div_class_name;
        }

        buttons_div.innerHTML = dlgButtons;

        if (jsPopup.arParams.height && jsPopup.arParams.height > 0)
            container_div.style.height = jsPopup.arParams.height + 'px';

        if (jsPopup.arParams.width && jsPopup.arParams.width > 0)
            container_div.style.width = jsPopup.arParams.width + 'px';

        if (jsPopup._content_wrapper_div_class_name_custom != null) {
            content_wrapper_div.className = jsPopup._content_wrapper_div_class_name_custom;
            jsPopup._content_wrapper_div_class_name_custom = null;
        }
        else
            content_wrapper_div.className = jsPopup._useStandardStyles ? jsPopup.content_wrapper_div_class_name_std : jsPopup.content_wrapper_div_class_name;

        var containerHeight = jsPopup._getElementHeightWithMargins(container_div);
        var titleHeight = jsPopup._getElementHeightWithMargins(title_div);
        var descriptionHeight = jsPopup._getElementHeightWithMargins(description_container_div);
        var buttonsHeight = jsPopup._getElementHeightWithMargins(buttons_div);

        setElementHeight(content_wrapper_div, (containerHeight - titleHeight - descriptionHeight - buttonsHeight - 1));
        var windowSize = jsUtils.GetWindowInnerSize();
        var windowScroll = jsUtils.GetWindowScrollPos();

        var left = parseInt(windowScroll.scrollLeft + windowSize.innerWidth / 2 - container_div.offsetWidth / 2);
        var top = parseInt(windowScroll.scrollTop + windowSize.innerHeight / 2 - container_div.offsetHeight / 2);

        jsFloatDiv.Show(container_div, left, top, 5, true);
        jsUtils.addEvent(document, "keypress", jsPopup.__OnKeyPress);

        jsPopup.div = container_div;
        jsPopup.div_inner = document.getElementById(jsPopup.content_wrapper_div_id);

        if (jsPopup.arParams.resize && null != jsPopup.div && null != jsPopup.div_inner)
            jsPopup.createResizer();

        jsPopup.__EvalDialogDataScripts();
        jsPopup.__ActivateDialogDataStyles();

        jsPopup._isOpened = true;
        setTimeout(function() { jsPopup.loaded = true; }, 300);

        return container_div;
    },

    __EvalDialogDataScripts: function() {
        if (typeof (window['bitrixDialogData']) == "undefined")
            throw "Could not get dialog data!";
        if (typeof (bitrixDialogData.scripts) != "undefined" && bitrixDialogData.scripts != null && bitrixDialogData.scripts.length > 0) {
            for (var i = 0; i < bitrixDialogData.scripts.length; i++) {
                var script = bitrixDialogData.scripts[i];
                if (script.src)
                    jsExtLoader.EvalExternal(script.src);
                else if (script.text)
                    jsExtLoader.EvalGlobal(script.text);
            }
        }
    },

    __ActivateDialogDataStyles: function() {
        if (typeof (window['bitrixDialogData']) == "undefined")
            throw "Could not get dialog data!";

        if (typeof (bitrixDialogData.externalStylesheets) != "undefined" && bitrixDialogData.externalStylesheets != null && bitrixDialogData.externalStylesheets.length > 0) {
            var head = document.getElementsByTagName("HEAD");
            if (head != null && head.length != 0)
                head = head[0];
            if (head != null) {
                for (var i = 0; i < bitrixDialogData.externalStylesheets.length; i++) {
                    var link = document.createElement("LINK");
                    head.appendChild(link);
                    link.type = "text/css";
                    link.rel = "stylesheet";
                    link.title = "jsPopupLink2Stylesheet";
                    link.href = bitrixDialogData.externalStylesheets[i];
                }
            }
        }
    },

    __DeactivateDialogStyles: function() {
        var head = document.getElementsByTagName("HEAD");
        if (head != null && head.length != 0)
            head = head[0];
        if (head != null) {
            var links = document.getElementsByTagName("LINK");
            if (links != null && links.length > 0) {
                var i = 0;
                while (i < links.length) {
                    if (typeof (links[i].title) == "undefined" || links[i].title != "jsPopupLink2Stylesheet") {
                        i++;
                        continue;
                    }
                    head.removeChild(links[i]);
                }
            }
        }
    },

    HideDialogContent: function() {
        var container_div = document.getElementById(jsPopup.container_div_id);
        if (container_div == null)
            return;

        var content_wrapper_div = document.getElementById(jsPopup.content_wrapper_div_id);
        if (content_wrapper_div == null)
            return;

        content_wrapper_div.style.display = "none";
        jsPopup.AdjustShadow();
    },


    StretchDialogContentOnFullScreen: function(fullSceen) {
        if (jsPopup._isFullSceen == fullSceen)
            return;

        jsPopup._isFullSceen = fullSceen;

        if (!jsPopup.div)
            return;


        if (fullSceen) {
            jsPopup.normalLayoutSettings.container.style.left = parseInt(jsPopup.div.style.left);
            jsPopup.normalLayoutSettings.container.style.top = parseInt(jsPopup.div.style.top);

            if (jsPopup.normalLayoutSettings.container.style.left != 0 || jsPopup.normalLayoutSettings.container.style.top != 0) {
                jsFloatDiv.Move(jsPopup.div, -1 * jsPopup.normalLayoutSettings.container.style.left, -1 * jsPopup.normalLayoutSettings.container.style.top);
            }


            var title_div = document.getElementById(jsPopup.title_div_id);
            if (title_div) {
                jsPopup.normalLayoutSettings.title.style.display = title_div.style.display;
                title_div.style.display = "none";
            }

            var description_container_div = document.getElementById(jsPopup.description_container_div_id);
            if (description_container_div) {
                jsPopup.normalLayoutSettings.description.style.display = description_container_div.style.display;
                description_container_div.style.display = "none";
            }
            //var content_wrapper_div = document.getElementById(jsPopup.content_wrapper_div_id);

            var buttons_div = document.getElementById(jsPopup.buttons_div_id);
            if (buttons_div) {
                jsPopup.normalLayoutSettings.buttons.style.display = buttons_div.style.display;
                buttons_div.style.display = "none";
            }
            var contentWrapper = document.getElementById(jsPopup.content_wrapper_div_id);
            if (contentWrapper == null) throw "Could not find content wrappper with ID = " + jsPopup.content_wrapper_div_id;

            jsPopup.normalLayoutSettings.contentWrapper.style.width = contentWrapper.style.width;
            jsPopup.normalLayoutSettings.contentWrapper.style.height = contentWrapper.style.height;
            jsPopup.normalLayoutSettings.contentWrapper.style.margin = contentWrapper.style.margin;
            jsPopup.normalLayoutSettings.contentWrapper.style.zIndex = contentWrapper.style.zIndex;
            jsPopup.normalLayoutSettings.contentWrapper.style.position = contentWrapper.style.position;

            var ws = jsUtils.GetWindowInnerSize();

            contentWrapper.style.position = "absolute";
            contentWrapper.style.zIndex = "1900";
            contentWrapper.style.width = parseInt(ws.innerWidth) + "px";
            contentWrapper.style.height = parseInt(ws.innerHeight) + "px";
            contentWrapper.style.margin = "0px";

            contentWrapper.scrollIntoView(true);
            jsPopup.RemoveOverlay();
        }
        else {
            if (jsPopup.normalLayoutSettings.container.style.left != 0 || jsPopup.normalLayoutSettings.container.style.top != 0) {
                jsFloatDiv.Move(jsPopup.div, jsPopup.normalLayoutSettings.container.style.left, jsPopup.normalLayoutSettings.container.style.top);
            }
            var contentWrapper = document.getElementById(jsPopup.content_wrapper_div_id);
            if (contentWrapper == null) throw "Could not find content wrappper with ID = " + jsPopup.content_wrapper_div_id;

            contentWrapper.style.width = jsPopup.normalLayoutSettings.contentWrapper.style.width;
            contentWrapper.style.height = jsPopup.normalLayoutSettings.contentWrapper.style.height;
            contentWrapper.style.margin = jsPopup.normalLayoutSettings.contentWrapper.style.margin;
            contentWrapper.style.zIndex = jsPopup.normalLayoutSettings.contentWrapper.style.zIndex;
            contentWrapper.style.position = jsPopup.normalLayoutSettings.contentWrapper.style.position;

            var title_div = document.getElementById(jsPopup.title_div_id);
            if (title_div)
                title_div.style.display = jsPopup.normalLayoutSettings.title.style.display;

            var description_container_div = document.getElementById(jsPopup.description_container_div_id);
            if (description_container_div)
                description_container_div.style.display = jsPopup.normalLayoutSettings.description.style.display;

            //var content_wrapper_div = document.getElementById(jsPopup.content_wrapper_div_id);

            var buttons_div = document.getElementById(jsPopup.buttons_div_id);
            if (buttons_div)
                buttons_div.style.display = jsPopup.normalLayoutSettings.buttons.style.display;

            //contentWrapper.scrollIntoView(true);
            jsPopup.CreateOverlay();
        }
    },

    SetAllButtonsDisabled: function(disabled) {
        var buttonDiv = document.getElementById(jsPopup.buttons_div_id);
        if (buttonDiv == null)
            throw "Could not find button panel div with ID '" + jsPopup.buttons_div_id + "'!";

        var buttons = buttonDiv.getElementsByTagName("input");
        if (buttons.length == 0) return;
        for (var i = 0; i < buttons.length; i++) {
            if (!buttons[i].type || buttons[i].type != "button") continue;
            buttons[i].disabled = disabled;
        }
    },

    __AjaxPostAction: function(result) // do not use 'this': will be called in other context
    {
        CloseWaitWindow();

        if (jsPopup._bDenyShow) {
            jsPopup.CloseDialog();
            return;
        }

        if (result == undefined || result == null) {
            //CloseWaitWindow();
            throw "Invalid result";
        }

        if (!jsPopup.loaded)
            return;

       if (typeof (window['bitrixDialogData']) == "undefined" || !bitrixDialogData) {
			jsPopup.CloseDialog();
			return;
       }

        if (typeof (bitrixDialogData.sections) == "undefined") {
            jsPopup.CloseDialog();
            window.alert("Dialog sections is not defined!");
            throw "Dialog sections is not defined";
        }

        if (bitrixDialogData.sections == null) {
            jsPopup.CloseDialog();
            window.alert("Dialog sections is not assigned!");
            throw "Dialog sections is not assigned!";
        }

        var dlgTitle = null;
        if (typeof (bitrixDialogData.sections.title) != "undefined" && typeof (bitrixDialogData.sections.title.innerHTML) != "undefined")
            dlgTitle = bitrixDialogData.sections.title.innerHTML;

        var dlgDescription = null;
        if (typeof (bitrixDialogData.sections.description) != "undefined" && typeof (bitrixDialogData.sections.description.innerHTML) != "undefined")
            dlgDescription = bitrixDialogData.sections.description.innerHTML;

        dlgDescription = trim(dlgDescription);

        var dlgContent = null;
        if (typeof (bitrixDialogData.sections.content) != "undefined" && typeof (bitrixDialogData.sections.content.innerHTML) != "undefined") {
            dlgContent = bitrixDialogData.sections.content.innerHTML;
            //cleanup
            dlgContent = dlgContent.replace(/\s*<title[^>]*>[\s\S]*<\/title>/gi, "");
        }

        var title_tab_row_td_pane_id = jsPopup.title_div_id + '_pane';
        var title_tab_row_td_pane = document.getElementById(title_tab_row_td_pane_id);
        if (title_tab_row_td_pane == undefined || title_tab_row_td_pane == null)
            throw "Could not find element '" + title_tab_row_td_pane_id + "'!";

        if (dlgTitle != null && dlgTitle.length > 0)
            title_tab_row_td_pane.innerHTML = dlgTitle;

        var description_div = document.getElementById(jsPopup.description_div_id);
        if (description_div == null)
            throw "Could not find description div!";

        if (dlgTitle != null && dlgTitle.length > 0)
            description_div.innerHTML = dlgDescription;

        var description_container_div = document.getElementById(jsPopup.description_container_div_id);
        if (description_container_div == null)
            throw "Could not find description container div!";
        //show || hide emty description
        description_container_div.style.display = dlgDescription != null && dlgDescription.length > 0 ? "block" : "none";

        var content_div = document.getElementById(jsPopup.content_div_id);
        if (content_div == undefined || title_tab_row_td_pane == null)
            throw "Could not find element '" + jsPopup.content_div_id + "'!";

        if (dlgContent != null && dlgContent.length > 0)
            content_div.innerHTML = dlgContent;

        var titleHeight = jsPopup._getElementHeightWithMargins(document.getElementById(jsPopup.title_div_id));
        var descriptionHeight = jsPopup._getElementHeightWithMargins(document.getElementById(jsPopup.description_container_div_id));
        var buttonsHeight = jsPopup._getElementHeightWithMargins(document.getElementById(jsPopup.buttons_div_id));
        var contentWrapperHeight = jsPopup._getElementHeightWithMargins(document.getElementById(jsPopup.content_wrapper_div_id));

        var container_div = document.getElementById(jsPopup.container_div_id);
        if (container_div == null)
            throw "Could not find element '" + jsPopup.container_div_id + "'!";

        setElementHeight(container_div, (titleHeight + descriptionHeight + contentWrapperHeight + buttonsHeight + 1));
        jsPopup.AdjustShadow();

        if (jsPopup.arParams.resize && null != jsPopup.div && null != jsPopup.div_inner)
            jsPopup.createResizer();

        jsPopup.__EvalDialogDataScripts();
        jsPopup.__ActivateDialogDataStyles();
    },

    HandlePostCompletion: function() {
        if (!jsPopup.loaded)
            return;

        var dlgFrame = document.getElementById('bx_dialog_form_target');
        if (dlgFrame == null)
            throw "Could not find form target frame!";

        var dlgFrameDoc = typeof (dlgFrame.contentDocument) != 'undefined' ? dlgFrame.contentDocument : dlgFrame.contentWindow.document;
        if (typeof (dlgFrameDoc) == 'undefined' || dlgFrameDoc == null)
            throw "Could not find dialog frame document!";
        var responseText = dlgFrameDoc.documentElement.innerHTML;

        //if(responseText == null || responseText.length == 0)
        //	return;

        var arCode = [];

        if (responseText != null && responseText.length > 0) {
            var rxScriptsAllRoughly = /<script[^>]*>[\S\s]*?<\/script>/gi;
            var rxScriptsRemoteFine = /<script[^>]*src\s*=\s*[\'\"]([^\"\']+)[\'\"][^>]*>/i;
            var rxScriptsLocalFine = /<script[^>]*>\s*(\/\/<\!\[CDATA\[)?\s*([\S\s]+?)\s*(\/\/\]\]>)?\s*<\/script>/i;

            var m = null, cap = null, count = -1, runFirstFlg = false, cnt = null;

            if ((count = (m = responseText.match(rxScriptsAllRoughly)) != null ? m.length : 0) > 0) {
                for (var i = 0; i < count; i++) {
                    cap = m[i];
                    runFirstFlg = cap.indexOf('bxrunfirst') != '-1';

                    if ((cnt = rxScriptsRemoteFine.exec(cap)) != null) {
                        arCode[arCode.length] = { "bRunFirst": runFirstFlg, "isInternal": false, "JS": cnt[1] };
                        continue;
                    }
                    if ((cnt = rxScriptsLocalFine.exec(cap)) != null) {
                        arCode[arCode.length] = { "bRunFirst": runFirstFlg, "isInternal": true, "JS": cnt[2] };
                        continue;
                    }
                    throw "Could not parse script '" + cap + "'! Please validate regexps...";
                }
            }
        }
        jsExtLoader.obContainer = null;
        jsExtLoader.onajaxfinish = jsPopup.__AjaxPostAction;
        jsExtLoader.processResult(responseText, arCode);
    },


    /*
    arParams = {
    width: window width in px
    height: window height in px
    resize: true|false - flag showing whether window is resizable
    min_width: min window width while resizing (250 by def.)
    min_height: min window height while resizing (200 by def.)
    }
    */
    ShowDialog: function(url, arParams) {
        if (document.getElementById(this.container_div_id))
            this.CloseDialog();

        if (null == arParams) arParams = {};

        if (null == arParams.resize) arParams.resize = true;
        if (null == arParams.min_width) arParams.min_width = 250;
        if (null == arParams.min_height) arParams.min_height = 200;

        var pos = url.indexOf('?');
        if (pos == -1)
            url += "?mode=public";
        else
            url = url.substring(0, pos) + "?mode=public&" + url.substring(pos + 1);

        jsPopup.check_url = pos == -1 ? url : url.substring(0, pos);

        if (arParams.resize && null != jsPopup.__arRuntimeResize[jsPopup.check_url]) {
            arParams.width = jsPopup.__arRuntimeResize[jsPopup.check_url].width;
            arParams.height = jsPopup.__arRuntimeResize[jsPopup.check_url].height;

            //console.log(url);
            var ipos = url.indexOf('bxpiheight');
            //console.log(ipos);
            if (ipos == -1)
                url += (pos == -1 ? '?' : '&') + 'bxpiheight=' + jsPopup.__arRuntimeResize[jsPopup.check_url].iheight;
            else
                url = url.substring(0, ipos) + 'bxpiheight=' + jsPopup.__arRuntimeResize[jsPopup.check_url].iheight;

            //console.log(url);
        }

        this.url = url;
        this.arParams = arParams;

        this.CreateOverlay();

        //zg
        this._bDenyShow = false;

        jsExtLoader.onajaxfinish = this.__AjaxAction;
        jsExtLoader.start(url);
    },

    RemoveOverlay: function() {
        var overlay = document.getElementById(this.overlay_id);

        if (overlay)
            overlay.parentNode.removeChild(overlay);

        jsUtils.removeEvent(window, "resize", this.OverlayResize);
    },

    OverlayResize: function(event) {
        var overlay = document.getElementById(jsPopup.overlay_id);

        if (!overlay)
            return;

        var windowSize = jsUtils.GetWindowScrollSize();

        overlay.style.width = windowSize.scrollWidth + "px";
    },

    CreateOverlay: function() {
        var opacity = new COpacity();

        if (!opacity.GetOpacityProperty())
            return;

        //Create overlay
        var overlay = document.body.appendChild(document.createElement("DIV"));
        overlay.className = "bx-popup-overlay";
        overlay.id = this.overlay_id;

        var windowSize = jsUtils.GetWindowScrollSize();

        overlay.style.width = windowSize.scrollWidth + "px";
        overlay.style.height = windowSize.scrollHeight + "px";

        jsUtils.addEvent(window, "resize", this.OverlayResize);
    },

    CloseDialog: function() {
        if (typeof (window['bitrixDialogData']) != "undefined") {
            bitrixDialogData = null;
        }

        //console.log("***CloseDialog_1");
        jsUtils.onCustomEvent('OnBeforeCloseDialog');
        //console.log("***CloseDialog_2");

        if (this.bDenyClose)
            return false;

        jsUtils.removeEvent(document, "keypress", jsPopup.__OnKeyPress);
        var div = document.getElementById(jsPopup.container_div_id);
        if (div != null) {
            jsFloatDiv.Close(div);
            div.parentNode.removeChild(div);
        }

        jsPopup.RemoveOverlay();
        jsPopup.loaded = false;

        //zg
        if (typeof (window['Type']) == 'function' && typeof (Type.isClass) == 'function' && typeof (Bitrix) == 'object' && Type.isClass(Bitrix.AspnetFormDispatcher)) {
            Bitrix.AspnetFormDispatcher.get_instance().handlePopupDialogClose();
        }

        jsPopup.__DeactivateDialogStyles();

        return true;
    },

    GetParameters: function(form_name) {
        if (null == form_name)
            var form = document.forms[this.form_name];
        else
            var form = document.forms[form_name];

        if (!form)
            return "";

        var i, s = "";
        var n = form.elements.length;

        var delim = '';
        for (i = 0; i < n; i++) {
            if (s != '') delim = '&';

            var el = form.elements[i];
            if (el.disabled)
                continue;

            switch (el.type.toLowerCase()) {
                case 'text':
                case 'textarea':
                case 'password':
                case 'hidden':
                    if (null == form_name && el.name.substr(el.name.length - 4) == '_alt' && form.elements[el.name.substr(0, el.name.length - 4)])
                        break;
                    s += delim + el.name + '=' + jsUtils.urlencode(el.value);
                    break;
                case 'radio':
                    if (el.checked)
                        s += delim + el.name + '=' + jsUtils.urlencode(el.value);
                    break;
                case 'checkbox':
                    s += delim + el.name + '=' + jsUtils.urlencode(el.checked ? 'Y' : 'N');
                    break;
                case 'select-one':
                    var val = "";
                    if (null == form_name && form.elements[el.name + '_alt'] && el.selectedIndex == 0)
                        val = form.elements[el.name + '_alt'].value;
                    else
                        val = el.value;
                    s += delim + el.name + '=' + jsUtils.urlencode(val);
                    break;
                case 'select-multiple':
                    var j;
                    var l = el.options.length;
                    for (j = 0; j < l; j++)
                        if (el.options[j].selected)
                        s += delim + el.name + '=' + jsUtils.urlencode(el.options[j].value);
                    break;
                default:
                    break;
            }
        }

        if (null != jsPopup.arParams && jsPopup.arParams.resize && jsPopup.div_inner) {
            var inner_width = parseInt(jsPopup.div_inner.style.width);
            var inner_height = parseInt(jsPopup.div_inner.style.height);

            if (inner_width > 0)
                s += '&bxpiwidth=' + inner_width;
            if (inner_height > 0)
                s += '&bxpiheight=' + inner_height;
        }

        return s;
    },

    PostParameters: function(params) {
        jsExtLoader.onajaxfinish = jsPopup.__AjaxPostAction

        ShowWaitWindow();

        var url = jsPopup.url;
        if (null != params) {
            index = url.indexOf('?')
            if (index == -1)
                url += '?' + params;
            else
                url = url.substring(0, index) + '?' + params + "&" + url.substring(index + 1);
        }

        jsExtLoader.startPost(url, jsPopup.GetParameters());
    },

    PostParametersOld: function(params) {
        CHttpRequest.Action = jsPopup.__AjaxPostAction;

        ShowWaitWindow();

        var url = jsPopup.url;
        if (null != params) {
            index = url.indexOf('?')
            if (index == -1)
                url += '?' + params;
            else
                url = url.substring(0, index) + '?' + params + "&" + url.substring(index + 1);
        }

        CHttpRequest.Post(url, jsPopup.GetParameters());
    },

    AdjustShadow: function() {
        if (jsPopup.div)
            jsFloatDiv.AdjustShadow(jsPopup.div);
    },

    HideShadow: function() {
        if (jsPopup.div)
            jsFloatDiv.HideShadow(jsPopup.div);
    },

    UnhideShadow: function() {
        if (jsPopup.div)
            jsFloatDiv.UnhideShadow(jsPopup.div);
    },

    DragPanel: function(event, td) {
        var div = jsUtils.FindParentObject(td, 'div');
        div.style.left = div.offsetLeft + 'px';
        div.style.top = div.offsetTop + 'px';
        jsFloatDiv.StartDrag(event, div);
    },

    // ************* resizers ************* //
    createResizer: function() {
        jsPopup.diff_x = null;
        jsPopup.diff_y = null;

        jsPopup.arPos = jsUtils.GetRealPos(jsPopup.div);

        var zIndex = parseInt(jsUtils.GetStyleValue(jsPopup.div, jsUtils.IsIE() ? 'zIndex' : 'z-index')) + 1;

        jsPopup.obResizer = document.createElement('DIV');
        jsPopup.obResizer.className = 'bxresizer';

        jsPopup.obResizer.style.position = 'absolute';
        jsPopup.obResizer.style.zIndex = zIndex;

        jsPopup.obResizer.onmousedown = jsPopup.startResize;

        jsPopup.div.appendChild(jsPopup.obResizer);
    },

    startResize: function(e) {
        if (!e)
            e = window.event;

        jsPopup.wndSize = jsUtils.GetWindowScrollPos();
        jsPopup.wndSize.innerWidth = jsUtils.GetWindowInnerSize().innerWidth;

        jsPopup.x = e.clientX + jsPopup.wndSize.scrollLeft;
        jsPopup.y = e.clientY + jsPopup.wndSize.scrollTop;

        jsPopup.obDescr = document.getElementById('bx_popup_description_container');

        if (jsUtils.IsIE()) {
            jsPopup.arPos = jsPopup.div.getBoundingClientRect();
            jsPopup.arPos = {
                left: jsPopup.arPos.left + jsPopup.wndSize.scrollLeft,
                top: jsPopup.arPos.top + jsPopup.wndSize.scrollTop,
                right: jsPopup.arPos.right + jsPopup.wndSize.scrollLeft,
                bottom: jsPopup.arPos.bottom + jsPopup.wndSize.scrollTop
            }
            jsPopup.arPosInner = jsPopup.div_inner.getBoundingClientRect();
            jsPopup.arPosInner = {
                left: jsPopup.arPosInner.left + jsPopup.wndSize.scrollLeft,
                top: jsPopup.arPosInner.top + jsPopup.wndSize.scrollTop,
                right: jsPopup.arPosInner.right + jsPopup.wndSize.scrollLeft,
                bottom: jsPopup.arPosInner.bottom + jsPopup.wndSize.scrollTop
            }
        }
        else {
            jsPopup.arPos = jsUtils.GetRealPos(jsPopup.div);
            jsPopup.arPosInner = jsUtils.GetRealPos(jsPopup.div_inner);
        }

        document.onmouseup = jsPopup.stopResize;
        jsUtils.addEvent(document, "mousemove", jsPopup.doResize);

        if (document.body.setCapture)
            document.body.setCapture();

        var b = document.body;
        b.ondrag = jsUtils.False;
        b.onselectstart = jsUtils.False;
        b.style.MozUserSelect = jsPopup.div.style.MozUserSelect = 'none';
        b.style.cursor = jsPopup.obResizer.style.cursor;

        jsPopup.HideShadow();
    },

    doResize: function(e) {
        if (!e)
            e = window.event;

        var x = e.clientX + jsPopup.wndSize.scrollLeft;
        var y = e.clientY + jsPopup.wndSize.scrollTop;

        if (jsPopup.x == x && jsPopup.y == y || x > jsPopup.wndSize.innerWidth + jsPopup.wndSize.scrollLeft - 10)
            return;

        jsPopup.Resize(x, y);
        jsPopup.x = x;
        jsPopup.y = y;
    },

    Resize: function(x, y) {
        if (null == jsPopup.diff_x) {
            jsPopup.diff_x = jsPopup.div.offsetWidth - jsPopup.div_inner.offsetWidth;
            jsPopup.diff_y = jsPopup.div.offsetHeight - jsPopup.div_inner.offsetHeight;
        }

        var new_width = x - jsPopup.arPos.left;
        var new_height = y - jsPopup.arPos.top;

        //var dx = new_width - jsPopup.div.offsetWidth;
        //var dy = y - jsPopup.y;

        if (null != jsPopup.obDescr)
            var descrHeight = jsPopup.obDescr.offsetHeight;

        if (new_width > jsPopup.arParams.min_width) {
            jsPopup.div.style.width = new_width + 'px';
            jsPopup.div_inner.style.width = (new_width - jsPopup.diff_x) + 'px';
        }

        if (null != jsPopup.obDescr)
            var dy = jsPopup.obDescr.offsetHeight - descrHeight;
        else
            var dy = 0;

        jsPopup.diff_y += dy;

        if (new_height > jsPopup.arParams.min_height) {
            jsPopup.div_inner.style.height = (new_height - jsPopup.diff_y) + 'px';
            jsPopup.div.style.height = new_height + 'px';
        }

        if (jsUtils.IsIE())
            jsPopup.AdjustShadow();
    },

    stopResize: function() {
        if (document.body.releaseCapture)
            document.body.releaseCapture();

        jsUtils.removeEvent(document, "mousemove", jsPopup.doResize);

        document.onmouseup = null;

        var b = document.body;
        b.ondrag = null;
        b.onselectstart = null;
        b.style.MozUserSelect = jsPopup.div.style.MozUserSelect = '';
        b.style.cursor = '';

        if (typeof (document["selection"]) == "object") {
            var sel = document.selection;
            if (sel) {
                if (typeof (sel["clear"]) == "function")
                    selection.clear();
                if (typeof (sel["empty"]) == "function")
                    selection.empty();
            }
        }
        else if (typeof (window["getSelection"]) == "function") { //ff
            var sel = window.getSelection();
            if (sel && typeof (sel["removeAllRanges"]) == "function")
                sel.removeAllRanges();
        }

        jsPopup.UnhideShadow();
        jsPopup.AdjustShadow();

        jsPopup.SavePosition()
    },

    SavePosition: function() {
        var arPos = {
            width: parseInt(jsPopup.div.style.width),
            height: parseInt(jsPopup.div.style.height),
            iheight: parseInt(jsPopup.div_inner.style.height)
        };

        if (null != jsPopup.error_dy)
            arPos.iheight += jsPopup.error_dy;

        jsUserOptions.SaveOption('jsPopup', 'size_' + jsPopup.check_url, 'width', arPos.width);
        jsUserOptions.SaveOption('jsPopup', 'size_' + jsPopup.check_url, 'height', arPos.height);
        jsUserOptions.SaveOption('jsPopup', 'size_' + jsPopup.check_url, 'iheight', arPos.iheight);

        jsPopup.__arRuntimeResize[jsPopup.check_url] = arPos;
    },


    IncludePrepare: function() {
        var obFrame = window.frames.editor;
        if (null == obFrame)
            return false;

        var obSrcForm = obFrame.document.forms.inner_form;
        var obDestForm = document.forms[this.form_name];

        if (null == obSrcForm || null == obDestForm)
            return false;

        obDestForm.include_data.value = obSrcForm.filesrc_pub.value;

        return true;
    },

    ShowError: function(error_text) {
        CloseWaitWindow();
        jsPopup.AllowClose();

        jsPopup.obDescr = document.getElementById('bx_popup_description_container');
        if (null != jsPopup.obDescr) {
            var descrHeight = jsPopup.obDescr.offsetHeight;

            var obError = document.getElementById('bx_popup_description_error');
            if (null == obError) {
                obError = document.createElement('P');
                obError.id = 'bx_popup_description_error';
                jsPopup.obDescr.firstChild.appendChild(obError);
            }

            obError.innerHTML = '<font class="errortext">' + error_text + '</font>';

            if (jsPopup.obDescr.offsetHeight != descrHeight) {
                jsPopup.error_dy = jsPopup.obDescr.offsetHeight - descrHeight;

                if (jsPopup.div_inner)
                    jsPopup.div_inner.style.height = (parseInt(jsUtils.GetStyleValue(jsPopup.div_inner, 'height')) - jsPopup.error_dy) + 'px';
            }
        }
        else
            alert(error_text);
    }
}

function COpacity(element)
{
	this.element = element;
	this.opacityProperty = this.GetOpacityProperty();

	this.startOpacity = null;
	this.finishOpacity = null;
	this.delay = 30;

	this.currentOpacity = null;
	this.fadingTimeoutID = null;
}


COpacity.prototype.SetElementOpacity = function(opacity)
{
	if (!this.opacityProperty)
		return false;

	if (this.opacityProperty == "filter")
	{
		opacity = opacity * 100;
		var alphaFilter = this.element.filters['DXImageTransform.Microsoft.alpha'] || this.element.filters.alpha;
		if (alphaFilter)
			alphaFilter.opacity = opacity;
		else
			this.element.style.filter += "progid:DXImageTransform.Microsoft.Alpha(opacity="+opacity+")";
	}
	else
		this.element.style[this.opacityProperty] = opacity;

	return true;
}

COpacity.prototype.GetOpacityProperty = function()
{
	if (typeof document.body.style.opacity == 'string')
		return 'opacity';
	else if (typeof document.body.style.MozOpacity == 'string')
		return 'MozOpacity';
	else if (typeof document.body.style.KhtmlOpacity == 'string')
		return 'KhtmlOpacity';
	else if (document.body.filters && navigator.appVersion.match(/MSIE ([\d.]+);/)[1]>=5.5)
		return 'filter';

	return false;
}

COpacity.prototype.Fading = function(startOpacity, finishOpacity, callback)
{
	if (!this.opacityProperty)
		return;

	this.startOpacity = startOpacity;
	this.finishOpacity = finishOpacity;
	this.currentOpacity = this.startOpacity;

	if (this.fadingTimeoutID)
		clearInterval(this.fadingTimeoutID);

	var _this = this;
	this.fadingTimeoutID = setInterval(function () {_this.Run(callback)}, this.delay);
}

COpacity.prototype.Run = function(callback)
{
	this.currentOpacity = Math.round((this.currentOpacity + 0.1*(this.finishOpacity - this.startOpacity > 0 ? 1: -1) )*10) / 10;
	this.SetElementOpacity(this.currentOpacity);

	if (this.currentOpacity == this.startOpacity || this.currentOpacity == this.finishOpacity)
	{
		clearInterval(this.fadingTimeoutID);
		if (typeof(callback) == "function")
			callback(this);
	}
}

COpacity.prototype.Undo = function()
{
}

// this object can be used to load any pages with huge scripts structure via AJAX
var jsExtLoader = {
    obContainer: null,
    obContainerInner: null,

    url: '',

    httpRequest: null,
    httpRequest2: null, // for Opera bug fix

    obTemporary: null,

    onajaxfinish: null,

    obFrame: null,

    start: function(url) {
        this.url = url;

        this.obContainer = null;

        ShowWaitWindow();

        this.httpRequest = this._CreateHttpObject();
        this.httpRequest.onreadystatechange = jsExtLoader.stepOne;

        this.httpRequest.open("GET", this.url, true);
        this.httpRequest.send("");
    },

    startPost: function(url, data) {
        this.url = url;
        this.obContainer = null;

        ShowWaitWindow();

        this.httpRequest = this._CreateHttpObject();
        this.httpRequest.onreadystatechange = jsExtLoader.stepOne;

        this.httpRequest.open("POST", this.url, true);
        this.httpRequest.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
        this.httpRequest.send(data);
    },

    post: function(form_name) {
        var obForm = document.forms[form_name];
        if (null == obForm)
            return;

        if (null == this.obFrame) {
            if (jsUtils.IsIE())
                this.obFrame = document.createElement('<iframe src="javascript:void(0)" name="frame_' + form_name + '">');
            else {
                this.obFrame = document.createElement('IFRAME');
                this.obFrame.name = 'frame_' + form_name;
                this.obFrame.src = 'javascript:void(0)';
            }

            this.obFrame.style.display = 'none';

            document.body.appendChild(this.obFrame);
        }

        obForm.target = this.obFrame.name;

        if (obForm.action.length <= 0)
            obForm.action = this.url;

        jsPopup.DenyClose();
        ShowWaitWindow();

        obForm.save.click();

        if (false === obForm.BXReturnValue) {
            jsPopup.AllowClose();
            CloseWaitWindow();
        }

        obForm.BXReturnValue = true;
    },

    urlencode: function(s) {
        return escape(s).replace(new RegExp('\\+', 'g'), '%2B');
    },

    __prepareOnload: function() {
        this.obTemporary = window.onload;
        window.onload = null;
    },

    __runOnload: function() {
        if (window.onload) window.onload();
        window.onload = this.obTemporary;
        this.obTemporary = null;
    },

    stepOne: function() {
        if (jsExtLoader.httpRequest.readyState == 4) {
            var content = jsExtLoader.httpRequest.responseText;
            var arCode = [];
            var matchScript;

            var regexp = new RegExp('<script([^>]*)>', 'i');
            var regexp1 = new RegExp('src=["\']([^"\']+)["\']', 'i');

            while ((matchScript = content.match(regexp)) !== null) {
                var end = content.search('<\/script>', 'i');
                if (end == -1)
                    break;

                var bRunFirst = matchScript[1].indexOf('bxrunfirst') != '-1';

                var matchSrc;
                if ((matchSrc = matchScript[1].match(regexp1)) !== null)
                    arCode[arCode.length] = { "bRunFirst": bRunFirst, "isInternal": false, "JS": matchSrc[1] };
                else {
                    var start = matchScript.index + matchScript[0].length;
                    var js = content.substr(start, end - start);

                    if (false && arCode.length > 0 && arCode[arCode.length - 1].isInternal && arCode[arCode.length - 1].bRunFirst == bRunFirst)
                        arCode[arCode.length - 1].JS += "\r\n\r\n" + js;
                    else
                        arCode[arCode.length] = { "bRunFirst": bRunFirst, "isInternal": true, "JS": js };
                }

                content = content.substr(0, matchScript.index) + content.substr(end + 9);
            }

            //alert(arCode.length);

            jsExtLoader.__prepareOnload();
            jsExtLoader.processResult(content, arCode);
            CloseWaitWindow();
            jsExtLoader.__runOnload();
        }
    },

    EvalGlobal: function(script) {
        if (typeof (window['Type']) == 'function' && typeof (Type.isClass) == 'function' && typeof (Bitrix) == 'object' && Type.isClass(Bitrix.AspnetFormDispatcher)) {
            
            var args = Bitrix.PopupDialogScriptPreEvaluteArgs.create(script);
            Bitrix.AspnetFormDispatcher.get_instance().handlePopupDialogScriptPreEvalute(args);
            if (args.getCancel()) {
                return;
            }
            if (args.isChanged()) {
                script = args.getContent();
            }
        }
        if (window.execScript)
            window.execScript(script, 'javascript');
        else if (jsUtils.IsSafari())
            window.setTimeout(script, 0);
        else
            window.eval(script);
    },

    arLoadedScripts: [],

    __isScriptLoaded: function(script_src) {
        for (var i = 0; i < jsExtLoader.arLoadedScripts.length; i++)
            if (jsExtLoader.arLoadedScripts[i] == script_src) return true;
        return false;
    },

    // evaluate external script
    EvalExternal: function(script_src) {
        if (jsExtLoader.__isScriptLoaded(script_src)) return;

        jsExtLoader.arLoadedScripts.push(script_src);

        //if (script_src.substring(0, 8) != '/bitrix/')
        //	script_src = '/bitrix/admin/' + script_src;

        // fix Opera bug with combining syncronous and asynchronuos requests using one XHR object.
        if (jsUtils.IsOpera()) {
            if (null == this.httpRequest2)
                this.httpRequest2 = this._CreateHttpObject();

            httpRequest = this.httpRequest2;
        }
        else {
            var httpRequest = this.httpRequest;
        }

        httpRequest.onreadystatechange = function(str) { };
        httpRequest.open("GET", script_src, false);
        httpRequest.send("");

        var s = httpRequest.responseText;

        httpRequest = null;

        try {
            this.EvalGlobal(s);
        }
        catch (e) {
            alert('script_src: ' + script_src + '<pre>' + s + '</pre>');
        }
    },

    processResult: function(content, arCode)
    {
        //Javascript
        jsExtLoader.processScripts(arCode, true);

        if (jsUtils.IsSafari())
            window.setTimeout(function() {
                if (null == jsExtLoader.obContainer)
                    jsExtLoader.obContainer = jsExtLoader.onajaxfinish(content);
                else
                    jsExtLoader.obContainer.innerHTML = content;
            }, 0);
        else {
            if (null == jsExtLoader.obContainer)
                jsExtLoader.obContainer = jsExtLoader.onajaxfinish(content);
            else
                jsExtLoader.obContainer.innerHTML = content;
        }
        //Javascript
        jsExtLoader.processScripts(arCode, false);
    },

    processScripts: function(arCode, bRunFirst) {
        for (var i = 0, length = arCode.length; i < length; i++) {
            if (arCode[i].bRunFirst != bRunFirst)
                continue;

            if (arCode[i].isInternal) {
                arCode[i].JS = arCode[i].JS.replace('<!--', '');
                jsExtLoader.EvalGlobal(arCode[i].JS);
            }
            else {
                jsExtLoader.EvalExternal(arCode[i].JS);
            }
        }
    },

    _CreateHttpObject: function() {
        var obj = null;
        if (window.XMLHttpRequest) {
            try { obj = new XMLHttpRequest(); } catch (e) { }
        }
        else if (window.ActiveXObject) {
            try { obj = new ActiveXObject("Microsoft.XMLHTTP"); } catch (e) { }
            if (!obj)
                try { obj = new ActiveXObject("Msxml2.XMLHTTP"); } catch (e) { }
        }
        return obj;
    }
}

/*
public jsStyle - external CSS manager
*/
var jsStyle = {

	arCSS: {},
	bInited: false,

	httpRequest: null,

	Init: function()
	{
		var arStyles = document.getElementsByTagName('LINK');
		if (arStyles.length > 0)
		{
			for (var i = 0; i<arStyles.length; i++)
			{
				if (arStyles[i].href)
				{
					var filename = arStyles[i].href;
					var pos = filename.indexOf('://');
					if (pos != -1)
						filename = filename.substr(filename.indexOf('/', pos + 3));

					arStyles[i].bxajaxflag = false;
					this.arCSS[filename] = arStyles[i];
				}
			}
		}

		this.bInited = true;
	},

	Load: function(filename)
	{
		if (!this.bInited) this.Init();

		if (null != this.arCSS[filename])
		{
			this.arCSS[filename].disabled = false;
			return;
		}

		var link = document.createElement("STYLE");
		link.type = 'text/css';

		var head = document.getElementsByTagName("HEAD")[0];
		head.insertBefore(link, head.firstChild);
		//head.appendChild(link);

		if (jsUtils.IsIE())
		{
			link.styleSheet.addImport(filename);
		}
		else
		{
			try
			{
				if (null == this.httpRequest)
					this.httpRequest = jsExtLoader._CreateHttpObject();

				this.httpRequest.onreadystatechange = null;

				this.httpRequest.open("GET", filename, false); // make *synchronous* request for css source
				this.httpRequest.send("");

				var s = this.httpRequest.responseText;

				link.appendChild(document.createTextNode(s));
			}
			catch (e) {}
		}
	},

	Unload: function(filename)
	{
		if (!this.bInited) this.Init();

		if (null != this.arCSS[filename])
		{
			this.arCSS[filename].disabled = true;
		}
	},

	UnloadAll: function()
	{
		if (!this.bInited) this.Init();
		else
			for (var i in this.arCSS)
			{
				if (this.arCSS[i].bxajaxflag)
					this.Unload(i);
			}
	}
}

// for compatibility with IE 5.0 browser
if (![].pop)
{
	Array.prototype.pop = function()
	{
		if (this.length <= 0) return false;
		var element = this[this.length-1];
		delete this[this.length-1];
		this.length--;
		return element;
	}

	Array.prototype.shift = function()
	{
		if (this.length <= 0) return false;
		var tmp = this.reverse();
		var element = tmp.pop();
		this.prototype = tmp.reverse();
		return element;
	}

	Array.prototype.push = function(element)
	{
		this[this.length] = element;
	}
}
//************************************************************
jsWizardButton = function() {
	this.initialized = false;
	this.id = "";
	this.wizard = null;
	this.inputID = "";
}
jsWizardButton.prototype = {
	initialize: function(id, inputID, wizard){
		this.id = id;
		this.inputID = inputID;
		this.wizard = wizard;
		
		var input = this.getInput();
		if(input)
			Bitrix.EventUtility.addEventListener(input, "click", Bitrix.TypeUtility.createDelegate(this, this.onInputClick));
		this.initialized = true;
	},
	getID: function() { return this.id; },
	getInput: function() { return document.getElementById(this.inputID); },
	hasInput: function() { var el = this.getInput(); return el != undefined && el != null; },
	setDisabled: function(disabled) { var el = this.getInput(); if(el) el.disabled = disabled; },
	isDisabled: function(){ var el = this.getInput(); return !el || el.disabled; },
	onInputClick: function() { this.wizard._OnButtonClick(this.id); }
}
jsWizardButton.create = function(id, inputID, wizard) {
	var self = new jsWizardButton();
	self.initialize(id, inputID, wizard);
	return self;
}

jsWizardStep = function() {
	this.initialized = false;
	this.id = this.elID = "";
	this.navigationData = {};
	this.wizard = null;
}

jsWizardStep.prototype = {
	initialize: function(id, elID, navigationData, wizard){
		this.id = id;
		this.elID = elID;
		if(navigationData && typeof(navigationData) == "object") this.navigationData = navigationData;
		this.wizard = wizard;
		this.initialized = true;
	},	
	getID: function() { return this.id; },
	getElement: function() { return document.getElementById(this.elID); },
	getNavigationData: function() { return this.navigationData; },
	handleButtonClick: function(buttonID){
		var n = "on" + buttonID;
		var d = this.navigationData;
		var callback = n in d && typeof(d[n]) == "function" ? d[n] : null;
		return callback ? callback(this.wizard) !== false : true;		
	},
	handleShow: function(){
		var d = this.navigationData;
		var callback = "onshow" in d && typeof(d["onshow"]) == "function" ? d["onshow"] : null;
		if (callback) callback(this.wizard);		
	},
	getTargetStepID: function(buttonID){ return this.navigationData[buttonID]; }
}

jsWizardStep.create = function(id, elID, navigationData, wizard){
	var self = new jsWizardStep();
	self.initialize(id, elID, navigationData, wizard);
	return self;
}


jsWizard = function() {
	this.initialized = false;
	this.name = "";
	this.currentStep = null;
	this.firstStep = null;
	this.arSteps = {};
	this.stepCount = 0;
	this.nextButtonID = "btn_popup_next";
	this.prevButtonID = "btn_popup_prev";
	this.finishButtonID = "btn_popup_finish";
	this.cancelButtonID = "btn_popup_cancel";
	this.arButtons = {};
}

jsWizard.prototype = {
	Initialize: function(name){
		this.name = name;
		this.initialized = true;
	},
	GetName: function(){ return this.name; },
	AddStep: function(stepID, navigationData) {
		var step = jsWizardStep.create(stepID, stepID, navigationData, this);
		this.arSteps[stepID] = step;
		this.stepCount++;
		if (this.firstStep == null) this.firstStep = stepID;		
	},
	SetCurrentStep: function(stepID) { this.currentStep = stepID; },
	SetFirstStep: function(stepID) { this.firstStep = stepID; },
	SetNextButtonID: function(id) { this.nextButtonID = id; },
	SetPrevButtonID: function(id) { this.prevButtonID = id; },
	SetFinishButtonID: function(id) { this.finishButtonID = id; },
	SetCancelButtonID: function(id) { this.cancelButtonID = id; },
	SetButtonDisabled: function(button, disabled) { if (this.arButtons[button]) this.arButtons[button].setDisabled(disabled); },
	IsStepExists: function(stepID) { return stepID in this.arSteps; },
	GetStepCount: function(){ return this.stepCount; },
	Display: function() {
		if (this.firstStep == null) return;
		this.currentStep = this.firstStep;

		var buttons = { "next":this.nextButtonID, "prev":this.prevButtonID, "finish":this.finishButtonID, "cancel":this.cancelButtonID };
		for (var button in buttons) {
			var b = jsWizardButton.create(button, buttons[button], this);
			this.arButtons[button] = b.hasInput() ? b : null;
		}
		this._OnStepShow();
	},
	_OnButtonClick: function(button) {
		if (this.currentStep in this.arSteps && !this.arSteps[this.currentStep].handleButtonClick(button))
			return;

		if (!this.arSteps[this.currentStep])
		{
			if (!this.arSteps[this.firstStep])
				return;

			this.currentStep = this.firstStep;
		}
		else {
			var targetStepID = this.arSteps[this.currentStep].getTargetStepID(button);
			if (targetStepID) this.currentStep = targetStepID;
		}

		this._OnStepShow();
	},
	_OnStepShow: function() {
		for (var stepID in this.arSteps) {
			var el = this.arSteps[stepID].getElement();
			if(el) el.style.display = (stepID == this.currentStep ? "" : "none");
		}

		if (this.currentStep in this.arSteps) {
			var cur = this.arSteps[this.currentStep];
			for (var button in this.arButtons) {
				var b = this.arButtons[button];
				if(!(b && b.hasInput())) continue;
				var stepID = cur.getTargetStepID(button);
				b.setDisabled(stepID && this.arSteps[stepID] ? false : true);
			}
			cur.handleShow();
		}
	}	
}

jsWizard._entries = null;
jsWizard._last = null;
jsWizard.get = function(name){
	return this._entries && name in this._entries ? this._entries[name] : null;
}
jsWizard.create = function(name){
	if(!this._entries) this._entries = {};
	var self = this._last = new jsWizard();
	self.Initialize(name);
	this._entries[name] = self;
	return self;
}
jsWizard.remove = function(name){
	if(!(this._entries && name in this._entries)) return;
	delete this._entries[name];
}
jsWizard.last = function(){ return this._last; }
if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
