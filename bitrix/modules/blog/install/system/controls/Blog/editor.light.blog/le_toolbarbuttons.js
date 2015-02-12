/* This code is used internally in Bitrix API and is not intended for public use. This file is subject to change. */

if (!window.LHEButtons)
	LHEButtons = {};

LHEButtons['CreateLink'] = {
	id: 'CreateLink',
	name: LHE_MESS.CreateLink,
	name_edit: LHE_MESS.EditLink,
	disableOnCodeView: true,
	handler: function(pBut)
	{
		var editor = pBut.pLEditor;
		var p = (pBut.arSelectedElement && pBut.arSelectedElement['A']) ? pBut.arSelectedElement['A'] : editor.GetSelectionObject();
		if (!p || p.tagName != 'A')
			p = false;

		editor.oPrevRange = pBut.pLEditor.GetSelectionRange();

		// Select Link
		if (p && !jsUtils.bIsIE)
			editor.oPrevRange = editor.SelectElement(p);

		var oRange = editor.oPrevRange;

		// Get selected text
		var selected = false;
		if (oRange.startContainer && oRange.endContainer) // DOM Model
		{
			if (oRange.startContainer == oRange.endContainer && (oRange.endContainer.nodeType == 3 || oRange.endContainer.nodeType == 1))
			{
				//check for inner tags
				var range = oRange.startContainer.textContent.substring(oRange.startOffset, oRange.endOffset) || '';
				if (oRange.endContainer.nodeType == 3 || !/<\\w+?[^>]*?>/i.test(range))
					selected = range;
			}
		}
		else // IE
		{
			if (oRange.text == oRange.htmlText)
				selected = oRange.text || '';
		}

		var url = false;
		if (p)
		{
			if (selected !== false)
			{
				var text = p.innerHTML;
				selected = !/<\w+?[^>]*?>/i.test(text) ? text : false;
			}
			
			var bxTag = editor.GetBxTag(p.id);
			if (bxTag.tag) 
				url = bxTag.params.href;
		}

		var dialogId = (selected !== false) ? "BXLHEDialog$Url$Full" : "BXLHEDialog$Url$UrlOnly";

		var dlg = Bitrix.Dialog.get(dialogId);
		if (!dlg)
		{
			dlg = Bitrix.LinkPasteDialog.create(dialogId, "", url ? LHE_MESS.EditLink : LHE_MESS.CreateLink, { ref: {}, linkPasteDialogLayout: (selected !== false ? Bitrix.LinkPasteDialogLayout.full : Bitrix.LinkPasteDialogLayout.urlOnly) });
			dlg.addCloseListener(Bitrix.TypeUtility.createDelegate(this, this.closeHandler));
		}

		var options = dlg.getOption("ref");
		options.selection = p;
		options.editor = pBut.pLEditor;
		options.selected = selected;

		dlg.setUrl(url || 'http://');
		if (selected !== false)
			dlg.setText(Bitrix.HttpUtility.htmlDecode(selected));

		dlg.open();
	},
	closeHandler: function(sender, args)
	{
		if (!(sender && args && "buttonId" in args && args["buttonId"] == Bitrix.Dialog.button.bOk)) return;
		var options = sender.getOption("ref")
		var selection = options.selection;
		var editor = options.editor;
		var selected = options.selected;

		var url = sender.getUrl();
		if (!url || url.length < 1) // Need for showing error
			return;

		if (selected !== false)
			selected = Bitrix.HttpUtility.htmlEncode(sender.getText());

		editor.SelectRange(editor.oPrevRange);

		var link = selection;
		if (!link)
		{
			link = false;
			var sRand = 'bx_lhe_a_' + Math.random().toString().substring(5);
			var tempHref = '#' + sRand;
			var pDoc = editor.pEditorDocument;

			pDoc.execCommand('CreateLink', false, tempHref);
			if (pDoc.evaluate)
				link = pDoc.evaluate("//a[@href='" + tempHref + "']", pDoc.body, null, 9, null).singleNodeValue;
			else
			{
				var arLinks = pDoc.getElementsByTagName('A');
				for (var i = 0; i < arLinks.length; i++)
				{
					if (arLinks[i].getAttribute('href', 2) == tempHref)
					{
						link = arLinks[i];
						break;
					}
				}
			}
		}
		
		var linkId;
		
		if (!link) // Create new link
		{
			linkId = editor.SetBxTag(false, {tag: 'a', params: { href: ''}})
			editor.InsertHTML('<a id="' + linkId + '">#</a>');
			link = pDoc.getElementById(linkId);
		}

		if (link)
		{
			var bxTag = editor.GetBxTag(link);
			if (!bxTag || !bxTag.tag)
				bxTag = editor.GetBxTag(editor.SetBxTag(link, {tag: 'a', params: { href: ''}}));
			bxTag.params.href = url;
			
			if (selected !== false)
				link.innerHTML = selected;
			SetAttr(link, 'href', url);						
		}
	},
	parser:
	{
		name: "a",
		obj: {
			Parse: function(sName, sContent, pLEditor)
			{
				// Link
				return sContent.replace(
					/<a([\s\S]*?(?:.*?[^\?]{1})??)(>[\s\S]*?<\/a>)/ig,
					function(str, attributes, leftover)
					{
						var arParams = pLEditor.GetAttributesList(attributes), res = "";
						
						res = "<a id=\"" + pLEditor.SetBxTag(false, {tag: 'a', params: arParams}) + "\" ";
						for (var i in arParams)
						{
							if (typeof arParams[i] == 'string' && i != 'id')
								res += i + '="' + BX.util.htmlspecialchars(arParams[i]) + '" ';
						}
						res += leftover;
						return res;
					}
				);
			},
			UnParse: function(bxTag, pNode, pLEditor)
			{
				if (!bxTag.params)
					return '';

				var i, res = '<a ';

				bxTag.params['class'] = pNode.arAttributes['class'] || false;
				for (i in bxTag.params)
				{
					if (bxTag.params[i])
						res += i + '="' + BX.util.htmlspecialchars(bxTag.params[i]) + '" ';
				}
				res += '>';
				res += pLEditor.GetNodeInnerHTML(pNode);
				res += '</a>';

				return res;
			}
		}
	}
};

LHEButtons['Image'] = {
	id: 'Image',
	name: LHE_MESS_BLOG.InsertImage,
	name_edit: LHE_MESS_BLOG.EditImage,
	disableOnCodeView: true,
	handler: function(pBut)
	{
		var p = (pBut.arSelectedElement && pBut.arSelectedElement['IMG']) ? pBut.arSelectedElement['IMG'] : pBut.pLEditor.GetSelectionObject();
		if (!p || p.tagName != 'IMG')
			p = false;
		
		var dlg = Bitrix.Dialog.get("BXLHEDialog$Image");
		if (!dlg)
		{
			dlg = Bitrix.ImagePasteDialog.create("BXLHEDialog$Image", "", LHE_MESS.Image, Bitrix.ImagePasteDialogMode.url, { ref: {} });
			dlg.addCloseListener(Bitrix.TypeUtility.createDelegate(this, this.closeHandler));
		}

		var options = dlg.getOption("ref");
		options.selection = p;
		options.editor = pBut.pLEditor;

		pBut.pLEditor.oPrevRange = pBut.pLEditor.GetSelectionRange();
		
		dlg.open();
		
		var bxTag;
		if (p && (bxTag = pBut.pLEditor.GetBxTag(p)) && bxTag.tag)
			dlg.setImageUrl(bxTag.params.src);
		else
			dlg.resetImageUrl();
	},
	closeHandler: function(sender, args)
	{
		if (!(sender && args && "buttonId" in args && args["buttonId"] == Bitrix.Dialog.button.bOk)) return;
		var options = sender.getOption("ref")
		var selection = options.selection;
		var editor = options.editor;

		var url = sender.getImageUrl();
		if (url.length < 0) // Need for showing error
			return;

		editor.SelectRange(editor.oPrevRange);
		var img = selection;
		if (!img)
		{
			var linkId = editor.SetBxTag(false, {tag: 'img', params: { src: ''}});
			editor.InsertHTML('<img id="' + linkId + '" src="" />');
			img = editor.pEditorDocument.getElementById(linkId);			
		}
		
		var bxTag = editor.GetBxTag(img);
		if (!bxTag || !bxTag.tag)
			bxTag = editor.GetBxTag(editor.SetBxTag(img, {tag: 'img', params: { src: ''}}));
		bxTag.params.src = url;
		
		SetAttr(img, "src", url);
		
		sender.resetImageUrl();
	},
	parser: {
		name: "img",
		obj: {
			Parse: function(sName, sContent, pLEditor)
			{
				// Image
				return sContent.replace(
					/<img\s+(.*?)\/?>/ig,
					function(str, attributes)
					{
						var arParams = pLEditor.GetAttributesList(attributes), res = "";
						if (arParams && arParams.id)
						{
							var oTag = pLEditor.GetBxTag(arParams.id);
							if (oTag.tag)
								return str;
						}

						res = "<img id=\"" + pLEditor.SetBxTag(false, {tag: 'img', params: arParams}) + "\" ";
						for (var i in arParams)
						{
							if (typeof arParams[i] == 'string')
								res += i + '="' + BX.util.htmlspecialchars(arParams[i]) + '" ';
						}
						res += " />";
						return res;
					}
				);
			},
			UnParse: function(bxTag, pNode, pLEditor)
			{
				if (!bxTag.params)
					return '';

				// width, height
				var
					w = parseInt(pNode.arStyle.width) || parseInt(pNode.arAttributes.width),
					h = parseInt(pNode.arStyle.height) || parseInt(pNode.arAttributes.height);

				if (w && !isNaN(w))
					bxTag.params.width = w;
				if (h && !isNaN(h))
					bxTag.params.height = h;

				bxTag.params['class'] = pNode.arAttributes['class'] || false;

				var i, res = '<img ';
				for (i in bxTag.params)
				{
					if (typeof bxTag.params[i] == 'string' || typeof bxTag.params[i] == 'number')
						res += i + '="' + BX.util.htmlspecialchars(bxTag.params[i]) + '" ';
				}
				res += ' />';

				return res;
			}
		}
	}
};

LHEButtons['ImageUpload'] = {
	id: 'ImageUpload',
	name: LHE_MESS_BLOG.UploadImage,
	name_edit: LHE_MESS_BLOG.EditImage,
	disableOnCodeView: true,
	handler: function(pBut)
	{
		var dlg = Bitrix.Dialog.get("BXLHEDialog$ImageUpload");
		if (!dlg)
		{
			if (!pBut.pLEditor.arConfig.blogImageUploadOptions)
				return;
			
			var options =
			{
				_contextRequestParams: pBut.pLEditor.arConfig.blogImageUploadOptions.contextRequestParams,
				_handlerPath: pBut.pLEditor.arConfig.blogImageUploadOptions.handlerPath,
				ref: {}
			}

			dlg = Bitrix.ImagePasteDialog.create("BXLHEDialog$ImageUpload", "", LHE_MESS.Image, Bitrix.ImagePasteDialogMode.file, options);
			dlg.addCloseListener(Bitrix.TypeUtility.createDelegate(this, this.closeHandler));
		}

		var options = dlg.getOption("ref");
		options.editor = pBut.pLEditor;

		pBut.pLEditor.oPrevRange = pBut.pLEditor.GetSelectionRange();
		dlg.open();
	},
	closeHandler: function(sender, args)
	{
		if (!(sender && args && "buttonId" in args && args["buttonId"] == Bitrix.Dialog.button.bOk)) return;
		var options = sender.getOption("ref")
		var editor = options.editor;

		var selected = sender.getSelectedImages();
		if (!selected || selected.length == 0) // Need for showing error
			return;


		editor.SelectRange(editor.oPrevRange);
		var ids = [];
		var html = '';
		for (var i = 0; i < selected.length; i++)
		{
			var id = editor.SetBxTag(false, {tag:'img', params: { src: ''}});
			html += editor.InsertHTML('<img id="' + id + '" src="" />'); 
			ids.push(id);
		}
		for (var i = 0; i < selected.length; i++)
		{
			var img = editor.pEditorDocument.getElementById(ids[i]);
			var bxTag = editor.GetBxTag(img);
			bxTag.params.src = selected[i];
			SetAttr(img, "src", selected[i]);		
		}
		sender.resetSelectedImages();
	}
};

LHEButtons['Video'] = {
	id: 'Video',
	name: LHE_MESS.InsertVideo,
	name_edit: LHE_MESS.EditVideo,
	disableOnCodeView: true,
	handler: function(pBut)
	{
		var p = pBut.pLEditor.GetSelectionObject();
		var bxTag;
		if (!p || (bxTag = pBut.pLEditor.GetBxTag(p)).tag != "video")
			p = false;

		var dlg = Bitrix.Dialog.get("BXLHEDialog$Movie");
		if (dlg == null)
		{
			dlg = Bitrix.MoviePasteDialog.create("BXLHEDialog$Movie", "", LHE_MESS.InsertVideo, { ref: {} });
			dlg.addCloseListener(Bitrix.TypeUtility.createDelegate(this, this.closeHandler));
		}
		var options = dlg.getOption("ref");
		options.selection = p;
		options.editor = pBut.pLEditor;

		if (p)
		{
			dlg.setMovieUrl(bxTag.params.file);
			dlg.setMovieWidthInPixels(bxTag.params.width);
			dlg.setMovieHeightInPixels(bxTag.params.height);
		}
		else
			dlg.reset();

		pBut.pLEditor.oPrevRange = pBut.pLEditor.GetSelectionRange();
		dlg.open();
	},
	closeHandler: function(sender, args)
	{
		if (!(sender && args && "buttonId" in args && args["buttonId"] == Bitrix.Dialog.button.bOk)) return;
		var options = sender.getOption("ref")
		var selection = options.selection;
		var editor = options.editor;
		var path = sender.getMovieUrl();
		var w = sender.getMovieWidthInPixels();
		var h = sender.getMovieHeightInPixels();
		var videoConfig = editor.arConfig.blogVideoOptions || {};

		if (path.length < 1) // Need for showing error
			return;

		editor.SelectRange(editor.oPrevRange);

		var video;
		if (selection)
		{
			video = selection;
		}
		else
		{
			var videoId = editor.SetBxTag(false,  {tag: 'video', params: {}});
			editor.InsertHTML('<img class="bx-wysiwyg-video" id="' + videoId + '" src="' + editor.oneGif + '" />');
			video = editor.pEditorDocument.getElementById(videoId);
		}

		if (videoConfig.maxWidth && w && parseInt(w) > parseInt(videoConfig.maxWidth))
			w = videoConfig.maxWidth;
		if (videoConfig.maxHeight && h && parseInt(h) > parseInt(videoConfig.maxHeight))
			h = videoConfig.maxHeight;

		var oVideo = { width: w, height: h, file: path };
		if (path.indexOf('http://') != -1 || path.indexOf('.') != -1)
		{
			var bxTag = editor.GetBxTag(video);
			if (!bxTag || !bxTag.tag)
				bxTag = editor.GetBxTag(editor.SetBxTag(video, { tag: 'video' }));
			bxTag.params = oVideo;
			
			video.style.width = w + 'px';
			video.style.height = h + 'px';
			video.style.backgroundImage = 'url(' + jsUtils.Path.ToAbsolute('~/bitrix/controls/Main/editor.light/images/video.gif') + ')';
		}
		else
		{
			editor.SetBxTag(video, null);
			editor.InsertHTML('');
		}
	},
	parser:
	{
		name: "video",
		obj:
		{
			Parse: function(sName, sContent, pLEditor)
			{
				var _this = this;
				return sContent.replace(
					/<video([^>]*)>((?:.|[\r\n])*?)<\/video>/ig,
					function(str, attributes, content)
					{
						var path = jsUtils.trim(Bitrix.HttpUtility.htmlDecode(content));
						if (path == '')
							return '';

						attributes = attributes.replace(/[\r\n]+/ig, ' ');
						attributes = attributes.replace(/\s+/ig, ' ');
						attributes = jsUtils.trim(attributes);
						var arParams = {};
						attributes.replace(
							/(\w+?)\s*=\s*(?:"([^"]*)"|'([^']*)'|(\w*))/ig,
							function(s, name, value1, value2, value3)
							{
								name = name.toLowerCase();
								arParams[name] = Bitrix.HttpUtility.htmlDecode(value1 || value2 || value3);
								return '';
							}
						);

						var id = pLEditor.SetBxTag(false, { tag: "video", params:
						{
							width: parseInt(arParams.width) || 0,
							height: parseInt(arParams.height) || 0,
							file: path
						}});

						var w = (parseInt(arParams.width) || 50) + 'px';
						var h = (parseInt(arParams.height) || 25) + 'px';

						return '<img class="bx-wysiwyg-video" id="' + id + '" src="' + pLEditor.oneGif + '" style="background-image: url(' + jsUtils.Path.ToAbsolute('~/bitrix/controls/Main/editor.light/images/video.gif') + '); width: ' + w + '; height: ' + h + ';"  title="' + LHE_MESS.Video + ': ' + Bitrix.HttpUtility.htmlEncode(path) + '"/>';
					}
				);
			},
			UnParse: function(bxTag, pNode, pLEditor)
			{
				var arParams = bxTag.params,
				i, str;

				var arVidConf = pLEditor.arConfig.blogVideoOptions || {};
				if (arVidConf.maxWidth && arParams.width && parseInt(arParams.width) > parseInt(arVidConf.maxWidth))
					arParams.width = arVidConf.maxWidth;
				if (arVidConf.maxHeight && arParams.height && parseInt(arParams.height) > parseInt(arVidConf.maxHeight))
					arParams.height = arVidConf.maxHeight;

				str = '<video';
				if (arParams.width)
					str += ' width="' + arParams.width + '"';
				if (arParams.height)
					str += ' height="' + arParams.height + '"';
				str += '>' + Bitrix.HttpUtility.htmlEncode(arParams.file) + '</video>';

				return str;
			}
		}
	}
};

LHEButtons['Audio'] = {
	id: 'Audio',
	name: LHE_MESS.InsertAudio,
	name_edit: LHE_MESS.EditAudio,
	disableOnCodeView: true,
	handler: function(pBut)
	{
		var p = pBut.pLEditor.GetSelectionObject();
		var bxTag;
		if (!p || (bxTag = pBut.pLEditor.GetBxTag(p)).tag != "audio")
			p = false;

		var dlg = Bitrix.Dialog.get("BXLHEDialog$Audio");
		if (dlg == null)
		{
			dlg = Bitrix.AudioPasteDialog.create("BXLHEDialog$Audio", "", LHE_MESS.InsertAudio, { ref: {} });
			dlg.addCloseListener(Bitrix.TypeUtility.createDelegate(this, this.closeHandler));
		}
		var options = dlg.getOption("ref");
		options.selection = p;
		options.editor = pBut.pLEditor;

		var audio;
		if (p)
		{
			dlg.setAudioUrl(bxTag.params.file);
		}
		else
			dlg.reset();

		pBut.pLEditor.oPrevRange = pBut.pLEditor.GetSelectionRange();
		dlg.open();
	},
	closeHandler: function(sender, args)
	{
		if (!(sender && args && "buttonId" in args && args["buttonId"] == Bitrix.Dialog.button.bOk)) return;
		var options = sender.getOption("ref")
		var selection = options.selection;
		var editor = options.editor;
		var path = sender.getAudioUrl();
	
		if (path.length < 1) // Need for showing error
			return;

		editor.SelectRange(editor.oPrevRange);

		var audio;
		if (selection)
		{
			audio = selection;
		}
		else
		{
			var audioId = editor.SetBxTag(false, {tag: 'audio', params: {}});
			editor.InsertHTML('<img class="bx-wysiwyg-audio" id="' + audioId + '" src="' + editor.oneGif + '" style="width: 192px; height: 24px;" />');
			audio = editor.pEditorDocument.getElementById(audioId);
		}

		var oAudio = { file: path };
		if (path.indexOf('http://') != -1 || path.indexOf('.') != -1)
		{
			var bxTag = editor.GetBxTag(audio);
			if (!bxTag || !bxTag.tag)
				bxTag = editor.GetBxTag(editor.SetBxTag(audio, { tag: 'audio' }));
			bxTag.params = oAudio;
			
			audio.style.backgroundImage = 'url(' + jsUtils.Path.ToAbsolute('~/bitrix/controls/Main/editor.light/images/audio.gif') + ')';
		}
		else
		{
			editor.SetBxTag(audio, null);
			editor.InsertHTML('');
		}
	},
	parser:
	{
		name: "audio",
		obj:
		{
			audios: {},
			Parse: function(sName, sContent, pLEditor)
			{
				var _this = this;
				return sContent.replace(
					/<audio([^>]*)>((?:.|[\r\n])*?)<\/audio>/ig,
					function(str, attributes, content)
					{
						var path = jsUtils.trim(Bitrix.HttpUtility.htmlDecode(content));
						if (path == '')
							return '';

						var id = pLEditor.SetBxTag(false, { tag: "audio", params: { file: path }});
						return '<img class="bx-wysiwyg-audio"  id="' + id + '" src="' + pLEditor.oneGif + '" style="background-image: url(' + jsUtils.Path.ToAbsolute('~/bitrix/controls/Main/editor.light/images/audio.gif') + '); width: 192px; height: 24px;" title="' + LHE_MESS.Audio + ': ' + Bitrix.HttpUtility.htmlEncode(path) + '"/>';
					}
				);
			},
			UnParse: function(bxTag, pNode, pLEditor)
			{
				return '<audio>' + Bitrix.HttpUtility.htmlEncode(bxTag.params.file) + '</audio>';
			}
		}
	}
};

LHEButtons['InsertPreview'] = {
	id: 'InsertPreview',
	name: LHE_MESS_BLOG.InsertPreview,
	disableOnCodeView: true,
	handler: function(pBut)
	{
		var selection = pBut.pLEditor.GetSelectionRange();
		pBut.pLEditor.SelectRange(selection);
		var selectedHtml = pBut.pLEditor.GetSelectedHTML();
		if (jsUtils.trim(selectedHtml) == '')
			selectedHtml = '<br/>';

		if (selection.deleteContents)
		{
			var div = pBut.pLEditor.pEditorDocument.createElement('DIV');
			div.setAttribute('class', 'bx-wysiwyg-preview');
			div.innerHTML = selectedHtml;
			
			pBut.pLEditor.SetBxTag(div, { tag: 'preview', params: {} });

			var insert = div.childNodes.length > 0 ? div.childNodes[0] : null;

			selection.deleteContents();
			selection.insertNode(div);

			var cursor = pBut.pLEditor.pEditorDocument.createRange();
			cursor.selectNodeContents(div);
			pBut.pLEditor.SelectRange(cursor);
			pBut.pLEditor.SetFocus();
		}
		else
			pBut.pLEditor.InsertHTML('<div class="bx-wysiwyg-preview" id="' + pBut.pLEditor.SetBxTag(div, { tag: 'preview', params: {} }) + '">' + selectedHtml + '</div>');
	},
	parser:
	{
		name: "preview",
		obj:
		{
			Parse: function(sName, sContent, pLEditor)
			{
				var _this = this;
				return sContent.replace(
					/<preview([^>]*)>((?:.|[\r\n])*?)<\/preview>/ig,
					function(str, attributes, content)
					{
						return '<div class="bx-wysiwyg-preview" id="' + pBut.pLEditor.SetBxTag(div, { tag: 'preview', params: {} }) + '" >' + content + '</div>';
					}
				);
			},
			UnParse: function(sName, pNode, pLEditor)
			{
				return '<preview>' + pLEditor.GetNodeInnerHTML(pNode) + '</preview>';
			}
		}
	}
};

LHEButtons["RemovePreview"] = {
	id: 'RemovePreview',
	name_edit: LHE_MESS_BLOG.RemovePreview,
	disableOnCodeView: true,
	handler: function(pBut)
	{
		var p = pBut.pLEditor.GetSelectionObject();
		var bxTag;
		while (p && (bxTag = pBut.pLEditor.GetBxTag(p)).tag != 'preview')
			p = p.parentNode;

		if (!p || bxTag != 'preview')
			return;

		var innerContent = p.innerHTML;
		pBut.pLEditor.DetachOuterElement(p, '<br/>', '<br/>');
	}
};

LHEButtons['InsertQuote'] = {
	id: 'InsertQuote',
	name: LHE_MESS_BLOG.InsertQuote,
	disableOnCodeView: true,
	handler: function(pBut)
	{
		var ed = pBut.pLEditor;
		var doc = pBut.pLEditor.pEditorDocument;
		
		var selection = ed.GetSelectionRange();
		ed.SelectRange(selection);

		if (typeof(selection.extractContents) != "undefined") 
		{
			var q = doc.createElement('BLOCKQUOTE');			
			q.setAttribute("contentEditable", "true");
			ed.SetBxTag(q, {tag: "blockquote", params: {} });
			
			var p = doc.createElement("P");
			p.appendChild(selection.extractContents());		
			p.appendChild(doc.createElement("BR"));
			q.appendChild(p);
			selection.insertNode(q);
			selection.insertNode(doc.createElement("BR"));
			selection.setStart(p, 0);
			selection.setEnd(p, p.childNodes.length > 0 ? p.childNodes.length - 1 : 0);			
			p.focus();
		}
		else if(typeof(selection.pasteHTML) != "undefined") 
		{
			var selectedHtml = ed.GetSelectedHTML();
			jsUtils.trim(selectedHtml);
	
			var id = ed.SetBxTag(false, {tag: "blockquote", params: {} });
			var elHtml ='<blockquote id="' +  id  + '" contentEditable="true">' + selectedHtml + '</blockquote><br/>';
			ed.InsertHTML(elHtml);
			
			var q = doc.getElementById(id);
			doc.selection.empty();
			selection = doc.selection.createRange();
			if(typeof(selection.moveToElementText) != "undefined")
				selection.moveToElementText(q);			
			else if(typeof(selection.add) != "undefined") {
				while(selection.length > 0)
					selection.remove(0);
				selection.add(q);
			}
			selection.select();			
			q.removeAttribute("id");	
		}
		ed.SetFocus();
	},
	parser:
	{
		name: "blockquote",
		obj:
		{
            Parse: function(sName, sContent, pLEditor)
            {
				return sContent.replace(
					/<blockquote\s*([^>]*)?>/ig,
					function(str, attrs)
					{
						var arParams = pLEditor.GetAttributesList(attrs), res = "";
						
						var res = '<blockquote contentEditable="true" id="' + pLEditor.SetBxTag(false, {tag: 'blockquote', params: arParams}) + '" ';
						for (var i in arParams)
						{
							if (typeof arParams[i] == 'string' && i != 'id' && i != "contenteditable")
								res += i + '="' + BX.util.htmlspecialchars(arParams[i]) + '" ';
						}
						res += '>';
						
						return res;
					}
					
				);            
            },
			UnParse: function(bxTag, pNode, pLEditor)
			{
				var res = '<blockquote';
				for (var i in bxTag.params)
					res += ' ' + i + '="' + BX.util.htmlspecialchars(bxTag.params[i]) + '" ';
				res += '>' + pLEditor.GetNodeInnerHTML(pNode) + '</blockquote>';
				return res ;
			}           		
		}	
	}	
};

LHEButtons["RemoveQuote"] = {
	id: 'RemoveQuote',
	name_edit: LHE_MESS_BLOG.RemoveQuote,
	disableOnCodeView: true,
	handler: function(pBut)
	{
		var p = pBut.pLEditor.GetSelectionObject();
		var bxTag;
		while (p && (bxTag = pBut.pLEditor.GetBxTag(p)).tag != 'blockquote')
			p = p.parentNode;

		if (!p || bxTag.tag != 'blockquote')
			return;

		var innerContent = p.innerHTML;
		pBut.pLEditor.DetachOuterElement(p, '<br/>', '<br/>');
	}	
};

LHEButtons['InsertCut'] = {
	id: 'InsertCut',
	name: LHE_MESS_BLOG.InsertCut,
	disableOnCodeView: true,
	handler: function(pBut)
	{
		var selection = pBut.pLEditor.GetSelectionRange();
		pBut.pLEditor.SelectRange(selection);
		var selectedHtml = pBut.pLEditor.GetSelectedHTML();
		if (jsUtils.trim(selectedHtml) == '')
			selectedHtml = '&nbsp;';

		pBut.pLEditor.InsertHTML(
			'<div class="bx-wysiwyg-cut-start" id="' + pBut.pLEditor.SetBxTag(false, { tag: 'bx_cut_start', params: {} }) + '" contentEditable="true">' 
			+ Bitrix.HttpUtility.htmlEncode(LHE_MESS_BLOG.DefaultCutTitle) 
			+ (Bitrix.NavigationHelper.isFirefox() ? '<br type="_moz" _moz_dirty=""/>' : '') 
			+ '</div>' 
			+ selectedHtml 
			+ '<img class="bx-wysiwyg-cut-end" id="' + pBut.pLEditor.SetBxTag(false, { tag: 'bx_cut_end', params: {} }) + '" src="' + pBut.pLEditor.oneGif + '" alt="end of cut" />'
		);
	},
	parsers:
	[
		{
			name: "bx_cut_start",
			obj:
			{
				Parse: function(sName, sContent, pLEditor)
				{
					var _this = this;
					return sContent.replace(
						/<cut([^>]*)>/ig,
						function(str, attributes)
						{
							var title;
							attributes = attributes.replace(/[\r\n]+/ig, ' ');
							attributes = attributes.replace(/\s+/ig, ' ');
							attributes = jsUtils.trim(attributes);
							attributes.replace(
								/(\w+?)\s*=\s*(?:"([^"]*)"|'([^']*)'|(\w*))/ig,
								function(s, name, value1, value2, value3)
								{
									if (name.toLowerCase() == 'title')
										title = Bitrix.HttpUtility.htmlDecode(value1 || value2 || value3);
									return '';
								}
							);
							var res = '<div class="bx-wysiwyg-cut-start" id="' + pLEditor.SetBxTag(false, { tag: 'bx_cut_start', params: {} }) + '" contentEditable="true">';
							res += title ? Bitrix.HttpUtility.htmlEncode(title) : '&nbsp;';
							res += Bitrix.NavigationHelper.isFirefox() ? '<br type="_moz" _moz_dirty=""/>' : ''
							res += '</div>';
							return res;
						}
					);
				},
				UnParse: function(bxTag, pNode, pLEditor)
				{
					var t = this._GetTextRecursive(pNode);
					return '<cut' + (t ? (' title="' + Bitrix.HttpUtility.htmlEncode(t) + '"') : '') + '>';
				},
				_GetTextRecursive: function(pNode)
				{
					if (pNode.type == 'text')
						return pNode.text;
					
					var r = '';
					var len = pNode.arNodes.length;
					for (var i = 0; i < len; i++)
						r += this._GetTextRecursive(pNode.arNodes[i]);
					return r;
				}
			}
		},
		{
			name: "bx_cut_end",
			obj:
			{
				Parse: function(sName, sContent, pLEditor)
				{
					var _this = this;
					return sContent.replace(
						/<\/cut>/ig,
						function(str, attributes, content)
						{
							return '<img id="' + pLEditor.SetBxTag(false, { tag: 'bx_cut_end', params: {} }) + '" src="' + pLEditor.oneGif + '" alt="end of cut" class="bx-wysiwyg-cut-end" />';
						}
					);
				},
				UnParse: function(bxTag, pNode, pLEditor)
				{
					return '</cut>';
				}
			}
		}
	]
};

LHEButtons['InsertCode'] = {
	id: 'InsertCode',
	name: LHE_MESS_BLOG.InsertCode,
	name_edit: LHE_MESS_BLOG.EditCode,
	disableOnCodeView: true,
	CO: function(value, title)
	{
		var op = document.createElement('OPTION');
		op.text = title;
		op.value = value;
		return op;
	},
	handler: function(pBut)
	{
		var p = pBut.pLEditor.GetSelectionObject();
		var bxTag;
		while (p && (bxTag = pBut.pLEditor.GetBxTag(p)).tag != 'code')
			p = p.parentNode;

		if (!p || bxTag.tag != 'code')
			p = false;
		
		var dlg = Bitrix.Dialog.get("BXLHEDialog$Code");
		if (dlg == null)
		{
			dlg = Bitrix.CodePasteDialog.create("BXLHEDialog$Code", "", LHE_MESS_BLOG.InsertCode, { ref: {} });
			dlg.addCloseListener(Bitrix.TypeUtility.createDelegate(this, this.closeHandler));
			dlg.setLanguageOptions([
				this.CO('text', 'Plain Text'),
				this.CO('csharp', 'C#'),
				this.CO('vb', 'Visual Basic'),
				this.CO('cpp', 'C++'),
				this.CO('sql', 'SQL'),
				this.CO('xml', 'XML/HTML'),
				this.CO('jscript', 'JavaScript'),
				this.CO('css', 'CSS'),
				this.CO('python', 'Python'),
				this.CO('ruby', 'Ruby'),
				this.CO('php', 'PHP'),
				this.CO('java', 'Java'),
				this.CO('delphi', 'Delphi')
			]);
		}
		var options = dlg.getOption("ref");
		options.selection = p;
		options.editor = pBut.pLEditor;

		dlg.setText(p ? p.value : '');
		dlg.setLanguage((p && bxTag.params && bxTag.params.lang) ? bxTag.params.lang : 'text');

		pBut.pLEditor.oPrevRange = pBut.pLEditor.GetSelectionRange();
		dlg.open();
	},
	closeHandler: function(sender, args)
	{
		if (!(sender && args && "buttonId" in args && args["buttonId"] == Bitrix.Dialog.button.bOk)) return;
		var options = sender.getOption("ref")
		var selection = options.selection;
		var editor = options.editor;
		var language = sender.getLanguage();
		var text = sender.getText();

		editor.SelectRange(editor.oPrevRange);

		var code;
		if (selection)
			code = selection;
		else
		{
			var id = editor.SetBxTag(false, { tag: 'code', params: { } });
			editor.InsertHTML('<textarea wrap="off" id="' + id + '" class="bx-wysiwyg-code"></textarea>' + (Bitrix.NavigationHelper.isFirefox() ? '<br type="_moz" _moz_dirty=""/>' : '<br/>'));
			code = editor.pEditorDocument.getElementById(id);			
		}

		var bxTag =  editor.GetBxTag(code);
		if (!bxTag)
			bxTag = editor.GetBxTag(editor.SetBxTag(code, { tag: 'code', params: { } }));
		bxTag.params.lang = language;
		code.value = text;
	},
	parser:
	{
		name: "code",
		obj:
		{
			Parse: function(sName, sContent, pLEditor)
			{
				var _this = this;
				return sContent.replace(
					/<code([^>]*)>((?:.|[\r\n])*?)<\/code>/ig,
					function(str, attributes, content)
					{
						var code = Bitrix.HttpUtility.htmlDecode(content);
						var language;

						attributes = attributes.replace(/[\r\n]+/ig, ' ');
						attributes = attributes.replace(/\s+/ig, ' ');
						attributes = jsUtils.trim(attributes);
						var arParams = {};
						attributes.replace(
							/(\w+?)\s*=\s*(?:"([^"]*)"|'([^']*)'|(\w*))/ig,
							function(s, name, value1, value2, value3)
							{
								if (name.toLowerCase() == 'language');
									language = Bitrix.HttpUtility.htmlDecode(value1 || value2 || value3);
								return '';
							}
						);

						return '<textarea wrap="off "class="bx-wysiwyg-code" id="' + pLEditor.SetBxTag(false, { tag: 'code', params: { lang: language || 'text' }}) + '" >' + Bitrix.HttpUtility.htmlEncode(code) + '</textarea>';
					}
				);
			},
			UnParse: function(bxTag, pNode, pLEditor)
			{
				var language = bxTag.params.lang;


				str = '<code';
				if (language)
					str += ' language="' + Bitrix.HttpUtility.htmlEncode(language) + '"';
				str += '>';
				str += Bitrix.HttpUtility.htmlEncode(pLEditor.pEditorDocument.getElementById(bxTag.id).value);
				str += '</code>';

				return str;
			}
		}
	}
};

for (var p in LHEButtons)
{
	if (p == "Source")
		continue;
	LHEButtons[p].disableOnCodeView = true;
}


/* CONTEXT MENU*/
if (!window.LHEContMenu)
	LHEContMenu = {};

LHEContMenu["IMG"] = [LHEButtons['Image']];
LHEContMenu["VIDEO"] = [LHEButtons['Video']];
LHEContMenu["AUDIO"] = [LHEButtons['Audio']];
LHEContMenu["PREVIEW"] = [LHEButtons['RemovePreview']];
LHEContMenu["BLOCKQUOTE"] = [LHEButtons['RemoveQuote']];
LHEContMenu["CODE"] = [LHEButtons['InsertCode']];

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded(); 