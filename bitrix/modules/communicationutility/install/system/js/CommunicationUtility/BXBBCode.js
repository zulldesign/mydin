/* This code is used internally in Bitrix API and is not intended for public use. This file is subject to change. */

if(typeof(Bitrix) == "undefined"){
	var Bitrix = new Object();
}

Bitrix.BBCodeTag = { 
	b: 		    1,	//Bold 
	i: 		    2, 	//Italic
	u: 		    3,	//Underline
	s: 		    4,	//Strike
	quote:	    5,
	code:	    6,
	a:		    7,	//hyperlink
	img:        8,
	imgupload:  9,
	video:      10,
	audio:      11,
	color:	    12,
	list:	    13,
	cut:	    14
};

Bitrix.BBCodeUtility = function Bitrix$BBCodeUtility(){
    if(typeof(Bitrix.BBCodeUtility.initializeBase) == "function")
        Bitrix.BBCodeUtility.initializeBase(this);
	this._initialized = false;
	this._imagePasteDialogCloseHandler = Bitrix.TypeUtility.createDelegate(this, this._handleImagePasteDialogClose);
	this._moviePasteDialogCloseHandler = Bitrix.TypeUtility.createDelegate(this, this._handleMoviePasteDialogClose);
	this._audioPasteDialogCloseHandler = Bitrix.TypeUtility.createDelegate(this, this._handleAudioPasteDialogClose);
	this._linkPasteDialogCloseHandler = Bitrix.TypeUtility.createDelegate(this, this._handleLinkPasteDialogClose);
	this._codePasteDialogCloseHandler = Bitrix.TypeUtility.createDelegate(this, this._handleCodePasteDialogClose);	
}

Bitrix.BBCodeInsertMode = {
	replace:		1, //replace selected
	insertAfter:	2
}; 

Bitrix.BBCodeInsertCursorPosition = {
	beforeCloseTag:	1, 
	afterCloseTag:	2
};

Bitrix.BBCodeUtility.prototype = {
    initialize: function() {
        this._initialized = true;
        this._urlRx = new RegExp("^(http|https|news|ftp|aim|mailto)\:\/\/", "i");
        this._palette = null;
        this._strings = {
            UrlTitle: "My site",
            UrlPromptUrl: "Enter full address (URL)",
            UrlPromptTitle: "Enter site name",
            ImgPromptUrl: "Enter full image address (URL)",
            ListFirst: "First Element",
            ListSecond: "Second Element",
            CutTitle: "More...",
            Default: "Default"
        };
    },

    setStrings: function(strings) {
        this._strings = strings;
    },

	createOptionElement: function(value, title)
	{
		var o = document.createElement('OPTION');
		o.text = title;
		o.value = value;
		return o;
	},
	textAreaSelectionToDialogOptions: function(selection, options)
	{
	    options["_BXBBCode_TextOriginal"] = selection.text;
        options["_BXBBCode_TextSelectionStart"] = selection.start;
        options["_BXBBCode_TextSelectionEnd"] = selection.end;
        options["_BXBBCode_TextSelectionCursorPos"] = "cursorPos" in selection ? selection.cursorPos : selection.end;			
	},
	textAreaSelectionFromDialogOptions: function(selection, options)
	{
	    selection.text = options["_BXBBCode_TextOriginal"];
        selection.start = options["_BXBBCode_TextSelectionStart"];
        selection.end = options["_BXBBCode_TextSelectionEnd"];
        selection.cursorPos = options["_BXBBCode_TextSelectionCursorPos"];			
	},	
    insertInTextArea: function(tag, textArea, args) {
        if (!(args instanceof Array))
            args = new Array();

        switch (tag) {
            case Bitrix.BBCodeTag.b:
                this.insertInTextAreaCustomTag("b", textArea, args);
                break;
            case Bitrix.BBCodeTag.i:
                this.insertInTextAreaCustomTag("i", textArea, args);
                break;
            case Bitrix.BBCodeTag.u:
                this.insertInTextAreaCustomTag("u", textArea, args);
                break;
            case Bitrix.BBCodeTag.s:
                this.insertInTextAreaCustomTag("s", textArea, args);
                break;
            case Bitrix.BBCodeTag.quote:
                this.insertInTextAreaCustomTag("quote", textArea, args);
                break;
            case Bitrix.BBCodeTag.code:
                {
					var dlg = Bitrix.Dialog.get("BXBBCode$Code");
                    if (!dlg) {
						dlg = Bitrix.CodePasteDialog.create("BXBBCode$Code", "BXBBCode$Code", window.COMMUNICATION_UTILITY_BBCODE_MSG["Code.DialogTitle"], options);                        
						dlg.addCloseListener(this._codePasteDialogCloseHandler);						
                    }
					else if(dlg.isOpened()) return;

                    var textAreaSelection = Bitrix.TextAreaSelection.create(textArea);
                    var currentTextAreaSelection = textAreaSelection.getSelection();
					var options = new Object();
					options["_BXBBCode_CustomTagTextArea"] = textArea;
					options["_BXBBCode_CustomTagArgs"] = args;
					options["_langOpts"] = [
						this.createOptionElement('text', 'Plain Text'),
						this.createOptionElement('csharp', 'C#'),
						this.createOptionElement('vb', 'Visual Basic'),
						this.createOptionElement('cpp', 'C++'),
						this.createOptionElement('sql', 'SQL'),
						this.createOptionElement('xml', 'XML/HTML'),
						this.createOptionElement('jscript', 'JavaScript'),
						this.createOptionElement('css', 'CSS'),
						this.createOptionElement('python', 'Python'),
						this.createOptionElement('ruby', 'Ruby'),
						this.createOptionElement('php', 'PHP'),
						this.createOptionElement('java', 'Java'),
						this.createOptionElement('delphi', 'Delphi')
					];					
					options["_lang"] = "text";
					options["_text"] = currentTextAreaSelection.text;	
				    //FOR IE
					this.textAreaSelectionToDialogOptions(currentTextAreaSelection, options);					
					dlg.setOptions(options);
                    dlg.open();
                    break;					                
                }
                break;
            case Bitrix.BBCodeTag.cut:
                this.insertInTextAreaCustomTag("cut", textArea, args);
                break;
            case Bitrix.BBCodeTag.color:
                {
                    if (args.length == 0)
                        args[0] = "#000000";
                    this.insertInTextAreaCustomTag("color", textArea, args);
                    break;
                }
            case Bitrix.BBCodeTag.a:
                {
					var dlg = Bitrix.Dialog.get("BXBBCode$Link");
                    if (!dlg) {
						dlg = Bitrix.LinkPasteDialog.create("BXBBCode$Link", "BXBBCode$Link", window.COMMUNICATION_UTILITY_BBCODE_MSG["Link.DialogTitle"], null);                        
						dlg.addCloseListener(this._linkPasteDialogCloseHandler);
                    }
					else if(dlg.isOpened()) return;
					
                    var textAreaSelection = Bitrix.TextAreaSelection.create(textArea);
                    var currentTextAreaSelection = textAreaSelection.getSelection();
                    var textOriginal = currentTextAreaSelection.text;
					
                    var url = "http://";
                    var siteName = this._strings.UrlTitle;
					if(Bitrix.TypeUtility.isNotEmptyString(textOriginal)){
						if (textOriginal.search(this._urlRx) >= 0)
							url = textOriginal;
						else
							siteName = textOriginal;
					}
					var options = new Object();
					options["_BXBBCode_CustomTagTextArea"] = textArea;
					options["_BXBBCode_CustomTagArgs"] = args;
					options["_url"] = url;
					options["_text"] = siteName;
					//FOR IE
					this.textAreaSelectionToDialogOptions(currentTextAreaSelection, options);									
					dlg.setOptions(options);
                    dlg.open();
                    break;
                }
            case Bitrix.BBCodeTag.img:
                {
					var dlg = Bitrix.Dialog.get("BXBBCode$Image");
                    if (!dlg) {
						dlg = Bitrix.ImagePasteDialog.create("BXBBCode$Image", "BXBBCode$Image", window.COMMUNICATION_UTILITY_BBCODE_MSG["Image.DialogTitle"], Bitrix.ImagePasteDialogMode.url, options);                        
						dlg.addCloseListener(this._imagePasteDialogCloseHandler);
                    }
					else if(dlg.isOpened()) return;
					
                    var textAreaSelection = Bitrix.TextAreaSelection.create(textArea);
                    var currentTextAreaSelection = textAreaSelection.getSelection();
                    var textOriginal = currentTextAreaSelection.text;

					var url = "http://";
                    var mode = Bitrix.BBCodeInsertMode.insertAfter;
                    if (textOriginal.search(this._urlRx) >= 0) {
                        url = textOriginal;
                        mode = Bitrix.BBCodeInsertMode.replace;
                    }

                    var options = new Object();
                    options["_BXBBCode_CustomTagTextArea"] = textArea;
                    options["_BXBBCode_CustomTagArgs"] = args;
                    options["_BXBBCode_CustomTagMode"] = mode;
                    if ("options" in Bitrix.BBCodeUtility) {
                        if ("contextParams" in Bitrix.BBCodeUtility.options)
                            options["_contextRequestParams"] = Bitrix.BBCodeUtility.options.contextParams;
                        if ("imageHandlerPath" in Bitrix.BBCodeUtility.options)
                            options["_handlerPath"] = Bitrix.BBCodeUtility.options.imageHandlerPath;
                    }
					options["_imageUrl"] = url;
				    //FOR IE
					this.textAreaSelectionToDialogOptions(currentTextAreaSelection, options);					
					dlg.setOptions(options);
                    dlg.open();
                    break;
                }
            case Bitrix.BBCodeTag.imgupload:
                {
					var dlg = Bitrix.Dialog.get("BXBBCode$ImageUpload");
                    if (!dlg) {
                        dlg = Bitrix.ImagePasteDialog.create("BXBBCode$ImageUpload", "BXBBCode$ImageUpload", window.COMMUNICATION_UTILITY_BBCODE_MSG["ImageUpload.DialogTitle"], Bitrix.ImagePasteDialogMode.file, null);
                        dlg.addCloseListener(this._imagePasteDialogCloseHandler);
                    }
					else if(dlg.isOpened()) return;
					
                    var options = new Object();
                    options["_BXBBCode_CustomTagTextArea"] = textArea;
                    options["_BXBBCode_CustomTagArgs"] = args;
                    if ("options" in Bitrix.BBCodeUtility) {
                        if ("contextParams" in Bitrix.BBCodeUtility.options)
                            options["_contextRequestParams"] = Bitrix.BBCodeUtility.options.contextParams;
                        if ("imageHandlerPath" in Bitrix.BBCodeUtility.options)
                            options["_handlerPath"] = Bitrix.BBCodeUtility.options.imageHandlerPath;
                    }
                    
				    //FOR IE
                    var textAreaSelection = Bitrix.TextAreaSelection.create(textArea);
                    var currentTextAreaSelection = textAreaSelection.getSelection();				    
					this.textAreaSelectionToDialogOptions(currentTextAreaSelection, options);                    
					dlg.setOptions(options);
                    dlg.open();
                    break;
                }
            case Bitrix.BBCodeTag.video:
                {
					var dlg = Bitrix.Dialog.get("BXBBCode$Movie");
                    if (!dlg) {
                        dlg = Bitrix.MoviePasteDialog.create("BXBBCode$Movie", "BXBBCode$Movie", window.COMMUNICATION_UTILITY_BBCODE_MSG["Movie.DialogTitle"], null);
                        dlg.addCloseListener(this._moviePasteDialogCloseHandler);
                    }
					else if(dlg.isOpened()) return;
					
                    var textAreaSelection = Bitrix.TextAreaSelection.create(textArea);
                    var currentTextAreaSelection = textAreaSelection.getSelection();					
                    var textOriginal = currentTextAreaSelection.text;
									
                    var url = "http://";
					var mode = Bitrix.BBCodeInsertMode.insertAfter;	
					if(Bitrix.TypeUtility.isNotEmptyString(textOriginal) && textOriginal.search(this._urlRx) >= 0) {
						url = textOriginal;
						mode = Bitrix.BBCodeInsertMode.replace
					}					
					
                    var options = new Object();
                    options["_BXBBCode_CustomTagTextArea"] = textArea;
                    options["_BXBBCode_CustomTagArgs"] = args;
					options["_BXBBCode_CustomTagMode"] = mode;
					options["_movieUrl"] = url;
                    if ("options" in Bitrix.BBCodeUtility) {
                        if ("contextParams" in Bitrix.BBCodeUtility.options)
                            options["_contextRequestParams"] = Bitrix.BBCodeUtility.options.contextParams;
                    }	
                    
				    //FOR IE				    
					this.textAreaSelectionToDialogOptions(currentTextAreaSelection, options);                      				
					dlg.setOptions(options);
					dlg.open();
                    break;
                }
            case Bitrix.BBCodeTag.audio:
                {
					var dlg = Bitrix.Dialog.get("BXBBCode$Audio");
                    if (!dlg) {
                        dlg = Bitrix.AudioPasteDialog.create("BXBBCode$Audio", "BXBBCode$Audio", window.COMMUNICATION_UTILITY_BBCODE_MSG["Audio.DialogTitle"], null);
                        dlg.addCloseListener(this._audioPasteDialogCloseHandler);
                    }
					else if(dlg.isOpened()) return;
					
                    var textAreaSelection = Bitrix.TextAreaSelection.create(textArea);
                    var currentTextAreaSelection = textAreaSelection.getSelection();					
                    var textOriginal = currentTextAreaSelection.text;
									
                    var url = "http://";
					var mode = Bitrix.BBCodeInsertMode.insertAfter;	
					if(Bitrix.TypeUtility.isNotEmptyString(textOriginal) && textOriginal.search(this._urlRx) >= 0) {
						url = textOriginal;
						mode = Bitrix.BBCodeInsertMode.replace
					}
                    var options = new Object();
                    options["_BXBBCode_CustomTagTextArea"] = textArea;
                    options["_BXBBCode_CustomTagArgs"] = args;
					options["_BXBBCode_CustomTagMode"] = mode;
					options["_audioUrl"] = url;
                    if ("options" in Bitrix.BBCodeUtility) {
                        if ("contextParams" in Bitrix.BBCodeUtility.options)
                            options["_contextRequestParams"] = Bitrix.BBCodeUtility.options.contextParams;
                    }
				    //FOR IE				    
					this.textAreaSelectionToDialogOptions(currentTextAreaSelection, options);                        					
					dlg.setOptions(options);
					dlg.open();
                    break;
                }                
            case Bitrix.BBCodeTag.list:
                {
                    var items = new Array(this._strings.ListFirst, this._strings.ListSecond);
                    var stub = "\n";
                    for (var i = 0; i < items.length; i++)
                        stub += "[*]" + items[i] + "\n";

                    args[0] = "1";
                    args[1] = stub;
                    this.insertInTextAreaCustomTag("list", textArea, args, Bitrix.BBCodeInsertMode.insertAfter);
                    break;
                }
        }
    },
    insertInTextAreaCustomTag: function(tagName, textArea, args, mode, cursorPos, selection) {
        if (!(args instanceof Array))
            args = new Array();

        if (!mode)
            mode = Bitrix.BBCodeInsertMode.replace; //replace

        if (!cursorPos)
            cursorPos = Bitrix.BBCodeInsertCursorPosition.beforeCloseTag;
        
        var value = args[0] ? args[0].toString() : "";
        var contents = args[1] ? args[1].toString() : "";
        var attributes = args[2] ? args[2] : null;
        var textAreaSelection = Bitrix.TextAreaSelection.create(textArea);
        //var scrollTop = textAreaSelection.getScrollTop();
        
        var textSelectionStart = 0;
        var textSelectionEnd = 0;
        var textFinalCursorPosition = 0;
        var textOriginal = "";
         
        var currentTextAreaSelection = textAreaSelection.getSelection();         
        if(!selection) {
            textSelectionStart = currentTextAreaSelection.start;
            textSelectionEnd = currentTextAreaSelection.end;
            textOriginal = currentTextAreaSelection.text; 
            textFinalCursorPosition = currentTextAreaSelection.end;    	
        }
        else {
            textSelectionStart = currentTextAreaSelection.start = selection.start;
            textSelectionEnd = currentTextAreaSelection.end = selection.end;
            textOriginal = currentTextAreaSelection.text = selection.text; 
            textFinalCursorPosition = "cursorPos" in selection ? selection.cursorPos : selection.end;   
       } 

        //textModified = (value.length > 0 ? "[" + tagName + "=" + value + "]" : "[" + tagName + "]") + textModified + "[/" + tagName + "]";
        var innerText = contents.length > 0 ? contents : (mode == 1 ? textOriginal : "");
        var textModified = "[" + tagName + (value.length > 0 ? "=" + value : "");
        if (typeof (attributes) == "object")
            for (var k in attributes)
            textModified += " " + k + "=" + attributes[k].toString();
        textModified += "]" + innerText + "[/" + tagName + "]";

        var tmpEl = document.createElement("TEXTAREA");
        tmpEl.value = textModified;
        textModified = tmpEl.value;

        if (mode == Bitrix.BBCodeInsertMode.replace) {
            textAreaSelection.replaceSelectionByText(currentTextAreaSelection, textModified);
            if (cursorPos == Bitrix.BBCodeInsertCursorPosition.beforeCloseTag)
                textFinalCursorPosition = textSelectionStart + textModified.length - 3 - tagName.length;
            else
                textFinalCursorPosition = textSelectionStart + textModified.length;
        }
        else {
            textAreaSelection.insertAfterSelection(currentTextAreaSelection, textModified);
            if (cursorPos == Bitrix.BBCodeInsertCursorPosition.beforeCloseTag)
                textFinalCursorPosition = textSelectionStart + textOriginal.length + textModified.length - 3 - tagName.length;
            else
                textFinalCursorPosition = textSelectionStart + textOriginal.length + textModified.length;
        }

        textAreaSelection.setCursorPosition(textFinalCursorPosition);
        textAreaSelection.setFocus();
    },

    getColorPicker: function() {
        if (this._palette != null)
            return this._palette;

        this._palette = new Bitrix.UI.ColorPicker();

        this._palette.Instantiate(this._strings);

        return this._palette;
    },

    insertInTextAreaQuotation: function(srcTextArea, dstTextArea, args) {
        if (!(args instanceof Array))
            args = new Array();

        var srcTextAreaSelection = Bitrix.TextAreaSelection.create(srcTextArea);
        var currentSrcTextAreaSelection = srcTextAreaSelection.getSelection();
        var text = currentSrcTextAreaSelection.text;
        if (text.length == 0)
            text = srcTextAreaSelection.getElementText();
        var author = args.length > 0 ? args[0].toString() : "";
        text = (author.length > 0 ? "[b]" + author + "[/b]: " : "") + text;

        args[0] = "";
        args[1] = text;

        this.insertInTextAreaCustomTag("quote", dstTextArea, args, Bitrix.BBCodeInsertMode.insertAfter, Bitrix.BBCodeInsertCursorPosition.afterCloseTag);
    },
    _handleImagePasteDialogClose: function(sender, args) {
        if (!(sender && args && "buttonId" in args && args["buttonId"] == Bitrix.Dialog.button.bOk)) return;
        var customTagTextArea = sender.getOption("_BXBBCode_CustomTagTextArea");
        var customTagArgs = sender.getOption("_BXBBCode_CustomTagArgs");
        if (!(customTagArgs instanceof Array)) customTagArgs = new Array();
		//FOR IE
		var selection = new Object();
        this.textAreaSelectionFromDialogOptions(selection, sender.getOptions()); 	        
        if (sender.getMode() == Bitrix.ImagePasteDialogMode.file) {
            var selected = sender.getSelectedImages();
            if (!selected || selected.length == 0) return;

            for (var i = 0; i < selected.length; i++) {
                customTagArgs[1] = selected[i];
                this.insertInTextAreaCustomTag("img", customTagTextArea, customTagArgs, Bitrix.BBCodeInsertMode.insertAfter, Bitrix.BBCodeInsertCursorPosition.afterCloseTag, selection);
            }
            sender.resetSelectedImages();
        }
        else if (sender.getMode() == Bitrix.ImagePasteDialogMode.url) {
            var customTagMode = sender.getOption("_BXBBCode_CustomTagMode");
            if (!customTagMode) customTagMode = Bitrix.BBCodeInsertMode.insertAfter;
            var url = sender.getImageUrl();
            if (url.length > 0) {
                customTagArgs[1] = url;
                this.insertInTextAreaCustomTag("img", customTagTextArea, customTagArgs, customTagMode, null, selection);
                sender.resetImageUrl();
            }
        }
    },
    _handleMoviePasteDialogClose: function(sender, args) {
        if (!(sender && args && "buttonId" in args && args["buttonId"] == Bitrix.Dialog.button.bOk)) return;
		try {
			var url = sender.getMovieUrl();
			if (!Bitrix.TypeUtility.isNotEmptyString(url)) return;
			var customTagTextArea = sender.getOption("_BXBBCode_CustomTagTextArea");
			var customTagArgs = sender.getOption("_BXBBCode_CustomTagArgs");
			if (!(customTagArgs instanceof Array)) customTagArgs = new Array();		
			customTagArgs[0] = sender.getMovieWidthInPixels() + "x" + sender.getMovieHeightInPixels();
			customTagArgs[1] = url;
			var customTagMode = sender.getOption("_BXBBCode_CustomTagMode");
            if (!customTagMode) customTagMode = Bitrix.BBCodeInsertMode.insertAfter;				
			//customTagArgs[2] = {"width":sender.getMovieWidthInPixels(), "height":sender.getMovieHeightInPixels()};
			//FOR IE
			var selection = new Object();
            this.textAreaSelectionFromDialogOptions(selection, sender.getOptions()); 			
			this.insertInTextAreaCustomTag("video", customTagTextArea, customTagArgs, customTagMode, Bitrix.BBCodeInsertCursorPosition.afterCloseTag, selection);
		}
		catch(e) {}
    },
    _handleAudioPasteDialogClose: function(sender, args) {
        if (!(sender && args && "buttonId" in args && args["buttonId"] == Bitrix.Dialog.button.bOk)) return;		
		try {
			var url = sender.getAudioUrl();
			if (!Bitrix.TypeUtility.isNotEmptyString(url)) return;
			var customTagTextArea = sender.getOption("_BXBBCode_CustomTagTextArea");
			var customTagArgs = sender.getOption("_BXBBCode_CustomTagArgs");
			if (!(customTagArgs instanceof Array)) customTagArgs = new Array();			
			customTagArgs[1] = url;	
			//FOR IE
			var selection = new Object();
            this.textAreaSelectionFromDialogOptions(selection, sender.getOptions());  					
			var customTagMode = sender.getOption("_BXBBCode_CustomTagMode");
            if (!customTagMode) customTagMode = Bitrix.BBCodeInsertMode.insertAfter;	
			this.insertInTextAreaCustomTag("audio", customTagTextArea, customTagArgs, customTagMode, Bitrix.BBCodeInsertCursorPosition.afterCloseTag, selection);
		}
		catch(e) {}
    },    
    _handleLinkPasteDialogClose: function(sender, args) {
        if (!(sender && args && "buttonId" in args && args["buttonId"] == Bitrix.Dialog.button.bOk)) return;
		try {
			var url = sender.getUrl();
			if (!Bitrix.TypeUtility.isNotEmptyString(url)) return;
			var customTagTextArea = sender.getOption("_BXBBCode_CustomTagTextArea");
			var customTagArgs = sender.getOption("_BXBBCode_CustomTagArgs");
			if (!(customTagArgs instanceof Array)) customTagArgs = new Array();	
			customTagArgs[0] = url;
			customTagArgs[1] = sender.getText();
			//FOR IE
			var selection = new Object();
            this.textAreaSelectionFromDialogOptions(selection, sender.getOptions());            					
			this.insertInTextAreaCustomTag("url", customTagTextArea, customTagArgs, null, null, selection);
		}
		catch(e) {}
    },
    _handleCodePasteDialogClose: function(sender,args) {
        if (!(sender && args && "buttonId" in args && args["buttonId"] == Bitrix.Dialog.button.bOk)) return;
		try {
			var url = sender.getText();
			if (!Bitrix.TypeUtility.isNotEmptyString(url)) return;
			var customTagTextArea = sender.getOption("_BXBBCode_CustomTagTextArea");
			var customTagArgs = sender.getOption("_BXBBCode_CustomTagArgs");
			if (!(customTagArgs instanceof Array)) customTagArgs = new Array();	
			customTagArgs[0] = sender.getLanguage();
			customTagArgs[1] = sender.getText();
			//FOR IE
			var selection = new Object();
            this.textAreaSelectionFromDialogOptions(selection, sender.getOptions());  					
			this.insertInTextAreaCustomTag("code", customTagTextArea, customTagArgs, null, null, selection);
		}
		catch(e) {}    
    }
}
Bitrix.BBCodeUtility._instance = null;
Bitrix.BBCodeUtility.getInstance = function(){
	if(this._instance == null){
		this._instance = new Bitrix.BBCodeUtility();
		this._instance.initialize();
	}
	return this._instance;
}
Bitrix.BBCodeUtility.setStrings = function(strings){	
	this.getInstance().setStrings(strings);
}

Bitrix.BBCodeUtility.insertInTextArea = function(tag, textArea, args){	
	this.getInstance().insertInTextArea(tag, textArea, args);
}
Bitrix.BBCodeUtility.getColorPicker = function(){	
	return this.getInstance().getColorPicker();
}
Bitrix.BBCodeUtility.insertInTextAreaQuotation = function(srcTextArea, dstTextArea, args){	
	this.getInstance().insertInTextAreaQuotation(srcTextArea, dstTextArea, args);
}
Bitrix.DocumentSelection = function Bitrix$DocumentSelection()
{
    if(typeof(Bitrix.DocumentSelection.initializeBase) == "function")
        Bitrix.DocumentSelection.initializeBase(this);
	this._initialized = false; 
}
Bitrix.DocumentSelection.prototype = {
    initialize: function(){
		this._initialized = true;
	},
	
	getSelectedText: function(){
		if (document.selection && document.selection.createRange)
			return document.selection.createRange().text;
		else if (window.getSelection)
			return window.getSelection().toString();
		return "";
	}
}
Bitrix.DocumentSelection.create = function(){
	var self = new Bitrix.DocumentSelection();
	self.initialize();
	return self;
}
Bitrix.TextAreaSelection = function Bitrix$TextAreaSelection(){
    if(typeof(Bitrix.TextAreaSelection.initializeBase) == "function")
        Bitrix.TextAreaSelection.initializeBase(this);
	this._initialized = false;
	this._element = null;
	this._elementScrollTop = 0;
}
Bitrix.TextAreaSelection.prototype = {
	initialize: function(textArea){
		var el = null;
		if(Bitrix.TypeUtility.isNotEmptyString(textArea))
			el = document.getElementById(textArea);
		else if(Bitrix.TypeUtility.isDomElement(textArea))
			el = textArea;	
		
		if(el == null)
			throw "TextArea element is not defined!";
			
		if(el.tagName.toUpperCase() != "TEXTAREA")
			throw "Element '" + textAreaEl.id, "'" + " is not a 'TEXTAREA'!";
		
		this._element = el;
		this._initialized = true;
	},
	
	getElement: function(){
		return this._element;
	},
	
	getElementText: function(){
		return this._element.value;
	},
	
	getSelection: function(){
		var result = { start: 0, end: 0, text: "" };
		if(/*@cc_on !@*/true){ //NOT IE
			result.start = this._element.selectionStart;
			result.end = this._element.selectionEnd;
			result.text = this._element.value.substring(result.start, result.end);
		}
		else{	//IE
			this._element.focus();
			var documentRange  = document.selection.createRange().duplicate();
			result.text = documentRange.text;
			var contentsOriginal = this._element.value;
			
			var marker = "", 
			markerCreationTryCount = 0;
		
			while(contentsOriginal.indexOf((marker = "BX_TEXTAREASELECTION_MARKER_#" + (new Date()).valueOf().toString())) != -1){
				if(++markerCreationTryCount > 32)
					throw "Could not create unique marker!";
			}
			documentRange.text = marker + documentRange.text + marker;
			var contentsWork = this._element.value;
			result.start = contentsWork.indexOf(marker);
			result.end = contentsWork.indexOf(marker, result.start + marker.length) - marker.length;	
			
			this._element.value = contentsOriginal;
			//TextRange.moveStart(), TextRange.moveEnd() treat '\r\n' as 1 char!
			var priorSelectionText = contentsOriginal.substring(0, result.start);
			var newLineInPriorSelectionTextCount = 0;
			if(priorSelectionText.length > 0){
				var newLineRx = /\r\n/g;
				while(newLineRx.exec(priorSelectionText) != null)
					newLineInPriorSelectionTextCount++;			
			}
			var elementRange = this._element.createTextRange().duplicate();
	        elementRange.collapse(true);
	        elementRange.moveStart("character", result.start - newLineInPriorSelectionTextCount);
	        elementRange.moveEnd("character", result.end - result.start);
	        elementRange.select();				
		}
		return result;
	},
	
	getSelectionStart: function(){
		return this.getSelection().start;
	},
	
	getSelectionEnd: function(){
		return this.getSelection().end;	
	},
	
	getSelectedText: function(){
		var contents = this._element.value;
		if(contents.length == 0)
			return "";
			
		var selection = this.getSelection();
		return 	selection.text;	
	},
	
	_scrollTopFromElement: function(){
		if(typeof(this._element.scrollTop) != "undefined")
			this._elementScrollTop = this._element.scrollTop;
	},
	_scrollTopToElement: function(){
		if(typeof(this._element.scrollTop) != "undefined")
			this._element.scrollTop = this._elementScrollTop;
	},
	replaceSelectionByText: function(selection, text){
		if(typeof(selection) != "object" || selection == null || !("start" in selection) || !("end" in selection))
			throw "Parameter 'selection' is not a valid!";
			
		if(!Bitrix.TypeUtility.isString(text))
			throw "Parameter 'text' is not a String!";
		
		var contentsOld = this._element.value;
		var contentsNew = contentsOld.substring(0, selection.start) + text + contentsOld.substring(selection.end);
		this._scrollTopFromElement();
		this._element.value = contentsNew;
		this._scrollTopToElement();
	},
	insertAfterSelection: function(selection, text)
	{
		if(typeof(selection) != "object" || selection == null || !("start" in selection) || !("end" in selection))
			throw "Parameter 'selection' is not a valid!";
			
		if(!Bitrix.TypeUtility.isString(text))
			throw "Parameter 'text' is not a String!";
		
		var contentsOld = this._element.value;
		var contentsNew = contentsOld.substring(0, selection.end) + text + contentsOld.substring(selection.end);
		this._scrollTopFromElement();
		this._element.value = contentsNew;
		this._scrollTopToElement();	
	},
	
	replaceSelectedText: function(text){
		if(!Bitrix.TypeUtility.isString(text))
			throw "Parameter 'text' is not a String!";
		
		var contentsOld = this._element.value;
		var selection = this.getSelection();
		
		var contentsNew = contentsOld.substring(0, selection.start) + text + contentsOld.substring(selection.end);
		this._scrollTopFromElement();
		this._element.value = contentsNew;
		this._scrollTopToElement();	
	},
	
	setCursorPosition: function(position){
		if(/*@cc_on !@*/true){ //NOT IE
			this._element.setSelectionRange(position, position);
		}
		else{ //IE
			var priorSelectionText = this._element.value.substring(0, position);
			var newLineInPriorSelectionTextCount = 0;
			if(priorSelectionText.length > 0){
				var newLineRx = /\r\n/g;
				while(newLineRx.exec(priorSelectionText) != null)
					newLineInPriorSelectionTextCount++;			
			}			
			var range = this._element.createTextRange();
	        range.collapse(true);			
	        range.moveStart("character", position - newLineInPriorSelectionTextCount);
	        range.moveEnd("character", 0);
	        range.select();			
		}
	},
	setFocus: function(){
		this._element.focus();	
	},
	scrollToBottom: function(){
	    if(typeof(this._element.scrollTop) != "undefined" && typeof(this._element.scrollHeight) != "undefined")
	        this._element.scrollTop = this._element.scrollHeight;
	}	
}

Bitrix.TextAreaSelection.create = function(textArea){
	var self = new Bitrix.TextAreaSelection();
	self.initialize(textArea);
	return self;
}

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded(); 