<%@ Page Language="C#" EnableEventValidation="false" EnableViewState="false" EnableViewStateMac="false" Inherits="Bitrix.UI.BXPage" ValidateRequest="False"%>

<%@ Import Namespace="Bitrix.Common" %>
<%@ Import Namespace="System.Text" %>
<%@ Import Namespace="Bitrix.Services" %>
<%@ Import Namespace="System.Collections" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Text.RegularExpressions" %>

<script runat="server" type="text/C#">
	string DialogName;
	bool show_controls;
	HtmlTextWriter w;
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		DialogName = Bitrix.Services.Text.BXTextEncoder.HtmlTextEncoder.Encode(Request.QueryString["name"]);
		show_controls = Request.QueryString["not_use_default"] == "Y" ? false : true;
	}

	protected override void Render(HtmlTextWriter writer)
	{
		w = writer;
		base.Render(writer);
	}
</script>

<%
	string htmlcontent = "";
%>

<% if (DialogName == "anchor") { %>
<script>
var pElement = null;
function OnLoad()
{	
	pElement = pObj.pMainObj.GetSelectionObject();
	window.oBXEditorDialog.SetTitle('<%= GetMessage("Anchor.WindowTitle.New") %>');
	var el = BX("anchor_value"), value = "";
	if (pElement)
	{
		var bxTag = pObj.pMainObj.GetBxTag(pElement);
		if (bxTag && bxTag.tag == "anchor")
			value = pObj.pMainObj.pParser.GetAnchorName(bxTag.params.value);
	}

	el.value = value;
	el.focus();
	window.oBXEditorDialog.adjustSizeEx();
}

function OnSave()
{
	BXSelectRange(oPrevRange, pObj.pMainObj.pEditorDocument, pObj.pMainObj.pEditorWindow);
	pElement = pObj.pMainObj.GetSelectionObject();
	pObj.pMainObj.bSkipChanges = true;
	var anchor_value = BX("anchor_value"), bxTag = false;

	if (pElement)
	{
		bxTag = pObj.pMainObj.GetBxTag(pElement);
		if (!bxTag || bxTag.tag != "anchor")
			pElement = false;
	}

	if(pElement && bxTag) // Modify or del anchor
	{
		if(anchor_value.value.length <= 0)
		{
			pObj.pMainObj.executeCommand('Delete');
		}
		else
		{
			bxTag.params.value = pObj.pMainObj.pParser.GetAnchorName(bxTag.params.value, anchor_value.value);
			pObj.pMainObj.SetBxTag(false, bxTag);
		}
	}
	else if(anchor_value.value.length > 0) // New anchor
	{
		var id = pObj.pMainObj.SetBxTag(false, {tag: "anchor", params: {value : '<a name="' + anchor_value.value + '"></a>'}});
		pObj.pMainObj.insertHTML('<img id="' + id + '" src="' + one_gif_src + '" class="bxed-anchor" />');

		var pEl = pObj.pMainObj.pEditorDocument.getElementById(id);
		if(pObj.pMainObj.pEditorWindow.getSelection)
			pObj.pMainObj.pEditorWindow.getSelection().selectAllChildren(pEl);
	}
	pObj.pMainObj.bSkipChanges = false;
	pObj.pMainObj.OnChange("anchor");
}
</script>

<asp:PlaceHolder runat="server" ID="DialogContentAnchor" Visible="False">
<div style="padding: 5px;">
<label for="anchor_value"><%= GetMessage("Anchor.Name") + ":" %>&nbsp;</label>
<input type="text" size="25" value="" id="anchor_value" />
</div>
</asp:PlaceHolder>

<%
	using (var s = new System.IO.StringWriter())
	using (var h = new HtmlTextWriter(s))
	{
		DialogContentAnchor.Visible = true;
		DialogContentAnchor.RenderControl(h);
		DialogContentAnchor.Visible = false;
		htmlcontent = s.ToString();
	}
%>

<% } else if (DialogName == "link") { %>

<script>
var pElement = null;
var curLinkType = 't1';
function OnLoad()
{
	var bWasSelectedElement = false, bxTag = false;
	pElement = pObj.pMainObj.GetSelectionObject();

	if (pElement && pElement.nodeName && pElement.nodeName.toUpperCase() != 'A')
	{
		var nodeName = pElement.nodeName.toUpperCase();
		if (nodeName == 'IMG')
			bWasSelectedElement = true;
		pElement = BXFindParentByTagName(pElement, 'A');
	}

	if (pElement)
	{
		bxTag = pObj.pMainObj.GetBxTag(pElement);
		if (!bxTag || bxTag.tag != "a")
			bxTag = false;
	}

	// Set title
	window.oBXEditorDialog.SetTitle((pElement && bxTag) ? '<%= GetMessage("Link.WindowTitle.Edit") %>' : '<%= GetMessage("Link.WindowTitle.New") %>');

	if (BX("OpenFileBrowserWindLink_button"))
		BX("OpenFileBrowserWindLink_button").onclick = function()
	{
		__OpenDialog(
			{
				ShowFiles: true,
				ItemsToSelect: 2,
				ExtensionsList: '',
				EnableExtras: true,
				DefaultUploadDirectory: ""
			},
			function(val)
			{
				var pInput = BX("bx_url_1");
				val = val.replace(/\\/g, '/');
				val = val.replace(/~/g, '');
				pInput .value = APPPath + val;
				
				pInput.focus();
				pInput.select();
			}
		);
	};
		

	// Set styles
	var
		arStFilter = ['A', 'DEFAULT'], i, j,
		elStyles = BX("bx_classname"),
		arStyles;

	for(i = 0; i < arStFilter.length; i++)
	{
		arStyles = pObj.pMainObj.oStyles.GetStyles(arStFilter[i]);
		for(j = 0; j < arStyles.length; j++)
		{
			if(arStyles[j].className.length<=0)
				continue;
			oOption = new Option(arStyles[j].className, arStyles[j].className, false, false);
			elStyles.options.add(oOption);
		}
	}

	// Fetch anchors
	var
		pAnchorSelect = BX('bx_url_3'),
		i, l, anc, ancName, anchorBxTag
		arImgs = pObj.pMainObj.pEditorDocument.getElementsByTagName('IMG');

	for(i = 0, l = arImgs.length; i < l; i++)
	{
		anchorBxTag = pObj.pMainObj.GetBxTag(arImgs[i]);
		if (anchorBxTag && anchorBxTag.tag == "anchor" && (ancName = pObj.pMainObj.pParser.GetAnchorName(anchorBxTag.params.value)))
			pAnchorSelect.options.add(new Option(ancName, '#' + ancName, false, false));
	}

	if (pAnchorSelect.options.length <= 0)
	{
		pAnchorSelect.options.add(new Option('<%= GetMessage("NO_ANCHORS")%>', '', true, true));
		pAnchorSelect.disabled = true;
	}

	var tip = pObj.pMainObj._dialogLinkTip || "t1";
	var selectedText = false;
	if(pElement && bxTag) /* Link selected*/
	{
		oPrevRange = pObj.pMainObj.SelectElement(pElement);
		if (pElement.childNodes && pElement.childNodes.length == 1 && pElement.childNodes[0].nodeType == 3)
			selectedText = pElement.innerHTML;

		var href = bxTag.params.href;
		if(href.substring(0, 7).toLowerCase() == 'mailto:') // email
		{
			tip = "t4";
			BX("bx_url_4").value = href.substring('mailto:'.length);
		}
		else if(href.substr(0, 1) == '#') // anchor
		{
			BX("bx_url_3").value = href;
			if(BX("bx_url_3").value == href)
			{
				tip = "t3";
			}
			else
			{
				tip = "t1";
				BX("bx_url_1").value = href;
			}
		}
		else if (href.indexOf("://") !== -1 || href.substr(0, 'www.'.length) == 'www.' || href.indexOf("&goto=") !== -1)
		{
			tip = "t2";
			if (href.substr(0, 'www.'.length) == 'www.')
				href = "http://" + href;

			var sProt = href.substr(0, href.indexOf("://") + 3);

			BX("bx_url_type").value = sProt;
			if (BX("bx_url_type").value != sProt)
				BX("bx_url_type").value = '';

			BX("bx_url_2").value = href.substring(href.indexOf("://") + 3);
		}
		else // link to page on server
		{
			tip = "t1";
			BX("bx_url_1").value = href;
		}

		var className = pElement.className;
		if(className)
		{
			var pClassSel = BX("bx_classname");
			pClassSel.value = className;
			if (pClassSel.value != className) // Add class to select if it's not exsist here
				pClassSel.options.add(new Option(className, className, true, true));
		}

		BX("bx_targ_list").value = bxTag.params.target || '';
		BX("__bx_id").value = bxTag.params.id || '';
		BX("BXEditorDialog_title").value = bxTag.params.title || '';

		var rel = bxTag.params.rel || '';
		if (bxTag.params.noindex || rel == 'nofollow')
		{
			BX("bx_noindex").checked = true;
			BX("bx_link_rel").disabled = true;
		}

		if (rel)
			BX("bx_link_rel").value = rel;
	}
	else if (!bWasSelectedElement)/* NO selected link*/
	{
		// Get selected text
		if (oPrevRange.startContainer && oPrevRange.endContainer) // DOM Model
		{
			if (oPrevRange.startContainer == oPrevRange.endContainer && (oPrevRange.endContainer.nodeType == 3 || oPrevRange.endContainer.nodeType == 1))
			{
				selectedText = oPrevRange.startContainer.textContent.substring(oPrevRange.startOffset, oPrevRange.endOffset) || '';
			}
		}
		else // IE
		{
			if (oPrevRange.text == oPrevRange.htmlText)
				selectedText = oPrevRange.text || '';
		}
	}

	if (selectedText === false)
		BX('bx_link_text_tr').style.display = "none";
	else
		BX('bx_link_text').value = selectedText || '';

	BX('bx_link_type').value = tip;

	ChangeLinkType();
}

function OnSave()
{
	var
		href='',
		target='',
		bText = BX('bx_link_text_tr').style.display != "none";
	switch(BX('bx_link_type').value)
	{
		case 't1':
			href = BX('bx_url_1').value;
			break;
		case 't2':
			href = BX('bx_url_2').value;

			if (BX("bx_url_type").value && href.indexOf('://') == -1)
				href = BX("bx_url_type").value + href;
			break;
		case 't3':
			href = BX('bx_url_3').value;
			break;
		case 't4':
			if(BX('bx_url_4').value)
				href = 'mailto:' + BX('bx_url_4').value;
			break;
	}

	BXSelectRange(oPrevRange, pObj.pMainObj.pEditorDocument, pObj.pMainObj.pEditorWindow);
	pObj.pMainObj.bSkipChanges = true;

	if(href.length > 0)
	{
		var arlinks = [];
		if (window.pElement)
		{
			arlinks[0] = pElement;
		}
		else
		{
			var sRand = '#'+Math.random().toString().substring(5);
			if (bText) // Simple case
			{
				pObj.pMainObj.insertHTML('<a id="bx_lhe_' + sRand + '">#</a>');
				arlinks[0] = pObj.pMainObj.pEditorDocument.getElementById('bx_lhe_' + sRand);
				arlinks[0].removeAttribute("id");
			}
			else
			{
				pObj.pMainObj.pEditorDocument.execCommand('CreateLink', false, sRand);
				var arLinks_ = pObj.pMainObj.pEditorDocument.getElementsByTagName('A');
				for(var i = 0; i < arLinks_.length; i++)
					if(arLinks_[i].getAttribute('href', 2) == sRand)
						arlinks.push(arLinks_[i]);
			}
		}

		var oTag, i, l = arlinks.length, link;
		for (i = 0;  i < l; i++)
		{
			link = arlinks[i];
			oTag = false;

			if (window.pElement && i == 0)
			{
				oTag = pObj.pMainObj.GetBxTag(pElement);
				if (oTag.tag != 'a' || !oTag.params)
					oTag = false;
			}

			if (!oTag)
				oTag = {tag: 'a', params: {}};

			oTag.params.href = href;
			oTag.params.title = BX("BXEditorDialog_title").value;
			oTag.params.id = BX("__bx_id").value;
			oTag.params.target = BX("bx_targ_list").value;
			oTag.params.noindex = !!BX("bx_noindex").checked;
			oTag.params.rel = BX("bx_link_rel").value;

			var arEls = ['href', 'title', 'id', 'rel', 'target'], i, l = arEls.length;
			for (i = 0; i < l; i++)
				if (!pObj.pMainObj.pParser.isPhpAttribute(oTag.params[arEls[i]]))
					SAttr(link, arEls[i], oTag.params[arEls[i]]);

			pObj.pMainObj.SetBxTag(link, oTag);
			SAttr(link, 'className', BX("bx_classname").value);

			// Add text
			if (bText)
				link.innerHTML = BX.util.htmlspecialchars(BX('bx_link_text').value || href);
		}
	}

	pObj.pMainObj.bSkipChanges = false;
	pObj.pMainObj.OnChange("link");
}

function showAddSect()
{
	var pCont = BX('bx_link_dialog_tbl').parentNode;
	var bShow = pCont.className.indexOf('bx-link-simple') == -1;

	if (bShow)
		BX.addClass(pCont, 'bx-link-simple');
	else
		BX.removeClass(pCont, 'bx-link-simple');

	window.oBXEditorDialog.adjustSizeEx();
}

function ChangeLinkType()
{
	var
		pTbl = BX('bx_link_dialog_tbl'),
		val = BX('bx_link_type').value;

	if (curLinkType == 't1' && val == 't2')
	{
		var url1 = BX('bx_url_1').value;
		if (url1 != '' && url1.indexOf('://') != -1)
		{
			BX('bx_url_2').value = url1.substr(url1.indexOf('://') + 3);
			BX('bx_url_type').value = url1.substr(0, url1.indexOf('://') + 3);
		}
	}
	curLinkType = val;
	pObj.pMainObj._dialogLinkTip = val;

	var pUrl = BX('bx_url_' + val.substr(1));
	if(pUrl && !pUrl.disabled)
		setTimeout(function(){pUrl.focus();}, 300);

	pTbl.className = ("bx-link-dialog-tbl bx--t1 bx--t2 bx--t3 bx--t4 bx-only-" + val).replace(' bx--' + val, '');
	window.oBXEditorDialog.adjustSizeEx();
}

function SetUrl(filename, path, site)
{
	var url, pInput = BX("bx_url_1"), pText = BX("bx_link_text"), pTitle = BX("BXEditorDialog_title");
	if (typeof filename == 'object') // Using medialibrary
	{
		url = filename.src;
		pText.value = filename.name;
		pTitle.value = filename.description || filename.name;
	}
	else // Using file dialog
	{
		url = (path == '/' ? '' : path) + '/' + filename;
	}

	pInput.value = url;
	pInput.focus();
	pInput.select();
}
</script>

<asp:PlaceHolder runat="server" ID="DialogContentLink" Visible="False">
<table class="bx-link-dialog-tbl bx--t1 bx--t2 bx--t3 bx--t4" id="bx_link_dialog_tbl">
	<tr class="bx-link-type">
		<td class="bx-par-title"><label for="bx_link_type"><%= GetMessage("Link.Type")%></label></td>
		<td class="bx-par-val">
			<select id='bx_link_type' onchange="ChangeLinkType();">
				<option value='t1'><%= GetMessage("Link.Type.Internal")%></option>
				<option value='t2'><%= GetMessage("Link.Type.External")%></option>
				<option value='t3'><%= GetMessage("Link.Type.Document")%></option>
				<option value='t4'><%= GetMessage("Link.Type.MailTo")%></option>				
			</select>
		</td>
	</tr>

	<tr><td colSpan="2" class="bx-link-sep"></td></tr>

	<tr id="bx_link_text_tr">
		<td class="bx-par-title"><label for="bx_link_text"><%= GetMessage("LINK_TEXT")%>:</label></td>
		<td class="bx-par-val"><input type="text" size="30" value="" id="bx_link_text" /></td>
	</tr>

	<tr class="bx-link-t1">
		<td class="bx-par-title"><label for="bx_url_1"><%= GetMessage("Link.Document")%>:</label></td>
		<td class="bx-par-val">
			<input type="text" size="30" value="" id="bx_url_1">
			<input type="button" id="OpenFileBrowserWindLink_button" value="...">
			
		</td>
	</tr>

	<!-- Link to external site -->
	<tr class="bx-link-t2">
		<td class="bx-par-title"><label for="bx_url_2"><%= GetMessage("Link.URL")%>:</label></td>
		<td class="bx-par-val">
			<select id='bx_url_type'>
				<option value="http://">http://</option>
				<option value="ftp://">ftp://</option>
				<option value="https://">https://</option>
				<option value=""></option>
			</select>
			<input type="text" size="25" value="" id="bx_url_2">
		</td>
	</tr>

	<!-- anchor -->
	<tr class="bx-link-t3">
		<td class="bx-par-title"><label for="bx_url_3"><%= GetMessage("Link.Anchor")+ ":" %></label></td>
		<td class="bx-par-val">
			<select id="bx_url_3"></select>
		</td>
	</tr>

	<!-- email -->
	<tr class="bx-link-t4">
		<td class="bx-par-title"><label for="bx_url_4"><%= GetMessage("Link.Email")+ ":" %></label></td>
		<td class="bx-par-val">
			<input type="text" size="30" value="" id="bx_url_4">
		</td>
	</tr>

	<tr class="bx-header"><td colSpan="2"><a  class="bx-adv-link" onclick="showAddSect(); return false;" href="javascript: void(0);"><%= GetMessage("ADDITIONAL")%> <span>(<%= GetMessage("HIDE")%>)</span></a></td></tr>

	<tr id="bx_target_row" class="bx-adv bx-hide-in-t3 bx-hide-in-t4">
		<td class="bx-par-title"><label for="bx_targ_list"><%= GetMessage("Link.Target")%>:</label></td>
		<td class="bx-par-val">
			<select id='bx_targ_list'>
				<option value=""></option>
				<option value="_blank"><%= GetMessage("Link.Target.Blank")%></option>
				<option value="_parent"><%= GetMessage("Link.Target.Parent")%></option>
				<option value="_self"><%= GetMessage("Link.Target.Self")%></option>
				<option value="_top"><%= GetMessage("Link.Target.Top")%></option>
			</select>
		</td>
	</tr>
	<tr class="bx-adv bx-hide-in-t3 bx-hide-in-t4">
		<td class="bx-par-title"><input type="checkbox" value="Y" id="bx_noindex" onclick="var rel = BX('bx_link_rel'); if (this.checked){rel.value='nofollow'; rel.disabled=true;}else{rel.disabled=false;rel.value='';}" /></td>
		<td class="bx-par-val"><label for="bx_noindex"><%= GetMessage("NOINDEX")%></label></td>
	</tr>
	<tr class="bx-adv">
		<td class="bx-par-title"><label for="BXEditorDialog_title"><%= GetMessage("Link.ToolTip")+ ":" %></label></td>
		<td class="bx-par-val">
			<input type="text" size="30" value="" id="BXEditorDialog_title">
		</td>
	</tr>
	<tr class="bx-adv">
		<td class="bx-par-title"><label for="bx_classname"><%= GetMessage("Link.CssClass")%>:</label></td>
		<td class="bx-par-val">
			<select id='bx_classname'><option value=""> - <%= GetMessage("NO_VAL")%> -</option></select>
		</td>
	</tr>
	<tr class="bx-adv">
		<td class="bx-par-title"><label for="__bx_id"><%= GetMessage("Link.ID")+ ":" %></label></td>
		<td class="bx-par-val"><input type="text" size="30" value="" id="__bx_id" /></td>
	</tr>
	<tr class="bx-adv">
		<td class="bx-par-title"><label for="bx_link_rel"><%= GetMessage("REL")%>:</label></td>
		<td class="bx-par-val"><input type="text" size="30" value="" id="bx_link_rel" /></td>
	</tr>
</table>
</asp:PlaceHolder>
<%
	using (var s = new System.IO.StringWriter())
	using (var h = new HtmlTextWriter(s))
	{
		DialogContentLink.Visible = true;
		DialogContentLink.RenderControl(h);
		DialogContentLink.Visible = false;
		htmlcontent = s.ToString();
	}
%>


<% } else if (DialogName == "image") { %>
<script>
var pElement = null;
function OnLoad()
{
	pElement = pObj.pMainObj.GetSelectionObject();
	var
		bxTag = false,
		preview = BX("bx_img_preview"),
		pWidth = BX("bx_width"),
		pHeight = BX("bx_height");

	preview.onload = PreviewOnLoad;

	if (pElement)
	{
		bxTag = pObj.pMainObj.GetBxTag(pElement);
		if (!bxTag || bxTag.tag != "img")
			bxTag = false;
	}

	if(!pElement || !bxTag)
	{
		pElement = null;
		window.oBXEditorDialog.SetTitle('<%= GetMessage("Image.WindowTitle.New") %>');
	}
	else
	{
		var w = parseInt(pElement.style.width || pElement.getAttribute('width') || pElement.offsetWidth);
		var h = parseInt(pElement.style.height || pElement.getAttribute('height') || pElement.offsetHeight);
		if (w && h)
		{
			pObj.iRatio = w / h; // Remember proportion
			pObj.curWidth = pWidth.value = w;
			pObj.curHeight = pHeight.value = h;
		}

		window.oBXEditorDialog.SetTitle('<%= GetMessage("Image.WindowTitle.Edit") %>');

		BX("bx_src").value = bxTag.params.src || "";
		BX("bx_img_title").value = bxTag.params.title || "";
		BX("bx_alt").value = bxTag.params.alt || "";
		BX("bx_border").value = bxTag.params.border || "";
		BX("bx_align").value = bxTag.params.align || "";
		BX("bx_hspace").value = bxTag.params.hspace || "";
		BX("bx_vspace").value = bxTag.params.vspace || "";

		preview.style.display = "";
		pObj.prevsrc = preview.src = BX("bx_src").value;
		preview.alt = BX("bx_alt").value;
		preview.border = BX("bx_border").value;
		preview.align = BX("bx_align").value;
		preview.hspace = BX("bx_hspace").value;
		preview.vspace = BX("bx_vspace").value;

		preview.onload = function(){PreviewReload(); preview.onload = PreviewOnLoad;};
	}	

	if (BX("bx_img_file_dialog_button"))
		BX("bx_img_file_dialog_button").onclick = function(){
		__OpenDialog(
			{
				ShowFiles: true,
				ItemsToSelect: 2,
				ExtensionsList: "", //"jpg,bmp,png,gif",
				ShowExtras: true,
				EnableExtras: true,
				DefaultUploadDirectory: "upload"
			},
			function(val)
			{
				val = val.replace(/\\/g, '/');
				val = val.replace(/~/g, '');
				var oSrc = BX("bx_src");
				oSrc.value = APPPath + val;
				if(oSrc.onchange)
					oSrc.onchange();
			}
		);
	}

	BX("bx_src").onchange = BX("bx_hspace").onchange =
	BX("bx_vspace").onchange = BX("bx_border").onchange =
	BX("bx_align").onchange = PreviewReload;

	var pSaveProp = BX("save_props");
	pSaveProp.onclick = function()
	{
		if (this.checked)
			pWidth.onchange();
	};

	pWidth.onchange = function()
	{
		var w = parseInt(this.value);
		if (isNaN(w))
			return;
		pObj.curWidth = pWidth.value = w;
		if (pSaveProp.checked)
		{
			var h = Math.round(w / pObj.iRatio);
			pObj.curHeight = pHeight.value = h;
		}
		PreviewReload();
	};

	pHeight.onchange = function()
	{
		var h = parseInt(this.value);
		if (isNaN(h))
			return;
		pObj.curHeight = pHeight.value = h;
		if (pSaveProp.checked)
		{
			var w = parseInt(h * pObj.iRatio);
			pObj.curWidth = pWidth.value = w;
		}
		PreviewReload();
	};

	window.oBXEditorDialog.adjustSizeEx();
}

function OnSave()
{
	pObj.pMainObj.bSkipChanges = true;
	var
		src = BX("bx_src").value,
		oTag = false;

	if (!src)
		return;

	if (window.pElement)
	{
		oTag = pObj.pMainObj.GetBxTag(pElement);
		if (oTag.tag != 'img' || !oTag.params)
			oTag = false;
	}

	if (!oTag)
	{
		oTag = {tag: 'img', params: {}};
		BXSelectRange(oPrevRange,pObj.pMainObj.pEditorDocument,pObj.pMainObj.pEditorWindow);
		pObj.pMainObj.insertHTML('<img id="__bx_img_temp_id" src="" />');
		pElement = pObj.pMainObj.pEditorDocument.getElementById("__bx_img_temp_id");
	}

	oTag.params.src = src;
	oTag.params.title = BX("bx_img_title").value;
	oTag.params.hspace = BX("bx_hspace").value;
	oTag.params.vspace = BX("bx_vspace").value;
	oTag.params.border = BX("bx_border").value;
	oTag.params.align = BX("bx_align").value;
	oTag.params.alt = BX("bx_alt").value;

	var arEls = ['src', 'alt', 'title', 'hspace', 'vspace', 'border', 'align'], i, l = arEls.length;
	for (i = 0; i < l; i++)
	{
		if (!pObj.pMainObj.pParser.isPhpAttribute(oTag.params[arEls[i]]))
			SAttr(pElement, arEls[i], oTag.params[arEls[i]]);
	}

	pElement.id = '';
	pElement.removeAttribute('id');
	pObj.pMainObj.SetBxTag(pElement, oTag);

	SAttr(pElement, "width", BX("bx_width").value);
	SAttr(pElement, "height", BX("bx_height").value);

	pObj.pMainObj.bSkipChanges = false;
	pObj.pMainObj.OnChange("image");
}

function PreviewOnLoad()
{
	var w = parseInt(this.style.width || this.getAttribute('width') || this.offsetWidth);
	var h = parseInt(this.style.height || this.getAttribute('hright') || this.offsetHeight);
	if (!w || !h)
		return;
	pObj.iRatio = w / h; // Remember proportion
	pObj.curWidth = BX("bx_width").value = w;
	pObj.curHeight = BX("bx_height").value = h;
};

function PreviewReload(bFirst)
{
	var el = BX("bx_img_preview");
	if(pObj.prevsrc != BX("bx_src").value)
	{
		el.style.display="";
		el.removeAttribute("width");
		el.removeAttribute("height");
		pObj.prevsrc = BX("bx_src").value;
		el.src=BX("bx_src").value;
	}

	if (pObj.curWidth && pObj.curHeight)
	{
		el.style.width = pObj.curWidth + 'px';
		el.style.height = pObj.curHeight + 'px';
	}

	el.alt = BX("bx_alt").value;
	el.title = BX("bx_img_title").value;
	el.border = BX("bx_border").value;
	el.align = BX("bx_align").value;
	el.hspace = BX("bx_hspace").value;
	el.vspace = BX("bx_vspace").value;
}

function SetUrl(filename, path, site)
{
	var url, srcInput = BX("bx_src");

	if (typeof filename == 'object') // Using medialibrary
	{
		url = filename.src;
		BX("bx_img_title").value = filename.description || filename.name;
		BX("bx_alt").value = filename.name;
	}
	else // Using file dialog
	{
		url = (path == '/' ? '' : path) + '/'+filename;
	}

	srcInput.value = url;
	if(srcInput.onchange)
		srcInput.onchange();
	srcInput.focus();
	srcInput.select();
}
</script>

<asp:PlaceHolder runat="server" ID="DialogContentImage" Visible="False">
<table class="bx-image-dialog-tbl">
	<tr>
		<td class="bx-par-title"><label for="bx_src"><%= GetMessage("Image.Path")+ ":" %></label></td>
		<td class="bx-par-val">
			<input type="text" size="25" value="" id="bx_src" style="float: left;" />
			<td><input type="button" value="..." id="bx_img_file_dialog_button" onclick=""></td>
		</td>
	</tr>
	<tr>
		<td class="bx-par-title"><label for="bx_img_title"><%= GetMessage("Image.Title") %>:</label></td>
		<td class="bx-par-val"><input type="text" size="30" value="" id="bx_img_title" /></td>
	</tr>
	<tr>
		<td class="bx-par-title"><label for="bx_width"><%= GetMessage("Image.Sizes")%>:</label></td>
		<td class="bx-par-val">
		<input type="text" size="4" id="bx_width" /> x <input type="text" size="4" id="bx_height" />
		<input type="checkbox" value="Y" checked="checked" id="save_props" /> <label for="save_props"><%= GetMessage("Image.SaveProps")%></label>
		</td>
	</tr>
	<tr>
		<td valign="top">
			<table class="bx-img-side">
				<tr>
					<td><label for="bx_hspace"><%= GetMessage("Image.Alt")%>:</label>
					<br />
					<input type="text" size="20" value="" id="bx_alt" />
					</td>
				</tr>
				<tr>
					<td><label for="bx_align"><%= GetMessage("Image.Align") %>:</label>
					<br />
					<select id="bx_align">
						<option value=""> - <%= GetMessage("Image.Align.None")%> -</option>
						<option value="top"><%= GetMessage("Image.Align.Top")%></option>
						<option value="bottom"><%= GetMessage("Image.Align.Bottom")%></option>
						<option value="left"><%= GetMessage("Image.Align.Left")%></option>
						<option value="middle"><%= GetMessage("Image.Align.Middle")%></option>
						<option value="right"><%= GetMessage("Image.Align.Right")%></option>
					</select>
					</td>
				</tr>
				<tr>
					<td><label for="bx_hspace"><%= GetMessage("Image.HSpace")%>:</label>
					<br />
					<input type="text" id="bx_hspace" size="10">px</td>
				</tr>
				<tr>
					<td><label for="bx_vspace"><%= GetMessage("Image.VSpace")%>:</label>
					<br />
					<input type="text" id="bx_vspace" size="10">px</td>
				</tr>
				<tr>
					<td><label for="bx_border"><%= GetMessage("Image.Border")%></label>
					<br />
					<input type="text" id="bx_border" size="10" value="0">px</td>
				</tr>
			</table>
		</td>
		<td valign="top" style="padding-top: 2px;"><%= GetMessage("Image.Preview")%>:
		<div class="bx-preview"><img id="bx_img_preview" style="display:none"/>
			<%for (int i = 0; i < 160; i++) { w.Write("text ");};%>
		</div>
		</td>
	</tr>
</table>
</asp:PlaceHolder>
<%
	using (var s = new System.IO.StringWriter())
	using (var h = new HtmlTextWriter(s))
	{
		DialogContentImage.Visible = true;
		DialogContentImage.RenderControl(h);
		DialogContentImage.Visible = false;
		htmlcontent = s.ToString();
	}
%>



<% } else if (DialogName == "table") { %>
<script>
var pElement = null;
function OnLoad()
{
	if(pObj.params.check_exists)
	{
		window.oBXEditorDialog.SetTitle('<%= GetMessage("Table.WindowTitle.Edit") %>');
		pElement = BXFindParentByTagName(pObj.pMainObj.GetSelectionObject(), 'TABLE');
	}
	else
	{
		window.oBXEditorDialog.SetTitle('<%= GetMessage("Table.WindowTitle.New") %>');
	}

	var
		arStFilter = ['TABLE', 'DEFAULT'], i, arStyles, j,
		elStyles = BX("bx_classname");

	for(i = 0; i < arStFilter.length; i++)
	{
		arStyles = pObj.pMainObj.oStyles.GetStyles(arStFilter[i]);
		for(j = 0; j < arStyles.length; j++)
		{
			if(arStyles[j].className != "")
				elStyles.options.add(new Option(arStyles[j].className, arStyles[j].className, false, false));
		}
	}

	if(pElement)
	{
		BX("rows").value=pElement.rows.length;
		BX("rows").disabled = true;
		BX("cols").value=pElement.rows[0].cells.length;
		BX("cols").disabled = true;
		BX("cellpadding").value = GAttr(pElement, "cellPadding");
		BX("cellspacing").value = GAttr(pElement, "cellSpacing");
		BX("bx_border").value = GAttr(pElement, "border");
		BX("bx_align").value = GAttr(pElement, "align");
		BX("bx_classname").value = GAttr(pElement, "className");
		var v = GAttr(pElement, "width");

		if(v.substr(-1, 1) == "%")
		{
			BX("bx_width").value = v.substr(0, v.length-1);
			BX("width_unit").value = "%";
		}
		else
		{
			if(v.substr(-2, 2) == "px")
				v = v.substr(0, v.length-2);

		 	BX("bx_width").value = v
		}

		v = GAttr(pElement, "height");
		if(v.substr(-1, 1) == "%")
		{
			BX("bx_height").value = v.substr(0, v.length-1);
			BX("height_unit").value = "%";
		}
		else
		{
			if(v.substr(-1, 2) == "px")
				v = v.substr(0, v.length-2);

			BX("bx_height").value = v
		}
	}
	else
	{
		BX("rows").value="2";
		BX("cols").value="3";
		BX("cellpadding").value="1";
		BX("cellspacing").value="1";
		BX("bx_border").value="0";
	}

	window.oBXEditorDialog.adjustSizeEx();
}

function OnSave()
{
	pObj.pMainObj.bSkipChanges = true;
	if(!pElement)
	{
		var tmpid = Math.random().toString().substring(2);
		var str = '<table id="'+tmpid+'"/>';
		BXSelectRange(oPrevRange, pObj.pMainObj.pEditorDocument,pObj.pMainObj.pEditorWindow);
		pObj.pMainObj.insertHTML(str);

		pElement = pObj.pMainObj.pEditorDocument.getElementById(tmpid);
		pElement.removeAttribute("id");

		var i, j, row, cell;
		for(i=0; i < BX("rows").value; i++)
		{
			row = pElement.insertRow(-1);
			for(j = 0; j < BX("cols").value; j++)
			{
				cell = row.insertCell(-1);
				cell.innerHTML = '<br _moz_editor_bogus_node="on">';
			}
		}
	}
	else
	{
		if(pObj.pMainObj.bTableBorder)
			pObj.pMainObj.__ShowTableBorder(pElement, false);
	}

	SAttr(pElement, "width", (BX("bx_width").value.length>0?BX("bx_width").value+''+(BX("width_unit").value=='%'?'%':''):''));
	SAttr(pElement, "height", (BX("bx_height").value.length>0?BX("bx_height").value+''+(BX("height_unit").value=='%'?'%':''):''));
	SAttr(pElement, "border", BX("bx_border").value);
	SAttr(pElement, "cellPadding", BX("cellpadding").value);
	SAttr(pElement, "cellSpacing", BX("cellspacing").value);
	SAttr(pElement, "align", BX("bx_align").value);
	SAttr(pElement, 'className', BX("bx_classname").value);

	pObj.pMainObj.OnChange("table");

	if(pObj.pMainObj.bTableBorder)
		pObj.pMainObj.__ShowTableBorder(pElement, true);
}
</script>

<asp:PlaceHolder runat="server" ID="DialogContentTable" Visible="False">
<table class="bx-dialog-table">
	<tr>
		<td align="right"><label for="rows"><%= GetMessage("Table.RowCount")%>:</label></td>
		<td><input type="text" size="3" id="rows"></td>
		<td>&nbsp;</td>
		<td align="right"><label for="bx_width"><%= GetMessage("Table.Width")%>:</label></td>
		<td nowrap><input type="text" size="3" id="bx_width"><select id="width_unit"><option value="px">px</option><option value="%">%</option></select></td>
	</tr>
	<tr>
		<td align="right"><label for="cols"><%= GetMessage("Table.ColumnCount")%>:</label></td>
		<td><input type="text" size="3" id="cols"></td>
		<td>&nbsp;</td>
		<td align="right"><label for="bx_height"><%= GetMessage("Table.Height")%>:</label></td>
		<td nowrap><input type="text" size="3" id="bx_height"><select id="height_unit"><option value="px">px</option><option value="%">%</option></td>
	</tr>
	<tr>
		<td colspan="5">&nbsp;</td>
	</tr>
	<tr>
		<td align="right" nowrap><label for="bx_border"><%= GetMessage("Table.Border")%>:</label></td>
		<td><input type="text" id="bx_border" size="3"></td>
		<td>&nbsp;</td>
		<td align="right" nowrap><label for="cellpadding"><%= GetMessage("Table.CellPadding")%>:</label></td>
		<td><input type="text" id="cellpadding" size="3"></td>
	</tr>
	<tr>
		<td align="right"><label for="bx_align"><%= GetMessage("Table.Align")%>:</label></td>
		<td>
			<select id="bx_align">
				<option value=""></option>
				<option value="left"><%= GetMessage("Image.Align.Left")%></option>
				<option value="center"><%= GetMessage("Image.Align.Middle")%></option>
				<option value="right"><%= GetMessage("Image.Align.Right")%></option>
			</select>
		</td>
		<td>&nbsp;</td>
		<td align="right" nowrap><label for="cellspacing"><%= GetMessage("Table.CellSpacing")%>:</label></td>
		<td><input type="text" id="cellspacing" size="3"></td>
	</tr>
	<tr>
		<td align="right"><label for="bx_classname"><%= GetMessage("Table.CssClass")%>:</label></td>
		<td colspan="4"><select id='bx_classname'><option value=""> - <%= GetMessage("NO_VAL")%> -</option></select></td>
	</tr>
</table>
</asp:PlaceHolder>
<%
	using (var s = new System.IO.StringWriter())
	using (var h = new HtmlTextWriter(s))
	{
		DialogContentTable.Visible = true;
		DialogContentTable.RenderControl(h);
		DialogContentTable.Visible = false;
		htmlcontent = s.ToString();
	}
%>

<% } else if (DialogName == "pasteastext") { %>
<script>
function OnLoad()
{
	window.oBXEditorDialog.SetTitle('<%= GetMessage("PasteText.WindowTitle")%>');
	BX("BXInsertAsText").focus();

	window.oBXEditorDialog.adjustSizeEx();
}

function OnSave()
{
	BXSelectRange(oPrevRange, pObj.pMainObj.pEditorDocument,pObj.pMainObj.pEditorWindow);
	pObj.pMainObj.PasteAsText(BX("BXInsertAsText").value);
}
</script>

<asp:PlaceHolder runat="server" ID="DialogContentPasteastext" Visible="False">
<table style="width: 100%;">
	<tr>
		<td><%= GetMessage("PasteText.FFFix") %></td>
	</tr>
	<tr><td>
		<textarea id="BXInsertAsText" style="width:100%; height:200px;"></textarea>
	</td></tr>
</table>

</asp:PlaceHolder>
<%
	using (var s = new System.IO.StringWriter())
	using (var h = new HtmlTextWriter(s))
	{
		DialogContentPasteastext.Visible = true;
		DialogContentPasteastext.RenderControl(h);
		DialogContentPasteastext.Visible = false;
		htmlcontent = s.ToString();
	}
%>

<% } else if (DialogName == "pasteword") { %>

<script>
var pFrame = null;
function OnLoad()
{
	window.oBXEditorDialog.SetTitle('<%= GetMessage("PasteWord.WindowTitle") %>');
	pFrame = BX("bx_word_text");

	if(pFrame.contentDocument)
		pFrame.pDocument = pFrame.contentDocument;
	else
		pFrame.pDocument = pFrame.contentWindow.document;
	pFrame.pWindow = pFrame.contentWindow;

	pFrame.pDocument.open();
	pFrame.pDocument.write('<html><head><style>BODY{margin:0px; padding:0px; border:0px;}</style></head><body></body></html>');
	pFrame.pDocument.close();

	if(pFrame.pDocument.addEventListener)
		pFrame.pDocument.addEventListener('keydown', dialog_OnKeyDown, false);
	else if (pFrame.pDocument.attachEvent)
		pFrame.pDocument.body.attachEvent('onpaste', dialog_OnPaste);

	if(jsUtils.IsIE())
	{
		BX("bx_word_ff").style.display = 'none';
		pFrame.pDocument.body.contentEditable = true;
		pFrame.pDocument.body.innerHTML = pObj.pMainObj.GetClipboardHTML();
		dialog_OnPaste();
	}
	else
		pFrame.pDocument.designMode='on';

	setTimeout(function()
	{
		var
			wnd = pFrame.contentWindow,
			doc = pFrame.contentDocument || pFrame.contentWindow.document;
		if(wnd.focus)
			wnd.focus();
		else
			doc.body.focus();
	},
	10);

	//attaching events
	BX("bx_word_removeFonts").onclick =
	BX("bx_word_removeStyles").onclick =
	BX("bx_word_removeIndents").onclick =
	BX("bx_word_removeSpaces").onclick =
	BX("bx_word_removeTableAtr").onclick =
	BX("bx_word_removeTrTdAtr").onclick =
	dialog_cleanAndShow;

	window.oBXEditorDialog.adjustSizeEx();
}

function dialog_OnKeyDown(e)
{
	if (e.ctrlKey && !e.shiftKey && !e.altKey)
	{
		if (!jsUtils.IsIE())
		{
			switch (e.which)
			{
				case 86: // "V" and "v"
				case 118:
					dialog_OnPaste(e);
					break ;
			}
		}
	}
	dialog_cleanAndShow();
}

function dialog_OnPaste(e)
{
	this.pOnChangeTimer = setTimeout(dialog_cleanAndShow, 10);
}

function dialog_cleanAndShow()
{
	dialog_showClenedHtml(pObj.pMainObj.CleanWordText(pFrame.pDocument.body.innerHTML,
	{
		fonts: BX('bx_word_removeFonts').checked,
		styles: BX('bx_word_removeStyles').checked,
		indents: BX('bx_word_removeIndents').checked,
		spaces: BX('bx_word_removeSpaces').checked,
		tableAtr: BX('bx_word_removeTableAtr').checked,
		trtdAtr: BX('bx_word_removeTrTdAtr').checked
	}));
}

function dialog_showClenedHtml(html)
{
	taSourse = BX('bx_word_sourse');
	taSourse.value = html;
}

function OnSave()
{
	BXSelectRange(oPrevRange,pObj.pMainObj.pEditorDocument,pObj.pMainObj.pEditorWindow);
	pObj.pMainObj.PasteWord(pFrame.pDocument.body.innerHTML,
	{
		fonts: BX('bx_word_removeFonts').checked,
		styles: BX('bx_word_removeStyles').checked,
		indents: BX('bx_word_removeIndents').checked,
		spaces: BX('bx_word_removeSpaces').checked,
		tableAtr: BX('bx_word_removeTableAtr').checked,
		trtdAtr: BX('bx_word_removeTrTdAtr').checked
	});
}
</script>
<asp:PlaceHolder runat="server" ID="DialogContentWord" Visible="False">
<table class="bx-dialog-pasteword">
	<tr id="bx_word_ff">
		<td><%= GetMessage("PasteWord.FFFix") %></td>
	</tr>
	<tr>
		<td><iframe id="bx_word_text" src="javascript:void(0)" style="width:100%; height:150px; border:1px solid #CCCCCC;"></iframe></td>
	</tr>
	<tr>
		<td><%= GetMessage("PasteWord.HTMLAfterClear")%></td>
	</tr>
	<tr>
		<td><textarea id="bx_word_sourse" style="width:100%; height:100px; border:1px solid #CCCCCC;" readonly="true"></textarea></td>
	</tr>
	<tr>
		<td>
			<input id="bx_word_removeFonts" type="checkbox" checked="checked"> <label for="bx_word_removeFonts"><%= GetMessage("PasteWord.DeleteFonts")%></label><br>
			<input id="bx_word_removeStyles" type="checkbox" checked="checked"> <label for="bx_word_removeStyles"><%= GetMessage("PasteWord.RemoveStyles")%></label><br>
			<input id="bx_word_removeIndents" type="checkbox" checked="checked"> <label for="bx_word_removeIndents"><%= GetMessage("PasteWord.RemoveIndents")%></label><br>
			<input id="bx_word_removeSpaces" type="checkbox" checked="checked"> <label for="bx_word_removeSpaces"><%= GetMessage("PasteWord.RemoveSpaces")%></label><br>
			<input id="bx_word_removeTableAtr" type="checkbox" checked="checked"> <label for="bx_word_removeTableAtr"><%= GetMessage("PasteWord.RemoveTR_TD_Atr")%></label><br>
			<input id="bx_word_removeTrTdAtr" type="checkbox" checked="checked"> <label for="bx_word_removeTrTdAtr"><%= GetMessage("PasteWord.RemoveTable_Atr")%></label><br>
		</td>
	</tr>
</table>

</asp:PlaceHolder>
<%
	using (var s = new System.IO.StringWriter())
	using (var h = new HtmlTextWriter(s))
	{
		DialogContentWord.Visible = true;
		DialogContentWord.RenderControl(h);
		DialogContentWord.Visible = false;
		htmlcontent = s.ToString();
	}
%>
<% } else if (DialogName == "asksave") { %>
<b> asksave </b>
<% } else if (DialogName == "pageprops") { %>
<b> pageprops </b>
<% } else if (DialogName == "specialchar") { %>

<script>
function OnLoad()
{
	window.oBXEditorDialog.SetTitle('<%= GetMessage("Char.WindowTitle")%>');

	arEntities_dialog = ['&iexcl;','&cent;','&pound;','&curren;','&yen;','&brvbar;','&sect;','&uml;','&copy;','&ordf;','&laquo;','&not;','&reg;','&macr;','&deg;','&plusmn;','&sup2;','&sup3;','&acute;','&micro;','&para;','&middot;','&cedil;','&sup1;','&ordm;','&raquo;','&frac14;','&frac12;','&frac34;','&iquest;','&Agrave;','&Aacute;','&Acirc;','&Atilde;','&Auml;','&Aring;','&AElig;','&Ccedil;','&Egrave;','&Eacute;','&Ecirc;','&Euml;','&Igrave;','&Iacute;','&Icirc;','&Iuml;','&ETH;','&Ntilde;','&Ograve;','&Oacute;','&Ocirc;','&Otilde;','&Ouml;','&times;','&Oslash;','&Ugrave;','&Uacute;','&Ucirc;','&Uuml;','&Yacute;','&THORN;','&szlig;','&agrave;','&aacute;','&acirc;','&atilde;','&auml;','&aring;','&aelig;','&ccedil;','&egrave;','&eacute;','&ecirc;','&euml;','&igrave;','&iacute;','&icirc;','&iuml;','&eth;','&ntilde;','&ograve;','&oacute;','&ocirc;','&otilde;','&ouml;','&divide;','&oslash;','&ugrave;','&uacute;','&ucirc;','&uuml;','&yacute;','&thorn;','&yuml;','&OElig;','&oelig;','&Scaron;','&scaron;','&Yuml;','&circ;','&tilde;','&ndash;','&mdash;','&lsquo;','&rsquo;','&sbquo;','&ldquo;','&rdquo;','&bdquo;','&dagger;','&Dagger;','&permil;','&lsaquo;','&rsaquo;','&euro;','&Alpha;','&Beta;','&Gamma;','&Delta;','&Epsilon;','&Zeta;','&Eta;','&Theta;','&Iota;','&Kappa;','&Lambda;','&Mu;','&Nu;','&Xi;','&Omicron;','&Pi;','&Rho;','&Sigma;','&Tau;','&Upsilon;','&Phi;','&Chi;','&Psi;','&Omega;','&alpha;','&beta;','&gamma;','&delta;','&epsilon;','&zeta;','&eta;','&theta;','&iota;','&kappa;','&lambda;','&mu;','&nu;','&xi;','&omicron;','&pi;','&rho;','&sigmaf;','&sigma;','&tau;','&upsilon;','&phi;','&chi;','&psi;','&omega;','&bull;','&hellip;','&prime;','&Prime;','&oline;','&frasl;','&trade;','&larr;','&uarr;','&rarr;','&darr;','&harr;','&part;','&sum;','&minus;','&radic;','&infin;','&int;','&asymp;','&ne;','&equiv;','&le;','&ge;','&loz;','&spades;','&clubs;','&hearts;'];

	if(!BX.browser.IsIE())
	{
		arEntities_dialog = arEntities_dialog.concat('&thetasym;','&upsih;','&piv;','&weierp;','&image;','&real;','&alefsym;','&crarr;','&lArr;','&uArr;','&rArr;','&dArr;','&hArr;','&forall;','&exist;','&empty;','&nabla;','&isin;','&notin;','&ni;','&prod;','&lowast;','&prop;','&ang;','&and;','&or;','&cap;','&cup;','&there4;','&sim;','&cong;','&sub;','&sup;','&nsub;','&sube;','&supe;','&oplus;','&otimes;','&perp;','&sdot;','&lceil;','&rceil;','&lfloor;','&rfloor;','&lang;','&rang;','&diams;');
	}

	var
		charCont = BX("charCont"),
		charPreview = BX('charPrev'),
		charEntName = BX('entityName'),
		chTable = charCont.appendChild(BX.create("TABLE")),
		i, r, c, lEn = arEntities_dialog.length,
		elEntity = document.createElement("span");

	for(i = 0; i < lEn; i++)
	{
		if (i%19 == 0)
			r = chTable.insertRow(-1);

		elEntity.innerHTML = arEntities_dialog[i];
		c = BX.adjust(r.insertCell(-1), {
			props: {id: 'e_' + i},
			html: elEntity.innerHTML,
			events: {
				mouseover: function(e){
					var entInd = this.id.substring(2);
					BX.addClass(this, 'bx-over');
					charPreview.innerHTML = this.innerHTML;
					charEntName.innerHTML = arEntities_dialog[entInd].substr(1, arEntities_dialog[entInd].length - 2);
				},
				mouseout: function(e){BX.removeClass(this, 'bx-over');},
				click: function(e){
					var entInd = this.id.substring(2);
					BXSelectRange(oPrevRange,pObj.pMainObj.pEditorDocument,pObj.pMainObj.pEditorWindow);
					pObj.pMainObj.insertHTML(arEntities_dialog[entInd]);
					window.oBXEditorDialog.Close();
				}
			}
		});
	}

	window.oBXEditorDialog.SetButtons([window.oBXEditorDialog.btnCancel]);
	window.oBXEditorDialog.adjustSizeEx();
}
</script>

<asp:PlaceHolder runat="server" ID="DialogContentChar" Visible="False">
<div style="height: 285px;">
	<div id="charCont" class="bx-d-char-cont"></div>
	<div id="charPrev" class="bx-d-prev-char"></div>
	<div id="entityName" class="bx-d-ent-name">&nbsp;</div>
</div>
</asp:PlaceHolder>
<%
	using (var s = new System.IO.StringWriter())
	using (var h = new HtmlTextWriter(s))
	{
		DialogContentChar.Visible = true;
		DialogContentChar.RenderControl(h);
		DialogContentChar.Visible = false;
		htmlcontent = s.ToString();
	}
%>

<% } else if (DialogName == "settings") { %>
<script>
function OnLoad()
{
	window.oBXEditorDialog.SetTitle('<%= GetMessage("Settings.WindowTitle")%>');
	if (!pObj.params.lightMode)
	{
		// TAB #1: Toolbar settings
		window.temp_arToolbarSettings = copyObj(SETTINGS[pObj.pMainObj.name].arToolbarSettings);
		_displayToolbarList(BX("__bx_set_1_toolbar"));
	}
	else
	{
		// Hack
		var toolbarTab = BX("BXEdTabControlOptions_tab_cont_0");
		if (toolbarTab)
			toolbarTab.style.display = 'none';
		BXEdTabControlOptions.SelectTab('1');
	}

	// TAB #2: Taskbar settings
	window.temp_arTaskbarSettings = copyObj(SETTINGS[pObj.pMainObj.name].arTaskbarSettings);
	_displayTaskbarList(BX("__bx_set_2_taskbar"));

	// TAB #3: Additional Properties
	_displayAdditionalProps(BX("__bx_set_3_add_props"));

	window.oBXEditorDialog.SetButtons([
		new BX.CWindowButton(
		{
			title: '<%= GetMessage("Dialog.Save")%>',
			id: 'save',
			name: 'save',
			action: function()
			{
				var r;
				if(window.OnSave && typeof window.OnSave == 'function')
					r = window.OnSave();

				window.oBXEditorDialog.Close();
			}
		}),
		new BX.CWindowButton(
		{
			title: '<%= GetMessage("Dialog.Restore")%>',
			id: 'restore',
			name: 'restore',
			action: function()
			{
				restoreSettings();
				window.oBXEditorDialog.Close();
			}
		}),
		window.oBXEditorDialog.btnClose
	]);


	window.oBXEditorDialog.adjustSizeEx();
}

function _displayToolbarList(oCont)
{
	var oTable = oCont.appendChild(BX.create("TABLE", {style: {width: "100%"}}));
	_displayTitle(oTable, '<%= GetMessage("Settings.ITab.TBAppearance")%>');
	pObj.arToolbarCheckboxes = [];

	for(var sToolBarId in arToolbars)
		if (arToolbars[sToolBarId] && typeof arToolbars[sToolBarId] == 'object')
			_displayToolbarRow(oTable, sToolBarId, SETTINGS[pObj.pMainObj.name].arToolbarSettings[sToolBarId].show);
}


function _displayToolbarRow(oTb, toolbarId, _show)
{
	var pCh = _displayRow(oTb, arToolbars[toolbarId][0], '__bx_' + toolbarId);
	SAttr(pCh, "__bxid", toolbarId);
	oBXEditorUtils.setCheckbox(pCh, _show);
	if (toolbarId != "standart")
		pObj.arToolbarCheckboxes.push(pCh);
	if (toolbarId == "standart")
		pCh.disabled = "disabled";
	pCh.onchange = function(e) {window.temp_arToolbarSettings[this.getAttribute("__bxid")].show = this.checked;}
}

function _displayRow(pTb, label, id)
{
	var pTr = pTb.insertRow(-1);
	var pTd = BX.adjust(pTr.insertCell(-1), {props: {className: "bx-par-title"}});

	BX.adjust(pTr.insertCell(-1), {props: {className: "bx-par-val"}, html: '<label for="' + id + '">' + label + '</label>'});
	return pTd.appendChild(BX.create("INPUT", {props: {type: 'checkbox', id: id}}));
}

function _displayTaskbarList(oCont)
{
	var oTable = oCont.appendChild(BX.create("TABLE", {style: {width: "100%"}}));
	_displayTitle(oTable,'<%= GetMessage("Settings.ITab.TsBAppearance")%>');
	pObj.arTaskbarCheckboxes = [];

	// TODO: bugs with two editors on page - fix IT
	var arTBAdded = {}, k, i, l;

	for(k in ar_BXTaskbarS)
	{
		if (ar_BXTaskbarS[k] && ar_BXTaskbarS[k].pMainObj && ar_BXTaskbarS[k].pMainObj.name == pObj.pMainObj.name)
		{
			arTBAdded[ar_BXTaskbarS[k].name] = true;
			_displayTaskbarRow(oTable, ar_BXTaskbarS[k], pObj.pMainObj.GetTaskbarConfig(ar_BXTaskbarS[k].name));
		}
	}

	for (i = 0, l = arBXTaskbars.length; i < l; i++)
	{
		k = arBXTaskbars[i].name;
		if(pObj.pMainObj.allowedTaskbars[k] && !arTBAdded[k])
		{
			var settings = pObj.pMainObj.GetTaskbarConfig(k);
			if (!settings.show)
			{
				_displayTaskbarRow(oTable, {name: k, title: arBXTaskbars[i].title}, settings);
				arTBAdded[k] = true;
			}
		}
	}

	oCont.appendChild(oTable);
}

function _displayTaskbarRow(pTb, oTaskbar, arSettings)
{
	var pCh = _displayRow(pTb, oTaskbar.title, '__bx_' + oTaskbar.name);
	SAttr(pCh, "__bxid", oTaskbar.name);

	if (oTaskbar.name == "BXPropertiesTaskbar")
	{
		arSettings.show = true;
		pCh.disabled = true;
	}	
	else if (oTaskbar.name == "ASPXComponentsTaskbar")
	{
		arSettings.show = true;
		pCh.disabled = true;
	}

	oBXEditorUtils.setCheckbox(pCh, arSettings.show);
	pObj.arTaskbarCheckboxes.push(pCh);
	pCh.onchange = function(e)
	{
		var id = this.getAttribute("__bxid");
		if (!window.temp_arTaskbarSettings[id])
			window.temp_arTaskbarSettings[id] = pObj.pMainObj.GetTaskbarConfig(id);
		window.temp_arTaskbarSettings[this.getAttribute("__bxid")].show = this.checked;
	}
}

function _displayTitle(pTb, sTitle)
{
	var pTr = pTb.insertRow(-1);
	pTr.className = "heading_dialog";
	BX.adjust(pTr.insertCell(-1), {props: {colSpan: 2}, text: sTitle});
}


function _displayAdditionalProps(oCont)
{
	var oTable = oCont.appendChild(pObj.pMainObj.CreateElement('TABLE', {width: '100%'}));
	_displayTitle(oTable,'<%= GetMessage("Settings.TabText.Additional")%>');

	oBXEditorUtils.setCheckbox(_displayRow(oTable, '<%= GetMessage("Settings.AddTab.ShowTooltips")%>', '__bx_show_tooltips'), pObj.pMainObj.showTooltips4Components);

	oBXEditorUtils.setCheckbox(_displayRow(oTable, '<%= GetMessage("Settings.AddTab.VisualEffects")%>', '__bx_visual_effects'), pObj.pMainObj.visualEffects);
}

function restoreSettings()
{
	pObj.pMainObj.RestoreConfig();
	var RSPreloader = new BXPreloader(
		[{func: BX.proxy(pObj.pMainObj.GetConfig, pObj.pMainObj), params: []}],
		{
			func: function()
			{
				if (!lightMode)
					BXRefreshToolbars(pObj.pMainObj);
				BXRefreshTaskbars(pObj.pMainObj);
				pObj.Close();
			}
		}
	);
	RSPreloader.LoadStep();
}


function OnSave()
{
	var Settings = SETTINGS[pObj.pMainObj.name];
	if (!lightMode)
	{
		if (!compareObj(Settings.arToolbarSettings, window.temp_arToolbarSettings))
		{
			Settings.arToolbarSettings = temp_arToolbarSettings;
			pObj.pMainObj.SaveConfig("toolbars", {tlbrset: temp_arToolbarSettings});
			BXRefreshToolbars(pObj.pMainObj);
		}
	}

	var showTooltips = !!BX("__bx_show_tooltips").checked;
	if (showTooltips != pObj.pMainObj.showTooltips4Components)
	{
		pObj.pMainObj.showTooltips4Components = showTooltips;
		pObj.pMainObj.SaveConfig("tooltips");
	}

	var visEff = !!BX("__bx_visual_effects").checked;
	if (visEff != pObj.pMainObj.visualEffects)
	{
		pObj.pMainObj.visualEffects = visEff;
		pObj.pMainObj.SaveConfig("visual_effects");
	}
}

function OnSave1()
{
	if (!document.getElementById("__bx_rs_tskbrs").checked)
		temp_arTaskbarSettings = arTaskbarSettings_default;

	var showTooltips = (document.getElementById("__bx_show_tooltips").checked);
	if (showTooltips != pObj.pMainObj.showTooltips4Components)
	{
		pObj.pMainObj.showTooltips4Components = showTooltips;
		BXSetConfiguration(pObj.pMainObj,"tooltips","GET");
	}

	var visEff = (document.getElementById("__bx_visual_effects").checked);
	if (visEff != pObj.pMainObj.visualEffects)
	{
		pObj.pMainObj.visualEffects = visEff;
		BXSetConfiguration(pObj.pMainObj, "visual_effects", "GET");
	}

	if (!lightMode)
	{
		if (!document.getElementById("__bx_rs_tlbrs").checked)
			temp_arToolbarSettings = arToolbarSettings_default;

		if (!compareObj(SETTINGS[pObj.pMainObj.name].arToolbarSettings,window.temp_arToolbarSettings) ||
			(document.getElementById("__bx_rs_tlbrs").checked != pObj.pMainObj.RS_toolbars))
		{
			pObj.pMainObj.RS_toolbars = document.getElementById("__bx_rs_tlbrs").checked;
			SETTINGS[pObj.pMainObj.name].arToolbarSettings = temp_arToolbarSettings;
			var postData = oBXEditorUtils.ConvertArray2Post(temp_arToolbarSettings,'tlbrset');
			BXSetConfiguration(pObj.pMainObj,"toolbars","POST",postData);
			BXRefreshToolbars(pObj.pMainObj);
		}
	}


	if (!compareObj(SETTINGS[pObj.pMainObj.name].arTaskbarSettings, window.temp_arTaskbarSettings) ||
		(document.getElementById("__bx_rs_tskbrs").checked != pObj.pMainObj.RS_taskbars))
	{
		pObj.pMainObj.RS_taskbars = document.getElementById("__bx_rs_tskbrs").checked;
		SETTINGS[pObj.pMainObj.name].arTaskbarSettings = temp_arTaskbarSettings;
		recreateTaskbars(pObj.pMainObj);
		
		var postData = oBXEditorUtils.ConvertArray2Post(temp_arTaskbarSettings, 'tskbrset');
		BXSetConfiguration(pObj.pMainObj, "taskbars", "POST", postData);
	}
}

function recreateTaskbars(pMainObj)
{
	setTimeout(function () {
			BXCreateTaskbars(pMainObj, false);
			BXRefreshTaskbars(pMainObj);
		}
	, 50);
}
</script>
<bx:BXTabControl ID="BXEdTabControlOptions" runat="server" ButtonsMode="Hidden" PublicMode="true">

<bx:BXTabControlTab ID="BXEdTabsOptions1" runat="server" Text="<%$ Loc:Settings.TabText.InstrumentPanel %>" Selected="true"  OnSelectScript="window.oBXEditorDialog.adjustSizeEx();">
	<div id="__bx_set_1_toolbar">&nbsp;</div>
</bx:BXTabControlTab>
<bx:BXTabControlTab ID="BXEdTabsOptions2" runat="server" Text="<%$ Loc:Settings.TabText.TaskPanel %>"  OnSelectScript="window.oBXEditorDialog.adjustSizeEx();">
	<div id="__bx_set_2_taskbar">&nbsp;</div>
</bx:BXTabControlTab>
<bx:BXTabControlTab ID="BXEdTabsOptions3" runat="server" Text="<%$ Loc:Settings.TabText.Additional %>"  OnSelectScript="window.oBXEditorDialog.adjustSizeEx();">
	<div id="__bx_set_3_add_props">&nbsp;</div>
</bx:BXTabControlTab>

</bx:BXTabControl>

<% } else if (DialogName == "flash") { %>
<script>
function OnLoad()
{
	// ************************ TAB #1: Base params *************************************
	var oDiv = BX("__bx_base_params");
	oDiv.style.padding = "5px";
	oDiv.innerHTML = '<table width="100%" border="0" height="260">'+
					'<tr>'+
						'<td align="right" width="40%">' + BX_MESS.PATH2SWF + ':</td>'+
						'<td width="60%">'+
							'<input type="text" size="30" value="" id="bx_flash_src" name="bx_src">'+
							'<input type="button" value="..." id="OpenFileBrowserWindFlash_button">'+
						'</td>'+
					'</tr>'+
					'<tr>'+
						'<td align="right">' + BX_MESS.TPropSize + ':</td>'+
						'<td align="left"><input type="text" size="4" id="flash_width" /> x <input type="text" size="4" id="flash_height" /></td>' +
					'</tr>'+
					'<tr>'+
						'<td align="right" valign="top"><%=GetMessage("Flash.Preview")%>:</td>'+
						'<td>'+
							'<div id="flash_preview_cont" style="height:200px; width:95%; overflow: hidden; border: 1px #999999 solid; overflow-y: auto; overflow-x: auto;">'+
							'</div>'+
						'</td>'+
					'</tr>'+
				'</table>';

	//Attaching Events
	BX("OpenFileBrowserWindFlash_button").onclick = function()
	{
		__OpenDialog(
			{
				ShowFiles: true,
				ItemsToSelect: 2,
				ExtensionsList: "swf",
				ShowExtras: true,
				EnableExtras: true,
				DefaultUploadDirectory: "upload"
			},
			function(val)
			{
				val = val.replace(/\\/g, '/');
				val = val.replace(/~/g, '');
				var oSrc = BX("bx_flash_src");
				oSrc.value = APPPath + val;
				if(oSrc.onchange)
					oSrc.onchange();
			}
		);
	};
	
	var oPreviewCont = BX("flash_preview_cont");
	BX("bx_flash_src").onchange = function(){Flash_Reload(oPreviewCont, BX("bx_flash_src").value, 150, 150)};

	// ************************ TAB #2: Additional params ***********************************
	var oDiv = BX("__bx_additional_params");
	oDiv.style.padding = "5px";
	oDiv.innerHTML = '<table width="100%" border="0" height="260">'+
				'<tr>'+
					'<td align="right" width="40%" colspan="2">' + BX_MESS.SWF_ID + ':</td>'+
					'<td width="60%" colspan="2">'+
						'<input type="text" size="30" value="" id="_flash_id">'+
					'</td>'+
				'</tr>'+
				'<tr>'+
					'<td align="right" colspan="2">' + BX_MESS.SWF_TITLE + ':</td>'+
					'<td colspan="2">'+
						'<input type="text" size="30" value="" id="_flash_title">'+
					'</td>'+
				'</tr>'+
				'<tr>'+
					'<td align="right" colspan="2">' + BX_MESS.SWF_CLASSNAME + ':</td>'+
					'<td colspan="2">'+
						'<input type="text" size="30" value="" id="_flash_classname">'+
					'</td>'+
				'</tr>'+
				'<tr>'+
					'<td align="right" colspan="2">' + BX_MESS.TPropStyle + '</td>'+
					'<td colspan="2">'+
						'<input type="text" size="30" value="" id="_flash_style">'+
					'</td>'+
				'</tr>'+
				'<tr>'+
					'<td align="right" colspan="2">' + BX_MESS.SWF_QUALITY + ':</td>'+
					'<td colspan="2">'+
						'<select id="_flash_quality" style="width:100px">'+
							'<option value=""></option>'+
							'<option value="low">low</option>'+
							'<option value="medium">medium</option>'+
							'<option value="high">high</option>'+
							'<option value="autolow">autolow</option>'+
							'<option value="autohigh">autohigh</option>'+
							'<option value="best">best</option>'+
						'</select>'+
					'</td>'+
				'</tr>'+
				'<tr>'+
					'<td align="right" colspan="2">' + BX_MESS.SWF_WMODE + ':</td>'+
					'<td colspan="2">'+
						'<select id="_flash_wmode" style="width:100px">'+
							'<option value=""></option>'+
							'<option value="window">window</option>'+
							'<option value="opaque">opaque</option>'+
							'<option value="transparent">transparent</option>'+
						'</select>'+
					'</td>'+
				'</tr>'+
				'<tr>'+
					'<td align="right" colspan="2">' + BX_MESS.SWF_SCALE + ':</td>'+
					'<td colspan="2">'+
						'<select id="_flash_scale"style="width:100px">'+
							'<option value=""></option>'+
							'<option value="showall">showall</option>'+
							'<option value="noborder">noborder</option>'+
							'<option value="exactfit">exactfit</option>'+
						'</select>'+
					'</td>'+
				'</tr>'+
				'<tr>'+
					'<td align="right" colspan="2">' + BX_MESS.SWF_SALIGN + ':</td>'+
					'<td colspan="2">'+
						'<select id="_flash_salign" style="width:100px">'+
							'<option value=""></option> '+
							'<option value="left">left</option> '+
							'<option value="top">top</option> '+
							'<option value="right">right</option> '+
							'<option value="bottom">bottom</option> '+
							'<option value="top left">top left</option>'+
							'<option value="top right">top right</option>'+
							'<option value="bottom left">bottom left</option>'+
							'<option value="bottom right">bottom right</option>'+
						'</select>'+
					'</td>'+
				'</tr>'+
				'<tr>'+
					'<td align="right" colspan="2">' + BX_MESS.SWF_AUTOPLAY + ':</td>'+
					'<td colspan="2">'+
						'<input type="checkbox" value="" id="_flash_autoplay">'+
					'</td>'+
				'</tr>'+
				'<tr>'+
					'<td align="right" colspan="2">' + BX_MESS.SWF_LOOP + ':</td>'+
					'<td colspan="2">'+
						'<input type="checkbox" value="" id="_flash_loop">'+
					'</td>'+
				'</tr>'+
				'<tr>'+
					'<td align="right" colspan="2">' + BX_MESS.SWF_SHOW_MENU + ':</td>'+
					'<td colspan="2">'+
						'<input type="checkbox" value="" id="_flash_showmenu">'+
					'</td>'+
				'</tr>'+
			'</table>';

	// ************************ TAB #3: HTML Code *************************************
	var oDiv = BX("__bx_code");
	oDiv.style.padding = "5px";
	oDiv.innerHTML = '<table width="100%" border="0" height="260">'+
					'<tr>'+
						'<td align="left" width="100%" style="padding-left: 30px !important;"><%= GetMessage("Flash.HTMLCode")%>:<br />'+
							'<textarea id="bx_flash_html_code" cols="49" rows="12"></textarea>'+
						'</td>'+
					'</tr>'+
				'</table>';

	var applyParams = function(arParams)
	{
		var re, _p, i, l;
		for(var i in pObj.bx_swf_arParams)
		{
			_p = pObj.bx_swf_arParams[i].p;
			if (!_p)
				continue;

			if (_p.type.toLowerCase() == 'checkbox')
				_p.checked = (arParams[i]);
			else
				_p.value = arParams[i] || '';
		}
	};

	pObj.bx_swf_source = BX("bx_flash_html_code");
	pObj.bx_swf_source.onblur = function()
	{
		var s = this.value;
		if (s.length <= 0)
			return;
		var flash_parser = function(str, attr)
		{
			if (attr.indexOf('.swf') === false || attr.indexOf('flash') === false) // not a flash
				return;

			attr = attr.replace(/[\r\n]+/ig, ' ');
			attr = attr.replace(/\s+/ig, ' ');
			attr = attr.trim();

			var _params = ['src', 'width', 'height', 'id', 'title', 'class', 'style', 'quality', 'wmode', 'scale', 'salign', 'autoplay', 'loop', 'showmenu' ];
			var arParams = {};
			var re, _p, i, l;
			for (i = 0, l = _params.length; i < l; i++)
			{
				_p = _params[i];
				re = new RegExp(_p+'\\s*=\\s*("|\')([^\\1]+?)\\1', "ig");
				attr = attr.replace(re, function(s, b1, value){arParams[_p] = value;});
			}
			applyParams(arParams);
		};
		s = s.replace(/<embed([^>]*?)>[^>]*?<\/embed>/ig, flash_parser);
		Flash_Reload(oPreviewCont, BX("bx_flash_src").value, 150, 150);
	};

	pObj.bx_swf_arParams = {
		src : {p : BX("bx_flash_src")},
		width : {p : BX("flash_width")},
		height : {p : BX("flash_height")},
		id : {p : BX("_flash_id")},
		title : {p : BX("_flash_title")},
		classname : {p : BX("_flash_classname")},
		style : {p : BX("_flash_style")},
		quality : {p : BX("_flash_quality")},
		wmode : {p : BX("_flash_wmode")},
		scale : {p : BX("_flash_scale")},
		salign : {p : BX("_flash_salign")},
		autoplay : {p : BX("_flash_autoplay")},
		loop : {p : BX("_flash_loop")},
		showmenu : {p : BX("_flash_showmenu")}
	};

	pElement = pObj.pMainObj.GetSelectionObject();
	pObj.bxTag = false;

	if (pElement)
	{
		bxTag = pObj.pMainObj.GetBxTag(pElement);
		if (!bxTag || bxTag.tag != "flash")
			pElement = false;
	}

	if(pElement && bxTag) // Edit flash
	{
		pObj.bxTag = bxTag;

		//var id  = pElement.id;
		pObj.bx_swf_source.disabled = true;
		window.oBXEditorDialog.SetTitle(BX_MESS.FLASH_MOV);

		//applyParams(pObj.pMainObj.arFlashParams[id]);
		applyParams(bxTag.params);
		Flash_Reload(oPreviewCont, BX("bx_flash_src").value, 150, 150);
	}
	else // insert flash
	{
		window.oBXEditorDialog.SetTitle('<%= GetMessage("Flash.WindowTitle.New")%>');
	}

	window.oBXEditorDialog.adjustSizeEx();
}


function OnSave()
{
	if (!pObj.bx_swf_arParams.src.p.value)
		return;

	pObj.pMainObj.bSkipChanges = true;
	BXSelectRange(oPrevRange,pObj.pMainObj.pEditorDocument, pObj.pMainObj.pEditorWindow);
	var html, i, p;

	if (pObj.bxTag)
	{
		for(i in pObj.bx_swf_arParams)
		{
			p = pObj.bx_swf_arParams[i].p;
			if (p)
			{
				if (p.type.toLowerCase() == 'checkbox' && p.checked)
					pObj.bxTag.params[i] = p.checked || null;
				else if(p.type.toLowerCase() != 'checkbox' && p.value.length > 0)
					pObj.bxTag.params[i] = p.value;
			}
		}

		pElement.style.width = (parseInt(pObj.bxTag.params.width) || 50) + 'px';
		pElement.style.height = (parseInt(pObj.bxTag.params.height) || 25) + 'px';
		pObj.pMainObj.bSkipChanges = false;
		pObj.pMainObj.SetBxTag(pElement, pObj.bxTag);
		return;
	}

	if (pObj.bx_swf_source.value.length > 0)
	{
		html = pObj.bx_swf_source.value;
	}
	else
	{
		html = '<EMBED ';
		for(var i in pObj.bx_swf_arParams)
		{
			_p = pObj.bx_swf_arParams[i].p;
			if (!_p) continue;

			if (_p.type.toLowerCase() == 'checkbox' && _p.checked)
				html += i + '="true" ';
			else if(_p.type.toLowerCase() != 'checkbox' && _p.value.length > 0)
				html += i + '="' + _p.value + '" ';
		}
		html += 'type = "application/x-shockwave-flash" '+
		'pluginspage = "http://www.macromedia.com/go/getflashplayer" '+
		'></EMBED>';
	}

	var html = pObj.pMainObj.pParser.SystemParse(html);
	pObj.pMainObj.insertHTML(html);
	pObj.pMainObj.bSkipChanges = false;
}
</script>

<bx:BXTabControl ID="BXEdTabControlFlash" runat="server" ButtonsMode="Hidden" PublicMode="true">
<bx:BXTabControlTab ID="BXEdTabsFlash1" runat="server" Text="<%$ Loc:Flash.TabText.General %>" Selected="true" OnSelectScript="window.oBXEditorDialog.adjustSizeEx();">
	<div id="__bx_base_params"></div>
</bx:BXTabControlTab>
<bx:BXTabControlTab ID="BXEdTabsFlash2" runat="server" Text="<%$ Loc:Flash.TabText.Additional %>" OnSelectScript="window.oBXEditorDialog.adjustSizeEx();">
	<div id="__bx_additional_params"></div>
</bx:BXTabControlTab>
<bx:BXTabControlTab ID="BXEdTabsFlash3" runat="server" Text="<%$ Loc:Flash.TabText.HTMLCode %>" OnSelectScript="window.oBXEditorDialog.adjustSizeEx();">
	<div id="__bx_code"></div>	
</bx:BXTabControlTab>
</bx:BXTabControl>

<% } else if (DialogName == "edit_hbf") { %>
<script>
function OnLoad()
{
	window.oBXEditorDialog.SetTitle(TE_MESS.FILEMAN_EDIT_HBF);
	// TAB #1: HEAD
	var oDiv = BX("__bx_head");
	//BX.findChild(oDiv, {tag: 'TABLE', className: 'edit-tab-title'}, true).style.display = BX.browser.IsIE() ? "inline" : "table";
	
	oDiv.style.padding = "5px";
	// Insert default HEAD	
	//oDiv.getElementsByTagName("TABLE")[0].rows[1].cells[0].appendChild(BX.create("DIV", {props: {title: '<%= GetMessage("INSERT_DEFAULT")%>', className: "iconkit_c bx-insert-hbf"}})).onclick = insertDefault_head;

	// Textarea
	oDiv.appendChild(BX.create("TEXTAREA", {props: {id: "__bx_head_ta", value: pObj.pMainObj._head + pObj.pMainObj._body}, style: {width: "99%", height: "280px"}}));

	// TAB #2: Footer
	var oDiv = BX("__bx_footer");
	//BX.findChild(oDiv, {tag: 'TABLE', className: 'edit-tab-title'}, true).style.display = BX.browser.IsIE() ? "inline" : "table";
	oDiv.style.padding = "5px";

	// Insert default FOOTER
	//oDiv.getElementsByTagName("TABLE")[0].rows[1].cells[0].appendChild(BX.create("DIV", {props: {title: '<%= GetMessage("INSERT_DEFAULT")%>', className: "iconkit_c bx-insert-hbf"}})).onclick = insertDefault_footer;

	// Textarea
	oDiv.appendChild(BX.create("TEXTAREA", {props: {id: "__bx_footer_ta", value: pObj.pMainObj._footer}, style:{width: "99%", height: "280px"}}));
}

function OnSave()
{
	BX("__bx_head_ta").value.replace(/(^[\s\S]*?)(<body.*?>)/i, "");
	pObj.pMainObj._head = RegExp.$1;
	pObj.pMainObj._body = RegExp.$2;
	pObj.pMainObj._footer = BX("__bx_footer_ta").value;
	pObj.pMainObj.updateBody();
}

function insertDefault_head()
{
	if (!confirm('<%= GetMessage("CONFIRM_HEAD")%>'))
		return;
	var oTA = BX("__bx_head_ta");
	var s60 = String.fromCharCode(60);
	var s62 = String.fromCharCode(62);
	oTA.value = s60 + '%@ Master Language="C#" Inherits="Bitrix.UI.BXMasterPage" %' + s62 + "\n" +
		'<html>' + "\n" +
		s60 + 'head runat="server"' + s62 + "\n" + 
		'<meta http-equiv="Content-Type" content="text/html;" />' + 
		'<title>Company name</title>' + 
		"</head>\n" + 
		'<body>';
}

function insertDefault_footer()
{
	if (!confirm('<%= GetMessage("CONFIRM_FOOTER")%>'))
		return;
	BX("__bx_footer_ta").value = "</body>\n</html>";
}
</script>

<bx:BXTabControl ID="BXEdTabControlHBF" runat="server" ButtonsMode="Hidden" PublicMode="true">
<bx:BXTabControlTab ID="BXTabHBFHead" runat="server" Text="<%$ Loc:TOP_AREA %>" Selected="true"  OnSelectScript="window.oBXEditorDialog.adjustSizeEx();">
	<div id="__bx_head">
	<table width="100%" border="0">
		<tr>
			<td width="90%" style="font-size: 13px !important; color: #494949 !important; font-weight: bold !important; padding-left: 5px !important;"><%= GetMessage("EDIT_HEAD")%></td>
			<td width="10%" align="right">
				<div title="<%= GetMessage("INSERT_DEFAULT")%>" class="iconkit_c bx-insert-hbf" onclick="insertDefault_head()"></div>
			</td>
		</tr>
	</table>
	</div>
</bx:BXTabControlTab>
<bx:BXTabControlTab ID="BXTabHBFFooter" runat="server" Text="<%$ Loc:BOTTOM_AREA %>"  OnSelectScript="window.oBXEditorDialog.adjustSizeEx();">
	<div id="__bx_footer">
	<table width="100%" border="0">
		<tr>
			<td width="90%" style="font-size: 13px !important; color: #494949 !important; font-weight: bold !important; padding-left: 5px !important;"><%= GetMessage("EDIT_FOOTER")%></td>
			<td width="10%" align="right">
				<div title="<%= GetMessage("INSERT_DEFAULT")%>" class="iconkit_c bx-insert-hbf" onclick="insertDefault_footer()"></div>
			</td>
		</tr>
	</table>
	</div>
</bx:BXTabControlTab>
</bx:BXTabControl>

<% }
   else { w.WriteLine("Dialog for \"{0}\" is not defined!", DialogName); }
// * * * * * * * * *  END DIALOGS  * * * * * * * * * * * *	
%>

<script>
	window.JSName = window[window._fileDialog.JSName];
	window.ExtraJSName = window[window._fileDialog.ExtraJSName];
	
	window.__OpenDialog = function(oParams, callBack)
	{
		ExtraJSName.Properties = oParams || {ShowFiles: true, ItemsToSelect: 2, ExtensionsList: '' /*'aspx,html'*/};
		ExtraJSName.SaveEvent = function(){callBack(ExtraJSName.GetTargetValue());};
		JSName.ShowPopupDialog();
		ExtraJSName.Reset();
		ExtraJSName.CollapseToRoot();
		return false;
	}
	if (!window.oBXEditorDialog.bUseTabControl)
	{	
		window.oBXEditorDialog.Show();
		window.oBXEditorDialog.SetContent('<%= JSEncode(htmlcontent) %>');
		OnLoad();
		window.oBXEditorDialog.adjustSizeEx();
	}
	else
	{
		OnLoad();
		window.oBXEditorDialog.adjustSizeEx();
	}

	BX.addClass(window.oBXEditorDialog.PARTS.CONTENT, "bxed-dialog");

	BX.addCustomEvent(window.oBXEditorDialog, 'onWindowUnRegister', function()
	{
		if (window.oBXEditorDialog && window.oBXEditorDialog.DIV && window.oBXEditorDialog.DIV.parentNode)
			window.oBXEditorDialog.DIV.parentNode.removeChild(window.oBXEditorDialog.DIV);
	});

	// Set default buttons
	if (!window.oBXEditorDialog.PARAMS.buttons || !window.oBXEditorDialog.PARAMS.buttons.length)
	{
		window.oBXEditorDialog.SetButtons([
			new BX.CWindowButton(
			{
				title: '<%= GetMessage("Dialog.Save")%>',
				id: 'save',
				name: 'save',
				action: function()
				{
					var r;
					if(window.OnSave && typeof window.OnSave == 'function')
						r = window.OnSave();
					if (r !== false)
						window.oBXEditorDialog.Close();
				}
			}),
			window.oBXEditorDialog.btnClose
		]);
	}
</script>

